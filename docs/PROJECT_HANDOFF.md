# Signal — current agent handoff

**Last reconciled:** 2026-11-27  
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
- Automatic fresh-session debug backpack/cargo seeding has now been removed. Fresh games should start without a backpack or test cargo; the F1 debug item spawner remains the deliberate test path.

**P-03.7 is closed and user-verified.** The user confirmed Unity compilation, **555/555 EditMode tests**, no duplicate stack IDs after split/merge/save, no access to dropped-backpack contents, correct F5/F9 restoration for equipped and dropped backpacks, and the Play Mode acceptance matrix. A later, separate cleanup removed automatic debug seeding from GameSession and retained the F1 spawner; that cleanup alone needs a fresh Unity compile/full EditMode run and Play Mode confirmation of empty fresh start + F1 spawner. This does not reopen P-03.7. Wearable presentation remains paused.

## Verification truth
- Latest user-confirmed full EditMode result: **552/552** after P-03.7c.
- P-03.7d added Core regressions for theft splitting, same-grid partial merging and full pocket-to-pocket merging. Expected current suite: **555**; not yet user-confirmed. Run the complete suite in Unity and inspect the XML before claiming a pass.
- P-03.7d searchable-container controls and service quantity audit are source-implemented; Play Mode acceptance remains outstanding.
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
1. P-04 Character Playability is the active priority: meaningful skill effects, dedicated weapon/tool equipment and a quick inventory bar. See `docs/tasks/PLAYER-P-04-CHARACTER-PLAYABILITY.md`.
2. Obtain explicit approval of equipment layout, tool actions, weapon/combat scope, quickbar controls and skill effects before implementing those mechanics.
3. B-07 Neighbourhood Expansion is paused.
4. Separately, the automatic-debug-loadout cleanup still needs Unity compile/full EditMode and fresh-empty-start/F1-spawner verification. This maintenance check does not reopen P-03.7.

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


### Inventory & Grid Audit update — Phases 2–3

Persistence hardening now rejects duplicate stable item IDs across player inventory, home storage, contact inventory and contract receiver during SaveMapper restore. Orphaned ownership records are skipped. Equipment/dropped backpack restore checks the already-restored ownership book; pocket restore already did so; searchable-container restore now checks existing ownership and uses a shared seen-ID set across all saved containers.

UI/drag-math hardening: InventoryPanel no longer considers an inaccessible backpack grid a valid drag destination, while the drag ghost still tracks the pointer when the grid is hidden. InventoryDragMath rejects infinite/overflowed cell geometry and invalid rotation footprints.

Added four persistence tests and three drag-math tests. Together with the prior P-03 and Phase 1 tests, there are 23 tests beyond the last explicit user-reported 506/506 baseline. Expected current total **529**, pending Unity verification. No 529 pass is claimed.

Task card: docs/tasks/INVENTORY-GRID-AUDIT.md. Phase 4 remains: sweep remaining Core/Game mutation call sites and save/load paths, then full Unity EditMode and Play Mode verification.


### Inventory audit — latest persistence guardrails and source sweep

SaveMapper rejects aliased inventory-role arguments before clearing any grid. SearchableContainerSaveMapper skips duplicate destination container IDs/references. Two regression tests cover these cases.

The remaining direct Game-layer inventory mutations were reviewed and classified as controlled creation/bootstrap/debug paths (container loot, authored resident-property stock, Milo parcel, debug spawner, legacy demo). Interactive cross-grid UI paths use transfer services; same-grid rearrangement uses GridInventory.

Expected current suite: **531** (25 tests added beyond the last explicit user-reported 506/506 baseline), pending Unity verification. No 531 pass is claimed. Next run the full suite, then Play Mode acceptance across F5/F9, equipped/dropped backpack, pockets, searchable containers, storage, repair, production, retail, sales and contracts.


### Phase 4 follow-up — GameSession item creation paths

- Player/customer item seeding now uses silent placement, registers the item owner, then publishes the inventory change. Failed ownership registration removes the silent placement.
- Debug spawner item creation registers ownership before the pocket/inventory change is published; failed pocket insertion removes the provisional owner record.
- Debug starting backpack creation and Milo parcel backpack placement now avoid publishing an item before the associated placement/opportunity state is established.
- These are source-level changes only and add no new gameplay rules. Expected EditMode suite remains **531**, pending Unity verification.


### Compile correction — cross-assembly transaction API visibility and observer test discards (2026-11-26)

The first Unity compile after the audit exposed two integration issues:
- GameSession and MiloParcelOpportunityController are in a separate assembly from Signal.Core, so they could not call GridInventory's internal silent mutation/notification methods. The transaction-facing methods `PlaceSilently`, `TryRemoveItemInstanceSilently(Guid)`, `AddSilently` and `NotifyChanged` are now public with XML documentation describing the required commit/notification contract. Quantity-specific removal and lower-level transfer/snapshot helpers remain internal.
- In several Changed-event tests, the lambda parameter was named `_`. In C#, that identifier shadowed the intended `out _` discard, causing the compiler to try passing the event's GridInventory argument as an out PlacedItem/EconomicActorId. Those event parameters now use a named identifier, restoring true discard semantics.

This addresses the compiler diagnostics supplied by the user. A fresh Unity compile and test run is still required; no new test pass is claimed.


## 2026-11-27 — Current handoff: P-03.7 Stack System

- Inventory & Grid Architecture Audit is user-verified complete: **531/531 EditMode tests passed**, and the user confirmed Play Mode acceptance passed.
- Active task: `docs/tasks/PLAYER-P-03-7-STACK-SYSTEM.md`.
- Approved controls: LMB drag whole stack; Shift+LMB quick-transfer; RMB inspect; Shift+RMB split half; Ctrl+RMB split one.
- Existing code already has `ItemType.MaxStack`, compatible-state stacking in `GridInventory.Add`, `ItemInstanceState`, snapshots and stable stack IDs. The current cross-inventory transfer path moves whole stacks only; no complete player-facing split/merge/quantity-transfer system exists yet.
- Identity contract: full move preserves ID; split preserves source ID and creates a fresh ID for the split portion; merge preserves destination ID, retires emptied source ID and records persistent lineage. Merge compatibility requires same definition and exact `ItemInstanceState`.
- Ownership is per stack ID. Never merge across actor ownership by simply discarding an ID; implement the whole ownership/lineage operation atomically or reject ambiguous cases.
- Start with P-03.7a Core operations + regression tests, then P-03.7b ownership-aware transfer, P-03.7c UI and P-03.7d universal integrations. Do not change authored `maxStack` values or start wearable sprite work.


## P-03.7a — Core stack operations implemented (Unity verification pending)

- Added `StackOperations.CanMerge`, atomic exact-quantity `Split` and capacity-limited `Merge` in `Signal.Core`.
- Split retains the original stable ID on the remainder and gives the new portion a fresh GUID; item state is copied exactly.
- Merge retains the destination stable ID, reduces/removes the source stack, and records source→destination lineage in `StackLineageBook`. Duplicate operation IDs reject and roll back the physical mutation.
- Merge compatibility requires the same ItemType and complete ItemInstanceState; MaxStack limits are enforced.
- Added nine Core tests covering split, merge, state mismatch, partial capacity, quantity conservation, observer timing and duplicate operation protection.
- These are physical Core primitives only. Ownership/economic transaction integration and persistence are deliberately not wired yet; Game/UI must not call them directly until P-03.7b supplies the ownership-aware orchestration.
- Commits: `513de74e7b92cad36a0baa5d7cc0d7f2852cabd8`, `b2a47e4c2978f8018ff5ee181f37c2c87f6a8af6`, `afffd44646322173d217b086a3ce76e6a7343580`.
- No Unity compile or test result is claimed. Next: review and harden P-03.7a, then integrate ownership and persistent lineage in P-03.7b before UI work.


- Compile fix after the first Unity pass: `StackMergeRecord`'s constructor was `internal`, but the EditMode test assembly constructs a record to verify duplicate-operation rejection. Made the constructor public; `StackLineageBook.TryRecordMerge` remains the validation boundary and rejects empty IDs, self-merges, invalid quantities and duplicate operation IDs. Commit: `fcb70dc75c5bc7f77be1c8fd0d3fb0943314463f`. Unity compilation/tests need to be rerun; no pass is claimed.

## Current checkpoint — P-03.7b source implementation (2026-11-27)

The last user-confirmed baseline before this phase is 540/540 EditMode tests passed for P-03.7a. P-03.7b was subsequently verified by the user at 550/550 EditMode tests.

Key additions:
- StackTransferService is the only approved route for new ownership-aware split/merge operations. Raw StackOperations remains a Core primitive and must not be wired directly from UI.
- Cross-owner split is explicit, requires the destination container custodian to match the new owner, and records an economic transaction against the fresh split-stack ID. Failed transaction/ownership commits roll back silently.
- Cross-owner merge is rejected. Same-owner merge stores persistent lineage including owner, both custodians, operation kind, quantity, timestamp and whether the source ID was retired.
- F5/F9 persists lineage. On restore, any source ID explicitly marked retired is purged from all restored grids and its ownership record is removed.
- Backpack grid registration is now an access-control boundary: unregistered before a backpack is equipped, removed when dropped, re-established on recovery/F9 only when equipped. New stack transfers reject stale references to dropped backpack storage.
- Equipment and portable backpacks have effective MaxStack 1 in ItemType; ordinary item stack tuning is unchanged.
- Ten new focused StackTransferServiceTests were added.

Next agent actions:
1. Ask the user to pull latest branch and compile in Unity.
2. Fix any compile errors from the first run; do not assume compilation passed.
3. Ask for full EditMode test result and inspect failures.
4. Run Play Mode acceptance: no backpack, equipped backpack, B-drop, stale grid reference rejection, pocket/container transfers while dropped, F5/F9 while dropped, exact recovery.
5. Only after user verification, proceed to P-03.7c UI (Shift+LMB quick transfer, Shift+RMB split half, Ctrl+RMB split one, stack count badges).

### Retired stack IDs — additional guard

StackLineageBook tracks retired IDs during runtime as well as in save data. Split ID generation excludes historical retired IDs; merge source/destination validation rejects retired IDs; rollback removes a provisional retired marker. A regression test deliberately reintroduces a retired GUID and verifies the ownership-aware service refuses it.

- F9 backpack reconciliation also checks the restored world status: the 8×6 grid is authorized only when the world book says Equipped. A mismatched Dropped state fails closed and is represented as a dropped backpack rather than exposing its contents.

## P-03.7c current work — player inventory stack controls (Unity verification pending)

P-03.7b is user-verified at 550/550. Player InventoryPanel now implements Shift+LMB quick transfer between backpack and personal pockets, Shift+RMB split-half, Ctrl+RMB split-one, and visible stack counts on backpack/pocket items. P3 retains HIDDEN while occupied. Split operations route through GameSession.SplitPlayerStack and StackTransferService. Added two Core tests for same-grid and 1×1 pocket-to-pocket splitting; expected full suite is 552, not yet run by the user.

Next: compile and run full EditMode suite; inspect any errors. Play Mode acceptance should cover splits from backpack and pockets, both modifiers, quick transfers with and without a backpack, full capacity no-op, count badges, P3 hidden label, and existing controls. Then continue P-03.7d searchable-container quick transfer/splitting and universal service quantity audit.

### P-03.7d current checkpoint

P-03.7d searchable-container stack integration is source-implemented. The user verified P-03.7c at 552/552. Unity then reported CS0136 in `SearchableContainerPanel.UpdatePointer`; the duplicate local was renamed to `draggedContainerItem` in commit `bb572b2f2633341a1a368a31f1ae178bf1eb7a1d`. Recompile before treating P-03.7d as verified. Expected EditMode count: 555. Play Mode checks still needed for container quick-transfer, split, merge, ownership/theft behaviour, and dropped-backpack isolation.

### P-03.7 player inventory drag-to-restack

A follow-up added direct drag-to-merge in `InventoryPanel`. Supported routes: backpack↔backpack, backpack↔pocket, pocket↔backpack and pocket↔pocket. The UI previews only `StackOperations.CanMerge`-compatible targets; GameSession routes through `StackTransferService.Merge`, which enforces ownership, capacity, lineage and destination identity. Partial merges preserve the source remainder ID; destination ID survives. Dropped backpack access remains blocked. Commits: `86ba1397ac77457fc0c9be41970afbcfa00ae601`, `cdf65030db48f21409dccb47d296952ce72e8c2e`, `2615a873e405b94a250269a337db7dd3c353277e`. Two added regressions cover same-grid partial backpack merging and full pocket-to-pocket merging. The UI captures requested quantity before drag cleanup. **Superseding verification:** user confirmed 555/555, Unity compile and Play Mode acceptance on 2026-11-27.
