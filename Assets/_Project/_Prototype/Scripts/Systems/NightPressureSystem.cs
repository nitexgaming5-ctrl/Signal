using System;
using UnityEngine;

/// <summary>Resolves the prototype's overnight bills and city supply swing.</summary>
public sealed class NightPressureSystem : MonoBehaviour
{
    [SerializeField, Min(0)] private int dailyRacket = 8;
    [SerializeField, Min(0)] private int weeklyRent = 35;
    [SerializeField, Min(1)] private int rentIntervalDays = 7;
    [SerializeField] private string racketFaction = "Canal Crew";

    private GameManager gameManager;
    private PlayerState playerState;
    private InventorySystem inventory;
    private int unpaidRent;
    private int unpaidRacket;

    public int UnpaidRent => unpaidRent;
    public int UnpaidRacket => unpaidRacket;
    public event Action<string> NightResolved;

    private void Awake()
    {
        gameManager = FindAnyObjectByType<GameManager>();
        playerState = FindAnyObjectByType<PlayerState>();
        inventory = FindAnyObjectByType<InventorySystem>();
    }

    private void OnEnable()
    {
        if (gameManager == null) gameManager = FindAnyObjectByType<GameManager>();
        if (gameManager != null) gameManager.DayChanged += ResolveNight;
    }

    private void OnDisable()
    {
        if (gameManager != null) gameManager.DayChanged -= ResolveNight;
    }

    public void RestoreArrears(int rent, int racket)
    {
        unpaidRent = Mathf.Max(0, rent);
        unpaidRacket = Mathf.Max(0, racket);
    }

    private void ResolveNight(int newDay)
    {
        if (playerState == null) playerState = FindAnyObjectByType<PlayerState>();
        if (inventory == null) inventory = FindAnyObjectByType<InventorySystem>();
        if (playerState == null) return;

        var demand = dailyRacket;
        var relations = FindAnyObjectByType<FactionStandingSystem>();
        var crewStanding = relations == null ? 0 : relations.GetStanding(racketFaction);
        if (crewStanding <= -10)
            demand += 4;
        else if (crewStanding >= 10)
            demand = Mathf.Max(0, demand - 4);

        var lines = "Night ledger: " + racketFaction + " demand " + demand + "c.";
        unpaidRacket += demand;
        var racketPaid = Mathf.Min(playerState.Money, unpaidRacket);
        playerState.TrySpendMoney(racketPaid);
        unpaidRacket -= racketPaid;
        if (unpaidRacket > 0)
            lines += " Racket arrears: " + unpaidRacket + "c.";
        else
            lines += " Collected.";

        if (newDay % rentIntervalDays == 0)
        {
            unpaidRent += weeklyRent;
            var rentPaid = Mathf.Min(playerState.Money, unpaidRent);
            playerState.TrySpendMoney(rentPaid);
            unpaidRent -= rentPaid;
            lines += " Rent due " + weeklyRent + "c.";
            if (unpaidRent > 0)
                lines += " Rent arrears: " + unpaidRent + "c.";
        }

        if (inventory != null && newDay % 3 == 0)
        {
            inventory.AddItem("Water", 1);
            lines += " Pipe outage eased; +1 Water recovered.";
        }

        Debug.Log(lines);
        NightResolved?.Invoke(lines);
    }
}
