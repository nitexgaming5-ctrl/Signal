using UnityEngine;

/// <summary>Door used by both the exterior entrance and the interior exit.</summary>
public sealed class ShelterDoorInteractable : InteractableBase
{
    private ShelterSpaceController spaceController;

    private void Awake()
    {
        spaceController = FindAnyObjectByType<ShelterSpaceController>();
    }

    public override void Interact(InteractionController interactor)
    {
        if (spaceController == null)
        {
            interactor.ShowFeedback("The door has forgotten where it leads.");
            return;
        }

        spaceController.Toggle(interactor);
    }
}
