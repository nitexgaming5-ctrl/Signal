using System;
using NUnit.Framework;
using Signal.Core;

namespace Signal.Core.Tests
{
    public sealed class SearchableContainerTests
    {
        private static ItemType Item(int maxStack = 5) => new ItemType(Guid.NewGuid(), 1, 1, maxStack);

        [Test]
        public void SearchOrderCanOnlyBeInitializedOnceAndRevealsOneItemAtATime()
        {
            var container = new SearchableContainer("street-bin-a");
            ItemType type = Item();
            container.Inventory.Place(type, 0, 0);
            container.Inventory.Place(type, 1, 0);
            Guid first = container.Inventory.Items[0].ItemInstanceId;
            Guid second = container.Inventory.Items[1].ItemInstanceId;

            Assert.That(container.TryInitializeSearchOrder(new[] { second, first }), Is.True);
            Assert.That(container.TryInitializeSearchOrder(new[] { first, second }), Is.False);
            Assert.That(container.DiscoveredCount, Is.Zero);
            Assert.That(container.TryStartSearch(), Is.True);
            Assert.That(container.TryStartSearch(), Is.False);
            Assert.That(container.TryRevealNext(out Guid revealed), Is.True);
            Assert.That(revealed, Is.EqualTo(second));
            Assert.That(container.Inventory.TryGetItemInstance(revealed, out _), Is.True);
            Assert.That(container.IsDiscovered(first), Is.False);
            Assert.That(container.TryRevealNext(out revealed), Is.True);
            Assert.That(revealed, Is.EqualTo(first));
            Assert.That(container.IsSearchComplete, Is.True);
            Assert.That(container.TryRevealNext(out _), Is.False);
        }

        [Test]
        public void InvalidSearchOrderDoesNotInitializeContainer()
        {
            var container = new SearchableContainer("street-bin-b");
            ItemType type = Item();
            container.Inventory.Place(type, 0, 0);
            Guid id = container.Inventory.Items[0].ItemInstanceId;

            Assert.That(container.TryInitializeSearchOrder(new[] { id, id }), Is.False);
            Assert.That(container.IsGenerated, Is.False);
            Assert.That(container.DiscoveredCount, Is.Zero);
        }

        [Test]
        public void ClosingAndReopeningIsRepresentedByUnchangedPersistentProgress()
        {
            var container = new SearchableContainer("street-bin-c");
            ItemType type = Item();
            container.Inventory.Place(type, 0, 0);
            container.Inventory.Place(type, 1, 0);
            Guid[] order = { container.Inventory.Items[1].ItemInstanceId, container.Inventory.Items[0].ItemInstanceId };
            container.TryInitializeSearchOrder(order);
            container.TryStartSearch();
            container.TryRevealNext(out _);

            var restored = new SearchableContainer("street-bin-c");
            Assert.That(restored.Inventory.Place(type, 0, 0, itemInstanceId: order[0]).HasValue, Is.True);
            Assert.That(restored.Inventory.Place(type, 1, 0, itemInstanceId: order[1]).HasValue, Is.True);
            Assert.That(restored.TryRestoreSearchState(true, order, 1, true), Is.True);
            Assert.That(restored.DiscoveredCount, Is.EqualTo(1));
            Assert.That(restored.HasSearchStarted, Is.True);
            Assert.That(restored.IsDiscovered(order[0]), Is.True);
            Assert.That(restored.IsDiscovered(order[1]), Is.False);
            Assert.That(restored.TryRevealNext(out Guid next), Is.True);
            Assert.That(next, Is.EqualTo(order[1]));
        }

        [Test]
        public void TakingAnAlreadyDiscoveredItemDoesNotInvalidateTheRemainingSearchOrder()
        {
            var container = new SearchableContainer("street-bin-d");
            ItemType type = Item();
            container.Inventory.Place(type, 0, 0);
            container.Inventory.Place(type, 1, 0);
            Guid[] order = { container.Inventory.Items[0].ItemInstanceId, container.Inventory.Items[1].ItemInstanceId };
            Assert.That(container.TryInitializeSearchOrder(order), Is.True);
            Assert.That(container.TryStartSearch(), Is.True);
            Assert.That(container.TryRevealNext(out Guid found), Is.True);
            var destination = new GridInventory();
            Assert.That(InventoryTransfer.MoveStack(container.Inventory, destination, found).Moved, Is.EqualTo(1));
            Assert.That(container.TryRevealNext(out Guid next), Is.True);
            Assert.That(next, Is.EqualTo(order[1]));
            Assert.That(container.IsSearchComplete, Is.True);
        }

        [Test]
        public void EmptyContainerCanBeGeneratedAndIsImmediatelyComplete()
        {
            var container = new SearchableContainer("empty-bin");
            Assert.That(container.TryInitializeSearchOrder(Array.Empty<Guid>()), Is.True);
            Assert.That(container.IsGenerated, Is.True);
            Assert.That(container.IsSearchComplete, Is.True);
            Assert.That(container.TryRevealNext(out _), Is.False);
        }
        [Test]
        public void SaveMapperRoundTripsGeneratedContentsOrderAndPartialDiscovery()
        {
            Guid definitionId = Guid.NewGuid();
            var type = new ItemType(definitionId, 1, 1, 5);
            var source = new SearchableContainer("street-bin-save");
            var savedState = new ItemInstanceState(ItemQuality.Fine, ItemCondition.Worn,
                ItemAuthenticity.Genuine, ItemOwnershipStatus.Stolen);
            source.Inventory.Place(type, 0, 0, count: 2, state: savedState);
            source.Inventory.Place(type, 1, 0, count: 1, state: ItemInstanceState.Default);
            Guid[] order = { source.Inventory.Items[1].ItemInstanceId, source.Inventory.Items[0].ItemInstanceId };
            Assert.That(source.TryInitializeSearchOrder(order), Is.True);
            Assert.That(source.TryStartSearch(), Is.True);
            Assert.That(source.TryRevealNext(out _), Is.True);
            var data = new SaveData();
            SearchableContainerSaveMapper.Capture(new[] { source }, data);
            var restored = new SearchableContainer("street-bin-save");
            int skipped = SearchableContainerSaveMapper.Apply(data, new[] { restored },
                id => id == definitionId ? type : (ItemType?)null);
            Assert.That(skipped, Is.Zero);
            Assert.That(restored.IsGenerated, Is.True);
            Assert.That(restored.Inventory.Items.Count, Is.EqualTo(2));
            Assert.That(restored.Inventory.Items[0].State, Is.EqualTo(savedState));
            Assert.That(restored.HasSearchStarted, Is.True);
            Assert.That(restored.SearchOrder[0], Is.EqualTo(order[0]));
            Assert.That(restored.DiscoveredCount, Is.EqualTo(1));
            Assert.That(restored.HasSearchStarted, Is.True);
            Assert.That(restored.IsDiscovered(order[0]), Is.True);
            Assert.That(restored.IsDiscovered(order[1]), Is.False);
        }

        [Test]
        public void SaveMapperPreservesRevealOrderAfterAnItemWasTaken()
        {
            Guid definitionId = Guid.NewGuid();
            var type = new ItemType(definitionId, 1, 1, 5);
            var source = new SearchableContainer("street-bin-taken");
            source.Inventory.Place(type, 0, 0);
            source.Inventory.Place(type, 1, 0);
            Guid taken = source.Inventory.Items[0].ItemInstanceId;
            Guid remaining = source.Inventory.Items[1].ItemInstanceId;
            Guid[] order = { taken, remaining };
            source.TryInitializeSearchOrder(order);
            source.TryStartSearch();
            source.TryRevealNext(out _);
            InventoryTransfer.MoveStack(source.Inventory, new GridInventory(), taken);
            var data = new SaveData();
            SearchableContainerSaveMapper.Capture(new[] { source }, data);
            var restored = new SearchableContainer("street-bin-taken");
            int skipped = SearchableContainerSaveMapper.Apply(data, new[] { restored },
                id => id == definitionId ? type : (ItemType?)null);
            Assert.That(skipped, Is.Zero);
            Assert.That(restored.SearchOrder[0], Is.EqualTo(taken));
            Assert.That(restored.DiscoveredCount, Is.EqualTo(1));
            Assert.That(restored.HasSearchStarted, Is.True);
            Assert.That(restored.Inventory.TryGetItemInstance(remaining, out _), Is.True);
            Assert.That(restored.TryRevealNext(out Guid next), Is.True);
            Assert.That(next, Is.EqualTo(remaining));
        }

        [Test]
        public void SearchCannotRevealItemsUntilThePlayerStartsSearching()
        {
            var container = new SearchableContainer("untouched-bin");
            ItemType type = Item();
            container.Inventory.Place(type, 0, 0);
            Guid id = container.Inventory.Items[0].ItemInstanceId;
            Assert.That(container.TryInitializeSearchOrder(new[] { id }), Is.True);
            Assert.That(container.TryRevealNext(out _), Is.False);
            Assert.That(container.DiscoveredCount, Is.Zero);
            Assert.That(container.TryStartSearch(), Is.True);
            Assert.That(container.TryRevealNext(out Guid revealed), Is.True);
            Assert.That(revealed, Is.EqualTo(id));
        }


        [Test]
        public void DiscoveredContainerItemCanTransferIntoAnExactPersonalPocket()
        {
            var container = new SearchableContainer("rhea-wardrobe");
            var pocket = new GridInventory(1, 1);
            ItemType type = Item();
            var state = new ItemInstanceState(ItemQuality.Fine, ItemCondition.Worn,
                ItemAuthenticity.Genuine, ItemOwnershipStatus.Legitimate);
            container.Inventory.Place(type, 0, 0, state: state);
            Guid itemId = container.Inventory.Items[0].ItemInstanceId;

            var resident = new EconomicActorId("resident:rhea");
            var ownership = new AssetOwnershipBook();
            Assert.That(ownership.TryRegister(itemId, resident), Is.True);
            var containers = new EconomicContainerRegistry();
            Assert.That(containers.TryRegister(container.Inventory, resident), Is.True);
            Assert.That(containers.TryRegister(pocket, EconomicActorId.Player), Is.True);
            var ledger = new EconomicTransactionLedger();

            AssetTransferResult result = new AssetTransferService(ownership, containers, ledger).TransferStack(
                container.Inventory, pocket, itemId, resident, EconomicActorId.Player,
                AssetTransferReason.Scavenging, 1, 600);

            Assert.That(result.Success, Is.True);
            Assert.That(container.Inventory.Items, Is.Empty);
            Assert.That(pocket.Items.Count, Is.EqualTo(1));
            Assert.That(pocket.Items[0].ItemInstanceId, Is.EqualTo(itemId));
            Assert.That(pocket.Items[0].State, Is.EqualTo(state));
            Assert.That(ownership.TryGetOwner(itemId, out EconomicActorId owner), Is.True);
            Assert.That(owner, Is.EqualTo(EconomicActorId.Player));
            Assert.That(ledger.Count, Is.EqualTo(1));
        }

        [Test]
        public void PersonalPocketItemCanBePlacedBackIntoAnExactContainerCell()
        {
            var container = new SearchableContainer("rhea-wardrobe");
            var pocket = new GridInventory(1, 1);
            ItemType type = Item();
            var state = new ItemInstanceState(ItemQuality.Fine, ItemCondition.Good,
                ItemAuthenticity.Genuine, ItemOwnershipStatus.Legitimate);
            pocket.Place(type, 0, 0, state: state);
            Guid itemId = pocket.Items[0].ItemInstanceId;

            var resident = new EconomicActorId("resident:rhea");
            var ownership = new AssetOwnershipBook();
            Assert.That(ownership.TryRegister(itemId, EconomicActorId.Player), Is.True);
            var containers = new EconomicContainerRegistry();
            Assert.That(containers.TryRegister(pocket, EconomicActorId.Player), Is.True);
            Assert.That(containers.TryRegister(container.Inventory, resident), Is.True);
            var ledger = new EconomicTransactionLedger();

            AssetTransferResult result = new AssetTransferService(ownership, containers, ledger).TransferStackAt(
                pocket, container.Inventory, itemId, 2, 1, false,
                EconomicActorId.Player, resident, AssetTransferReason.Other, 1, 610);

            Assert.That(result.Success, Is.True);
            Assert.That(pocket.Items, Is.Empty);
            Assert.That(container.Inventory.TryGetItemInstance(itemId, out PlacedItem placed), Is.True);
            Assert.That(placed.X, Is.EqualTo(2));
            Assert.That(placed.Y, Is.EqualTo(1));
            Assert.That(placed.State, Is.EqualTo(state));
            Assert.That(ownership.TryGetOwner(itemId, out EconomicActorId owner), Is.True);
            Assert.That(owner, Is.EqualTo(resident));
            Assert.That(ledger.Count, Is.EqualTo(1));
        }

        [Test]
        public void DiscoveredContainerItemCanBeRepositionedWithinTheSameContainer()
        {
            var container = new SearchableContainer("reorganised-wardrobe");
            var type = new ItemType(Guid.NewGuid(), 2, 1, 1);
            var state = new ItemInstanceState(ItemQuality.Fine, ItemCondition.Good,
                ItemAuthenticity.Genuine, ItemOwnershipStatus.Legitimate);
            Assert.That(container.Inventory.Place(type, 0, 0, state: state).HasValue, Is.True);
            PlacedItem original = container.Inventory.Items[0];
            Assert.That(container.TryInitializeSearchOrder(new[] { original.ItemInstanceId }), Is.True);
            Assert.That(container.TryStartSearch(), Is.True);
            Assert.That(container.TryRevealNext(out _), Is.True);

            Assert.That(container.Inventory.CanMove(original.InstanceId, 2, 1, false), Is.True);
            Assert.That(container.Inventory.TryMove(original.InstanceId, 2, 1, false), Is.True);
            Assert.That(container.Inventory.TryGetItemInstance(original.ItemInstanceId, out PlacedItem moved), Is.True);
            Assert.That(moved.X, Is.EqualTo(2));
            Assert.That(moved.Y, Is.EqualTo(1));
            Assert.That(moved.State, Is.EqualTo(state));
            Assert.That(moved.ItemInstanceId, Is.EqualTo(original.ItemInstanceId));
        }

        [Test]
        public void PersonalPocketsCanBeReorganisedByMovingAndSwappingStacks()
        {
            var pockets = new PlayerPocketBook();
            ItemType type = Item();
            var firstState = new ItemInstanceState(ItemQuality.Fine, ItemCondition.Worn,
                ItemAuthenticity.Genuine, ItemOwnershipStatus.Legitimate);
            var secondState = new ItemInstanceState(ItemQuality.Poor, ItemCondition.Good,
                ItemAuthenticity.Genuine, ItemOwnershipStatus.Stolen);
            Assert.That(pockets.TryAdd(type, Guid.NewGuid(), 2, firstState, out int first), Is.True);
            Assert.That(pockets.TryAdd(type, Guid.NewGuid(), 1, secondState, out int second), Is.True);
            Guid firstId = pockets.GetPocket(first).Items[0].ItemInstanceId;
            Guid secondId = pockets.GetPocket(second).Items[0].ItemInstanceId;

            Assert.That(pockets.TryMoveBetweenPockets(first, 2), Is.True);
            Assert.That(pockets.GetPocket(first).Items, Is.Empty);
            Assert.That(pockets.GetPocket(2).Items[0].ItemInstanceId, Is.EqualTo(firstId));

            Assert.That(pockets.TryMoveBetweenPockets(second, 2), Is.True);
            Assert.That(pockets.GetPocket(second).Items[0].ItemInstanceId, Is.EqualTo(firstId));
            Assert.That(pockets.GetPocket(2).Items[0].ItemInstanceId, Is.EqualTo(secondId));
            Assert.That(pockets.GetPocket(second).Items[0].State, Is.EqualTo(firstState));
            Assert.That(pockets.GetPocket(2).Items[0].State, Is.EqualTo(secondState));
        }

        [Test]
        public void BackpackItemCanBePlacedAtAnExactCellInsideSearchableContainer()
        {
            var backpack = new GridInventory(4, 3);
            var container = new SearchableContainer("backpack-return-wardrobe");
            ItemType type = new ItemType(Guid.NewGuid(), 2, 1, 5);
            var state = new ItemInstanceState(ItemQuality.Fine, ItemCondition.Good,
                ItemAuthenticity.Genuine, ItemOwnershipStatus.Legitimate);
            backpack.Place(type, 0, 0, count: 2, state: state);
            Guid itemId = backpack.Items[0].ItemInstanceId;

            var ownership = new AssetOwnershipBook();
            Assert.That(ownership.TryRegister(itemId, EconomicActorId.Player), Is.True);
            var containers = new EconomicContainerRegistry();
            Assert.That(containers.TryRegister(backpack, EconomicActorId.Player), Is.True);
            var resident = new EconomicActorId("resident:rhea");
            Assert.That(containers.TryRegister(container.Inventory, resident), Is.True);
            var ledger = new EconomicTransactionLedger();

            AssetTransferResult result = new AssetTransferService(ownership, containers, ledger).TransferStackAt(
                backpack, container.Inventory, itemId, 2, 1, false,
                EconomicActorId.Player, resident, AssetTransferReason.Other, 1, 620);

            Assert.That(result.Success, Is.True);
            Assert.That(backpack.Items, Is.Empty);
            Assert.That(container.Inventory.TryGetItemInstance(itemId, out PlacedItem placed), Is.True);
            Assert.That(placed.X, Is.EqualTo(2));
            Assert.That(placed.Y, Is.EqualTo(1));
            Assert.That(placed.Count, Is.EqualTo(2));
            Assert.That(placed.State, Is.EqualTo(state));
            Assert.That(ownership.TryGetOwner(itemId, out EconomicActorId owner), Is.True);
            Assert.That(owner, Is.EqualTo(resident));
            Assert.That(ledger.Count, Is.EqualTo(1));
        }
        [Test]
        public void SaveMapperRejectsContainerItemIdentityAlreadyOwnedInAnotherInventory()
        {
            Guid typeId = Guid.NewGuid();
            Guid itemId = Guid.NewGuid();
            var type = new ItemType(typeId, 1, 1, 5);
            var source = new SearchableContainer("saved-wardrobe-identity");
            Assert.That(source.Inventory.Place(type, 0, 0, false, 1, itemId).HasValue, Is.True);
            Assert.That(source.TryInitializeSearchOrder(new[] { itemId }), Is.True);
            var data = new SaveData();
            SearchableContainerSaveMapper.Capture(new[] { source }, data);

            var ownership = new AssetOwnershipBook();
            Assert.That(ownership.TryRegister(itemId, EconomicActorId.Player), Is.True);
            var restored = new SearchableContainer("saved-wardrobe-identity");

            int skipped = SearchableContainerSaveMapper.Apply(data, new[] { restored },
                id => id == typeId ? type : (ItemType?)null, ownership);

            Assert.That(skipped, Is.GreaterThanOrEqualTo(1));
            Assert.That(restored.Inventory.TryGetItemInstance(itemId, out _), Is.False);
        }

        [Test]
        public void SaveMapperRejectsDuplicateStableIdsAcrossSearchableContainers()
        {
            Guid typeId = Guid.NewGuid();
            Guid itemId = Guid.NewGuid();
            var type = new ItemType(typeId, 1, 1, 5);
            var firstSource = new SearchableContainer("duplicate-bin-first");
            var secondSource = new SearchableContainer("duplicate-bin-second");
            Assert.That(firstSource.Inventory.Place(type, 0, 0, false, 1, itemId).HasValue, Is.True);
            Assert.That(secondSource.Inventory.Place(type, 0, 0, false, 1, itemId).HasValue, Is.True);
            Assert.That(firstSource.TryInitializeSearchOrder(new[] { itemId }), Is.True);
            Assert.That(secondSource.TryInitializeSearchOrder(new[] { itemId }), Is.True);
            var data = new SaveData();
            SearchableContainerSaveMapper.Capture(new[] { firstSource, secondSource }, data);

            var firstTarget = new SearchableContainer("duplicate-bin-first");
            var secondTarget = new SearchableContainer("duplicate-bin-second");
            int skipped = SearchableContainerSaveMapper.Apply(data,
                new[] { firstTarget, secondTarget },
                id => id == typeId ? type : (ItemType?)null);

            Assert.That(skipped, Is.GreaterThanOrEqualTo(1));
            Assert.That(firstTarget.Inventory.TryGetItemInstance(itemId, out _), Is.True);
            Assert.That(secondTarget.Inventory.TryGetItemInstance(itemId, out _), Is.False);
        }

    }
}
