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
        private const string DirtyCashContactNpcId = "Clean_Dirtycash";
        private static readonly EconomicActorId DirtyCashContactActorId =
            new EconomicActorId("resident:" + DirtyCashContactNpcId);
        private const float InteractionDistance = 1.85f;
        [Header("Content")]
        [SerializeField] private ItemDefinition[] items;
        [SerializeField] private ItemDefinition parcelItem;
        [SerializeField] private DirtyCashContactOfferDefinition dirtyCashContactOffer;
        [SerializeField] private NpcCharacterDefinition[] npcDefinitions;
        [SerializeField] private NpcAgendaDefinition[] npcAgendas;
        [SerializeField] private BusinessDefinition[] businesses;
        [SerializeField] private ContainerLootProfile[] searchableLootProfiles;
        [SerializeField] private ResidentPropertyContainerDefinition[] residentPropertyContainers;

        [Header("Livelihood — first workshop slice")]
        [SerializeField] private RecipeDefinition[] startingWorkshopRecipes;
        [SerializeField] private CustomerContractDefinition firstCustomerContract;
        [SerializeField] private CustomerRepairOrderDefinition firstCustomerRepairOrder;
        [SerializeField] private WorkshopUpgradeDefinition firstWorkshopUpgrade;

        [Header("Livelihood — repair and restoration")]
        [SerializeField] private ItemDefinition repairTargetItem;
        [SerializeField] private ItemDefinition basicPatchKitItem;
        [SerializeField] private ItemDefinition reinforcedPatchKitItem;

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
        private ProductionTransactionLedger _productionTransactions;
        private CustomerContractBook _customerContracts;
        private CustomerRepairOrderBook _customerRepairOrders;
        private ServicePaymentTransactionLedger _servicePaymentTransactions;
        private WorkshopProgressionBook _workshopProgression;
        private GridInventory _contractReceiverInventory;
        private EconomicActorId _contractReceiverActor;
        private readonly List<RecipeDefinition> _workshopRecipes = new List<RecipeDefinition>();
        private GridInventory _contactInventory;
        private GridInventory _inventory;
        private GridInventory _storage;
        private ItemKnowledgeBook _itemKnowledge;
        private PlayerDiscoveryLedger _playerDiscoveries;
        private NpcAgendaProgressBook _npcAgendaProgress;
        private NpcAgendaInteractionDirector _npcAgendaDirector;
        private NpcAgendaConversationPanel _npcAgendaPanel;
        private OpportunityState _miloOpportunity;
        private PlayerRelationshipBook _playerRelationships;
        private GameClock _clock;
        private HeatMeter _heat;
        private CriminalProgressionTrack _criminalProgression;
        private CrimeOutcomeLedger _crimeOutcomes;
        private RentLedger _rent;

        private RoomBootstrap _room;
        private PlayerMotor2D _motor;
        private PlayerDevelopmentState _playerDevelopment;
        private PlayerDevelopmentPanel _playerDevelopmentPanel;

        // Prototype content IDs; the final skill catalogue remains a data/content decision.
        private static readonly string[] ProvisionalPlayerSkillIds =
            { "repair", "production", "trading", "scavenging", "social" };
        private const int PrototypeStartingSkillPoints = 8;
        private const int PrototypeMaximumSkillRank = 5;
        private const int PrototypePointsPerSkillRank = 1;
        private InventoryPanel _panel;
        private ItemViewerPanel _itemViewer;
        private DebugItemSpawnerPanel _debugItemSpawnerPanel;
        private StoragePanel _storagePanel;
        private HudPanel _hud;
        private RetailPanel _retailPanel;
        private DirtyCashContactPanel _dirtyCashPanel;
        private CustomerContractPanel _customerContractPanel;
        private CustomerRepairOrderPanel _customerRepairOrderPanel;
        private WorkshopUpgradePanel _workshopUpgradePanel;
        private GameObject _installedUpgradeVisual;
        private SearchableContainerPanel _containerPanel;
        private SearchableContainer _activeSearchableContainer;
        private WorkbenchWorkspace _workspace;
        private CityBlockBootstrap _city;
        private MiloParcelOpportunityController _miloScenario;

        private float _minuteBuffer;
        private bool _debugMenuOpen;
        private string _interactionPausedNpcId;
        
        /// <summary>Player-scoped discovered item facts, shared by inventory and scenario interactions.</summary>
        public ItemKnowledgeBook PlayerItemKnowledge => _itemKnowledge;
        public PlayerDiscoveryLedger PlayerDiscoveries => _playerDiscoveries;
        public NpcAgendaProgressBook NpcAgendaProgress => _npcAgendaProgress;
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
        public ProductionTransactionLedger ProductionTransactions => _productionTransactions;
        public CustomerContractBook CustomerContracts => _customerContracts;
        public CustomerRepairOrderBook CustomerRepairOrders => _customerRepairOrders;
        public ServicePaymentTransactionLedger ServicePaymentTransactions => _servicePaymentTransactions;
        public WorkshopProgressionBook WorkshopProgression => _workshopProgression;
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
            _productionTransactions = new ProductionTransactionLedger();
            _customerContracts = new CustomerContractBook();
            _customerRepairOrders = new CustomerRepairOrderBook();
            _servicePaymentTransactions = new ServicePaymentTransactionLedger();
            _workshopProgression = new WorkshopProgressionBook();
            _contactInventory = new GridInventory(8, 6);
            _economicContainers.TryRegister(_contactInventory, DirtyCashContactActorId);
            _itemKnowledge = new ItemKnowledgeBook();
            _playerDiscoveries = new PlayerDiscoveryLedger();
            _npcAgendaProgress = new NpcAgendaProgressBook();
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
            _playerDevelopment = new PlayerDevelopmentState(ProvisionalPlayerSkillIds,
                PrototypeStartingSkillPoints, PrototypeMaximumSkillRank, PrototypePointsPerSkillRank);
            _playerDevelopmentPanel = gameObject.AddComponent<PlayerDevelopmentPanel>();
            _playerDevelopmentPanel.Bind(_playerDevelopment);

            _city = gameObject.AddComponent<CityBlockBootstrap>();
            _city.Build(_room.Player, _room.PlayerBody, camera, _clock, npcDefinitions);
            if (dirtyCashContactOffer == null)
                Debug.LogError("GameSession: assign the Slice B Dirty Cash contact offer.");
            else if (!dirtyCashContactOffer.TryValidate(out string offerError))
                Debug.LogError("GameSession: invalid Dirty Cash contact offer: " + offerError);
            else if (!_city.TryAddResident(dirtyCashContactOffer.Contact))
                Debug.LogError("GameSession: the " + DirtyCashContactNpcId + " resident could not be added to the city.");
            CreateSearchableContainers();
            InitializeLivelihoodWorld();
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
            _debugItemSpawnerPanel = gameObject.AddComponent<DebugItemSpawnerPanel>();
            _debugItemSpawnerPanel.Bind(_allItemDefinitions, SpawnDebugItem);
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

            if (firstCustomerContract != null && _contractReceiverInventory != null)
            {
                _customerContractPanel = gameObject.AddComponent<CustomerContractPanel>();
                _customerContractPanel.Bind(firstCustomerContract, _inventory, _assetOwnership,
                    _allItemDefinitions, GetCustomerDisplayName(), AcceptCustomerContract, FulfilCustomerContract,
                    GetCustomerContractPhase);
            }
            if (firstCustomerRepairOrder != null && _contractReceiverInventory != null)
            {
                _customerRepairOrderPanel = gameObject.AddComponent<CustomerRepairOrderPanel>();
                _customerRepairOrderPanel.Bind(firstCustomerRepairOrder, GetCustomerDisplayName(),
                    AcceptCustomerRepairOrder, ReturnCustomerRepairOrder, GetCustomerRepairOrderPhase);
            }

            if (firstWorkshopUpgrade != null)
            {
                _workshopUpgradePanel = gameObject.AddComponent<WorkshopUpgradePanel>();
                _workshopUpgradePanel.Bind(firstWorkshopUpgrade,
                    _city.GetResidentDisplayName(firstWorkshopUpgrade.PayeeNpcId),
                    GetFirstUpgradeAvailabilityMessage, IsFirstUpgradeAcquired,
                    CanAcquireFirstUpgrade, AcquireFirstUpgrade);
            }

            NpcAgendaDefinition[] agendaDefinitions = npcAgendas != null && npcAgendas.Length > 0
                ? npcAgendas
                : Resources.LoadAll<NpcAgendaDefinition>("Agendas");
            try
            {
                _npcAgendaDirector = new NpcAgendaInteractionDirector(agendaDefinitions,
                    _npcAgendaProgress, _playerDiscoveries, _playerRelationships,
                    IsAgendaContractCompleted, IsAgendaUpgradeAcquired, IsAgendaOpportunityInPhase,
                    OpenAgendaWorkshopOffer, () => _clock.Day, () => _clock.MinuteOfDay, _city.SocialWorld);
            }
            catch (ArgumentException exception)
            {
                Debug.LogError("NPC agenda content could not be loaded: " + exception.Message, this);
                _npcAgendaDirector = null;
            }
            _npcAgendaPanel = gameObject.AddComponent<NpcAgendaConversationPanel>();
            _npcAgendaPanel.Bind(HandleAgendaChoice);

            if (parcelItem != null)
            {
                _miloScenario = gameObject.AddComponent<MiloParcelOpportunityController>();
                _miloScenario.Bind(_inventory, parcelItem.ToItemType(),
                    _playerDiscoveries, _playerRelationships, _itemKnowledge, _clock, _city,
                    _miloOpportunity, message => Say(message, 8f));
            }

            _workspace = gameObject.AddComponent<WorkbenchWorkspace>();
            ItemType? repairTargetType = repairTargetItem != null ? repairTargetItem.ToItemType() : (ItemType?)null;
            ItemType? basicKitType = basicPatchKitItem != null ? basicPatchKitItem.ToItemType() : (ItemType?)null;
            ItemType? reinforcedKitType = reinforcedPatchKitItem != null ? reinforcedPatchKitItem.ToItemType() : (ItemType?)null;
            _workspace.Bind(_motor, _inventory, _assetOwnership, _workshopRecipes,
                IsRecipeUnlocked, ProduceRecipe, repairTargetType, basicKitType, reinforcedKitType,
                _itemKnowledge, CanRepairAtWorkbench, RepairItem, CanRepairTarget);
            RefreshWorkshopUpgradeVisual();
            _hud.ShowMessage("Your hideout is ready. The street is just outside.", 5f);
        }

        private void InitializeLivelihoodWorld()
        {
            if (firstCustomerContract == null && firstCustomerRepairOrder == null)
                return;
            string issuerNpcId = firstCustomerContract != null
                ? firstCustomerContract.IssuerNpcId : firstCustomerRepairOrder.IssuerNpcId;
            if (firstCustomerContract != null && !firstCustomerContract.TryValidate(out string contractError))
            {
                Debug.LogError("GameSession: invalid first customer contract: " + contractError);
                return;
            }
            if (firstCustomerRepairOrder != null && !firstCustomerRepairOrder.TryValidate(out string repairError))
            {
                Debug.LogError("GameSession: invalid first customer repair order: " + repairError);
                return;
            }
            if (firstCustomerRepairOrder != null && firstCustomerRepairOrder.IssuerNpcId != issuerNpcId)
            {
                Debug.LogError("GameSession: the first contract and repair order must share an issuer inventory in this slice.");
                return;
            }
            if (!_city.HasResident(issuerNpcId))
            {
                Debug.LogError("GameSession: customer issuer '" + issuerNpcId +
                    "' is not present in the shared city.");
                return;
            }

            _contractReceiverActor = new EconomicActorId("resident:" + issuerNpcId);
            _contractReceiverInventory = new GridInventory(8, 6);
            if (!_economicContainers.TryRegister(_contractReceiverInventory, _contractReceiverActor))
            {
                Debug.LogError("GameSession: could not register the contract receiver inventory.");
                _contractReceiverInventory = null;
                return;
            }
            if (firstCustomerContract != null && !_customerContracts.TryRegisterOffer(firstCustomerContract.Id))
                Debug.LogError("GameSession: the first customer contract could not be registered.");
            if (firstCustomerRepairOrder != null)
            {
                if (!_customerRepairOrders.TryRegisterOffer(firstCustomerRepairOrder.Id,
                    firstCustomerRepairOrder.IssuerActor, firstCustomerRepairOrder.EntrustedItem.ToItemType().Id,
                    firstCustomerRepairOrder.CleanCardReward, firstCustomerRepairOrder.RequiredReturnCondition))
                    Debug.LogError("GameSession: the first customer repair order could not be registered.");
                _playerDevelopment = PlayerDevelopmentSaveMapper.Restore(data, ProvisionalPlayerSkillIds,
                PrototypeStartingSkillPoints, PrototypeMaximumSkillRank, PrototypePointsPerSkillRank);
            if (_playerDevelopmentPanel != null)
                _playerDevelopmentPanel.Bind(_playerDevelopment);
            EnsureCustomerRepairItem();
            }
        }

        private string GetCustomerDisplayName()
        {
            string npcId = firstCustomerContract != null ? firstCustomerContract.IssuerNpcId :
                firstCustomerRepairOrder != null ? firstCustomerRepairOrder.IssuerNpcId : null;
            if (_city != null && !string.IsNullOrWhiteSpace(npcId))
                foreach (NpcPresenter resident in _city.DebugResidents)
                    if (resident != null && resident.NpcId == npcId)
                        return resident.DisplayName;
            if (firstCustomerContract != null) return firstCustomerContract.DisplayName;
            return firstCustomerRepairOrder != null ? firstCustomerRepairOrder.DisplayName : "Customer";
        }

        private bool IsNearContractCustomer(Vector2 playerPosition)
        {
            return firstCustomerContract != null && _contractReceiverInventory != null && _city != null &&
                _city.IsResidentWithin(firstCustomerContract.IssuerNpcId, playerPosition, InteractionDistance);
        }

        private CustomerContractPhase GetCustomerContractPhase()
        {
            if (firstCustomerContract != null &&
                _customerContracts.TryGetState(firstCustomerContract.Id, out CustomerContractState state))
                return state.Phase;
            return CustomerContractPhase.Offered;
        }

        private CustomerRepairOrderPhase GetCustomerRepairOrderPhase()
        {
            return firstCustomerRepairOrder != null &&
                _customerRepairOrders.TryGetState(firstCustomerRepairOrder.Id, out CustomerRepairOrderState state)
                ? state.Phase : CustomerRepairOrderPhase.Offered;
        }

        private bool IsNearRepairOrderCustomer(Vector2 playerPosition)
        {
            return firstCustomerRepairOrder != null && _customerRepairOrderPanel != null && _contractReceiverInventory != null && _city != null &&
                _city.IsResidentWithin(firstCustomerRepairOrder.IssuerNpcId, playerPosition, InteractionDistance);
        }

        private void EnsureCustomerRepairItem()
        {
            if (firstCustomerRepairOrder == null || _contractReceiverInventory == null ||
                !_customerRepairOrders.TryGetState(firstCustomerRepairOrder.Id, out CustomerRepairOrderState order) ||
                order.Phase != CustomerRepairOrderPhase.Offered)
                return;
            ItemType type = firstCustomerRepairOrder.EntrustedItem.ToItemType();
            foreach (PlacedItem existing in _contractReceiverInventory.Items)
                if (existing.Type == type && _assetOwnership.TryGetOwner(existing.ItemInstanceId, out EconomicActorId existingOwner) &&
                    existingOwner == firstCustomerRepairOrder.IssuerActor)
                    return;

            ItemInstanceState defaults = firstCustomerRepairOrder.EntrustedItem.DefaultInstanceState;
            var state = new ItemInstanceState(defaults.Quality, firstCustomerRepairOrder.StartingCondition,
                defaults.Authenticity, ItemOwnershipStatus.Legitimate);
            int? placedInstanceId = null;
            for (int y = 0; y < _contractReceiverInventory.Height && !placedInstanceId.HasValue; y++)
                for (int x = 0; x < _contractReceiverInventory.Width && !placedInstanceId.HasValue; x++)
                    placedInstanceId = _contractReceiverInventory.Place(type, x, y, state: state);
            if (!placedInstanceId.HasValue ||
                !_contractReceiverInventory.TryGet(placedInstanceId.Value, out PlacedItem placed) ||
                !_assetOwnership.TryRegister(placed.ItemInstanceId, firstCustomerRepairOrder.IssuerActor))
                Debug.LogError("GameSession: could not seed the customer's entrusted Work Jacket.");
        }

        private string AcceptCustomerRepairOrder()
        {
            if (!IsNearRepairOrderCustomer(_room.PlayerBody.position)) return "Move closer to the customer.";
            if (firstCustomerRepairOrder == null || _contractReceiverInventory == null)
                return "The repair order is unavailable.";
            if (_customerContracts != null && _customerContracts.HasActiveContract)
                return "Complete your active customer request before accepting a repair order.";
            CustomerRepairOrderResult result = CustomerRepairOrderService.TryAccept(firstCustomerRepairOrder.Id,
                firstCustomerRepairOrder.EntrustedItem.ToItemType(), _contractReceiverInventory, _inventory,
                _economicContainers, _assetOwnership, _customerRepairOrders, _clock.Day, _clock.MinuteOfDay);
            if (result.Success)
            {
                if (_workspace != null) _workspace.Refresh();
                return "Entrusted jacket received. It remains the customer's property. Repair it at your hideout workbench.";
            }
            switch (result.Status)
            {
                case CustomerRepairOrderStatus.ItemNotFound: return "The customer's jacket is unavailable.";
                case CustomerRepairOrderStatus.ItemNotRepairable: return "The customer's jacket does not need this repair.";
                case CustomerRepairOrderStatus.OwnershipMismatch: return "The customer ownership record could not be verified.";
                case CustomerRepairOrderStatus.InventoryFull: return "Your inventory has no room for the jacket.";
                default: return "The repair order could not be accepted. No ownership changed.";
            }
        }

        private string ReturnCustomerRepairOrder()
        {
            if (!IsNearRepairOrderCustomer(_room.PlayerBody.position)) return "Move closer to the customer.";
            if (firstCustomerRepairOrder == null || _contractReceiverInventory == null)
                return "The repair order is unavailable.";
            CustomerRepairOrderResult result = CustomerRepairOrderService.TryReturn(firstCustomerRepairOrder.Id,
                firstCustomerRepairOrder.EntrustedItem.ToItemType(), _inventory, _contractReceiverInventory,
                _economicContainers, _assetOwnership, _stats.CleanMoney, _economicAccounts,
                _servicePaymentTransactions, _customerRepairOrders, _clock.Day, _clock.MinuteOfDay);
            if (result.Success)
            {
                if (_workspace != null) _workspace.Refresh();
                return "Jacket returned. Received " + firstCustomerRepairOrder.CleanCardReward +
                    " clean CARD. Service payment recorded.";
            }
            switch (result.Status)
            {
                case CustomerRepairOrderStatus.ItemNotRestored: return "Restore the jacket to " + firstCustomerRepairOrder.RequiredReturnCondition + " before returning it.";
                case CustomerRepairOrderStatus.InsufficientFunds: return "The customer cannot currently afford the agreed service fee.";
                case CustomerRepairOrderStatus.CustomerAccountUnavailable: return "The customer's CARD account is unavailable.";
                case CustomerRepairOrderStatus.PlayerAccountUnavailable: return "Your CARD account is unavailable.";
                case CustomerRepairOrderStatus.InventoryFull: return "The customer has no room to receive the jacket.";
                default: return "Return failed safely. The jacket and both CARD balances were left unchanged.";
            }
        }

        private bool CanRepairTarget(Guid itemInstanceId)
        {
            if (!_assetOwnership.TryGetOwner(itemInstanceId, out EconomicActorId owner)) return false;
            if (firstCustomerRepairOrder != null &&
                _customerRepairOrders.TryGetState(firstCustomerRepairOrder.Id, out CustomerRepairOrderState order) &&
                order.Phase == CustomerRepairOrderPhase.Active)
                return order.ItemInstanceId == itemInstanceId && owner == order.Issuer;
            return owner == EconomicActorId.Player;
        }

        private string AcceptCustomerContract()
        {
            if (!IsNearContractCustomer(_room.PlayerBody.position))
                return "Move closer to the customer.";
            if (_customerRepairOrders != null && _customerRepairOrders.HasActiveOrder)
                return "Finish the entrusted repair order before accepting another customer job.";
            if (firstCustomerContract == null ||
                !_customerContracts.TryAccept(firstCustomerContract.Id, _clock.Day, _clock.MinuteOfDay))
                return "You already have an active request, or this offer is no longer available.";
            return "Request accepted. Bring the exact goods to the customer.";
        }

        private string FulfilCustomerContract(Guid itemInstanceId)
        {
            if (!IsNearContractCustomer(_room.PlayerBody.position))
                return "Move closer to the customer before handing over goods.";
            if (firstCustomerContract == null || _contractReceiverInventory == null)
                return "The customer request is unavailable.";
            if (!_economicAccounts.TryGetAccount(firstCustomerContract.IssuerActor, out CleanMoneyAccount payer))
                return "The customer's CARD account is unavailable.";

            CustomerContractFulfilmentResult result = CustomerContractService.TryFulfil(
                firstCustomerContract.Id, itemInstanceId, firstCustomerContract.RequiredItem.ToItemType(),
                firstCustomerContract.RequiredQuantity, firstCustomerContract.AcceptStolenGoods,
                firstCustomerContract.CleanCardReward, _contractReceiverActor, firstCustomerContract.IssuerActor,
                _inventory, _contractReceiverInventory, _assetOwnership, _assetTransfers,
                _economicTransactions, _stats.CleanMoney, payer, _customerContracts,
                _clock.Day, _clock.MinuteOfDay, _playerRelationships,
                firstCustomerContract.IssuerNpcId,
                firstCustomerContract.PlayerFamiliarityDelta,
                firstCustomerContract.PlayerTrustDelta,
                firstCustomerContract.PlayerAffinityDelta,
                _city != null ? _city.SocialWorld : null,
                firstCustomerContract.IssuerNpcId,
                firstCustomerContract.IssuerFamiliarityDelta,
                firstCustomerContract.IssuerTrustDelta,
                firstCustomerContract.IssuerAffinityDelta);

            if (result.Success)
                return "Goods handed over. Received " + result.Reward + " clean CARD.";
            switch (result.Status)
            {
                case CustomerContractFulfilmentStatus.ContractNotActive: return "Accept this request before handing over goods.";
                case CustomerContractFulfilmentStatus.ItemNotFound: return "That stack is no longer available.";
                case CustomerContractFulfilmentStatus.WrongItem: return "The customer needs " + firstCustomerContract.RequiredItem.DisplayName + ".";
                case CustomerContractFulfilmentStatus.WrongQuantity: return "The customer requires one complete stack of exactly " + firstCustomerContract.RequiredQuantity + ".";
                case CustomerContractFulfilmentStatus.OwnershipMismatch: return "You must hand over goods that you own.";
                case CustomerContractFulfilmentStatus.StolenGoodsNotAccepted: return "This customer will not accept stolen goods.";
                case CustomerContractFulfilmentStatus.InsufficientFunds: return "The customer does not have enough CARD to pay the agreed reward.";
                case CustomerContractFulfilmentStatus.ReceiverFull: return "The customer has no room to receive those goods.";
                default: return "Handover failed. Your goods and balances were left unchanged.";
            }
        }

        private string ProduceRecipe(Guid recipeId)
        {
            if (!_city.IsAtHome || _room.Workbench == null ||
                Vector2.Distance(_room.PlayerBody.position, _room.Workbench.position) > 2.15f)
                return "Use the production station in your hideout.";
            RecipeDefinition definition = FindRecipeDefinition(recipeId);
            if (definition == null || !IsRecipeUnlocked(definition) || !definition.TryValidate(out _))
                return "That recipe is unavailable.";
            ProductionResult result = ProductionService.TryProduce(recipeId, definition.ToRecipe(),
                _inventory, EconomicActorId.Player, _economicContainers, _assetOwnership, _productionTransactions,
                _clock.Day, _clock.MinuteOfDay, definition.Output.item.DefaultInstanceState);
            if (result.Success)
                return "Produced " + definition.Output.count + " " + definition.Output.item.DisplayName + ".";
            switch (result.Status)
            {
                case ProductionStatus.MissingInputs: return "You do not carry enough ingredients.";
                case ProductionStatus.OwnershipMismatch: return "Production only uses ingredients you own.";
                case ProductionStatus.NoSpaceForOutput: return "Not enough room for the produced goods. Ingredients were returned.";
                default: return "Production failed. Your inventory was left unchanged.";
            }
        }

        private bool CanRepairAtWorkbench()
        {
            return IsFirstUpgradeAcquired() && _city != null && _city.IsAtHome &&
                _room != null && _room.Workbench != null && _room.PlayerBody != null &&
                Vector2.Distance(_room.PlayerBody.position, _room.Workbench.position) <= 2.15f;
        }

        private string RepairItem(Guid targetInstanceId, Guid kitInstanceId)
        {
            if (!CanRepairAtWorkbench())
                return "Install the Compact Sewing Machine and use the hideout workbench before repairing.";
            if (repairTargetItem == null || basicPatchKitItem == null || reinforcedPatchKitItem == null)
                return "Repair content is not configured. No items were consumed.";

            EconomicActorId targetOwner = EconomicActorId.Player;
            if (_customerRepairOrders != null && firstCustomerRepairOrder != null &&
                _customerRepairOrders.TryGetState(firstCustomerRepairOrder.Id, out CustomerRepairOrderState order) &&
                order.Phase == CustomerRepairOrderPhase.Active && order.ItemInstanceId == targetInstanceId)
                targetOwner = order.Issuer;
            RepairResult result = RepairService.TryRepairForOwner(targetInstanceId, kitInstanceId,
                repairTargetItem.ToItemType(), basicPatchKitItem.ToItemType(), reinforcedPatchKitItem.ToItemType(),
                _inventory, EconomicActorId.Player, targetOwner, _economicContainers, _assetOwnership,
                _itemKnowledge, _clock.Day, _clock.MinuteOfDay);

            if (result.Success)
                return "Work Jacket restored to " + result.ResultingCondition + ".";

            switch (result.Status)
            {
                case RepairStatus.ContainerNotOwned: return "Repairs can only be performed on your own hideout workbench.";
                case RepairStatus.TargetNotFound: return "The Work Jacket is no longer in your carried inventory.";
                case RepairStatus.UnsupportedTarget: return "Only the Work Jacket is supported by this first textile repair service.";
                case RepairStatus.KitNotFound: return "The selected patch kit is no longer in your inventory.";
                case RepairStatus.UnsupportedKit: return "That is not a supported physical patch kit.";
                case RepairStatus.OwnershipMismatch: return "You must own both the Work Jacket and the patch kit.";
                case RepairStatus.ConditionNotRepairable: return "This Work Jacket's condition cannot be improved further.";
                case RepairStatus.KitCannotRepairCondition: return "Use a Basic Patch Kit for a Worn jacket. The Reinforced kit is for Damaged jackets.";
                case RepairStatus.InventoryConflict: return "Repair failed safely. Your inventory and ownership were restored.";
                default: return "Repair setup is invalid. No items were consumed.";
            }
        }

        private RecipeDefinition FindRecipeDefinition(Guid id)
        {
            foreach (RecipeDefinition recipe in _workshopRecipes)
                if (recipe != null && recipe.Id == id)
                    return recipe;
            return null;
        }

        private bool IsRecipeUnlocked(RecipeDefinition recipe)
        {
            if (recipe == null)
                return false;
            if (startingWorkshopRecipes != null)
                foreach (RecipeDefinition starting in startingWorkshopRecipes)
                    if (starting == recipe)
                        return true;
            return firstWorkshopUpgrade != null && firstWorkshopUpgrade.UnlockedRecipe == recipe &&
                _workshopProgression != null && _workshopProgression.HasAcquired(firstWorkshopUpgrade.Id);
        }

        private void RefreshWorkshopUpgradeVisual()
        {
            if (_installedUpgradeVisual != null)
            {
                Destroy(_installedUpgradeVisual);
                _installedUpgradeVisual = null;
            }
            if (!IsFirstUpgradeAcquired() || firstWorkshopUpgrade.InstalledPropPrefab == null ||
                _room == null || _room.Workbench == null)
                return;

            _installedUpgradeVisual = Instantiate(firstWorkshopUpgrade.InstalledPropPrefab,
                _room.Workbench, false);
            _installedUpgradeVisual.transform.localPosition = firstWorkshopUpgrade.InstalledPropLocalOffset;
        }

        private bool IsFirstUpgradeAcquired()
        {
            return firstWorkshopUpgrade != null && _workshopProgression != null &&
                _workshopProgression.HasAcquired(firstWorkshopUpgrade.Id);
        }

        private bool CanAcquireFirstUpgrade()
        {
            if (firstWorkshopUpgrade == null || !firstWorkshopUpgrade.TryValidate(out _) ||
                _room == null || _room.PlayerBody == null ||
                !IsNearFirstUpgradeSupplier(_room.PlayerBody.position) || IsFirstUpgradeAcquired())
                return false;
            if (firstWorkshopUpgrade.PrerequisiteMode == WorkshopUpgradePrerequisiteMode.CompletedContract &&
                (firstWorkshopUpgrade.PrerequisiteContract == null ||
                 !_customerContracts.TryGetState(firstWorkshopUpgrade.PrerequisiteContract.Id,
                    out CustomerContractState contract) || contract.Phase != CustomerContractPhase.Completed))
                return false;
            return _economicAccounts.TryGetAccount(firstWorkshopUpgrade.PayeeActor, out CleanMoneyAccount payee) &&
                _stats.CleanMoney.Balance >= firstWorkshopUpgrade.CleanCardCost &&
                payee.Balance <= int.MaxValue - firstWorkshopUpgrade.CleanCardCost;
        }

        private bool IsNearFirstUpgradeSupplier(Vector2 playerPosition)
        {
            return firstWorkshopUpgrade != null && _city != null &&
                _city.IsResidentWithin(firstWorkshopUpgrade.PayeeNpcId, playerPosition, InteractionDistance);
        }

        private string GetFirstUpgradeAvailabilityMessage()
        {
            if (firstWorkshopUpgrade == null)
                return "Rhea has no workshop offer available.";
            if (!firstWorkshopUpgrade.TryValidate(out string error))
                return "Offer configuration error: " + error;
            if (IsFirstUpgradeAcquired())
                return "Purchased. The machine is installed at your hideout workbench and the Reinforced Patch Kit recipe is unlocked.";
            if (firstWorkshopUpgrade.PrerequisiteMode == WorkshopUpgradePrerequisiteMode.CompletedContract &&
                (firstWorkshopUpgrade.PrerequisiteContract == null ||
                 !_customerContracts.TryGetState(firstWorkshopUpgrade.PrerequisiteContract.Id,
                     out CustomerContractState contract) ||
                 contract.Phase != CustomerContractPhase.Completed))
                return "Not available yet. Complete Milo's customer request first.";
            if (!_economicAccounts.TryGetAccount(firstWorkshopUpgrade.PayeeActor, out CleanMoneyAccount payee))
                return "Rhea's CARD account is unavailable.";
            if (_stats.CleanMoney.Balance < firstWorkshopUpgrade.CleanCardCost)
                return "You need " + firstWorkshopUpgrade.CleanCardCost + " CARD. You currently have " +
                    _stats.CleanMoney.Balance + " CARD.";
            if (payee.Balance > int.MaxValue - firstWorkshopUpgrade.CleanCardCost)
                return "Rhea cannot receive this payment right now.";
            return "Available now. You have " + _stats.CleanMoney.Balance + " CARD.";
        }

        private string AcquireFirstUpgrade()
        {
            string error = firstWorkshopUpgrade == null ? "No upgrade asset is assigned." : null;
            if (firstWorkshopUpgrade == null || !firstWorkshopUpgrade.TryValidate(out error))
                return "The workshop upgrade is not configured: " + error;
            if (!IsNearFirstUpgradeSupplier(_room.PlayerBody.position))
                return "Talk to " + _city.GetResidentDisplayName(firstWorkshopUpgrade.PayeeNpcId) +
                    " to purchase this workshop upgrade.";
            if (!CanAcquireFirstUpgrade())
                return "The upgrade is already installed, its prerequisite is incomplete, or the required CARD is unavailable.";
            if (!_economicAccounts.TryGetAccount(firstWorkshopUpgrade.PayeeActor, out CleanMoneyAccount payee))
                return "The upgrade supplier's CARD account is unavailable.";
            Guid prerequisiteId = firstWorkshopUpgrade.PrerequisiteMode == WorkshopUpgradePrerequisiteMode.CompletedContract
                ? firstWorkshopUpgrade.PrerequisiteContract.Id : Guid.Empty;
            WorkshopUpgradeResult result = WorkshopUpgradeService.TryAcquire(
                firstWorkshopUpgrade.Id, prerequisiteId,
                firstWorkshopUpgrade.CleanCardCost, firstWorkshopUpgrade.PayeeActor,
                _stats.CleanMoney, payee, _customerContracts, _workshopProgression);
            switch (result.Status)
            {
                case WorkshopUpgradeStatus.Acquired:
                    RefreshWorkshopUpgradeVisual();
                    return firstWorkshopUpgrade.DisplayName + " installed. " +
                        firstWorkshopUpgrade.UnlockedRecipe.DisplayName + " is now unlocked.";
                case WorkshopUpgradeStatus.AlreadyAcquired: return "This workshop upgrade is already installed.";
                case WorkshopUpgradeStatus.ContractPrerequisiteNotMet: return "Complete the customer request first.";
                case WorkshopUpgradeStatus.InsufficientFunds: return "Not enough CARD to install this upgrade.";
                default: return "The upgrade could not be installed. No payment was taken.";
            }
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
            {
                _debugMenuOpen = !_debugMenuOpen;
                if (!_debugMenuOpen && _debugItemSpawnerPanel != null)
                    _debugItemSpawnerPanel.SetOpen(false);
                if (_debugMenuOpen && _playerDevelopmentPanel != null)
                    _playerDevelopmentPanel.SetOpen(false);
            }

            if (keyboard != null && keyboard.f2Key.wasPressedThisFrame && !_debugMenuOpen &&
                _playerDevelopmentPanel != null)
                _playerDevelopmentPanel.SetOpen(!_playerDevelopmentPanel.IsOpen);

            bool workspaceOpen = _workspace != null && _workspace.IsOpen;
            bool inventoryOpen = _panel != null && _panel.IsOpen;
            bool storageOpen = _storagePanel != null && _storagePanel.IsOpen;
            bool itemViewerOpen = _itemViewer != null && _itemViewer.IsOpen;
            bool retailOpen = _retailPanel != null && _retailPanel.IsOpen;
            bool containerOpen = _containerPanel != null && _containerPanel.IsOpen;
            bool dirtyCashOpen = _dirtyCashPanel != null && _dirtyCashPanel.IsOpen;
            bool contractOpen = _customerContractPanel != null && _customerContractPanel.IsOpen;
            bool repairOrderOpen = _customerRepairOrderPanel != null && _customerRepairOrderPanel.IsOpen;
            bool upgradeOpen = _workshopUpgradePanel != null && _workshopUpgradePanel.IsOpen;
            bool agendaOpen = _npcAgendaPanel != null && _npcAgendaPanel.IsOpen;
            bool itemSpawnerOpen = _debugItemSpawnerPanel != null && _debugItemSpawnerPanel.IsOpen;
            if (_panel != null)
                _panel.enabled = !_debugMenuOpen && !workspaceOpen && !storageOpen && !itemViewerOpen && !retailOpen && !containerOpen && !dirtyCashOpen && !contractOpen && !repairOrderOpen && !upgradeOpen && !agendaOpen;
            if (_storagePanel != null)
                _storagePanel.enabled = !_debugMenuOpen && !itemViewerOpen && !retailOpen && !containerOpen && !dirtyCashOpen && !contractOpen && !repairOrderOpen && !upgradeOpen && !agendaOpen;
            if (_containerPanel != null)
                _containerPanel.enabled = !_debugMenuOpen && !itemViewerOpen && !dirtyCashOpen && !contractOpen && !repairOrderOpen && !upgradeOpen && !agendaOpen;
            if (_dirtyCashPanel != null)
                _dirtyCashPanel.enabled = !_debugMenuOpen && !upgradeOpen && !agendaOpen;
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
            if (_debugMenuOpen && repairOrderOpen)
            {
                _customerRepairOrderPanel.Close();
                repairOrderOpen = false;
            }
            if (_debugMenuOpen && upgradeOpen)
            {
                _workshopUpgradePanel.Close();
                upgradeOpen = false;
            }
            if (_debugMenuOpen && agendaOpen)
            {
                _npcAgendaPanel.Close();
                agendaOpen = false;
            }

            UpdateNpcInteractionPause(
                agendaOpen ? _npcAgendaPanel.ActiveNpcId :
                retailOpen ? _retailPanel.ActiveOwnerNpcId :
                repairOrderOpen && firstCustomerRepairOrder != null ? firstCustomerRepairOrder.IssuerNpcId :
                contractOpen && firstCustomerContract != null ? firstCustomerContract.IssuerNpcId :
                upgradeOpen && firstWorkshopUpgrade != null ? firstWorkshopUpgrade.PayeeNpcId :
                dirtyCashOpen ? DirtyCashContactNpcId : null);

            bool scenarioChoiceOpen = _miloScenario != null && _miloScenario.HasPendingChoice;
            _motor.enabled = !inventoryOpen && !workspaceOpen && !storageOpen && !itemViewerOpen &&
                !scenarioChoiceOpen && !retailOpen && !containerOpen && !dirtyCashOpen && !contractOpen && !repairOrderOpen && !upgradeOpen && !agendaOpen && !itemSpawnerOpen && !_debugMenuOpen &&
                (_playerDevelopmentPanel == null || !_playerDevelopmentPanel.IsOpen);

            Vector2 playerPosition = _room.Player.position;
            bool nearWorkbench = _room.Workbench != null &&
                Vector2.Distance(playerPosition, _room.Workbench.position) <= 2.15f;
            bool nearStorage = _city.IsAtHome && _room.Storage != null &&
                Vector2.Distance(playerPosition, _room.Storage.position) <= 2.15f;
            BusinessDefinition nearbyBusiness = FindNearbyBusiness(playerPosition);
            bool nearDirtyCashContact = _city.IsResidentWithin(DirtyCashContactNpcId, playerPosition, InteractionDistance);
            bool nearContractCustomer = IsNearContractCustomer(playerPosition);
            bool nearRepairOrderCustomer = IsNearRepairOrderCustomer(playerPosition);
            bool nearUpgradeSupplier = IsNearFirstUpgradeSupplier(playerPosition);
            if ((nearContractCustomer || nearRepairOrderCustomer) && nearUpgradeSupplier)
            {
                // Milo and Rhea can share the CornerShop work area; route E/F to the closest resident.
                string customerNpcId = firstCustomerContract != null
                    ? firstCustomerContract.IssuerNpcId : firstCustomerRepairOrder.IssuerNpcId;
                Vector2 customerPosition = _city.GetResidentPosition(customerNpcId);
                Vector2 supplierPosition = _city.GetResidentPosition(firstWorkshopUpgrade.PayeeNpcId);
                float customerDistance = (playerPosition - customerPosition).sqrMagnitude;
                float supplierDistance = (playerPosition - supplierPosition).sqrMagnitude;
                if (supplierDistance <= customerDistance)
                {
                    nearContractCustomer = false;
                    nearRepairOrderCustomer = false;
                }
                else
                    nearUpgradeSupplier = false;
            }
            bool nearSearchableContainer = _city.TryGetNearbySearchableContainer(playerPosition,
                out string nearbyContainerId, out string nearbyContainerLabel);
            ResidentPropertyContainerDefinition nearbyProperty = FindNearbyResidentProperty(playerPosition);
            if (nearbyProperty != null)
            {
                nearSearchableContainer = true;
                nearbyContainerId = nearbyProperty.ContainerId;
                nearbyContainerLabel = nearbyProperty.DisplayName;
            }
            bool nearAgendaInteraction = CanStartAgendaInteraction(playerPosition);
            string cityPrompt = _city.GetPrompt(playerPosition);
            bool hasScenarioPrompt = false;
            if (nearSearchableContainer)
                cityPrompt = "E  Search " + nearbyContainerLabel;
            if (nearDirtyCashContact)
                cityPrompt = "E  Talk";
            if (nearContractCustomer && nearRepairOrderCustomer)
                cityPrompt = "E  Customer request   /   F  Repair order";
            else if (nearContractCustomer)
                cityPrompt = "E  Customer request";
            else if (nearRepairOrderCustomer)
                cityPrompt = "F  Repair order";
            if (nearUpgradeSupplier)
                cityPrompt = "E  Talk to " + _city.GetResidentDisplayName(firstWorkshopUpgrade.PayeeNpcId);
            if (_miloScenario != null)
            {
                if (!workspaceOpen && !inventoryOpen && !storageOpen && !itemViewerOpen && !retailOpen && !containerOpen && !dirtyCashOpen && !contractOpen && !repairOrderOpen && !upgradeOpen && !agendaOpen)
                    _miloScenario.Tick(playerPosition, _city.CurrentPlace);
                string scenarioPrompt = _miloScenario.GetPrompt(playerPosition, _city.CurrentPlace);
                if (!string.IsNullOrEmpty(scenarioPrompt))
                {
                    cityPrompt = scenarioPrompt;
                    hasScenarioPrompt = true;
                }
            }

            if (nearbyBusiness != null && !nearSearchableContainer && !hasScenarioPrompt)
                cityPrompt = nearbyBusiness.IsOpen(_clock.MinuteOfDay)
                    ? "E  Browse " + nearbyBusiness.DisplayName
                    : nearbyBusiness.DisplayName + "  —  Closed";
            if (nearSearchableContainer)
                cityPrompt = "E  Search " + nearbyContainerLabel;
            if (nearDirtyCashContact)
                cityPrompt = "E  Talk";
            if (nearContractCustomer && nearRepairOrderCustomer)
                cityPrompt = "E  Customer request   /   F  Repair order";
            else if (nearContractCustomer)
                cityPrompt = "E  Customer request";
            else if (nearRepairOrderCustomer)
                cityPrompt = "F  Repair order";
            if (nearUpgradeSupplier)
                cityPrompt = "E  Talk to " + _city.GetResidentDisplayName(firstWorkshopUpgrade.PayeeNpcId);
            if (!hasScenarioPrompt && !nearSearchableContainer && !nearDirtyCashContact &&
                !nearContractCustomer && !nearRepairOrderCustomer && !nearUpgradeSupplier && nearAgendaInteraction)
                cityPrompt = "E  Talk";
            if (repairOrderOpen)
                _hud.SetPrompt("ESC  Close repair order");
            else if (contractOpen)
                _hud.SetPrompt("ESC  Close customer request");
            else if (upgradeOpen)
                _hud.SetPrompt("ESC  Close workshop offer");
            else if (agendaOpen)
                _hud.SetPrompt("ESC  Close conversation");
            else if (dirtyCashOpen)
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
            else if (hasScenarioPrompt && !nearSearchableContainer && !nearDirtyCashContact &&
                !nearContractCustomer && !nearUpgradeSupplier)
                _hud.SetPrompt(cityPrompt);
            else if (!hasScenarioPrompt && nearAgendaInteraction && !nearSearchableContainer &&
                !nearDirtyCashContact && !nearContractCustomer && !nearUpgradeSupplier)
                _hud.SetPrompt("E  Talk");
            else if (nearWorkbench)
                _hud.SetPrompt("E  Use workbench");
            else if (nearStorage)
                _hud.SetPrompt("E  Open home storage");
            else
                _hud.SetPrompt(cityPrompt);

            TickClock();

            if (_debugMenuOpen || keyboard == null)
                return;

            if (!workspaceOpen && !inventoryOpen && !storageOpen && !itemViewerOpen && !retailOpen && !containerOpen && !dirtyCashOpen && !contractOpen && !repairOrderOpen && !upgradeOpen && !agendaOpen &&
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
                !dirtyCashOpen && !contractOpen && !repairOrderOpen && !upgradeOpen && !agendaOpen &&
                nearRepairOrderCustomer && keyboard.fKey.wasPressedThisFrame)
            {
                _customerRepairOrderPanel.Open();
                _motor.enabled = false;
            }
            else if (!workspaceOpen && !inventoryOpen && !storageOpen && !itemViewerOpen && !retailOpen && !containerOpen &&
                !dirtyCashOpen && !contractOpen && !repairOrderOpen && !upgradeOpen && !agendaOpen &&
                nearContractCustomer && keyboard.eKey.wasPressedThisFrame)
            {
                _customerContractPanel.Open();
            }
            else if (!workspaceOpen && !inventoryOpen && !storageOpen && !itemViewerOpen && !retailOpen && !containerOpen &&
                !dirtyCashOpen && !contractOpen && !repairOrderOpen && !upgradeOpen && !agendaOpen && nearUpgradeSupplier && keyboard.eKey.wasPressedThisFrame)
            {
                if (!TryOpenAgendaConversationForNpc(firstWorkshopUpgrade.PayeeNpcId) &&
                    _workshopUpgradePanel != null)
                {
                    _workshopUpgradePanel.Open();
                    _motor.enabled = false;
                }
            }
            else if (!workspaceOpen && !inventoryOpen && !storageOpen && !itemViewerOpen && !retailOpen && !containerOpen &&
                !dirtyCashOpen && !contractOpen && !repairOrderOpen && !upgradeOpen && !agendaOpen && hasScenarioPrompt &&
                !nearContractCustomer && !nearUpgradeSupplier && !nearDirtyCashContact &&
                !nearSearchableContainer && keyboard.eKey.wasPressedThisFrame &&
                _miloScenario != null && _miloScenario.TryInteract(playerPosition, _city.CurrentPlace, out string scenarioPromptMessage))
            {
                RegisterUnownedPlayerAssets(_inventory);
                Say(scenarioPromptMessage, 8f);
            }
            else if (!workspaceOpen && !inventoryOpen && !storageOpen && !itemViewerOpen && !retailOpen && !containerOpen &&
                !dirtyCashOpen && !contractOpen && !repairOrderOpen && !upgradeOpen && !agendaOpen && !hasScenarioPrompt && nearAgendaInteraction &&
                !nearContractCustomer && !nearUpgradeSupplier && !nearDirtyCashContact &&
                !nearSearchableContainer && keyboard.eKey.wasPressedThisFrame)
            {
                if (!TryOpenAgendaConversation(playerPosition) &&
                    _city.TryInteractNpc(playerPosition, out string agendaFallbackDialogue))
                    Say(agendaFallbackDialogue, 5f);
            }
            else if (!workspaceOpen && !inventoryOpen && !storageOpen && !itemViewerOpen && !retailOpen && !containerOpen &&
                !dirtyCashOpen && !contractOpen && !repairOrderOpen && !upgradeOpen && !agendaOpen && nearDirtyCashContact && keyboard.eKey.wasPressedThisFrame)
            {
                if (_dirtyCashPanel != null) _dirtyCashPanel.Open();
            }
            else if (!workspaceOpen && !inventoryOpen && !storageOpen && !itemViewerOpen && !retailOpen && !containerOpen &&
                !dirtyCashOpen && !contractOpen && !repairOrderOpen && !upgradeOpen && !agendaOpen && nearSearchableContainer && keyboard.eKey.wasPressedThisFrame)
            {
                OpenSearchableContainer(nearbyContainerId);
            }
            else if (!workspaceOpen && !inventoryOpen && !storageOpen && !itemViewerOpen && !retailOpen && !containerOpen && !dirtyCashOpen && !contractOpen && !repairOrderOpen && !upgradeOpen && !agendaOpen &&
                nearbyBusiness != null && keyboard.eKey.wasPressedThisFrame)
            {
                if (!nearbyBusiness.TryValidate(out string businessError))
                    Say("Business configuration error: " + businessError, 6f);
                else if (!nearbyBusiness.IsOpen(_clock.MinuteOfDay))
                    Say(nearbyBusiness.DisplayName + " is closed.");
                else
                    _retailPanel.Open(nearbyBusiness);
            }
            else if (!workspaceOpen && !inventoryOpen && !storageOpen && !itemViewerOpen && !retailOpen && !containerOpen && !dirtyCashOpen && !contractOpen && !repairOrderOpen && !upgradeOpen && !agendaOpen && nearWorkbench && keyboard.eKey.wasPressedThisFrame)
                _workspace.Open();
            else if (!workspaceOpen && !inventoryOpen && !storageOpen && !itemViewerOpen && !retailOpen && !containerOpen && !dirtyCashOpen && !contractOpen && !repairOrderOpen && !upgradeOpen && !agendaOpen && nearStorage && keyboard.eKey.wasPressedThisFrame)
                _storagePanel.SetOpen(true);
            else if (!workspaceOpen && !inventoryOpen && !storageOpen && !itemViewerOpen && !retailOpen && !containerOpen &&
                     !dirtyCashOpen && !contractOpen && !repairOrderOpen && !upgradeOpen && !agendaOpen && !nearDirtyCashContact && !nearContractCustomer && !nearUpgradeSupplier && !nearSearchableContainer && nearbyBusiness == null && !nearWorkbench && !nearStorage &&
                     cityPrompt.Length > 0 && keyboard.eKey.wasPressedThisFrame)
            {
                if (_miloScenario != null &&
                    _miloScenario.TryInteract(playerPosition, _city.CurrentPlace, out string scenarioMessage))
                {
                    RegisterUnownedPlayerAssets(_inventory);
                    Say(scenarioMessage, 8f);
                }
                else if (TryOpenAgendaConversation(playerPosition))
                {
                    // The agenda panel now owns this interaction until the player chooses or closes it.
                }
                else if (_city.TryInteractNpc(playerPosition, out string npcDialogue))
                    Say(npcDialogue, 5f);
                else if (_city.TryInteract(playerPosition))
                    Say("Location: " + _city.CurrentPlace);
            }

            if (!retailOpen && !dirtyCashOpen && !upgradeOpen && !agendaOpen)
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
            GUILayout.Label("ITEM TESTING");
            if (GUILayout.Button("Open item spawner (" + (_debugItemSpawnerPanel != null ? _debugItemSpawnerPanel.ItemCount : _allItemDefinitions.Count) + " catalogue items)"))
                _debugItemSpawnerPanel.SetOpen(true);

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

        private string SpawnDebugItem(ItemDefinition definition)
        {
            if (definition == null)
                return "That item definition is unavailable.";

            ItemType type;
            try
            {
                type = definition.ToItemType();
            }
            catch (InvalidOperationException)
            {
                return "This item has no valid stable ID.";
            }

            string displayName = string.IsNullOrWhiteSpace(definition.DisplayName)
                ? definition.name : definition.DisplayName;
            int remaining = _inventory.Add(type, 1, definition.DefaultInstanceState);
            if (remaining > 0)
                return "Inventory full. No " + displayName + " was added.";

            RegisterUnownedPlayerAssets(_inventory);
            return "Added " + displayName + " to your inventory.";
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
                                               _contactInventory, _illicitExchanges, _dirtyCashCleanings,
                                               _productionTransactions, _customerContracts, _workshopProgression,
                                               _contractReceiverInventory, _contractReceiverActor.Value,
                                               _npcAgendaProgress, _city != null ? _city.SocialWorld : null,
                                               _customerRepairOrders, _servicePaymentTransactions);
            PlayerDevelopmentSaveMapper.Capture(_playerDevelopment, data);
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
            if (_customerContractPanel != null) _customerContractPanel.Close();
            if (_customerRepairOrderPanel != null) _customerRepairOrderPanel.Close();
            if (_workshopUpgradePanel != null) _workshopUpgradePanel.Close();
            if (_npcAgendaPanel != null) _npcAgendaPanel.Close();
            _activeSearchableContainer = null;
            LoadReport report = SaveMapper.Apply(data, _stats, _inventory, _storage, _clock, _heat, _rent,
                ResolveType, _itemKnowledge, _playerDiscoveries, _playerRelationships, _assetOwnership, _economicTransactions,
                _economicAccounts, _retailTransactions, _shopSaleTransactions, _criminalProgression, _crimeOutcomes,
                _contactInventory, _illicitExchanges, _dirtyCashCleanings,
                _productionTransactions, _customerContracts, _workshopProgression, _contractReceiverInventory,
                _npcAgendaProgress, _city != null ? _city.SocialWorld : null, _customerRepairOrders, _servicePaymentTransactions);
            if (!report.Success)
            {
                Say("Load failed: " + report.Error);
                return;
            }
            EnsureCustomerRepairItem();
            RefreshWorkshopUpgradeVisual();
            if (_workspace != null)
                _workspace.Refresh();

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
                skippedContainers + report.SkippedProductionTransactions + report.SkippedCustomerContracts +
                report.SkippedWorkshopUpgradeIds + report.SkippedContractReceiverItems + report.SkippedCustomerRepairOrders + report.SkippedServicePaymentTransactions +
                report.SkippedNpcAgendaProgress + report.SkippedNpcKnowledge +
                report.SkippedNpcSocialRelationships + report.SkippedNpcPlayerRelationships > 0
                ? "Loaded at " + data.worldPlace + ". Some saved item or discovery details could not be restored."
                : "Loaded at " + data.worldPlace + ".");
        }

        private bool IsNearDirtyCashContact()
        {
            return _city != null && _city.IsResidentWithin(DirtyCashContactNpcId, _room.PlayerBody.position, InteractionDistance);
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
                dirtyCashUnits, DirtyCashContactActorId,
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

            var contactActor = DirtyCashContactActorId;
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
                if (Vector2.Distance(playerPosition, business.InteractionPoint) <= InteractionDistance)
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
            if (definition == null ||
                !business.TryGetBuybackOffer(definition, selected.State, out int unitPrice))
                return "This business does not buy that item in its current condition.";
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

            ItemDefinition[] survivalCatalogue = Resources.LoadAll<ItemDefinition>("SurvivalItems/Definitions");
            foreach (ItemDefinition survivalItem in survivalCatalogue)
                RegisterItemCatalog(survivalItem);

            RegisterItemCatalog(repairTargetItem);
            RegisterItemCatalog(basicPatchKitItem);
            RegisterItemCatalog(reinforcedPatchKitItem);

            _workshopRecipes.Clear();
            if (startingWorkshopRecipes != null)
                foreach (RecipeDefinition recipe in startingWorkshopRecipes)
                    RegisterRecipeCatalog(recipe);
            if (firstWorkshopUpgrade != null && firstWorkshopUpgrade.UnlockedRecipe != null)
                RegisterRecipeCatalog(firstWorkshopUpgrade.UnlockedRecipe);
            if (firstCustomerContract != null && firstCustomerContract.RequiredItem != null)
                RegisterItemCatalog(firstCustomerContract.RequiredItem);
            if (firstCustomerRepairOrder != null && firstCustomerRepairOrder.EntrustedItem != null)
                RegisterItemCatalog(firstCustomerRepairOrder.EntrustedItem);

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


        private void RegisterRecipeCatalog(RecipeDefinition recipe)
        {
            if (recipe == null) return;
            if (!recipe.TryValidate(out string error))
            {
                Debug.LogError("GameSession: invalid workshop recipe '" + recipe.name + "': " + error);
                return;
            }
            if (!_workshopRecipes.Contains(recipe))
                _workshopRecipes.Add(recipe);
            foreach (RecipeDefinition.IngredientEntry input in recipe.Inputs)
                if (input != null && input.item != null)
                    RegisterItemCatalog(input.item);
            if (recipe.Output != null && recipe.Output.item != null)
                RegisterItemCatalog(recipe.Output.item);
        }

        private void RegisterItemCatalog(ItemDefinition definition)
        {
            if (definition == null) return;
            try
            {
                ItemType type = definition.ToItemType();
                _catalog[type.Id] = type;
                if (!_allItemDefinitions.Contains(definition))
                    _allItemDefinitions.Add(definition);
            }
            catch (InvalidOperationException)
            {
                Debug.LogError("GameSession: item '" + definition.name + "' has no valid stable ID.");
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


        private bool CanStartAgendaInteraction(Vector2 playerPosition)
        {
            return _npcAgendaDirector != null && _city != null &&
                _city.TryGetNearestResidentWithin(playerPosition, InteractionDistance, out string npcId) &&
                !_city.IsResidentTravelling(npcId) && _npcAgendaDirector.HasAgenda(npcId);
        }

        private void UpdateNpcInteractionPause(string npcId)
        {
            if (string.Equals(_interactionPausedNpcId, npcId, StringComparison.Ordinal))
                return;

            SetResidentInteractionPaused(_interactionPausedNpcId, false);
            _interactionPausedNpcId = string.IsNullOrWhiteSpace(npcId) ? null : npcId;
            SetResidentInteractionPaused(_interactionPausedNpcId, true);
        }

        private void SetResidentInteractionPaused(string npcId, bool paused)
        {
            if (string.IsNullOrWhiteSpace(npcId) || _city == null)
                return;

            foreach (NpcPresenter resident in _city.DebugResidents)
            {
                if (resident != null && resident.NpcId == npcId)
                {
                    resident.SetInteractionPaused(paused);
                    return;
                }
            }
        }

        private bool TryOpenAgendaConversation(Vector2 playerPosition)
        {
            if (_npcAgendaDirector == null || _npcAgendaPanel == null || _city == null ||
                !_city.TryGetNearestResidentWithin(playerPosition, InteractionDistance, out string npcId) ||
                _city.IsResidentTravelling(npcId) ||
                !_npcAgendaDirector.TryGetNextStage(npcId, _city.GetResidentActivity(npcId),
                    _city.GetResidentCurrentPlace(npcId),
                    out NpcAgendaThreadDefinition thread, out NpcAgendaStageDefinition stage))
                return false;

            _npcAgendaPanel.Open(npcId, thread, stage);
            return _npcAgendaPanel.IsOpen;
        }

        private bool TryOpenAgendaConversationForNpc(string npcId)
        {
            if (_npcAgendaDirector == null || _npcAgendaPanel == null || _city == null ||
                _room == null || string.IsNullOrWhiteSpace(npcId))
                return false;

            foreach (NpcPresenter resident in _city.DebugResidents)
            {
                if (resident == null || resident.NpcId != npcId ||
                    resident.CurrentPlace != _city.CurrentPlace || resident.IsTravelling ||
                    Vector2.Distance(resident.Position, _room.PlayerBody.position) > InteractionDistance)
                    continue;
                if (!_npcAgendaDirector.TryGetNextStage(resident.NpcId, resident.Activity,
                    resident.CurrentPlace, out NpcAgendaThreadDefinition thread,
                    out NpcAgendaStageDefinition stage))
                    return false;

                _npcAgendaPanel.Open(resident.NpcId, thread, stage);
                return _npcAgendaPanel.IsOpen;
            }
            return false;
        }

        private void HandleAgendaChoice(string npcId, string threadId, string stageId, string choiceId)
        {
            if (_npcAgendaDirector == null || _city == null || _room == null ||
                !_city.IsResidentWithin(npcId, _room.PlayerBody.position, InteractionDistance) ||
                _city.IsResidentTravelling(npcId) ||
                !_npcAgendaDirector.TryGetNextStage(npcId, _city.GetResidentActivity(npcId),
                    _city.GetResidentCurrentPlace(npcId),
                    out NpcAgendaThreadDefinition currentThread, out NpcAgendaStageDefinition currentStage) ||
                currentThread.ThreadId != threadId || currentStage.StageId != stageId ||
                !_npcAgendaDirector.TryChoose(npcId, threadId, stageId, choiceId,
                    out string response, out _))
            {
                if (_npcAgendaPanel != null) _npcAgendaPanel.Close();
                Say("That conversation has ended or is no longer available.");
                return;
            }

            if (_npcAgendaPanel != null)
                _npcAgendaPanel.Close();
            if (!string.IsNullOrWhiteSpace(response))
                Say(response, 8f);
        }

        private bool IsAgendaContractCompleted(string contractId)
        {
            if (string.IsNullOrWhiteSpace(contractId) || _customerContracts == null)
                return false;
            if (firstCustomerContract != null &&
                string.Equals("first-customer-contract", contractId, StringComparison.Ordinal))
                return GetCustomerContractPhase() == CustomerContractPhase.Completed;
            return Guid.TryParse(contractId, out Guid id) &&
                _customerContracts.TryGetState(id, out CustomerContractState state) &&
                state.Phase == CustomerContractPhase.Completed;
        }

        private bool IsAgendaUpgradeAcquired(string upgradeId)
        {
            if (string.IsNullOrWhiteSpace(upgradeId) || _workshopProgression == null)
                return false;
            if (firstWorkshopUpgrade != null &&
                string.Equals("compact-sewing-machine", upgradeId, StringComparison.Ordinal))
                return IsFirstUpgradeAcquired();
            return Guid.TryParse(upgradeId, out Guid id) && _workshopProgression.HasAcquired(id);
        }

        private bool IsAgendaOpportunityInPhase(string opportunityId, OpportunityPhase phase)
        {
            return _miloOpportunity != null &&
                string.Equals(_miloOpportunity.OpportunityId, opportunityId, StringComparison.Ordinal) &&
                _miloOpportunity.Phase == phase;
        }

        private bool OpenAgendaWorkshopOffer(WorkshopUpgradeDefinition upgrade)
        {
            if (upgrade == null || firstWorkshopUpgrade == null ||
                upgrade.Id != firstWorkshopUpgrade.Id || _workshopUpgradePanel == null ||
                _city == null || _room == null ||
                !_city.IsResidentWithin(upgrade.PayeeNpcId,
                    _room.PlayerBody.position, InteractionDistance))
                return false;

            _workshopUpgradePanel.Open();
            return _workshopUpgradePanel.IsOpen;
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
            float bestDistance = InteractionDistance * InteractionDistance;
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