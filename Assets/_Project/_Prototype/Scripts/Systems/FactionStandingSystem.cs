using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>Owns a small set of original city factions and explicit standing changes.</summary>
public sealed class FactionStandingSystem : MonoBehaviour
{
    [Serializable]
    public sealed class Standing
    {
        public string factionId;
        [Range(-100, 100)] public int value;

        public Standing(string id, int initialValue)
        {
            factionId = id;
            value = initialValue;
        }
    }

    [SerializeField] private List<Standing> standings = new List<Standing>();
    public IReadOnlyList<Standing> Standings => standings;
    public event Action<string, int> StandingChanged;

    private void Awake()
    {
        EnsureDefaults();
    }

    public int GetStanding(string factionId)
    {
        EnsureDefaults();
        var standing = standings.Find(item => string.Equals(item.factionId, factionId, StringComparison.OrdinalIgnoreCase));
        return standing == null ? 0 : standing.value;
    }

    public void ModifyStanding(string factionId, int delta)
    {
        if (string.IsNullOrWhiteSpace(factionId) || delta == 0)
            return;

        EnsureDefaults();
        var standing = standings.Find(item => string.Equals(item.factionId, factionId, StringComparison.OrdinalIgnoreCase));
        if (standing == null)
        {
            standing = new Standing(factionId, 0);
            standings.Add(standing);
        }
        standing.value = Mathf.Clamp(standing.value + delta, -100, 100);
        StandingChanged?.Invoke(standing.factionId, standing.value);
    }

    public void RestoreStandings(List<Standing> saved)
    {
        standings.Clear();
        if (saved != null)
        {
            foreach (var entry in saved)
            {
                if (entry != null && !string.IsNullOrWhiteSpace(entry.factionId))
                    standings.Add(new Standing(entry.factionId, Mathf.Clamp(entry.value, -100, 100)));
            }
        }
        EnsureDefaults();
        foreach (var entry in standings)
            StandingChanged?.Invoke(entry.factionId, entry.value);
    }

    private void EnsureDefaults()
    {
        if (standings.Count > 0)
            return;
        standings.Add(new Standing("Canal Crew", -5));
        standings.Add(new Standing("Water Co-op", 0));
        standings.Add(new Standing("Block Watch", 0));
    }
}
