using System;
using System.Collections.Generic;

namespace Signal.Core
{
    /// <summary>
    /// Captures and restores each physical backpack's contents under its own stable item-instance ID.
    /// Backpack world state and equipment state are persisted separately.
    /// </summary>
    public static class PortableBackpackStorageSaveMapper
    {
        public static void Capture(PortableStorageBook source, SaveData destination)
        {
            if (destination == null)
                throw new ArgumentNullException(nameof(destination));

            destination.portableBackpackStorages.Clear();
            if (source == null)
                return;

            foreach (PortableBackpackStorageEntry entry in source.GetAll())
            {
                GridInventory contents = entry.Contents;
                if (entry.BackpackInstanceId == Guid.Empty || contents == null)
                    continue;

                var saved = new SavedPortableBackpackStorage
                {
                    instanceId = entry.BackpackInstanceId.ToString("N"),
                    width = contents.Width,
                    height = contents.Height
                };

                foreach (PlacedItem item in contents.Items)
                {
                    saved.items.Add(new SavedItem
                    {
                        typeId = item.Type.Id.ToString("N"),
                        instanceId = item.ItemInstanceId.ToString("N"),
                        x = item.X,
                        y = item.Y,
                        rotated = item.Rotated,
                        count = item.Count,
                        quality = (int)item.State.Quality,
                        condition = (int)item.State.Condition,
                        authenticity = (int)item.State.Authenticity,
                        ownership = (int)item.State.Ownership
                    });
                }

                destination.portableBackpackStorages.Add(saved);
            }
        }

        /// <summary>
        /// Rebuilds isolated grids and their registry bindings. The shared item-ID set lets callers
        /// reject duplicate physical item identities across player, pocket and backpack inventories.
        /// </summary>
        public static int Apply(SaveData source, PortableStorageBook destination,
            InventoryStorageRegistry registry, Func<Guid, ItemType?> resolve,
            HashSet<Guid> restoredItemIds)
        {
            if (destination == null)
                throw new ArgumentNullException(nameof(destination));
            if (registry == null)
                throw new ArgumentNullException(nameof(registry));
            if (resolve == null)
                throw new ArgumentNullException(nameof(resolve));
            if (restoredItemIds == null)
                throw new ArgumentNullException(nameof(restoredItemIds));

            destination.Clear(registry);
            if (source == null || source.portableBackpackStorages == null)
                return 0;

            int skipped = 0;
            var seenBackpacks = new HashSet<Guid>();
            foreach (SavedPortableBackpackStorage saved in source.portableBackpackStorages)
            {
                if (saved == null || !Guid.TryParse(saved.instanceId, out Guid backpackId) ||
                    backpackId == Guid.Empty || !seenBackpacks.Add(backpackId) ||
                    saved.width < 1 || saved.height < 1)
                {
                    skipped++;
                    continue;
                }

                var contents = new GridInventory(saved.width, saved.height);
                InventoryStorageId identity = PortableStorageBook.GetStorageIdentity(backpackId);
                if (!registry.TryRegister(identity, contents))
                {
                    skipped++;
                    continue;
                }

                if (!destination.TryRegister(backpackId, contents))
                {
                    registry.TryUnregister(identity);
                    skipped++;
                    continue;
                }

                if (saved.items == null)
                    continue;

                foreach (SavedItem item in saved.items)
                {
                    if (item == null || !Guid.TryParse(item.typeId, out Guid typeId) ||
                        !Guid.TryParse(item.instanceId, out Guid itemId) || itemId == Guid.Empty ||
                        restoredItemIds.Contains(itemId) ||
                        !Enum.IsDefined(typeof(ItemQuality), item.quality) ||
                        !Enum.IsDefined(typeof(ItemCondition), item.condition) ||
                        !Enum.IsDefined(typeof(ItemAuthenticity), item.authenticity) ||
                        !Enum.IsDefined(typeof(ItemOwnershipStatus), item.ownership))
                    {
                        skipped++;
                        continue;
                    }

                    ItemType? type = resolve(typeId);
                    var state = new ItemInstanceState((ItemQuality)item.quality,
                        (ItemCondition)item.condition, (ItemAuthenticity)item.authenticity,
                        (ItemOwnershipStatus)item.ownership);
                    if (!type.HasValue || !contents.Place(type.Value, item.x, item.y,
                        item.rotated, item.count, itemId, state).HasValue)
                    {
                        skipped++;
                        continue;
                    }

                    restoredItemIds.Add(itemId);
                }
            }

            return skipped;
        }
    }
}
