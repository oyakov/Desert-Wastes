using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Wastelands.Core.Data;

namespace Wastelands.Simulation.Phases
{
    internal static class PhaseMath
    {
        public static float Clamp01(float value)
        {
            if (value < 0f)
            {
                return 0f;
            }

            if (value > 1f)
            {
                return 1f;
            }

            return value;
        }
    }

    public sealed class HazardPropagationPhase : IOverworldSimulationPhase
    {
        public string Name => "hazards";

        public void Execute(in OverworldTickContext context)
        {
            var channel = context.GetChannel(Name);
            var severityDelta = (float)(channel.NextDouble() * 0.06d - 0.03d);
            context.World.Apocalypse.Severity = PhaseMath.Clamp01(context.World.Apocalypse.Severity + severityDelta);

            if (context.World.Apocalypse.Severity < 0.6f)
            {
                return;
            }

            foreach (var tile in context.World.Tiles)
            {
                if (tile.Temperature > 0.7f && !tile.HazardTags.Contains("haz_scorch_wave"))
                {
                    tile.HazardTags.Add("haz_scorch_wave");
                }
            }
        }
    }

    public sealed class FactionDiplomacyPhase : IOverworldSimulationPhase
    {
        public string Name => "diplomacy";

        public void Execute(in OverworldTickContext context)
        {
            foreach (var faction in context.World.Factions)
            {
                foreach (var relation in faction.Relations)
                {
                    var channel = context.GetChannel($"{Name}.{faction.Id}.{relation.TargetFactionId}");
                    var delta = (float)(channel.NextDouble() * 0.1d - 0.05d);
                    relation.Standing = PhaseMath.Clamp01(relation.Standing + delta);
                    relation.State = relation.Standing switch
                    {
                        > 0.65f => RelationState.Allied,
                        < 0.35f => RelationState.Hostile,
                        _ => RelationState.Neutral
                    };
                }
            }
        }
    }

    public sealed class SettlementLogisticsPhase : IOverworldSimulationPhase
    {
        public string Name => "logistics";

        public void Execute(in OverworldTickContext context)
        {
            foreach (var settlement in context.World.Settlements)
            {
                var channel = context.GetChannel($"{Name}.{settlement.Id}");
                var productionDelta = (float)(channel.NextDouble() * 0.08d - 0.04d);
                var tradeDelta = (float)(channel.NextDouble() * 0.06d - 0.03d);
                var researchDelta = (float)(channel.NextDouble() * 0.05d - 0.025d);

                settlement.Economy = new EconomyProfile
                {
                    Production = ClampToRange(settlement.Economy.Production + productionDelta, 0f, 3f),
                    Trade = ClampToRange(settlement.Economy.Trade + tradeDelta, 0f, 3f),
                    Research = ClampToRange(settlement.Economy.Research + researchDelta, 0f, 3f)
                };

                settlement.DefenseRating = ClampToRange(settlement.DefenseRating + productionDelta * 0.25f, 0f, 5f);
            }
        }

        private static float ClampToRange(float value, float min, float max)
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
    }

    public sealed class OracleReviewPhase : IOverworldSimulationPhase
    {
        public string Name => "oracle";

        public void Execute(in OverworldTickContext context)
        {
            var channel = context.GetChannel(Name);
            var tensionDelta = (float)(channel.NextDouble() * 0.08d - 0.04d);
            var severityInfluence = (context.World.Apocalypse.Severity - 0.5f) * 0.1f;
            var newTension = PhaseMath.Clamp01(context.World.OracleState.TensionScore + tensionDelta + severityInfluence);
            context.World.OracleState.TensionScore = newTension;

            var cooldownKeys = context.World.OracleState.Cooldowns.Keys.ToList();
            foreach (var key in cooldownKeys)
            {
                var value = context.World.OracleState.Cooldowns[key];
                context.World.OracleState.Cooldowns[key] = Math.Max(0, value - 1);
            }

            var activeDeck = context.World.OracleState.AvailableDecks
                .OrderByDescending(deck => deck.Tier)
                .FirstOrDefault();

            if (activeDeck != null && newTension > 0.6f)
            {
                context.World.OracleState.ActiveDeckId = activeDeck.Id;
            }
        }
    }

    public sealed class LegendCompilationPhase : IOverworldSimulationPhase
    {
        public string Name => "legends";

        public void Execute(in OverworldTickContext context)
        {
            var world = context.World;
            var eventId = $"event_tick_{context.Tick:D6}";
            if (world.Events.Any(e => e.Id == eventId))
            {
                return;
            }

            var actorId = world.Characters.FirstOrDefault()?.Id;
            var locationId = world.Settlements.FirstOrDefault()?.Id ?? world.BaseState.SiteTileId;

            var record = new EventRecord
            {
                Id = eventId,
                Timestamp = context.Tick,
                EventType = EventType.Mandate,
                Actors = string.IsNullOrEmpty(actorId) ? new List<string>() : new List<string> { actorId },
                LocationId = locationId,
                Details = new Dictionary<string, string>
                {
                    { "apocalypseSeverity", world.Apocalypse.Severity.ToString("0.00", CultureInfo.InvariantCulture) },
                    { "tension", world.OracleState.TensionScore.ToString("0.00", CultureInfo.InvariantCulture) }
                }
            };

            world.Events.Add(record);
            world.Legends.Add(new LegendEntry
            {
                Id = $"legend_tick_{context.Tick:D6}",
                Summary = $"Year {context.Tick}: factions adapt to the wastes.",
                EventIds = new List<string> { record.Id }
            });

            context.EventBus.Publish(new OverworldLegendUpdated(record.Id));
        }
    }

    public readonly struct OverworldLegendUpdated
    {
        public OverworldLegendUpdated(string eventId)
        {
            EventId = eventId ?? throw new ArgumentNullException(nameof(eventId));
        }

        public string EventId { get; }
    }
}
