using System;
using Signal.Core;
using UnityEngine;

namespace Signal.Game
{
    [CreateAssetMenu(menuName = "Signal/Item Definition", fileName = "ItemDefinition")]
    public sealed class ItemDefinition : ScriptableObject
    {
        [SerializeField] private string displayName;
        [SerializeField, TextArea(2, 5)] private string description;
        [SerializeField] private string category = "General";
        [SerializeField, Min(0)] private int baseValue = 10;
        [SerializeField] private Sprite icon;
        [Header("Default instance state")]
        [SerializeField] private ItemQuality defaultQuality = ItemQuality.Standard;
        [SerializeField] private ItemCondition defaultCondition = ItemCondition.Good;
        [SerializeField] private ItemAuthenticity defaultAuthenticity = ItemAuthenticity.Genuine;
        [SerializeField] private ItemOwnershipStatus defaultOwnership = ItemOwnershipStatus.Legitimate;
        [SerializeField, HideInInspector] private string id;
        [SerializeField, Min(1)] private int gridWidth = 1;
        [SerializeField, Min(1)] private int gridHeight = 1;
        [SerializeField, Min(1)] private int maxStack = 1;

        public string DisplayName => displayName;
        public string Description => description;
        public string Category => category;
        public int BaseValue => Mathf.Max(0, baseValue);
        public Sprite Icon => icon;
        public ItemInstanceState DefaultInstanceState => new ItemInstanceState(
            defaultQuality, defaultCondition, defaultAuthenticity, defaultOwnership);

        public ItemType ToItemType()
        {
            if (!Guid.TryParse(id, out Guid guid) || guid == Guid.Empty)
            {
                throw new InvalidOperationException(
                    "ItemDefinition '" + name + "' has no valid id. Re-create the asset or select it in the Inspector so an id is generated.");
            }

            return new ItemType(guid, gridWidth, gridHeight, maxStack);
        }

        private void Reset()
        {
            EnsureId();
        }

        private void OnValidate()
        {
            EnsureId();
        }

        // The id is generated once and never replaced, because saves refer to it.
        private void EnsureId()
        {
            if (!string.IsNullOrEmpty(id))
                return;

            id = Guid.NewGuid().ToString();
#if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(this);
#endif
        }
    }
}
