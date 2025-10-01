using System;
using System.Collections.Generic;
using System.Linq;
using Wastelands.Core.Data;
using Wastelands.Core.Management;
using Wastelands.Core.Services;

namespace Wastelands.BaseMode
{
    public readonly struct BaseModeTickContext
    {
        private readonly TickContext _tickContext;
        private readonly IRngService _rngService;

        internal BaseModeTickContext(WorldData world, BaseRuntimeState runtime, TickContext tickContext, IRngService rngService)
        {
            World = world ?? throw new ArgumentNullException(nameof(world));
            Runtime = runtime ?? throw new ArgumentNullException(nameof(runtime));
            _tickContext = tickContext;
            _rngService = rngService ?? throw new ArgumentNullException(nameof(rngService));
        }

        public long Tick => _tickContext.Tick;
        public WorldData World { get; }
        public BaseRuntimeState Runtime { get; }
        public BaseState BaseState => Runtime.BaseState;
        public ITimeProvider TimeProvider => _tickContext.TimeProvider;
        public IEventBus EventBus => _tickContext.EventBus;
        public int HoursPerDay => Runtime.HoursPerDay;

        public IRngChannel GetChannel(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("Channel name must be provided.", nameof(name));
            }

            var channel = _rngService.GetChannel($"base.{name}");
            channel.Reseed(DeriveOffset(name));
            return channel;
        }

        private int DeriveOffset(string name)
        {
            unchecked
            {
                var seedLow = (int)(World.Seed & 0xFFFFFFFF);
                var tickLow = (int)(Tick & 0xFFFFFFFF);
                var nameHash = StringComparer.Ordinal.GetHashCode(name);
                return HashCode.Combine(seedLow, tickLow, nameHash);
            }
        }
    }

    public interface IBaseModeSystem
    {
        string Name { get; }
        void Execute(in BaseModeTickContext context);
    }

    public sealed class BaseModeSimulationLoop : ITickSystem
    {
        private readonly WorldData _world;
        private readonly BaseRuntimeState _runtime;
        private readonly List<IBaseModeSystem> _systems;
        private readonly IRngService _rngService;

        public BaseModeSimulationLoop(WorldData world, BaseRuntimeState runtime, IEnumerable<IBaseModeSystem> systems, IRngService rngService)
        {
            _world = world ?? throw new ArgumentNullException(nameof(world));
            _runtime = runtime ?? throw new ArgumentNullException(nameof(runtime));
            _rngService = rngService ?? throw new ArgumentNullException(nameof(rngService));

            if (systems == null)
            {
                throw new ArgumentNullException(nameof(systems));
            }

            _systems = systems.ToList();
            if (_systems.Count == 0)
            {
                throw new ArgumentException("At least one base mode system must be provided.", nameof(systems));
            }
        }

        public IReadOnlyList<IBaseModeSystem> Systems => _systems;

        public void Tick(in TickContext context)
        {
            if (!_runtime.BaseState.Active)
            {
                return;
            }

            var baseContext = new BaseModeTickContext(_world, _runtime, context, _rngService);
            foreach (var system in _systems)
            {
                system.Execute(baseContext);
            }

            WorldDataNormalizer.Normalize(_world);
            context.EventBus.Publish(new BaseModeTickCompleted(baseContext.Tick));
        }
    }

    public readonly struct BaseModeTickCompleted
    {
        public BaseModeTickCompleted(long tick)
        {
            Tick = tick;
        }

        public long Tick { get; }
    }
}
