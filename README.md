# Desert Wastes: World Unification Simulator

## Project Vision
"Desert Wastes" is a post-apocalyptic, Dwarf Fortress-inspired world and history simulator built in Unity 2D URP. Players guide a fledgling faction to rebuild civilization by uniting the fractured world, balancing overworld strategy with detailed base management. Leadership matters: indirect orders cascade through command hierarchies, the permadeath of the expedition leader ends the campaign, and a semi-autonomous "divine" AI occasionally intervenes through curated event decks that reshape the world. Presentation favors square-tile pixel art for tactical clarity, while combat, labor, social, and intellectual skill tracks echo the depth of classic fortress sims.

## One-Pager
- **Genre**: Simulation / Strategy / Base Builder
- **Engine**: Unity 2022/2023 LTS (2D URP, Input System)
- **Platform Goals**: PC (mouse + keyboard, accessibility in mind)
- **Core Loop**: Generate world → Survey history → Embark → Manage base with indirect orders → Respond to raids/disasters → Oracle AI draws from event decks → Return to overworld → Expand influence → Unite world.
- **Fail State**: Leader permadeath or total faction collapse.

## Documentation & Orientation
The repository ships with both the Unity project scaffold and an extensive design corpus. Start with `docs/README.md` for a topic-by-topic index that links architecture overviews, system design references, contributor process guides, and milestone status reports.【F:docs/README.md†L1-L47】 Keep this top-level `README.md` aligned with those artifacts whenever large features land so newcomers can trust it as the authoritative entry point.

## Contribution Rules
- Read `docs/ARCHITECTURE.md`, `docs/TEST_PLAN.md`, and `docs/NAMING.md` before contributing.
- All contributions must include updated documentation and tests according to `docs/CHECKLISTS.md`.
- Follow the branching and PR process described in `docs/CONTRIBUTING.md`.
- Preserve determinism and testability seams (RNG, Time, IO) in all systems.

## Continuous Integration
- Use the Unity Test Runner in batch mode to execute EditMode and PlayMode suites in CI
  as documented in `docs/CI_PIPELINE.md`; export the NUnit XML results so your pipeline
  can surface failures inline on pull requests.【F:docs/CI_PIPELINE.md†L1-L99】
- Configure one of the supported Unity licensing secrets (`UNITY_LICENSE`, `UNITY_SERIAL`,
  or `UNITY_EMAIL`/`UNITY_PASSWORD`) in GitHub Actions so the bundled workflow can
  activate the editor headlessly.【F:docs/CI_PIPELINE.md†L4-L15】【F:.github/workflows/unity-tests.yml†L24-L58】

## Current State
- Documentation of design pillars, world generation, AI, social systems, mechanic context, architecture, and test plan remains the single source of truth for feature intent and contributor expectations.【F:docs/ARCHITECTURE.md†L1-L8】【F:docs/DESIGN.md†L1-L9】
- Unity project settings, scoped assembly definitions, and deterministic core services (time provider, RNG, event bus, tick manager) are in place to support reproducible simulation systems and automated EditMode coverage.【F:Assets/_Project/Scripts/Core/Management/TickManager.cs†L1-L80】【F:Assets/_Project/Scripts/Core/Management/DeterministicServiceContainer.cs†L1-L77】
- The overworld generation pipeline now creates normalized worlds with factions, settlements, characters, oracle hooks, and base state seeds, backed by deterministic serialization tests.【F:Assets/_Project/Scripts/Generation/OverworldGenerationPipeline.cs†L9-L120】【F:Tests/EditMode/OverworldGenerationPipelineTests.cs†L9-L35】
- A multi-phase overworld simulation loop mutates world state, emits tick/legend events, and is validated for deterministic behavior through EditMode tests.【F:Assets/_Project/Scripts/Simulation/OverworldSimulationLoop.cs†L1-L126】【F:Tests/EditMode/OverworldSimulationLoopTests.cs†L9-L47】
- Base Mode scaffolding—including the scene bootstrapper, runtime state container, simulation systems for zones/jobs/raids/mandates, and deterministic EditMode coverage—runs today, but Unity UI and player command surfaces remain debug-only.【F:Assets/_Project/Scripts/BaseMode/BaseSceneBootstrapper.cs†L39-L61】【F:Assets/_Project/Scripts/BaseMode/Systems/BaseModeSystems.cs†L42-L390】【F:Tests/EditMode/BaseModeSimulationLoopTests.cs†L13-L71】【F:Assets/_Project/Scripts/BaseMode/Unity/BaseSceneDebugHud.cs†L9-L135】
- Persistence seams support serializing and snapshotting overworld data for future save/load integration work.【F:Assets/_Project/Scripts/Persistence/OverworldSnapshotGateway.cs†L1-L124】【F:Tests/EditMode/BaseStateDiffTests.cs†L9-L73】

## What Comes Next
Milestone M3 centers on turning the deterministic Base Mode scaffolding into a player-facing loop. Immediate priorities include:

1. Replace the debug HUD with production UI that surfaces jobs, mandates, raids, and Oracle events while invoking the indirect command dispatcher provided by the scene bootstrapper.【F:Assets/_Project/Scripts/BaseMode/BaseSceneBootstrapper.cs†L39-L61】【F:Assets/_Project/Scripts/BaseMode/Unity/BaseSceneDebugHud.cs†L11-L160】
2. Extend persistence so overworld and base state snapshots travel together, building on the existing overworld gateway before UI-driven saves ship.【F:Assets/_Project/Scripts/Persistence/OverworldSnapshotGateway.cs†L7-L83】
3. Flesh out Oracle incident handling, ensuring injected incidents from deck draws have gameplay-visible effects and telemetry beyond the current tension adjustments and command hooks.【F:Assets/_Project/Scripts/BaseMode/Systems/OracleSynchronizer.cs†L8-L162】【F:Tests/EditMode/BaseModeSimulationLoopTests.cs†L53-L70】

Refer to `docs/ROADMAP.md` and `docs/RELEASE_STATUS.md` for milestone tracking and delivery evidence.【F:docs/ROADMAP.md†L1-L46】【F:docs/RELEASE_STATUS.md†L1-L33】
