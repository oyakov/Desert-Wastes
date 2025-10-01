using System;
using Wastelands.Core.Services;

namespace Wastelands.Core.Management
{
    /// <summary>
    /// Aggregate of deterministic runtime services shared across scenes.
    /// </summary>
    public sealed class DeterministicServiceContainer
    {
        public DeterministicServiceContainer(ITimeProvider timeProvider, IRngService rngService, IEventBus eventBus, ITickManager tickManager)
        {
            TimeProvider = timeProvider ?? throw new ArgumentNullException(nameof(timeProvider));
            RngService = rngService ?? throw new ArgumentNullException(nameof(rngService));
            EventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
            TickManager = tickManager ?? throw new ArgumentNullException(nameof(tickManager));
        }

        public ITimeProvider TimeProvider { get; }
        public IRngService RngService { get; }
        public IEventBus EventBus { get; }
        public ITickManager TickManager { get; }
    }

    /// <summary>
    /// Lightweight static provider to bridge Unity scenes until a DI container is introduced.
    /// </summary>
    public static class DeterministicServicesProvider
    {
        private static DeterministicServiceContainer? _container;

        public static DeterministicServiceContainer Container
        {
            get
            {
                if (_container == null)
                {
                    throw new InvalidOperationException("Deterministic services have not been initialized.");
                }

                return _container;
            }
        }

        public static void SetContainer(DeterministicServiceContainer container)
        {
            _container = container ?? throw new ArgumentNullException(nameof(container));
        }

        public static void Clear() => _container = null;
    }
}
