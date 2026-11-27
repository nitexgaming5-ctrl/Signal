# Data model — current definitions and persistence

## Governing rule

Definitions describe what something is and belong in Game-layer ScriptableObjects. Runtime state describes what exists/has happened and belongs in plain C# Core models. Gameplay code references definitions; it must not invent string-based item IDs.

## Authored definitions (ScriptableObjects)

- **ItemDefinition:** stable Guid, display name/description/category, base reference value, footprint, max stack, optional icon and default ItemInstanceState.
- **RecipeDefinition:** stable Guid, display name/description, explicit Immediate timing, ItemDefinition input entries and one ItemDefinition output with positive quantities.
- **CustomerContractDefinition:** stable Guid, customer/issuer NPC ID, required ItemDefinition/quantity, fixed clean CARD reward, goods eligibility and whole-stack delivery mode.
- **CustomerRepairOrderDefinition:** stable Guid, issuer NPC ID, entrusted ItemDefinition, fixed clean CARD service fee, starting condition and required return condition.
- **WorkshopUpgradeDefinition:** stable Guid, description, unlocked recipe, installed prop prefab/offset, acquisition mode, prerequisite, positive CARD cost and resident payee NPC ID.
- **NpcCharacterDefinition:** stable NPC ID, display name, public role, awareness/movement settings, starting clean balance, utility routine profile, destinations, appearance and dialogue.
- **NpcAgendaDefinition:** stable NPC ID, prioritized independent threads, sequential stages, activity/place/discovery/relationship/opportunity/contract/upgrade conditions and typed player choices. Workshop offer choices reference existing WorkshopUpgradeDefinition assets.
- **BusinessDefinition:** stable business ID, resident owner, location/schedule, fixed-price merchandise catalogue and separate buyback entries. Each buyback entry is either fixed-price or condition-based with authored prices for Damaged, Worn, Good and Pristine states; condition offers resolve against the selected physical stack.
- **ContainerLootProfile:** weighted ItemDefinition entries and quantity ranges for one-time generation.
- **ResidentPropertyContainerDefinition:** stable container identity, owner/location, fixed item content and initial crime/exposure tuning.
- Other narrative definitions should be introduced only when a real scenario needs them.

New ScriptableObjects generate their stable Guid once. Never change it after content has been saved or referenced.

## Runtime domains (Core)

- **GridInventory / PlacedItem:** spatial grid, definition ItemType, position, rotation, stack count, stable ItemInstanceId and ItemInstanceState.
- **ItemInstanceState:** quality, condition, authenticity and ownership. Ownership is a property of the item; storage custody is separate.
- **ItemKnowledgeBook:** player beliefs about item state, each with aspect, believed value, source, confidence and learned time. Unknown aspects remain unknown.
- **NpcSocialWorld / NpcAgentState:** resident presence, each NPC's directed relationships with other NPCs, that NPC's opinion of the player, world events and individual knowledge. NPC belief is not world truth. NPC→NPC and NPC→player relationships plus individual knowledge are persisted by stable NPC/event IDs; the transient event journal and spatial perception are rebuilt.
- **PlayerRelationshipBook / PlayerDiscoveryLedger:** player-scoped directed relationship impressions and learned discovery signals. Player→NPC persistence remains separate from NPC-owned social state; all relationship directions share `NpcRelationship` without sharing storage authority.
- **NpcAgendaProgressBook:** completed NPC/thread/stage IDs and selected choice IDs; dialogue content remains in Game ScriptableObjects.
- **CleanMoneyAccount / EconomicAccountBook:** clean CARD integer balances by actor. Dirty Cash is never an account field.
- **AssetOwnershipBook / EconomicContainerRegistry:** asset owner and container custodian are distinct registries.
- **AssetTransferService / EconomicTransactionLedger:** atomic physical ownership-changing transfer and append-only asset transaction history.
- **CustomerRepairOrderBook / CustomerRepairOrderService:** one active entrusted-item repair order; item custody moves between registered inventories while AssetOwnershipBook continues to identify the resident as owner. Return atomically restores the item to the issuer, debits issuer CARD, credits player CARD and completes the order.
- **ServicePaymentTransactionLedger:** append-only clean CARD service fees, linked to the order and entrusted item but separate from physical asset transfers; one payment per order ID.
- **RetailTransactionLedger / ShopSaleTransactionLedger:** authored catalogue purchases and player sell-to-shop operations are separate from physical asset transfer.
- **Dirty Cash ledgers:** illicit exchange and cleaning histories are distinct from clean retail.
- **ProductionService / ProductionTransactionLedger:** actor-owned production and operator/time/recipe/output history.
- **CustomerContractBook:** one active authored contract state, acceptance/completion time, reward/payer and delivery transaction identity.
- **CustomerRepairOrderBook:** authored order lifecycle, exact entrusted item instance, acceptance/completion time and payment transaction identity.
- **WorkshopProgressionBook:** acquired upgrade IDs; recipe definitions remain authored content.
- **SearchableContainer:** stable identity, generated inventory, reveal order, search-started state and discovered count.
- **CrimeOutcomeLedger / CriminalProgressionTrack / HeatMeter:** semantic crime outcomes, global criminal progression and current heat are separate concepts.

## Save/load policy for build 0.01

Save DTOs are serializable plain data. They store definition GUIDs, stack instance GUIDs, enum values, scalar state, actor IDs and transaction records. They do not store Unity object references.

The current `SaveData` snapshot persists:
- player carried inventory, home storage, player stats/CARD, clock, heat and rent;
- player item knowledge, discoveries, relationships and opportunity state;
- asset ownership and economic transactions;
- resident clean account balances and retail/shop-sale transactions;
- searchable container generation/content/reveal state;
- criminal progression and crime outcomes, including selected witness observation identity;
- physical Dirty Cash contact inventory and illicit exchange/cleaning ledgers;
- customer repair-order lifecycle and service-payment transactions, alongside the entrusted item's inventory location and unchanged resident ownership;
- production transactions/operator, customer contract lifecycle, acquired workshop upgrade IDs, issuer receiver inventory and receiver actor identity;
- completed NPC agenda stage/choice progress by stable NPC/thread/stage IDs.

### Development save policy

- Build 0.01 has no serialized save-schema version, version gate or automatic migration chain.
- Save/load is expected to round-trip the current working build.
- Older experimental save files are not guaranteed to remain compatible after persisted data changes. Start a fresh development save when necessary.
- Keep current-build persistence reliable: reject a missing/unreadable file clearly and skip malformed or unknown records without silently corrupting unrelated state.

## Important inventory/economy invariants

- Same-owner container moves do not change ownership or create economic transactions.
- Ownership-changing transfers require registered source/destination custody, correct current owner, exact stack and destination capacity.
- Whole-stack transfers preserve stable identity and item state; partial operations must be explicit and preserve correct identity semantics.
- Clean CARD is a non-negative integer balance. Physical Dirty Cash is an inventory item and cannot fund legal retail.
- Business catalogue entries are authored unlimited availability; they are not a finite physical shop inventory.
- Contract rewards debit a real resident account; do not mint money or create a business account.
- Save the stable IDs and domain state required to reconstruct relationships; rebuild Unity references and runtime container registrations from authored content/live objects.


## Player Development data model

This section records the implemented P-01/P-02 foundation and the current P-04a extension.

### Authored definitions (Game)

- `CharacterAppearanceDefinition`: stable appearance option ID and compatible modular sprite-part references.
- `CharacterAnimationSetDefinition`: animation/action frame metadata, directional coverage, pivots/alignment and per-action draw-order recipes.
- `SkillDefinition`: stable skill ID, display metadata and authored rank/progression metadata.
- `MilestoneDefinition`: stable milestone ID, point reward and source/trigger context. A milestone must correspond to a successfully completed gameplay event.

### Runtime domains (Core)

- `PlayerCharacterProfile`: stable appearance selection IDs; no Unity object references.
- `PlayerAttributeSet`: exactly three base attributes — Health (primary maximum-health attribute), Armor and Heat Protection.
- `PlayerSkillBook`: skill ID to rank state.
- `PlayerSkillProgression`: fixed starting point budget, earned/unspent point accounting and validated spend/redistribution.
- `PlayerMilestoneLedger`: stable one-time milestone award identities and amounts.
- `CharacterAttributeCalculator`: pure calculation of effective Health, Armor and Heat Protection from base values plus supplied equipment/skill modifiers. P-03 owns the equipment loadout and item-to-modifier mapping.

Candidate skills (Repair, Production, Trading, Scavenging, Social) are not yet a locked catalogue. Starting budget, rank caps, costs and milestone rewards remain tuning decisions.

### Persistence target

Future SaveData additions should store only stable IDs and primitive values:
- player appearance selection IDs;
- base Health, Armor and Heat Protection;
- skill ranks and canonical point totals;
- one-time milestone award records.

Do not serialize ScriptableObject, Sprite, Texture, Animator, GameObject or prefab references. Current-build round-trip tests are mandatory. Build 0.01 has no save-schema version or migration chain, so older experimental saves may be incompatible after these DTO changes.

## Planned weather data model (W-01; not implemented)

W-01 is a separate future phase. Its likely Core state includes a shared weather snapshot (season, temperature/band, precipitation kind/intensity, wind, severity, pattern identity and transition timing), deterministic world weather progression, and player cold-exposure state. The Game layer supplies authored seasonal patterns and presentation. Weather must consume effective Heat Protection through a value/calculation interface; it must not inspect specific clothing item IDs. No hot-weather exposure model is planned.


## Player Development — implemented first-slice model

The following Core types now exist:
- `PlayerAttributeSet`: base maximum Health, Armor and Heat Protection.
- `EffectiveCharacterAttributes` and `CharacterAttributeCalculator`: pure calculation of all three effective attributes. P-03's equipment loadout combines its equipment modifiers with supplied skill modifiers.
- Current HP remains the separate `PlayerStats.Health` resource; effective Health supplies its maximum. Damage/healing rules are unchanged by P-04a.
- `PlayerSkillBook`: stable skill ID/rank map.
- `PlayerSkillProgression`: configurable starting points, rank cap, point cost, earned milestone points and unspent/allocated accounting.
- `PlayerMilestoneLedger`: one-time stable milestone awards with source identity.
- `PlayerDevelopmentState`: aggregate of attributes, skills, milestones and progression.
- `PlayerDevelopmentSaveMapper`: persists primitive values and stable IDs; no Unity object references.

The current authored Game catalog is a prototype: 8 starting points, maximum rank 5 and one point per rank. Repair, Production, Trading, Scavenging and Social remain provisional candidates. Two additional provisional test fixtures validate the attribute pipeline: `health_test` adds +10 maximum Health per rank and `heat_protection_test` adds +1 Heat Protection per rank. These are replaceable test values, not final balance or permanent skill mappings. The other livelihood skills do not yet have activity-specific effects.

## Player appearance — implemented first-slice presentation

- `PlayerCharacterVisual` composes five SpriteRenderer layers (Legs, Arm_L, Chest, Arm_R, Head) from the Body_A sheets.
- Current runtime animation set: Idle and Walk, with Down/Side/Top directional sheets, shared frame selection and side mirroring.
- `CharacterPackImporter` imports the user-supplied ZIP to Resources; the resource archive itself is not committed.
- RoomBootstrap retains Rigidbody2D/PlayerMotor2D and falls back to the existing teal placeholder if required sheets are not imported.

P-03 still owns actual clothing/armor item slots, physical item identity/custody, equipment modifiers and wearable appearance. The supplied pack has one modular base body and is not a complete wardrobe.


## Player equipment and portable storage — P-03 first slice

- EquipmentSlot defines the seven approved slots: Head, Torso, Legs, Feet and Accessory1–3. All accessory slots are generic; backpacks are compatible accessories, not a hardcoded exclusive slot.
- EquippedItem retains the exact PlacedItem and authored Armor/Heat Protection modifiers. PlayerEquipmentLoadout transfers exact single item instances between GridInventory and a slot, rejects duplicate identity/occupied slots, and aggregates effective attributes. It does not change AssetOwnershipBook.
- PortableStorageBook maps a stable backpack item instance GUID to its own GridInventory. Contents are attached to the physical backpack identity rather than to the player or equipment slot.
- Historical status at the first Core design checkpoint: GameSession integration, SaveData DTOs, UI and world drop/pickup were still pending. These gameplay/runtime paths have since been implemented in stages. Visual wearable presentation remains paused; the current active work is multiple dropped-bag metadata/visuals and automatic swapping.


## Equipped items and backpack association — P-03 second slice

- Game `ItemDefinition` now optionally authors `Equippable`, `EquipmentSlot`, Armor and Heat Protection modifiers, and portable-backpack dimensions. Core remains Unity-free.
- `SavedEquippedItem` stores stable item instance ID, definition ID, owner actor ID, slot, modifiers, item state and portable-backpack marker.
- Historical single-backpack implementation: GameSession initially attached the session's existing 8×6 grid to the backpack. This was superseded on 2026-11-27 by registry-backed allocation of a distinct grid per physical backpack and identity-keyed F5/F9 persistence.
- Current authored backpack dimensions are per ItemDefinition. Three independent 1×1 personal pockets remain accessible without a backpack. Active-grid switching and drop detachment are implemented; multiple dropped bags and automatic swap remain open.
- Prototype item assets: Canvas Backpack (Accessory 1), Padded Work Jacket (Armor +2, Heat Protection +1), Insulated Coat (Armor +0, Heat Protection +3). Modifiers are test fixtures only.

## Backpack physical world state — P-03.3

- `PortableBackpackWorldBook` maps a stable backpack item instance ID to Equipped or Dropped state and a world position. It requires the corresponding `PortableStorageBook` contents registration; drop/recovery transitions do not replace the contents grid.
- `SavedPortableBackpack` persists world state and dropped coordinates. `SavedEquippedItem` persists equipped metadata; legacy `SaveData.droppedBackpack` preserves the original single dropped bag. Multi-bag dropped metadata is the next save-schema/runtime increment.
- `PortableBackpackWorldSaveMapper` captures/restores world state after the backpack contents grid is reattached. GameSession recreates a placeholder world visual for dropped state and E recovery restores the same item to Accessory 1.
- Each physical backpack now owns a distinct GridInventory allocated for its authored dimensions and keyed by its stable instance GUID. PlayerPocketBook owns three independent GridInventory(1,1) instances; each pocket stores one stack of a 1×1 item and remains separate from every backpack when dropped. PlayerPocketSaveMapper serializes stable item IDs, stack counts, instance state and owner. The character UI displays live pocket contents and supports moving 1×1 stacks between backpack and pockets.


## Personal pockets — P-03.4b
- PlayerPocketBook owns exactly three independent GridInventory(1,1) instances. A pocket can contain one stack, and the item footprint must be 1×1 and unrotated. Stack quantity and item instance state are preserved.
- TryMoveFrom and TryMoveTo use InventoryTransfer, preserving the stable item instance ID, count and state while moving between a pocket and backpack. Both inventories remain under the player's economic custodian; no ownership transfer is created for an internal move.
- All three pocket grids are registered with EconomicContainerRegistry as player-owned containers so existing atomic transfer services can receive physical assets into them.
- SavedPocketItem and PlayerPocketSaveMapper persist pocket index, item definition/instance IDs, quantity, quality, condition, authenticity, ownership status and owner actor. GameSession captures/restores pockets independently of the backpack's 8×6 contents, so they remain available while the backpack is dropped.
- Item-receipt routing currently selects an available backpack grid or pocket for customer repair-order acceptance, retail purchase and searchable-container pickup. Milo's parcel opportunity can also place/locate its parcel in a personal pocket. CustomerContractPanel enumerates pocket items and fulfils from the source pocket. Retail SELL and Dirty Cash selection do not yet enumerate pockets; those panels switch to an empty carry view whenever the backpack is inaccessible, preventing access to dropped cargo.


## Player Quickbar (P-04b)

- `PlayerQuickbar` owns exactly six slot references (`Guid` item-instance IDs), not physical items or inventory grids.
- A physical item identity may appear in at most one quickbar slot. Replacing, swapping and unassigning slots only changes references; it never moves, copies or changes item quantity.
- Eligible references resolve to items in the player's backpack grid or one of the three personal pockets. Equipped clothing/accessory items are not eligible.
- If the backpack is dropped, references to items inside its grid remain saved but are unavailable until that same backpack is recovered. Personal-pocket references remain accessible.
- `SaveData.quickbarItemInstanceIds` stores six string GUIDs. `PlayerQuickbarSaveMapper` rejects malformed, duplicate or missing identities. Save-time/UI reconciliation clears references to items that have left carried storage.


## Inventory ownership and isolation invariant — 2026-11-27

Every physical storage location is an independent inventory aggregate. A `GridInventory` instance belongs to exactly one storage identity for its lifetime; it must never be reused as the backing grid for a different owner or physical container. Item ownership and inventory custody remain separate authorities, but neither permits two inventories to share one mutable grid.

Required independent storage identities include:
- Each physical portable backpack (keyed by its stable backpack item-instance GUID), whether equipped, dropped, carried, or otherwise stored.
- Each player personal pocket (each pocket remains its own 1×1 inventory).
- Player hideout/home storage.
- Every searchable world container, including bins and resident-owned property containers.
- Every NPC-owned carried inventory/pocket set, keyed by stable NPC identity and pocket/storage identity.
- Each business/shop stock inventory and each customer-contract/service escrow inventory.
- Any future vehicle, workbench, parcel, stash or other physical storage.

Access is a separate, temporary capability: the player can access only inventories made available by the current interaction/equipment state. Dropping, transferring, hiding, locking, or changing custody must not merge, clone, clear, or retarget another storage identity's contents.

Save data must serialize contents by storage identity, not by a single global `storageItems` list. Restore must rehydrate each identity into its own grid and reject duplicate identity-to-grid bindings. Legacy saves may be migrated only with an explicit, deterministic mapping; never silently attach all legacy contents to every bag/container.

Regression coverage must prove bidirectional isolation: adding/removing/moving items in one inventory cannot mutate any other inventory; cross-inventory movement occurs only through an explicit validated transfer operation. This applies equally to player, NPC, world, business, escrow and portable storage.

### Runtime storage identity registry (2026-11-27)

`InventoryStorageId` identifies a storage location by kind, stable owner ID and optional slot ID. `InventoryStorageRegistry` enforces a one-to-one mapping between each registered identity and one `GridInventory` instance, supports reverse lookup for audits, and rejects both duplicate identities and attempts to register an already-bound grid under another identity. Registrations remain for the session lifetime so a storage location can become inaccessible without losing its contents identity.

The registry deliberately does not grant player access, set item ownership, or determine economic custody. Those remain separate authorities. GameSession currently registers player pockets, hideout storage, the dirty-cash contact inventory, the first customer receiver/escrow inventory, street searchable containers and resident-owned searchable property containers.

**Runtime integration is partially implemented:** GameSession now allocates a distinct registry-backed grid for each equipped physical backpack and switches its active `_inventory` reference to that grid. Dropping immediately detaches the player-facing reference to a separate staging inventory; the dropped bag's grid remains registered under `PlayerBackpack/<GUID>/contents`. Inventory panels and service consumers rebind on active-grid changes. Multiple dropped bag metadata/visuals and automatic swap remain open; business stock and NPC-owned pocket runtime inventories are not yet wired. The registry and backpack work must not be mistaken for completion of the global isolation invariant.



## Portable backpack storage persistence (2026-11-27)

Each physical backpack is persisted independently through SavedPortableBackpackStorage, keyed by the backpack's stable item-instance GUID. The record stores grid dimensions and each contained item's stable instance ID, type ID, position, rotation, quantity and item-state fields. PortableBackpackStorageSaveMapper rebuilds a new GridInventory per backpack and registers it under PlayerBackpack/<GUID>/contents. It accepts a shared restored-item-ID set so duplicate physical item identities across inventory domains are rejected.

GameSession now calls PortableBackpackStorageSaveMapper during F5/F9. The legacy data.items path is reserved for the staging inventory, while each physical backpack's contents are stored under its stable backpack GUID. Legacy single-backpack saves deterministically adopt the old global items grid as the equipped or dropped bag's private grid, then allocate a separate staging grid. Core persistence was user-verified at 580/580; a further registry-cleanup test brings the expected suite to 581 pending Unity verification. Multi-dropped-backpack runtime state and migration of multiple dropped-bag metadata remain future work.


### Runtime integration boundary (2026-11-27)
The Core persistence mapper is now invoked by GameSession. `_inventory` follows the equipped bag's private grid; on drop it switches to the staging grid. All inventory UI/service consumers are rebound, and quickbar assignment validation searches every backpack grid so dropped contents remain assigned but inaccessible. The next slice is multi-dropped-backpack metadata/visual tracking and automatic swap. Unity verification of the runtime integration remains pending.


### GameSession integration notes (2026-11-27)
The active equipped backpack grid is now allocated through the registry-backed `PortableStorageBook` overload. Drop switches `_inventory` to `_emptyCarryInventory`; recovery reactivates the exact grid returned by `TryGetContents`. `SaveMapper.Capture/Apply` now operate on the staging inventory, and `PortableBackpackStorageSaveMapper` handles backpack grids separately. Legacy single-bag saves adopt the old global `items` grid as that bag's private storage. The runtime still supports only one dropped bag, so multi-bag save metadata and automatic swap are the next step.
