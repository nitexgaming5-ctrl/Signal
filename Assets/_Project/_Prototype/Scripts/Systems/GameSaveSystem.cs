using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Versioned local save for the first playable loop. F5 saves and F9 loads.
/// Kept beside the project/build so user data remains on Signal's project drive.
/// </summary>
public sealed class GameSaveSystem : MonoBehaviour
{
    [Serializable]
    private sealed class SaveData
    {
        public int version = 3;
        public int day = 1;
        public int money;
        public int health;
        public string locationName;
        public List<InventorySystem.Stack> inventory = new List<InventorySystem.Stack>();
        public int shelterIndex;
        public int unpaidRent;
        public int unpaidRacket;
        public bool isInside;
        public Vector3 playerPosition;
        public int lastScavengeDay = -1;
        public List<FactionStandingSystem.Standing> factionStandings = new List<FactionStandingSystem.Standing>();
        public bool outingActive;
        public int outingExposure;
        public int outingStartDay;
        public List<InventorySystem.Stack> carriedLoot = new List<InventorySystem.Stack>();
        public int maraTrust;
        public bool maraRequestAccepted;
        public bool maraRequestComplete;
    }

    public event Action<string> Feedback;
    public string SavePath => Path.GetFullPath(Path.Combine(Application.dataPath, "..", "SignalSave", "signal-save.json"));

    private void Update()
    {
        if (Keyboard.current == null)
            return;

        if (Keyboard.current.f5Key.wasPressedThisFrame)
            SaveNow();
        else if (Keyboard.current.f9Key.wasPressedThisFrame)
            LoadNow();
    }

    public bool SaveNow()
    {
        var game = GameManager.Instance;
        var player = FindAnyObjectByType<PlayerState>();
        var inventory = FindAnyObjectByType<InventorySystem>();
        if (game == null || player == null || inventory == null)
        {
            Notify("Save failed: the run state is incomplete.");
            return false;
        }

        var shelter = FindAnyObjectByType<ShelterManager>();
        var pressure = FindAnyObjectByType<NightPressureSystem>();
        var relations = FindAnyObjectByType<FactionStandingSystem>();
        var cache = FindAnyObjectByType<StreetCacheInteractable>(FindObjectsInactive.Include);
        var outing = FindAnyObjectByType<DistrictOutingSystem>();
        var mara = FindAnyObjectByType<MaraVennInteractable>(FindObjectsInactive.Include);
        var space = FindAnyObjectByType<ShelterSpaceController>();
        var playerObject = player.gameObject;
        var data = new SaveData
        {
            version = 3,
            day = game.CurrentDay,
            money = player.Money,
            health = player.Health,
            locationName = player.LocationName,
            shelterIndex = shelter == null ? 0 : shelter.CurrentIndex,
            unpaidRent = pressure == null ? 0 : pressure.UnpaidRent,
            unpaidRacket = pressure == null ? 0 : pressure.UnpaidRacket,
            isInside = space != null && space.IsInside,
            playerPosition = playerObject.transform.position,
            lastScavengeDay = cache == null ? -1 : cache.LastScavengeDay,
            outingActive = outing != null && outing.IsOuting,
            outingExposure = outing == null ? 0 : outing.Exposure,
            outingStartDay = outing == null ? 0 : outing.StartedOnDay,
            maraTrust = mara == null ? 0 : mara.Trust,
            maraRequestAccepted = mara != null && mara.RequestAccepted,
            maraRequestComplete = mara != null && mara.RequestComplete
        };

        if (relations != null)
        {
            foreach (var standing in relations.Standings)
                data.factionStandings.Add(new FactionStandingSystem.Standing(standing.factionId, standing.value));
        }

        foreach (var stack in inventory.Items)
            data.inventory.Add(new InventorySystem.Stack(stack.itemId, stack.quantity));

        if (outing != null)
        {
            foreach (var stack in outing.CarriedLoot)
                data.carriedLoot.Add(new InventorySystem.Stack(stack.itemId, stack.quantity));
        }

        try
        {
            var directory = Path.GetDirectoryName(SavePath);
            if (string.IsNullOrEmpty(directory))
                throw new InvalidOperationException("Save directory could not be resolved.");
            Directory.CreateDirectory(directory);
            File.WriteAllText(SavePath, JsonUtility.ToJson(data, true));
            Notify("Run saved. Even the rent collector respects a backup.");
            Debug.Log("Signal save written: " + SavePath);
            return true;
        }
        catch (Exception exception)
        {
            Notify("Save failed: " + exception.Message);
            Debug.LogError("Signal save failed: " + exception);
            return false;
        }
    }

    public bool LoadNow()
    {
        if (!File.Exists(SavePath))
        {
            Notify("No save found. The city has no record of you yet.");
            return false;
        }

        try
        {
            var data = JsonUtility.FromJson<SaveData>(File.ReadAllText(SavePath));
            if (data == null || data.version < 1 || data.version > 3)
            {
                Notify("Save is invalid or from an unsupported version.");
                return false;
            }

            var game = GameManager.Instance;
            var player = FindAnyObjectByType<PlayerState>();
            var inventory = FindAnyObjectByType<InventorySystem>();
            if (game == null || player == null || inventory == null)
            {
                Notify("Load failed: the run state is incomplete.");
                return false;
            }

            var shelter = FindAnyObjectByType<ShelterManager>();
            var relations = FindAnyObjectByType<FactionStandingSystem>();
            var cache = FindAnyObjectByType<StreetCacheInteractable>(FindObjectsInactive.Include);
            var outing = FindAnyObjectByType<DistrictOutingSystem>();
            var mara = FindAnyObjectByType<MaraVennInteractable>(FindObjectsInactive.Include);
            if (shelter != null)
                shelter.RestoreIndex(data.shelterIndex);

            var space = FindAnyObjectByType<ShelterSpaceController>();
            if (space != null)
                space.RestoreSpace(data.isInside);

            inventory.RestoreItems(data.inventory);
            player.Restore(data.money, data.health, data.locationName);
            player.transform.position = data.playerPosition;
            var body = player.GetComponent<Rigidbody2D>();
            if (body != null)
                body.position = new Vector2(data.playerPosition.x, data.playerPosition.y);

            var pressure = FindAnyObjectByType<NightPressureSystem>();
            if (pressure != null)
                pressure.RestoreArrears(data.unpaidRent, data.unpaidRacket);
            if (relations != null)
                relations.RestoreStandings(data.factionStandings);
            if (cache != null)
                cache.RestoreUsedDay(data.lastScavengeDay);
            if (outing != null)
                outing.RestoreRun(data.version >= 2 && data.outingActive,
                    data.outingExposure, data.outingStartDay, data.carriedLoot);
            if (mara != null)
                mara.RestoreState(data.version >= 3 ? data.maraTrust : 0,
                    data.version >= 3 && data.maraRequestAccepted,
                    data.version >= 3 && data.maraRequestComplete);

            game.RestoreDay(data.day);
            Notify("Run loaded. The city has not improved in your absence.");
            Debug.Log("Signal save loaded: Day " + game.CurrentDay);
            return true;
        }
        catch (Exception exception)
        {
            Notify("Load failed: " + exception.Message);
            Debug.LogError("Signal load failed: " + exception);
            return false;
        }
    }

    private void Notify(string message)
    {
        Feedback?.Invoke(message);
    }
}
