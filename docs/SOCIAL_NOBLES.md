# Social Systems & Noble Hierarchy

## Personalities & Needs
- **Traits**: Stoic, Visionary, Pragmatic, Zealous, Empathic, Ruthless. Affect decision weights and interpersonal compatibility.
- **Needs**: Safety, Purpose, Recognition, Comfort, Autonomy. Failure generates mood penalties and potential mandates.
- **Relationships**: Friendship, rivalry, mentorship, kinship tracked via weighted scores and shared history events (`docs/DATA_MODEL.md`).

## Mood & Mandates
- Moods derive from needs, recent events, leadership actions, and environment quality (`docs/BASE_MODE.md`). `BaseRuntimeState.MandateTracker` queues mandates seeded during bootstrap and from noble responses.
- Nobles and influential characters issue mandates (resource quotas, construction, policy shifts) that resolve through `MandateResolutionSystem` with deterministic rewards/penalties.
- Compliance boosts loyalty/infrastructure; failure increases resentment, drains morale infrastructure, and may elevate raid alert levels.

## Noble Roles
| Role | Responsibilities | Failure Consequence |
| --- | --- | --- |
| Overseer | Overall base governance, mandate arbitration, final say on policies. | Legitimacy loss → morale penalties, possible mutiny. |
| Warlord | Military doctrine, raid readiness, command assignments (`docs/AI_COMBAT_INDIRECT.md`). | Defense penalties, increased casualties. |
| Quartermaster | Logistics, inventory control, rationing. | Supply shortages, unrest. |
| Research Chief | Oversees research queue, tech unlocks. | Stalled progress, morale loss among scholars. |
| Steward | Housing, morale spaces, sanitation. | Disease outbreaks, productivity decline. |
| Diplomatic Envoy | Manages overworld relations. | Trade embargoes, war declarations. |

## Succession Rules
- Leader death triggers succession council: nobles vote based on loyalty, ethos alignment, and personal ambitions.
- If no consensus within defined ticks, campaign ends (leader permadeath fail state).
- Succession outcomes update legends log and influence faction diplomacy (`docs/WORLDGEN.md`).

## Social Simulation Hooks
- Events pipeline integrates with the deterministic event bus (`docs/ARCHITECTURE.md`). `BaseMandateResolved` events feed Oracle tension adjustments and legends logging.
- EditMode tests cover mandate queuing/resolution and Oracle tension feedback via `BaseModeSimulationLoopTests` and `SampleWorldBuilder` fixtures (`docs/TEST_PLAN.md`).

## Cross-References
- Data model specifics: `docs/DATA_MODEL.md`
- Base operations: `docs/BASE_MODE.md`
- Command AI impacts: `docs/AI_COMBAT_INDIRECT.md`
