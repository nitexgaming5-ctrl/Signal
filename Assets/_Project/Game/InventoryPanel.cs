using System;
using System.Collections.Generic;
using Signal.Core;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace Signal.Game
{
    /// <summary>
    /// Grid inventory view, built entirely from code (placeholder look).
    /// Tab opens or closes it, left mouse drags an item, right mouse inspects an item, R turns the dragged item, Esc cancels the drag.
    /// All rules live in GridInventory; this class only reads input and draws.
    /// </summary>
    public sealed class InventoryPanel : MonoBehaviour
    {
        private const float ItemInset = 2f;
        private const float Padding = 16f;
        private const float TitleHeight = 28f;
        private const int LabelFontSize = 14;

        private static readonly Color PanelColor = new Color32(0x0E, 0x11, 0x16, 0xF2);
        private static readonly Color CellColor = new Color32(0x23, 0x28, 0x30, 0xFF);
        private static readonly Color InkColor = new Color32(0x0E, 0x11, 0x16, 0xFF);
        private static readonly Color TitleColor = new Color32(0xC9, 0xCE, 0xD6, 0xFF);
        private static readonly Color FallbackColor = new Color32(0x6B, 0x72, 0x80, 0xFF);
        private static readonly Color ValidDropColor = new Color32(0x3E, 0xE6, 0xD2, 0x66);
        private static readonly Color InvalidDropColor = new Color32(0xE2, 0x4F, 0xB5, 0x66);

        private static readonly Color[] Palette =
        {
            new Color32(0x8B, 0x4A, 0x2B, 0xFF), // rust
            new Color32(0xC8, 0xA2, 0x4A, 0xFF), // dull yellow
            new Color32(0x3E, 0xE6, 0xD2, 0xFF), // cyan
            new Color32(0xE2, 0x4F, 0xB5, 0xFF), // magenta
            new Color32(0x6B, 0x72, 0x80, 0xFF), // mid grey
            new Color32(0xC9, 0xCE, 0xD6, 0xFF), // light grey
        };

        [SerializeField] private float cellSize = 64f;

        private struct ItemVisual
        {
            public string Label;
            public Color Color;
            public Sprite Icon;
        }

        private readonly Dictionary<Guid, ItemVisual> _visuals = new Dictionary<Guid, ItemVisual>();

        private GridInventory _inventory;
        private Action<PlacedItem> _inspectItem;
        private Func<PlacedItem, bool> _equipItem;
        private Font _font;
        private bool _built;
        private bool _open;

        private RectTransform _root;
        private RectTransform _gridRect;
        private RectTransform _cellsRoot;
        private RectTransform _itemsRoot;
        private Text _title;
        private Image _preview;
        private Image _ghost;
        private Text _ghostLabel;

        private bool _dragging;
        private int _dragId;
        private ItemType _dragType;
        private bool _dragRotated;
        private int _grabX;
        private int _grabY;

        public bool IsOpen => _open;

        public void Bind(GridInventory inventory, IReadOnlyList<ItemDefinition> definitions, Action<PlacedItem> inspectItem = null,
            Func<PlacedItem, bool> equipItem = null)
        {
            if (inventory == null)
                throw new ArgumentNullException(nameof(inventory));

            CancelDrag();

            if (_inventory != null)
                _inventory.Changed -= OnInventoryChanged;

            _inventory = inventory;
            _inspectItem = inspectItem;
            _equipItem = equipItem;
            _inventory.Changed += OnInventoryChanged;

            _visuals.Clear();
            if (definitions != null)
            {
                for (int i = 0; i < definitions.Count; i++)
                {
                    ItemDefinition definition = definitions[i];
                    if (definition == null)
                        continue;

                    ItemType type;
                    try
                    {
                        type = definition.ToItemType();
                    }
                    catch (InvalidOperationException)
                    {
                        Debug.LogWarning("InventoryPanel: skipping '" + definition.name + "' because it has no valid id.");
                        continue;
                    }

                    string label = string.IsNullOrWhiteSpace(definition.DisplayName) ? definition.name : definition.DisplayName;
                    _visuals[type.Id] = new ItemVisual
                    {
                        Label = label,
                        Color = Palette[i % Palette.Length],
                        Icon = definition.Icon
                    };
                }
            }

            BuildIfNeeded();
            BuildGrid();
            SetOpen(false);
        }

        public void SetOpen(bool open)
        {
            if (!open)
                CancelDrag();

            _open = open;

            if (_root != null)
                _root.gameObject.SetActive(open);

            if (open && _inventory != null)
                Refresh();
        }

        private void OnDestroy()
        {
            if (_inventory != null)
                _inventory.Changed -= OnInventoryChanged;
        }

        private void OnInventoryChanged(GridInventory inventory)
        {
            if (_open)
                Refresh();
        }

        private void Update()
        {
            if (_inventory == null || _root == null)
                return;

            Keyboard keyboard = Keyboard.current;
            Mouse mouse = Mouse.current;

            if (keyboard != null && keyboard.tabKey.wasPressedThisFrame)
                SetOpen(!_open);

            if (!_open || mouse == null)
                return;

            UpdatePointer(keyboard, mouse);
        }

        private void UpdatePointer(Keyboard keyboard, Mouse mouse)
        {
            Vector2 screen = mouse.position.ReadValue();
            bool inGrid = TryGetHoverCell(screen, out int hoverX, out int hoverY, out Vector2 local);

            if (!_dragging && keyboard != null && keyboard.eKey.wasPressedThisFrame && inGrid &&
                _inventory.TryGetItemAt(hoverX, hoverY, out PlacedItem equipTarget))
            {
                if (_equipItem != null && _equipItem(equipTarget))
                    Refresh();
                return;
            }

            if (!_dragging)
            {
                if (mouse.rightButton.wasPressedThisFrame && inGrid &&
                    _inventory.TryGetItemAt(hoverX, hoverY, out PlacedItem inspected))
                {
                    _inspectItem?.Invoke(inspected);
                    return;
                }

                if (!mouse.leftButton.wasPressedThisFrame || !inGrid)
                    return;
                if (!_inventory.TryGetItemAt(hoverX, hoverY, out PlacedItem hit))
                    return;

                BeginDrag(hit, hoverX, hoverY);
            }

            if (keyboard != null && keyboard.escapeKey.wasPressedThisFrame)
            {
                EndDrag();
                return;
            }

            if (keyboard != null && keyboard.rKey.wasPressedThisFrame)
                RotateDrag();

            int targetX = hoverX - _grabX;
            int targetY = hoverY - _grabY;
            bool canDrop = inGrid && _inventory.CanMove(_dragId, targetX, targetY, _dragRotated);

            UpdateDragVisuals(local, inGrid, targetX, targetY, canDrop);

            if (mouse.leftButton.wasReleasedThisFrame)
            {
                int id = _dragId;
                bool rotated = _dragRotated;

                EndDrag();

                if (canDrop)
                    _inventory.TryMove(id, targetX, targetY, rotated);
            }
        }

        private bool TryGetHoverCell(Vector2 screen, out int cellX, out int cellY, out Vector2 local)
        {
            cellX = 0;
            cellY = 0;

            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(_gridRect, screen, null, out local))
                return false;

            // The grid's pivot is its top-left corner, so local.y is zero at the top and negative below it.
            return InventoryDragMath.TryGetCell(local.x, -local.y, cellSize, _inventory.Width, _inventory.Height,
                                                out cellX, out cellY);
        }

        private void BeginDrag(PlacedItem item, int cellX, int cellY)
        {
            _dragging = true;
            _dragId = item.InstanceId;
            _dragType = item.Type;
            _dragRotated = item.Rotated;
            _grabX = cellX - item.X;
            _grabY = cellY - item.Y;

            ItemVisual visual = VisualFor(_dragType);
            Color color = visual.Color;
            color.a = 0.85f;
            _ghost.sprite = visual.Icon;
            _ghost.preserveAspect = visual.Icon != null;
            _ghost.color = visual.Icon != null ? new Color(1f, 1f, 1f, 0.85f) : color;
            _ghostLabel.text = visual.Label;
            _ghostLabel.gameObject.SetActive(visual.Icon == null);
            _ghost.gameObject.SetActive(true);

            Refresh();
        }

        private void RotateDrag()
        {
            int width = _dragRotated ? _dragType.Height : _dragType.Width;
            int height = _dragRotated ? _dragType.Width : _dragType.Height;

            InventoryDragMath.RotateGrabOffsetClockwise(_grabX, _grabY, width, height, out int newX, out int newY);

            _grabX = newX;
            _grabY = newY;
            _dragRotated = !_dragRotated;
        }

        private void CancelDrag()
        {
            if (!_dragging)
                return;

            _dragging = false;

            if (_ghost != null)
            {
                _ghost.gameObject.SetActive(false);
                _ghost.sprite = null;
            }
            if (_ghostLabel != null)
                _ghostLabel.gameObject.SetActive(true);
            if (_preview != null)
                _preview.gameObject.SetActive(false);
        }

        private void EndDrag()
        {
            if (!_dragging)
                return;

            CancelDrag();
            Refresh();
        }

        private void UpdateDragVisuals(Vector2 local, bool inGrid, int targetX, int targetY, bool canDrop)
        {
            float width = (_dragRotated ? _dragType.Height : _dragType.Width) * cellSize;
            float height = (_dragRotated ? _dragType.Width : _dragType.Height) * cellSize;

            // Keep the cell the player grabbed under the cursor.
            SetTopLeft(_ghost.rectTransform,
                       local.x - (_grabX + 0.5f) * cellSize,
                       -local.y - (_grabY + 0.5f) * cellSize,
                       width, height);

            if (!inGrid)
            {
                _preview.gameObject.SetActive(false);
                return;
            }

            _preview.color = canDrop ? ValidDropColor : InvalidDropColor;
            SetTopLeft(_preview.rectTransform, targetX * cellSize, targetY * cellSize, width, height);
            _preview.gameObject.SetActive(true);
        }

        private void Refresh()
        {
            if (_itemsRoot == null || _inventory == null)
                return;

            ClearChildren(_itemsRoot);

            foreach (PlacedItem item in _inventory.Items)
                CreateItemView(item);
        }

        private void CreateItemView(PlacedItem item)
        {
            ItemVisual visual = VisualFor(item.Type);

            Color color = visual.Color;
            if (_dragging && item.InstanceId == _dragId)
                color.a = 0.25f;

            Image image = NewImage("Item_" + item.InstanceId, _itemsRoot,
                visual.Icon != null ? new Color(1f, 1f, 1f, color.a) : color);
            image.sprite = visual.Icon;
            image.preserveAspect = visual.Icon != null;
            SetTopLeft(image.rectTransform,
                       item.X * cellSize + ItemInset,
                       item.Y * cellSize + ItemInset,
                       item.FootprintWidth * cellSize - ItemInset * 2f,
                       item.FootprintHeight * cellSize - ItemInset * 2f);

            if (visual.Icon == null)
            {
                string label = item.Count > 1 ? visual.Label + "\nx" + item.Count : visual.Label;
                Text text = NewText("Label", image.transform, label, LabelFontSize, TextAnchor.MiddleCenter, InkColor);
                Stretch(text.rectTransform);
            }
            else if (item.Count > 1)
            {
                Text count = NewText("StackCount", image.transform, "x" + item.Count, 12,
                    TextAnchor.LowerRight, Color.white);
                RectTransform countRect = count.rectTransform;
                countRect.anchorMin = countRect.anchorMax = new Vector2(1f, 0f);
                countRect.pivot = new Vector2(1f, 0f);
                countRect.anchoredPosition = new Vector2(-3f, 3f);
                countRect.sizeDelta = new Vector2(30f, 18f);
                count.fontStyle = FontStyle.Bold;
            }
        }

        private ItemVisual VisualFor(ItemType type)
        {
            if (_visuals.TryGetValue(type.Id, out ItemVisual found))
                return found;

            return new ItemVisual { Label = "?", Color = FallbackColor };
        }

        private void BuildIfNeeded()
        {
            if (_built)
                return;

            _font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (_font == null)
                _font = Font.CreateDynamicFontFromOSFont("Arial", 16);

            var canvasObject = new GameObject("InventoryCanvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler));
            canvasObject.transform.SetParent(transform, false);

            var canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 100;

            var scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ConstantPixelSize;
            scaler.scaleFactor = 1f;

            Image panel = NewImage("Panel", canvasObject.transform, PanelColor);
            _root = panel.rectTransform;
            _root.anchorMin = new Vector2(0.5f, 0.5f);
            _root.anchorMax = new Vector2(0.5f, 0.5f);
            _root.pivot = new Vector2(0.5f, 0.5f);
            _root.anchoredPosition = Vector2.zero;

            _title = NewText("Title", _root, "INVENTORY   [Tab] close   [E] equip   [LMB] move   [RMB] inspect   [R] rotate", 16,
                             TextAnchor.MiddleLeft, TitleColor);

            _gridRect = NewRect("Grid", _root);
            _cellsRoot = NewRect("Cells", _gridRect);
            Stretch(_cellsRoot);
            _itemsRoot = NewRect("Items", _gridRect);
            Stretch(_itemsRoot);

            _preview = NewImage("DropPreview", _gridRect, ValidDropColor);
            _preview.gameObject.SetActive(false);

            _ghost = NewImage("DragGhost", _gridRect, Color.white);
            _ghostLabel = NewText("Label", _ghost.transform, string.Empty, LabelFontSize, TextAnchor.MiddleCenter, InkColor);
            Stretch(_ghostLabel.rectTransform);
            _ghost.gameObject.SetActive(false);

            _built = true;
        }

        private void BuildGrid()
        {
            ClearChildren(_cellsRoot);

            float gridWidth = _inventory.Width * cellSize;
            float gridHeight = _inventory.Height * cellSize;

            _root.sizeDelta = new Vector2(gridWidth + Padding * 2f, gridHeight + Padding * 2f + TitleHeight);
            SetTopLeft(_title.rectTransform, Padding, Padding * 0.5f, gridWidth, TitleHeight);
            SetTopLeft(_gridRect, Padding, Padding + TitleHeight, gridWidth, gridHeight);

            for (int y = 0; y < _inventory.Height; y++)
            {
                for (int x = 0; x < _inventory.Width; x++)
                {
                    Image cell = NewImage("Cell_" + x + "_" + y, _cellsRoot, CellColor);
                    SetTopLeft(cell.rectTransform, x * cellSize + 1f, y * cellSize + 1f, cellSize - 2f, cellSize - 2f);
                }
            }
        }

        private static void ClearChildren(RectTransform parent)
        {
            for (int i = parent.childCount - 1; i >= 0; i--)
            {
                GameObject child = parent.GetChild(i).gameObject;
                child.SetActive(false);
                Destroy(child);
            }
        }

        private static RectTransform NewRect(string objectName, Transform parent)
        {
            var go = new GameObject(objectName, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            return go.GetComponent<RectTransform>();
        }

        private static Image NewImage(string objectName, Transform parent, Color color)
        {
            var go = new GameObject(objectName, typeof(RectTransform), typeof(Image));
            go.transform.SetParent(parent, false);

            var image = go.GetComponent<Image>();
            image.color = color;
            image.raycastTarget = false;
            return image;
        }

        private Text NewText(string objectName, Transform parent, string content, int size, TextAnchor anchor, Color color)
        {
            var go = new GameObject(objectName, typeof(RectTransform), typeof(Text));
            go.transform.SetParent(parent, false);

            var text = go.GetComponent<Text>();
            text.font = _font;
            text.fontSize = size;
            text.alignment = anchor;
            text.color = color;
            text.text = content;
            text.raycastTarget = false;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Truncate;
            return text;
        }

        // yDown is measured downwards from the parent's top edge.
        private static void SetTopLeft(RectTransform rect, float x, float yDown, float width, float height)
        {
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(0f, 1f);
            rect.pivot = new Vector2(0f, 1f);
            rect.anchoredPosition = new Vector2(x, -yDown);
            rect.sizeDelta = new Vector2(width, height);
        }

        private static void Stretch(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }
    }
}
