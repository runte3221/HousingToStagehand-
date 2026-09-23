# [Implementation Plan] HousingToStagehand プラグインの作成

MakePlace 形式のハウジングレイアウト JSON ファイルを読み込み、家具を **Stagehand** のステージ定義（`StageDefinition`）に変換する Dalamud プラグイン **「HousingToStagehand」** を開発します。
生成したステージは Stagehand のステージ保存フォルダ（`Documents\Stages\`）に JSON として保存できるほか、Stagehand の公式 IPC API を通じてインゲームで即座に一時ステージとしてスポーン・表示させることができます。

## User Review Required

> [!IMPORTANT]
> **GitHub リポジトリの登録先について**
> ローカルで Git 初期化を行いコミットまで行いますが、リモートの GitHub へプッシュ（`git push`）するためには、プッシュ先の GitHub リポジトリ（例: `https://github.com/runte/HousingToStagehand.git`）が必要です。
> GitHub 上で空のリポジトリを作成してその URL をご教示いただくか、または SSH / HTTPS でのプッシュ先を指定していただければ設定いたします。

> [!NOTE]
> **プロジェクト名称について**
> ワークスペースのフォルダ名は `Brio to Stagehand` となっていますが、機能の実態に合わせてプラグイン名およびリポジトリ名を **`HousingToStagehand`** と命名することを提案します。（Brio を介さず、MakePlace のハウジングデータを直接 Stagehand に渡すためです）

---

## Proposed Changes

### プロジェクト基盤
- **`.gitignore`**: `bin/`, `obj/`, `_repos/`（調査用フォルダ）, `.vs/` 等を除外。
- **`HousingToStagehand.csproj`**:
  - `Dalamud.NET.Sdk` を採用し、Dalamud API 最新版に適合。
  - NuGet パッケージ:
    - `Stagehand.Definitions` (v0.4.x): StageDefinition, BgObjectDefinition の公式データ構造
    - `Stagehand.Api` (v1.x): Dalamud IPC クライアント
- **`HousingToStagehand.json`**: Dalamud プラグインマニフェスト（名前、作者、説明、APIバージョンなど）。

---

### コア変換ロジック
#### [NEW] `Layout.cs`
- MakePlace の layout.json（`houseSize`, `interiorFurniture`, `exteriorFurniture`, `transform`, `properties.color` 等）をデシリアライズするデータ構造。
- ハウスサイズの自動判定（`HouseSizeDetector`）。

#### [NEW] `FurnitureModelResolver.cs`
- Lumina のゲーム内 Excel シート（`HousingFurniture`, `HousingYardObject`）から、家具 `itemId` に対応する `modelKey`（4桁数値）を取得。
- Stagehand が利用する `.mdl`（3Dメッシュモデル）のパスを構築：
  - 屋内: `bgcommon/hou/indoor/general/{modelKey:D4}/bgparts/fun_b0_m{modelKey:D4}.mdl`
  - 屋外: `bgcommon/hou/outdoor/general/{modelKey:D4}/bgparts/gar_b0_m{modelKey:D4}.mdl`
  - Dalamud の `IDataManager` でファイル存在を確認し、存在しない場合のフォールバックを実装。

#### [NEW] `LayoutToStagehandConverter.cs`
- **座標変換**:
  - スケール除算（実寸 100 分の 1 にスケールバック）
  - Y/Z 軸スワップ（MakePlace の Z 軸をゲーム内の Y 軸 / 高さへ、Y 軸を Z 軸 / 奥行きへ）
  - クォータニオンの符号反転（左右系反転の補正: $-X, -Z, -Y, W$）
- **染色（DyeColor）変換**:
  - MakePlace の HEX カラーコード（`#RRGGBB`）を RGBA の `Vector4` に変換。
- **StageDefinition の生成**:
  - 各家具を `BgObjectDefinition` としてインスタンス化し、GUID をキーとして `StageDefinition.Objects` に格納。
  - `StageInfo`（ステージ名、作者名、TerritoryType 等）のメタデータを付与。

---

### Stagehand 連携層
#### [NEW] `StagehandExporter.cs`
- ユーザーのドキュメントフォルダ（`Environment.GetFolderPath(SpecialFolder.MyDocuments)\Stages\`）を自動検出。
- `StageDefinition.WriteToJSONStream()` を用いてインデント付きの標準 Stagehand JSON ファイルを出力。

#### [NEW] `StagehandIpcClient.cs`
- `StagehandApi.CreateIpcClient(pluginInterface)` を使用。
- Stagehand が有効化されているかチェック。
- `TryCreateOrUpdateTemporaryStageWithTransform()` および `TrySetTemporaryStageVisible()` を呼び出し、ファイル保存を介さずゲーム内で即時出現させる。
- 不要になった一時ステージの破棄（`TryDestroyTemporaryStage()`）。

---

### UI & プラグイン本体
#### [NEW] `Configuration.cs`
- 前回のファイルパス、各種変換オプション（屋内/屋外、染色適用、プレイヤアンカー配置など）の永続化。

#### [NEW] `MainWindow.cs`
- ImGui による直感的な設定画面：
  - レイアウトファイルの選択ダイアログ & パス入力
  - 読み込んだレイアウト情報のプレビュー（家具数、外装数、スキップ数、ハウスサイズ）
  - 変換オプション（屋内・屋外トグル、染色トグル、プレイヤー位置を基準にするか原点にするか）
  - アクションボタン：
    - **「Save as Stagehand Stage (.json)」**: ステージファイル書き出し
    - **「Spawn Directly (IPC)」**: ゲーム内で即時表示
    - **「Clear Spawned Stage」**: 即時表示したステージの消去

#### [NEW] `Plugin.cs`
- コマンド `/housingtostagehand`, `/h2s` の登録。
- UI ビルダーへのウィンドウ登録、IPC クライアントの破棄管理。

---

### リポジトリ管理・CI/CD
- **`README.md`**: 日本語・英語での詳細な説明書、インストール手順、使い方。
- **`CHANGELOG.md`**: バージョン履歴（初期リリース v1.0.0）。
- **`.github/workflows/build.yml`**: GitHub Actions による Dalamud プラグインの自動ビルドとリリースパッケージ（zip / repo.json）生成。
- **プロジェクトドキュメント**: ルールに従い `docs/housing_to_stagehand/` フォルダへ `task.md`, `implementation_plan.md`, `walkthrough.md` を保存。

---

## Verification Plan

### 自動ビルド / コード品質検証
- C# コードの構文・参照チェック（C# 12 / .NET 9 準拠、null 安全性）。
- GitHub Actions ワークフロー定義の正当性検証。

### 手動検証（動作確認手順）
1. XIVLauncher の Dalamud DevPlugins（または設定したカスタムリポジトリ）に本プラグインを配置。
2. `/housingtostagehand` コマンドでウィンドウを開く。
3. サンプル MakePlace レイアウト `.json` を選択。
4. 「Save as Stagehand Stage」をクリックし、`Documents\Stages\` に `.json` が正しく生成されることを確認。
5. Stagehand の UI から当該ステージを開き、家具モデル、配置座標、回転、染色が正確に反映されていることを確認。
6. 「Spawn Directly (IPC)」をクリックし、Stagehand 上で一時ステージとしてワンクリックで出現・消去できることを確認。
