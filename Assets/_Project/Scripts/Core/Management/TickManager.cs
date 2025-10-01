using System;
using System.Collections.Generic;
using Wastelands.Core.Services;

namespace Wastelands.Core.Management
{
    public interface ITickSystem
    {
        void Tick(in TickContext context);
    }

    public readonly struct TickContext
    {
        public TickContext(long tick, ITimeProvider timeProvider, IRngService rngService, IEventBus eventBus)
        {
            Tick = tick;
            TimeProvider = timeProvider ?? throw new ArgumentNullException(nameof(timeProvider));
            RngService = rngService ?? throw new ArgumentNullException(nameof(rngService));
            EventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
        }

        public long Tick { get; }
        public ITimeProvider TimeProvider { get; }
        public IRngService RngService { get; }
        public IEventBus EventBus { get; }
    }

    public interface ITickManager
    {
        void RegisterSystem(ITickSystem system);
        void UnregisterSystem(ITickSystem system);
        void Advance(int ticks = 1);
    }

    /// <summary>
    /// Deterministic tick manager that advances registered systems in order.
    /// </summary>
    public sealed class TickManager : ITickManager
    {
        private readonly List<ITickSystem> _systems = new();
        private readonly ITimeProvider _timeProvider;
        private readonly IRngService _rngService;
        private readonly IEventBus _eventBus;

        public TickManager(ITimeProvider timeProvider, IRngService rngService, IEventBus eventBus)
        {
            _timeProvider = timeProvider ?? throw new ArgumentNullException(nameof(timeProvider));
            _rngService = rngService ?? throw new ArgumentNullException(nameof(rngService));
            _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
        }

        public void RegisterSystem(ITickSystem system)
        {
            if (system == null)
            {
                throw new ArgumentNullException(nameof(system));
            }

            if (_systems.Contains(system))
            {
                return;
            }

            _systems.Add(system);
        }

        public void UnregisterSystem(ITickSystem system)
        {
            if (system == null)
            {
                return;
            }

            _systems.Remove(system);
        }

        public void Advance(int ticks = 1)
        {
            if (ticks <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(ticks));
            }

            for (var step = 0; step < ticks; step++)
            {
                var nextTick = _timeProvider.CurrentTick + 1;
                _timeProvider.SetTick(nextTick);
                var context = new TickContext(nextTick, _timeProvider, _rngService, _eventBus);

                foreach (var system in _systems)
                {
                    system.Tick(context);
                }
            }
        }
    }
}
