# Roadmap — connected livelihoods in one lived-in city

## North star
Signal is a top-down urban RPG about making a precarious living in one shared city. The player inhabits a practical hideout, meets residents with public and private lives, handles physical goods, pursues lawful and unlawful opportunities, and sees progress through possessions, capabilities, relationships and places.

Current implementation and verification are authoritative in [PROGRESS.md](PROGRESS.md). Approved player-facing rules live in DESIGN and decision records.

## Completed Direction B sequence — B-01 to B-06
- **B-01 Production/workbench:** authored immediate recipes create physical goods.
- **B-02 Customer contracts:** one active contract; exact whole-stack physical fulfilment; issuer-funded clean CARD.
- **B-03 Practical hideout upgrade:** Compact Sewing Machine unlocks the next recipe.
- **B-04 Repair/restoration:** repair kits, item-state preservation and textile buyer/supplier loop.
- **B-05 Global NPC awareness & affinity:** bounded individual knowledge and separate player→NPC, NPC→NPC and NPC→player relationship authority.
- **B-06 Customer Repair Orders:** entrusted Worn Work Jacket, repair, exact-item return and 35 CARD service payment.
The user confirmed B-06's **456/456** EditMode baseline and in-game repair/return/payment loop on 2026-11-25. Do not rebuild these foundations without a concrete defect. No additional story cast was authored.

## Parallel Player Development track
- **P-01 Character Foundation & Appearance:** modular Body_A renderer, directional Idle/Walk and local importer implemented; user imported the pack and confirmed presentation.
- **P-02 Attributes & Skills:** exactly Armor and Heat Protection; fixed starting points, milestone awards and free respecialisation. Core/save/UI implementation user-verified at 467/467.
- **P-03 Equipment & Portable Backpack:** equipment slots, backpack lifecycle, pockets, hidden-pocket visibility, pocket transfers/swaps and pocket-aware service integration implemented in source. Non-art acceptance/closeout remains. See [P-03](tasks/PLAYER-P-03-EQUIPMENT-BACKPACK.md).

**Art boundary:** the user has paused sprite creation and wearable/wardrobe presentation. P-03.6 is parked; do not resume overlays, sprite templates, wardrobe preview or character-art work without explicit re-authorisation.

## Immediate sequence

### 1. P-03 non-art closeout
- Run the current full EditMode suite (expected 505; last confirmed 504/504 before the additional automatic-pocket-routing test).
- Resolve regressions before further gameplay expansion.
- Complete Play Mode acceptance of backpack equip/drop/recovery, F5/F9 while equipped and dropped, pocket operations, hidden-pocket visibility contract and all pocket-aware service paths.
- Keep temporary fresh-session repair/production cargo until acceptance is complete. Then remove only that automatic debug seeding; keep F1 debug item spawner.
- Update P-03 status and record user verification. Wearable visuals are not a closeout dependency while art is paused.

### 2. B-07 Neighbourhood Expansion
This is the next strategic gameplay initiative after P-03 non-art closeout. The goal is not map size for its own sake. Add a compact, connected area only when it creates meaningful reasons to travel, revisit, meet people and use existing livelihoods.

Before implementation, prepare a detailed task specification covering:
- player outcome and how it extends the existing loop;
- geographic connection to current Street/CornerShop/Apartment/Hideout spaces;
- proposed destinations and their practical purpose;
- existing systems reused (repair, production, retail, storage, customer work, NPC routines/relationships, scavenging);
- whether any new mechanic is actually necessary (explain it and seek approval before coding);
- resident scope, with no invented story cast/backstories;
- travel/navigation, scene composition, save/load and performance implications;
- acceptance criteria and Unity test/Play Mode plan.

Do not implement an empty district, disconnected map, placeholder service or new mechanic merely to increase content count.

## Later / separate tracks

### W-01 — Dynamic Weather & Cold Exposure
A separate future world-simulation phase. Intended direction: seasonal temperature changes, rain, storms, snow and blizzards; gradual cold exposure; shelter and effective Heat Protection mitigate cold. Heat Protection keeps its name and has no hot-weather/overheating meaning. Design and balance remain open until W-01 is activated.

### Broader pillars (not scheduled)
Crime consequences, police response, additional lawful/illicit activities, investigation, services, social favours, deeper scenario persistence, more city blocks and hideout personalisation remain part of the wider vision. Design them as connected player-facing scenarios, not isolated managers.

## Engineering guardrails
- One active gameplay initiative at a time.
- Complete the smallest coherent player-facing loop through Core, Game integration, UI, content and save state.
- Reuse existing authorities for inventory, ownership, transactions, accounts, NPC knowledge and persistence.
- Core is plain C#; Game owns Unity integration and authored content.
- Persistent changes require current-build save/load round-trip coverage.
- No new Unity packages or external dependencies without approval.
- No universal supply/demand, automatic NPC shopping, passive income, global gossip or per-frame NPC decision scoring.
- Do not add story residents/backstories before the main storyline/cast is designed.
- Unity Editor is the authority for compile, EditMode and Play Mode verification.


## P-03 — dropped backpack isolation regression (2026-11-26)

- **Source fix committed; verification pending.** SearchableContainerPanel no longer retains or hit-tests the live backpack inventory while the bag is dropped. GameSession and DirtyCashContactPanel now receive/use only accessible backpack references or an isolated empty grid. Personal pockets and automatic loot routing remain available.
- Preserve the dropped bag's stable identity and exact contents; the fix does not mutate its storage.
- Required next gate before P-03 closeout: Unity EditMode run, then Play Mode acceptance using B to drop the bag, opening Rhea's wardrobe and a street bin/searchable container, and recovering the same bag with its cargo intact.


## P-03 — searchable-container pocket interaction (2026-11-26)

- Implemented drag/drop from discovered searchable-container items into an exact personal pocket, and from a pocket into an exact searchable-container cell. Backpack↔pocket interaction and backpack rearrangement are preserved while the backpack is accessible.
- Ownership, custody and transaction history flow through AssetTransferService; resident-property theft outcomes remain intact.
- Two regression tests added. User-confirmed baseline was 504/504 before those additions; current expected suite total is 506, pending Unity verification.
- Required acceptance: Rhea wardrobe and street container, both directions, exact pocket targeting, rejection of occupied/oversized drops, and repeat with backpack dropped before recovery.


### P-03 searchable-container rearrangement follow-up (2026-11-26)

- User confirmed 506/506 on the preceding revision.
- Shared container UI now supports pocket↔pocket move/swap, same-container item rearrangement, and backpack→container placement at an exact cell. Applies to all searchable containers.
- Three EditMode regression tests added; expected suite total **509**, pending Unity verification.
- Commits: 6ad663c32d77de09527bb6ecd49740270ded1596, 924f7cbc8032266809420df8b361954fd7462d84, 05f03a9d88e580ffac5f5e5f106bea6efe60d173.
- Acceptance: run 509 EditMode tests; Play Mode verify all new paths in wardrobe and street container, including dropped-backpack isolation.


## Inventory & Grid Architecture Audit (2026-11-26)

- Approved as a stability checkpoint before further major development; preserve existing gameplay and avoid a rewrite.
- Phase 1 core hardening implemented: transactional inventory notifications, silent rollback, item-definition validity and snapshot/grid dimension validation. Compound operations covered include transfers, production, repair, crafting, contracts, repair orders, Dirty Cash, retail, shop sales and equipment.
- Added 13 Phase 1 tests plus three searchable-container tests from the preceding follow-up. Last explicit user-reported baseline 506/506; expected current suite 522, pending Unity verification.
- Task card: docs/tasks/INVENTORY-GRID-AUDIT.md.
- Remaining: persistence/identity reconciliation, UI drag/drop and capacity routing audit, remaining mutation call-site sweep, full EditMode and Play Mode verification.


### Inventory & Grid Architecture Audit — Phases 2–3

- Save/load now rejects duplicate stable item IDs across the four SaveMapper inventory grids; ownership restore skips orphaned IDs. Equipment, dropped backpack, pockets and searchable containers share the ownership-based duplicate guard; searchable containers also deduplicate against one another.
- InventoryPanel now prevents drop previews into an inaccessible backpack grid without freezing pocket drag ghost movement. InventoryDragMath rejects non-finite/overflowed geometry and invalid rotation footprints.
- Added 7 regression tests in these phases. Cumulative expected suite total is **529** relative to the last explicit user-reported 506/506 baseline; pending Unity verification.
- Phase 4: remaining Core/Game inventory mutation sweep, then full EditMode and Play Mode verification. See docs/tasks/INVENTORY-GRID-AUDIT.md.


### Inventory audit — additional restore guards and source sweep

- SaveMapper rejects aliased inventory-role arguments; SearchableContainerSaveMapper skips duplicate runtime destination containers. Two tests added.
- Remaining direct Game-layer mutations were source-reviewed as controlled item creation/bootstrap/debug flows; cross-grid UI movement uses transfer services.
- Expected suite total **531**, pending Unity verification (25 tests beyond last explicit user-reported 506/506).
- Next: full EditMode run, then Play Mode save/load and feature-integration acceptance. Audit task card: docs/tasks/INVENTORY-GRID-AUDIT.md.


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


## 2026-11-27 — P-03.7 Stack System & Inventory Interaction

The inventory/grid audit is now user-verified complete (**531/531 EditMode tests and Play Mode acceptance passed**). The next active P-03 foundation task is stack interaction: quantity labels, split half/one, quick-transfer, partial transfers, compatible merges and persistent stack lineage. See `docs/tasks/PLAYER-P-03-7-STACK-SYSTEM.md` for the approved identity, ownership, input and acceptance contracts. Do not proceed to unrelated roadmap work until this phase is integrated and verified.

## P-03.7 — Stack System & Inventory Interaction

- P-03.7a Core split/merge primitives: implemented; last user-confirmed suite result 540/540.
- P-03.7b ownership-aware transfer service, lineage persistence, retired-ID reconciliation and dropped-backpack container authorization: source implemented; Unity verification pending.
- P-03.7c inventory controls and stack quantity display: not started; gated on P-03.7b compile/test/Play Mode verification.
- P-03.7d universal service integration and end-to-end acceptance: not started.
- Existing wearable presentation remains paused. No authored stack-size balancing changes are included.

## P-03.7 — Stack System & Inventory Interaction

- P-03.7a Core split/merge primitives: user-verified at 540/540.
- P-03.7b ownership-aware transfers, lineage persistence and dropped-backpack authorization: user-verified at 550/550.
- P-03.7c player-carried inventory controls and quantity display: source implemented; Unity verification pending (two new regression tests; expected suite total 552).
- P-03.7d searchable-container quick transfer/splitting, universal service integration and end-to-end acceptance: not started.
- Existing wearable presentation remains paused. No authored stack-size balancing changes are included.
