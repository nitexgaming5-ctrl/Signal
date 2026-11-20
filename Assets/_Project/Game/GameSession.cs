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
        [SerializeField] private DirtyCashContactOfferDefinition dirtyCashContactOffer;
        [SerializeField] private NpcCharacterDefinition[] npcDefinitions;
        [SerializeField] private BusinessDefinition[] businesses;
        [SerializeField] private ContainerLootProfile[] searchableLootProfiles;
        [SerializeField] private ResidentPropertyContainerDefinition[] residentPropertyContainers;

        [Header("Placeholder tuning (not final design)")]
        [SerializeField] private int startingMoney;
        [SerializeField] private int rentAmount = 50;
        [SerializeField] private int rentEveryDays = 7;
        [SerializeField] private float gameMinutesPerRealSecond = 1f;

        private readonly Dictionary<Guid, ItemType> _catalog = new Dictionary<Guid, ItemType>();
        private readonly List<ItemDefinition> _allItemDefinitions = new List<ItemDefinition>();
        private readonly List<SearchableContainer> _searchableContainers = new List<SearchableContainer>();
        private readonly Dictionary<string, ContainerLootProfile> _searchableProfiles = new Dictionary<string, ContainerLootProfile>(StringComparer.Ordinal);
        private readonly Dictionary<string, EconomicActorId> _searchableActors = new Dictionary<string, EconomicActorId>(StringComparer.Ordinal);
        private readonly Dictionary<string, ResidentPropertyContainerDefinition> _residentPropertyDefinitions =
            new Dictionary<string, ResidentPropertyContainerDefinition>(StringComparer.Ordinal);

        private PlayerStats _stats;
        private AssetOwnershipBook _assetOwnership;
        private EconomicTransactionLedger _economicTransactions;
        private EconomicContainerRegistry _economicContainers;
        private AssetTransferService _assetTransfers;
        private EconomicAccountBook _economicAccounts;
        private RetailTransactionLedger _retailTransactions;
        private RetailService _retailService;
        private ShopSaleTransactionLedger _shopSaleTransactions;
        private IllicitExchangeTransactionLedger _illicitExchanges;
        private DirtyCashCleaningTransactionLedger _dirtyCashCleanings;
        private GridInventory _contactInventory;
        private GridInventory _inventory;
        private GridInventory _storage;
        private ItemKnowledgeBook _itemKnowledge;
        private PlayerDiscoveryLedger _playerDiscoveries;
        private OpportunityState _miloOpportunity;
        private PlayerRelationshipBook _playerRelationships;
        private GameClock _clock;
        private HeatMeter _heat;
        private CriminalProgressionTrack _criminalProgression;
        private CrimeOutcomeLedger _crimeOutcomes;
        private RentLedger _rent;

        private RoomBootstrap _room;
        private PlayerMotor2D _motor;
        private InventoryPanel _panel;
        private ItemViewerPanel _itemViewer;
        private StoragePanel _storagePanel;
        private HudPanel _hud;
        private RetailPanel _retailPanel;
        private DirtyCashContactPanel _dirtyCashPanel;
        private SearchableContainerPanel _containerPanel;
        private SearchableContainer _activeSearchableContainer;
        private WorkbenchWorkspace _workspace;
        private CityBlockBootstrap _city;
        private MiloParcelOpportunityController _miloScenario;

        private float _minuteBuffer;
        private bool _debugMenuOpen;
        
        /// <summary>Player-scoped discovered item facts, shared by inventory and scenario interactions.</summary>
        public ItemKnowledgeBook PlayerItemKnowledge => _itemKnowledge;
        public PlayerDiscoveryLedger PlayerDiscoveries => _playerDiscoveries;
        public OpportunityState MiloOpportunity => _miloOpportunity;
        public PlayerRelationshipBook PlayerRelationships => _playerRelationships;
        public CleanMoneyAccount CleanMoney => _stats != null ? _stats.CleanMoney : null;
        public AssetOwnershipBook AssetOwnership => _assetOwnership;
        public EconomicTransactionLedger EconomicTransactions => _economicTransactions;
        public AssetTransferService AssetTransfers => _assetTransfers;
        public EconomicAccountBook EconomicAccounts => _economicAccounts;
        public RetailTransactionLedger RetailTransactions => _retailTransactions;
        public RetailService Retail => _retailService;
        public ShopSaleTransactionLedger ShopSaleTransactions => _shopSaleTransactions;
        public IllicitExchangeTransactionLedger IllicitExchanges => _illicitExchanges;
        public DirtyCashCleaningTransactionLedger DirtyCashCleanings => _dirtyCashCleanings;
        public CriminalProgressionTrack CriminalProgression => _criminalProgression;
        public CrimeOutcomeLedger CrimeOutcomes => _crimeOutcomes;

        private void Start()
        {
            _stats = new PlayerStats(startingMoney);
            _inventory = new GridInventory();
            _storage = new GridInventory(8, 6);
            _assetOwnership = new AssetOwnershipBook();
            _economicTransactions = new EconomicTransactionLedger();
            _economicContainers = new EconomicContainerRegistry();
            _economicContainers.TryRegister(_inventory, EconomicActorId.Player);
            _economicContainers.TryRegister(_storage, EconomicActorId.Player);
            _assetTransfers = new AssetTransferService(_assetOwnership, _economicContainers, _economicTransactions);
            _economicAccounts = new EconomicAccountBook();
            _economicAccounts.TryRegister(EconomicActorId.Player, _stats.CleanMoney);
            _retailTransactions = new RetailTransactionLedger();
            _retailService = new RetailService(_economicAccounts, _assetOwnership, _retailTransactions);
            _shopSaleTransactions = new ShopSaleTransactionLedger();
            _illicitExchanges = new IllicitExchangeTransactionLedger();
            _dirtyCashCleanings = new DirtyCashCleaningTransactionLedger();
            _contactInventory = new GridInventory(8, 6);
            _economicContainers.TryRegister(_contactInventory, new EconomicActorId("resident:Clean_Dirtycash"));
            _itemKnowledge = new ItemKnowledgeBook();
            _playerDiscoveries = new PlayerDiscoveryLedger();
            _miloOpportunity = new OpportunityState("milo-parcel");
            _playerRelationships = new PlayerRelationshipBook();
            _clock = new GameClock();
            _heat = new HeatMeter();
            _criminalProgression = new CriminalProgressionTrack();
            _crimeOutcomes = new CrimeOutcomeLedger();
            _rent = new RentLedger(rentAmount, rentEveryDays);
            _clock.DayStarted += OnDayStarted;

            BuildCatalog();
            GiveStartingItems();
            RegisterUnownedPlayerAssets(_inventory);

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
            if (dirtyCashContactOffer == null)
                Debug.LogError("GameSession: assign the Slice B Dirty Cash contact offer.");
            else if (!dirtyCashContactOffer.TryValidate(out string offerError))
                Debug.LogError("GameSession: invalid Dirty Cash contact offer: " + offerError);
            else if (!_city.TryAddResident(dirtyCashContactOffer.Contact))
                Debug.LogError("GameSession: the Clean_Dirtycash resident could not be added to the city.");
            CreateSearchableContainers();
            foreach (NpcPresenter resident in _city.DebugResidents)
            {
                if (resident == null || string.IsNullOrWhiteSpace(resident.NpcId))
                    continue;
                _economicAccounts.GetOrCreate(new EconomicActorId("resident:" + resident.NpcId),
                    resident.Definition != null ? resident.Definition.StartingCleanBalance : 0);
            }

            _itemViewer = gameObject.AddComponent<ItemViewerPanel>();
            _itemViewer.Bind(_allItemDefinitions, _itemKnowledge);
            _panel = gameObject.AddComponent<InventoryPanel>();
            _panel.Bind(_inventory, _allItemDefinitions, _itemViewer.Open);
            _containerPanel = gameObject.AddComponent<SearchableContainerPanel>();
            _containerPanel.Bind(_allItemDefinitions, TakeSearchableItem, TakeSearchableItemAt, _itemViewer.Open);
            _storagePanel = gameObject.AddComponent<StoragePanel>();
            _storagePanel.Bind(_inventory, _storage, _allItemDefinitions, _itemViewer.Open, _itemKnowledge);

            _hud = gameObject.AddComponent<HudPanel>();
            _hud.Bind(_stats, _clock);
            _retailPanel = gameObject.AddComponent<RetailPanel>();
            _retailPanel.Bind(PurchaseFromBusiness, SellToBusiness, _inventory, _allItemDefinitions,
                _assetOwnership, () => { });
            if (dirtyCashContactOffer != null && dirtyCashContactOffer.TryValidate(out _))
            {
                _dirtyCashPanel = gameObject.AddComponent<DirtyCashContactPanel>();
                _dirtyCashPanel.Bind(_inventory, _contactInventory, _assetOwnership, _allItemDefinitions,
                    dirtyCashContactOffer.DirtyCashItem, dirtyCashContactOffer.AcceptedItem,
                    dirtyCashContactOffer.AcceptedQuantity, dirtyCashContactOffer.DirtyCashUnits,
                    dirtyCashContactOffer.CleaningFeePercent, ExchangeWithContact, CleanDirtyCash);
            }
            else
            {
                Debug.LogError("GameSession: the Dirty Cash contact panel cannot be created without a valid offer.");
            }

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
            bool retailOpen = _retailPanel != null && _retailPanel.IsOpen;
            bool containerOpen = _containerPanel != null && _containerPanel.IsOpen;
            bool dirtyCashOpen = _dirtyCashPanel != null && _dirtyCashPanel.IsOpen;
            if (_panel != null)
                _panel.enabled = !_debugMenuOpen && !workspaceOpen && !storageOpen && !itemViewerOpen && !retailOpen && !containerOpen && !dirtyCashOpen;
            if (_storagePanel != null)
                _storagePanel.enabled = !_debugMenuOpen && !itemViewerOpen && !retailOpen && !containerOpen && !dirtyCashOpen;
            if (_containerPanel != null)
                _containerPanel.enabled = !_debugMenuOpen && !itemViewerOpen && !dirtyCashOpen;
            if (_dirtyCashPanel != null)
                _dirtyCashPanel.enabled = !_debugMenuOpen;
            if (_debugMenuOpen && retailOpen)
            {
                _retailPanel.Close();
                retailOpen = false;
            }
            if (_debugMenuOpen && dirtyCashOpen)
            {
                _dirtyCashPanel.Close();
                dirtyCashOpen = false;
            }

            bool scenarioChoiceOpen = _miloScenario != null && _miloScenario.HasPendingChoice;
            _motor.enabled = !inventoryOpen && !workspaceOpen && !storageOpen && !itemViewerOpen &&
                !scenarioChoiceOpen && !retailOpen && !containerOpen && !dirtyCashOpen && !_debugMenuOpen;

            Vector2 playerPosition = _room.Player.position;
            bool nearWorkbench = _room.Workbench != null &&
                Vector2.Distance(playerPosition, _room.Workbench.position) <= 2.15f;
            bool nearStorage = _city.IsAtHome && _room.Storage != null &&
                Vector2.Distance(playerPosition, _room.Storage.position) <= 2.15f;
            BusinessDefinition nearbyBusiness = FindNearbyBusiness(playerPosition);
            bool nearDirtyCashContact = _city.IsResidentWithin("Clean_Dirtycash", playerPosition, 1.85f);
            bool nearSearchableContainer = _city.TryGetNearbySearchableContainer(playerPosition,
                out string nearbyContainerId, out string nearbyContainerLabel);
            ResidentPropertyContainerDefinition nearbyProperty = FindNearbyResidentProperty(playerPosition);
            if (nearbyProperty != null)
            {
                nearSearchableContainer = true;
                nearbyContainerId = nearbyProperty.ContainerId;
                nearbyContainerLabel = nearbyProperty.DisplayName;
            }
            string cityPrompt = _city.GetPrompt(playerPosition);
            if (nearSearchableContainer)
                cityPrompt = "E  Search " + nearbyContainerLabel;
            if (nearDirtyCashContact)
                cityPrompt = "E  Talk";
            if (_miloScenario != null)
            {
                if (!workspaceOpen && !inventoryOpen && !storageOpen && !itemViewerOpen && !retailOpen && !containerOpen && !dirtyCashOpen)
                    _miloScenario.Tick(playerPosition, _city.CurrentPlace);
                string scenarioPrompt = _miloScenario.GetPrompt(playerPosition, _city.CurrentPlace);
                if (!string.IsNullOrEmpty(scenarioPrompt))
                    cityPrompt = scenarioPrompt;
            }

            if (nearbyBusiness != null && !nearSearchableContainer)
                cityPrompt = nearbyBusiness.IsOpen(_clock.MinuteOfDay)
                    ? "E  Browse " + nearbyBusiness.DisplayName
                    : nearbyBusiness.DisplayName + "  —  Closed";
            if (nearSearchableContainer)
                cityPrompt = "E  Search " + nearbyContainerLabel;
            if (nearDirtyCashContact)
                cityPrompt = "E  Talk";
            if (dirtyCashOpen)
                _hud.SetPrompt("ESC  Close contact");
            else if (itemViewerOpen)
                _hud.SetPrompt("ESC  Close item inspection");
            else if (containerOpen)
                _hud.SetPrompt("ESC  Close container");
            else if (retailOpen)
                _hud.SetPrompt("ESC  Close shop");
            else if (workspaceOpen)
                _hud.SetPrompt("ESC  Back to room");
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

            if (!workspaceOpen && !inventoryOpen && !storageOpen && !itemViewerOpen && !retailOpen && !containerOpen && !dirtyCashOpen &&
                _miloScenario != null && _miloScenario.HasPendingChoice)
            {
                int choice = keyboard.digit1Key.wasPressedThisFrame ? 1 :
                    keyboard.digit2Key.wasPressedThisFrame ? 2 :
                    keyboard.digit3Key.wasPressedThisFrame ? 3 :
                    keyboard.digit4Key.wasPressedThisFrame ? 4 : 0;
                if (choice > 0 && _miloScenario.HandleChoice(choice, out string choiceMessage))
                {
                    RegisterUnownedPlayerAssets(_inventory);
                    Say(choiceMessage, 8f);
                    return;
                }
            }

            if (!workspaceOpen && !inventoryOpen && !storageOpen && !itemViewerOpen && !retailOpen && !containerOpen &&
                !dirtyCashOpen && nearDirtyCashContact && keyboard.eKey.wasPressedThisFrame)
            {
                if (_dirtyCashPanel != null) _dirtyCashPanel.Open();
            }
            else if (!workspaceOpen && !inventoryOpen && !storageOpen && !itemViewerOpen && !retailOpen && !containerOpen &&
                !dirtyCashOpen && nearSearchableContainer && keyboard.eKey.wasPressedThisFrame)
            {
                OpenSearchableContainer(nearbyContainerId);
            }
            else if (!workspaceOpen && !inventoryOpen && !storageOpen && !itemViewerOpen && !retailOpen && !containerOpen && !dirtyCashOpen &&
                nearbyBusiness != null && keyboard.eKey.wasPressedThisFrame)
            {
                if (!nearbyBusiness.TryValidate(out string businessError))
                    Say("Business configuration error: " + businessError, 6f);
                else if (!nearbyBusiness.IsOpen(_clock.MinuteOfDay))
                    Say(nearbyBusiness.DisplayName + " is closed.");
                else
                    _retailPanel.Open(nearbyBusiness);
            }
            else if (!workspaceOpen && !inventoryOpen && !storageOpen && !itemViewerOpen && !retailOpen && !containerOpen && !dirtyCashOpen && nearWorkbench && keyboard.eKey.wasPressedThisFrame)
                _workspace.Open();
            else if (!workspaceOpen && !inventoryOpen && !storageOpen && !itemViewerOpen && !retailOpen && !containerOpen && !dirtyCashOpen && nearStorage && keyboard.eKey.wasPressedThisFrame)
                _storagePanel.SetOpen(true);
            else if (!workspaceOpen && !inventoryOpen && !storageOpen && !itemViewerOpen && !retailOpen && !containerOpen &&
                     !dirtyCashOpen && !nearDirtyCashContact && !nearSearchableContainer && nearbyBusiness == null && !nearWorkbench && !nearStorage &&
                     cityPrompt.Length > 0 && keyboard.eKey.wasPressedThisFrame)
            {
                if (_miloScenario != null &&
                    _miloScenario.TryInteract(playerPosition, _city.CurrentPlace, out string scenarioMessage))
                {
                    RegisterUnownedPlayerAssets(_inventory);
                    Say(scenarioMessage, 8f);
                }
                else if (_city.TryInteractNpc(playerPosition, out string npcDialogue))
                    Say(npcDialogue, 5f);
                else if (_city.TryInteract(playerPosition))
                    Say("Location: " + _city.CurrentPlace);
            }

            if (!retailOpen && !dirtyCashOpen)
            {
                if (!containerOpen && keyboard.f6Key.wasPressedThisFrame) Sleep();
                if (keyboard.f5Key.wasPressedThisFrame) SaveGame();
                if (keyboard.f9Key.wasPressedThisFrame) LoadGame();
            }
        }

        private void TickClock()
        {
            if (_clock == null || _clock.IsPaused)
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
            GUILayout.Label("Clock: " + _clock.Label + (_clock.IsPaused ? "  [PAUSED]" : ""));
            GUILayout.Label("Location: " + _city.CurrentPlace);
            GUILayout.Label("NPC simulation: " + (_city.DebugSimulationPaused ? "PAUSED" : "RUNNING"));
            GUILayout.Label(_city.DebugResidentSummary("milo"));
            GUILayout.Label(_city.DebugResidentSummary("rhea"));
            GUILayout.Label("Milo opportunity: " + (_miloOpportunity != null ? _miloOpportunity.Phase.ToString() : "Unavailable"));
            GUILayout.Label("Heat: " + (_heat != null ? _heat.Heat.ToString() : "0") +
                "   Criminal progression: " + (_criminalProgression != null ? _criminalProgression.Points.ToString() : "0"));
            GUILayout.Label("Crime outcomes: " + (_crimeOutcomes != null ? _crimeOutcomes.Count.ToString() : "0"));
            GUILayout.Label("Discovery signals: " + (_playerDiscoveries != null ? _playerDiscoveries.Count.ToString() : "0") +
                "   Familiarity: " + (_playerRelationships != null ? _playerRelationships.Get("milo").Familiarity.ToString("0.00") : "0.00"));
            GUILayout.Label("Resident clean accounts:");
            foreach (NpcPresenter resident in _city.DebugResidents)
            {
                if (resident == null || string.IsNullOrWhiteSpace(resident.NpcId))
                    continue;
                EconomicActorId actor = new EconomicActorId("resident:" + resident.NpcId);
                if (_economicAccounts.TryGetAccount(actor, out CleanMoneyAccount account))
                    GUILayout.Label("  " + resident.DisplayName + ": $" + account.Balance);
            }
            GUILayout.Label("Retail transactions: " + (_retailTransactions != null ? _retailTransactions.Count.ToString() : "0"));
            GUILayout.Label("Shop sale transactions: " + (_shopSaleTransactions != null ? _shopSaleTransactions.Count.ToString() : "0"));
            GUILayout.Space(8f);

            if (GUILayout.Button(_city.DebugSimulationPaused ? "Resume all NPC schedules" : "Pause all NPC schedules"))
                _city.DebugSetNpcSimulationPaused(!_city.DebugSimulationPaused);
            if (GUILayout.Button(_clock.IsPaused ? "Resume game clock" : "Pause game clock"))
                _clock.SetPaused(!_clock.IsPaused);
            GUILayout.Label("Navigation overlay: " + (_city.DebugNavigationOverlayVisible ? "ON" : "OFF"));
            if (GUILayout.Button("Toggle navigation geometry / NPC route overlay"))
                _city.DebugToggleNavigationOverlay();
            if (GUILayout.Button("Retry failed NPC routes (once)"))
            {
                int recovered = _city.DebugRetryFailedNpcRoutes();
                Say("Navigation retry: " + recovered + " NPC route(s) recovered.", 4f);
            }

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
            GUILayout.Label("RHEA — PROPERTY THEFT TEST");
            if (GUILayout.Button("Force Rhea: Home + focus"))
                DebugForceRheaHome();
            if (GUILayout.Button("Return Rhea to authored schedule"))
            {
                _city.DebugSetNpcSimulationPaused(false);
                _city.DebugResumeResidentSchedule("rhea");
                Say("Rhea returned to her authored schedule.", 4f);
            }

            GUILayout.Space(8f);
            GUILayout.Label("OPPORTUNITY 001 — TEST CONTROLS");
            if (GUILayout.Button("TRIGGER MILO OFFER NOW (bypass discovery gates)"))
                DebugTriggerMiloOffer();

            GUILayout.Space(8f);
            GUILayout.Label("F1 closes this menu. Debug controls do not represent player abilities.");
            GUILayout.EndArea();
        }

        private void DebugForceRheaHome()
        {
            _city.DebugSetNpcSimulationPaused(true);
            if (!_city.DebugForceResidentActivity("rhea", NpcActivity.Home) ||
                !_city.DebugFocusResident("rhea"))
            {
                Say("Rhea is unavailable.", 4f);
                return;
            }

            _debugMenuOpen = false;
            Say("DEBUG: Rhea is home. Move to her wardrobe to test the theft.", 6f);
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
                                               _playerDiscoveries, _miloOpportunity, _playerRelationships,
                                               _assetOwnership, _economicTransactions, _economicAccounts, _retailTransactions,
                                               _shopSaleTransactions, _criminalProgression, _crimeOutcomes,
                                               _contactInventory, _illicitExchanges, _dirtyCashCleanings);
            SearchableContainerSaveMapper.Capture(_searchableContainers, data);

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

            if (_containerPanel != null) _containerPanel.Close();
            if (_dirtyCashPanel != null) _dirtyCashPanel.Close();
            _activeSearchableContainer = null;
            LoadReport report = SaveMapper.Apply(data, _stats, _inventory, _storage, _clock, _heat, _rent,
                ResolveType, _itemKnowledge, _playerDiscoveries, _playerRelationships, _assetOwnership, _economicTransactions,
                _economicAccounts, _retailTransactions, _shopSaleTransactions, _criminalProgression, _crimeOutcomes,
                _contactInventory, _illicitExchanges, _dirtyCashCleanings);
            if (!report.Success)
            {
                Say("Load failed: " + report.Error);
                return;
            }

            int skippedContainers = SearchableContainerSaveMapper.Apply(data, _searchableContainers, ResolveType);
            RegisterSearchableContainerAssets();
            RegisterUnownedPlayerAssets(_inventory);
            _miloOpportunity = report.RestoredOpportunity != null &&
                report.RestoredOpportunity.OpportunityId == "milo-parcel"
                ? report.RestoredOpportunity
                : new OpportunityState("milo-parcel");
            if (_miloScenario != null)
                _miloScenario.RestoreOpportunityState(_miloOpportunity);

            RestoreCrimeWitnessKnowledge();
            var position = new Vector2(data.playerX, data.playerY);
            if (!_city.TryRestorePlace(data.worldPlace, position))
            {
                Say("Loaded inventory, but the saved location is unknown. Returned to the hideout.");
                _city.TryRestorePlace("Hideout", new Vector2(10f, 5f));
                return;
            }

            Say(report.SkippedItems + report.SkippedStorageItems + report.SkippedKnowledgeFacts +
                report.SkippedDiscoverySignals + report.SkippedPlayerRelationships +
                report.SkippedAssetOwners + report.SkippedEconomicTransactions +
                report.SkippedResidentAccounts + report.SkippedRetailTransactions + report.SkippedShopSaleTransactions +
                report.SkippedCrimeOutcomes + report.SkippedContactItems +
                report.SkippedIllicitExchangeTransactions + report.SkippedDirtyCashCleaningTransactions +
                skippedContainers > 0
                ? "Loaded at " + data.worldPlace + ". Some saved item or discovery details could not be restored."
                : "Loaded at " + data.worldPlace + ".");
        }

        private bool IsNearDirtyCashContact()
        {
            return _city != null && _city.IsResidentWithin("Clean_Dirtycash", _room.PlayerBody.position, 1.85f);
        }

        private string ExchangeWithContact(Guid selectedItemInstanceId)
        {
            if (!IsNearDirtyCashContact())
                return "Move closer to the contact.";
            if (dirtyCashContactOffer == null || !dirtyCashContactOffer.TryValidate(out _))
                return "The offer is unavailable.";

            ItemDefinition acceptedItem = dirtyCashContactOffer.AcceptedItem;
            ItemDefinition cashItem = dirtyCashContactOffer.DirtyCashItem;
            int acceptedQuantity = dirtyCashContactOffer.AcceptedQuantity;
            int dirtyCashUnits = dirtyCashContactOffer.DirtyCashUnits;
            ItemType acceptedType = acceptedItem.ToItemType();
            if (!_inventory.TryGetItemInstance(selectedItemInstanceId, out PlacedItem selected) ||
                selected.Type != acceptedType || selected.Count != acceptedQuantity ||
                selected.State.Ownership != ItemOwnershipStatus.Stolen ||
                !_assetOwnership.TryGetOwner(selectedItemInstanceId, out EconomicActorId owner) ||
                owner != EconomicActorId.Player)
                return "Select one player-owned stolen stack of exactly " + acceptedQuantity + " " +
                    acceptedItem.DisplayName + " items.";

            IllicitExchangeResult result = IllicitExchangeService.TryExchange(_inventory, _contactInventory,
                selected.ItemInstanceId, acceptedType, acceptedQuantity, cashItem.ToItemType(),
                dirtyCashUnits, new EconomicActorId("resident:Clean_Dirtycash"),
                _assetOwnership, _economicTransactions, _illicitExchanges, _clock.Day, _clock.MinuteOfDay);
            if (result.Success)
                return "Exchange complete. Received " + dirtyCashUnits + " Dirty Cash units.";
            switch (result.Status)
            {
                case IllicitExchangeStatus.ContactStorageFull:
                    return "The contact has no room for that item.";
                case IllicitExchangeStatus.RewardInventoryFull:
                    return "Make room in your carried inventory for the Dirty Cash. Nothing was exchanged.";
                case IllicitExchangeStatus.ItemNotStolen:
                    return "The contact only accepts the exact stolen item stack.";
                case IllicitExchangeStatus.OwnershipMismatch:
                    return "You must carry an item stack that you own.";
                default:
                    return "The exchange could not be completed. Nothing was changed.";
            }
        }

        private string CleanDirtyCash(Guid cashInstanceId)
        {
            if (!IsNearDirtyCashContact())
                return "Move closer to the contact.";
            if (dirtyCashContactOffer == null || !dirtyCashContactOffer.TryValidate(out _))
                return "The cleaning offer is unavailable.";

            var contactActor = new EconomicActorId("resident:Clean_Dirtycash");
            if (!_economicAccounts.TryGetAccount(contactActor, out CleanMoneyAccount contactAccount))
                return "The contact's account is unavailable.";

            DirtyCashCleaningResult result = DirtyCashCleaningService.TryClean(_inventory, cashInstanceId,
                dirtyCashContactOffer.DirtyCashItem.ToItemType(), EconomicActorId.Player, _assetOwnership,
                _stats.CleanMoney, contactAccount, contactActor, dirtyCashContactOffer.CleaningFeePercent,
                _dirtyCashCleanings,
                _clock.Day, _clock.MinuteOfDay);
            if (result.Success)
                return "Cleaned " + result.Transaction.DirtyUnits + " Dirty Cash units. $" +
                    result.Transaction.CleanPayout + " added to CARD.";
            switch (result.Status)
            {
                case DirtyCashCleaningStatus.AccountOverflow: return "The transaction accounts cannot accept that amount. Nothing was changed.";
                case DirtyCashCleaningStatus.OwnershipMismatch: return "You do not own that Dirty Cash stack.";
                default: return "Cleaning failed (" + result.Status + "). Nothing was changed.";
            }
        }

        private BusinessDefinition FindNearbyBusiness(Vector2 playerPosition)
        {
            if (businesses == null || _city == null)
                return null;
            for (int i = 0; i < businesses.Length; i++)
            {
                BusinessDefinition business = businesses[i];
                if (business == null || business.LocationId != _city.CurrentPlace)
                    continue;
                if (Vector2.Distance(playerPosition, business.InteractionPoint) <= 1.85f)
                    return business;
            }
            return null;
        }

        private string PurchaseFromBusiness(BusinessDefinition business, ItemDefinition item)
        {
            if (business == null || item == null)
                return "The business is unavailable.";
            if (!business.TryValidate(out string businessError))
                return "Business configuration error: " + businessError;
            if (!business.IsOpen(_clock.MinuteOfDay))
                return "The business is closed.";
            if (!business.TryGetOffer(item, out int unitPrice))
                return "That item is not offered here.";
            if (string.IsNullOrWhiteSpace(business.OwnerNpcId))
                return "This business has no valid owner.";
            EconomicActorId owner = business.OwnerActor;
            if (!_economicAccounts.TryGetAccount(owner, out _))
                return "The business owner's account is unavailable.";

            RetailPurchaseResult result = _retailService.TryPurchase(business.BusinessId, _inventory,
                EconomicActorId.Player, owner, item.ToItemType(), item.DefaultInstanceState,
                1, unitPrice, _clock.Day, _clock.MinuteOfDay);
            if (result.Success)
                return "Purchased " + item.DisplayName + " for $" + unitPrice + ".";
            switch (result.Status)
            {
                case RetailPurchaseStatus.InsufficientFunds: return "Not enough CARD balance.";
                case RetailPurchaseStatus.InventoryFull: return "Not enough inventory space.";
                case RetailPurchaseStatus.AccountOverflow: return "The owner's account cannot accept this payment.";
                case RetailPurchaseStatus.AccountNotFound: return "A required clean-money account is unavailable.";
                default: return "Purchase failed (" + result.Status + ").";
            }
        }

        private string SellToBusiness(BusinessDefinition business, Guid itemInstanceId, int quantity)
        {
            if (business == null || itemInstanceId == Guid.Empty || quantity < 1)
                return "The sale request is invalid.";

            BusinessDefinition nearby = FindNearbyBusiness(_room.PlayerBody.position);
            if (!ReferenceEquals(nearby, business) || nearby.LocationId != _city.CurrentPlace)
                return "Move back to this business counter before selling.";

            if (!business.TryValidate(out string businessError))
                return "Business configuration error: " + businessError;
            if (!business.IsOpen(_clock.MinuteOfDay))
                return "The business is closed.";
            if (!_inventory.TryGetItemInstance(itemInstanceId, out PlacedItem selected))
                return "That inventory stack is no longer available.";
            if (!_assetOwnership.TryGetOwner(itemInstanceId, out EconomicActorId owner) ||
                owner != EconomicActorId.Player)
                return "You do not own that item.";

            ItemDefinition definition = FindItemDefinition(selected.Type.Id);
            if (definition == null || !business.TryGetBuybackOffer(definition, out int unitPrice))
                return "This business does not buy that item.";
            if (quantity > selected.Count)
                return "That stack no longer contains the requested quantity.";

            FulfilResult result = Shop.TryFulfilExact(new ShopOrder(selected.Type, quantity, unitPrice),
                itemInstanceId, business.BusinessId, EconomicActorId.Player, _inventory, _assetOwnership,
                _stats.CleanMoney, _shopSaleTransactions, _clock.Day, _clock.MinuteOfDay);
            if (result == FulfilResult.Done)
                return "Sold " + quantity + " " + definition.DisplayName +
                    (quantity == 1 ? "" : "s") + " for $" + ((long)quantity * unitPrice) + ".";

            return result == FulfilResult.NotEnoughStock
                ? "That stack no longer contains the requested quantity."
                : "Sale failed. Your items and CARD balance were left unchanged.";
        }

        private ItemDefinition FindItemDefinition(Guid itemDefinitionId)
        {
            for (int i = 0; i < _allItemDefinitions.Count; i++)
            {
                ItemDefinition definition = _allItemDefinitions[i];
                if (definition == null)
                    continue;
                try
                {
                    if (definition.ToItemType().Id == itemDefinitionId)
                        return definition;
                }
                catch (InvalidOperationException) { }
            }
            return null;
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

            if (businesses != null)
            {
                foreach (BusinessDefinition business in businesses)
                {
                    if (business == null)
                        continue;
                    if (!business.TryValidate(out string businessError))
                    {
                        Debug.LogError("GameSession: business '" + business.name + "' is invalid: " + businessError);
                        continue;
                    }
                    foreach (BusinessDefinition.MerchandiseEntry entry in business.Merchandise)
                    {
                        ItemType businessItemType = entry.Item.ToItemType();
                        _catalog[businessItemType.Id] = businessItemType;
                        if (!_allItemDefinitions.Contains(entry.Item))
                            _allItemDefinitions.Add(entry.Item);
                    }
                    foreach (BusinessDefinition.BuybackEntry entry in business.BuybackOffers)
                    {
                        ItemType buybackItemType = entry.Item.ToItemType();
                        _catalog[buybackItemType.Id] = buybackItemType;
                        if (!_allItemDefinitions.Contains(entry.Item))
                            _allItemDefinitions.Add(entry.Item);
                    }
                }
            }

            if (searchableLootProfiles != null)
            {
                foreach (ContainerLootProfile profile in searchableLootProfiles)
                {
                    if (profile == null) continue;
                    foreach (ItemDefinition lootItem in profile.Items)
                    {
                        if (lootItem == null) continue;
                        ItemType lootType = lootItem.ToItemType();
                        _catalog[lootType.Id] = lootType;
                        if (!_allItemDefinitions.Contains(lootItem)) _allItemDefinitions.Add(lootItem);
                    }
                }
            }

            if (residentPropertyContainers != null)
            {
                foreach (ResidentPropertyContainerDefinition property in residentPropertyContainers)
                {
                    if (property == null) continue;
                    if (!property.TryValidate(out string propertyError))
                    {
                        Debug.LogError("GameSession: invalid resident property container '" + property.name + "': " + propertyError);
                        continue;
                    }
                    foreach (ItemDefinition propertyItem in property.Items)
                    {
                        ItemType propertyType = propertyItem.ToItemType();
                        _catalog[propertyType.Id] = propertyType;
                        if (!_allItemDefinitions.Contains(propertyItem)) _allItemDefinitions.Add(propertyItem);
                    }
                }
            }

            if (dirtyCashContactOffer != null && dirtyCashContactOffer.TryValidate(out _))
            {
                ItemDefinition cashItem = dirtyCashContactOffer.DirtyCashItem;
                ItemDefinition acceptedItem = dirtyCashContactOffer.AcceptedItem;
                ItemType cashType = cashItem.ToItemType();
                ItemType acceptedType = acceptedItem.ToItemType();
                _catalog[cashType.Id] = cashType;
                _catalog[acceptedType.Id] = acceptedType;
                if (!_allItemDefinitions.Contains(cashItem)) _allItemDefinitions.Add(cashItem);
                if (!_allItemDefinitions.Contains(acceptedItem)) _allItemDefinitions.Add(acceptedItem);
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


        private void CreateSearchableContainers()
        {
            _searchableContainers.Clear();
            _searchableProfiles.Clear();
            _searchableActors.Clear();
            _residentPropertyDefinitions.Clear();
            IReadOnlyList<string> ids = _city.SearchableContainerIds;
            for (int i = 0; i < ids.Count; i++)
            {
                string id = ids[i];
                var container = new SearchableContainer(id, 8, 6);
                _searchableContainers.Add(container);
                var actor = new EconomicActorId("world:" + id);
                _searchableActors[id] = actor;
                _economicContainers.TryRegister(container.Inventory, actor);
                ContainerLootProfile profile = searchableLootProfiles != null && i < searchableLootProfiles.Length
                    ? searchableLootProfiles[i] : null;
                if (profile != null) _searchableProfiles[id] = profile;
            }

            if (residentPropertyContainers == null) return;
            foreach (ResidentPropertyContainerDefinition definition in residentPropertyContainers)
            {
                if (definition == null)
                    continue;
                if (!definition.TryValidate(out string error))
                {
                    Debug.LogError("GameSession: invalid resident property container '" + definition.name + "': " + error);
                    continue;
                }
                if (!_city.HasResident(definition.OwnerNpcId))
                {
                    Debug.LogError("GameSession: property container '" + definition.ContainerId +
                        "' references an unavailable owner NPC '" + definition.OwnerNpcId + "'.");
                    continue;
                }
                if (_searchableActors.ContainsKey(definition.ContainerId))
                {
                    Debug.LogError("GameSession: duplicate searchable container id '" + definition.ContainerId + "'.");
                    continue;
                }

                var container = new SearchableContainer(definition.ContainerId, 8, 6);
                var owner = new EconomicActorId("resident:" + definition.OwnerNpcId);
                _searchableContainers.Add(container);
                _searchableActors.Add(definition.ContainerId, owner);
                _residentPropertyDefinitions.Add(definition.ContainerId, definition);
                _economicContainers.TryRegister(container.Inventory, owner);
            }
        }

        private void OpenSearchableContainer(string containerId)
        {
            SearchableContainer container = FindSearchableContainer(containerId);
            if (container == null || _containerPanel == null)
                return;
            if (!container.IsGenerated)
            {
                if (_residentPropertyDefinitions.TryGetValue(containerId, out ResidentPropertyContainerDefinition property))
                {
                    if (!property.TryPopulate(container, out string error))
                    {
                        Say("This property cannot be searched: " + error, 5f);
                        return;
                    }
                }
                else
                {
                    if (!_searchableProfiles.TryGetValue(containerId, out ContainerLootProfile profile))
                    {
                        Say("This container has no loot profile assigned.", 5f);
                        return;
                    }
                    if (!profile.TryGenerate(container, out string error))
                    {
                        Say("This container cannot be searched: " + error, 5f);
                        return;
                    }
                }
                RegisterSearchableContainerAssets(container);
            }

            _activeSearchableContainer = container;
            string displayName = _residentPropertyDefinitions.TryGetValue(containerId,
                out ResidentPropertyContainerDefinition propertyDefinition)
                ? propertyDefinition.DisplayName : null;
            _containerPanel.Open(container, _inventory, displayName);
        }

        private SearchableContainer FindSearchableContainer(string containerId)
        {
            for (int i = 0; i < _searchableContainers.Count; i++)
                if (_searchableContainers[i].ContainerId == containerId)
                    return _searchableContainers[i];
            return null;
        }

        private void RegisterSearchableContainerAssets()
        {
            for (int i = 0; i < _searchableContainers.Count; i++)
                RegisterSearchableContainerAssets(_searchableContainers[i]);
        }

        private void RegisterSearchableContainerAssets(SearchableContainer container)
        {
            if (container == null || !_searchableActors.TryGetValue(container.ContainerId, out EconomicActorId actor))
                return;
            foreach (PlacedItem item in container.Inventory.Items)
                if (!_assetOwnership.TryGetOwner(item.ItemInstanceId, out _))
                    _assetOwnership.TryRegister(item.ItemInstanceId, actor);
        }

        private bool TakeSearchableItem(Guid itemInstanceId)
        {
            if (_activeSearchableContainer == null || !_activeSearchableContainer.IsDiscovered(itemInstanceId))
                return false;
            if (!_searchableActors.TryGetValue(_activeSearchableContainer.ContainerId, out EconomicActorId actor))
                return false;
            return TransferSearchableItem(itemInstanceId, actor, false, 0, 0, false);
        }

        private bool TakeSearchableItemAt(Guid itemInstanceId, int x, int y, bool rotated)
        {
            if (_activeSearchableContainer == null || !_activeSearchableContainer.IsDiscovered(itemInstanceId))
                return false;
            if (!_searchableActors.TryGetValue(_activeSearchableContainer.ContainerId, out EconomicActorId actor))
                return false;
            return TransferSearchableItem(itemInstanceId, actor, true, x, y, rotated);
        }

        private bool TransferSearchableItem(Guid itemInstanceId, EconomicActorId actor,
            bool positioned, int x, int y, bool rotated)
        {
            bool isTheft = _residentPropertyDefinitions.TryGetValue(_activeSearchableContainer.ContainerId,
                out ResidentPropertyContainerDefinition property);
            string witnessNpcId = null;
            if (isTheft && _city.TryGetNearestResidentWithin(property.InteractionPosition,
                property.WitnessRadius, out string nearbyWitness))
                witnessNpcId = nearbyWitness;

            AssetTransferResult result = positioned
                ? _assetTransfers.TransferStackAt(_activeSearchableContainer.Inventory, _inventory,
                    itemInstanceId, x, y, rotated, actor, EconomicActorId.Player,
                    isTheft ? AssetTransferReason.Theft : AssetTransferReason.Scavenging,
                    _clock.Day, _clock.MinuteOfDay)
                : _assetTransfers.TransferStack(_activeSearchableContainer.Inventory, _inventory,
                    itemInstanceId, actor, EconomicActorId.Player,
                    isTheft ? AssetTransferReason.Theft : AssetTransferReason.Scavenging,
                    _clock.Day, _clock.MinuteOfDay);

            if (!result.Success)
            {
                if (result.Status == AssetTransferStatus.DestinationFull)
                    Say(positioned ? "That spot is occupied or does not fit." : "Not enough room in your inventory.");
                return false;
            }

            if (isTheft)
            {
                RecordTheft(property, result.Transaction, witnessNpcId);
            }
            else
            {
                Say("Item taken.");
            }
            return true;
        }

        private void RecordTheft(ResidentPropertyContainerDefinition property,
            EconomicTransaction transaction, string witnessNpcId)
        {
            bool witnessed = !string.IsNullOrWhiteSpace(witnessNpcId);
            string eventId = witnessed ? "crime-" + transaction.TransactionId.ToString("N") : string.Empty;
            int heatAward = property.GetHeatAward(witnessed);
            var outcome = new CrimeOutcome(transaction.TransactionId, property.ActionId,
                EconomicActorId.Player.Value, transaction.ItemInstanceId, property.LocationId, witnessNpcId, eventId,
                property.WitnessDescription, property.InteractionPosition.x, property.InteractionPosition.y,
                property.WitnessRadius, _clock.Day, _clock.MinuteOfDay,
                heatAward, property.ProgressionAward);

            if (!_crimeOutcomes.TryRecord(outcome))
            {
                Debug.LogError("A completed theft could not be recorded in the crime outcome ledger: " +
                    transaction.TransactionId);
                return;
            }

            _heat.Add(outcome.HeatAward);
            _criminalProgression.Award(property.ProgressionAward);

            if (witnessed)
            {
                var worldEvent = new NpcWorldEvent(eventId, "crime-observed",
                    property.WitnessDescription, null, property.LocationId,
                    property.InteractionPosition.x, property.InteractionPosition.y,
                    property.WitnessRadius, _clock.Day, _clock.MinuteOfDay);
                if (_city.SocialWorld == null || !_city.SocialWorld.PublishEventToNpc(worldEvent, witnessNpcId))
                    Debug.LogWarning("The theft was recorded, but its selected witness is no longer registered: " + witnessNpcId);
            }

            Say(witnessed ? "Taken. Someone nearby saw it." : "Taken. No one nearby witnessed it.", 4f);
        }

        private void RestoreCrimeWitnessKnowledge()
        {
            if (_crimeOutcomes == null || _city == null || _city.SocialWorld == null)
                return;

            foreach (CrimeOutcome outcome in _crimeOutcomes.Outcomes)
            {
                if (!outcome.WasWitnessed)
                    continue;

                var worldEvent = new NpcWorldEvent(outcome.WitnessEventId, "crime-observed",
                    outcome.WitnessDescription, null, outcome.LocationId,
                    outcome.WitnessX, outcome.WitnessY, outcome.WitnessRadius,
                    outcome.Day, outcome.MinuteOfDay);
                _city.SocialWorld.PublishEventToNpc(worldEvent, outcome.WitnessNpcId);
            }
        }

        private ResidentPropertyContainerDefinition FindNearbyResidentProperty(Vector2 playerPosition)
        {
            if (_city == null || residentPropertyContainers == null)
                return null;

            ResidentPropertyContainerDefinition nearest = null;
            float bestDistance = 1.85f * 1.85f;
            foreach (ResidentPropertyContainerDefinition definition in residentPropertyContainers)
            {
                if (definition == null || definition.LocationId != _city.CurrentPlace ||
                    !_residentPropertyDefinitions.ContainsKey(definition.ContainerId)) continue;
                float distance = (playerPosition - definition.InteractionPosition).sqrMagnitude;
                if (distance > bestDistance) continue;
                bestDistance = distance;
                nearest = definition;
            }
            return nearest;
        }

        private void RegisterUnownedPlayerAssets(GridInventory inventory)
        {
            if (inventory == null || _assetOwnership == null) return;
            foreach (PlacedItem item in inventory.Items)
                if (!_assetOwnership.TryGetOwner(item.ItemInstanceId, out _))
                    _assetOwnership.TryRegister(item.ItemInstanceId, EconomicActorId.Player);
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