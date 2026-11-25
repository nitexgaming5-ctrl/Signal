using System;
using System.Collections.Generic;

namespace Signal.Core
{
    /// <summary>Three independent one-cell personal pockets. Each cell holds one 1x1 item stack.</summary>
    public sealed class PlayerPocketBook
    {
        public const int PocketCount = 3;
        private readonly GridInventory[] _pockets =
        {
            new GridInventory(1, 1),
            new GridInventory(1, 1),
            new GridInventory(1, 1)
        };

        public GridInventory GetPocket(int index)
        {
            if (index < 0 || index >= PocketCount) throw new ArgumentOutOfRangeException(nameof(index));
            return _pockets[index];
        }

        public bool TryGetPocket(int index, out GridInventory pocket)
        {
            if (index < 0 || index >= PocketCount) { pocket = null; return false; }
            pocket = _pockets[index];
            return true;
        }

        public bool TryFindAvailablePocket(ItemType type, out int index, out GridInventory pocket)
        {
            if (type.Width == 1 && type.Height == 1)
            {
                for (int i = 0; i < PocketCount; i++)
                    if (_pockets[i].Items.Count == 0)
                    { index = i; pocket = _pockets[i]; return true; }
            }
            index = -1;
            pocket = null;
            return false;
        }

        public bool TryAdd(ItemType type, Guid itemInstanceId, int count, ItemInstanceState state, out int pocketIndex)
        {
            pocketIndex = -1;
            if (itemInstanceId == Guid.Empty || type.Width != 1 || type.Height != 1 ||
                count < 1 || count > type.MaxStack)
                return false;
            for (int i = 0; i < PocketCount; i++)
            {
                if (_pockets[i].Items.Count != 0) continue;
                if (!_pockets[i].Place(type, 0, 0, false, count, itemInstanceId, state).HasValue)
                    return false;
                pocketIndex = i;
                return true;
            }
            return false;
        }

        public bool TryAdd(PlacedItem item, out int pocketIndex)
        {
            if (item.Rotated)
            { pocketIndex = -1; return false; }
            return TryAdd(item.Type, item.ItemInstanceId, item.Count, item.State, out pocketIndex);
        }

        public bool TryMoveFrom(GridInventory source, Guid itemInstanceId, int pocketIndex)
        {
            if (source == null || !TryGetPocket(pocketIndex, out GridInventory destination) ||
                !source.TryGetItemInstance(itemInstanceId, out PlacedItem item) ||
                item.Type.Width != 1 || item.Type.Height != 1 || item.Rotated ||
                destination.Items.Count != 0)
                return false;
            InventoryTransferResult result = InventoryTransfer.MoveStack(source, destination, itemInstanceId);
            return result.Success && result.Moved == item.Count;
        }

        public bool TryMoveTo(GridInventory destination, int pocketIndex)
        {
            if (destination == null || !TryGetPocket(pocketIndex, out GridInventory source) ||
                source.Items.Count != 1)
                return false;
            Guid itemId = source.Items[0].ItemInstanceId;
            InventoryTransferResult result = InventoryTransfer.MoveStack(source, destination, itemId);
            return result.Success && result.Moved > 0;
        }

        public void Clear()
        {
            for (int i = 0; i < PocketCount; i++) _pockets[i].Clear();
        }
    }

    [Serializable]
    public sealed class SavedPocketItem
    {
        public int pocketIndex;
        public string typeId;
        public string instanceId;
        public int count;
        public int quality;
        public int condition;
        public int authenticity;
        public int ownership;
        public string ownerActorId;
    }

    public static class PlayerPocketSaveMapper
    {
        public static void Capture(PlayerPocketBook pockets, AssetOwnershipBook ownership, SaveData destination)
        {
            if (pockets == null || destination == null) return;
            destination.pocketItems.Clear();
            for (int i = 0; i < PlayerPocketBook.PocketCount; i++)
            {
                foreach (PlacedItem item in pockets.GetPocket(i).Items)
                {
                    EconomicActorId owner = ownership != null && ownership.TryGetOwner(item.ItemInstanceId, out EconomicActorId registered)
                        ? registered : EconomicActorId.Player;
                    destination.pocketItems.Add(new SavedPocketItem
                    {
                        pocketIndex = i,
                        typeId = item.Type.Id.ToString(),
                        instanceId = item.ItemInstanceId.ToString(),
                        count = item.Count,
                        quality = (int)item.State.Quality,
                        condition = (int)item.State.Condition,
                        authenticity = (int)item.State.Authenticity,
                        ownership = (int)item.State.Ownership,
                        ownerActorId = owner.Value
                    });
                }
            }
        }

        public static int Apply(SaveData data, PlayerPocketBook pockets, Func<Guid, ItemType?> resolve,
            AssetOwnershipBook ownership)
        {
            if (pockets == null) return 0;
            pockets.Clear();
            int skipped = 0;
            if (data == null || data.pocketItems == null) return skipped;
            var restoredSlots = new HashSet<int>();
            foreach (SavedPocketItem saved in data.pocketItems)
            {
                if (saved == null || saved.pocketIndex < 0 || saved.pocketIndex >= PlayerPocketBook.PocketCount ||
                    restoredSlots.Contains(saved.pocketIndex) || !Guid.TryParse(saved.typeId, out Guid typeId) ||
                    !Guid.TryParse(saved.instanceId, out Guid instanceId) || instanceId == Guid.Empty)
                { skipped++; continue; }

                ItemType? type = resolve != null ? resolve(typeId) : null;
                ItemInstanceState state;
                EconomicActorId owner = EconomicActorId.Player;
                try
                {
                    state = new ItemInstanceState((ItemQuality)saved.quality, (ItemCondition)saved.condition,
                        (ItemAuthenticity)saved.authenticity, (ItemOwnershipStatus)saved.ownership);
                    if (!string.IsNullOrWhiteSpace(saved.ownerActorId))
                        owner = new EconomicActorId(saved.ownerActorId);
                }
                catch (ArgumentException) { skipped++; continue; }

                if (!type.HasValue || type.Value.Width != 1 || type.Value.Height != 1 ||
                    !pockets.GetPocket(saved.pocketIndex).Place(type.Value, 0, 0, false, saved.count, instanceId, state).HasValue)
                { skipped++; continue; }
                restoredSlots.Add(saved.pocketIndex);
                if (ownership != null && !ownership.TryRegister(instanceId, owner))
                    skipped++;
            }
            return skipped;
        }
    }
}