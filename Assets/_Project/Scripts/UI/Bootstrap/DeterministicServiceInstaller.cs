using System;
using UnityEngine;
using Wastelands.Core.Management;
using Wastelands.Core.Services;

namespace Wastelands.UI.Bootstrap
{
    /// <summary>
    /// Scriptable object used by the Boot scene to configure deterministic services.
    /// </summary>
    [CreateAssetMenu(fileName = "DeterministicServiceInstaller", menuName = "Wastelands/Deterministic Service Installer")]
    public sealed class DeterministicServiceInstaller : ScriptableObject
    {
        [Header("Seed & Timing")] [SerializeField] private int seed = 12345;
        [SerializeField] private int ticksPerYear = 1;
        [SerializeField] private long ticksPerDay = 24;

        [NonSerialized] private DeterministicServiceContainer? _container;

        public DeterministicServiceContainer Install()
        {
            if (_container != null)
            {
                DeterministicServicesProvider.SetContainer(_container);
                return _container;
            }

            var timeProvider = new ManualTimeProvider(ticksPerYear, ticksPerDay);
            var rngService = new DeterministicRngService(seed);
            var eventBus = new EventBus();
            var tickManager = new TickManager(timeProvider, rngService, eventBus);
            _container = new DeterministicServiceContainer(timeProvider, rngService, eventBus, tickManager);
            DeterministicServicesProvider.SetContainer(_container);
            return _container;
        }

        public void ResetContainer()
        {
            _container = null;
            DeterministicServicesProvider.Clear();
        }

        public void ConfigureSeed(int newSeed)
        {
            seed = newSeed;
            if (_container != null)
            {
                _container.RngService.Reset(newSeed);
            }
        }
    }
}
