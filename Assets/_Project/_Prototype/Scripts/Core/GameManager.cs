using System;
using UnityEngine;

/// <summary>
/// Owns game-wide progression state. Other systems should react to DayChanged
/// instead of changing the day directly, keeping future pressure systems loosely coupled.
/// </summary>
public sealed class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [field: SerializeField, Min(1)]
    public int CurrentDay { get; private set; } = 1;

    /// <summary>Raised after a loaded day value has been restored.</summary>
    public event Action<int> DayRestored;

    /// <summary>Raised after the day has advanced.</summary>
    public event Action<int> DayChanged;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    /// <summary>Restores persisted progression without replaying a sleep transition.</summary>
    public void RestoreDay(int day)
    {
        CurrentDay = Mathf.Max(1, day);
        DayRestored?.Invoke(CurrentDay);
    }

    /// <summary>Advances the game by one day and notifies interested systems.</summary>
    public void NextDay()
    {
        CurrentDay++;
        Debug.Log($"Day advanced → Day {CurrentDay}");
        DayChanged?.Invoke(CurrentDay);
    }
}
