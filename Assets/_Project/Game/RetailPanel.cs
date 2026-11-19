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
    /// <summary>Reusable click-driven catalogue panel for authored businesses.</summary>
    public sealed class RetailPanel : MonoBehaviour
    {
        private const int PageSize = 8;
        private const float RowHeight = 42f;

        private static readonly Color PanelColor = new Color32(28, 31, 36, 248);
        private static readonly Color TextColor = new Color32(224, 226, 230, 255);
        private static readonly Color AccentColor = new Color32(62, 230, 210, 255);
        private static readonly Color ButtonColor = new Color32(43, 91, 91, 255);
        private static readonly Color RowColor = new Color32(38, 43, 50, 255);

        private Font _font;
        private GameObject _root;
        private Text _title;
        private Text _status;
        private Text _pageLabel;
        private RectTransform _content;
        private ScrollRect _scroll;
        private BusinessDefinition _business;
        private int _pageIndex;
        private Func<BusinessDefinition, ItemDefinition, string> _purchase;
        private Action _closed;

        public bool IsOpen => _root != null && _root.activeSelf;

        public void Bind(Func<BusinessDefinition, ItemDefinition, string> purchase, Action closed)
        {
            _purchase = purchase ?? throw new ArgumentNullException(nameof(purchase));
            _closed = closed;
            BuildIfNeeded();
            _root.SetActive(false);
        }

        public void Open(BusinessDefinition business)
        {
            if (business == null)
                return;

            BuildIfNeeded();
            _business = business;
            _pageIndex = 0;
            _title.text = business.DisplayName + "  —  " + business.BusinessType;
            _status.text = string.Empty;
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

        private void RefreshCatalogue()
        {
            if (_business == null || _content == null)
                return;

            for (int i = _content.childCount - 1; i >= 0; i--)
                Destroy(_content.GetChild(i).gameObject);

            int total = _business.Merchandise.Count;
            int pageCount = Math.Max(1, (total + PageSize - 1) / PageSize);
            _pageIndex = Mathf.Clamp(_pageIndex, 0, pageCount - 1);
            int start = _pageIndex * PageSize;
            int shown = Math.Min(PageSize, Math.Max(0, total - start));

            for (int i = 0; i < shown; i++)
            {
                BusinessDefinition.MerchandiseEntry entry = _business.Merchandise[start + i];
                CreateMerchandiseRow(i, entry);
            }

            _content.sizeDelta = new Vector2(0f, Math.Max(330f, shown * RowHeight));
            _scroll.verticalNormalizedPosition = 1f;
            _pageLabel.text = total == 0 ? "No products" : "Page " + (_pageIndex + 1) + " / " + pageCount;
        }

        private void CreateMerchandiseRow(int rowIndex, BusinessDefinition.MerchandiseEntry entry)
        {
            var rowObject = new GameObject("MerchandiseRow", typeof(RectTransform), typeof(Image));
            rowObject.transform.SetParent(_content, false);
            Image rowImage = rowObject.GetComponent<Image>();
            rowImage.color = RowColor;

            RectTransform rowRect = rowObject.GetComponent<RectTransform>();
            rowRect.anchorMin = new Vector2(0f, 1f);
            rowRect.anchorMax = new Vector2(1f, 1f);
            rowRect.pivot = new Vector2(0.5f, 1f);
            rowRect.anchoredPosition = new Vector2(0f, -rowIndex * RowHeight);
            rowRect.sizeDelta = new Vector2(0f, RowHeight - 3f);

            string itemName = entry != null && entry.Item != null
                ? entry.Item.DisplayName
                : "Unconfigured merchandise";
            string price = entry != null && entry.Item != null && entry.UnitPrice > 0
                ? "$" + entry.UnitPrice
                : "Unavailable";

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
                    if (_business != null)
                        _status.text = _purchase(_business, selectedItem);
                });
            }
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

            var viewportObject = new GameObject("CatalogueViewport", typeof(RectTransform), typeof(Image), typeof(RectMask2D));
            viewportObject.transform.SetParent(rootRect, false);
            RectTransform viewport = viewportObject.GetComponent<RectTransform>();
            SetTopRect(viewport, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
                new Vector2(0f, -82f), new Vector2(574f, 340f));
            viewportObject.GetComponent<Image>().color = new Color(1f, 1f, 1f, 0.015f);

            var contentObject = new GameObject("CatalogueContent", typeof(RectTransform));
            contentObject.transform.SetParent(viewportObject.transform, false);
            _content = contentObject.GetComponent<RectTransform>();
            _content.anchorMin = new Vector2(0f, 1f);
            _content.anchorMax = new Vector2(1f, 1f);
            _content.pivot = new Vector2(0.5f, 1f);
            _content.anchoredPosition = Vector2.zero;
            _content.sizeDelta = new Vector2(0f, 330f);

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
                new Vector2(0f, 164f), new Vector2(180f, 30f));

            Button next = NewButton("NextPage", rootRect, "NEXT", ButtonColor);
            SetRect(next.GetComponent<RectTransform>(), new Vector2(1f, 0f), new Vector2(1f, 0f),
                new Vector2(-78f, 164f), new Vector2(116f, 32f));
            next.onClick.AddListener(() => ChangePage(1));

            _status = NewText("Status", rootRect, 16, AccentColor, TextAnchor.MiddleCenter);
            SetRect(_status.rectTransform, new Vector2(0f, 0f), new Vector2(1f, 0f),
                new Vector2(0f, 76f), new Vector2(-48f, 34f));

            Text footer = NewText("Footer", rootRect, 14, TextColor, TextAnchor.MiddleCenter);
            footer.text = "Choose BUY to purchase with your CARD balance  •  ESC to close";
            SetRect(footer.rectTransform, new Vector2(0f, 0f), new Vector2(1f, 0f),
                new Vector2(0f, 30f), new Vector2(-48f, 28f));
        }

        private void ChangePage(int direction)
        {
            if (_business == null)
                return;

            int pageCount = Math.Max(1, (_business.Merchandise.Count + PageSize - 1) / PageSize);
            _pageIndex = (_pageIndex + direction + pageCount) % pageCount;
            _status.text = string.Empty;
            RefreshCatalogue();
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
                var eventSystemObject = new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
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
