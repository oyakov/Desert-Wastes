# Release Plan Status Evaluation

_Last updated: 2025-10-15_

## Current Stage Summary
- **Stage**: Milestone M3 – Base Mode MVP (simulation scaffolding in progress).
- **Rationale**: Overworld generation, overworld simulation, and persistence seams are complete. Base Mode now boots into a deterministic runtime with systems for zones, jobs, raids, mandates, and Oracle incidents under EditMode coverage, and a production UI surfaces data through the indirect command dispatcher. Save/load for combined overworld/base state and deeper Oracle effect loops remain outstanding.【F:Assets/_Project/Scripts/Generation/OverworldGenerationPipeline.cs†L9-L94】【F:Assets/_Project/Scripts/Simulation/OverworldSimulationLoop.cs†L57-L103】【F:Assets/_Project/Scripts/Persistence/OverworldSnapshotGateway.cs†L7-L83】【F:Assets/_Project/Scripts/BaseMode/BaseSceneBootstrapper.cs†L39-L61】【F:Assets/_Project/Scripts/BaseMode/Systems/BaseModeSystems.cs†L42-L390】【F:Tests/EditMode/BaseModeSimulationLoopTests.cs†L13-L150】【F:Assets/_Project/Scripts/BaseMode/Unity/BaseSceneProductionUI.cs†L1-L266】

## Milestone Breakdown

| Milestone | Deliverable Snapshot | Status | Evidence |
| --- | --- | --- | --- |
| **M0 – Docs & Scaffolding** | Documentation set (design, architecture, tests), project scaffolding, Unity-focused `.gitignore`, placeholder assets | ✅ Complete | `docs/` design docs, refreshed `README.md`, Unity folder layout in `Assets/` and `ProjectSettings/` |
| **M1 – Code Scaffold** | Unity project settings, assembly definitions, deterministic services, TickManager, baseline data containers with tests | ✅ Complete | `ProjectSettings/*.asset`, asmdefs in `Assets/_Project/Scripts/**`, deterministic services & tick manager (`Assets/_Project/Scripts/Core/**`), data containers + normalizer (`Assets/_Project/Scripts/Core/Data/*.cs`), persistence & installer coverage in `Tests/EditMode/*.cs` |
| **M2 – Overworld Gen + Sim** | World generation pipeline, overworld simulation loop, legends logging, persistence seams with tests | ✅ Complete | `Assets/_Project/Scripts/Generation/OverworldGenerationPipeline.cs`, `Assets/_Project/Scripts/Simulation/**`, persistence gateway in `Assets/_Project/Scripts/Persistence/OverworldSnapshotGateway.cs`, deterministic tests in `Tests/EditMode/OverworldGenerationPipelineTests.cs` & `Tests/EditMode/OverworldSimulationLoopTests.cs` |
| **M3 – Base Mode MVP** | Base scene systems (zones, jobs, raids, social mandates), Oracle integration, comprehensive save/load & tests | 🚧 In progress | Simulation scaffolding, Oracle hooks, and deterministic tests exist; production UI and persistence expansion outstanding.【F:Assets/_Project/Scripts/BaseMode/Systems/BaseModeSystems.cs†L42-L390】【F:Assets/_Project/Scripts/BaseMode/Systems/OracleSynchronizer.cs†L8-L162】【F:Tests/EditMode/BaseModeSimulationLoopTests.cs†L13-L71】 |

## Key Gating Items to Exit M3
1. Ship production UI and input loops that replace the debug HUD while leveraging the indirect command dispatcher and runtime systems seeded by the bootstrapper.【F:Assets/_Project/Scripts/BaseMode/BaseSceneBootstrapper.cs†L39-L61】【F:Assets/_Project/Scripts/BaseMode/Unity/BaseSceneProductionUI.cs†L1-L266】
2. Expand persistence coverage to serialize combined overworld/base state and verify deterministic round-trips in EditMode suites.【F:Assets/_Project/Scripts/Persistence/OverworldSnapshotGateway.cs†L7-L83】【F:Tests/EditMode/BaseModeSimulationLoopTests.cs†L35-L70】
3. Implement gameplay-visible Oracle incident handlers and deck balancing loops so injected incidents impact base state beyond tension adjustments.【F:Assets/_Project/Scripts/BaseMode/Systems/OracleSynchronizer.cs†L8-L162】【F:Assets/_Project/Scripts/BaseMode/Systems/BaseModeSystems.cs†L221-L390】

## Recommended Next Steps
- Align simulation tick cadence with planned Base Mode systems to avoid conflicting mutations.
- Expand EditMode coverage for Oracle/Base event interactions ahead of interactive UI work.
- Establish CI/editor automation to run deterministic EditMode suites after Base Mode features start landing.
