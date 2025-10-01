using NUnit.Framework;
using Wastelands.Core.Services;

namespace Wastelands.Tests.EditMode
{
    public class DeterministicRngServiceTests
    {
        [Test]
        public void ChannelsWithSameNameProduceIdenticalSequences()
        {
            var rng = new DeterministicRngService(1234);

            var first = rng.GetChannel("worldgen");
            var second = rng.GetChannel("worldgen");

            for (var i = 0; i < 10; i++)
            {
                Assert.AreEqual(first.NextInt(0, 100), second.NextInt(0, 100));
            }
        }

        [Test]
        public void ResetClearsChannelState()
        {
            var rng = new DeterministicRngService(42);
            var channel = rng.GetChannel("factions");
            var initial = channel.NextInt(0, 1000);

            rng.Reset(42);
            channel = rng.GetChannel("factions");
            var afterReset = channel.NextInt(0, 1000);

            Assert.AreEqual(initial, afterReset);
        }

        [Test]
        public void ReseedBranchesDeterministically()
        {
            var rng = new DeterministicRngService(7);
            var channel = rng.GetChannel("combat");
            channel.NextInt(0, 100);

            channel.Reseed(10);
            var branchOne = channel.NextInt(0, 100);

            channel.Reseed(10);
            var branchTwo = channel.NextInt(0, 100);

            Assert.AreEqual(branchOne, branchTwo);
        }
    }
}
