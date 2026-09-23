# Changelog

All notable changes to this project will be documented in this file.

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
