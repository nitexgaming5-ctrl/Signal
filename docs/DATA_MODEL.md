# Data model

## Principle
Definitions (what an item *is*) are ScriptableObjects in `Content/`. Runtime state (what the player *has*) is plain C# in `Signal.Core`. Gameplay code refers to definitions by reference, never by string id.

## Definitions (ScriptableObjects, Content/)
- `ItemDefinition`: stable id, `displayName`, `description`, `category`, `baseValue`, `gridWidth`, `gridHeight`, `maxStack`, optional `icon`, and default item-instance state for starting/test grants.
- `RecipeDefinition` (M8+): `inputs` (ItemDefinition + count list), `output`, `steps`
- Factions, customers, recipes-with-quality: added in their own milestones, each needing an OPEN_QUESTIONS answer first

Ids: each ScriptableObject gets a `Guid` field generated once and never changed, for saves. Do not generate ids at runtime.

## Runtime state (Signal.Core)
- `PlayerStats`: `Money` (int, ≥ 0), `Health` (int, 0–100). Nothing else until listed in OPEN_QUESTIONS.
- `GridInventory`: width, height, list of `PlacedItem { int InstanceId, Guid ItemInstanceId, ItemType Type, int x, int y, bool rotated, int count, ItemInstanceState State }`.
- `ItemType` is an immutable Core value resolved from the authored `ItemDefinition` (definition Guid, footprint and max stack).
- `ItemInstanceId` identifies a particular stack and persists across movement, snapshots, container transfers and save/load. `InstanceId` is only a local integer used by the current inventory UI/placement operations. `TryUpdateItemState` changes the authoritative state without replacing the physical stack identity.
- Transfers move complete stacks without merging separate stack identities. `MoveStack` targets one identity; `MoveAll` is the bulk Core operation. A stack that does not fit remains in the source. No split-stack operation exists yet; a future split must explicitly assign a new identity to the resulting stack.
- `GridInventory.Add(type, count, state)` tops up only stacks with the same definition and equal `ItemInstanceState`; distinct quality/condition/authenticity/ownership states remain separate stacks. `CountOf` and `TryRemove` also have state-specific overloads. The legacy overloads use the neutral default state or all matching definition stacks, respectively.
- `Clock` (M6): `Day`, `TimeOfDay`

## Save format
- Save DTOs are separate classes with `[Serializable]`, plain fields, and a `version` int.
- Save version 5 stores definition Guid, stable stack instance Guid, quantity, grid position, rotation, four per-stack state enums and the player's item-knowledge facts.
- Carried and home-storage inventories are saved separately. Player item knowledge is saved independently of current inventory contents, so a discovery can remain after the physical item is transferred or removed.
- v1 migration preserves the old Hideout/default-empty-storage behavior; v1/v2 migration assigns unique stack identities across both containers. v1/v2/v3 saves receive neutral default item state; v1-v4 saves receive an empty player-knowledge book. Empty or duplicate saved identities are repaired.
- Unknown Guid on load: skip the item, log a warning, do not crash.
- File location: `SignalSave/` in project root (already gitignored).

## Migration
When a save shape changes, bump `version` and write a migration function in Core with a test.


## Item state, player knowledge and inspection
- `ItemInstanceState` stores authoritative quality (Poor/Standard/Fine/Exceptional), condition (Damaged/Worn/Good/Pristine), authenticity (Genuine/Counterfeit) and ownership (Legitimate/Stolen) per stack.
- `ItemKnowledgeFact` stores one player belief about one property of one stable item stack: item identity, aspect, believed enum value, source, confidence and learned in-game time. Sources are DirectInspection, Conversation, Rumour and Document.
- `ItemKnowledgeBook` is player-scoped and separate from both authoritative item state and NPC personal knowledge. A belief may be uncertain or incorrect; it must never be silently reconciled against hidden truth. New evidence replaces the latest belief for that aspect only.
- Knowledge is partial: unknown aspects remain unknown. A full known state can be reconstructed only when all four facts are present.
- The item viewer shows only known facts, with source/confidence. It withholds state-based reference valuation until all four properties are known, and then calculates from the player's known values rather than authoritative hidden state.
- No player-facing inspect action, appraisal rule, skill, contradiction-resolution or automatic NPC-to-player knowledge transfer is implemented yet. Authored interactions must explicitly record facts through the book.
- `ItemValueCalculator` applies provisional multipliers to an authored base reference value. This remains an indicative item reference value only, not a buy/sell price or functioning economy.
- Inventory and locker viewers show one stack at a time. Right-click in the carried grid or INSPECT in home storage opens the same read-only item viewer.
- Home storage lists physical stacks separately and transfers one identified stack at a time, preserving identity and state. Knowledge follows the stable identity, not the inventory/container.

## NPC simulation (first prototype)
- `NpcUtilityBrain` is transient Core runtime state: current activity, energy and social need. It advances from `GameClock` and deterministically reconstructs from the current day/time after a rewind or load.
- The first activity set is `Home`, `Work`, `Rest` and `Socialize`. Decisions are evaluated every 15 in-game minutes; need drift is applied once per elapsed in-game minute.
- Character identity, visual presentation, movement targets, interaction range and authored activity-specific dialogue live in the Game layer for this first prototype.
- No persistent relationship, memory, secret-discovery or NPC inventory data exists yet. Add save DTOs and a versioned migration only when a connected scenario requires such state.
