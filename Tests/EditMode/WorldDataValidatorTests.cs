using NUnit.Framework;
using Wastelands.Core.Data;

namespace Wastelands.Tests.EditMode
{
    public class WorldDataValidatorTests
    {
        [Test]
        public void Validate_ReturnsSuccess_ForConsistentData()
        {
            var world = SampleWorldBuilder.CreateValidWorld();
            var validator = new WorldDataValidator();

            var result = validator.Validate(world);

            Assert.IsTrue(result.IsValid, string.Join(";", result.Errors));
        }

        [Test]
        public void Validate_Fails_WhenSettlementReferencesMissingFaction()
        {
            var world = SampleWorldBuilder.CreateValidWorld();
            world.Settlements[0].FactionId = "missing";
            var validator = new WorldDataValidator();

            var result = validator.Validate(world);

            Assert.IsFalse(result.IsValid);
            StringAssert.Contains("unknown faction", result.Errors[0]);
        }

        [Test]
        public void Validate_Fails_ForDuplicateCharacterIds()
        {
            var world = SampleWorldBuilder.CreateValidWorld();
            world.Characters.Add(world.Characters[0]);
            var validator = new WorldDataValidator();

            var result = validator.Validate(world);

            Assert.IsFalse(result.IsValid);
            StringAssert.Contains("Duplicate Character id", result.Errors[0]);
        }
    }
}
