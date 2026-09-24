# 修正内容の確認 (Walkthrough) - v1.0.14.0

## 実施した修正

### 1. 消失していた家具（計 12 個）の完全救済
- **背景**:
  `CL03 Meridian Neue L.json` の全 596 オブジェクト（79 種類）をゲーム内部データ（SqPack インデックス）と照合した結果、4 種類の家具（計 12 個）のモデルパスが存在せず、Stagehand のスポーン処理で拒否（消失）されていました。
- **原因と修正**:
  - `Queen's Rest` (7個): `fun_b0_m1026.mdl` ではなく **`fun_b0_m1026a.mdl`**（末尾に `a` が付く）であったため、末尾バリエーション探索（`a`, `b`）を追加。
  - `Chilled Red` (3個) / `Starlight Dodo` (1個) / `Riviera Table Chronometer` (1個): 室内家具シートに登録されているものの、ゲーム内アセットは **`outdoor`（庭具フォルダ `gar_b0_mXXXX.mdl`）** に格納されていたため、indoor/outdoor 相互の自動フォールバック探索を追加。
- **検証結果**:
  全 79 種類の家具モデルがゲームデータ内に 100% 存在することが確認され、**MISSING パスは 0 件** となりました。

### 2. 染色パイプラインの完全修正（白化・薄まりの根本解消）
- **背景**:
  黒色（スートブラック `#2B2923`）に染色されたはずのステージパネルやウッドスラット、黒板などが、明るいベージュ〜グレーに白化し、コントラストが失われていました。
- **原因と修正**:
  Stagehand の `LiveBgObject.set_DyeColor` は渡された `DyeColor` を Linear RGB とみなし、`MathF.Sqrt(val) * 255` を計算して `ByteColor` に変換します。
  そのため、`LayoutToStagehandConverter.cs` で `MathF.Pow(r / 255f, 2f)`（リニア値）を渡すよう修正しました。
  これにより、Stagehand 内の `MathF.Sqrt` と完全に相殺し、**元の HEX カラー（スートブラック＝43, 41, 35）が 1bit の狂いもなくビット完全（100% bit-exact）でゲームに渡される** ようになりました。
  未染色の家具は `Vector4.Zero` を維持し、白飛びを防止しています。

### 3. 右側スクリーン・暖炉・床の仕様整理
- **テレビ画面**:
  正体はスートブラックに染められた 2 枚の `Stage Panel` です。白化バグ解消により、引き締まった真っ黒なスクリーンとして描画されます。
- **テレビ中央の紫の文字**:
  正体は `Recollection Sword Stand` (記憶の剣架 / マンダヴィルウェポンの紫に輝く刀掛け台) です。テレビ画面が黒く引き締まることで、紫のクリスタル発光が鮮明に浮かび上がります。
- **暖炉の炎**:
  正体は床下 Y=-0.45m に埋め込まれた 3 本の `Iron Torch` (松明) です。Stagehand の BgObject は 3D メッシュ（.mdl）のみを描画する仕様のため、床下に埋まったメッシュの先端は見えず、VFX（火の粉）も出ません。
- **床**:
  理想画像の黒大理石床は、家具ではなく `interiorFixture` に含まれる「Marble Flooring（大理石フローリング、ID 8014）」という内装建材です。ハウスの内装リフォームで床材を「大理石フローリング」に変更していただくことで、理想画像と全く同じ床になります。

---

## リリース情報
- バージョン: **v1.0.14.0**
- 更新対象: `FurnitureModelResolver.cs`, `LayoutToStagehandConverter.cs`, `package.json`, `HoToSta.json`, `repo.json`, `HoToSta.csproj`, `CHANGELOG.md`
