using System;
using Signal.Core;
using UnityEngine;
using UnityEngine.UI;

namespace Signal.Game
{
    /// <summary>Top-left status line, a short message line and a controls hint. Built from code, placeholder look.</summary>
    public sealed class HudPanel : MonoBehaviour
    {
        private static readonly Color StatusColor = new Color32(0xC9, 0xCE, 0xD6, 0xFF);
        private static readonly Color MessageColor = new Color32(0x3E, 0xE6, 0xD2, 0xFF);
        private static readonly Color HintColor = new Color32(0x6B, 0x72, 0x80, 0xFF);

        private PlayerStats _stats;
        private GameClock _clock;

        private Font _font;
        private Text _status;
        private Text _message;
        private Text _prompt;
        private float _messageUntil;
        private bool _built;

        public void Bind(PlayerStats stats, GameClock clock)
        {
            _stats = stats ?? throw new ArgumentNullException(nameof(stats));
            _clock = clock ?? throw new ArgumentNullException(nameof(clock));

            Build();
            Refresh();
        }

        public void SetPrompt(string text)
        {
            if (_prompt != null)
                _prompt.text = text ?? string.Empty;
        }

        public void ShowMessage(string text, float seconds = 3f)
        {
            if (_message == null)
                return;

            _message.text = text;
            _messageUntil = Time.unscaledTime + seconds;
        }

        private void Update()
        {
            if (!_built)
                return;

            Refresh();

            if (_message.text.Length > 0 && Time.unscaledTime > _messageUntil)
                _message.text = string.Empty;
        }

        private void Refresh()
        {
            _status.text = _clock.Label
                         + "    $" + _stats.Money
                         + "    HP " + _stats.Health;
        }

        private void Build()
        {
            if (_built)
                return;

            _font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (_font == null)
                _font = Font.CreateDynamicFontFromOSFont("Arial", 16);

            var canvasObject = new GameObject("HudCanvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler));
            canvasObject.transform.SetParent(transform, false);

            var canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 90;

            var scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ConstantPixelSize;
            scaler.scaleFactor = 1f;

            _status = NewText(canvasObject.transform, "Status", 18, TextAnchor.UpperLeft, StatusColor,
                              new Vector2(0f, 1f), new Vector2(12f, -10f), new Vector2(1400f, 30f));
            _message = NewText(canvasObject.transform, "Message", 20, TextAnchor.UpperLeft, MessageColor,
                               new Vector2(0f, 1f), new Vector2(12f, -42f), new Vector2(1400f, 30f));
            Text hint = NewText(canvasObject.transform, "Hint", 14, TextAnchor.LowerLeft, HintColor,
                                new Vector2(0f, 0f), new Vector2(12f, 10f), new Vector2(1600f, 24f));
            hint.text = "WASD move   E interact   Tab inventory   Esc close panels   F6 sleep at home   F5 save   F9 load";
            _prompt = NewText(canvasObject.transform, "InteractionPrompt", 20, TextAnchor.LowerCenter,
                              MessageColor, new Vector2(0.5f, 0f), new Vector2(0f, 52f), new Vector2(720f, 34f));

            _built = true;
        }

        // Anchored to a corner of the screen: the pivot is the same corner, so the offset points inwards.
        private Text NewText(Transform parent, string objectName, int size, TextAnchor alignment, Color color,
                             Vector2 corner, Vector2 offset, Vector2 area)
        {
            var go = new GameObject(objectName, typeof(RectTransform), typeof(Text));
            go.transform.SetParent(parent, false);

            var rect = go.GetComponent<RectTransform>();
            rect.anchorMin = corner;
            rect.anchorMax = corner;
            rect.pivot = corner;
            rect.anchoredPosition = offset;
            rect.sizeDelta = area;

            var text = go.GetComponent<Text>();
            text.font = _font;
            text.fontSize = size;
            text.alignment = alignment;
            text.color = color;
            text.text = string.Empty;
            text.raycastTarget = false;
            text.horizontalOverflow = HorizontalWrapMode.Overflow;
            text.verticalOverflow = VerticalWrapMode.Overflow;
            return text;
        }
    }
}
