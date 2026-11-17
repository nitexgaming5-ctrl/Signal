using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Owns the first district outing: leave the shelter, carry unbanked finds,
/// accumulate exposure, and bank what survives inspection on a safe return.
/// </summary>
public sealed class DistrictOutingSystem : MonoBehaviour
{
    [SerializeField, Range(0, 100)] private int exposurePerSearch = 30;
    [SerializeField, Range(0, 100)] private int maximumExposure = 90;
    [SerializeField] private string inspectionFaction = "Block Watch";

    private readonly List<InventorySystem.Stack> carriedLoot = new List<InventorySystem.Stack>();
    private ShelterSpaceController shelterSpace;
    private InventorySystem inventory;
    private FactionStandingSystem factions;
    private GameManager game;
    private int exposure;
    private int startedOnDay;
    private bool outingActive;

    public event Action<string> StatusChanged;
    public bool IsOuting => outingActive;
    public int Exposure => exposure;
    public int StartedOnDay => startedOnDay;
    public IReadOnlyList<InventorySystem.Stack> CarriedLoot => carriedLoot;

    private void Awake()
    {
        CacheReferences();
    }

    private void Start()
    {
        CacheReferences();
        if (shelterSpace != null)
            shelterSpace.SpaceChanged += OnSpaceChanged;

        // The shelter controller initializes its starting space in Awake, so
        // this is deterministic even if Start order differs between objects.
        if (shelterSpace == null || !shelterSpace.IsInside)
            BeginOuting();
    }

    private void OnDestroy()
    {
        if (shelterSpace != null)
            shelterSpace.SpaceChanged -= OnSpaceChanged;
    }

    /// <summary>Stages loot outside inventory until a safe return banks it.</summary>
    public bool TryAddLoot(string itemId, int amount)
    {
        if (!outingActive || string.IsNullOrWhiteSpace(itemId) || amount <= 0)
            return false;

        var stack = carriedLoot.Find(item => string.Equals(item.itemId, itemId, StringComparison.OrdinalIgnoreCase));
        if (stack == null)
            carriedLoot.Add(new InventorySystem.Stack(itemId, amount));
        else
            stack.quantity += amount;

        exposure = Mathf.Clamp(exposure + exposurePerSearch, 0, maximumExposure);
        RaiseStatus("Haul secured for now. Return exposure: " + exposure + "%. Nothing is banked until you get home.");
        return true;
    }

    /// <summary>Applies a noisy-search penalty without silently granting loot.</summary>
    public void AddExposure(int amount)
    {
        if (!outingActive || amount <= 0)
            return;

        exposure = Mathf.Clamp(exposure + amount, 0, maximumExposure);
        RaiseStatus("Noise carries. Return exposure is now " + exposure + "%. The exit is still open.");
    }

    /// <summary>Restores an outing snapshot; version-1 saves pass an empty haul.</summary>
    public void RestoreRun(bool active, int savedExposure, int savedStartDay, List<InventorySystem.Stack> savedLoot)
    {
        CacheReferences();
        outingActive = active && (shelterSpace == null || !shelterSpace.IsInside);
        exposure = outingActive ? Mathf.Clamp(savedExposure, 0, maximumExposure) : 0;
        startedOnDay = outingActive ? Mathf.Max(1, savedStartDay) : 0;
        carriedLoot.Clear();

        if (outingActive && savedLoot != null)
        {
            foreach (var item in savedLoot)
            {
                if (item != null && !string.IsNullOrWhiteSpace(item.itemId) && item.quantity > 0)
                    carriedLoot.Add(new InventorySystem.Stack(item.itemId, item.quantity));
            }
        }

        RaiseStatus(outingActive
            ? "Outing restored. Carried goods remain at risk; the shelter door is your return route."
            : "At the shelter. Pack light; the city is not sentimental.");
    }

    private void CacheReferences()
    {
        if (shelterSpace == null) shelterSpace = FindAnyObjectByType<ShelterSpaceController>();
        if (inventory == null) inventory = FindAnyObjectByType<InventorySystem>();
        if (factions == null) factions = FindAnyObjectByType<FactionStandingSystem>();
        if (game == null) game = FindAnyObjectByType<GameManager>();
    }

    private void OnSpaceChanged(bool inside)
    {
        if (inside)
        {
            if (outingActive)
                ResolveReturn();
        }
        else if (!outingActive)
        {
            BeginOuting();
        }
    }

    private void BeginOuting()
    {
        if (outingActive)
            return;

        CacheReferences();
        outingActive = true;
        exposure = 0;
        startedOnDay = game == null ? 1 : game.CurrentDay;
        carriedLoot.Clear();
        RaiseStatus("Street outing started. Scavenge, watch your exposure, then return through the shelter door.");
    }

    private void ResolveReturn()
    {
        CacheReferences();
        var lostItem = false;
        var summary = "Safe return.";

        if (carriedLoot.Count > 0 && exposure > 0 && UnityEngine.Random.Range(0, 100) < exposure)
        {
            var lostStack = carriedLoot[UnityEngine.Random.Range(0, carriedLoot.Count)];
            lostStack.quantity--;
            if (lostStack.quantity <= 0)
                carriedLoot.Remove(lostStack);
            if (factions != null)
                factions.ModifyStanding(inspectionFaction, -1);
            lostItem = true;
            summary = "A Block Watch checkpoint confiscates one carried item; trust -1. The rest gets through.";
        }

        if (inventory != null)
        {
            foreach (var item in carriedLoot)
                inventory.AddItem(item.itemId, item.quantity);
        }

        var banked = carriedLoot.Count;
        carriedLoot.Clear();
        outingActive = false;
        exposure = 0;
        startedOnDay = 0;

        if (!lostItem && banked > 0)
            summary += " Haul banked in the shelter. It is ugly, but it is yours.";
        else if (banked == 0)
            summary += " No goods to bank.";

        RaiseStatus(summary);
    }

    private void RaiseStatus(string message)
    {
        StatusChanged?.Invoke(message);
        Debug.Log("District outing: " + message);
    }
}
