using UnityEngine;

/// <summary>Prototype bed interaction. It is the first interactable owner of sleep.</summary>
public sealed class BedInteractable : InteractableBase
{
    [SerializeField]
    private DayCycle dayCycle;

    private void Awake()
    {
        if (dayCycle == null)
            dayCycle = FindAnyObjectByType<DayCycle>();
    }

    public override void Interact(InteractionController interactor)
    {
        if (dayCycle == null)
        {
            interactor.ShowFeedback("The bed is here. Time is not.");
            return;
        }

        dayCycle.Sleep();
        interactor.ShowFeedback("You sleep. The city keeps its receipts.");
    }
}
