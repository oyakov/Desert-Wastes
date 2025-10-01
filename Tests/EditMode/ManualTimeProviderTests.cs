using NUnit.Framework;
using Wastelands.Core.Services;

namespace Wastelands.Tests.EditMode
{
    public class ManualTimeProviderTests
    {
        [Test]
        public void AdvanceTicksIncrementsCurrentTick()
        {
            var provider = new ManualTimeProvider();
            provider.AdvanceTicks(5);
            Assert.AreEqual(5, provider.CurrentTick);
        }

        [Test]
        public void ConvertYearsToDailyTicksUsesConfiguredRates()
        {
            var provider = new ManualTimeProvider(ticksPerYear: 4, ticksPerDay: 6);
            Assert.AreEqual(4 * 6 * 3, provider.ConvertYearsToDailyTicks(3));
        }

        [Test]
        public void ConvertDailyTicksToYearsFloorsValue()
        {
            var provider = new ManualTimeProvider(ticksPerYear: 2, ticksPerDay: 10);
            Assert.AreEqual(1, provider.ConvertDailyTicksToYears(25));
        }
    }
}
