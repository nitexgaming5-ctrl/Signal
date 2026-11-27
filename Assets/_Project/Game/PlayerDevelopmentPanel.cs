using System;
using System.Collections.Generic;
using Signal.Core;
using UnityEngine;

namespace Signal.Game
{
    /// <summary>Player-facing Character panel for attributes, skill allocation and respecialisation.</summary>
    public sealed class PlayerDevelopmentPanel : MonoBehaviour
    {
        private PlayerDevelopmentState _state;
        private PlayerDevelopmentCatalog _catalog;
        private Func<EffectiveCharacterAttributes> _effectiveAttributes;
        private PlayerStats _playerStats;
        private bool _isOpen;
        private Rect _window = new Rect(24f, 72f, 460f, 590f);

        public bool IsOpen => _isOpen;
        public event Action DevelopmentChanged;

        public void Bind(PlayerDevelopmentState state, PlayerDevelopmentCatalog catalog,
            Func<EffectiveCharacterAttributes> effectiveAttributes = null, PlayerStats playerStats = null)
        {
            _state = state;
            _catalog = catalog;
            _effectiveAttributes = effectiveAttributes;
            _playerStats = playerStats;
        }

        public void SetOpen(bool open) => _isOpen = open;

        private void OnGUI()
        {
            if (!_isOpen || _state == null || _catalog == null)
                return;
            _window = GUI.Window(73021, _window, DrawWindow, "CHARACTER  •  F2");
        }

        private void DrawWindow(int windowId)
        {
            EffectiveCharacterAttributes effective = _effectiveAttributes != null
                ? _effectiveAttributes()
                : CharacterAttributeCalculator.Calculate(_state.Attributes, 0, 0);

            GUILayout.Label("ATTRIBUTES");
            GUILayout.Label("Health  •  PRIMARY: " + (_playerStats != null ? _playerStats.Health : 0) +
                " / " + effective.Health + " current / maximum");
            GUILayout.Label("Armor: " + effective.Armor + "  (physical protection)");
            GUILayout.Label("Heat Protection: " + effective.HeatProtection + "  (cold exposure)");
            GUILayout.Space(10f);
            GUILayout.Label("SKILLS" + (HasProvisionalSkills() ? "  •  PROVISIONAL CATALOGUE" : string.Empty));
            GUILayout.Label("Unspent points: " + _state.Progression.UnspentPoints +
                " / " + _state.Progression.TotalPoints);
            GUILayout.Space(4f);

            foreach (PlayerSkillDefinition skill in _catalog.Skills)
            {
                if (skill == null || !_state.Skills.Contains(skill.Id))
                    continue;
                GUILayout.BeginVertical(GUI.skin.box);
                GUILayout.BeginHorizontal();
                GUILayout.Label(skill.DisplayName + "  Rank " + _state.Skills.GetRank(skill.Id),
                    GUILayout.Width(300f));
                if (GUILayout.Button("−", GUILayout.Width(34f)))
                    DecreaseRank(skill.Id);
                if (GUILayout.Button("+", GUILayout.Width(34f)) &&
                    _state.Progression.TrySpendRank(_state.Skills, skill.Id))
                    DevelopmentChanged?.Invoke();
                GUILayout.EndHorizontal();
                if (!string.IsNullOrWhiteSpace(skill.Description))
                    GUILayout.Label(skill.Description);
                GUILayout.EndVertical();
            }

            GUILayout.Space(8f);
            GUILayout.Label("Milestone points are awarded by completed gameplay events.");
            GUILayout.Label("Respecialization is free and returns every allocated point.");
            if (GUILayout.Button("Reset all skill allocations"))
            {
                _state.Progression.Respecialize(_state.Skills);
                DevelopmentChanged?.Invoke();
            }
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
                for (int i = 0; i < pair.Value; i++)
                    _state.Progression.TrySpendRank(_state.Skills, pair.Key);
            DevelopmentChanged?.Invoke();
        }
    }
}
