using System;
using System.Collections.Generic;
using Signal.Core;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

namespace Signal.Game
{
    /// <summary>Side-by-side contact/player inventory workspace for the fixed fence and cleaning offers.</summary>
    public sealed class DirtyCashContactPanel : MonoBehaviour
    {
        private const float CellSize = 44f;
        private static readonly Color Background = new Color32(14, 17, 22, 248);
        private static readonly Color PanelColor = new Color32(22, 27, 34, 255);
        private static readonly Color CellColor = new Color32(35, 40, 48, 255);
        private static readonly Color Accent = new Color32(62, 230, 210, 255);
        private static readonly Color TextColor = new Color32(225, 228, 230, 255);
        private static readonly Color Muted = new Color32(157, 166, 176, 255);
        private static readonly Color Selected = new Color32(200, 162, 74, 255);
        private static readonly Color[] Palette =
        {
            new Color32(139, 74, 43, 255), new Color32(200, 162, 74, 255),
            new Color32(62, 230, 210, 255), new Color32(226, 79, 181, 255),
            new Color32(107, 114, 128, 255), new Color32(201, 206, 214, 255)
        };

        private enum ServiceMode { Fence, Cleaning }

        private readonly Dictionary<Guid, string> _names = new Dictionary<Guid, string>();
        private readonly Dictionary<Guid, Color> _colors = new Dictionary<Guid, Color>();
        private Font _font;
        private GameObject _rootObject;
        private RectTransform _root;
        private RectTransform _contactItems;
        private RectTransform _playerItems;
        private Text _contactTitle;
        private Text _playerTitle;
        private Text _offerText;
        private Text _preview;
        private Text _status;
        private Button _fenceTab;
        private Button _cleanTab;
        private Button _reviewFence;
        private Button _confirmFence;
        private Button _cancelReview;
        private Button _confirmClean;

        private GridInventory _playerInventory;
        private GridInventory _contactInventory;
        private AssetOwnershipBook _ownership;
        private ItemDefinition _dirtyCash;
        private ItemDefinition _acceptedItem;
        private int _acceptedQuantity;
        private int _dirtyCashUnits;
        private int _feePercent;
        private Func<Guid, string> _exchange;
        private Func<Guid, string> _clean;
        private Guid _selectedPlayerStack;
        private ServiceMode _mode;
        private bool _reviewingFence;

        public bool IsOpen => _rootObject != null && _rootObject.activeSelf;

        public void Bind(GridInventory playerInventory, GridInventory contactInventory, AssetOwnershipBook ownership,
            IReadOnlyList<ItemDefinition> definitions, ItemDefinition dirtyCash, ItemDefinition acceptedItem,
            int acceptedQuantity, int dirtyCashUnits, int feePercent,
            Func<Guid, string> exchange, Func<Guid, string> clean)
        {
            _playerInventory = playerInventory ?? throw new ArgumentNullException(nameof(playerInventory));
            _contactInventory = contactInventory ?? throw new ArgumentNullException(nameof(contactInventory));
            _ownership = ownership ?? throw new ArgumentNullException(nameof(ownership));
            _dirtyCash = dirtyCash ?? throw new ArgumentNullException(nameof(dirtyCash));
            _acceptedItem = acceptedItem ?? throw new ArgumentNullException(nameof(acceptedItem));
            _acceptedQuantity = acceptedQuantity;
            _dirtyCashUnits = dirtyCashUnits;
            _feePercent = feePercent;
            _exchange = exchange ?? throw new ArgumentNullException(nameof(exchange));
            _clean = clean ?? throw new ArgumentNullException(nameof(clean));

            _names.Clear();
            _colors.Clear();
            if (definitions != null)
            {
                for (int i = 0; i < definitions.Count; i++)
                {
                    ItemDefinition definition = definitions[i];
                    if (definition == null) continue;
                    try
                    {
                        ItemType type = definition.ToItemType();
                        _names[type.Id] = string.IsNullOrWhiteSpace(definition.DisplayName) ? definition.name : definition.DisplayName;
                        _colors[type.Id] = Palette[i % Palette.Length];
                    }
                    catch (InvalidOperationException) { }
                }
            }

            _playerInventory.Changed += OnInventoryChanged;
            _contactInventory.Changed += OnInventoryChanged;
            BuildIfNeeded();
            Close();
        }

        public void Open()
        {
            if (_rootObject == null) BuildIfNeeded();
            _mode = ServiceMode.Fence;
            _selectedPlayerStack = Guid.Empty;
            _reviewingFence = false;
            _status.text = string.Empty;
            _rootObject.SetActive(true);
            Refresh();
        }

        public void Close()
        {
            if (_rootObject != null) _rootObject.SetActive(false);
            _reviewingFence = false;
        }

        private void OnDestroy()
        {
            if (_playerInventory != null) _playerInventory.Changed -= OnInventoryChanged;
            if (_contactInventory != null) _contactInventory.Changed -= OnInventoryChanged;
        }

        private void OnInventoryChanged(GridInventory changed)
        {
            if (IsOpen) Refresh();
        }

        private void Update()
        {
            if (IsOpen && Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
                Close();
        }

        private void SetMode(ServiceMode mode)
        {
            _mode = mode;
            _selectedPlayerStack = Guid.Empty;
            _reviewingFence = false;
            _status.text = string.Empty;
            Refresh();
        }

        private void SelectPlayerStack(Guid id)
        {
            _selectedPlayerStack = id;
            _reviewingFence = false;
            _status.text = string.Empty;
            Refresh();
        }

        private bool TryGetSelected(out PlacedItem selected)
        {
            selected = default;
            return _selectedPlayerStack != Guid.Empty &&
                _playerInventory.TryGetItemInstance(_selectedPlayerStack, out selected);
        }

        private bool IsPlayerOwned(PlacedItem item)
        {
            return _ownership.TryGetOwner(item.ItemInstanceId, out EconomicActorId owner) &&
                owner == EconomicActorId.Player;
        }

        private bool IsEligibleFenceStack(PlacedItem item)
        {
            return item.Type == _acceptedItem.ToItemType() &&
                item.Count == _acceptedQuantity &&
                item.State.Ownership == ItemOwnershipStatus.Stolen &&
                IsPlayerOwned(item);
        }

        private bool IsEligibleCashStack(PlacedItem item)
        {
            return item.Type == _dirtyCash.ToItemType() && item.Count > 0 && IsPlayerOwned(item);
        }

        private void Refresh()
        {
            if (!IsOpen) return;
            Clear(_contactItems);
            Clear(_playerItems);

            foreach (PlacedItem item in _contactInventory.Items)
                DrawItem(item, _contactItems, false);
            foreach (PlacedItem item in _playerInventory.Items)
                DrawItem(item, _playerItems, true);

            _contactTitle.text = "CONTACT INVENTORY  ·  " + _contactInventory.Width + " × " + _contactInventory.Height;
            _playerTitle.text = "YOUR CARRIED INVENTORY  ·  " + _playerInventory.Width + " × " + _playerInventory.Height;

            bool selectedExists = TryGetSelected(out PlacedItem selected);
            if (!selectedExists) _selectedPlayerStack = Guid.Empty;
            bool selectedFence = selectedExists && IsEligibleFenceStack(selected);
            bool selectedCash = selectedExists && IsEligibleCashStack(selected);

            _fenceTab.interactable = _mode != ServiceMode.Fence;
            _cleanTab.interactable = _mode != ServiceMode.Cleaning;
            _reviewFence.gameObject.SetActive(_mode == ServiceMode.Fence && !_reviewingFence);
            _confirmFence.gameObject.SetActive(_mode == ServiceMode.Fence && _reviewingFence);
            _cancelReview.gameObject.SetActive(_mode == ServiceMode.Fence && _reviewingFence);
            _reviewFence.interactable = selectedFence;
            _confirmFence.interactable = selectedFence;
            _confirmClean.gameObject.SetActive(_mode == ServiceMode.Cleaning);
            _confirmClean.interactable = selectedCash;

            if (_mode == ServiceMode.Fence)
            {
                _offerText.text = "FENCE OFFER\nExchange exactly " + _acceptedQuantity + " stolen, player-owned " +
                    DisplayName(_acceptedItem) + " in one stack for " + _dirtyCashUnits + " Dirty Cash units.\n" +
                    (selectedFence ? "Selected stack qualifies." : "Select a qualifying stack in your carried inventory.");
                _preview.text = selectedExists
                    ? "Selected: " + DisplayName(selected.Type) + " ×" + selected.Count +
                      (selectedFence ? "  ·  STOLEN / PLAYER-OWNED" : "  ·  DOES NOT QUALIFY")
                    : "No item stack selected.";
            }
            else
            {
                _offerText.text = "CLEANING SERVICE\nConvert your selected Dirty Cash stack into clean CARD.\n" +
                    "The contact retains " + _feePercent + "%; cleaning is immediate.";
                if (selectedCash && DirtyCashRules.TryGetCleaningAmounts(selected.Count, _feePercent, out int payout, out int fee))
                {
                    int gross = selected.Count * DirtyCashRules.FaceValuePerUnit;
                    _preview.text = "Selected: " + selected.Count + " Dirty Cash units  ·  Face value $" + gross +
                        "\nContact fee: $" + fee + " (" + _feePercent + "%)  ·  You receive: $" + payout + " clean CARD.";
                }
                else
                    _preview.text = selectedExists
                        ? "Select a player-owned Dirty Cash stack to see its exact cleaning payout."
                        : "Select a Dirty Cash stack in your carried inventory.";
            }
        }

        private void DrawItem(PlacedItem item, RectTransform parent, bool selectable)
        {
            string label = DisplayName(item.Type);
            Color color = _colors.TryGetValue(item.Type.Id, out Color found) ? found : Palette[5];
            bool selected = selectable && item.ItemInstanceId == _selectedPlayerStack;
            if (selected) color = Selected;

            var go = new GameObject("Item_" + item.ItemInstanceId,
                typeof(RectTransform), typeof(Image), typeof(Button));
            go.transform.SetParent(parent, false);
            Image image = go.GetComponent<Image>();
            image.color = color;
            image.raycastTarget = selectable;
            Button button = go.GetComponent<Button>();
            button.targetGraphic = image;
            button.interactable = selectable;
            SetTopLeft(go.GetComponent<RectTransform>(),
                item.X * CellSize + 2f, item.Y * CellSize + 2f,
                item.FootprintWidth * CellSize - 4f, item.FootprintHeight * CellSize - 4f);
            Text text = NewText(go.transform, label + (item.Count > 1 ? "\nx" + item.Count : string.Empty),
                11, TextAnchor.MiddleCenter, new Color32(14, 17, 22, 255));
            Stretch(text.rectTransform);
            if (selectable)
            {
                Guid id = item.ItemInstanceId;
                button.onClick.AddListener(() => SelectPlayerStack(id));
            }
        }

        private string DisplayName(ItemDefinition definition)
        {
            if (definition == null) return "Unknown item";
            try { return DisplayName(definition.ToItemType()); }
            catch (InvalidOperationException) { return definition.name; }
        }

        private string DisplayName(ItemType type)
        {
            return _names.TryGetValue(type.Id, out string name) ? name : type.Id.ToString();
        }

        private void BuildIfNeeded()
        {
            if (_rootObject != null) return;
            EnsureEventSystem();
            _font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (_font == null) _font = Font.CreateDynamicFontFromOSFont("Arial", 16);

            var canvasObject = new GameObject("DirtyCashContactCanvas",
                typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvasObject.transform.SetParent(transform, false);
            Canvas canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 140;
            CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1600f, 900f);
            scaler.matchWidthOrHeight = 0.5f;

            Image background = NewImage("Background", canvasObject.transform, Background);
            _root = background.rectTransform;
            _root.anchorMin = _root.anchorMax = new Vector2(0.5f, 0.5f);
            _root.pivot = new Vector2(0.5f, 0.5f);
            _root.sizeDelta = new Vector2(1100f, 680f);
            _rootObject = background.gameObject;

            Text heading = NewText(_root, "A DISCREET ARRANGEMENT  ·  [ESC] CLOSE", 21,
                TextAnchor.MiddleLeft, Accent);
            SetTopLeft(heading.rectTransform, 28f, 18f, 760f, 34f);
            Button close = NewButton(_root, "CLOSE", 130f, 18f, 100f, 36f);
            close.onClick.AddListener(Close);

            Image contactPanel = NewImage("ContactInventoryPanel", _root, PanelColor);
            SetTopLeft(contactPanel.rectTransform, 24f, 70f, 500f, 420f);
            _contactTitle = NewText(contactPanel.transform, "CONTACT INVENTORY", 16, TextAnchor.MiddleLeft, TextColor);
            SetTopLeft(_contactTitle.rectTransform, 16f, 12f, 460f, 28f);
            RectTransform contactGrid = NewRect("Grid", contactPanel.transform);
            SetTopLeft(contactGrid, 20f, 52f, 8f * CellSize, 6f * CellSize);
            BuildCells(contactGrid, 8, 6);
            _contactItems = NewRect("Items", contactGrid);
            Stretch(_contactItems);

            Image playerPanel = NewImage("PlayerInventoryPanel", _root, PanelColor);
            SetTopLeft(playerPanel.rectTransform, 550f, 70f, 526f, 420f);
            _playerTitle = NewText(playerPanel.transform, "YOUR CARRIED INVENTORY", 16, TextAnchor.MiddleLeft, TextColor);
            SetTopLeft(_playerTitle.rectTransform, 16f, 12f, 490f, 28f);
            RectTransform playerGrid = NewRect("Grid", playerPanel.transform);
            SetTopLeft(playerGrid, 20f, 52f, 6f * CellSize, 4f * CellSize);
            BuildCells(playerGrid, 6, 4);
            _playerItems = NewRect("Items", playerGrid);
            Stretch(_playerItems);

            _fenceTab = NewButton(_root, "FENCE OFFER", 28f, 510f, 170f, 38f);
            _fenceTab.onClick.AddListener(() => SetMode(ServiceMode.Fence));
            _cleanTab = NewButton(_root, "CLEAN DIRTY CASH", 208f, 510f, 210f, 38f);
            _cleanTab.onClick.AddListener(() => SetMode(ServiceMode.Cleaning));

            _offerText = NewText(_root, string.Empty, 15, TextAnchor.UpperLeft, TextColor);
            SetTopLeft(_offerText.rectTransform, 28f, 558f, 500f, 58f);
            _preview = NewText(_root, string.Empty, 14, TextAnchor.UpperLeft, Muted);
            SetTopLeft(_preview.rectTransform, 550f, 558f, 520f, 58f);

            _reviewFence = NewButton(_root, "REVIEW FENCE OFFER", 28f, 625f, 190f, 36f);
            _reviewFence.onClick.AddListener(() =>
            {
                if (TryGetSelected(out PlacedItem item) && IsEligibleFenceStack(item))
                {
                    _reviewingFence = true;
                    Refresh();
                }
            });
            _confirmFence = NewButton(_root, "CONFIRM FENCE EXCHANGE", 28f, 625f, 230f, 36f);
            _confirmFence.onClick.AddListener(() =>
            {
                if (!TryGetSelected(out PlacedItem item) || !IsEligibleFenceStack(item)) return;
                _status.text = _exchange(item.ItemInstanceId);
                _selectedPlayerStack = Guid.Empty;
                _reviewingFence = false;
                Refresh();
            });
            _cancelReview = NewButton(_root, "CANCEL", 270f, 625f, 110f, 36f);
            _cancelReview.onClick.AddListener(() => { _reviewingFence = false; Refresh(); });
            _confirmClean = NewButton(_root, "CONFIRM CLEANING", 28f, 625f, 190f, 36f);
            _confirmClean.onClick.AddListener(() =>
            {
                if (!TryGetSelected(out PlacedItem item) || !IsEligibleCashStack(item)) return;
                _status.text = _clean(item.ItemInstanceId);
                _selectedPlayerStack = Guid.Empty;
                Refresh();
            });

            _status = NewText(_root, string.Empty, 14, TextAnchor.MiddleLeft, new Color32(240, 195, 120, 255));
            SetTopLeft(_status.rectTransform, 550f, 625f, 520f, 36f);
            _rootObject.SetActive(false);
        }

        private void BuildCells(RectTransform grid, int width, int height)
        {
            for (int y = 0; y < height; y++)
                for (int x = 0; x < width; x++)
                {
                    Image cell = NewImage("Cell_" + x + "_" + y, grid, CellColor);
                    SetTopLeft(cell.rectTransform, x * CellSize + 1f, y * CellSize + 1f,
                        CellSize - 2f, CellSize - 2f);
                }
        }

        private Button NewButton(Transform parent, string label, float x, float y, float width, float height)
        {
            Image image = NewImage(label.Replace(" ", "") + "Button", parent, new Color32(62, 230, 210, 255));
            SetTopLeft(image.rectTransform, x, y, width, height);
            image.raycastTarget = true;
            Button button = image.gameObject.AddComponent<Button>();
            button.targetGraphic = image;
            Text text = NewText(image.transform, label, 13, TextAnchor.MiddleCenter,
                new Color32(14, 17, 22, 255));
            Stretch(text.rectTransform);
            return button;
        }

        private Image NewImage(string name, Transform parent, Color color)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image));
            go.transform.SetParent(parent, false);
            Image image = go.GetComponent<Image>();
            image.color = color;
            image.raycastTarget = false;
            return image;
        }

        private Text NewText(Transform parent, string value, int size, TextAnchor alignment, Color color)
        {
            var go = new GameObject("Text", typeof(RectTransform), typeof(Text));
            go.transform.SetParent(parent, false);
            Text text = go.GetComponent<Text>();
            text.font = _font;
            text.fontSize = size;
            text.alignment = alignment;
            text.color = color;
            text.text = value;
            text.raycastTarget = false;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Truncate;
            return text;
        }

        private static RectTransform NewRect(string name, Transform parent)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            return go.GetComponent<RectTransform>();
        }

        private static void SetTopLeft(RectTransform rect, float x, float y, float width, float height)
        {
            rect.anchorMin = rect.anchorMax = new Vector2(0f, 1f);
            rect.pivot = new Vector2(0f, 1f);
            rect.anchoredPosition = new Vector2(x, -y);
            rect.sizeDelta = new Vector2(width, height);
        }

        private static void Stretch(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }

        private static void Clear(RectTransform parent)
        {
            for (int i = parent.childCount - 1; i >= 0; i--)
                Destroy(parent.GetChild(i).gameObject);
        }

        private static void EnsureEventSystem()
        {
            if (EventSystem.current == null)
            {
                var eventSystem = new GameObject("DirtyCashEventSystem",
                    typeof(EventSystem), typeof(InputSystemUIInputModule));
                eventSystem.GetComponent<InputSystemUIInputModule>().AssignDefaultActions();
            }
            else if (EventSystem.current.GetComponent<InputSystemUIInputModule>() == null)
                EventSystem.current.gameObject.AddComponent<InputSystemUIInputModule>().AssignDefaultActions();
        }
    }
}
