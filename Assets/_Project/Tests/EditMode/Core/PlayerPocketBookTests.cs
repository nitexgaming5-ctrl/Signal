using System;
using NUnit.Framework;

namespace Signal.Core.Tests
{
    public sealed class PlayerPocketBookTests
    {
        private static ItemType Item(int width = 1, int height = 1, int maxStack = 5) =>
            new ItemType(Guid.NewGuid(), width, height, maxStack);

        [Test]
        public void CreatesExactlyThreeIndependentOneCellPockets()
        {
            var pockets = new PlayerPocketBook();
            Assert.That(PlayerPocketBook.PocketCount, Is.EqualTo(3));
            for (int i = 0; i < 3; i++)
            {
                Assert.That(pockets.GetPocket(i).Width, Is.EqualTo(1));
                Assert.That(pockets.GetPocket(i).Height, Is.EqualTo(1));
                Assert.That(pockets.GetPocket(i).Items, Is.Empty);
            }
        }

        [Test]
        public void OrdinarySearchSeesP1AndP2ButNeverHiddenP3()
        {
            var pockets = new PlayerPocketBook();
            ItemType type = Item();
            Assert.That(pockets.TryAdd(type, Guid.NewGuid(), 1, ItemInstanceState.Default, out int first), Is.True);
            Assert.That(first, Is.EqualTo(0));
            Assert.That(pockets.TryAdd(type, Guid.NewGuid(), 1, ItemInstanceState.Default, out int second), Is.True);
            Assert.That(second, Is.EqualTo(1));
            Assert.That(pockets.TryAdd(type, Guid.NewGuid(), 1, ItemInstanceState.Default, out int hidden), Is.True);
            Assert.That(hidden, Is.EqualTo(PlayerPocketBook.HiddenPocketIndex));

            Assert.That(pockets.IsExposedToOrdinarySearch(0), Is.True);
            Assert.That(pockets.IsExposedToOrdinarySearch(1), Is.True);
            Assert.That(pockets.IsExposedToOrdinarySearch(2), Is.False);
            Assert.That(pockets.GetItemsVisibleToOrdinarySearch().Count, Is.EqualTo(2));
            Guid hiddenId = pockets.GetPocket(2).Items[0].ItemInstanceId;
            Assert.That(pockets.GetItemsVisibleToOrdinarySearch()[0].ItemInstanceId, Is.Not.EqualTo(hiddenId));
            Assert.That(pockets.GetItemsVisibleToOrdinarySearch()[1].ItemInstanceId, Is.Not.EqualTo(hiddenId));
        }

        [Test]
        public void FindsFirstEmptyPocketOnlyForOneByOneItems()
        {
            var pockets = new PlayerPocketBook();
            Assert.That(pockets.TryFindAvailablePocket(Item(), out int index, out _), Is.True);
            Assert.That(index, Is.EqualTo(0));
            Assert.That(pockets.TryFindAvailablePocket(Item(2, 1), out _, out _), Is.False);
            Assert.That(pockets.TryFindAvailablePocket(Item(), out _, out _, true), Is.False);
        }

        [Test]
        public void AutomaticPocketFallbackUsesFirstSuitableEmptyPocketInOrder()
        {
            var pockets = new PlayerPocketBook();
            ItemType type = Item();

            Assert.That(pockets.TryAdd(type, Guid.NewGuid(), 1, ItemInstanceState.Default, out int first), Is.True);
            Assert.That(first, Is.EqualTo(0));
            Assert.That(pockets.TryFindAvailablePocket(type, out int second, out GridInventory secondPocket), Is.True);
            Assert.That(second, Is.EqualTo(1));
            Assert.That(secondPocket, Is.SameAs(pockets.GetPocket(1)));

            Assert.That(pockets.TryAdd(type, Guid.NewGuid(), 1, ItemInstanceState.Default, out int secondAdded), Is.True);
            Assert.That(secondAdded, Is.EqualTo(1));
            Assert.That(pockets.TryFindAvailablePocket(type, out int third, out GridInventory thirdPocket), Is.True);
            Assert.That(third, Is.EqualTo(2));
            Assert.That(thirdPocket, Is.SameAs(pockets.GetPocket(2)));

            Assert.That(pockets.TryAdd(type, Guid.NewGuid(), 1, ItemInstanceState.Default, out int thirdAdded), Is.True);
            Assert.That(thirdAdded, Is.EqualTo(2));
            Assert.That(pockets.TryFindAvailablePocket(type, out _, out _), Is.False);
            Assert.That(pockets.GetPocket(0).Items.Count, Is.EqualTo(1));
            Assert.That(pockets.GetPocket(1).Items.Count, Is.EqualTo(1));
            Assert.That(pockets.GetPocket(2).Items.Count, Is.EqualTo(1));
        }

        [Test]
        public void AddPreservesStableIdentityCountAndState()
        {
            var pockets = new PlayerPocketBook();
            ItemType type = Item();
            Guid id = Guid.NewGuid();
            var state = new ItemInstanceState(ItemQuality.Fine, ItemCondition.Worn,
                ItemAuthenticity.Genuine, ItemOwnershipStatus.Stolen);
            Assert.That(pockets.TryAdd(type, id, 3, state, out int index), Is.True);
            Assert.That(index, Is.EqualTo(0));
            PlacedItem stored = pockets.GetPocket(0).Items[0];
            Assert.That(stored.ItemInstanceId, Is.EqualTo(id));
            Assert.That(stored.Count, Is.EqualTo(3));
            Assert.That(stored.State, Is.EqualTo(state));
        }

        [Test]
        public void RejectsLargeRotatedOrFullPocketItemsWithoutMutation()
        {
            var pockets = new PlayerPocketBook();
            Assert.That(pockets.TryAdd(Item(2, 1), Guid.NewGuid(), 1, ItemInstanceState.Default, out _), Is.False);
            Assert.That(pockets.TryAdd(Item(1, 2), Guid.NewGuid(), 1, ItemInstanceState.Default, out _), Is.False);
            var rotatedSource = new GridInventory(1, 1);
            ItemType rotatedType = Item();
            rotatedSource.Place(rotatedType, 0, 0, true, itemInstanceId: Guid.NewGuid());
            Assert.That(pockets.TryAdd(rotatedSource.Items[0], out _), Is.False);
            for (int i = 0; i < 3; i++)
                Assert.That(pockets.TryAdd(Item(), Guid.NewGuid(), 1, ItemInstanceState.Default, out _), Is.True);
            Assert.That(pockets.TryAdd(Item(), Guid.NewGuid(), 1, ItemInstanceState.Default, out _), Is.False);
            Assert.That(pockets.GetPocket(0).Items.Count, Is.EqualTo(1));
            Assert.That(pockets.GetPocket(1).Items.Count, Is.EqualTo(1));
            Assert.That(pockets.GetPocket(2).Items.Count, Is.EqualTo(1));
        }

        [Test]
        public void MovesExactStackBetweenBackpackAndPocket()
        {
            var pockets = new PlayerPocketBook();
            var backpack = new GridInventory(2, 2);
            ItemType type = Item();
            Guid id = Guid.NewGuid();
            int localId = backpack.Place(type, 0, 0, count: 4, itemInstanceId: id).Value;

            Assert.That(pockets.TryMoveFrom(backpack, id, 2), Is.True);
            Assert.That(backpack.Items, Is.Empty);
            Assert.That(pockets.GetPocket(2).Items[0].ItemInstanceId, Is.EqualTo(id));
            Assert.That(pockets.GetPocket(2).Items[0].Count, Is.EqualTo(4));

            Assert.That(pockets.TryMoveTo(backpack, 2), Is.True);
            Assert.That(pockets.GetPocket(2).Items, Is.Empty);
            Assert.That(backpack.TryGet(localId, out _), Is.False);
            Assert.That(backpack.Items[0].ItemInstanceId, Is.EqualTo(id));
            Assert.That(backpack.Items[0].Count, Is.EqualTo(4));
        }

        [Test]
        public void SaveMapperRoundTripsPocketContentsAndPlayerOwnership()
        {
            var pockets = new PlayerPocketBook();
            var ownership = new AssetOwnershipBook();
            var data = new SaveData();
            ItemType type = Item();
            Guid id = Guid.NewGuid();
            var state = new ItemInstanceState(ItemQuality.Exceptional, ItemCondition.Damaged,
                ItemAuthenticity.Counterfeit, ItemOwnershipStatus.Stolen);
            Assert.That(pockets.TryAdd(type, id, 2, state, out int index), Is.True);
            Assert.That(index, Is.EqualTo(0));
            Assert.That(ownership.TryRegister(id, EconomicActorId.Player), Is.True);

            PlayerPocketSaveMapper.Capture(pockets, ownership, data);
            var restored = new PlayerPocketBook();
            var restoredOwnership = new AssetOwnershipBook();
            int skipped = PlayerPocketSaveMapper.Apply(data, restored,
                candidate => candidate == type.Id ? type : (ItemType?)null, restoredOwnership);

            Assert.That(skipped, Is.EqualTo(0));
            PlacedItem item = restored.GetPocket(0).Items[0];
            Assert.That(item.ItemInstanceId, Is.EqualTo(id));
            Assert.That(item.Count, Is.EqualTo(2));
            Assert.That(item.State, Is.EqualTo(state));
            Assert.That(restoredOwnership.TryGetOwner(id, out EconomicActorId owner), Is.True);
            Assert.That(owner, Is.EqualTo(EconomicActorId.Player));
        }

        [Test]
        public void SaveMapperSkipsDuplicatePocketSlotAndMalformedEntries()
        {
            var data = new SaveData();
            ItemType type = Item();
            Guid id = Guid.NewGuid();
            data.pocketItems.Add(new SavedPocketItem
            {
                pocketIndex = 0, typeId = type.Id.ToString(), instanceId = id.ToString(),
                count = 1, quality = (int)ItemQuality.Standard, condition = (int)ItemCondition.Good,
                authenticity = (int)ItemAuthenticity.Genuine, ownership = (int)ItemOwnershipStatus.Legitimate
            });
            data.pocketItems.Add(new SavedPocketItem
            {
                pocketIndex = 0, typeId = type.Id.ToString(), instanceId = Guid.NewGuid().ToString(),
                count = 1, quality = (int)ItemQuality.Standard, condition = (int)ItemCondition.Good,
                authenticity = (int)ItemAuthenticity.Genuine, ownership = (int)ItemOwnershipStatus.Legitimate
            });
            data.pocketItems.Add(new SavedPocketItem { pocketIndex = 4, typeId = "bad", instanceId = "bad" });

            var restored = new PlayerPocketBook();
            int skipped = PlayerPocketSaveMapper.Apply(data, restored,
                candidate => candidate == type.Id ? type : (ItemType?)null, new AssetOwnershipBook());

            Assert.That(skipped, Is.EqualTo(2));
            Assert.That(restored.GetPocket(0).Items.Count, Is.EqualTo(1));
            Assert.That(restored.GetPocket(0).Items[0].ItemInstanceId, Is.EqualTo(id));
        }

        [Test]
        public void MovesStackDirectlyIntoAnEmptyPocket()
        {
            var pockets = new PlayerPocketBook();
            ItemType type = Item();
            Guid id = Guid.NewGuid();
            var state = new ItemInstanceState(ItemQuality.Fine, ItemCondition.Worn,
                ItemAuthenticity.Genuine, ItemOwnershipStatus.Stolen);
            Assert.That(pockets.TryAdd(type, id, 3, state, out int source), Is.True);
            Assert.That(source, Is.EqualTo(0));

            Assert.That(pockets.TryMoveBetweenPockets(0, 2), Is.True);
            Assert.That(pockets.GetPocket(0).Items, Is.Empty);
            PlacedItem moved = pockets.GetPocket(2).Items[0];
            Assert.That(moved.ItemInstanceId, Is.EqualTo(id));
            Assert.That(moved.Count, Is.EqualTo(3));
            Assert.That(moved.State, Is.EqualTo(state));
        }

        [Test]
        public void SwapsOccupiedPocketsWithoutChangingEitherStack()
        {
            var pockets = new PlayerPocketBook();
            ItemType firstType = Item();
            ItemType secondType = Item();
            Guid firstId = Guid.NewGuid();
            Guid secondId = Guid.NewGuid();
            var firstState = new ItemInstanceState(ItemQuality.Fine, ItemCondition.Worn,
                ItemAuthenticity.Genuine, ItemOwnershipStatus.Stolen);
            var secondState = new ItemInstanceState(ItemQuality.Exceptional, ItemCondition.Damaged,
                ItemAuthenticity.Counterfeit, ItemOwnershipStatus.Legitimate);

            Assert.That(pockets.TryAdd(firstType, firstId, 2, firstState, out int firstPocket), Is.True);
            Assert.That(pockets.TryAdd(secondType, secondId, 4, secondState, out int secondPocket), Is.True);
            Assert.That(firstPocket, Is.EqualTo(0));
            Assert.That(secondPocket, Is.EqualTo(1));

            Assert.That(pockets.TryMoveBetweenPockets(0, 1), Is.True);

            PlacedItem nowFirst = pockets.GetPocket(0).Items[0];
            PlacedItem nowSecond = pockets.GetPocket(1).Items[0];
            Assert.That(nowFirst.ItemInstanceId, Is.EqualTo(secondId));
            Assert.That(nowFirst.Type, Is.EqualTo(secondType));
            Assert.That(nowFirst.Count, Is.EqualTo(4));
            Assert.That(nowFirst.State, Is.EqualTo(secondState));
            Assert.That(nowSecond.ItemInstanceId, Is.EqualTo(firstId));
            Assert.That(nowSecond.Type, Is.EqualTo(firstType));
            Assert.That(nowSecond.Count, Is.EqualTo(2));
            Assert.That(nowSecond.State, Is.EqualTo(firstState));
        }

        [Test]
        public void RejectsInvalidPocketMoveWithoutChangingEitherPocket()
        {
            var pockets = new PlayerPocketBook();
            ItemType type = Item();
            Guid id = Guid.NewGuid();
            Assert.That(pockets.TryAdd(type, id, 1, ItemInstanceState.Default, out _), Is.True);

            Assert.That(pockets.TryMoveBetweenPockets(0, 0), Is.False);
            Assert.That(pockets.TryMoveBetweenPockets(1, 0), Is.False);
            Assert.That(pockets.GetPocket(0).Items[0].ItemInstanceId, Is.EqualTo(id));
            Assert.That(pockets.GetPocket(1).Items, Is.Empty);
        }

        [Test]
        public void MovesPocketItemToExactBackpackDropCell()
        {
            var pockets = new PlayerPocketBook();
            var backpack = new GridInventory(3, 2);
            // Pocket contents must be 1x1; this test verifies exact destination placement.
            ItemType type = Item(1, 1);
            Guid id = Guid.NewGuid();
            Assert.That(pockets.TryAdd(type, id, 1, ItemInstanceState.Default, out int pocketIndex), Is.True);

            Assert.That(pockets.TryMoveToAt(backpack, pocketIndex, 1, 1), Is.True);
            Assert.That(pockets.GetPocket(pocketIndex).Items, Is.Empty);
            Assert.That(backpack.TryGetItemInstance(id, out PlacedItem moved), Is.True);
            Assert.That(moved.X, Is.EqualTo(1));
            Assert.That(moved.Y, Is.EqualTo(1));
            Assert.That(moved.Type, Is.EqualTo(type));
        }

        [Test]
        public void FailedExactPocketDropKeepsItemInPocket()
        {
            var pockets = new PlayerPocketBook();
            var backpack = new GridInventory(2, 2);
            ItemType type = Item(1, 1);
            Guid pocketId = Guid.NewGuid();
            Guid blockerId = Guid.NewGuid();
            Assert.That(pockets.TryAdd(type, pocketId, 1, ItemInstanceState.Default, out int pocketIndex), Is.True);
            backpack.Place(type, 0, 0, itemInstanceId: blockerId);

            Assert.That(pockets.TryMoveToAt(backpack, pocketIndex, 0, 0), Is.False);
            Assert.That(pockets.GetPocket(pocketIndex).Items[0].ItemInstanceId, Is.EqualTo(pocketId));
            Assert.That(backpack.Items.Count, Is.EqualTo(1));
        }

        [Test]
        public void FailedPocketTransferLeavesBothInventoriesUnchanged()
        {
            var pockets = new PlayerPocketBook();
            var backpack = new GridInventory(1, 1);
            ItemType type = Item();
            Guid first = Guid.NewGuid();
            Guid second = Guid.NewGuid();
            pockets.GetPocket(0).Place(type, 0, 0, itemInstanceId: first);
            backpack.Place(type, 0, 0, itemInstanceId: second);

            Assert.That(pockets.TryMoveTo(backpack, 0), Is.False);
            Assert.That(pockets.GetPocket(0).Items[0].ItemInstanceId, Is.EqualTo(first));
            Assert.That(backpack.Items[0].ItemInstanceId, Is.EqualTo(second));
        }
    }
}