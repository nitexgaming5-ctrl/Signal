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
        public void FindsFirstEmptyPocketOnlyForOneByOneItems()
        {
            var pockets = new PlayerPocketBook();
            Assert.That(pockets.TryFindAvailablePocket(Item(), out int index, out _), Is.True);
            Assert.That(index, Is.EqualTo(0));
            Assert.That(pockets.TryFindAvailablePocket(Item(2, 1), out _, out _), Is.False);
            Assert.That(pockets.TryFindAvailablePocket(Item(), out _, out _, true), Is.False);
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