using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Wastelands.Core.Data;
using Wastelands.Core.Services;

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

            OracleSynchronizer.RecordRaidOutcome(context, raidState.AttackingFactionId, eventId);
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
                OracleSynchronizer.RecordMandateOutcome(context, resolution);

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

    public sealed class OracleIncidentResolutionSystem : IBaseModeSystem
    {
        private const float DeckRecoveryRate = 0.05f;
        private const float MinDeckWeight = 0.2f;
        private const float MaxDeckWeight = 3f;

        private readonly Queue<OracleIncidentInjected> _pendingIncidents = new();
        private readonly IDisposable _subscription;
        private readonly Dictionary<string, float> _defaultDeckWeights = new(StringComparer.Ordinal);
        private int _incidentSequence;

        public OracleIncidentResolutionSystem(IEventBus eventBus)
        {
            if (eventBus == null)
            {
                throw new ArgumentNullException(nameof(eventBus));
            }

            _subscription = eventBus.Subscribe<OracleIncidentInjected>(OnIncidentInjected);
        }

        ~OracleIncidentResolutionSystem()
        {
            _subscription.Dispose();
        }

        public string Name => "oracle.incidents";

        private void OnIncidentInjected(OracleIncidentInjected incident)
        {
            _pendingIncidents.Enqueue(incident);
        }

        public void Execute(in BaseModeTickContext context)
        {
            var oracle = context.World.OracleState;
            if (oracle == null)
            {
                return;
            }

            CacheDefaultWeights(oracle);
            StepCooldowns(oracle);

            if (_pendingIncidents.Count > 0)
            {
                while (_pendingIncidents.Count > 0)
                {
                    var incident = _pendingIncidents.Dequeue();
                    ApplyIncident(incident, context);
                }
            }

            RecoverDeckWeights(oracle);
        }

        private void CacheDefaultWeights(OracleState oracle)
        {
            if (oracle.AvailableDecks == null)
            {
                return;
            }

            foreach (var deck in oracle.AvailableDecks)
            {
                if (deck == null || string.IsNullOrEmpty(deck.Id) || _defaultDeckWeights.ContainsKey(deck.Id))
                {
                    continue;
                }

                var baseline = deck.Weight <= 0f ? 1f : deck.Weight;
                _defaultDeckWeights[deck.Id] = BaseMath.Clamp(baseline, MinDeckWeight, MaxDeckWeight);
            }
        }

        private static void StepCooldowns(OracleState oracle)
        {
            if (oracle.Cooldowns == null || oracle.Cooldowns.Count == 0)
            {
                return;
            }

            var keys = oracle.Cooldowns.Keys.ToList();
            foreach (var key in keys)
            {
                var value = oracle.Cooldowns[key];
                if (value <= 0)
                {
                    continue;
                }

                oracle.Cooldowns[key] = Math.Max(0, value - 1);
            }
        }

        private void RecoverDeckWeights(OracleState oracle)
        {
            if (oracle.AvailableDecks == null)
            {
                return;
            }

            foreach (var deck in oracle.AvailableDecks)
            {
                if (deck == null || string.IsNullOrEmpty(deck.Id))
                {
                    continue;
                }

                if (!_defaultDeckWeights.TryGetValue(deck.Id, out var baseline))
                {
                    baseline = deck.Weight <= 0f ? 1f : deck.Weight;
                    _defaultDeckWeights[deck.Id] = BaseMath.Clamp(baseline, MinDeckWeight, MaxDeckWeight);
                }

                if (Math.Abs(deck.Weight - baseline) < 0.001f)
                {
                    continue;
                }

                if (deck.Weight < baseline)
                {
                    var delta = Math.Min(DeckRecoveryRate, baseline - deck.Weight);
                    deck.Weight = BaseMath.Clamp(deck.Weight + delta, MinDeckWeight, MaxDeckWeight);
                }
                else
                {
                    var delta = Math.Min(DeckRecoveryRate, deck.Weight - baseline);
                    deck.Weight = BaseMath.Clamp(deck.Weight - delta, MinDeckWeight, MaxDeckWeight);
                }
            }
        }

        private void ApplyIncident(OracleIncidentInjected incident, in BaseModeTickContext context)
        {
            var details = new Dictionary<string, string>(StringComparer.Ordinal)
            {
                { "deckId", incident.DeckId },
                { "cardId", incident.CardId },
                { "trigger", incident.Trigger }
            };

            foreach (var parameter in incident.TriggerParameters)
            {
                details[$"trigger.{parameter.Key}"] = parameter.Value;
            }

            if (!string.IsNullOrEmpty(incident.Narrative))
            {
                details["narrative"] = incident.Narrative;
            }

            foreach (var effect in incident.Effects)
            {
                ApplyEffect(effect, context, details);
            }

            var record = new EventRecord
            {
                Id = $"oracle_{incident.CardId}_{context.Tick:D6}_{_incidentSequence++:D2}",
                Timestamp = context.Tick,
                EventType = ResolveEventType(details),
                Actors = new List<string>(),
                LocationId = context.BaseState.SiteTileId,
                Details = details
            };

            context.World.Events.Add(record);
        }

        private static EventType ResolveEventType(IReadOnlyDictionary<string, string> details)
        {
            if (details.TryGetValue("eventType", out var value) && Enum.TryParse(value, true, out EventType parsed))
            {
                return parsed;
            }

            return EventType.Catastrophe;
        }

        private static void ApplyEffect(EventEffect effect, in BaseModeTickContext context, Dictionary<string, string> details)
        {
            if (effect == null)
            {
                return;
            }

            var parameters = effect.Parameters ?? new Dictionary<string, string>(StringComparer.Ordinal);
            switch (effect.EffectType)
            {
                case "adjust_infrastructure":
                    ApplyInfrastructureAdjustment(parameters, context, details);
                    break;
                case "adjust_tension":
                    ApplyTensionAdjustment(parameters, context, details);
                    break;
                case "add_inventory":
                    ApplyInventoryAdjustment(parameters, context, details);
                    break;
                case "adjust_zone_morale":
                    ApplyZoneMoraleAdjustment(parameters, context, details);
                    break;
                case "schedule_job":
                    ScheduleJob(parameters, context, details);
                    break;
                case "set_alert_level":
                    ApplyAlertLevel(parameters, context, details);
                    break;
                case "spawn_event":
                    ApplySpawnMetadata(parameters, details);
                    break;
            }
        }

        private static void ApplyInfrastructureAdjustment(IReadOnlyDictionary<string, string> parameters, in BaseModeTickContext context, Dictionary<string, string> details)
        {
            if (!TryGetString(parameters, "stat", out var stat) || !TryGetFloat(parameters, "delta", out var delta) || Math.Abs(delta) < float.Epsilon)
            {
                return;
            }

            AdjustInfrastructure(context.BaseState.Infrastructure, stat, delta);
            if (context.BaseState.Infrastructure.TryGetValue(stat, out var value))
            {
                details[$"infrastructure.{stat}"] = value.ToString("0.###", CultureInfo.InvariantCulture);
            }
        }

        private static void ApplyTensionAdjustment(IReadOnlyDictionary<string, string> parameters, in BaseModeTickContext context, Dictionary<string, string> details)
        {
            if (!TryGetFloat(parameters, "delta", out var delta) || Math.Abs(delta) < float.Epsilon)
            {
                return;
            }

            if (context.World.OracleState == null)
            {
                return;
            }

            context.World.OracleState.TensionScore = BaseMath.Clamp01(context.World.OracleState.TensionScore + delta);
            details["tension"] = context.World.OracleState.TensionScore.ToString("0.###", CultureInfo.InvariantCulture);
        }

        private static void ApplyInventoryAdjustment(IReadOnlyDictionary<string, string> parameters, in BaseModeTickContext context, Dictionary<string, string> details)
        {
            if (!TryGetString(parameters, "itemId", out var itemId) || !TryGetInt(parameters, "quantity", out var quantity) || quantity == 0)
            {
                return;
            }

            var stack = context.BaseState.Inventory.FirstOrDefault(s => string.Equals(s.ItemId, itemId, StringComparison.Ordinal));
            if (stack == null)
            {
                if (quantity > 0)
                {
                    context.BaseState.Inventory.Add(new ItemStack { ItemId = itemId, Quantity = quantity });
                }
            }
            else
            {
                stack.Quantity = Math.Max(0, stack.Quantity + quantity);
            }

            var updated = context.BaseState.Inventory.FirstOrDefault(s => string.Equals(s.ItemId, itemId, StringComparison.Ordinal));
            if (updated != null)
            {
                details[$"inventory.{itemId}"] = updated.Quantity.ToString(CultureInfo.InvariantCulture);
            }
        }

        private static void ApplyZoneMoraleAdjustment(IReadOnlyDictionary<string, string> parameters, in BaseModeTickContext context, Dictionary<string, string> details)
        {
            if (!TryGetString(parameters, "zoneId", out var zoneId) || !TryGetFloat(parameters, "delta", out var delta))
            {
                return;
            }

            if (!context.Runtime.TryGetZoneRuntime(zoneId, out var zoneRuntime))
            {
                return;
            }

            zoneRuntime.MoraleModifier = BaseMath.Clamp(zoneRuntime.MoraleModifier + delta, 0.1f, 2f);
            details[$"zone.{zoneId}.morale"] = zoneRuntime.MoraleModifier.ToString("0.###", CultureInfo.InvariantCulture);
        }

        private static void ScheduleJob(IReadOnlyDictionary<string, string> parameters, in BaseModeTickContext context, Dictionary<string, string> details)
        {
            var jobId = TryGetString(parameters, "jobId", out var idValue) && !string.IsNullOrWhiteSpace(idValue)
                ? idValue
                : $"oracle_job_{context.Tick:D6}";

            var jobType = TryGetString(parameters, "jobType", out var typeValue) && Enum.TryParse(typeValue, true, out BaseJobType parsedType)
                ? parsedType
                : BaseJobType.Maintenance;

            var priority = TryGetString(parameters, "priority", out var priorityValue) && Enum.TryParse(priorityValue, true, out BaseJobPriority parsedPriority)
                ? parsedPriority
                : BaseJobPriority.High;

            var duration = TryGetInt(parameters, "duration", out var parsedDuration) && parsedDuration > 0
                ? parsedDuration
                : 6;

            parameters.TryGetValue("zoneId", out var zoneId);
            var repeatable = TryGetString(parameters, "repeatable", out var repeatValue) && string.Equals(repeatValue, "true", StringComparison.OrdinalIgnoreCase);

            var job = new BaseJob
            {
                Id = jobId,
                Type = jobType,
                Priority = priority,
                ZoneId = string.IsNullOrWhiteSpace(zoneId) ? null : zoneId,
                DurationHours = duration,
                RemainingHours = duration,
                Repeatable = repeatable
            };

            context.Runtime.JobBoard.Enqueue(job);
            details[$"job.{jobId}"] = jobType.ToString();
        }

        private static void ApplyAlertLevel(IReadOnlyDictionary<string, string> parameters, in BaseModeTickContext context, Dictionary<string, string> details)
        {
            if (!TryGetString(parameters, "level", out var levelValue) || !Enum.TryParse(levelValue, true, out AlertLevel level))
            {
                return;
            }

            context.BaseState.AlertLevel = level;
            details["alertLevel"] = level.ToString();
        }

        private static void ApplySpawnMetadata(IReadOnlyDictionary<string, string> parameters, Dictionary<string, string> details)
        {
            if (parameters.TryGetValue("eventType", out var eventType))
            {
                details["eventType"] = eventType;
            }

            if (parameters.TryGetValue("summary", out var summary))
            {
                details["summary"] = summary;
            }

            if (parameters.TryGetValue("target", out var target))
            {
                details["target"] = target;
            }
        }

        private static void AdjustInfrastructure(IDictionary<string, float> infrastructure, string key, float delta)
        {
            if (!infrastructure.TryGetValue(key, out var value))
            {
                value = 0.5f;
            }

            infrastructure[key] = BaseMath.Clamp(value + delta, 0f, 1.5f);
        }

        private static bool TryGetString(IReadOnlyDictionary<string, string> parameters, string key, out string value)
        {
            if (parameters != null && parameters.TryGetValue(key, out var raw))
            {
                value = raw;
                return true;
            }

            value = string.Empty;
            return false;
        }

        private static bool TryGetFloat(IReadOnlyDictionary<string, string> parameters, string key, out float value)
        {
            value = 0f;
            if (parameters == null || !parameters.TryGetValue(key, out var raw))
            {
                return false;
            }

            return float.TryParse(raw, NumberStyles.Float, CultureInfo.InvariantCulture, out value);
        }

        private static bool TryGetInt(IReadOnlyDictionary<string, string> parameters, string key, out int value)
        {
            value = 0;
            if (parameters == null || !parameters.TryGetValue(key, out var raw))
            {
                return false;
            }

            return int.TryParse(raw, NumberStyles.Integer, CultureInfo.InvariantCulture, out value);
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
