using System.Collections.Generic;
using Wastelands.Core.Data;

namespace Wastelands.Tests.EditMode
{
    internal static class SampleWorldBuilder
    {
        public static WorldData CreateValidWorld()
        {
            var tile = new Tile
            {
                Id = "tile_0_0",
                Position = new Int2(0, 0),
                BiomeId = "biome_desert",
                HazardTags = new List<string> { "dust" }
            };

            var faction = new Faction
            {
                Id = "fac_alpha",
                Name = "Alpha",
                Archetype = FactionArchetype.Nomads,
                NobleRoster = new List<NobleRoleAssignment>(),
                Relations = new List<RelationRecord>()
            };

            var settlement = new Settlement
            {
                Id = "set_01",
                FactionId = faction.Id,
                TileId = tile.Id,
                Population = 100,
                Economy = new EconomyProfile { Production = 1f, Trade = 1f, Research = 0.2f }
            };

            var character = new Character
            {
                Id = "char_leader",
                Name = "Leader",
                FactionId = faction.Id,
                Traits = new List<TraitId> { TraitId.Stoic },
                Skills = new Dictionary<SkillId, SkillLevel>
                {
                    { SkillId.Leadership, new SkillLevel { Level = 3, Experience = 10, Aptitude = 1.1f } }
                },
                Relationships = new List<RelationshipRecord>()
            };

            faction.NobleRoster.Add(new NobleRoleAssignment { CharacterId = character.Id, Role = NobleRole.Overseer });
            faction.Holdings.Add(settlement.Id);

            var world = new WorldData
            {
                Seed = 42,
                Tiles = new List<Tile> { tile },
                Factions = new List<Faction> { faction },
                Settlements = new List<Settlement> { settlement },
                Characters = new List<Character> { character },
                Events = new List<EventRecord>
                {
                    new()
                    {
                        Id = "event_01",
                        Timestamp = 1,
                        EventType = EventType.Discovery,
                        Actors = new List<string> { character.Id },
                        LocationId = tile.Id,
                        Details = new Dictionary<string, string> { { "resource", "water" } }
                    }
                },
                OracleState = new OracleState
                {
                    ActiveDeckId = "deck_minor_01",
                    TensionScore = 0.5f,
                    Cooldowns = new Dictionary<string, long> { { "card_rise_nemesis", 10 } },
                    AvailableDecks = new List<EventDeck>
                    {
                        new()
                        {
                            Id = "deck_minor_01",
                            Tier = OracleDeckTier.Minor,
                            Weight = 1f,
                            Cards = new List<EventCard>
                            {
                                new()
                                {
                                    Id = "card_rise_nemesis",
                                    Narrative = "Nemesis stirs",
                                    Effects = new List<EventEffect>
                                    {
                                        new()
                                        {
                                            EffectType = "spawn_event",
                                            Parameters = new Dictionary<string, string> { { "target", settlement.Id } }
                                        }
                                    }
                                }
                            }
                        }
                    }
                },
                Legends = new List<LegendEntry>
                {
                    new()
                    {
                        Id = "legend_01",
                        Summary = "Found water",
                        EventIds = new List<string> { "event_01" }
                    }
                },
                Apocalypse = new ApocalypseMeta
                {
                    Type = ApocalypseType.RadiantStorm,
                    Severity = 0.7f,
                    OriginTileId = tile.Id,
                    EraTimeline = new List<EraEvent>
                    {
                        new() { Timestamp = 0, Description = "Storm begins" }
                    }
                },
                BaseState = new BaseState
                {
                    Active = true,
                    SiteTileId = tile.Id,
                    Zones = new List<BaseZone>
                    {
                        new() { Id = "zone_hab", Name = "Hab", Type = ZoneType.Habitat, Efficiency = 1f }
                    },
                    Population = new List<string> { character.Id },
                    Infrastructure = new Dictionary<string, float> { { "power", 1f } },
                    Inventory = new List<ItemStack>
                    {
                        new() { ItemId = "water", Quantity = 10 }
                    },
                    AlertLevel = AlertLevel.Calm,
                    Research = new ResearchState
                    {
                        CompletedProjects = new List<string> { "tech_filters" },
                        ActiveProjectId = "tech_drills",
                        ActiveProgress = 0.5f
                    }
                }
            };

            faction.Relations.Add(new RelationRecord { TargetFactionId = faction.Id, Standing = 1f, State = RelationState.Allied });

            return world;
        }
    }
}
