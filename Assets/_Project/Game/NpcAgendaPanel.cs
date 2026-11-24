using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Signal.Game
{
    /// <summary>Reusable, keyboard- and mouse-driven presentation for an authored agenda stage.</summary>
    public sealed class NpcAgendaPanel : MonoBehaviour
    {
        private NpcAgendaInteraction _interaction;
        private Action<NpcAgendaInteraction, string> _selectChoice;
        private bool _open;

        public bool IsOpen => _open;

        public void Bind(Action<NpcAgendaInteraction, string> selectChoice)
        {
            _selectChoice = selectChoice ?? throw new ArgumentNullException(nameof(selectChoice));
        }

        public void Open(NpcAgendaInteraction interaction)
        {
            if (interaction.Stage == null) return;
            _interaction = interaction;
            _open = true;
        }

        public void Close()
        {
            _open = false;
        }

        private void Update()
        {
            if (!_open || Keyboard.current == null) return;
            if (Keyboard.current.escapeKey.wasPressedThisFrame)
            {
                Close();
                return;
            }

            NpcAgendaChoiceDefinition[] choices = _interaction.Stage.Choices;
            if (Keyboard.current.digit1Key.wasPressedThisFrame) SelectAt(0, choices);
            else if (Keyboard.current.digit2Key.wasPressedThisFrame) SelectAt(1, choices);
            else if (Keyboard.current.digit3Key.wasPressedThisFrame) SelectAt(2, choices);
            else if (Keyboard.current.digit4Key.wasPressedThisFrame) SelectAt(3, choices);
        }

        private void OnGUI()
        {
            if (!_open || _interaction.Stage == null) return;

            const float width = 720f;
            const float height = 470f;
            Rect area = new Rect((Screen.width - width) * 0.5f,
                (Screen.height - height) * 0.5f, width, height);
            GUILayout.BeginArea(area, GUI.skin.box);
            GUILayout.Space(16f);
            var titleStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 22,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleLeft
            };
            GUILayout.Label(_interaction.Stage.Title, titleStyle, GUILayout.Height(34f));
            GUILayout.Space(10f);
            var dialogueStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 18,
                wordWrap = true,
                alignment = TextAnchor.UpperLeft
            };
            GUILayout.Label(_interaction.Stage.Dialogue, dialogueStyle, GUILayout.MinHeight(115f));
            GUILayout.Space(18f);

            NpcAgendaChoiceDefinition[] choices = _interaction.Stage.Choices;
            for (int i = 0; i < choices.Length; i++)
            {
                NpcAgendaChoiceDefinition choice = choices[i];
                if (choice == null) continue;
                if (GUILayout.Button("[" + (i + 1) + "]  " + choice.Label, GUILayout.Height(48f)))
                    Select(choice.ChoiceId);
                GUILayout.Space(7f);
            }

            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Leave conversation  [ESC]", GUILayout.Height(38f)))
                Close();
            GUILayout.EndArea();
        }

        private void SelectAt(int index, NpcAgendaChoiceDefinition[] choices)
        {
            if (index < 0 || index >= choices.Length || choices[index] == null) return;
            Select(choices[index].ChoiceId);
        }

        private void Select(string choiceId)
        {
            if (_open)
                _selectChoice?.Invoke(_interaction, choiceId);
        }
    }
}
