using System;
using NUnit.Framework;

namespace Signal.Core.Tests
{
    public sealed class GridInventoryTests
    {
        private GridInventory _grid;

        [SetUp]
        public void SetUp()
        {
            _grid = new GridInventory();
        }

        private static ItemType Item(int width = 1, int height = 1, int maxStack = 1)
        {
            return new ItemType(Guid.NewGuid(), width, height, maxStack);
        }

        [Test]
        public void DefaultGridIs6By4()
        {
            Assert.That(_grid.Width, Is.EqualTo(6));
            Assert.That(_grid.Height, Is.EqualTo(4));
        }

        [Test]
        public void ConstructorWithWidthZeroThrows()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new GridInventory(0, 4));
        }

        [Test]
        public void ConstructorWithHeightZeroThrows()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new GridInventory(6, 0));
        }

        [Test]
        public void PlaceInsideBoundsReturnsInstanceId()
        {
            int? id = _grid.Place(Item(), 0, 0);

            Assert.That(id.HasValue, Is.True);
            Assert.That(id.Value, Is.GreaterThanOrEqualTo(1));
            Assert.That(_grid.Items.Count, Is.EqualTo(1));
        }

        [Test]
        public void InstanceIdsStartAtOneAndIncrease()
        {
            int? first = _grid.Place(Item(), 0, 0);
            int? second = _grid.Place(Item(), 1, 0);

            Assert.That(first, Is.EqualTo(1));
            Assert.That(second, Is.EqualTo(2));
        }

        [Test]
        public void PlacedItemRecordsWhatWasPlaced()
        {
            var type = Item(2, 1, 5);

            _grid.Place(type, 3, 2, rotated: true, count: 4);

            PlacedItem placed = _grid.Items[0];
            Assert.That(placed.Type, Is.EqualTo(type));
            Assert.That(placed.X, Is.EqualTo(3));
            Assert.That(placed.Y, Is.EqualTo(2));
            Assert.That(placed.Rotated, Is.True);
            Assert.That(placed.Count, Is.EqualTo(4));
        }

        [Test]
        public void PlacePastRightEdgeIsRejected()
        {
            int? id = _grid.Place(Item(3, 1), 4, 0);

            Assert.That(id.HasValue, Is.False);
            Assert.That(_grid.Items.Count, Is.EqualTo(0));
        }

        [Test]
        public void PlaceFlushAgainstRightEdgeSucceeds()
        {
            int? id = _grid.Place(Item(3, 1), 3, 0);

            Assert.That(id.HasValue, Is.True);
        }

        [Test]
        public void PlacePastBottomEdgeIsRejected()
        {
            Assert.That(_grid.Place(Item(1, 2), 0, 3).HasValue, Is.False);
            Assert.That(_grid.Place(Item(1, 2), 0, 2).HasValue, Is.True);
        }

        [TestCase(-1, 0)]
        [TestCase(0, -1)]
        public void PlaceAtNegativePositionIsRejected(int x, int y)
        {
            int? id = _grid.Place(Item(), x, y);

            Assert.That(id.HasValue, Is.False);
            Assert.That(_grid.Items.Count, Is.EqualTo(0));
        }

        [Test]
        public void PlaceAtHugeCoordinateIsRejectedWithoutOverflow()
        {
            Assert.That(_grid.Place(Item(3, 1), int.MaxValue, 0).HasValue, Is.False);
        }

        [Test]
        public void PlaceOverExistingItemIsRejected()
        {
            _grid.Place(Item(2, 2), 0, 0);

            int? id = _grid.Place(Item(), 1, 1);

            Assert.That(id.HasValue, Is.False);
            Assert.That(_grid.Items.Count, Is.EqualTo(1));
        }

        [Test]
        public void PlaceEdgeAdjacentToExistingItemSucceeds()
        {
            _grid.Place(Item(), 0, 0);

            int? id = _grid.Place(Item(), 1, 0);

            Assert.That(id.HasValue, Is.True);
            Assert.That(_grid.Items.Count, Is.EqualTo(2));
        }

        [Test]
        public void RotatedItemSwapsWidthAndHeight()
        {
            var tall = Item(1, 3);

            Assert.That(_grid.CanPlace(tall, 3, 0, true), Is.True);
            Assert.That(_grid.CanPlace(tall, 4, 0, true), Is.False);
            Assert.That(_grid.CanPlace(tall, 5, 0, true), Is.False);
            Assert.That(_grid.CanPlace(tall, 5, 0, false), Is.True);
        }

        [Test]
        public void RotatedItemBlocksTheSwappedFootprint()
        {
            _grid.Place(Item(1, 3), 0, 0, rotated: true);

            Assert.That(_grid.Place(Item(), 2, 0).HasValue, Is.False);
            Assert.That(_grid.Place(Item(), 0, 1).HasValue, Is.True);
        }

        [Test]
        public void CountAboveMaxStackIsRejected()
        {
            int? id = _grid.Place(Item(maxStack: 1), 0, 0, count: 5);

            Assert.That(id.HasValue, Is.False);
            Assert.That(_grid.Items.Count, Is.EqualTo(0));
        }

        [Test]
        public void CountEqualToMaxStackSucceeds()
        {
            Assert.That(_grid.Place(Item(maxStack: 5), 0, 0, count: 5).HasValue, Is.True);
        }

        [TestCase(0)]
        [TestCase(-2)]
        public void CountBelowOneIsRejected(int count)
        {
            int? id = _grid.Place(Item(), 0, 0, count: count);

            Assert.That(id.HasValue, Is.False);
            Assert.That(_grid.Items.Count, Is.EqualTo(0));
        }

        [Test]
        public void ChangedFiresOnlyForSuccessfulPlace()
        {
            int fired = 0;
            _grid.Changed += _ => fired++;

            _grid.Place(Item(), 0, 0);
            _grid.Place(Item(), 0, 0);
            _grid.Place(Item(), -1, 0);
            _grid.Place(Item(), 1, 0, count: 5);

            Assert.That(fired, Is.EqualTo(1));
        }

        [Test]
        public void RemoveExistingItemReturnsTrueAndRemovesIt()
        {
            int id = _grid.Place(Item(), 0, 0).Value;

            bool removed = _grid.Remove(id);

            Assert.That(removed, Is.True);
            Assert.That(_grid.Items.Count, Is.EqualTo(0));
            Assert.That(_grid.TryGet(id, out _), Is.False);
        }

        [Test]
        public void RemoveFreesTheCells()
        {
            int id = _grid.Place(Item(2, 2), 0, 0).Value;

            _grid.Remove(id);

            Assert.That(_grid.Place(Item(2, 2), 0, 0).HasValue, Is.True);
        }

        [Test]
        public void RemoveUnknownIdReturnsFalseAndFiresNoChanged()
        {
            _grid.Place(Item(), 0, 0);
            int fired = 0;
            _grid.Changed += _ => fired++;

            bool removed = _grid.Remove(999);

            Assert.That(removed, Is.False);
            Assert.That(fired, Is.EqualTo(0));
            Assert.That(_grid.Items.Count, Is.EqualTo(1));
        }

        [Test]
        public void RemoveFiresChangedOnce()
        {
            int id = _grid.Place(Item(), 0, 0).Value;
            int fired = 0;
            _grid.Changed += _ => fired++;

            _grid.Remove(id);

            Assert.That(fired, Is.EqualTo(1));
        }

        [Test]
        public void TryGetReturnsThePlacedData()
        {
            var type = Item(1, 2, 5);
            int id = _grid.Place(type, 2, 1, count: 3).Value;

            bool found = _grid.TryGet(id, out PlacedItem item);

            Assert.That(found, Is.True);
            Assert.That(item.InstanceId, Is.EqualTo(id));
            Assert.That(item.Type, Is.EqualTo(type));
            Assert.That(item.X, Is.EqualTo(2));
            Assert.That(item.Y, Is.EqualTo(1));
            Assert.That(item.Count, Is.EqualTo(3));
        }

        [Test]
        public void TryGetUnknownIdReturnsFalse()
        {
            bool found = _grid.TryGet(42, out PlacedItem item);

            Assert.That(found, Is.False);
            Assert.That(item.InstanceId, Is.EqualTo(0));
        }

        [Test]
        public void MoveToFreeCellSucceeds()
        {
            int id = _grid.Place(Item(maxStack: 5), 0, 0, count: 3).Value;

            bool moved = _grid.TryMove(id, 4, 2, false);

            Assert.That(moved, Is.True);
            _grid.TryGet(id, out PlacedItem item);
            Assert.That(item.X, Is.EqualTo(4));
            Assert.That(item.Y, Is.EqualTo(2));
            Assert.That(item.InstanceId, Is.EqualTo(id));
            Assert.That(item.Count, Is.EqualTo(3));
            Assert.That(_grid.Items.Count, Is.EqualTo(1));
        }

        [Test]
        public void MoveOntoAnotherItemFailsAndKeepsOldPosition()
        {
            _grid.Place(Item(), 0, 0);
            int id = _grid.Place(Item(), 2, 0).Value;

            bool moved = _grid.TryMove(id, 0, 0, false);

            Assert.That(moved, Is.False);
            _grid.TryGet(id, out PlacedItem item);
            Assert.That(item.X, Is.EqualTo(2));
            Assert.That(item.Y, Is.EqualTo(0));
        }

        [Test]
        public void MoveOverlappingOwnOldFootprintSucceeds()
        {
            int id = _grid.Place(Item(2, 1), 0, 0).Value;

            bool moved = _grid.TryMove(id, 1, 0, false);

            Assert.That(moved, Is.True);
            _grid.TryGet(id, out PlacedItem item);
            Assert.That(item.X, Is.EqualTo(1));
        }

        [Test]
        public void MoveOutOfBoundsFailsAndChangesNothing()
        {
            int id = _grid.Place(Item(3, 1), 0, 0).Value;

            bool moved = _grid.TryMove(id, 4, 0, false);

            Assert.That(moved, Is.False);
            _grid.TryGet(id, out PlacedItem item);
            Assert.That(item.X, Is.EqualTo(0));
            Assert.That(item.Y, Is.EqualTo(0));
        }

        [Test]
        public void MoveUnknownIdReturnsFalse()
        {
            Assert.That(_grid.TryMove(999, 0, 0, false), Is.False);
        }

        [Test]
        public void MoveCanRotateTheItem()
        {
            int id = _grid.Place(Item(1, 3), 0, 0).Value;

            bool moved = _grid.TryMove(id, 0, 0, true);

            Assert.That(moved, Is.True);
            _grid.TryGet(id, out PlacedItem item);
            Assert.That(item.Rotated, Is.True);
        }

        [Test]
        public void SuccessfulMoveFiresChangedOnce()
        {
            int id = _grid.Place(Item(), 0, 0).Value;
            int fired = 0;
            _grid.Changed += _ => fired++;

            _grid.TryMove(id, 3, 3, false);

            Assert.That(fired, Is.EqualTo(1));
        }

        [Test]
        public void FailedMoveFiresNoChanged()
        {
            int id = _grid.Place(Item(), 0, 0).Value;
            int fired = 0;
            _grid.Changed += _ => fired++;

            _grid.TryMove(id, -1, 0, false);
            _grid.TryMove(999, 0, 0, false);

            Assert.That(fired, Is.EqualTo(0));
        }

        // ---------- M2-04: rotate in place ----------

        [Test]
        public void RotateTurnsItemAndKeepsPositionAndCount()
        {
            int id = _grid.Place(Item(1, 3, 5), 0, 0, count: 2).Value;

            bool rotated = _grid.TryRotate(id);

            Assert.That(rotated, Is.True);
            _grid.TryGet(id, out PlacedItem item);
            Assert.That(item.Rotated, Is.True);
            Assert.That(item.X, Is.EqualTo(0));
            Assert.That(item.Y, Is.EqualTo(0));
            Assert.That(item.Count, Is.EqualTo(2));
        }

        [Test]
        public void RotatingTwiceRestoresTheOriginalOrientation()
        {
            int id = _grid.Place(Item(1, 3), 0, 0).Value;

            _grid.TryRotate(id);
            bool second = _grid.TryRotate(id);

            Assert.That(second, Is.True);
            _grid.TryGet(id, out PlacedItem item);
            Assert.That(item.Rotated, Is.False);
        }

        [Test]
        public void RotateFailsAtRightEdgeAndStaysUnrotated()
        {
            int id = _grid.Place(Item(1, 3), 5, 0).Value;

            bool rotated = _grid.TryRotate(id);

            Assert.That(rotated, Is.False);
            _grid.TryGet(id, out PlacedItem item);
            Assert.That(item.Rotated, Is.False);
            Assert.That(item.X, Is.EqualTo(5));
        }

        [Test]
        public void RotateFailsWhenANeighbourBlocksTheNewFootprint()
        {
            int id = _grid.Place(Item(1, 3), 0, 0).Value;
            _grid.Place(Item(), 1, 0);

            bool rotated = _grid.TryRotate(id);

            Assert.That(rotated, Is.False);
            _grid.TryGet(id, out PlacedItem item);
            Assert.That(item.Rotated, Is.False);
        }

        [Test]
        public void RotateUnknownIdReturnsFalse()
        {
            Assert.That(_grid.TryRotate(999), Is.False);
        }

        [Test]
        public void FailedRotateFiresNoChanged()
        {
            int id = _grid.Place(Item(1, 3), 5, 0).Value;
            int fired = 0;
            _grid.Changed += _ => fired++;

            _grid.TryRotate(id);
            _grid.TryRotate(999);

            Assert.That(fired, Is.EqualTo(0));
        }

        [Test]
        public void SuccessfulRotateFiresChangedOnce()
        {
            int id = _grid.Place(Item(1, 3), 0, 0).Value;
            int fired = 0;
            _grid.Changed += _ => fired++;

            _grid.TryRotate(id);

            Assert.That(fired, Is.EqualTo(1));
        }

        // ---------- M2-05: stacking and Add ----------

        [Test]
        public void AddToEmptyGridCreatesOneStack()
        {
            var type = Item(maxStack: 5);

            int leftover = _grid.Add(type, 3);

            Assert.That(leftover, Is.EqualTo(0));
            Assert.That(_grid.Items.Count, Is.EqualTo(1));
            Assert.That(_grid.Items[0].Count, Is.EqualTo(3));
            Assert.That(_grid.Items[0].X, Is.EqualTo(0));
            Assert.That(_grid.Items[0].Y, Is.EqualTo(0));
        }

        [Test]
        public void AddOverMaxStackSplitsIntoSeveralStacks()
        {
            var type = Item(maxStack: 5);

            int leftover = _grid.Add(type, 7);

            Assert.That(leftover, Is.EqualTo(0));
            Assert.That(_grid.Items.Count, Is.EqualTo(2));
            Assert.That(_grid.Items[0].Count, Is.EqualTo(5));
            Assert.That(_grid.Items[1].Count, Is.EqualTo(2));
            Assert.That(_grid.Items[1].X, Is.EqualTo(1));
            Assert.That(_grid.Items[1].Y, Is.EqualTo(0));
        }

        [Test]
        public void AddTopsUpAnExistingStackWithoutCreatingANewOne()
        {
            var type = Item(maxStack: 5);
            _grid.Place(type, 0, 0, count: 3);

            int leftover = _grid.Add(type, 2);

            Assert.That(leftover, Is.EqualTo(0));
            Assert.That(_grid.Items.Count, Is.EqualTo(1));
            Assert.That(_grid.Items[0].Count, Is.EqualTo(5));
        }

        [Test]
        public void AddToAFullGridReturnsTheFullCount()
        {
            var grid = new GridInventory(2, 1);
            grid.Add(Item(), 2);

            int leftover = grid.Add(Item(), 3);

            Assert.That(leftover, Is.EqualTo(3));
            Assert.That(grid.Items.Count, Is.EqualTo(2));
        }

        [TestCase(0)]
        [TestCase(-3)]
        public void AddZeroOrNegativeReturnsZeroAndFiresNothing(int count)
        {
            int fired = 0;
            _grid.Changed += _ => fired++;

            int leftover = _grid.Add(Item(maxStack: 5), count);

            Assert.That(leftover, Is.EqualTo(0));
            Assert.That(fired, Is.EqualTo(0));
            Assert.That(_grid.Items.Count, Is.EqualTo(0));
        }

        [Test]
        public void AddThatPartlyFitsPlacesWhatFitsAndReturnsTheRest()
        {
            var grid = new GridInventory(3, 1);
            grid.Place(Item(), 0, 0);
            var type = Item(maxStack: 5);

            int leftover = grid.Add(type, 12);

            Assert.That(leftover, Is.EqualTo(2));
            Assert.That(grid.Items.Count, Is.EqualTo(3));
        }

        [Test]
        public void AddNeverMovesAnExistingItem()
        {
            var type = Item(maxStack: 5);
            int id = _grid.Place(type, 3, 2, count: 1).Value;

            _grid.Add(type, 2);

            _grid.TryGet(id, out PlacedItem item);
            Assert.That(item.X, Is.EqualTo(3));
            Assert.That(item.Y, Is.EqualTo(2));
            Assert.That(item.Count, Is.EqualTo(3));
            Assert.That(_grid.Items.Count, Is.EqualTo(1));
        }

        [Test]
        public void AddFiresChangedOncePerCall()
        {
            var type = Item(maxStack: 5);
            _grid.Place(type, 0, 0, count: 4);
            int fired = 0;
            _grid.Changed += _ => fired++;

            _grid.Add(type, 8);

            Assert.That(fired, Is.EqualTo(1));
            Assert.That(_grid.Items.Count, Is.EqualTo(3));
        }

        [Test]
        public void AddToAFullGridFiresNoChanged()
        {
            var grid = new GridInventory(1, 1);
            grid.Place(Item(), 0, 0);
            int fired = 0;
            grid.Changed += _ => fired++;

            int leftover = grid.Add(Item(), 1);

            Assert.That(leftover, Is.EqualTo(1));
            Assert.That(fired, Is.EqualTo(0));
        }

        [Test]
        public void AddPlacesAtTheFirstFreeSpotForABigItem()
        {
            _grid.Place(Item(), 0, 0);

            int leftover = _grid.Add(Item(2, 2), 1);

            Assert.That(leftover, Is.EqualTo(0));
            Assert.That(_grid.Items[1].X, Is.EqualTo(1));
            Assert.That(_grid.Items[1].Y, Is.EqualTo(0));
        }

        [Test]
        public void AddWithAnUninitialisedItemTypeStoresNothing()
        {
            int leftover = _grid.Add(default(ItemType), 3);

            Assert.That(leftover, Is.EqualTo(3));
            Assert.That(_grid.Items.Count, Is.EqualTo(0));
        }

        // ---------- M3-01: queries used by the UI ----------

        [Test]
        public void PlacedItemFootprintSwapsWhenRotated()
        {
            var type = Item(1, 3);
            var upright = new PlacedItem(1, type, 0, 0, false, 1);
            var turned = new PlacedItem(2, type, 0, 0, true, 1);

            Assert.That(upright.FootprintWidth, Is.EqualTo(1));
            Assert.That(upright.FootprintHeight, Is.EqualTo(3));
            Assert.That(turned.FootprintWidth, Is.EqualTo(3));
            Assert.That(turned.FootprintHeight, Is.EqualTo(1));
        }

        [Test]
        public void TryGetItemAtFindsEveryCoveredCell()
        {
            int id = _grid.Place(Item(2, 2), 1, 1).Value;

            for (int y = 1; y <= 2; y++)
            {
                for (int x = 1; x <= 2; x++)
                {
                    bool found = _grid.TryGetItemAt(x, y, out PlacedItem item);

                    Assert.That(found, Is.True);
                    Assert.That(item.InstanceId, Is.EqualTo(id));
                }
            }
        }

        [Test]
        public void TryGetItemAtReturnsFalseOutsideItemsAndGrid()
        {
            _grid.Place(Item(2, 2), 1, 1);

            Assert.That(_grid.TryGetItemAt(0, 0, out _), Is.False);
            Assert.That(_grid.TryGetItemAt(3, 1, out _), Is.False);
            Assert.That(_grid.TryGetItemAt(1, 3, out _), Is.False);
            Assert.That(_grid.TryGetItemAt(-1, 0, out _), Is.False);
            Assert.That(_grid.TryGetItemAt(99, 99, out _), Is.False);
        }

        [Test]
        public void TryGetItemAtRespectsRotation()
        {
            _grid.Place(Item(1, 3), 0, 0, rotated: true);

            Assert.That(_grid.TryGetItemAt(2, 0, out _), Is.True);
            Assert.That(_grid.TryGetItemAt(0, 2, out _), Is.False);
        }

        [Test]
        public void CanMoveMatchesTryMoveWithoutChangingAnything()
        {
            _grid.Place(Item(), 0, 0);
            int id = _grid.Place(Item(), 2, 0).Value;
            int fired = 0;
            _grid.Changed += _ => fired++;

            Assert.That(_grid.CanMove(id, 4, 2, false), Is.True);
            Assert.That(_grid.CanMove(id, 0, 0, false), Is.False);
            Assert.That(_grid.CanMove(id, 6, 0, false), Is.False);

            _grid.TryGet(id, out PlacedItem item);
            Assert.That(item.X, Is.EqualTo(2));
            Assert.That(fired, Is.EqualTo(0));
        }

        [Test]
        public void CanMoveAllowsOverlapWithOwnFootprint()
        {
            int id = _grid.Place(Item(2, 1), 0, 0).Value;

            Assert.That(_grid.CanMove(id, 1, 0, false), Is.True);
        }

        [Test]
        public void CanMoveUnknownIdReturnsFalse()
        {
            Assert.That(_grid.CanMove(999, 0, 0, false), Is.False);
        }

        // ---------- stock queries, removal by quantity, snapshots ----------

        [Test]
        public void CountOfSumsAllStacksOfAType()
        {
            var type = Item(maxStack: 5);
            _grid.Place(type, 0, 0, count: 5);
            _grid.Place(type, 1, 0, count: 2);

            Assert.That(_grid.CountOf(type), Is.EqualTo(7));
        }

        [Test]
        public void CountOfIgnoresOtherTypes()
        {
            var wanted = Item(maxStack: 5);
            _grid.Place(wanted, 0, 0, count: 3);
            _grid.Place(Item(maxStack: 5), 1, 0, count: 4);

            Assert.That(_grid.CountOf(wanted), Is.EqualTo(3));
        }

        [Test]
        public void TryRemoveTakesFromStacksAndDropsEmptyOnes()
        {
            var type = Item(maxStack: 5);
            _grid.Place(type, 0, 0, count: 5);
            _grid.Place(type, 1, 0, count: 2);

            bool removed = _grid.TryRemove(type, 6);

            Assert.That(removed, Is.True);
            Assert.That(_grid.CountOf(type), Is.EqualTo(1));
            Assert.That(_grid.Items.Count, Is.EqualTo(1));
        }

        [Test]
        public void TryRemoveFailsWithoutEnoughAndChangesNothing()
        {
            var type = Item(maxStack: 5);
            _grid.Place(type, 0, 0, count: 3);
            int fired = 0;
            _grid.Changed += _ => fired++;

            bool removed = _grid.TryRemove(type, 4);

            Assert.That(removed, Is.False);
            Assert.That(_grid.CountOf(type), Is.EqualTo(3));
            Assert.That(fired, Is.EqualTo(0));
        }

        [TestCase(0)]
        [TestCase(-1)]
        public void TryRemoveRejectsZeroOrNegative(int count)
        {
            var type = Item(maxStack: 5);
            _grid.Place(type, 0, 0, count: 3);

            Assert.That(_grid.TryRemove(type, count), Is.False);
            Assert.That(_grid.CountOf(type), Is.EqualTo(3));
        }

        [Test]
        public void TryRemoveFiresChangedOnce()
        {
            var type = Item(maxStack: 5);
            _grid.Place(type, 0, 0, count: 5);
            _grid.Place(type, 1, 0, count: 5);
            int fired = 0;
            _grid.Changed += _ => fired++;

            _grid.TryRemove(type, 7);

            Assert.That(fired, Is.EqualTo(1));
        }

        [Test]
        public void ClearRemovesEverythingAndFiresOnce()
        {
            _grid.Place(Item(), 0, 0);
            _grid.Place(Item(), 1, 0);
            int fired = 0;
            _grid.Changed += _ => fired++;

            _grid.Clear();

            Assert.That(_grid.Items.Count, Is.EqualTo(0));
            Assert.That(fired, Is.EqualTo(1));
        }

        [Test]
        public void ClearOnAnEmptyGridFiresNothing()
        {
            int fired = 0;
            _grid.Changed += _ => fired++;

            _grid.Clear();

            Assert.That(fired, Is.EqualTo(0));
        }

        [Test]
        public void SnapshotRestoreBringsBackTheOldState()
        {
            int first = _grid.Place(Item(), 0, 0).Value;
            _grid.Place(Item(), 1, 0);
            InventorySnapshot snapshot = _grid.CreateSnapshot();

            _grid.Remove(first);
            int extra = _grid.Place(Item(), 4, 2).Value;
            _grid.Restore(snapshot);

            Assert.That(_grid.Items.Count, Is.EqualTo(2));
            Assert.That(_grid.TryGet(first, out _), Is.True);
            Assert.That(_grid.TryGet(extra, out _), Is.False);
            Assert.That(_grid.Place(Item(), 5, 3), Is.EqualTo(extra));
        }

        [Test]
        public void RestoreWithNullThrows()
        {
            Assert.Throws<ArgumentNullException>(() => _grid.Restore(null));
        }
    }
}
