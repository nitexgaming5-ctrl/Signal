using System;
using Signal.Core;
using UnityEngine;

namespace Signal.Game
{
    /// <summary>Authored first customer repair order. Runtime custody and payment live in Core.</summary>
    [CreateAssetMenu(menuName = "Signal/Customer Repair Order", fileName = "CustomerRepairOrder")]
    public sealed class CustomerRepairOrderDefinition : ScriptableObject
    {
        [SerializeField, HideInInspector] private string id;
        [SerializeField] private string displayName;
        [SerializeField, TextArea(2, 5)] private string description;
        [SerializeField] private string issuerNpcId;
        [SerializeField] private ItemDefinition entrustedItem;
        [SerializeField, Min(1)] private int cleanCardReward = 30;
        [SerializeField] private ItemCondition startingCondition = ItemCondition.Damaged;
        [SerializeField] private ItemCondition requiredReturnCondition = ItemCondition.Good;

        public Guid Id => Guid.TryParse(id, out Guid value) ? value : Guid.Empty;
        public string DisplayName => displayName;
        public string Description => description;
        public string IssuerNpcId => issuerNpcId;
        public EconomicActorId IssuerActor => new EconomicActorId("resident:" + issuerNpcId);
        public ItemDefinition EntrustedItem => entrustedItem;
        public int CleanCardReward => cleanCardReward;
        public ItemCondition StartingCondition => startingCondition;
        public ItemCondition RequiredReturnCondition => requiredReturnCondition;

        public bool TryValidate(out string error)
        {
            if (Id == Guid.Empty || string.IsNullOrWhiteSpace(displayName) ||
                string.IsNullOrWhiteSpace(issuerNpcId) || entrustedItem == null || cleanCardReward < 1 ||
                !Enum.IsDefined(typeof(ItemCondition), startingCondition) ||
                !Enum.IsDefined(typeof(ItemCondition), requiredReturnCondition) ||
                (startingCondition != ItemCondition.Damaged && startingCondition != ItemCondition.Worn) ||
                requiredReturnCondition != ItemCondition.Good)
            {
                error = "Assign a stable ID, display name, issuer and single-instance item, a positive CARD reward, a Worn or Damaged starting condition, and a Good return condition.";
                return false;
            }
            try
            {
                if (entrustedItem.ToItemType().MaxStack != 1)
                {
                    error = "The entrusted repair item must have a maximum stack size of one.";
                    return false;
                }
            }
            catch (InvalidOperationException exception)
            {
                error = "The entrusted item is invalid: " + exception.Message;
                return false;
            }
            error = null;
            return true;
        }

        private void Reset() => EnsureId();
        private void OnValidate() => EnsureId();
        private void EnsureId()
        {
            if (!string.IsNullOrEmpty(id)) return;
            id = Guid.NewGuid().ToString();
#if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(this);
#endif
        }
    }
}