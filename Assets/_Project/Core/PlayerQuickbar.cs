using System;
using System.Collections.Generic;

namespace Signal.Core
{
    /// <summary>
    /// Six visual references to physical items in the player's carried storage.
    /// This model never owns or copies inventory items.
    /// </summary>
    public sealed class PlayerQuickbar
    {
        public const int SlotCount = 6;
        private readonly Guid[] _slots = new Guid[SlotCount];
        private int _selectedSlot = -1;
        private bool _isHolstered = true;

        public int SelectedSlot => _selectedSlot;
        public bool IsHolstered => _isHolstered;
        public Guid HeldItemInstanceId => !_isHolstered && _selectedSlot >= 0 ? _slots[_selectedSlot] : Guid.Empty;

        public bool TrySelectSlot(int slotIndex)
        {
            ValidateSlot(slotIndex);
            _selectedSlot = slotIndex;
            _isHolstered = false;
            return true;
        }

        public bool TryCycle(int direction)
        {
            if (direction == 0) return false;
            int next = _selectedSlot < 0
                ? (direction > 0 ? 0 : SlotCount - 1)
                : (_selectedSlot + (direction > 0 ? 1 : -1) + SlotCount) % SlotCount;
            return TrySelectSlot(next);
        }

        public void Holster()
        {
            _isHolstered = true;
        }

        public void ResetActiveHand()
        {
            _selectedSlot = -1;
            _isHolstered = true;
        }

        public bool ReconcileActiveHand(Func<Guid, bool> itemAccessible)
        {
            if (_isHolstered || _selectedSlot < 0) return false;
            Guid item = _slots[_selectedSlot];
            if (item == Guid.Empty || itemAccessible == null || itemAccessible(item)) return false;
            _isHolstered = true;
            return true;
        }

        public Guid GetItem(int slotIndex)
        {
            ValidateSlot(slotIndex);
            return _slots[slotIndex];
        }

        public bool IsAssigned(Guid itemInstanceId)
        {
            if (itemInstanceId == Guid.Empty) return false;
            for (int i = 0; i < _slots.Length; i++)
                if (_slots[i] == itemInstanceId) return true;
            return false;
        }

        public int FindSlot(Guid itemInstanceId)
        {
            if (itemInstanceId == Guid.Empty) return -1;
            for (int i = 0; i < _slots.Length; i++)
                if (_slots[i] == itemInstanceId) return i;
            return -1;
        }

        /// <summary>Assigns an existing physical item. An item can appear in only one slot.</summary>
        public bool TryAssign(int slotIndex, Guid itemInstanceId)
        {
            ValidateSlot(slotIndex);
            if (itemInstanceId == Guid.Empty) return false;
            int existingSlot = FindSlot(itemInstanceId);
            if (existingSlot >= 0 && existingSlot != slotIndex) return false;
            _slots[slotIndex] = itemInstanceId;
            return true;
        }

        /// <summary>Replaces the slot reference; the previously referenced inventory item is untouched.</summary>
        public bool TryReplace(int slotIndex, Guid itemInstanceId)
        {
            ValidateSlot(slotIndex);
            if (itemInstanceId == Guid.Empty || (FindSlot(itemInstanceId) >= 0 && FindSlot(itemInstanceId) != slotIndex))
                return false;
            _slots[slotIndex] = itemInstanceId;
            return true;
        }

        /// <summary>Moves an item's existing reference to a chosen slot without duplicating it.</summary>
        public bool TryMoveAssignment(int targetSlot, Guid itemInstanceId)
        {
            ValidateSlot(targetSlot);
            if (itemInstanceId == Guid.Empty) return false;
            int existingSlot = FindSlot(itemInstanceId);
            if (existingSlot == targetSlot) return true;
            if (existingSlot >= 0) _slots[existingSlot] = Guid.Empty;
            _slots[targetSlot] = itemInstanceId;
            return true;
        }

        public bool TrySwap(int firstSlot, int secondSlot)
        {
            ValidateSlot(firstSlot);
            ValidateSlot(secondSlot);
            if (firstSlot == secondSlot) return true;
            Guid value = _slots[firstSlot];
            _slots[firstSlot] = _slots[secondSlot];
            _slots[secondSlot] = value;
            return true;
        }

        public bool TryUnassign(int slotIndex)
        {
            ValidateSlot(slotIndex);
            if (_slots[slotIndex] == Guid.Empty) return false;
            _slots[slotIndex] = Guid.Empty;
            if (_selectedSlot == slotIndex) _isHolstered = true;
            return true;
        }

        public int Reconcile(Func<Guid, bool> itemExists)
        {
            if (itemExists == null) return 0;
            int cleared = 0;
            for (int i = 0; i < _slots.Length; i++)
            {
                if (_slots[i] == Guid.Empty || itemExists(_slots[i])) continue;
                _slots[i] = Guid.Empty;
                if (_selectedSlot == i) _isHolstered = true;
                cleared++;
            }
            return cleared;
        }

        public void ClearItem(Guid itemInstanceId)
        {
            int slot = FindSlot(itemInstanceId);
            if (slot >= 0)
            {
                _slots[slot] = Guid.Empty;
                if (_selectedSlot == slot) _isHolstered = true;
            }
        }

        public void Clear()
        {
            Array.Clear(_slots, 0, _slots.Length);
            _selectedSlot = -1;
            _isHolstered = true;
        }

        public Guid[] Capture()
        {
            var snapshot = new Guid[SlotCount];
            Array.Copy(_slots, snapshot, SlotCount);
            return snapshot;
        }

        /// <summary>
        /// Restores only unique, non-empty IDs accepted by the supplied existence check.
        /// Invalid entries are ignored rather than granting access to missing items.
        /// </summary>
        public int Restore(IReadOnlyList<Guid> itemInstanceIds, Func<Guid, bool> itemExists)
        {
            Clear();
            if (itemInstanceIds == null) return 0;
            int skipped = 0;
            var seen = new HashSet<Guid>();
            int count = Math.Min(SlotCount, itemInstanceIds.Count);
            for (int i = 0; i < count; i++)
            {
                Guid id = itemInstanceIds[i];
                if (id == Guid.Empty) continue;
                if (!seen.Add(id) || itemExists == null || !itemExists(id))
                {
                    skipped++;
                    continue;
                }
                _slots[i] = id;
            }
            if (itemInstanceIds.Count > SlotCount)
                skipped += itemInstanceIds.Count - SlotCount;
            return skipped;
        }

        private static void ValidateSlot(int slotIndex)
        {
            if (slotIndex < 0 || slotIndex >= SlotCount)
                throw new ArgumentOutOfRangeException(nameof(slotIndex));
        }
    }

    public static class PlayerQuickbarSaveMapper
    {
        public static void Capture(PlayerQuickbar quickbar, SaveData destination)
        {
            if (destination == null) return;
            destination.quickbarItemInstanceIds = new List<string>();
            if (quickbar == null) return;
            Guid[] ids = quickbar.Capture();
            for (int i = 0; i < PlayerQuickbar.SlotCount; i++)
                destination.quickbarItemInstanceIds.Add(ids[i] == Guid.Empty ? string.Empty : ids[i].ToString("D"));
        }

        public static int Apply(SaveData data, PlayerQuickbar quickbar, Func<Guid, bool> itemExists)
        {
            if (quickbar == null) return 0;
            if (data == null || data.quickbarItemInstanceIds == null)
            {
                quickbar.Clear();
                return 0;
            }

            var ids = new List<Guid>(data.quickbarItemInstanceIds.Count);
            int skipped = 0;
            foreach (string value in data.quickbarItemInstanceIds)
            {
                if (string.IsNullOrWhiteSpace(value))
                {
                    ids.Add(Guid.Empty);
                    continue;
                }
                if (Guid.TryParse(value, out Guid id) && id != Guid.Empty)
                    ids.Add(id);
                else
                {
                    ids.Add(Guid.Empty);
                    skipped++;
                }
            }
            return skipped + quickbar.Restore(ids, itemExists);
        }
    }
}