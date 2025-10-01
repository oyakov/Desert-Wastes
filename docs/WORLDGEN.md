# World Generation & History Simulation

## Historical Eras
1. **Pre-Fall**: Flourishing civilizations, baseline biome distribution, thriving trade.
2. **Cataclysm**: Apocalypse event reshapes terrain, climate, and resource access.
3. **Fracture**: Societies splinter into factions, relic sites emerge, hazards persist.
4. **Present**: Player era; factions fight for survival and dominance.

## Map Generation Pipeline
1. **Seed Intake**: Single 64-bit seed enters deterministic RNG wrapper (see `docs/TEST_PLAN.md`).
2. **Heightmap**: Generate base landmass using layered noise; enforce tectonic-style ridgelines.
3. **Climate**: Derive temperature and moisture via latitude, elevation, and prevailing winds.
4. **Biomes**: Map tiles to biomes using height/temp/moisture thresholds with apocalypse modifiers (e.g., scorched deserts, fungal forests).
5. **Hazards**: Overlay apocalypse-type hazards (radiation zones, nanite storms) with weighted falloff.
6. **Resources**: Place resource veins and relic caches with biome-specific tables.

## Faction Seeding
- Identify habitable clusters by biome and resource richness.
- Assign faction archetypes (warlords, scholars, merchants) influenced by era events.
- Determine leadership traits and noble lineages per faction.
- Establish diplomacy predispositions (alliances, rivalries) for simulation seeding.

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
