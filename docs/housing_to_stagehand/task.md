# タスクリスト: 染色反映の修正と家具消失の原因究明・修正

## 完了したタスク
- [x] HousingToBrio (画像1) と Stagehand (画像2) の比較分析
- [x] Stagehand 内部の `LiveBgObject.set_DyeColor` および FFXIV `BgObject.TrySetStainColor` のパイプライン再解析
- [x] 未染色の家具（Stone Partition等）に `Vector4.One`（真っ白）が強制適用されて白飛びしていた原因の特定
- [x] 染色済み家具に対する ガンマ/リニア二重変換（sRGBを2乗して渡していた）の解消
- [x] 右側スクリーン上部に配置されている家具（Butterfly Specimen / 蝶の標本 4個）および暖炉の火（Iron Torch 3個）の特定と調査
- [x] `LayoutToStagehandConverter.cs` の修正:
  - 未染色家具の `DyeColor` を `Vector4.Zero` に変更（デフォルトテクスチャ保持）
  - 染色家具の `DyeColor` を標準 sRGB [0..1] のまま渡すように変更
- [x] バージョン更新 (v1.0.13.0)、`package.json`、`HoToSta.json`、`repo.json`、`CHANGELOG.md` 更新
- [x] Git コミットおよびプッシュ
