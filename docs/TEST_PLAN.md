# Test Strategy

## Guiding Principles
- Determinism-first: All simulations must be reproducible given identical seeds and inputs.
- Layered testing: Core logic validated via EditMode tests; scene integration verified via PlayMode tests.
- Property-based coverage for RNG-heavy systems to expose edge cases.

## Unit Tests (EditMode)
| Focus | Description | Related Docs |
| --- | --- | --- |
| Worldgen Determinism | Validate that given seed produces identical `WorldData` structures (`docs/WORLDGEN.md`, `docs/DATA_MODEL.md`). | `docs/WORLDGEN.md`, `docs/DATA_MODEL.md` |
| Event Generation Invariants | Ensure events maintain referential integrity (actors, locations) and chronological ordering. | `docs/DATA_MODEL.md`, `docs/ARCHITECTURE.md` |
| Command Outcome Distributions | Verify indirect combat outcomes align with expected probability distributions and leadership modifiers. | `docs/AI_COMBAT_INDIRECT.md` |
| Social Mandate Resolution | Confirm mandates adjust loyalty/morale consistently with trait modifiers. | `docs/SOCIAL_NOBLES.md` |
| Oracle Deck Selection | Assert `IOracleInterventionService` chooses decks/cards deterministically given tension thresholds and cooldowns. | `docs/AI_COMBAT_INDIRECT.md`, `docs/DATA_MODEL.md`, `docs/ARCHITECTURE.md` |

## PlayMode Tests
| Focus | Description | Related Docs |
| --- | --- | --- |
| Scene Boot | Load Boot → World → Base scenes ensuring DI services initialize and persist. | `docs/SCENES_AND_FLOW.md`, `docs/ARCHITECTURE.md` |
| Tick Cadence | Validate overworld yearly ticks and base daily ticks maintain timing contract. | `docs/BASE_MODE.md`, `docs/ARCHITECTURE.md` |
| Save/Load Round-Trip | Serialize `WorldData` and `BaseState`, reload, and compare for equality. | `docs/DATA_MODEL.md`, `docs/WORLDGEN.md` |

## Property-Based Tests
- RNG wrapper: Ensure sequences are repeatable and cover distribution expectations (`docs/ARCHITECTURE.md`).
- Oracle interventions: Generate random stress profiles and property-test that deck eligibility obeys tier rules (`docs/AI_COMBAT_INDIRECT.md`).
- Legends log: Generate random event sequences and assert chronological order and referential integrity.

## Test Data Strategy
- Text-based fixtures (JSON) stored under `Tests/PlayMode/Fixtures/` (to be created in M1).
- No binary assets; all references to textures/audio remain placeholders.
- Use versioned fixture naming aligned with `WorldData.Version` (see `docs/DATA_MODEL.md`).

## Tooling & Automation
- Unity Test Framework for EditMode & PlayMode suites.
- Continuous Integration (planned in `.tools/`) runs tests on each PR.
- Coverage thresholds defined post-M1.

## Cross-References
- Architecture seam definitions: `docs/ARCHITECTURE.md`
- Data model invariants: `docs/DATA_MODEL.md`
- Gameplay systems: `docs/WORLDGEN.md`, `docs/BASE_MODE.md`, `docs/AI_COMBAT_INDIRECT.md`, `docs/SOCIAL_NOBLES.md`
