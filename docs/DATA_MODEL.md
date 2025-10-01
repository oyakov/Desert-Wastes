# Data Model Overview

## High-Level Schema
```
WorldData
 ├─ Seed : ulong
 ├─ ApocalypseMeta
 ├─ Tiles : List<Tile>
 ├─ Factions : List<Faction>
 ├─ Settlements : List<Settlement>
 ├─ Characters : List<Character>
 ├─ Events : List<EventRecord>
 ├─ OracleState : OracleState
 ├─ Legends : List<LegendEntry>
 └─ BaseState : BaseState
```

### Tile
| Field | Type | Notes |
| --- | --- | --- |
| Id | string | Stable ID `tile_{x}_{y}` |
| Position | Vector2Int (serialized) | Plain ints to avoid Unity dependency. |
| Height | float |
| Temperature | float |
| Moisture | float |
| BiomeId | string |
| HazardTags | string[] |

### Faction
| Field | Type | Notes |
| Id | string | `fac_{guid}` |
| Name | string |
| Archetype | enum |
| EthosProfile | struct |
| Relations | List<RelationRecord> |
| NobleRoster | List<NobleRoleAssignment> |
| Holdings | List<string> | Settlement IDs |

### Settlement
| Field | Type | Notes |
| Id | string |
| FactionId | string | Foreign key → Faction.Id |
| TileId | string | Foreign key → Tile.Id |
| Population | int |
| EconomyProfile | struct |
| DefenseRating | float |

### Character
| Field | Type | Notes |
| Id | string |
| Name | string |
| FactionId | string |
| Traits | List<TraitId> |
| Skills | Dictionary<SkillId, SkillLevel> |
| Relationships | List<RelationshipRecord> |
| CurrentRole | NobleRole? |
| Status | enum (Active, Missing, Dead) |

### Skills & Traits
- `SkillId` values: Tactics, Leadership, Charisma, Organization, Ethos, Industry, Research, Survival.
- `TraitId` values align with personalities in `docs/SOCIAL_NOBLES.md`.
- Skill levels stored as structs with current level, experience, and aptitude modifiers.

### Events & Legends
| Field | Type | Notes |
| EventRecord.Id | string |
| Timestamp | long | Tick count from deterministic time provider. |
| EventType | enum |
| Actors | string[] | Character IDs |
| LocationId | string | Tile/Settlement/Base reference |
| Details | Dictionary<string, string> |

`LegendEntry` references EventRecord.Id sequences to allow timeline reconstruction (see `docs/WORLDGEN.md`).

### OracleState
| Field | Type | Notes |
| --- | --- | --- |
| ActiveDeckId | string | Last deck drawn (Minor, Major, Epic variants). |
| TensionScore | float | Derived from overworld stressors; drives deck eligibility. |
| Cooldowns | Dictionary<string, long> | Prevents repeat interventions across ticks. |
| AvailableDecks | List<EventDeck> | Configurable deck definitions. |

`EventDeck`
| Field | Type | Notes |
| --- | --- | --- |
| Id | string | `deck_minor_01` etc. |
| Tier | enum (Minor, Major, Epic) |
| Weight | float | Weighted deterministic selection seed. |
| Cards | List<EventCard> | Ordered for deterministic draws. |

`EventCard`
| Field | Type | Notes |
| --- | --- | --- |
| Id | string | Stable card identifier (e.g., `card_rise_nemesis`). |
| Effects | List<EventEffect> | Declarative payload consumed by services (`docs/ARCHITECTURE.md`). |
| Narrative | string | Narrative snippet written directly into legends/alerts. |

### ApocalypseMeta
| Field | Type | Notes |
| Type | enum (Radiant Storm, Nano Plague, Arcane Sundering, etc.) |
| Severity | float |
| OriginTileId | string |
| EraTimeline | List<EraEvent> |

### BaseState
| Field | Type | Notes |
| Active | bool |
| SiteTileId | string |
| Zones | List<BaseZone> |
| Population | List<CharacterId> |
| Infrastructure | Dictionary<string, float> |
| AlertLevel | enum |
| Inventory | List<ItemStack> |
| Research | ResearchState |

## Serialization Approach
- Use JSON (UTF-8) for save snapshots, legends export, and deterministic test fixtures.
- Maintain schema versioning via `WorldData.Version` field to support migrations.
- Ensure deterministic ordering of collections (sorted lists) before serialization (verified in `docs/TEST_PLAN.md`).

## IDs & Referential Integrity
- IDs are string-based but generated via deterministic GUID-with-seed hashing to guarantee reproducibility.
- All foreign keys validated during save/load tests (see `docs/TEST_PLAN.md`).
- No implicit references to UnityEngine objects; pure data resides in Core assembly definitions.

## Cross-References
- Architectural layering: `docs/ARCHITECTURE.md`
- Social systems: `docs/SOCIAL_NOBLES.md`
- Base systems: `docs/BASE_MODE.md`
- Oracle interventions: `docs/AI_COMBAT_INDIRECT.md`, `docs/TEST_PLAN.md`
