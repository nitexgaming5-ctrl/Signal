using System;
using Signal.Core;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

namespace Signal.Game
{
    /// <summary>
    /// Supplier-facing offer UI for one authored workshop upgrade.
    /// The GameSession callbacks remain authoritative for proximity, prerequisites and payment.
    /// </summary>
    public sealed class WorkshopUpgradePanel : MonoBehaviour
    {
        private static readonly Color Backdrop = new Color32(8, 11, 15, 238);
        private static readonly Color Frame = new Color32(27, 33, 39, 255);
        private static readonly Color Accent = new Color32(200, 162, 74, 255);
        private static readonly Color Ink = new Color32(235, 226, 207, 255);
        private static readonly Color Muted = new Color32(170, 178, 184, 255);

        private WorkshopUpgradeDefinition _definition;
        private string _supplierDisplayName;
        private Func<string> _getAvailability;
        private Func<bool> _isAcquired;
        private Func<bool> _canPurchase;
        private Func<string> _purchase;
        private GameObject _root;
        private Text _details;
        private Text _status;
        private Text _purchaseLabel;
        private Button _purchaseButton;
        private Font _font;

        public bool IsOpen => _root != null && _root.activeSelf;

        public void Bind(WorkshopUpgradeDefinition definition, string supplierDisplayName,
            Func<string> getAvailability, Func<bool> isAcquired, Func<bool> canPurchase,
            Func<string> purchase)
        {
            _definition = definition ?? throw new ArgumentNullException(nameof(definition));
            _supplierDisplayName = string.IsNullOrWhiteSpace(supplierDisplayName)
                ? definition.PayeeNpcId
                : supplierDisplayName;
            _getAvailability = getAvailability ?? throw new ArgumentNullException(nameof(getAvailability));
            _isAcquired = isAcquired ?? throw new ArgumentNullException(nameof(isAcquired));
            _canPurchase = canPurchase ?? throw new ArgumentNullException(nameof(canPurchase));
            _purchase = purchase ?? throw new ArgumentNullException(nameof(purchase));
        }

        public void Open()
        {
            if (_definition == null || _getAvailability == null)
                return;

            if (_root == null)
                Build();

            _root.SetActive(true);
            Refresh();
        }

        public void Close()
        {
            if (_root != null)
                _root.SetActive(false);
        }

        private void Update()
        {
            if (IsOpen && Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
                Close();
        }

        private void Refresh()
        {
            if (!IsOpen)
                return;

            string prerequisite = _definition.PrerequisiteMode == WorkshopUpgradePrerequisiteMode.CompletedContract &&
                                  _definition.PrerequisiteContract != null
                ? "Complete the customer request: " + _definition.PrerequisiteContract.DisplayName + "."
                : "No customer request prerequisite.";

            _details.text = _definition.DisplayName + "\n\n" + _definition.Description +
                "\n\nPRICE\n" + _definition.CleanCardCost + " clean CARD" +
                "\n\nREQUIREMENT\n" + prerequisite +
                "\n\nINSTALLATION\nRhea will arrange for it to be installed at your hideout workbench.";

            bool acquired = _isAcquired();
            _status.text = _getAvailability();
            _purchaseButton.interactable = !acquired && _canPurchase();
            _purchaseLabel.text = acquired
                ? "PURCHASED  /  INSTALLED"
                : "BUY & INSTALL  ·  " + _definition.CleanCardCost + " CARD";
        }

        private void Purchase()
        {
            _status.text = _purchase();
            Refresh();
        }

        private void Build()
        {
            EnsureEventSystem();
            _font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

            _root = new GameObject("WorkshopUpgradeCanvas", typeof(RectTransform), typeof(Canvas),
                typeof(CanvasScaler), typeof(GraphicRaycaster));
            _root.transform.SetParent(transform, false);
            Canvas canvas = _root.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 230;

            CanvasScaler scaler = _root.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);

            RectTransform backdrop = Panel(_root.transform, "Backdrop", Backdrop);
            Stretch(backdrop);
            RectTransform frame = Panel(backdrop, "SupplierOfferFrame", Frame);
            Center(frame, new Vector2(820f, 600f));

            Text title = MakeText(frame, "WORKSHOP OFFER", 28, Accent, TextAnchor.MiddleLeft);
            Anchor(title.rectTransform, new Vector2(32f, -28f), new Vector2(740f, 42f), true);

            Text supplier = MakeText(frame, "SUPPLIER  /  " + _supplierDisplayName, 16, Muted, TextAnchor.MiddleLeft);
            Anchor(supplier.rectTransform, new Vector2(34f, -72f), new Vector2(740f, 28f), true);

            _details = MakeText(frame, string.Empty, 19, Ink, TextAnchor.UpperLeft);
            Anchor(_details.rectTransform, new Vector2(36f, -122f), new Vector2(748f, 330f), true);

            _status = MakeText(frame, string.Empty, 16, Muted, TextAnchor.MiddleLeft);
            Anchor(_status.rectTransform, new Vector2(36f, 96f), new Vector2(748f, 46f), false);

            _purchaseButton = CreateButton(frame, "BUY & INSTALL", new Vector2(330f, 54f), Accent, 16);
            Anchor(_purchaseButton.GetComponent<RectTransform>(), new Vector2(30f, 28f),
                new Vector2(330f, 54f), false);
            _purchaseLabel = _purchaseButton.GetComponentInChildren<Text>();
            _purchaseButton.onClick.AddListener(Purchase);

            Button close = CreateButton(frame, "BACK TO RHEA  [ESC]", new Vector2(270f, 50f),
                new Color32(55, 63, 67, 255), 15);
            Anchor(close.GetComponent<RectTransform>(), new Vector2(30f, -28f),
                new Vector2(270f, 50f), true, true);
            _root.SetActive(false);
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

            new GameObject("WorkshopUpgradeEventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
        }

        private Text MakeText(Transform parent, string value, int size, Color color, TextAnchor alignment)
        {
            GameObject go = new GameObject("Text", typeof(RectTransform), typeof(Text));
            go.transform.SetParent(parent, false);
            Text text = go.GetComponent<Text>();
            text.font = _font != null ? _font : Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = size;
            text.color = color;
            text.alignment = alignment;
            text.text = value;
            text.raycastTarget = false;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Overflow;
            return text;
        }

        private Button CreateButton(Transform parent, string label, Vector2 size, Color color, int fontSize)
        {
            RectTransform rect = Panel(parent, "Button", color);
            rect.sizeDelta = size;
            Button button = rect.gameObject.AddComponent<Button>();
            button.targetGraphic = rect.GetComponent<Image>();
            Text text = MakeText(rect, label, fontSize, Color.black, TextAnchor.MiddleCenter);
            Stretch(text.rectTransform);
            return button;
        }

        private static RectTransform Panel(Transform parent, string objectName, Color color)
        {
            GameObject go = new GameObject(objectName, typeof(RectTransform), typeof(Image));
            go.transform.SetParent(parent, false);
            go.GetComponent<Image>().color = color;
            return go.GetComponent<RectTransform>();
        }

        private static void Stretch(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }

        private static void Center(RectTransform rect, Vector2 size)
        {
            rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = size;
        }

        private static void Anchor(RectTransform rect, Vector2 position, Vector2 size, bool topLeft, bool right = false)
        {
            if (right)
            {
                rect.anchorMin = rect.anchorMax = new Vector2(1f, topLeft ? 1f : 0f);
                rect.pivot = new Vector2(1f, topLeft ? 1f : 0f);
                rect.anchoredPosition = new Vector2(-position.x, position.y);
            }
            else
            {
                rect.anchorMin = rect.anchorMax = topLeft ? new Vector2(0f, 1f) : Vector2.zero;
                rect.pivot = rect.anchorMin;
                rect.anchoredPosition = position;
            }
            rect.sizeDelta = size;
        }
    }
}
