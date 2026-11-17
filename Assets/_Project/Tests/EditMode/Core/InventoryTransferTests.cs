using System;
using NUnit.Framework;

namespace Signal.Core.Tests
{
    public sealed class InventoryTransferTests
    {
        private static ItemType Item(int maxStack = 5)
        {
            return new ItemType(Guid.NewGuid(), 1, 1, maxStack);
        }

        [Test]
        public void MoveAllTransfersEveryAvailableItemWhenThereIsRoom()
        {
            ItemType type = Item();
            var carried = new GridInventory();
            var storage = new GridInventory(4, 4);
            carried.Add(type, 3);

            InventoryTransferResult result = InventoryTransfer.MoveAll(carried, storage, type);

            Assert.That(result.Success, Is.True);
            Assert.That(result.Moved, Is.EqualTo(3));
            Assert.That(result.Remaining, Is.Zero);
            Assert.That(carried.CountOf(type), Is.Zero);
            Assert.That(storage.CountOf(type), Is.EqualTo(3));
        }

        [Test]
        public void MoveAllKeepsDistinctStacksAndTheirIdentitiesInsteadOfMergingThem()
        {
            ItemType type = Item();
            var carried = new GridInventory();
            var storage = new GridInventory(2, 1);
            carried.Add(type, 3);
            storage.Place(type, 0, 0, count: 4);
            Guid sourceIdentity = carried.Items[0].ItemInstanceId;
            Guid existingIdentity = storage.Items[0].ItemInstanceId;

            InventoryTransferResult result = InventoryTransfer.MoveAll(carried, storage, type);

            Assert.That(result.Success, Is.True);
            Assert.That(result.Moved, Is.EqualTo(3));
            Assert.That(result.Remaining, Is.Zero);
            Assert.That(carried.CountOf(type), Is.Zero);
            Assert.That(storage.CountOf(type), Is.EqualTo(7));
            Assert.That(storage.Items.Count, Is.EqualTo(2));
            Assert.That(storage.Items[0].ItemInstanceId, Is.EqualTo(existingIdentity));
            Assert.That(storage.Items[1].ItemInstanceId, Is.EqualTo(sourceIdentity));
        }

        [Test]
        public void MoveAllChangesNothingWhenDestinationHasNoRoom()
        {
            ItemType type = Item();
            ItemType blocker = Item();
            var carried = new GridInventory();
            var storage = new GridInventory(1, 1);
            carried.Add(type, 2);
            storage.Place(blocker, 0, 0);

            InventoryTransferResult result = InventoryTransfer.MoveAll(carried, storage, type);

            Assert.That(result.Success, Is.True);
            Assert.That(result.Moved, Is.Zero);
            Assert.That(result.Remaining, Is.EqualTo(2));
            Assert.That(carried.CountOf(type), Is.EqualTo(2));
            Assert.That(storage.CountOf(type), Is.Zero);
            Assert.That(storage.CountOf(blocker), Is.EqualTo(1));
        }

        [Test]
        public void MoveAllRollsBackBothInventoriesIfSourceChangesDuringTransfer()
        {
            ItemType type = Item();
            var carried = new GridInventory();
            var storage = new GridInventory(2, 2);
            carried.Add(type, 2);
            bool changed = false;
            storage.Changed += _ =>
            {
                if (changed) return;
                changed = true;
                carried.Clear();
            };

            InventoryTransferResult result = InventoryTransfer.MoveAll(carried, storage, type);

            Assert.That(result.Success, Is.False);
            Assert.That(result.Moved, Is.Zero);
            Assert.That(carried.CountOf(type), Is.EqualTo(2));
            Assert.That(storage.CountOf(type), Is.Zero);
        }

        [Test]
        public void SourceAndDestinationMustBeDifferentInventories()
        {
            var inventory = new GridInventory();
            Assert.Throws<ArgumentException>(() => InventoryTransfer.MoveAll(inventory, inventory, Item()));
        }
    }
}
