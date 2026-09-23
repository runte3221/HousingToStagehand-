# 修正内容の確認 (Walkthrough) - v1.0.13

## 1. 判明した根本原因の総括

### ① 色の違い（Stone Partition などの壁が真っ白に白飛びしていた原因）
- **未染色家具への白色強制適用**:
  - `CL03 Meridian Neue L.json` 内の `Stone Partition`（41個）など未染色（カラー指定なし）の家具に対し、従来のコードでは `DyeColor` の初期値として `Vector4.One`（真っ白: RGB 1.0, 1.0, 1.0）を設定していました。
  - Stagehand の `LiveBgObject.set_DyeColor` は `DyeColor` を受け取ると、ゲーム内の `BgObject.TrySetStainColor` を呼び出します。
  - その結果、未染色の家具に対しても「真っ白（RGB 255, 255, 255）の染色」が強制的に焼き付けられ、奥の石壁や中段の壁が真っ白に光り輝く（白飛びする）現象が発生していました。
  - **解決策**:
    - Stagehand の `LiveBgObject` は初期化時に `_dyeColor = Vector4.Zero` となっています。
    - 未染色の家具に `DyeColor = Vector4.Zero` を指定することで、`set_DyeColor` 内のガード条件 `if (_dyeColor != val)` により `TrySetStainColor` が一切呼ばれず、**ゲーム本来の未染色テクスチャ（重厚で暗い石の質感）が100%維持**されます。

### ② 染色済み家具の色がくすむ・意図しない色になる原因
- **ガンマ・リニアの二重変換**:
  - Stagehand 内部の `LiveBgObject.set_DyeColor` は：
    `MathF.Sqrt(val) * 255` でバイト値（ByteColor）に変換。
  - その後ゲーム側（`FFXIVClientStructs.BgObject.TrySetStainColor`）が：
    `(ByteColor / 255) ^ 2` で2乗してシェーダー用リニアカラーに戻します。
  - つまり、Stagehand の内部処理とゲーム側の処理は **互いに打ち消し合う（Sqrt(val)^2 = val）** ように設計されており、Stagehand の公式UIエディタも ImGui のカラーピッカーの sRGB 値（0.0〜1.0）をそのまま渡しています。
  - 前回修正でプラグイン側であらかじめ2乗した値を渡してしまっていたため、シェーダーに渡る値が「二重に2乗」され、色が極端に狂っていました。
  - **解決策**: `TryParseColor` で2乗せず、標準の sRGB [0..1] の値をそのまま渡すように戻しました。

---

### ③ 画像右側のスクリーン上の家具消失および暖炉の火の消失原因
1. **スクリーン上の家具（紫色の記号）**:
   - 正体は **`Butterfly Specimen (ID 27276)`（蝶の標本）4個** です。
   - 黒板（Classroom Blackboard）の `attachments`（卓上小物・壁掛け小物）として配置されていました。
   - Stagehand の生成ステージ JSON 内には 4 つとも正常に出力されていますが、
     - 未染色なのに `DyeColor = (1, 1, 1, 1)`（白）が塗られて羽のエミッシブ（自発光）テクスチャが白飛びして消えていたこと
     - 黒板が 2 枚重ね（手前 Z=-4.3, 奥 Z=-4.7）で配置されており、蝶がその間（Z=-4.5）に配置されているため、黒板が真っ黒に塗られたことで視覚的に埋もれていたこと
     が原因です。未染色状態（`Vector4.Zero`）に戻すことで、本来の青紫色に自発光するマテリアルが復元されます。
2. **暖炉の火**:
   - 正体は **`Iron Torch (ID 32219)`（アイウントーチ）3個** です。
   - 鉄の松明自体は Stagehand 内にスポーンされていますが、**「炎（火）」はゲームの SGB に含まれる VFX（パーティクルエフェクト）** です。
   - Stagehand の `BgObject` は `.mdl`（純粋な3Dメッシュ）しか描画できない仕様のため、炎のパーティクルは表示されません（Brio は SGB 単位でゲームのレイアウトインスタンスを生成するため VFX も表示されます）。

---

## 2. 変更内容一覧

| ファイル | 変更内容 |
|---|---|
| `LayoutToStagehandConverter.cs` | 未染色家具の `DyeColor` を `Vector4.Zero` に変更（本来のテクスチャを保持）。`TryParseColor` で sRGB [0..1] をそのまま渡すよう修正 |
| `package.json` | バージョンを `1.0.13` に更新 |
| `HoToSta.json` | `AssemblyVersion` を `1.0.13.0` に更新 |
| `repo.json` | `AssemblyVersion` を `1.0.13.0` に更新 |
| `CHANGELOG.md` | v1.0.13 の変更点を追記 |
