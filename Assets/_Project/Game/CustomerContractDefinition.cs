using System;
using Signal.Core;
using UnityEngine;

namespace Signal.Game
{
    public enum ContractGoodsEligibility
    {
        Unconfigured = 0,
        LegitimateOnly = 1,
        PlayerOwnedIncludingStolen = 2
    }

    public enum ContractDeliveryMode
    {
        Unconfigured = 0,
        WholeStackToIssuerInventory = 1
    }

    /// <summary>Authored offer data. Runtime acceptance and completion live in Signal.Core.</summary>
    [CreateAssetMenu(menuName = "Signal/Customer Contract", fileName = "CustomerContract")]
    public sealed class CustomerContractDefinition : ScriptableObject
    {
        [SerializeField, HideInInspector] private string id;
        [SerializeField] private string displayName;
        [SerializeField, TextArea(2, 5)] private string description;
        [SerializeField] private string issuerNpcId;
        [SerializeField] private ItemDefinition requiredItem;
        [SerializeField, Min(1)] private int requiredQuantity = 1;
        [SerializeField, Min(1)] private int cleanCardReward = 1;
        [SerializeField] private ContractGoodsEligibility goodsEligibility;
        [SerializeField] private ContractDeliveryMode deliveryMode;
        [Header("Relationship outcome")]
        [SerializeField, Range(-1f, 1f)] private float relationshipFamiliarityDelta;
        [SerializeField, Range(-1f, 1f)] private float relationshipTrustDelta;
        [SerializeField, Range(-1f, 1f)] private float relationshipAffinityDelta;

        public Guid Id => Guid.TryParse(id, out Guid value) ? value : Guid.Empty;
        public string DisplayName => displayName;
        public string Description => description;
        public string IssuerNpcId => issuerNpcId;
        public EconomicActorId IssuerActor => new EconomicActorId("resident:" + issuerNpcId);
        public ItemDefinition RequiredItem => requiredItem;
        public int RequiredQuantity => requiredQuantity;
        public int CleanCardReward => cleanCardReward;
        public ContractGoodsEligibility GoodsEligibility => goodsEligibility;
        public ContractDeliveryMode DeliveryMode => deliveryMode;
        public float RelationshipFamiliarityDelta => relationshipFamiliarityDelta;
        public float RelationshipTrustDelta => relationshipTrustDelta;
        public float RelationshipAffinityDelta => relationshipAffinityDelta;
        public bool AcceptStolenGoods => goodsEligibility == ContractGoodsEligibility.PlayerOwnedIncludingStolen;

        public bool TryValidate(out string error)
        {
            if (Id == Guid.Empty || string.IsNullOrWhiteSpace(displayName) ||
                string.IsNullOrWhiteSpace(issuerNpcId) || requiredItem == null ||
                requiredQuantity < 1 || cleanCardReward < 1)
            {
                error = "Assign a stable ID, display name, issuer, required item, positive quantity and positive CARD reward.";
                return false;
            }
            if (!ValidRelationshipDelta(relationshipFamiliarityDelta) ||
                !ValidRelationshipDelta(relationshipTrustDelta) ||
                !ValidRelationshipDelta(relationshipAffinityDelta))
            {
                error = "Relationship outcome deltas must be finite and between -1 and 1.";
                return false;
            }
            if (!Enum.IsDefined(typeof(ContractGoodsEligibility), goodsEligibility) ||
                goodsEligibility == ContractGoodsEligibility.Unconfigured)
            {
                error = "Choose whether legitimate-only or player-owned (including stolen) goods are accepted.";
                return false;
            }
            if (deliveryMode != ContractDeliveryMode.WholeStackToIssuerInventory)
            {
                error = "Select whole-stack delivery to the issuer's physical inventory.";
                return false;
            }
            try { requiredItem.ToItemType(); }
            catch (InvalidOperationException exception)
            {
                error = "The required item is invalid: " + exception.Message;
                return false;
            }
            error = null;
            return true;
        }

        private static bool ValidRelationshipDelta(float value)
        {
            return !float.IsNaN(value) && !float.IsInfinity(value) && value >= -1f && value <= 1f;
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