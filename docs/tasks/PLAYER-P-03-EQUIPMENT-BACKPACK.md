# P-03 — Equipment, Wearables & Portable Backpack

**Status (2026-11-26):** Equipment, backpack lifecycle, three pockets, hidden-pocket visibility, pocket transfers/swaps and pocket-aware service integration are implemented in source. Non-art verification and closeout remain. Wearable presentation is paused by explicit user direction; it is not a P-03 completion dependency while art is paused.
**Track:** Player Development  
**Dependencies:** P-01 modular player foundation; P-02 Armor/Heat Protection attributes.

## Player outcome

The player equips physical clothing/protection/accessories and carries goods through a physical backpack plus three personal pockets. The backpack remains a distinct physical item: its contents stay attached to its stable instance when equipped, dropped or recovered.

## Approved equipment layout

Seven slots:
| Slot | Intended category |
|---|---|
| Head | Hats, helmets, headwear |
| Torso | Shirts, jackets, body armor |
| Legs | Trousers and legwear |
| Feet | Shoes and boots |
| Accessory 1 | Generic accessory; backpack is compatible |
| Accessory 2 | Generic accessory |
| Accessory 3 | Generic accessory |

Accessories are generic, not hardcoded categories. Backpack consumes one accessory slot.

## Approved binding and attribute rules

1. Each equipped item is one exact physical instance; stacked quantities cannot be equipped as one item.
2. Equip/unequip preserves ItemInstanceId, ItemType, ItemInstanceState and ownership.
3. Custody and ownership remain separate. Equipping a resident-owned item does not transfer ownership.
4. One item per slot; one physical item cannot occupy multiple slots.
5. Failed transfers leave source/destination unchanged.
6. Effective Armor and Heat Protection equal base attributes plus equipped modifiers, safely clamped. No combat/weather effects are implied by this task.
7. Backpack contents belong to the backpack's stable instance ID, not to the player inventory or equipment slot.
8. Dropping a backpack leaves all cargo inside it and inaccessible until recovery.
9. No weight, movement/pursuit penalty or automatic backpack dropping without separate approval.

## Approved carry model

- Without an equipped backpack, the player has exactly three independent 1×1 personal pockets, each holding one stack of one 1×1 item.
- P1 and P2 are ordinary pockets.
- P3 is the hidden pocket, visually tinted/labelled HIDDEN and excluded from ordinary-search enumeration. The player can still access and move its item.
- Advanced scanners/dogs/thorough searches are not specified by this task. Do not invent those mechanics.
- With the Canvas Backpack equipped, its 8×6 grid is accessible.
- Dropping the backpack hides/blocks access to that grid; the three personal pockets remain available.
- Recovering the dropped backpack restores access to the same grid and exact cargo.
- Manual pocket transfers are player-directed and target the exact selected pocket/cell.
- Automatic receipts choose accessible backpack capacity first; if the entire incoming item cannot fit, use the first suitable empty pocket P1 → P2 → P3. No prompt. Oversized/rotated items cannot use 1×1 pockets. Reject safely if no destination exists.

## Implemented foundations

### Core
- `PlayerEquipmentLoadout`: seven slots, exact-instance equip/unequip and modifier aggregation.
- `PortableStorageBook`: stable backpack instance ID → independent contents grid.
- `PortableBackpackWorldBook`: Equipped/Dropped status, dropped world position and identity-safe transitions.
- `PlayerPocketBook`: three independent 1×1 inventories, exact transfers, pocket-to-pocket move/swap and ordinary-search visibility API.
- Save mappers/DTOs preserve equipment, backpack world state and pocket item identity/state/ownership.

### Game/UI
- ItemDefinition equipment metadata supports compatible slot, Armor/Heat Protection modifiers, portable-backpack marker and grid dimensions.
- Hover + E equips; keys 1–7 unequip ordinary slots; B drops backpack; E near bag recovers it.
- Inventory panel shows equipment slots, backpack storage and P1/P2/P3. P3 is visibly marked HIDDEN.
- F5/F9 persists equipped/dropped backpack state and position, contents, equipment and pockets.
- Temporary backpack ground silhouette is placeholder presentation.
- No-backpack NPC, mission and world interactions remain usable; physical receipts use capacity checks.
- Pocket-aware service paths include Retail SELL, Dirty Cash selection, Home Storage, Searchable Container, workbench repair, multi-grid production, customer flows and GameSession receipt/transfer routing. Dropped cargo is excluded from accessible inventory views.

## Current verification truth

- Latest user-confirmed full EditMode suite: **504/504** after the Play Mode-only guard for temporary NPC visual-test residents.
- One automatic-pocket-routing regression test was committed afterward. Expected current suite is **505**, but this has not been confirmed in Unity.
- Source audit for pocket-aware service integration is complete.
- Full current-build Play Mode acceptance remains outstanding. Do not call P-03 complete until the user confirms the current EditMode suite and acceptance matrix below.

## Remaining closeout work — non-art only

### 1. Current EditMode verification
- Run all tests in Unity.
- Expected count: 505.
- Inspect the XML; distinguish total/passed/failed and report exact results.
- Fix any regression before proceeding.

### 2. Backpack and pocket Play Mode matrix
- Fresh session: verify temporary debug backpack/cargo appears as intended.
- Equip backpack; verify the exact cargo and character/backpack placeholder.
- Drop with B and UI button; confirm backpack appears at the world position and the 8×6 grid is inaccessible.
- Confirm P1/P2/P3 remain accessible while bag is dropped.
- Recover with E; confirm same backpack identity and cargo return.
- Save/load while equipped and while dropped; verify position/status and exact cargo persist with no duplication.
- Manual drag/drop into each exact pocket; move pocket item to exact backpack cell; move between pockets and swap occupied pockets.
- Verify P3 remains player-accessible but is excluded from ordinary-search enumeration.
- Exercise retail, Dirty Cash, home storage, searchable container, repair, production and customer flows with items split across accessible grids.
- Verify dropped backpack contents are never visible/usable in panels or transactions.
- Verify automatic incoming routing: backpack first, then first suitable pocket P1→P2→P3; no prompt.
- Verify social/dialogue/mission/world navigation remains usable without a backpack.

### 3. Debug-loadout cleanup
- Keep temporary fresh-session repair/production cargo until the user finishes acceptance.
- At final closeout remove only automatic fresh-session backpack/cargo seeding.
- Retain the deliberate F1 debug item spawner.
- Do not alter authored item definitions or intended new-game inventory without a separate design decision.

## Explicitly paused — P-03.6 wearable presentation

The user has paused sprite creation and wearable/wardrobe presentation. P-03.6 is parked in [its task card](PLAYER-P-03-6-WEARABLE-PRESENTATION.md). Do not create sprites/templates, implement clothing overlays, build a wardrobe preview or perform art-import work unless explicitly re-authorised. Visual wearables are not required to close the current non-art P-03 scope.

## Exit criteria

- Current full EditMode suite passes in Unity and the result is user-confirmed.
- Backpack lifecycle, save/load, pockets, hidden-pocket contract and pocket-aware service interactions pass the Play Mode matrix.
- Debug fresh-session cargo is removed only after acceptance; F1 spawner remains.
- PROGRESS, ROADMAP, this card and PROJECT_HANDOFF agree on final status and verification.
- P-03 is marked complete only after those conditions are met.
