# P-03 — Equipment, Wearables & Portable Backpack

**Status:** P-03.3 backpack drop/recovery and fresh-start cleanup are user-verified. User confirmed 486/486 after P-03.4a. P-03.4b now implements three persistent 1×1 pockets, UI transfer controls, and pocket-aware routing for selected item-receive flows. Eight new Core tests are added; expected total 494 is pending Unity execution. P-03 remains active.
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
