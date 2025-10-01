using System;
using System.Collections.Generic;
using System.Linq;
using Wastelands.Core.Data;

namespace Wastelands.BaseMode
{
    /// <summary>
    /// Holds runtime-only data for the Base Mode simulation loop.
    /// </summary>
    public sealed class BaseRuntimeState
    {
        private readonly Dictionary<string, BaseZoneRuntime> _zonesById;
        private readonly List<BaseJobResult> _recentlyCompletedJobs = new();

        public BaseRuntimeState(WorldData world, BaseState baseState, int hoursPerDay)
        {
            World = world ?? throw new ArgumentNullException(nameof(world));
            BaseState = baseState ?? throw new ArgumentNullException(nameof(baseState));
            if (hoursPerDay <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(hoursPerDay));
            }

            HoursPerDay = hoursPerDay;
            _zonesById = baseState.Zones
                .ToDictionary(zone => zone.Id, zone => new BaseZoneRuntime(zone), StringComparer.Ordinal);
            JobBoard = new BaseJobBoard();
            RaidThreat = new RaidThreatState();
            MandateTracker = new MandateTracker(hoursPerDay);
        }

        public WorldData World { get; }
        public BaseState BaseState { get; }
        public int HoursPerDay { get; }
        public BaseJobBoard JobBoard { get; }
        public RaidThreatState RaidThreat { get; }
        public MandateTracker MandateTracker { get; }
        public List<BaseJobResult> RecentlyCompletedJobs => _recentlyCompletedJobs;

        public IReadOnlyDictionary<string, BaseZoneRuntime> Zones => _zonesById;

        public IEnumerable<BaseZoneRuntime> EnumerateZones() => _zonesById.Values;

        public bool TryGetZoneRuntime(string zoneId, out BaseZoneRuntime runtime)
        {
            return _zonesById.TryGetValue(zoneId, out runtime!);
        }

        public void SeedInitialJobs()
        {
            JobBoard.SeedFromBaseState(BaseState, EnumerateZones());
        }

        public void SeedInitialMandates(WorldData world)
        {
            MandateTracker.Initialize(world, BaseState);
        }

        public void RecordCompletedJobs(IEnumerable<BaseJobResult> jobs)
        {
            _recentlyCompletedJobs.Clear();
            _recentlyCompletedJobs.AddRange(jobs);
        }
    }

    public sealed class BaseZoneRuntime
    {
        public BaseZoneRuntime(BaseZone zone)
        {
            Zone = zone ?? throw new ArgumentNullException(nameof(zone));
            MoraleModifier = 0.5f;
            Wear = 0.1f;
            WorkforceAllocation = 1f;
        }

        public BaseZone Zone { get; }
        public float MoraleModifier { get; set; }
        public float Wear { get; set; }
        public float WorkforceAllocation { get; set; }
    }

    public sealed class BaseJobBoard
    {
        private readonly List<BaseJob> _jobs = new();

        public IReadOnlyList<BaseJob> Jobs => _jobs;

        public void SeedFromBaseState(BaseState state, IEnumerable<BaseZoneRuntime> zones)
        {
            if (state == null)
            {
                throw new ArgumentNullException(nameof(state));
            }

            if (zones == null)
            {
                throw new ArgumentNullException(nameof(zones));
            }

            _jobs.Clear();

            foreach (var zone in zones)
            {
                var job = CreateZoneJob(state, zone.Zone);
                if (job != null)
                {
                    _jobs.Add(job);
                }
            }

            if (!string.IsNullOrEmpty(state.Research.ActiveProjectId))
            {
                _jobs.Add(new BaseJob
                {
                    Id = $"job_research_{state.Research.ActiveProjectId}",
                    Type = BaseJobType.Research,
                    Priority = BaseJobPriority.High,
                    ZoneId = state.Zones.FirstOrDefault(z => z.Type == ZoneType.ResearchLab)?.Id,
                    DurationHours = 6,
                    RemainingHours = 6,
                    Repeatable = true
                });
            }

            if (state.Population.Count > 0)
            {
                _jobs.Add(new BaseJob
                {
                    Id = "job_patrol_default",
                    Type = BaseJobType.Patrol,
                    Priority = BaseJobPriority.Normal,
                    ZoneId = state.Zones.FirstOrDefault(z => z.Type == ZoneType.Watchtower)?.Id,
                    DurationHours = 8,
                    RemainingHours = 8,
                    Repeatable = true
                });
            }
        }

        public void Enqueue(BaseJob job)
        {
            if (job == null)
            {
                throw new ArgumentNullException(nameof(job));
            }

            if (_jobs.Any(existing => string.Equals(existing.Id, job.Id, StringComparison.Ordinal)))
            {
                return;
            }

            job.RemainingHours = job.DurationHours;
            _jobs.Add(job);
        }

        public void Advance(in BaseModeTickContext context, ICollection<BaseJobResult> completed)
        {
            if (completed == null)
            {
                throw new ArgumentNullException(nameof(completed));
            }

            if (_jobs.Count == 0)
            {
                return;
            }

            var workforce = Math.Max(1, context.BaseState.Population.Count);
            var ordered = _jobs
                .OrderByDescending(job => job.Priority)
                .ThenBy(job => job.Id, StringComparer.Ordinal)
                .ToList();

            var processed = 0;
            foreach (var job in ordered)
            {
                if (processed >= workforce)
                {
                    break;
                }

                job.RemainingHours--;
                processed++;

                if (job.RemainingHours > 0)
                {
                    continue;
                }

                completed.Add(new BaseJobResult(job.Id, job.Type, job.ZoneId, job.Priority, job.DurationHours));

                if (job.Repeatable)
                {
                    job.RemainingHours = job.DurationHours;
                }
                else
                {
                    _jobs.Remove(job);
                }
            }
        }

        private static BaseJob? CreateZoneJob(BaseState state, BaseZone zone)
        {
            var priority = zone.Type switch
            {
                ZoneType.Watchtower => BaseJobPriority.High,
                ZoneType.Workshop => BaseJobPriority.Normal,
                ZoneType.ResearchLab => BaseJobPriority.High,
                ZoneType.Farm => BaseJobPriority.Normal,
                _ => BaseJobPriority.Low
            };

            var type = zone.Type switch
            {
                ZoneType.Habitat => BaseJobType.Maintenance,
                ZoneType.Workshop => BaseJobType.Production,
                ZoneType.Farm => BaseJobType.Production,
                ZoneType.Watchtower => BaseJobType.Patrol,
                ZoneType.ResearchLab => BaseJobType.Research,
                _ => BaseJobType.Maintenance
            };

            var duration = zone.Type switch
            {
                ZoneType.Farm => 10,
                ZoneType.Workshop => 8,
                ZoneType.Watchtower => 8,
                ZoneType.ResearchLab => 6,
                _ => 6
            };

            return new BaseJob
            {
                Id = $"job_{zone.Id}_{type.ToString().ToLowerInvariant()}",
                Type = type,
                Priority = priority,
                ZoneId = zone.Id,
                DurationHours = duration,
                RemainingHours = duration,
                Repeatable = true
            };
        }
    }

    public sealed class BaseJob
    {
        public string Id { get; set; } = string.Empty;
        public BaseJobType Type { get; set; }
        public BaseJobPriority Priority { get; set; }
        public string? ZoneId { get; set; }
        public int DurationHours { get; set; }
        public int RemainingHours { get; set; }
        public bool Repeatable { get; set; }
    }

    public readonly struct BaseJobResult
    {
        public BaseJobResult(string id, BaseJobType type, string? zoneId, BaseJobPriority priority, int durationHours)
        {
            Id = id ?? throw new ArgumentNullException(nameof(id));
            Type = type;
            ZoneId = zoneId;
            Priority = priority;
            DurationHours = durationHours;
        }

        public string Id { get; }
        public BaseJobType Type { get; }
        public string? ZoneId { get; }
        public BaseJobPriority Priority { get; }
        public int DurationHours { get; }
    }

    public enum BaseJobType
    {
        Maintenance,
        Production,
        Research,
        Patrol
    }

    public enum BaseJobPriority
    {
        Low = 0,
        Normal = 1,
        High = 2,
        Critical = 3
    }

    public sealed class RaidThreatState
    {
        public float ThreatMeter { get; set; } = 0.3f;
        public bool RaidScheduled { get; set; }
        public int HoursUntilRaid { get; set; }
        public string AttackingFactionId { get; set; } = string.Empty;
    }

    public sealed class MandateTracker
    {
        private readonly List<BaseMandate> _mandates = new();
        private readonly int _hoursPerDay;
        private int _hourAccumulator;

        public MandateTracker(int hoursPerDay)
        {
            if (hoursPerDay <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(hoursPerDay));
            }

            _hoursPerDay = hoursPerDay;
        }

        public IReadOnlyList<BaseMandate> Mandates => _mandates;

        public void Initialize(WorldData world, BaseState state)
        {
            if (world == null)
            {
                throw new ArgumentNullException(nameof(world));
            }

            if (state == null)
            {
                throw new ArgumentNullException(nameof(state));
            }

            _mandates.Clear();

            var issuer = world.Characters.FirstOrDefault()?.Id ?? string.Empty;
            var hasWorkshop = state.Zones.Any(z => z.Type == ZoneType.Workshop);
            var hasResearch = state.Zones.Any(z => z.Type == ZoneType.ResearchLab);

            _mandates.Add(new BaseMandate
            {
                Id = "mandate_secure_water",
                IssuerCharacterId = issuer,
                Type = MandateType.Infrastructure,
                Status = MandateStatus.Active,
                TargetJobType = BaseJobType.Maintenance,
                RequiredCompletions = 2,
                DaysRemaining = 4
            });

            if (hasWorkshop)
            {
                _mandates.Add(new BaseMandate
                {
                    Id = "mandate_stockpile_supplies",
                    IssuerCharacterId = issuer,
                    Type = MandateType.Production,
                    Status = MandateStatus.Active,
                    TargetJobType = BaseJobType.Production,
                    RequiredCompletions = 3,
                    DaysRemaining = 5
                });
            }

            if (hasResearch)
            {
                _mandates.Add(new BaseMandate
                {
                    Id = "mandate_finish_research",
                    IssuerCharacterId = issuer,
                    Type = MandateType.Research,
                    Status = MandateStatus.Active,
                    TargetJobType = BaseJobType.Research,
                    RequiredCompletions = 2,
                    DaysRemaining = 6
                });
            }
        }

        public IReadOnlyList<BaseMandateResolution> Advance(in BaseModeTickContext context, IReadOnlyList<BaseJobResult> completedJobs)
        {
            if (_mandates.Count == 0)
            {
                return Array.Empty<BaseMandateResolution>();
            }

            _hourAccumulator++;
            if (_hourAccumulator < _hoursPerDay)
            {
                return Array.Empty<BaseMandateResolution>();
            }

            _hourAccumulator = 0;
            var resolutions = new List<BaseMandateResolution>();

            foreach (var mandate in _mandates)
            {
                if (mandate.Status != MandateStatus.Active)
                {
                    continue;
                }

                var progress = completedJobs.Count(job => job.Type == mandate.TargetJobType);
                if (progress > 0)
                {
                    mandate.CompletedCount += progress;
                    if (mandate.CompletedCount >= mandate.RequiredCompletions)
                    {
                        mandate.Status = MandateStatus.Completed;
                        resolutions.Add(new BaseMandateResolution(mandate, MandateStatus.Completed));
                        continue;
                    }
                }

                mandate.DaysRemaining--;
                if (mandate.DaysRemaining < 0)
                {
                    mandate.Status = MandateStatus.Failed;
                    resolutions.Add(new BaseMandateResolution(mandate, MandateStatus.Failed));
                }
            }

            return resolutions;
        }

        public void EnqueueFollowUpMandate(BaseMandate source, long tick)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            var followUp = new BaseMandate
            {
                Id = $"mandate_followup_{tick:D6}_{source.Type.ToString().ToLowerInvariant()}",
                IssuerCharacterId = source.IssuerCharacterId,
                Type = source.Type,
                Status = MandateStatus.Active,
                TargetJobType = source.TargetJobType,
                RequiredCompletions = Math.Max(1, source.RequiredCompletions),
                DaysRemaining = Math.Max(3, source.DaysRemaining + 3)
            };

            _mandates.Add(followUp);
        }
    }

    public sealed class BaseMandate
    {
        public string Id { get; set; } = string.Empty;
        public string IssuerCharacterId { get; set; } = string.Empty;
        public MandateType Type { get; set; }
        public MandateStatus Status { get; set; }
        public BaseJobType TargetJobType { get; set; }
        public int RequiredCompletions { get; set; }
        public int CompletedCount { get; set; }
        public int DaysRemaining { get; set; }
    }

    public enum MandateType
    {
        Infrastructure,
        Production,
        Defense,
        Research
    }

    public enum MandateStatus
    {
        Active,
        Completed,
        Failed
    }

    public readonly struct BaseMandateResolution
    {
        public BaseMandateResolution(BaseMandate mandate, MandateStatus result)
        {
            Mandate = mandate ?? throw new ArgumentNullException(nameof(mandate));
            Result = result;
        }

        public BaseMandate Mandate { get; }
        public MandateStatus Result { get; }
    }
}
