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

The home locker is a second `GridInventory` owned by `GameSession`. `StoragePanel` lists individual stacks from both containers, exposes inspection and moves a selected whole stack with Core `InventoryTransfer.MoveStack`. The bulk `MoveAll` API remains available for system-level use. Transfers preserve stable identity, state and orientation; distinct stacks are never merged during transfer, and a stack that does not fit remains in the source. Transfer rollback uses inventory snapshots. HideoutSlice currently references ITEM_A/B/C as temporary test fixtures from the existing inventory demo; these are not final content.

Each `PlacedItem` has a persistent `Guid ItemInstanceId` identifying its stack, separate from the inventory-local integer `InstanceId` used by the UI. `ItemInstanceState` stores quality, condition, authenticity and ownership status per stack. `GridInventory.Add` merges only stacks whose definition and state match. Save format version 4 persists these state fields; v1/v2/v3 migration preserves existing identities where present and defaults new state to Standard/Good/Genuine/Legitimate. `CityBlockBootstrap.TryRestorePlace` restores the saved active world root and camera bounds. The user confirmed **235/235 EditMode tests passed** after Item Instance v1; the new state/viewer implementation and tests are awaiting Unity verification.

## Item instance state and inspection

- Core owns `ItemInstanceState` and `ItemValueCalculator`; Unity does not calculate item values itself.
- A stack currently carries quality, condition, authenticity and ownership status. These are the item's actual facts, not yet the player's discovered knowledge. A future inspection/appraisal skill or uncertainty model must be a separate layer so the UI cannot accidentally expose secrets.
- `ItemDefinition` authors the item's descriptive metadata, icon, base reference value and default state. Runtime stacks can differ from those defaults.
- The viewer is a read-only presentation of the selected physical stack. Right-clicking an item in the carried grid or choosing INSPECT in home storage opens it; both paths use the same viewer.
- The value calculation is provisional reference valuation only. It is not a transaction price, resale offer, market simulation or economy rule.
- Save format v4 persists all four state enums. Existing v1/v2/v3 saves migrate to the neutral default state.

## Connected NPC simulation

NPC runtime belongs to the shared city simulation, not to isolated character-specific scripts.

- Core owns the deterministic social-world model: NPC registration and presence, local perception, world-event records, individual knowledge, directed relationships and explicit knowledge exchange. NpcPerception builds a character-scoped snapshot for decision systems.
- Game owns Unity presentation, spatial movement, scene/location integration and publishes player-facing/world events into the shared Core context.
- An NPC's knowledge is not the same as the world event journal. Decision-making must consume the individual agent's NpcPerception snapshot (nearby people plus its own knowledge), never the global journal as an omniscient shortcut.
- World events must contain only information that could be perceived by their witnesses. Hidden motives, identities and causes must not leak through event descriptions.
- The current Milo component is a temporary integration fixture. It is not the production character-authoring pattern. Move identity, appearance, routine preferences and dialogue into data-driven definitions before bulk NPC creation.
- Social knowledge and relationships are not yet saved. Add stable-ID persistence and explicit save migration only when the framework's long-lived state is ready.

The implementation contract and phased delivery plan are documented in [NPC_FRAMEWORK.md](NPC_FRAMEWORK.md). Keep the simulation deterministic and event-driven; do not add per-frame full-population reasoning, external AI services or separate legal/criminal social worlds.


## Player discovery and contextual interactions

- PlayerDiscoveryLedger is player-scoped and separate from ItemKnowledgeBook, NPC NpcKnowledge and authoritative world state.
- PlayerDiscoverySignal records one learned signal with stable occurrence ID, subject ID, signal type (routine observation, conversation, physical evidence or rumour), source ID, observable/communicated description, confidence and in-game learned time. The ledger is a provenance-preserving record, not a truth evaluator or conclusion generator.
- Re-recording the same stable signal ID is idempotent. A new occurrence requires a new ID; repeating one observation cannot inflate the number of distinct signal types.
- InteractionConditionSet evaluates current NPC activity, the player's directed relationship impression and required discovery signal IDs/types. Conditions are conjunctive and evaluated against an explicit InteractionContext; no global lookups or character-specific branches are introduced.
- The first implementation does not infer what the player believes about Milo. Authored opportunity logic will evaluate combinations and define conclusions separately. Item facts continue to use ItemKnowledgeBook; NPC facts remain character-scoped in NpcKnowledge.
- Opportunity state, save integration, world interactions and delivery outcome resolution are subsequent milestones. Keep Core plain C# and Game responsible for scene objects, input and presentation.


## Authored opportunity lifecycle
- OpportunityDefinition is an immutable Core definition keyed by stable opportunity ID and associated NPC ID; its offer gate reuses InteractionConditionSet.
- OpportunityState is the small runtime lifecycle: Dormant, Available, Accepted and Resolved. Decline is recorded without failing the opportunity. Returning the parcel moves Accepted back to Available. Delivery resolves once. Expiry returns an available opportunity to Dormant and does not queue a future release.
- The state model stores the parcel's stable item-stack identity rather than a separate quest-item copy. The Game layer must move the actual item through existing inventory/world interaction services.
- This is not a generic quest system. It has no journal, objective tree, reward catalogue or autonomous backlog. Save integration and GameSession ownership are pending.
