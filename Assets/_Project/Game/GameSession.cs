using System;
using System.Collections.Generic;
using Signal.Core;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Signal.Game
{
    /// <summary>
    /// Composition root for the vertical slice. One GameObject with this component gives you:
    /// a walled room with a walking player, the grid inventory (Tab), a clock, rent and heat,
    /// and save/load. Keys: F6 sleep, F5 save, F9 load, F7 debug sell, F8 debug craft.
    /// Function keys are used because letter keys sit in different places on AZERTY keyboards.
    /// </summary>
    public sealed class GameSession : MonoBehaviour
    {
        [Header("Content")]
        [SerializeField] private ItemDefinition[] items;
        [SerializeField] private RecipeDefinition debugRecipe;

        [Header("Placeholder tuning (not final design)")]
        [SerializeField] private int startingMoney;
        [SerializeField] private int rentAmount = 50;
        [SerializeField] private int rentEveryDays = 7;
        [SerializeField] private float gameMinutesPerRealSecond = 1f;
        [SerializeField] private int debugSalePrice = 10;

        private readonly Dictionary<Guid, ItemType> _catalog = new Dictionary<Guid, ItemType>();

        private PlayerStats _stats;
        private GridInventory _inventory;
        private GameClock _clock;
        private HeatMeter _heat;
        private RentLedger _rent;

        private RoomBootstrap _room;
        private PlayerMotor2D _motor;
        private InventoryPanel _panel;
        private HudPanel _hud;

        private float _minuteBuffer;

        private void Start()
        {
            _stats = new PlayerStats(startingMoney);
            _inventory = new GridInventory();
            _clock = new GameClock();
            _heat = new HeatMeter();
            _rent = new RentLedger(rentAmount, rentEveryDays);
            _clock.DayStarted += OnDayStarted;

            BuildCatalog();
            GiveStartingItems();

            Camera camera = Camera.main;
            if (camera == null)
                camera = CreateCamera();

            var roomObject = new GameObject("Room");
            roomObject.transform.SetParent(transform, false);
            _room = roomObject.AddComponent<RoomBootstrap>();
            _room.Build(camera);
            _motor = _room.Player.GetComponent<PlayerMotor2D>();

            _panel = gameObject.AddComponent<InventoryPanel>();
            _panel.Bind(_inventory, items);

            _hud = gameObject.AddComponent<HudPanel>();
            _hud.Bind(_stats, _clock, _heat, _rent);
            _hud.ShowMessage("Walk with WASD. Tab opens the inventory.", 5f);
        }

        private void OnDestroy()
        {
            if (_clock != null)
                _clock.DayStarted -= OnDayStarted;
        }

        private void Update()
        {
            if (_stats == null)
                return;

            // The player stands still while the inventory is open.
            _motor.enabled = !_panel.IsOpen;

            TickClock();

            Keyboard keyboard = Keyboard.current;
            if (keyboard == null)
                return;

            if (keyboard.f6Key.wasPressedThisFrame) Sleep();
            if (keyboard.f5Key.wasPressedThisFrame) SaveGame();
            if (keyboard.f9Key.wasPressedThisFrame) LoadGame();
            if (keyboard.f7Key.wasPressedThisFrame) DebugSell();
            if (keyboard.f8Key.wasPressedThisFrame) DebugCraft();
        }

        private void TickClock()
        {
            _minuteBuffer += Time.deltaTime * gameMinutesPerRealSecond;

            int whole = (int)_minuteBuffer;
            if (whole > 0)
            {
                _minuteBuffer -= whole;
                _clock.Advance(whole);
            }
        }

        private void OnDayStarted(GameClock clock)
        {
            _heat.NewDay();

            string message;
            switch (_rent.Settle(clock.Day, _stats))
            {
                case RentOutcome.Paid:
                    message = "Day " + clock.Day + ": rent paid (" + _rent.Amount + ")";
                    break;
                case RentOutcome.Missed:
                    message = "Day " + clock.Day + ": rent MISSED (" + _rent.MissedPayments + " so far)";
                    break;
                default:
                    message = "Day " + clock.Day + " begins";
                    break;
            }

            Say(message);
        }

        private void Sleep()
        {
            int dayBefore = _clock.Day;
            _clock.Sleep();

            if (_clock.Day == dayBefore)
                Say("You slept. It is " + _clock.Label);
        }

        private void SaveGame()
        {
            Vector2 position = _room.PlayerBody.position;
            SaveData data = SaveMapper.Capture(_stats, _inventory, _clock, _heat, _rent, position.x, position.y);

            try
            {
                SaveFileStore.Save(data, SaveFileStore.DefaultPath);
                Say("Saved.");
            }
            catch (Exception exception)
            {
                Debug.LogError("Save failed: " + exception);
                Say("Save failed: " + exception.Message);
            }
        }

        private void LoadGame()
        {
            if (!SaveFileStore.TryLoad(SaveFileStore.DefaultPath, out SaveData data, out string error))
            {
                Say(error);
                return;
            }

            LoadReport report = SaveMapper.Apply(data, _stats, _inventory, _clock, _heat, _rent, ResolveType);
            if (!report.Success)
            {
                Say("Load failed: " + report.Error);
                return;
            }

            var position = new Vector2(data.playerX, data.playerY);
            _room.Player.position = position;
            _room.PlayerBody.position = position;
            _room.PlayerBody.linearVelocity = Vector2.zero;

            Say(report.SkippedItems > 0
                ? "Loaded. " + report.SkippedItems + " item(s) could not be restored."
                : "Loaded.");
        }

        private void DebugSell()
        {
            if (items == null || items.Length == 0 || items[0] == null)
            {
                Say("Assign at least one item to sell.");
                return;
            }

            ItemType type = items[0].ToItemType();
            FulfilResult result = Shop.TryFulfil(new ShopOrder(type, 1, debugSalePrice), _inventory, _stats);

            switch (result)
            {
                case FulfilResult.Done:
                    Say("Sold 1 " + items[0].DisplayName + " for " + debugSalePrice + ".");
                    break;
                case FulfilResult.NotEnoughStock:
                    Say("You have none of " + items[0].DisplayName + " to sell.");
                    break;
                default:
                    Say("That sale is not valid.");
                    break;
            }
        }

        private void DebugCraft()
        {
            if (debugRecipe == null)
            {
                Say("No debug recipe assigned.");
                return;
            }

            try
            {
                switch (Crafting.TryCraft(debugRecipe.ToRecipe(), _inventory))
                {
                    case CraftResult.Done:
                        Say("Crafted using '" + debugRecipe.name + "'.");
                        break;
                    case CraftResult.MissingInputs:
                        Say("Missing ingredients.");
                        break;
                    default:
                        Say("No room in the inventory for the result.");
                        break;
                }
            }
            catch (Exception exception)
            {
                Say(exception.Message);
            }
        }

        private void BuildCatalog()
        {
            _catalog.Clear();
            if (items == null)
                return;

            foreach (ItemDefinition definition in items)
            {
                if (definition == null)
                    continue;

                ItemType type = definition.ToItemType();
                _catalog[type.Id] = type;
            }
        }

        private void GiveStartingItems()
        {
            if (items == null)
                return;

            foreach (ItemDefinition definition in items)
            {
                if (definition == null)
                    continue;

                ItemType type = definition.ToItemType();
                _inventory.Add(type, type.MaxStack);
            }
        }

        private ItemType? ResolveType(Guid id)
        {
            return _catalog.TryGetValue(id, out ItemType type) ? type : (ItemType?)null;
        }

        private void Say(string text)
        {
            if (_hud != null)
                _hud.ShowMessage(text);
        }

        private static Camera CreateCamera()
        {
            var cameraObject = new GameObject("Main Camera");
            cameraObject.tag = "MainCamera";
            cameraObject.transform.position = new Vector3(0f, 0f, -10f);

            var camera = cameraObject.AddComponent<Camera>();
            camera.orthographic = true;
            return camera;
        }
    }
}
