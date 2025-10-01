using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Wastelands.BaseMode;
using Wastelands.Core.Management;
using Wastelands.Core.Services;

namespace Wastelands.BaseMode.Unity
{
    [AddComponentMenu("Wastelands/Base Scene Debug HUD")]
    public sealed class BaseSceneDebugHud : MonoBehaviour
    {
        private BaseRuntimeState? _runtime;
        private IBaseIndirectCommandDispatcher? _dispatcher;
        private ITimeProvider? _timeProvider;
        private IDisposable? _bootSubscription;
        private IDisposable? _dispatcherSubscription;
        private Vector2 _scrollPosition;

        private void OnEnable()
        {
            if (!Application.isPlaying)
            {
                return;
            }

            Subscribe();
        }

        private void OnDisable()
        {
            _bootSubscription?.Dispose();
            _dispatcherSubscription?.Dispose();
            _bootSubscription = null;
            _dispatcherSubscription = null;
            _runtime = null;
            _dispatcher = null;
        }

        private void Subscribe()
        {
            try
            {
                var services = DeterministicServicesProvider.Container;
                _timeProvider = services.TimeProvider;
                _bootSubscription = services.EventBus.Subscribe<BaseSceneBootstrapped>(OnBootstrapped);
                _dispatcherSubscription = services.EventBus.Subscribe<BaseIndirectCommandDispatcherReady>(OnDispatcherReady);
            }
            catch (Exception ex)
            {
                Debug.LogError($"BaseSceneDebugHud failed to subscribe: {ex}");
            }
        }

        private void OnBootstrapped(BaseSceneBootstrapped evt)
        {
            _runtime = evt.Runtime;
        }

        private void OnDispatcherReady(BaseIndirectCommandDispatcherReady evt)
        {
            _dispatcher = evt.Dispatcher;
        }

        private void OnGUI()
        {
            if (_runtime == null)
            {
                return;
            }

            var area = new Rect(10f, 10f, 360f, Screen.height - 20f);
            GUILayout.BeginArea(area, GUI.skin.box);
            GUILayout.Label("Base Mode Debug");
            GUILayout.Label($"Tick: {_timeProvider?.CurrentTick ?? 0}");
            GUILayout.Label($"Population: {_runtime.BaseState.Population.Count}");
            GUILayout.Space(6f);

            GUILayout.Label("Zones");
            _scrollPosition = GUILayout.BeginScrollView(_scrollPosition, GUILayout.Height(220f));
            foreach (var zone in _runtime.EnumerateZones())
            {
                GUILayout.BeginVertical(GUI.skin.box);
                GUILayout.Label($"{zone.Zone.Name} ({zone.Zone.Id})");
                GUILayout.Label($"Morale: {zone.MoraleModifier:0.00}  Wear: {zone.Wear:0.00}");
                GUILayout.Label($"Workforce: {zone.WorkforceAllocation:0.00}");

                if (_dispatcher != null && GUILayout.Button("Boost Morale"))
                {
                    IssueCommand("zone.adjust_morale", zone.Zone.Id, "delta", "0.1");
                }

                if (_dispatcher != null && GUILayout.Button("Schedule Inspection"))
                {
                    IssueCommand("zone.schedule_inspection", zone.Zone.Id);
                }

                GUILayout.EndVertical();
            }

            GUILayout.EndScrollView();
            GUILayout.Space(6f);

            GUILayout.Label("Jobs");
            foreach (var job in _runtime.JobBoard.Jobs.Take(5))
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label($"{job.Id} ({job.Type})", GUILayout.Width(200f));
                GUILayout.Label($"ETA: {job.RemainingHours}h", GUILayout.Width(80f));
                if (_dispatcher != null && GUILayout.Button("Rush"))
                {
                    IssueCommand("job.rush", job.Id, "duration", "-1");
                }

                GUILayout.EndHorizontal();
            }

            GUILayout.Space(6f);
            GUILayout.Label("Mandates");
            foreach (var mandate in _runtime.MandateTracker.Mandates.Take(5))
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label($"{mandate.Id} [{mandate.State}]");
                if (_dispatcher != null && GUILayout.Button("Acknowledge"))
                {
                    IssueCommand("mandate.acknowledge", mandate.Id);
                }

                GUILayout.EndHorizontal();
            }

            if (_dispatcher != null)
            {
                GUILayout.Space(6f);
                GUILayout.Label("Recent Commands");
                foreach (var command in _dispatcher.RecentCommands.Reverse().Take(5))
                {
                    GUILayout.Label($"{command.CommandType} -> {command.TargetId}");
                }
            }

            GUILayout.EndArea();
        }

        private void IssueCommand(string commandType, string targetId, string? payloadKey = null, string? payloadValue = null)
        {
            if (_dispatcher == null)
            {
                return;
            }

            var payload = new Dictionary<string, string>(StringComparer.Ordinal);
            if (!string.IsNullOrEmpty(payloadKey) && payloadValue != null)
            {
                payload[payloadKey] = payloadValue;
            }

            _dispatcher.Issue(new BaseIndirectCommand(commandType, targetId, payload));
        }
    }
}
