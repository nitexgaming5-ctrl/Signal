# Progress — current repository truth

Last reconciled: **2026-11-27**. This is the concise status board. Historical detail belongs in [the November log](progress/LOG-2026-11.md); approved rules and acceptance detail belong in linked design/task documents.

## Project baseline
- Unity **6000.6.9f1**, 2D URP.
- Active architecture: `Signal.Core` (plain C#) + `Signal.Game` (Unity integration).
- Active scene: `Assets/_Project/Scenes/HideoutSlice.unity`.
- Working branch: `chore/collaboration-workflow-and-audit`; keep `main` untouched.
- Application version **0.01**. Save data has no schema-version field or automatic migration chain; incompatible experimental saves may be discarded/recreated.
- **Latest user-confirmed full EditMode suite: 555/555 passed** (P-03.7d, 2026-11-27). User also confirmed Unity compilation, stack-ID integrity after split/merge/save, dropped-backpack isolation and F5/F9 restoration for equipped and dropped backpacks.
- Unity Editor is authoritative for compilation, EditMode tests and Play Mode acceptance. Source review is not Unity verification.

## Strategic direction and sequence
The user selected **Direction B — meaningful everyday livelihood**: repair, production, customers, services, relationships and a practical hideout. Keep one active gameplay initiative at a time.

### Completed livelihood foundations — B-01 to B-06
- **B-01 Production/workbench:** authored immediate recipes create physical goods.
- **B-02 Customer contracts:** one active contract, physical whole-stack fulfilment and issuer-funded clean CARD.
- **B-03 Practical upgrade:** Compact Sewing Machine unlocks the next recipe.
- **B-04 Repair/restoration:** repair kits, item-state preservation and Rhea's textile buyer/supplier loop.
- **B-05 Global NPC awareness & affinity:** bounded individual knowledge, local perception and separate relationship directions.
- **B-06 Customer Repair Orders:** Milo entrusts a Worn Work Jacket; player repairs and returns the exact item for 35 CARD. Ownership/custody remain distinct; payment and return are atomic and replay-protected.
- User confirmed **456/456** and successful B-06 Play Mode acceptance on 2026-11-25. Do not rebuild these foundations without a concrete defect. No additional story cast was authored.

### Player Development
- **P-01 Character Foundation & Appearance — implemented/user-verified.** Modular Body_A renderer, directional Idle/Walk, local ZIP importer and fallback. User imported the supplied pack and confirmed the enlarged character presentation.
- **P-02 Attributes & Skills — implemented/user-verified.** Exactly two attributes: Armor and Heat Protection. Fixed starting skill-point budget, milestone awards, free respecialisation, save mapping and F2 allocation panel. User confirmed **467/467** after this implementation slice. Catalog tuning and actual skill-effect/milestone-trigger integrations remain provisional/future work.
- **P-03 Equipment & Portable Backpack — P-03.7 closed and user-verified.** User confirmed 555/555, compile, stack/backpack persistence invariants and Play Mode acceptance. Automatic fresh-session debug cargo has since been removed in source; its separate post-cleanup Unity check remains. Wearable presentation is paused. See [P-03 task card](tasks/PLAYER-P-03-EQUIPMENT-BACKPACK.md).

## P-03 current implementation truth
Implemented in source:
- Seven equipment slots: Head, Torso, Legs, Feet, Accessory 1–3. Backpack occupies one generic accessory slot.
- Exact-instance equip/unequip preserves stable identity, item state and ownership.
- Equipped backpack owns its 8×6 contents grid by stable backpack instance ID.
- B drops the backpack; E recovers and re-equips the same item. Dropped contents remain inaccessible until recovery.
- F5/F9 captures backpack equipped/dropped state, position, identity and contents.
- Three persistent 1×1 pockets: P1/P2 ordinary; P3 hidden from ordinary-search enumeration and visibly marked HIDDEN.
- Manual pocket moves are exact-target; pocket-to-pocket move/swap preserves identity/state/ownership.
- Automatic receipts choose accessible backpack capacity first, then first suitable empty pocket P1 → P2 → P3. No destination prompt; oversized/rotated items cannot use pockets.
- Pocket-aware integration/source audit covers Retail SELL, Dirty Cash selection, Home Storage, Searchable Container, workbench repair, multi-grid production, customer flows and GameSession receipt/transfer paths. Dropped-backpack cargo is excluded from accessible views.
- The automatic debug starting loadout has been removed: fresh sessions now start without a backpack or test cargo. Use the retained F1 debug item spawner to equip a backpack and spawn repair/production test items.

P-03.7 stack-system status:
1. P-03.7a Core operations — user-verified at 540/540.
2. P-03.7b ownership-aware split/merge, lineage persistence and dropped-backpack authorization — user-verified at 550/550.
3. P-03.7c backpack/pocket UI controls — user-verified at 552/552.
4. P-03.7d searchable-container quick-transfer/split/merge and service quantity audit — **user-verified at 555/555**, with Unity compilation and Play Mode acceptance confirmed.

P-03.7d acceptance is complete: the user confirmed successful Unity compilation, **555/555 EditMode tests**, no duplicate stack IDs after split/merge/save, no access to dropped-backpack cargo, and correct F5/F9 restoration of equipped and dropped backpacks. The user also confirmed the Play Mode acceptance matrix. P-03.7 is closed.

P-03 non-art closeout source cleanup: automatic fresh-session backpack/cargo seeding has been removed; the F1 debug item spawner is retained. This cleanup is separate from the closed P-03.7 milestone and still needs a fresh Unity compile/full EditMode run and Play Mode confirmation that a new game starts empty and F1 spawning works. Wearable presentation remains deferred.

**P-03.6 Wearable Presentation is paused/deferred.** The task card is historical preparation only, not the next implementation step. No new sprites, sprite templates, clothing overlays, wardrobe panel or art import work.

## Next strategic track — B-07 Neighbourhood Expansion
**Specification prepared; awaiting explicit user approval.** See [B-07 task card](tasks/B-07-NEIGHBOURHOOD-EXPANSION.md). Recommended vertical slice: a connected Market Lane/courtyard attached to the current Street, one enterable second-hand/materials shop using existing fixed-stock retail, and two persistent public searchable containers. No new named residents, story content or mechanics. Reuse the existing location graph/pathfinder, retail, inventory/ownership/stack transfer, searchable-container and save/load authorities. Do not begin implementation until the user approves or revises the proposed geography and content choices. Any new gameplay mechanic requires its own explanation and explicit approval.

## Separate future track — W-01 Weather & Cold Exposure
W-01 is separate and inactive. Intended direction: seasonal temperature/weather, rain, storms, snow/blizzards and gradual cold exposure; Heat Protection retains its name and mitigates cold only. No hot-weather/overheating gameplay. Detailed mechanics/balance remain open until W-01 is activated.

## Reusable foundations — do not rebuild
| Area | Existing foundation |
|---|---|
| World | Hideout, Street, CornerShop, Apartment, persistent GameSession/player |
| Inventory | Spatial grid, stable stack GUIDs, item state, three pockets, portable backpack lifecycle |
| Ownership/economy | Ownership vs custody, atomic asset transfers, CARD accounts, service-payment ledger |
| Workshop | Actor-aware repair/production, authored recipes, one-active customer job |
| Scavenging/crime | One-time searchable bins, resident-owned containers, theft outcomes, witness-scoped knowledge |
| NPCs | Shared NpcSocialWorld, local perception, individual knowledge, routines, directed relationships |
| Dirty Cash | Physical currency, explicit exchange/cleaning and separate transaction history |
| Player Development | Armor, Heat Protection, skills, points, milestones and save mapping |

## Guardrails
- One active gameplay initiative at a time; complete a coherent player-facing loop.
- Core remains plain C#; Game owns Unity integration and authored content.
- Reuse existing inventory, transaction, ownership, account and save authorities.
- Persistent state needs current-build F5/F9 round-trip coverage.
- No new Unity packages/dependencies without approval.
- No universal supply/demand, automatic NPC shopping, passive revenue, global gossip or incidental simulation.
- Do not author new story cast/backstories until the main storyline/cast are designed.
- Do not run `dotnet` or `msbuild` for this Unity project.
- Keep this file concise; append history to the November log.


## 2026-11-26 — P-03 dropped-backpack panel isolation fix

- User reproduced a glitch by pressing B to drop the backpack and then opening Rhea's wardrobe / street searchable containers.
- Found that SearchableContainerPanel hid its backpack grid visually but retained the real _inventory reference. Its pointer hit-testing and drag/drop paths still queried that reference.
- Hardened SearchableContainerPanel to use an isolated empty 1×1 grid whenever the backpack is inaccessible, and to skip backpack-grid pointer hit-testing entirely in that state.
- GameSession now passes only the accessible backpack grid or the empty carry sentinel to SearchableContainerPanel and DirtyCashContactPanel. DirtyCashContactPanel also independently normalizes inaccessible state to an isolated empty grid.
- Personal pockets remain available; loot receipts continue through the existing capacity-aware backpack→P1→P2→P3 selector, which excludes the dropped backpack.
- The dropped backpack's actual storage grid and contents are untouched.
- Commits: e8c57416a279af7bc6d0acaa69ba5e421a98d87b (searchable panel isolation), c61532aad7e8102872c622e1beb9ed7f6bb10283 (Dirty Cash isolation), 73be827f6382cb92d0dddbe66bed9b955c79e269 (GameSession panel bindings), a037dca3ca6ae61526ee065b0fd4c5bab987024b (pointer hit-test guard).
- Verification pending: no Unity compile or test result is claimed. User must test B-drop → Rhea wardrobe → street bin/container, then recover the same backpack and confirm its original cargo is intact.


## 2026-11-26 — P-03 searchable-container pocket drag/drop

- User identified that searchable-container panels (e.g. Rhea's wardrobe) displayed pocket items but did not provide working drag-and-drop to/from pockets.
- Implemented exact-pocket drop targets for discovered container items and pocket-item drag into exact cells in the active container. Also preserved backpack rearrangement and added backpack↔pocket moves while the backpack is accessible.
- Container↔pocket movement uses AssetTransferService, so stable instance IDs, item state, ownership/custody and transaction history remain authoritative. Taking resident property still follows the existing theft/witness path; returning an item transfers ownership to the container's actor. Invalid/occupied/oversized pocket drops fail safely.
- Added two EditMode regression tests for container→pocket and pocket→container transfer integrity.
- User confirmed **504/504** before the two new tests. Current expected suite total is **506**, pending Unity verification. No new pass is claimed.
- Source commits: 2224a3baffffb3f7dfdc5c0e7aa0dffba10fdbb3 (panel interaction), f35ef49ce7dca66707ffc3ad243788fece0aa3c8 (GameSession transfer callbacks), 9e4c20fe275562c3249ce8aef9551cc0b2653fe9 (UI hint), 46d34a89bcb1336f4c1a628f72b1e1dcd7bf551c (backpack/pocket interaction continuity), ae853be9e0d1ce8c83ec6c3446cb50937ac8ccb0 (drag-origin handling), ee7850dab68dc0eb2ca06cd52e555690414db7e7 (regression tests).
- Next: pull, run Unity EditMode tests, then Play Mode test pocket→wardrobe, wardrobe→pocket, pocket→street-container and street-container→pocket. Repeat with backpack dropped; verify cargo isolation and recovery remain intact.


### P-03 searchable-container rearrangement follow-up (2026-11-26)

- User confirmed **506/506** on the preceding build.
- Shared searchable-container UI now supports pocket↔pocket moves/swaps, moving discovered items within the same container grid, and placing backpack items into exact container cells.
- Same-container rearrangement uses the inventory's in-place move operation; backpack/pocket placement into containers uses AssetTransferService for ownership/custody consistency.
- Three EditMode regression tests added. Expected suite total: **509**, pending Unity verification; do not treat this as a confirmed pass yet.
- Commits: 6ad663c32d77de09527bb6ecd49740270ded1596, 924f7cbc8032266809420df8b361954fd7462d84, 05f03a9d88e580ffac5f5e5f106bea6efe60d173.
- Next: verify 509/509 and Play Mode across wardrobe + street container, including pocket↔pocket, in-container rearrangement, backpack→container, pocket→container, and dropped-backpack isolation.


## Inventory & Grid Architecture Audit — 2026-11-26

- User approved a dedicated stability audit of the original grid/inventory foundation and its integrations. This is a hardening pass, not a rewrite.
- Phase 1 implemented: transfer and compound-operation inventory events are now published only after physical contents, ownership, account/payment state and transaction history have committed. Failed operations restore silently.
- Covered: InventoryTransfer, AssetTransferService, ProductionService, RepairService, Crafting, CustomerContractService, CustomerRepairOrderService, IllicitExchangeService, DirtyCashCleaningService, RetailService, Shop and PlayerEquipmentLoadout.
- Added ItemType validity checks at grid boundaries; default/uninitialised ItemType no longer passes CanPlace. InventorySnapshot now records grid dimensions and cannot be restored into a differently sized grid.
- Added 13 Phase 1 regression tests. Together with the three searchable-container tests added immediately before this audit, there are 16 tests beyond the last explicit user-reported 506/506 baseline. Expected current total: **522**, pending Unity verification. No new pass is claimed.
- Task card: docs/tasks/INVENTORY-GRID-AUDIT.md.
- Phase 2 still open: audit save/load ordering, global stable-ID uniqueness, ownership/location reconciliation, equipped/dropped backpack restoration. Then audit UI drag math and remaining integration mutation call sites.
- No Unity compile, EditMode run or Play Mode run has been performed by the assistant.


### Inventory & Grid Audit — Phases 2–3 (2026-11-26)

- Persistence audit found that identical stable item IDs could be restored into multiple independent inventories because each grid validated uniqueness only locally. SaveMapper now shares a single restored-ID set across player inventory, home storage, contact inventory and contract receiver. Malformed/empty IDs and cross-grid duplicates are skipped.
- Asset ownership restore now ignores orphaned owner records for IDs with no restored physical item, while retaining fallback owner registration for valid restored items.
- Equipment and dropped-backpack restore now reject IDs already present in the ownership book. Pocket restore already had this guard. SearchableContainerSaveMapper now receives the existing ownership book and rejects duplicates against previously restored locations and across searchable containers.
- UI audit found InventoryPanel could still treat the hidden backpack grid as a drop target while the backpack was inaccessible. Accessibility now gates drop targeting, while cursor-local coordinates continue updating so pocket drag ghosts follow the cursor.
- InventoryDragMath now rejects infinite/overflowed grid pixel dimensions and invalid rotation footprint dimensions.
- Added 4 persistence/identity tests and 3 drag-math tests. Cumulative additions since the last explicit user-reported 506/506 baseline: 23 tests; expected current suite total **529**, pending Unity verification.
- Phase 4 remains: sweep every remaining Core/Game inventory mutation and persistence route; then run Unity EditMode and Play Mode acceptance. No tests are claimed passed by the assistant.


### Inventory & Grid Audit — additional restore guards and source sweep (2026-11-26)

- SaveMapper now rejects aliased inventory-role arguments before clearing any grid. SearchableContainerSaveMapper skips duplicate destination container IDs/references so one runtime container cannot be restored twice in one pass.
- Added two regression tests for these guards.
- Source-level Game-layer sweep classified remaining direct inventory mutations as controlled creation/bootstrap/debug flows: container loot generation, authored resident-property stock, Milo parcel creation/restoration, debug item spawning and legacy inventory demo bootstrap. Interactive UI paths use central transfer services for cross-grid movement; single-grid rearrangement uses GridInventory.
- Expected current EditMode total is now **531**: 25 tests beyond the last explicit user-reported 506/506 baseline. Pending Unity verification; no pass claimed.
- Remaining acceptance: run 531 EditMode tests, then Play Mode save/load with duplicate/malformed test saves where feasible, equipped/dropped backpack, pockets and searchable containers. Verify repair, production, retail, shop sale and customer contract flows after transaction notification changes.


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


## 2026-11-27 — Inventory audit verified; P-03.7 Stack System started

- User confirmed the full inventory/grid architecture audit: **531/531 EditMode tests passed** and Play Mode acceptance passed. This closes the audit's outstanding verification requirement; no Unity verification is claimed independently by the assistant.
- User approved P-03.7 Stack System & Inventory Interaction, including LMB drag, Shift+LMB quick-transfer, RMB inspect, Shift+RMB split half, and Ctrl+RMB split one.
- Existing foundations confirmed in source: `ItemType.MaxStack`, `GridInventory.Add` compatible-state stacking, immutable `ItemInstanceState`, snapshots and stable stack IDs. Missing foundations include explicit split/merge/quantity transfer operations, lineage records and player-facing quantity controls.
- Created `docs/tasks/PLAYER-P-03-7-STACK-SYSTEM.md`. Implementation is sequenced as Core operations/lineage, ownership-aware transfers, UI controls, then universal service integration and acceptance.
- Identity contract: whole move preserves ID; split keeps source ID and assigns a fresh ID to the new portion; merge keeps destination ID and retires the consumed source ID with persistent lineage. Merges require identical item type and complete instance state. Ownership-changing partial merges must be atomic and rejected if one-owner-per-stack cannot be preserved unambiguously.
- Next: implement and test P-03.7a Core operations before wiring UI. Existing item stack-size asset values remain unchanged pending separate balance decisions.


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

## P-03.7b — Ownership-aware stack transfers (source implemented; Unity verification pending)

- Added StackTransferService to coordinate split/merge with live container registration, AssetOwnershipBook, EconomicTransactionLedger and StackLineageBook.
- Same-owner splits preserve owner and create no economic transaction. Explicit cross-owner splits assign the new stack to the destination custodian and record the exact moved quantity; failed ledger/ownership commits roll back inventory and ownership before any Changed event.
- Cross-owner merges are rejected. Same-owner merges retain destination identity, record owner/custodian/time/quantity lineage, and retire the source owner record only when fully consumed.
- Equipment/backpack types are intrinsically non-stackable at the Core ItemType level; no authored ordinary-material MaxStack tuning was changed.
- Added F5/F9 lineage persistence and restore-time removal of any stack ID recorded as retired, including stale IDs found in backpack, pockets, home storage, contacts, contract receiver or searchable containers.
- Backpack _inventory registration now exists only while equipped. Drop unregisters it; recovery/F9 re-authorizes it. This closes the stale-grid-reference path for new stack transfers when a backpack is dropped.
- Added ten focused EditMode regression tests. Unity compile and test execution are pending; no pass count is claimed.
- Next: user runs Unity compile/full EditMode suite; fix any diagnostics; then Play Mode test B-drop → attempt transfer against stale backpack grid → F5/F9 while dropped → recover exact contents. P-03.7c UI remains gated on this verification.

### P-03.7b follow-up hardening — retired identity reuse

- StackLineageBook now maintains a runtime retired-ID set. A fully consumed source ID cannot be used as a later merge source/destination, and rollback removes the retired marker together with the lineage record.
- StackTransferService generates fresh split IDs while excluding both currently owned and historically retired IDs.
- Added a regression assertion that deliberately reintroduces a retired source GUID into a grid and verifies the stack service refuses to operate on it. Save restoration still purges such stale records from all supplied grids.

- F9 backpack reconciliation also checks the restored world status: the 8×6 grid is authorized only when the world book says Equipped. A mismatched Dropped state fails closed and is represented as a dropped backpack rather than exposing its contents.

## P-03.7c — Player inventory stack controls (source implemented; Unity verification pending)

User confirmed the P-03.7b checkpoint at 550/550 EditMode tests. P-03.7c now has its first player-carried-inventory UI implementation:

- Stack quantity remains visible on backpack items; pocket labels now also display quantities, including when an item has an icon.
- P3 retains its explicit HIDDEN label even while occupied and displaying a stack count.
- Shift+LMB quick-transfers an entire 1×1 stack between backpack and the first available personal pocket, or between pockets when the backpack is inaccessible.
- Shift+RMB splits half the stack (integer floor; minimum one) and Ctrl+RMB splits one item.
- Splits search for a valid empty cell in the accessible backpack first, then a free 1×1 pocket where the item fits. If no destination exists, the UI does not mutate the source.
- All split requests route through GameSession.SplitPlayerStack → StackTransferService; the UI never calls StackOperations directly.
- Existing LMB drag, RMB inspect, R rotate, ESC cancel, E equip, digit unequip and B drop controls are retained.
- Added two EditMode regression tests for same-grid split and 1×1 pocket-to-pocket split. Expected suite baseline is now 552; Unity compilation and full suite execution are pending.
- Searchable-container Shift+LMB / partial-split integration and service-wide quantity audit remain P-03.7d work, not claimed complete here.

Source commits: 2d8b2a098de1f0c4da759f5676a20ca71ecf8bf8, 2502dc89c302cb0ee65abb6c44b05274f4fa3ab8, fd6533636a1ad171b1ddf99f028d053e1105c923, 0055b65603453c91062b756d12560daa18cc994f.


P-03.7c follow-up: split placement preserves rotated source footprints and keeps the manual exact-pocket drag instruction visible. Unity verification remains pending; expected suite count is 552.

Latest P-03.7c source commits after the first UI log: 02b4d8f1d3322f0a03f0e9993af42c1dac5d8610, dfdd9f564c8d1febcb07891e81bb0b5165c512af, aff762ca81ffef03e850bf330af9b1fcb77dc1d2, eb743aaa0b373818ab7e26563699001fe4d78a1b, db5a968e215ab7a9cec74cd4511b71b1f06c58ca.

- **P-03.7d compile correction:** fixed Unity CS0136 in `SearchableContainerPanel.UpdatePointer` by renaming the second overlapping `containerItem` local to `draggedContainerItem` (commit `bb572b2f2633341a1a368a31f1ae178bf1eb7a1d`). Source-level audit remains complete; Unity compile, expected 555/555 EditMode tests, and Play Mode acceptance await user verification.

- **P-03.7 drag-to-restack follow-up:** player inventory now supports dragging a stack onto a compatible occupied stack across backpack/backpack, backpack/pocket, pocket/backpack and pocket/pocket. GameSession routes merges through StackTransferService; same-owner/state/capacity checks, destination-ID survival, partial remainder identity and dropped-backpack access guards remain authoritative. UI legend updated. Commits: `86ba1397ac77457fc0c9be41970afbcfa00ae601`, `cdf65030db48f21409dccb47d296952ce72e8c2e`, `2615a873e405b94a250269a337db7dd3c353277e`. Unity verification pending. Added two focused Core regressions for same-grid partial merging and full pocket-to-pocket merging; expected current EditMode count is 555. The UI also captures the merge request quantity before drag cleanup.
