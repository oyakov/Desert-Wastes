# Naming Conventions

## General Principles
- Use descriptive, consistent names reflecting function and layer.
- Avoid abbreviations unless industry-standard (e.g., RNG, DTO).
- Prefix interfaces with `I` (e.g., `IRngService`).

## Scripts
| Category | Convention | Example |
| --- | --- | --- |
| Core Logic | `Namespace.PurposeSuffix` with PascalCase | `Time.TickScheduler` |
| MonoBehaviours | PascalCase with suffix `Controller`, `Presenter`, or `View`. | `BaseZonePresenter` |
| ScriptableObjects | PascalCase with suffix `Config`, `Database`, or `Profile`. | `RngStreamConfig` |
| Tests | `<ClassUnderTest>Tests` for unit; `<Feature>PlayModeTests` for integration. | `WorldGeneratorTests` |

## Assets
| Asset Type | Convention |
| --- | --- |
| Sprites | `spr_<context>_<descriptor>` |
| Audio | `aud_<category>_<descriptor>` |
| Prefabs | `pfb_<context>_<descriptor>` |
| Scenes | `Scene_<Name>` (e.g., `Scene_Boot`) until actual `.unity` files created. |
| Resources | `res_<context>_<descriptor>` |

## Data & JSON Fixtures
- File names: `worldgen_seed-<seed>_v<version>.json`
- Legends logs: `legends_<era>_v<version>.json`
- Store under `Tests/PlayMode/Fixtures/`.

## Namespaces
- Root namespace: `DesertWastes`.
- Assemblies map to namespaces: `DesertWastes.Core`, `DesertWastes.Generation`, etc.
- Tests namespaces mirror runtime with `.Tests` suffix.

## Git & Branches
- Branch names: lowercase with hyphens (`feature/overworld-sim-loop`).
- Tags for releases: `vMajor.Minor.Patch` (e.g., `v0.3.0`).

## Cross-References
- Contribution process: `docs/CONTRIBUTING.md`
- Architecture: `docs/ARCHITECTURE.md`
- Test fixtures: `docs/TEST_PLAN.md`
