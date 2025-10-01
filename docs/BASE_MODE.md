# Base Mode Design

## Embark Flow
1. **World Context**: Player reviews overworld state (factions, hazards, relics) before embarking (see `docs/WORLDGEN.md`).
2. **Leader Selection**: Choose expedition leader; leader skills set campaign modifiers. Permadeath = game over.
3. **Team Composition**: Draft specialists with complementary skills, personalities, and social ties.
4. **Loadout Planning**: Allocate starting items, supplies, and relics influenced by overworld logistics.
5. **Site Selection**: Evaluate site viability via biome data, hazards, and proximity to factions.
6. **Commit**: Generate base map chunk and transition into Base scene (see `docs/SCENES_AND_FLOW.md`).

## Core Systems
- **Zones**: DF-style designations (housing, workshops, storage, agriculture, defense).
- **Jobs & Labor**: Work orders prioritized by command hierarchy; players set intents, AI handles execution.
- **Industries**: Crafting chains (salvage → refinement → production → research) with resource dependencies.
- **Research**: Unlock tech tiers affecting overworld influence and base resilience.
- **Hazards**: Environmental threats (storms, toxins), mechanical failures, raids triggered by overworld state.
- **Oracle Incursions**: Event cards manifest as miracles or calamities (supply caches, nemesis assaults, morale visions) depending on deck tier draws (`docs/AI_COMBAT_INDIRECT.md`).
- **Infrastructure**: Power, water, sanitation, morale spaces to maintain settlement stability.

## Time Scales
- **Overworld**: Yearly ticks, aggregated events (handled in Simulation layer).
- **Base Mode**: Daily ticks with sub-hour granular scheduling for jobs and incidents.
- Synchronization occurs via state exchanges when transitioning scenes (see `docs/ARCHITECTURE.md`).

## Raids & Defense
- **Alert Levels**: Normal, Elevated, Siege. Affects AI behavior and resource usage.
- **Defense Prep**: Build traps, assign guard posts, manage patrol routes via indirect orders.
- **Siege Phases**: Approach → Encirclement → Breach → Resolution. Each phase logs events to legends (see `docs/AI_COMBAT_INDIRECT.md`).

## Cross-References
- Command AI: `docs/AI_COMBAT_INDIRECT.md`
- Social systems & mandates: `docs/SOCIAL_NOBLES.md`
- Data and persistence: `docs/DATA_MODEL.md`, `docs/ARCHITECTURE.md`
