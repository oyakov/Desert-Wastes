using System;
using System.Collections.Generic;
using System.Linq;
using Wastelands.Core.Data;
using Wastelands.Core.Services;

namespace Wastelands.Generation
{
    public readonly struct OverworldGenerationConfig
    {
        public OverworldGenerationConfig(ulong seed, int width, int height, ApocalypseType apocalypse)
        {
            if (width <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(width));
            }

            if (height <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(height));
            }

            Seed = seed;
            Width = width;
            Height = height;
            Apocalypse = apocalypse;
        }

        public ulong Seed { get; }
        public int Width { get; }
        public int Height { get; }
        public ApocalypseType Apocalypse { get; }
    }

    public interface IOverworldGenerationPipeline
    {
        WorldData Generate(OverworldGenerationConfig config);
    }

    /// <summary>
    /// Deterministic overworld generation pipeline that follows the steps outlined in docs/WORLDGEN.md.
    /// </summary>
    public sealed class OverworldGenerationPipeline : IOverworldGenerationPipeline
    {
        private const string HeightmapChannel = "worldgen.heightmap";
        private const string ClimateChannel = "worldgen.climate";
        private const string BiomeChannel = "worldgen.biomes";
        private const string HazardChannel = "worldgen.hazards";
        private const string ResourceChannel = "worldgen.resources";
        private const int DefaultFactionCount = 3;

        private readonly IRngService _rngService;

        public OverworldGenerationPipeline(IRngService rngService)
        {
            _rngService = rngService ?? throw new ArgumentNullException(nameof(rngService));
        }

        public WorldData Generate(OverworldGenerationConfig config)
        {
            var world = new WorldData
            {
                Seed = config.Seed,
                Apocalypse = new ApocalypseMeta
                {
                    Type = config.Apocalypse,
                    Severity = 0.5f,
                }
            };

            var tiles = GenerateTiles(config);
            world.Tiles = tiles;

            PopulateApocalypseMetadata(world, config, tiles);

            var factions = SeedFactions(world, config, tiles);
            world.Factions = factions;

            var settlements = CreateSettlements(tiles, factions);
            world.Settlements = settlements;

            var characters = CreateLeaders(factions);
            world.Characters = characters;

            HookupFactionReferences(factions, settlements, characters);

            world.Events = CreateInitialEvents(world);
            world.Legends = CreateInitialLegends(world.Events);
            world.OracleState = CreateOracleState();
            world.BaseState = CreateBaseState(settlements, characters, world);

            WorldDataNormalizer.Normalize(world);
            return world;
        }

        private List<Tile> GenerateTiles(OverworldGenerationConfig config)
        {
            var tiles = new List<Tile>(config.Width * config.Height);
            var heightRng = _rngService.GetChannel(HeightmapChannel);
            var climateRng = _rngService.GetChannel(ClimateChannel);
            var biomeRng = _rngService.GetChannel(BiomeChannel);
            var hazardRng = _rngService.GetChannel(HazardChannel);
            var resourceRng = _rngService.GetChannel(ResourceChannel);

            for (var y = 0; y < config.Height; y++)
            {
                for (var x = 0; x < config.Width; x++)
                {
                    var tileId = $"tile_{x}_{y}";
                    var height = SampleSigned(heightRng, config.Seed, x, y, 11);
                    var latitude = (float)y / Math.Max(1, config.Height - 1);
                    var temperatureBase = 1f - (float)Math.Abs(latitude - 0.5f) * 2f; // equator warmer
                    var temperatureNoise = (float)(Sample(heightRng, config.Seed, x, y, 29) * 0.35f);
                    var temperature = Clamp01(temperatureBase + temperatureNoise);

                    var moisture = Clamp01((float)Sample(climateRng, config.Seed, x, y, 7));
                    var biome = SelectBiome(height, temperature, moisture, biomeRng, config.Seed, x, y);
                    var hazards = DetermineHazards(config.Apocalypse, hazardRng, config.Seed, x, y, temperature, biome);
                    var resources = DetermineResources(resourceRng, config.Seed, x, y, biome);

                    var tile = new Tile
                    {
                        Id = tileId,
                        Position = new Int2(x, y),
                        Height = height,
                        Temperature = temperature,
                        Moisture = moisture,
                        BiomeId = biome,
                        HazardTags = hazards,
                    };

                    if (resources.Count > 0)
                    {
                        tile.HazardTags.AddRange(resources);
                    }

                    tiles.Add(tile);
                }
            }

            return tiles;
        }

        private static float SampleSigned(IRngChannel channel, ulong seed, int x, int y, int salt)
        {
            var value = Sample(channel, seed, x, y, salt);
            return (float)(value * 2d - 1d);
        }

        private static double Sample(IRngChannel channel, ulong seed, int x, int y, int salt)
        {
            channel.Reseed(DeriveOffset(seed, x, y, salt));
            return channel.NextDouble();
        }

        private static int DeriveOffset(ulong seed, int x, int y, int salt)
        {
            unchecked
            {
                var lower = (int)(seed & 0xFFFFFFFF);
                var upper = (int)((seed >> 32) & 0xFFFFFFFF);
                return HashCode.Combine(lower, upper, x, y, salt);
            }
        }

        private static string SelectBiome(float height, float temperature, float moisture, IRngChannel channel, ulong seed, int x, int y)
        {
            if (height < -0.1f)
            {
                return "biome_sunken_basin";
            }

            if (height > 0.6f)
            {
                return temperature > 0.5f ? "biome_crimson_mesa" : "biome_frozen_peak";
            }

            if (moisture < 0.25f)
            {
                return temperature > 0.6f ? "biome_glass_desert" : "biome_shattered_steppe";
            }

            if (moisture > 0.75f)
            {
                return temperature > 0.5f ? "biome_fungal_forest" : "biome_rust_mire";
            }

            var roll = Sample(channel, seed, x, y, 53);
            return roll > 0.5d ? "biome_ashen_plains" : "biome_marrow_fields";
        }

        private static List<string> DetermineHazards(ApocalypseType apocalypse, IRngChannel channel, ulong seed, int x, int y, float temperature, string biome)
        {
            var hazards = new List<string>();
            var severityRoll = Sample(channel, seed, x, y, 71);
            if (severityRoll > 0.7d)
            {
                hazards.Add(apocalypse switch
                {
                    ApocalypseType.RadiantStorm => "haz_radiant_flux",
                    ApocalypseType.NanoPlague => "haz_nanite_bloom",
                    ApocalypseType.ArcaneSundering => "haz_void_rupture",
                    ApocalypseType.VoidBlight => "haz_hollow_winds",
                    _ => "haz_unknown"
                });
            }

            if (temperature > 0.8f && !hazards.Contains("haz_radiant_flux"))
            {
                hazards.Add("haz_solar_scorch");
            }

            if (biome == "biome_fungal_forest" && !hazards.Contains("haz_sporefall"))
            {
                hazards.Add("haz_sporefall");
            }

            return hazards;
        }

        private static List<string> DetermineResources(IRngChannel channel, ulong seed, int x, int y, string biome)
        {
            var list = new List<string>();
            var roll = Sample(channel, seed, x, y, 83);
            if (roll > 0.85d)
            {
                list.Add("res_relic_cache");
            }
            else if (roll > 0.55d)
            {
                list.Add(biome switch
                {
                    "biome_glass_desert" => "res_silica_vein",
                    "biome_fungal_forest" => "res_myco_spores",
                    "biome_crimson_mesa" => "res_iron_spine",
                    _ => "res_salvage_field"
                });
            }

            return list;
        }

        private static void PopulateApocalypseMetadata(WorldData world, OverworldGenerationConfig config, IReadOnlyList<Tile> tiles)
        {
            var centerTile = tiles.OrderBy(t => DistanceToCenter(t.Position, config)).First();
            world.Apocalypse.OriginTileId = centerTile.Id;
            world.Apocalypse.EraTimeline = new List<EraEvent>
            {
                new() { Timestamp = 0, Description = "Pre-Fall prosperity" },
                new() { Timestamp = 1, Description = "Cataclysm reshapes the wastes" },
                new() { Timestamp = 2, Description = "Factions fracture into splinters" },
                new() { Timestamp = 3, Description = "Present day conflicts ignite" }
            };
        }

        private static double DistanceToCenter(Int2 position, OverworldGenerationConfig config)
        {
            var centerX = (config.Width - 1) / 2.0;
            var centerY = (config.Height - 1) / 2.0;
            var dx = position.X - centerX;
            var dy = position.Y - centerY;
            return Math.Sqrt(dx * dx + dy * dy);
        }

        private List<Faction> SeedFactions(WorldData world, OverworldGenerationConfig config, IReadOnlyList<Tile> tiles)
        {
            var viableTiles = tiles
                .Where(t => t.HazardTags.All(tag => !tag.StartsWith("haz_", StringComparison.Ordinal) || tag == "haz_sporefall"))
                .OrderBy(t => DistanceToCenter(t.Position, config))
                .Take(DefaultFactionCount)
                .ToList();

            var channel = _rngService.GetChannel("worldgen.factions");

            var archetypes = Enum.GetValues(typeof(FactionArchetype)).Cast<FactionArchetype>().ToArray();
            var factions = new List<Faction>();
            for (var index = 0; index < viableTiles.Count; index++)
            {
                var tile = viableTiles[index];
                channel.Reseed(DeriveOffset(world.Seed, tile.Position.X, tile.Position.Y, 97));
                var archetype = archetypes[channel.NextInt(0, archetypes.Length)];

                var faction = new Faction
                {
                    Id = $"fac_{index:D2}",
                    Name = GenerateFactionName(channel, world.Seed, tile.Position),
                    Archetype = archetype,
                    EthosProfile = new EthosProfile
                    {
                        Compassion = (float)Sample(channel, world.Seed, tile.Position.X, tile.Position.Y, 101),
                        Ruthlessness = (float)Sample(channel, world.Seed, tile.Position.X, tile.Position.Y, 103),
                        Tradition = (float)Sample(channel, world.Seed, tile.Position.X, tile.Position.Y, 107),
                        Innovation = (float)Sample(channel, world.Seed, tile.Position.X, tile.Position.Y, 109)
                    },
                    Relations = new List<RelationRecord>(),
                    NobleRoster = new List<NobleRoleAssignment>(),
                    Holdings = new List<string>()
                };

                factions.Add(faction);
            }

            foreach (var faction in factions)
            {
                foreach (var other in factions)
                {
                    if (ReferenceEquals(faction, other))
                    {
                        continue;
                    }

                    var relationStrength = 0.25f + 0.5f * (float)Sample(channel, world.Seed, faction.Id.GetHashCode(), other.Id.GetHashCode(), 113);
                    faction.Relations.Add(new RelationRecord
                    {
                        TargetFactionId = other.Id,
                        Standing = relationStrength,
                        State = relationStrength > 0.6f ? RelationState.Allied : relationStrength < 0.35f ? RelationState.Hostile : RelationState.Neutral
                    });
                }
            }

            return factions;
        }

        private static string GenerateFactionName(IRngChannel channel, ulong seed, Int2 position)
        {
            var prefixes = new[] { "Dust", "Iron", "Solar", "Echo", "Shard" };
            var suffixes = new[] { "Walkers", "Legion", "Covenant", "Collective", "Syndicate" };
            channel.Reseed(DeriveOffset(seed, position.X, position.Y, 131));
            var prefix = prefixes[channel.NextInt(0, prefixes.Length)];
            var suffix = suffixes[channel.NextInt(0, suffixes.Length)];
            return $"{prefix} {suffix}";
        }

        private static List<Settlement> CreateSettlements(IEnumerable<Tile> tiles, IReadOnlyList<Faction> factions)
        {
            var settlements = new List<Settlement>();
            var paired = tiles.Zip(factions, (tile, faction) => (tile, faction)).ToList();
            foreach (var (tile, faction) in paired)
            {
                var settlement = new Settlement
                {
                    Id = $"set_{tile.Position.X}_{tile.Position.Y}",
                    FactionId = faction.Id,
                    TileId = tile.Id,
                    Population = 150 + tile.Position.X * 5 + tile.Position.Y * 3,
                    Economy = new EconomyProfile
                    {
                        Production = Clamp(tile.Height + 1f, 0f, 2f),
                        Trade = Clamp(tile.Moisture + 0.5f, 0f, 2f),
                        Research = Clamp(tile.Temperature + 0.3f, 0f, 2f)
                    },
                    DefenseRating = 0.4f + tile.Height * 0.3f
                };

                settlements.Add(settlement);
            }

            return settlements;
        }

        private static List<Character> CreateLeaders(IReadOnlyList<Faction> factions)
        {
            var characters = new List<Character>();
            foreach (var faction in factions)
            {
                var character = new Character
                {
                    Id = $"char_{faction.Id}",
                    Name = $"{faction.Name} Primus",
                    FactionId = faction.Id,
                    Traits = new List<TraitId> { TraitId.Visionary },
                    Skills = new Dictionary<SkillId, SkillLevel>
                    {
                        { SkillId.Leadership, new() { Level = 4, Experience = 25, Aptitude = 1.2f } },
                        { SkillId.Tactics, new() { Level = 3, Experience = 18, Aptitude = 1.05f } }
                    },
                    Relationships = new List<RelationshipRecord>(),
                    CurrentRole = NobleRole.Overseer,
                    Status = CharacterStatus.Active
                };

                characters.Add(character);
            }

            return characters;
        }

        private static void HookupFactionReferences(IEnumerable<Faction> factions, IEnumerable<Settlement> settlements, IEnumerable<Character> characters)
        {
            var characterLookup = characters.ToDictionary(c => c.FactionId, c => c, StringComparer.Ordinal);
            foreach (var faction in factions)
            {
                if (characterLookup.TryGetValue(faction.Id, out var leader))
                {
                    faction.NobleRoster.Add(new NobleRoleAssignment { CharacterId = leader.Id, Role = NobleRole.Overseer });
                }

                foreach (var settlement in settlements.Where(s => s.FactionId == faction.Id))
                {
                    faction.Holdings.Add(settlement.Id);
                }
            }
        }

        private static List<EventRecord> CreateInitialEvents(WorldData world)
        {
            return new List<EventRecord>
            {
                new()
                {
                    Id = "event_foundation",
                    Timestamp = 0,
                    EventType = EventType.Discovery,
                    Actors = world.Characters.Select(c => c.Id).ToList(),
                    LocationId = world.Settlements.First().Id,
                    Details = new Dictionary<string, string>
                    {
                        { "message", "Settlements established after the Cataclysm" }
                    }
                }
            };
        }

        private static List<LegendEntry> CreateInitialLegends(IEnumerable<EventRecord> events)
        {
            return new List<LegendEntry>
            {
                new()
                {
                    Id = "legend_reclamation",
                    Summary = "The surviving houses carve footholds into the wastes.",
                    EventIds = events.Select(e => e.Id).ToList()
                }
            };
        }

        private static OracleState CreateOracleState()
        {
            return new OracleState
            {
                ActiveDeckId = "deck_minor_intro",
                TensionScore = 0.35f,
                Cooldowns = new Dictionary<string, long>(),
                AvailableDecks = new List<EventDeck>
                {
                    new()
                    {
                        Id = "deck_minor_intro",
                        Tier = OracleDeckTier.Minor,
                        Weight = 1f,
                        Cards = new List<EventCard>
                        {
                            new()
                            {
                                Id = "card_supply_cache",
                                Narrative = "A cache of pre-fall supplies appears in the wastes.",
                                Effects = new List<EventEffect>
                                {
                                    new()
                                    {
                                        EffectType = "spawn_cache",
                                        Parameters = new Dictionary<string, string>
                                        {
                                            { "rarity", "uncommon" }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            };
        }

        private static BaseState CreateBaseState(IReadOnlyList<Settlement> settlements, IReadOnlyList<Character> characters, WorldData world)
        {
            var anchorSettlement = settlements.FirstOrDefault();
            var leader = characters.FirstOrDefault();
            return new BaseState
            {
                Active = false,
                SiteTileId = anchorSettlement?.TileId ?? world.Tiles.First().Id,
                Zones = new List<BaseZone>
                {
                    new()
                    {
                        Id = "zone_command",
                        Name = "Command Nexus",
                        Type = ZoneType.Watchtower,
                        Efficiency = 0.85f
                    }
                },
                Population = leader != null ? new List<string> { leader.Id } : new List<string>(),
                Infrastructure = new Dictionary<string, float>
                {
                    { "power", 0.75f },
                    { "water", 0.65f }
                },
                Inventory = new List<ItemStack>
                {
                    new() { ItemId = "supply_basic", Quantity = 25 }
                },
                AlertLevel = AlertLevel.Calm,
                Research = new ResearchState
                {
                    CompletedProjects = new List<string>(),
                    ActiveProjectId = string.Empty,
                    ActiveProgress = 0f
                }
            };
        }

        private static float Clamp(float value, float min, float max)
        {
            if (value < min)
            {
                return min;
            }

            if (value > max)
            {
                return max;
            }

            return value;
        }

        private static float Clamp01(float value) => Clamp(value, 0f, 1f);
    }
}
