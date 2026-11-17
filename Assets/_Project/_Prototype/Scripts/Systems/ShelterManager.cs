using System;
using UnityEngine;

/// <summary>Data for a rentable player base. Values are intentionally prototype-sized.</summary>
[Serializable]
public sealed class ShelterOption
{
    public string displayName;
    public string description;
    [Min(0)] public int rentCost;
    [Min(1)] public int storageCapacity;
    [Range(0, 100)] public int comfort;
}

/// <summary>
/// Owns the current player base. The visual shelter can be swapped later while
/// this authority keeps rent, capacity, comfort, and location consistent.
/// </summary>
public sealed class ShelterManager : MonoBehaviour
{
    [SerializeField]
    private ShelterOption[] options;

    [SerializeField, Min(0)]
    private int currentShelterIndex;

    public event Action<ShelterOption> ShelterChanged;
    public ShelterOption Current => options != null && options.Length > 0 ? options[currentShelterIndex] : null;
    public int CurrentIndex => currentShelterIndex;
    public ShelterOption[] Options => options;

    private void Awake()
    {
        EnsureDefaults();
        currentShelterIndex = Mathf.Clamp(currentShelterIndex, 0, options.Length - 1);
        var playerState = FindAnyObjectByType<PlayerState>();
        if (playerState != null)
            playerState.SetLocation(Current.displayName);
    }

    public void RestoreIndex(int shelterIndex)
    {
        EnsureDefaults();
        currentShelterIndex = Mathf.Clamp(shelterIndex, 0, options.Length - 1);
        var playerState = FindAnyObjectByType<PlayerState>();
        if (playerState != null && Current != null)
            playerState.SetLocation(Current.displayName);
        ShelterChanged?.Invoke(Current);
    }

    public bool TryRentNext(out string message)
    {
        EnsureDefaults();
        var nextIndex = currentShelterIndex + 1;
        if (nextIndex >= options.Length)
        {
            message = "No higher listing. The bunker is already judging you.";
            return false;
        }

        var next = options[nextIndex];
        var playerState = FindAnyObjectByType<PlayerState>();
        if (playerState == null || !playerState.TrySpendMoney(next.rentCost))
        {
            message = $"Not enough cash for {next.displayName}. The landlord remains unmoved.";
            return false;
        }

        currentShelterIndex = nextIndex;
        playerState.SetLocation(next.displayName);
        ShelterChanged?.Invoke(next);
        Debug.Log($"Shelter rented → {next.displayName}");
        message = $"New base rented: {next.displayName}. Rent paid: {next.rentCost}.";
        return true;
    }

    private void EnsureDefaults()
    {
        if (options != null && options.Length > 0)
            return;

        options = new[]
        {
            new ShelterOption { displayName = "Basement Shelter", description = "Dry enough. Mostly.", rentCost = 0, storageCapacity = 8, comfort = 10 },
            new ShelterOption { displayName = "Cabin", description = "A door that almost locks.", rentCost = 80, storageCapacity = 16, comfort = 25 },
            new ShelterOption { displayName = "Small House", description = "Two rooms and one suspicious stain.", rentCost = 250, storageCapacity = 28, comfort = 45 },
            new ShelterOption { displayName = "Villa", description = "Walls thick enough for rich mistakes.", rentCost = 700, storageCapacity = 50, comfort = 70 },
            new ShelterOption { displayName = "Bunker", description = "No windows. No excuses.", rentCost = 1200, storageCapacity = 80, comfort = 90 },
        };
    }
}
