# Changelog

All notable changes to this project will be documented in this file.

## [1.0.14] - 2026-09-24

### Fixed
- Enhanced furniture model resolution by supporting variant suffixes (e.g. `fun_b0_m1026a.mdl` for Queen's Rest) and automatic cross-location fallbacks between indoor and outdoor asset folders (e.g. Chilled Red, Starlight Dodo, Riviera Table Chronometer). Resolves missing furniture issues.
- Corrected dye color calculation by passing linear RGB (`MathF.Pow(c / 255f, 2f)`) into Stagehand, perfectly cancelling Stagehand's internal `MathF.Sqrt` and ensuring exact 100% bit-accurate original colors (e.g. soot black `#2B2923`) are delivered to the game engine without washed-out gray rendering.
- Prevented non-existent model paths from being emitted to Stagehand stages by returning false when all file existence checks fail.

## [1.0.13] - 2026-09-24

### Fixed
- Fixed unstained furniture defaulting to pure white (`Vector4.One`) which caused whiteout / washed-out rendering (e.g. Stone Partitions and butterflies). Now defaults to `Vector4.Zero` so game model's original textures are preserved.
- Corrected dye color pipeline by passing standard sRGB directly instead of squaring components, perfectly matching Stagehand's internal `MathF.Sqrt` restoration logic.

## [1.0.12] - 2026-09-24

### Changed
- Improved furniture count display in UI to include recursive attachments (tabletop decor) alongside root furniture counts (e.g. `596 (565 + 31 attachments)`).

## [1.0.11] - 2026-09-24

### Fixed
- Fixed Stagehand file lock contention by saving stage JSON to a temporary file before atomically replacing the target file.

## [1.0.10] - 2026-09-24

### Fixed
- Fixed furniture dye color calculation by converting sRGB values to linear color space (squaring normalized components), matching Stagehand's internal `MathF.Sqrt` restoration logic.

## [1.0.9] - 2026-09-24

### Changed
- Renamed plugin and internal assembly to `HoToSta`.
- Removed detailed descriptions across project manifests and documentation.

## [1.0.8] - 2026-09-24

### Fixed
- Fixed release archive by using the official DalamudPackager generated `latest.zip` containing all dependencies and manifests.

## [1.0.7] - 2026-09-24

### Fixed
- Fixed release zip structure so `HousingToStagehand.json` is located at the root of `latest.zip`.

## [1.0.6] - 2026-09-24

### Fixed
- Fixed compilation errors: used `IObjectTable[0]` for player position and handled non-disposable `IStagehandApi`.

## [1.0.5] - 2026-09-24

### Changed
- Added verbose build logging to CI workflow to capture compilation diagnostics.

## [1.0.4] - 2026-09-24

### Fixed
- Fixed invalid package version of `Stagehand.Api` from `1.2.0` to `0.4.15`.

## [1.0.3] - 2026-09-24

### Fixed
- Updated TargetFramework to `net10.0-windows` to match `Stagehand.Definitions` dependency requirements.

## [1.0.2] - 2026-09-24

### Fixed
- Fixed CI build failure by downloading Dalamud distribution and configuring `DALAMUD_HOME`.

## [1.0.1] - 2026-09-24

### Added
- Added `repo.json` for Dalamud Custom Plugin Repository registration.
- Added automated GitHub Releases workflow to publish distribution zips on push.
- Updated `README.md` with in-game installation guide.

## [1.0.0.0] - 2026-09-24

### Added
- Initial release of **HousingToStagehand**.
- Support for importing MakePlace housing layout `.json` files.
- Accurate coordinate translation (Y/Z swap, scaling, handedness-corrected rotation).
- Furniture item ID to `.mdl` asset path resolution using Lumina game sheets.
- Direct export to Stagehand `.json` files in `Documents\Stages\`.
- Real-time in-game stage spawning and despawning via Stagehand IPC API.
- ImGui configuration UI with `/housingtostagehand` and `/h2s` chat commands.
- GitHub Actions CI/CD build workflow.
