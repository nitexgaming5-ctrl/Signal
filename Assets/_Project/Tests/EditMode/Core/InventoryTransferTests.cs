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
        public void MoveAllPreservesTheTransferredStacksRotation()
        {
            ItemType type = new ItemType(Guid.NewGuid(), 1, 2, 1);
            var carried = new GridInventory(2, 2);
            var storage = new GridInventory(2, 2);
            int localId = carried.Place(type, 0, 0, rotated: true).Value;
            Guid identity = carried.Items[0].ItemInstanceId;

            InventoryTransferResult result = InventoryTransfer.MoveAll(carried, storage, type);

            Assert.That(result.Moved, Is.EqualTo(1));
            Assert.That(storage.Items.Count, Is.EqualTo(1));
            Assert.That(storage.Items[0].ItemInstanceId, Is.EqualTo(identity));
            Assert.That(storage.Items[0].Rotated, Is.True);
            Assert.That(carried.TryGet(localId, out _), Is.False);
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
        public void MoveAllNotifiesObserversOnlyAfterBothInventoriesCommit()
        {
            ItemType type = Item();
            var carried = new GridInventory();
            var storage = new GridInventory(2, 2);
            carried.Add(type, 2);
            bool observerSawCommittedState = false;
            storage.Changed += _ =>
            {
                observerSawCommittedState = carried.CountOf(type) == 0 && storage.CountOf(type) == 2;
            };

            InventoryTransferResult result = InventoryTransfer.MoveAll(carried, storage, type);

            Assert.That(result.Success, Is.True);
            Assert.That(result.Moved, Is.EqualTo(2));
            Assert.That(observerSawCommittedState, Is.True);
            Assert.That(carried.CountOf(type), Is.Zero);
            Assert.That(storage.CountOf(type), Is.EqualTo(2));
        }

        [Test]
        public void MoveStackTransfersOnlyTheSelectedPhysicalStack()
        {
            ItemType type = Item();
            ItemInstanceState selectedState = new ItemInstanceState(
                ItemQuality.Fine, ItemCondition.Worn, ItemAuthenticity.Genuine, ItemOwnershipStatus.Stolen);
            ItemInstanceState otherState = new ItemInstanceState(
                ItemQuality.Poor, ItemCondition.Damaged, ItemAuthenticity.Counterfeit, ItemOwnershipStatus.Legitimate);
            var carried = new GridInventory(3, 1);
            var storage = new GridInventory(3, 1);
            carried.Add(type, 2, selectedState);
            carried.Add(type, 3, otherState);
            Guid selectedId = carried.Items[0].ItemInstanceId;
            Guid otherId = carried.Items[1].ItemInstanceId;

            InventoryTransferResult result = InventoryTransfer.MoveStack(carried, storage, selectedId);

            Assert.That(result.Success, Is.True);
            Assert.That(result.Moved, Is.EqualTo(2));
            Assert.That(result.Remaining, Is.Zero);
            Assert.That(carried.Items.Count, Is.EqualTo(1));
            Assert.That(carried.Items[0].ItemInstanceId, Is.EqualTo(otherId));
            Assert.That(carried.Items[0].State, Is.EqualTo(otherState));
            Assert.That(storage.Items.Count, Is.EqualTo(1));
            Assert.That(storage.Items[0].ItemInstanceId, Is.EqualTo(selectedId));
            Assert.That(storage.Items[0].State, Is.EqualTo(selectedState));
        }

        [Test]
        public void MoveStackLeavesTheSourceUntouchedWhenDestinationHasNoRoom()
        {
            ItemType type = Item();
            ItemType blocker = Item();
            var carried = new GridInventory(2, 1);
            var storage = new GridInventory(1, 1);
            carried.Add(type, 2);
            storage.Place(blocker, 0, 0);
            Guid identity = carried.Items[0].ItemInstanceId;

            InventoryTransferResult result = InventoryTransfer.MoveStack(carried, storage, identity);

            Assert.That(result.Success, Is.True);
            Assert.That(result.Moved, Is.Zero);
            Assert.That(result.Remaining, Is.EqualTo(2));
            Assert.That(carried.Items[0].ItemInstanceId, Is.EqualTo(identity));
            Assert.That(carried.CountOf(type), Is.EqualTo(2));
        }

        [Test]
        public void MoveStackNotifiesObserversOnlyAfterBothInventoriesCommit()
        {
            ItemType type = Item();
            var carried = new GridInventory();
            var storage = new GridInventory(2, 2);
            carried.Add(type, 2);
            Guid identity = carried.Items[0].ItemInstanceId;
            bool observerSawCommittedState = false;
            storage.Changed += _ =>
            {
                observerSawCommittedState = carried.Items.Count == 0 &&
                    storage.TryGetItemInstance(identity, out PlacedItem transferred) &&
                    transferred.Count == 2;
            };

            InventoryTransferResult result = InventoryTransfer.MoveStack(carried, storage, identity);

            Assert.That(result.Success, Is.True);
            Assert.That(result.Moved, Is.EqualTo(2));
            Assert.That(observerSawCommittedState, Is.True);
            Assert.That(carried.Items, Is.Empty);
            Assert.That(storage.Items.Count, Is.EqualTo(1));
            Assert.That(storage.Items[0].ItemInstanceId, Is.EqualTo(identity));
        }

        [Test]
        public void SourceAndDestinationMustBeDifferentInventories()
        {
            var inventory = new GridInventory();
            Assert.Throws<ArgumentException>(() => InventoryTransfer.MoveAll(inventory, inventory, Item()));
        }
    }
}
