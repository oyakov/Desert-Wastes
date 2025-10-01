using System.Collections.Generic;
using NUnit.Framework;
using Wastelands.Core.Services;

namespace Wastelands.Tests.EditMode
{
    public class EventBusTests
    {
        private struct DummyEvent
        {
            public int Value { get; init; }
        }

        [Test]
        public void PublishInvokesSubscribersInOrder()
        {
            var bus = new EventBus();
            var values = new List<int>();

            bus.Subscribe<DummyEvent>(e => values.Add(e.Value));
            bus.Subscribe<DummyEvent>(e => values.Add(e.Value + 10));

            bus.Publish(new DummyEvent { Value = 1 });

            CollectionAssert.AreEqual(new[] { 1, 11 }, values);
        }

        [Test]
        public void DisposeSubscriptionStopsReceivingEvents()
        {
            var bus = new EventBus();
            var values = new List<int>();
            var subscription = bus.Subscribe<DummyEvent>(e => values.Add(e.Value));

            bus.Publish(new DummyEvent { Value = 5 });
            subscription.Dispose();
            bus.Publish(new DummyEvent { Value = 10 });

            CollectionAssert.AreEqual(new[] { 5 }, values);
        }
    }
}
