using System;

/// <summary>Pure rules for validating a shop order and calculating its receipt.</summary>
public static class ShopSaleRules
{
    /// <summary>Calculates revenue for one quoted unit of a customer order.</summary>
    public static bool TryCalculateSale(
        string requestedItemId,
        string offeredItemId,
        int unitPrice,
        int availableQuantity,
        int requestedQuantity,
        out int total,
        out string reason)
    {
        total = 0;
        if (string.IsNullOrWhiteSpace(requestedItemId) ||
            string.IsNullOrWhiteSpace(offeredItemId) ||
            !string.Equals(requestedItemId, offeredItemId, StringComparison.OrdinalIgnoreCase))
        {
            reason = "That isn't what the customer asked for.";
            return false;
        }

        if (unitPrice <= 0)
        {
            reason = "A sale needs a positive quoted price.";
            return false;
        }

        if (requestedQuantity <= 0)
        {
            reason = "Order quantity must be at least one.";
            return false;
        }

        if (availableQuantity < requestedQuantity)
        {
            reason = "Stock won't cover the full order.";
            return false;
        }

        long calculatedTotal = (long)unitPrice * requestedQuantity;
        if (calculatedTotal > int.MaxValue)
        {
            reason = "The receipt exceeds the shop ledger's limit.";
            return false;
        }

        total = (int)calculatedTotal;
        reason = string.Empty;
        return true;
    }
}
