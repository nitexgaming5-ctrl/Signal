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
    /// <summary>Shows and moves individual item stacks between carried inventory and home storage.</summary>
    public sealed class StoragePanel : MonoBehaviour
    {
        private static readonly Color BackdropColor = new Color32(8, 11, 15, 232);
        private static readonly Color FrameColor = new Color32(32, 38, 43, 255);
        private static readonly Color RowColor = new Color32(48, 54, 57, 255);
        private static readonly Color InkColor = new Color32(235, 226, 207, 255);
        private static readonly Color MutedColor = new Color32(185, 181, 168, 255);
        private static readonly Color AccentColor = new Color32(202, 163, 91, 255);

        private GridInventory _carried;
        private readonly List<GridInventory> _carriedInventories = new List<GridInventory>();
        private Func<PlacedItem, GridInventory> _carryDestination;
        private GridInventory _stored;
        private Action<PlacedItem> _inspectItem;
        private ItemKnowledgeBook _itemKnowledge;
        private readonly Dictionary<Guid, string> _names = new Dictionary<Guid, string>();
        private GameObject _canvasObject;
        private RectTransform _rows;
        private Text _status;
        private bool _built;

        public bool IsOpen { get; private set; }

        public void Bind(GridInventory carried, GridInventory stored, IReadOnlyList<ItemDefinition> definitions,
                         Action<PlacedItem> inspectItem = null, ItemKnowledgeBook itemKnowledge = null)
        {
            if (carried == null) throw new ArgumentNullException(nameof(carried));
            if (stored == null) throw new ArgumentNullException(nameof(stored));

            for (int i = 0; i < _carriedInventories.Count; i++)
                _carriedInventories[i].Changed -= OnInventoryChanged;
            if (_stored != null) _stored.Changed -= OnInventoryChanged;

            _carried = carried;
            _carriedInventories.Clear();
            _carriedInventories.Add(carried);
            _stored = stored;
            _inspectItem = inspectItem;
            if (_itemKnowledge != null)
                _itemKnowledge.Changed -= OnKnowledgeChanged;
            _itemKnowledge = itemKnowledge;
            if (_itemKnowledge != null)
                _itemKnowledge.Changed += OnKnowledgeChanged;
            _carried.Changed += OnInventoryChanged;
            _stored.Changed += OnInventoryChanged;

            _names.Clear();
            if (definitions != null)
            {
                foreach (ItemDefinition definition in definitions)
                {
                    if (definition == null) continue;
                    try
                    {
                        ItemType type = definition.ToItemType();
                        _names[type.Id] = string.IsNullOrWhiteSpace(definition.DisplayName)
                            ? definition.name : definition.DisplayName;
                    }
                    catch (InvalidOperationException)
                    {
                        Debug.LogWarning("StoragePanel: skipping item with an invalid id: " + definition.name);
                    }
                }
            }

            BuildIfNeeded();
            Refresh();
            SetOpen(false);
        }

        public void SetCarriedInventories(IReadOnlyList<GridInventory> inventories,
            Func<PlacedItem, GridInventory> carryDestination)
        {
            for (int i = 0; i < _carriedInventories.Count; i++)
                _carriedInventories[i].Changed -= OnInventoryChanged;
            _carriedInventories.Clear();
            if (inventories != null)
                for (int i = 0; i < inventories.Count; i++)
                {
                    GridInventory inventory = inventories[i];
                    if (inventory == null || _carriedInventories.Contains(inventory)) continue;
                    _carriedInventories.Add(inventory);
                    inventory.Changed += OnInventoryChanged;
                }
            _carried = _carriedInventories.Count > 0 ? _carriedInventories[0] : null;
            _carryDestination = carryDestination;
            if (IsOpen) Refresh();
        }

        public void SetOpen(bool open)
        {
            IsOpen = open;
            if (_canvasObject != null)
                _canvasObject.SetActive(open);
            if (open)
                Refresh();
        }

        private void Update()
        {
            if (IsOpen && Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
                SetOpen(false);
        }

        private void OnDestroy()
        {
            if (_carried != null) _carried.Changed -= OnInventoryChanged;
            if (_stored != null) _stored.Changed -= OnInventoryChanged;
            if (_itemKnowledge != null) _itemKnowledge.Changed -= OnKnowledgeChanged;
        }

        private void OnInventoryChanged(GridInventory inventory)
        {
            if (IsOpen) Refresh();
        }

        private void OnKnowledgeChanged(ItemKnowledgeBook knowledge)
        {
            if (IsOpen) Refresh();
        }

        private void BuildIfNeeded()
        {
            if (_built) return;
            EnsureEventSystem();

            var canvasObject = new GameObject("StorageCanvas", typeof(RectTransform), typeof(Canvas),
                                              typeof(CanvasScaler), typeof(GraphicRaycaster));
            _canvasObject = canvasObject;
            _canvasObject.transform.SetParent(transform, false);
            Canvas canvas = _canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 180;

            CanvasScaler scaler = _canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;

            RectTransform backdrop = Panel(_canvasObject.transform, "Backdrop", BackdropColor);
            Stretch(backdrop);

            RectTransform frame = Panel(backdrop, "StorageFrame", FrameColor);
            Center(frame, new Vector2(1120f, 760f));

            Text title = Label(frame, "HOME STORAGE", 28, InkColor, TextAnchor.MiddleLeft);
            Anchor(title.rectTransform, new Vector2(30f, -24f), new Vector2(650f, 46f), true);

            Text subtitle = Label(frame, "Inspect individual objects or move a complete stack between your pockets and locker.", 17, MutedColor, TextAnchor.MiddleLeft);
            Anchor(subtitle.rectTransform, new Vector2(30f, -70f), new Vector2(900f, 32f), true);

            Button close = MakeButton(frame, "CLOSE  [ESC]", new Vector2(190f, 48f));
            RectTransform closeRect = close.transform as RectTransform;
            closeRect.anchorMin = closeRect.anchorMax = new Vector2(1f, 1f);
            closeRect.pivot = new Vector2(1f, 1f);
            closeRect.anchoredPosition = new Vector2(-28f, -24f);
            close.onClick.AddListener(() => SetOpen(false));

            Text columns = Label(frame, "ITEM / KNOWN DETAILS                                               LOCATION                                  ACTIONS", 15, MutedColor, TextAnchor.MiddleLeft);
            Anchor(columns.rectTransform, new Vector2(30f, -120f), new Vector2(1050f, 28f), true);

            RectTransform viewport = Panel(frame, "Viewport", new Color32(20, 24, 27, 255));
            Stretch(viewport, new Vector2(24f, 74f), new Vector2(-24f, -158f));
            viewport.gameObject.AddComponent<RectMask2D>();

            RectTransform content = new GameObject("Rows", typeof(RectTransform), typeof(VerticalLayoutGroup),
                                                    typeof(ContentSizeFitter)).GetComponent<RectTransform>();
            content.SetParent(viewport, false);
            content.anchorMin = new Vector2(0f, 1f);
            content.anchorMax = new Vector2(1f, 1f);
            content.pivot = new Vector2(0.5f, 1f);
            content.anchoredPosition = Vector2.zero;
            content.sizeDelta = Vector2.zero;

            VerticalLayoutGroup layout = content.GetComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(12, 12, 12, 12);
            layout.spacing = 8f;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;

            ContentSizeFitter fitter = content.GetComponent<ContentSizeFitter>();
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            ScrollRect scroll = viewport.gameObject.AddComponent<ScrollRect>();
            scroll.viewport = viewport;
            scroll.content = content;
            scroll.horizontal = false;
            scroll.vertical = true;
            scroll.movementType = ScrollRect.MovementType.Clamped;
            _rows = content;

            _status = Label(frame, "The locker is empty.", 16, InkColor, TextAnchor.MiddleLeft);
            Anchor(_status.rectTransform, new Vector2(30f, 22f), new Vector2(1000f, 32f), false);

            _canvasObject.SetActive(false);
            _built = true;
        }

        private void Refresh()
        {
            if (_rows == null || _carried == null || _stored == null) return;
            ClearRows();

            int carriedCount = 0;
            for (int i = 0; i < _carriedInventories.Count; i++)
                carriedCount += _carriedInventories[i].Items.Count;
            int count = carriedCount + _stored.Items.Count;
            if (count == 0)
            {
                AddEmptyRow();
                _status.text = "Nothing here yet. Bring something back from the city.";
                return;
            }

            _status.text = "Backpack and all personal pockets are accessible here. Transfers preserve item identity.";
            for (int i = 0; i < _carriedInventories.Count; i++)
                AddInventoryRows(_carriedInventories[i], true);
            AddInventoryRows(_stored, false);
        }

        private void AddInventoryRows(GridInventory inventory, bool carried)
        {
            // Refresh is triggered by inventory change events, so this read-only pass is stable.
            foreach (PlacedItem item in inventory.Items)
                AddItemRow(item, inventory, carried);
        }

        private void AddItemRow(PlacedItem item, GridInventory source, bool carried)
        {
            RectTransform row = Panel(_rows, "ItemStack_" + item.ItemInstanceId.ToString("N"), RowColor);
            LayoutElement rowLayout = row.gameObject.AddComponent<LayoutElement>();
            rowLayout.minHeight = 76f;
            rowLayout.preferredHeight = 76f;

            HorizontalLayoutGroup layout = row.gameObject.AddComponent<HorizontalLayoutGroup>();
            layout.padding = new RectOffset(14, 14, 8, 8);
            layout.spacing = 10f;
            layout.childAlignment = TextAnchor.MiddleLeft;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = false;
            layout.childForceExpandHeight = true;

            string name = _names.TryGetValue(item.Type.Id, out string display) ? display : "Unknown item";
            string detail = name + "  ·  " + FormatKnownState(item.ItemInstanceId) + "  x" + item.Count;
            Text itemLabel = Label(row, detail, 16, InkColor, TextAnchor.MiddleLeft);
            itemLabel.gameObject.AddComponent<LayoutElement>().flexibleWidth = 1f;

            Text location = Label(row, carried ? "CARRIED" : "LOCKER", 13, AccentColor, TextAnchor.MiddleCenter);
            location.gameObject.AddComponent<LayoutElement>().preferredWidth = 110f;

            Button inspect = MakeButton(row, "INSPECT", new Vector2(100f, 42f));
            inspect.gameObject.AddComponent<LayoutElement>().preferredWidth = 100f;
            inspect.onClick.AddListener(() => _inspectItem?.Invoke(item));

            string action = carried ? "STORE" : "TAKE";
            Button transfer = MakeButton(row, action, new Vector2(100f, 42f));
            transfer.gameObject.AddComponent<LayoutElement>().preferredWidth = 100f;
            transfer.onClick.AddListener(() =>
            {
                GridInventory destination = carried
                    ? _stored
                    : (_carryDestination != null ? _carryDestination(item) : _carried);
                Transfer(item, carried ? source : _stored, destination, carried ? "Stored" : "Taken");
            });
        }

        private string FormatKnownState(Guid itemInstanceId)
        {
            if (_itemKnowledge == null)
                return "Q:?  C:?  A:?  O:?";

            string quality = _itemKnowledge.TryGetFact(itemInstanceId, ItemKnowledgeAspect.Quality, out ItemKnowledgeFact q)
                ? ((ItemQuality)q.Value).ToString().ToUpperInvariant() : "?";
            string condition = _itemKnowledge.TryGetFact(itemInstanceId, ItemKnowledgeAspect.Condition, out ItemKnowledgeFact c)
                ? ((ItemCondition)c.Value).ToString().ToUpperInvariant() : "?";
            string authenticity = _itemKnowledge.TryGetFact(itemInstanceId, ItemKnowledgeAspect.Authenticity, out ItemKnowledgeFact a)
                ? ((ItemAuthenticity)a.Value).ToString().ToUpperInvariant() : "?";
            string ownership = _itemKnowledge.TryGetFact(itemInstanceId, ItemKnowledgeAspect.Ownership, out ItemKnowledgeFact o)
                ? ((ItemOwnershipStatus)o.Value).ToString().ToUpperInvariant() : "?";

            return "Q:" + quality + "  C:" + condition + "  A:" + authenticity + "  O:" + ownership;
        }

        private void Transfer(PlacedItem item, GridInventory source, GridInventory destination, string verb)
        {
            if (source == null || destination == null)
            {
                _status.text = "No accessible carry space for that item. The locker item was kept.";
                return;
            }
            InventoryTransferResult result = InventoryTransfer.MoveStack(source, destination, item.ItemInstanceId);
            string name = _names.TryGetValue(item.Type.Id, out string displayName) ? displayName : "item";

            if (!result.Success)
                _status.text = "Transfer failed; nothing was moved.";
            else if (result.Moved == 0)
                _status.text = "No room in the destination for this complete stack.";
            else
                _status.text = verb + " " + result.Moved + " x " + name + ".";
        }

        private void AddEmptyRow()
        {
            RectTransform row = Panel(_rows, "EmptyRow", RowColor);
            row.gameObject.AddComponent<LayoutElement>().preferredHeight = 64f;
            Label(row, "No items in your pockets or locker.", 18, MutedColor, TextAnchor.MiddleCenter);
        }

        private void ClearRows()
        {
            for (int i = _rows.childCount - 1; i >= 0; i--)
            {
                GameObject child = _rows.GetChild(i).gameObject;
                child.SetActive(false);
                Destroy(child);
            }
        }

        private void EnsureEventSystem()
        {
            EventSystem current = EventSystem.current;
            if (current != null)
            {
                if (current.GetComponent<InputSystemUIInputModule>() == null)
                    current.gameObject.AddComponent<InputSystemUIInputModule>();
                return;
            }

            var eventObject = new GameObject("StorageEventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
            eventObject.transform.SetParent(transform, false);
        }

        private static RectTransform Panel(Transform parent, string name, Color color)
        {
            GameObject go = new GameObject(name, typeof(RectTransform), typeof(Image));
            go.transform.SetParent(parent, false);
            go.GetComponent<Image>().color = color;
            return go.GetComponent<RectTransform>();
        }

        private static Text Label(Transform parent, string value, int size, Color color, TextAnchor alignment)
        {
            GameObject go = new GameObject("Text", typeof(RectTransform), typeof(Text));
            go.transform.SetParent(parent, false);
            Text text = go.GetComponent<Text>();
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (text.font == null) text.font = Font.CreateDynamicFontFromOSFont("Arial", size);
            text.fontSize = size;
            text.color = color;
            text.alignment = alignment;
            text.text = value;
            text.raycastTarget = false;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Truncate;
            return text;
        }

        private static Button MakeButton(Transform parent, string value, Vector2 size)
        {
            RectTransform rect = Panel(parent, value + "Button", new Color32(75, 91, 88, 255));
            rect.sizeDelta = size;
            Button button = rect.gameObject.AddComponent<Button>();
            button.targetGraphic = rect.GetComponent<Image>();
            Text text = Label(rect, value, 15, InkColor, TextAnchor.MiddleCenter);
            Stretch(text.rectTransform);
            return button;
        }

        private static void Center(RectTransform rect, Vector2 size)
        {
            rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = size;
        }

        private static void Stretch(RectTransform rect)
        {
            Stretch(rect, Vector2.zero, Vector2.zero);
        }

        private static void Stretch(RectTransform rect, Vector2 minOffset, Vector2 maxOffset)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = minOffset;
            rect.offsetMax = maxOffset;
        }

        private static void Anchor(RectTransform rect, Vector2 position, Vector2 size, bool fromTop)
        {
            if (fromTop)
            {
                rect.anchorMin = rect.anchorMax = new Vector2(0f, 1f);
                rect.pivot = new Vector2(0f, 1f);
                rect.anchoredPosition = position;
                rect.sizeDelta = size;
            }
            else
            {
                rect.anchorMin = rect.anchorMax = Vector2.zero;
                rect.pivot = Vector2.zero;
                rect.anchoredPosition = position;
                rect.sizeDelta = size;
            }
        }
    }
}