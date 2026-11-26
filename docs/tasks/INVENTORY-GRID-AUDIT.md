# Inventory & Grid Architecture Audit

**Status:** In progress — Phase 1 core transaction hardening implemented; Unity verification pending  
**Started:** 2026-11-26  
**Branch:** `chore/collaboration-workflow-and-audit`

## Objective

Audit and harden the original grid/inventory foundation and every important system built on it. Preserve verified gameplay behaviour. Prefer small, evidence-based fixes and regression tests over broad rewrites.

## Audit boundaries

1. Grid model: dimensions, placement, rotation, stack operations, movement, snapshots and stable item identity.
2. Transfer model: same-owner movement, ownership-changing transfers, rollback, event publication and transaction history.
3. Integrations: equipment, backpack/pockets, searchable containers, home storage, retail, shop sales, repair, production, contracts and Dirty Cash.
4. Persistence: inventory, equipment, pocket and dropped-backpack save/load; duplicate identity and ownership reconciliation.
5. UI: drag/drop coordinate math, destination previews, exact-cell routing, inaccessible storage isolation and capacity routing.
6. Regression coverage: failed operations leave no partial state; successful compound operations publish only committed state.

## Guardrails

- Do not redesign gameplay or introduce new inventory mechanics.
- Do not remove existing systems or change intended ownership rules.
- Preserve stable item instance IDs, item state, stack quantities, rotation and item footprints.
- Keep the player pockets and dropped-backpack access rules unchanged.
- Any direct inventory mutation used inside a multi-system operation must either be silent until commit or be rolled back without exposing a false intermediate state.
- Keep all non-task-card work recorded in `docs/PROGRESS.md`, `docs/progress/LOG-2026-11.md` and this handoff.

## Phase 1 — Core model and compound transaction hardening

### Findings and fixes

**1. Cross-inventory notifications could expose an intermediate state.**

Previously, a stack transfer placed the item into the destination and fired its `Changed` event before removing it from the source. Ownership-changing transfers could notify observers before the ownership book and transaction ledger were updated. Similar intermediate notifications existed in multi-grid production, repair, crafting, customer contracts, repair orders, Dirty Cash, retail purchases, shop sales and equipment transfers.

Implemented:
- Added internal silent mutation primitives for exact-stack removal, quantity removal, placement, output addition, state updates and snapshot restoration.
- `InventoryTransfer` now completes both inventory mutations before notifying either inventory. Failed transfers restore silently and publish no false intermediate state.
- `AssetTransferService` now commits physical movement, theft state, ownership and economic transaction history before publishing inventory changes. Internal deferred variants support wider compound operations.
- Production, repair, crafting, customer contracts, customer repair orders, Dirty Cash exchange/cleaning, retail purchase, shop sale and equipment equip/unequip now notify only after their complete transaction state is committed.
- Rollbacks in these operations restore silently, so observers are not told that a failed operation changed the inventory.

**2. Default/uninitialised item definitions could pass a geometry-only placement query.**

A default `ItemType` has zero dimensions and zero stack capacity. `CanPlace` previously relied on footprint geometry and could report that such a type fit even though it could not be stored. Added `ItemType.IsValid` and validation at grid placement/fit, transferred placement and add boundaries.

**3. Inventory snapshots were not bound to grid dimensions.**

A snapshot created from one grid could be restored into a differently sized grid, bypassing normal placement bounds. Snapshots now record their originating dimensions and restoration rejects a mismatched grid before mutating the target.

### Regression tests added

- Grid rejects default `ItemType` placement and rejects cross-dimension snapshot restoration without changing the destination.
- Inventory transfer observers see both grids after a successful move, not a temporary duplicate.
- Asset transfer observers see committed physical location, ownership and ledger state.
- Production, repair, crafting, customer contract fulfilment, customer repair-order acceptance, Dirty Cash exchange/cleaning, retail purchase, shop sale and equipment equip/unequip observers see committed state.

The searchable-container follow-up immediately preceding this audit added three tests. The last explicit user-reported numeric pass remains **506/506**. There are now **16 additional tests** relative to that reported baseline, so the expected current suite total is **522**. This is an expected count only; Unity has not yet verified it.

## Remaining phases

### Phase 2 — Persistence and identity reconciliation
- Trace all inventory save mappers and GameSession save/restore ordering.
- Verify one stable item ID cannot be restored into multiple accessible inventories or equipment slots.
- Verify ownership records agree with physical location and actor custody.
- Verify equipped/dropped backpack identity, contents and recovery after F5/F9.
- Verify malformed and duplicate records are skipped without leaving orphaned inventory entries or ownership records.

### Phase 3 — UI and routing
- Audit `InventoryDragMath`, `InventoryPanel`, `SearchableContainerPanel`, `StoragePanel`, `RetailPanel` and workbench item selection.
- Check rotated footprints, grab offsets, exact destination cells, swaps, preview-vs-commit agreement and inaccessible grids.
- Check all item receipt paths use the same capacity policy and all manual moves preserve exact target selection.

### Phase 4 — Integration sweep
- Search every inventory mutation call site in Core and Game.
- Classify direct mutations as safe single operations or compound transactions requiring silent mutation/commit notification.
- Verify ownership registration and cleanup for every creation, consumption, transfer, sale, repair and production path.
- Add regression tests for any confirmed gap.

### Phase 5 — Verification and closeout
- Run the complete Unity EditMode suite.
- Run Play Mode acceptance for backpack equipped/dropped, pockets, searchable containers, storage, repair, production, retail and contracts.
- Record exact user-reported test count and Play Mode result.
- Close the audit only after no unresolved high-severity findings remain.

## Current verification state

- Source-level review and edits completed for Phase 1.
- No Unity compilation, EditMode test run or Play Mode run has been performed by the assistant.
- Current expected EditMode suite total: **522**, pending user verification.
- Do not treat the audit as complete; persistence, UI and remaining integration sweeps are still open.
