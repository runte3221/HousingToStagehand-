# Changelog

All notable changes to this project will be documented in this file.

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
