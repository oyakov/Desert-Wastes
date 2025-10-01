# Base Mode Design

## Embark Flow
1. **World Context**: Player reviews overworld state (factions, hazards, relics) before embarking (see `docs/WORLDGEN.md`).
2. **Leader Selection**: Choose expedition leader; leader skills set campaign modifiers. Permadeath = game over.
3. **Team Composition**: Draft specialists with complementary skills, personalities, and social ties.
4. **Loadout Planning**: Allocate starting items, supplies, and relics influenced by overworld logistics.
5. **Site Selection**: Evaluate site viability via biome data, hazards, and proximity to factions.
6. **Commit**: Generate base map chunk and transition into Base scene (see `docs/SCENES_AND_FLOW.md`).

## Core Systems (Runtime Implementation)
- **Zone Maintenance**: `ZoneMaintenanceSystem` iterates every designated `BaseZoneRuntime`, applying morale drift, wear, and efficiency deltas based on infrastructure levels and seeded RNG channels. Watchtowers bleed the raid threat meter while infrastructure decay nudges the player to schedule upkeep.
- **Jobs & Labor**: `JobSchedulingSystem` advances queued `BaseJob`s, emits deterministic `BaseJobCompleted` events, and applies outcomes that touch infrastructure, inventory, and research state. Patrol jobs suppress raid threat; production jobs mint inventory stacks used by mandates and incidents.
- **Raid Threat**: `RaidThreatSystem` manages a tension-driven meter that schedules raids once thresholds are crossed. Resolved raids publish `BaseRaidResolved` events, adjust infrastructure, and funnel results into the Oracle synchronizer for deck balancing.
- **Mandates**: `MandateResolutionSystem` consumes mandate tracker resolutions, applies infrastructure/resource deltas, enqueues follow-ups, logs legend-ready events, and forwards outcomes to the Oracle for tension adjustments.
- **Oracle Incidents**: `OracleIncidentResolutionSystem` subscribes to `OracleIncidentInjected` events, enforces card cooldowns, clamps deck weights, and applies effect payloads (resource injections, morale swings, raid modifiers) directly to `BaseState`.
- **Infrastructure Backbone**: Base infrastructure values (power, water, morale, defense) feed multiple systems—zones, raids, mandates—creating a feedback loop that rewards proactive maintenance.

## Time Scales & Ticks
- **Overworld**: `OverworldSimulationLoop` advances yearly ticks before Base Mode begins, populating Oracle tension and raid seeds.
- **Base Mode**: `BaseModeSimulationLoop` executes once per daily tick, exposing an hourly cadence via `BaseModeTickContext.HoursPerDay` for systems that subdivide work (jobs, raids, incidents).
- **Synchronization**: `BaseSceneBootstrapper` normalizes incoming `WorldData`, builds `BaseRuntimeState`, and registers the base loop with the deterministic `TickManager` so overworld-to-base transitions share RNG/time seams (`docs/ARCHITECTURE.md`).

## Raids & Defense
- **Alert Levels**: `BaseState.AlertLevel` (Calm, Elevated, Critical) is mutated by raid scheduling/resolution and by mandate completions that focus on defense.
- **Defense Prep**: Patrol jobs and infrastructure upgrades reduce the raid meter; failure to maintain defense infrastructure or morale accelerates raid scheduling.
- **Resolution Logging**: Every raid resolution writes `EventRecord` entries with attacker IDs, alert levels, and post-raid tension, ensuring legends and telemetry remain synchronized (`docs/AI_COMBAT_INDIRECT.md`).

## Cross-References
- Command AI: `docs/AI_COMBAT_INDIRECT.md`
- Social systems & mandates: `docs/SOCIAL_NOBLES.md`
- Data and persistence: `docs/DATA_MODEL.md`, `docs/ARCHITECTURE.md`
