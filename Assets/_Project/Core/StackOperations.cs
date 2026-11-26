using System;
using System.Collections.Generic;

namespace Signal.Core
{
    public enum StackOperationStatus
    {
        Completed = 0,
        InvalidRequest = 1,
        SourceNotFound = 2,
        DestinationNotFound = 3,
        IncompatibleStacks = 4,
        InsufficientQuantity = 5,
        DestinationFull = 6,
        DuplicateOperation = 7
    }

    public readonly struct StackOperationResult
    {
        internal StackOperationResult(StackOperationStatus status, int quantity, Guid resultingStackId)
        {
            Status = status;
            Quantity = quantity;
            ResultingStackId = resultingStackId;
        }

        public StackOperationStatus Status { get; }
        public int Quantity { get; }
        public Guid ResultingStackId { get; }
        public bool Success => Status == StackOperationStatus.Completed;
    }

    public readonly struct StackMergeRecord
    {
        public StackMergeRecord(Guid operationId, Guid sourceStackId, Guid destinationStackId,
            Guid itemDefinitionId, int quantity)
        {
            OperationId = operationId;
            SourceStackId = sourceStackId;
            DestinationStackId = destinationStackId;
            ItemDefinitionId = itemDefinitionId;
            Quantity = quantity;
        }

        public Guid OperationId { get; }
        public Guid SourceStackId { get; }
        public Guid DestinationStackId { get; }
        public Guid ItemDefinitionId { get; }
        public int Quantity { get; }
    }

    /// <summary>
    /// Records the identity lineage of physical stack merges. Persistence and actor ownership
    /// reconciliation are added by the Game/economy integration phase.
    /// </summary>
    public sealed class StackLineageBook
    {
        private readonly List<StackMergeRecord> _merges = new List<StackMergeRecord>();
        private readonly HashSet<Guid> _operationIds = new HashSet<Guid>();

        public IReadOnlyList<StackMergeRecord> Merges => _merges.AsReadOnly();

        public bool TryRecordMerge(StackMergeRecord record)
        {
            if (record.OperationId == Guid.Empty || record.SourceStackId == Guid.Empty ||
                record.DestinationStackId == Guid.Empty || record.SourceStackId == record.DestinationStackId ||
                record.ItemDefinitionId == Guid.Empty || record.Quantity < 1 ||
                !_operationIds.Add(record.OperationId))
                return false;

            _merges.Add(record);
            return true;
        }

        public void Clear()
        {
            _merges.Clear();
            _operationIds.Clear();
        }
    }

    /// <summary>
    /// Atomic physical stack operations. These methods do not change actor ownership;
    /// ownership-aware callers must coordinate AssetOwnershipBook before exposing a commit.
    /// </summary>
    public static class StackOperations
    {
        public static bool CanMerge(PlacedItem source, PlacedItem destination)
        {
            return source.ItemInstanceId != Guid.Empty && destination.ItemInstanceId != Guid.Empty &&
                source.ItemInstanceId != destination.ItemInstanceId &&
                source.Type.IsValid && destination.Type.IsValid &&
                source.Type == destination.Type && source.Type.MaxStack > 1 &&
                source.State == destination.State && source.Count > 0 && destination.Count > 0 &&
                destination.Count < destination.Type.MaxStack;
        }

        public static StackOperationResult Split(GridInventory source, GridInventory destination,
            Guid sourceStackId, int quantity, int destinationX, int destinationY, bool rotated = false,
            Guid? newStackId = null)
        {
            if (source == null || destination == null || sourceStackId == Guid.Empty ||
                quantity < 1 || (newStackId.HasValue && newStackId.Value == Guid.Empty))
                return Result(StackOperationStatus.InvalidRequest);

            if (!source.TryGetItemInstance(sourceStackId, out PlacedItem original))
                return Result(StackOperationStatus.SourceNotFound);
            if (original.Type.MaxStack <= 1 || quantity >= original.Count)
                return Result(StackOperationStatus.InsufficientQuantity);

            Guid splitId = newStackId ?? Guid.NewGuid();
            if (splitId == sourceStackId || source.TryGetItemInstance(splitId, out _) ||
                destination.TryGetItemInstance(splitId, out _))
                return Result(StackOperationStatus.InvalidRequest);
            if (!destination.CanPlace(original.Type, destinationX, destinationY, rotated))
                return Result(StackOperationStatus.DestinationFull);

            InventorySnapshot sourceBefore = source.CreateSnapshot();
            InventorySnapshot destinationBefore = ReferenceEquals(source, destination)
                ? sourceBefore : destination.CreateSnapshot();

            if (!source.TryRemoveItemInstanceSilently(sourceStackId, quantity) ||
                !destination.PlaceSilently(original.Type, destinationX, destinationY, rotated,
                    quantity, splitId, original.State).HasValue)
            {
                source.RestoreSilently(sourceBefore);
                if (!ReferenceEquals(source, destination))
                    destination.RestoreSilently(destinationBefore);
                return Result(StackOperationStatus.DestinationFull);
            }

            source.NotifyChanged();
            if (!ReferenceEquals(source, destination))
                destination.NotifyChanged();
            return new StackOperationResult(StackOperationStatus.Completed, quantity, splitId);
        }

        public static StackOperationResult Merge(GridInventory source, GridInventory destination,
            Guid sourceStackId, Guid destinationStackId, int quantity, StackLineageBook lineage,
            Guid? operationId = null)
        {
            if (source == null || destination == null || lineage == null ||
                sourceStackId == Guid.Empty || destinationStackId == Guid.Empty ||
                sourceStackId == destinationStackId || quantity < 1 ||
                (operationId.HasValue && operationId.Value == Guid.Empty))
                return Result(StackOperationStatus.InvalidRequest);

            if (!source.TryGetItemInstance(sourceStackId, out PlacedItem sourceItem))
                return Result(StackOperationStatus.SourceNotFound);
            if (!destination.TryGetItemInstance(destinationStackId, out PlacedItem destinationItem))
                return Result(StackOperationStatus.DestinationNotFound);
            if (!CanMerge(sourceItem, destinationItem))
                return Result(StackOperationStatus.IncompatibleStacks);
            if (quantity > sourceItem.Count)
                return Result(StackOperationStatus.InsufficientQuantity);

            int accepted = Math.Min(quantity, destinationItem.Type.MaxStack - destinationItem.Count);
            if (accepted < 1)
                return Result(StackOperationStatus.DestinationFull);

            Guid transactionId = operationId ?? Guid.NewGuid();
            var record = new StackMergeRecord(transactionId, sourceStackId, destinationStackId,
                sourceItem.Type.Id, accepted);

            InventorySnapshot sourceBefore = source.CreateSnapshot();
            InventorySnapshot destinationBefore = ReferenceEquals(source, destination)
                ? sourceBefore : destination.CreateSnapshot();

            bool sourceReduced = source.TryRemoveItemInstanceSilently(sourceStackId, accepted);
            bool destinationRemoved = destination.TryRemoveItemInstanceSilently(destinationStackId);
            bool destinationRecreated = destinationRemoved &&
                destination.PlaceSilently(destinationItem.Type, destinationItem.X, destinationItem.Y,
                    destinationItem.Rotated, destinationItem.Count + accepted,
                    destinationStackId, destinationItem.State).HasValue;

            if (!sourceReduced || !destinationRecreated || !lineage.TryRecordMerge(record))
            {
                source.RestoreSilently(sourceBefore);
                if (!ReferenceEquals(source, destination))
                    destination.RestoreSilently(destinationBefore);
                return Result(StackOperationStatus.InvalidRequest);
            }

            source.NotifyChanged();
            if (!ReferenceEquals(source, destination))
                destination.NotifyChanged();

            return new StackOperationResult(StackOperationStatus.Completed, accepted, destinationStackId);
        }

        private static StackOperationResult Result(StackOperationStatus status)
        {
            return new StackOperationResult(status, 0, Guid.Empty);
        }
    }
}
