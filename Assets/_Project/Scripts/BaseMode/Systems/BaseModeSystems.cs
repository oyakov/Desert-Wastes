using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Wastelands.Core.Data;

namespace Wastelands.BaseMode
{
    internal static class BaseMath
    {
        public static float Clamp01(float value)
        {
            if (value < 0f)
            {
                return 0f;
            }

            if (value > 1f)
            {
                return 1f;
            }

            return value;
        }

        public static float Clamp(float value, float min, float max)
        {
            if (value < min)
            {
                return min;
            }

            if (value > max)
            {
                return max;
            }

            return value;
        }
    }

    public sealed class ZoneMaintenanceSystem : IBaseModeSystem
    {
        public string Name => "zones";

        public void Execute(in BaseModeTickContext context)
        {
            foreach (var zoneRuntime in context.Runtime.EnumerateZones())
            {
                var channel = context.GetChannel($"{Name}.{zoneRuntime.Zone.Id}");
                var moraleDrift = (float)(channel.NextDouble() * 0.1d - 0.05d);
                var infrastructureFactor = CalculateInfrastructureFactor(context.BaseState.Infrastructure, zoneRuntime.Zone.Type);
                zoneRuntime.MoraleModifier = BaseMath.Clamp(zoneRuntime.MoraleModifier + moraleDrift + infrastructureFactor * 0.05f, 0.1f, 1.5f);

                var wearDelta = 0.015f - zoneRuntime.MoraleModifier * 0.01f - infrastructureFactor * 0.01f;
                zoneRuntime.Wear = BaseMath.Clamp(zoneRuntime.Wear + wearDelta, 0f, 1f);

                var efficiencyDelta = zoneRuntime.MoraleModifier * 0.03f - zoneRuntime.Wear * 0.025f + infrastructureFactor * 0.02f;
                zoneRuntime.Zone.Efficiency = BaseMath.Clamp(zoneRuntime.Zone.Efficiency + efficiencyDelta, 0.3f, 1.35f);

                if (zoneRuntime.Zone.Type == ZoneType.Watchtower)
                {
                    context.Runtime.RaidThreat.ThreatMeter = BaseMath.Clamp01(context.Runtime.RaidThreat.ThreatMeter - zoneRuntime.Zone.Efficiency * 0.01f);
                }
            }

            ApplyInfrastructureDecay(context.BaseState);
        }

        private static float CalculateInfrastructureFactor(IReadOnlyDictionary<string, float> infrastructure, ZoneType zoneType)
        {
            if (infrastructure.Count == 0)
            {
                return 0f;
            }

            var baseline = infrastructure.TryGetValue("power", out var power) ? power : 0.5f;
            var support = infrastructure.TryGetValue("water", out var water) ? water : 0.5f;
            var morale = infrastructure.TryGetValue("morale", out var moraleValue) ? moraleValue : 0.5f;
            var defense = infrastructure.TryGetValue("defense", out var defenseValue) ? defenseValue : 0.5f;

            return zoneType switch
            {
                ZoneType.Habitat => (baseline + support + morale) / 3f,
                ZoneType.Workshop => (baseline + morale) / 2f,
                ZoneType.Farm => (support + morale) / 2f,
                ZoneType.Watchtower => (baseline + defense) / 2f,
                ZoneType.ResearchLab => (baseline + morale) / 2f,
                _ => baseline
            };
        }

        private static void ApplyInfrastructureDecay(BaseState state)
        {
            var keys = new[] { "power", "water", "morale" };
            foreach (var key in keys)
            {
                if (!state.Infrastructure.TryGetValue(key, out var value))
                {
                    state.Infrastructure[key] = 0.5f;
                    continue;
                }

                state.Infrastructure[key] = BaseMath.Clamp(value - 0.01f, 0f, 1.5f);
            }

            if (!state.Infrastructure.ContainsKey("defense"))
            {
                state.Infrastructure["defense"] = 0.4f;
            }
        }
    }

    public sealed class JobSchedulingSystem : IBaseModeSystem
    {
        public string Name => "jobs";

        public void Execute(in BaseModeTickContext context)
        {
            var completed = new List<BaseJobResult>();
            context.Runtime.JobBoard.Advance(context, completed);
            context.Runtime.RecordCompletedJobs(completed);

            foreach (var result in completed)
            {
                ApplyJobOutcome(result, context);
                context.EventBus.Publish(new BaseJobCompleted(result, context.Tick));
            }
        }

        private static void ApplyJobOutcome(BaseJobResult result, in BaseModeTickContext context)
        {
            switch (result.Type)
            {
                case BaseJobType.Maintenance:
                    ApplyMaintenance(result, context);
                    break;
                case BaseJobType.Production:
                    ApplyProduction(result, context);
                    break;
                case BaseJobType.Research:
                    ApplyResearch(result, context);
                    break;
                case BaseJobType.Patrol:
                    ApplyPatrol(result, context);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(result.Type), result.Type, "Unsupported job type.");
            }
        }

        private static void ApplyMaintenance(BaseJobResult result, in BaseModeTickContext context)
        {
            if (result.ZoneId != null && context.Runtime.TryGetZoneRuntime(result.ZoneId, out var runtime))
            {
                runtime.Wear = BaseMath.Clamp(runtime.Wear - 0.2f, 0f, 1f);
                runtime.Zone.Efficiency = BaseMath.Clamp(runtime.Zone.Efficiency + 0.04f, 0.3f, 1.4f);
            }

            AdjustInfrastructure(context.BaseState.Infrastructure, "morale", 0.05f);
        }

        private static void ApplyProduction(BaseJobResult result, in BaseModeTickContext context)
        {
            var channel = context.GetChannel($"{nameof(BaseJobType.Production)}.{result.ZoneId ?? "global"}");
            var yield = 2 + channel.NextInt(0, 3);
            var itemId = result.ZoneId != null && context.BaseState.Zones.FirstOrDefault(z => z.Id == result.ZoneId)?.Type == ZoneType.Farm
                ? "supply_food"
                : "supply_basic";

            var stack = context.BaseState.Inventory.FirstOrDefault(s => string.Equals(s.ItemId, itemId, StringComparison.Ordinal));
            if (stack == null)
            {
                context.BaseState.Inventory.Add(new ItemStack { ItemId = itemId, Quantity = yield });
            }
            else
            {
                stack.Quantity += yield;
            }

            AdjustInfrastructure(context.BaseState.Infrastructure, "power", 0.02f);
        }

        private static void ApplyResearch(BaseJobResult result, in BaseModeTickContext context)
        {
            if (string.IsNullOrEmpty(context.BaseState.Research.ActiveProjectId))
            {
                context.BaseState.Research.ActiveProgress = 0f;
                return;
            }

            var channel = context.GetChannel("research.progress");
            var delta = 0.1f + (float)channel.NextDouble() * 0.05f;
            context.BaseState.Research.ActiveProgress = BaseMath.Clamp01(context.BaseState.Research.ActiveProgress + delta);

            if (context.BaseState.Research.ActiveProgress >= 0.999f)
            {
                context.BaseState.Research.CompletedProjects.Add(context.BaseState.Research.ActiveProjectId);
                context.BaseState.Research.ActiveProjectId = string.Empty;
                context.BaseState.Research.ActiveProgress = 0f;
            }
        }

        private static void ApplyPatrol(BaseJobResult result, in BaseModeTickContext context)
        {
            context.Runtime.RaidThreat.ThreatMeter = BaseMath.Clamp01(context.Runtime.RaidThreat.ThreatMeter - 0.12f);
            AdjustInfrastructure(context.BaseState.Infrastructure, "defense", 0.05f);
        }

        private static void AdjustInfrastructure(IDictionary<string, float> infrastructure, string key, float delta)
        {
            if (!infrastructure.TryGetValue(key, out var value))
            {
                value = 0.5f;
            }

            infrastructure[key] = BaseMath.Clamp(value + delta, 0f, 1.5f);
        }
    }

    public sealed class RaidThreatSystem : IBaseModeSystem
    {
        public string Name => "raids";

        public void Execute(in BaseModeTickContext context)
        {
            var raidState = context.Runtime.RaidThreat;
            var channel = context.GetChannel("raids.threat");
            var tensionModifier = (context.World.OracleState.TensionScore - 0.5f) * 0.08f;
            var infrastructureModifier = context.BaseState.Infrastructure.TryGetValue("defense", out var defense) ? (0.6f - defense) * 0.05f : 0.02f;
            var delta = (float)(channel.NextDouble() * 0.06d - 0.02d) + tensionModifier + infrastructureModifier;

            delta -= context.Runtime.RecentlyCompletedJobs.Count(job => job.Type == BaseJobType.Patrol) * 0.04f;

            raidState.ThreatMeter = BaseMath.Clamp01(raidState.ThreatMeter + delta);

            if (raidState.RaidScheduled)
            {
                raidState.HoursUntilRaid--;
                if (raidState.HoursUntilRaid <= 0)
                {
                    ResolveRaid(context);
                }

                return;
            }

            if (raidState.ThreatMeter > 0.85f)
            {
                raidState.RaidScheduled = true;
                raidState.HoursUntilRaid = Math.Max(4, context.HoursPerDay / 3);
                raidState.AttackingFactionId = context.World.Factions.FirstOrDefault()?.Id ?? string.Empty;
                context.BaseState.AlertLevel = AlertLevel.Elevated;
                context.EventBus.Publish(new BaseRaidScheduled(raidState.AttackingFactionId, raidState.HoursUntilRaid));
            }
            else if (context.BaseState.AlertLevel != AlertLevel.Calm && raidState.ThreatMeter < 0.25f)
            {
                context.BaseState.AlertLevel = AlertLevel.Calm;
            }
        }

        private static void ResolveRaid(in BaseModeTickContext context)
        {
            var raidState = context.Runtime.RaidThreat;
            raidState.RaidScheduled = false;
            raidState.ThreatMeter = 0.35f;
            context.BaseState.AlertLevel = AlertLevel.Critical;

            var eventId = $"base_raid_{context.Tick:D6}";
            context.World.Events.Add(new EventRecord
            {
                Id = eventId,
                Timestamp = context.Tick,
                EventType = EventType.Raid,
                Actors = new List<string>(),
                LocationId = context.BaseState.SiteTileId,
                Details = new Dictionary<string, string>
                {
                    { "attacker", raidState.AttackingFactionId },
                    { "alertLevel", context.BaseState.AlertLevel.ToString() },
                    { "threat", raidState.ThreatMeter.ToString("0.00", CultureInfo.InvariantCulture) }
                }
            });

            AdjustDefenseAfterRaid(context.BaseState.Infrastructure);
            context.EventBus.Publish(new BaseRaidResolved(eventId, raidState.AttackingFactionId));
        }

        private static void AdjustDefenseAfterRaid(IDictionary<string, float> infrastructure)
        {
            if (!infrastructure.TryGetValue("defense", out var defense))
            {
                defense = 0.4f;
            }

            infrastructure["defense"] = BaseMath.Clamp(defense - 0.1f, 0f, 1.2f);
        }
    }

    public sealed class MandateResolutionSystem : IBaseModeSystem
    {
        public string Name => "mandates";

        public void Execute(in BaseModeTickContext context)
        {
            var resolutions = context.Runtime.MandateTracker.Advance(context, context.Runtime.RecentlyCompletedJobs);
            if (resolutions.Count == 0)
            {
                return;
            }

            foreach (var resolution in resolutions)
            {
                ApplyResolutionEffects(resolution, context);
                LogResolutionEvent(resolution, context);
                context.EventBus.Publish(new BaseMandateResolved(resolution.Mandate, resolution.Result, context.Tick));

                if (resolution.Result == MandateStatus.Completed)
                {
                    context.Runtime.MandateTracker.EnqueueFollowUpMandate(resolution.Mandate, context.Tick);
                }
            }
        }

        private static void ApplyResolutionEffects(BaseMandateResolution resolution, in BaseModeTickContext context)
        {
            switch (resolution.Result)
            {
                case MandateStatus.Completed:
                    ApplyCompletion(resolution.Mandate, context);
                    break;
                case MandateStatus.Failed:
                    ApplyFailure(context.BaseState.Infrastructure);
                    break;
            }
        }

        private static void ApplyCompletion(BaseMandate mandate, in BaseModeTickContext context)
        {
            switch (mandate.Type)
            {
                case MandateType.Infrastructure:
                    AdjustInfrastructure(context.BaseState.Infrastructure, "water", 0.08f);
                    AdjustInfrastructure(context.BaseState.Infrastructure, "morale", 0.05f);
                    break;
                case MandateType.Production:
                    var stack = context.BaseState.Inventory.FirstOrDefault(item => item.ItemId == "supply_refined");
                    if (stack == null)
                    {
                        context.BaseState.Inventory.Add(new ItemStack { ItemId = "supply_refined", Quantity = 4 });
                    }
                    else
                    {
                        stack.Quantity += 4;
                    }

                    AdjustInfrastructure(context.BaseState.Infrastructure, "power", 0.05f);
                    break;
                case MandateType.Defense:
                    AdjustInfrastructure(context.BaseState.Infrastructure, "defense", 0.1f);
                    context.BaseState.AlertLevel = AlertLevel.Elevated;
                    break;
                case MandateType.Research:
                    if (!string.IsNullOrEmpty(context.BaseState.Research.ActiveProjectId))
                    {
                        context.BaseState.Research.CompletedProjects.Add(context.BaseState.Research.ActiveProjectId);
                        context.BaseState.Research.ActiveProjectId = string.Empty;
                        context.BaseState.Research.ActiveProgress = 0f;
                    }
                    break;
            }
        }

        private static void ApplyFailure(IDictionary<string, float> infrastructure)
        {
            AdjustInfrastructure(infrastructure, "morale", -0.08f);
            AdjustInfrastructure(infrastructure, "defense", -0.05f);
        }

        private static void AdjustInfrastructure(IDictionary<string, float> infrastructure, string key, float delta)
        {
            if (!infrastructure.TryGetValue(key, out var value))
            {
                value = 0.4f;
            }

            infrastructure[key] = BaseMath.Clamp(value + delta, 0f, 1.5f);
        }

        private static void LogResolutionEvent(BaseMandateResolution resolution, in BaseModeTickContext context)
        {
            var eventId = $"mandate_{resolution.Mandate.Id}_{context.Tick:D6}";
            context.World.Events.Add(new EventRecord
            {
                Id = eventId,
                Timestamp = context.Tick,
                EventType = EventType.Mandate,
                Actors = string.IsNullOrEmpty(resolution.Mandate.IssuerCharacterId)
                    ? new List<string>()
                    : new List<string> { resolution.Mandate.IssuerCharacterId },
                LocationId = context.BaseState.SiteTileId,
                Details = new Dictionary<string, string>
                {
                    { "result", resolution.Result.ToString() },
                    { "type", resolution.Mandate.Type.ToString() }
                }
            });
        }
    }

    public readonly struct BaseJobCompleted
    {
        public BaseJobCompleted(BaseJobResult job, long tick)
        {
            Job = job;
            Tick = tick;
        }

        public BaseJobResult Job { get; }
        public long Tick { get; }
    }

    public readonly struct BaseRaidScheduled
    {
        public BaseRaidScheduled(string attackerFactionId, int hoursUntilRaid)
        {
            AttackerFactionId = attackerFactionId;
            HoursUntilRaid = hoursUntilRaid;
        }

        public string AttackerFactionId { get; }
        public int HoursUntilRaid { get; }
    }

    public readonly struct BaseRaidResolved
    {
        public BaseRaidResolved(string eventId, string attackerFactionId)
        {
            EventId = eventId ?? throw new ArgumentNullException(nameof(eventId));
            AttackerFactionId = attackerFactionId;
        }

        public string EventId { get; }
        public string AttackerFactionId { get; }
    }

    public readonly struct BaseMandateResolved
    {
        public BaseMandateResolved(BaseMandate mandate, MandateStatus resolutionResult, long tick)
        {
            Mandate = mandate ?? throw new ArgumentNullException(nameof(mandate));
            ResolutionResult = resolutionResult;
            Tick = tick;
        }

        public BaseMandate Mandate { get; }
        public MandateStatus ResolutionResult { get; }
        public long Tick { get; }
    }
}
