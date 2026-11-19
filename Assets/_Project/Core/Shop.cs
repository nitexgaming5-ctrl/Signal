using System;

namespace Signal.Core
{
    public readonly struct ShopOrder
    {
        public ShopOrder(ItemType item, int quantity, int unitPrice)
        {
            if (quantity < 1)
                throw new ArgumentOutOfRangeException(nameof(quantity), "Quantity must be at least 1.");
            if (unitPrice < 0)
                throw new ArgumentOutOfRangeException(nameof(unitPrice), "Price cannot be negative.");

            Item = item;
            Quantity = quantity;
            UnitPrice = unitPrice;
        }

        public ItemType Item { get; }

        public int Quantity { get; }

        public int UnitPrice { get; }

        public long Total => (long)Quantity * UnitPrice;
    }

    public enum FulfilResult
    {
        Done,
        NotEnoughStock,
        InvalidOrder
    }

    public static class Shop
    {
        /// <summary>Takes the goods out of the inventory and pays the player, or changes nothing.</summary>
        public static FulfilResult TryFulfil(ShopOrder order, GridInventory inventory, PlayerStats stats)
        {
            if (inventory == null)
                throw new ArgumentNullException(nameof(inventory));
            if (stats == null)
                throw new ArgumentNullException(nameof(stats));

            if (order.Quantity < 1 || order.Item.Id == Guid.Empty || order.UnitPrice <= 0 ||
                order.Total < 1 || order.Total > int.MaxValue)
                return FulfilResult.InvalidOrder;

            if (inventory.CountOf(order.Item) < order.Quantity)
                return FulfilResult.NotEnoughStock;

            InventorySnapshot before = inventory.CreateSnapshot();
            if (!inventory.TryRemove(order.Item, order.Quantity))
                return FulfilResult.NotEnoughStock;

            if (!stats.TryEarn((int)order.Total))
            {
                inventory.Restore(before);
                return FulfilResult.InvalidOrder;
            }

            return FulfilResult.Done;
        }
    }
}
