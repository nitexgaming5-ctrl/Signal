using System;
using System.Collections.Generic;

namespace Signal.Core
{
    public enum EquipmentSlot
    {
        Head = 0,
        Torso = 1,
        Legs = 2,
        Feet = 3,
        Accessory1 = 4,
        Accessory2 = 5,
        Accessory3 = 6
    }

    public readonly struct EquippedItem
    {
        public EquippedItem(EquipmentSlot slot, PlacedItem item, int armorModifier, int heatProtectionModifier)
        {
            Slot = slot;
            Item = item;
            ArmorModifier = armorModifier;
            HeatProtectionModifier = heatProtectionModifier;
        }

        public EquipmentSlot Slot { get; }
        public PlacedItem Item { get; }
        public int ArmorModifier { get; }
        public int HeatProtectionModifier { get; }
    }

    /// <summary>
    /// Authoritative seven-slot equipment state. Transfers preserve the exact physical item
    /// instance, state and ownership identity; this class does not change asset ownership.
    /// </summary>
    public sealed class PlayerEquipmentLoadout
    {
        private readonly Dictionary<EquipmentSlot, EquippedItem> _equipped =
            new Dictionary<EquipmentSlot, EquippedItem>();

        public int Count => _equipped.Count;
        public IEnumerable<EquippedItem> Items => _equipped.Values;

        public bool IsOccupied(EquipmentSlot slot) => _equipped.ContainsKey(slot);

        public bool TryGet(EquipmentSlot slot, out EquippedItem item) => _equipped.TryGetValue(slot, out item);

        public bool TryEquip(GridInventory source, Guid itemInstanceId, EquipmentSlot slot,
            int armorModifier = 0, int heatProtectionModifier = 0)
        {
            if (source == null || !IsValidSlot(slot) || _equipped.ContainsKey(slot) ||
                itemInstanceId == Guid.Empty || ContainsInstance(itemInstanceId) ||
                !source.TryGetItemInstance(itemInstanceId, out PlacedItem item) || item.Count != 1)
                return false;

            if (!source.TryRemoveItemInstance(itemInstanceId, item.Count))
                return false;

            _equipped.Add(slot, new EquippedItem(slot, item, armorModifier, heatProtectionModifier));
            return true;
        }

        /// <summary>Restores a previously equipped single item from save data without requiring a free inventory cell.</summary>
        public bool TryRestoreEquipped(Guid itemInstanceId, ItemType type, ItemInstanceState state,
            EquipmentSlot slot, int armorModifier = 0, int heatProtectionModifier = 0)
        {
            if (itemInstanceId == Guid.Empty || type.Id == Guid.Empty || !IsValidSlot(slot) ||
                _equipped.ContainsKey(slot) || ContainsInstance(itemInstanceId))
                return false;

            var staging = new GridInventory(type.Width, type.Height);
            if (!staging.Place(type, 0, 0, false, 1, itemInstanceId, state).HasValue ||
                !staging.TryGetItemInstance(itemInstanceId, out PlacedItem item) ||
                !staging.TryRemoveItemInstance(itemInstanceId, 1))
                return false;

            _equipped.Add(slot, new EquippedItem(slot, item, armorModifier, heatProtectionModifier));
            return true;
        }

        /// <summary>Removes an equipped item from the loadout without placing it in an inventory.</summary>
        public bool TryTakeEquipped(EquipmentSlot slot, out EquippedItem equipped)
        {
            if (!_equipped.TryGetValue(slot, out equipped))
                return false;
            _equipped.Remove(slot);
            return true;
        }

        /// <summary>Returns the exact equipped item to the inventory; if it cannot fit, nothing changes.</summary>
        public bool TryUnequip(GridInventory destination, EquipmentSlot slot)
        {
            if (destination == null || !_equipped.TryGetValue(slot, out EquippedItem equipped))
                return false;

            if (!destination.TryPlaceTransferred(equipped.Item))
                return false;

            _equipped.Remove(slot);
            return true;
        }

        public EffectiveCharacterAttributes CalculateEffectiveAttributes(PlayerAttributeSet baseAttributes)
        {
            if (baseAttributes == null)
                throw new ArgumentNullException(nameof(baseAttributes));

            long armor = baseAttributes.Armor;
            long heat = baseAttributes.HeatProtection;
            foreach (EquippedItem item in _equipped.Values)
            {
                armor += item.ArmorModifier;
                heat += item.HeatProtectionModifier;
            }

            return new EffectiveCharacterAttributes(Clamp(armor), Clamp(heat));
        }

        public bool ContainsInstance(Guid itemInstanceId)
        {
            foreach (EquippedItem item in _equipped.Values)
                if (item.Item.ItemInstanceId == itemInstanceId)
                    return true;
            return false;
        }

        public void Clear() => _equipped.Clear();

        private static bool IsValidSlot(EquipmentSlot slot)
        {
            return slot >= EquipmentSlot.Head && slot <= EquipmentSlot.Accessory3;
        }

        private static int Clamp(long value)
        {
            if (value <= 0) return 0;
            return value >= int.MaxValue ? int.MaxValue : (int)value;
        }
    }

    /// <summary>
    /// Keeps backpack contents keyed by the backpack's stable physical item identity.
    /// Dropping/picking up a backpack changes its world location, not this contents owner.
    /// </summary>
    public sealed class PortableStorageBook
    {
        private readonly Dictionary<Guid, GridInventory> _contents =
            new Dictionary<Guid, GridInventory>();

        public int Count => _contents.Count;

        public bool TryRegister(Guid backpackInstanceId, int width, int height)
        {
            if (width < 1 || height < 1)
                return false;
            return TryRegister(backpackInstanceId, new GridInventory(width, height));
        }

        /// <summary>Attaches an existing grid as this backpack instance's contents without copying items.</summary>
        public bool TryRegister(Guid backpackInstanceId, GridInventory existingContents)
        {
            if (backpackInstanceId == Guid.Empty || existingContents == null ||
                _contents.ContainsKey(backpackInstanceId))
                return false;

            foreach (GridInventory registeredContents in _contents.Values)
                if (ReferenceEquals(registeredContents, existingContents))
                    return false;

            _contents.Add(backpackInstanceId, existingContents);
            return true;
        }

        public bool TryGetContents(Guid backpackInstanceId, out GridInventory contents)
        {
            contents = null;
            return backpackInstanceId != Guid.Empty && _contents.TryGetValue(backpackInstanceId, out contents);
        }

        /// <summary>Detaches a registration without deleting the referenced inventory.</summary>
        public bool TryUnregister(Guid backpackInstanceId)
        {
            return backpackInstanceId != Guid.Empty && _contents.Remove(backpackInstanceId);
        }

        public bool TryRemoveEmpty(Guid backpackInstanceId)
        {
            if (!_contents.TryGetValue(backpackInstanceId, out GridInventory contents) ||
                contents.Items.Count != 0)
                return false;
            return _contents.Remove(backpackInstanceId);
        }

        public void Clear() => _contents.Clear();
    }
}
