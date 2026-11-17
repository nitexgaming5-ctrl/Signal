# Progress

## Current milestone: vertical slice 0.1 assembled (M4 to M9 foundations written)

## Done and verified in Unity
- [x] M0 Foundation, M1 PlayerStats, M2 grid inventory and ItemDefinition, M3 inventory UI
- Last verified run: 109 of 109 passed

## Written, awaiting the user's test run
- [ ] M4 One room: RoomBootstrap (tilemap room, solid walls, door tile), PlayerMotor2D, CameraFollow2D
- [ ] M5 Save and load: SaveData, SaveMapper, SaveMigrator (Core), SaveFileStore (JSON file)
- [ ] M6 Clock: GameClock (Day, minute of day, Sleep), ticking in GameSession
- [ ] M7 Shop (logic only): ShopOrder and Shop.TryFulfil, plus GridInventory.CountOf and TryRemove
- [ ] M8 Recipes (logic only): Recipe, Crafting.TryCraft with rollback, RecipeDefinition asset
- [ ] M9 Consequences: RentLedger and HeatMeter (state only, nothing reads heat yet)
- [ ] Glue: GameSession (composition root), HudPanel
- Expected total after applying: 189 tests (163 Core, 18 Game, 8 prototype Shop)

## User steps
- [ ] Apply the zip, Assets > Refresh, run the EditMode tests
- [ ] Create the slice scene: File > New Scene (2D template), save as Assets/_Project/Scenes/Slice.unity
- [ ] Add an empty GameObject, add the Game Session component, set Items to 3 and drag in ITEM_A, ITEM_B, ITEM_C
- [ ] Optional: Create > Signal > Recipe Definition (for example 2 of ITEM_A in, 1 of ITEM_B out) and assign it as Debug Recipe
- [ ] Press Play and check: walk around, walls block you, Tab opens the inventory, F6 sleeps, F5 saves, F9 loads

## Not built yet (honest gaps)
- Shop counter and customers in the world (only the rules and an F7 debug sale exist)
- Mixing interaction for chemistry (drag components, heat, contamination, quality)
- Factions, reputation, relationships, quests, story
- Any real art, sound, or animation
- Outings and a second area (M10)

## Next
- Tell me what the first real recipe, customer, and price should be, then I build the shop counter and mixing interaction around them
- Decide what heat and missed rent cause

## Notes
- The old prototype lives in Assets/_Project/_Prototype/ (read only).
- All decisions from the chat are in docs/OPEN_QUESTIONS.md (Q1 to Q19).
