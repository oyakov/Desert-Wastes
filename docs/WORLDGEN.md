# World Generation & History Simulation

## Historical Eras
1. **Pre-Fall**: Flourishing civilizations, baseline biome distribution, thriving trade.
2. **Cataclysm**: Apocalypse event reshapes terrain, climate, and resource access.
3. **Fracture**: Societies splinter into factions, relic sites emerge, hazards persist.
4. **Present**: Player era; factions fight for survival and dominance.

## Map Generation Pipeline
1. **Seed Intake**: Single 64-bit seed enters deterministic RNG wrapper (see `docs/TEST_PLAN.md`).
2. **Heightmap**: `GenerateTiles` samples deterministic noise channels (`worldgen.heightmap`) to produce height values and stable tile IDs.
3. **Climate**: Temperature/moisture derive from latitude formulas plus channel noise (`worldgen.climate`).
4. **Biomes**: `SelectBiome` categorizes tiles using height/temperature/moisture thresholds and apocalypse modifiers (scorched deserts, fungal forests).
5. **Hazards**: `DetermineHazards` overlays apocalypse-specific tags (radiation zones, nanite storms) with channel weighting.
6. **Resources**: `DetermineResources` injects biome-weighted supply nodes via `worldgen.resources` channel.

## Faction Seeding
- Identify habitable clusters by biome and resource richness.
- Assign faction archetypes (Nomads, Technocracy, Zealots, Guardians, etc.) using deterministic RNG seeded by faction index.
- Determine leadership traits and noble lineages per faction; create leaders tied to `NobleRole` slots for immediate Base Mode use.
- Establish diplomacy predispositions (alliances, rivalries) stored in `RelationRecord` collections for overworld simulation seeding.

## Relic & Hazard Sites
- **Relic Sites**: Remnants of Pre-Fall tech with unique modifiers; require expedition-level skills.
- **Hazards**: Persistent threats scaling with proximity to cataclysm epicenters; impact trade routes and embark viability.
- **Legends Hooks**: Each site logs discoveries, tragedies, and conquests.

## Legends Log Design
- Structured entries capturing era, actors, location IDs, outcomes, and consequences.
- Supports chronological queries and filtered playback (per faction, per site).
- Stored in JSON for readability and deterministic testing (see `docs/DATA_MODEL.md`).

## Determinism Guarantees
- Single seed reproduces terrain, factions, sites, and initial diplomacy.
- RNG wrapper exposes deterministic streams for terrain, factions, relics, and hazards to avoid cross-system interference.
- Generation functions are pure where possible, returning data models defined in `docs/DATA_MODEL.md`.

## Cross-References
- Data schema: `docs/DATA_MODEL.md`
- Simulation tick usage: `docs/ARCHITECTURE.md`
- Testing strategy: `docs/TEST_PLAN.md`
- Feature context: `docs/DESIGN.md`
