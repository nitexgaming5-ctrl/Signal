using UnityEngine;

/// <summary>One risky, replayable-per-day street search with readable outcomes.</summary>
public sealed class StreetCacheInteractable : InteractableBase
{
    [SerializeField] private int injuryDamage = 12;
    [SerializeField] private string watchFaction = "Block Watch";
    [SerializeField] private string crewFaction = "Canal Crew";

    private int lastScavengeDay = -1;
    public int LastScavengeDay => lastScavengeDay;

    private void Start()
    {
        SetPrompt("E  Search the street cache");
    }

    public void RestoreUsedDay(int day)
    {
        lastScavengeDay = day;
    }

    public override void Interact(InteractionController interactor)
    {
        if (GameManager.Instance == null)
        {
            interactor.ShowFeedback("No day ledger. The cache can wait.");
            return;
        }

        var day = GameManager.Instance.CurrentDay;
        var outing = FindAnyObjectByType<DistrictOutingSystem>();
        if (outing == null || !outing.IsOuting)
        {
            interactor.ShowFeedback("Start an outing from the shelter first. The city does not issue free loot bags.");
            return;
        }

        if (lastScavengeDay == day)
        {
            interactor.ShowFeedback("Picked clean for today. The rats left a review.");
            return;
        }

        lastScavengeDay = day;
        var player = interactor.GetComponent<PlayerState>();
        var relations = FindAnyObjectByType<FactionStandingSystem>();
        var roll = Random.Range(0, 100);

        if (roll < 10)
        {
            if (player != null) player.ApplyDamage(injuryDamage);
            if (relations != null) relations.ModifyStanding(crewFaction, -1);
            outing.AddExposure(15);
            interactor.ShowFeedback("A jagged can bites back. -" + injuryDamage + " HP; Canal Crew heard the noise.");
            return;
        }

        if (roll < 50)
        {
            var count = Random.Range(1, 3);
            RecordFind(outing, interactor, "Scrap", count,
                "Cache: Scrap x" + count + ". Somebody's old problem is your new shelf.");
        }
        else if (roll < 75)
        {
            RecordFind(outing, interactor, "Water", 1,
                "Cache: Water x1. The bottle is sealed. Mostly reassuring.");
        }
        else if (roll < 90)
        {
            RecordFind(outing, interactor, "Bandage", 1,
                "Cache: Bandage x1. Expiry date is a philosophical question.");
        }
        else
        {
            if (RecordFind(outing, interactor, "Street Evidence", 1,
                    "Found Street Evidence. Block Watch trust +1; reporting it may matter later.") && relations != null)
                relations.ModifyStanding(watchFaction, 1);
        }
    }

    private static bool RecordFind(DistrictOutingSystem outing, InteractionController interactor,
        string itemId, int amount, string feedback)
    {
        if (outing.TryAddLoot(itemId, amount))
        {
            interactor.ShowFeedback(feedback + " Carry it home to bank it.");
            return true;
        }

        interactor.ShowFeedback("The find slips away. Your outing ledger is unavailable.");
        return false;
    }
}
