using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Wastelands.Core.Data;
using Wastelands.Persistence;

namespace Wastelands.Tests.EditMode
{
    public class BaseStateDiffTests
    {
        [Test]
        public void Compute_NoChangesProducesEmptyDiff()
        {
            var world = SampleWorldBuilder.CreateValidWorld();
            var calculator = new BaseStateDiffCalculator();
            var previous = Clone(world.BaseState);
            var next = Clone(world.BaseState);

            var diff = calculator.Compute(previous, next);

            Assert.IsNull(diff.Active);
            Assert.IsNull(diff.SiteTileId);
            Assert.IsNull(diff.AlertLevel);
            Assert.IsEmpty(diff.UpsertedZones);
            Assert.IsEmpty(diff.RemovedZoneIds);
            Assert.IsEmpty(diff.AddedPopulation);
            Assert.IsEmpty(diff.RemovedPopulation);
            Assert.IsEmpty(diff.UpsertedInfrastructure);
            Assert.IsEmpty(diff.RemovedInfrastructureKeys);
            Assert.IsEmpty(diff.UpsertedInventory);
            Assert.IsEmpty(diff.RemovedInventoryItemIds);
            Assert.IsNull(diff.Research);
        }

        [Test]
        public void ComputeAndApplyDiff_RoundTripsChanges()
        {
            var world = SampleWorldBuilder.CreateValidWorld();
            var calculator = new BaseStateDiffCalculator();
            var previous = Clone(world.BaseState);
            var next = Clone(world.BaseState);

            next.Active = !previous.Active;
            next.SiteTileId = "tile_custom";
            next.AlertLevel = AlertLevel.Elevated;

            next.Zones[0].Efficiency = BaseClamp(next.Zones[0].Efficiency + 0.1f);
            next.Zones.Add(new BaseZone
            {
                Id = "zone_new",
                Name = "New Zone",
                Type = ZoneType.Workshop,
                Efficiency = 0.95f
            });

            next.Population.Add("char_new");
            if (next.Population.Count > 0)
            {
                next.Population.RemoveAt(0);
            }

            next.Infrastructure["power"] = 1.2f;
            next.Infrastructure["morale"] = 0.9f;
            next.Infrastructure.Remove("water");
            next.Infrastructure["defense"] = 0.6f;

            if (next.Inventory.Count > 0)
            {
                next.Inventory[0].Quantity += 7;
            }

            next.Inventory.Add(new ItemStack { ItemId = "supply_new", Quantity = 5 });

            next.Research.ActiveProjectId = "tech_new";
            next.Research.ActiveProgress = 0.6f;
            next.Research.CompletedProjects.Add("tech_old");

            var diff = calculator.Compute(previous, next);
            var target = Clone(previous);
            calculator.Apply(target, diff);

            AssertBaseStateEqual(next, target);
        }

        private static BaseState Clone(BaseState source)
        {
            return new BaseState
            {
                Active = source.Active,
                SiteTileId = source.SiteTileId,
                Zones = source.Zones.Select(zone => new BaseZone
                {
                    Id = zone.Id,
                    Name = zone.Name,
                    Type = zone.Type,
                    Efficiency = zone.Efficiency
                }).ToList(),
                Population = new List<string>(source.Population),
                Infrastructure = source.Infrastructure.ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal),
                AlertLevel = source.AlertLevel,
                Inventory = source.Inventory.Select(item => new ItemStack { ItemId = item.ItemId, Quantity = item.Quantity }).ToList(),
                Research = new ResearchState
                {
                    ActiveProjectId = source.Research.ActiveProjectId,
                    ActiveProgress = source.Research.ActiveProgress,
                    CompletedProjects = new List<string>(source.Research.CompletedProjects)
                }
            };
        }

        private static void AssertBaseStateEqual(BaseState expected, BaseState actual)
        {
            Assert.AreEqual(expected.Active, actual.Active);
            Assert.AreEqual(expected.SiteTileId, actual.SiteTileId);
            Assert.AreEqual(expected.AlertLevel, actual.AlertLevel);

            CollectionAssert.AreEquivalent(expected.Zones.Select(z => z.Id), actual.Zones.Select(z => z.Id));
            foreach (var zone in expected.Zones)
            {
                var match = actual.Zones.Single(z => z.Id == zone.Id);
                Assert.AreEqual(zone.Name, match.Name);
                Assert.AreEqual(zone.Type, match.Type);
                Assert.That(match.Efficiency, Is.EqualTo(zone.Efficiency).Within(0.0001f));
            }

            CollectionAssert.AreEquivalent(expected.Population, actual.Population);

            CollectionAssert.AreEquivalent(expected.Infrastructure.Keys, actual.Infrastructure.Keys);
            foreach (var pair in expected.Infrastructure)
            {
                Assert.That(actual.Infrastructure[pair.Key], Is.EqualTo(pair.Value).Within(0.0001f));
            }

            CollectionAssert.AreEquivalent(expected.Inventory.Select(item => item.ItemId), actual.Inventory.Select(item => item.ItemId));
            foreach (var item in expected.Inventory)
            {
                var match = actual.Inventory.Single(i => i.ItemId == item.ItemId);
                Assert.AreEqual(item.Quantity, match.Quantity);
            }

            Assert.AreEqual(expected.Research.ActiveProjectId, actual.Research.ActiveProjectId);
            Assert.That(actual.Research.ActiveProgress, Is.EqualTo(expected.Research.ActiveProgress).Within(0.0001f));
            CollectionAssert.AreEqual(expected.Research.CompletedProjects, actual.Research.CompletedProjects);
        }

        private static float BaseClamp(float value)
        {
            if (value < 0f)
            {
                return 0f;
            }

            if (value > 1.5f)
            {
                return 1.5f;
            }

            return value;
        }
    }
}
