using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Wastelands.Core.Data;
using Wastelands.Persistence;

namespace Wastelands.Tests.EditMode
{
    public class WorldDataSerializerTests
    {
        [Test]
        public void Serialize_ProducesDeterministicOrdering()
        {
            var world = SampleWorldBuilder.CreateValidWorld();
            var worldCopy = SampleWorldBuilder.CreateValidWorld();

            var supportA = new Character
            {
                Id = "char_support",
                Name = "Support",
                FactionId = world.Factions[0].Id,
                Traits = new List<TraitId>(),
                Skills = new Dictionary<SkillId, SkillLevel>(),
                Relationships = new List<RelationshipRecord>()
            };

            var supportB = new Character
            {
                Id = "char_support",
                Name = "Support",
                FactionId = worldCopy.Factions[0].Id,
                Traits = new List<TraitId>(),
                Skills = new Dictionary<SkillId, SkillLevel>(),
                Relationships = new List<RelationshipRecord>()
            };

            world.Characters.Add(supportA);
            worldCopy.Characters.Insert(0, supportB);

            world.Events[0].Details = new Dictionary<string, string>
            {
                { "beta", "2" },
                { "alpha", "1" }
            };

            worldCopy.Events[0].Details = new Dictionary<string, string>
            {
                { "alpha", "1" },
                { "beta", "2" }
            };

            world.OracleState.AvailableDecks[0].Cards.Add(new EventCard
            {
                Id = "card_extra",
                Narrative = "Extra",
                Effects = new()
                {
                    new EventEffect { EffectType = "boost", Parameters = new Dictionary<string, string> { { "value", "5" } } }
                }
            });

            worldCopy.OracleState.AvailableDecks[0].Cards.Insert(0, new EventCard
            {
                Id = "card_extra",
                Narrative = "Extra",
                Effects = new()
                {
                    new EventEffect { EffectType = "boost", Parameters = new Dictionary<string, string> { { "value", "5" } } }
                }
            });

            var serializer = new WorldDataSerializer();
            var jsonA = serializer.Serialize(world);
            var jsonB = serializer.Serialize(worldCopy);

            Assert.AreEqual(jsonB, jsonA);
        }

        [Test]
        public void Deserialize_NormalizesCollections()
        {
            var serializer = new WorldDataSerializer();
            var world = SampleWorldBuilder.CreateValidWorld();
            world.Events[0].Actors.Insert(0, "zzz");
            var json = serializer.Serialize(world);

            var result = serializer.Deserialize(json);

            Assert.That(result.Events[0].Actors[0], Is.EqualTo("char_leader"));
            Assert.That(result.Characters[0].Id, Is.EqualTo("char_leader"));
        }

        [Test]
        public void Serialize_RoundTripsCombinedSnapshotsWithBaseDiff()
        {
            var serializer = new WorldDataSerializer();
            var diffCalculator = new BaseStateDiffCalculator();

            var overworld = SampleWorldBuilder.CreateValidWorld();
            var baseSnapshot = SampleWorldBuilder.CreateValidWorld();

            baseSnapshot.BaseState.AlertLevel = AlertLevel.Elevated;
            baseSnapshot.BaseState.Infrastructure["defense"] = 0.95f;
            baseSnapshot.BaseState.Inventory.Add(new ItemStack { ItemId = "supply_rare", Quantity = 3 });
            baseSnapshot.BaseState.Zones[0].Efficiency = 1.1f;

            baseSnapshot.OracleState.TensionScore = 0.72f;
            baseSnapshot.OracleState.ActiveDeckId = "deck_minor_01";
            baseSnapshot.OracleState.Cooldowns["card_rise_nemesis"] = 4;

            baseSnapshot.Events.Add(new EventRecord
            {
                Id = "event_base_raid_0001",
                Timestamp = 99,
                EventType = EventType.Raid,
                Actors = new List<string>(),
                LocationId = baseSnapshot.BaseState.SiteTileId,
                Details = new Dictionary<string, string> { { "note", "synchronized" } }
            });

            var diff = diffCalculator.Compute(overworld.BaseState, baseSnapshot.BaseState);
            diffCalculator.Apply(overworld.BaseState, diff);

            overworld.OracleState = CloneOracle(baseSnapshot.OracleState);
            overworld.Events = baseSnapshot.Events.Select(CloneEvent).ToList();

            var mergedJson = serializer.Serialize(overworld);
            var baseJson = serializer.Serialize(baseSnapshot);

            Assert.AreEqual(baseJson, mergedJson);

            var roundTrip = serializer.Deserialize(mergedJson);
            Assert.AreEqual(mergedJson, serializer.Serialize(roundTrip));
        }

        private static OracleState CloneOracle(OracleState source)
        {
            return new OracleState
            {
                ActiveDeckId = source.ActiveDeckId,
                TensionScore = source.TensionScore,
                Cooldowns = source.Cooldowns.ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal),
                AvailableDecks = source.AvailableDecks.Select(CloneDeck).ToList()
            };
        }

        private static EventDeck CloneDeck(EventDeck deck)
        {
            return new EventDeck
            {
                Id = deck.Id,
                Tier = deck.Tier,
                Weight = deck.Weight,
                Cards = deck.Cards.Select(CloneCard).ToList()
            };
        }

        private static EventCard CloneCard(EventCard card)
        {
            return new EventCard
            {
                Id = card.Id,
                Narrative = card.Narrative,
                Effects = card.Effects
                    .Select(effect => new EventEffect
                    {
                        EffectType = effect.EffectType,
                        Parameters = effect.Parameters != null
                            ? effect.Parameters.ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal)
                            : new Dictionary<string, string>(StringComparer.Ordinal)
                    })
                    .ToList()
            };
        }

        private static EventRecord CloneEvent(EventRecord record)
        {
            return new EventRecord
            {
                Id = record.Id,
                Timestamp = record.Timestamp,
                EventType = record.EventType,
                Actors = new List<string>(record.Actors),
                LocationId = record.LocationId,
                Details = record.Details != null
                    ? record.Details.ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal)
                    : new Dictionary<string, string>(StringComparer.Ordinal)
            };
        }
    }
}
