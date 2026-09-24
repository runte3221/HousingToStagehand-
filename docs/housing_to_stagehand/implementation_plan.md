# 家具消失の解消および染色精度の完全修正 実装計画

`CL03 Meridian Neue L.json` を Stagehand に変換した際に発生していた以下の問題について、ゲーム内部データ（SqPack）および Stagehand の内部実装（IL / C#）を徹底解析し、原因を完全に突き止めました。
本計画では、これらの原因を根本から解決するための修正を行います。

---

## 徹底調査により判明した根本原因

### 1. 消失していた家具（合計 12 個）の原因
SqPack のインデックスを直接照合した結果、以下の 4 種類の家具のモデルパスがゲームデータに存在せず、Stagehand のスポーン処理（`FileExists` チェック）で **null と判定されて消失** していました：
- **`Queen's Rest` (7個)**:
  - 現状: `fun_b0_m1026.mdl` を探している。
  - 実態: ゲーム内では **`fun_b0_m1026a.mdl`**（末尾に `a` が付く）というファイル名。
- **`Chilled Red` (3個) / `Starlight Dodo` (1個) / `Riviera Table Chronometer` (1個)**:
  - 現状: 室内家具シートにあるため `indoor/.../fun_b0_mXXXX.mdl` を探している。
  - 実態: ゲームデータ上、これらは **`outdoor/.../gar_b0_mXXXX.mdl`**（庭具フォルダ）に格納されている。

### 2. 染色が薄く・白化していた原因（Stagehand 内部の計算式との不整合）
Stagehand の `LiveBgObject.set_DyeColor` の内部実装：
```csharp
var srgbColor = new Vector4(MathF.Sqrt(value.X), MathF.Sqrt(value.Y), MathF.Sqrt(value.Z), value.Z) * byte.MaxValue;
var byteColor = new ByteColor() { R = (byte)srgbColor.X, G = (byte)srgbColor.Y, B = (byte)srgbColor.Z, A = (byte)srgbColor.W };
```
- Stagehand は渡された `DyeColor`（`value`）を **Linear Color (0..1)** とみなし、`MathF.Sqrt`（平方根）を掛けて sRGB バイト値（0..255）を求めています。
- 前回のコードでは sRGB値（`r / 255.0f`）をそのまま渡していたため、例えばスートブラック（#2B2923 = 43, 41, 35）の場合：
  `MathF.Sqrt(43 / 255.0f) * 255 = 104.7`（約 105）という、**本来の約 2.5倍も明るい薄いグレー** に白化してしまっていました。
- ゲームのシェーダーではさらにこれが 2乗されるため、マテリアルには **本来の約 6 倍も明るい値** が適用されていました。
- **解決策**: 染色家具の `DyeColor` には **`MathF.Pow(r / 255f, 2f)`（リニア色）** を渡すことで、Stagehand 内の `MathF.Sqrt` と相殺し、**元の HEX カラー（スートブラック＝43, 41, 35）が 1bit の狂いもなくゲームに渡されます**。

### 3. 右側スクリーン（テレビ・暖炉）の正体と見え方の差異
- **テレビ画面**:
  - 正体はスートブラックに染められた 2 枚の **`Stage Panel` (ステージパネル)**。
  - 上記の白化バグにより、黒い画面にならず **巨大な薄いグレーの壁** になっていました。
- **テレビ画面上の紫の文字**:
  - 正体は **`Recollection Sword Stand` (記憶の剣架 / マンダヴィルウェポンの紫に輝く刀掛け台)**。
  - 背景のステージパネルが白化したことで、刀の光が白飛びに埋もれてしまっていました。
- **暖炉の炎**:
  - 正体は床下 Y=-0.45m に埋め込まれた 3 本の **`Iron Torch` (松明)**。
  - ハウジングでは松明先端の火の粉パーティクル（VFX）が床の上に出て炎に見えていますが、Stagehand の BgObject は 3D メッシュ（.mdl）のみを描画する仕様であるため、床下に埋まったメッシュの先端は見えず、VFX も出ません。
- **暖炉の奥の縦ルーバー**:
  - 正体は **`Wood Slat Partition` (ウッドスラット・パーティション)**。
  - これもスートブラック染色ですが白化していたため、手前のステージパネルとコントラストがつかず同化していました。

### 4. 床（黒い大理石風タイル）の仕様
- 理想画像の黒大理石床は、家具ではなく、MakePlace の `interiorFixture` に記録されている **「Marble Flooring（大理石フローリング、ID 8014）」というハウジングの内装材（建材）** です。
- Stagehand は内装建材（壁紙・床材）の変更機能を持たないため、ユーザーのハウスの元々の床（木製フローリング）が露出しています。
- ハウスの内装リフォームで床材を「大理石フローリング」に変更していただくことで、理想画像と全く同じ床になります。

---

## 修正提案

### 1. [FurnitureModelResolver.cs](file:///c:/Users/RYO/Desktop/Brio%20to%20Stagehand/FurnitureModelResolver.cs)
- モデルキーからの探索候補に、末尾バリエーション（`a`, `b`）を追加：
  - `bgcommon/hou/{location}/general/{model}/bgparts/{prefix}_b0_m{model}a.mdl`
  - `bgcommon/hou/{location}/general/{model}/bgparts/{prefix}_b0_m{model}b.mdl`
- `indoor`（室内）で見つからない場合、自動的に反対側の `outdoor`（庭具 `gar_b0_m{model}.mdl`）をフォールバック探索する機能を追加（Chilled Red 等の救済）。
- 逆に `outdoor` で見つからない場合も `indoor`（`fun_b0_m{model}.mdl`）を探索。
- 全ての候補が存在しない場合は、実在しないパスを返さずに `return false` とし、正しくスキップリストに記録するよう安全化。

### 2. [LayoutToStagehandConverter.cs](file:///c:/Users/RYO/Desktop/Brio%20to%20Stagehand/LayoutToStagehandConverter.cs)
- 染色計算式（`TryParseColor`）を修正：
  ```csharp
  // Stagehand receives Linear RGB [0..1] and computes:
  //   byteColor = (byte)(MathF.Sqrt(Linear) * 255)
  // Therefore, inputting (sRGB)^2 cancels out MathF.Sqrt perfectly,
  // ensuring the game receives the exact original byte color (e.g. 43 for soot black).
  var rLin = MathF.Pow(r / 255f, 2f);
  var gLin = MathF.Pow(g / 255f, 2f);
  var bLin = MathF.Pow(b / 255f, 2f);
  color = new Vector4(rLin, gLin, bLin, 1f);
  ```
- 未染色家具は `Vector4.Zero` を維持（デフォルトテクスチャを保持し、白飛びを防止）。

---

## 検証計画

1. **ビルド検証**:
   - `dotnet build` を実行し、コンパイルエラーや警告がないことを確認。
2. **モデルパス解決の完全性検証**:
   - `CL03 Meridian Neue L.json` に含まれる全 79 種類の家具モデルについて、SqPack 内の実在チェックを行い、**実在しないモデル（MISSING）が 0 件になること** を確認。
3. **染色計算の数学的検証**:
   - スートブラック（`#2B2923` = 43, 41, 35）が `MathF.Sqrt` を経て正確に `byteColor = (43, 41, 35)` になることを検証。
4. **バージョン更新・リリース**:
   - バージョンを `1.0.14.0` に更新し、Git コミット＆プッシュを実行。
   - `docs/housing_to_stagehand/` へのドキュメント同期。
