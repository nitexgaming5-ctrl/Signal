using Signal.Core;
using UnityEngine;

namespace Signal.Game
{
    /// <summary>Authored content for one discreet contact's fixed fence and cleaning offer.</summary>
    [CreateAssetMenu(menuName = "Signal/Dirty Cash Contact Offer", fileName = "DirtyCashContactOffer")]
    public sealed class DirtyCashContactOfferDefinition : ScriptableObject
    {
        [SerializeField] private NpcCharacterDefinition contact;
        [SerializeField] private ItemDefinition dirtyCashItem;
        [SerializeField] private ItemDefinition acceptedItem;
        [SerializeField, Min(1)] private int acceptedQuantity = 2;
        [SerializeField, Min(1)] private int dirtyCashUnits = 3;
        [SerializeField, Range(0, 99)] private int cleaningFeePercent = 20;

        public NpcCharacterDefinition Contact => contact;
        public ItemDefinition DirtyCashItem => dirtyCashItem;
        public ItemDefinition AcceptedItem => acceptedItem;
        public int AcceptedQuantity => acceptedQuantity;
        public int DirtyCashUnits => dirtyCashUnits;
        public int CleaningFeePercent => cleaningFeePercent;

        public bool TryValidate(out string error)
        {
            error = null;
            if (contact == null || contact.NpcId != "Clean_Dirtycash" ||
                !string.IsNullOrWhiteSpace(contact.DisplayName))
            {
                error = "The offer must reference the blank-name Clean_Dirtycash resident.";
                return false;
            }
            if (dirtyCashItem == null || acceptedItem == null || dirtyCashItem == acceptedItem)
            {
                error = "The offer requires distinct accepted-item and Dirty Cash definitions.";
                return false;
            }
            if (acceptedQuantity < 1 || acceptedQuantity > acceptedItem.ToItemType().MaxStack ||
                dirtyCashUnits < 1 || dirtyCashUnits > dirtyCashItem.ToItemType().MaxStack ||
                dirtyCashItem.BaseValue != DirtyCashRules.FaceValuePerUnit ||
                !DirtyCashRules.TryGetCleaningAmounts(dirtyCashUnits, cleaningFeePercent, out _, out _))
            {
                error = "The offer has an invalid quantity, denomination or cleaning fee.";
                return false;
            }
            return true;
        }
    }
}