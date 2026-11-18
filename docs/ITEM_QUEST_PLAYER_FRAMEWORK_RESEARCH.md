# Inventory, quests and player systems — research and next-step brief

> Prepared 2026-11-18 after reviewing the active Core/Game implementation, project vision, handoff, roadmap and the reference-game list. This is an architecture and sequencing recommendation, not approval of new gameplay rules.

## Executive recommendation

**Build the item-instance foundation next, not a full quest system or a large player-stat system.**

Signal already has a working spatial grid inventory and a separate home locker. The main limitation is that an item is currently only a definition plus position, rotation and stack count. Signal's intended goods gameplay will eventually need to distinguish two copies of the same product: for example, a clean item from a damaged one, a genuine item from a counterfeit, or an ordinary item from one with a meaningful history. The current model cannot express those differences.

The next technical milestone should therefore be a deliberately narrow **Item Instance v1**:
1. Preserve the existing grid inventory and storage behavior.
2. Separate immutable item definition data from the state of a particular physical item/stack.
3. Give item instances stable identity that survives moving between containers and saving/loading.
4. Ensure stack merging is only possible when the items are genuinely equivalent under the fields that exist.
5. Version and migrate saves without losing existing v2 inventory layouts.

Do not add quality grades, counterfeit rules, stolen flags, durability loss, prices, item use, crafting recipes or an economy in this milestone. First establish a data model that can support those features when their player-facing rules are designed.

## 1. What exists today

### Core inventory

Relevant active files:
- `Assets/_Project/Core/ItemType.cs`
- `Assets/_Project/Core/GridInventory.cs`
- `Assets/_Project/Core/InventoryTransfer.cs`
- `Assets/_Project/Core/SaveData.cs`
- `Assets/_Project/Game/ItemDefinition.cs`
- `Assets/_Project/Game/InventoryPanel.cs`
- `Assets/_Project/Game/StoragePanel.cs`
- `Assets/_Project/Game/GameSession.cs`

Current behavior:
- `ItemDefinition` is a Unity ScriptableObject with a stable generated GUID, display name, grid width/height and maximum stack size.
- Core `ItemType` is an immutable value containing only definition GUID, footprint and maximum stack size.
- `GridInventory` is a spatial grid (default 6×4) with rectangular item footprints, rotation, stack counts, placement/movement/removal, add/remove, and change notifications.
- Each placed stack has an integer instance ID, but it is allocated locally by each inventory. It is not a stable physical-item identity and is not preserved by container transfers.
- Adding items fills existing stacks first, then places new stacks in the first free grid location.
- The hideout locker is a separate 8×6 `GridInventory`.
- `InventoryTransfer.MoveAll` is capacity-aware and uses snapshots to roll back an unexpected partial transfer failure.
- Inventory UI is generated in code. It supports a grid, drag/drop and rotation; current item visuals are placeholder labels/colours rather than a complete authored icon/description presentation.
- `GameSession.GiveStartingItems` currently grants the maximum stack of every assigned item definition. This is fixture/bootstrap behavior, not a final starting-loadout design.

### Save behavior

Save format v2 stores each stack's definition ID, grid position, rotation and count. It also stores the carried inventory, locker inventory and world location/player coordinates. It does **not** store a stable identity or mutable state for each physical item/stack.

### Player state

Core `PlayerStats` currently contains money and health. Heat and rent are separate Core systems. This is a useful minimal runtime foundation, not a designed player-character progression system. No complete player creation, appearance customization, equipment, attributes or skills framework was found in the active Core/Game slice.

### Quest state

The active project contains NPC world events, personal NPC knowledge, relationships and a world pacing permission gate. These are useful building blocks, but they are not a quest system. No active quest definition, quest runtime state, objective tracker or quest journal was found in the reviewed active files.

## 2. Why item instances are the best next foundation

Signal's confirmed direction gives goods an eventual role beyond generic inventory tokens:
- quality and condition;
- authenticity;
- origin/provenance;
- ownership status and risk;
- meaningful inspection, appraisal, use and trade decisions.

The reference games reinforce different parts of this:
- **Probably Stolen** makes inspection item-specific: different goods call for different checks, and the player can make a loss by misjudging quality, authenticity or value. Its appraisal work is also a service distinct from buying the item. This is a strong reference for *goods having discoverable properties*, not a request to copy its shop loop.
- **Schedule I** and **Drug Dealer Simulator** show why goods must eventually move through suppliers, storage, preparation, customers and distribution. Signal should learn from the physical handling and connected chain, not copy their drug-business progression or automation scale.
- **S.T.A.L.K.E.R. / Escape from Tarkov** are references for scarcity, loot pressure and meaningful choices about what is worth carrying or risking. The existing spatial grid already supports part of that inventory tension.
- **Torn Crimes 2.0** demonstrates varied crime activities, prerequisites, time/context modifiers, special outcomes and consequences. However, its visible per-crime skill levels are specifically not Signal's chosen progression direction: the user confirmed one global criminal progression track across criminal activities.

A future item-instance model can support these ideas without committing to any of their detailed mechanics.

## 3. Proposed Item Instance v1 — narrow technical scope

### Data separation

- **Item definition (authored, immutable at runtime):** stable definition ID, display name and grid footprint/stack constraints already exist. Optional presentation metadata such as icon/description can be added when the UI pass needs it.
- **Item instance/stack (runtime state):** stable unique identity plus a reference/ID for its definition and quantity. It is a particular owned stack, not the shared definition asset.
- **Grid placement:** coordinates and rotation belong to the inventory/container placement layer, not to the item's identity.
- **Container:** carried inventory, home locker and future containers remain instances of the same Core container abstraction.

### Identity and transfer

- Stable item-instance IDs must survive moving a stack from player inventory to locker and back.
- A split operation, if/when introduced, must define whether it creates a new identity or preserves identity for one resulting stack; do not silently choose semantics before that operation is designed.
- A merge must not erase distinct mutable state. For v1, only stacks with identical definition and identical instance-state payload may merge.
- The inventory's local placement/slot bookkeeping must not be mistaken for a persistent item identity.

### Persistence

- Add a new save version only when the new representation is implemented.
- Migrate v2 stack records into v3 item instances, generating stable IDs for the migrated stacks while preserving definition, count, position and rotation.
- Add tests for v2 migration, round-trip identity preservation, transfer identity preservation, placement failure/no-loss and stack compatibility.
- Unknown item definitions should continue to be reported/skipped safely rather than corrupting the whole save.

### Explicit exclusions from v1

No gameplay values or rules for:
- quality grades, condition/durability, authenticity/counterfeit detection;
- stolen/ownership flags, provenance chains or criminal evidence;
- buy/sell price, customer budgets, supply/demand or bargaining;
- item use, equipment slots, encumbrance/weight, containers inside containers;
- crafting/recipes, workbench production or item transformation;
- quest items, quest objective counters or reward distribution.

Those are future consumers of the item model and need their own design decisions.

## 4. Quest framework — what to build later, and how to avoid a generic quest checklist

A quest framework will eventually be useful, but building it before the player can meaningfully interact with people, goods and places risks creating a generic checklist disconnected from Signal's strengths.

Recommended future architecture:

### Authored definition
A data-driven quest/opportunity definition should describe:
- stable ID and presentation text;
- how it becomes discoverable (which is not the same as being active);
- entry conditions and optional prerequisites;
- a small set of objective definitions;
- authored transitions/outcomes;
- optional failure/expiry rules only where the scenario needs them.

### Runtime state
Keep player-specific progress in Core runtime state, separate from ScriptableObject definitions:
- undiscovered / discovered / active / completed / failed (only states needed by actual content);
- objective progress;
- relevant NPC/character IDs and world-place IDs;
- selected outcome flags needed by the scenario.

### Event-driven objectives
Objectives should react to semantic gameplay events from authoritative systems, not poll UI text or inspect arbitrary inventory snapshots every frame. Examples of future event families:
- item acquired, transferred, inspected, used, sold or delivered;
- conversation or relationship choice;
- NPC observed at a location or a fact learned;
- entering a place, interacting with an object or completing an activity.

An objective must count the *meaningful action*, not every inventory mutation. Moving a stack between the backpack and locker must not accidentally satisfy “obtain an item”; sorting or merging must not count as newly acquired goods.

### Narrative fit
- A quest is not necessarily a glowing marker or a formal mission board entry.
- An opportunity may begin as a rumour, a visible anomaly, a favour or a conversation.
- Discovery, acceptance and completion should be distinct where the story needs those distinctions.
- NPC personal knowledge remains character-scoped; the quest system must not expose facts the player has not discovered.
- Major narrative beats remain authored. The WorldPacingDirector is a shared permission gate for consequential world developments, not a quest generator or backlog scheduler.

### Recommended timing
Do not implement a broad quest framework yet. First establish item identity and a small set of player-facing interactions. Then implement a tiny objective/event contract alongside the first real scenario, proving it with one short opportunity from discovery through outcome. Generalize only what the scenario actually uses.

## 5. Player creation, stats and customization

### Current state
The active player stats model is intentionally tiny: money and health, with heat and rent as separate systems. There is no complete character creation or customization system in the active slice.

### Recommended order
1. **Cosmetic identity:** name/appearance choices can be added independently once the player sprite/animation pipeline is ready. They should not determine a legal or criminal class.
2. **Survival/condition stats:** add only values required by actual activities and consequences. Avoid adding hunger, thirst, stamina, stress, hygiene, sleep debt and injury simultaneously without a proven loop.
3. **Skills/progression:** derive the required progression from repeated player activities and the intended global crime track. Do not copy Stardew's five skills or Torn's per-crime skill bars by default.
4. **Equipment:** only when clothing/tools/equipment have clear use cases and the item model supports per-instance state.
5. **Character creation:** keep it lightweight initially; do not block the player behind a large RPG attribute/class questionnaire.

Stardew Valley is a useful reference for readable progression, relationships, crafting, collection and visible home customization. Its farming, seasonal crops, five skill tracks, marriage/family, museum and community-centre structure are examples of a complete game with a different core fantasy, not a feature checklist for Signal.

## 6. Reference-game feature map

| Reference | Useful ideas to study for Signal | Do not copy by default |
|---|---|---|
| Schedule I | Compact city and customer/contact network; suppliers; production/distribution chains; physical transport; properties and base expansion; pressure from law enforcement and rivals | Drug-only economy, automated empire scale, exact map/progression |
| Drug Dealer Simulator | Physical hideout, shipment handling, storage, preparation/portioning, different customer scales, police patrol/ambush pressure, hideout/lab upgrades and staff | Exact drug recipes, territory/crew structure, forced criminal-only progression |
| Probably Stolen | Item-specific inspection, authenticity and quality checks, appraisal, bargaining, customer patience, reputation/service business, risk of buying misjudged goods | Shop-only core loop or copying its precise checklists/economy |
| Torn Crimes 2.0 | Broad activity categories, sub-activities, prerequisites/unlocks, context/time modifiers, success/failure/critical-failure distinction, rare outcomes and cross-activity consequences | Separate permanent skill tree for every crime; Signal's approved crime progression is global |
| Stardew Valley | Clear top-down readability; daily routines; relationships; authored character events; collection/crafting; home decoration/upgrades; gentle long-term progression | Mandatory farming, seasonal calendar, marriage/family, five skill trees or its exact art style |
| S.T.A.L.K.E.R. / Tarkov | Scarcity, meaningful loot decisions, risk vs value, limited carrying capacity, preparation before dangerous outings, equipment trade-offs | Shooter-first systems, extraction gameplay or realism for its own sake |

## 7. Recommended implementation sequence

### Item Instance v1 — first implementation committed; Unity verification pending

The user approved this as the next technical milestone on 2026-11-18. The first pass is now implemented on the shared branch:
- `PlacedItem` has a stable GUID stack identity separate from its inventory-local integer UI ID.
- Moving/rotating and snapshot/restore preserve identity.
- Container transfer moves whole stacks, preserves identity/orientation and does not merge distinct stacks. If a complete stack does not fit, it remains in the source. No split-stack operation exists yet.
- Save v3 captures/restores identity; v1/v2 migration generates unique IDs across carried and storage inventories and repairs duplicate/empty IDs.
- Core tests cover identity, transfers, orientation, snapshot restore, v1/v2 migration and save round-trip.
- Existing UI is retained; the storage transfer hint now explains whole-stack behavior.
- Unity compilation and the full EditMode suite remain pending user verification.

`GridInventory.Add` still tops up a same-definition stack because no mutable per-instance state exists yet. When such state is introduced, stack compatibility must compare it before merging.

### Future follow-up after Unity verification
- Review any Unity test failures and correct them before proceeding.
- Verify an actual v2 save and an actual v1 save migrate in the Unity runtime.
- Verify locker transfers preserve stack identity through repeated take/store operations.
- Keep existing UI and normal player flow working; no new item gameplay rules.

### After Item Instance v1
1. Add item presentation metadata (icon, description, category) as needed for a readable inventory UI.
2. Design and implement one meaningful inspection/trade interaction, using a deliberately tiny item catalogue.
3. Establish semantic gameplay event contracts from the systems that own those actions.
4. Build the first minimal quest/opportunity runtime against those events as part of a real short scenario.
5. Design player progression/attributes only after the activities that need them exist.

## 8. Approval boundary

The user asked for research and a recommended next step. This document does not approve the detailed gameplay mechanics listed above. The Item Instance v1 proposal is intended as a technical data/persistence foundation only. Any player-facing quality, authenticity, ownership, price, risk, progression, quest outcome or character-stat rules must be explained and explicitly approved before implementation.


## Implementation update — item state and inspection (2026-11-18)

The user confirmed the previous item identity/state/inspection foundation passed **245/245 EditMode tests** (2026-11-18). They then approved the next technical layer: separate authoritative item state from player knowledge, record discoveries and persist them.

The active implementation now extends the original proposal:

- `ItemInstanceState` is stored per stack and currently contains quality (Poor/Standard/Fine/Exceptional), condition (Damaged/Worn/Good/Pristine), authenticity (Genuine/Counterfeit) and ownership (Legitimate/Stolen).
- These are authoritative facts. The new `ItemKnowledgeFact`/`ItemKnowledgeBook` layer is separate and records partial player beliefs with source, confidence and learned time. A rumour can be wrong; the book does not compare beliefs to hidden truth.
- `ItemDefinition` now authors description, category, optional icon, base reference value and default instance state.
- `ItemValueCalculator` combines the authored base value with central provisional state multipliers. This is an indicative reference valuation only, not a transaction price or economy.
- Inventory stacking now requires both definition and matching state. Moving/rotating/transferring/saving/loading preserves state and stable stack identity.
- Home storage lists individual stacks instead of aggregating all copies by definition. Each stack can be inspected or transferred individually.
- The same read-only item viewer is opened by right-clicking a carried item or pressing INSPECT in home storage. It now displays only known facts and withholds state-based valuation until all four properties are known.
- `GameSession.PlayerItemKnowledge` exposes the player-scoped knowledge book for future authored interactions.
- Save format is v5. It persists item knowledge independently of inventory contents; v1-v4 saves migrate with an empty knowledge book. v1/v2/v3 saves receive neutral item-state defaults (Standard, Good, Genuine, Legitimate); stable IDs from v3/v4 are preserved.

### Verification still required
The user-confirmed 245/245 result covers the preceding item identity/state/inspection work, not the new player-knowledge implementation. Unity compilation, the full EditMode suite, knowledge-aware viewer behavior, and v4→v5 save/load round-trip remain pending.

### Boundaries retained
No split-stack behavior, per-unit identity, player-facing inspection action, appraisal skill, contradiction-resolution, automatic NPC-to-player knowledge transfer, provenance chain, durability decay, item use, equipment, crafting, weight, trading, bargaining or market economy is implemented. The next meaningful extension should be designed around a real player-facing scenario rather than a broad item-simulation catalogue.
