# ECONOMY — player-facing sell-to-shop implementation specification

**Status:** implemented in the working branch; awaiting Unity compilation, EditMode tests and Play Mode verification. Q21 approved by the user on 2026-11-19.  
**Date:** 2026-11-19  
**Depends on:** Economy Phase 3 (335/335 EditMode tests and Play Mode functionality user-verified); transaction-safety audit (342/342 EditMode tests user-verified).

## Player-facing outcome

At an open business counter, the player can switch the existing retail panel between BUY and SELL.

- BUY remains the existing unlimited authored merchandise catalogue and continues to call Core `RetailService`.
- SELL lets the player offer eligible goods currently carried in their player inventory to that specific business.
- A successful sale consumes the selected goods and credits the player's clean CARD account.
- Both directions use the same local business interaction and opening-hours gate.

This adds a basic two-way retail interaction without turning businesses into a management simulator.

## Approved buyback policy — Q21

**Approved: each `BusinessDefinition` owns a separate authored fixed-price buyback-offer list.**

Each offer contains:
- A reference to one `ItemDefinition`.
- A positive fixed clean-CARD unit price paid by that business.

Rules:
- A business accepts only item definitions explicitly present in its own buyback-offer list.
- The buyback list is independent of the business's player-facing `Merchandise` catalogue. Selling Bread to the player does not imply that the business buys Bread back.
- Different businesses may accept different goods and may offer different prices for the same item.
- A missing offer means the business refuses that item.
- Offers are authored data, not dynamically generated prices.
- No `ItemValueCalculator` formula, retail-price multiplier, quality/condition/authenticity/ownership modifier, reputation adjustment, bargaining or market fluctuation is introduced.
- Offers are available whenever the business is open. No business cash budget or account debit is modeled.

The user has approved the model, not any specific new buyback item or price. Do not invent buyback entries or prices in content assets without an explicit content decision. Existing temporary Bread retail price is not a buyback offer.

## Existing sale primitive and required adaptation

The current Core `ShopOrder` contains item type, quantity and caller-supplied unit price. `Shop.TryFulfil` validates the order, removes matching goods and credits the player's clean account, with inventory rollback if account credit fails. It rejects invalid item/quantity/price and oversized totals.

It does not calculate prices, define per-business acceptance, target a selected physical stack, validate `AssetOwnershipBook`, or record a sale transaction. Its existing item-type removal may consume matching stacks other than the one the player selected.

Implementation must preserve the established sale rules and safety semantics (positive quantity/price, overflow-safe total, sufficient goods, clean-account credit and rollback), while extending the operation to target one exact `ItemInstanceId`. Do not route a selected-stack sale through the old type-wide removal path and risk consuming a different stack. Prefer extracting/reusing the shared validation and atomic account-credit behavior inside the existing Core sale boundary rather than creating a competing, weaker transaction path.

## Player interaction

- Reuse the existing `RetailPanel` and business interaction point.
- Add BUY and SELL tabs. BUY remains unchanged and continues to call `RetailService`.
- SELL lists eligible stacks in the player's carried inventory only. Home locker contents are not sellable at the counter; the player must physically carry goods to the business.
- Each row identifies the item, available stack quantity, that business's fixed unit offer and the selected quantity's total.
- Operate on one selected stack at a time. Quantity is bounded from 1 through that exact stack's current count. Do not aggregate multiple stacks.
- The UI must not be authoritative. Revalidate business identity, opening hours, offer existence/current price, selected stack identity, quantity, carried-inventory presence and ownership at confirmation time.
- A closed business, removed/changed offer, missing stack or failed validation rejects the sale without consuming goods or crediting money.
- After success, refresh the row/list and CARD display. Provide clear failure feedback while preserving the goods.
- Closing the panel restores normal movement and interaction as it does today.

## Exact stack identity and ownership

- Validate that the selected `ItemInstanceId` still exists in the player's carried inventory.
- Validate `AssetOwnershipBook` says the owner is `player`. Missing or mismatched ownership is a hard rejection; do not silently repair it during a sale.
- Add a Core inventory operation that removes a requested positive quantity from one exact stack.
- Partial sale: remaining stack retains its stable `ItemInstanceId`, `ItemInstanceState`, orientation and player ownership. No split/new identity is needed.
- Full sale: remove the stack and its ownership entry.
- Never consume another stack of the same item type to satisfy the request.
- Item state is recorded for provenance but does not affect eligibility or price in this phase.

## Money, ownership and transaction semantics

- Sale proceeds credit only the player's existing `CleanMoneyAccount` / CARD.
- Dirty-money items are not accepted as payment and are not generated by this flow.
- The business owner account is not debited. Phase 3 deliberately has no business budget/account constraint.
- Sold goods are consumed by the sale operation. They do not become business stock, catalogue stock or a finite shop inventory.
- A sale is not an `AssetTransferService` transfer because there is no registered receiving container and no modeled business inventory custody.
- Add a dedicated `ShopSaleTransaction` and `ShopSaleTransactionLedger`, separate from:
  - `EconomicTransactionLedger` (physical owner-to-owner asset transfers);
  - `RetailTransactionLedger` (unlimited catalogue purchases).
- Each sale record should contain a stable transaction ID, business ID, seller actor ID, source `ItemInstanceId`, item-definition ID, quantity, fixed unit price, total clean amount, item-state snapshot and in-game timestamp.
- A successful sale appends exactly one sale record. A failed sale appends none and must not alter the other ledgers.
- Persist sale history in the current save format. Since the current format is v9, add the sale collection in v10 and initialize it empty for v9 saves. Do not build a broad historical migration framework; obsolete development saves may be intentionally reset under the user's persistence policy.

## Atomicity and failure behavior

A sale is all-or-nothing:

1. Resolve and validate the active business, open schedule and current authored offer.
2. Validate positive quantity, positive unit price and overflow-safe total.
3. Resolve the exact selected stack and verify it is still carried by the player and owned by `player`.
4. Snapshot the relevant inventory, player account, ownership entry and sale-ledger state.
5. Remove the requested quantity from that exact stack.
6. Remove ownership only if the whole stack was consumed; preserve it for a partial sale.
7. Credit the player's clean CARD account.
8. Append exactly one sale transaction.
9. If any step after mutation fails, restore inventory, account, ownership and ledger to their snapshots.

A failure must never create money, consume goods, lose ownership, duplicate a record or leave a partial update. Revalidate at commit time; never trust UI values.

## Required implementation work

### Core
- Add an immutable buyback-offer data value (or equivalent) that binds an `ItemDefinition` identity to a positive fixed clean price.
- Extend `BusinessDefinition` with a distinct authored buyback-offer collection and validation for null/uninitialized item references, duplicate item offers and non-positive prices.
- Add exact-stack quantity removal to `GridInventory`, preserving identity/state for partial remainder.
- Extend/refactor the Core sale transaction boundary so exact-stack removal, ownership validation, account credit and rollback share one authoritative path.
- Add `ShopSaleTransaction` and `ShopSaleTransactionLedger` with duplicate/stable-ID protection and immutable records.
- Keep `RetailService` and `RetailTransactionLedger` behavior unchanged.

### Game
- Extend the existing business interaction composition to resolve offers for the active `BusinessDefinition`.
- Add SELL tab and row/quantity/confirm interaction to `RetailPanel`.
- List only carried, player-owned stacks for which the current business has an offer.
- Revalidate at confirmation and refresh UI/CARD after success.
- Preserve current proximity, opening-hours, close/ESC and input restoration behavior.
- Do not add buyback offers or prices to content without approved authored values.

### Persistence
- Add sale-ledger DTO mapping and current-format v10 save round-trip.
- Initialize v9 saves with an empty sale history; do not invest in compatibility for obsolete earlier development saves beyond the project's existing policy.
- Keep save writes reliable and atomic; malformed sale records must not invalidate otherwise valid current player state where record-level recovery is practical.

## Required tests

### Core transaction tests
- Full-stack sale removes the exact stack and ownership entry, credits exact CARD and records one sale.
- Partial sale changes only the selected stack count and preserves its ID, state and ownership.
- A sale never consumes another stack of the same item type.
- Missing stack, missing ownership, wrong owner, invalid item, zero/negative quantity, quantity above selected count, zero/negative price and total overflow fail without mutation.
- Insufficient goods and account-credit overflow fail without mutation.
- Duplicate/invalid ledger append or injected failure rolls back inventory, account and ownership.
- Failed sale creates no sale record; successful sale creates exactly one.
- Sale never debits a resident/business account and never changes retail purchase history or physical asset-transfer history.

### Business definition and Game integration tests
- Buyback offer validation rejects invalid, duplicate and non-positive entries.
- Business A accepts only its authored offers; Business B's offers do not leak across businesses.
- Selling a catalogue item is impossible unless that same item is separately listed as a buyback offer.
- SELL only lists carried eligible player-owned stacks; locker contents and non-player-owned stacks are excluded.
- Closed business, removed offer, changed offer price, removed stack or changed ownership is rejected at confirmation.
- BUY continues to use `RetailService` unchanged.
- Successful sale refreshes the UI and CARD display; failure leaves goods and balance unchanged.

### Persistence tests
- Current v10 save/load round-trip preserves sale history and all relevant player state.
- v9 save initializes an empty sale history while preserving supported current player state.
- Malformed individual sale records do not discard otherwise valid current-format player state.

## Explicit exclusions

No dynamic pricing, automatic resale multiplier, `ItemValueCalculator` transaction formula, bargaining, reputation discounts, business budgets, finite stock, restocking, NPC shopping/spending AI, passive sales, operating costs, dirty-money cleaning, laundering, criminal progression, police/heat, or acceptance rules based on stolen/quality/condition/authenticity state.

## Approval gate

**Q21 is approved. This specification is finalized for implementation planning. No code has been changed as part of this design update.** The implementation itself begins only after the user explicitly approves proceeding with this specification.
