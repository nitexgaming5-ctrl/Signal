# Progress — current repository truth

Last reconciled: **2026-11-26**. This is the concise status board. Historical detail belongs in [the November log](progress/LOG-2026-11.md); approved rules and acceptance detail belong in linked design/task documents.

## Project baseline
- Unity **6000.6.9f1**, 2D URP.
- Active architecture: `Signal.Core` (plain C#) + `Signal.Game` (Unity integration).
- Active scene: `Assets/_Project/Scenes/HideoutSlice.unity`.
- Working branch: `chore/collaboration-workflow-and-audit`; keep `main` untouched.
- Application version **0.01**. Save data has no schema-version field or automatic migration chain; incompatible experimental saves may be discarded/recreated.
- **Latest user-confirmed full EditMode suite: 504/504 passed**, after the Play Mode-only guard for temporary NPC visual-test residents. One automatic-pocket-routing regression test was committed afterward; expected current suite is **505**, but 505 has not been confirmed. Do not report 505/505 as passing.
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
- **P-03 Equipment & Portable Backpack — core implementation largely complete; non-art closeout remains.** See [P-03 task card](tasks/PLAYER-P-03-EQUIPMENT-BACKPACK.md).

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
- Temporary fresh-session debug loadout seeds a backpack and repair/production test cargo. It is test scaffolding, not intended new-game inventory.

Still outstanding:
1. Run the complete current EditMode suite (expected **505**) and inspect the XML.
2. Complete Play Mode acceptance: fresh session/debug loadout; equip/drop/recover; F5/F9 while equipped and dropped; pocket access while bag is dropped; manual exact-target moves/swaps; P3 ordinary-search exclusion contract; service flows with items split across backpack/pockets; ensure dropped cargo is inaccessible.
3. Keep temporary repair/production cargo until the user finishes acceptance. At final P-03 closeout, remove only automatic fresh-session cargo seeding; retain the F1 debug item spawner.
4. Record user-confirmed results. Do not mark P-03 complete before these checks.

**P-03.6 Wearable Presentation is paused/deferred.** The task card is historical preparation only, not the next implementation step. No new sprites, sprite templates, clothing overlays, wardrobe panel or art import work.

## Next strategic track — B-07 Neighbourhood Expansion
B-07 remains the next Direction B initiative after P-03 non-art closeout. Add a compact connected area only when it has meaningful destinations and repeatable reasons to visit. Reuse the existing player/session, clock, inventory, accounts, NPC social world and save context. Before implementation, prepare a dedicated task specification covering player outcome, geography, destinations, practical interactions, system reuse, NPC scope, navigation, save/load and acceptance. Explain any genuinely new mechanic and obtain user approval before coding it.

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
