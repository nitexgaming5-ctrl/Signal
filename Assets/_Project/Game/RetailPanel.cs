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
    /// <summary>Shared click-driven BUY/SELL panel for authored businesses.</summary>
    public sealed class RetailPanel : MonoBehaviour
    {
        private const int PageSize = 8;
        private const float RowHeight = 42f;

        private static readonly Color PanelColor = new Color32(28, 31, 36, 248);
        private static readonly Color TextColor = new Color32(224, 226, 230, 255);
        private static readonly Color AccentColor = new Color32(62, 230, 210, 255);
        private static readonly Color ButtonColor = new Color32(43, 91, 91, 255);
        private static readonly Color RowColor = new Color32(38, 43, 50, 255);

        private sealed class SellableStack
        {
            public PlacedItem Stack;
            public ItemDefinition Definition;
            public int UnitPrice;
        }

        private Font _font;
        private GameObject _root;
        private Text _title;
        private Text _status;
        private Text _pageLabel;
        private Text _footer;
        private RectTransform _content;
        private ScrollRect _scroll;
        private Button _buyTab;
        private Button _sellTab;
        private BusinessDefinition _business;
        private GridInventory _inventory;
        private readonly List<GridInventory> _inventories = new List<GridInventory>();
        private IReadOnlyList<ItemDefinition> _itemDefinitions;
        private AssetOwnershipBook _ownership;
        private readonly List<SellableStack> _sellableStacks = new List<SellableStack>();
        private readonly Dictionary<Guid, int> _sellQuantities = new Dictionary<Guid, int>();
        private int _pageIndex;
        private bool _showSell;
        private Func<BusinessDefinition, ItemDefinition, string> _purchase;
        private Func<BusinessDefinition, Guid, int, string> _sell;
        private Action _closed;

        public bool IsOpen => _root != null && _root.activeSelf;
        public string ActiveOwnerNpcId => IsOpen && _business != null ? _business.OwnerNpcId : string.Empty;

        public void Bind(Func<BusinessDefinition, ItemDefinition, string> purchase,
            Func<BusinessDefinition, Guid, int, string> sell, GridInventory inventory,
            IReadOnlyList<ItemDefinition> itemDefinitions, AssetOwnershipBook ownership, Action closed)
        {
            _purchase = purchase ?? throw new ArgumentNullException(nameof(purchase));
            _sell = sell ?? throw new ArgumentNullException(nameof(sell));
            _inventory = inventory ?? throw new ArgumentNullException(nameof(inventory));
            _inventories.Clear();
            _inventories.Add(_inventory);
            _itemDefinitions = itemDefinitions ?? throw new ArgumentNullException(nameof(itemDefinitions));
            _ownership = ownership ?? throw new ArgumentNullException(nameof(ownership));
            _closed = closed;
            BuildIfNeeded();
            _root.SetActive(false);
        }

        public void SetInventory(GridInventory inventory)
        {
            if (inventory == null) return;
            SetInventories(new[] { inventory });
        }

        public void SetInventories(IReadOnlyList<GridInventory> inventories)
        {
            _inventories.Clear();
            if (inventories != null)
                for (int i = 0; i < inventories.Count; i++)
                {
                    GridInventory inventory = inventories[i];
                    if (inventory == null || _inventories.Contains(inventory)) continue;
                    _inventories.Add(inventory);
                }
            _inventory = _inventories.Count > 0 ? _inventories[0] : null;
            if (IsOpen && _showSell) RefreshCatalogue();
        }

        public void Open(BusinessDefinition business)
        {
            if (business == null)
                return;

            BuildIfNeeded();
            _business = business;
            _showSell = false;
            _pageIndex = 0;
            _sellQuantities.Clear();
            _title.text = business.DisplayName + "  —  " + business.BusinessType;
            _status.text = string.Empty;
            UpdateTabs();
            RefreshCatalogue();
            _root.SetActive(true);
        }

        public void Close()
        {
            if (!IsOpen)
                return;

            _root.SetActive(false);
            _business = null;
            _closed?.Invoke();
        }

        private void Update()
        {
            if (IsOpen && Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
                Close();
        }

        private void SelectMode(bool sell)
        {
            if (_business == null || _showSell == sell)
                return;
            _showSell = sell;
            _pageIndex = 0;
            _status.text = string.Empty;
            UpdateTabs();
            RefreshCatalogue();
        }

        private void UpdateTabs()
        {
            SetTab(_buyTab, !_showSell);
            SetTab(_sellTab, _showSell);
            _footer.text = _showSell
                ? "Sell only goods you carry and own  •  ESC to close"
                : "Buy from this business using your clean CARD balance  •  ESC to close";
        }

        private static void SetTab(Button button, bool selected)
        {
            if (button == null)
                return;
            Color color = selected ? new Color32(43, 91, 91, 255) : new Color32(48, 52, 59, 255);
            Image image = button.GetComponent<Image>();
            if (image != null)
                image.color = color;
            ColorBlock colors = button.colors;
            colors.normalColor = color;
            colors.highlightedColor = Color.Lerp(color, Color.white, 0.18f);
            colors.pressedColor = Color.Lerp(color, Color.black, 0.2f);
            button.colors = colors;
        }

        private void RefreshCatalogue()
        {
            if (_business == null || _content == null)
                return;

            for (int i = _content.childCount - 1; i >= 0; i--)
                Destroy(_content.GetChild(i).gameObject);

            if (_showSell)
                RefreshSellRows();
            else
                RefreshBuyRows();
        }

        private void RefreshBuyRows()
        {
            int total = _business.Merchandise.Count;
            int pageCount = Math.Max(1, (total + PageSize - 1) / PageSize);
            _pageIndex = Mathf.Clamp(_pageIndex, 0, pageCount - 1);
            int start = _pageIndex * PageSize;
            int shown = Math.Min(PageSize, Math.Max(0, total - start));

            for (int i = 0; i < shown; i++)
                CreateMerchandiseRow(i, _business.Merchandise[start + i]);

            _content.sizeDelta = new Vector2(0f, Math.Max(310f, shown * RowHeight));
            _scroll.verticalNormalizedPosition = 1f;
            _pageLabel.text = total == 0 ? "No products" : "Page " + (_pageIndex + 1) + " / " + pageCount;
        }

        private void RefreshSellRows()
        {
            _sellableStacks.Clear();
            for (int inventoryIndex = 0; inventoryIndex < _inventories.Count; inventoryIndex++)
            {
                GridInventory inventory = _inventories[inventoryIndex];
                foreach (PlacedItem stack in inventory.Items)
                {
                    if (!_ownership.TryGetOwner(stack.ItemInstanceId, out EconomicActorId owner) ||
                        owner != EconomicActorId.Player)
                        continue;

                    ItemDefinition definition = FindDefinition(stack.Type.Id);
                    if (definition == null ||
                        !_business.TryGetBuybackOffer(definition, stack.State, out int unitPrice))
                        continue;

                    _sellableStacks.Add(new SellableStack
                    {
                        Stack = stack,
                        Definition = definition,
                        UnitPrice = unitPrice
                    });
                }
            }

            int total = _sellableStacks.Count;
            int pageCount = Math.Max(1, (total + PageSize - 1) / PageSize);
            _pageIndex = Mathf.Clamp(_pageIndex, 0, pageCount - 1);
            int start = _pageIndex * PageSize;
            int shown = Math.Min(PageSize, Math.Max(0, total - start));

            for (int i = 0; i < shown; i++)
                CreateSellRow(i, _sellableStacks[start + i]);

            _content.sizeDelta = new Vector2(0f, Math.Max(310f, shown * RowHeight));
            _scroll.verticalNormalizedPosition = 1f;
            _pageLabel.text = total == 0 ? "No accepted goods in your carried inventory" :
                "Page " + (_pageIndex + 1) + " / " + pageCount;
        }

        private ItemDefinition FindDefinition(Guid itemDefinitionId)
        {
            for (int i = 0; i < _itemDefinitions.Count; i++)
            {
                ItemDefinition definition = _itemDefinitions[i];
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

        private void CreateMerchandiseRow(int rowIndex, BusinessDefinition.MerchandiseEntry entry)
        {
            RectTransform rowRect = CreateRow(rowIndex, "MerchandiseRow");
            string itemName = entry != null && entry.Item != null ? entry.Item.DisplayName : "Unconfigured merchandise";
            string price = entry != null && entry.Item != null && entry.UnitPrice > 0 ? "$" + entry.UnitPrice : "Unavailable";

            Text itemLabel = NewText("ItemName", rowRect, 17, TextColor, TextAnchor.MiddleLeft);
            SetRect(itemLabel.rectTransform, new Vector2(0f, 0f), new Vector2(1f, 1f),
                new Vector2(12f, 0f), new Vector2(-150f, 0f));
            itemLabel.text = itemName;

            Button buyButton = NewButton("BuyButton", rowRect, "BUY  " + price, ButtonColor);
            SetRect(buyButton.GetComponent<RectTransform>(), new Vector2(1f, 0.5f), new Vector2(1f, 0.5f),
                new Vector2(-66f, 0f), new Vector2(116f, 32f));
            buyButton.interactable = entry != null && entry.Item != null && entry.UnitPrice > 0;
            if (buyButton.interactable)
            {
                ItemDefinition selectedItem = entry.Item;
                buyButton.onClick.AddListener(() =>
                {
                    if (_business == null)
                        return;
                    _status.text = _purchase(_business, selectedItem);
                    RefreshCatalogue();
                });
            }
        }

        private void CreateSellRow(int rowIndex, SellableStack sellable)
        {
            RectTransform rowRect = CreateRow(rowIndex, "SellableStackRow");
            PlacedItem stack = sellable.Stack;
            Guid stackId = stack.ItemInstanceId;
            int quantity = _sellQuantities.TryGetValue(stackId, out int selected) ? Mathf.Clamp(selected, 1, stack.Count) : 1;
            _sellQuantities[stackId] = quantity;
            long total = (long)quantity * sellable.UnitPrice;

            Text itemLabel = NewText("ItemName", rowRect, 15, TextColor, TextAnchor.MiddleLeft);
            SetRect(itemLabel.rectTransform, new Vector2(0f, 0f), new Vector2(1f, 1f),
                new Vector2(12f, 0f), new Vector2(-300f, 0f));
            itemLabel.text = sellable.Definition.DisplayName + "  x" + stack.Count + "  @ $" + sellable.UnitPrice;

            Button minus = NewButton("QuantityMinus", rowRect, "−", ButtonColor);
            SetRect(minus.GetComponent<RectTransform>(), new Vector2(1f, 0.5f), new Vector2(1f, 0.5f),
                new Vector2(-244f, 0f), new Vector2(28f, 30f));
            minus.interactable = quantity > 1;
            minus.onClick.AddListener(() =>
            {
                _sellQuantities[stackId] = Math.Max(1, quantity - 1);
                RefreshCatalogue();
            });

            Text quantityLabel = NewText("Quantity", rowRect, 15, TextColor, TextAnchor.MiddleCenter);
            SetRect(quantityLabel.rectTransform, new Vector2(1f, 0.5f), new Vector2(1f, 0.5f),
                new Vector2(-207f, 0f), new Vector2(38f, 30f));
            quantityLabel.text = quantity.ToString();

            Button plus = NewButton("QuantityPlus", rowRect, "+", ButtonColor);
            SetRect(plus.GetComponent<RectTransform>(), new Vector2(1f, 0.5f), new Vector2(1f, 0.5f),
                new Vector2(-170f, 0f), new Vector2(28f, 30f));
            plus.interactable = quantity < stack.Count;
            plus.onClick.AddListener(() =>
            {
                _sellQuantities[stackId] = Math.Min(stack.Count, quantity + 1);
                RefreshCatalogue();
            });

            Button sellButton = NewButton("SellButton", rowRect,
                total <= int.MaxValue ? "SELL  $" + total : "TOTAL TOO HIGH", ButtonColor);
            SetRect(sellButton.GetComponent<RectTransform>(), new Vector2(1f, 0.5f), new Vector2(1f, 0.5f),
                new Vector2(-65f, 0f), new Vector2(150f, 32f));
            sellButton.interactable = total > 0 && total <= int.MaxValue;
            sellButton.onClick.AddListener(() =>
            {
                if (_business == null)
                    return;
                _status.text = _sell(_business, stackId, quantity);
                _sellQuantities.Remove(stackId);
                RefreshCatalogue();
            });
        }

        private RectTransform CreateRow(int rowIndex, string objectName)
        {
            var rowObject = new GameObject(objectName, typeof(RectTransform), typeof(Image));
            rowObject.transform.SetParent(_content, false);
            rowObject.GetComponent<Image>().color = RowColor;

            RectTransform rowRect = rowObject.GetComponent<RectTransform>();
            rowRect.anchorMin = new Vector2(0f, 1f);
            rowRect.anchorMax = new Vector2(1f, 1f);
            rowRect.pivot = new Vector2(0.5f, 1f);
            rowRect.anchoredPosition = new Vector2(0f, -rowIndex * RowHeight);
            rowRect.sizeDelta = new Vector2(0f, RowHeight - 3f);
            return rowRect;
        }

        private void BuildIfNeeded()
        {
            if (_root != null)
                return;

            EnsureEventSystem();
            _font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (_font == null)
                _font = Font.CreateDynamicFontFromOSFont("Arial", 16);

            var canvasObject = new GameObject("RetailCanvas", typeof(RectTransform), typeof(Canvas),
                typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvasObject.transform.SetParent(transform, false);
            Canvas canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 120;
            CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1280f, 720f);
            scaler.matchWidthOrHeight = 0.5f;

            var panelObject = new GameObject("RetailPanel", typeof(RectTransform), typeof(Image));
            panelObject.transform.SetParent(canvasObject.transform, false);
            panelObject.GetComponent<Image>().color = PanelColor;
            _root = panelObject;
            RectTransform rootRect = panelObject.GetComponent<RectTransform>();
            rootRect.anchorMin = rootRect.anchorMax = new Vector2(0.5f, 0.5f);
            rootRect.pivot = new Vector2(0.5f, 0.5f);
            rootRect.sizeDelta = new Vector2(620f, 560f);

            _title = NewText("Title", rootRect, 23, AccentColor, TextAnchor.MiddleLeft);
            SetRect(_title.rectTransform, new Vector2(0f, 1f), new Vector2(1f, 1f),
                new Vector2(18f, -34f), new Vector2(-150f, 42f));

            Button closeButton = NewButton("CloseButton", rootRect, "CLOSE", new Color32(75, 49, 53, 255));
            SetRect(closeButton.GetComponent<RectTransform>(), new Vector2(1f, 1f), new Vector2(1f, 1f),
                new Vector2(-58f, -34f), new Vector2(92f, 34f));
            closeButton.onClick.AddListener(Close);

            _buyTab = NewButton("BuyTab", rootRect, "BUY", ButtonColor);
            SetRect(_buyTab.GetComponent<RectTransform>(), new Vector2(0f, 1f), new Vector2(0f, 1f),
                new Vector2(70f, -76f), new Vector2(110f, 30f));
            _buyTab.onClick.AddListener(() => SelectMode(false));

            _sellTab = NewButton("SellTab", rootRect, "SELL", ButtonColor);
            SetRect(_sellTab.GetComponent<RectTransform>(), new Vector2(0f, 1f), new Vector2(0f, 1f),
                new Vector2(190f, -76f), new Vector2(110f, 30f));
            _sellTab.onClick.AddListener(() => SelectMode(true));

            var viewportObject = new GameObject("CatalogueViewport", typeof(RectTransform), typeof(Image), typeof(RectMask2D));
            viewportObject.transform.SetParent(rootRect, false);
            RectTransform viewport = viewportObject.GetComponent<RectTransform>();
            SetTopRect(viewport, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
                new Vector2(0f, -112f), new Vector2(574f, 310f));
            viewportObject.GetComponent<Image>().color = new Color(1f, 1f, 1f, 0.015f);

            var contentObject = new GameObject("CatalogueContent", typeof(RectTransform));
            contentObject.transform.SetParent(viewportObject.transform, false);
            _content = contentObject.GetComponent<RectTransform>();
            _content.anchorMin = new Vector2(0f, 1f);
            _content.anchorMax = new Vector2(1f, 1f);
            _content.pivot = new Vector2(0.5f, 1f);
            _content.anchoredPosition = Vector2.zero;
            _content.sizeDelta = new Vector2(0f, 310f);

            _scroll = viewportObject.AddComponent<ScrollRect>();
            _scroll.viewport = viewport;
            _scroll.content = _content;
            _scroll.horizontal = false;
            _scroll.vertical = true;
            _scroll.movementType = ScrollRect.MovementType.Clamped;
            _scroll.scrollSensitivity = 30f;

            Button previous = NewButton("PreviousPage", rootRect, "PREVIOUS", ButtonColor);
            SetRect(previous.GetComponent<RectTransform>(), new Vector2(0f, 0f), new Vector2(0f, 0f),
                new Vector2(78f, 164f), new Vector2(116f, 32f));
            previous.onClick.AddListener(() => ChangePage(-1));

            _pageLabel = NewText("PageLabel", rootRect, 15, TextColor, TextAnchor.MiddleCenter);
            SetRect(_pageLabel.rectTransform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f),
                new Vector2(0f, 164f), new Vector2(320f, 30f));

            Button next = NewButton("NextPage", rootRect, "NEXT", ButtonColor);
            SetRect(next.GetComponent<RectTransform>(), new Vector2(1f, 0f), new Vector2(1f, 0f),
                new Vector2(-78f, 164f), new Vector2(116f, 32f));
            next.onClick.AddListener(() => ChangePage(1));

            _status = NewText("Status", rootRect, 16, AccentColor, TextAnchor.MiddleCenter);
            SetRect(_status.rectTransform, new Vector2(0f, 0f), new Vector2(1f, 0f),
                new Vector2(0f, 76f), new Vector2(-48f, 34f));

            _footer = NewText("Footer", rootRect, 14, TextColor, TextAnchor.MiddleCenter);
            SetRect(_footer.rectTransform, new Vector2(0f, 0f), new Vector2(1f, 0f),
                new Vector2(0f, 30f), new Vector2(-48f, 28f));
        }

        private void ChangePage(int direction)
        {
            if (_business == null)
                return;

            int total = _showSell ? CountSellableStacks() : _business.Merchandise.Count;
            int pageCount = Math.Max(1, (total + PageSize - 1) / PageSize);
            _pageIndex = (_pageIndex + direction + pageCount) % pageCount;
            _status.text = string.Empty;
            RefreshCatalogue();
        }

        private int CountSellableStacks()
        {
            int count = 0;
            foreach (PlacedItem stack in _inventory.Items)
            {
                if (!_ownership.TryGetOwner(stack.ItemInstanceId, out EconomicActorId owner) ||
                    owner != EconomicActorId.Player)
                    continue;
                ItemDefinition definition = FindDefinition(stack.Type.Id);
                if (definition != null && _business.TryGetBuybackOffer(definition, out _))
                    count++;
            }
            return count;
        }

        private Button NewButton(string objectName, Transform parent, string label, Color color)
        {
            var buttonObject = new GameObject(objectName, typeof(RectTransform), typeof(Image), typeof(Button));
            buttonObject.transform.SetParent(parent, false);
            Image image = buttonObject.GetComponent<Image>();
            image.color = color;

            Button button = buttonObject.GetComponent<Button>();
            button.targetGraphic = image;
            ColorBlock colors = button.colors;
            colors.normalColor = color;
            colors.highlightedColor = Color.Lerp(color, Color.white, 0.18f);
            colors.pressedColor = Color.Lerp(color, Color.black, 0.2f);
            colors.selectedColor = colors.highlightedColor;
            colors.disabledColor = new Color32(65, 68, 72, 170);
            button.colors = colors;

            Text text = NewText("Label", buttonObject.transform, 14, Color.white, TextAnchor.MiddleCenter);
            RectTransform textRect = text.rectTransform;
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = new Vector2(4f, 2f);
            textRect.offsetMax = new Vector2(-4f, -2f);
            text.text = label;
            return button;
        }

        private Text NewText(string objectName, Transform parent, int size, Color color, TextAnchor alignment)
        {
            var go = new GameObject(objectName, typeof(RectTransform), typeof(Text));
            go.transform.SetParent(parent, false);
            Text text = go.GetComponent<Text>();
            text.font = _font;
            text.fontSize = size;
            text.color = color;
            text.alignment = alignment;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Overflow;
            text.raycastTarget = false;
            return text;
        }

        private static void SetTopRect(RectTransform rect, Vector2 anchorMin, Vector2 anchorMax,
            Vector2 position, Vector2 size)
        {
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.pivot = new Vector2(0.5f, 1f);
            rect.anchoredPosition = position;
            rect.sizeDelta = size;
        }

        private static void SetRect(RectTransform rect, Vector2 anchorMin, Vector2 anchorMax,
            Vector2 position, Vector2 size)
        {
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = position;
            rect.sizeDelta = size;
        }

        private static void EnsureEventSystem()
        {
            EventSystem eventSystem = EventSystem.current;
            if (eventSystem == null)
            {
                var eventSystemObject = new GameObject("EventSystem", typeof(EventSystem));
                eventSystem = eventSystemObject.GetComponent<EventSystem>();
            }
            InputSystemUIInputModule inputModule = eventSystem.GetComponent<InputSystemUIInputModule>();
            if (inputModule == null)
                inputModule = eventSystem.gameObject.AddComponent<InputSystemUIInputModule>();

            BaseInputModule[] modules = eventSystem.GetComponents<BaseInputModule>();
            for (int i = 0; i < modules.Length; i++)
                if (modules[i] != inputModule)
                    modules[i].enabled = false;
        }
    }
}