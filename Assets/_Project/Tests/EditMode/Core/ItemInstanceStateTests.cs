using System;
using NUnit.Framework;

namespace Signal.Core.Tests
{
    public sealed class ItemInstanceStateTests
    {
        [Test]
        public void InventoryOnlyStacksItemsWithMatchingInstanceState()
        {
            ItemType type = new ItemType(Guid.NewGuid(), 1, 1, 10);
            var inventory = new GridInventory();

            inventory.Add(type, 2, ItemInstanceState.Default);
            inventory.Add(type, 3, new ItemInstanceState(
                ItemQuality.Fine, ItemCondition.Good, ItemAuthenticity.Genuine, ItemOwnershipStatus.Legitimate));
            inventory.Add(type, 4, ItemInstanceState.Default);

            Assert.That(inventory.Items.Count, Is.EqualTo(2));
            Assert.That(inventory.Items[0].Count, Is.EqualTo(6));
            Assert.That(inventory.Items[1].Count, Is.EqualTo(3));
            Assert.That(inventory.Items[0].State, Is.EqualTo(ItemInstanceState.Default));
            Assert.That(inventory.Items[1].State.Quality, Is.EqualTo(ItemQuality.Fine));
            Assert.That(inventory.Items[0].ItemInstanceId, Is.Not.EqualTo(inventory.Items[1].ItemInstanceId));
        }

        [Test]
        public void MovingRotatingAndTransferringPreservesAllInstanceState()
        {
            ItemType type = new ItemType(Guid.NewGuid(), 1, 2, 5);
            ItemInstanceState state = new ItemInstanceState(
                ItemQuality.Exceptional, ItemCondition.Pristine,
                ItemAuthenticity.Genuine, ItemOwnershipStatus.Stolen);
            var source = new GridInventory(3, 3);
            var destination = new GridInventory(3, 3);
            int localId = source.Place(type, 0, 0, state: state).Value;
            Guid identity = source.Items[0].ItemInstanceId;

            Assert.That(source.TryMove(localId, 1, 0, true), Is.True);
            Assert.That(source.Items[0].State, Is.EqualTo(state));

            InventoryTransferResult transfer = InventoryTransfer.MoveAll(source, destination, type);

            Assert.That(transfer.Success, Is.True);
            Assert.That(transfer.Moved, Is.EqualTo(1));
            Assert.That(source.Items, Is.Empty);
            Assert.That(destination.Items[0].ItemInstanceId, Is.EqualTo(identity));
            Assert.That(destination.Items[0].State, Is.EqualTo(state));
            Assert.That(destination.Items[0].Rotated, Is.True);
        }

        [Test]
        public void ItemValueReflectsQualityConditionAuthenticityAndOwnership()
        {
            ItemInstanceState premium = new ItemInstanceState(
                ItemQuality.Exceptional, ItemCondition.Pristine,
                ItemAuthenticity.Genuine, ItemOwnershipStatus.Legitimate);
            ItemInstanceState poor = new ItemInstanceState(
                ItemQuality.Poor, ItemCondition.Damaged,
                ItemAuthenticity.Counterfeit, ItemOwnershipStatus.Stolen);

            ItemValueBreakdown premiumValue = ItemValueCalculator.Calculate(100, premium);
            ItemValueBreakdown poorValue = ItemValueCalculator.Calculate(100, poor);

            Assert.That(premiumValue.FinalValue, Is.EqualTo(193));
            Assert.That(poorValue.FinalValue, Is.EqualTo(2));
            Assert.That(premiumValue.FinalValue, Is.GreaterThan(poorValue.FinalValue));
        }

        [Test]
        public void ItemStateCanChangeWithoutReplacingThePhysicalStack()
        {
            ItemType type = new ItemType(Guid.NewGuid(), 1, 1, 5);
            var inventory = new GridInventory();
            int localId = inventory.Place(type, 0, 0).Value;
            Guid identity = inventory.Items[0].ItemInstanceId;
            ItemInstanceState changed = new ItemInstanceState(
                ItemQuality.Fine, ItemCondition.Worn, ItemAuthenticity.Genuine, ItemOwnershipStatus.Legitimate);

            Assert.That(inventory.TryUpdateItemState(identity, changed), Is.True);
            Assert.That(inventory.TryGet(localId, out PlacedItem item), Is.True);
            Assert.That(item.ItemInstanceId, Is.EqualTo(identity));
            Assert.That(item.State, Is.EqualTo(changed));
        }

        [Test]
        public void StateAwareRemovalDoesNotConsumeOtherVariants()
        {
            ItemType type = new ItemType(Guid.NewGuid(), 1, 1, 10);
            var inventory = new GridInventory();
            ItemInstanceState standard = ItemInstanceState.Default;
            ItemInstanceState damaged = new ItemInstanceState(
                ItemQuality.Standard, ItemCondition.Damaged,
                ItemAuthenticity.Genuine, ItemOwnershipStatus.Legitimate);
            inventory.Add(type, 3, standard);
            inventory.Add(type, 4, damaged);

            Assert.That(inventory.CountOf(type, damaged), Is.EqualTo(4));
            Assert.That(inventory.TryRemove(type, 2, damaged), Is.True);
            Assert.That(inventory.CountOf(type), Is.EqualTo(5));
            Assert.That(inventory.CountOf(type, standard), Is.EqualTo(3));
            Assert.That(inventory.CountOf(type, damaged), Is.EqualTo(2));
        }

        [Test]
        public void NegativeBaseValueIsClampedToZero()
        {
            ItemValueBreakdown value = ItemValueCalculator.Calculate(-50, ItemInstanceState.Default);
            Assert.That(value.BaseValue, Is.Zero);
            Assert.That(value.FinalValue, Is.Zero);
        }
    }
}