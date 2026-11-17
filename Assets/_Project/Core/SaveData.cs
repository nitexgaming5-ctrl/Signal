using System;
using System.Collections.Generic;

namespace Signal.Core
{
    [Serializable]
    public sealed class SavedItem
    {
        public string typeId;
        public string instanceId;
        public int x;
        public int y;
        public bool rotated;
        public int count;
        public int quality;
        public int condition;
        public int authenticity;
        public int ownership;
    }

    [Serializable]
    public sealed class SaveData
    {
        public int version = SaveMigrator.CurrentVersion;
        public int money;
        public int health;
        public int day = 1;
        public int minuteOfDay;
        public int heat;
        public int missedRent;
        public int inventoryWidth;
        public int inventoryHeight;
        public string worldPlace = "Hideout";
        public float playerX;
        public float playerY;
        public List<SavedItem> items = new List<SavedItem>();
        public List<SavedItem> storageItems = new List<SavedItem>();
    }

    public sealed class LoadReport
    {
        public bool Success { get; set; }
        public string Error { get; set; }
        public int SkippedItems { get; set; }
        public int SkippedStorageItems { get; set; }
    }

    public static class SaveMigrator
    {
        public const int CurrentVersion = 4;

        /// <summary>Accepts the first public save format and the current hideout-storage format.</summary>
        public static bool TryMigrate(SaveData data, out string error)
        {
            error = null;
            if (data == null)
            {
                error = "No save data.";
                return false;
            }

            if (data.version < 1)
            {
                error = "Save has an invalid version (" + data.version + ").";
                return false;
            }

            if (data.version > CurrentVersion)
            {
                error = "Save was made by a newer version of the game (" + data.version + ").";
                return false;
            }

            // Version 1 did not contain a separate home store or a world location.
            if (data.version == 1)
            {
                data.worldPlace = "Hideout";
                data.storageItems = new List<SavedItem>();
            }

            // Versions 1 and 2 had no stable identity for an inventory stack.
            // Assign identities once during migration, across both containers.
            var usedIds = new HashSet<Guid>();
            EnsureInstanceIds(data.items, usedIds);
            EnsureInstanceIds(data.storageItems, usedIds);
            // Version 3 introduced stable stack identity but had no per-stack state. The new
            // enum fields default to their zero values, which are the neutral starting state.
            data.version = CurrentVersion;

            return true;
        }

        private static void EnsureInstanceIds(List<SavedItem> items, HashSet<Guid> usedIds)
        {
            if (items == null)
                return;

            foreach (SavedItem item in items)
            {
                if (item == null)
                    continue;

                if (!Guid.TryParse(item.instanceId, out Guid id) || id == Guid.Empty || !usedIds.Add(id))
                {
                    do { id = Guid.NewGuid(); }
                    while (!usedIds.Add(id));

                    item.instanceId = id.ToString();
                }
            }
        }
    }

    public static class SaveMapper
    {
        public static SaveData Capture(PlayerStats stats, GridInventory inventory, GameClock clock, HeatMeter heat,
                                       RentLedger rent, float playerX, float playerY)
        {
            return Capture(stats, inventory, null, clock, heat, rent, "Hideout", playerX, playerY);
        }

        public static SaveData Capture(PlayerStats stats, GridInventory inventory, GridInventory storage,
                                       GameClock clock, HeatMeter heat, RentLedger rent, string worldPlace,
                                       float playerX, float playerY)
        {
            if (stats == null) throw new ArgumentNullException(nameof(stats));
            if (inventory == null) throw new ArgumentNullException(nameof(inventory));
            if (clock == null) throw new ArgumentNullException(nameof(clock));
            if (heat == null) throw new ArgumentNullException(nameof(heat));
            if (rent == null) throw new ArgumentNullException(nameof(rent));

            var data = new SaveData
            {
                money = stats.Money,
                health = stats.Health,
                day = clock.Day,
                minuteOfDay = clock.MinuteOfDay,
                heat = heat.Heat,
                missedRent = rent.MissedPayments,
                inventoryWidth = inventory.Width,
                inventoryHeight = inventory.Height,
                worldPlace = string.IsNullOrWhiteSpace(worldPlace) ? "Hideout" : worldPlace,
                playerX = playerX,
                playerY = playerY
            };

            CaptureItems(inventory, data.items);
            if (storage != null)
                CaptureItems(storage, data.storageItems);

            return data;
        }

        public static LoadReport Apply(SaveData data, PlayerStats stats, GridInventory inventory, GameClock clock,
                                       HeatMeter heat, RentLedger rent, Func<Guid, ItemType?> resolve)
        {
            return Apply(data, stats, inventory, null, clock, heat, rent, resolve);
        }

        public static LoadReport Apply(SaveData data, PlayerStats stats, GridInventory inventory, GridInventory storage,
                                       GameClock clock, HeatMeter heat, RentLedger rent, Func<Guid, ItemType?> resolve)
        {
            if (stats == null) throw new ArgumentNullException(nameof(stats));
            if (inventory == null) throw new ArgumentNullException(nameof(inventory));
            if (clock == null) throw new ArgumentNullException(nameof(clock));
            if (heat == null) throw new ArgumentNullException(nameof(heat));
            if (rent == null) throw new ArgumentNullException(nameof(rent));
            if (resolve == null) throw new ArgumentNullException(nameof(resolve));

            if (!SaveMigrator.TryMigrate(data, out string error))
                return new LoadReport { Success = false, Error = error };

            stats.Restore(data.money, data.health);
            clock.Restore(data.day, data.minuteOfDay);
            heat.Restore(data.heat);
            rent.Restore(data.missedRent);

            inventory.Clear();
            int skipped = RestoreItems(data.items, inventory, resolve);

            int skippedStorage = 0;
            if (storage != null)
            {
                storage.Clear();
                skippedStorage = RestoreItems(data.storageItems, storage, resolve);
            }

            return new LoadReport
            {
                Success = true,
                SkippedItems = skipped,
                SkippedStorageItems = skippedStorage
            };
        }

        private static void CaptureItems(GridInventory inventory, List<SavedItem> destination)
        {
            foreach (var item in inventory.Items)
            {
                destination.Add(new SavedItem
                {
                    typeId = item.Type.Id.ToString(),
                    instanceId = item.ItemInstanceId.ToString(),
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
        }

        private static int RestoreItems(List<SavedItem> savedItems, GridInventory destination, Func<Guid, ItemType?> resolve)
        {
            int skipped = 0;
            if (savedItems == null)
                return skipped;

            foreach (var saved in savedItems)
            {
                if (saved == null || !Guid.TryParse(saved.typeId, out Guid id))
                {
                    skipped++;
                    continue;
                }

                ItemType? type = resolve(id);
                ItemInstanceState state;
                try
                {
                    state = new ItemInstanceState((ItemQuality)saved.quality, (ItemCondition)saved.condition,
                        (ItemAuthenticity)saved.authenticity, (ItemOwnershipStatus)saved.ownership);
                }
                catch (ArgumentOutOfRangeException)
                {
                    skipped++;
                    continue;
                }

                if (!type.HasValue || !destination.Place(type.Value, saved.x, saved.y, saved.rotated, saved.count,
                    Guid.TryParse(saved.instanceId, out Guid instanceId) ? instanceId : Guid.Empty, state).HasValue)
                    skipped++;
            }

            return skipped;
        }
    }
}
