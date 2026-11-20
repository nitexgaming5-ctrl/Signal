using System;
using Signal.Core;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

namespace Signal.Game
{
    /// <summary>Player-confirmed fence exchange and Dirty Cash cleaning for the unnamed neighbourhood contact.</summary>
    public sealed class DirtyCashContactPanel : MonoBehaviour
    {
        private static readonly Color PanelColor = new Color32(27, 30, 35, 250);
        private static readonly Color RowColor = new Color32(39, 44, 51, 255);
        private static readonly Color ButtonColor = new Color32(49, 86, 82, 255);
        private static readonly Color TextColor = new Color32(225, 228, 230, 255);
        private Font _font;
        private GameObject _root;
        private RectTransform _cashRows;
        private Text _status;
        private Text _preview;
        private Text _fenceInfo;
        private Button _reviewFence;
        private Button _confirmFence;
        private Button _confirmClean;
        private GridInventory _inventory;
        private AssetOwnershipBook _ownership;
        private ItemDefinition _dirtyCash;
        private ItemDefinition _acceptedItem;
        private int _acceptedQuantity;
        private int _dirtyCashUnits;
        private int _cleaningFeePercent;
        private Func<string> _exchange;
        private Func<Guid, string> _clean;
        private Guid _selectedCashStack;
        private bool _fenceReviewed;

        public bool IsOpen => _root != null && _root.activeSelf;

        public void Bind(GridInventory inventory, AssetOwnershipBook ownership, ItemDefinition dirtyCash, ItemDefinition acceptedItem,
            int acceptedQuantity, int dirtyCashUnits, int cleaningFeePercent,
            Func<string> exchange, Func<Guid, string> clean)
        {
            _inventory = inventory ?? throw new ArgumentNullException(nameof(inventory));
            _ownership = ownership ?? throw new ArgumentNullException(nameof(ownership));
            _dirtyCash = dirtyCash ?? throw new ArgumentNullException(nameof(dirtyCash));
            _acceptedItem = acceptedItem ?? throw new ArgumentNullException(nameof(acceptedItem));
            _acceptedQuantity = acceptedQuantity;
            _dirtyCashUnits = dirtyCashUnits;
            _cleaningFeePercent = cleaningFeePercent;
            _exchange = exchange ?? throw new ArgumentNullException(nameof(exchange));
            _clean = clean ?? throw new ArgumentNullException(nameof(clean));
            BuildIfNeeded();
            _root.SetActive(false);
        }

        public void Open()
        {
            if (_root == null) BuildIfNeeded();
            _selectedCashStack = Guid.Empty;
            _fenceReviewed = false;
            _status.text = string.Empty;
            _root.SetActive(true);
            Refresh();
        }

        public void Close()
        {
            if (IsOpen) _root.SetActive(false);
        }

        private void Update()
        {
            if (IsOpen && Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
                Close();
        }

        private void Refresh()
        {
            if (_inventory == null || _cashRows == null) return;
            for (int i = _cashRows.childCount - 1; i >= 0; i--)
                Destroy(_cashRows.GetChild(i).gameObject);

            ItemType cashType = _dirtyCash.ToItemType();
            int row = 0;
            bool selectedStillExists = false;
            foreach (PlacedItem stack in _inventory.Items)
            {
                if (stack.Type != cashType || !IsPlayerOwned(stack)) continue;
                Guid stackId = stack.ItemInstanceId;
                if (stackId == _selectedCashStack) selectedStillExists = true;
                Button choose = NewButton("DirtyCashStack", "Select Dirty Cash  x" + stack.Count);
                choose.transform.SetParent(_cashRows, false);
                RectTransform rect = choose.GetComponent<RectTransform>();
                rect.anchorMin = new Vector2(0f, 1f);
                rect.anchorMax = new Vector2(1f, 1f);
                rect.pivot = new Vector2(0.5f, 1f);
                rect.anchoredPosition = new Vector2(0f, -row++ * 38f);
                rect.sizeDelta = new Vector2(0f, 34f);
                choose.onClick.AddListener(() => SelectCash(stackId));
            }

            if (!selectedStillExists) _selectedCashStack = Guid.Empty;
            _cashRows.sizeDelta = new Vector2(0f, Math.Max(38, row * 38));
            _preview.text = BuildCleaningPreview();
            _confirmClean.interactable = _selectedCashStack != Guid.Empty;
            bool canFence = HasEligibleFenceStack();
            _reviewFence.interactable = canFence;
            _confirmFence.interactable = canFence;
            _reviewFence.gameObject.SetActive(!_fenceReviewed);
            _confirmFence.gameObject.SetActive(_fenceReviewed);
            _fenceInfo.text = "FENCE OFFER\nBring " + _acceptedQuantity + " stolen " + _acceptedItem.DisplayName +
                " items. Payment: " + _dirtyCashUnits + " Dirty Cash units.\n" +
                (canFence ? "Offer available." : "Requires an eligible stolen stack in carried inventory.");
        }

        private void SelectCash(Guid stackId)
        {
            _selectedCashStack = stackId;
            _status.text = string.Empty;
            Refresh();
        }

        private bool HasEligibleFenceStack()
        {
            ItemType acceptedType = _acceptedItem.ToItemType();
            foreach (PlacedItem stack in _inventory.Items)
                if (stack.Type == acceptedType && stack.Count == _acceptedQuantity &&
                    stack.State.Ownership == ItemOwnershipStatus.Stolen && IsPlayerOwned(stack))
                    return true;
            return false;
        }

        private bool IsPlayerOwned(PlacedItem stack)
        {
            return _ownership.TryGetOwner(stack.ItemInstanceId, out EconomicActorId owner) &&
                owner == EconomicActorId.Player;
        }

        private string BuildCleaningPreview()
        {
            if (_selectedCashStack == Guid.Empty ||
                !_inventory.TryGetItemInstance(_selectedCashStack, out PlacedItem stack))
                return "Select a Dirty Cash stack to preview cleaning.";
            if (!DirtyCashRules.TryGetCleaningAmounts(stack.Count, _cleaningFeePercent, out int payout, out int feeAmount))
                return "That Dirty Cash quantity cannot be cleaned.";
            int faceValue = stack.Count * DirtyCashRules.FaceValuePerUnit;
            return "Selected: " + stack.Count + " Dirty Cash units  |  Face value $" + faceValue +
                "  |  Contact keeps $" + feeAmount + " (" + _cleaningFeePercent + "%)  |  You receive $" + payout + " clean CARD.";
        }

        private void BuildIfNeeded()
        {
            if (_root != null) return;
            EnsureEventSystem();
            _font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (_font == null) _font = Font.CreateDynamicFontFromOSFont("Arial", 16);

            var canvasObject = new GameObject("DirtyCashContactCanvas", typeof(RectTransform), typeof(Canvas),
                typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvasObject.transform.SetParent(transform, false);
            Canvas canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 140;
            CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1280f, 720f);
            scaler.matchWidthOrHeight = 0.5f;

            var panel = new GameObject("ContactPanel", typeof(RectTransform), typeof(Image));
            panel.transform.SetParent(canvasObject.transform, false);
            panel.GetComponent<Image>().color = PanelColor;
            _root = panel;
            RectTransform root = panel.GetComponent<RectTransform>();
            root.anchorMin = root.anchorMax = new Vector2(0.5f, 0.5f);
            root.pivot = new Vector2(0.5f, 0.5f);
            root.sizeDelta = new Vector2(650f, 520f);

            Text title = NewText("Title", root, 24, new Color32(92, 225, 204, 255));
            SetRect(title.rectTransform, new Vector2(0f, 1f), new Vector2(1f, 1f),
                new Vector2(22f, -34f), new Vector2(-130f, 42f));
            title.text = "A discreet arrangement";

            Button close = NewButton("Close", "CLOSE");
            close.transform.SetParent(root, false);
            SetRect(close.GetComponent<RectTransform>(), new Vector2(1f, 1f), new Vector2(1f, 1f),
                new Vector2(-62f, -34f), new Vector2(96f, 34f));
            close.onClick.AddListener(Close);

            _fenceInfo = NewText("FenceOffer", root, 17, TextColor);
            SetRect(_fenceInfo.rectTransform, new Vector2(0f, 1f), new Vector2(1f, 1f),
                new Vector2(22f, -100f), new Vector2(-44f, 68f));
            _fenceInfo.text = "FENCE OFFER\nBring " + _acceptedQuantity + " stolen " + _acceptedItem.DisplayName +
                " items. Payment: " + _dirtyCashUnits + " Dirty Cash units.";

            _reviewFence = NewButton("ReviewFence", "REVIEW EXCHANGE");
            _reviewFence.transform.SetParent(root, false);
            SetRect(_reviewFence.GetComponent<RectTransform>(), new Vector2(0f, 1f), new Vector2(0f, 1f),
                new Vector2(110f, -165f), new Vector2(190f, 34f));
            _reviewFence.onClick.AddListener(() => { _fenceReviewed = true; Refresh(); });

            _confirmFence = NewButton("ConfirmFence", "CONFIRM EXCHANGE");
            _confirmFence.transform.SetParent(root, false);
            SetRect(_confirmFence.GetComponent<RectTransform>(), new Vector2(0f, 1f), new Vector2(0f, 1f),
                new Vector2(110f, -165f), new Vector2(190f, 34f));
            _confirmFence.onClick.AddListener(() =>
            {
                _status.text = _exchange();
                _fenceReviewed = false;
                Refresh();
            });

            Text cleaningTitle = NewText("CleaningTitle", root, 18, new Color32(92, 225, 204, 255));
            SetRect(cleaningTitle.rectTransform, new Vector2(0f, 1f), new Vector2(1f, 1f),
                new Vector2(22f, -220f), new Vector2(-44f, 30f));
            cleaningTitle.text = "CLEAN DIRTY CASH  —  " + _cleaningFeePercent + "% fee, paid immediately";

            var scrollObject = new GameObject("DirtyCashScroll", typeof(RectTransform), typeof(Image), typeof(ScrollRect));
            scrollObject.transform.SetParent(root, false);
            SetRect(scrollObject.GetComponent<RectTransform>(), new Vector2(0f, 1f), new Vector2(1f, 1f),
                new Vector2(22f, -250f), new Vector2(-44f, 130f));
            scrollObject.GetComponent<Image>().color = RowColor;
            var viewport = new GameObject("Viewport", typeof(RectTransform), typeof(Image), typeof(Mask));
            viewport.transform.SetParent(scrollObject.transform, false);
            RectTransform viewportRect = viewport.GetComponent<RectTransform>();
            viewportRect.anchorMin = Vector2.zero;
            viewportRect.anchorMax = Vector2.one;
            viewportRect.offsetMin = Vector2.zero;
            viewportRect.offsetMax = Vector2.zero;
            viewport.GetComponent<Image>().color = Color.white;
            viewport.GetComponent<Mask>().showMaskGraphic = false;
            var rows = new GameObject("Rows", typeof(RectTransform));
            rows.transform.SetParent(viewport.transform, false);
            _cashRows = rows.GetComponent<RectTransform>();
            _cashRows.anchorMin = new Vector2(0f, 1f);
            _cashRows.anchorMax = new Vector2(1f, 1f);
            _cashRows.pivot = new Vector2(0.5f, 1f);
            _cashRows.sizeDelta = new Vector2(0f, 38f);
            ScrollRect scroll = scrollObject.GetComponent<ScrollRect>();
            scroll.viewport = viewportRect;
            scroll.content = _cashRows;
            scroll.horizontal = false;
            scroll.vertical = true;

            _preview = NewText("CleaningPreview", root, 15, TextColor);
            SetRect(_preview.rectTransform, new Vector2(0f, 0f), new Vector2(1f, 0f),
                new Vector2(22f, 66f), new Vector2(-44f, 36f));
            _confirmClean = NewButton("ConfirmCleaning", "CONFIRM CLEANING");
            _confirmClean.transform.SetParent(root, false);
            SetRect(_confirmClean.GetComponent<RectTransform>(), new Vector2(1f, 0f), new Vector2(1f, 0f),
                new Vector2(-112f, 26f), new Vector2(190f, 34f));
            _confirmClean.onClick.AddListener(() =>
            {
                if (_selectedCashStack == Guid.Empty) return;
                _status.text = _clean(_selectedCashStack);
                _selectedCashStack = Guid.Empty;
                Refresh();
            });

            _status = NewText("Status", root, 15, new Color32(240, 195, 120, 255));
            SetRect(_status.rectTransform, new Vector2(0f, 0f), new Vector2(1f, 0f),
                new Vector2(22f, 18f), new Vector2(-230f, 32f));
            _root.SetActive(false);
        }

        private Text NewText(string name, Transform parent, int size, Color color)
        {
            var obj = new GameObject(name, typeof(RectTransform), typeof(Text));
            obj.transform.SetParent(parent, false);
            Text text = obj.GetComponent<Text>();
            text.font = _font;
            text.fontSize = size;
            text.color = color;
            text.alignment = TextAnchor.MiddleLeft;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Overflow;
            return text;
        }

        private Button NewButton(string name, string label)
        {
            var obj = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
            obj.GetComponent<Image>().color = ButtonColor;
            Button button = obj.GetComponent<Button>();
            Text text = NewText("Label", obj.transform, 14, Color.white);
            SetRect(text.rectTransform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            text.alignment = TextAnchor.MiddleCenter;
            text.text = label;
            return button;
        }

        private static void SetRect(RectTransform rect, Vector2 anchorMin, Vector2 anchorMax,
            Vector2 anchoredPosition, Vector2 size)
        {
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = anchoredPosition;
            rect.sizeDelta = size;
        }

        private static void EnsureEventSystem()
        {
            if (EventSystem.current == null)
            {
                var eventSystem = new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
                eventSystem.GetComponent<InputSystemUIInputModule>().AssignDefaultActions();
            }
            else if (EventSystem.current.GetComponent<InputSystemUIInputModule>() == null)
            {
                EventSystem.current.gameObject.AddComponent<InputSystemUIInputModule>().AssignDefaultActions();
            }
        }
    }
}
