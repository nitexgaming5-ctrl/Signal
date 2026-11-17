using System;

namespace Signal.Core
{
    public readonly struct InventoryTransferResult
    {
        internal InventoryTransferResult(int moved, int remaining, bool success)
        {
            Moved = moved;
            Remaining = remaining;
            Success = success;
        }

        public int Moved { get; }
        public int Remaining { get; }
        public bool Success { get; }
    }

    /// <summary>Moves item quantities between inventories without losing items when capacity is limited.</summary>
    public static class InventoryTransfer
    {
        public static InventoryTransferResult MoveAll(GridInventory source, GridInventory destination, ItemType type)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (destination == null) throw new ArgumentNullException(nameof(destination));
            if (ReferenceEquals(source, destination))
                throw new ArgumentException("Source and destination must be different inventories.");

            int available = source.CountOf(type);
            if (available <= 0)
                return new InventoryTransferResult(0, 0, true);

            InventorySnapshot sourceBefore = source.CreateSnapshot();
            InventorySnapshot destinationBefore = destination.CreateSnapshot();

            int remaining = destination.Add(type, available);
            int moved = available - remaining;
            if (moved == 0)
                return new InventoryTransferResult(0, available, true);

            if (source.TryRemove(type, moved))
                return new InventoryTransferResult(moved, remaining, true);

            source.Restore(sourceBefore);
            destination.Restore(destinationBefore);
            return new InventoryTransferResult(0, available, false);
        }
    }
}
