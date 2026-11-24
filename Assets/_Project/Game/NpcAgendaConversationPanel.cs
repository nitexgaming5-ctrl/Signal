using System;
using Signal.Core;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

namespace Signal.Game
{
    /// <summary>Reusable presentation for authored NPC agenda choices.</summary>
    public sealed class NpcAgendaConversationPanel : MonoBehaviour
    {
        private static readonly Color Backdrop = new Color32(8, 11, 15, 238);
        private static readonly Color Frame = new Color32(27, 33, 39, 255);
        private static readonly Color Accent = new Color32(200, 162, 74, 255);
        private static readonly Color Ink = new Color32(235, 226, 207, 255);
        private static readonly Color Muted = new Color32(170, 178, 184, 255);

        private GameObject _root;
        private Font _font;
        private Action<string> _choose;
        private Action _closed;
        private Text _title;
        private Text _dialogue;
        private RectTransform _choiceRoot;
        private string _npcId;
        private string _threadId;
        private string _stageId;

        public bool IsOpen => _root != null && _root.activeSelf;

        public void Bind(Action<string, string, string, string> choose, Action closed = null)
        {
            if (choose == null) throw new ArgumentNullException(nameof(choose));
            _choose = choiceId => choose(_npcId, _threadId, _stageId, choiceId);
            _closed = closed;
        }

        public void Open(string npcId, NpcAgendaThreadDefinition thread,
            NpcAgendaStageDefinition stage)
        {
            if (string.IsNullOrWhiteSpace(npcId) || thread == null || stage == null)
                return;
            if (_root == null) Build();

            _npcId = npcId;
            _threadId = thread.ThreadId;
            _stageId = stage.StageId;
            _title.text = string.IsNullOrWhiteSpace(stage.Title) ? thread.DisplayName : stage.Title;
            _dialogue.text = stage.Dialogue;
            ClearChoices();
            for (int i = 0; i < stage.Choices.Length; i++)
            {
                NpcAgendaChoiceDefinition choice = stage.Choices[i];
                if (choice == null) continue;
                Button button = CreateButton(_choiceRoot, choice.Label);
                string choiceId = choice.ChoiceId;
                button.onClick.AddListener(() => _choose?.Invoke(choiceId));
            }
            _root.SetActive(true);
        }

        public void Close()
        {
            if (!IsOpen) return;
            _root.SetActive(false);
            _closed?.Invoke();
        }

        private void Update()
        {
            if (IsOpen && Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
                Close();
        }

        private void Build()
        {
            EventSystem current = EventSystem.current;
            if (current == null)
                new GameObject("NpcAgendaEventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
            else if (current.GetComponent<InputSystemUIInputModule>() == null)
                current.gameObject.AddComponent<InputSystemUIInputModule>();

            _font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            _root = new GameObject("NpcAgendaConversationCanvas", typeof(RectTransform), typeof(Canvas),
                typeof(CanvasScaler), typeof(GraphicRaycaster));
            _root.transform.SetParent(transform, false);
            Canvas canvas = _root.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 240;
            CanvasScaler scaler = _root.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);

            RectTransform backdrop = Panel(_root.transform, "Backdrop", Backdrop);
            Stretch(backdrop);
            RectTransform frame = Panel(backdrop, "ConversationFrame", Frame);
            Center(frame, new Vector2(900f, 650f));

            _title = MakeText(frame, 28, Accent, TextAnchor.MiddleLeft);
            Anchor(_title.rectTransform, new Vector2(36f, -30f), new Vector2(820f, 44f), true);
            _dialogue = MakeText(frame, 21, Ink, TextAnchor.UpperLeft);
            Anchor(_dialogue.rectTransform, new Vector2(38f, -100f), new Vector2(820f, 250f), true);

            GameObject choices = new GameObject("Choices", typeof(RectTransform), typeof(VerticalLayoutGroup));
            choices.transform.SetParent(frame, false);
            _choiceRoot = choices.GetComponent<RectTransform>();
            _choiceRoot.anchorMin = new Vector2(0f, 0f);
            _choiceRoot.anchorMax = new Vector2(1f, 0f);
            _choiceRoot.pivot = new Vector2(0.5f, 0f);
            _choiceRoot.anchoredPosition = new Vector2(0f, 92f);
            _choiceRoot.sizeDelta = new Vector2(-76f, 260f);
            VerticalLayoutGroup layout = choices.GetComponent<VerticalLayoutGroup>();
            layout.spacing = 10f;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;

            Button close = CreateButton(frame, "CLOSE  [ESC]");
            RectTransform closeRect = close.GetComponent<RectTransform>();
            closeRect.anchorMin = closeRect.anchorMax = new Vector2(1f, 0f);
            closeRect.pivot = new Vector2(1f, 0f);
            closeRect.anchoredPosition = new Vector2(-36f, 28f);
            closeRect.sizeDelta = new Vector2(220f, 48f);
            close.onClick.AddListener(Close);
            _root.SetActive(false);
        }

        private void ClearChoices()
        {
            for (int i = _choiceRoot.childCount - 1; i >= 0; i--)
                Destroy(_choiceRoot.GetChild(i).gameObject);
        }

        private Button CreateButton(Transform parent, string label)
        {
            RectTransform rect = Panel(parent, "Choice", new Color32(200, 162, 74, 255));
            rect.sizeDelta = new Vector2(0f, 54f);
            Button button = rect.gameObject.AddComponent<Button>();
            button.targetGraphic = rect.GetComponent<Image>();
            Text text = MakeText(rect, 16, Color.black, TextAnchor.MiddleCenter);
            text.text = label;
            Stretch(text.rectTransform);
            return button;
        }

        private Text MakeText(Transform parent, int size, Color color, TextAnchor alignment)
        {
            GameObject go = new GameObject("Text", typeof(RectTransform), typeof(Text));
            go.transform.SetParent(parent, false);
            Text text = go.GetComponent<Text>();
            text.font = _font;
            text.fontSize = size;
            text.color = color;
            text.alignment = alignment;
            text.raycastTarget = false;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Overflow;
            return text;
        }

        private static RectTransform Panel(Transform parent, string name, Color color)
        {
            GameObject go = new GameObject(name, typeof(RectTransform), typeof(Image));
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

        private static void Anchor(RectTransform rect, Vector2 position, Vector2 size, bool topLeft)
        {
            rect.anchorMin = rect.anchorMax = topLeft ? new Vector2(0f, 1f) : Vector2.zero;
            rect.pivot = rect.anchorMin;
            rect.anchoredPosition = position;
            rect.sizeDelta = size;
        }
    }
}
