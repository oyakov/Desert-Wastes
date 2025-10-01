# Desert Wastes – System Evaluation (2025-10-15)

## Current Runtime & Test Coverage Snapshot
- **Deterministic service core**: The deterministic TickManager, RNG, and event bus are implemented and shared between overworld and base simulations, enabling reproducible EditMode tests.【F:Assets/_Project/Scripts/Core/Management/TickManager.cs†L1-L80】【F:Assets/_Project/Scripts/Core/Management/DeterministicServiceContainer.cs†L1-L77】
- **Overworld pipeline**: World generation and the multi-phase overworld simulation loop are complete with serialization-backed tests validating deterministic outputs.【F:Assets/_Project/Scripts/Generation/OverworldGenerationPipeline.cs†L9-L120】【F:Tests/EditMode/OverworldGenerationPipelineTests.cs†L9-L35】【F:Assets/_Project/Scripts/Simulation/OverworldSimulationLoop.cs†L1-L126】
- **Base mode scaffolding**: The base scene bootstrapper creates runtime state, registers the base simulation loop, seeds jobs/mandates, and dispatches events into the deterministic bus.【F:Assets/_Project/Scripts/BaseMode/BaseSceneBootstrapper.cs†L9-L83】 The base loop executes modular systems covering zone maintenance, job scheduling, raid threat management, mandate resolution, and Oracle synchronization with deterministic tests validating the loop and Oracle incident flow.【F:Assets/_Project/Scripts/BaseMode/BaseModeSimulationLoop.cs†L1-L87】【F:Assets/_Project/Scripts/BaseMode/Systems/BaseModeSystems.cs†L1-L462】【F:Assets/_Project/Scripts/BaseMode/Systems/OracleSynchronizer.cs†L1-L122】【F:Tests/EditMode/BaseModeSimulationLoopTests.cs†L1-L109】
- **Unity hooks**: Editor-time BaseSceneDebugHud and installer behaviour listen for boot events but remain debug-facing; no production UI or player command surfaces are present yet.【F:Assets/_Project/Scripts/BaseMode/Unity/BaseSceneDebugHud.cs†L1-L135】

## Gaps & Risks
1. **Player-facing Base Mode** – Systems operate on simulation data, but there is no Unity UI, indirect order interface, or integration with generated world data beyond debug HUDs. Base mode remains non-interactive for players.【F:Assets/_Project/Scripts/BaseMode/Unity/BaseSceneDebugHud.cs†L1-L135】
2. **Persistence breadth** – Persistence currently snapshots overworld state; combined overworld/base save-load flows and delta tracking are unimplemented, leaving long-session continuity a risk for M3.【F:Assets/_Project/Scripts/Persistence/OverworldSnapshotGateway.cs†L1-L124】
3. **Oracle/Base coupling depth** – OracleSynchronizer publishes incidents, but downstream incident handlers, deck balancing, and narrative feedback loops are not defined, risking shallow Oracle presence during base play.【F:Assets/_Project/Scripts/BaseMode/Systems/OracleSynchronizer.cs†L1-L122】
4. **Content scaling** – Zone/job definitions and resource inventories are hard-coded placeholders. Scaling to multiple zones, NPC roles, and raids will require data-driven configs and tooling beyond current scaffolding.【F:Assets/_Project/Scripts/BaseMode/BaseRuntimeState.cs†L1-L373】
5. **CI automation** – Docs describe Unity batch test workflows, but no evidence of automated Base Mode coverage gating exists yet, increasing regression risk once UI/content work begins.【F:docs/CI_PIPELINE.md†L1-L99】

## Proposed Next Steps (Milestone M3 Kick-off)
1. **Interactive Base Scene MVP**
   - Build production UI panels for zones, job intents, mandates, and raid alerts using Unity UI Toolkit.
   - Wire indirect command dispatcher to player inputs and extend BaseMode systems to react to issued commands.
   - Extend BaseSceneBootstrapper to ingest generated world site data (biome hazards, nearby factions) for initial base layout and hazard modifiers.【F:Assets/_Project/Scripts/BaseMode/BaseSceneBootstrapper.cs†L9-L83】

2. **Persistence & Save/Load Expansion**
   - Implement BaseState diffing and integrate with OverworldSnapshotGateway for combined save payloads.
   - Add round-trip EditMode tests validating save/load determinism across overworld and base simulations.【F:Assets/_Project/Scripts/Persistence/OverworldSnapshotGateway.cs†L1-L124】【F:Tests/EditMode/BaseModeSimulationLoopTests.cs†L1-L109】

3. **Oracle Event Handling Pipeline**
   - Define Oracle incident effect resolvers in Base Mode (resource injections, raids, morale shifts).
   - Create balancing hooks for deck tension adjustments and integrate telemetry into legends/events for narrative continuity.【F:Assets/_Project/Scripts/BaseMode/Systems/OracleSynchronizer.cs†L1-L122】【F:Assets/_Project/Scripts/BaseMode/Systems/BaseModeSystems.cs†L1-L462】

4. **Data & Content Authoring Framework**
   - Replace hard-coded zone/job templates with data assets (ScriptableObjects or JSON) and introduce authoring tools to add new jobs, mandates, and raid archetypes without code changes.
   - Expand SampleWorldBuilder coverage to multiple factions/zone mixes to ensure systems scale before UI polish.【F:Assets/_Project/Scripts/BaseMode/BaseRuntimeState.cs†L1-L373】【F:Tests/EditMode/SampleWorldBuilder.cs†L1-L106】

5. **Automation & QA Readiness**
   - Bring Unity Test Runner batch execution into CI with Base Mode suites and coverage reporting as outlined in docs/CI_PIPELINE.md.
   - Establish performance baselines for the base loop (profiling scripts) to catch deterministic drift or frame spikes early.

## Suggested Milestone Exit Criteria
- Player can interact with Base Mode via UI to influence jobs, mandates, and raid readiness in deterministic builds.
- Save/load restores combined overworld/base state without drift over 7 in-game days.
- Oracle incidents have gameplay-visible outcomes with automated tests validating incident effect determinism.
- CI pipeline runs Base Mode EditMode tests headlessly on every PR.
