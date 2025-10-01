using NUnit.Framework;
using Wastelands.Core.Data;
using Wastelands.Core.Services;
using Wastelands.Generation;
using Wastelands.Persistence;

namespace Wastelands.Tests.EditMode
{
    public class OverworldGenerationPipelineTests
    {
        [Test]
        public void Generate_ProducesDeterministicWorld()
        {
            var seed = 123456789UL;
            var config = new OverworldGenerationConfig(seed, width: 6, height: 4, ApocalypseType.RadiantStorm);
            var rngA = new DeterministicRngService(9001);
            var rngB = new DeterministicRngService(9001);
            var pipelineA = new OverworldGenerationPipeline(rngA);
            var pipelineB = new OverworldGenerationPipeline(rngB);

            var worldA = pipelineA.Generate(config);
            var worldB = pipelineB.Generate(config);

            Assert.That(worldA.Tiles, Has.Count.EqualTo(config.Width * config.Height));
            Assert.That(worldA.Factions, Has.Count.GreaterThan(0));
            Assert.That(worldA.Settlements, Has.Count.EqualTo(worldA.Factions.Count));

            var serializer = new WorldDataSerializer();
            var jsonA = serializer.Serialize(worldA);
            var jsonB = serializer.Serialize(worldB);

            Assert.AreEqual(jsonA, jsonB);

            var validator = new WorldDataValidator();
            var validation = validator.Validate(worldA);
            Assert.That(validation.IsValid, Is.True, string.Join(",", validation.Errors));
        }
    }
}
