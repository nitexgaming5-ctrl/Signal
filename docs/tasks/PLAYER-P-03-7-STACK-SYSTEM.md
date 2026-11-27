# P-03.7 — Stack System & Inventory Interaction

**Status:** In progress — P-03.7a verified (540/540); P-03.7b verified (550/550); P-03.7c source implemented, Unity verification pending  
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


- Compile fix after the first Unity pass: `StackMergeRecord`'s constructor was `internal`, but the EditMode test assembly constructs a record to verify duplicate-operation rejection. Made the constructor public; `StackLineageBook.TryRecordMerge` remains the validation boundary and rejects empty IDs, self-merges, invalid quantities and duplicate operation IDs. Commit: `fcb70dc75c5bc7f77be1c8fd0d3fb0943314463f`. Unity compilation/tests need to be rerun; no pass is claimed.

## Implementation log — P-03.7b source pass (Unity verification pending)

Implemented the ownership-aware Core orchestration and GameSession authorization/persistence wiring:

- Added ItemType.Equippable, ItemType.PortableBackpack and ItemType.IsStackable. Equippable items and portable backpacks now expose an effective MaxStack of 1 even if an asset has a larger serialized value; authored ordinary-material stack limits are unchanged.
- Added StackTransferService:
  - Partial split requires registered source/destination containers and a verified source owner.
  - The original ID and remainder stay with the source; the new stack gets a fresh ID, exact item state and an explicit owner.
  - Cross-owner split requires the destination container's custodian to be the new owner and records an EconomicTransaction for the new stack ID and moved quantity.
  - Failed ownership/ledger commits restore both inventories and remove provisional ownership without publishing Changed events.
  - Theft splits mark only the transferred portion Stolen, matching the existing whole-stack theft semantics.
  - Merge rejects cross-owner stacks, preserves destination ID, records owner/custodian/timestamp/quantity lineage, and retires the source ownership record only when the source stack is fully consumed.
- Extended StackMergeRecord with owner, source/destination custodian, operation kind, day/minute and source-retired metadata. Duplicate operation IDs are rejected and can be rolled back if the wider transaction fails.
- Added StackLineageSaveMapper and SavedStackMergeRecord. F5 captures merge lineage; F9 restores validated records after all inventory containers are restored. A retired source ID found in a restored inventory is removed together with its ownership record, preventing malformed/stale saves from resurrecting a consumed stack identity.
- Hardened backpack-grid authorization: _inventory is no longer registered at session startup. It is registered only while a backpack is equipped, unregistered on drop, and reconciled after F9. A stale reference to a dropped backpack grid is therefore rejected by StackTransferService even if a caller bypasses the UI.
- Added ten StackTransferServiceTests covering same-owner split, cross-owner split and transaction history, duplicate-ledger rollback, cross-owner merge rejection, partial/full same-owner merge identity, ownership/custody lineage, F5/F9 lineage restoration and retired-ID cleanup, non-stackable equipment/backpacks, and unregistered dropped-backpack grid rejection.

Source files:
- Assets/_Project/Core/ItemType.cs
- Assets/_Project/Core/StackOperations.cs
- Assets/_Project/Core/StackTransferService.cs
- Assets/_Project/Core/StackLineageSaveMapper.cs
- Assets/_Project/Core/SaveData.cs
- Assets/_Project/Game/ItemDefinition.cs
- Assets/_Project/Game/GameSession.cs
- Assets/_Project/Tests/EditMode/Core/StackTransferServiceTests.cs

Source commits include: c5ff1c2fd60d9fb069de9f6dd8ea9a580d976b81, 2dc732aa32d2856e23b05246f6d6bcdd4f90f895, 413587b8c7f64158757260244aa38cab6d092b007, ff57dc7f5162015cd17ec1c2b1da98f50a315027, a6973678708e5225c8ddca7b4d512918585923b1, 6b2ed39dbc296e192a1c02f9a5ed5d2fc00e8e5e, 96a53c220457d1f6f9f62e6b8780794beccf6612.

Important boundary: P-03.7b is source-implemented but not yet Unity-compiled or user-verified. Do not wire UI controls directly to raw StackOperations; use GameSession.StackTransfers. Next: inspect/fix any Unity compile diagnostics, then run the full EditMode suite and Play Mode dropped-backpack acceptance before starting P-03.7c UI.

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


### P-03.7c follow-up hardening

- Split destination search preserves the source stack's rotation for backpack-to-backpack splits and computes the rotated footprint bounds correctly. Pocket destinations remain unrotated 1×1 only.
- Updated the inventory legend to retain the manual exact-pocket drag instruction alongside the new quick-transfer/split bindings.
- Additional source commits: 02b4d8f1d3322f0a03f0e9993af42c1dac5d8610, dfdd9f564c8d1febcb07891e81bb0b5165c512af, aff762ca81ffef03e850bf330af9b1fcb77dc1d2, eb743aaa0b373818ab7e26563699001fe4d78a1b, db5a968e215ab7a9cec74cd4511b71b1f06c58ca.
