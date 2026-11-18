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
- Player-to-NPC relationship impressions are persisted in save v7 for the approved Milo scenario. NPC-to-NPC relationships, NPC memories and transient perception remain runtime-only; add those only when a connected scenario requires them.


## Player discovery signals (Opportunity 001 foundation)
- PlayerDiscoverySignal is one player-learned signal, keyed by a stable authored occurrence ID. It stores subject ID, signal type, source ID, observable/communicated description, confidence and learned day/minute.
- Signal types are RoutineObservation, Conversation, PhysicalEvidence and Rumour. Type and source are retained; rumours remain uncertain records and are not promoted to truth.
- PlayerDiscoveryLedger stores signals independently of item facts and NPC knowledge. It supports subject-scoped queries, required-signal checks and counting distinct signal types. Re-recording an existing signal ID is idempotent.
- InteractionConditionSet is an immutable conjunctive condition bundle for required NPC activity, minimum directed familiarity/trust/affinity and required signal IDs/types. InteractionContext supplies current activity, the player's relationship impression and discovery ledger.
- Save v7 persists the ledger independently of item facts and inventory contents. v1-v6 migrations initialize the new collection empty.


## Opportunity 001 runtime state (first pass)
- OpportunityDefinition contains stable opportunity ID, associated NPC ID and an InteractionConditionSet for its offer.
- OpportunityState contains phase (Dormant, Available, Accepted, Resolved), resolution (None/Delivered), associated parcel stack Guid, acceptance/decline counts and relevant in-game timestamps.
- Declining leaves the opportunity Available. Returning the parcel moves Accepted back to Available. Delivery resolves exactly once. Expiry can return Available to Dormant without scheduling a backlog.
- Save v7 persists opportunity phase, resolution, parcel stack identity, accept/decline counters and timestamps, plus the delivery assessment (on-time, intact and witnessed). Invalid opportunity snapshots are ignored rather than failing the full save.


## Player-to-NPC relationships (save v7)

- PlayerRelationshipBook stores a directed relationship impression from the player toward each stable NPC ID. Each entry contains familiarity (0..1), trust (0..1) and affinity (-1..1).
- Missing entries resolve to the neutral relationship (0 familiarity, 0.5 trust, 0 affinity).
- This book is distinct from NpcSocialWorld relationships, which describe NPC-to-NPC impressions and are not yet persisted.
- Save v7 stores stable NPC ID and all three values; v1-v6 migrations initialize the collection empty.
