using System.Collections.Generic;
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
    }
}
