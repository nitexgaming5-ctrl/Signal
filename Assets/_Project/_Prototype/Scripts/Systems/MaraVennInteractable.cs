using UnityEngine;

/// <summary>One recurring neighborhood contact and her small, persistent water request.</summary>
public sealed class MaraVennInteractable : InteractableBase
{
    private const string WaterFaction = "Water Co-op";
    private const int RequestReward = 18;

    [SerializeField, Range(0, 100)] private int trust;
    [SerializeField] private bool requestAccepted;
    [SerializeField] private bool requestComplete;

    public int Trust => trust;
    public bool RequestAccepted => requestAccepted;
    public bool RequestComplete => requestComplete;

    private void Start()
    {
        SetPrompt("E  Talk to Mara Venn");
    }

    public void RestoreState(int savedTrust, bool accepted, bool complete)
    {
        trust = Mathf.Clamp(savedTrust, 0, 100);
        requestComplete = complete;
        requestAccepted = accepted || complete;
    }

    public override void Interact(InteractionController interactor)
    {
        if (!requestAccepted)
        {
            requestAccepted = true;
            interactor.ShowFeedback("Mara Venn: One clean bottle of water. I can pay 18c. The pump has started tasting like a court case.");
            return;
        }

        if (requestComplete)
        {
            interactor.ShowFeedback("Mara Venn: Still here. Still thirsty. At least the pump has stopped making that noise.");
            return;
        }

        var inventory = interactor.GetComponent<InventorySystem>();
        if (inventory == null || !inventory.RemoveItem("Water", 1))
        {
            interactor.ShowFeedback("Mara Venn: I asked for water, not a compelling story about water.");
            return;
        }

        requestComplete = true;
        trust = Mathf.Clamp(trust + 1, 0, 100);

        var player = interactor.GetComponent<PlayerState>();
        if (player != null)
            player.AddMoney(RequestReward);

        var factions = FindAnyObjectByType<FactionStandingSystem>();
        if (factions != null)
            factions.ModifyStanding(WaterFaction, 2);

        interactor.ShowFeedback("Mara takes the bottle. +18c, personal trust +1, Water Co-op standing +2. A rare transaction without shouting.");
    }
}
