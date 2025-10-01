# Desert Wastes: World Unification Simulator

## Project Vision
"Desert Wastes" is a post-apocalyptic, Dwarf Fortress-inspired world and history simulator built in Unity 2D URP. Players guide a fledgling faction to rebuild civilization by uniting the fractured world, balancing overworld strategy with detailed base management. Leadership matters: indirect orders cascade through command hierarchies, the permadeath of the expedition leader ends the campaign, and a semi-autonomous "divine" AI occasionally intervenes through curated event decks that reshape the world. Presentation favors square-tile pixel art for tactical clarity, while combat, labor, social, and intellectual skill tracks echo the depth of classic fortress sims.

## One-Pager
- **Genre**: Simulation / Strategy / Base Builder
- **Engine**: Unity 2022/2023 LTS (2D URP, Input System)
- **Platform Goals**: PC (mouse + keyboard, accessibility in mind)
- **Core Loop**: Generate world → Survey history → Embark → Manage base with indirect orders → Respond to raids/disasters → Oracle AI draws from event decks → Return to overworld → Expand influence → Unite world.
- **Fail State**: Leader permadeath or total faction collapse.

## Repository Usage
This repository now includes the Unity project scaffold alongside the design documentation. Project settings, assembly definitions, and core deterministic services are in place to support forthcoming overworld and base mode systems. Use the documents in `docs/` to understand the intended architecture, systems, and testing strategy while the early runtime foundation comes together.

## Contribution Rules
- Read `docs/ARCHITECTURE.md`, `docs/TEST_PLAN.md`, and `docs/NAMING.md` before contributing.
- All contributions must include updated documentation and tests according to `docs/CHECKLISTS.md`.
- Follow the branching and PR process described in `docs/CONTRIBUTING.md`.
- Preserve determinism and testability seams (RNG, Time, IO) in all systems.

## Current State
- Documentation of design pillars, world generation, AI, social systems, mechanic context, architecture, and test plan remains the single source of truth for feature intent and contributor expectations.【F:docs/ARCHITECTURE.md†L1-L8】【F:docs/DESIGN.md†L1-L9】
- Unity project settings, scoped assembly definitions, and deterministic core services (time provider, RNG, event bus, tick manager) are in place to support reproducible simulation systems and automated EditMode coverage.【F:Assets/_Project/Scripts/Core/Management/TickManager.cs†L1-L80】【F:Assets/_Project/Scripts/Core/Management/DeterministicServiceContainer.cs†L1-L77】
- The overworld generation pipeline now creates normalized worlds with factions, settlements, characters, oracle hooks, and base state seeds, backed by deterministic serialization tests.【F:Assets/_Project/Scripts/Generation/OverworldGenerationPipeline.cs†L9-L120】【F:Tests/EditMode/OverworldGenerationPipelineTests.cs†L9-L35】
- A multi-phase overworld simulation loop mutates world state, emits tick/legend events, and is validated for deterministic behavior through EditMode tests.【F:Assets/_Project/Scripts/Simulation/OverworldSimulationLoop.cs†L1-L126】【F:Tests/EditMode/OverworldSimulationLoopTests.cs†L9-L47】
- Base Mode scaffolding—including the scene bootstrapper, runtime state container, job/mandate trackers, and tick-driven simulation loop—exists in code with deterministic tests, but the gameplay-facing systems (zones, raids, UI) are still stubs awaiting production assets and integration.【F:Assets/_Project/Scripts/BaseMode/BaseSceneBootstrapper.cs†L9-L63】【F:Assets/_Project/Scripts/BaseMode/BaseRuntimeState.cs†L9-L205】【F:Tests/EditMode/BaseModeSimulationLoopTests.cs†L9-L67】
- Persistence seams support serializing and snapshotting overworld data for future save/load integration work.【F:Assets/_Project/Scripts/Persistence/OverworldSnapshotGateway.cs†L1-L124】【F:Tests/EditMode/BaseStateDiffTests.cs†L9-L73】

## What Comes Next
The team is transitioning from Milestone M2 to Milestone M3. Overworld generation, simulation, and persistence deliverables are implemented, while Base Mode gameplay systems remain to be built. Immediate priorities include:

1. Wiring the Base scene to consume generated `WorldData`, expose player-interactive systems, and extend UI scaffolding.
2. Implementing Base Mode systems (zones, jobs, raids, mandates) outlined in `docs/BASE_MODE.md`, ensuring they cooperate with the deterministic tick manager.
3. Expanding persistence to cover combined overworld/base state diffs and providing round-trip save/load coverage.

Refer to `docs/ROADMAP.md` and `docs/RELEASE_STATUS.md` for detailed milestone tracking and acceptance evidence.【F:docs/RELEASE_STATUS.md†L1-L33】【F:docs/ROADMAP.md†L1-L46】
