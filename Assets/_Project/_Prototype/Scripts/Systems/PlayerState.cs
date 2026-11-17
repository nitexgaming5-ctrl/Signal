using System;
using UnityEngine;

/// <summary>
/// Small player-owned state container. It intentionally contains only the data
/// needed by this vertical slice; inventory, skills, and consequences can grow
/// beside it later without changing the interaction contract.
/// </summary>
public sealed class PlayerState : MonoBehaviour
{
    [field: SerializeField, Min(0)]
    public int Money { get; private set; } = 120;

    [field: SerializeField, Min(1)]
    public int Health { get; private set; } = 100;

    [field: SerializeField]
    public string LocationName { get; private set; } = "Sublevel 01";

    public event Action StateChanged;

    public void SetLocation(string locationName)
    {
        if (string.IsNullOrWhiteSpace(locationName))
            return;

        LocationName = locationName;
        StateChanged?.Invoke();
    }

    /// <summary>Restores a validated snapshot from the local save file.</summary>
    public void ApplyDamage(int amount)
    {
        if (amount <= 0)
            return;
        Health = Mathf.Max(0, Health - amount);
        StateChanged?.Invoke();
    }

    public void Restore(int money, int health, string locationName)
    {
        Money = Mathf.Max(0, money);
        Health = Mathf.Clamp(health, 0, 100);
        if (!string.IsNullOrWhiteSpace(locationName))
            LocationName = locationName;
        StateChanged?.Invoke();
    }

    public void AddMoney(int amount)
    {
        Money = Mathf.Max(0, Money + amount);
        StateChanged?.Invoke();
    }

    public bool TrySpendMoney(int amount)
    {
        if (amount < 0 || Money < amount)
            return false;

        Money -= amount;
        StateChanged?.Invoke();
        return true;
    }
}
