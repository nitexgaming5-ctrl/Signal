using System;
using NUnit.Framework;

namespace Signal.Core.Tests
{
    public sealed class StackOperationsTests
    {
        private static ItemType Stackable(int max = 20) => new ItemType(Guid.NewGuid(), 1, 1, max);

        [Test]
        public void SplitKeepsSourceIdentityAndCreatesNewStackWithExactState()
        {
            var grid = new GridInventory(4, 2);
            ItemType type = Stackable();
            ItemInstanceState state = new ItemInstanceState(ItemQuality.Fine, ItemCondition.Worn,
                ItemAuthenticity.Genuine, ItemOwnershipStatus.Legitimate);
            Guid sourceId = Guid.NewGuid();
            grid.Place(type, 0, 0, count: 13, itemInstanceId: sourceId, state: state);

            StackOperationResult result = StackOperations.Split(grid, grid, sourceId, 6, 1, 0);

            Assert.That(result.Success, Is.True);
            Assert.That(grid.TryGetItemInstance(sourceId, out PlacedItem source), Is.True);
            Assert.That(source.Count, Is.EqualTo(7));
            Assert.That(source.State, Is.EqualTo(state));
            Assert.That(result.ResultingStackId, Is.Not.EqualTo(sourceId));
            Assert.That(grid.TryGetItemInstance(result.ResultingStackId, out PlacedItem split), Is.True);
            Assert.That(split.Count, Is.EqualTo(6));
            Assert.That(split.State, Is.EqualTo(state));
            Assert.That(grid.CountOf(type), Is.EqualTo(13));
        }

        [Test]
        public void SplitIntoAnotherInventoryPreservesQuantityAndNotifiesAfterCommit()
        {
            var source = new GridInventory(2, 1);
            var destination = new GridInventory(2, 1);
            ItemType type = Stackable();
            Guid id = Guid.NewGuid();
            source.Place(type, 0, 0, count: 9, itemInstanceId: id);
            bool observedCommittedState = false;
            source.Changed += changedInventory =>
                observedCommittedState = source.CountOf(type) == 5 && destination.CountOf(type) == 4;

            StackOperationResult result = StackOperations.Split(source, destination, id, 4, 0, 0);

            Assert.That(result.Success, Is.True);
            Assert.That(observedCommittedState, Is.True);
            Assert.That(source.CountOf(type) + destination.CountOf(type), Is.EqualTo(9));
        }

        [Test]
        public void SplitRejectsTakingEntireStackAndLeavesStateUntouched()
        {
            var grid = new GridInventory(2, 1);
            ItemType type = Stackable();
            Guid id = Guid.NewGuid();
            grid.Place(type, 0, 0, count: 5, itemInstanceId: id);
            int changes = 0;
            grid.Changed += changedInventory => changes++;

            StackOperationResult result = StackOperations.Split(grid, grid, id, 5, 1, 0);

            Assert.That(result.Status, Is.EqualTo(StackOperationStatus.InsufficientQuantity));
            Assert.That(grid.CountOf(type), Is.EqualTo(5));
            Assert.That(changes, Is.Zero);
        }

        [Test]
        public void SplitIntoBlockedCellDoesNotChangeSource()
        {
            var grid = new GridInventory(2, 1);
            ItemType type = Stackable();
            Guid id = Guid.NewGuid();
            grid.Place(type, 0, 0, count: 5, itemInstanceId: id);
            grid.Place(Stackable(), 1, 0);

            StackOperationResult result = StackOperations.Split(grid, grid, id, 2, 1, 0);

            Assert.That(result.Status, Is.EqualTo(StackOperationStatus.DestinationFull));
            Assert.That(grid.CountOf(type), Is.EqualTo(5));
        }

        [Test]
        public void MergeKeepsDestinationIdentityAndRecordsSourceLineage()
        {
            var grid = new GridInventory(3, 1);
            ItemType type = Stackable(10);
            Guid sourceId = Guid.NewGuid();
            Guid destinationId = Guid.NewGuid();
            grid.Place(type, 0, 0, count: 7, itemInstanceId: sourceId);
            grid.Place(type, 1, 0, count: 2, itemInstanceId: destinationId);
            var lineage = new StackLineageBook();

            StackOperationResult result = StackOperations.Merge(grid, grid, sourceId, destinationId, 7, lineage);

            Assert.That(result.Success, Is.True);
            Assert.That(result.ResultingStackId, Is.EqualTo(destinationId));
            Assert.That(grid.TryGetItemInstance(sourceId, out _), Is.False);
            Assert.That(grid.TryGetItemInstance(destinationId, out PlacedItem survivor), Is.True);
            Assert.That(survivor.Count, Is.EqualTo(9));
            Assert.That(grid.CountOf(type), Is.EqualTo(9));
            Assert.That(lineage.Merges.Count, Is.EqualTo(1));
            Assert.That(lineage.Merges[0].SourceStackId, Is.EqualTo(sourceId));
            Assert.That(lineage.Merges[0].DestinationStackId, Is.EqualTo(destinationId));
            Assert.That(lineage.Merges[0].Quantity, Is.EqualTo(7));
        }

        [Test]
        public void PartialMergeKeepsSourceIdentityAndOnlyMovesRequestedQuantity()
        {
            var grid = new GridInventory(3, 1);
            ItemType type = Stackable(10);
            Guid sourceId = Guid.NewGuid();
            Guid destinationId = Guid.NewGuid();
            grid.Place(type, 0, 0, count: 8, itemInstanceId: sourceId);
            grid.Place(type, 1, 0, count: 3, itemInstanceId: destinationId);
            var lineage = new StackLineageBook();

            StackOperationResult result = StackOperations.Merge(grid, grid, sourceId, destinationId, 4, lineage);

            Assert.That(result.Success, Is.True);
            Assert.That(grid.TryGetItemInstance(sourceId, out PlacedItem remainder), Is.True);
            Assert.That(remainder.Count, Is.EqualTo(4));
            Assert.That(grid.TryGetItemInstance(destinationId, out PlacedItem target), Is.True);
            Assert.That(target.Count, Is.EqualTo(7));
            Assert.That(grid.CountOf(type), Is.EqualTo(11));
        }

        [Test]
        public void MergeRejectsDifferentInstanceState()
        {
            var grid = new GridInventory(2, 1);
            ItemType type = Stackable();
            Guid sourceId = Guid.NewGuid();
            Guid destinationId = Guid.NewGuid();
            grid.Place(type, 0, 0, count: 2, itemInstanceId: sourceId,
                state: new ItemInstanceState(ItemQuality.Poor, ItemCondition.Good, ItemAuthenticity.Genuine, ItemOwnershipStatus.Legitimate));
            grid.Place(type, 1, 0, count: 2, itemInstanceId: destinationId);
            var lineage = new StackLineageBook();

            StackOperationResult result = StackOperations.Merge(grid, grid, sourceId, destinationId, 1, lineage);

            Assert.That(result.Status, Is.EqualTo(StackOperationStatus.IncompatibleStacks));
            Assert.That(grid.CountOf(type), Is.EqualTo(4));
            Assert.That(lineage.Merges, Is.Empty);
        }

        [Test]
        public void MergeCapsAtMaxStackAndLeavesRemainder()
        {
            var grid = new GridInventory(3, 1);
            ItemType type = Stackable(10);
            Guid sourceId = Guid.NewGuid();
            Guid destinationId = Guid.NewGuid();
            grid.Place(type, 0, 0, count: 8, itemInstanceId: sourceId);
            grid.Place(type, 1, 0, count: 8, itemInstanceId: destinationId);
            var lineage = new StackLineageBook();

            StackOperationResult result = StackOperations.Merge(grid, grid, sourceId, destinationId, 8, lineage);

            Assert.That(result.Success, Is.True);
            Assert.That(result.Quantity, Is.EqualTo(2));
            Assert.That(grid.TryGetItemInstance(sourceId, out PlacedItem remainder), Is.True);
            Assert.That(remainder.Count, Is.EqualTo(6));
            Assert.That(grid.TryGetItemInstance(destinationId, out PlacedItem target), Is.True);
            Assert.That(target.Count, Is.EqualTo(10));
            Assert.That(grid.CountOf(type), Is.EqualTo(16));
        }

        [Test]
        public void DuplicateMergeOperationIdRollsBackWithoutChangingInventories()
        {
            var grid = new GridInventory(3, 1);
            ItemType type = Stackable();
            Guid sourceId = Guid.NewGuid();
            Guid destinationId = Guid.NewGuid();
            Guid operationId = Guid.NewGuid();
            grid.Place(type, 0, 0, count: 2, itemInstanceId: sourceId);
            grid.Place(type, 1, 0, count: 2, itemInstanceId: destinationId);
            var lineage = new StackLineageBook();
            Assert.That(lineage.TryRecordMerge(new StackMergeRecord(operationId, Guid.NewGuid(),
                Guid.NewGuid(), type.Id, 1)), Is.True);

            StackOperationResult result = StackOperations.Merge(grid, grid, sourceId, destinationId, 1, lineage, operationId);

            Assert.That(result.Success, Is.False);
            Assert.That(grid.TryGetItemInstance(sourceId, out PlacedItem source), Is.True);
            Assert.That(source.Count, Is.EqualTo(2));
            Assert.That(grid.TryGetItemInstance(destinationId, out PlacedItem destination), Is.True);
            Assert.That(destination.Count, Is.EqualTo(2));
            Assert.That(grid.CountOf(type), Is.EqualTo(4));
        }
    }
}
