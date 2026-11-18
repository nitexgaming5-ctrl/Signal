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
    public sealed class SavedItemKnowledge
    {
        public string instanceId;
        public int aspect;
        public int value;
        public int source;
        public float confidence;
        public int learnedDay;
        public int learnedMinuteOfDay;
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
        public List<SavedItemKnowledge> itemKnowledge = new List<SavedItemKnowledge>();
    }

    public sealed class LoadReport
    {
        public bool Success { get; set; }
        public string Error { get; set; }
        public int SkippedItems { get; set; }
        public int SkippedStorageItems { get; set; }
        public int SkippedKnowledgeFacts { get; set; }
    }

    public static class SaveMigrator
    {
        public const int CurrentVersion = 5;

        /// <summary>Accepts save formats v1 through v4 and migrates them to the current item-state format.</summary>
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
            // Versions 1-3 had no per-stack state. Enum fields default to neutral values.
            // Version 4 introduced per-stack state but no player-specific item knowledge.
            if (data.itemKnowledge == null)
                data.itemKnowledge = new List<SavedItemKnowledge>();

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
                                       float playerX, float playerY, ItemKnowledgeBook itemKnowledge = null)
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
            CaptureKnowledge(itemKnowledge, data.itemKnowledge);

            return data;
        }

        public static LoadReport Apply(SaveData data, PlayerStats stats, GridInventory inventory, GameClock clock,
                                       HeatMeter heat, RentLedger rent, Func<Guid, ItemType?> resolve)
        {
            return Apply(data, stats, inventory, null, clock, heat, rent, resolve, null);
        }

        public static LoadReport Apply(SaveData data, PlayerStats stats, GridInventory inventory, GridInventory storage,
                                       GameClock clock, HeatMeter heat, RentLedger rent, Func<Guid, ItemType?> resolve,
                                       ItemKnowledgeBook itemKnowledge = null)
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

            int skippedKnowledge = RestoreKnowledge(data.itemKnowledge, itemKnowledge);

            return new LoadReport
            {
                Success = true,
                SkippedItems = skipped,
                SkippedStorageItems = skippedStorage,
                SkippedKnowledgeFacts = skippedKnowledge
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


        private static void CaptureKnowledge(ItemKnowledgeBook knowledge, List<SavedItemKnowledge> destination)
        {
            if (knowledge == null)
                return;

            foreach (ItemKnowledgeFact fact in knowledge.Facts)
            {
                destination.Add(new SavedItemKnowledge
                {
                    instanceId = fact.ItemInstanceId.ToString(),
                    aspect = (int)fact.Aspect,
                    value = fact.Value,
                    source = (int)fact.Source,
                    confidence = fact.Confidence,
                    learnedDay = fact.LearnedDay,
                    learnedMinuteOfDay = fact.LearnedMinuteOfDay
                });
            }
        }

        private static int RestoreKnowledge(List<SavedItemKnowledge> savedFacts, ItemKnowledgeBook destination)
        {
            if (destination == null)
                return 0;

            destination.Clear();
            int skipped = 0;
            if (savedFacts == null)
                return skipped;

            foreach (SavedItemKnowledge saved in savedFacts)
            {
                if (saved == null || !Guid.TryParse(saved.instanceId, out Guid instanceId))
                {
                    skipped++;
                    continue;
                }

                try
                {
                    destination.RecordFact(instanceId, (ItemKnowledgeAspect)saved.aspect, saved.value,
                        (ItemKnowledgeSource)saved.source, saved.confidence, saved.learnedDay, saved.learnedMinuteOfDay);
                }
                catch (ArgumentException)
                {
                    skipped++;
                }
            }

            return skipped;
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
