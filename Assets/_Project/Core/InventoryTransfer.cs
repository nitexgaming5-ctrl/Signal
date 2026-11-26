using System;
using System.Collections.Generic;

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

    /// <summary>
    /// Moves complete matching stacks while preserving stable identity. Cross-inventory
    /// mutations are completed silently first; observers are notified only after both grids
    /// contain the committed result.
    /// </summary>
    public static class InventoryTransfer
    {
        public static InventoryTransferResult MoveStack(GridInventory source, GridInventory destination, Guid itemInstanceId)
        {
            return MoveStackCore(source, destination, itemInstanceId, false, 0, 0, false, true);
        }

        internal static InventoryTransferResult MoveStackSilently(GridInventory source, GridInventory destination, Guid itemInstanceId)
        {
            return MoveStackCore(source, destination, itemInstanceId, false, 0, 0, false, false);
        }

        public static InventoryTransferResult MoveStackAt(GridInventory source, GridInventory destination, Guid itemInstanceId,
            int destinationX, int destinationY, bool rotated)
        {
            return MoveStackCore(source, destination, itemInstanceId, true, destinationX, destinationY, rotated, true);
        }

        internal static InventoryTransferResult MoveStackAtSilently(GridInventory source, GridInventory destination,
            Guid itemInstanceId, int destinationX, int destinationY, bool rotated)
        {
            return MoveStackCore(source, destination, itemInstanceId, true, destinationX, destinationY, rotated, false);
        }

        private static InventoryTransferResult MoveStackCore(GridInventory source, GridInventory destination,
            Guid itemInstanceId, bool positioned, int destinationX, int destinationY, bool rotated, bool notify)
        {
            ValidateInventories(source, destination);
            if (!source.TryGetItemInstance(itemInstanceId, out PlacedItem item))
                return new InventoryTransferResult(0, 0, true);

            InventorySnapshot sourceBefore = source.CreateSnapshot();
            InventorySnapshot destinationBefore = destination.CreateSnapshot();

            bool placed = positioned
                ? destination.TryPlaceTransferredSilently(item, destinationX, destinationY, rotated)
                : destination.TryPlaceTransferredSilently(item);
            if (!placed)
                return new InventoryTransferResult(0, item.Count, true);

            if (!source.TryRemoveItemInstanceSilently(item.ItemInstanceId))
            {
                source.RestoreSilently(sourceBefore);
                destination.RestoreSilently(destinationBefore);
                return new InventoryTransferResult(0, item.Count, false);
            }

            if (notify)
            {
                source.NotifyChanged();
                destination.NotifyChanged();
            }
            return new InventoryTransferResult(item.Count, 0, true);
        }

        public static InventoryTransferResult MoveAll(GridInventory source, GridInventory destination, ItemType type)
        {
            ValidateInventories(source, destination);
            int available = source.CountOf(type);
            if (available <= 0)
                return new InventoryTransferResult(0, 0, true);

            InventorySnapshot sourceBefore = source.CreateSnapshot();
            InventorySnapshot destinationBefore = destination.CreateSnapshot();
            var candidates = new List<PlacedItem>(source.Items);
            int moved = 0;

            foreach (PlacedItem item in candidates)
            {
                if (item.Type != type || !destination.TryPlaceTransferredSilently(item))
                    continue;

                if (!source.TryRemoveItemInstanceSilently(item.ItemInstanceId))
                {
                    source.RestoreSilently(sourceBefore);
                    destination.RestoreSilently(destinationBefore);
                    return new InventoryTransferResult(0, available, false);
                }
                moved += item.Count;
            }

            if (moved > 0)
            {
                source.NotifyChanged();
                destination.NotifyChanged();
            }
            return new InventoryTransferResult(moved, available - moved, true);
        }

        private static void ValidateInventories(GridInventory source, GridInventory destination)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (destination == null) throw new ArgumentNullException(nameof(destination));
            if (ReferenceEquals(source, destination))
                throw new ArgumentException("Source and destination must be different inventories.");
        }
    }
}
