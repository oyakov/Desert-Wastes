using System;
using System.Collections.Generic;
using System.Linq;

namespace Wastelands.Core.Data
{
    /// <summary>
    /// Ensures collections within <see cref="WorldData"/> follow deterministic ordering before serialization.
    /// </summary>
    public static class WorldDataNormalizer
    {
        public static void Normalize(WorldData world)
        {
            if (world == null)
            {
                throw new ArgumentNullException(nameof(world));
            }

            world.Tiles ??= new List<Tile>();
            world.Factions ??= new List<Faction>();
            world.Settlements ??= new List<Settlement>();
            world.Characters ??= new List<Character>();
            world.Events ??= new List<EventRecord>();
            world.Legends ??= new List<LegendEntry>();
            world.Apocalypse ??= new ApocalypseMeta();
            world.Apocalypse.EraTimeline ??= new List<EraEvent>();
            world.OracleState ??= new OracleState();
            world.OracleState.AvailableDecks ??= new List<EventDeck>();
            world.OracleState.Cooldowns ??= new Dictionary<string, long>();
            world.BaseState ??= new BaseState();
            world.BaseState.Zones ??= new List<BaseZone>();
            world.BaseState.Population ??= new List<string>();
            world.BaseState.Infrastructure ??= new Dictionary<string, float>();
            world.BaseState.Inventory ??= new List<ItemStack>();
            world.BaseState.Research ??= new ResearchState();
            world.BaseState.Research.CompletedProjects ??= new List<string>();

            world.Tiles = world.Tiles
                .OrderBy(t => t.Id, StringComparer.Ordinal)
                .ToList();

            world.Factions = world.Factions
                .OrderBy(f => f.Id, StringComparer.Ordinal)
                .ToList();

            foreach (var faction in world.Factions)
            {
                faction.Relations = faction.Relations
                    .OrderBy(r => r.TargetFactionId, StringComparer.Ordinal)
                    .ToList();

                faction.NobleRoster = faction.NobleRoster
                    .OrderBy(r => r.Role)
                    .ThenBy(r => r.CharacterId, StringComparer.Ordinal)
                    .ToList();

                faction.Holdings = faction.Holdings
                    .OrderBy(id => id, StringComparer.Ordinal)
                    .ToList();
            }

            world.Settlements = world.Settlements
                .OrderBy(s => s.Id, StringComparer.Ordinal)
                .ToList();

            world.Characters = world.Characters
                .OrderBy(c => c.Id, StringComparer.Ordinal)
                .ToList();

            foreach (var character in world.Characters)
            {
                character.Traits ??= new List<TraitId>();
                character.Skills ??= new Dictionary<SkillId, SkillLevel>();
                character.Relationships ??= new List<RelationshipRecord>();

                character.Traits = character.Traits
                    .OrderBy(t => t)
                    .ToList();

                if (character.Skills.Count > 1)
                {
                    var orderedSkills = character.Skills
                        .OrderBy(pair => pair.Key)
                        .ToArray();

                    character.Skills.Clear();
                    foreach (var (skill, level) in orderedSkills)
                    {
                        character.Skills[skill] = level;
                    }
                }

                character.Relationships = character.Relationships
                    .OrderBy(r => r.TargetId, StringComparer.Ordinal)
                    .ThenBy(r => r.Type)
                    .ToList();
            }

            world.Events = world.Events
                .OrderBy(e => e.Timestamp)
                .ThenBy(e => e.Id, StringComparer.Ordinal)
                .ToList();

            foreach (var @event in world.Events)
            {
                @event.Actors ??= new List<string>();
                @event.Details ??= new Dictionary<string, string>();

                @event.Actors = @event.Actors
                    .OrderBy(id => id, StringComparer.Ordinal)
                    .ToList();

                @event.Details.SortKeysInPlace();
            }

            world.OracleState.Cooldowns.SortKeysInPlace();

            world.OracleState.AvailableDecks = world.OracleState.AvailableDecks
                .OrderBy(deck => deck.Tier)
                .ThenBy(deck => deck.Id, StringComparer.Ordinal)
                .ToList();

            foreach (var deck in world.OracleState.AvailableDecks)
            {
                deck.Cards ??= new List<EventCard>();

                deck.Cards = deck.Cards
                    .OrderBy(card => card.Id, StringComparer.Ordinal)
                    .ToList();

                foreach (var card in deck.Cards)
                {
                    card.Effects ??= new List<EventEffect>();

                    card.Effects = card.Effects
                        .OrderBy(effect => effect.EffectType, StringComparer.Ordinal)
                        .ThenBy(effect => effect.Narrative, StringComparer.Ordinal)
                        .ToList();

                    foreach (var effect in card.Effects)
                    {
                        effect.Parameters ??= new Dictionary<string, string>();
                        effect.Parameters.SortKeysInPlace();
                    }
                }
            }

            world.Legends = world.Legends
                .OrderBy(l => l.Id, StringComparer.Ordinal)
                .ToList();

            foreach (var legend in world.Legends)
            {
                legend.EventIds ??= new List<string>();

                legend.EventIds = legend.EventIds
                    .OrderBy(id => id, StringComparer.Ordinal)
                    .ToList();
            }

            if (world.Apocalypse.EraTimeline.Count > 1)
            {
                world.Apocalypse.EraTimeline = world.Apocalypse.EraTimeline
                    .OrderBy(e => e.Timestamp)
                    .ThenBy(e => e.Description, StringComparer.Ordinal)
                    .ToList();
            }

            var baseState = world.BaseState ?? new BaseState();
            baseState.Zones = baseState.Zones
                .OrderBy(z => z.Id, StringComparer.Ordinal)
                .ToList();

            baseState.Population = baseState.Population
                .OrderBy(id => id, StringComparer.Ordinal)
                .ToList();

            baseState.Infrastructure.SortKeysInPlace();

            baseState.Inventory = baseState.Inventory
                .OrderBy(stack => stack.ItemId, StringComparer.Ordinal)
                .ToList();

            baseState.Research.CompletedProjects = baseState.Research.CompletedProjects
                .OrderBy(id => id, StringComparer.Ordinal)
                .ToList();

            world.BaseState = baseState;
        }
    }
}
