using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Wastelands.BaseMode;
using Wastelands.Core.Data;
using Wastelands.Core.Management;
using Wastelands.Core.Services;

namespace Wastelands.Tests.EditMode
{
    public class BaseSceneBootstrapperSiteContextTests
    {
        [Test]
        public void Initialize_PopulatesSiteContextFromWorldMetadata()
        {
            var world = SampleWorldBuilder.CreateValidWorld();
            world.Tiles[0].BiomeId = "biome_fungal_forest";
            world.Tiles[0].HazardTags = new List<string> { "haz_sporefall", "haz_solar_scorch" };

            world.Tiles.Add(new Tile
            {
                Id = "tile_1_0",
                Position = new Int2(1, 0),
                BiomeId = "biome_glass_desert",
                HazardTags = new List<string>()
            });

            world.Factions.Add(new Faction
            {
                Id = "fac_beta",
                Name = "Beta",
                Archetype = FactionArchetype.Raiders,
                NobleRoster = new List<NobleRoleAssignment>(),
                Relations = new List<RelationRecord>()
            });

            world.Settlements.Add(new Settlement
            {
                Id = "set_02",
                FactionId = "fac_beta",
                TileId = "tile_1_0",
                Population = 80,
                Economy = new EconomyProfile { Production = 0.6f, Trade = 0.4f, Research = 0.2f }
            });

            var services = CreateServices();
            var bootstrapper = new BaseSceneBootstrapper(world, services);

            bootstrapper.Initialize();

            Assert.IsNotNull(bootstrapper.Runtime);
            var context = bootstrapper.Runtime!.SiteContext;
            Assert.AreEqual("biome_fungal_forest", context.BiomeId);
            CollectionAssert.AreEquivalent(new[] { "haz_sporefall", "haz_solar_scorch" }, context.Hazards);
            Assert.AreEqual(2, context.NearbyFactions.Count);
            Assert.AreEqual("fac_alpha", context.NearbyFactions[0].FactionId);
            Assert.That(context.NearbyFactions[0].Distance, Is.EqualTo(0f).Within(0.0001f));
            Assert.AreEqual("fac_beta", context.NearbyFactions[1].FactionId);
            Assert.Greater(context.NearbyFactions[1].Distance, 0f);
        }

        [Test]
        public void Initialize_AppliesHazardModifiersToBaseState()
        {
            var world = SampleWorldBuilder.CreateValidWorld();
            world.Tiles[0].HazardTags = new List<string> { "haz_solar_scorch", "haz_sporefall" };

            world.BaseState.Infrastructure["water"] = 1f;
            world.BaseState.Infrastructure["morale"] = 1f;
            world.BaseState.Infrastructure["defense"] = 1f;
            world.BaseState.Zones.Add(new BaseZone { Id = "zone_farm", Name = "Farm", Type = ZoneType.Farm, Efficiency = 1f });
            world.BaseState.Zones.Add(new BaseZone { Id = "zone_watch", Name = "Watch", Type = ZoneType.Watchtower, Efficiency = 1f });

            var services = CreateServices();
            var bootstrapper = new BaseSceneBootstrapper(world, services);

            bootstrapper.Initialize();

            var baseState = bootstrapper.Runtime!.BaseState;
            Assert.That(baseState.Infrastructure["water"], Is.LessThan(1f));
            Assert.That(baseState.Infrastructure["morale"], Is.LessThan(1f));

            var farm = baseState.Zones.Single(z => z.Id == "zone_farm");
            Assert.That(farm.Efficiency, Is.LessThan(1f));

            var habitat = baseState.Zones.Single(z => z.Id == "zone_hab");
            Assert.That(habitat.Efficiency, Is.LessThan(1f));
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
