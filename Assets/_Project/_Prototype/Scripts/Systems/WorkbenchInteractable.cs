using UnityEngine;

/// <summary>Small recipe station proving resource-in/resource-out crafting.</summary>
public sealed class WorkbenchInteractable : InteractableBase
{
    public override void Interact(InteractionController interactor)
    {
        var inventory = interactor.GetComponent<InventorySystem>();
        if (inventory == null)
        {
            interactor.ShowFeedback("No ledger, no chemistry. Attach an inventory first.");
            return;
        }

        // Deliberately low-tech starter recipes; deeper recipe data can replace these rules.
        if (inventory.GetQuantity("Water") > 0)
        {
            inventory.RemoveItem("Water", 1);
            inventory.AddItem("Purified Water", 1);
            interactor.ShowFeedback("Purified Water x1. Filter's probably older than the war.");
            return;
        }

        if (inventory.GetQuantity("Scrap") >= 2)
        {
            inventory.RemoveItem("Scrap", 2);
            inventory.AddItem("Field Patch", 1);
            interactor.ShowFeedback("Field Patch x1. It will hold until it doesn't.");
            return;
        }

        interactor.ShowFeedback("Workbench: need 1 Water or 2 Scrap for a starter recipe.");
    }
}
