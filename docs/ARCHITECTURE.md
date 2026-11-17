# Architecture

## Layers
1. **Signal.Core** (asmdef, no UnityEngine, no scene) — stats, inventory grid, rules, save DTOs, clock. All testable in EditMode.
2. **Signal.Game** (asmdef, references Core) — MonoBehaviours, UI, tilemap, input, glue to Unity.
3. **Content** (ScriptableObject assets) — items, recipes, factions, placed in `Assets/_Project/Content/`. Data only, no logic.

Dependencies go one way: Game → Core. Core never references Game.

## Folder layout
```
Assets/_Project/
  Core/            Signal.Core.asmdef     pure C# logic (no UnityEngine)
  Game/            Signal.Game.asmdef     MonoBehaviours, glue
  UI/              (inside Game) inventory, HUD, panels
  World/           (inside Game) tilemaps, room, camera, player
  Content/         ScriptableObject assets only
    Items/
    Recipes/
  Art/             Aseprite sources, imported sprites, tilesets
  Tests/
    EditMode/      Signal.Core.Tests.asmdef, Signal.Game.Tests.asmdef
  _Prototype/      old prototype code, read-only reference, do not extend
```

Prototype code lives in `_Prototype/`. New work does not build on it. Its string ids and hardcoded values are the reason for this rebuild.

## Assembly definitions
- `Signal.Core`: no references, `noEngineReferences: true`
- `Signal.Core.Tests`: references `Signal.Core`, EditMode only
- `Signal.Game`: references `Signal.Core`, `UnityEngine.UI`, `Unity.InputSystem`
- `Signal.Game.Tests`: references `Signal.Game`, `Signal.Core`, EditMode only

Rule: one `.asmdef` per folder, and each test folder has its own subfolder. (This caused the earlier compile errors.)

## UI conventions
- UGUI panels are built from code at runtime; a scene needs only the composition-root GameObject.
- World input is read through the Input System (`Keyboard.current`, `Mouse.current`). Add an EventSystem with InputSystemUIInputModule only when a UI needs pointer-driven buttons or dragging.
- Every rule (can this item go there?) lives in Core. UI asks Core objects and presents the result.
- Text uses UGUI `Text` with Unity's built-in font until the pixel font pass (see ART_TODO.md).

## Communication between systems
- Systems expose C# events for state changes. UI subscribes and refreshes; it does not duplicate domain state.
- Save/load goes through DTOs, not runtime classes.
- `GameSession` is the composition root that wires Core state to Game presentation and world objects.

## Coding conventions
- Namespaces: `Signal.Core.*`, `Signal.Game.*` matching folders.
- One main public type per file; tiny value types may share their owning file.
- Serialized fields are private with `[SerializeField]`. No public fields.
- Prefer immutable definitions and explicit runtime state.
- Comments explain why, not what.

## Active architecture decision (2026-11-17)

**Signal.Core + Signal.Game are the single active gameplay architecture.** Core owns pure domain state and rules; Game owns Unity runtime, input, scene composition, world objects, UI and persistence adapters.

The old `Assets/_Project/Scenes/Main.unity` and `Assets/_Project/_Prototype/Scripts` are retained as legacy references because the scene contains useful city/shelter art and gameplay experiments. They are not the active build path, and no new gameplay should be added there. The active build scene is `Assets/_Project/Scenes/HideoutSlice.unity`, composed by `Signal.Game.GameSession`.

Why Core/Game: clearer boundaries, reusable domain rules, and focused EditMode coverage. The prototype contains useful ideas (outing/loot risk, shelter switching, factions and NPC interactions), but duplicates state and uses global lookups and tightly coupled scene wiring. Migrate only useful behavior required by a slice; rewrite it against Core/Game rather than bulk-copying it.

Do not run both architectures in the same active scene. Preserve Main and its asset GUIDs until valuable content has been migrated and verified. Remove legacy scripts only in a separate cleanup after their scene references are gone.

## First hideout implementation

The active HideoutSlice is a code-built first-pass room. It contains placeholder bed, table, chair, storage and a physical workbench. The workbench opens a close-up UGUI tabletop with draggable and inspectable props. It remains a visual interaction prototype until recipe assets are authored and connected; no production/economy rules are implied by the tabletop props.

## Connected city and home storage

`CityBlockBootstrap` composes the compact outdoor street and two simple interior spaces under the active `GameSession`. It reuses the same player Rigidbody2D and keeps inventory, clock and other session state alive while changing the active visual root and camera bounds.

The home locker is a second `GridInventory` owned by `GameSession`. `StoragePanel` transfers item quantities between carried inventory and locker inventory through Core's capacity-aware `Add` and `TryRemove` operations. The panel does not invent item values or production rules.

Save format version 2 stores carried inventory, locker inventory, the current place identifier and player coordinates. Version-1 saves migrate to an empty locker and the hideout. `CityBlockBootstrap.TryRestorePlace` restores the saved active world root and camera bounds. The save implementation and UI still require Unity Play Mode verification on the target platform.
