using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Wastelands.BaseMode;
using Wastelands.BaseMode.Unity;
using Wastelands.Core.Data;
using Wastelands.Core.Management;
using Wastelands.UI.Bootstrap;

namespace Wastelands.Tests.PlayMode
{
    public class BaseSceneIndirectCommandSmokeTests
    {
        private DeterministicServiceInstaller? _installer;

        [SetUp]
        public void SetUp()
        {
            _installer = ScriptableObject.CreateInstance<DeterministicServiceInstaller>();
            _installer.Install();
        }

        [TearDown]
        public void TearDown()
        {
            if (_installer != null)
            {
                _installer.ResetContainer();
                ScriptableObject.DestroyImmediate(_installer);
                _installer = null;
            }
        }

        [UnityTest]
        public IEnumerator InstallerPublishesDispatcherAndAcceptsCommands()
        {
            var world = CreateWorld();
            var go = new GameObject("BaseSceneInstaller");
            var installer = go.AddComponent<BaseSceneInstallerBehaviour>();
            installer.AutoAdvanceTicks = false;
            installer.SetWorld(world);
            go.AddComponent<BaseSceneDebugHud>();

            BaseRuntimeState? runtime = null;
            IBaseIndirectCommandDispatcher? dispatcher = null;
            BaseIndirectCommand? issuedCommand = null;

            using var runtimeSubscription = DeterministicServicesProvider.Container.EventBus.Subscribe<BaseSceneBootstrapped>(evt => runtime = evt.Runtime);
            using var dispatcherSubscription = DeterministicServicesProvider.Container.EventBus.Subscribe<BaseIndirectCommandDispatcherReady>(evt => dispatcher = evt.Dispatcher);
            using var commandSubscription = DeterministicServicesProvider.Container.EventBus.Subscribe<BaseIndirectCommandQueued>(evt => issuedCommand = evt.Command);

            installer.BootstrapNow();
            yield return null;

            Assert.IsNotNull(runtime, "Runtime should be published after bootstrapping.");
            Assert.IsNotNull(dispatcher, "Dispatcher should be provided for indirect commands.");

            dispatcher!.Issue(new BaseIndirectCommand("debug.test", targetId: "zone_hab"));
            yield return null;

            Assert.IsNotNull(issuedCommand, "Issuing a command should publish a queued event.");
            Assert.AreEqual("debug.test", issuedCommand?.CommandType);
            Assert.AreEqual("zone_hab", issuedCommand?.TargetId);

            Object.DestroyImmediate(go);
        }

        private static WorldData CreateWorld()
        {
            var tile = new Tile
            {
                Id = "tile_0_0",
                Position = new Int2(0, 0),
                BiomeId = "biome_desert",
                HazardTags = new List<string> { "dust" }
            };

            var faction = new Faction
            {
                Id = "fac_alpha",
                Name = "Alpha",
                Archetype = FactionArchetype.Nomads,
                NobleRoster = new List<NobleRoleAssignment>(),
                Relations = new List<RelationRecord>()
            };

            var settlement = new Settlement
            {
                Id = "set_01",
                FactionId = faction.Id,
                TileId = tile.Id,
                Population = 10,
                Economy = new EconomyProfile { Production = 1f, Trade = 0.5f, Research = 0.2f }
            };

            var character = new Character
            {
                Id = "char_leader",
                Name = "Leader",
                FactionId = faction.Id,
                Traits = new List<TraitId> { TraitId.Stoic },
                Skills = new Dictionary<SkillId, SkillLevel>
                {
                    { SkillId.Leadership, new SkillLevel { Level = 3, Experience = 5, Aptitude = 1f } }
                },
                Relationships = new List<RelationshipRecord>()
            };

            faction.NobleRoster.Add(new NobleRoleAssignment { CharacterId = character.Id, Role = NobleRole.Overseer });
            faction.Holdings.Add(settlement.Id);

            var world = new WorldData
            {
                Seed = 42,
                Tiles = new List<Tile> { tile },
                Factions = new List<Faction> { faction },
                Settlements = new List<Settlement> { settlement },
                Characters = new List<Character> { character },
                OracleState = new OracleState(),
                Legends = new List<LegendEntry>(),
                BaseState = new BaseState
                {
                    Active = true,
                    SiteTileId = tile.Id,
                    Zones = new List<BaseZone>
                    {
                        new() { Id = "zone_hab", Name = "Hab", Type = ZoneType.Habitat, Efficiency = 1f }
                    },
                    Population = new List<string> { character.Id },
                    Infrastructure = new Dictionary<string, float> { { "power", 1f } },
                    Inventory = new List<ItemStack>(),
                    AlertLevel = AlertLevel.Calm,
                    Research = new ResearchState
                    {
                        CompletedProjects = new List<string>(),
                        ActiveProjectId = "tech_drills",
                        ActiveProgress = 0.5f
                    }
                }
            };

            faction.Relations.Add(new RelationRecord { TargetFactionId = faction.Id, Standing = 1f, State = RelationState.Allied });

            return world;
        }
    }
}
