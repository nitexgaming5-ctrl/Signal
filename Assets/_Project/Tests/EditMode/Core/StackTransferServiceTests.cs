using System;
using NUnit.Framework;

namespace Signal.Core.Tests
{
    public sealed class StackTransferServiceTests
    {
        private static ItemType Stackable(int max = 20) => new ItemType(Guid.NewGuid(), 1, 1, max);
        private static ItemInstanceState FineWorn() => new ItemInstanceState(
            ItemQuality.Fine, ItemCondition.Worn, ItemAuthenticity.Genuine, ItemOwnershipStatus.Legitimate);

        [Test]
        public void SameOwnerSplitPreservesOwnerAndDoesNotCreateEconomicTransaction()
        {
            var source = new GridInventory(2, 1);
            var destination = new GridInventory(2, 1);
            var ownership = new AssetOwnershipBook();
            var containers = new EconomicContainerRegistry();
            var transactions = new EconomicTransactionLedger();
            var lineage = new StackLineageBook();
            containers.TryRegister(source, EconomicActorId.Player);
            containers.TryRegister(destination, EconomicActorId.Player);
            ItemType type = Stackable();
            Guid sourceId = Guid.NewGuid();
            source.Place(type, 0, 0, count: 12, itemInstanceId: sourceId, state: FineWorn());
            ownership.TryRegister(sourceId, EconomicActorId.Player);
            var service = new StackTransferService(ownership, containers, transactions, lineage);
            bool observerSawOwnershipCommitted = false;
            destination.Changed += changed => observerSawOwnershipCommitted =
                destination.Items.Count == 1 &&
                ownership.TryGetOwner(destination.Items[0].ItemInstanceId, out EconomicActorId eventOwner) &&
                eventOwner == EconomicActorId.Player;

            StackTransferResult result = service.Split(source, destination, sourceId, 5, 0, 0,
                EconomicActorId.Player, EconomicActorId.Player, AssetTransferReason.Other, 1, 30);

            Assert.That(result.Success, Is.True);
            Assert.That(result.Quantity, Is.EqualTo(5));
            Assert.That(result.ResultingStackId, Is.Not.EqualTo(sourceId));
            Assert.That(source.TryGetItemInstance(sourceId, out PlacedItem remainder), Is.True);
            Assert.That(remainder.Count, Is.EqualTo(7));
            Assert.That(destination.TryGetItemInstance(result.ResultingStackId, out PlacedItem split), Is.True);
            Assert.That(split.Count, Is.EqualTo(5));
            Assert.That(split.State, Is.EqualTo(FineWorn()));
            Assert.That(ownership.TryGetOwner(result.ResultingStackId, out EconomicActorId splitOwner), Is.True);
            Assert.That(splitOwner, Is.EqualTo(EconomicActorId.Player));
            Assert.That(transactions.Count, Is.Zero);
            Assert.That(observerSawOwnershipCommitted, Is.True);
        }

        [Test]
        public void CrossOwnerSplitRegistersNewIdentityAndRecordsExactQuantityTransfer()
        {
            var source = new GridInventory(2, 1);
            var destination = new GridInventory(2, 1);
            var resident = new EconomicActorId("resident:test");
            var ownership = new AssetOwnershipBook();
            var containers = new EconomicContainerRegistry();
            var transactions = new EconomicTransactionLedger();
            containers.TryRegister(source, EconomicActorId.Player);
            containers.TryRegister(destination, resident);
            ItemType type = Stackable();
            Guid sourceId = Guid.NewGuid();
            source.Place(type, 0, 0, count: 10, itemInstanceId: sourceId);
            ownership.TryRegister(sourceId, EconomicActorId.Player);
            var service = new StackTransferService(ownership, containers, transactions, new StackLineageBook());

            StackTransferResult result = service.Split(source, destination, sourceId, 4, 0, 0,
                EconomicActorId.Player, resident, AssetTransferReason.Gift, 3, 240);

            Assert.That(result.Success, Is.True);
            Assert.That(source.TryGetItemInstance(sourceId, out PlacedItem remainder), Is.True);
            Assert.That(remainder.Count, Is.EqualTo(6));
            Assert.That(ownership.TryGetOwner(sourceId, out EconomicActorId sourceOwner), Is.True);
            Assert.That(sourceOwner, Is.EqualTo(EconomicActorId.Player));
            Assert.That(ownership.TryGetOwner(result.ResultingStackId, out EconomicActorId splitOwner), Is.True);
            Assert.That(splitOwner, Is.EqualTo(resident));
            Assert.That(transactions.Count, Is.EqualTo(1));
            Assert.That(transactions.Transactions[0].ItemInstanceId, Is.EqualTo(result.ResultingStackId));
            Assert.That(transactions.Transactions[0].Quantity, Is.EqualTo(4));
            Assert.That(transactions.Transactions[0].FromActor, Is.EqualTo(EconomicActorId.Player));
            Assert.That(transactions.Transactions[0].ToActor, Is.EqualTo(resident));
        }

        [Test]
        public void DuplicateEconomicTransactionRollsBackCrossOwnerSplitCompletely()
        {
            var source = new GridInventory(3, 1);
            var destination = new GridInventory(3, 1);
            var resident = new EconomicActorId("resident:test");
            var ownership = new AssetOwnershipBook();
            var containers = new EconomicContainerRegistry();
            var transactions = new EconomicTransactionLedger();
            containers.TryRegister(source, EconomicActorId.Player);
            containers.TryRegister(destination, resident);
            ItemType type = Stackable();
            Guid sourceId = Guid.NewGuid();
            source.Place(type, 0, 0, count: 9, itemInstanceId: sourceId);
            ownership.TryRegister(sourceId, EconomicActorId.Player);
            var service = new StackTransferService(ownership, containers, transactions, new StackLineageBook());
            Guid transactionId = Guid.NewGuid();
            StackTransferResult first = service.Split(source, destination, sourceId, 2, 0, 0,
                EconomicActorId.Player, resident, AssetTransferReason.Gift, 1, 15, transactionId);
            Assert.That(first.Success, Is.True);
            Assert.That(source.TryGetItemInstance(sourceId, out PlacedItem afterFirst), Is.True);
            Assert.That(afterFirst.Count, Is.EqualTo(7));
            // The second split is routed to another empty cell but reuses the ledger transaction ID.
            StackTransferResult duplicate = service.Split(source, destination, sourceId, 2, 1, 0,
                EconomicActorId.Player, resident, AssetTransferReason.Gift, 1, 16, transactionId);
            Assert.That(duplicate.Status, Is.EqualTo(StackTransferStatus.TransferFailed));
            Assert.That(source.TryGetItemInstance(sourceId, out PlacedItem sourceAfterRollback), Is.True);
            Assert.That(sourceAfterRollback.Count, Is.EqualTo(7));
            Assert.That(destination.TryGetItemInstance(first.ResultingStackId, out PlacedItem firstSplit), Is.True);
            Assert.That(firstSplit.Count, Is.EqualTo(2));
            Assert.That(destination.Items.Count, Is.EqualTo(1));
            Assert.That(ownership.TryGetOwner(first.ResultingStackId, out EconomicActorId owner), Is.True);
            Assert.That(owner, Is.EqualTo(resident));
            Assert.That(transactions.Count, Is.EqualTo(1));
        }

        [Test]
        public void CrossOwnerMergeIsRejectedWithoutChangingEitherStackOrOwnership()
        {
            var source = new GridInventory(2, 1);
            var destination = new GridInventory(2, 1);
            var resident = new EconomicActorId("resident:test");
            var ownership = new AssetOwnershipBook();
            var containers = new EconomicContainerRegistry();
            var transactions = new EconomicTransactionLedger();
            var lineage = new StackLineageBook();
            containers.TryRegister(source, EconomicActorId.Player);
            containers.TryRegister(destination, EconomicActorId.Player);
            ItemType type = Stackable();
            Guid sourceId = Guid.NewGuid();
            Guid destinationId = Guid.NewGuid();
            source.Place(type, 0, 0, count: 4, itemInstanceId: sourceId);
            destination.Place(type, 0, 0, count: 3, itemInstanceId: destinationId);
            ownership.TryRegister(sourceId, EconomicActorId.Player);
            ownership.TryRegister(destinationId, resident);
            var service = new StackTransferService(ownership, containers, transactions, lineage);

            StackTransferResult result = service.Merge(source, destination, sourceId, destinationId, 2, 1, 20);

            Assert.That(result.Status, Is.EqualTo(StackTransferStatus.CrossOwnerMergeRejected));
            Assert.That(source.TryGetItemInstance(sourceId, out PlacedItem sourceItem), Is.True);
            Assert.That(sourceItem.Count, Is.EqualTo(4));
            Assert.That(destination.TryGetItemInstance(destinationId, out PlacedItem destinationItem), Is.True);
            Assert.That(destinationItem.Count, Is.EqualTo(3));
            Assert.That(lineage.Merges, Is.Empty);
            Assert.That(transactions.Count, Is.Zero);
        }

        [Test]
        public void PartialSameOwnerMergeRetainsSourceIdentityAndOwnership()
        {
            var source = new GridInventory(2, 1);
            var destination = new GridInventory(2, 1);
            var ownership = new AssetOwnershipBook();
            var containers = new EconomicContainerRegistry();
            var transactions = new EconomicTransactionLedger();
            var lineage = new StackLineageBook();
            containers.TryRegister(source, EconomicActorId.Player);
            containers.TryRegister(destination, EconomicActorId.Player);
            ItemType type = Stackable(10);
            Guid sourceId = Guid.NewGuid();
            Guid destinationId = Guid.NewGuid();
            source.Place(type, 0, 0, count: 8, itemInstanceId: sourceId);
            destination.Place(type, 0, 0, count: 3, itemInstanceId: destinationId);
            ownership.TryRegister(sourceId, EconomicActorId.Player);
            ownership.TryRegister(destinationId, EconomicActorId.Player);
            var service = new StackTransferService(ownership, containers, transactions, lineage);

            StackTransferResult result = service.Merge(source, destination, sourceId, destinationId, 4, 1, 50);

            Assert.That(result.Success, Is.True);
            Assert.That(source.TryGetItemInstance(sourceId, out PlacedItem remainder), Is.True);
            Assert.That(remainder.Count, Is.EqualTo(4));
            Assert.That(ownership.TryGetOwner(sourceId, out EconomicActorId sourceOwner), Is.True);
            Assert.That(sourceOwner, Is.EqualTo(EconomicActorId.Player));
            Assert.That(destination.TryGetItemInstance(destinationId, out PlacedItem survivor), Is.True);
            Assert.That(survivor.Count, Is.EqualTo(7));
            Assert.That(lineage.Merges[0].SourceRetired, Is.False);
        }

        [Test]
        public void UnregisteredBackpackGridCannotBeUsedAsMergeSource()
        {
            var backpackGrid = new GridInventory(2, 1);
            var pocket = new GridInventory(1, 1);
            var ownership = new AssetOwnershipBook();
            var containers = new EconomicContainerRegistry();
            var transactions = new EconomicTransactionLedger();
            var lineage = new StackLineageBook();
            containers.TryRegister(backpackGrid, EconomicActorId.Player);
            containers.TryRegister(pocket, EconomicActorId.Player);
            ItemType type = Stackable(10);
            Guid backpackId = Guid.NewGuid();
            Guid pocketId = Guid.NewGuid();
            backpackGrid.Place(type, 0, 0, count: 4, itemInstanceId: backpackId);
            pocket.Place(type, 0, 0, count: 2, itemInstanceId: pocketId);
            ownership.TryRegister(backpackId, EconomicActorId.Player);
            ownership.TryRegister(pocketId, EconomicActorId.Player);
            containers.TryUnregister(backpackGrid);
            var service = new StackTransferService(ownership, containers, transactions, lineage);

            StackTransferResult result = service.Merge(backpackGrid, pocket, backpackId, pocketId, 2, 1, 5);

            Assert.That(result.Status, Is.EqualTo(StackTransferStatus.ContainerNotRegistered));
            Assert.That(backpackGrid.TryGetItemInstance(backpackId, out PlacedItem sourceItem), Is.True);
            Assert.That(sourceItem.Count, Is.EqualTo(4));
            Assert.That(pocket.TryGetItemInstance(pocketId, out PlacedItem destinationItem), Is.True);
            Assert.That(destinationItem.Count, Is.EqualTo(2));
            Assert.That(lineage.Merges, Is.Empty);
        }

        [Test]
        public void SameOwnerFullMergeRetiresSourceOwnershipAndRecordsCustodyLineage()
        {
            var source = new GridInventory(2, 1);
            var destination = new GridInventory(2, 1);
            var ownership = new AssetOwnershipBook();
            var containers = new EconomicContainerRegistry();
            var transactions = new EconomicTransactionLedger();
            var lineage = new StackLineageBook();
            containers.TryRegister(source, EconomicActorId.Player);
            containers.TryRegister(destination, EconomicActorId.Player);
            ItemType type = Stackable(10);
            Guid sourceId = Guid.NewGuid();
            Guid destinationId = Guid.NewGuid();
            source.Place(type, 0, 0, count: 4, itemInstanceId: sourceId, state: FineWorn());
            destination.Place(type, 0, 0, count: 3, itemInstanceId: destinationId, state: FineWorn());
            ownership.TryRegister(sourceId, EconomicActorId.Player);
            ownership.TryRegister(destinationId, EconomicActorId.Player);
            var service = new StackTransferService(ownership, containers, transactions, lineage);

            StackTransferResult result = service.Merge(source, destination, sourceId, destinationId, 4, 2, 75);

            Assert.That(result.Success, Is.True);
            Assert.That(source.TryGetItemInstance(sourceId, out _), Is.False);
            Assert.That(ownership.TryGetOwner(sourceId, out _), Is.False);
            Assert.That(destination.TryGetItemInstance(destinationId, out PlacedItem survivor), Is.True);
            Assert.That(survivor.Count, Is.EqualTo(7));
            Assert.That(ownership.TryGetOwner(destinationId, out EconomicActorId owner), Is.True);
            Assert.That(owner, Is.EqualTo(EconomicActorId.Player));
            Assert.That(transactions.Count, Is.Zero);
            Assert.That(lineage.Merges.Count, Is.EqualTo(1));
            Assert.That(lineage.Merges[0].OwnerActorId, Is.EqualTo(EconomicActorId.Player.Value));
            Assert.That(lineage.Merges[0].SourceCustodianId, Is.EqualTo(EconomicActorId.Player.Value));
            Assert.That(lineage.Merges[0].Day, Is.EqualTo(2));
            Assert.That(lineage.Merges[0].MinuteOfDay, Is.EqualTo(75));
        }

        [Test]
        public void UnregisteredBackpackGridReferenceCannotBeUsedForStackSplit()
        {
            var backpackGrid = new GridInventory(2, 1);
            var pocket = new GridInventory(1, 1);
            var ownership = new AssetOwnershipBook();
            var containers = new EconomicContainerRegistry();
            var transactions = new EconomicTransactionLedger();
            containers.TryRegister(backpackGrid, EconomicActorId.Player);
            containers.TryRegister(pocket, EconomicActorId.Player);
            ItemType type = Stackable();
            Guid sourceId = Guid.NewGuid();
            backpackGrid.Place(type, 0, 0, count: 8, itemInstanceId: sourceId);
            ownership.TryRegister(sourceId, EconomicActorId.Player);
            Assert.That(containers.TryUnregister(backpackGrid), Is.True);
            var service = new StackTransferService(ownership, containers, transactions, new StackLineageBook());

            StackTransferResult result = service.Split(backpackGrid, pocket, sourceId, 2, 0, 0,
                EconomicActorId.Player, EconomicActorId.Player, AssetTransferReason.Other, 1, 10);

            Assert.That(result.Status, Is.EqualTo(StackTransferStatus.ContainerNotRegistered));
            Assert.That(backpackGrid.TryGetItemInstance(sourceId, out PlacedItem untouched), Is.True);
            Assert.That(untouched.Count, Is.EqualTo(8));
            Assert.That(pocket.Items, Is.Empty);
        }

        [Test]
        public void StackLineageRoundTripsThroughSaveMapperAndRejectsDuplicateOperations()
        {
            var source = new GridInventory(2, 1);
            var destination = new GridInventory(2, 1);
            var ownership = new AssetOwnershipBook();
            var containers = new EconomicContainerRegistry();
            var transactions = new EconomicTransactionLedger();
            var lineage = new StackLineageBook();
            containers.TryRegister(source, EconomicActorId.Player);
            containers.TryRegister(destination, EconomicActorId.Player);
            ItemType type = Stackable(10);
            Guid sourceId = Guid.NewGuid();
            Guid destinationId = Guid.NewGuid();
            source.Place(type, 0, 0, count: 2, itemInstanceId: sourceId);
            destination.Place(type, 0, 0, count: 2, itemInstanceId: destinationId);
            ownership.TryRegister(sourceId, EconomicActorId.Player);
            ownership.TryRegister(destinationId, EconomicActorId.Player);
            var service = new StackTransferService(ownership, containers, transactions, lineage);
            Assert.That(service.Merge(source, destination, sourceId, destinationId, 2, 1, 5).Success, Is.True);

            // Simulate a malformed/stale save that tries to resurrect the retired source identity.
            source.Place(type, 0, 0, count: 1, itemInstanceId: sourceId);
            ownership.TryRegister(sourceId, EconomicActorId.Player);
            var save = new SaveData();
            StackLineageSaveMapper.Capture(lineage, save);
            var restored = new StackLineageBook();
            Assert.That(StackLineageSaveMapper.Apply(save, restored, ownership, source, destination), Is.Zero);
            Assert.That(source.TryGetItemInstance(sourceId, out _), Is.False);
            Assert.That(ownership.TryGetOwner(sourceId, out _), Is.False);
            Assert.That(restored.Merges.Count, Is.EqualTo(1));
            Assert.That(restored.Merges[0].SourceStackId, Is.EqualTo(sourceId));
            Assert.That(restored.Merges[0].DestinationStackId, Is.EqualTo(destinationId));
            Assert.That(restored.Merges[0].Quantity, Is.EqualTo(2));
            Assert.That(restored.Merges[0].OwnerActorId, Is.EqualTo(EconomicActorId.Player.Value));
            Assert.That(restored.Merges[0].SourceRetired, Is.True);
            Assert.That(StackLineageSaveMapper.Apply(save, restored, ownership, source, destination), Is.Zero);
            Assert.That(restored.Merges.Count, Is.EqualTo(1));
        }

        [Test]
        public void EquippableAndPortableBackpackTypesAreNeverStackable()
        {
            ItemType equippable = new ItemType(Guid.NewGuid(), 1, 1, 10, true, false);
            ItemType backpack = new ItemType(Guid.NewGuid(), 1, 1, 10, true, true);
            Assert.That(equippable.IsStackable, Is.False);
            Assert.That(backpack.IsStackable, Is.False);
            Assert.That(equippable.MaxStack, Is.EqualTo(1));
            Assert.That(backpack.MaxStack, Is.EqualTo(1));
            var grid = new GridInventory(2, 1);
            Assert.That(grid.Place(equippable, 0, 0, count: 2).HasValue, Is.False);
            Assert.That(grid.Place(backpack, 0, 0, count: 2).HasValue, Is.False);
        }
    }
}
