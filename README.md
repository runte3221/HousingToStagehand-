# Housing To Stagehand

An FFXIV Dalamud plugin that loads [MakePlace](https://makeplace.app/) housing layout files (`.json`) and converts them into [Stagehand](https://github.com/UniversalConquistador/Stagehand) stages (`.json`), or spawns them directly in-game via Stagehand's IPC API!

Now you can decorate venues, void stages, and photoshoot scenes with complete housing designs anywhere in the world—without owning the house or placing furniture manually.

---

## Install / インストール方法

1. ゲーム内で `/xlsettings` を開き、**Experimental（高度な設定 / 実験的機能）** タブを選択します。
2. **Custom Plugin Repositories（カスタムプラグインリポジトリ）** に以下の URL を追加し、**Save（保存）** をクリックします：
   ```
   https://raw.githubusercontent.com/runte3221/HousingToStagehand-/main/repo.json
   ```
3. ゲーム内で `/xlplugins` を開き、プラグイン一覧から **HousingToStagehand** を検索してインストールします。

---

## Features / 主な機能

- **Direct Stagehand Export**: Converts MakePlace layout files into standard Stagehand `.json` files saved directly into your `Documents\Stages\` folder.
- **Direct In-Game Spawning (IPC)**: Spawns the entire layout as a temporary Stage in real-time with one click—no file reloading necessary!
- **Accurate Coordinate & Rotation Conversion**: Properly corrects the Y/Z-axis swap and quaternion handedness flipping between MakePlace and FFXIV scene space.
- **Dye & Color Support**: Automatically translates MakePlace dye colors (hex values) into Stagehand dye stains.
- **Player Anchoring**: Option to anchor the loaded layout to your current character position.

---

## Requirements / 前提条件

- [XIVLauncher / Dalamud](https://github.com/goatcorp/XIVLauncher)
- [Stagehand Plugin](https://github.com/UniversalConquistador/Stagehand) installed and enabled.

---

## Usage / 使い方

1. Type `/housingtostagehand` or `/h2s` in the game chat to open the plugin window.
2. Click **Browse...** to select your MakePlace `.json` layout file and click **Load Layout**.
3. Configure conversion options (Include interior/exterior, Apply dye colors, Anchor to player).
4. Choose an action:
   - **Save as Stagehand Stage (.json)**: Writes the stage file to your `Documents\Stages\` folder so it appears permanently in Stagehand's stage list.
   - **Spawn via Stagehand (IPC)**: Instantly spawns all furniture models around you as a temporary stage!
   - **Clear Spawned Stage (IPC)**: Despawns the temporary stage.

---

## Limitations / 制限事項

- Fixtures (structural walls, flooring, roof, exterior doors, etc.) do not have individual 3D coordinate transforms in MakePlace layouts, so they are not converted (furniture only).
- Furniture items that do not map to standard game model sheets are skipped and listed in the summary.

---

## Building from Source / ビルド手順

```bash
dotnet build HousingToStagehand.csproj -c Release
```

Load the resulting `.dll` from `bin/Release/` as a Dalamud DevPlugin.

---

## License & Credits

- [Stagehand](https://github.com/UniversalConquistador/Stagehand) by UniversalConquistador
- [HousingToBrio](https://github.com/JTayGang/MeowyUtils/tree/main/HousingToBrio) by JTayGang
- [MakePlace](https://makeplace.app/)
