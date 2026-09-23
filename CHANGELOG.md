# Changelog

All notable changes to this project will be documented in this file.

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
