# Design Pillars

| Pillar | Description |
| --- | --- |
| Overworld Simulation ↔ Base Builder | A living overworld simulates factions, trade, warfare, and ecology while the base builder offers DF-style zone management; both feed data to each other. |
| Indirect Combat | Players issue strategic intents; units act autonomously based on skills, morale, and leadership influence. |
| Divine AI Orchestration | A godlike Oracle AI occasionally intervenes by drawing from tiered event decks (minor, major, epic) that reshape challenges and opportunities. |
| Command Hierarchy | Structured leadership with cascading authority; leader traits determine unit quality, with permadeath ending the campaign. |
| Social & Noble Systems | Personalities, needs, and noble mandates create emergent stories and political pressure. |
| Unification Victory | Long-term goal is to integrate rival factions into a united world through diplomacy, conquest, or cultural influence. |
| Leader Permadeath | The expedition leader is irreplaceable; losing them triggers game over regardless of base prosperity. |

## Non-Goals for MVP
- No direct unit control or manual combat micromanagement.
- No physics-based combat; interactions are simulated via stats and outcomes tables.
- No multiplayer or network features.
- No modding support prior to post-MVP milestones.
- No high-fidelity art or audio assets beyond placeholders.

## Scope Planning

| Feature Area | MVP Scope | Future Scope |
| --- | --- | --- |
| World Generation | Deterministic height/temp/moisture map, factions, relic sites, apocalypse hazards, legends log. | Dynamic weather, migrations, procedural story events, mod hooks. |
| Overworld Simulation | Yearly ticks for faction expansion, diplomacy, trade, raids. | Multi-faction diplomacy UI, espionage, cultural diffusion. |
| Base Mode | DF-style zones, job assignments, industries, research queue, raids. | Multi-layer bases, advanced automation, noble courts. |
| AI & Combat | Indirect orders, leadership modifiers, morale, command radius. | Complex doctrines, psychological warfare, advanced logistics. |
| Social Systems | Personalities, needs, noble mandates, succession rules. | Festivals, religion, dynamic laws, player-made policies. |
| Persistence | JSON snapshots, legends log export, manual saves/loads. | Cloud sync, replay viewer, timeline scrubbing. |

## Cross-References
- Generation details: `docs/WORLDGEN.md`
- Base gameplay: `docs/BASE_MODE.md`
- Indirect AI combat: `docs/AI_COMBAT_INDIRECT.md`
- Social hierarchy: `docs/SOCIAL_NOBLES.md`
- Architecture & test plan: `docs/ARCHITECTURE.md`, `docs/TEST_PLAN.md`
