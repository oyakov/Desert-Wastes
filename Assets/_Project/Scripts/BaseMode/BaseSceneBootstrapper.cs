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

            Runtime = new BaseRuntimeState(_world, baseState, _hoursPerDay);
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
