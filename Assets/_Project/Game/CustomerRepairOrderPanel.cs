using System;
using Signal.Core;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

namespace Signal.Game
{
    /// <summary>Customer repair-order UI; custody, repair and payment rules remain in Core.</summary>
    public sealed class CustomerRepairOrderPanel : MonoBehaviour
    {
        private static readonly Color Backdrop = new Color32(8, 11, 15, 238);
        private static readonly Color Frame = new Color32(27, 33, 39, 255);
        private static readonly Color Accent = new Color32(62, 230, 210, 255);
        private static readonly Color Ink = new Color32(235, 226, 207, 255);
        private static readonly Color Muted = new Color32(170, 178, 184, 255);

        private CustomerRepairOrderDefinition _definition;
        private string _issuerName;
        private Func<string> _accept;
        private Func<string> _return;
        private Func<CustomerRepairOrderPhase> _phase;
        private GameObject _root;
        private Text _details;
        private Text _status;
        private Button _acceptButton;
        private Button _returnButton;
        private bool _built;

        public bool IsOpen => _root != null && _root.activeSelf;
        public string ActiveIssuerNpcId => IsOpen && _definition != null ? _definition.IssuerNpcId : string.Empty;

        public void Bind(CustomerRepairOrderDefinition definition, string issuerName,
            Func<string> accept, Func<string> returnItem, Func<CustomerRepairOrderPhase> phase)
        {
            _definition = definition ?? throw new ArgumentNullException(nameof(definition));
            _issuerName = string.IsNullOrWhiteSpace(issuerName) ? definition.IssuerNpcId : issuerName;
            _accept = accept ?? throw new ArgumentNullException(nameof(accept));
            _return = returnItem ?? throw new ArgumentNullException(nameof(returnItem));
            _phase = phase ?? throw new ArgumentNullException(nameof(phase));
            BuildIfNeeded();
            Close();
        }

        public void Open()
        {
            BuildIfNeeded();
            _status.text = string.Empty;
            _root.SetActive(true);
            Refresh();
        }

        public void Close()
        {
            if (_root != null) _root.SetActive(false);
        }

        private void Update()
        {
            if (IsOpen && Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
                Close();
        }

        private void Refresh()
        {
            if (!IsOpen) return;
            CustomerRepairOrderPhase phase = _phase();
            _details.text = _definition.DisplayName + "\n\n" + _definition.Description +
                "\n\nITEM  /  " + _definition.EntrustedItem.DisplayName +
                "\nREWARD  /  " + _definition.CleanCardReward + " clean CARD" +
                "\nRETURN CONDITION  /  " + _definition.RequiredReturnCondition +
                "\n\nThe item remains " + _issuerName + "'s property while in your custody.";
            _acceptButton.gameObject.SetActive(phase == CustomerRepairOrderPhase.Offered);
            _returnButton.gameObject.SetActive(phase == CustomerRepairOrderPhase.Active);
            _acceptButton.interactable = phase == CustomerRepairOrderPhase.Offered;
            _returnButton.interactable = phase == CustomerRepairOrderPhase.Active;
            if (phase == CustomerRepairOrderPhase.Completed && string.IsNullOrEmpty(_status.text))
                _status.text = "Order completed. The exact jacket was returned and the CARD payment was recorded.";
            else if (phase == CustomerRepairOrderPhase.Active && string.IsNullOrEmpty(_status.text))
                _status.text = "Take the jacket to your hideout workbench, restore it, then return here.";
            else if (phase == CustomerRepairOrderPhase.Offered && string.IsNullOrEmpty(_status.text))
                _status.text = "Accept the entrusted item to begin the repair.";
        }

        private void BuildIfNeeded()
        {
            if (_built) return;
            EnsureEventSystem();
            _root = new GameObject("CustomerRepairOrderCanvas", typeof(RectTransform), typeof(Canvas),
                typeof(CanvasScaler), typeof(GraphicRaycaster));
            _root.transform.SetParent(transform, false);
            Canvas canvas = _root.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 225;
            CanvasScaler scaler = _root.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);

            RectTransform backdrop = Panel(_root.transform, "Backdrop", Backdrop);
            Stretch(backdrop);
            RectTransform frame = Panel(backdrop, "RepairOrderFrame", Frame);
            Center(frame, new Vector2(760f, 560f));

            Text title = MakeText(frame, "CUSTOMER REPAIR ORDER", 27, Accent, TextAnchor.MiddleLeft);
            Anchor(title.rectTransform, new Vector2(30f, -24f), new Vector2(690f, 42f), true);
            Text issuer = MakeText(frame, "ISSUER  /  " + _issuerName, 16, Muted, TextAnchor.MiddleLeft);
            Anchor(issuer.rectTransform, new Vector2(32f, -68f), new Vector2(690f, 28f), true);

            _details = MakeText(frame, string.Empty, 20, Ink, TextAnchor.UpperLeft);
            Anchor(_details.rectTransform, new Vector2(34f, -112f), new Vector2(680f, 310f), true);
            _status = MakeText(frame, string.Empty, 16, Muted, TextAnchor.MiddleLeft);
            Anchor(_status.rectTransform, new Vector2(32f, 76f), new Vector2(690f, 52f), false);

            _acceptButton = CreateButton(frame, "ACCEPT ENTRUSTED JACKET", new Vector2(300f, 52f));
            Anchor(_acceptButton.GetComponent<RectTransform>(), new Vector2(28f, 18f), new Vector2(300f, 52f), false);
            _acceptButton.onClick.AddListener(() =>
            {
                _status.text = _accept();
                Refresh();
            });

            _returnButton = CreateButton(frame, "RETURN JACKET  /  RECEIVE PAYMENT", new Vector2(360f, 52f));
            Anchor(_returnButton.GetComponent<RectTransform>(), new Vector2(28f, 18f), new Vector2(360f, 52f), false);
            _returnButton.onClick.AddListener(() =>
            {
                _status.text = _return();
                Refresh();
            });

            Button close = CreateButton(frame, "CLOSE  [ESC]", new Vector2(190f, 48f));
            Anchor(close.GetComponent<RectTransform>(), new Vector2(24f, -24f), new Vector2(190f, 48f), true, true);
            close.onClick.AddListener(Close);
            _root.SetActive(false);
            _built = true;
        }

        private static void EnsureEventSystem()
        {
            EventSystem current = EventSystem.current;
            if (current != null)
            {
                if (current.GetComponent<InputSystemUIInputModule>() == null)
                    current.gameObject.AddComponent<InputSystemUIInputModule>();
                return;
            }
            new GameObject("CustomerRepairOrderEventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
        }

        private static RectTransform Panel(Transform parent, string name, Color color)
        {
            GameObject go = new GameObject(name, typeof(RectTransform), typeof(Image));
            go.transform.SetParent(parent, false);
            go.GetComponent<Image>().color = color;
            return go.GetComponent<RectTransform>();
        }

        private static Text MakeText(Transform parent, string value, int size, Color color, TextAnchor alignment)
        {
            GameObject go = new GameObject("Text", typeof(RectTransform), typeof(Text));
            go.transform.SetParent(parent, false);
            Text text = go.GetComponent<Text>();
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
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
            RectTransform rect = Panel(parent, "Button", new Color32(55, 63, 67, 255));
            rect.sizeDelta = size;
            Button button = rect.gameObject.AddComponent<Button>();
            button.targetGraphic = rect.GetComponent<Image>();
            Text text = MakeText(rect, label, 15, Ink, TextAnchor.MiddleCenter);
            Stretch(text.rectTransform);
            return button;
        }

        private static void Stretch(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero; rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero; rect.offsetMax = Vector2.zero;
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