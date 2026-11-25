using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

namespace Signal.Game
{
    /// <summary>
    /// Developer-only catalogue browser. Clicking an entry asks the owning session to
    /// spawn one normally-owned item into the player's real inventory.
    /// </summary>
    public sealed class DebugItemSpawnerPanel : MonoBehaviour
    {
        private static readonly Color PanelColor = new Color32(0x0E, 0x11, 0x16, 0xF5);
        private static readonly Color TileColor = new Color32(0x23, 0x28, 0x30, 0xFF);
        private static readonly Color InkColor = new Color32(0xC9, 0xCE, 0xD6, 0xFF);
        private static readonly Color MutedColor = new Color32(0x9A, 0xA3, 0xB0, 0xFF);
        private static readonly Color AccentColor = new Color32(0x3E, 0xE6, 0xD2, 0xFF);

        private readonly List<ItemDefinition> _definitions = new List<ItemDefinition>();
        private Func<ItemDefinition, string> _spawn;
        private Font _font;
        private bool _open;
        private string _filter = string.Empty;
        private string _status = "Click an item to add one to your carried inventory.";

        private GameObject _root;
        private InputField _search;
        private Text _count;
        private Text _statusText;
        private RectTransform _content;
        private ScrollRect _scroll;

        public bool IsOpen => _open;
        public int ItemCount => _definitions.Count;

        public void Bind(IReadOnlyList<ItemDefinition> definitions, Func<ItemDefinition, string> spawn)
        {
            if (spawn == null)
                throw new ArgumentNullException(nameof(spawn));

            _spawn = spawn;
            _definitions.Clear();
            var registeredIds = new HashSet<Guid>();
            if (definitions != null)
            {
                for (int i = 0; i < definitions.Count; i++)
                {
                    ItemDefinition definition = definitions[i];
                    if (definition == null)
                        continue;

                    try
                    {
                        if (registeredIds.Add(definition.ToItemType().Id))
                            _definitions.Add(definition);
                    }
                    catch (InvalidOperationException)
                    {
                        Debug.LogWarning("DebugItemSpawnerPanel: skipping '" + definition.name +
                            "' because it has no valid stable item ID.");
                    }
                }
            }

            _definitions.Sort((a, b) =>
                string.Compare(DisplayName(a), DisplayName(b), StringComparison.OrdinalIgnoreCase));

            BuildIfNeeded();
            RefreshTiles();
            SetOpen(false);
        }

        public void SetOpen(bool open)
        {
            _open = open;
            if (_root != null)
                _root.SetActive(open);
            if (open)
            {
                _filter = string.Empty;
                if (_search != null)
                    _search.text = string.Empty;
                _status = "Click an item to add one to your carried inventory.";
                RefreshTiles();
                if (_scroll != null)
                    _scroll.verticalNormalizedPosition = 1f;
            }
        }

        /// <summary>Shared spawn boundary, also exposed for focused EditMode tests.</summary>
        public string Spawn(ItemDefinition definition)
        {
            if (definition == null)
                return "That item definition is unavailable.";
            if (_spawn == null)
                return "The item spawner is not connected.";
            _status = _spawn(definition) ?? string.Empty;
            if (_statusText != null)
                _statusText.text = _status;
            return _status;
        }

        private void Update()
        {
            if (!_open)
                return;

            Keyboard keyboard = Keyboard.current;
            if (keyboard != null && keyboard.escapeKey.wasPressedThisFrame)
                SetOpen(false);
        }

        private void BuildIfNeeded()
        {
            if (_root != null)
                return;

            EnsureEventSystem();

            _font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (_font == null)
                _font = Font.CreateDynamicFontFromOSFont("Arial", 16);

            var canvasObject = new GameObject("DebugItemSpawnerCanvas",
                typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvasObject.transform.SetParent(transform, false);
            Canvas canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 130;
            CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1600f, 900f);
            scaler.matchWidthOrHeight = 0.5f;

            Image background = NewImage("Panel", canvasObject.transform, PanelColor);
            _root = background.gameObject;
            RectTransform rootRect = background.rectTransform;
            rootRect.anchorMin = new Vector2(0.5f, 0.5f);
            rootRect.anchorMax = new Vector2(0.5f, 0.5f);
            rootRect.pivot = new Vector2(0.5f, 0.5f);
            rootRect.sizeDelta = new Vector2(1040f, 720f);

            Text title = NewText("Title", rootRect, "ITEM SPAWNER  —  click an item to add one", 22,
                TextAnchor.MiddleLeft, InkColor);
            SetTopLeft(title.rectTransform, 22f, 16f, 760f, 34f);

            Button close = NewButton("Close", rootRect, "CLOSE  [Esc]", new Vector2(866f, 16f),
                new Vector2(150f, 34f));
            close.onClick.AddListener(() => SetOpen(false));

            _search = NewInputField("Search", rootRect, new Vector2(22f, 62f), new Vector2(520f, 38f));
            _search.onValueChanged.AddListener(value =>
            {
                _filter = value ?? string.Empty;
                RefreshTiles();
            });

            _count = NewText("Count", rootRect, string.Empty, 14, TextAnchor.MiddleRight, MutedColor);
            SetTopLeft(_count.rectTransform, 560f, 64f, 458f, 34f);

            Image viewportImage = NewImage("Viewport", rootRect, new Color32(0x12, 0x16, 0x1D, 0xFF));
            RectTransform viewport = viewportImage.rectTransform;
            SetTopLeft(viewport, 22f, 112f, 996f, 540f);
            viewportImage.raycastTarget = true;
            Mask mask = viewportImage.gameObject.AddComponent<Mask>();
            mask.showMaskGraphic = false;

            var contentObject = new GameObject("Content", typeof(RectTransform), typeof(GridLayoutGroup));
            contentObject.transform.SetParent(viewport, false);
            _content = contentObject.GetComponent<RectTransform>();
            _content.anchorMin = new Vector2(0f, 1f);
            _content.anchorMax = new Vector2(1f, 1f);
            _content.pivot = new Vector2(0.5f, 1f);
            _content.anchoredPosition = Vector2.zero;
            _content.sizeDelta = Vector2.zero;

            GridLayoutGroup grid = contentObject.GetComponent<GridLayoutGroup>();
            grid.cellSize = new Vector2(180f, 112f);
            grid.spacing = new Vector2(12f, 12f);
            grid.padding = new RectOffset(12, 12, 12, 12);
            grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            grid.constraintCount = 5;
            grid.startCorner = GridLayoutGroup.Corner.UpperLeft;
            grid.startAxis = GridLayoutGroup.Axis.Horizontal;
            grid.childAlignment = TextAnchor.UpperLeft;

            ContentSizeFitter fitter = contentObject.AddComponent<ContentSizeFitter>();
            fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            _scroll = viewportImage.gameObject.AddComponent<ScrollRect>();
            _scroll.viewport = viewport;
            _scroll.content = _content;
            _scroll.horizontal = false;
            _scroll.vertical = true;
            _scroll.movementType = ScrollRect.MovementType.Clamped;
            _scroll.scrollSensitivity = 32f;

            _statusText = NewText("Status", rootRect, _status, 14, TextAnchor.MiddleLeft, AccentColor);
            SetTopLeft(_statusText.rectTransform, 22f, 664f, 996f, 32f);
        }

        private static void EnsureEventSystem()
        {
            if (EventSystem.current == null)
            {
                var eventSystemObject = new GameObject("SignalEventSystem", typeof(EventSystem));
                eventSystemObject.AddComponent<InputSystemUIInputModule>();
                return;
            }

            if (EventSystem.current.GetComponent<BaseInputModule>() == null)
                EventSystem.current.gameObject.AddComponent<InputSystemUIInputModule>();
        }

        private void RefreshTiles()
        {
            if (_content == null)
                return;

            for (int i = _content.childCount - 1; i >= 0; i--)
                Destroy(_content.GetChild(i).gameObject);

            string query = (_filter ?? string.Empty).Trim();
            int shown = 0;
            foreach (ItemDefinition definition in _definitions)
            {
                if (!Matches(definition, query))
                    continue;
                CreateTile(definition);
                shown++;
            }

            _count.text = shown + " / " + _definitions.Count + " ITEMS";
            LayoutRebuilder.ForceRebuildLayoutImmediate(_content);
        }

        private void CreateTile(ItemDefinition definition)
        {
            var tileObject = new GameObject("Item_" + definition.name,
                typeof(RectTransform), typeof(Image), typeof(Button));
            tileObject.transform.SetParent(_content, false);
            Image tileImage = tileObject.GetComponent<Image>();
            tileImage.color = TileColor;
            tileImage.raycastTarget = true;
            Button button = tileObject.GetComponent<Button>();
            button.targetGraphic = tileImage;
            button.onClick.AddListener(() => Spawn(definition));

            if (definition.Icon != null)
            {
                Image icon = NewImage("Icon", tileObject.transform, Color.white);
                icon.sprite = definition.Icon;
                icon.preserveAspect = true;
                SetTopLeft(icon.rectTransform, 8f, 8f, 64f, 64f);
            }
            else
            {
                Text fallback = NewText("NoIcon", tileObject.transform, "NO ICON", 10,
                    TextAnchor.MiddleCenter, MutedColor);
                SetTopLeft(fallback.rectTransform, 8f, 8f, 64f, 64f);
            }

            Text name = NewText("Name", tileObject.transform, DisplayName(definition), 13,
                TextAnchor.MiddleLeft, InkColor);
            SetTopLeft(name.rectTransform, 78f, 8f, 98f, 48f);

            Text category = NewText("Category", tileObject.transform,
                string.IsNullOrWhiteSpace(definition.Category) ? "General" : definition.Category,
                11, TextAnchor.MiddleLeft, MutedColor);
            SetTopLeft(category.rectTransform, 78f, 58f, 98f, 22f);

            Text footprint = NewText("Footprint", tileObject.transform,
                definition.ToItemType().Width + "×" + definition.ToItemType().Height +
                "  •  stack " + definition.ToItemType().MaxStack,
                10, TextAnchor.MiddleLeft, AccentColor);
            SetTopLeft(footprint.rectTransform, 8f, 84f, 168f, 20f);
        }

        private static bool Matches(ItemDefinition definition, string query)
        {
            if (string.IsNullOrEmpty(query))
                return true;
            return Contains(DisplayName(definition), query) ||
                   Contains(definition.name, query) ||
                   Contains(definition.Category, query) ||
                   Contains(definition.Description, query);
        }

        private static bool Contains(string value, string query)
        {
            return !string.IsNullOrEmpty(value) &&
                   value.IndexOf(query, StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private static string DisplayName(ItemDefinition definition)
        {
            return string.IsNullOrWhiteSpace(definition.DisplayName)
                ? definition.name : definition.DisplayName;
        }

        private InputField NewInputField(string objectName, Transform parent, Vector2 position, Vector2 size)
        {
            Image background = NewImage(objectName, parent, new Color32(0x23, 0x28, 0x30, 0xFF));
            SetTopLeft(background.rectTransform, position.x, position.y, size.x, size.y);
            background.raycastTarget = true;

            Text text = NewText("Text", background.transform, string.Empty, 16,
                TextAnchor.MiddleLeft, InkColor);
            Stretch(text.rectTransform);
            text.rectTransform.offsetMin = new Vector2(10f, 2f);
            text.rectTransform.offsetMax = new Vector2(-10f, -2f);

            Text placeholder = NewText("Placeholder", background.transform, "Search name, category or description…",
                14, TextAnchor.MiddleLeft, MutedColor);
            Stretch(placeholder.rectTransform);
            placeholder.rectTransform.offsetMin = new Vector2(10f, 2f);
            placeholder.rectTransform.offsetMax = new Vector2(-10f, -2f);

            InputField field = background.gameObject.AddComponent<InputField>();
            field.targetGraphic = background;
            field.textComponent = text;
            field.placeholder = placeholder;
            field.lineType = InputField.LineType.SingleLine;
            return field;
        }

        private Button NewButton(string objectName, Transform parent, string label,
            Vector2 position, Vector2 size)
        {
            Image image = NewImage(objectName, parent, new Color32(0x3A, 0x44, 0x50, 0xFF));
            SetTopLeft(image.rectTransform, position.x, position.y, size.x, size.y);
            image.raycastTarget = true;
            Text text = NewText("Label", image.transform, label, 13, TextAnchor.MiddleCenter, InkColor);
            Stretch(text.rectTransform);
            Button button = image.gameObject.AddComponent<Button>();
            button.targetGraphic = image;
            return button;
        }

        private Image NewImage(string objectName, Transform parent, Color color)
        {
            var go = new GameObject(objectName, typeof(RectTransform), typeof(Image));
            go.transform.SetParent(parent, false);
            Image image = go.GetComponent<Image>();
            image.color = color;
            image.raycastTarget = false;
            return image;
        }

        private Text NewText(string objectName, Transform parent, string value,
            int size, TextAnchor alignment, Color color)
        {
            var go = new GameObject(objectName, typeof(RectTransform), typeof(Text));
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

        private static void SetTopLeft(RectTransform rect, float x, float y, float width, float height)
        {
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(0f, 1f);
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
    }
}
