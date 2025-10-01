# Milestone Evaluation – October 2025

## Summary
The Desert Wastes project has progressed beyond the original documentation scaffold and now delivers deterministic overworld and base-mode simulation features. Milestones M0 through M2 remain fully satisfied, and a substantial portion of the M3 base-mode MVP is already implemented in code with supporting EditMode coverage. The remaining work for M3 centers on scene integration, persistence polish, and player-facing interaction layers.

## Milestone Status
### M0 – Docs & Scaffolding ✅
- Comprehensive design and planning documentation is in place alongside a populated README outlining the project vision, current assets, and contribution expectations.【F:README.md†L1-L33】

### M1 – Code Scaffold ✅
- Deterministic service abstractions (RNG, time, event bus) and the tick manager compose the core runtime foundation with assembly definitions wired into the Unity project structure.【F:Assets/_Project/Scripts/Core/Services/IRngService.cs†L4-L140】【F:Assets/_Project/Scripts/Core/Management/TickManager.cs†L5-L96】
- World data containers, normalizers, and validators ship with EditMode tests that exercise the deterministic services and installer seams (see `Tests/EditMode` folder for coverage).

### M2 – Overworld Generation & Simulation ✅
- The overworld generation pipeline produces deterministic worlds seeded by configuration and normalized into the shared `WorldData` model.【F:Assets/_Project/Scripts/Generation/OverworldGenerationPipeline.cs†L7-L194】
- Overworld simulation phases run through the tick manager and publish completion events, with deterministic regression tests verifying behavior.【F:Assets/_Project/Scripts/Simulation/OverworldSimulationLoop.cs†L8-L104】【F:Tests/EditMode/OverworldGenerationPipelineTests.cs†L1-L37】

### M3 – Base Mode MVP ▶️ In Progress
Implemented:
- Base scene bootstrapper, runtime state container, and simulation loop register zone maintenance, job scheduling, raid escalation, and mandate resolution systems.【F:Assets/_Project/Scripts/BaseMode/BaseSceneBootstrapper.cs†L7-L76】【F:Assets/_Project/Scripts/BaseMode/BaseModeSimulationLoop.cs†L8-L114】【F:Assets/_Project/Scripts/BaseMode/Systems/BaseModeSystems.cs†L7-L459】
- Deterministic EditMode coverage validates bootstrap registration, simulation determinism, and persistence diffs for the base state.【F:Tests/EditMode/BaseModeSimulationLoopTests.cs†L1-L94】【F:Tests/EditMode/BaseStateDiffTests.cs†L1-L160】
Remaining gaps:
- No Unity scene or UI wiring is committed yet to expose the base-mode systems to players.
- Oracle event decks and overworld/BaseMode transition seams are not yet connected inside the runtime.
- Save/load currently exercises base diffs in isolation; round-tripping combined overworld + base snapshots is not demonstrated in tests.

## Recommended Next Tasks (M3 Focus)
1. **Scene Integration & UI Hooks** – Create the Base scene prefab(s) that instantiate `BaseSceneBootstrapper`, present zone/job/mandate status, and expose indirect command inputs for playtesting.
2. **Oracle & Overworld Sync** – Implement the data handoff that updates `WorldData.OracleState` and overworld factions when base-mode raids or mandates resolve, and ensure Oracle decks can inject base incidents via shared event bus channels.
3. **Persistence Round-Trip** – Extend serializer tests to save/load combined overworld + base snapshots, applying `BaseStateDiff` patches during scene transitions and validating deterministic equality.
4. **Automation & Tooling** – Wire the EditMode suite into CI (or a local automation script) so base-mode regressions run with every commit; add logging hooks for legends/event journal updates emitted by the base systems.
5. **Player Feedback & Balancing** – Instrument simulation outputs (raid threat curve, job throughput, mandate cadence) to surface telemetry for future tuning and to confirm economy pacing against `docs/BASE_MODE.md` goals.

## Risks & Mitigations
- **Integration Complexity** – As base mode begins touching UI and Oracle systems, maintain deterministic seams by routing all randomness through `IRngService` channels and covering transitions with tests.【F:Assets/_Project/Scripts/BaseMode/BaseModeSimulationLoop.cs†L61-L103】
- **Scope Creep** – Prioritize the documented MVP behaviors (zones, jobs, raids, mandates) before layering advanced diplomacy or narrative systems to keep M3 bounded.【F:docs/ROADMAP.md†L4-L29】

