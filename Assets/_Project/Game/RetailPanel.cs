using System;
using System.Text;
using Signal.Core;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace Signal.Game
{
    /// <summary>Compact keyboard-driven catalogue panel shared by all authored businesses.</summary>
    public sealed class RetailPanel : MonoBehaviour
    {
        private static readonly Color PanelColor = new Color32(28, 31, 36, 245);
        private static readonly Color TextColor = new Color32(224, 226, 230, 255);
        private static readonly Color AccentColor = new Color32(62, 230, 210, 255);

        private Font _font;
        private GameObject _root;
        private Text _title;
        private Text _catalogue;
        private Text _status;
        private Text _footer;
        private BusinessDefinition _business;
        private Func<BusinessDefinition, ItemDefinition, string> _purchase;
        private Action _closed;

        public bool IsOpen => _root != null && _root.activeSelf;

        public void Bind(Func<BusinessDefinition, ItemDefinition, string> purchase, Action closed)
        {
            _purchase = purchase ?? throw new ArgumentNullException(nameof(purchase));
            _closed = closed;
            BuildIfNeeded();
            _root.SetActive(false);
        }

        public void Open(BusinessDefinition business)
        {
            if (business == null)
                return;
            BuildIfNeeded();
            _business = business;
            _title.text = business.DisplayName.ToUpperInvariant() + "  —  " + business.BusinessType;
            RefreshCatalogue();
            _status.text = string.Empty;
            _root.SetActive(true);
        }

        public void Close()
        {
            if (!IsOpen)
                return;
            _root.SetActive(false);
            _business = null;
            _closed?.Invoke();
        }

        private void Update()
        {
            if (!IsOpen)
                return;

            Keyboard keyboard = Keyboard.current;
            if (keyboard == null)
                return;
            if (keyboard.escapeKey.wasPressedThisFrame)
            {
                Close();
                return;
            }

            int selected = keyboard.digit1Key.wasPressedThisFrame ? 0 :
                keyboard.digit2Key.wasPressedThisFrame ? 1 :
                keyboard.digit3Key.wasPressedThisFrame ? 2 :
                keyboard.digit4Key.wasPressedThisFrame ? 3 :
                keyboard.digit5Key.wasPressedThisFrame ? 4 :
                keyboard.digit6Key.wasPressedThisFrame ? 5 :
                keyboard.digit7Key.wasPressedThisFrame ? 6 :
                keyboard.digit8Key.wasPressedThisFrame ? 7 :
                keyboard.digit9Key.wasPressedThisFrame ? 8 : -1;

            if (selected < 0 || _business == null || selected >= _business.Merchandise.Count)
                return;

            BusinessDefinition.MerchandiseEntry entry = _business.Merchandise[selected];
            if (entry == null || entry.Item == null)
            {
                _status.text = "This catalogue entry is not configured.";
                return;
            }

            _status.text = _purchase(_business, entry.Item);
        }

        private void RefreshCatalogue()
        {
            var builder = new StringBuilder();
            if (_business.Merchandise.Count == 0)
            {
                builder.AppendLine("No merchandise has been authored for this business.");
            }
            else
            {
                int shown = Math.Min(9, _business.Merchandise.Count);
                for (int i = 0; i < shown; i++)
                {
                    BusinessDefinition.MerchandiseEntry entry = _business.Merchandise[i];
                    if (entry == null || entry.Item == null)
                        builder.AppendLine((i + 1) + ". [Unconfigured item]");
                    else
                        builder.AppendLine((i + 1) + ". " + entry.Item.DisplayName + "    $" + entry.UnitPrice);
                }
                if (_business.Merchandise.Count > 9)
                    builder.AppendLine("More catalogue entries are hidden; the first nine are selectable.");
            }
            _catalogue.text = builder.ToString();
        }

        private void BuildIfNeeded()
        {
            if (_root != null)
                return;

            _font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (_font == null)
                _font = Font.CreateDynamicFontFromOSFont("Arial", 16);

            var canvasObject = new GameObject("RetailCanvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler));
            canvasObject.transform.SetParent(transform, false);
            Canvas canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 120;
            CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ConstantPixelSize;
            scaler.scaleFactor = 1f;

            var panelObject = new GameObject("RetailPanel", typeof(RectTransform), typeof(Image));
            panelObject.transform.SetParent(canvasObject.transform, false);
            Image background = panelObject.GetComponent<Image>();
            background.color = PanelColor;
            _root = panelObject;
            RectTransform rootRect = panelObject.GetComponent<RectTransform>();
            rootRect.anchorMin = rootRect.anchorMax = new Vector2(0.5f, 0.5f);
            rootRect.pivot = new Vector2(0.5f, 0.5f);
            rootRect.sizeDelta = new Vector2(620f, 390f);

            _title = NewText("Title", rootRect, 22, AccentColor, TextAnchor.MiddleCenter);
            SetRect(_title.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
                new Vector2(0f, -28f), new Vector2(580f, 40f));

            _catalogue = NewText("Catalogue", rootRect, 20, TextColor, TextAnchor.UpperLeft);
            SetRect(_catalogue.rectTransform, new Vector2(0f, 1f), new Vector2(1f, 1f),
                new Vector2(28f, -86f), new Vector2(-56f, 205f));

            _status = NewText("Status", rootRect, 17, AccentColor, TextAnchor.MiddleCenter);
            SetRect(_status.rectTransform, new Vector2(0f, 0f), new Vector2(1f, 0f),
                new Vector2(0f, 54f), new Vector2(-48f, 34f));

            _footer = NewText("Footer", rootRect, 15, TextColor, TextAnchor.MiddleCenter);
            _footer.text = "Press 1–9 to purchase one item   •   ESC to close";
            SetRect(_footer.rectTransform, new Vector2(0f, 0f), new Vector2(1f, 0f),
                new Vector2(0f, 20f), new Vector2(-48f, 28f));
        }

        private Text NewText(string objectName, Transform parent, int size, Color color, TextAnchor alignment)
        {
            var go = new GameObject(objectName, typeof(RectTransform), typeof(Text));
            go.transform.SetParent(parent, false);
            Text text = go.GetComponent<Text>();
            text.font = _font;
            text.fontSize = size;
            text.color = color;
            text.alignment = alignment;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Overflow;
            text.raycastTarget = false;
            return text;
        }

        private static void SetRect(RectTransform rect, Vector2 anchorMin, Vector2 anchorMax,
            Vector2 position, Vector2 size)
        {
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = position;
            rect.sizeDelta = size;
        }
    }
}
