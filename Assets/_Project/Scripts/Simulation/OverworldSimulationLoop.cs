using System;
using System.Collections.Generic;
using System.Linq;
using Wastelands.Core.Data;
using Wastelands.Core.Management;
using Wastelands.Core.Services;

namespace Wastelands.Simulation
{
    public readonly struct OverworldTickContext
    {
        private readonly TickContext _tickContext;
        private readonly IRngService _rngService;

        internal OverworldTickContext(WorldData world, TickContext tickContext, IRngService rngService)
        {
            World = world ?? throw new ArgumentNullException(nameof(world));
            _tickContext = tickContext;
            _rngService = rngService ?? throw new ArgumentNullException(nameof(rngService));
        }

        public long Tick => _tickContext.Tick;
        public WorldData World { get; }
        public ITimeProvider TimeProvider => _tickContext.TimeProvider;
        public IEventBus EventBus => _tickContext.EventBus;

        public IRngChannel GetChannel(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("Channel name must be provided.", nameof(name));
            }

            var channel = _rngService.GetChannel($"simulation.{name}");
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

    public interface IOverworldSimulationPhase
    {
        string Name { get; }
        void Execute(in OverworldTickContext context);
    }

    public sealed class OverworldSimulationLoop : ITickSystem
    {
        private readonly WorldData _world;
        private readonly List<IOverworldSimulationPhase> _phases;
        private readonly IRngService _rngService;

        public OverworldSimulationLoop(WorldData world, IEnumerable<IOverworldSimulationPhase> phases, IRngService rngService)
        {
            _world = world ?? throw new ArgumentNullException(nameof(world));
            _rngService = rngService ?? throw new ArgumentNullException(nameof(rngService));

            if (phases == null)
            {
                throw new ArgumentNullException(nameof(phases));
            }

            _phases = phases.ToList();
            if (_phases.Count == 0)
            {
                throw new ArgumentException("At least one simulation phase must be provided.", nameof(phases));
            }
        }

        public IReadOnlyList<IOverworldSimulationPhase> Phases => _phases;

        public void Tick(in TickContext context)
        {
            var overworldContext = new OverworldTickContext(_world, context, _rngService);
            foreach (var phase in _phases)
            {
                phase.Execute(overworldContext);
            }

            WorldDataNormalizer.Normalize(_world);
            context.EventBus.Publish(new OverworldTickCompleted(overworldContext.Tick));
        }
    }

    public readonly struct OverworldTickCompleted
    {
        public OverworldTickCompleted(long tick)
        {
            Tick = tick;
        }

        public long Tick { get; }
    }
}
