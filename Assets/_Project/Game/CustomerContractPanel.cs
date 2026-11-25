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
    /// <summary>Customer-facing review, acceptance and physical fulfilment UI. It never owns contract rules.</summary>
    public sealed class CustomerContractPanel : MonoBehaviour
    {
        private static readonly Color Backdrop = new Color32(8, 11, 15, 238);
        private static readonly Color Frame = new Color32(27, 33, 39, 255);
        private static readonly Color Accent = new Color32(62, 230, 210, 255);
        private static readonly Color Ink = new Color32(235, 226, 207, 255);
        private static readonly Color Muted = new Color32(170, 178, 184, 255);

        private readonly Dictionary<Guid, string> _names = new Dictionary<Guid, string>();
        private readonly Dictionary<Guid, Button> _itemButtons = new Dictionary<Guid, Button>();
        private CustomerContractDefinition _definition;
        private string _issuerDisplayName;
        private GridInventory _inventory;
        private PlayerPocketBook _pockets;
        private AssetOwnershipBook _ownership;
        private Func<string> _accept;
        private Func<Guid, string> _fulfil;
        private Func<CustomerContractPhase> _phase;
        private Font _font;
        private GameObject _root;
        private RectTransform _itemRoot;
        private Text _details;
        private Text _status;
        private Text _selected;
        private Button _acceptButton;
        private Button _fulfilButton;
        private Guid _selectedItem;

        public bool IsOpen => _root != null && _root.activeSelf;
        public string ActiveIssuerNpcId => IsOpen && _definition != null ? _definition.IssuerNpcId : string.Empty;

        public void Bind(CustomerContractDefinition definition, GridInventory inventory,
            AssetOwnershipBook ownership, IReadOnlyList<ItemDefinition> itemDefinitions,
            string issuerDisplayName, Func<string> accept, Func<Guid, string> fulfil,
            Func<CustomerContractPhase> phase, PlayerPocketBook pockets = null)
        {
            _definition = definition ?? throw new ArgumentNullException(nameof(definition));
            _issuerDisplayName = string.IsNullOrWhiteSpace(issuerDisplayName) ? definition.DisplayName : issuerDisplayName;
            _inventory = inventory ?? throw new ArgumentNullException(nameof(inventory));
            _pockets = pockets;
            _ownership = ownership ?? throw new ArgumentNullException(nameof(ownership));
            _accept = accept ?? throw new ArgumentNullException(nameof(accept));
            _fulfil = fulfil ?? throw new ArgumentNullException(nameof(fulfil));
            _phase = phase ?? throw new ArgumentNullException(nameof(phase));

            _names.Clear();
            if (itemDefinitions != null)
            {
                foreach (ItemDefinition item in itemDefinitions)
                {
                    if (item == null) continue;
                    try { _names[item.ToItemType().Id] = string.IsNullOrWhiteSpace(item.DisplayName) ? item.name : item.DisplayName; }
                    catch (InvalidOperationException) { }
                }
            }

            _inventory.Changed += OnInventoryChanged;
            if (_pockets != null)
                for (int i = 0; i < PlayerPocketBook.PocketCount; i++)
                    _pockets.GetPocket(i).Changed += OnPocketChanged;
            BuildIfNeeded();
            Close();
        }

        public void SetPlayerInventory(GridInventory inventory)
        {
            if (inventory == null || ReferenceEquals(_inventory, inventory)) return;
            if (_inventory != null) _inventory.Changed -= OnInventoryChanged;
            _inventory = inventory;
            _inventory.Changed += OnInventoryChanged;
            if (IsOpen) Refresh();
        }

        public void Open()
        {
            if (_root == null) BuildIfNeeded();
            _selectedItem = Guid.Empty;
            _status.text = string.Empty;
            _root.SetActive(true);
            Refresh();
        }

        public void Close()
        {
            if (_root != null) _root.SetActive(false);
            _selectedItem = Guid.Empty;
        }

        private void OnDestroy()
        {
            if (_inventory != null) _inventory.Changed -= OnInventoryChanged;
            if (_pockets != null)
                for (int i = 0; i < PlayerPocketBook.PocketCount; i++)
                    _pockets.GetPocket(i).Changed -= OnPocketChanged;
        }

        private void Update()
        {
            if (IsOpen && Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
                Close();
        }

        private void OnInventoryChanged(GridInventory inventory)
        {
            if (IsOpen) Refresh();
        }

        private void OnPocketChanged(GridInventory inventory)
        {
            if (IsOpen) Refresh();
        }

        private void Refresh()
        {
            if (!IsOpen) return;
            CustomerContractPhase phase = _phase();
            string required = _definition.RequiredItem != null ? DisplayName(_definition.RequiredItem) : "Unknown goods";
            _details.text = _definition.DisplayName + "\n\n" + _definition.Description +
                "\n\nREQUESTED GOODS\n" + required + " × " + _definition.RequiredQuantity +
                "\nREWARD\n" + _definition.CleanCardReward + " clean CARD" +
                (_definition.AcceptStolenGoods ? "\nThis offer accepts stolen goods." : "\nOnly legitimate goods are accepted.");
            _itemRoot.gameObject.SetActive(phase == CustomerContractPhase.Active);
            _acceptButton.gameObject.SetActive(phase == CustomerContractPhase.Offered);
            _fulfilButton.gameObject.SetActive(phase == CustomerContractPhase.Active);
            _selected.gameObject.SetActive(phase == CustomerContractPhase.Active);
            ClearItems();

            if (phase == CustomerContractPhase.Offered)
            {
                _acceptButton.interactable = true;
                if (string.IsNullOrEmpty(_status.text)) _status.text = "Review the goods and reward, then accept the request.";
            }
            else if (phase == CustomerContractPhase.Active)
            {
                if (string.IsNullOrEmpty(_status.text))
                    _status.text = "Select the exact carried stack to hand over. The customer receives the physical goods.";
                bool selectedValid = false;
                foreach (PlacedItem item in _inventory.Items)
                {
                    bool owned = _ownership.TryGetOwner(item.ItemInstanceId, out EconomicActorId owner) &&
                        owner == EconomicActorId.Player;
                    CreateItemButton(item, owned);
                    if (item.ItemInstanceId == _selectedItem) selectedValid = true;
                }
                if (_pockets != null)
                    for (int pocketIndex = 0; pocketIndex < PlayerPocketBook.PocketCount; pocketIndex++)
                        foreach (PlacedItem item in _pockets.GetPocket(pocketIndex).Items)
                        {
                            bool owned = _ownership.TryGetOwner(item.ItemInstanceId, out EconomicActorId owner) &&
                                owner == EconomicActorId.Player;
                            CreateItemButton(item, owned);
                            if (item.ItemInstanceId == _selectedItem) selectedValid = true;
                        }
                if (!selectedValid) _selectedItem = Guid.Empty;
                _selected.text = _selectedItem == Guid.Empty ? "No stack selected." : "Selected: " + SelectedName();
                _fulfilButton.interactable = _selectedItem != Guid.Empty;
            }
            else
            {
                if (string.IsNullOrEmpty(_status.text))
                    _status.text = "This request has been completed. The customer has received the goods.";
            }
        }

        private Button CreateItemButton(PlacedItem item, bool owned)
        {
            string label = DisplayName(item.Type.Id) + " × " + item.Count +
                (owned ? string.Empty : "  [NOT YOURS]") +
                (item.State.Ownership == ItemOwnershipStatus.Stolen ? "  [STOLEN]" : string.Empty);
            Button button = CreateButton(_itemRoot, label, new Vector2(430f, 40f),
                item.ItemInstanceId == _selectedItem ? Accent : new Color32(48, 56, 62, 255), 14);
            button.interactable = owned;
            RectTransform rect = button.GetComponent<RectTransform>();
            Anchor(rect, new Vector2(4f, -4f - _itemButtons.Count * 46f), new Vector2(344f, 40f), true);
            _itemButtons[item.ItemInstanceId] = button;
            button.onClick.AddListener(() =>
            {
                _selectedItem = item.ItemInstanceId;
                _status.text = string.Empty;
                Refresh();
            });
            return button;
        }

        private string SelectedName()
        {
            if (_inventory.TryGetItemInstance(_selectedItem, out PlacedItem item))
                return DisplayName(item.Type.Id) + " × " + item.Count;
            if (_pockets != null)
                for (int i = 0; i < PlayerPocketBook.PocketCount; i++)
                    if (_pockets.GetPocket(i).TryGetItemInstance(_selectedItem, out item))
                        return DisplayName(item.Type.Id) + " × " + item.Count + " (Pocket " + (i + 1) + ")";
            return "Unavailable";
        }

        private void ClearItems()
        {
            for (int i = _itemRoot.childCount - 1; i >= 0; i--)
                Destroy(_itemRoot.GetChild(i).gameObject);
            _itemButtons.Clear();
        }

        private string DisplayName(ItemDefinition item)
        {
            if (item == null) return "Unknown goods";
            try { return DisplayName(item.ToItemType().Id); }
            catch (InvalidOperationException) { return item.name; }
        }

        private string DisplayName(Guid id) => _names.TryGetValue(id, out string name) ? name : "Unknown item";

        private void BuildIfNeeded()
        {
            if (_root != null) return;
            EnsureEventSystem();
            _font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            _root = new GameObject("CustomerContractCanvas", typeof(RectTransform), typeof(Canvas),
                typeof(CanvasScaler), typeof(GraphicRaycaster));
            _root.transform.SetParent(transform, false);
            Canvas canvas = _root.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 220;
            CanvasScaler scaler = _root.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);

            RectTransform bg = Panel(_root.transform, "Backdrop", Backdrop);
            Stretch(bg);
            RectTransform frame = Panel(bg, "ContractFrame", Frame);
            Center(frame, new Vector2(900f, 700f));

            Text title = MakeText(frame, "CUSTOMER REQUEST", 28, Accent, TextAnchor.MiddleLeft);
            Anchor(title.rectTransform, new Vector2(32f, -26f), new Vector2(820f, 42f), true);
            Text issuer = MakeText(frame, "ISSUER  /  " + _issuerDisplayName + " — " + _definition.DisplayName, 16, Muted, TextAnchor.MiddleLeft);
            Anchor(issuer.rectTransform, new Vector2(32f, -68f), new Vector2(820f, 28f), true);

            _details = MakeText(frame, string.Empty, 19, Ink, TextAnchor.UpperLeft);
            Anchor(_details.rectTransform, new Vector2(34f, -112f), new Vector2(420f, 410f), true);
            _itemRoot = Panel(frame, "CarriedGoods", new Color32(20, 25, 30, 255));
            Anchor(_itemRoot, new Vector2(470f, -112f), new Vector2(395f, 410f), true);
            Text goodsTitle = MakeText(_itemRoot, "YOUR CARRIED GOODS", 15, Accent, TextAnchor.MiddleLeft);
            Anchor(goodsTitle.rectTransform, new Vector2(12f, -8f), new Vector2(360f, 28f), true);
            RectTransform list = Panel(_itemRoot, "GoodsList", Color.clear);
            Anchor(list, new Vector2(12f, -48f), new Vector2(360f, 340f), true);
            _itemRoot = list;

            _selected = MakeText(frame, string.Empty, 14, Muted, TextAnchor.MiddleLeft);
            Anchor(_selected.rectTransform, new Vector2(470f, -532f), new Vector2(395f, 28f), true);
            _status = MakeText(frame, string.Empty, 16, Muted, TextAnchor.MiddleLeft);
            Anchor(_status.rectTransform, new Vector2(34f, 20f), new Vector2(480f, 38f), false);

            _acceptButton = CreateButton(frame, "ACCEPT REQUEST", new Vector2(240f, 52f), Accent, 16);
            Anchor(_acceptButton.GetComponent<RectTransform>(), new Vector2(24f, 20f), new Vector2(240f, 52f), false, true);
            _acceptButton.onClick.AddListener(() =>
            {
                SetStatus(_accept());
                Refresh();
            });

            _fulfilButton = CreateButton(frame, "HAND OVER GOODS", new Vector2(240f, 52f), Accent, 16);
            Anchor(_fulfilButton.GetComponent<RectTransform>(), new Vector2(1f, 20f), new Vector2(240f, 52f), false, true);
            _fulfilButton.onClick.AddListener(() =>
            {
                if (_selectedItem == Guid.Empty) return;
                SetStatus(_fulfil(_selectedItem));
                Refresh();
            });

            Button close = CreateButton(frame, "BACK TO CITY  [ESC]", new Vector2(240f, 48f),
                new Color32(55, 63, 67, 255), 15);
            Anchor(close.GetComponent<RectTransform>(), new Vector2(24f, -24f), new Vector2(240f, 48f), true, true);
            close.onClick.AddListener(Close);
            _root.SetActive(false);
        }

        private void SetStatus(string message)
        {
            _status.text = message ?? string.Empty;
        }

        private static void EnsureEventSystem()
        {
            EventSystem current = EventSystem.current;
            if (current != null)
            {
                if (current.GetComponent<InputSystemUIInputModule>() == null)
                    current.gameObject.AddComponent<InputSystemUIInputModule>();
                return;
            }
            new GameObject("CustomerContractEventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
        }

        private Text MakeText(Transform parent, string value, int size, Color color, TextAnchor alignment)
        {
            GameObject go = new GameObject("Text", typeof(RectTransform), typeof(Text));
            go.transform.SetParent(parent, false);
            Text text = go.GetComponent<Text>();
            text.font = _font != null ? _font : Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = size; text.color = color; text.alignment = alignment;
            text.text = value; text.raycastTarget = false;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Overflow;
            return text;
        }

        private static RectTransform Panel(Transform parent, string name, Color color)
        {
            GameObject go = new GameObject(name, typeof(RectTransform), typeof(Image));
            go.transform.SetParent(parent, false);
            go.GetComponent<Image>().color = color;
            return go.GetComponent<RectTransform>();
        }

        private Button CreateButton(Transform parent, string label, Vector2 size, Color color, int fontSize)
        {
            RectTransform rect = Panel(parent, "Button", color);
            rect.sizeDelta = size;
            Button button = rect.gameObject.AddComponent<Button>();
            button.targetGraphic = rect.GetComponent<Image>();
            Text text = MakeText(rect, label, fontSize, Color.black, TextAnchor.MiddleCenter);
            Stretch(text.rectTransform);
            return button;
        }

        private static void Stretch(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero; rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero; rect.offsetMax = Vector2.zero;
        }

        private static void Center(RectTransform rect, Vector2 size)
        {
            rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = size;
        }

        private static void Anchor(RectTransform rect, Vector2 position, Vector2 size, bool topLeft, bool right = false)
        {
            if (right)
            {
                rect.anchorMin = rect.anchorMax = new Vector2(1f, topLeft ? 1f : 0f);
                rect.pivot = new Vector2(1f, topLeft ? 1f : 0f);
                rect.anchoredPosition = new Vector2(-position.x, topLeft ? position.y : position.y);
            }
            else
            {
                rect.anchorMin = rect.anchorMax = topLeft ? new Vector2(0f, 1f) : Vector2.zero;
                rect.pivot = rect.anchorMin;
                rect.anchoredPosition = position;
            }
            rect.sizeDelta = size;
        }
    }
}
