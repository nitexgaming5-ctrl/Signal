using System;

namespace Signal.Core
{
    public enum StackTransferStatus
    {
        Completed = 0,
        InvalidRequest = 1,
        SourceNotFound = 2,
        DestinationNotFound = 3,
        ContainerNotRegistered = 4,
        OwnershipNotRegistered = 5,
        OwnershipMismatch = 6,
        CrossOwnerMergeRejected = 7,
        IncompatibleStacks = 8,
        InsufficientQuantity = 9,
        DestinationFull = 10,
        TransferFailed = 11,
        DuplicateOperation = 12
    }

    public readonly struct StackTransferResult
    {
        internal StackTransferResult(StackTransferStatus status, int quantity, Guid resultingStackId)
        { Status = status; Quantity = quantity; ResultingStackId = resultingStackId; }
        public StackTransferStatus Status { get; }
        public int Quantity { get; }
        public Guid ResultingStackId { get; }
        public bool Success => Status == StackTransferStatus.Completed;
    }

    /// <summary>
    /// Coordinates physical quantity changes with live container authorization, stack ownership,
    /// economic transfer history and persistent merge lineage. A container must be registered
    /// while it is accessible; unregistering a dropped backpack immediately disables this service.
    /// </summary>
    public sealed class StackTransferService
    {
        private readonly AssetOwnershipBook _ownership;
        private readonly EconomicContainerRegistry _containers;
        private readonly EconomicTransactionLedger _transactions;
        private readonly StackLineageBook _lineage;

        public StackTransferService(AssetOwnershipBook ownership, EconomicContainerRegistry containers,
            EconomicTransactionLedger transactions, StackLineageBook lineage)
        {
            _ownership = ownership ?? throw new ArgumentNullException(nameof(ownership));
            _containers = containers ?? throw new ArgumentNullException(nameof(containers));
            _transactions = transactions ?? throw new ArgumentNullException(nameof(transactions));
            _lineage = lineage ?? throw new ArgumentNullException(nameof(lineage));
        }

        /// <summary>
        /// Splits a quantity into an empty cell. The original stack keeps its ID. The new stack
        /// inherits the source owner unless an explicit owner change is requested.
        /// </summary>
        public StackTransferResult Split(GridInventory source, GridInventory destination, Guid sourceStackId,
            int quantity, int destinationX, int destinationY, EconomicActorId expectedOwner,
            EconomicActorId newOwner, AssetTransferReason reason, int day, int minuteOfDay,
            Guid? transactionId = null, bool rotated = false)
        {
            if (source == null || destination == null || sourceStackId == Guid.Empty ||
                !expectedOwner.IsValid || !newOwner.IsValid || quantity < 1 ||
                (transactionId.HasValue && transactionId.Value == Guid.Empty) ||
                day < 1 || minuteOfDay < 0 || minuteOfDay >= 1440 ||
                !Enum.IsDefined(typeof(AssetTransferReason), reason) ||
                (reason == AssetTransferReason.Theft && newOwner == expectedOwner))
                return Result(StackTransferStatus.InvalidRequest);

            if (_lineage.IsRetiredStackId(sourceStackId))
                return Result(StackTransferStatus.InvalidRequest);
            if (!_containers.TryGetCustodian(source, out _) ||
                !_containers.TryGetCustodian(destination, out EconomicActorId destinationCustodian))
                return Result(StackTransferStatus.ContainerNotRegistered);
            if (!source.TryGetItemInstance(sourceStackId, out PlacedItem original))
                return Result(StackTransferStatus.SourceNotFound);
            if (!_ownership.TryGetOwner(sourceStackId, out EconomicActorId actualOwner))
                return Result(StackTransferStatus.OwnershipNotRegistered);
            if (actualOwner != expectedOwner)
                return Result(StackTransferStatus.OwnershipMismatch);
            if (!original.Type.IsStackable || quantity >= original.Count || quantity > original.Count - 1)
                return Result(StackTransferStatus.InsufficientQuantity);
            if (newOwner != actualOwner && destinationCustodian != newOwner)
                return Result(StackTransferStatus.OwnershipMismatch);

            InventorySnapshot sourceBefore = source.CreateSnapshot();
            InventorySnapshot destinationBefore = ReferenceEquals(source, destination)
                ? sourceBefore : destination.CreateSnapshot();

            Guid newStackId;
            do { newStackId = Guid.NewGuid(); }
            while (_lineage.IsRetiredStackId(newStackId) || _ownership.TryGetOwner(newStackId, out _));
            StackOperationResult physical = StackOperations.Split(source, destination, sourceStackId,
                quantity, destinationX, destinationY, rotated, newStackId, false);
            if (!physical.Success)
                return Result(physical.Status == StackOperationStatus.DestinationFull
                    ? StackTransferStatus.DestinationFull : StackTransferStatus.TransferFailed);

            newStackId = physical.ResultingStackId;
            if (reason == AssetTransferReason.Theft)
            {
                var stolenState = new ItemInstanceState(original.State.Quality, original.State.Condition,
                    original.State.Authenticity, ItemOwnershipStatus.Stolen);
                if (!destination.TryUpdateItemStateSilently(newStackId, stolenState))
                {
                    Restore(source, destination, sourceBefore, destinationBefore);
                    return Result(StackTransferStatus.TransferFailed);
                }
            }
            if (!_ownership.TryRegister(newStackId, newOwner))
            {
                Restore(source, destination, sourceBefore, destinationBefore);
                return Result(StackTransferStatus.TransferFailed);
            }

            if (newOwner != actualOwner)
            {
                var transaction = new EconomicTransaction(transactionId ?? Guid.NewGuid(), newStackId,
                    original.Type.Id, quantity, actualOwner, newOwner, reason, day, minuteOfDay);
                if (!_transactions.TryRecord(transaction))
                {
                    _ownership.TryRemove(newStackId, newOwner);
                    Restore(source, destination, sourceBefore, destinationBefore);
                    return Result(StackTransferStatus.TransferFailed);
                }
            }

            source.NotifyChanged();
            if (!ReferenceEquals(source, destination)) destination.NotifyChanged();
            return new StackTransferResult(StackTransferStatus.Completed, quantity, newStackId);
        }

        /// <summary>
        /// Merges only stacks with the same owner. Cross-owner merges are rejected because one
        /// surviving destination ID cannot faithfully represent two owners.
        /// </summary>
        public StackTransferResult Merge(GridInventory source, GridInventory destination,
            Guid sourceStackId, Guid destinationStackId, int quantity, int day, int minuteOfDay,
            Guid? operationId = null)
        {
            if (source == null || destination == null || sourceStackId == Guid.Empty ||
                destinationStackId == Guid.Empty || sourceStackId == destinationStackId ||
                quantity < 1 || day < 1 || minuteOfDay < 0 || minuteOfDay >= 1440 ||
                (operationId.HasValue && operationId.Value == Guid.Empty))
                return Result(StackTransferStatus.InvalidRequest);

            if (!_containers.TryGetCustodian(source, out EconomicActorId sourceCustodian) ||
                !_containers.TryGetCustodian(destination, out EconomicActorId destinationCustodian))
                return Result(StackTransferStatus.ContainerNotRegistered);
            if (!source.TryGetItemInstance(sourceStackId, out PlacedItem sourceItem))
                return Result(StackTransferStatus.SourceNotFound);
            if (!destination.TryGetItemInstance(destinationStackId, out PlacedItem destinationItem))
                return Result(StackTransferStatus.DestinationNotFound);
            if (!_ownership.TryGetOwner(sourceStackId, out EconomicActorId sourceOwner) ||
                !_ownership.TryGetOwner(destinationStackId, out EconomicActorId destinationOwner))
                return Result(StackTransferStatus.OwnershipNotRegistered);
            if (sourceOwner != destinationOwner)
                return Result(StackTransferStatus.CrossOwnerMergeRejected);
            if (!StackOperations.CanMerge(sourceItem, destinationItem))
                return Result(StackTransferStatus.IncompatibleStacks);
            if (quantity > sourceItem.Count)
                return Result(StackTransferStatus.InsufficientQuantity);

            if (_lineage.IsRetiredStackId(sourceStackId) || _lineage.IsRetiredStackId(destinationStackId))
                return Result(StackTransferStatus.InvalidRequest);
            Guid id = operationId ?? Guid.NewGuid();
            if (_lineage.ContainsOperation(id))
                return Result(StackTransferStatus.DuplicateOperation);

            int accepted = Math.Min(quantity, destinationItem.Type.MaxStack - destinationItem.Count);
            if (accepted < 1) return Result(StackTransferStatus.DestinationFull);
            var record = new StackMergeRecord(id, sourceStackId, destinationStackId, sourceItem.Type.Id,
                accepted, sourceOwner, sourceCustodian, destinationCustodian,
                StackLineageOperationKind.Merge, day, minuteOfDay, accepted == sourceItem.Count);

            InventorySnapshot sourceBefore = source.CreateSnapshot();
            InventorySnapshot destinationBefore = ReferenceEquals(source, destination)
                ? sourceBefore : destination.CreateSnapshot();
            StackOperationResult physical = StackOperations.Merge(source, destination, sourceStackId,
                destinationStackId, accepted, _lineage, id, record, false);
            if (!physical.Success)
                return Result(physical.Status == StackOperationStatus.DuplicateOperation
                    ? StackTransferStatus.DuplicateOperation : StackTransferStatus.TransferFailed);

            if (accepted == sourceItem.Count && !_ownership.TryRemove(sourceStackId, sourceOwner))
            {
                _lineage.TryRemoveMerge(id);
                Restore(source, destination, sourceBefore, destinationBefore);
                return Result(StackTransferStatus.TransferFailed);
            }

            source.NotifyChanged();
            if (!ReferenceEquals(source, destination)) destination.NotifyChanged();
            return new StackTransferResult(StackTransferStatus.Completed, accepted, destinationStackId);
        }

        private static void Restore(GridInventory source, GridInventory destination,
            InventorySnapshot sourceBefore, InventorySnapshot destinationBefore)
        {
            source.RestoreSilently(sourceBefore);
            if (!ReferenceEquals(source, destination)) destination.RestoreSilently(destinationBefore);
        }

        private static StackTransferResult Result(StackTransferStatus status) =>
            new StackTransferResult(status, 0, Guid.Empty);
    }
}
