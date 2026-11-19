using System;
using System.Collections.Generic;
using Signal.Core;
using UnityEngine;

namespace Signal.Game
{
    [CreateAssetMenu(menuName = "Signal/Business Definition", fileName = "Business")]
    public sealed class BusinessDefinition : ScriptableObject
    {
        [Serializable]
        public sealed class MerchandiseEntry
        {
            public ItemDefinition Item;
            [Min(1)] public int UnitPrice = 1;
        }

        [SerializeField, HideInInspector] private string businessId;
        [SerializeField] private string displayName = "Business";
        [SerializeField] private string businessType = "Shop";
        [SerializeField] private string ownerNpcId = "milo";
        [SerializeField] private string locationId = "CornerShop";
        [SerializeField] private Vector2 interactionPoint = new Vector2(1000f, 1000.4f);
        [SerializeField, Range(0, 1439)] private int openingMinute = 390;
        [SerializeField, Range(0, 1439)] private int closingMinute = 1080;
        [SerializeField] private MerchandiseEntry[] merchandise = new MerchandiseEntry[0];

        public string BusinessId => businessId;
        public string DisplayName => displayName;
        public string BusinessType => businessType;
        public string OwnerNpcId => ownerNpcId;
        public string LocationId => locationId;
        public Vector2 InteractionPoint => interactionPoint;
        public int OpeningMinute => openingMinute;
        public int ClosingMinute => closingMinute;
        public IReadOnlyList<MerchandiseEntry> Merchandise => merchandise ?? Array.Empty<MerchandiseEntry>();

        public EconomicActorId OwnerActor => new EconomicActorId("resident:" + (ownerNpcId ?? string.Empty));

        public bool IsOpen(int minuteOfDay)
        {
            if (minuteOfDay < 0 || minuteOfDay >= 1440)
                return false;
            if (openingMinute == closingMinute)
                return true;
            if (openingMinute < closingMinute)
                return minuteOfDay >= openingMinute && minuteOfDay < closingMinute;
            return minuteOfDay >= openingMinute || minuteOfDay < closingMinute;
        }

        public bool TryGetOffer(ItemDefinition item, out int unitPrice)
        {
            unitPrice = 0;
            if (item == null || merchandise == null)
                return false;

            for (int i = 0; i < merchandise.Length; i++)
            {
                MerchandiseEntry entry = merchandise[i];
                if (entry == null || entry.Item != item || entry.UnitPrice < 1)
                    continue;
                unitPrice = entry.UnitPrice;
                return true;
            }
            return false;
        }

        private void Reset()
        {
            EnsureBusinessId();
        }

        private void OnValidate()
        {
            EnsureBusinessId();
            openingMinute = Mathf.Clamp(openingMinute, 0, 1439);
            closingMinute = Mathf.Clamp(closingMinute, 0, 1439);
            if (merchandise == null)
                merchandise = new MerchandiseEntry[0];
        }

        private void EnsureBusinessId()
        {
            if (!string.IsNullOrWhiteSpace(businessId))
                return;
            businessId = Guid.NewGuid().ToString();
#if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(this);
#endif
        }
    }
}
