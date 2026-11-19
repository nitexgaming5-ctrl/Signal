# ECONOMY-PHASE-3 — businesses and retail

**Status:** approved for implementation by the user on 2026-11-19  
**Branch:** `chore/collaboration-workflow-and-audit`  
**Depends on:** Economy Phases 1–2 (asset ownership/transfers, clean account and save v8)

## Player-facing outcome

The player can visit an ordinary business during its authored opening hours, browse its predefined merchandise and purchase goods with clean bank/card money. The business owner's personal clean-money account receives the revenue. The existing player-to-shop selling behaviour is retained and integrated without inventing new acceptance or valuation rules.

This is the first reusable business/retail layer for the shared city economy. It is not a shop-management simulator.

## Approved design decisions

- Business merchandise is predefined by authored business/NPC role. There are no suppliers, deliveries, ingredients, skills, production chains or crafting requirements.
- Prices are authored fixed values for now. Reputation-based price changes may be added later, but no dynamic pricing is implemented here.
- Every resident has a personal clean-money account. NPCs do not need shopping, spending or financial AI.
- Business revenue from player purchases is credited directly to the owner's personal account. No separate business revenue balance is introduced.
- Existing player-to-shop selling behaviour is retained; Phase 3 must not invent new valuation or acceptance rules.
- Merchandise availability is effectively unlimited for now. Purchases do not deplete catalogue availability. Finite stock and restocking are deferred.
- Economy updates are event-driven. No continuous/hourly simulation, automatic sales, resident shopping or passive business revenue.
- Dirty money remains a physical item, separate from clean accounts. Legitimate retail accepts clean account money only. Cleaning remains a separate future operation that consumes dirty items and credits a clean account directly.

## Business content definition

Create a reusable authored business definition (ScriptableObject) containing:

- stable business identity;
- display name and business type/category;
- stable owner resident reference/ID;
- city location and interaction point;
- authored opening schedule;
- merchandise entries referencing ItemDefinition assets directly, each with a fixed integer clean-money price.

Do not use string item IDs. A business catalogue is content data, not a physical inventory. Its merchandise is available whenever the business is open; no item stock container is created and no stock quantity is persisted.

A business may share its owner with an NPC's public role, but the framework must not assume that all residents own businesses or that every business is legitimate.

## Resident accounts

- Use the existing CleanMoneyAccount implementation for both player and residents.
- Register one account per stable resident economic actor ID (the existing actor convention is `resident:<npc-id>`).
- PlayerStats remains the compatibility owner of the player's account; the shared account registry references that same account rather than creating a duplicate.
- Authored resident starting balances are applied only when an account is first initialized.
- Saved balances override authored starting balances on load. Repeated loads and repeated registration must never grant starting money again.
- NPC accounts are passive stored balances in this phase. No NPC purchasing, wages, expenses, automatic income or financial AI.

## Purchase rules

The retail purchase operation must:

1. Revalidate business opening hours and the selected catalogue entry at confirmation time.
2. Resolve the customer's and business owner's clean accounts.
3. Validate quantity, price and total using overflow-safe integer arithmetic.
4. Validate that the customer has sufficient clean balance and enough inventory capacity.
5. Add the purchased merchandise using the ItemDefinition's authored default instance state.
6. Debit the customer and credit the owner.
7. Record one durable retail transaction containing stable transaction ID, business/owner/customer IDs, item definition ID, quantity, total clean amount and in-game timestamp.

The operation is atomic. On any failure, inventory, both account balances and the retail ledger must be restored to their pre-operation state. A failed or repeated confirmation cannot create free items, lose money or duplicate a transaction.

The catalogue is unlimited, so purchases create customer-owned item instances; they do not transfer a pre-existing finite stock stack from a business inventory. Keep this distinct from physical asset transfers in the existing AssetTransferService ledger.

## Existing sell-to-shop behaviour

Retain the existing Shop/ShopOrder selling rules and valuation exactly. Do not add a new price formula, acceptance category, stock model or shop budget rule as part of Phase 3. Integrate the existing sale entry point with the business context where the active runtime already exposes it; do not break the existing debug/test path. A future authored shop-purchasing budget or business expense rule requires separate approval.

## Runtime integration

- Provide a reusable business/retail runtime that resolves authored business definitions and checks opening schedules.
- Integrate the existing CornerShop as the first business using authored definition data; avoid a per-shop implementation.
- Reuse existing UGUI conventions and GameSession composition. A compact panel should show business name, product names, authored prices, quantity selection (or a simple one-unit purchase if quantity controls are not already supported), confirm and close.
- Keep movement disabled while the panel is open. Revalidate all purchase conditions when confirming.
- The owner must be a registered resident account. Missing definitions, owner accounts or item definitions should fail visibly and safely rather than silently minting content.
- Do not add new NPC behaviour. The shop may be available according to its business schedule independently of whether the owner is physically standing at the counter.

## Persistence

- Advance save format v8 → v9 for resident account balances and retail transaction history.
- Preserve all v1–v8 migration behaviour and the player's clean balance, inventory, storage, item knowledge, discoveries, opportunity state, relationships, asset ownership and asset-transfer ledger.
- Save resident account IDs and balances. Do not save authored business definitions, opening schedules, prices or unlimited catalogue entries.
- Register authored resident accounts once, then apply saved balances. Residents introduced after an older save receive their authored initial balance once; existing saved residents retain their saved balance.
- Persist retail transaction history separately from the physical asset-transfer ledger. Validate IDs, actors, item definition, quantity, amount and time while loading; skip malformed individual records without rejecting otherwise valid player data.
- Repeated load must not duplicate account initialization or transaction records.

## Tests and acceptance criteria

Core EditMode tests:
- account registry references the player's existing account and provides independent resident accounts;
- account registration is idempotent and authored starting balance is granted once;
- account credit/debit rejects invalid values and overflow;
- valid business catalogue and opening-hours checks; invalid/missing owner or merchandise rejected;
- successful purchase debits customer, credits owner, adds the correct item state and records one transaction;
- closed business, missing account, invalid product/quantity/price, insufficient balance and insufficient inventory capacity change nothing;
- forced failure during account/ledger commit restores inventory and both balances;
- repeated confirmation cannot duplicate a purchase;
- retail ledger round-trip and malformed-record handling;
- v8 → v9 migration preserves all previous data and initializes new resident/retail collections empty.

Game/EditMode integration tests:
- existing CornerShop resolves its business definition and owner account;
- the retail panel opens only at the configured interaction point and during opening hours;
- successful purchase updates the player's inventory, CARD balance, owner's account and retail history;
- closing the panel restores movement and normal interactions;
- existing sell-to-shop behaviour remains unchanged.

## Explicit exclusions

No finite stock, restocking, suppliers, deliveries, production, ingredients, crafting/skills, NPC shopping, automatic business sales/revenue, operating costs, wages, dynamic prices, reputation pricing, business budgets, dirty-money retail, cleaning, illicit exchange, laundering, criminal progression, police/heat or new NPC scenario.

## Verification

The assistant cannot run the user's Unity Editor. Report code inspection, tests authored, Unity compilation, EditMode execution and Play Mode verification separately. User verification must include compilation, full EditMode suite, a purchase at the CornerShop, account/ledger checks, save/load, and a check that existing sell-to-shop behaviour remains intact.
