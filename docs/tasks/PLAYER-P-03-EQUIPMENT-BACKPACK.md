# P-03 — Equipment, Wearables & Portable Backpack

**Status:** Equipped-backpack integration verified by the user at 478/478. P-03.3 Core world-state foundation committed with seven focused tests; Unity verification pending. Physical Game drop/pickup and pocket switching remain the next integration work.
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

- Final no-backpack state: only 2–3 small personal pockets. This is the intended end-state, not implemented yet.
- With a backpack equipped: the existing large tabbed grid is the backpack's contents.
- Dropping the backpack removes access to that large grid from the player; the physical bag retains every item. Recovering/equipping it restores access to the same contents.
- For the current implementation/test pass, the player grid is temporarily enlarged to 8×6. It remains accessible without a backpack until the physical drop/pickup sub-phase is completed. Do not mistake this test convenience for final inventory design.

## Architecture

### Core (implemented first slice)
- EquipmentSlot: Head, Torso, Legs, Feet, Accessory1, Accessory2, Accessory3.
- EquippedItem: exact PlacedItem plus authored Armor/Heat Protection modifiers.
- PlayerEquipmentLoadout: validated equip/unequip, slot uniqueness and effective-attribute aggregation.
- PortableStorageBook: maps stable backpack item instance IDs to independent GridInventory contents; refuses duplicate identities and refuses to discard a non-empty backpack container.

### Implemented in the second slice
- `ItemDefinition` now has optional equipment metadata: compatible slot, Armor/Heat Protection modifiers, backpack flag and backpack grid dimensions.
- `GameSession` loads the three prototype items from `Resources/SurvivalItems/Definitions` through the existing Resources catalogue discovery.
- Inventory interaction: hover an item and press E to equip; press 1–7 to unequip the corresponding non-backpack slot. The backpack cannot yet be unequipped because the physical drop flow is the next sub-phase.
- The prototype player inventory is expanded to 8×6 for testing. Equipping the Canvas Backpack binds that exact backpack instance ID to the existing inventory grid without copying any contents.
- F5/F9 now capture/restore equipped item identity, type, slot, modifiers, item state, owner actor and backpack association. The current grid's items were already handled by the existing inventory save path.
- A provisional backpack silhouette is shown behind the character when equipped; this is a placeholder, not final backpack art.

### P-03.3 Core lifecycle foundation (2026-11-25)

- Added `PortableBackpackWorldBook`, which records each registered backpack as Equipped or Dropped and stores the dropped world coordinates.
- Registration requires an existing `PortableStorageBook` contents grid keyed by the exact same backpack instance ID. Drop/recover transitions never copy, clear or reassign the contents grid.
- Added `SavedPortableBackpack` and `PortableBackpackWorldSaveMapper` for serializing/restoring world state. Restore rejects malformed IDs, duplicate records, invalid statuses, non-finite coordinates and backpacks whose contents identity is unavailable.
- Added seven focused EditMode tests for registration preconditions, exact identity/content continuity, valid/invalid transitions, duplicate registration, dropped position save/restore, malformed records and equipped-coordinate normalization.
- This is a Core data/persistence foundation only. It is not yet wired into GameSession, world object spawning, interaction, inventory access switching or F5/F9 orchestration. Do not describe physical drop/pickup as playable yet.

### Remaining integration
- Enforce 2–3 personal pockets when no backpack is equipped and make the 8×6 grid accessible only through the backpack.
- Expand the new character sidebar into a fully functional equipment/pocket screen. Seven equipment slots are displayed; the three pocket cells are currently visual placeholders and still need their own small inventory model and item-transfer controls.
- Finish the GameSession backpack lifecycle integration by validating the new B/UI drop and E pickup flow in Unity. The Core world-state book, saved dropped item identity, F5/F9 state, world placeholder and auto-equip recovery are now wired. Reuse the existing custody/ownership authority; do not create a parallel loot-container system.
- Add actual wearable sprite overlays for head/torso/legs/feet and equipment variants. The supplied Body_A pack is only a base body; do not invent a full wardrobe from missing sprites.
- Author more clothing/armor content after the core loop is validated.

## Next Play Mode test game — Backpack lifecycle acceptance

Run this after the current GameSession/world-object integration is pulled and the expected 486 EditMode tests are green.

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
- While the backpack is dropped, the backpack grid is hidden and pointer item interaction with that grid is blocked. The player can still move to the bag and press E to recover it. While dropped, GameSession currently applies a conservative temporary gate: normal E/F interactions that could consume, receive or inspect items are blocked until the backpack is recovered. This is a safety fallback, not the final pocket-based interaction model.
- F5 captures the backpack world state and, when dropped, the exact item definition, stable instance ID, owner, modifiers and item state. F9 restores the bag at its saved world position and respawns its ground placeholder; recovery equips it.
- Added a focused Core test for extracting an equipped item without copying it into an inventory. The user's confirmed baseline was 485/485; **486/486 is expected after this test**, pending Unity compilation and EditMode execution. No Unity or Play Mode result is claimed by this repository pass.
- Next user test: equip → place cargo → B/button drop → confirm grid hidden → E pickup/auto-equip → F5 while dropped → F9 → recover; then repeat F5/F9 while equipped. Check cargo identity/state and ownership throughout.
