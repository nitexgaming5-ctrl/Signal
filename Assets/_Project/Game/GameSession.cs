using System;
using System.Collections.Generic;
using Signal.Core;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Signal.Game
{
    /// <summary>
    /// Composition root for the first connected urban slice. The same player and session
    /// travel between the hideout, street and basic interiors.
    /// </summary>
    public sealed class GameSession : MonoBehaviour
    {
        [Header("Content")]
        [SerializeField] private ItemDefinition[] items;
        [SerializeField] private ItemDefinition parcelItem;
        [SerializeField] private NpcCharacterDefinition[] npcDefinitions;

        [Header("Placeholder tuning (not final design)")]
        [SerializeField] private int startingMoney;
        [SerializeField] private int rentAmount = 50;
        [SerializeField] private int rentEveryDays = 7;
        [SerializeField] private float gameMinutesPerRealSecond = 1f;

        private readonly Dictionary<Guid, ItemType> _catalog = new Dictionary<Guid, ItemType>();
        private readonly List<ItemDefinition> _allItemDefinitions = new List<ItemDefinition>();

        private PlayerStats _stats;
        private GridInventory _inventory;
        private GridInventory _storage;
        private ItemKnowledgeBook _itemKnowledge;
        private PlayerDiscoveryLedger _playerDiscoveries;
        private OpportunityState _miloOpportunity;
        private PlayerRelationshipBook _playerRelationships;
        private GameClock _clock;
        private HeatMeter _heat;
        private RentLedger _rent;

        private RoomBootstrap _room;
        private PlayerMotor2D _motor;
        private InventoryPanel _panel;
        private ItemViewerPanel _itemViewer;
        private StoragePanel _storagePanel;
        private HudPanel _hud;
        private WorkbenchWorkspace _workspace;
        private CityBlockBootstrap _city;
        private MiloParcelOpportunityController _miloScenario;

        private float _minuteBuffer;
        private bool _debugMenuOpen;
        private bool _debugClockPaused;

        /// <summary>Player-scoped discovered item facts, shared by inventory and scenario interactions.</summary>
        public ItemKnowledgeBook PlayerItemKnowledge => _itemKnowledge;
        public PlayerDiscoveryLedger PlayerDiscoveries => _playerDiscoveries;
        public OpportunityState MiloOpportunity => _miloOpportunity;
        public PlayerRelationshipBook PlayerRelationships => _playerRelationships;

        private void Start()
        {
            _stats = new PlayerStats(startingMoney);
            _inventory = new GridInventory();
            _storage = new GridInventory(8, 6);
            _itemKnowledge = new ItemKnowledgeBook();
            _playerDiscoveries = new PlayerDiscoveryLedger();
            _miloOpportunity = new OpportunityState("milo-parcel");
            _playerRelationships = new PlayerRelationshipBook();
            _clock = new GameClock();
            _heat = new HeatMeter();
            _rent = new RentLedger(rentAmount, rentEveryDays);
            _clock.DayStarted += OnDayStarted;

            BuildCatalog();
            GiveStartingItems();

            Camera camera = Camera.main;
            if (camera == null)
                camera = CreateCamera();

            var roomObject = new GameObject("Room");
            roomObject.transform.SetParent(transform, false);
            _room = roomObject.AddComponent<RoomBootstrap>();
            _room.Build(camera);
            _motor = _room.Player.GetComponent<PlayerMotor2D>();

            _city = gameObject.AddComponent<CityBlockBootstrap>();
            _city.Build(_room.Player, _room.PlayerBody, camera, _clock, npcDefinitions);

            _itemViewer = gameObject.AddComponent<ItemViewerPanel>();
            _itemViewer.Bind(_allItemDefinitions, _itemKnowledge);
            _panel = gameObject.AddComponent<InventoryPanel>();
            _panel.Bind(_inventory, _allItemDefinitions, _itemViewer.Open);
            _storagePanel = gameObject.AddComponent<StoragePanel>();
            _storagePanel.Bind(_inventory, _storage, _allItemDefinitions, _itemViewer.Open, _itemKnowledge);

            _hud = gameObject.AddComponent<HudPanel>();
            _hud.Bind(_stats, _clock);

            if (parcelItem != null)
            {
                _miloScenario = gameObject.AddComponent<MiloParcelOpportunityController>();
                _miloScenario.Bind(_inventory, parcelItem.ToItemType(),
                    _playerDiscoveries, _playerRelationships, _itemKnowledge, _clock, _city,
                    _miloOpportunity, message => Say(message, 8f));
            }

            _workspace = gameObject.AddComponent<WorkbenchWorkspace>();
            _workspace.Bind(_motor);
            _hud.ShowMessage("Your hideout is ready. The street is just outside.", 5f);
        }

        private void OnDestroy()
        {
            if (_clock != null)
                _clock.DayStarted -= OnDayStarted;
        }

        private void Update()
        {
            if (_stats == null)
                return;

            Keyboard keyboard = Keyboard.current;
            if (keyboard != null && keyboard.f1Key.wasPressedThisFrame)
                _debugMenuOpen = !_debugMenuOpen;

            bool workspaceOpen = _workspace != null && _workspace.IsOpen;
            bool inventoryOpen = _panel != null && _panel.IsOpen;
            bool storageOpen = _storagePanel != null && _storagePanel.IsOpen;
            bool itemViewerOpen = _itemViewer != null && _itemViewer.IsOpen;
            if (_panel != null)
                _panel.enabled = !_debugMenuOpen && !workspaceOpen && !storageOpen && !itemViewerOpen;
            if (_storagePanel != null)
                _storagePanel.enabled = !_debugMenuOpen && !itemViewerOpen;

            bool scenarioChoiceOpen = _miloScenario != null && _miloScenario.HasPendingChoice;
            _motor.enabled = !inventoryOpen && !workspaceOpen && !storageOpen && !itemViewerOpen &&
                !scenarioChoiceOpen && !_debugMenuOpen;

            Vector2 playerPosition = _room.Player.position;
            bool nearWorkbench = _room.Workbench != null &&
                Vector2.Distance(playerPosition, _room.Workbench.position) <= 2.15f;
            bool nearStorage = _city.IsAtHome && _room.Storage != null &&
                Vector2.Distance(playerPosition, _room.Storage.position) <= 2.15f;
            string cityPrompt = _city.GetPrompt(playerPosition);
            if (_miloScenario != null)
            {
                if (!workspaceOpen && !inventoryOpen && !storageOpen && !itemViewerOpen)
                    _miloScenario.Tick(playerPosition, _city.CurrentPlace);
                string scenarioPrompt = _miloScenario.GetPrompt(playerPosition, _city.CurrentPlace);
                if (!string.IsNullOrEmpty(scenarioPrompt))
                    cityPrompt = scenarioPrompt;
            }

            if (workspaceOpen)
                _hud.SetPrompt("ESC  Back to room");
            else if (itemViewerOpen)
                _hud.SetPrompt("ESC  Close item inspection");
            else if (storageOpen)
                _hud.SetPrompt("ESC  Close storage");
            else if (inventoryOpen)
                _hud.SetPrompt(string.Empty);
            else if (nearWorkbench)
                _hud.SetPrompt("E  Use workbench");
            else if (nearStorage)
                _hud.SetPrompt("E  Open home storage");
            else
                _hud.SetPrompt(cityPrompt);

            TickClock();

            if (_debugMenuOpen || keyboard == null)
                return;

            if (!workspaceOpen && !inventoryOpen && !storageOpen && !itemViewerOpen &&
                _miloScenario != null && _miloScenario.HasPendingChoice)
            {
                int choice = keyboard.digit1Key.wasPressedThisFrame ? 1 :
                    keyboard.digit2Key.wasPressedThisFrame ? 2 :
                    keyboard.digit3Key.wasPressedThisFrame ? 3 :
                    keyboard.digit4Key.wasPressedThisFrame ? 4 : 0;
                if (choice > 0 && _miloScenario.HandleChoice(choice, out string choiceMessage))
                {
                    Say(choiceMessage, 8f);
                    return;
                }
            }

            if (!workspaceOpen && !inventoryOpen && !storageOpen && !itemViewerOpen && nearWorkbench && keyboard.eKey.wasPressedThisFrame)
                _workspace.Open();
            else if (!workspaceOpen && !inventoryOpen && !storageOpen && !itemViewerOpen && nearStorage && keyboard.eKey.wasPressedThisFrame)
                _storagePanel.SetOpen(true);
            else if (!workspaceOpen && !inventoryOpen && !storageOpen && !itemViewerOpen && !nearWorkbench && !nearStorage &&
                     cityPrompt.Length > 0 && keyboard.eKey.wasPressedThisFrame)
            {
                if (_miloScenario != null &&
                    _miloScenario.TryInteract(playerPosition, _city.CurrentPlace, out string scenarioMessage))
                    Say(scenarioMessage, 8f);
                else if (_city.TryInteractNpc(playerPosition, out string npcDialogue))
                    Say(npcDialogue, 5f);
                else if (_city.TryInteract(playerPosition))
                    Say("Location: " + _city.CurrentPlace);
            }

            if (keyboard.f6Key.wasPressedThisFrame) Sleep();
            if (keyboard.f5Key.wasPressedThisFrame) SaveGame();
            if (keyboard.f9Key.wasPressedThisFrame) LoadGame();
        }

        private void TickClock()
        {
            if (_debugClockPaused)
                return;

            _minuteBuffer += Time.deltaTime * gameMinutesPerRealSecond;

            int whole = (int)_minuteBuffer;
            if (whole > 0)
            {
                _minuteBuffer -= whole;
                _clock.Advance(whole);
            }
        }


        private void OnGUI()
        {
            if (!_debugMenuOpen || _city == null)
                return;

            const float width = 390f;
            GUILayout.BeginArea(new Rect(12f, 12f, width, Screen.height - 24f), GUI.skin.box);
            GUILayout.Label("SIGNAL — DEVELOPER DEBUG  [F1]");
            GUILayout.Label("Clock: " + _clock.Label + (_debugClockPaused ? "  [PAUSED]" : ""));
            GUILayout.Label("Location: " + _city.CurrentPlace);
            GUILayout.Label("NPC simulation: " + (_city.DebugSimulationPaused ? "PAUSED" : "RUNNING"));
            GUILayout.Label(_city.DebugResidentSummary("milo"));
            GUILayout.Label(_city.DebugResidentSummary("rhea"));
            GUILayout.Label("Milo opportunity: " + (_miloOpportunity != null ? _miloOpportunity.Phase.ToString() : "Unavailable"));
            GUILayout.Label("Discovery signals: " + (_playerDiscoveries != null ? _playerDiscoveries.Count.ToString() : "0") +
                "   Familiarity: " + (_playerRelationships != null ? _playerRelationships.Get("milo").Familiarity.ToString("0.00") : "0.00"));
            GUILayout.Space(8f);

            if (GUILayout.Button(_city.DebugSimulationPaused ? "Resume all NPC schedules" : "Pause all NPC schedules"))
                _city.DebugSetNpcSimulationPaused(!_city.DebugSimulationPaused);
            if (GUILayout.Button(_debugClockPaused ? "Resume game clock" : "Pause game clock"))
                _debugClockPaused = !_debugClockPaused;

            GUILayout.Space(8f);
            GUILayout.Label("MILO — POSITION / ACTIVITY");
            if (GUILayout.Button("Focus Milo (current activity)"))
                DebugFocusMilo();
            if (GUILayout.Button("Force Milo: Socialize (street) + focus"))
                DebugForceMilo(NpcActivity.Socialize);
            if (GUILayout.Button("Force Milo: Work (shop) + focus"))
                DebugForceMilo(NpcActivity.Work);
            if (GUILayout.Button("Force Milo: Rest (street) + focus"))
                DebugForceMilo(NpcActivity.Rest);
            if (GUILayout.Button("Force Milo: Home (apartment) + focus"))
                DebugForceMilo(NpcActivity.Home);
            if (GUILayout.Button("Return Milo to authored schedule"))
            {
                _city.DebugResumeResidentSchedule("milo");
                Say("Milo returned to his authored schedule.", 4f);
            }

            GUILayout.Space(8f);
            GUILayout.Label("OPPORTUNITY 001 — TEST CONTROLS");
            if (GUILayout.Button("TRIGGER MILO OFFER NOW (bypass discovery gates)"))
                DebugTriggerMiloOffer();

            GUILayout.Space(8f);
            GUILayout.Label("F1 closes this menu. Debug controls do not represent player abilities.");
            GUILayout.EndArea();
        }

        private void DebugFocusMilo()
        {
            if (_city.DebugFocusResident("milo"))
            {
                _debugMenuOpen = false;
                Say("Focused Milo. NPC schedule state is unchanged.", 4f);
            }
            else
            {
                Say("Milo is unavailable.", 4f);
            }
        }

        private void DebugForceMilo(NpcActivity activity)
        {
            if (!_city.DebugForceResidentActivity("milo", activity) || !_city.DebugFocusResident("milo"))
            {
                Say("Could not move Milo to that debug activity.", 4f);
                return;
            }

            _debugMenuOpen = false;
            Say("DEBUG: Milo forced to " + activity + ". Press F1 to reopen the menu.", 5f);
        }

        private void DebugTriggerMiloOffer()
        {
            if (_miloScenario == null)
            {
                Say("Milo scenario is unavailable. Check the parcel ItemDefinition on GameSession.", 6f);
                return;
            }

            if (!_city.DebugSimulationPaused)
                _city.DebugSetNpcSimulationPaused(true);
            _city.DebugForceResidentActivity("milo", NpcActivity.Socialize);
            _city.DebugFocusResident("milo");

            if (_miloScenario.DebugTriggerOffer(out string message))
            {
                _debugMenuOpen = false;
                Say(message, 12f);
            }
            else
            {
                Say(message, 6f);
            }
        }

        private void OnDayStarted(GameClock clock)
        {
            _heat.NewDay();

            string message;
            if (_rent.Amount <= 0)
            {
                message = "Day " + clock.Day + " begins";
            }
            else
            {
                switch (_rent.Settle(clock.Day, _stats))
                {
                    case RentOutcome.Paid:
                        message = "Day " + clock.Day + ": rent paid (" + _rent.Amount + ")";
                        break;
                    case RentOutcome.Missed:
                        message = "Day " + clock.Day + ": rent MISSED (" + _rent.MissedPayments + " so far)";
                        break;
                    default:
                        message = "Day " + clock.Day + " begins";
                        break;
                }
            }

            Say(message);
        }

        private void Sleep()
        {
            if (_city != null && !_city.IsAtHome)
            {
                Say("Return to your hideout to sleep.");
                return;
            }

            int dayBefore = _clock.Day;
            _clock.Sleep();

            if (_clock.Day == dayBefore)
                Say("You slept. It is " + _clock.Label);
        }

        private void SaveGame()
        {
            Vector2 position = _room.PlayerBody.position;
            SaveData data = SaveMapper.Capture(_stats, _inventory, _storage, _clock, _heat, _rent,
                                               _city.CurrentPlace, position.x, position.y, _itemKnowledge,
                                               _playerDiscoveries, _miloOpportunity, _playerRelationships);

            try
            {
                SaveFileStore.Save(data, SaveFileStore.DefaultPath);
                Say("Saved.");
            }
            catch (Exception exception)
            {
                Debug.LogError("Save failed: " + exception);
                Say("Save failed: " + exception.Message);
            }
        }

        private void LoadGame()
        {
            if (!SaveFileStore.TryLoad(SaveFileStore.DefaultPath, out SaveData data, out string error))
            {
                Say(error);
                return;
            }

            LoadReport report = SaveMapper.Apply(data, _stats, _inventory, _storage, _clock, _heat, _rent, ResolveType, _itemKnowledge, _playerDiscoveries, _playerRelationships);
            if (!report.Success)
            {
                Say("Load failed: " + report.Error);
                return;
            }

            _miloOpportunity = report.RestoredOpportunity != null &&
                report.RestoredOpportunity.OpportunityId == "milo-parcel"
                ? report.RestoredOpportunity
                : new OpportunityState("milo-parcel");
            if (_miloScenario != null)
                _miloScenario.RestoreOpportunityState(_miloOpportunity);

            var position = new Vector2(data.playerX, data.playerY);
            if (!_city.TryRestorePlace(data.worldPlace, position))
            {
                Say("Loaded inventory, but the saved location is unknown. Returned to the hideout.");
                _city.TryRestorePlace("Hideout", new Vector2(10f, 5f));
                return;
            }

            Say(report.SkippedItems + report.SkippedStorageItems + report.SkippedKnowledgeFacts +
                report.SkippedDiscoverySignals + report.SkippedPlayerRelationships > 0
                ? "Loaded at " + data.worldPlace + ". Some saved item or discovery details could not be restored."
                : "Loaded at " + data.worldPlace + ".");
        }

        private void BuildCatalog()
        {
            _catalog.Clear();
            _allItemDefinitions.Clear();

            if (items != null)
            {
                foreach (ItemDefinition definition in items)
                {
                    if (definition == null)
                        continue;

                    ItemType type = definition.ToItemType();
                    _catalog[type.Id] = type;
                    _allItemDefinitions.Add(definition);
                }
            }

            if (parcelItem != null)
            {
                ItemType parcelType = parcelItem.ToItemType();
                _catalog[parcelType.Id] = parcelType;
                if (!_allItemDefinitions.Contains(parcelItem))
                    _allItemDefinitions.Add(parcelItem);
            }
            else
            {
                Debug.LogError("GameSession: assign the Milo parcel ItemDefinition before playing Opportunity 001.");
            }
        }

        private void GiveStartingItems()
        {
            if (items == null)
                return;

            foreach (ItemDefinition definition in items)
            {
                if (definition == null)
                    continue;

                ItemType type = definition.ToItemType();
                _inventory.Add(type, type.MaxStack, definition.DefaultInstanceState);
            }
        }

        private ItemType? ResolveType(Guid id)
        {
            return _catalog.TryGetValue(id, out ItemType type) ? type : (ItemType?)null;
        }

        private void Say(string text, float seconds = 3f)
        {
            if (_hud != null)
                _hud.ShowMessage(text, seconds);
        }

        private static Camera CreateCamera()
        {
            var cameraObject = new GameObject("Main Camera");
            cameraObject.tag = "MainCamera";
            cameraObject.transform.position = new Vector3(0f, 0f, -10f);

            var camera = cameraObject.AddComponent<Camera>();
            camera.orthographic = true;
            return camera;
        }
    }
}