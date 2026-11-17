using UnityEngine;

/// <summary>Fulfils one repeatable shop order using the player's persisted inventory and cash.</summary>
public sealed class ShopCounterInteractable : InteractableBase
{
    [SerializeField] private string requestedItemId = "Purified Water";
    [SerializeField, Min(0)] private int requestedQuantity = 1;
    [SerializeField, Min(0)] private int unitPrice = 12;

    private void Start()
    {
        RefreshPrompt();
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        RefreshPrompt();
    }
#endif

    public override void Interact(InteractionController interactor)
    {
        if (interactor == null)
            return;

        var inventory = interactor.GetComponent<InventorySystem>();
        var player = interactor.GetComponent<PlayerState>();
        if (inventory == null || player == null)
        {
            interactor.ShowFeedback("Counter ledger unavailable. No stock moved, no cash changed.");
            return;
        }

        if (!ShopSaleRules.TryCalculateSale(
                requestedItemId,
                requestedItemId,
                unitPrice,
                inventory.GetQuantity(requestedItemId),
                requestedQuantity,
                out var total,
                out var reason))
        {
            interactor.ShowFeedback(reason);
            return;
        }

        if (!inventory.RemoveItem(requestedItemId, requestedQuantity))
        {
            interactor.ShowFeedback("Stock changed before the receipt printed. Sale cancelled; no cash charged.");
            return;
        }

        player.AddMoney(total);
        interactor.ShowFeedback(
            $"Order filled: {requestedQuantity}x {requestedItemId} for {total} cash. The customer checks the door twice.");
    }

    private void RefreshPrompt()
    {
        var itemName = string.IsNullOrWhiteSpace(requestedItemId) ? "requested stock" : requestedItemId;
        SetPrompt($"E  Sell {requestedQuantity}x {itemName}  ·  {unitPrice}c");
    }
}
