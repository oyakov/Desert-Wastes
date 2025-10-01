# Scenes & Flow

## Scene List
| Scene | Responsibility |
| --- | --- |
| Boot | Initialize services, load settings, run smoke tests, present main menu. |
| World | Render overworld map, manage simulation ticks, handle faction interactions, allow embark selection. |
| Base | Manage settlement view, jobs, indirect combat events, and social dynamics. |

An additive UI scene provides HUD, overlays, and shared interface components across World and Base scenes.

## State Flow
1. **Boot**: Composition root initializes DI container, deterministic RNG streams, and Time provider (`docs/ARCHITECTURE.md`).
2. **Generate**: World scene requests WorldData from Generation layer; deterministically creates map, factions, legends seeds.
3. **Simulate**: Overworld ticks yearly, updating factions, logging events, and feeding tension metrics into the Oracle AI for potential deck draws.
4. **Embark**: Player selects leader, team, and site (`docs/BASE_MODE.md`).
5. **Base**: Daily ticks manage jobs, hazards, raids, and Oracle-triggered incidents while writing events to legends.
6. **Return**: On exit, Base state syncs back to World scene; overworld simulation resumes.

## Performance Notes
- Tile layers batched by biome/hazard to minimize draw calls.
- Overlays pooled and updated incrementally per tick.
- Service interfaces for pathfinding and AI operate on pure data to enable burstable job systems in future.
- Profiled update loops separate simulation (fixed timestep) from rendering (frame-based) to maintain determinism.

## Cross-References
- Architecture & assemblies: `docs/ARCHITECTURE.md`
- Test coverage for scene flow: `docs/TEST_PLAN.md`
