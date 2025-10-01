using System.Collections.Generic;
using NUnit.Framework;
using Wastelands.Core.Data;
using Wastelands.Core.Management;
using Wastelands.Core.Services;
using Wastelands.Persistence;
using Wastelands.Simulation;
using Wastelands.Simulation.Phases;

namespace Wastelands.Tests.EditMode
{
    public class OverworldSimulationLoopTests
    {
        [Test]
        public void Tick_RunsPhasesDeterministically()
        {
            var worldA = SampleWorldBuilder.CreateValidWorld();
            var worldB = SampleWorldBuilder.CreateValidWorld();

            var phasesA = CreatePhases();
            var phasesB = CreatePhases();

            var tracker = new List<long>();
            var legendEvents = new List<string>();

            RunSimulation(worldA, phasesA, tracker, legendEvents, ticks: 3);
            Assert.That(tracker, Is.EqualTo(new[] { 1L, 2L, 3L }));
            Assert.That(legendEvents, Has.Count.EqualTo(3));

            RunSimulation(worldB, phasesB, ticks: 3);

            var serializer = new WorldDataSerializer();
            Assert.AreEqual(serializer.Serialize(worldA), serializer.Serialize(worldB));
        }

        private static IReadOnlyList<IOverworldSimulationPhase> CreatePhases()
        {
            return new IOverworldSimulationPhase[]
            {
                new HazardPropagationPhase(),
                new FactionDiplomacyPhase(),
                new SettlementLogisticsPhase(),
                new OracleReviewPhase(),
                new LegendCompilationPhase()
            };
        }

        private static void RunSimulation(WorldData world, IReadOnlyList<IOverworldSimulationPhase> phases, int ticks)
        {
            RunSimulation(world, phases, null, null, ticks);
        }

        private static void RunSimulation(WorldData world, IReadOnlyList<IOverworldSimulationPhase> phases, List<long>? tickTracker, List<string>? legendEvents, int ticks)
        {
            var timeProvider = new ManualTimeProvider();
            var rng = new DeterministicRngService(4242);
            var eventBus = new EventBus();
            var tickManager = new TickManager(timeProvider, rng, eventBus);
            var loop = new OverworldSimulationLoop(world, phases, rng);
            tickManager.RegisterSystem(loop);

            if (tickTracker != null)
            {
                eventBus.Subscribe<OverworldTickCompleted>(evt => tickTracker.Add(evt.Tick));
            }

            if (legendEvents != null)
            {
                eventBus.Subscribe<OverworldLegendUpdated>(evt => legendEvents.Add(evt.EventId));
            }

            tickManager.Advance(ticks);
        }
    }
}
