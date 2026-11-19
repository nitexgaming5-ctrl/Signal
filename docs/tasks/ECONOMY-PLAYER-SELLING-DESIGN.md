# ECONOMY — player-facing sell-to-shop (design draft)

**Status:** design proposal; awaiting user approval of Q21 before implementation.  
**Date:** 2026-11-19  
**Depends on:** verified Economy Phase 3; transaction-safety audit (342/342 EditMode tests).

## Player-facing outcome

At an open business counter, the player can switch the existing retail panel between BUY and SELL. SELL shows goods carried by the player and allows them to sell eligible quantities to that business for clean CARD. The flow is local to the shared city: no separate underground market or new map.

This completes the basic two-way retail interaction without turning businesses into a management simulator.

## Existing rules and important limitation

The current Core `ShopOrder` contains `ItemType`, `Quantity` and caller-supplied `UnitPrice`. `Shop.TryFulfil` validates the order, removes matching goods from the inventory and credits the player's clean CARD account. It protects against invalid quantities/items/prices, insufficient goods, oversized totals and failed account credit (inventory rollback).

It does **not**:
- calculate a sale price;
- decide which goods a named business accepts;
- identify a selected physical stack (it removes matching item types, newest stacks first);
- validate `AssetOwnershipBook`;
- record a completed sale transaction.

The old $10 developer sale value is not a complete gameplay pricing policy. `ItemValueCalculator` is an indicative reference valuation only, not an approved buy/sell formula.

Therefore the transaction primitive can be reused, but the game still needs an authored source for per-business acceptance and the unit price supplied to `ShopOrder`.

## Proposed design — pending Q21 approval

### Business buyback offers

Add a separate authored buyback-offer collection to `BusinessDefinition`, distinct from its existing player-facing `Merchandise` catalogue. Each entry references an `ItemDefinition` and a positive fixed clean-CARD unit price.

- Only explicitly listed item definitions are accepted by that business.
- Buyback offers are not inferred from the retail catalogue; a business selling bread does not automatically buy bread back.
- Price is fixed and authored; no automatic `ItemValueCalculator` formula, quality/condition/authenticity/ownership multipliers, reputation, negotiation or market fluctuation.
- No finite shop budget, stock simulation, operating cost, NPC shopping or passive economy simulation.
- A missing offer means the item cannot be sold there.
- The existing ShopOrder and Shop.TryFulfil semantics remain the transaction authority for positive quantity/price, total overflow, inventory removal and clean CARD credit.

This keeps business acceptance explicit while avoiding a new pricing algorithm. The user must approve this model or choose the shared-list alternative in Q21.

## Player interaction

- Reuse the existing `RetailPanel` and business interaction point.
- Add BUY and SELL tabs; BUY remains unchanged and continues to call `RetailService`.
- SELL lists eligible stacks in the player's carried inventory only. Home locker contents are not sellable from the counter; the player must physically carry goods to the shop.
- Show item name, carried quantity, fixed offer price per unit and calculated total for the selected quantity.
- Initial interaction should target one selected stack at a time. Quantity is bounded from 1 to that stack's current count. No cross-stack aggregation.
- Revalidate business identity/opening hours, offer, selected stack, quantity, price, inventory presence and ownership at confirmation time.
- Disable or refresh the row after a successful sale; show clear failure feedback without hiding the goods.
- Closing the panel restores normal movement and interaction, as it does today.

## Ownership and item identity

The inventory currently supports removing quantities by ItemType across stacks, but that is unsafe for a player-facing sale because different stacks can have different state and stable identities. The new sale path must target one exact `ItemInstanceId`.

- Validate that the selected stack still exists in carried inventory and `AssetOwnershipBook` says its owner is `player`.
- Do not trust the UI's displayed count, price or ownership.
- Add a Core inventory operation to remove a requested quantity from one exact stack while preserving its stable identity for any remainder.
- If the full stack is sold, remove its ownership entry after the physical stack is removed.
- If only part is sold, the remaining stack retains the same identity, state and player ownership. No stack split/new identity is required for this first pass.
- Missing or mismatched ownership is a hard rejection; do not silently repair it as part of a sale.
- Preserve the stack's item state. No state-based valuation or acceptance rules are introduced in this slice.

## Money and transaction semantics

- Sales credit the player's existing `CleanMoneyAccount` / CARD only. Dirty-money items are not involved.
- Preserve existing `Shop.TryFulfil` behaviour: the business does not debit a simulated budget/account. The Phase 3 decision against business budgets remains in force.
- No finite shop stock is created or restocked. Sold goods are consumed by the sale operation as they are in the existing helper; they do not become catalogue stock or silently increase inventory.
- Do not record a sale as an `AssetTransferService` transfer: the goods are consumed rather than transferred into a registered destination container.
- Add a dedicated `ShopSaleTransaction` and `ShopSaleTransactionLedger`, separate from the physical asset-transfer ledger and the unlimited-catalogue `RetailTransactionLedger`. Each record should contain stable transaction ID, business ID, seller actor, source item-instance ID, item-definition ID, sold quantity, unit price, total clean amount, item-state snapshot and in-game timestamp.
- Persist the sale ledger with the current save format. A simple v9-to-v10 initialization of an empty sale collection is sufficient; do not build a large historical migration framework.

## Atomicity and failure behaviour

A sale must be all-or-nothing:
1. Validate request, business offer, exact stack, player ownership, quantity, positive unit price and overflow-safe total.
2. Snapshot inventory, player balance and ownership state.
3. Remove the requested quantity from the exact stack.
4. If the stack is fully consumed, remove its ownership record; otherwise keep ownership unchanged.
5. Credit player clean CARD.
6. Record exactly one sale transaction.
7. On any failure after mutation, restore inventory, account balance and ownership to the snapshots; no transaction record remains.

A failed sale must never create money, consume goods, lose ownership, duplicate a record or leave a partial update. UI confirmation is not trusted as validation.

## Required tests

Core EditMode:
- Valid full-stack sale removes the stack, removes ownership, credits exact CARD amount and records one sale.
- Valid partial sale decreases only the selected stack count; preserves its ID, state and ownership.
- Sale cannot consume a different stack of the same ItemType.
- Missing stack, missing ownership, wrong owner, invalid item, zero/negative quantity, quantity above selected stack, zero/negative price and overflow all fail without mutation.
- Insufficient inventory and account-credit overflow fail without mutation.
- Ledger failure/duplicate transaction rolls back inventory, balance and ownership.
- A failed sale creates no ledger record; a successful sale creates exactly one.
- Sales never debit resident/business accounts or alter retail purchase history.

Game/EditMode integration:
- SELL tab appears only at an open, valid business interaction.
- Only carried eligible stacks appear; home storage stacks do not.
- Closed business or changed/removed offer is rejected on confirmation.
- BUY tab still uses RetailService unchanged.
- Successful sale refreshes the UI and CARD display.

Persistence:
- Save/load round-trip preserves sale transaction history.
- v9-to-v10 adds an empty sale history for older development saves.
- Malformed individual sale records are skipped without losing valid player state.

## Explicit exclusions

No dynamic pricing, automatic resale multiplier, ItemValueCalculator-based trade price, bargaining, reputation discounts, business budgets, finite stock, restocking, NPC shopping, passive sales, operating costs, dirty-money cleaning, laundering, criminal progression, police/heat, or new acceptance rules based on stolen/quality/condition/authenticity state.

## Approval gate

**Q21 must be answered before implementation.** Recommended answer: approve the separate per-business fixed-price buyback offer list. The rest of this document is the proposed technical and interaction contract, subject to any changes the user requests.
