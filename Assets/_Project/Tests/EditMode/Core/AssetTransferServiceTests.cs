using System;
using NUnit.Framework;
namespace Signal.Core.Tests
{
    public sealed class AssetTransferServiceTests
    {
        private static readonly EconomicActorId Player = EconomicActorId.Player;
        private static readonly EconomicActorId Milo = new EconomicActorId("resident:milo");
        private static ItemType Item() => new ItemType(Guid.NewGuid(), 1, 1, 10);
        private static AssetTransferService Service(AssetOwnershipBook ownership, EconomicContainerRegistry containers, EconomicTransactionLedger ledger)
            => new AssetTransferService(ownership, containers, ledger);
        private static EconomicContainerRegistry Containers(GridInventory source, GridInventory destination)
        {
            var containers = new EconomicContainerRegistry();
            Assert.That(containers.TryRegister(source, Milo), Is.True);
            Assert.That(containers.TryRegister(destination, Milo), Is.True);
            return containers;
        }

        [Test]
        public void TransferMovesWholeStackPreservesIdentityStateChangesOwnerAndRecordsTransaction()
        {
            ItemType type = Item(); var source = new GridInventory(2, 1); var destination = new GridInventory(2, 1);
            var ownership = new AssetOwnershipBook(); var ledger = new EconomicTransactionLedger();
            var containers = Containers(source, destination);
            var state = new ItemInstanceState(ItemQuality.Fine, ItemCondition.Worn, ItemAuthenticity.Genuine, ItemOwnershipStatus.Stolen);
            source.Place(type, 0, 0, count: 4, state: state);
            Guid itemId = source.Items[0].ItemInstanceId;
            Assert.That(ownership.TryRegister(itemId, Player), Is.True);
            AssetTransferResult result = Service(ownership, containers, ledger).TransferStack(source, destination, itemId,
                Player, Milo, AssetTransferReason.Gift, 3, 725);
            Assert.That(result.Success, Is.True);
            Assert.That(source.Items, Is.Empty);
            Assert.That(destination.Items.Count, Is.EqualTo(1));
            Assert.That(destination.Items[0].ItemInstanceId, Is.EqualTo(itemId));
            Assert.That(destination.Items[0].Count, Is.EqualTo(4));
            Assert.That(destination.Items[0].State, Is.EqualTo(state));
            Assert.That(ownership.TryGetOwner(itemId, out EconomicActorId owner), Is.True);
            Assert.That(owner, Is.EqualTo(Milo));
            Assert.That(ledger.Count, Is.EqualTo(1));
            Assert.That(result.Transaction.ItemInstanceId, Is.EqualTo(itemId));
            Assert.That(result.Transaction.ItemDefinitionId, Is.EqualTo(type.Id));
            Assert.That(result.Transaction.Quantity, Is.EqualTo(4));
            Assert.That(result.Transaction.FromActor, Is.EqualTo(Player));
            Assert.That(result.Transaction.ToActor, Is.EqualTo(Milo));
            Assert.That(result.Transaction.Reason, Is.EqualTo(AssetTransferReason.Gift));
            Assert.That(result.Transaction.Day, Is.EqualTo(3));
            Assert.That(result.Transaction.MinuteOfDay, Is.EqualTo(725));
        }

        [Test]
        public void TheftTransferRecordsTheftWithoutChangingTheAtomicTransferRules()
        {
            var source = new GridInventory();
            var destination = new GridInventory();
            ItemType type = Item();
            var originalState = new ItemInstanceState(ItemQuality.Fine, ItemCondition.Worn,
                ItemAuthenticity.Genuine, ItemOwnershipStatus.Legitimate);
            source.Place(type, 0, 0, count: 2, state: originalState);
            Guid id = source.Items[0].ItemInstanceId;
            var ownership = new AssetOwnershipBook();
            ownership.TryRegister(id, Milo);
            var ledger = new EconomicTransactionLedger();
            var containers = new EconomicContainerRegistry();
            Assert.That(containers.TryRegister(source, Milo), Is.True);
            Assert.That(containers.TryRegister(destination, Player), Is.True);

            AssetTransferResult result = Service(ownership, containers, ledger).TransferStack(
                source, destination, id, Milo, Player, AssetTransferReason.Theft, 1, 615);

            Assert.That(result.Success, Is.True);
            Assert.That(result.Transaction.Reason, Is.EqualTo(AssetTransferReason.Theft));
            Assert.That(result.Transaction.Quantity, Is.EqualTo(2));
            Assert.That(destination.Items[0].ItemInstanceId, Is.EqualTo(id));
            Assert.That(destination.Items[0].State.Quality, Is.EqualTo(ItemQuality.Fine));
            Assert.That(destination.Items[0].State.Condition, Is.EqualTo(ItemCondition.Worn));
            Assert.That(destination.Items[0].State.Ownership, Is.EqualTo(ItemOwnershipStatus.Stolen));
            Assert.That(ownership.TryGetOwner(id, out EconomicActorId owner), Is.True);
            Assert.That(owner, Is.EqualTo(Player));
            Assert.That(ledger.Count, Is.EqualTo(1));
        }

        [Test]
        public void FailedTheftTransferDoesNotMarkTheItemStolenOrRecordATransaction()
        {
            var source = new GridInventory(1, 1);
            var destination = new GridInventory(1, 1);
            ItemType type = Item();
            ItemType blocker = Item();
            var legitimateState = new ItemInstanceState(ItemQuality.Fine, ItemCondition.Worn,
                ItemAuthenticity.Genuine, ItemOwnershipStatus.Legitimate);
            source.Place(type, 0, 0, state: legitimateState);
            destination.Place(blocker, 0, 0);
            Guid id = source.Items[0].ItemInstanceId;
            var ownership = new AssetOwnershipBook();
            ownership.TryRegister(id, Milo);
            var containers = new EconomicContainerRegistry();
            containers.TryRegister(source, Milo);
            containers.TryRegister(destination, Player);
            var ledger = new EconomicTransactionLedger();

            AssetTransferResult result = Service(ownership, containers, ledger).TransferStack(
                source, destination, id, Milo, Player, AssetTransferReason.Theft, 1, 615);

            Assert.That(result.Status, Is.EqualTo(AssetTransferStatus.DestinationFull));
            Assert.That(source.Items[0].ItemInstanceId, Is.EqualTo(id));
            Assert.That(source.Items[0].State.Ownership, Is.EqualTo(ItemOwnershipStatus.Legitimate));
            Assert.That(ownership.TryGetOwner(id, out EconomicActorId owner), Is.True);
            Assert.That(owner, Is.EqualTo(Milo));
            Assert.That(ledger.Count, Is.Zero);
        }

        [Test]
        public void OwnershipMismatchChangesNothing()
        {
            ItemType type = Item(); var source = new GridInventory(); var destination = new GridInventory();
            source.Place(type, 0, 0, count: 2); Guid itemId = source.Items[0].ItemInstanceId;
            var ownership = new AssetOwnershipBook(); ownership.TryRegister(itemId, Milo);
            var ledger = new EconomicTransactionLedger(); var containers = Containers(source, destination);
            AssetTransferResult result = Service(ownership, containers, ledger).TransferStack(source, destination, itemId,
                Player, Milo, AssetTransferReason.Purchase, 1, 500);
            Assert.That(result.Status, Is.EqualTo(AssetTransferStatus.OwnershipMismatch));
            Assert.That(source.CountOf(type), Is.EqualTo(2)); Assert.That(destination.CountOf(type), Is.Zero);
            Assert.That(ownership.TryGetOwner(itemId, out EconomicActorId owner), Is.True);
            Assert.That(owner, Is.EqualTo(Milo)); Assert.That(ledger.Count, Is.Zero);
        }

        [Test]
        public void UnregisteredAssetCannotBeTransferred()
        {
            ItemType type = Item(); var source = new GridInventory(); var destination = new GridInventory();
            source.Place(type, 0, 0); var ownership = new AssetOwnershipBook(); var ledger = new EconomicTransactionLedger(); var containers = Containers(source, destination);
            AssetTransferResult result = Service(ownership, containers, ledger).TransferStack(source, destination, source.Items[0].ItemInstanceId,
                Player, Milo, AssetTransferReason.Gift, 1, 0);
            Assert.That(result.Status, Is.EqualTo(AssetTransferStatus.OwnershipNotRegistered));
            Assert.That(source.Items.Count, Is.EqualTo(1)); Assert.That(destination.Items, Is.Empty); Assert.That(ledger.Count, Is.Zero);
        }

        [Test]
        public void FullDestinationLeavesAssetAndOwnerUnchanged()
        {
            ItemType type = Item(); ItemType blocker = Item(); var source = new GridInventory(1, 1); var destination = new GridInventory(1, 1);
            source.Place(type, 0, 0); destination.Place(blocker, 0, 0); Guid itemId = source.Items[0].ItemInstanceId;
            var ownership = new AssetOwnershipBook(); ownership.TryRegister(itemId, Player); var ledger = new EconomicTransactionLedger();
            var containers = Containers(source, destination);
            AssetTransferResult result = Service(ownership, containers, ledger).TransferStack(source, destination, itemId,
                Player, Milo, AssetTransferReason.Gift, 1, 0);
            Assert.That(result.Status, Is.EqualTo(AssetTransferStatus.DestinationFull));
            Assert.That(source.Items.Count, Is.EqualTo(1)); Assert.That(source.Items[0].ItemInstanceId, Is.EqualTo(itemId));
            Assert.That(destination.Items.Count, Is.EqualTo(1));
            Assert.That(ownership.TryGetOwner(itemId, out EconomicActorId owner), Is.True);
            Assert.That(owner, Is.EqualTo(Player)); Assert.That(ledger.Count, Is.Zero);
        }

        [Test]
        public void MissingAssetDoesNotChangeOwnershipOrLedger()
        {
            var source = new GridInventory(); var destination = new GridInventory();
            var ownership = new AssetOwnershipBook(); var ledger = new EconomicTransactionLedger();
            var containers = Containers(source, destination);
            AssetTransferResult result = Service(ownership, containers, ledger).TransferStack(source, destination,
                Guid.NewGuid(), Player, Milo, AssetTransferReason.Gift, 1, 0);
            Assert.That(result.Status, Is.EqualTo(AssetTransferStatus.AssetNotFound));
            Assert.That(ownership.Count, Is.Zero); Assert.That(ledger.Count, Is.Zero);
        }

        [Test]
        public void InvalidActorPairAndTimestampAreRejected()
        {
            var source = new GridInventory(); var destination = new GridInventory();
            var service = Service(new AssetOwnershipBook(), Containers(source, destination), new EconomicTransactionLedger());
            Assert.That(service.TransferStack(new GridInventory(), new GridInventory(), Guid.NewGuid(), Player, Player,
                AssetTransferReason.Gift, 1, 0).Status, Is.EqualTo(AssetTransferStatus.InvalidRequest));
            Assert.That(service.TransferStack(new GridInventory(), new GridInventory(), Guid.NewGuid(), Player, Milo,
                AssetTransferReason.Gift, 0, 0).Status, Is.EqualTo(AssetTransferStatus.InvalidRequest));
            Assert.That(service.TransferStack(new GridInventory(), new GridInventory(), Guid.NewGuid(), Player, Milo,
                AssetTransferReason.Gift, 1, 1440).Status, Is.EqualTo(AssetTransferStatus.InvalidRequest));
        }

        [Test]
        public void PositionedScavengingTransferPreservesIdentityStateAndChangesOwnership()
        {
            var source = new GridInventory(3, 2);
            var destination = new GridInventory(3, 2);
            ItemType type = Item();
            var state = new ItemInstanceState(ItemQuality.Fine, ItemCondition.Worn,
                ItemAuthenticity.Genuine, ItemOwnershipStatus.Stolen);
            source.Place(type, 0, 0, count: 3, state: state);
            Guid id = source.Items[0].ItemInstanceId;
            var world = new EconomicActorId("world:street-bin-a");
            var ownership = new AssetOwnershipBook();
            ownership.TryRegister(id, world);
            var containers = new EconomicContainerRegistry();
            containers.TryRegister(source, world);
            containers.TryRegister(destination, Player);
            var ledger = new EconomicTransactionLedger();

            AssetTransferResult result = Service(ownership, containers, ledger).TransferStackAt(
                source, destination, id, 2, 1, false, world, Player, AssetTransferReason.Scavenging, 2, 615);

            Assert.That(result.Success, Is.True);
            Assert.That(source.Items, Is.Empty);
            Assert.That(destination.Items.Count, Is.EqualTo(1));
            Assert.That(destination.Items[0].ItemInstanceId, Is.EqualTo(id));
            Assert.That(destination.Items[0].X, Is.EqualTo(2));
            Assert.That(destination.Items[0].Y, Is.EqualTo(1));
            Assert.That(destination.Items[0].State, Is.EqualTo(state));
            Assert.That(ownership.TryGetOwner(id, out EconomicActorId owner), Is.True);
            Assert.That(owner, Is.EqualTo(Player));
            Assert.That(ledger.Transactions[0].Reason, Is.EqualTo(AssetTransferReason.Scavenging));
        }

        [Test]
        public void PositionedScavengingTransferRejectsFullDestinationWithoutChangingOwnerOrSource()
        {
            var source = new GridInventory(2, 1);
            var destination = new GridInventory(1, 1);
            ItemType type = Item();
            ItemType blocker = Item();
            source.Place(type, 0, 0);
            destination.Place(blocker, 0, 0);
            Guid id = source.Items[0].ItemInstanceId;
            var world = new EconomicActorId("world:street-bin-b");
            var ownership = new AssetOwnershipBook();
            ownership.TryRegister(id, world);
            var containers = new EconomicContainerRegistry();
            containers.TryRegister(source, world);
            containers.TryRegister(destination, Player);
            var ledger = new EconomicTransactionLedger();

            AssetTransferResult result = Service(ownership, containers, ledger).TransferStackAt(
                source, destination, id, 0, 0, false, world, Player, AssetTransferReason.Scavenging, 2, 615);

            Assert.That(result.Status, Is.EqualTo(AssetTransferStatus.DestinationFull));
            Assert.That(source.Items.Count, Is.EqualTo(1));
            Assert.That(source.Items[0].ItemInstanceId, Is.EqualTo(id));
            Assert.That(ownership.TryGetOwner(id, out EconomicActorId owner), Is.True);
            Assert.That(owner, Is.EqualTo(world));
            Assert.That(ledger.Count, Is.Zero);
        }

        [Test]
        public void OwnershipBookRejectsDuplicateRegistrationAndWrongOwnerRemoval()
        {
            var ownership = new AssetOwnershipBook(); Guid itemId = Guid.NewGuid();
            Assert.That(ownership.TryRegister(itemId, Player), Is.True);
            Assert.That(ownership.TryRegister(itemId, Milo), Is.False);
            Assert.That(ownership.TryRemove(itemId, Milo), Is.False);
            Assert.That(ownership.TryGetOwner(itemId, out EconomicActorId owner), Is.True);
            Assert.That(owner, Is.EqualTo(Player)); Assert.That(ownership.TryRemove(itemId, Player), Is.True);
            Assert.That(ownership.Count, Is.Zero);
        }

        [Test]
        public void SameOwnerContainerMovementRemainsAnInventoryTransfer()
        {
            ItemType type = Item(); var source = new GridInventory(); var destination = new GridInventory();
            source.Place(type, 0, 0, count: 3); Guid itemId = source.Items[0].ItemInstanceId;
            var ownership = new AssetOwnershipBook(); ownership.TryRegister(itemId, Player);
            var ledger = new EconomicTransactionLedger();
            InventoryTransferResult result = InventoryTransfer.MoveStack(source, destination, itemId);
            Assert.That(result.Success, Is.True); Assert.That(result.Moved, Is.EqualTo(3));
            Assert.That(ownership.TryGetOwner(itemId, out EconomicActorId owner), Is.True);
            Assert.That(owner, Is.EqualTo(Player)); Assert.That(ledger.Count, Is.Zero);
        }

        [Test]
        public void DestinationMustBeRegisteredToTheReceivingActor()
        {
            ItemType type = Item();
            var source = new GridInventory(); var destination = new GridInventory();
            source.Place(type, 0, 0);
            Guid itemId = source.Items[0].ItemInstanceId;
            var ownership = new AssetOwnershipBook(); ownership.TryRegister(itemId, Player);
            var containers = new EconomicContainerRegistry();
            containers.TryRegister(source, Milo);
            containers.TryRegister(destination, Player);
            var ledger = new EconomicTransactionLedger();
            AssetTransferResult result = Service(ownership, containers, ledger).TransferStack(source, destination, itemId,
                Player, Milo, AssetTransferReason.Gift, 1, 0);
            Assert.That(result.Status, Is.EqualTo(AssetTransferStatus.DestinationActorMismatch));
            Assert.That(source.Items.Count, Is.EqualTo(1));
            Assert.That(destination.Items, Is.Empty);
            Assert.That(ownership.TryGetOwner(itemId, out EconomicActorId owner), Is.True);
            Assert.That(owner, Is.EqualTo(Player));
            Assert.That(ledger.Count, Is.Zero);
        }

        [Test]
        public void UnregisteredContainerCannotParticipateInEconomicTransfer()
        {
            ItemType type = Item();
            var source = new GridInventory(); var destination = new GridInventory();
            source.Place(type, 0, 0);
            Guid itemId = source.Items[0].ItemInstanceId;
            var ownership = new AssetOwnershipBook(); ownership.TryRegister(itemId, Player);
            var containers = new EconomicContainerRegistry();
            containers.TryRegister(destination, Milo);
            var ledger = new EconomicTransactionLedger();
            AssetTransferResult result = Service(ownership, containers, ledger).TransferStack(source, destination, itemId,
                Player, Milo, AssetTransferReason.Gift, 1, 0);
            Assert.That(result.Status, Is.EqualTo(AssetTransferStatus.SourceContainerNotRegistered));
            Assert.That(source.Items.Count, Is.EqualTo(1));
            Assert.That(destination.Items, Is.Empty);
            Assert.That(ledger.Count, Is.Zero);
        }

        [Test]
        public void UnregisteredDestinationContainerIsRejected()
        {
            ItemType type = Item();
            var source = new GridInventory(); var destination = new GridInventory();
            source.Place(type, 0, 0);
            Guid itemId = source.Items[0].ItemInstanceId;
            var ownership = new AssetOwnershipBook(); ownership.TryRegister(itemId, Player);
            var containers = new EconomicContainerRegistry();
            containers.TryRegister(source, Milo);
            var ledger = new EconomicTransactionLedger();
            AssetTransferResult result = Service(ownership, containers, ledger).TransferStack(source, destination, itemId,
                Player, Milo, AssetTransferReason.Gift, 1, 0);
            Assert.That(result.Status, Is.EqualTo(AssetTransferStatus.DestinationContainerNotRegistered));
            Assert.That(source.Items.Count, Is.EqualTo(1));
            Assert.That(destination.Items, Is.Empty);
            Assert.That(ledger.Count, Is.Zero);
        }
    }
}

