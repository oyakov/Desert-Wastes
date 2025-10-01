using System;
using UnityEngine;
using Wastelands.BaseMode;
using Wastelands.Core.Data;
using Wastelands.Core.Management;
using Wastelands.Generation;
using Wastelands.Persistence;

namespace Wastelands.BaseMode.Unity
{
    [AddComponentMenu("Wastelands/Base Scene Installer")]
    public sealed class BaseSceneInstallerBehaviour : MonoBehaviour
    {
        [Header("World Snapshot")]
        [SerializeField] private TextAsset? worldSnapshot;

        [Header("Runtime Settings")]
        [SerializeField] private int hoursPerDay = 24;
        [SerializeField] private bool autoAdvanceTicks = true;
        [SerializeField] private int ticksPerFrame = 1;

        [Header("Generated World Defaults")]
        [SerializeField] private int worldWidth = 24;
        [SerializeField] private int worldHeight = 16;
        [SerializeField] private ApocalypseType apocalypseType = ApocalypseType.RadiantStorm;

        private bool _bootstrapped;
        private BaseSceneBootstrapper? _bootstrapper;
        private WorldData? _overrideWorld;

        public bool HasBootstrapped => _bootstrapped;
        public BaseSceneBootstrapper? Bootstrapper => _bootstrapper;
        public bool AutoAdvanceTicks { get => autoAdvanceTicks; set => autoAdvanceTicks = value; }
        public int TicksPerFrame { get => ticksPerFrame; set => ticksPerFrame = Mathf.Max(0, value); }
        public int HoursPerDay { get => hoursPerDay; set => hoursPerDay = Mathf.Max(1, value); }

        public void SetWorld(WorldData world)
        {
            _overrideWorld = world ?? throw new ArgumentNullException(nameof(world));
        }

        public void BootstrapNow()
        {
            if (_bootstrapped)
            {
                return;
            }

            try
            {
                var services = DeterministicServicesProvider.Container;
                var world = ResolveWorldData(services);
                _bootstrapper = new BaseSceneBootstrapper(world, services, hoursPerDay: hoursPerDay);
                _bootstrapper.Initialize();
                _bootstrapped = true;
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to bootstrap Base scene: {ex}");
            }
        }

        private WorldData ResolveWorldData(DeterministicServiceContainer services)
        {
            if (_overrideWorld != null)
            {
                return _overrideWorld;
            }

            if (worldSnapshot != null && !string.IsNullOrWhiteSpace(worldSnapshot.text))
            {
                var serializer = new WorldDataSerializer();
                return serializer.Deserialize(worldSnapshot.text);
            }

            var config = new OverworldGenerationConfig(
                seed: (ulong)(uint)services.RngService.Seed,
                width: Mathf.Max(4, worldWidth),
                height: Mathf.Max(4, worldHeight),
                apocalypse: apocalypseType);

            var pipeline = new OverworldGenerationPipeline(services.RngService);
            return pipeline.Generate(config);
        }

        private void Start()
        {
            if (!Application.isPlaying)
            {
                return;
            }

            BootstrapNow();
        }

        private void Update()
        {
            if (!Application.isPlaying || !autoAdvanceTicks || !_bootstrapped)
            {
                return;
            }

            var services = DeterministicServicesProvider.Container;
            var steps = Mathf.Max(0, ticksPerFrame);
            for (var i = 0; i < steps; i++)
            {
                services.TickManager.Advance();
            }
        }

        private void OnDestroy()
        {
            if (!Application.isPlaying)
            {
                return;
            }

            if (_bootstrapper?.SimulationLoop != null)
            {
                DeterministicServicesProvider.Container.TickManager.UnregisterSystem(_bootstrapper.SimulationLoop);
            }
        }
    }
}
