# タスクリスト: 染色反映の完全修正と消失家具の完全解決

## 調査・原因究明 (完了)
- [x] SqPack（010000.win32.index / dat1）の実データ解析による家具消失の完全解明:
  - `Queen's Rest` (7個): `fun_b0_m1026a.mdl` (末尾に `a` が必要)
  - `Chilled Red` (3個): `outdoor/general/0253/bgparts/gar_b0_m0253.mdl` (室内家具だが outdoor フォルダにある)
  - `Starlight Dodo` (1個): `outdoor/general/0293/bgparts/gar_b0_m0293.mdl` (室内家具だが outdoor フォルダにある)
  - `Riviera Table Chronometer` (1個): `outdoor/general/0116/bgparts/gar_b0_m0116.mdl` (室内家具だが outdoor フォルダにある)
- [x] Stagehand 内部の `LiveBgObject.set_DyeColor` の設計仕様の完全解明:
  - Stagehand は `MathF.Sqrt(value) * 255` を計算して `ByteColor`（0..255）を生成している。
  - したがって、入力 `DyeColor` は **Linear RGB [0..1]** でなければならず、sRGB をそのまま渡すと `Sqrt` で値が約2.5倍に跳ね上がり白化（薄いグレー化）する。
- [x] テレビ画面・暖炉の正体の完全特定:
  - テレビ画面の正体は黒く染めた `Stage Panel` (2枚)。白化のため白い壁に見えていた。
  - テレビ中央の発光物は `Recollection Sword Stand` (記憶の剣架)。白飛びで埋もれていた。
  - 暖炉の炎は床下 Y=-0.45m に埋め込まれた `Iron Torch` (松明 3本)。Stagehand の BgObject はメッシュのみのためパーティクル（炎）が出ない。
- [x] 床の正体の完全特定:
  - 黒大理石床は置物家具ではなく、`interiorFixture` の「大理石フローリング（Marble Flooring, ID 8014）」という内装建材。

## 実装計画と実行 (予定)
- [ ] `FurnitureModelResolver.cs` のモデルパス解決の強化:
  - 末尾 `a`/`b` バリエーション探索の追加
  - `indoor` で見つからない場合の `outdoor`（`gar_b0_m...`）自動フォールバック探索
  - `outdoor` で見つからない場合の `indoor`（`fun_b0_m...`）自動フォールバック探索
  - ファイルが実在しない場合は `false` を返し正しくスキップリストに計上する安全化
- [ ] `LayoutToStagehandConverter.cs` の染色計算式の修正:
  - 染色家具の `DyeColor` を正確なリニア色 `MathF.Pow(r / 255f, 2f)` に修正
  - 未染色家具は `Vector4.Zero` を維持
- [ ] `dotnet build` によるコンパイル検証
- [ ] 変換シミュレーション検証（全 79 パスが実在することの確認）
- [ ] バージョン更新 (v1.0.14.0):
  - `package.json`
  - `HoToSta.json`
  - `repo.json`
  - `HoToSta.csproj`
  - `CHANGELOG.md`
- [ ] Git コミット＆プッシュ（単一コマンド）
- [ ] `docs/housing_to_stagehand/` へのドキュメント同期
