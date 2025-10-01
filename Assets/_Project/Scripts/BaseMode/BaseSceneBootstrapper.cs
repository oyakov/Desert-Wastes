using System;
using System.Collections.Generic;
using System.Linq;
using Wastelands.Core.Data;
using Wastelands.Core.Management;

namespace Wastelands.BaseMode
{
    /// <summary>
    /// Composition root for the Base scene. Creates runtime state, registers simulation loop,
    /// and signals readiness via the deterministic event bus.
    /// </summary>
    public sealed class BaseSceneBootstrapper
    {
        private readonly WorldData _world;
        private readonly DeterministicServiceContainer _services;
        private readonly List<IBaseModeSystem> _systems;
        private readonly IBaseIndirectCommandDispatcher _commandDispatcher;
        private readonly int _hoursPerDay;

        public BaseSceneBootstrapper(WorldData world, DeterministicServiceContainer services, IEnumerable<IBaseModeSystem>? systems = null, int hoursPerDay = 24, IBaseIndirectCommandDispatcher? commandDispatcher = null)
        {
            _world = world ?? throw new ArgumentNullException(nameof(world));
            _services = services ?? throw new ArgumentNullException(nameof(services));
            if (hoursPerDay <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(hoursPerDay));
            }

            _hoursPerDay = hoursPerDay;
            _systems = systems?.ToList() ?? new List<IBaseModeSystem>();
            _commandDispatcher = commandDispatcher ?? new BaseIndirectCommandDispatcher(_services.EventBus);
        }

        public BaseRuntimeState? Runtime { get; private set; }
        public BaseModeSimulationLoop? SimulationLoop { get; private set; }
        public IBaseIndirectCommandDispatcher CommandDispatcher => _commandDispatcher;

        public void Initialize()
        {
            WorldDataNormalizer.Normalize(_world);
            var baseState = _world.BaseState ?? throw new InvalidOperationException("World data is missing BaseState.");
            baseState.Active = true;

            var siteContext = BuildSiteContext(_world, baseState);
            Runtime = new BaseRuntimeState(_world, baseState, _hoursPerDay, siteContext);
            ApplySiteModifiers(Runtime);
            Runtime.SeedInitialJobs();
            Runtime.SeedInitialMandates(_world);
            Runtime.BindCommandDispatcher(_commandDispatcher);

            if (_systems.Count == 0)
            {
                _systems.AddRange(CreateDefaultSystems());
            }

            SimulationLoop = new BaseModeSimulationLoop(_world, Runtime, _systems, _services.RngService);
            _services.TickManager.RegisterSystem(SimulationLoop);
            _services.EventBus.Publish(new BaseSceneBootstrapped(Runtime));
            _services.EventBus.Publish(new BaseIndirectCommandDispatcherReady(_commandDispatcher));
        }

        private static IEnumerable<IBaseModeSystem> CreateDefaultSystems()
        {
            return new IBaseModeSystem[]
            {
                new ZoneMaintenanceSystem(),
                new JobSchedulingSystem(),
                new RaidThreatSystem(),
                new MandateResolutionSystem()
            };
        }

        private static BaseSiteContext BuildSiteContext(WorldData world, BaseState baseState)
        {
            if (string.IsNullOrEmpty(baseState.SiteTileId))
            {
                return BaseSiteContext.Empty;
            }

            var tilesById = world.Tiles.ToDictionary(tile => tile.Id, StringComparer.Ordinal);
            if (!tilesById.TryGetValue(baseState.SiteTileId, out var siteTile))
            {
                return BaseSiteContext.Empty;
            }

            var hazards = siteTile.HazardTags ?? new List<string>();
            var biomeId = siteTile.BiomeId ?? string.Empty;

            var factionLookup = world.Factions.ToDictionary(faction => faction.Id, faction => faction.Name, StringComparer.Ordinal);
            var nearbyFactions = world.Settlements
                .Select(settlement => CreateFactionProximity(settlement, siteTile, tilesById, factionLookup))
                .Where(proximity => proximity != null)
                .Select(proximity => proximity!)
                .OrderBy(proximity => proximity.Distance)
                .Take(5)
                .ToArray();

            return new BaseSiteContext(biomeId, hazards, nearbyFactions);
        }

        private static BaseNearbyFaction? CreateFactionProximity(Settlement settlement, Tile siteTile, IReadOnlyDictionary<string, Tile> tilesById, IReadOnlyDictionary<string, string> factionLookup)
        {
            if (!tilesById.TryGetValue(settlement.TileId, out var settlementTile))
            {
                return null;
            }

            var factionId = settlement.FactionId ?? string.Empty;
            factionLookup.TryGetValue(factionId, out var factionName);
            var distance = ComputeDistance(siteTile.Position, settlementTile.Position);
            return new BaseNearbyFaction(factionId, factionName ?? factionId, distance);
        }

        private static float ComputeDistance(Int2 a, Int2 b)
        {
            var dx = a.X - b.X;
            var dy = a.Y - b.Y;
            return (float)Math.Sqrt(dx * dx + dy * dy);
        }

        private static void ApplySiteModifiers(BaseRuntimeState runtime)
        {
            var baseState = runtime.BaseState;
            foreach (var hazard in runtime.SiteContext.Hazards)
            {
                ApplyHazardModifier(baseState, hazard);
            }
        }

        private static void ApplyHazardModifier(BaseState baseState, string hazardId)
        {
            switch (hazardId)
            {
                case "haz_solar_scorch":
                    AdjustInfrastructure(baseState, "water", -0.15f);
                    AdjustZoneEfficiency(baseState, ZoneType.Farm, -0.1f);
                    break;
                case "haz_sporefall":
                    AdjustInfrastructure(baseState, "morale", -0.1f);
                    AdjustZoneEfficiency(baseState, ZoneType.Habitat, -0.05f);
                    break;
                case "haz_radiant_flux":
                    AdjustInfrastructure(baseState, "morale", -0.12f);
                    AdjustInfrastructure(baseState, "defense", -0.1f);
                    AdjustZoneEfficiency(baseState, ZoneType.Watchtower, -0.05f);
                    break;
                case "haz_nanite_bloom":
                    AdjustInfrastructure(baseState, "power", -0.12f);
                    AdjustZoneEfficiency(baseState, ZoneType.Workshop, -0.07f);
                    break;
                case "haz_void_rupture":
                    AdjustInfrastructure(baseState, "morale", -0.12f);
                    AdjustZoneEfficiency(baseState, ZoneType.ResearchLab, -0.08f);
                    break;
                case "haz_hollow_winds":
                    AdjustInfrastructure(baseState, "defense", -0.1f);
                    AdjustZoneEfficiency(baseState, ZoneType.Watchtower, -0.07f);
                    break;
                case "haz_unknown":
                    AdjustInfrastructure(baseState, "water", -0.05f);
                    AdjustInfrastructure(baseState, "morale", -0.05f);
                    break;
                default:
                    if (hazardId.StartsWith("haz_", StringComparison.Ordinal))
                    {
                        AdjustInfrastructure(baseState, "power", -0.05f);
                    }

                    break;
            }
        }

        private static void AdjustInfrastructure(BaseState baseState, string key, float delta)
        {
            if (!baseState.Infrastructure.TryGetValue(key, out var value))
            {
                value = 0.5f;
            }

            baseState.Infrastructure[key] = Clamp(value + delta, 0f, 1.5f);
        }

        private static void AdjustZoneEfficiency(BaseState baseState, ZoneType zoneType, float delta)
        {
            foreach (var zone in baseState.Zones.Where(z => z.Type == zoneType))
            {
                zone.Efficiency = Clamp(zone.Efficiency + delta, 0.3f, 1.5f);
            }
        }

        private static float Clamp(float value, float min, float max)
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

    public readonly struct BaseSceneBootstrapped
    {
        public BaseSceneBootstrapped(BaseRuntimeState runtime)
        {
            Runtime = runtime ?? throw new ArgumentNullException(nameof(runtime));
        }

        public BaseRuntimeState Runtime { get; }
    }
}
