using NUnit.Framework;
using Wastelands.BaseMode;
using Wastelands.Core.Management;
using Wastelands.Core.Services;

namespace Wastelands.Tests.EditMode
{
    public class BaseSceneBootstrapperCommandTests
    {
        [Test]
        public void Initialize_PublishesDispatcherEventAndBindsRuntime()
        {
            var world = SampleWorldBuilder.CreateValidWorld();
            var services = CreateServices();
            var bootstrapper = new BaseSceneBootstrapper(world, services);

            BaseRuntimeState? runtimeFromEvent = null;
            IBaseIndirectCommandDispatcher? dispatcherFromEvent = null;
            BaseIndirectCommand? publishedCommand = null;

            using var runtimeSubscription = services.EventBus.Subscribe<BaseSceneBootstrapped>(evt => runtimeFromEvent = evt.Runtime);
            using var dispatcherSubscription = services.EventBus.Subscribe<BaseIndirectCommandDispatcherReady>(evt => dispatcherFromEvent = evt.Dispatcher);
            using var commandSubscription = services.EventBus.Subscribe<BaseIndirectCommandQueued>(evt => publishedCommand = evt.Command);

            bootstrapper.Initialize();

            Assert.IsNotNull(runtimeFromEvent);
            Assert.IsNotNull(dispatcherFromEvent);
            Assert.AreSame(bootstrapper.CommandDispatcher, dispatcherFromEvent);
            Assert.AreSame(bootstrapper.Runtime!.CommandDispatcher, dispatcherFromEvent);

            dispatcherFromEvent!.Issue(new BaseIndirectCommand("test.command", targetId: "zone_hab"));
            Assert.IsNotNull(publishedCommand);
            Assert.AreEqual("test.command", publishedCommand?.CommandType);
            Assert.AreEqual("zone_hab", publishedCommand?.TargetId);
        }

        private static DeterministicServiceContainer CreateServices()
        {
            var timeProvider = new ManualTimeProvider(ticksPerYear: 1, ticksPerDay: 24);
            var rng = new DeterministicRngService(9876);
            var eventBus = new EventBus();
            var tickManager = new TickManager(timeProvider, rng, eventBus);
            return new DeterministicServiceContainer(timeProvider, rng, eventBus, tickManager);
        }
    }
}
