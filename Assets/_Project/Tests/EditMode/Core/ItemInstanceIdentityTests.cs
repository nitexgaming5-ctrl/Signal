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

    }
}
