using System.Collections.Generic;
using NUnit.Framework;
using Wastelands.Core.Management;
using Wastelands.Core.Services;

namespace Wastelands.Tests.EditMode
{
    public class TickManagerTests
    {
        private sealed class RecordingSystem : ITickSystem
        {
            public readonly List<long> Ticks = new();

            public void Tick(in TickContext context)
            {
                Ticks.Add(context.Tick);
            }
        }

        [Test]
        public void AdvanceTicksInvokesSystemsSequentially()
        {
            var timeProvider = new ManualTimeProvider();
            var rng = new DeterministicRngService(1);
            var bus = new EventBus();
            var manager = new TickManager(timeProvider, rng, bus);
            var system = new RecordingSystem();
            manager.RegisterSystem(system);

            manager.Advance(3);

            CollectionAssert.AreEqual(new[] { 1L, 2L, 3L }, system.Ticks);
        }

        [Test]
        public void UnregisteredSystemStopsReceivingTicks()
        {
            var timeProvider = new ManualTimeProvider();
            var rng = new DeterministicRngService(2);
            var bus = new EventBus();
            var manager = new TickManager(timeProvider, rng, bus);
            var system = new RecordingSystem();
            manager.RegisterSystem(system);

            manager.Advance();
            manager.UnregisterSystem(system);
            manager.Advance();

            CollectionAssert.AreEqual(new[] { 1L }, system.Ticks);
        }
    }
}
