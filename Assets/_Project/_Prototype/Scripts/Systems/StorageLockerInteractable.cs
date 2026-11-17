using UnityEngine;

/// <summary>Prototype storage interaction that proves the inventory loop.</summary>
public sealed class StorageLockerInteractable : InteractableBase
{
    public override void Interact(InteractionController interactor)
    {
        var inventory = interactor.GetComponent<InventorySystem>();
        if (inventory == null)
        {
            interactor.ShowFeedback("The locker has no inventory attached.");
            return;
        }

        inventory.AddItem("Scrap", 1);
        interactor.ShowFeedback("Locker opened: recovered 1 scrap.");
    }
}
