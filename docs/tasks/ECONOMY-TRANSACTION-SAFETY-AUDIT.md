# Economy transaction-safety audit

**Date:** 2026-11-19  
**Branch:** `chore/collaboration-workflow-and-audit`  
**Status:** code safeguards and regression tests added; Unity verification pending.

## Scope

Audit the Phase 3 purchase path and the existing sell-to-shop helper for invalid quantities/prices, integer overflow, account safety, inventory capacity, ownership and partial completion. Preserve the user's approved Phase 3 rules: RetailService remains the purchase authority; existing Shop/ShopOrder sale valuation and acceptance rules are not redesigned.

## Findings

### Retail purchases

`RetailService.TryPurchase` already:
- Rejects missing business identity/inventory, invalid/equal actors, empty item IDs, non-positive quantities/prices, invalid timestamps and totals above `int.MaxValue`.
- Resolves both customer and owner accounts and rejects insufficient customer funds.
- Verifies that any matching stack that could be topped up is already owned by the customer.
- Uses inventory snapshots and account-balance restoration if inventory capacity, ownership registration or ledger commit fails.
- Registers newly created stack ownership and records one RetailTransaction only on success.
- Uses `CleanMoneyAccount` guarded debit/credit methods, which prevent negative balances and integer overflow.

No purchase shortcut was introduced. UI/GameSession must continue to call RetailService.

### Existing sell-to-shop helper

`Shop.TryFulfil` already:
- Rejects default/invalid quantities and totals above `int.MaxValue`.
- Checks stock before removing goods.
- Snapshots inventory and restores it if the clean account cannot accept the credit.
- Uses `PlayerStats.TryEarn` → `CleanMoneyAccount.TryCredit`, which rejects non-positive credits and overflow.

Gaps found and corrected:
- A default `ItemType` could be reported as insufficient stock instead of invalid input.
- A zero-value order was only rejected by account credit after removing and restoring inventory, causing unnecessary mutation notifications.

The helper now rejects empty item IDs and non-positive unit prices/totals before inventory changes. Existing valuation and acceptance behavior is otherwise unchanged.

## Regression coverage added

- Shop: zero-value order is rejected without inventory mutation or Changed notification.
- Shop: order with uninitialized ItemType is rejected as invalid.
- RetailService: parameterized cases for zero/negative quantity, zero/negative unit price and total-cost overflow. Each asserts balances, inventory, ownership and retail ledger remain unchanged.

Previous user-verified suite: **335/335**. Seven new test cases were added; expected next suite count is **342**. The assistant cannot run Unity; the new cases await user verification.

## Deliberate boundary / remaining design work

The existing `Shop.TryFulfil` call in GameSession is a developer sale control, not a player-facing sell interaction. The helper does not consult AssetOwnershipBook and does not create an economic transaction record. These are not silently added because doing so would change the scope and economic semantics of the legacy sale path.

Before exposing player-facing selling, design a sale operation that:
- validates the actor's ownership of the exact physical stack(s);
- validates the existing accepted item and valuation rules without inventing new ones;
- removes the goods and credits clean CARD atomically;
- records a completed economic transaction;
- rolls back all state if any commit step fails.

That future integration must preserve the current user decision that sell-to-shop acceptance/valuation rules are existing rules, not new rules invented by the audit.

## Save policy clarification

The user decided that current-format save/load reliability and round-trip tests matter most. Obsolete development saves need not be preserved indefinitely; substantial changes may intentionally require a fresh game. Current saves must not be silently corrupted or discarded.
