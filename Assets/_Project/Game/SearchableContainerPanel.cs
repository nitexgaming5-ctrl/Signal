using System;
using System.Collections.Generic;
using Signal.Core;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace Signal.Game
{
    /// <summary>Side-by-side physical container and carried inventory UI with resumable progressive search.</summary>
    public sealed class SearchableContainerPanel : MonoBehaviour
    {
        private const float CellSize = 46f;
        private const float RevealInterval = 0.85f;
        private const float DoubleClickWindow = 0.34f;
        private static readonly Color PanelColor = new Color32(14, 17, 22, 246);
        private static readonly Color CellColor = new Color32(35, 40, 48, 255);
        private static readonly Color TitleColor = new Color32(201, 206, 214, 255);
        private static readonly Color InkColor = new Color32(14, 17, 22, 255);
        private static readonly Color[] Palette =
        {
            new Color32(139, 74, 43, 255), new Color32(200, 162, 74, 255),
            new Color32(62, 230, 210, 255), new Color32(226, 79, 181, 255),
            new Color32(107, 114, 128, 255), new Color32(201, 206, 214, 255)
        };

        private struct ItemVisual { public string Label; public Color Color; }

        private readonly Dictionary<Guid, ItemVisual> _visuals = new Dictionary<Guid, ItemVisual>();
        private GridInventory _playerInventory;
        private readonly List<GridInventory> _pocketInventories = new List<GridInventory>();
        private bool _backpackAccessible = true;
        private SearchableContainer _container;
        private string _containerDisplayName;
        private Func<Guid, bool> _takeFirstFit;
        private Func<Guid, int, int, bool, bool> _takeAt;
        private Action<PlacedItem> _inspectItem;
        private Font _font;
        private bool _built;
        private bool _open;
        private bool _searching;
        private float _revealTimer;
        private float _lastClickTime = -10f;
        private Guid _lastClickId;

        private RectTransform _root;
        private RectTransform _containerGrid;
        private RectTransform _containerCells;
        private RectTransform _containerItems;
        private RectTransform _playerGrid;
        private RectTransform _playerCells;
        private RectTransform _playerItems;
        private RectTransform _pocketItems;
        private Text _containerTitle;
        private Text _playerTitle;
        private Text _status;
        private Image _searchButton;
        private Image _closeButton;
        private Text _searchButtonText;
        private Image _dragGhost;
        private Text _dragGhostText;
        private Image _dropPreview;

        private bool _dragging;
        private bool _dragFromPlayer;
        private Guid _draggedItemId;
        private PlacedItem _draggedItem;
        private int _grabX;
        private int _grabY;
        private bool _dragRotated;

        public bool IsOpen => _open;

        public void Bind(IReadOnlyList<ItemDefinition> definitions, Func<Guid, bool> takeFirstFit,
            Func<Guid, int, int, bool, bool> takeAt, Action<PlacedItem> inspectItem = null)
        {
            _takeFirstFit = takeFirstFit ?? throw new ArgumentNullException(nameof(takeFirstFit));
            _takeAt = takeAt ?? throw new ArgumentNullException(nameof(takeAt));
            _inspectItem = inspectItem;
            _visuals.Clear();
            if (definitions != null)
            {
                for (int i = 0; i < definitions.Count; i++)
                {
                    ItemDefinition definition = definitions[i];
                    if (definition == null) continue;
                    try
                    {
                        ItemType type = definition.ToItemType();
                        string label = string.IsNullOrWhiteSpace(definition.DisplayName) ? definition.name : definition.DisplayName;
                        _visuals[type.Id] = new ItemVisual { Label = label, Color = Palette[i % Palette.Length] };
                    }
                    catch (InvalidOperationException)
                    {
                        Debug.LogWarning("SearchableContainerPanel skipped an item with an invalid definition ID: " + definition.name);
                    }
                }
            }
            BuildIfNeeded();
            Close();
        }

        public void Open(SearchableContainer container, GridInventory playerInventory, string displayName = null)
        {
            Open(container, playerInventory, null, true, displayName);
        }

        public void Open(SearchableContainer container, GridInventory backpackInventory,
            IReadOnlyList<GridInventory> pockets, bool backpackAccessible, string displayName = null)
        {
            if (container == null || backpackInventory == null)
                throw new ArgumentNullException(container == null ? nameof(container) : nameof(backpackInventory));

            Unsubscribe();
            CancelDrag();
            _container = container;
            _containerDisplayName = string.IsNullOrWhiteSpace(displayName) ? "CONTAINER" : displayName.ToUpperInvariant();
            _playerInventory = backpackInventory;
            _backpackAccessible = backpackAccessible;
            _pocketInventories.Clear();
            if (pockets != null)
                for (int i = 0; i < pockets.Count && i < PlayerPocketBook.PocketCount; i++)
                    if (pockets[i] != null && !_pocketInventories.Contains(pockets[i]))
                        _pocketInventories.Add(pockets[i]);
            RebuildPlayerGrid(_backpackAccessible ? _playerInventory.Width : 1,
                _backpackAccessible ? _playerInventory.Height : 1);
            _playerGrid.gameObject.SetActive(_backpackAccessible);
            _lastClickId = Guid.Empty;
            _lastClickTime = -10f;
            _container.Inventory.Changed += OnInventoryChanged;
            _playerInventory.Changed += OnInventoryChanged;
            for (int i = 0; i < _pocketInventories.Count; i++)
                _pocketInventories[i].Changed += OnInventoryChanged;
            _open = true;
            _searching = _container.HasSearchStarted && !_container.IsSearchComplete;
            _revealTimer = 0f;
            _root.gameObject.SetActive(true);
            Refresh();
        }

        public void Close()
        {
            _searching = false;
            _lastClickId = Guid.Empty;
            _lastClickTime = -10f;
            _open = false;
            CancelDrag();
            Unsubscribe();
            _container = null;
            _playerInventory = null;
            if (_root != null) _root.gameObject.SetActive(false);
        }

        private void OnDestroy() => Unsubscribe();

        private void Unsubscribe()
        {
            if (_container != null) _container.Inventory.Changed -= OnInventoryChanged;
            if (_playerInventory != null) _playerInventory.Changed -= OnInventoryChanged;
            for (int i = 0; i < _pocketInventories.Count; i++)
                _pocketInventories[i].Changed -= OnInventoryChanged;
        }

        private void OnInventoryChanged(GridInventory inventory)
        {
            if (_open) Refresh();
        }

        private void Update()
        {
            if (!_open || _container == null || _playerInventory == null) return;
            Keyboard keyboard = Keyboard.current;
            Mouse mouse = Mouse.current;

            if (keyboard != null && keyboard.escapeKey.wasPressedThisFrame)
            {
                Close();
                return;
            }

            if (_searching)
            {
                _revealTimer += Time.unscaledDeltaTime;
                if (_revealTimer >= RevealInterval)
                {
                    _revealTimer -= RevealInterval;
                    if (!_container.TryRevealNext(out _))
                        _searching = false;
                    if (_container.IsSearchComplete)
                        _searching = false;
                    Refresh();
                }
            }

            if (mouse != null)
                UpdatePointer(keyboard, mouse);
        }

        private void StartSearch()
        {
            if (_container == null || _container.HasSearchStarted || !_container.TryStartSearch()) return;
            _searching = true;
            _revealTimer = 0f;
            Refresh();
        }

        private void UpdatePointer(Keyboard keyboard, Mouse mouse)
        {
            Vector2 screen = mouse.position.ReadValue();
            if (mouse.leftButton.wasPressedThisFrame && _closeButton != null &&
                RectTransformUtility.RectangleContainsScreenPoint(_closeButton.rectTransform, screen, null))
            {
                Close();
                return;
            }
            if (mouse.leftButton.wasPressedThisFrame && _searchButton != null &&
                RectTransformUtility.RectangleContainsScreenPoint(_searchButton.rectTransform, screen, null))
            {
                StartSearch();
                return;
            }

            bool inContainer = TryGetCell(_containerGrid, _container.Inventory.Width, _container.Inventory.Height,
                screen, out int containerX, out int containerY, out _);
            bool inPlayer = TryGetCell(_playerGrid, _playerInventory.Width, _playerInventory.Height,
                screen, out int playerX, out int playerY, out _);

            if (!_dragging)
            {
                if (mouse.rightButton.wasPressedThisFrame && inContainer &&
                    _container.Inventory.TryGetItemAt(containerX, containerY, out PlacedItem inspectedContainerItem) &&
                    _container.IsDiscovered(inspectedContainerItem.ItemInstanceId))
                {
                    _inspectItem?.Invoke(inspectedContainerItem);
                    return;
                }

                if (mouse.rightButton.wasPressedThisFrame && inPlayer &&
                    _playerInventory.TryGetItemAt(playerX, playerY, out PlacedItem inspectedPlayerItem))
                {
                    _inspectItem?.Invoke(inspectedPlayerItem);
                    return;
                }

                if (!mouse.leftButton.wasPressedThisFrame)
                    return;

                if (inContainer &&
                    _container.Inventory.TryGetItemAt(containerX, containerY, out PlacedItem containerItem) &&
                    _container.IsDiscovered(containerItem.ItemInstanceId))
                {
                    if (_lastClickId == containerItem.ItemInstanceId &&
                        Time.unscaledTime - _lastClickTime <= DoubleClickWindow)
                    {
                        _lastClickId = Guid.Empty;
                        _lastClickTime = -10f;
                        _takeFirstFit?.Invoke(containerItem.ItemInstanceId);
                        Refresh();
                        return;
                    }

                    _lastClickId = containerItem.ItemInstanceId;
                    _lastClickTime = Time.unscaledTime;
                    BeginDrag(containerItem, containerX, containerY, false);
                }
                else if (inPlayer &&
                    _playerInventory.TryGetItemAt(playerX, playerY, out PlacedItem playerItem))
                {
                    _lastClickId = Guid.Empty;
                    _lastClickTime = -10f;
                    BeginDrag(playerItem, playerX, playerY, true);
                }
                else
                {
                    return;
                }
            }

            if (!_dragging) return;
            if (keyboard != null && keyboard.escapeKey.wasPressedThisFrame)
            {
                CancelDrag();
                Refresh();
                return;
            }
            if (keyboard != null && keyboard.rKey.wasPressedThisFrame)
                RotateDrag();

            int targetX = playerX - _grabX;
            int targetY = playerY - _grabY;
            bool canDrop = inPlayer && (_dragFromPlayer
                ? _playerInventory.CanMove(_draggedItem.InstanceId, targetX, targetY, _dragRotated)
                : _playerInventory.CanPlaceTransferred(_draggedItem, targetX, targetY, _dragRotated));
            UpdateDragVisuals(screen, inPlayer, targetX, targetY, canDrop);

            if (mouse.leftButton.wasReleasedThisFrame)
            {
                Guid itemId = _draggedItemId;
                int localInstanceId = _draggedItem.InstanceId;
                bool fromPlayer = _dragFromPlayer;
                bool rotated = _dragRotated;
                EndDrag();
                if (canDrop)
                {
                    if (fromPlayer)
                        _playerInventory.TryMove(localInstanceId, targetX, targetY, rotated);
                    else
                        _takeAt?.Invoke(itemId, targetX, targetY, rotated);
                }
                Refresh();
            }
        }

        private void BeginDrag(PlacedItem item, int cellX, int cellY, bool fromPlayer)
        {
            _dragging = true;
            _dragFromPlayer = fromPlayer;
            _draggedItem = item;
            _draggedItemId = item.ItemInstanceId;
            _dragRotated = item.Rotated;
            _grabX = cellX - item.X;
            _grabY = cellY - item.Y;
            ItemVisual visual = VisualFor(item.Type);
            _dragGhost.color = visual.Color;
            _dragGhostText.text = visual.Label;
            _dragGhost.gameObject.SetActive(true);
            Refresh();
        }

        private void RotateDrag()
        {
            int width = _dragRotated ? _draggedItem.Type.Height : _draggedItem.Type.Width;
            int height = _dragRotated ? _draggedItem.Type.Width : _draggedItem.Type.Height;
            InventoryDragMath.RotateGrabOffsetClockwise(_grabX, _grabY, width, height, out int x, out int y);
            _grabX = x;
            _grabY = y;
            _dragRotated = !_dragRotated;
        }

        private void CancelDrag()
        {
            _dragging = false;
            _dragFromPlayer = false;
            if (_dragGhost != null) _dragGhost.gameObject.SetActive(false);
            if (_dropPreview != null) _dropPreview.gameObject.SetActive(false);
        }

        private void EndDrag() => CancelDrag();

        private void UpdateDragVisuals(Vector2 screen, bool inPlayer, int targetX, int targetY, bool canDrop)
        {
            float width = (_dragRotated ? _draggedItem.Type.Height : _draggedItem.Type.Width) * CellSize;
            float height = (_dragRotated ? _draggedItem.Type.Width : _draggedItem.Type.Height) * CellSize;
            _dragGhost.rectTransform.sizeDelta = new Vector2(width, height);

            if (inPlayer)
            {
                Vector3 targetWorld = _playerGrid.TransformPoint(
                    new Vector3(targetX * CellSize, -targetY * CellSize, 0f));
                _dragGhost.rectTransform.anchoredPosition = _root.InverseTransformPoint(targetWorld);
            }
            else if (RectTransformUtility.ScreenPointToLocalPointInRectangle(_root, screen, null, out Vector2 local))
            {
                // Keep the cell originally grabbed under the pointer instead of centring the whole item on it.
                _dragGhost.rectTransform.anchoredPosition = new Vector2(
                    local.x - _grabX * CellSize, local.y + _grabY * CellSize);
            }

            if (!inPlayer)
            {
                _dropPreview.gameObject.SetActive(false);
                return;
            }
            _dropPreview.color = canDrop ? new Color(0.2f, 0.9f, 0.75f, 0.35f) : new Color(0.9f, 0.25f, 0.3f, 0.35f);
            SetTopLeft(_dropPreview.rectTransform, targetX * CellSize, targetY * CellSize,
                width, height);
            _dropPreview.gameObject.SetActive(true);
        }

        private static bool TryGetCell(RectTransform grid, int width, int height, Vector2 screen,
            out int x, out int y, out Vector2 local)
        {
            x = y = 0;
            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(grid, screen, null, out local))
                return false;
            return InventoryDragMath.TryGetCell(local.x, -local.y, CellSize, width, height, out x, out y);
        }

        private void Refresh()
        {
            if (!_open || _container == null || _playerInventory == null) return;
            ClearChildren(_containerItems);
            ClearChildren(_playerItems);
            ClearChildren(_pocketItems);

            foreach (PlacedItem item in _container.Inventory.Items)
                if (_container.IsDiscovered(item.ItemInstanceId))
                    CreateItemView(item, _containerItems, _dragging && item.ItemInstanceId == _draggedItemId);

            if (_backpackAccessible)
                foreach (PlacedItem item in _playerInventory.Items)
                    CreateItemView(item, _playerItems, _dragging && _dragFromPlayer && item.ItemInstanceId == _draggedItemId);

            for (int pocketIndex = 0; pocketIndex < _pocketInventories.Count; pocketIndex++)
                foreach (PlacedItem item in _pocketInventories[pocketIndex].Items)
                    CreateItemViewAt(item, _pocketItems, false, pocketIndex, 0);

            _playerTitle.text = _backpackAccessible
                ? "BACKPACK STORAGE + PERSONAL POCKETS"
                : "PERSONAL POCKETS  ·  BACKPACK INACCESSIBLE";
            _status.text = _container.IsSearchComplete ? "Search complete" :
                _container.HasSearchStarted ? (_searching ? "Searching..." : "Search paused — reopening resumes") :
                "Contents are concealed";
            _containerTitle.text = _container.HasSearchStarted
                ? _containerDisplayName + "  " + _container.DiscoveredCount + " / " + _container.TotalGeneratedCount
                : _containerDisplayName;
            _searchButton.color = _container.HasSearchStarted || _container.IsSearchComplete
                ? new Color32(80, 86, 92, 255) : new Color32(62, 230, 210, 255);
            _searchButtonText.text = _container.IsSearchComplete ? "SEARCH COMPLETE" :
                _container.HasSearchStarted ? "SEARCHING..." : "SEARCH";
        }

        private void CreateItemView(PlacedItem item, RectTransform parent, bool dim)
        {
            CreateItemViewAt(item, parent, dim, item.X, item.Y);
        }

        private void CreateItemViewAt(PlacedItem item, RectTransform parent, bool dim, int x, int y)
        {
            ItemVisual visual = VisualFor(item.Type);
            Color color = visual.Color;
            if (dim) color.a = 0.25f;
            Image image = NewImage("Item_" + item.ItemInstanceId, parent, color);
            SetTopLeft(image.rectTransform, x * CellSize + 2f, y * CellSize + 2f,
                item.FootprintWidth * CellSize - 4f, item.FootprintHeight * CellSize - 4f);
            string label = visual.Label + (item.Count > 1 ? "\\nx" + item.Count : string.Empty);
            Text text = NewText("Label", image.transform, label, 12, TextAnchor.MiddleCenter, InkColor);
            Stretch(text.rectTransform);
        }

        private ItemVisual VisualFor(ItemType type)
        {
            return _visuals.TryGetValue(type.Id, out ItemVisual visual)
                ? visual : new ItemVisual { Label = "?", Color = Palette[5] };
        }

        private void BuildIfNeeded()
        {
            if (_built) return;
            _font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (_font == null) _font = Font.CreateDynamicFontFromOSFont("Arial", 16);

            var canvasObject = new GameObject("SearchableContainerCanvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler));
            canvasObject.transform.SetParent(transform, false);
            Canvas canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 120;
            CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1600f, 900f);
            scaler.matchWidthOrHeight = 0.5f;

            Image background = NewImage("Background", canvasObject.transform, PanelColor);
            _root = background.rectTransform;
            _root.anchorMin = _root.anchorMax = new Vector2(0.5f, 0.5f);
            _root.pivot = new Vector2(0.5f, 0.5f);
            _root.sizeDelta = new Vector2(1000f, 560f);

            Text heading = NewText("Heading", _root, "CONTAINER SEARCH  —  [ESC] close   [R] rotate while dragging", 18,
                TextAnchor.MiddleLeft, TitleColor);
            SetTopLeft(heading.rectTransform, 24f, 12f, 760f, 30f);

            _closeButton = CreateButton("Close", _root, "CLOSE", new Vector2(860f, 12f), new Vector2(110f, 34f));

            Image left = NewImage("ContainerPanel", _root, new Color32(22, 27, 34, 255));
            Center(left.rectTransform, new Vector2(-255f, -22f), new Vector2(470f, 470f));
            _containerTitle = NewText("ContainerTitle", left.transform, "CONTAINER", 16, TextAnchor.MiddleLeft, TitleColor);
            SetTopLeft(_containerTitle.rectTransform, 18f, 12f, 410f, 30f);
            _containerGrid = NewRect("Grid", left.transform);
            SetTopLeft(_containerGrid, 24f, 54f, 8f * CellSize, 6f * CellSize);
            _containerCells = NewRect("Cells", _containerGrid);
            Stretch(_containerCells);
            _containerItems = NewRect("Items", _containerGrid);
            Stretch(_containerItems);
            _status = NewText("SearchStatus", left.transform, string.Empty, 13, TextAnchor.MiddleLeft, TitleColor);
            SetTopLeft(_status.rectTransform, 20f, 350f, 420f, 24f);
            _searchButton = CreateButton("Search", left.transform, "SEARCH", new Vector2(20f, 390f), new Vector2(180f, 42f));
            _searchButtonText = _searchButton.GetComponentInChildren<Text>();

            Image right = NewImage("PlayerPanel", _root, new Color32(22, 27, 34, 255));
            Center(right.rectTransform, new Vector2(255f, -22f), new Vector2(470f, 470f));
            _playerTitle = NewText("PlayerTitle", right.transform, "CARRIED INVENTORY", 16, TextAnchor.MiddleLeft, TitleColor);
            SetTopLeft(_playerTitle.rectTransform, 18f, 12f, 420f, 30f);
            _playerGrid = NewRect("BackpackGrid", right.transform);
            SetTopLeft(_playerGrid, 24f, 54f, 8f * CellSize, 6f * CellSize);
            _playerCells = NewRect("Cells", _playerGrid);
            Stretch(_playerCells);
            _playerItems = NewRect("Items", _playerGrid);
            Stretch(_playerItems);

            RectTransform pocketGrid = NewRect("PersonalPockets", right.transform);
            SetTopLeft(pocketGrid, 24f, 340f, PlayerPocketBook.PocketCount * CellSize, CellSize);
            BuildGridCells(pocketGrid, PlayerPocketBook.PocketCount, 1);
            _pocketItems = NewRect("PocketItems", pocketGrid);
            Stretch(_pocketItems);

            _dragGhost = NewImage("DragGhost", _root, Color.white);
            Center(_dragGhost.rectTransform, Vector2.zero, new Vector2(CellSize, CellSize));
            _dragGhost.rectTransform.pivot = new Vector2(0f, 1f);
            _dragGhostText = NewText("Label", _dragGhost.transform, string.Empty, 12, TextAnchor.MiddleCenter, InkColor);
            Stretch(_dragGhostText.rectTransform);
            _dragGhost.gameObject.SetActive(false);
            _dropPreview = NewImage("DropPreview", _playerGrid, Color.white);
            _dropPreview.gameObject.SetActive(false);

            BuildGridCells(_containerCells, 8, 6);
            _built = true;
            _root.gameObject.SetActive(false);
        }

        private void RebuildPlayerGrid(int width, int height)
        {
            if (_playerGrid == null || _playerCells == null || width <= 0 || height <= 0)
                return;

            ClearChildren(_playerCells);
            _playerGrid.sizeDelta = new Vector2(width * CellSize, height * CellSize);
            BuildGridCells(_playerCells, width, height);
        }

        private void BuildGridCells(RectTransform parent, int width, int height)
        {
            for (int y = 0; y < height; y++)
                for (int x = 0; x < width; x++)
                {
                    Image cell = NewImage("Cell_" + x + "_" + y, parent, CellColor);
                    SetTopLeft(cell.rectTransform, x * CellSize + 1f, y * CellSize + 1f, CellSize - 2f, CellSize - 2f);
                }
        }

        private Image CreateButton(string name, Transform parent, string label, Vector2 topLeft, Vector2 size)
        {
            Image image = NewImage(name, parent, new Color32(62, 230, 210, 255));
            SetTopLeft(image.rectTransform, topLeft.x, topLeft.y, size.x, size.y);
            Text text = NewText("Label", image.transform, label, 13, TextAnchor.MiddleCenter, InkColor);
            Stretch(text.rectTransform);
            return image;
        }

        private static RectTransform NewRect(string name, Transform parent)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            return go.GetComponent<RectTransform>();
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

        private Text NewText(string name, Transform parent, string value, int size, TextAnchor alignment, Color color)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Text));
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

        private static void Center(RectTransform rect, Vector2 position, Vector2 size)
        {
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = position;
            rect.sizeDelta = size;
        }

        private static void SetTopLeft(RectTransform rect, float x, float yDown, float width, float height)
        {
            rect.anchorMin = rect.anchorMax = new Vector2(0f, 1f);
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

        private static void ClearChildren(RectTransform parent)
        {
            for (int i = parent.childCount - 1; i >= 0; i--)
            {
                GameObject child = parent.GetChild(i).gameObject;
                child.SetActive(false);
                Destroy(child);
            }
        }
    }
}
