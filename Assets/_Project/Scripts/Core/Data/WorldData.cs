using System;
using System.Collections.Generic;
using System.Linq;

namespace Wastelands.Core.Data
{
    /// <summary>
    /// Root snapshot for deterministic world state persistence.
    /// </summary>
    [Serializable]
    public sealed class WorldData
    {
        public const int CurrentVersion = 1;

        public int Version { get; set; } = CurrentVersion;
        public ulong Seed { get; set; }
        public ApocalypseMeta Apocalypse { get; set; } = new();
        public List<Tile> Tiles { get; set; } = new();
        public List<Faction> Factions { get; set; } = new();
        public List<Settlement> Settlements { get; set; } = new();
        public List<Character> Characters { get; set; } = new();
        public List<EventRecord> Events { get; set; } = new();
        public OracleState OracleState { get; set; } = new();
        public List<LegendEntry> Legends { get; set; } = new();
        public BaseState BaseState { get; set; } = new();
    }

    [Serializable]
    public sealed class Tile
    {
        public string Id { get; set; } = string.Empty;
        public Int2 Position { get; set; }
        public float Height { get; set; }
        public float Temperature { get; set; }
        public float Moisture { get; set; }
        public string BiomeId { get; set; } = string.Empty;
        public List<string> HazardTags { get; set; } = new();
    }

    [Serializable]
    public sealed class Faction
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public FactionArchetype Archetype { get; set; }
        public EthosProfile EthosProfile { get; set; } = new();
        public List<RelationRecord> Relations { get; set; } = new();
        public List<NobleRoleAssignment> NobleRoster { get; set; } = new();
        public List<string> Holdings { get; set; } = new();
    }

    public enum FactionArchetype
    {
        Nomads,
        Technocracy,
        Zealots,
        Mercantile,
        Raiders,
        Guardians
    }

    [Serializable]
    public struct EthosProfile
    {
        public float Compassion { get; set; }
        public float Ruthlessness { get; set; }
        public float Tradition { get; set; }
        public float Innovation { get; set; }
    }

    [Serializable]
    public sealed class RelationRecord
    {
        public string TargetFactionId { get; set; } = string.Empty;
        public float Standing { get; set; }
        public RelationState State { get; set; }
    }

    public enum RelationState
    {
        Allied,
        Neutral,
        Hostile
    }

    [Serializable]
    public sealed class NobleRoleAssignment
    {
        public string CharacterId { get; set; } = string.Empty;
        public NobleRole Role { get; set; }
    }

    public enum NobleRole
    {
        Overseer,
        Warlord,
        Quartermaster,
        ResearchChief,
        Steward,
        DiplomaticEnvoy
    }

    [Serializable]
    public sealed class Settlement
    {
        public string Id { get; set; } = string.Empty;
        public string FactionId { get; set; } = string.Empty;
        public string TileId { get; set; } = string.Empty;
        public int Population { get; set; }
        public EconomyProfile Economy { get; set; } = new();
        public float DefenseRating { get; set; }
    }

    [Serializable]
    public struct EconomyProfile
    {
        public float Production { get; set; }
        public float Trade { get; set; }
        public float Research { get; set; }
    }

    [Serializable]
    public sealed class Character
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string FactionId { get; set; } = string.Empty;
        public List<TraitId> Traits { get; set; } = new();
        public Dictionary<SkillId, SkillLevel> Skills { get; set; } = new();
        public List<RelationshipRecord> Relationships { get; set; } = new();
        public NobleRole? CurrentRole { get; set; }
        public CharacterStatus Status { get; set; }
    }

    public enum TraitId
    {
        Stoic,
        Visionary,
        Pragmatic,
        Zealous,
        Empathic,
        Ruthless
    }

    public enum SkillId
    {
        Tactics,
        Leadership,
        Charisma,
        Organization,
        Ethos,
        Industry,
        Research,
        Survival
    }

    [Serializable]
    public struct SkillLevel
    {
        public int Level { get; set; }
        public float Experience { get; set; }
        public float Aptitude { get; set; }
    }

    [Serializable]
    public sealed class RelationshipRecord
    {
        public string TargetId { get; set; } = string.Empty;
        public RelationshipType Type { get; set; }
        public int Intensity { get; set; }
    }

    public enum RelationshipType
    {
        Friendship,
        Rivalry,
        Mentorship,
        Kinship
    }

    public enum CharacterStatus
    {
        Active,
        Missing,
        Dead
    }

    [Serializable]
    public sealed class EventRecord
    {
        public string Id { get; set; } = string.Empty;
        public long Timestamp { get; set; }
        public EventType EventType { get; set; }
        public List<string> Actors { get; set; } = new();
        public string LocationId { get; set; } = string.Empty;
        public Dictionary<string, string> Details { get; set; } = new();
    }

    public enum EventType
    {
        Battle,
        Discovery,
        Mandate,
        Raid,
        Research,
        Catastrophe
    }

    [Serializable]
    public sealed class LegendEntry
    {
        public string Id { get; set; } = string.Empty;
        public List<string> EventIds { get; set; } = new();
        public string Summary { get; set; } = string.Empty;
    }

    [Serializable]
    public sealed class OracleState
    {
        public string ActiveDeckId { get; set; } = string.Empty;
        public float TensionScore { get; set; }
        public Dictionary<string, long> Cooldowns { get; set; } = new();
        public List<EventDeck> AvailableDecks { get; set; } = new();
    }

    [Serializable]
    public sealed class EventDeck
    {
        public string Id { get; set; } = string.Empty;
        public OracleDeckTier Tier { get; set; }
        public float Weight { get; set; }
        public List<EventCard> Cards { get; set; } = new();
    }

    public enum OracleDeckTier
    {
        Minor,
        Major,
        Epic
    }

    [Serializable]
    public sealed class EventCard
    {
        public string Id { get; set; } = string.Empty;
        public List<EventEffect> Effects { get; set; } = new();
        public string Narrative { get; set; } = string.Empty;
    }

    [Serializable]
    public sealed class EventEffect
    {
        public string EffectType { get; set; } = string.Empty;
        public Dictionary<string, string> Parameters { get; set; } = new();
    }

    [Serializable]
    public sealed class ApocalypseMeta
    {
        public ApocalypseType Type { get; set; }
        public float Severity { get; set; }
        public string OriginTileId { get; set; } = string.Empty;
        public List<EraEvent> EraTimeline { get; set; } = new();
    }

    public enum ApocalypseType
    {
        RadiantStorm,
        NanoPlague,
        ArcaneSundering,
        VoidBlight
    }

    [Serializable]
    public sealed class EraEvent
    {
        public long Timestamp { get; set; }
        public string Description { get; set; } = string.Empty;
    }

    [Serializable]
    public sealed class BaseState
    {
        public bool Active { get; set; }
        public string SiteTileId { get; set; } = string.Empty;
        public List<BaseZone> Zones { get; set; } = new();
        public List<string> Population { get; set; } = new();
        public Dictionary<string, float> Infrastructure { get; set; } = new();
        public AlertLevel AlertLevel { get; set; }
        public List<ItemStack> Inventory { get; set; } = new();
        public ResearchState Research { get; set; } = new();
    }

    [Serializable]
    public sealed class BaseZone
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public ZoneType Type { get; set; }
        public float Efficiency { get; set; }
    }

    public enum ZoneType
    {
        Habitat,
        Workshop,
        Farm,
        Watchtower,
        ResearchLab
    }

    public enum AlertLevel
    {
        Calm,
        Elevated,
        Critical
    }

    [Serializable]
    public sealed class ItemStack
    {
        public string ItemId { get; set; } = string.Empty;
        public int Quantity { get; set; }
    }

    [Serializable]
    public sealed class ResearchState
    {
        public List<string> CompletedProjects { get; set; } = new();
        public string ActiveProjectId { get; set; } = string.Empty;
        public float ActiveProgress { get; set; }
    }

    /// <summary>
    /// Simple integer coordinate struct to avoid Unity dependencies.
    /// </summary>
    [Serializable]
    public struct Int2 : IEquatable<Int2>
    {
        public Int2(int x, int y)
        {
            X = x;
            Y = y;
        }

        public int X { get; set; }
        public int Y { get; set; }

        public bool Equals(Int2 other) => X == other.X && Y == other.Y;

        public override bool Equals(object? obj) => obj is Int2 other && Equals(other);

        public override int GetHashCode() => HashCode.Combine(X, Y);

        public override string ToString() => $"({X}, {Y})";
    }

    internal static class DictionaryExtensions
    {
        public static void SortKeysInPlace<T>(this IDictionary<string, T> dictionary)
        {
            if (dictionary.Count <= 1)
            {
                return;
            }

            var ordered = dictionary
                .OrderBy(pair => pair.Key, StringComparer.Ordinal)
                .ToArray();

            dictionary.Clear();
            foreach (var pair in ordered)
            {
                dictionary[pair.Key] = pair.Value;
            }
        }
    }
}
