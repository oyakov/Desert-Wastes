# Release Plan Status Evaluation

_Last updated: 2025-10-15_

## Current Stage Summary
- **Stage**: Preparing Milestone M3 – Base Mode MVP (not started).
- **Rationale**: Overworld generation and simulation deliverables are implemented with deterministic coverage. The pipeline produces normalized worlds, the multi-phase overworld loop mutates state and publishes events, and snapshot persistence seams are wired with serializer gateways and tests. Base mode integration work has not yet begun.

## Milestone Breakdown

| Milestone | Deliverable Snapshot | Status | Evidence |
| --- | --- | --- | --- |
| **M0 – Docs & Scaffolding** | Documentation set (design, architecture, tests), project scaffolding, Unity-focused `.gitignore`, placeholder assets | ✅ Complete | `docs/` design docs, refreshed `README.md`, Unity folder layout in `Assets/` and `ProjectSettings/` |
| **M1 – Code Scaffold** | Unity project settings, assembly definitions, deterministic services, TickManager, baseline data containers with tests | ✅ Complete | `ProjectSettings/*.asset`, asmdefs in `Assets/_Project/Scripts/**`, deterministic services & tick manager (`Assets/_Project/Scripts/Core/**`), data containers + normalizer (`Assets/_Project/Scripts/Core/Data/*.cs`), persistence & installer coverage in `Tests/EditMode/*.cs` |
| **M2 – Overworld Gen + Sim** | World generation pipeline, overworld simulation loop, legends logging, persistence seams with tests | ✅ Complete | `Assets/_Project/Scripts/Generation/OverworldGenerationPipeline.cs`, `Assets/_Project/Scripts/Simulation/**`, persistence gateway in `Assets/_Project/Scripts/Persistence/OverworldSnapshotGateway.cs`, deterministic tests in `Tests/EditMode/OverworldGenerationPipelineTests.cs` & `Tests/EditMode/OverworldSimulationLoopTests.cs` |
| **M3 – Base Mode MVP** | Base scene systems (zones, jobs, raids, social mandates), Oracle integration, comprehensive save/load & tests | ⏳ Not started | Base mode gameplay systems not yet implemented |

## Key Gating Items to Kick Off M3
1. Define the base scene bootstrap that consumes `WorldData` outputs and seeds `BaseState` structures for player interaction.
2. Implement core Base Mode systems (zones, jobs, raids, mandates) per `docs/BASE_MODE.md`, ensuring deterministic tick integration with the overworld loop.
3. Extend persistence coverage to include Base Mode state diffs and round-trip save/load of combined overworld/base data.

## Recommended Next Steps
- Align simulation tick cadence with planned Base Mode systems to avoid conflicting mutations.
- Expand EditMode coverage for Oracle/Base event interactions ahead of interactive UI work.
- Establish CI/editor automation to run deterministic EditMode suites after Base Mode features start landing.
