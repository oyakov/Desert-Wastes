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
- **Role**: A semi-divine overseer AI acts as a game master, drawing from tiered event decks (Minor, Major, Epic) to escalate or ease pressure based on world tension metrics.
- **Event Decks**:
  - *Minor*: Tactical twists (reinforcement delays, sandstorms, morale visions) that alter immediate combat modifiers.
  - *Major*: Strategic shifts (enemy champions, supply boons, diplomatic ultimatums) that ripple across overworld simulation.
  - *Epic*: Campaign-defining events (Rise of a Nemesis commander, apocalyptic omen, divine respite) that reconfigure long-term goals.
- **Trigger Logic**: Deck selection uses deterministic thresholds tied to simulation stress, noble mandates, and player choices to preserve testability and fairness (`docs/ARCHITECTURE.md`).
- **Delivery**: Interventions manifest as orders relayed through command hierarchy, environmental modifiers, or narrative events logged into legends.
- **Player Response**: Players receive limited countermeasures (rituals, diplomacy, intel) to anticipate the Oracle's next draw.

## Cross-References
- Social impacts: `docs/SOCIAL_NOBLES.md`
- Base defense specifics: `docs/BASE_MODE.md`
- Architecture & services: `docs/ARCHITECTURE.md`
