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
        /// <summary>Consumes one exact player-owned stack quantity and credits clean CARD atomically.</summary>
        public static FulfilResult TryFulfilExact(ShopOrder order, Guid itemInstanceId, string businessId,
            EconomicActorId seller, GridInventory inventory, AssetOwnershipBook ownership,
            CleanMoneyAccount account, ShopSaleTransactionLedger ledger, int day, int minuteOfDay)
        {
            if (inventory == null) throw new ArgumentNullException(nameof(inventory));
            if (ownership == null) throw new ArgumentNullException(nameof(ownership));
            if (account == null) throw new ArgumentNullException(nameof(account));
            if (ledger == null) throw new ArgumentNullException(nameof(ledger));

            if (order.Quantity < 1 || order.Item.Id == Guid.Empty || order.UnitPrice <= 0 ||
                order.Total < 1 || order.Total > int.MaxValue || itemInstanceId == Guid.Empty ||
                string.IsNullOrWhiteSpace(businessId) || !seller.IsValid ||
                day < 1 || minuteOfDay < 0 || minuteOfDay >= 1440)
                return FulfilResult.InvalidOrder;

            if (!inventory.TryGetItemInstance(itemInstanceId, out PlacedItem selected) ||
                selected.Type != order.Item || selected.Count < order.Quantity)
                return FulfilResult.NotEnoughStock;

            if (!ownership.TryGetOwner(itemInstanceId, out EconomicActorId currentOwner) || currentOwner != seller)
                return FulfilResult.InvalidOrder;

            int total = (int)order.Total;
            int balanceBefore = account.Balance;
            InventorySnapshot inventoryBefore = inventory.CreateSnapshot();
            bool fullStack = order.Quantity == selected.Count;

            if (!inventory.TryRemoveItemInstance(itemInstanceId, order.Quantity))
                return FulfilResult.NotEnoughStock;

            bool ownershipRemoved = false;
            if (fullStack)
            {
                ownershipRemoved = ownership.TryRemove(itemInstanceId, seller);
                if (!ownershipRemoved)
                {
                    inventory.Restore(inventoryBefore);
                    return FulfilResult.InvalidOrder;
                }
            }

            if (!account.TryCredit(total))
            {
                inventory.Restore(inventoryBefore);
                if (ownershipRemoved)
                    ownership.TryRegister(itemInstanceId, seller);
                return FulfilResult.InvalidOrder;
            }

            var transaction = new ShopSaleTransaction(Guid.NewGuid(), businessId, seller,
                itemInstanceId, selected.Type.Id, order.Quantity, order.UnitPrice, total,
                selected.State, day, minuteOfDay);
            if (!ledger.TryRecord(transaction))
            {
                inventory.Restore(inventoryBefore);
                account.Restore(balanceBefore);
                if (ownershipRemoved)
                    ownership.TryRegister(itemInstanceId, seller);
                return FulfilResult.InvalidOrder;
            }

            return FulfilResult.Done;
        }

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
