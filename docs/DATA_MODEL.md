# Data model

## Principle
Definitions (what an item *is*) are ScriptableObjects in `Content/`. Runtime state (what the player *has*) is plain C# in `Signal.Core`. Gameplay code refers to definitions by reference, never by string id.

## Definitions (ScriptableObjects, Content/)
- `ItemDefinition`: `displayName`, `gridWidth`, `gridHeight`, `maxStack`, `icon` (optional, placeholder ok)
- `RecipeDefinition` (M8+): `inputs` (ItemDefinition + count list), `output`, `steps`
- Factions, customers, recipes-with-quality: added in their own milestones, each needing an OPEN_QUESTIONS answer first

Ids: each ScriptableObject gets a `Guid` field generated once and never changed, for saves. Do not generate ids at runtime.

## Runtime state (Signal.Core)
- `PlayerStats`: `Money` (int, ≥ 0), `Health` (int, 0–100). Nothing else until listed in OPEN_QUESTIONS.
- `GridInventory`: width, height, list of `PlacedItem { int InstanceId, Guid ItemInstanceId, ItemType Type, int x, int y, bool rotated, int count }`.
- `ItemType` is an immutable Core value resolved from the authored `ItemDefinition` (definition Guid, footprint and max stack).
- `ItemInstanceId` identifies a particular stack and persists across movement, snapshots, container transfers and save/load. `InstanceId` is only a local integer used by the current inventory UI/placement operations.
- V1 transfers move complete stacks without merging separate stack identities. A stack that does not fit remains in the source. No split-stack operation exists yet; a future split must explicitly assign a new identity to the resulting stack.
- `GridInventory.Add` can top up an existing same-definition stack while there is no mutable per-instance state. Once such state exists, stack compatibility must compare it before merging.
- `Clock` (M6): `Day`, `TimeOfDay`

## Save format
- Save DTOs are separate classes with `[Serializable]`, plain fields, and a `version` int.
- Save version 3 stores both the item definition Guid and stable stack instance Guid, plus quantity, grid position and rotation. Carried and home-storage inventories are saved separately.
- v1 migration preserves the old Hideout/default-empty-storage behavior; v1/v2 migration assigns unique stack identities across both containers. Empty or duplicate saved identities are repaired.
- Unknown Guid on load: skip the item, log a warning, do not crash.
- File location: `SignalSave/` in project root (already gitignored).

## Migration
When a save shape changes, bump `version` and write a migration function in Core with a test.


## NPC simulation (first prototype)
- `NpcUtilityBrain` is transient Core runtime state: current activity, energy and social need. It advances from `GameClock` and deterministically reconstructs from the current day/time after a rewind or load.
- The first activity set is `Home`, `Work`, `Rest` and `Socialize`. Decisions are evaluated every 15 in-game minutes; need drift is applied once per elapsed in-game minute.
- Character identity, visual presentation, movement targets, interaction range and authored activity-specific dialogue live in the Game layer for this first prototype.
- No persistent relationship, memory, secret-discovery or NPC inventory data exists yet. Add save DTOs and a versioned migration only when a connected scenario requires such state.
