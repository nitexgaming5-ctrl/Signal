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
            PlacedItem[] candidates = new PlacedItem[source.Items.Count];
            source.Items.CopyTo(candidates, 0);

            int moved = 0;
            foreach (PlacedItem item in candidates)
            {
                if (item.Type != type)
                    continue;

                // Keep stack identity intact: a transferred stack gets its own destination
                // placement. Distinct stacks are never silently merged.
                if (!destination.TryPlaceTransferred(item))
                    continue;

                if (!source.TryRemoveItemInstance(item.ItemInstanceId))
                {
                    source.Restore(sourceBefore);
                    destination.Restore(destinationBefore);
                    return new InventoryTransferResult(0, available, false);
                }

                moved += item.Count;
            }

            return new InventoryTransferResult(moved, available - moved, true);
        }
    }
}
