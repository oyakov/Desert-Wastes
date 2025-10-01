# Test Strategy

## Guiding Principles
- Determinism-first: All simulations must be reproducible given identical seeds and inputs.
- Layered testing: Core logic validated via EditMode tests; scene integration verified via PlayMode tests.
- Property-based coverage for RNG-heavy systems to expose edge cases.

## Unit Tests (EditMode)
| Focus | Description | Related Docs |
| --- | --- | --- |
| Deterministic Services | `DeterministicRngServiceTests`, `ManualTimeProviderTests`, and `TickManagerTests` ensure reproducible seeds/ticks and pub-sub wiring. | `docs/ARCHITECTURE.md`, `docs/DATA_MODEL.md` |
| World Generation | `OverworldGenerationPipelineTests` validate tile/faction/site creation, oracle/base seeds, and normalization. | `docs/WORLDGEN.md`, `docs/DESIGN.md` |
| Overworld Simulation | `OverworldSimulationLoopTests` exercise yearly phase ordering, Oracle tension adjustments, and legends output. | `docs/ARCHITECTURE.md`, `docs/AI_COMBAT_INDIRECT.md` |
| Base Bootstrap & Loop | `BaseSceneBootstrapper*Tests` and `BaseModeSimulationLoopTests` verify runtime initialization, deterministic system execution, mandate/raid/oracle flows. | `docs/BASE_MODE.md`, `docs/SOCIAL_NOBLES.md`, `docs/AI_COMBAT_INDIRECT.md` |
| Persistence | `OverworldSnapshotGatewayTests`, `WorldDataSerializerTests`, and `WorldDataValidatorTests` cover JSON round-trips and schema invariants. | `docs/DATA_MODEL.md`, `docs/ARCHITECTURE.md` |

## PlayMode Tests
| Focus | Description | Related Docs |
| --- | --- | --- |
| Scene Boot | `BaseSceneIndirectCommandSmokeTests` loads the Base scene, verifies bootstrap events, and issues indirect command samples. | `docs/SCENES_AND_FLOW.md`, `docs/BASE_MODE.md` |
| Command Dispatch | Validate indirect command dispatcher wiring between UI Toolkit components and runtime services. | `docs/ARCHITECTURE.md`, `docs/BASE_MODE.md` |
| Future Coverage | Save/load round-trips and overworld/base transitions will be added alongside persistence expansion. | `docs/DATA_MODEL.md`, `docs/ROADMAP.md` |

## Property-Based Tests
- RNG wrapper: Property tests will complement `DeterministicRngServiceTests` by validating distribution bounds (`docs/ARCHITECTURE.md`).
- Oracle interventions: Randomized stress profiles verify deck eligibility, cooldown recovery, and incident weight balancing (`docs/AI_COMBAT_INDIRECT.md`).
- Legends log: Random event sequences assert chronological order and referential integrity once overworld/base telemetry expands.

## Test Data Strategy
- Text-based fixtures (JSON) stored under `Tests/PlayMode/Fixtures/`; placeholder assets ship with the repo for future expansion.
- No binary assets; all references to textures/audio remain placeholders.
- Use versioned fixture naming aligned with `WorldData.Version` (see `docs/DATA_MODEL.md`).

## Tooling & Automation
- Unity Test Framework for EditMode & PlayMode suites.
- Continuous Integration via `.github/workflows/unity-tests.yml` runs EditMode + PlayMode suites on each PR when license secrets are configured.
- Coverage thresholds defined post-M1.

## Cross-References
- Architecture seam definitions: `docs/ARCHITECTURE.md`
- Data model invariants: `docs/DATA_MODEL.md`
- Gameplay systems: `docs/WORLDGEN.md`, `docs/BASE_MODE.md`, `docs/AI_COMBAT_INDIRECT.md`, `docs/SOCIAL_NOBLES.md`
