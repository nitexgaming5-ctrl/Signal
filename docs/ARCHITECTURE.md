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

## UI conventions (M3 onward)
- UGUI only. M3 panels are built from code at runtime (no prefab wiring), so a scene needs just one GameObject.
- Input is read by polling the Input System (`Keyboard.current`, `Mouse.current`). No EventSystem is needed until the UI has buttons.
- Every rule (can this item go there?) lives in Core. The UI asks `GridInventory`, then draws the answer.
- Text uses UGUI `Text` with Unity's built-in font until the pixel font pass (see ART_TODO.md).

## Communication between systems
- Systems expose C# events (`event Action<int> MoneyChanged`). Other systems subscribe. Nothing reaches into another system's private fields.
- UI reads state and subscribes to events. UI never changes state directly; it calls a method on a Core object.
- Save/load goes through DTOs, not runtime classes.

## Coding conventions
- Namespaces: `Signal.Core.*`, `Signal.Game.*` matching folders.
- One main public type per file; the file is named after it. Tiny value types or enums that only exist for one class (PlacedItem, Ingredient, SavedItem, result enums) may share its file.
- Serialized fields are private with `[SerializeField]`. No public fields.
- Prefer `readonly` and immutable data for definitions; mutable only for runtime state.
- Comments explain *why*, not *what*. Keep XML summaries to one line.
- No `Update()` for logic that can be event-driven.

## When in doubt
Put logic in Core, tested. Put presentation in Game. Put numbers and names in Content assets. If it needs a scene, it is Game.

## Placeholder world (M4)
- `RoomBootstrap` builds the room, the player and the camera follow from code. Tiles are flat-colour 32×32 sprites made at runtime (`PixelSprites`).
- Swap in real Tilemap assets and sprites in the art pass; the Core rules do not change.

## Composition (slice)
- `GameSession` is the only place that wires Core objects (stats, inventory, clock, rent, heat) to Game objects (room, panels, save file).
- Core objects never know about Unity. Game objects only call Core methods and draw the result.

