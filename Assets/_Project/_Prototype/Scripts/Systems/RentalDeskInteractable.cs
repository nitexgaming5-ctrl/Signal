using UnityEngine;

/// <summary>Lets the player rent the next shelter tier from the test desk.</summary>
public sealed class RentalDeskInteractable : InteractableBase
{
    private ShelterManager shelterManager;

    private void Awake()
    {
        shelterManager = FindAnyObjectByType<ShelterManager>();
    }

    public override void Interact(InteractionController interactor)
    {
        if (shelterManager == null)
        {
            interactor.ShowFeedback("The rental office has been looted.");
            return;
        }

        shelterManager.TryRentNext(out var message);
        interactor.ShowFeedback(message);
    }
}
