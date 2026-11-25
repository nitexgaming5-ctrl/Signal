using System.Collections.Generic;
using Signal.Core;
using UnityEngine;

namespace Signal.Game
{
    /// <summary>Compact F2 debug panel for inspecting and testing player development during the slice.</summary>
    public sealed class PlayerDevelopmentPanel : MonoBehaviour
    {
        private PlayerDevelopmentState _state;
        private PlayerDevelopmentCatalog _catalog;
        private bool _isOpen;
        private Rect _window = new Rect(24f, 72f, 390f, 470f);

        public bool IsOpen => _isOpen;

        public void Bind(PlayerDevelopmentState state, PlayerDevelopmentCatalog catalog)
        {
            _state = state;
            _catalog = catalog;
        }

        public void SetOpen(bool open) => _isOpen = open;

        private void OnGUI()
        {
            if (!_isOpen || _state == null || _catalog == null)
                return;
            _window = GUI.Window(73021, _window, DrawWindow, "PLAYER DEVELOPMENT  •  F2");
        }

        private void DrawWindow(int windowId)
        {
            GUILayout.Label("ATTRIBUTES");
            GUILayout.Label("Armor: " + _state.Attributes.Armor);
            GUILayout.Label("Heat Protection: " + _state.Attributes.HeatProtection + "  (cold exposure)");
            GUILayout.Space(8f);
            GUILayout.Label("SKILLS" + (HasProvisionalSkills() ? "  •  PROVISIONAL CATALOGUE" : string.Empty));
            GUILayout.Label("Unspent points: " + _state.Progression.UnspentPoints +
                " / " + _state.Progression.TotalPoints);
            GUILayout.Space(4f);

            foreach (PlayerSkillDefinition skill in _catalog.Skills)
            {
                if (skill == null || !_state.Skills.Contains(skill.Id))
                    continue;
                GUILayout.BeginHorizontal();
                GUILayout.Label(skill.DisplayName + "  Rank " + _state.Skills.GetRank(skill.Id),
                    GUILayout.Width(215f));
                if (GUILayout.Button("−", GUILayout.Width(34f)))
                    DecreaseRank(skill.Id);
                if (GUILayout.Button("+", GUILayout.Width(34f)))
                    _state.Progression.TrySpendRank(_state.Skills, skill.Id);
                GUILayout.EndHorizontal();
            }

            GUILayout.Space(8f);
            GUILayout.Label("Milestone points are awarded by completed gameplay events.");
            GUILayout.Label("Respecialization is free and returns every allocated point.");
            if (GUILayout.Button("Reset all skill allocations"))
                _state.Progression.Respecialize(_state.Skills);
            if (GUILayout.Button("Close"))
                _isOpen = false;

            GUI.DragWindow(new Rect(0f, 0f, 10000f, 24f));
        }

        private bool HasProvisionalSkills()
        {
            foreach (PlayerSkillDefinition skill in _catalog.Skills)
                if (skill != null && skill.IsProvisional)
                    return true;
            return false;
        }

        private void DecreaseRank(string skillId)
        {
            var ranks = new Dictionary<string, int>();
            foreach (KeyValuePair<string, int> pair in _state.Skills.Ranks)
                ranks[pair.Key] = pair.Value;
            if (!ranks.TryGetValue(skillId, out int current) || current <= 0)
                return;

            ranks[skillId] = current - 1;
            _state.Progression.Respecialize(_state.Skills);
            foreach (KeyValuePair<string, int> pair in ranks)
            {
                for (int i = 0; i < pair.Value; i++)
                    _state.Progression.TrySpendRank(_state.Skills, pair.Key);
            }
        }
    }
}
