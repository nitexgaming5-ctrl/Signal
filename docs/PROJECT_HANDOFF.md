# Signal — current agent handoff

**Last reconciled:** 2026-11-27  
**Repository:** `nitexgaming5-ctrl/Signal`  
**Working branch:** `chore/collaboration-workflow-and-audit`  
**Do not modify:** `main`

Signal is a Unity 2D top-down urban RPG. Unity 6000.6.9f1; active scene `Assets/_Project/Scenes/HideoutSlice.unity`; architecture is `Signal.Core` (plain C# domain) + `Signal.Game` (Unity integration/UI/content).

## Read first

1. `AGENTS.md` — authority and workflow.
2. `docs/INDEX.md` — documentation map.
3. `docs/PROGRESS.md` — concise implementation and verification truth.
4. `docs/ROADMAP.md` — active sequence and next proposal.
5. The relevant task card and specialist references.

Historical chronology belongs in `docs/progress/LOG-2026-11.md`; do not read it wholesale unless needed.

## Direction and workflow

The user selected **Direction B — meaningful everyday livelihood**: repair, production, customers, services, relationships and a practical hideout. Character development was reprioritised ahead of neighbourhood expansion. Keep one active initiative at a time and avoid isolated systems.

**Gameplay mechanics must be explained and explicitly approved before implementation.** User expects substantial coherent work after a green light, accurate documentation, and no false test claims. Keep `docs/PROGRESS.md`, `docs/ROADMAP.md`, `docs/PROJECT_HANDOFF.md`, the active task card and monthly log reconciled.

## Verification snapshot

- **Latest full EditMode suite: 563/563 passed**, user-confirmed 2026-11-27 after P-04b usability follow-up.
- P-04a: 557/557 confirmed.
- P-04b initial assignment foundation: 562/562 confirmed; usability follow-up: 563/563 confirmed.
- P-03.7: 555/555 plus Unity compile and Play Mode acceptance confirmed; closed.
- B-06: 456/456 plus successful Play Mode repair/return/payment loop confirmed.
- Unity Editor is authoritative for compile, EditMode and Play Mode. Do not infer Play Mode acceptance for P-04b solely from the test count.

## Current implementation

### Player attributes / P-04a

Exactly three attributes: Health (primary/max HP), Armor and Heat Protection. Current HP remains separate in PlayerStats. F2 opens the Character panel; HUD shows current/max HP. Effective attributes combine equipment and skill modifiers. Temporary test fixtures only: `health_test` (+10 max Health/rank) and `heat_protection_test` (+1 Heat Protection/rank). Actual Repair/Production/Trading/Scavenging/Social effects and comprehensive milestone trigger wiring remain future work.

### Equipment / P-03

Seven slots: Head, Torso, Legs, Feet and Accessory 1–3; backpack uses an accessory slot. Backpack owns an 8×6 grid by stable item ID; B drops it and E recovers/re-equips the same item. Three persistent 1×1 pockets exist, with P3 hidden from ordinary-search enumeration. Stack split/merge and pocket-aware service integrations are in place. P-03.7 is closed at 555/555 with Play Mode acceptance.

A later cleanup removed automatic fresh-session debug backpack/cargo; F1 spawner remains. That cleanup still needs a separate fresh Unity verification (empty new game and F1 spawner). Do not reopen P-03.7. Wearable presentation is paused.

### Quickbar / P-04b — verified

- Six general-purpose slots, stable item-instance references only; never duplicate/copy physical items.
- One physical item can appear in at most one slot.
- Assignable from accessible backpack grid or any personal pocket; equipped clothing/accessories excluded.
- Tab inventory has no duplicate quickbar row. The single bottom HUD quickbar is the drag/drop target while Tab is open: inventory/pocket→HUD slot assigns/replaces, HUD slot→HUD slot swaps, and HUD slot→inventory unassigns. RMB Inspect / Equip to Quick Slot 1–6 remains.
- RMB reassignment moves the existing reference to the new slot.
- Semi-transparent pointer-following drag ghost.
- The only quickbar display is the always-visible bottom-centre six-slot HUD bar; it accepts drag/drop while Tab is open.
- Dropped-backpack references persist but are unavailable until the same backpack is recovered; pocket entries remain available.
- Save/load mapping persists and reconciles assignments.
- User confirmed **563/563** after these changes.

### P-04c active hand and controls — implementation committed, verification pending

- Core `PlayerQuickbar` owns selected-slot and holstered state. The held item is derived from the selected slot's existing GUID; inventory remains ownership authority.
- Mouse wheel up/down cycles next/previous through all six positions, including empty slots, with wraparound.
- Number keys 1–6 directly select slots 1–6. H toggles between holstered and unholstered without clearing the selected slot or assignment. Selecting an empty slot means empty hand.
- All three controls are ignored while any supported menu/panel is open.
- HUD slot clicks remain non-interactive, but while Tab is open the HUD slots are the drag/drop assignment surface. The HUD highlights the selected slot differently when active versus holstered.
- If a held item in the backpack grid becomes inaccessible because the backpack is dropped, the active hand holsters but the slot assignment remains. Pocket items remain accessible.
- F9/load resets the active hand to no selected slot and holstered; six assignments are restored as before.
- No consumable use, tool action, weapon attack, combat, targeting, durability, or new art is included.
- Seven Core P-04c regression tests plus one debug-loadout callback test have been added since the last user-confirmed 563/563 baseline. Expected current EditMode total: 571; verify in Unity rather than assuming it.
- Source commits: `c17a9f7e27d06caf6066d5fd010594d51d33fd85`, `3f8157afb5ca80e744ed83d9ea25922b33f99e50`, `e0ea41bdf2b11a30b4a99941f7153979fd04223e`.
- Next action: run Unity compile, all EditMode tests and the Play Mode acceptance checklist in the task card; do not mark P-04c complete until those pass.

## Project-wide constraints

- Core must not reference UnityEngine; dependencies flow Game → Core.
- Reuse canonical inventory, ownership, custody, transaction, account and save authorities.
- Preserve stable ScriptableObject IDs, item GUIDs and Unity .meta GUIDs.
- No new packages/dependencies without approval.
- Do not author new story cast/backstories until the main storyline/cast are designed.
- Art pause remains active: no new sprites, wearable overlays, wardrobe presentation or character-art exploration.
- Do not run `dotnet` or `msbuild` for this Unity project.
- B-07 Neighbourhood Expansion is paused while P-04 is active.


### Debug test loadout (2026-11-27)
- F1 Item Spawner now includes a one-click **TEST LOADOUT** action; it is opt-in and does not change fresh-session starting inventory.
- It equips a Canvas Backpack if the player has none, then adds Bread ×5, Fabric Roll ×5, Thread Spool ×3, one Work Jacket and one Basic Patch Kit through the normal debug spawn/inventory path. Existing equipped backpacks are reused. Known defect: a dropped backpack still occupies the single session-wide grid and blocks replacement; per-backpack storage isolation is pending.
- Stack quantities are added one item at a time through normal inventory rules, so expected stack groupings are Bread 5, Fabric Roll 3+2, Thread Spool 2+1. This deliberately gives both stack top-up and multi-stack cases.
- No quickbar assignments are overwritten. This loadout only seeds inventory; it does not implement consumable activation/use. Item-use remains a separate gameplay slice.
- Added an EditMode callback test for the loadout action. Unity compilation, tests and Play Mode verification are pending.
- Commits: `c70907a557d48b16c5b9fb0e857693481b27d341`, `c3a9f4cf05b7afa316727c6a60765a24bf556fd7`.


### Current follow-up — 2026-11-27
- User confirmed the quickbar/H-toggle work at **571/571**.
- Drag ghost z-order follow-up committed as `d980ebba7d53a3ffbfb602b74adb9593b1e652e4`: it is promoted to the last sibling on the overlay canvas during dragging so it appears above the HUD quickbar. Unity verification pending.
- Backpack bug diagnosis: `GameSession` uses one `_inventory` grid for every physical backpack. Dropping unregisters the grid but keeps the same reference as the session inventory; debug loadout refuses a replacement while a dropped bag exists. Do not merely remove that guard. Correct design is one distinct grid per backpack identity, active-grid switching on equip/drop, multiple independently recoverable dropped bags, and save/load persistence for every bag.
- Await user decision: if the player recovers an old dropped bag while wearing another, should this automatically drop the currently worn bag at the player's feet and swap, or require the player to drop the current bag first?
- No backpack lifecycle correction has been implemented yet; the storage identity registry foundation is now in place. Preserve the confirmed 571/571 baseline and re-run Unity tests after the pending fixes.


## Active follow-up — backpack isolation and swap (2026-11-27)
User approved automatic swap: when near dropped Backpack A while Backpack B is equipped, pressing E drops B at the player's feet and equips A; each backpack retains independent contents. Dropping a backpack must immediately revoke access to its contents. The debug TEST LOADOUT must be able to equip a fresh backpack while another is dropped. Current code still has a single `_droppedBackpackItem` and maps every backpack to the same `_inventory`; debug loadout blocks while one is dropped. Do not remove that guard without introducing independent per-instance storage and persistence. The quickbar drag ghost was moved to a dedicated override-sorting Canvas (sorting order 200) in commit `1971e800a08011e8f031ecd79fdd67cb56d3a17d`. Backpack correction remains open and requires save/load coverage plus Unity verification; last user-confirmed suite is 571/571.


## Global inventory isolation requirement (2026-11-27)
The user clarified that the approved backpack correction must enforce a broader invariant: **every inventory, world container and NPC pocket owns independent contents**. Treat this as a cross-cutting storage architecture task, not a backpack-only patch.

Required storage domains: each physical backpack (stable item-instance ID), each player pocket, hideout/home storage, each searchable/world container, each NPC/resident-owned pocket or carried inventory, each shop/business stock inventory, and each contract/service escrow inventory. One storage identity must map to one unique mutable `GridInventory` instance. Access/custody is separate from ownership; dropping or switching access must never retarget, merge, clear or duplicate contents. Cross-inventory movement only occurs through explicit validated atomic transfer operations.

Audit note: GameSession currently registers the same session-wide `_inventory` grid for a backpack, so the apparent PortableStorageBook mapping is not sufficient for independent backpack storage. GameSession also creates separate player storage, pockets, contact inventory, contract receiver inventory and per-searchable-container grids; all registration, service, UI, save/load and stack-lineage paths still need an aliasing audit. NPC pockets are a required domain; verify whether they currently exist as runtime inventories before describing them as implemented.

Persistence must be keyed by stable storage identity, with deterministic migration for legacy global/single-backpack save fields. Regression tests must check pairwise isolation and explicit transfers across player, backpack, pocket, world container, NPC, business and escrow inventories.

### 2026-11-27 — Storage identity registry foundation
- Added `InventoryStorageId` + `InventoryStorageRegistry` in Core. One stable typed storage identity maps to exactly one unique `GridInventory`; duplicate identities and shared grid instances are rejected. Reverse lookup supports auditing. Registry identity is separate from access, item ownership and economic custody.
- GameSession now registers the three player pockets, hideout storage, dirty-cash contact inventory, customer receiver/escrow inventory, all street searchable containers and all resident-owned searchable property containers.
- Added five EditMode Core tests covering stable IDs, duplicate/alias rejection and cross-grid isolation.
- Commits: `cd4b182015525429b1ebc97c22628a862b28ef89`, `bf4d54e9be82dfd61760528a11a1f6a3186f93d8`, `13ff69d98d661cd0bfeecff8ee9c9bb3a8cbf50a`, `bea52619e72a6ebe0f5e0ac4b50d3e4a4c481585`, `ffa503494a62593e0ffe99d4e57f06a569b57387`.
- Incomplete by design: backpack grids are not yet registered per backpack (the existing `_inventory` alias defect remains); multiple dropped backpack runtime items/visuals and per-backpack contents save/load are not implemented. Business stock inventories and NPC-owned pocket inventories are not currently wired as runtime grids. Do not remove the debug-loadout guard or claim global storage isolation complete yet.
- Unity compilation, EditMode tests and Play Mode verification are pending. Expected +5 EditMode tests. Check startup logs for duplicate/invalid storage identity errors.

