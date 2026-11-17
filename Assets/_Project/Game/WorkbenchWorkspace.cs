using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

namespace Signal.Game
{
    /// <summary>
    /// A close-up, tactile first-pass workbench workspace. This is a presentation and
    /// interaction surface only; it deliberately does not invent crafting or economy rules.
    /// </summary>
    public sealed class WorkbenchWorkspace : MonoBehaviour
    {
        private static readonly Color BackdropColor = new Color32(8, 11, 15, 232);
        private static readonly Color FrameColor = new Color32(32, 38, 43, 255);
        private static readonly Color SurfaceColor = new Color32(119, 85, 61, 255);
        private static readonly Color InkColor = new Color32(235, 226, 207, 255);
        private static readonly Color MutedColor = new Color32(185, 181, 168, 255);

        private PlayerMotor2D _motor;
        private Canvas _canvas;
        private GameObject _canvasObject;
        private Text _status;
        private RectTransform _surface;
        private bool _built;

        public bool IsOpen { get; private set; }

        public void Bind(PlayerMotor2D motor)
        {
            _motor = motor;
        }

        public void Open()
        {
            if (IsOpen)
                return;

            if (!_built)
                Build();

            _canvasObject.SetActive(true);
            IsOpen = true;
            if (_motor != null)
                _motor.enabled = false;
        }

        public void Close()
        {
            if (!IsOpen)
                return;

            IsOpen = false;
            if (_canvasObject != null)
                _canvasObject.SetActive(false);
            if (_motor != null)
                _motor.enabled = true;
        }

        private void Update()
        {
            if (IsOpen && Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
                Close();
        }

        private void OnDestroy()
        {
            if (_motor != null)
                _motor.enabled = true;
        }

        private void Build()
        {
            EnsureEventSystem();

            _canvasObject = new GameObject("WorkbenchWorkspaceCanvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            _canvasObject.transform.SetParent(transform, false);
            _canvas = _canvasObject.GetComponent<Canvas>();
            _canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            _canvas.sortingOrder = 200;

            CanvasScaler scaler = _canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;

            RectTransform overlay = CreatePanel(_canvasObject.transform, "Backdrop", BackdropColor);
            Stretch(overlay, Vector2.zero, Vector2.zero);

            RectTransform frame = CreatePanel(overlay, "WorkbenchFrame", FrameColor);
            SetCentered(frame, new Vector2(1240f, 760f), Vector2.zero);

            Text title = CreateText(frame, "WORKBENCH  /  PERSONAL SPACE", 28, InkColor, TextAnchor.MiddleLeft);
            Anchor(title.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(34f, -28f), new Vector2(820f, 42f));

            Text subtitle = CreateText(frame, "Arrange the tools. Inspect the objects. More functionality will come with the game.", 16, MutedColor, TextAnchor.MiddleLeft);
            Anchor(subtitle.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(34f, -70f), new Vector2(900f, 30f));

            Button closeButton = CreateButton(frame, "BACK TO ROOM  [ESC]", new Vector2(230f, 48f));
            RectTransform closeRect = closeButton.GetComponent<RectTransform>();
            closeRect.anchorMin = new Vector2(1f, 1f);
            closeRect.anchorMax = new Vector2(1f, 1f);
            closeRect.pivot = new Vector2(1f, 1f);
            closeRect.anchoredPosition = new Vector2(-28f, -28f);
            closeButton.onClick.AddListener(Close);

            RectTransform tableBorder = CreatePanel(frame, "TableBorder", new Color32(47, 34, 28, 255));
            Anchor(tableBorder, new Vector2(0f, 0f), new Vector2(1f, 1f), new Vector2(28f, 72f), new Vector2(-28f, -126f));

            _surface = CreatePanel(tableBorder, "TableSurface", SurfaceColor);
            Stretch(_surface, new Vector2(10f, 10f), new Vector2(-10f, -10f));

            CreateTableMark(_surface, new Vector2(0f, 0f), new Vector2(900f, 2f), new Color32(93, 66, 49, 120));
            CreateTableMark(_surface, new Vector2(0f, 95f), new Vector2(900f, 2f), new Color32(93, 66, 49, 100));
            CreateTableMark(_surface, new Vector2(0f, -120f), new Vector2(900f, 2f), new Color32(93, 66, 49, 100));

            AddDraggable("SCALE", "A battered pocket scale. The display is scratched.", new Color32(74, 82, 84, 255), new Vector2(-300f, 80f), new Vector2(190f, 116f));
            AddDraggable("GLASS JARS", "A few empty jars, cleaned and ready to be used.", new Color32(92, 132, 119, 255), new Vector2(80f, 125f), new Vector2(210f, 120f));
            AddDraggable("NOTEBOOK", "Loose notes, crossed-out numbers and a coffee stain.", new Color32(157, 132, 91, 255), new Vector2(300f, -105f), new Vector2(220f, 150f));
            AddDraggable("TOOLS", "Small tools. Some are useful. Some are just old.", new Color32(99, 105, 112, 255), new Vector2(-80f, -135f), new Vector2(210f, 72f));
            AddDraggable("TRAY", "A shallow metal tray with a dent in one corner.", new Color32(111, 111, 105, 255), new Vector2(-350f, -135f), new Vector2(170f, 92f));

            _status = CreateText(frame, "Drag objects to rearrange the table. Click one to inspect it.", 17, InkColor, TextAnchor.MiddleLeft);
            Anchor(_status.rectTransform, new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(34f, 18f), new Vector2(1050f, 34f));
            closeRect.SetAsLastSibling();

            _canvasObject.SetActive(false);
            _built = true;
        }

        private void AddDraggable(string label, string description, Color color, Vector2 position, Vector2 size)
        {
            RectTransform item = CreatePanel(_surface, label.Replace(" ", string.Empty), color);
            SetCentered(item, size, position);

            Image image = item.GetComponent<Image>();
            image.raycastTarget = true;

            Text itemLabel = CreateText(item, label, 18, Color.white, TextAnchor.MiddleCenter);
            Stretch(itemLabel.rectTransform, new Vector2(8f, 6f), new Vector2(-8f, -6f));

            WorkbenchDraggable draggable = item.gameObject.AddComponent<WorkbenchDraggable>();
            draggable.Configure(_surface, _canvas, label, description, SelectObject);
        }

        private void SelectObject(string message)
        {
            if (_status != null)
                _status.text = message;
        }

        private void EnsureEventSystem()
        {
            if (EventSystem.current != null)
                return;

            GameObject eventObject = new GameObject("WorkbenchEventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
            eventObject.transform.SetParent(transform, false);
        }

        private static RectTransform CreatePanel(Transform parent, string objectName, Color color)
        {
            GameObject go = new GameObject(objectName, typeof(RectTransform), typeof(Image));
            go.transform.SetParent(parent, false);
            go.GetComponent<Image>().color = color;
            return go.GetComponent<RectTransform>();
        }

        private static Text CreateText(Transform parent, string value, int size, Color color, TextAnchor alignment)
        {
            GameObject go = new GameObject("Text", typeof(RectTransform), typeof(Text));
            go.transform.SetParent(parent, false);
            Text text = go.GetComponent<Text>();
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (text.font == null)
                text.font = Font.CreateDynamicFontFromOSFont("Arial", size);
            text.fontSize = size;
            text.color = color;
            text.alignment = alignment;
            text.text = value;
            text.raycastTarget = false;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Overflow;
            return text;
        }

        private static Button CreateButton(Transform parent, string label, Vector2 size)
        {
            RectTransform rect = CreatePanel(parent, "CloseButton", new Color32(55, 63, 67, 255));
            SetCentered(rect, size, Vector2.zero);
            Text text = CreateText(rect, label, 16, InkColor, TextAnchor.MiddleCenter);
            Stretch(text.rectTransform, Vector2.zero, Vector2.zero);
            Button button = rect.gameObject.AddComponent<Button>();
            button.targetGraphic = rect.GetComponent<Image>();
            return button;
        }

        private static void CreateTableMark(Transform parent, Vector2 position, Vector2 size, Color color)
        {
            RectTransform mark = CreatePanel(parent, "WoodGrain", color);
            SetCentered(mark, size, position);
            mark.GetComponent<Image>().raycastTarget = false;
        }

        private static void Stretch(RectTransform rect, Vector2 minOffset, Vector2 maxOffset)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = minOffset;
            rect.offsetMax = maxOffset;
        }

        private static void SetCentered(RectTransform rect, Vector2 size, Vector2 position)
        {
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = size;
            rect.anchoredPosition = position;
        }

        private static void Anchor(RectTransform rect, Vector2 min, Vector2 max, Vector2 position, Vector2 size)
        {
            rect.anchorMin = min;
            rect.anchorMax = max;
            rect.pivot = min;
            rect.anchoredPosition = position;
            rect.sizeDelta = size;
        }
    }
}