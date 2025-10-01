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

| Feature Area | MVP Scope | Current Implementation Notes | Future Scope |
| --- | --- | --- | --- |
| World Generation | Deterministic height/temp/moisture map, factions, relic sites, apocalypse hazards, legends log. | `OverworldGenerationPipeline` builds tiles, factions, settlements, characters, oracle/base seeds, and normalizes data before handing to simulation. | Dynamic weather, migrations, procedural story events, mod hooks. |
| Overworld Simulation | Yearly ticks for faction expansion, diplomacy, trade, raids. | `OverworldSimulationLoop` chains discrete phases (faction growth, trade, Oracle review) and logs deterministic events for legends and persistence. | Multi-faction diplomacy UI, espionage, cultural diffusion. |
| Base Mode | DF-style zones, job assignments, industries, research queue, raids. | `BaseSceneBootstrapper` seeds `BaseRuntimeState`; modular systems (zones, jobs, raids, mandates, Oracle) execute each tick via `BaseModeSimulationLoop`. | Multi-layer bases, advanced automation, noble courts. |
| AI & Combat | Indirect orders, leadership modifiers, morale, command radius. | Command intents travel through data-driven jobs/raids; Oracle incidents and raid threat tie into command hierarchy without direct control. | Complex doctrines, psychological warfare, advanced logistics. |
| Social Systems | Personalities, needs, noble mandates, succession rules. | Mandate tracker & noble roles integrate with base/overworld state; incident effects adjust loyalty and infrastructure. | Festivals, religion, dynamic laws, player-made policies. |
| Persistence | JSON snapshots, legends log export, manual saves/loads. | `OverworldSnapshotGateway` serializes deterministic `WorldData` snapshots; combined overworld/base save pipeline planned for M3 exit. | Cloud sync, replay viewer, timeline scrubbing. |

## Cross-References
- Generation details: `docs/WORLDGEN.md`
- Base gameplay: `docs/BASE_MODE.md`
- Indirect AI combat: `docs/AI_COMBAT_INDIRECT.md`
- Social hierarchy: `docs/SOCIAL_NOBLES.md`
- Architecture & test plan: `docs/ARCHITECTURE.md`, `docs/TEST_PLAN.md`
