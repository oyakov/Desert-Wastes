using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;
using Wastelands.BaseMode;
using Wastelands.Core.Management;
using Wastelands.Core.Services;

namespace Wastelands.BaseMode.Unity
{
    [AddComponentMenu("Wastelands/Base Scene Production UI")]
    [RequireComponent(typeof(UIDocument))]
    public sealed class BaseSceneProductionUI : MonoBehaviour
    {
        [SerializeField] private UIDocument? document;

        private BaseRuntimeState? _runtime;
        private IBaseIndirectCommandDispatcher? _dispatcher;
        private ITimeProvider? _timeProvider;
        private IDisposable? _bootSubscription;
        private IDisposable? _dispatcherSubscription;
        private UIDocument? _resolvedDocument;
        private Label? _tickLabel;
        private Label? _populationLabel;
        private ScrollView? _zoneList;
        private ScrollView? _jobList;
        private ScrollView? _mandateList;
        private bool _uiBuilt;

        private readonly List<ZoneEntry> _zoneEntries = new();
        private readonly List<JobEntry> _jobEntries = new();
        private readonly List<MandateEntry> _mandateEntries = new();
        private readonly List<BaseZoneRuntime> _zoneBuffer = new();
        private readonly List<BaseJob> _jobBuffer = new();
        private readonly List<BaseMandate> _mandateBuffer = new();

        private void OnEnable()
        {
            if (!Application.isPlaying)
            {
                return;
            }

            Subscribe();
            EnsureDocument();
            BuildStaticUi();
        }

        private void OnDisable()
        {
            _bootSubscription?.Dispose();
            _dispatcherSubscription?.Dispose();
            _bootSubscription = null;
            _dispatcherSubscription = null;
            _runtime = null;
            _dispatcher = null;
            _timeProvider = null;
            _uiBuilt = false;
            _zoneEntries.Clear();
            _jobEntries.Clear();
            _mandateEntries.Clear();
        }

        private void Update()
        {
            if (!Application.isPlaying || _runtime == null)
            {
                return;
            }

            if (!_uiBuilt)
            {
                BuildStaticUi();
                if (!_uiBuilt)
                {
                    return;
                }
            }

            RefreshUi();
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
                Debug.LogError($"BaseSceneProductionUI failed to subscribe: {ex}");
            }
        }

        private void OnBootstrapped(BaseSceneBootstrapped evt)
        {
            _runtime = evt.Runtime;
            if (_runtime?.CommandDispatcher != null)
            {
                _dispatcher = _runtime.CommandDispatcher;
            }

            _uiBuilt = false;
        }

        private void OnDispatcherReady(BaseIndirectCommandDispatcherReady evt)
        {
            _dispatcher = evt.Dispatcher;
        }

        private void EnsureDocument()
        {
            if (_resolvedDocument != null)
            {
                return;
            }

            _resolvedDocument = document != null ? document : GetComponent<UIDocument>();
        }

        private void BuildStaticUi()
        {
            EnsureDocument();
            if (_resolvedDocument == null)
            {
                return;
            }

            var root = _resolvedDocument.rootVisualElement;
            if (root == null)
            {
                return;
            }

            root.Clear();
            root.style.flexDirection = FlexDirection.Column;
            root.style.paddingLeft = 8;
            root.style.paddingRight = 8;
            root.style.paddingTop = 8;
            root.style.paddingBottom = 8;
            root.style.marginBottom = 4;

            var header = new VisualElement();
            header.style.flexDirection = FlexDirection.Row;
            header.style.justifyContent = Justify.SpaceBetween;
            header.style.alignItems = Align.Center;

            _tickLabel = new Label("Tick: 0");
            _tickLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
            _populationLabel = new Label("Population: 0");

            header.Add(_tickLabel);
            header.Add(_populationLabel);
            root.Add(header);

            _zoneList = CreateSection(root, "Zones");
            _jobList = CreateSection(root, "Jobs");
            _mandateList = CreateSection(root, "Mandates");

            _uiBuilt = true;
        }

        private static ScrollView CreateSection(VisualElement root, string title)
        {
            var titleLabel = new Label(title);
            titleLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
            titleLabel.style.marginTop = 4;
            titleLabel.style.marginBottom = 4;
            root.Add(titleLabel);

            var scroll = new ScrollView();
            scroll.style.flexGrow = 0f;
            scroll.style.height = 220;
            scroll.style.borderTopWidth = 1;
            scroll.style.borderBottomWidth = 1;
            scroll.style.borderLeftWidth = 1;
            scroll.style.borderRightWidth = 1;
            scroll.style.borderTopColor = new Color(0.2f, 0.2f, 0.2f, 1f);
            scroll.style.borderBottomColor = new Color(0.2f, 0.2f, 0.2f, 1f);
            scroll.style.borderLeftColor = new Color(0.2f, 0.2f, 0.2f, 1f);
            scroll.style.borderRightColor = new Color(0.2f, 0.2f, 0.2f, 1f);
            scroll.style.paddingLeft = 4;
            scroll.style.paddingRight = 4;
            scroll.style.paddingTop = 4;
            scroll.style.paddingBottom = 4;

            root.Add(scroll);
            return scroll;
        }

        private void RefreshUi()
        {
            if (_runtime == null)
            {
                return;
            }

            if (_tickLabel != null)
            {
                var tick = _timeProvider?.CurrentTick ?? 0L;
                _tickLabel.text = $"Tick: {tick}";
            }

            if (_populationLabel != null)
            {
                _populationLabel.text = $"Population: {_runtime.BaseState.Population.Count}";
            }

            UpdateZones();
            UpdateJobs();
            UpdateMandates();
        }

        private void UpdateZones()
        {
            if (_zoneList == null || _runtime == null)
            {
                return;
            }

            _zoneBuffer.Clear();
            _zoneBuffer.AddRange(_runtime.EnumerateZones().OrderBy(z => z.Zone.Name, StringComparer.Ordinal));

            EnsureZoneEntryCapacity(_zoneBuffer.Count);

            for (var i = 0; i < _zoneEntries.Count; i++)
            {
                var entry = _zoneEntries[i];
                if (i >= _zoneBuffer.Count)
                {
                    entry.Root.style.display = DisplayStyle.None;
                    continue;
                }

                var zone = _zoneBuffer[i];
                entry.Root.style.display = DisplayStyle.Flex;
                entry.ZoneId = zone.Zone.Id;
                entry.Title.text = $"{zone.Zone.Name} ({zone.Zone.Id})";
                entry.Stats.text = string.Format(CultureInfo.InvariantCulture, "Morale {0:0.00}  Wear {1:0.00}", zone.MoraleModifier, zone.Wear);
                entry.Workforce.text = string.Format(CultureInfo.InvariantCulture, "Workforce {0:0.00}", zone.WorkforceAllocation);
            }
        }

        private void EnsureZoneEntryCapacity(int desired)
        {
            if (_zoneList == null)
            {
                return;
            }

            while (_zoneEntries.Count < desired)
            {
                var entry = CreateZoneEntry();
                _zoneEntries.Add(entry);
                _zoneList.contentContainer.Add(entry.Root);
            }
        }

        private ZoneEntry CreateZoneEntry()
        {
            var container = new VisualElement();
            container.style.flexDirection = FlexDirection.Column;
            container.style.marginBottom = 6;
            container.style.paddingLeft = 6;
            container.style.paddingRight = 6;
            container.style.paddingTop = 4;
            container.style.paddingBottom = 4;
            container.style.borderTopWidth = 1;
            container.style.borderBottomWidth = 1;
            container.style.borderLeftWidth = 1;
            container.style.borderRightWidth = 1;
            container.style.borderTopColor = new Color(0.25f, 0.25f, 0.25f, 1f);
            container.style.borderBottomColor = new Color(0.25f, 0.25f, 0.25f, 1f);
            container.style.borderLeftColor = new Color(0.25f, 0.25f, 0.25f, 1f);
            container.style.borderRightColor = new Color(0.25f, 0.25f, 0.25f, 1f);

            var title = new Label();
            title.style.unityFontStyleAndWeight = FontStyle.Bold;
            var stats = new Label();
            var workforce = new Label();

            var buttonRow = new VisualElement();
            buttonRow.style.flexDirection = FlexDirection.Row;
            buttonRow.style.marginTop = 4;

            var boostButton = new Button { text = "Boost Morale" };
            var inspectionButton = new Button { text = "Schedule Inspection" };
            inspectionButton.style.marginLeft = 4;

            var entry = new ZoneEntry(container, title, stats, workforce, boostButton, inspectionButton);
            boostButton.clicked += () => IssueCommand("zone.adjust_morale", entry.ZoneId, "delta", "0.1");
            inspectionButton.clicked += () => IssueCommand("zone.schedule_inspection", entry.ZoneId);

            container.Add(title);
            container.Add(stats);
            container.Add(workforce);
            buttonRow.Add(boostButton);
            buttonRow.Add(inspectionButton);
            container.Add(buttonRow);

            return entry;
        }

        private void UpdateJobs()
        {
            if (_jobList == null || _runtime == null)
            {
                return;
            }

            _jobBuffer.Clear();
            _jobBuffer.AddRange(_runtime.JobBoard.Jobs.Take(5));
            EnsureJobEntryCapacity(_jobBuffer.Count);

            for (var i = 0; i < _jobEntries.Count; i++)
            {
                var entry = _jobEntries[i];
                if (i >= _jobBuffer.Count)
                {
                    entry.Root.style.display = DisplayStyle.None;
                    continue;
                }

                var job = _jobBuffer[i];
                entry.Root.style.display = DisplayStyle.Flex;
                entry.JobId = job.Id;
                entry.Description.text = $"{job.Id} ({job.Type})";
                entry.Eta.text = string.Format(CultureInfo.InvariantCulture, "ETA: {0}h", job.RemainingHours);
            }
        }

        private void EnsureJobEntryCapacity(int desired)
        {
            if (_jobList == null)
            {
                return;
            }

            while (_jobEntries.Count < desired)
            {
                var entry = CreateJobEntry();
                _jobEntries.Add(entry);
                _jobList.contentContainer.Add(entry.Root);
            }
        }

        private JobEntry CreateJobEntry()
        {
            var container = new VisualElement();
            container.style.flexDirection = FlexDirection.Row;
            container.style.alignItems = Align.Center;
            container.style.justifyContent = Justify.SpaceBetween;
            container.style.marginBottom = 4;
            container.style.paddingLeft = 4;
            container.style.paddingRight = 4;

            var description = new Label();
            description.style.flexGrow = 1f;
            var eta = new Label();
            eta.style.width = 80;
            eta.style.unityTextAlign = TextAnchor.MiddleRight;

            var rushButton = new Button { text = "Rush" };
            var entry = new JobEntry(container, description, eta, rushButton);
            rushButton.clicked += () => IssueCommand("job.rush", entry.JobId, "duration", "-1");

            container.Add(description);
            container.Add(eta);
            container.Add(rushButton);
            return entry;
        }

        private void UpdateMandates()
        {
            if (_mandateList == null || _runtime == null)
            {
                return;
            }

            _mandateBuffer.Clear();
            _mandateBuffer.AddRange(_runtime.MandateTracker.Mandates.Take(5));
            EnsureMandateEntryCapacity(_mandateBuffer.Count);

            for (var i = 0; i < _mandateEntries.Count; i++)
            {
                var entry = _mandateEntries[i];
                if (i >= _mandateBuffer.Count)
                {
                    entry.Root.style.display = DisplayStyle.None;
                    continue;
                }

                var mandate = _mandateBuffer[i];
                entry.Root.style.display = DisplayStyle.Flex;
                entry.MandateId = mandate.Id;
                entry.Description.text = $"{mandate.Id} [{mandate.Status}]";
            }
        }

        private void EnsureMandateEntryCapacity(int desired)
        {
            if (_mandateList == null)
            {
                return;
            }

            while (_mandateEntries.Count < desired)
            {
                var entry = CreateMandateEntry();
                _mandateEntries.Add(entry);
                _mandateList.contentContainer.Add(entry.Root);
            }
        }

        private MandateEntry CreateMandateEntry()
        {
            var container = new VisualElement();
            container.style.flexDirection = FlexDirection.Row;
            container.style.alignItems = Align.Center;
            container.style.justifyContent = Justify.SpaceBetween;
            container.style.marginBottom = 4;
            container.style.paddingLeft = 4;
            container.style.paddingRight = 4;

            var description = new Label();
            description.style.flexGrow = 1f;
            var acknowledgeButton = new Button { text = "Acknowledge" };
            var entry = new MandateEntry(container, description, acknowledgeButton);
            acknowledgeButton.clicked += () => IssueCommand("mandate.acknowledge", entry.MandateId);

            container.Add(description);
            container.Add(acknowledgeButton);
            return entry;
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

        private sealed class ZoneEntry
        {
            public ZoneEntry(VisualElement root, Label title, Label stats, Label workforce, Button boostButton, Button inspectionButton)
            {
                Root = root;
                Title = title;
                Stats = stats;
                Workforce = workforce;
                BoostButton = boostButton;
                InspectionButton = inspectionButton;
                ZoneId = string.Empty;
            }

            public VisualElement Root { get; }
            public Label Title { get; }
            public Label Stats { get; }
            public Label Workforce { get; }
            public Button BoostButton { get; }
            public Button InspectionButton { get; }
            public string ZoneId { get; set; }
        }

        private sealed class JobEntry
        {
            public JobEntry(VisualElement root, Label description, Label eta, Button rushButton)
            {
                Root = root;
                Description = description;
                Eta = eta;
                RushButton = rushButton;
                JobId = string.Empty;
            }

            public VisualElement Root { get; }
            public Label Description { get; }
            public Label Eta { get; }
            public Button RushButton { get; }
            public string JobId { get; set; }
        }

        private sealed class MandateEntry
        {
            public MandateEntry(VisualElement root, Label description, Button acknowledgeButton)
            {
                Root = root;
                Description = description;
                AcknowledgeButton = acknowledgeButton;
                MandateId = string.Empty;
            }

            public VisualElement Root { get; }
            public Label Description { get; }
            public Button AcknowledgeButton { get; }
            public string MandateId { get; set; }
        }
    }
}
