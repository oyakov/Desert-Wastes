using System.Collections.Generic;
using System.Linq;
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
        public void OracleIncidents_ApplyEffectsAndBalanceDecks()
        {
            var world = SampleWorldBuilder.CreateValidWorld();
            world.OracleState.Cooldowns["card_rise_nemesis"] = 0;
            world.BaseState.Infrastructure["morale"] = 0.5f;
            world.BaseState.Infrastructure["power"] = 0.5f;
            world.BaseState.Inventory.Clear();

            var deck = world.OracleState.AvailableDecks[0];
            deck.Cards[0].Effects = new List<EventEffect>
            {
                new()
                {
                    EffectType = "adjust_infrastructure",
                    Parameters = new Dictionary<string, string> { { "stat", "morale" }, { "delta", "0.2" } }
                },
                new()
                {
                    EffectType = "add_inventory",
                    Parameters = new Dictionary<string, string> { { "itemId", "oracle_cache" }, { "quantity", "3" } }
                },
                new()
                {
                    EffectType = "adjust_zone_morale",
                    Parameters = new Dictionary<string, string> { { "zoneId", "zone_hab" }, { "delta", "0.15" } }
                },
                new()
                {
                    EffectType = "schedule_job",
                    Parameters = new Dictionary<string, string>
                    {
                        { "jobId", "oracle_inspection" },
                        { "jobType", BaseJobType.Maintenance.ToString() },
                        { "priority", BaseJobPriority.High.ToString() },
                        { "duration", "4" },
                        { "repeatable", "false" }
                    }
                },
                new()
                {
                    EffectType = "set_alert_level",
                    Parameters = new Dictionary<string, string> { { "level", AlertLevel.Elevated.ToString() } }
                },
                new()
                {
                    EffectType = "adjust_tension",
                    Parameters = new Dictionary<string, string> { { "delta", "-0.1" } }
                }
            };

            world.OracleState.AvailableDecks.Add(new EventDeck
            {
                Id = "deck_minor_alt",
                Tier = OracleDeckTier.Minor,
                Weight = 0.5f,
                Cards = new List<EventCard>
                {
                    new()
                    {
                        Id = "card_minor_placeholder",
                        Narrative = "Minor guidance",
                        Effects = new List<EventEffect>()
                    }
                }
            });

            var services = CreateServices();
            var bootstrapper = new BaseSceneBootstrapper(world, services);

            var incidents = new List<OracleIncidentInjected>();
            services.EventBus.Subscribe<OracleIncidentInjected>(evt => incidents.Add(evt));

            bootstrapper.Initialize();

            var steps = 0;
            while (incidents.Count == 0 && steps < 96)
            {
                services.TickManager.Advance(1);
                steps++;
            }

            Assert.That(incidents, Is.Not.Empty);
            Assert.Less(world.OracleState.Cooldowns["card_rise_nemesis"], 6);
            Assert.That(world.OracleState.TensionScore, Is.LessThan(0.5f));
            Assert.That(world.BaseState.Infrastructure["morale"], Is.GreaterThan(0.5f));
            Assert.That(world.BaseState.Inventory.Any(stack => stack.ItemId == "oracle_cache" && stack.Quantity >= 3));
            Assert.That(world.BaseState.AlertLevel, Is.EqualTo(AlertLevel.Elevated));
            Assert.That(world.Events.Any(evt => evt.Id.StartsWith("oracle_")));
            Assert.That(world.Events.Last().Details.ContainsKey("job.oracle_inspection"));

            var primaryDeck = world.OracleState.AvailableDecks.First(d => d.Id == "deck_minor_01");
            var secondaryDeck = world.OracleState.AvailableDecks.First(d => d.Id == "deck_minor_alt");
            Assert.That(primaryDeck.Weight, Is.LessThan(1f));
            Assert.That(secondaryDeck.Weight, Is.GreaterThan(0.5f));
            Assert.That(bootstrapper.Runtime!.JobBoard.Jobs.Any(job => job.Id == "oracle_inspection"));
        }

        [Test]
        public void Persistence_RoundTripsBaseStateAfterSimulationTicks()
        {
            var world = SampleWorldBuilder.CreateValidWorld();
            var services = CreateServices();
            var bootstrapper = new BaseSceneBootstrapper(world, services);

            bootstrapper.Initialize();
            services.TickManager.Advance(48);

            var gateway = new OverworldSnapshotGateway(new WorldDataSerializer());
            var snapshot = gateway.SaveToString(world);
            var restored = gateway.LoadFromString(snapshot);

            services.TickManager.Advance(24);

            var resumedServices = CreateServices();
            var resumedBootstrapper = new BaseSceneBootstrapper(restored, resumedServices);
            resumedBootstrapper.Initialize();
            resumedServices.TickManager.Advance(24);

            var serializer = new WorldDataSerializer();
            Assert.AreEqual(serializer.Serialize(world), serializer.Serialize(restored));
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
