# ECONOMY-PHASE-3 — businesses and retail

**Status:** implemented and user-verified; 335/335 EditMode tests passed (2026-11-19)  
**Branch:** `chore/collaboration-workflow-and-audit`  
**Depends on:** Economy Phases 1–2 (asset ownership/transfers, clean account and save v8)

## Player-facing outcome

The player can visit an ordinary business during its authored opening hours, browse its predefined merchandise and purchase goods with clean bank/card money. The business owner's personal clean-money account receives the revenue. The existing player-to-shop selling rules are retained without inventing new acceptance or valuation rules. The active GameSession currently has no player-facing sell entry point to integrate.

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

Retain the existing Shop/ShopOrder selling rules and valuation exactly. Do not add a new price formula, acceptance category, stock model or shop budget rule as part of Phase 3. The current active GameSession does not expose a player-facing sale entry point. Keep the existing Shop/ShopOrder Core API and tests unchanged; do not add a new sale UI or alter its rules in this phase. A future authored shop-purchasing budget or business expense rule requires separate approval.

## Runtime integration

- Provide a reusable business/retail runtime that resolves authored business definitions and checks opening schedules.
- Integrate the existing CornerShop as the first business using authored definition data; avoid a per-shop implementation.
- Reuse existing UGUI conventions and GameSession composition. A compact click-driven panel should show the business name and one merchandise row per catalogue entry, with its authored price and a BUY button. Purchases are one unit per click for this first pass; include visible paging when the catalogue exceeds the page size and a clear close action.
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


## Implementation checkpoint — first pass added (2026-11-19)

### Added to the working branch
- Core EconomicAccountBook: one clean account per economic actor; player registration shares the existing PlayerStats.CleanMoney instance; resident account creation is idempotent and stores authored initial balances.
- Core RetailService: atomic catalogue purchase coordinating customer debit, owner credit, inventory grant, new asset ownership registration and rollback.
- Core RetailTransactionLedger: durable purchase history distinct from the physical asset-transfer ledger.
- Save v9: persists resident account balances and retail transactions; v1–v8 migration initializes these collections safely. Legacy loads reset resident accounts to authored initial balances before saved v9 balances are applied.
- Game content/runtime: NpcCharacterDefinition.StartingCleanBalance, reusable BusinessDefinition, click-driven RetailPanel with BUY buttons, and GameSession composition for resident accounts, business interaction, purchase handling and catalogue item resolution.
- Tests authored for account isolation/initialization, purchases, rollback cases, business opening hours and v9 persistence/malformed records.
- Existing Shop / ShopOrder selling rules were left unchanged.

### Remaining integration and verification
- `Assets/_Project/Content/Businesses/Business_CornerShop.asset` is now created and assigned to `GameSession.Businesses` in `Assets/_Project/Scenes/HideoutSlice.unity`. It targets Milo at `CornerShop`, opens 06:30–18:00, and uses interaction point (1000, 1000.6).
- Added `Assets/_Project/Content/Items/Item_Bread.asset` and referenced it from the Corner Bakery catalogue at CARD $5. This is a temporary test value chosen by the user; it is not a final economy balance. Bread is 1×1, stack limit 10, and uses ordinary default item state.
- GameSession validates business identity, owner/location data and catalogue entries before opening/purchasing; invalid definitions fail visibly. Existing `Shop` / `ShopOrder` sell-to-shop Core behavior and tests remain unchanged; no new sale UI or valuation rule was introduced.
- The assistant cannot run Unity. All newly authored EditMode tests remain unexecuted; Unity compilation, full EditMode suite and Play Mode purchase/save-load checks are pending user verification. The next test should buy one Bread and confirm CARD -$5, Milo's resident account +$5, inventory grant, retail ledger entry and persistence after save/load.


## 2026-11-19 — Click-driven retail panel and test starting balance

- Replaced the F2–F10 purchase shortcuts with clickable merchandise rows and adjacent BUY buttons. The panel also has clickable previous/next page and close controls, a scrollable catalogue viewport, purchase feedback and ESC-to-close support. The panel remains data-driven for other businesses; each BUY action still calls the same validated atomic RetailService path.
- Ensured the runtime-created UGUI canvas has a GraphicRaycaster and an EventSystem using InputSystemUIInputModule so mouse clicks are routed through the project's Input System.
- Set the active HideoutSlice GameSession serialized starting clean balance to **$5,000 CARD** as a temporary testing seed. This is not an economy-balance decision. A previously saved game will restore its saved balance when loaded; use a fresh game/new save to see the $5,000 starting value.
- Unity compilation, EditMode tests and Play Mode click/purchase verification remain pending user validation.


## Verification update — 2026-11-19

The user confirmed the full Unity EditMode suite **335/335 passed** and reported that all Phase 3 functionality works as intended in Unity. Phase 3 is complete.

The subsequent transaction-safety audit did not change the RetailService transaction design or any approved shop-sale valuation/acceptance rule. It added:
- Fail-fast rejection in `Shop.TryFulfil` for an uninitialized item definition and a zero-value order.
- Regression tests proving those invalid sale requests do not mutate inventory.
- RetailService parameterized regression tests for zero/negative quantity, zero/negative price and multiplication totals above `int.MaxValue`, asserting no account, inventory, ownership or ledger mutation.

These newest tests are **not yet Unity-verified**. Expected full EditMode suite count: 342 test cases (335 previously verified plus 7 added cases).

Audit boundary: `Shop.TryFulfil` is currently called only by the developer sale control. It does not yet consult AssetOwnershipBook or record a completed economic transaction. The existing behavior is retained; a player-facing sale system must add those economic guarantees through a deliberately designed integration rather than by changing this legacy helper's valuation/acceptance rules in this phase.
