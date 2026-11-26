# P-03.7 — Stack System & Inventory Interaction

**Status:** In progress — P-03.7a Core split/merge primitives implemented; Unity verification pending  
**Started:** 2026-11-27  
**Branch:** `chore/collaboration-workflow-and-audit`  
**Prerequisite:** Inventory & Grid Architecture Audit — user-confirmed 531/531 EditMode and Play Mode acceptance

## Objective

Turn the existing quantity-capable grid model into a complete, player-facing stack system. Support partial transfers, splitting, merging, quick-transfer and auditable stack identity while preserving the current inventory, ownership, persistence and dropped-backpack rules.

This is a continuation of the inventory foundation, not a new gameplay mechanic. Do not change unrelated economy, equipment, backpack, pocket or service rules.

## Confirmed interaction contract

- **LMB drag:** move the entire stack (existing behaviour).
- **Shift + LMB:** quick-transfer the entire stack to the paired/other inventory when a valid destination exists.
- **RMB:** inspect (existing behaviour).
- **Shift + RMB:** split half the stack.
- **Ctrl + RMB:** split one item.
- A split creates a new stack in a valid empty grid cell; the player can then drag it normally.
- If no valid destination exists, the operation does nothing and leaves the source untouched.
- Existing controls (R rotation, ESC cancel, E equipment, B backpack drop) must not be regressed.

## Stack compatibility

Two stacks may merge only when all of the following match:
- Item definition/type ID.
- Complete authoritative `ItemInstanceState` (quality, condition, authenticity and ownership status).
- Compatible physical item semantics: stackable, non-equippable and not a portable backpack.
- Destination has remaining capacity under the item's `MaxStack`.

Knowledge/discovery state is not an item property and does not block merging. Stackable item tuning is authored through each ItemDefinition's existing `maxStack`; do not globally change current asset values without a separate balance decision.

Unique equipment, backpacks and other authored non-stackable items remain individual stacks (MaxStack 1).

## Identity and ownership contract

### Whole-stack movement
- Moving an entire stack preserves its stable stack ID.
- Ownership changes only through the existing ownership-aware transfer service.

### Split
- The original/source stack retains its ID and remaining quantity.
- The newly created portion receives a fresh, unique GUID.
- The new stack inherits the exact item state and current owner.
- Both inventory and ownership changes commit atomically.

### Merge
- The destination stack's ID survives.
- Its quantity increases by the accepted amount, never beyond MaxStack.
- The source stack is reduced by that amount; if emptied, its ID is retired.
- A separate persistent stack-lineage record records source ID, surviving destination ID, item definition, quantity, owner/custody context, operation kind and game timestamp.
- The source ID is never silently reused or duplicated.
- A merge across actor ownership is performed as one ownership-aware transaction. If the existing ownership model cannot represent the partial/merged result without ambiguity, reject that merge rather than silently changing ownership. Do not weaken AssetOwnershipBook's one-owner-per-stack invariant.

### Partial transfer
- Move only the quantity that the destination can accept.
- Source retains its ID and untransferred remainder.
- An empty destination stack receives a fresh ID for the moved portion; an existing compatible destination stack retains its ID and records lineage.
- A failed operation changes nothing and publishes no Changed event.

## Transaction and persistence requirements

- Core operations must be atomic: snapshot/rollback or equivalent all-or-nothing mutation.
- Changed events publish only after physical inventory, ownership and relevant transaction/lineage ledgers agree.
- No duplicate live stack IDs across inventories.
- Lineage records survive F5/F9 and are validated during restore; malformed, duplicate or impossible records are skipped safely.
- A retired source ID must not restore as a live stack.
- Preserve condition, quality, authenticity and ownership state through split/transfer/merge.
- Do not change save compatibility policy or invent a global per-unit GUID for ordinary materials.

## Implementation sequence

### P-03.7a — Core stack operations
- Inspect and reuse existing `ItemType.MaxStack`, `GridInventory.Add`, `ItemInstanceState`, snapshots and transfer primitives.
- Add explicit compatibility checks, split, merge and quantity transfer operations.
- Add stack-lineage record/ledger with capture/restore DTOs.
- Add Core regression tests for exact quantity conservation, stable IDs, state compatibility, capacity, invalid requests, rollback and replay/duplicate protection.

### P-03.7b — Ownership-aware transfers
- Integrate quantity movement with `AssetOwnershipBook`, `AssetTransferService`, `EconomicContainerRegistry` and transaction history.
- Ensure same-owner partial transfer and cross-owner merge are unambiguous.
- Record quantity and lineage without producing duplicate or orphaned ownership records.
- Test successful and failed ownership-changing partial transfers.

### P-03.7c — Inventory UI
- Display quantity on every stack with count > 1 (and keep singleton presentation uncluttered).
- Implement approved keyboard/mouse bindings.
- Add clear drag/split feedback and ensure cancel/failure restores the held quantity.
- Keep pocket 1×1 constraints: partial split into a pocket is allowed only for a compatible 1×1 item and a free pocket.
- Keep P3 visually HIDDEN and excluded from ordinary-search enumeration.

### P-03.7d — Universal integration and acceptance
- Integrate quick-transfer/split/partial move across backpack, pockets and searchable containers first.
- Audit home storage, retail, repair, production, customer contracts, Dirty Cash and all item receipt/consumption paths for assumptions that a stack is indivisible.
- Ensure all services continue to consume the requested quantity and preserve the remaining stack.
- Verify F5/F9, dropped backpack isolation, ownership and balances.

## Acceptance tests

- Split half and one from stacks of even, odd and maximum quantities.
- Reject splitting singleton/empty stacks and reject invalid modifiers.
- Split into empty cell; preserve source ID, create a unique new ID, copy state and owner.
- Merge compatible stacks; destination ID survives and source ID is retired/lineage-recorded.
- Reject different item types or any differing state field.
- Enforce MaxStack and partial capacity exactly.
- Transfer quantity into empty and occupied destinations.
- Verify no quantity loss/duplication after failure, rollback, save/load or repeated input.
- Verify ownership for same-owner and cross-owner operations; reject ambiguous cases.
- Test backpack↔pockets, pocket↔pocket, container↔pockets/backpack, and same-container stack rearrangement.
- Test dropped-backpack isolation and recovery.
- Test retail, home storage, repair, production, contracts and Dirty Cash paths.
- Run complete EditMode suite and Play Mode acceptance; record exact user-reported results.

## Guardrails

- No new item categories or final stack-size balancing decisions.
- Do not globally change existing ItemDefinition maxStack values merely to make tests convenient.
- No per-unit GUID model for ordinary materials.
- Do not bypass AssetTransferService for ownership-changing transfers.
- Do not mutate quantities directly from UI code.
- Do not claim Unity compilation or tests until the user confirms them.
- Keep wearable presentation paused; do not restart sprite/art work.


## Implementation log — P-03.7a source pass

Implemented in Core:
- `StackOperations.CanMerge` validates identical ItemType and complete ItemInstanceState, valid distinct stable IDs, and stack capacity.
- `StackOperations.Split` moves an exact quantity into an explicit destination cell, keeps the original ID on the remainder, creates a fresh ID for the new stack, and rolls back both grids on failure.
- `StackOperations.Merge` moves up to the requested quantity/capacity, preserves the destination ID, reduces or retires the source ID, and records a merge lineage entry. Duplicate operation IDs cause rollback.
- `StackLineageBook` stores merge lineage in the Core runtime model. Persistence and ownership-aware wrappers remain in P-03.7b.
- Added nine Core regression tests for split, merge, state compatibility, quantity conservation, partial capacity, event timing and duplicate operation protection.

Source files:
- `Assets/_Project/Core/StackOperations.cs`
- `Assets/_Project/Tests/EditMode/Core/StackOperationsTests.cs`

Commits: `513de74e7b92cad36a0baa5d7cc0d7f2852cabd8`, `b2a47e4c2978f8018ff5ee181f37c2c87f6a8af6`, `afffd44646322173d217b086a3ce76e6a7343580`.

Important boundary: these first-pass Core operations mutate physical stack quantities and lineage only. They do not yet change AssetOwnershipBook or economic transaction history. Do not call them directly from Game/UI until P-03.7b provides ownership-aware orchestration. No Unity compilation or test pass is claimed.
