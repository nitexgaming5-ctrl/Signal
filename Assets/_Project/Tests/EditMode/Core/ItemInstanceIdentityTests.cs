using System;
using NUnit.Framework;

namespace Signal.Core.Tests
{
    public sealed class ItemInstanceIdentityTests
    {
        private static ItemType CreateItem()
        {
            return new ItemType(Guid.NewGuid(), 1, 1, 10);
        }

        [Test]
        public void NewStacksReceiveDistinctStableIdentities()
        {
            ItemType type = CreateItem();
            var inventory = new GridInventory();

            inventory.Add(type, 12);

            Assert.That(inventory.Items.Count, Is.EqualTo(2));
            Assert.That(inventory.Items[0].ItemInstanceId, Is.Not.EqualTo(Guid.Empty));
            Assert.That(inventory.Items[1].ItemInstanceId, Is.Not.EqualTo(Guid.Empty));
            Assert.That(inventory.Items[0].ItemInstanceId, Is.Not.EqualTo(inventory.Items[1].ItemInstanceId));
        }

        [Test]
        public void InventoryRejectsDuplicateStableIdentity()
        {
            ItemType type = CreateItem();
            var inventory = new GridInventory(2, 1);
            Guid identity = Guid.NewGuid();

            Assert.That(inventory.Place(type, 0, 0, itemInstanceId: identity).HasValue, Is.True);
            Assert.That(inventory.Place(type, 1, 0, itemInstanceId: identity).HasValue, Is.False);
            Assert.That(inventory.Items.Count, Is.EqualTo(1));
        }

        [Test]
        public void MovingAndRotatingAStackPreservesItsIdentity()
        {
            ItemType type = CreateItem();
            var inventory = new GridInventory(3, 3);
            int localId = inventory.Place(type, 0, 0).Value;
            Guid identity = inventory.Items[0].ItemInstanceId;

            Assert.That(inventory.TryMove(localId, 1, 1, true), Is.True);
            Assert.That(inventory.TryGet(localId, out PlacedItem moved), Is.True);
            Assert.That(moved.ItemInstanceId, Is.EqualTo(identity));
        }

        [Test]
        public void SnapshotRestorePreservesStableIdentity()
        {
            ItemType type = CreateItem();
            var inventory = new GridInventory();
            int localId = inventory.Place(type, 0, 0).Value;
            Guid identity = inventory.Items[0].ItemInstanceId;
            InventorySnapshot snapshot = inventory.CreateSnapshot();

            inventory.Remove(localId);
            inventory.Restore(snapshot);

            Assert.That(inventory.Items[0].ItemInstanceId, Is.EqualTo(identity));
        }

        [Test]
        public void SaveCaptureAndApplyPreserveIdentityAcrossBothContainers()
        {
            ItemType type = CreateItem();
            var stats = new PlayerStats(25);
            var carried = new GridInventory();
            var storage = new GridInventory(8, 6);
            ItemInstanceState carriedState = new ItemInstanceState(ItemQuality.Fine, ItemCondition.Worn,
                ItemAuthenticity.Counterfeit, ItemOwnershipStatus.Stolen);
            carried.Add(type, 3, carriedState);
            storage.Add(type, 2, ItemInstanceState.Default);
            Guid carriedId = carried.Items[0].ItemInstanceId;
            Guid storedId = storage.Items[0].ItemInstanceId;

            SaveData save = SaveMapper.Capture(stats, carried, storage, new GameClock(),
                new HeatMeter(), new RentLedger(0, 7), "Hideout", 1f, 2f);

            Assert.That(save.version, Is.EqualTo(SaveMigrator.CurrentVersion));
            Assert.That(save.items[0].instanceId, Is.EqualTo(carriedId.ToString()));
            Assert.That(save.storageItems[0].instanceId, Is.EqualTo(storedId.ToString()));

            var loadedCarried = new GridInventory();
            var loadedStorage = new GridInventory(8, 6);
            LoadReport report = SaveMapper.Apply(save, new PlayerStats(), loadedCarried, loadedStorage,
                new GameClock(), new HeatMeter(), new RentLedger(0, 7),
                id => id == type.Id ? type : (ItemType?)null);

            Assert.That(report.Success, Is.True);
            Assert.That(report.SkippedItems, Is.Zero);
            Assert.That(report.SkippedStorageItems, Is.Zero);
            Assert.That(loadedCarried.Items[0].ItemInstanceId, Is.EqualTo(carriedId));
            Assert.That(loadedStorage.Items[0].ItemInstanceId, Is.EqualTo(storedId));
            Assert.That(loadedCarried.Items[0].State, Is.EqualTo(carriedState));
            Assert.That(loadedStorage.Items[0].State, Is.EqualTo(ItemInstanceState.Default));
        }

        [Test]
        public void VersionTwoSaveMigrationAssignsUniqueIdsAcrossCarriedAndStoredItemsAndAdvancesToCurrentVersion()
        {
            Guid typeId = Guid.NewGuid();
            var save = new SaveData
            {
                version = 2,
                items = { new SavedItem { typeId = typeId.ToString(), x = 0, y = 0, count = 1 } },
                storageItems = { new SavedItem { typeId = typeId.ToString(), x = 0, y = 0, count = 1 } }
            };

            Assert.That(SaveMigrator.TryMigrate(save, out string error), Is.True, error);
            Assert.That(save.version, Is.EqualTo(SaveMigrator.CurrentVersion));
            Assert.That(Guid.TryParse(save.items[0].instanceId, out Guid carriedId), Is.True);
            Assert.That(Guid.TryParse(save.storageItems[0].instanceId, out Guid storedId), Is.True);
            Assert.That(carriedId, Is.Not.EqualTo(Guid.Empty));
            Assert.That(storedId, Is.Not.EqualTo(Guid.Empty));
            Assert.That(carriedId, Is.Not.EqualTo(storedId));
        }

        [Test]
        public void VersionOneSaveStillMigratesToCurrentVersionWithStableIds()
        {
            var save = new SaveData
            {
                version = 1,
                worldPlace = null,
                items = { new SavedItem { typeId = Guid.NewGuid().ToString(), count = 2 } }
            };

            Assert.That(SaveMigrator.TryMigrate(save, out string error), Is.True, error);
            Assert.That(save.version, Is.EqualTo(SaveMigrator.CurrentVersion));
            Assert.That(save.worldPlace, Is.EqualTo("Hideout"));
            Assert.That(save.storageItems, Is.Not.Null);
            Assert.That(save.storageItems, Is.Empty);
            Assert.That(Guid.TryParse(save.items[0].instanceId, out Guid identity), Is.True);
            Assert.That(identity, Is.Not.EqualTo(Guid.Empty));
        }

        [Test]
        public void VersionThreeSaveMigrationDefaultsNewInstanceStateWithoutChangingIdentityAndAdvancesToCurrentVersion()
        {
            Guid identity = Guid.NewGuid();
            var save = new SaveData
            {
                version = 3,
                items =
                {
                    new SavedItem { typeId = Guid.NewGuid().ToString(), instanceId = identity.ToString(), count = 1 }
                }
            };

            Assert.That(SaveMigrator.TryMigrate(save, out string error), Is.True, error);
            Assert.That(save.version, Is.EqualTo(SaveMigrator.CurrentVersion));
            Assert.That(save.items[0].instanceId, Is.EqualTo(identity.ToString()));
            Assert.That(save.items[0].quality, Is.EqualTo((int)ItemQuality.Standard));
            Assert.That(save.items[0].condition, Is.EqualTo((int)ItemCondition.Good));
            Assert.That(save.items[0].authenticity, Is.EqualTo((int)ItemAuthenticity.Genuine));
            Assert.That(save.items[0].ownership, Is.EqualTo((int)ItemOwnershipStatus.Legitimate));
        }

        [Test]
        public void MigrationRepairsMissingOrDuplicateInstanceIds()
        {
            string duplicate = Guid.NewGuid().ToString();
            var save = new SaveData
            {
                version = 4,
                items =
                {
                    new SavedItem { typeId = Guid.NewGuid().ToString(), instanceId = duplicate, count = 1 },
                    new SavedItem { typeId = Guid.NewGuid().ToString(), instanceId = duplicate, count = 1 }
                }
            };

            Assert.That(SaveMigrator.TryMigrate(save, out string error), Is.True, error);
            Assert.That(save.items[0].instanceId, Is.Not.EqualTo(save.items[1].instanceId));
            Assert.That(Guid.TryParse(save.items[1].instanceId, out _), Is.True);
        }
    }
}
