using UnityEngine;

/// <summary>Placeholder exit interaction; future scenes can replace the feedback.</summary>
public sealed class DoorInteractable : InteractableBase
{
    public override void Interact(InteractionController interactor)
    {
        var state = interactor.GetComponent<PlayerState>();
        if (state != null)
            state.SetLocation("Contested Street");

        interactor.ShowFeedback("The street is open. The problems are worse.");
    }
}
