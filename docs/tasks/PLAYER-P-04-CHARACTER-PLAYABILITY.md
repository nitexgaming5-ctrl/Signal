# P-04 — Character Playability: Progression, Active Hand & Quickbar

**Status:** P-04a, P-04b and P-04c complete and user-verified. Follow-up drag-overlay fix and global storage-isolation initiative remain in progress.  
**Track:** Player Development  
**Priority:** Ahead of B-07 Neighbourhood Expansion.  
**Dependencies:** P-01 character foundation, P-02 attributes/skills, P-03 equipment/inventory/backpack.

## 1. Purpose

Make the player a coherent RPG character before expanding city geography: meaningful attributes/progression, reliable carried-item preparation, and one general-purpose quickbar that doubles as the active hand.

B-07 remains paused while P-04 is active.

## 2. Verified prior implementation

### P-04a — Attributes and development

- Exactly three attributes: **Health** (primary; determines maximum HP), **Armor**, **Heat Protection**.
- Current HP remains a separate PlayerStats resource; damage/healing behavior is unchanged.
- Effective attributes combine equipment modifiers and authored skill modifiers through the authoritative calculation path.
- F2 opens the player-facing Character panel; HUD shows current/max HP.
- Base attributes, skills and milestone ledger are persisted; load restores effective maximum Health before saved current HP.
- Temporary test fixtures: `health_test` (+10 maximum Health/rank) and `heat_protection_test` (+1 Heat Protection/rank). These are not final skill identities or balance.
- User verified **557/557** and confirmed the connections/functionality tested good.
- Actual Repair, Production, Trading, Scavenging and Social effects, plus comprehensive milestone trigger wiring, remain future work.

### P-04b — Six-slot assignment and visible HUD

**Complete; user verified 563/563 on 2026-11-27.**

- Exactly six general-purpose quickbar slots. Suitable carried items include consumables, tools, weapons, backpacks and other supported items.
- Quickbar entries are stable item-instance references only. They do not copy/move items or create a second owner. A physical item can occupy at most one slot.
- Eligible sources: accessible backpack grid or any of three personal pockets. Equipped clothing/accessories are excluded.
- Tab inventory has no separate quickbar row. Its inventory/pocket items can be dragged onto the single bottom HUD quickbar while Tab is open; dragging HUD slots onto each other swaps them, and dragging a HUD slot back into inventory unassigns it. RMB Inspect / Equip to Quick Slot 1–6 remains.
- Reassigning an already assigned item moves its reference from its old slot to the chosen slot. An occupied destination loses only its former quickbar reference.
- Drag ghost follows the pointer in screen space.
- The only six-slot bar is the always-visible bottom-centre HUD quickbar; it remains visible and interactive as a drag/drop target while Tab inventory is open.
- Dropped-backpack references persist but are unavailable until the same backpack is recovered. Pocket entries remain accessible.
- Save/load persists assignments and reconciles invalid/retired identities.

## 3. P-04c — Active hand and controls

<badge color="success">Complete — user verified</badge>

### Player-facing behavior

1. The selected quickbar slot is the item held in the player's hand.
2. Mouse wheel up selects the next slot; mouse wheel down selects the previous slot. Cycling wraps around and visits all six slots, including empty ones.
3. Number keys **1–6** directly select their matching quickbar slots.
4. **H** toggles between holstered and unholstered without clearing the selected slot or its quickbar assignment.
5. Selecting an empty slot means empty hand.
6. Mouse wheel, number keys and H are ignored while any supported gameplay/menu panel is open.
7. Clicking HUD slots does nothing. While Tab is open, however, the HUD slots are the sole quickbar drag/drop surface for assigning, replacing, swapping and unassigning references.
8. Selecting a slot only draws/holds its item. It never consumes or activates it.
9. Supported consumables require an intentional left-click to use in a later phase. Tools/weapons are held only; tool actions and weapon attacks are out of scope.

### Core and inventory authority

- The existing Core `PlayerQuickbar` owns selected-slot and holstered state.
- The held item is derived from the selected slot's existing stable item-instance GUID. No second held-item GUID or duplicate ownership is introduced.
- Selecting an empty slot produces no held item while retaining the selected slot.
- If a selected item is removed/retired, the active hand safely holsters as the reference is reconciled.
- If an item inside the backpack grid becomes inaccessible because the backpack is dropped, the active hand holsters but the quickbar reference remains assigned and unavailable until the backpack is recovered.
- Pocket-backed items remain accessible.
- H preserves the selected slot and toggles its held/holstered state; loading resets to no selected slot and holstered.

### Persistence

- The six quickbar assignments continue to use the existing save mapper.
- Active/holstered state is not persisted.
- F9/load restores the six assignments, then resets active-hand state to no selected slot and holstered. This avoids unexpectedly drawing an item after load.

### Implementation files

- `Assets/_Project/Core/PlayerQuickbar.cs` — selected-slot, held-item derivation, cycle, holster and accessibility reconciliation.
- `Assets/_Project/Game/GameSession.cs` — number-key, mouse-wheel and H input; menu gating; load reset; accessibility check.
- `Assets/_Project/Game/InventoryPanel.cs` — single HUD quickbar rendering, selected/active feedback and Tab-open drag/drop hit-testing.
- `Assets/_Project/Game/HudPanel.cs` — controls hint.
- `Assets/_Project/Tests/EditMode/Core/PlayerQuickbarTests.cs` — seven new regression tests, including reversible H toggling.

### Source commits

- `c17a9f7e27d06caf6066d5fd010594d51d33fd85` — P-04c active hand and quickbar controls.
- `3f8157afb5ca80e744ed83d9ea25922b33f99e50` — HUD controls hint.
- `e0ea41bdf2b11a30b4a99941f7153979fd04223e` — reset active hand on load.

## 4. P-04c verification — complete

The user confirmed Unity Play Mode behavior and **571/571 EditMode tests passed** on 2026-11-27. This closes P-04c. A later drag-overlay sorting correction still requires fresh Unity verification; the global storage-isolation initiative is tracked separately below.

### EditMode

- Active selection derives the held item from the selected slot.
- H toggles to holstered and back to unholstered while preserving the selected slot and all six assignments.
- Wheel cycling visits empty slots and wraps 6→1 and 1→6.
- Number keys 1–6 select the exact matching slot.
- Unassigning/removing an active item safely holsters.
- Inaccessible backpack-backed active item holsters without clearing its assignment.
- Save/load preserves assignments but resets selected slot and held item.

### Play Mode — user-confirmed acceptance

- Draw an item from the accessible backpack grid and each of the three pockets.
- Select slots using both number keys and wheel; verify wheel direction and wraparound.
- Verify empty slots produce empty hand and still occupy their wheel positions.
- Press H twice; verify the first press holsters and the second unholsters the same selected item without changing assignments.
- Open Tab, storage, workbench, retail, customer, NPC, debug and other supported panels; verify wheel, number keys and H do not change the active hand while a menu is open.
- Drop the backpack while holding an item from its grid: verify the hand holsters and the slot remains assigned/unavailable. Recover the same backpack and reselect.
- Verify pocket-backed references remain accessible when the backpack is dropped.
- Click HUD slots: verify clicks do nothing. Open Tab and verify inventory→HUD assignment, HUD→HUD swap, and HUD→inventory unassignment.
- F5/F9 round trip: assignments persist, but load begins with no selected slot and an empty/holstered hand.

## 5. Explicit exclusions

- No consumable activation/use implementation in this slice.
- No tool actions, weapon attacks, combat, targeting, durability, reload or action animations.
- No new sprites, wearable overlays or wardrobe presentation while the art pause is active.
- No city expansion, NPC cast or story content.
- No changes to inventory ownership, backpack/pocket rules, stack behavior or economy.
- No number-key alternatives beyond the approved number row 1–6.

## 6. Later work

- **P-04d:** remaining interface polish after P-04c is verified. The always-visible HUD bar, drag ghost and selected-slot feedback are already delivered; do not duplicate them.
- Actual livelihood skill effects and milestone event wiring require their own connected proposals.
- B-07 Neighbourhood Expansion remains paused until P-04 is complete or the user reprioritises it.


### Debug test loadout (2026-11-27)
- F1 Item Spawner now includes a one-click **TEST LOADOUT** action; it is opt-in and does not change fresh-session starting inventory.
- It equips a Canvas Backpack if the player has none, then adds Bread ×5, Fabric Roll ×5, Thread Spool ×3, one Work Jacket and one Basic Patch Kit through the normal debug spawn/inventory path. Existing equipped backpacks are reused; a dropped backpack must be recovered first.
- Stack quantities are added one item at a time through normal inventory rules, so expected stack groupings are Bread 5, Fabric Roll 3+2, Thread Spool 2+1. This deliberately gives both stack top-up and multi-stack cases.
- No quickbar assignments are overwritten. This loadout only seeds inventory; it does not implement consumable activation/use. Item-use remains a separate gameplay slice.
- Added an EditMode callback test for the loadout action. Unity compilation, tests and Play Mode verification are pending.
- Commits: `c70907a557d48b16c5b9fb0e857693481b27d341`, `c3a9f4cf05b7afa316727c6a60765a24bf556fd7`.


### 2026-11-27 follow-up — quickbar drag layer and backpack isolation
- The drag ghost now renders on a dedicated override-sorting Canvas at sorting order 200, above the HUD quickbar/inventory canvas. Commit: `1971e800a08011e8f031ecd79fdd67cb56d3a17d`.
- Backpack isolation remains open. User approved automatic swap when recovering a dropped bag while another is equipped. Required behavior: drop current bag at player's feet, equip recovered bag, preserve independent contents, and keep every dropped bag inaccessible until recovered. F1 TEST LOADOUT must be able to equip a new backpack while other bags are dropped.
- GameSession now allocates a distinct registry-backed grid per equipped backpack, switches `_inventory` to the active bag, and detaches to staging immediately on drop. F5/F9 serialize backpack contents by physical GUID and migrate legacy single-bag global items. Runtime still tracks one dropped backpack; multi-bag state/visuals and automatic swap remain open.
- Historical baseline at this point was **576/576**; subsequent allocator and persistence milestones brought the user-confirmed suite to **580/580**. Play Mode verification of backpack identity registration and the drag overlay correction remains required.


### Global storage identity foundation (2026-11-27)
- Added Core `InventoryStorageId` and `InventoryStorageRegistry`: one typed stable storage identity maps to one unique `GridInventory` instance; duplicate identities and grid aliasing are rejected. Reverse lookup supports audits. The registry does not grant access or determine item ownership/custody.
- GameSession registers its existing independent player pockets, hideout storage, contact inventory, customer receiver/escrow inventory, street searchable containers and resident-owned searchable property containers.
- Five new Core EditMode tests cover identity stability, duplicate/alias rejection and independent grid contents.
- Remaining active work: multiple dropped backpack items/visuals and approved automatic swap; TEST LOADOUT with other bags dropped; business stock and future NPC pocket integration; audit all service/UI/transfer access paths. Current runtime integration needs Unity compile/EditMode/Play Mode verification.
- Core allocator step: `PortableStorageBook` can now create a unique grid for each backpack GUID and bind it to `PlayerBackpack/<GUID>/contents`; canonical identity helper added. Two tests cover pairwise isolation and duplicate-registration rollback. Commits: `bd9ed8c515265120e6d5ad98e26bc1007fe9cd56`, `7d374b16991da37d864999c9f560ba7a0fe5b704`. Not yet wired into GameSession; legacy attach-existing-grid overload remains active.
- Source commits: `cd4b182015525429b1ebc97c22628a862b28ef89`, `bf4d54e9be82dfd61760528a11a1f6a3186f93d8`, `13ff69d98d661cd0bfeecff8ee9c9bb3a8cbf50a`, `bea52619e72a6ebe0f5e0ac4b50d3e4a4c481585`, `ffa503494a62593e0ffe99d4e57f06a569b57387`.
- Unity compile, full EditMode and Play Mode verification pending; expected five additional tests.


### 2026-11-27 — Global storage isolation implementation notes

- The user confirmed the full EditMode suite at 578/578 after the allocator step.
- Added Core persistence records and PortableBackpackStorageSaveMapper for per-physical-backpack grid capture/restore by stable backpack GUID. Each restored bag gets a new independent grid and canonical registry binding; duplicate bag IDs and duplicate item instance IDs are rejected.
- Added precise registry unregistration and a PortableStorageBook.Clear(registry) path so backpack restore can clear stale backpack mappings without deleting unrelated storage registrations.
- Two new tests cover two-bag round-trip isolation/dimensions and duplicate identity rejection. Expected suite count: 580; Unity verification pending.
- Commits: 2462112a32e88de815f53b0305ce3b4cf1bf63e5, 8156a03390ab1d2184f813530f34b229e783d7cb, 8ff1da85acca6de1b727f2d9dcd734eea709285d, 68eb632cbd4f46b85dbe88751335c2cdd6aa5369, 34330e52abf2c30b7b95314565284eda135db6c4.
- GameSession now wires the mapper into F5/F9 using a separate staging inventory, avoiding duplicate serialization. The single dropped-backpack runtime slot and debug-loadout restriction remain in place.


### Latest storage verification — 2026-11-27
The user confirmed **580/580** after adding independent per-backpack persistence round-trip and duplicate-identity tests. Core storage allocation and persistence primitives are verified. The runtime still aliases the equipped backpack to GameSession's session-wide `_inventory` and tracks only one dropped backpack. Next work is active-grid switching plus UI/service rebinding, followed by multi-bag swap and GameSession save/load integration. Do not mark global storage independence complete.


### 2026-11-27 — GameSession active-grid integration

- Each newly equipped physical backpack now receives a distinct grid via `PortableStorageBook.TryRegister(Guid,width,height,registry)`. The active `_inventory` switches to that grid only after allocation and registry/economic registration succeed.
- Dropping switches `_inventory` immediately to the separate staging grid; the original bag grid remains in `PortableStorageBook` and `InventoryStorageRegistry` but is excluded from all accessible carry inventory lists.
- Rebound InventoryPanel, StoragePanel, RetailPanel, DirtyCashContactPanel, CustomerContractPanel, WorkbenchWorkspace and MiloParcelOpportunityController when the active grid changes.
- Quickbar identity reconciliation now searches all registered backpack grids, preserving assignments while a bag is dropped. Active-hand accessibility still resolves only current carried inventories and holsters inaccessible bag items.
- F5/F9 now use the per-backpack storage mapper and reserve the legacy `items` path for staging. Legacy single-backpack saves deterministically adopt their old global items grid as the bag's private contents.
- Added one registry cleanup regression test. Expected suite: **581**; latest user-confirmed result remains **580/580** pending Unity verification.
- Commits: `3ef8447809dba93b58b67f210b8bdd9287f17b54`, `270d4427fb84a9ae7822c6bb34590403758a150a`, `7aa3f78c6e7ae7faf2872de91188e50da4f1fb61`, `cb2b3fe6a8cee57b258906601cc0fda1010e4d15`, `523b7199c3506f4de2609b0d196091c943bf6238`, `e0e44acb1cf06333b6f77cf9ab31e93b34af5245`, `24d34d773b8e6287103844ac0641f9299a95329a`.
