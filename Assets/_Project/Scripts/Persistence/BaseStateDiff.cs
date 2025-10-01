using System;
using System.Collections.Generic;
using System.Linq;
using Wastelands.Core.Data;

namespace Wastelands.Persistence
{
    /// <summary>
    /// Represents a minimal patch describing changes applied to a <see cref="BaseState"/>.
    /// </summary>
    public sealed class BaseStateDiff
    {
        public bool? Active { get; set; }
        public string? SiteTileId { get; set; }
        public AlertLevel? AlertLevel { get; set; }
        public List<BaseZone> UpsertedZones { get; set; } = new();
        public List<string> RemovedZoneIds { get; set; } = new();
        public List<string> AddedPopulation { get; set; } = new();
        public List<string> RemovedPopulation { get; set; } = new();
        public Dictionary<string, float> UpsertedInfrastructure { get; set; } = new();
        public List<string> RemovedInfrastructureKeys { get; set; } = new();
        public List<ItemStack> UpsertedInventory { get; set; } = new();
        public List<string> RemovedInventoryItemIds { get; set; } = new();
        public ResearchState? Research { get; set; }
    }

    public interface IBaseStateDiffCalculator
    {
        BaseStateDiff Compute(BaseState previous, BaseState next);
        void Apply(BaseState target, BaseStateDiff diff);
    }

    /// <summary>
    /// Computes and applies diffs between <see cref="BaseState"/> snapshots for incremental persistence.
    /// </summary>
    public sealed class BaseStateDiffCalculator : IBaseStateDiffCalculator
    {
        public BaseStateDiff Compute(BaseState previous, BaseState next)
        {
            if (previous == null)
            {
                throw new ArgumentNullException(nameof(previous));
            }

            if (next == null)
            {
                throw new ArgumentNullException(nameof(next));
            }

            var diff = new BaseStateDiff();

            if (previous.Active != next.Active)
            {
                diff.Active = next.Active;
            }

            if (!string.Equals(previous.SiteTileId, next.SiteTileId, StringComparison.Ordinal))
            {
                diff.SiteTileId = next.SiteTileId;
            }

            if (previous.AlertLevel != next.AlertLevel)
            {
                diff.AlertLevel = next.AlertLevel;
            }

            PopulateZoneDiff(previous, next, diff);
            PopulatePopulationDiff(previous, next, diff);
            PopulateInfrastructureDiff(previous, next, diff);
            PopulateInventoryDiff(previous, next, diff);

            if (ResearchChanged(previous.Research, next.Research))
            {
                diff.Research = CloneResearch(next.Research);
            }

            return diff;
        }

        public void Apply(BaseState target, BaseStateDiff diff)
        {
            if (target == null)
            {
                throw new ArgumentNullException(nameof(target));
            }

            if (diff == null)
            {
                throw new ArgumentNullException(nameof(diff));
            }

            if (diff.Active.HasValue)
            {
                target.Active = diff.Active.Value;
            }

            if (diff.SiteTileId != null)
            {
                target.SiteTileId = diff.SiteTileId;
            }

            if (diff.AlertLevel.HasValue)
            {
                target.AlertLevel = diff.AlertLevel.Value;
            }

            ApplyZoneDiff(target, diff);
            ApplyPopulationDiff(target, diff);
            ApplyInfrastructureDiff(target, diff);
            ApplyInventoryDiff(target, diff);

            if (diff.Research != null)
            {
                target.Research = CloneResearch(diff.Research);
            }
        }

        private static void PopulateZoneDiff(BaseState previous, BaseState next, BaseStateDiff diff)
        {
            var previousLookup = previous.Zones.ToDictionary(z => z.Id, StringComparer.Ordinal);
            var nextLookup = next.Zones.ToDictionary(z => z.Id, StringComparer.Ordinal);

            foreach (var pair in nextLookup)
            {
                if (!previousLookup.TryGetValue(pair.Key, out var prior) || !ZonesEqual(prior, pair.Value))
                {
                    diff.UpsertedZones.Add(CloneZone(pair.Value));
                }
            }

            foreach (var pair in previousLookup)
            {
                if (!nextLookup.ContainsKey(pair.Key))
                {
                    diff.RemovedZoneIds.Add(pair.Key);
                }
            }
        }

        private static void PopulatePopulationDiff(BaseState previous, BaseState next, BaseStateDiff diff)
        {
            var previousSet = new HashSet<string>(previous.Population, StringComparer.Ordinal);
            var nextSet = new HashSet<string>(next.Population, StringComparer.Ordinal);

            diff.AddedPopulation.AddRange(nextSet.Except(previousSet));
            diff.RemovedPopulation.AddRange(previousSet.Except(nextSet));
        }

        private static void PopulateInfrastructureDiff(BaseState previous, BaseState next, BaseStateDiff diff)
        {
            foreach (var pair in next.Infrastructure)
            {
                if (!previous.Infrastructure.TryGetValue(pair.Key, out var priorValue) || !FloatEquals(priorValue, pair.Value))
                {
                    diff.UpsertedInfrastructure[pair.Key] = pair.Value;
                }
            }

            foreach (var key in previous.Infrastructure.Keys)
            {
                if (!next.Infrastructure.ContainsKey(key))
                {
                    diff.RemovedInfrastructureKeys.Add(key);
                }
            }
        }

        private static void PopulateInventoryDiff(BaseState previous, BaseState next, BaseStateDiff diff)
        {
            var previousLookup = previous.Inventory.ToDictionary(item => item.ItemId, StringComparer.Ordinal);
            var nextLookup = next.Inventory.ToDictionary(item => item.ItemId, StringComparer.Ordinal);

            foreach (var pair in nextLookup)
            {
                if (!previousLookup.TryGetValue(pair.Key, out var prior) || prior.Quantity != pair.Value.Quantity)
                {
                    diff.UpsertedInventory.Add(new ItemStack { ItemId = pair.Key, Quantity = pair.Value.Quantity });
                }
            }

            foreach (var pair in previousLookup)
            {
                if (!nextLookup.ContainsKey(pair.Key))
                {
                    diff.RemovedInventoryItemIds.Add(pair.Key);
                }
            }
        }

        private static void ApplyZoneDiff(BaseState target, BaseStateDiff diff)
        {
            foreach (var zone in diff.UpsertedZones)
            {
                var existing = target.Zones.FirstOrDefault(z => string.Equals(z.Id, zone.Id, StringComparison.Ordinal));
                if (existing == null)
                {
                    target.Zones.Add(CloneZone(zone));
                    continue;
                }

                existing.Name = zone.Name;
                existing.Type = zone.Type;
                existing.Efficiency = zone.Efficiency;
            }

            if (diff.RemovedZoneIds.Count == 0)
            {
                return;
            }

            target.Zones = target.Zones
                .Where(z => !diff.RemovedZoneIds.Contains(z.Id, StringComparer.Ordinal))
                .ToList();
        }

        private static void ApplyPopulationDiff(BaseState target, BaseStateDiff diff)
        {
            if (diff.AddedPopulation.Count > 0)
            {
                foreach (var member in diff.AddedPopulation)
                {
                    if (!target.Population.Contains(member, StringComparer.Ordinal))
                    {
                        target.Population.Add(member);
                    }
                }
            }

            if (diff.RemovedPopulation.Count > 0)
            {
                target.Population = target.Population
                    .Where(member => !diff.RemovedPopulation.Contains(member, StringComparer.Ordinal))
                    .ToList();
            }
        }

        private static void ApplyInfrastructureDiff(BaseState target, BaseStateDiff diff)
        {
            foreach (var pair in diff.UpsertedInfrastructure)
            {
                target.Infrastructure[pair.Key] = pair.Value;
            }

            foreach (var key in diff.RemovedInfrastructureKeys)
            {
                target.Infrastructure.Remove(key);
            }
        }

        private static void ApplyInventoryDiff(BaseState target, BaseStateDiff diff)
        {
            foreach (var stack in diff.UpsertedInventory)
            {
                var existing = target.Inventory.FirstOrDefault(item => string.Equals(item.ItemId, stack.ItemId, StringComparison.Ordinal));
                if (existing == null)
                {
                    target.Inventory.Add(new ItemStack { ItemId = stack.ItemId, Quantity = stack.Quantity });
                }
                else
                {
                    existing.Quantity = stack.Quantity;
                }
            }

            if (diff.RemovedInventoryItemIds.Count == 0)
            {
                return;
            }

            target.Inventory = target.Inventory
                .Where(item => !diff.RemovedInventoryItemIds.Contains(item.ItemId, StringComparer.Ordinal))
                .ToList();
        }

        private static bool ZonesEqual(BaseZone left, BaseZone right)
        {
            return string.Equals(left.Id, right.Id, StringComparison.Ordinal)
                   && string.Equals(left.Name, right.Name, StringComparison.Ordinal)
                   && left.Type == right.Type
                   && FloatEquals(left.Efficiency, right.Efficiency);
        }

        private static bool ResearchChanged(ResearchState previous, ResearchState next)
        {
            if (!string.Equals(previous.ActiveProjectId, next.ActiveProjectId, StringComparison.Ordinal))
            {
                return true;
            }

            if (!FloatEquals(previous.ActiveProgress, next.ActiveProgress))
            {
                return true;
            }

            if (previous.CompletedProjects.Count != next.CompletedProjects.Count)
            {
                return true;
            }

            for (var i = 0; i < previous.CompletedProjects.Count; i++)
            {
                if (!string.Equals(previous.CompletedProjects[i], next.CompletedProjects[i], StringComparison.Ordinal))
                {
                    return true;
                }
            }

            return false;
        }

        private static BaseZone CloneZone(BaseZone zone)
        {
            return new BaseZone
            {
                Id = zone.Id,
                Name = zone.Name,
                Type = zone.Type,
                Efficiency = zone.Efficiency
            };
        }

        private static ResearchState CloneResearch(ResearchState research)
        {
            return new ResearchState
            {
                CompletedProjects = research.CompletedProjects.Select(id => id).ToList(),
                ActiveProjectId = research.ActiveProjectId,
                ActiveProgress = research.ActiveProgress
            };
        }

        private static bool FloatEquals(float a, float b)
        {
            return Math.Abs(a - b) < 0.0001f;
        }
    }
}
