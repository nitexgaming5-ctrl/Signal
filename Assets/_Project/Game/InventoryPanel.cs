using System;
using System.Collections.Generic;
using Signal.Core;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.EventSystems;
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
        private static readonly Color ActiveSlotColor = new Color32(0x3E, 0xE6, 0xD2, 0xFF);
        private static readonly Color HolsteredSlotColor = new Color32(0xC8, 0xA2, 0x4A, 0xFF);

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
        private PlayerQuickbar _quickbar;
        private readonly Image[] _hudQuickbarImages = new Image[PlayerQuickbar.SlotCount];
        private readonly Image[] _hudQuickbarSlots = new Image[PlayerQuickbar.SlotCount];
        private readonly Text[] _hudQuickbarLabels = new Text[PlayerQuickbar.SlotCount];
        private RectTransform _hudQuickbarRoot;
        private RectTransform _contextMenuRoot;
        private RectTransform _quickbarSubmenuRoot;
        private Guid _contextItemInstanceId;
        private bool _draggingFromQuickbar;
        private int _dragQuickbarSlot = -1;
        private Action<PlacedItem> _inspectItem;
        private Func<PlacedItem, bool> _equipItem;
        private Func<EquipmentSlot, bool> _unequipItem;
        private PlayerEquipmentLoadout _equipment;
        private Func<bool> _dropBackpack;
        private Func<bool> _hasBackpack;
        private Func<bool> _canAccessGrid;
        private PlayerPocketBook _pockets;
        private Func<Guid, int, bool> _moveItemToPocket;
        private Func<int, bool> _movePocketToBackpack;
        private Func<int, int, int, bool> _movePocketToBackpackAt;
        private Func<int, int, bool> _movePocketToPocket;
        private Func<GridInventory, GridInventory, Guid, int, int, int, bool, bool> _splitStack;
        private Func<GridInventory, GridInventory, Guid, Guid, int, bool> _mergeStack;
        private Func<int, bool> _equipPocketItem;
        private bool _draggingFromPocket;
        private int _dragPocketIndex = -1;
        private Guid _dragItemInstanceId;
        private readonly Button[] _pocketButtons = new Button[PlayerPocketBook.PocketCount];
        private readonly Text[] _pocketLabels = new Text[PlayerPocketBook.PocketCount];
        private readonly Image[] _pocketImages = new Image[PlayerPocketBook.PocketCount];
        private readonly List<Text> _equipmentLabels = new List<Text>();
        private Button _dropButton;
        private Text _emptyState;
        private Text _backpackTitle;
        private RectTransform _characterRoot;
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
            Func<PlacedItem, bool> equipItem = null, Func<EquipmentSlot, bool> unequipItem = null,
            PlayerEquipmentLoadout equipment = null, Func<bool> dropBackpack = null,
            Func<bool> hasBackpack = null, Func<bool> canAccessGrid = null,
            PlayerPocketBook pockets = null, Func<Guid, int, bool> moveItemToPocket = null,
            Func<int, bool> movePocketToBackpack = null, Func<int, bool> equipPocketItem = null,
            Func<int, int, int, bool> movePocketToBackpackAt = null,
            Func<int, int, bool> movePocketToPocket = null,
            Func<GridInventory, GridInventory, Guid, int, int, int, bool, bool> splitStack = null,
            Func<GridInventory, GridInventory, Guid, Guid, int, bool> mergeStack = null,
            PlayerQuickbar quickbar = null)
        {
            if (inventory == null)
                throw new ArgumentNullException(nameof(inventory));

            CancelDrag();

            if (_inventory != null)
                _inventory.Changed -= OnInventoryChanged;

            _inventory = inventory;
            _quickbar = quickbar;
            _inspectItem = inspectItem;
            _equipItem = equipItem;
            _unequipItem = unequipItem;
            _equipment = equipment;
            _dropBackpack = dropBackpack;
            _hasBackpack = hasBackpack;
            _canAccessGrid = canAccessGrid;
            _pockets = pockets;
            _moveItemToPocket = moveItemToPocket;
            _movePocketToBackpack = movePocketToBackpack;
            _movePocketToBackpackAt = movePocketToBackpackAt;
            _movePocketToPocket = movePocketToPocket;
            _splitStack = splitStack;
            _mergeStack = mergeStack;
            _equipPocketItem = equipPocketItem;
            _inventory.Changed += OnInventoryChanged;
            if (_pockets != null)
                for (int i = 0; i < PlayerPocketBook.PocketCount; i++)
                    _pockets.GetPocket(i).Changed += OnPocketChanged;

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
            {
                CancelDrag();
                CloseContextMenu();
            }

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
            if (_pockets != null)
                for (int i = 0; i < PlayerPocketBook.PocketCount; i++)
                    _pockets.GetPocket(i).Changed -= OnPocketChanged;
        }

        private void OnPocketChanged(GridInventory inventory)
        {
            if (_open) Refresh();
            else RefreshQuickbarAfterStorageChange();
        }

        private void OnInventoryChanged(GridInventory inventory)
        {
            if (_open) Refresh();
            else RefreshQuickbarAfterStorageChange();
        }

        private void RefreshQuickbarAfterStorageChange()
        {
            _quickbar?.Reconcile(IsQuickbarItemPresent);
            RefreshQuickbarViews();
        }

        private void Update()
        {
            if (_inventory == null || _root == null)
                return;

            Keyboard keyboard = Keyboard.current;
            Mouse mouse = Mouse.current;

            if (keyboard != null && keyboard.tabKey.wasPressedThisFrame)
                SetOpen(!_open);

            if (!_open)
            {
                RefreshQuickbarAfterStorageChange();
                return;
            }
            if (mouse == null)
                return;

            UpdatePointer(keyboard, mouse);
            if (_open) { RefreshQuickbarViews(); RefreshCharacterPanel(); }
        }

        private void UpdatePointer(Keyboard keyboard, Mouse mouse)
        {
            Vector2 screen = mouse.position.ReadValue();
            if (_contextMenuRoot != null && _contextMenuRoot.gameObject.activeSelf)
            {
                if ((keyboard != null && keyboard.escapeKey.wasPressedThisFrame) ||
                    (mouse.leftButton.wasPressedThisFrame && !RectTransformUtility.RectangleContainsScreenPoint(_contextMenuRoot, screen, null) &&
                     (_quickbarSubmenuRoot == null || !RectTransformUtility.RectangleContainsScreenPoint(_quickbarSubmenuRoot, screen, null))))
                { CloseContextMenu(); return; }
                // While a context menu is open, block the underlying inventory pointer loop.
                // Unity's EventSystem still receives the click and dispatches the menu button.
                return;
            }
            bool backpackAccessible = _canAccessGrid == null || _canAccessGrid();
            int hoverX = 0;
            int hoverY = 0;
            Vector2 local = Vector2.zero;
            bool pointerOverGrid = TryGetHoverCell(screen, out hoverX, out hoverY, out local);
            bool inGrid = backpackAccessible && pointerOverGrid;

            if (!_dragging && keyboard != null)
            {
                EquipmentSlot? unequipSlot = null;
                if (keyboard.digit1Key.wasPressedThisFrame) unequipSlot = EquipmentSlot.Head;
                else if (keyboard.digit2Key.wasPressedThisFrame) unequipSlot = EquipmentSlot.Torso;
                else if (keyboard.digit3Key.wasPressedThisFrame) unequipSlot = EquipmentSlot.Legs;
                else if (keyboard.digit4Key.wasPressedThisFrame) unequipSlot = EquipmentSlot.Feet;
                else if (keyboard.digit5Key.wasPressedThisFrame) unequipSlot = EquipmentSlot.Accessory1;
                else if (keyboard.digit6Key.wasPressedThisFrame) unequipSlot = EquipmentSlot.Accessory2;
                else if (keyboard.digit7Key.wasPressedThisFrame) unequipSlot = EquipmentSlot.Accessory3;

                if (unequipSlot.HasValue)
                {
                    _unequipItem?.Invoke(unequipSlot.Value);
                    Refresh();
                    return;
                }
            }

            // Equipment can be equipped directly from either the accessible backpack grid
            // or a personal pocket. Pockets remain usable without an equipped backpack.
            if (!_dragging && keyboard != null && keyboard.eKey.wasPressedThisFrame)
            {
                if (backpackAccessible && inGrid &&
                    _inventory.TryGetItemAt(hoverX, hoverY, out PlacedItem equipTarget))
                {
                    if (_equipItem != null && _equipItem(equipTarget))
                        Refresh();
                    return;
                }

                if (TryGetPocketIndex(screen, out int equipPocketIndex) &&
                    _pockets != null && _pockets.GetPocket(equipPocketIndex).Items.Count > 0)
                {
                    _equipPocketItem?.Invoke(equipPocketIndex);
                    Refresh();
                    return;
                }
            }

            if (!_dragging && TryGetPocketIndex(screen, out int sourcePocketIndex) &&
                _pockets != null && _pockets.GetPocket(sourcePocketIndex).Items.Count > 0)
            {
                PlacedItem pocketItem = _pockets.GetPocket(sourcePocketIndex).Items[0];
                if (mouse.rightButton.wasPressedThisFrame)
                {
                    if (IsControlDown(keyboard))
                        SplitPocketStack(sourcePocketIndex, pocketItem, 1);
                    else if (IsShiftDown(keyboard))
                        SplitPocketStack(sourcePocketIndex, pocketItem, Mathf.Max(1, pocketItem.Count / 2));
                    else
                        OpenContextMenu(pocketItem, screen);
                    return;
                }

                if (mouse.leftButton.wasPressedThisFrame)
                {
                    if (IsShiftDown(keyboard))
                        QuickTransferPocketStack(sourcePocketIndex, pocketItem);
                    else
                        BeginPocketDrag(sourcePocketIndex);
                    return;
                }
            }

            if (!_dragging && mouse.leftButton.wasPressedThisFrame && TryGetQuickbarSlot(screen, out int quickbarSourceSlot))
            { BeginQuickbarDrag(quickbarSourceSlot); return; }

            if (!backpackAccessible && !_dragging)
                return;


            if (!_dragging)
            {
                if (mouse.rightButton.wasPressedThisFrame && inGrid &&
                    _inventory.TryGetItemAt(hoverX, hoverY, out PlacedItem inspected))
                {
                    if (IsControlDown(keyboard))
                        SplitBackpackStack(inspected, 1);
                    else if (IsShiftDown(keyboard))
                        SplitBackpackStack(inspected, Mathf.Max(1, inspected.Count / 2));
                    else
                        OpenContextMenu(inspected, screen);
                    return;
                }

                if (!mouse.leftButton.wasPressedThisFrame || !inGrid)
                    return;
                if (!_inventory.TryGetItemAt(hoverX, hoverY, out PlacedItem hit))
                    return;

                if (IsShiftDown(keyboard))
                {
                    QuickTransferBackpackStack(hit);
                    return;
                }

                BeginDrag(hit, hoverX, hoverY);
            }

            if (_draggingFromQuickbar)
            {
                UpdateDragGhost(screen, 48f, 48f);
                if (keyboard != null && keyboard.escapeKey.wasPressedThisFrame) { EndDrag(); return; }
                if (mouse.leftButton.wasReleasedThisFrame)
                {
                    int targetSlot = -1;
                    bool overQuickbar = TryGetQuickbarSlot(screen, out targetSlot);
                    bool overInventory = (TryGetHoverCell(screen, out _, out _, out _) && backpackAccessible) || TryGetPocketIndex(screen, out _);
                    int sourceSlot = _dragQuickbarSlot;
                    CancelDrag();
                    if (overQuickbar) _quickbar?.TrySwap(sourceSlot, targetSlot);
                    else if (overInventory) _quickbar?.TryUnassign(sourceSlot);
                    Refresh();
                }
                return;
            }

            if (keyboard != null && keyboard.escapeKey.wasPressedThisFrame)
            {
                EndDrag();
                return;
            }

            if (keyboard != null && keyboard.rKey.wasPressedThisFrame && !_draggingFromQuickbar)
                RotateDrag();

            int targetX = hoverX - _grabX;
            int targetY = hoverY - _grabY;
            bool canDrop = inGrid && (_draggingFromPocket
                ? _inventory.CanPlace(_dragType, targetX, targetY, _dragRotated)
                : _inventory.CanMove(_dragId, targetX, targetY, _dragRotated));

            GridInventory mergeSource = _draggingFromPocket ? _pockets?.GetPocket(_dragPocketIndex) : _inventory;
            GridInventory mergeDestination = null;
            Guid mergeTargetId = Guid.Empty;
            int targetPocket = -1;
            if (TryGetMergeTarget(screen, inGrid, hoverX, hoverY, mergeSource,
                out mergeDestination, out mergeTargetId, out targetPocket))
                canDrop = true;

            UpdateDragVisuals(local, screen, inGrid, targetX, targetY, canDrop);

            if (mouse.leftButton.wasReleasedThisFrame)
            {
                int id = _dragId;
                Guid itemInstanceId = _dragItemInstanceId;
                bool rotated = _dragRotated;
                bool fromPocket = _draggingFromPocket;
                int sourcePocket = _dragPocketIndex;
                int requestedMergeQuantity = _dragType.MaxStack;
                int releasePocket = -1;
                bool dropIntoQuickbar = TryGetQuickbarSlot(screen, out int releaseQuickbarSlot);
                bool dropIntoPocket = !canDrop && !rotated &&
                    TryGetPocketIndex(screen, out releasePocket) && (!fromPocket || releasePocket != sourcePocket);

                EndDrag();

                if (dropIntoQuickbar && itemInstanceId != Guid.Empty &&
                    TryFindQuickbarItem(itemInstanceId, out _, out bool quickbarItemAccessible) && quickbarItemAccessible)
                    _quickbar?.TryReplace(releaseQuickbarSlot, itemInstanceId);
                else if (mergeTargetId != Guid.Empty && mergeSource != null && mergeDestination != null)
                {
                    _mergeStack?.Invoke(mergeSource, mergeDestination, itemInstanceId,
                        mergeTargetId, requestedMergeQuantity);
                }
                else if (canDrop)
                {
                    if (fromPocket)
                        _movePocketToBackpackAt?.Invoke(sourcePocket, targetX, targetY);
                    else
                        _inventory.TryMove(id, targetX, targetY, rotated);
                }
                else if (dropIntoPocket)
                {
                    if (fromPocket)
                        _movePocketToPocket?.Invoke(sourcePocket, releasePocket);
                    else if (itemInstanceId != Guid.Empty)
                        _moveItemToPocket?.Invoke(itemInstanceId, releasePocket);
                }

                Refresh();
            }
        }


        private bool TryGetMergeTarget(Vector2 screen, bool inGrid, int hoverX, int hoverY,
            GridInventory source, out GridInventory destination, out Guid destinationStackId,
            out int targetPocket)
        {
            destination = null;
            destinationStackId = Guid.Empty;
            targetPocket = -1;
            if (_mergeStack == null || source == null || _dragItemInstanceId == Guid.Empty)
                return false;

            if (inGrid && _inventory.TryGetItemAt(hoverX, hoverY, out PlacedItem gridTarget) &&
                gridTarget.ItemInstanceId != _dragItemInstanceId &&
                source.TryGetItemInstance(_dragItemInstanceId, out PlacedItem dragged) &&
                StackOperations.CanMerge(dragged, gridTarget))
            {
                destination = _inventory;
                destinationStackId = gridTarget.ItemInstanceId;
                return true;
            }

            if (TryGetPocketIndex(screen, out targetPocket) && _pockets != null &&
                targetPocket >= 0 && targetPocket < PlayerPocketBook.PocketCount)
            {
                GridInventory pocket = _pockets.GetPocket(targetPocket);
                if (!ReferenceEquals(source, pocket) && pocket.TryGetItemAt(0, 0, out PlacedItem pocketTarget) &&
                    pocketTarget.ItemInstanceId != _dragItemInstanceId &&
                    source.TryGetItemInstance(_dragItemInstanceId, out PlacedItem pocketDragged) &&
                    StackOperations.CanMerge(pocketDragged, pocketTarget))
                {
                    destination = pocket;
                    destinationStackId = pocketTarget.ItemInstanceId;
                    return true;
                }
            }

            return false;
        }

        private static bool IsShiftDown(Keyboard keyboard) =>
            keyboard != null && (keyboard.leftShiftKey.isPressed || keyboard.rightShiftKey.isPressed);

        private static bool IsControlDown(Keyboard keyboard) =>
            keyboard != null && (keyboard.leftCtrlKey.isPressed || keyboard.rightCtrlKey.isPressed);

        private void QuickTransferBackpackStack(PlacedItem item)
        {
            if (_pockets == null || item.Rotated || item.Type.Width != 1 || item.Type.Height != 1)
                return;
            if (_pockets.TryFindAvailablePocket(item.Type, out int pocketIndex, out _, false))
                _moveItemToPocket?.Invoke(item.ItemInstanceId, pocketIndex);
            Refresh();
        }

        private void QuickTransferPocketStack(int sourcePocket, PlacedItem item)
        {
            if ((_canAccessGrid == null || _canAccessGrid()) &&
                TryFindEmptyCell(_inventory, item.Type, false, out _, out _))
            {
                if (_movePocketToBackpack?.Invoke(sourcePocket) == true)
                {
                    Refresh();
                    return;
                }
            }

            if (_pockets != null && _pockets.TryFindAvailablePocket(item.Type, out int targetPocket, out _, false) &&
                targetPocket != sourcePocket)
                _movePocketToPocket?.Invoke(sourcePocket, targetPocket);
            Refresh();
        }

        private void SplitBackpackStack(PlacedItem item, int quantity)
        {
            if (_splitStack == null || item.Count <= 1)
                return;
            if (TryFindEmptyCell(_inventory, item.Type, item.Rotated, out int x, out int y))
            {
                _splitStack(_inventory, _inventory, item.ItemInstanceId, quantity, x, y, item.Rotated);
                Refresh();
                return;
            }
            if (_pockets != null && item.Type.Width == 1 && item.Type.Height == 1 &&
                !item.Rotated && _pockets.TryFindAvailablePocket(item.Type, out int pocketIndex, out GridInventory pocket, false))
            {
                _splitStack(_inventory, pocket, item.ItemInstanceId, quantity, 0, 0, false);
                Refresh();
            }
        }

        private void SplitPocketStack(int sourcePocket, PlacedItem item, int quantity)
        {
            if (_splitStack == null || item.Count <= 1 || _pockets == null)
                return;
            GridInventory source = _pockets.GetPocket(sourcePocket);
            if (_canAccessGrid == null || _canAccessGrid())
            {
                if (TryFindEmptyCell(_inventory, item.Type, false, out int x, out int y))
                {
                    _splitStack(source, _inventory, item.ItemInstanceId, quantity, x, y, false);
                    Refresh();
                    return;
                }
            }
            if (_pockets.TryFindAvailablePocket(item.Type, out int targetPocket, out GridInventory destination, false) &&
                targetPocket != sourcePocket)
            {
                _splitStack(source, destination, item.ItemInstanceId, quantity, 0, 0, false);
                Refresh();
            }
        }

        private static bool TryFindEmptyCell(GridInventory grid, ItemType type, bool rotated, out int x, out int y)
        {
            x = y = 0;
            if (grid == null || !type.IsStackable)
                return false;
            int footprintWidth = rotated ? type.Height : type.Width;
            int footprintHeight = rotated ? type.Width : type.Height;
            for (int row = 0; row <= grid.Height - footprintHeight; row++)
                for (int column = 0; column <= grid.Width - footprintWidth; column++)
                    if (grid.CanPlace(type, column, row, rotated))
                    {
                        x = column;
                        y = row;
                        return true;
                    }
            return false;
        }

        private bool TryGetPocketIndex(Vector2 screen, out int pocketIndex)
        {
            pocketIndex = -1;
            for (int i = 0; i < _pocketButtons.Length; i++)
            {
                Button button = _pocketButtons[i];
                if (button == null || !button.gameObject.activeInHierarchy)
                    continue;
                if (RectTransformUtility.RectangleContainsScreenPoint(button.GetComponent<RectTransform>(), screen, null))
                {
                    pocketIndex = i;
                    return true;
                }
            }
            return false;
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

        private void BeginPocketDrag(int pocketIndex)
        {
            if (_pockets == null || !_pockets.TryGetPocket(pocketIndex, out GridInventory pocket) ||
                pocket.Items.Count != 1)
                return;

            PlacedItem item = pocket.Items[0];
            _dragging = true;
            _draggingFromPocket = true;
            _dragPocketIndex = pocketIndex;
            _dragId = 0;
            _dragItemInstanceId = item.ItemInstanceId;
            _dragType = item.Type;
            _dragRotated = false;
            _grabX = 0;
            _grabY = 0;

            ItemVisual visual = VisualFor(_dragType);
            _ghost.sprite = visual.Icon;
            _ghost.preserveAspect = visual.Icon != null;
            _ghost.color = visual.Icon != null ? new Color(1f, 1f, 1f, 0.85f) : visual.Color;
            _ghostLabel.text = visual.Label;
            _ghostLabel.gameObject.SetActive(visual.Icon == null);
            _ghost.gameObject.SetActive(true);
            Refresh();
        }

        private void BeginDrag(PlacedItem item, int cellX, int cellY)
        {
            _dragging = true;
            _dragId = item.InstanceId;
            _dragItemInstanceId = item.ItemInstanceId;
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
            _draggingFromQuickbar = false;
            _dragQuickbarSlot = -1;
            _draggingFromPocket = false;
            _dragPocketIndex = -1;

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

        private void UpdateDragVisuals(Vector2 local, Vector2 screen, bool inGrid, int targetX, int targetY, bool canDrop)
        {
            float width = (_dragRotated ? _dragType.Height : _dragType.Width) * cellSize;
            float height = (_dragRotated ? _dragType.Width : _dragType.Height) * cellSize;
            UpdateDragGhost(screen, width, height);

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

            bool canAccess = _canAccessGrid == null || _canAccessGrid();
            if (_gridRect != null) _gridRect.gameObject.SetActive(canAccess);
            if (_emptyState != null) _emptyState.gameObject.SetActive(!canAccess);
            if (canAccess)
                foreach (PlacedItem item in _inventory.Items)
                    CreateItemView(item);
            _quickbar?.Reconcile(IsQuickbarItemPresent);
            RefreshPocketViews();
            RefreshQuickbarViews();
            RefreshCharacterPanel();
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

            var canvasObject = new GameObject("InventoryCanvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvasObject.transform.SetParent(transform, false);

            var canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 100;

            if (FindAnyObjectByType<EventSystem>() == null)
            {
                GameObject eventSystemObject = new GameObject("InventoryEventSystem",
                    typeof(EventSystem), typeof(InputSystemUIInputModule));
                eventSystemObject.transform.SetParent(canvasObject.transform, false);
                eventSystemObject.GetComponent<InputSystemUIInputModule>().AssignDefaultActions();
            }

            var scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ConstantPixelSize;
            scaler.scaleFactor = 1f;

            Image panel = NewImage("Panel", canvasObject.transform, PanelColor);
            _root = panel.rectTransform;
            _root.anchorMin = new Vector2(0.5f, 0.5f);
            _root.anchorMax = new Vector2(0.5f, 0.5f);
            _root.pivot = new Vector2(0.5f, 0.5f);
            _root.anchoredPosition = Vector2.zero;

            _title = NewText("Title", _root, "INVENTORY   Drag onto stack to restack   Drag to chosen pocket   SHIFT+LMB quick move   SHIFT+RMB split half   CTRL+RMB split 1   R rotate   ESC cancel   E equip   1-7 unequip   B drop", 13,
                             TextAnchor.MiddleLeft, TitleColor);

            _characterRoot = NewRect("CharacterPanel", _root);
            _gridRect = NewRect("Grid", _root);
            _backpackTitle = NewText("BackpackStorageTitle", _root, "BACKPACK STORAGE", 14, TextAnchor.MiddleLeft, TitleColor);
            _emptyState = NewText("NoBackpack", _root, "BACKPACK STORAGE IS UNAVAILABLE\nEquip or recover a backpack to access its contents.", 18, TextAnchor.MiddleCenter, TitleColor);
            _cellsRoot = NewRect("Cells", _gridRect);
            Stretch(_cellsRoot);
            _itemsRoot = NewRect("Items", _gridRect);
            Stretch(_itemsRoot);

            _preview = NewImage("DropPreview", _gridRect, ValidDropColor);
            _preview.gameObject.SetActive(false);

            _ghost = NewImage("DragGhost", canvasObject.transform, Color.white);
            _ghostLabel = NewText("Label", _ghost.transform, string.Empty, LabelFontSize, TextAnchor.MiddleCenter, InkColor);
            Stretch(_ghostLabel.rectTransform);
            _ghost.gameObject.SetActive(false);

            BuildCharacterPanel();
            BuildHudQuickbar(canvasObject.transform);
            _built = true;
        }

        private void BuildGrid()
        {
            ClearChildren(_cellsRoot);

            float gridWidth = _inventory.Width * cellSize;
            float gridHeight = _inventory.Height * cellSize;

            const float characterWidth = 220f;
            float contentWidth = characterWidth + 24f + gridWidth;
            _root.sizeDelta = new Vector2(contentWidth + Padding * 2f, Mathf.Max(gridHeight, 430f) + Padding * 2f + TitleHeight + 92f);
            SetTopLeft(_title.rectTransform, Padding, Padding * 0.5f, contentWidth, TitleHeight);
            SetTopLeft(_characterRoot, Padding, Padding + TitleHeight, characterWidth, Mathf.Max(gridHeight, 430f));
            SetTopLeft(_backpackTitle.rectTransform, Padding + characterWidth + 24f, Padding + TitleHeight, gridWidth, 22f);
            SetTopLeft(_gridRect, Padding + characterWidth + 24f, Padding + TitleHeight + 24f, gridWidth, gridHeight);
            SetTopLeft(_emptyState.rectTransform, Padding + characterWidth + 24f, Padding + TitleHeight + 24f, gridWidth, gridHeight);

            for (int y = 0; y < _inventory.Height; y++)
            {
                for (int x = 0; x < _inventory.Width; x++)
                {
                    Image cell = NewImage("Cell_" + x + "_" + y, _cellsRoot, CellColor);
                    SetTopLeft(cell.rectTransform, x * cellSize + 1f, y * cellSize + 1f, cellSize - 2f, cellSize - 2f);
                }
            }
        }

        private void BuildCharacterPanel()
        {
            Image background = NewImage("CharacterBackground", _characterRoot, new Color32(0x18, 0x1D, 0x25, 0xFF));
            Stretch(background.rectTransform);
            Text characterTitle = NewText("CharacterTitle", _characterRoot, "CHARACTER", 16, TextAnchor.MiddleLeft, TitleColor);
            SetTopLeft(characterTitle.rectTransform, 10f, 8f, 190f, 24f);
            Text equipmentTitle = NewText("EquipmentTitle", _characterRoot, "EQUIPMENT", 12, TextAnchor.MiddleLeft, FallbackColor);
            SetTopLeft(equipmentTitle.rectTransform, 10f, 38f, 190f, 20f);
            EquipmentSlot[] slots = { EquipmentSlot.Head, EquipmentSlot.Torso, EquipmentSlot.Legs,
                EquipmentSlot.Feet, EquipmentSlot.Accessory1, EquipmentSlot.Accessory2, EquipmentSlot.Accessory3 };
            for (int i = 0; i < slots.Length; i++)
            {
                Text label = NewText("Slot_" + slots[i], _characterRoot, slots[i] + ": —", 13,
                    TextAnchor.MiddleLeft, TitleColor);
                SetTopLeft(label.rectTransform, 12f, 62f + i * 25f, 196f, 22f);
                _equipmentLabels.Add(label);
            }

            Text pocketsTitle = NewText("PocketsTitle", _characterRoot, "PERSONAL POCKETS", 12,
                TextAnchor.MiddleLeft, FallbackColor);
            SetTopLeft(pocketsTitle.rectTransform, 10f, 252f, 196f, 20f);
            for (int i = 0; i < PlayerPocketBook.PocketCount; i++)
            {
                Color pocketColor = i == PlayerPocketBook.HiddenPocketIndex
                    ? new Color32(0x7A, 0x32, 0x3A, 0xFF)
                    : CellColor;
                Image pocket = NewImage("Pocket_" + (i + 1), _characterRoot, pocketColor);
                pocket.raycastTarget = true;
                Button pocketButton = pocket.gameObject.AddComponent<Button>();
                pocketButton.targetGraphic = pocket;
                int pocketIndex = i;
                pocketButton.onClick.AddListener(() =>
                {
                    if (_pockets != null && _pockets.GetPocket(pocketIndex).Items.Count > 0)
                        _inspectItem?.Invoke(_pockets.GetPocket(pocketIndex).Items[0]);
                });
                _pocketButtons[i] = pocketButton;
                SetTopLeft(pocket.rectTransform, 10f + i * 66f, 278f, 60f, 54f);
                Image pocketItemImage = NewImage("PocketItemImage", pocket.transform, Color.white);
                pocketItemImage.preserveAspect = true;
                pocketItemImage.raycastTarget = false;
                SetTopLeft(pocketItemImage.rectTransform, 12f, 13f, 36f, 36f);
                _pocketImages[i] = pocketItemImage;
                string pocketName = i == PlayerPocketBook.HiddenPocketIndex ? "P3 · HIDDEN" : "P" + (i + 1);
                Text pocketLabel = NewText("PocketLabel", pocket.transform, pocketName, 10,
                    TextAnchor.UpperCenter, TitleColor);
                SetTopLeft(pocketLabel.rectTransform, 1f, 1f, 58f, 14f);
                _pocketLabels[i] = pocketLabel;
            }

            GameObject buttonObject = new GameObject("DropBackpackButton", typeof(RectTransform), typeof(Image), typeof(Button));
            buttonObject.transform.SetParent(_characterRoot, false);
            Image buttonImage = buttonObject.GetComponent<Image>();
            buttonImage.color = new Color32(0x8B, 0x4A, 0x2B, 0xFF);
            _dropButton = buttonObject.GetComponent<Button>();
            _dropButton.targetGraphic = buttonImage;
            _dropButton.onClick.AddListener(() => { _dropBackpack?.Invoke(); Refresh(); });
            SetTopLeft(buttonObject.GetComponent<RectTransform>(), 10f, 356f, 196f, 42f);
            Text buttonLabel = NewText("ButtonLabel", buttonObject.transform, "DROP BACKPACK  [B]", 13,
                TextAnchor.MiddleCenter, Color.white);
            Stretch(buttonLabel.rectTransform);
        }

        private void RefreshCharacterPanel()
        {
            if (_equipment == null || _equipmentLabels.Count == 0) return;
            EquipmentSlot[] slots = { EquipmentSlot.Head, EquipmentSlot.Torso, EquipmentSlot.Legs,
                EquipmentSlot.Feet, EquipmentSlot.Accessory1, EquipmentSlot.Accessory2, EquipmentSlot.Accessory3 };
            for (int i = 0; i < slots.Length; i++)
            {
                string value = "—";
                if (_equipment.TryGet(slots[i], out EquippedItem equipped))
                {
                    ItemVisual visual = VisualFor(equipped.Item.Type);
                    value = string.IsNullOrWhiteSpace(visual.Label) ? "Equipped" : visual.Label;
                }
                _equipmentLabels[i].text = slots[i] + ": " + value;
            }
            bool hasBackpack = _hasBackpack != null
                ? _hasBackpack()
                : _equipment.TryGet(EquipmentSlot.Accessory1, out _);
            if (_dropButton != null) _dropButton.interactable = hasBackpack;
        }

        private bool IsQuickbarItemPresent(Guid id)
        {
            if (id == Guid.Empty) return false;
            if (_inventory != null && _inventory.TryGetItemInstance(id, out _)) return true;
            if (_pockets != null)
                for (int i = 0; i < PlayerPocketBook.PocketCount; i++)
                    if (_pockets.GetPocket(i).TryGetItemInstance(id, out _)) return true;
            return false;
        }

        private bool TryFindQuickbarItem(Guid id, out PlacedItem item, out bool accessible)
        {
            accessible = false;
            if (_inventory != null && _inventory.TryGetItemInstance(id, out item))
            { accessible = _canAccessGrid == null || _canAccessGrid(); return true; }
            if (_pockets != null)
                for (int i = 0; i < PlayerPocketBook.PocketCount; i++)
                    if (_pockets.GetPocket(i).TryGetItemInstance(id, out item)) { accessible = true; return true; }
            item = default;
            return false;
        }

        private void RefreshQuickbarViews()
        {
            if (_quickbar == null) return;
            for (int i = 0; i < PlayerQuickbar.SlotCount; i++)
            {
                Guid id = _quickbar.GetItem(i);
                Color slotColor = _quickbar.SelectedSlot == i
                    ? (_quickbar.IsHolstered ? HolsteredSlotColor : ActiveSlotColor)
                    : CellColor;
                if (_hudQuickbarSlots[i] != null) _hudQuickbarSlots[i].color = slotColor;

                if (id == Guid.Empty || !TryFindQuickbarItem(id, out PlacedItem item, out bool accessible))
                {
                    _hudQuickbarImages[i].sprite = null;
                    _hudQuickbarImages[i].gameObject.SetActive(false);
                    _hudQuickbarLabels[i].text = (i + 1).ToString();
                    continue;
                }

                ItemVisual visual = VisualFor(item.Type);
                _hudQuickbarImages[i].sprite = visual.Icon;
                _hudQuickbarImages[i].color = accessible
                    ? Color.white
                    : new Color(0.35f, 0.35f, 0.35f, 0.75f);
                _hudQuickbarImages[i].gameObject.SetActive(visual.Icon != null);
                _hudQuickbarLabels[i].text = visual.Icon == null ? visual.Label : (i + 1).ToString();
                if (item.Count > 1) _hudQuickbarLabels[i].text += " x" + item.Count;
            }
        }

        private void RefreshPocketViews()
        {
            for (int i = 0; i < PlayerPocketBook.PocketCount; i++)
            {
                if (_pocketLabels[i] == null) continue;
                if (_pockets != null && _pockets.GetPocket(i).Items.Count > 0)
                {
                    PlacedItem item = _pockets.GetPocket(i).Items[0];
                    ItemVisual visual = VisualFor(item.Type);
                    string pocketName = i == PlayerPocketBook.HiddenPocketIndex
                        ? "P3 · HIDDEN" : "P" + (i + 1);
                    string countSuffix = item.Count > 1 ? " · x" + item.Count : string.Empty;
                    _pocketLabels[i].text = visual.Icon != null
                        ? pocketName + countSuffix
                        : pocketName + "\n" + visual.Label + countSuffix;
                    _pocketLabels[i].color = TitleColor;
                    _pocketImages[i].sprite = visual.Icon;
                    _pocketImages[i].color = Color.white;
                    _pocketImages[i].gameObject.SetActive(visual.Icon != null);
                    _pocketButtons[i].interactable = true;
                }
                else
                {
                    _pocketLabels[i].text = i == PlayerPocketBook.HiddenPocketIndex ? "P3 · HIDDEN" : "P" + (i + 1);
                    _pocketLabels[i].color = FallbackColor;
                    _pocketImages[i].sprite = null;
                    _pocketImages[i].gameObject.SetActive(false);
                    _pocketButtons[i].interactable = false;
                }
            }
        }

        private void BuildHudQuickbar(Transform canvasRoot)
        {
            Image background = NewImage("HudQuickbarBackground", canvasRoot, new Color32(0x12, 0x16, 0x1D, 0xE8));
            _hudQuickbarRoot = background.rectTransform;
            _hudQuickbarRoot.anchorMin = _hudQuickbarRoot.anchorMax = new Vector2(0.5f, 0f);
            _hudQuickbarRoot.pivot = new Vector2(0.5f, 0f);
            _hudQuickbarRoot.anchoredPosition = new Vector2(0f, 22f);
            _hudQuickbarRoot.sizeDelta = new Vector2(6f * 58f + 12f, 68f);
            for (int i = 0; i < PlayerQuickbar.SlotCount; i++)
            {
                Image slot = NewImage("HudQuickSlot_" + (i + 1), _hudQuickbarRoot, CellColor);
                slot.raycastTarget = true;
                _hudQuickbarSlots[i] = slot;
                RectTransform slotRect = slot.rectTransform;
                slotRect.anchorMin = slotRect.anchorMax = new Vector2(0f, 0f);
                slotRect.pivot = new Vector2(0f, 0f);
                slotRect.anchoredPosition = new Vector2(6f + i * 58f, 7f);
                slotRect.sizeDelta = new Vector2(54f, 54f);
                _hudQuickbarImages[i] = NewImage("HudQuickSlotIcon", slot.transform, Color.white);
                _hudQuickbarImages[i].preserveAspect = true;
                _hudQuickbarImages[i].raycastTarget = false;
                SetTopLeft(_hudQuickbarImages[i].rectTransform, 6f, 5f, 40f, 40f);
                _hudQuickbarLabels[i] = NewText("HudQuickSlotLabel", slot.transform,
                    (i + 1).ToString(), 10, TextAnchor.LowerRight, TitleColor);
                SetTopLeft(_hudQuickbarLabels[i].rectTransform, 2f, 2f, 50f, 18f);
            }
        }

        private bool TryGetQuickbarSlot(Vector2 screen, out int slotIndex)
        {
            slotIndex = -1;
            for (int i = 0; i < _hudQuickbarSlots.Length; i++)
            {
                Image slot = _hudQuickbarSlots[i];
                if (slot == null || !slot.gameObject.activeInHierarchy) continue;
                if (RectTransformUtility.RectangleContainsScreenPoint(slot.rectTransform, screen, null))
                { slotIndex = i; return true; }
            }
            return false;
        }

        private void BeginQuickbarDrag(int slotIndex)
        {
            if (_quickbar == null || slotIndex < 0 || slotIndex >= PlayerQuickbar.SlotCount ||
                !TryFindQuickbarItem(_quickbar.GetItem(slotIndex), out PlacedItem item, out bool accessible) || !accessible) return;
            _dragging = true;
            _draggingFromQuickbar = true;
            _dragQuickbarSlot = slotIndex;
            _dragItemInstanceId = item.ItemInstanceId;
            _dragType = item.Type;
            ItemVisual visual = VisualFor(item.Type);
            _ghost.sprite = visual.Icon;
            _ghost.preserveAspect = visual.Icon != null;
            _ghost.color = visual.Icon != null ? new Color(1f, 1f, 1f, 0.82f) : new Color(visual.Color.r, visual.Color.g, visual.Color.b, 0.82f);
            _ghostLabel.text = visual.Label;
            _ghostLabel.gameObject.SetActive(visual.Icon == null);
            _ghost.gameObject.SetActive(true);
            Refresh();
        }

        private void UpdateDragGhost(Vector2 screen, float width, float height)
        {
            if (_ghost == null || _ghost.transform.parent is not RectTransform canvasRect ||
                !RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, screen, null, out Vector2 local))
                return;
            RectTransform ghostRect = _ghost.rectTransform;
            ghostRect.anchorMin = ghostRect.anchorMax = new Vector2(0.5f, 0.5f);
            ghostRect.pivot = new Vector2(0.5f, 0.5f);
            ghostRect.anchoredPosition = local;
            ghostRect.sizeDelta = new Vector2(width, height);
        }

        private void OpenContextMenu(PlacedItem item, Vector2 screen)
        {
            CloseContextMenu();
            if (item.ItemInstanceId == Guid.Empty || _quickbar == null) return;
            _contextItemInstanceId = item.ItemInstanceId;
            _contextMenuRoot = NewRect("ItemContextMenu", _root);
            Image background = NewImage("ContextBackground", _contextMenuRoot, new Color32(0x12, 0x16, 0x1D, 0xFF));
            background.raycastTarget = true;
            Stretch(background.rectTransform);
            _contextMenuRoot.anchorMin = _contextMenuRoot.anchorMax = new Vector2(0.5f, 0.5f);
            _contextMenuRoot.pivot = new Vector2(0f, 1f);
            RectTransformUtility.ScreenPointToLocalPointInRectangle(_root, screen, null, out Vector2 local);
            _contextMenuRoot.anchoredPosition = local;
            _contextMenuRoot.sizeDelta = new Vector2(190f, 72f);
            AddContextButton("Inspect", 0, () =>
            { if (TryFindQuickbarItem(_contextItemInstanceId, out PlacedItem found, out bool accessible) && accessible) _inspectItem?.Invoke(found); CloseContextMenu(); });
            AddContextButton("Equip to Quick Slot  ▶", 34, ShowQuickbarSubmenu);
        }

        private void AddContextButton(string label, float y, Action action)
        {
            Image image = NewImage("ContextOption", _contextMenuRoot, CellColor);
            image.raycastTarget = true;
            Button button = image.gameObject.AddComponent<Button>();
            button.targetGraphic = image;
            SetTopLeft(image.rectTransform, 4f, 4f + y, 182f, 30f);
            Text text = NewText("ContextOptionLabel", image.transform, label, 12, TextAnchor.MiddleLeft, TitleColor);
            SetTopLeft(text.rectTransform, 8f, 0f, 174f, 30f);
            button.onClick.AddListener(() => action?.Invoke());
        }

        private void ShowQuickbarSubmenu()
        {
            if (_quickbarSubmenuRoot != null) Destroy(_quickbarSubmenuRoot.gameObject);
            _quickbarSubmenuRoot = NewRect("QuickbarSubmenu", _root);
            Image background = NewImage("SubmenuBackground", _quickbarSubmenuRoot, new Color32(0x12, 0x16, 0x1D, 0xFF));
            Stretch(background.rectTransform);
            _quickbarSubmenuRoot.anchorMin = _quickbarSubmenuRoot.anchorMax = new Vector2(0.5f, 0.5f);
            _quickbarSubmenuRoot.pivot = new Vector2(0f, 1f);
            _quickbarSubmenuRoot.anchoredPosition = _contextMenuRoot.anchoredPosition + new Vector2(194f, 0f);
            _quickbarSubmenuRoot.sizeDelta = new Vector2(150f, 192f);
            for (int i = 0; i < PlayerQuickbar.SlotCount; i++)
            {
                int slot = i;
                Image image = NewImage("QuickbarOption", _quickbarSubmenuRoot, CellColor);
                image.raycastTarget = true;
                Button button = image.gameObject.AddComponent<Button>();
                button.targetGraphic = image;
                SetTopLeft(image.rectTransform, 4f, 4f + i * 31f, 142f, 28f);
                Text label = NewText("QuickbarOptionLabel", image.transform,
                    "Slot " + (i + 1) + (_quickbar.GetItem(i) != Guid.Empty ? "  · replace" : ""), 12, TextAnchor.MiddleLeft, TitleColor);
                SetTopLeft(label.rectTransform, 8f, 0f, 134f, 28f);
                button.onClick.AddListener(() =>
                {
                    if (TryFindQuickbarItem(_contextItemInstanceId, out _, out bool accessible) && accessible)
                        _quickbar.TryMoveAssignment(slot, _contextItemInstanceId);
                    CloseContextMenu(); Refresh();
                });
            }
        }

        private void CloseContextMenu()
        {
            if (_contextMenuRoot != null) Destroy(_contextMenuRoot.gameObject);
            if (_quickbarSubmenuRoot != null) Destroy(_quickbarSubmenuRoot.gameObject);
            _contextMenuRoot = null; _quickbarSubmenuRoot = null; _contextItemInstanceId = Guid.Empty;
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
