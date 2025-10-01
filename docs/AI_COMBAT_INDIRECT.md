# Indirect Combat & Command AI

## Philosophy
Players set intents, priorities, and doctrines. Units interpret orders based on their roles, personalities, and the authority of their leaders. No direct click-to-attack; instead, success emerges from preparation, morale, and logistics.

## Order Semantics
- **Intent Types**: Defend, Patrol, Assault, Harass, Retreat, Hold Position, Secure Asset.
- **Priority Levels**: Critical, High, Routine, Opportunistic.
- **Constraints**: Resource budgets, time windows, acceptable losses.

## Leadership Skill Effects
| Skill | Impact |
| --- | --- |
| Tactics | Determines formation quality, engagement choices, and adaptive maneuvers. |
| Leadership | Increases obedience radius and reduces order latency. |
| Charisma | Boosts morale regeneration and loyalty during stressful events. |
| Organization | Enhances supply chain efficiency and job assignment throughput. |
| Ethos | Shapes ethical limits, influencing prisoner treatment, civilian collateral, and faction reputation. |

## Command Structure
- **Hierarchy**: Leader → Captains → Squad Leads → Specialists.
- **Command Radius**: Orders degrade outside communication range; requires signal posts or messengers.
- **Communications**: Deterministic latency modeled via time provider (see `docs/ARCHITECTURE.md`).
- **Morale & Loyalty**: Driven by victories, supplies, social ties (`docs/SOCIAL_NOBLES.md`).
- **Fallback Behavior**: Units retreat to safe zones, hold chokepoints, or protect VIPs based on doctrine.

## Siege Phases
1. **Recon**: Patrols gather intel; results update legends log (`docs/WORLDGEN.md`).
2. **Approach**: Forces mobilize; logistics checks ensure supply viability.
3. **Encirclement**: Blockades established; morale tests for defenders.
4. **Breach**: Indirect orders shift to room-by-room control; traps and defenses trigger.
5. **Resolution**: Aftermath events recorded, casualties updated, noble mandates evaluated.

## Event Logging
- Every command generates events with IDs linking to actors and locations (see `docs/DATA_MODEL.md`).
- Logs feed into legends and UI battle reports.
- Deterministic RNG ensures reproducible outcomes for testing (`docs/TEST_PLAN.md`).

## Oracle AI Interventions
- **Role**: The `OracleReviewPhase` inside `OverworldSimulationLoop` monitors world tension, adjusts deck weights, and emits deterministic `OracleIncidentInjected` payloads when thresholds are crossed.
- **Event Decks**:
  - *Minor*: Tactical twists (reinforcement delays, sandstorms, morale visions) applied by `OracleIncidentResolutionSystem` as infrastructure/morale nudges and temporary raid modifiers.
  - *Major*: Strategic shifts (enemy champions, supply boons, diplomatic ultimatums) that update overworld factions and Base Mode threat meters.
  - *Epic*: Campaign-defining events (Rise of a Nemesis commander, apocalyptic omen, divine respite) that inject multi-step incidents and legends entries.
- **Trigger Logic**: Deck selection uses deterministic thresholds tied to Oracle tension, mandate outcomes, and raid results to preserve testability (`docs/ARCHITECTURE.md`). Cooldowns and deck weights are normalized after each incident.
- **Delivery**: `OracleSynchronizer` publishes incidents to Base Mode, where `OracleIncidentResolutionSystem` applies effect payloads and adjusts deck weights/cooldowns. Overworld incidents route through event logs and faction state.
- **Player Response**: Players counter by pursuing mandates, patrol jobs, and infrastructure upgrades that lower tension or add defensive buffers before the next draw.

## Cross-References
- Social impacts: `docs/SOCIAL_NOBLES.md`
- Base defense specifics: `docs/BASE_MODE.md`
- Architecture & services: `docs/ARCHITECTURE.md`
