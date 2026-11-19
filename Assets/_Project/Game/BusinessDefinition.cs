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

        public bool TryValidate(out string error)
        {
            error = null;
            if (string.IsNullOrWhiteSpace(businessId) || string.IsNullOrWhiteSpace(displayName) ||
                string.IsNullOrWhiteSpace(businessType) || string.IsNullOrWhiteSpace(ownerNpcId) ||
                string.IsNullOrWhiteSpace(locationId))
            {
                error = "Business identity, name, type, owner and location are required.";
                return false;
            }
            if (float.IsNaN(interactionPoint.x) || float.IsInfinity(interactionPoint.x) ||
                float.IsNaN(interactionPoint.y) || float.IsInfinity(interactionPoint.y) ||
                openingMinute < 0 || openingMinute >= 1440 || closingMinute < 0 || closingMinute >= 1440)
            {
                error = "Business interaction point or opening schedule is invalid.";
                return false;
            }

            var itemIds = new HashSet<Guid>();
            if (merchandise == null)
                return true;
            for (int i = 0; i < merchandise.Length; i++)
            {
                MerchandiseEntry entry = merchandise[i];
                if (entry == null || entry.Item == null || entry.UnitPrice < 1)
                {
                    error = "Every merchandise entry needs a valid item and a positive fixed price.";
                    return false;
                }
                try
                {
                    if (!itemIds.Add(entry.Item.ToItemType().Id))
                    {
                        error = "A business catalogue cannot offer the same item more than once.";
                        return false;
                    }
                }
                catch (InvalidOperationException)
                {
                    error = "A merchandise item has no valid stable item ID.";
                    return false;
                }
            }
            return true;
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
