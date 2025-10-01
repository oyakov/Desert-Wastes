using System.Collections.Generic;
using NUnit.Framework;
using Wastelands.BaseMode;
using Wastelands.Core.Data;
using Wastelands.Core.Management;
using Wastelands.Core.Services;
using Wastelands.Persistence;

namespace Wastelands.Tests.EditMode
{
    public class BaseModeSimulationLoopTests
    {
        [Test]
        public void Bootstrapper_InitializesRuntimeAndRegistersLoop()
        {
            var world = SampleWorldBuilder.CreateValidWorld();
            var services = CreateServices();
            var bootstrapper = new BaseSceneBootstrapper(world, services);

            BaseRuntimeState? runtimeFromEvent = null;
            services.EventBus.Subscribe<BaseSceneBootstrapped>(evt => runtimeFromEvent = evt.Runtime);

            bootstrapper.Initialize();

            Assert.IsNotNull(bootstrapper.Runtime);
            Assert.IsNotNull(bootstrapper.SimulationLoop);
            Assert.IsNotNull(runtimeFromEvent);
            Assert.IsTrue(world.BaseState.Active);
            Assert.That(bootstrapper.Runtime!.JobBoard.Jobs, Is.Not.Empty);
            Assert.That(bootstrapper.Runtime.MandateTracker.Mandates, Is.Not.Empty);

            Assert.DoesNotThrow(() => services.TickManager.Advance(1));
        }

        [Test]
        public void SimulationLoop_ProducesDeterministicResults()
        {
            var worldA = SampleWorldBuilder.CreateValidWorld();
            var worldB = SampleWorldBuilder.CreateValidWorld();

            var resultA = RunSimulation(worldA, ticks: 72);
            var resultB = RunSimulation(worldB, ticks: 72);

            Assert.That(resultA.JobCompletions, Is.Not.Empty);
            Assert.That(resultA.MandateResolutions, Is.Not.Empty);
            Assert.AreEqual(resultA.JobCompletions, resultB.JobCompletions);
            Assert.AreEqual(resultA.MandateResolutions, resultB.MandateResolutions);

            var serializer = new WorldDataSerializer();
            Assert.AreEqual(serializer.Serialize(worldA), serializer.Serialize(worldB));
        }

        [Test]
        public void SimulationLoop_OracleSynchronizationEmitsIncidents()
        {
            var world = SampleWorldBuilder.CreateValidWorld();
            world.OracleState.Cooldowns["card_rise_nemesis"] = 0;

            var services = CreateServices();
            var bootstrapper = new BaseSceneBootstrapper(world, services);

            var incidents = new List<OracleIncidentInjected>();
            services.EventBus.Subscribe<OracleIncidentInjected>(evt => incidents.Add(evt));

            bootstrapper.Initialize();
            services.TickManager.Advance(240);

            Assert.That(incidents, Is.Not.Empty);
            Assert.That(world.OracleState.Cooldowns["card_rise_nemesis"], Is.EqualTo(6));
            Assert.AreNotEqual(0.5f, world.OracleState.TensionScore);
        }

        private static SimulationResult RunSimulation(WorldData world, int ticks)
        {
            var services = CreateServices();
            var bootstrapper = new BaseSceneBootstrapper(world, services);

            var jobCompletions = new List<string>();
            var mandateResolutions = new List<string>();
            var raidEvents = new List<string>();

            services.EventBus.Subscribe<BaseJobCompleted>(evt => jobCompletions.Add(evt.Job.Id));
            services.EventBus.Subscribe<BaseMandateResolved>(evt => mandateResolutions.Add($"{evt.ResolutionResult}:{evt.Mandate.Id}:{evt.Tick}"));
            services.EventBus.Subscribe<BaseRaidResolved>(evt => raidEvents.Add(evt.EventId));

            bootstrapper.Initialize();
            services.TickManager.Advance(ticks);

            return new SimulationResult(jobCompletions, mandateResolutions, raidEvents);
        }

        private static DeterministicServiceContainer CreateServices()
        {
            var timeProvider = new ManualTimeProvider(ticksPerYear: 1, ticksPerDay: 24);
            var rng = new DeterministicRngService(1337);
            var eventBus = new EventBus();
            var tickManager = new TickManager(timeProvider, rng, eventBus);
            return new DeterministicServiceContainer(timeProvider, rng, eventBus, tickManager);
        }

        private readonly struct SimulationResult
        {
            public SimulationResult(List<string> jobCompletions, List<string> mandateResolutions, List<string> raidEvents)
            {
                JobCompletions = jobCompletions;
                MandateResolutions = mandateResolutions;
                RaidEvents = raidEvents;
            }

            public List<string> JobCompletions { get; }
            public List<string> MandateResolutions { get; }
            public List<string> RaidEvents { get; }
        }
    }
}
