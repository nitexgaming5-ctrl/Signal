using System;
using System.Collections.Generic;
using UnityEngine;

namespace Signal.Game
{
    [Serializable]
    public sealed class PlayerSkillDefinition
    {
        [SerializeField] private string id;
        [SerializeField] private string displayName;
        [SerializeField, TextArea] private string description;
        [SerializeField] private bool provisional = true;

        public string Id => id;
        public string DisplayName => displayName;
        public string Description => description;
        public bool IsProvisional => provisional;

        public PlayerSkillDefinition(string id, string displayName, string description, bool provisional = true)
        {
            this.id = id;
            this.displayName = displayName;
            this.description = description;
            this.provisional = provisional;
        }
    }

    [CreateAssetMenu(menuName = "Signal/Player Development Catalog", fileName = "PlayerDevelopmentCatalog")]
    public sealed class PlayerDevelopmentCatalog : ScriptableObject
    {
        [SerializeField, Min(0)] private int startingSkillPoints = 8;
        [SerializeField, Min(1)] private int maximumSkillRank = 5;
        [SerializeField, Min(1)] private int pointsPerRank = 1;
        [SerializeField] private PlayerSkillDefinition[] skills = Array.Empty<PlayerSkillDefinition>();

        public int StartingSkillPoints => startingSkillPoints;
        public int MaximumSkillRank => maximumSkillRank;
        public int PointsPerRank => pointsPerRank;
        public IReadOnlyList<PlayerSkillDefinition> Skills => skills ?? Array.Empty<PlayerSkillDefinition>();

        public string[] GetSkillIds()
        {
            var ids = new List<string>();
            foreach (PlayerSkillDefinition skill in Skills)
                if (skill != null && !string.IsNullOrWhiteSpace(skill.Id) && !ids.Contains(skill.Id))
                    ids.Add(skill.Id);
            return ids.ToArray();
        }

        public static PlayerDevelopmentCatalog CreatePrototypeDefaults()
        {
            var catalog = CreateInstance<PlayerDevelopmentCatalog>();
            catalog.startingSkillPoints = 8;
            catalog.maximumSkillRank = 5;
            catalog.pointsPerRank = 1;
            catalog.skills = new[]
            {
                new PlayerSkillDefinition("repair", "Repair", "Repair and restore entrusted or owned items."),
                new PlayerSkillDefinition("production", "Production", "Improve practical production work."),
                new PlayerSkillDefinition("trading", "Trading", "Improve buying, selling and commercial work."),
                new PlayerSkillDefinition("scavenging", "Scavenging", "Improve finding and evaluating useful goods."),
                new PlayerSkillDefinition("social", "Social", "Improve social and customer-facing activities.")
            };
            return catalog;
        }
    }
}
