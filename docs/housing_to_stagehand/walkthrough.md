# [Walkthrough] HoToSta プラグインの実装および名称変更完了

MakePlace 形式のハウジングレイアウトを読み込み、Stagehand 用のステージ（`.json`）への変換および Stagehand IPC 経由での直接ゲーム内スポーンを可能にする Dalamud プラグイン **HoToSta**（旧: HousingToStagehand）の実装・改名が完了しました。

---

## 主な実装・改名内容

### 1. プラグイン名称・設定の変更
- **プラグイン名**: `HoToSta`
- **チャットコマンド**: `/hotosta` または `/h2s`
- **内部アセンブリ名 / Namespace**: `HoToSta`
- **説明文の削除**: `README.md`、Dalamud マニフェスト（`HoToSta.json` / `repo.json`）、GitHub Release の詳細説明を削除し最小限に設定。

### 2. プロジェクト基盤
- **[HoToSta.csproj](file:///c:/Users/RYO/Desktop/Brio%20to%20Stagehand/HoToSta.csproj)**:
  - `Dalamud.NET.Sdk/15.0.0` をベースに `net10.0-windows` ターゲット。
  - 公式 NuGet パッケージ `Stagehand.Definitions` (v0.4.15) および `Stagehand.Api` (v0.4.15) を参照。
- **[HoToSta.json](file:///c:/Users/RYO/Desktop/Brio%20to%20Stagehand/HoToSta.json)**: Dalamud プラグインマニフェスト（API Level 15、Name: `HoToSta`）。

### 3. コア変換ロジック
- **[Layout.cs](file:///c:/Users/RYO/Desktop/Brio%20to%20Stagehand/Layout.cs)**: MakePlace レイアウト JSON のデシリアライズモデルおよび現在のハウスサイズ判定処理。
- **[FurnitureModelResolver.cs](file:///c:/Users/RYO/Desktop/Brio%20to%20Stagehand/FurnitureModelResolver.cs)**: Lumina シート（`HousingFurniture` / `HousingYardObject`）から家具 ItemId を解析し、Stagehand が配置する `.mdl`（3Dモデル）パスへの解決処理。
- **[LayoutToStagehandConverter.cs](file:///c:/Users/RYO/Desktop/Brio%20to%20Stagehand/LayoutToStagehandConverter.cs)**:
  - MakePlace 座標系からのスケール除算（100分の1）、Y/Zスワップ、クォータニオンの符号反転（$-X, -Z, -Y, W$）による座標補正。
  - MakePlace の HEX 染色コードを `BgObjectDefinition.DyeColor`（Vector4）へ変換。
  - 各家具を `BgObjectDefinition` としてインスタンス化し、`StageDefinition` を生成。

### 4. Stagehand 連携層
- **[StagehandExporter.cs](file:///c:/Users/RYO/Desktop/Brio%20to%20Stagehand/StagehandExporter.cs)**: ユーザーの `Documents\Stages\` フォルダへ `StageDefinition.WriteToJSONStream()` を用いて標準 JSON 形式で出力。
- **[StagehandIpcClient.cs](file:///c:/Users/RYO/Desktop/Brio%20to%20Stagehand/StagehandIpcClient.cs)**: `StagehandApi` の IPC クライアントを使用し、一時ステージ（`HoToSta_LivePreview`）としてゲーム内でワンクリック即時スポーン・消去を実行。

### 5. UI & プラグイン本体
- **[Configuration.cs](file:///c:/Users/RYO/Desktop/Brio%20to%20Stagehand/Configuration.cs)**: 設定の永続化。
- **[MainWindow.cs](file:///c:/Users/RYO/Desktop/Brio%20to%20Stagehand/MainWindow.cs)**: ファイル選択、変換オプション、ファイル保存ボタン、IPC 即時スポーンボタンを備えた ImGui ウィンドウ。
- **[Plugin.cs](file:///c:/Users/RYO/Desktop/Brio%20to%20Stagehand/Plugin.cs)**: コマンド `/hotosta`, `/h2s` の登録とライフサイクル管理。

### 6. CI/CD & ドキュメント
- **[.github/workflows/build.yml](file:///c:/Users/RYO/Desktop/Brio%20to%20Stagehand/.github/workflows/build.yml)**: GitHub Actions での自動ビルド＆公式 `latest.zip` 配布パッケージ作成ワークフロー。
- **[README.md](file:///c:/Users/RYO/Desktop/Brio%20to%20Stagehand/README.md)** / **[CHANGELOG.md](file:///c:/Users/RYO/Desktop/Brio%20to%20Stagehand/CHANGELOG.md)**: 変更履歴。
- **[repo.json](file:///c:/Users/RYO/Desktop/Brio%20to%20Stagehand/repo.json)**: Dalamud カスタムリポジトリ登録用マニフェスト。
  - URL: `https://raw.githubusercontent.com/runte3221/HousingToStagehand-/main/repo.json`

---

## 使い方（検証手順）

1. ゲーム内で `/hotosta` または `/h2s` と入力してウィンドウを表示。
2. MakePlace のレイアウト JSON を選択して **Load Layout** をクリック。
3. プレビュー情報を確認し、**Save as Stagehand Stage (.json)** をクリックして `Documents\Stages\` に保存。
4. Stagehand のウィンドウを開くと、一覧に保存したステージが表示され、ロード可能。
5. Stagehand が有効な場合は **Spawn via Stagehand (IPC)** を押すことで、ファイル保存を経由せずにその場で即座に家具を出現させることができます。
