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
When a save shape changes, keep the current format reliable and add round-trip coverage for the new persistent state. The project does not need to preserve every obsolete development save or maintain a migration chain for all intermediate versions. A substantial schema change may intentionally invalidate older development saves and require a fresh game. Never silently corrupt or discard a current-format save; make intentional incompatibility explicit and protect the active save during writes.


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
## Economic foundation — Phase 1 (implementation started 2026-11-19)

- EconomicActorId is a stable Core identity for a person, business or institution that can own assets. The reserved player identity is player.
- AssetOwnershipBook records the authoritative owner of each physical stack by its stable ItemInstanceId. Ownership is independent of the inventory containing the stack.
- AssetTransferService transfers a whole physical stack between inventories, validates the registered current owner, changes ownership and records the completed transaction. Same-owner container moves remain InventoryTransfer operations and do not create economic transactions.
- EconomicTransaction records stable transaction ID, item instance and definition IDs, quantity, source/destination actors, reason and in-game timestamp. EconomicTransactionLedger rejects duplicate transaction IDs.
- Initial transfer reasons are metadata only: Gift, Purchase, Theft, ServicePayment, SupplierDelivery and Other. They do not independently implement those gameplay activities.
- Phase 1 is a runtime Core foundation. Save persistence and GameSession actor registration are deliberately deferred to the money/account integration phase, before the system is used for persistent player-facing transactions.

### Approved money direction (not yet implemented)
- Clean money is a bank/card balance used for legitimate purchases; no ordinary clean physical cash is required initially.
- Dirty money is a physical inventory asset for explicitly illicit payments (shady markets, bribes, etc.). It cannot be used for legitimate card/bank purchases or deposited directly.
- A future cleaning operation consumes dirty-money assets and credits the clean bank/card balance directly.
- Monetary amounts use integer minor units.
- Dirty-money provenance remains attached to the physical asset until a designed operation changes or consumes it.

### Economy simulation boundary
- No universal supply/demand, price simulation or continuous consumption simulation initially.
- Ordinary goods use authored/simple availability. Stock is tracked selectively for gameplay-sensitive categories such as drugs and illegal materials.
- Stock market, lottery, betting, business ownership, laundering and large-scale economic events are future consumers of the shared foundations.


## Economic container custody
- EconomicContainerRegistry maps each runtime GridInventory container to its responsible custodian actor. Asset ownership remains separate: an actor may own a stack stored in a container maintained by another actor.
- AssetTransferService requires both physical containers to be registered and the destination container to be maintained by the receiving actor. The source custodian need not be the asset owner; custody and ownership are distinct.


## Economic persistence (save v8)

- `CleanMoneyAccount` is the player's clean bank/card balance. It is a non-negative integer balance with guarded credit/debit operations and no direct physical-item deposit API.
- `PlayerStats.Money` remains a compatibility view over `PlayerStats.CleanMoney.Balance`; existing rent and debug-sale paths therefore operate on clean money, not dirty cash.
- Dirty money is a physical inventory item and is not stored in `CleanMoneyAccount`. Its item definition, stack identity, state and ownership use the existing inventory save shape.
- `AssetOwnershipBook` is saved per stable stack GUID for carried and home-storage items. Legacy v1-v7 saves migrate those player-held assets to the player actor by default.
- `EconomicTransactionLedger` is saved as completed item-transfer records, preserving transaction ID, item and definition IDs, quantity, actors, reason and in-game timestamp.
- Save v8 adds `cleanBalance`, `assetOwners` and `economicTransactions`. v1-v7 `money` migrates to the clean account unchanged.
- The runtime container registry is reconstructed by GameSession for carried inventory and home storage. Container custody remains separate from asset ownership.



## Economy Phase 3 — Business & Retail (first implementation pass added 2026-11-19; Unity verification pending)

- `EconomicAccountBook` is the runtime registry of clean accounts by stable `EconomicActorId`. The player's existing `PlayerStats.CleanMoney` instance is registered as actor `player`; each resident receives one account under `resident:<npc-id>`. No duplicate player account is created.
- Authored resident starting balances are applied only when an account is first registered. Save v9 stores stable actor IDs and balances; saved balances override authored values. Missing newly authored residents in an older save receive their initial balance once.
- `BusinessDefinition` is authored content (ScriptableObject), not save state. It contains stable business ID, display name/type, owner resident ID, location/interaction point, opening schedule and merchandise entries that reference `ItemDefinition` assets with fixed integer prices.
- A business catalogue is unlimited availability, not a physical inventory. It does not create supplier, delivery, production or stock state.
- `RetailTransaction` records a purchase from a business catalogue: transaction ID, business ID, customer/owner actor IDs, item definition ID, quantity, total clean amount and in-game timestamp. This is separate from the physical asset-transfer ledger because the catalogue product is not a pre-existing physical stack.
- Retail purchase updates customer account, owner account and player inventory atomically. Item instance state comes from the referenced ItemDefinition's authored default state.
- Save v9 stores resident account balances and retail transaction history. Authored business definitions, schedules, prices and unlimited catalogue entries are loaded from content and are not serialized.
- Existing player-to-shop sale behaviour and its valuation/acceptance rules remain unchanged in this phase.
- No NPC spending/earning simulation, automatic business sales, operating costs, finite stock, restocking, dynamic pricing or dirty-money retail is implemented.


## Current persistence policy (user decision, 2026-11-19)

- Current-format save/load correctness and round-trip tests are mandatory when persistent state changes.
- Backward compatibility with obsolete development saves is optional; do not spend disproportionate effort maintaining historical migrations.
- Substantial changes may intentionally invalidate older development saves and require a new game.
- Current-format saves must not be silently corrupted or discarded. Save writes should remain reliable/atomic, and intentional resets/incompatibilities must be explicit.


## Player-to-business sales (save v10)

- Each `BusinessDefinition` has a separate authored buyback-offer list. Each entry references an `ItemDefinition` and a positive fixed clean-CARD unit price. This is independent of the merchandise catalogue.
- A player sale targets one exact `ItemInstanceId` in carried inventory. Partial quantity removal preserves the stack ID and `ItemInstanceState`; full removal deletes the stack's player ownership entry.
- `ShopSaleTransaction` records business/seller identity, exact stack and definition IDs, quantity, fixed unit price/total, item-state snapshot and in-game timestamp.
- `ShopSaleTransactionLedger` is separate from physical asset transfers and unlimited retail purchases.
- Save v10 persists this ledger. A v9 save initializes an empty sale history. The business's authored offer data remains in ScriptableObject content and is not duplicated into the save.
- The business does not debit a separate budget or acquire finite stock; the sale consumes the selected goods and credits only the player's clean CARD account.



## Save schema versioning rule

- `SaveMigrator.CurrentVersion` is the single source of truth for the current serialized save format.
- Increment it only when the serialized persistent schema changes (adding/removing/changing the meaning or representation of persisted data). UI changes, runtime-only systems, and gameplay logic that do not alter persisted data do not require a version increment.
- Migration tests should keep historical input versions explicit, but assert `SaveMigrator.CurrentVersion` for the resulting current-format version. This prevents unrelated future schema additions from making old migration tests fail just because they expected a previous current version.
- Current-format capture/restore round-trip coverage remains mandatory whenever persistent state changes. Do not weaken explicit historical migration behavior or current-save integrity to avoid maintaining tests.


## Physical searchable containers — approved 2026-11-20

- Each physical container has a stable authored ID and persistent runtime GridInventory.
- Contents are generated on first interaction only. Generated item instances, stable IDs and randomized reveal order are persisted. Reopening never rerolls.
- One Search action starts an automatic sequence revealing items one at a time. Discovery changes visibility, not physical location: revealed items remain in the container.
- Container and carried inventory are presented together. The player may double-click or drag/drop a discovered stack into carried inventory. Transfer preserves stable item identity and instance state; failed capacity/placement leaves the item in the bin.
- Closing the UI pauses the sequence without resetting it. Reopening resumes at the next undiscovered entry.
- No respawn or replenishment in the first implementation.
- First test content uses three authored profiles A/B/C and 2–3 bins in Street. These are fixtures, not a final city loot economy.
- Save schema v11 adds searchable-container contents, reveal order and discovery progress. v10 saves migrate with an empty container-state collection; bins generate on first interaction.
- No new package or external code is required. Tarkov is an interaction reference only; its skills, timed resets, raid mechanics and rarity economy are excluded.


## Searchable-container runtime and save model (save v12)

- `SearchableContainer` is a stable-ID runtime aggregate containing a normal `GridInventory`, a randomized immutable reveal order, a discovered count and a persisted `HasSearchStarted` flag.
- Generation is performed by a Game-layer `ContainerLootProfile` exactly once, on first interaction. The generated item stacks, stable ItemInstanceIds, instance states and reveal order are saved. The random generator is not rerun during load.
- The reveal order retains IDs for items already transferred out. This lets the remaining search sequence resume without rediscovering or recreating taken items.
- Search can begin once. Each presentation tick reveals the next ordered stack; discovery does not move it out of the bin. The UI only renders discovered container stacks.
- Closing the panel stops the presentation timer but does not change generated state, reveal order, started state or discovered count. Reopening a started incomplete container resumes automatically.
- Each bin's inventory is registered with `EconomicContainerRegistry` under `world:<container-id>`; generated item instances are initially owned by that actor. A successful `AssetTransferService` scavenging transfer changes ownership to Player, records reason `Scavenging`, and preserves exact stack identity and state.
- Container state is persisted separately from carried inventory by `SearchableContainerSaveMapper`. The economic container registry is reconstructed from runtime inventory references. Remaining bin contents receive their world actor ownership after load; taken items remain in the player's normal saved inventory and ownership data.
- Save v12 adds `SavedSearchableContainer.searchStarted`. v10 saves without container state leave bins ungenerated. v11 generated contents/progress are retained, with the newly introduced search-started flag defaulting false.
- Game content includes three test profiles A/B/C referencing existing ITEM_A/B/C definitions. These are vertical-slice fixtures, not a generic loot-economy framework.
- The side-by-side UI uses the existing UGUI and Input System; no package or third-party code was added.


## Resident property containers, crime outcomes and progression — save v13

- `ResidentPropertyContainerDefinition` is authored Game content. It identifies a stable container, resident owner, location, world interaction point, fixed ItemDefinition contents, witness radius, and the crime action's heat/progression awards and observable witness description.
- Unlike street-bin `ContainerLootProfile` content, resident-property contents are fixed authored entries. They are initialized once and are never randomized or respawned.
- The runtime property inventory is registered with `EconomicContainerRegistry` under the resident owner actor. Container custody and asset ownership remain distinct.
- A successful transfer from a resident-property container uses `AssetTransferReason.Theft`; street-bin transfers remain `Scavenging`.
- `AssetTransferService` marks the moved stack's `ItemOwnershipStatus` as `Stolen` inside the existing snapshot/rollback boundary. Quality, condition, authenticity and ItemInstanceId are preserved.
- The economic transfer ledger records the exact item/quantity, from/to actors, Theft reason and timestamp. `CrimeOutcome` is a separate semantic record tied to the transfer transaction ID; it records the action, actor, item, location/time, authored awards and optional single witness/event.
- `CrimeOutcomeLedger` is independent from both the economic transaction ledger and NPC knowledge. `CriminalProgressionTrack` is one global non-decaying integer total; heat remains the separate bounded/cooling `HeatMeter`.
- A witness is selected only from residents physically in the property's authored location and within both the authored witness radius and the NPC's awareness radius. The selected resident receives one explicit character-scoped `NpcWorldEvent`; other residents do not receive a broadcast. Generic ambient social encounters skip `crime-observed` events until witness-sharing rules are approved. The saved outcome retains enough information to restore that one witness's knowledge after load.
- The first Rhea wardrobe fixture uses ITEM_A ×2, ITEM_C ×1, +2 heat, +1 progression point per successful stolen stack and an 8-unit witness radius. These are prototype playtest values, not final balance. There is no automatic reporting, police response, heat-band consequence or relationship change.
- Save v13 adds `criminalProgressionPoints` and `SavedCrimeOutcome` records. `SearchableContainerSaveMapper` continues to persist the wardrobe's generated fixed contents, reveal order and search progress. v12 and older saves initialize an empty crime ledger and zero progression.


## Dirty Cash and Slice B illicit transactions — approved design

- Dirty Cash is a normal physical item stack with stable ItemInstanceId and ItemInstanceState, not a clean-money account balance.
- Denomination is 10 per unit. This contact's offer asset authors a 20% cleaning fee; each unit credits 8 clean CARD to the player and the retained 2 CARD is credited to the contact's NPC account.
- The first fence exchange consumes exactly 2 player-owned, Stolen ITEM_A items and creates 3 Dirty Cash units as a new player-owned physical stack. The sold item transfers to the contact. These fixture values are authored in `DirtyCashContactOffer_SliceB.asset`.
- Cleaning consumes an exact player-owned Dirty Cash stack, credits 80% as clean CARD to the player and credits the retained 20% fee to the contact's existing NPC account. Both account credits, inventory consumption and ownership removal are atomic.
- Illicit exchange and cleaning histories are distinct from legitimate retail purchase and shop-sale histories.
- The contact is an ordinary shared-city resident with internal NPC ID `Clean_Dirtycash` and an intentionally blank visible display name.
- Dirty Cash cannot pay for legal retail or be deposited directly. No passive conversion, automatic cleaning, dynamic rate or global laundering simulation.
- Save v14 preserves the contact's physical inventory, item ownership, resident clean-money accounts and both dedicated transaction histories. v13 migration initializes the new collections empty while preserving existing inventory, crime and searchable-container data.
- Full specification: `docs/tasks/CRIME-ECONOMY-SLICE-B-DIRTY-CASH.md`.
