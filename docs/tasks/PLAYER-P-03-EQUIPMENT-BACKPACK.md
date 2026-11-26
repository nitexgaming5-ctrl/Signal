# P-03 — Equipment, Wearables & Portable Backpack

**Status (2026-11-26):** User confirmed **503/503 EditMode tests passing** after the three targeted P-03.5 tests and pocket-aware service/repair/production integration. Backpack drop/recovery, three persistent pockets, hidden-pocket visibility contract, pocket/backpack transfers and pocket swaps are implemented. P-03.5 source integration now covers Retail SELL, Dirty Cash fence/cleaning selection, Home Storage, workbench repair and production across accessible backpack/pocket grids. Three targeted EditMode tests were added for split-grid repair and multi-grid production; the user subsequently confirmed the full 503/503 suite. P-03 remains active: broader direct-grid audit, wearable visuals, debug-loadout cleanup and full Play Mode acceptance are outstanding.
**Track:** Player Development
**Dependency:** P-01 modular player visual and P-02 Armor/Heat Protection modifier contract.

## Player outcome

The player equips physical clothing, protection and accessories. Equipment is authoritative item state, not a second copy of inventory data. A backpack is a physical item whose contents remain attached to that backpack instance when it is equipped, carried, dropped or recovered.

## Confirmed slot layout

Seven slots total:

| Slot | Intended category |
|---|---|
| Head | Hats, helmets and headwear |
| Torso | Shirts, jackets, body armor |
| Legs | Trousers and legwear |
| Feet | Shoes and boots |
| Accessory 1 | Generic accessory; backpack is compatible |
| Accessory 2 | Generic accessory |
| Accessory 3 | Generic accessory |

Accessory slots are generic. Do not reserve all three for hardcoded item categories. A backpack is one possible accessory and consumes one accessory slot.

## Binding rules

1. Each equipped item is one exact physical item instance; stacked quantities cannot be equipped as a single item.
2. Equip/unequip moves the same stable ItemInstanceId, ItemInstanceState and ItemType; it does not clone, recreate or change ownership.
3. Equipment custody is separate from ownership. Equipping a resident-owned item does not silently transfer ownership.
4. A slot holds at most one item. The same physical item cannot occupy multiple slots.
5. Failed equip/unequip operations leave both source and destination unchanged.
6. Effective Armor and Heat Protection are base attributes plus the sum of equipped item modifiers, clamped safely. No combat or weather effects are introduced here.
7. Backpack contents belong to the backpack's stable physical instance ID, not to the player inventory or the equipment slot. Dropping the backpack must not spill, duplicate or destroy its contents.
8. When a backpack is dropped, its contents remain inside the same portable container. Picking it up recovers that same container and its contents.
9. Backpack capacity is authored per item. The initial Core registry represents each backpack's contents as its own grid. UI presentation and how that grid appears alongside player inventory are a later Game integration.
10. If the backpack is lost, stolen or abandoned, its contents remain with it. Ownership and world custody continue to use existing asset/container authorities.
11. Do not introduce weight, movement penalties, pursuit penalties, or automatic backpack dropping without a separate approved rule. The player chooses when to abandon it.
12. No new NPCs, backstories or story-specific content.

## Inventory direction confirmed

- Final no-backpack state: exactly three independent 1×1 personal pockets, each holding one stack of a 1×1 item. Implemented in P-03.4b; Unity verification pending.
- With a backpack equipped: the existing large tabbed grid is the backpack's contents.
- Dropping the backpack removes access to that large grid from the player; the physical bag retains every item. Recovering/equipping it restores access to the same contents.
- The 8×6 grid is accessible only while the backpack is equipped. It remains attached to the physical bag when dropped; the three personal pockets remain accessible.

## Architecture

### Core (implemented first slice)
- EquipmentSlot: Head, Torso, Legs, Feet, Accessory1, Accessory2, Accessory3.
- EquippedItem: exact PlacedItem plus authored Armor/Heat Protection modifiers.
- PlayerEquipmentLoadout: validated equip/unequip, slot uniqueness and effective-attribute aggregation.
- PortableStorageBook: maps stable backpack item instance IDs to independent GridInventory contents; refuses duplicate identities and refuses to discard a non-empty backpack container.

### Implemented in the second slice
- `ItemDefinition` now has optional equipment metadata: compatible slot, Armor/Heat Protection modifiers, backpack flag and backpack grid dimensions.
- `GameSession` loads the three prototype items from `Resources/SurvivalItems/Definitions` through the existing Resources catalogue discovery.
- Inventory interaction: hover an item and press E to equip; press 1–7 to unequip ordinary slots. Press B or use the UI button to drop the backpack; E near the dropped bag recovers and auto-equips it.
- The prototype player inventory is expanded to 8×6 for testing. Equipping the Canvas Backpack binds that exact backpack instance ID to the existing inventory grid without copying any contents.
- F5/F9 now capture/restore equipped item identity, type, slot, modifiers, item state, owner actor and backpack association. The current grid's items were already handled by the existing inventory save path.
- A provisional backpack silhouette is shown behind the character when equipped; this is a placeholder, not final backpack art.

### P-03.3 Core lifecycle foundation (2026-11-25)

- Added `PortableBackpackWorldBook`, which records each registered backpack as Equipped or Dropped and stores the dropped world coordinates.
- Registration requires an existing `PortableStorageBook` contents grid keyed by the exact same backpack instance ID. Drop/recover transitions never copy, clear or reassign the contents grid.
- Added `SavedPortableBackpack` and `PortableBackpackWorldSaveMapper` for serializing/restoring world state. Restore rejects malformed IDs, duplicate records, invalid statuses, non-finite coordinates and backpacks whose contents identity is unavailable.
- Added seven focused EditMode tests for registration preconditions, exact identity/content continuity, valid/invalid transitions, duplicate registration, dropped position save/restore, malformed records and equipped-coordinate normalization.
- The Core lifecycle foundation is integrated into GameSession: B drops the equipped bag, a world placeholder appears, E recovers the same instance, and F5/F9 persist its dropped/equipped state and position. User verified the fresh-start/drop/recovery UI path; continue testing save/load edge cases.

### Remaining integration
- Complete the pocket-aware service-panel audit: Retail SELL, Dirty Cash selection, home storage, workbench/production and any other panels still bound directly to the backpack grid.
- Add actual wearable sprite overlays for head/torso/legs/feet and equipment variants. The supplied Body_A pack is only a base body; do not invent a full wardrobe from missing sprites.
- Complete the P-03.4b Play Mode acceptance tests, then author more clothing/armor content after the core loop is validated.
- Add actual wearable sprite overlays for head/torso/legs/feet and equipment variants. The supplied Body_A pack is only a base body; do not invent a full wardrobe from missing sprites.
- Author more clothing/armor content after the core loop is validated.

## Next Play Mode test game — Backpack lifecycle acceptance

Run this after the current GameSession/world-object integration is pulled and the expected 494 EditMode tests are green.

### Setup

1. Start a fresh test save in the hideout.
2. Use the debug item spawner to add one Canvas Backpack and several identifiable cargo items with different footprints/states (for example, a Basic Patch Kit and a Work Jacket).
3. Open the inventory, place the cargo in distinct cells, and equip the Canvas Backpack with E.
4. Confirm the backpack silhouette appears and the large grid still contains the exact cargo.

### Scenario A — drop and recover in one session

1. Press B or click **DROP BACKPACK** in the character sidebar.
2. Confirm the backpack appears at the player's drop position and the player no longer has access to its large grid.
3. Confirm the small personal pockets remain usable and can hold only their authored capacity.
4. Move away, return, and press E beside the dropped backpack. It should be picked up and automatically re-equipped.
5. Confirm the same backpack is equipped and every cargo item returns in the same condition, quality, authenticity, quantity and identity.

### Scenario B — save while dropped

1. Equip the backpack, ensure at least two cargo items are inside, then drop it.
2. Move a few tiles away and press F5.
3. Restart the Play Mode session (or reload the scene) and press F9.
4. Confirm the backpack is still on the ground at the saved position; it must not reappear equipped or duplicate.
5. Recover it and verify all cargo is present exactly once.

### Scenario C — save while equipped

1. With the backpack recovered/equipped and cargo still inside, press F5.
2. Restart/reload and press F9.
3. Confirm the backpack is equipped, its visual is present, and its same cargo is accessible.
4. Drop and recover it once more to ensure restored state did not break the lifecycle.

### Scenario D — custody and edge cases

- Attempt to equip a second backpack while one is equipped; reject without losing either item.
- Attempt to recover a backpack that is already equipped; reject harmlessly.
- Attempt to drop a backpack while its inventory UI is open; the UI must close safely and no drag operation may be left active.
- Confirm ordinary clothing/accessory equipment remains equipped when the backpack is dropped.
- Confirm the backpack's owner remains unchanged by dropping/recovering; custody changes, ownership does not.
- Confirm no duplicate world object appears after repeated F9 loads.

### Acceptance criteria

- One backpack identity maps to exactly one world/equipment state and exactly one contents grid.
- The inventory grid is hidden and pointer interaction is blocked whenever the backpack is dropped. Other item-consuming/producing interactions still need a complete active-inventory audit; the final pocket inventory remains pending.
- Dropped position and state survive F5/F9.
- Recovery restores access to the exact contents; no cargo loss, duplication, state reset or ownership mutation.
- The user reports the full EditMode count and all four Play Mode scenarios passing before P-03.3 is marked complete.

## First Core implementation slice committed

- Added PlayerEquipmentLoadout.cs in Core with the seven slots, exact-instance equip/unequip transfers, slot uniqueness, and Armor/Heat Protection aggregation.
- Added PortableStorageBook keyed by backpack stable item ID. Contents remain in their own GridInventory; non-empty storage cannot be discarded.
- Added nine focused EditMode tests covering slot count, exact identity/state transfer, invalid/stacked items, duplicate slot identity, capacity failure/rollback, modifier aggregation/clamping, backpack identity/content continuity and safe empty-container removal.
- Added Unity .meta files for both new source files.

## Verification and remaining work

- The 2–3 pocket no-backpack fallback is not enforced yet; the temporary 8×6 grid remains available for testing.
- A dedicated equipment/loadout panel and final wearable sprite overlays are not implemented. The current backpack silhouette is deliberately placeholder art.
- Physical backpack drop/pickup is not implemented yet. Backpack unequip is blocked until that physical transfer exists.
- No Unity compilation or EditMode test run has been performed by the repository-edit pass. The user confirmed **478/478 EditMode tests passing** after the two additional P-03 tests.
- P-03 is not complete until the 2–3 pocket fallback, equipped backpack, physical drop/pickup with exact contents, visual equipment and full EditMode/Play Mode acceptance are verified.


## Second implementation slice — backpack equipped (2026-11-25)

- Increased the temporary player test grid from 6×4 to 8×6. This is explicitly provisional; final no-backpack inventory remains 2–3 pockets.
- Added equipment metadata to ItemDefinition and three prototype test assets: Canvas Backpack (Accessory 1, 8×6), Padded Work Jacket (Torso, Armor +2 / Heat Protection +1), and Insulated Coat (Torso, Armor +0 / Heat Protection +3). Modifier values are test tuning, not final balance.
- Inventory hover + E equips the item. Number keys 1–7 unequip ordinary equipment slots; the backpack remains equipped until the physical drop/pickup phase exists.
- Equipping the backpack binds its stable instance ID to the existing 8×6 GridInventory without copying or moving contents. A placeholder silhouette appears behind the player.
- Equipped item records (identity, type, owner, slot, modifiers and item state) are captured/restored through F5/F9. The 8×6 inventory contents continue through the existing inventory save mapper.
- Added focused tests for attaching an existing grid and restoring an equipped item without requiring a free inventory cell. The user subsequently confirmed **478/478 EditMode tests passing**.
- Not yet complete: no-backpack 2–3 pocket restriction, final equipment panel, visual clothing overlays, or physical backpack drop/pickup.


## P-03.3 Game integration — backpack drop, pickup and character sidebar (2026-11-25)

- Added exact equipped-item extraction to PlayerEquipmentLoadout so a backpack can leave the equipment slot without being returned into its own contents grid. The physical backpack identity, item state and owner remain unchanged.
- GameSession now tracks the backpack's Equipped/Dropped world state, creates a visible ground placeholder at the drop position, and supports recovery with E. Recovery equips the same backpack instance automatically.
- Added B as the direct drop key and a clickable **DROP BACKPACK [B]** button in the inventory character sidebar.
- Reworked the inventory presentation into a left character/equipment panel and right **BACKPACK STORAGE** grid. Seven equipment slots are listed; three personal-pocket cells are currently visual placeholders, not yet backed by a functional pocket inventory.
- While the backpack is dropped, the backpack grid is hidden and pointer item interaction with that grid is blocked. The player can still move to the bag and press E to recover it. Item-driven E/F interactions remain blocked until recovery, but world navigation is explicitly allowed: E can still enter/leave buildings and return to the street, with the matching door/exit prompt retained. This is a safety fallback, not the final pocket-based interaction model.
- F5 captures the backpack world state and, when dropped, the exact item definition, stable instance ID, owner, modifiers and item state. F9 restores the bag at its saved world position and respawns its ground placeholder; recovery equips it.
- Added a focused Core test for extracting an equipped item without copying it into an inventory. The user's confirmed baseline was 485/485; **486/486 is expected after this test**, pending Unity compilation and EditMode execution. No Unity or Play Mode result is claimed by this repository pass.
- Next user test: equip → place cargo → B/button drop → confirm grid hidden → E pickup/auto-equip → F5 while dropped → F9 → recover; then repeat F5/F9 while equipped. Check cargo identity/state and ownership throughout.

- Follow-up fix: the first navigation-prompt exception used exact door strings and missed alternate authored wording (for example, “E Exit building”). It now recognizes E-prefixed enter/exit/leave/return prompts case-insensitively, while continuing to suppress item-based prompts when the backpack is dropped. Unity verification is pending.

### HUD exit-prompt and Unity warning follow-up (2026-11-25)
- User clarified that exiting the building worked while the backpack was dropped, but the HUD did not display the required E instruction. The prior prompt check inspected the modified interaction prompt and required an E prefix, so an authored world prompt such as `Exit building` could be missed.
- GameSession now preserves the raw CityBlock world-transition prompt separately from nearby interaction overrides. During dropped-backpack mode, the HUD recognises enter/exit/leave/return wording and adds `E` if the authored prompt does not include it. Item-interaction blocking remains unchanged.
- Replaced obsolete `FindObjectOfType<EventSystem>()` with `FindAnyObjectByType<EventSystem>()` in InventoryPanel to remove CS0618.
- User reports **486/486 EditMode tests passing** after pulling the previous implementation. The latest prompt/API cleanup commits are newer than that test run; recompile and rerun tests, then verify the visible E exit prompt in Play Mode. No verification of these latest changes is claimed yet.


## P-03.4a — Backpack-independent interaction and safe item receipt (2026-11-25)

### Approved interaction rule
A backpack is a capacity upgrade, not a prerequisite for participating in the city. Without one, the player can still talk to NPCs, open and accept customer requests, use agenda conversations, make dialogue choices, and navigate buildings. The global E/F early-return gate has been removed.

### Physical item capacity guards
Until the three personal pockets are implemented, the current no-backpack state has no functional carry slots. Operations that would place physical items into carried inventory now fail safely with an explicit "Not enough space / equip a backpack" message:
- accepting the entrusted repair-order jacket;
- buying merchandise from a business;
- taking items from searchable containers;
- receiving Dirty Cash from the illicit exchange;
- receiving production output.

Home-storage transfers and workbench access are blocked without a backpack because those flows currently depend on the large inventory grid. Retail browsing, container inspection, customer-contract review/acceptance and NPC dialogue remain available. Existing atomic Core services remain authoritative; the Game guards run before invoking them, so a rejected receive does not debit money, transfer ownership or consume inputs.

### P-03.4a verification
- Source change is committed in GameSession. No automated test was added in this subphase.
- Latest user-confirmed full suite remains **486/486** before this change; Unity compile, EditMode rerun and Play Mode verification of no-backpack dialogue/mission flows are pending.
- Test: start without backpack; talk to an ordinary resident; open an agenda conversation; open and accept the customer request; open the repair order and attempt acceptance (must show capacity warning and leave the order/item unchanged); browse a shop and attempt purchase (must show warning and preserve CARD); inspect a searchable container and attempt to take an item (must show warning and preserve item/ownership); equip backpack and repeat the physical item flows successfully.

### Next P-03.4b — functional personal pockets
Implement three actual 1×1 personal pocket slots, separate from the backpack's 8×6 contents grid. The pocket inventory must persist through F5/F9, remain accessible when the backpack is dropped, and participate in item-receive capacity checks. UI must show real pocket contents and support moving items between pockets and backpack. Do not treat the current placeholder cells as functional storage.


## P-03.4b — functional personal pockets (2026-11-25)

### Confirmed pocket rules
- Exactly three personal pockets exist independently of any backpack.
- Each pocket is a separate 1×1 slot and holds one stack of one unrotated 1×1 item. Larger items require the backpack grid.
- Pocket contents stay with the player while the backpack is dropped. Backpack contents stay inside the dropped physical bag.
- Each pocket stack keeps its stable item instance ID, quantity, condition, quality, authenticity and ownership state.
- Internal pocket/backpack moves preserve the exact item identity and do not change the player's ownership.

### Implementation
- Added Core PlayerPocketBook with three independent GridInventory(1,1) pockets, capacity checks and exact-stack transfer methods.
- Registered each pocket as a player-owned economic container. Existing transfer services can now receive searchable-container assets into a pocket without bypassing ownership validation.
- Added SavedPocketItem and PlayerPocketSaveMapper; F5/F9 captures/restores pocket index, stable item identity, stack count, item state and owner separately from backpack contents.
- Inventory UI now renders live pocket contents. Hover a 1×1 item in backpack storage and press P to move it to the first free pocket. Click an occupied pocket to move its stack into the equipped backpack; if no backpack is available, clicking inspects the item. Ordinary equipped 1×1 gear can also be unequipped into a free pocket when the backpack is absent/dropped.
- Item-receive routing can select an available pocket when the backpack is absent/dropped or has no suitable free cell: entrusted repair-order acceptance, retail purchase, searchable-container pickup and Dirty Cash exchange output.
- Milo's parcel opportunity now accepts and tracks its exact parcel in a pocket when no backpack is equipped; a parcel inside a dropped backpack is not treated as carried/accessibly held.

### Automated tests added
- Eight Core tests cover three independent 1×1 pockets, supported item footprints, capacity, stable identity/state, exact-stack moves in both directions, failed-transfer atomicity, save/load round-trip and malformed/duplicate save records.
- User's latest confirmed result is 486/486 before these eight tests. Expected total is 494/494, pending Unity compilation and EditMode execution. No Unity result is claimed by this implementation pass.

### P-03.4b Play Mode test game
1. Start a fresh session without F9 and confirm all three pockets show EMPTY; the backpack grid remains unavailable.
2. Use the debug item spawner to add three different 1×1 items. Confirm they occupy P1, P2 and P3. Try adding a fourth 1×1 item and confirm a clear full-pocket warning with no item loss.
3. Try spawning or receiving an item with a 2×1 or 1×2 footprint without a backpack. Confirm it is rejected without losing the source item or taking payment.
4. Press F5, restart/load with F9, and confirm all pocket stacks return with the same item IDs, quantities and states.
5. Accept Milo's parcel opportunity without a backpack while a pocket is free. Confirm the parcel appears in that pocket and can be returned/delivered; with all pockets full, acceptance must fail without changing the opportunity.
6. Accept a customer contract without a backpack. Confirm the panel lists pocket-held goods and that a qualifying player-owned pocket stack can be handed over.
7. Open a searchable container and take a 1×1 item with no backpack. Confirm it enters the first free pocket and ownership is Player. Fill all pockets and confirm the next pickup leaves the container item untouched.
8. Browse a shop without a backpack and buy a 1×1 item with a free pocket. Confirm payment and item receipt both succeed; with full pockets, confirm no CARD is debited.
9. Equip the Canvas Backpack. Move a 1×1 item from backpack storage to a pocket with P; click the occupied pocket to move it back. Confirm exact identity and quantity in both directions.
10. Drop the backpack with B. Confirm pockets remain visible/usable, backpack grid is inaccessible, and cargo remains in the dropped bag. Save/load while dropped, then recover and verify both pocket and backpack contents.
11. Accept the entrusted repair order without a backpack with a free pocket; verify the jacket enters that pocket. Equip a backpack and move it into backpack storage before attempting the workbench repair flow.

### Known follow-up limits
- Home-storage transfer and workbench/production still depend on the backpack grid; production is not available without a backpack.
- Customer Contract now lists pocket-held goods, so a player can accept and fulfil a qualifying request without a backpack. When the backpack is absent/dropped, Retail, Dirty Cash and searchable-container panels use an empty carry view rather than exposing hidden backpack cargo; pocket-held items are not yet enumerated in Retail SELL or Dirty Cash selection.
- P-03 is not complete: full pocket-aware service-panel coverage, equipment/clothing visual overlays and end-to-end Play Mode acceptance remain.

## UI follow-up — pocket drag/drop, equipment access and search-grid sizing (2026-11-25)
- Added drag-and-drop from the backpack grid onto any of the three personal pocket cells. Pocket acceptance still enforces the approved rule: unrotated 1×1 items only, one stack per pocket; rejected drops leave the source item untouched.
- E now equips an item when hovering either an accessible backpack item or an occupied pocket. Pocket equipment uses the actual pocket GridInventory as the source, preserving exact item identity and ownership. This also allows equippable debug-spawned items to be equipped when the player has no backpack.
- Fixed SearchableContainerPanel's player-side grid from a hardcoded 6×4 display to the active inventory's actual dimensions. With the current backpack this should render 8×6, matching the real GridInventory and item placement coordinates.
- User reports the preceding P-03.4b suite passed **494/494**. These UI follow-up commits still require Unity compilation and Play Mode verification.
- Add to Play Mode acceptance: drag a 1×1 item onto P1/P2/P3 and verify the stack moves; drag a 2×1 item or rotated item and verify rejection; hover an equippable item in a pocket and press E; open a searchable bin with the backpack equipped and verify the player grid is 8×6 and the transferred item remains in the same backpack cell.

## Debug-only fresh-session starting loadout (2026-11-26)
- Added a temporary GameSession startup bootstrap for Play Mode testing. Each fresh session finds a compatible portable backpack definition (8×6), creates a unique backpack item instance, equips it through the normal equipment/storage/world-state path, then seeds up to three non-backpack items with distinct grid footprints into the backpack.
- The seed items are selected from the loaded item catalogue, sorted largest footprint first, and placed through normal GridInventory placement. Ownership is registered to the Player. Existing item definitions and save schemas are unchanged.
- This is explicitly debug/test scaffolding, not intended starting gear or final game balance. Remove this helper before establishing the real new-game starting inventory.
- If no compatible backpack is available, the helper logs a warning and leaves the session otherwise unchanged. If fewer than three distinct item footprints exist in the catalogue, it seeds however many are available.
- No Unity compile, EditMode or Play Mode result is claimed for this commit. User should pull and confirm the character spawns with an equipped backpack and 2–3 differently-sized items visible in its 8×6 grid.

## Pocket UI follow-up and hidden-pocket design (2026-11-26)
- Pocket cells now render the corresponding ItemDefinition sprite when one exists; when no sprite is authored, the cell displays the item name as a readable fallback rather than only an empty-looking slot.
- Pocket-to-backpack drag/drop is now supported. Dragging an occupied pocket onto a valid backpack cell transfers the exact item stack to that precise cell, preserving stable instance identity and item state. Invalid/occupied destinations leave the item in its pocket. The existing click action remains available for inspecting a pocket item.
- Added Core coverage for exact-position pocket transfer and atomic failure when the target cell is occupied. These tests have been added but have NOT yet been run in Unity; the last user-confirmed baseline remains 494/494.
- User clarified the intended pocket layout: two ordinary personal pockets plus one hidden pocket, with the hidden pocket intended to preserve at least one object from police/search confiscation. This changes the current three-equivalent-pocket design. Do not implement hidden-pocket search/concealment rules until the user explicitly confirms the proposed rules. Proposed baseline for confirmation: all three pockets remain player-accessible; only the first two are exposed to ordinary inventory searches; the hidden slot is excluded from routine police searches and preserves its single 1x1 item. A special thorough/strip search could be a separate future rule if desired.

## Pocket roles and interaction follow-up (2026-11-26)

- Pocket layout is now explicitly asymmetric: P1 and P2 are ordinary pockets; P3 is the hidden pocket.
- Ordinary-search visibility is represented in PlayerPocketBook: routine searches can enumerate P1/P2 contents only. P3 remains player-accessible but is omitted from ordinary-search results. Advanced detection methods (scanner, dogs, thorough search) are intentionally undefined and out of scope until separately designed.
- The inventory UI marks P3 with a muted light-red background and a “HIDDEN” label.
- Backpack-to-pocket drag/drop now targets the exact pocket cell under the pointer. The old first-free-pocket shortcut has been removed, so the player chooses P1, P2 or P3.
- Pocket-to-backpack drag/drop uses the exact backpack cell under the pointer and preserves the item instance/state; invalid drops leave the item in its pocket.
- Pocket item visuals use the matching ItemDefinition icon when available, with a text fallback when no icon is authored.
- Added Core coverage for ordinary-search visibility and hidden-pocket exclusion. New tests are pending Unity execution.
- Important integration boundary: the current project does not yet have a complete police-search/confiscation flow. The new visibility API is the canonical rule for routine-search enumeration; when the police-search system is implemented, it must use this API rather than scanning all three pocket inventories directly.


## Direct pocket-to-pocket transfers (2026-11-26)

- InventoryPanel now allows dragging an occupied pocket directly onto another pocket, whether or not a backpack is equipped.
- Dropping onto an empty pocket moves the complete stack. Dropping onto an occupied pocket swaps both complete stacks.
- Core validates distinct pocket indices and the existing one-cell/unrotated item contract. Both paths preserve stable instance IDs, stack counts, condition, quality, authenticity and ownership state. Occupied-pocket swaps update both inventories before emitting change notifications; no temporary empty-pocket state is exposed.
- Added three EditMode tests for move-to-empty, occupied-pocket swap identity/state preservation, and invalid-move non-mutation.
- Unity compilation, EditMode execution and Play Mode verification are pending. The last user-confirmed suite remains 494/494; expected total after these three tests is 500, subject to the user's run.
- Play Mode check: with the backpack equipped and then again with it dropped, drag P1 onto empty P3; then drag P3 onto occupied P2 and confirm the two item stacks exchange positions. Confirm P3 remains marked HIDDEN and the routine-search visibility rule remains P1/P2 only.


## Verified checkpoint — 503/503 (2026-11-26)

- User confirmed the complete Unity EditMode suite passes at **503/503** after the direct pocket-to-pocket move/swap and P-03.5 pocket-aware service integration.
- This verifies the automated suite only; it does not by itself certify all P-03 Play Mode scenarios.
- Direct pocket moves/swaps preserve stable item identity, stack quantity, condition, quality, authenticity and ownership. P1/P2 remain ordinary; P3 remains player-accessible but excluded from ordinary-search enumeration.

## Remaining P-03 delivery sequence

1. **Pocket-aware service integration:** complete an inventory-access audit and remove assumptions that the 8×6 backpack grid is the only player-carried container. First cover Retail SELL and Dirty Cash item selection; then home storage, workbench/production inputs and outputs, and any other item-consuming/producing panels. Transfers and transactions must remain atomic across the actual source/destination pocket or backpack grid. Never expose cargo inside a dropped backpack.
2. **Wearable presentation:** add actual equipped-item visuals for Head, Torso, Legs and Feet where the supplied Body_A pack and authored item sprites support them. Do not fabricate unavailable art; document unsupported slots/variants and keep equipment state authoritative even when no overlay exists. Accessories and backpack visuals should remain distinct layers.
3. **Remove temporary debug bootstrap:** remove the fresh-session debug backpack/cargo seeding once it is no longer needed for acceptance testing. Keep debug item spawning available as a deliberate test tool; do not turn the debug loadout into permanent starting gear.
4. **End-to-end Play Mode acceptance:** run the complete backpack lifecycle, pockets, save/load, customer/retail/Dirty Cash/workbench/storage interactions, and wearable equip/unequip scenarios in the active HideoutSlice scene. Record only user-reported Unity results.

Recommended next implementation slice: **P-03.5 — pocket-aware service-panel integration**, beginning with a focused audit of Retail SELL and Dirty Cash selection. This is the next dependency before visual polish because items must be usable consistently regardless of which carried container holds them.


### P-03.5 implementation checkpoint — service panels and hideout storage (2026-11-26)

- Retail SELL now enumerates all accessible carried grids (equipped backpack plus P1/P2/P3), resolves sale by stable item ID through GameSession's source-inventory lookup, and refreshes when any carried grid changes.
- Dirty Cash fence/cleaning panel now displays the 8×6 backpack grid only while the backpack is accessible, always displays the three personal pockets separately, and resolves selected stack IDs across those source inventories. The existing exchange/cleaning transaction services still receive the actual source grid from GameSession; no dropped-backpack cargo is enumerated.
- Home Storage now lists items from every accessible carried grid. STORE transfers from the exact source grid; TAKE chooses a destination through GameSession's carry-capacity helper, so a pocket can receive an item when the backpack is absent/full (subject to the existing item-footprint rules).
- This initial service/storage checkpoint was expanded by the implementation update immediately below; it is retained as session history. All source changes still require Unity compile, EditMode tests and Play Mode acceptance.


### P-03.5 implementation checkpoint — service panels, storage and workbench (2026-11-26)

- Retail SELL enumerates owned accepted stacks across the accessible backpack and P1/P2/P3, resolves the exact source grid by stable item instance ID, and refreshes on carried-grid changes.
- Dirty Cash fence/cleaning selection displays the accessible 8×6 backpack grid and three separate personal pockets. Selection resolves against the true source grid before invoking the existing atomic exchange/cleaning services. Dropped-backpack cargo is never enumerated.
- Home Storage lists every accessible carried grid. STORE transfers from the exact source grid; TAKE uses GameSession's carry-capacity selector to choose the backpack or a suitable pocket.
- Searchable Container now displays the accessible backpack grid only when available and always displays P1/P2/P3 separately, with P3 visibly marked HIDDEN. Loot pickup still routes through the capacity-aware GameSession callback; the panel does not expose dropped-backpack cargo.
- Workbench repair target and patch-kit discovery now spans all accessible carried grids. RepairService accepts separate target and kit inventories, so the jacket can remain in the backpack while a pocket-held kit is consumed (and vice versa).
- ProductionService now supports transactional ingredient consumption across multiple accessible grids and a separately selected output grid. It snapshots all participating grids and restores them plus ownership registrations on failure. GameSession chooses output capacity through the same backpack/pocket carry selector; producing no longer requires an equipped backpack when the pockets can receive the output.
- Added three targeted EditMode cases: repair with target and kit in separate grids; production consuming ingredients across backpack/pocket grids with output routed to a pocket; and multi-grid output failure restoring every ingredient grid and ownership record.
- All of the above is source-level work only. The user must run Unity compilation and the full EditMode suite before any verification claim; Play Mode acceptance remains outstanding.
- Next: finish the remaining direct-grid audit in customer/service panels and any other item consumers, then move to wearable presentation, temporary debug-loadout cleanup and end-to-end P-03 acceptance.


### Temporary repair-debug starting loadout (2026-11-26)

- Fresh Play Mode sessions now seed the equipped Canvas Backpack with a Worn Work Jacket, one ready-to-use Basic Patch Kit, and one full Basic Patch Kit recipe set: Fabric Roll, Thread Spool and Adhesive Tube.
- The jacket instance is explicitly Worn, so a Basic Patch Kit can restore it to Good. The separate recipe materials let the player craft a replacement kit and test production without first shopping or scavenging.
- Three additional distinct-footprint catalogue items are still added for general backpack placement/transfer testing. This remains temporary debug-only startup scaffolding; it is not permanent new-game starting inventory and must be removed during P-03 closeout.
- Source change committed; Unity compilation, EditMode and Play Mode verification are pending.
