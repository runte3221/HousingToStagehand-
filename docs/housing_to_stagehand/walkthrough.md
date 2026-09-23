# [Walkthrough] HoToSta プラグインの実装および染色反映修正完了

MakePlace 形式のハウジングレイアウトを読み込み、Stagehand 用のステージ（`.json`）への変換および Stagehand IPC 経由での直接ゲーム内スポーンを可能にする Dalamud プラグイン **HoToSta**（旧: HousingToStagehand）の実装・改名および家具染色反映の不具合修正が完了しました。

---

## 主な実装・修正内容

### 1. 家具染色反映の不具合修正（v1.0.10）
- **現象**: MakePlace で黒やダークトーンに染色した家具が、Stagehand 上で白木や明るいグレー（ほぼ無染色）として表示されてしまう。
- **原因の特定**:
  - Stagehand の `LiveBgObject.cs` において、`DyeColor` の各成分に対して `MathF.Sqrt(value.X/Y/Z) * 255` を適用してゲーム内の sRGB バイト値（0〜255）を復元する仕様となっていた。
  - HoToSta 側で生の sRGB 正規化値（例: 黒色 R=50/255=0.196）をそのまま `DyeColor` に渡していたため、Stagehand 側で `MathF.Sqrt(0.196) * 255 = 113` と計算され、暗い色が明るいライトグレーに化けてしまっていた。
- **修正内容**:
  - `LayoutToStagehandConverter.cs` の `TryParseColor` を改修し、sRGB の正規化値を 2 乗（`sRGB * sRGB`）してリニア空間に変換した上で Stagehand に渡すよう修正。
  - これにより、Stagehand が `MathF.Sqrt` した際に正確な元の染色カラー（スートブラック等）が復元され、黒や濃い色の家具が正しくゲーム内に反映されるよう修正。

### 2. プラグイン名称・設定の変更（v1.0.9）
- **プラグイン名**: `HoToSta`
- **チャットコマンド**: `/hotosta` または `/h2s`
- **内部アセンブリ名 / Namespace**: `HoToSta`
- **説明文の削除**: `README.md`、Dalamud マニフェスト（`HoToSta.json` / `repo.json`）、GitHub Release の詳細説明を削除し最小限に設定。

### 3. プロジェクト基盤
- **[HoToSta.csproj](file:///c:/Users/RYO/Desktop/Brio%20to%20Stagehand/HoToSta.csproj)**:
  - `Dalamud.NET.Sdk/15.0.0` をベースに `net10.0-windows` ターゲット。
  - 公式 NuGet パッケージ `Stagehand.Definitions` (v0.4.15) および `Stagehand.Api` (v0.4.15) を参照。
- **[HoToSta.json](file:///c:/Users/RYO/Desktop/Brio%20to%20Stagehand/HoToSta.json)**: Dalamud プラグインマニフェスト（API Level 15、Name: `HoToSta`）。

### 4. コア変換ロジック
- **[Layout.cs](file:///c:/Users/RYO/Desktop/Brio%20to%20Stagehand/Layout.cs)**: MakePlace レイアウト JSON のデシリアライズモデルおよび現在のハウスサイズ判定処理。
- **[FurnitureModelResolver.cs](file:///c:/Users/RYO/Desktop/Brio%20to%20Stagehand/FurnitureModelResolver.cs)**: Lumina シート（`HousingFurniture` / `HousingYardObject`）から家具 ItemId を解析し、Stagehand が配置する `.mdl`（3Dモデル）パスへの解決処理。
- **[LayoutToStagehandConverter.cs](file:///c:/Users/RYO/Desktop/Brio%20to%20Stagehand/LayoutToStagehandConverter.cs)**:
  - MakePlace 座標系からのスケール除算（100分の1）、Y/Zスワップ、クォータニオンの符号反転（$-X, -Z, -Y, W$）による座標補正。
  - MakePlace の HEX 染色コードをリニア空間 `BgObjectDefinition.DyeColor`（Vector4）へ変換。
  - 各家具を `BgObjectDefinition` としてインスタンス化し、`StageDefinition` を生成。

### 5. Stagehand 連携層
- **[StagehandExporter.cs](file:///c:/Users/RYO/Desktop/Brio%20to%20Stagehand/StagehandExporter.cs)**: ユーザーの `Documents\Stages\` フォルダへ `StageDefinition.WriteToJSONStream()` を用いて標準 JSON 形式で出力。
- **[StagehandIpcClient.cs](file:///c:/Users/RYO/Desktop/Brio%20to%20Stagehand/StagehandIpcClient.cs)**: `StagehandApi` の IPC クライアントを使用し、一時ステージ（`HoToSta_LivePreview`）としてゲーム内でワンクリック即時スポーン・消去を実行。

### 6. UI & プラグイン本体
- **[Configuration.cs](file:///c:/Users/RYO/Desktop/Brio%20to%20Stagehand/Configuration.cs)**: 設定の永続化。
- **[MainWindow.cs](file:///c:/Users/RYO/Desktop/Brio%20to%20Stagehand/MainWindow.cs)**: ファイル選択、変換オプション、ファイル保存ボタン、IPC 即時スポーンボタンを備えた ImGui ウィンドウ。
- **[Plugin.cs](file:///c:/Users/RYO/Desktop/Brio%20to%20Stagehand/Plugin.cs)**: コマンド `/hotosta`, `/h2s` の登録とライフサイクル管理。

### 7. CI/CD & ドキュメント
- **[.github/workflows/build.yml](file:///c:/Users/RYO/Desktop/Brio%20to%20Stagehand/.github/workflows/build.yml)**: GitHub Actions での自動ビルド＆公式 `latest.zip` 配布パッケージ作成ワークフロー。
- **[README.md](file:///c:/Users/RYO/Desktop/Brio%20to%20Stagehand/README.md)** / **[CHANGELOG.md](file:///c:/Users/RYO/Desktop/Brio%20to%20Stagehand/CHANGELOG.md)**: 変更履歴。
- **[repo.json](file:///c:/Users/RYO/Desktop/Brio%20to%20Stagehand/repo.json)**: Dalamud カスタムリポジトリ登録用マニフェスト。
  - URL: `https://raw.githubusercontent.com/runte3221/HousingToStagehand-/main/repo.json`

---

## 使い方（検証手順）

1. ゲーム内で `/hotosta` または `/h2s` と入力してウィンドウを表示。
2. MakePlace のレイアウト JSON を選択して **Load Layout** をクリック。
3. **Save as Stagehand Stage (.json)** をクリックして `Documents\Stages\` に保存（または **Spawn via Stagehand (IPC)** を押して直接スポーン）。
4. Stagehand のウィンドウでステージをリロードすると、黒やダークトーンの染色が正確に反映されます。
