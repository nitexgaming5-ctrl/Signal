using System;
namespace Signal.Core
{
    public enum AssetTransferStatus
    {
        Completed = 0, InvalidRequest = 1, AssetNotFound = 2, OwnershipNotRegistered = 3,
        OwnershipMismatch = 4, SourceContainerNotRegistered = 5, DestinationContainerNotRegistered = 6, DestinationActorMismatch = 7, DestinationFull = 8, TransferFailed = 9
    }
    public readonly struct AssetTransferResult
    {
        internal AssetTransferResult(AssetTransferStatus status, EconomicTransaction transaction) { Status = status; Transaction = transaction; }
        public AssetTransferStatus Status { get; }
        public EconomicTransaction Transaction { get; }
        public bool Success => Status == AssetTransferStatus.Completed;
    }

    /// <summary>Transfers a whole physical stack, changes its owner and records the completed operation.</summary>
    public sealed class AssetTransferService
    {
        private readonly AssetOwnershipBook _ownership;
        private readonly EconomicContainerRegistry _containers;
        private readonly EconomicTransactionLedger _ledger;
        public AssetTransferService(AssetOwnershipBook ownership, EconomicContainerRegistry containers, EconomicTransactionLedger ledger)
        {
            _ownership = ownership ?? throw new ArgumentNullException(nameof(ownership));
            _containers = containers ?? throw new ArgumentNullException(nameof(containers));
            _ledger = ledger ?? throw new ArgumentNullException(nameof(ledger));
        }
        public AssetTransferResult TransferStack(GridInventory source, GridInventory destination, Guid itemInstanceId,
            EconomicActorId fromActor, EconomicActorId toActor, AssetTransferReason reason, int day, int minuteOfDay)
        {
            return TransferStackCore(source, destination, itemInstanceId, fromActor, toActor, reason, day, minuteOfDay,
                () => InventoryTransfer.MoveStack(source, destination, itemInstanceId));
        }

        public AssetTransferResult TransferStackAt(GridInventory source, GridInventory destination, Guid itemInstanceId,
            int destinationX, int destinationY, bool rotated, EconomicActorId fromActor, EconomicActorId toActor,
            AssetTransferReason reason, int day, int minuteOfDay)
        {
            return TransferStackCore(source, destination, itemInstanceId, fromActor, toActor, reason, day, minuteOfDay,
                () => InventoryTransfer.MoveStackAt(source, destination, itemInstanceId, destinationX, destinationY, rotated));
        }

        private AssetTransferResult TransferStackCore(GridInventory source, GridInventory destination, Guid itemInstanceId,
            EconomicActorId fromActor, EconomicActorId toActor, AssetTransferReason reason, int day, int minuteOfDay,
            Func<InventoryTransferResult> move)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (destination == null) throw new ArgumentNullException(nameof(destination));
            if (ReferenceEquals(source, destination) || itemInstanceId == Guid.Empty || !fromActor.IsValid || !toActor.IsValid ||
                fromActor == toActor || day < 1 || minuteOfDay < 0 || minuteOfDay >= 1440 ||
                !Enum.IsDefined(typeof(AssetTransferReason), reason))
                return new AssetTransferResult(AssetTransferStatus.InvalidRequest, null);
            if (!_containers.TryGetCustodian(source, out _))
                return new AssetTransferResult(AssetTransferStatus.SourceContainerNotRegistered, null);
            if (!_containers.TryGetCustodian(destination, out EconomicActorId destinationCustodian))
                return new AssetTransferResult(AssetTransferStatus.DestinationContainerNotRegistered, null);
            if (destinationCustodian != toActor)
                return new AssetTransferResult(AssetTransferStatus.DestinationActorMismatch, null);
            if (!source.TryGetItemInstance(itemInstanceId, out PlacedItem item))
                return new AssetTransferResult(AssetTransferStatus.AssetNotFound, null);
            if (!_ownership.TryGetOwner(itemInstanceId, out EconomicActorId currentOwner))
                return new AssetTransferResult(AssetTransferStatus.OwnershipNotRegistered, null);
            if (currentOwner != fromActor)
                return new AssetTransferResult(AssetTransferStatus.OwnershipMismatch, null);

            InventorySnapshot sourceBefore = source.CreateSnapshot();
            InventorySnapshot destinationBefore = destination.CreateSnapshot();
            InventoryTransferResult movement = move();
            if (!movement.Success) return new AssetTransferResult(AssetTransferStatus.TransferFailed, null);
            if (movement.Moved == 0) return new AssetTransferResult(AssetTransferStatus.DestinationFull, null);

            if (reason == AssetTransferReason.Theft)
            {
                var stolenState = new ItemInstanceState(item.State.Quality, item.State.Condition,
                    item.State.Authenticity, ItemOwnershipStatus.Stolen);
                if (!destination.TryUpdateItemState(itemInstanceId, stolenState))
                {
                    source.Restore(sourceBefore);
                    destination.Restore(destinationBefore);
                    return new AssetTransferResult(AssetTransferStatus.TransferFailed, null);
                }
            }

            if (!_ownership.TryTransferOwnership(itemInstanceId, fromActor, toActor))
            {
                source.Restore(sourceBefore); destination.Restore(destinationBefore);
                return new AssetTransferResult(AssetTransferStatus.TransferFailed, null);
            }
            var transaction = new EconomicTransaction(Guid.NewGuid(), item.ItemInstanceId, item.Type.Id, item.Count,
                fromActor, toActor, reason, day, minuteOfDay);
            if (!_ledger.TryRecord(transaction))
            {
                _ownership.TryTransferOwnership(itemInstanceId, toActor, fromActor);
                source.Restore(sourceBefore); destination.Restore(destinationBefore);
                return new AssetTransferResult(AssetTransferStatus.TransferFailed, null);
            }
            return new AssetTransferResult(AssetTransferStatus.Completed, transaction);
        }
    }
}
