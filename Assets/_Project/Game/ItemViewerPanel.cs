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
    /// <summary>
    /// Focused read-only viewer for one physical item stack. It presents the item's authored
    /// definition alongside the state and provisional value of this particular instance.
    /// </summary>
    public sealed class ItemViewerPanel : MonoBehaviour
    {
        private static readonly Color BackdropColor = new Color32(5, 8, 12, 238);
        private static readonly Color FrameColor = new Color32(24, 29, 34, 255);
        private static readonly Color CardColor = new Color32(35, 41, 46, 255);
        private static readonly Color AccentColor = new Color32(202, 163, 91, 255);
        private static readonly Color MainTextColor = new Color32(235, 229, 215, 255);
        private static readonly Color MutedTextColor = new Color32(166, 171, 170, 255);

        private readonly Dictionary<Guid, ItemDefinition> _definitions = new Dictionary<Guid, ItemDefinition>();
        private GameObject _canvasObject;
        private RectTransform _frame;
        private Image _itemImage;
        private Text _imageHint;
        private Text _title;
        private Text _category;
        private Text _description;
        private Text _quantity;
        private Text _quality;
        private Text _condition;
        private Text _authenticity;
        private Text _ownership;
        private Text _baseValue;
        private Text _valueBreakdown;
        private bool _built;

        public bool IsOpen { get; private set; }

        public void Bind(IReadOnlyList<ItemDefinition> definitions)
        {
            _definitions.Clear();
            if (definitions != null)
            {
                foreach (ItemDefinition definition in definitions)
                {
                    if (definition == null)
                        continue;

                    try
                    {
                        _definitions[definition.ToItemType().Id] = definition;
                    }
                    catch (InvalidOperationException)
                    {
                        Debug.LogWarning("ItemViewerPanel: skipping item with an invalid id: " + definition.name);
                    }
                }
            }

            BuildIfNeeded();
            Close();
        }

        public void Open(PlacedItem item)
        {
            BuildIfNeeded();
            ItemDefinition definition = _definitions.TryGetValue(item.Type.Id, out ItemDefinition found) ? found : null;
            string name = definition == null || string.IsNullOrWhiteSpace(definition.DisplayName)
                ? (definition == null ? "Unknown item" : definition.name)
                : definition.DisplayName;

            _title.text = name;
            _category.text = "CATEGORY  /  " +
                (definition == null || string.IsNullOrWhiteSpace(definition.Category) ? "Unclassified" : definition.Category);
            _description.text = definition == null || string.IsNullOrWhiteSpace(definition.Description)
                ? "No description has been authored for this item."
                : definition.Description;
            _quantity.text = "STACK QUANTITY  /  " + item.Count;
            _quality.text = "QUALITY  /  " + Format(item.State.Quality);
            _condition.text = "CONDITION  /  " + Format(item.State.Condition);
            _authenticity.text = "AUTHENTICITY  /  " + Format(item.State.Authenticity);
            _ownership.text = "OWNERSHIP  /  " + Format(item.State.Ownership);

            int baseValue = definition == null ? 0 : definition.BaseValue;
            ItemValueBreakdown value = ItemValueCalculator.Calculate(baseValue, item.State);
            _baseValue.text = "REFERENCE UNIT VALUE  /  " + baseValue;
            _valueBreakdown.text =
                "Quality  ×" + value.QualityMultiplier.ToString("0.##") +
                "     Condition  ×" + value.ConditionMultiplier.ToString("0.##") + "\n" +
                "Authenticity  ×" + value.AuthenticityMultiplier.ToString("0.##") +
                "     Ownership  ×" + value.OwnershipMultiplier.ToString("0.##") + "\n\n" +
                "INDICATIVE UNIT VALUE  /  " + value.FinalValue + "\\n" +
                "STACK REFERENCE VALUE  /  " + (value.FinalValue * item.Count);

            _itemImage.sprite = definition == null ? null : definition.Icon;
            _itemImage.enabled = _itemImage.sprite != null;
            _imageHint.gameObject.SetActive(_itemImage.sprite == null);
            IsOpen = true;
            _canvasObject.SetActive(true);
        }

        public void Close()
        {
            IsOpen = false;
            if (_canvasObject != null)
                _canvasObject.SetActive(false);
        }

        private void Update()
        {
            if (IsOpen && Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
                Close();
        }

        private void BuildIfNeeded()
        {
            if (_built)
                return;

            EnsureEventSystem();
            _canvasObject = new GameObject("ItemViewerCanvas", typeof(RectTransform), typeof(Canvas),
                                           typeof(CanvasScaler), typeof(GraphicRaycaster));
            _canvasObject.transform.SetParent(transform, false);
            Canvas canvas = _canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 240;

            CanvasScaler scaler = _canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;

            RectTransform backdrop = Panel(_canvasObject.transform, "Backdrop", BackdropColor);
            Stretch(backdrop);
            _frame = Panel(backdrop, "InspectionFrame", FrameColor);
            Center(_frame, new Vector2(1120f, 760f));

            Text heading = Label(_frame, "ITEM INSPECTION", 28, AccentColor, TextAnchor.MiddleLeft);
            Anchor(heading.rectTransform, new Vector2(34f, -24f), new Vector2(650f, 46f), true);

            Button close = MakeButton(_frame, "CLOSE  [ESC]", new Vector2(180f, 48f));
            RectTransform closeRect = close.transform as RectTransform;
            closeRect.anchorMin = closeRect.anchorMax = new Vector2(1f, 1f);
            closeRect.pivot = new Vector2(1f, 1f);
            closeRect.anchoredPosition = new Vector2(-30f, -24f);
            close.onClick.AddListener(Close);

            RectTransform imageCard = Panel(_frame, "ItemImageCard", CardColor);
            Anchor(imageCard, new Vector2(34f, -100f), new Vector2(330f, 270f), true);
            GameObject artwork = new GameObject("ItemArtwork", typeof(RectTransform), typeof(Image));
            artwork.transform.SetParent(imageCard, false);
            _itemImage = artwork.GetComponent<Image>();
            _itemImage.color = Color.white;
            _itemImage.preserveAspect = true;
            _itemImage.raycastTarget = false;
            _itemImage.enabled = false;
            Stretch(_itemImage.rectTransform);
            _itemImage.rectTransform.offsetMin = new Vector2(12f, 12f);
            _itemImage.rectTransform.offsetMax = new Vector2(-12f, -12f);

            _imageHint = Label(imageCard, "ITEM IMAGE", 14, MutedTextColor, TextAnchor.MiddleCenter);
            Stretch(_imageHint.rectTransform);

            float rightX = 400f;
            float rightWidth = 680f;
            _title = Label(_frame, string.Empty, 30, MainTextColor, TextAnchor.MiddleLeft);
            Anchor(_title.rectTransform, new Vector2(rightX, -94f), new Vector2(rightWidth, 48f), true);

            _category = Label(_frame, string.Empty, 15, AccentColor, TextAnchor.MiddleLeft);
            Anchor(_category.rectTransform, new Vector2(rightX, -144f), new Vector2(rightWidth, 30f), true);

            _description = Label(_frame, string.Empty, 17, MutedTextColor, TextAnchor.UpperLeft);
            Anchor(_description.rectTransform, new Vector2(rightX, -184f), new Vector2(rightWidth, 86f), true);

            RectTransform stateCard = Panel(_frame, "ItemState", CardColor);
            Anchor(stateCard, new Vector2(34f, -400f), new Vector2(1012f, 160f), true);
            _quantity = Label(stateCard, string.Empty, 16, AccentColor, TextAnchor.MiddleLeft);
            Anchor(_quantity.rectTransform, new Vector2(18f, -12f), new Vector2(300f, 30f), true);
            _quality = Label(stateCard, string.Empty, 17, MainTextColor, TextAnchor.MiddleLeft);
            Anchor(_quality.rectTransform, new Vector2(18f, -48f), new Vector2(460f, 30f), true);
            _condition = Label(stateCard, string.Empty, 17, MainTextColor, TextAnchor.MiddleLeft);
            Anchor(_condition.rectTransform, new Vector2(500f, -48f), new Vector2(460f, 30f), true);
            _authenticity = Label(stateCard, string.Empty, 17, MainTextColor, TextAnchor.MiddleLeft);
            Anchor(_authenticity.rectTransform, new Vector2(18f, -94f), new Vector2(460f, 30f), true);
            _ownership = Label(stateCard, string.Empty, 17, MainTextColor, TextAnchor.MiddleLeft);
            Anchor(_ownership.rectTransform, new Vector2(500f, -94f), new Vector2(460f, 30f), true);

            _baseValue = Label(_frame, string.Empty, 16, MutedTextColor, TextAnchor.MiddleLeft);
            Anchor(_baseValue.rectTransform, new Vector2(34f, 170f), new Vector2(500f, 26f), false);
            _valueBreakdown = Label(_frame, string.Empty, 17, MainTextColor, TextAnchor.UpperLeft);
            Anchor(_valueBreakdown.rectTransform, new Vector2(34f, 30f), new Vector2(1012f, 126f), false);

            _canvasObject.SetActive(false);
            _built = true;
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

            var eventObject = new GameObject("ItemViewerEventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
            eventObject.transform.SetParent(transform, false);
        }

        private static string Format(Enum value)
        {
            return value.ToString().ToUpperInvariant();
        }

        private static RectTransform Panel(Transform parent, string objectName, Color color)
        {
            GameObject go = new GameObject(objectName, typeof(RectTransform), typeof(Image));
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
            if (text.font == null)
                text.font = Font.CreateDynamicFontFromOSFont("Arial", size);
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
            Text text = Label(rect, value, 15, MainTextColor, TextAnchor.MiddleCenter);
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
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
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