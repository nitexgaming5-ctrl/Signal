# Signal — current agent handoff

**Last reconciled:** 2026-11-26  
**Repository:** `nitexgaming5-ctrl/Signal`  
**Working branch:** `chore/collaboration-workflow-and-audit`  
**Do not modify:** `main`

Signal is a Unity 2D top-down urban RPG. Unity version is 6000.6.9f1; active scene is `Assets/_Project/Scenes/HideoutSlice.unity`. Active architecture is `Signal.Core` (plain C# domain logic) + `Signal.Game` (Unity integration, UI and authored assets).

## Read first
1. `AGENTS.md` — authority, safeguards and workflow.
2. `docs/INDEX.md` — game direction and documentation map.
3. `docs/PROGRESS.md` — implementation, verification and next action.
4. `docs/ROADMAP.md` — active sequence.
5. The task card and specialist references for the assigned work.

Do not read the entire historical log by default. Use it to resolve chronology or audit regressions.

## Game direction
The user selected **Direction B — meaningful everyday livelihood**: repair, production, customer jobs, services, relationships and a practical hideout. Legal and illegal activity coexist in one city. Avoid disconnected systems.

No additional story cast, personalities or backstories until the main storyline/cast is designed. Rhea, Milo and the existing Dirty Cash contact remain framework/test residents.

## Current implementation snapshot

### Livelihood B-01 to B-06
Implemented. B-06 Customer Repair Orders is user-verified: Milo entrusts a Worn Work Jacket, the player repairs and returns the exact item, and 35 CARD is paid through a separate service-payment ledger. Ownership remains Milo's while the player has custody. Return/payment is atomic and replay-protected. User confirmed 456/456 and the Play Mode loop on 2026-11-25.

### Player Development
- **P-01:** modular Body_A player renderer, directional Idle/Walk and local asset importer; user imported the supplied pack and confirmed the enlarged character.
- **P-02:** Armor and Heat Protection only; fixed starting skill points, milestone awards, free respecialisation, save mapping and F2 panel. User confirmed 467/467 after the implementation slice. Skill-effect and milestone-trigger integrations remain future work.
- **P-03:** equipment, backpack and personal-pocket foundation substantially implemented; see below.

### P-03 current state
- Seven slots: Head, Torso, Legs, Feet, Accessory 1–3; backpack occupies one generic accessory slot.
- Exact physical item identity/state/ownership preserved when equipping and unequipping.
- Backpack owns its 8×6 grid by stable item-instance ID. B drops it; E recovers/re-equips the same item. Dropped contents stay inside and inaccessible.
- F5/F9 persists equipped/dropped state, dropped position and contents.
- Three persistent 1×1 pockets: P1/P2 ordinary; P3 hidden from ordinary-search enumeration and visibly labelled HIDDEN.
- Manual pocket moves target the exact selected slot. Pocket-to-pocket move/swap is supported.
- Automatic item receipts choose accessible backpack capacity first, then first suitable empty pocket P1→P2→P3. Oversized/rotated items cannot use pockets.
- Retail SELL, Dirty Cash selection, Home Storage, Searchable Container, repair, multi-grid production, customer flows and GameSession receipt paths have been audited/integrated for accessible backpack/pocket inventory. Dropped cargo must never be exposed.
- Temporary fresh-session debug loadout provides backpack + repair/production test cargo. It is not intended new-game inventory and must remain until the user finishes acceptance testing.

**P-03 is not closed yet.** Current work is non-art verification/acceptance and eventual temporary debug-loadout cleanup.

## Verification truth
- Latest user-confirmed full EditMode result: **504/504** after the Play Mode-only guard for temporary NPC visual-test residents.
- One automatic-pocket-routing regression test was committed afterward. Expected current suite: **505**; not yet user-confirmed. Run the complete suite in Unity and inspect the XML before claiming a pass.
- P-03 service integration has been source-audited, but full Play Mode acceptance remains outstanding.
- Do not claim compilation, tests or Play Mode success unless the user/current Unity tooling confirms it.
- Do not run `dotnet` or `msbuild` for this Unity project.

## Explicit art pause
The user said to stop sprite work and return to roadmap/game development. Do not:
- create or regenerate player/NPC sprites or sprite templates;
- implement clothing/wearable sprite overlays;
- build a wardrobe preview/panel;
- continue character-art exploration or sprite import work.

P-03.6 Wearable Presentation is parked as historical preparation, not an active dependency. Existing imported assets and already-committed temporary visual tests should not be removed or altered as part of this documentation handoff.

## Next actions
1. Pull the active branch and run the current full EditMode suite (expected 506); inspect the XML and fix regressions.
2. Complete P-03 Play Mode matrix: fresh debug loadout; equip/drop/recover; save/load while equipped and dropped; P1/P2/P3 moves/swaps; P3 ordinary-search exclusion; service interactions with items split across backpack/pockets; ensure dropped cargo is inaccessible.
3. Keep temporary repair/production cargo until the user confirms acceptance. Then remove only automatic fresh-session cargo seeding; retain F1 debug item spawner.
4. Reconcile P-03 closeout status and then prepare the detailed B-07 Neighbourhood Expansion task specification. Do not start B-07 code until specification and any new mechanics are approved.

## Technical safeguards
- Core must not reference UnityEngine; dependencies flow Game → Core.
- Reuse canonical inventory, ownership, custody, transaction, account and save authorities.
- Preserve stable ScriptableObject IDs, item GUIDs and Unity .meta GUIDs.
- Avoid case-collision paths `Content/Items/` and `Content/items/`; use a distinct folder until safely reconciled in Unity.
- Build 0.01 saves are unversioned; incompatible experimental saves may be discarded/recreated.
- Unity Editor is the authority for compile/tests/Play Mode.


## Latest P-03 regression fix — dropped backpack isolation (2026-11-26)

A user-reproduced glitch occurred after pressing B to drop the backpack and opening a resident wardrobe or street searchable container. Root cause was a UI reference leak: SearchableContainerPanel hid the backpack grid but retained and queried the real _inventory grid. The panel now substitutes an isolated empty 1×1 grid when backpack access is false and disables backpack-grid pointer hit-testing. GameSession passes only accessible inventory references to searchable and Dirty Cash panels; DirtyCashContactPanel independently normalizes inaccessible state. Pockets remain usable and incoming loot still uses the normal capacity selector. The real dropped backpack storage and cargo are not modified.

Source commits: e8c57416a279af7bc6d0acaa69ba5e421a98d87b, c61532aad7e8102872c622e1beb9ed7f6bb10283, 73be827f6382cb92d0dddbe66bed9b955c79e269, a037dca3ca6ae61526ee065b0fd4c5bab987024b.

**Not yet verified in Unity.** Next action: pull, run the full EditMode suite, then Play Mode test B-drop → Rhea wardrobe → street bin/container → recover backpack and verify exact original contents. Do not close P-03 until this passes.


## Latest P-03 addition — searchable-container pocket drag/drop (2026-11-26)

SearchableContainerPanel previously rendered pocket contents but only implemented container→backpack dragging. It now supports container→exact pocket, pocket→exact container cell, pocket↔backpack, and backpack rearrangement. Container↔pocket transfers use AssetTransferService, preserving identity/state and updating ownership/custody/transaction history. Theft outcomes remain routed through the existing resident-property logic. Two EditMode regression tests were added.

User-confirmed suite baseline before these tests: **504/504**. Expected current total: **506**, not yet run in Unity. Source commits: 2224a3baffffb3f7dfdc5c0e7aa0dffba10fdbb3, f35ef49ce7dca66707ffc3ad243788fece0aa3c8, 9e4c20fe275562c3249ce8aef9551cc0b2653fe9, 46d34a89bcb1336f4c1a628f72b1e1dcd7bf551c, ae853be9e0d1ce8c83ec6c3446cb50937ac8ccb0, ee7850dab68dc0eb2ca06cd52e555690414db7e7.

Next gate: run Unity tests and Play Mode acceptance in Rhea's wardrobe and a street searchable container. Test both transfer directions, exact pocket selection, occupied/oversized rejection, and repeat with backpack dropped; then recover and confirm original cargo remains intact.


### Latest P-03 searchable-container interaction update (2026-11-26)

User confirmed **506/506** on the previous revision. The shared searchable-container panel now supports:
- Pocket→pocket moves and swaps while a container is open.
- Repositioning already-discovered items within the same container grid.
- Backpack→exact-container-cell placement, with ownership/custody handled by AssetTransferService.

Three new EditMode regression tests bring the expected suite total to **509**; Unity verification is pending. Do not claim 509/509 until the user runs it. Commits: 6ad663c32d77de09527bb6ecd49740270ded1596, 924f7cbc8032266809420df8b361954fd7462d84, 05f03a9d88e580ffac5f5e5f106bea6efe60d173.

Next: run EditMode (expected 509), then Play Mode test pocket moves/swaps, container reorganisation, backpack→container and pocket→container in both Rhea's wardrobe and a street container. Repeat with the backpack dropped and verify the dropped bag's contents remain isolated.


## Current checkpoint — Inventory & Grid Architecture Audit (2026-11-26)

User approved a stability audit before further major development. The audit is deliberately conservative: preserve gameplay behaviour and fix only concrete defects or missing regression coverage.

Phase 1 code hardening is implemented. Cross-inventory transfers and compound inventory operations now publish Changed events only after the complete operation has committed. Silent mutation/restore primitives are used by ordinary transfers, ownership transfers, production, repair, crafting, customer contracts, repair orders, Dirty Cash, retail purchases, shop sales and equipment equip/unequip. Rollbacks no longer announce transient failed state. ItemType validity is enforced at grid boundaries, and inventory snapshots reject restoration into grids with mismatched dimensions.

Thirteen Phase 1 tests were added, plus three searchable-container tests from the immediately preceding follow-up. Last explicit numeric user-reported baseline: **506/506**. Expected current total: **522**, pending Unity verification. Do not claim 522 passed until the user runs it.

Task card: docs/tasks/INVENTORY-GRID-AUDIT.md.

Next: Phase 2 persistence and identity reconciliation (save/load ordering, duplicate stable IDs across inventories/equipment, ownership vs physical location, equipped/dropped backpack recovery); then Phase 3 UI drag/drop math and Phase 4 remaining integration mutation sweep. No Unity compile/test or Play Mode run was performed by the assistant.
