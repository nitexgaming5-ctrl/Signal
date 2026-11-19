# ECONOMY-PHASE-3 — first illicit exchange

**Status:** Finalized design specification; awaiting explicit user approval before implementation  
**Initiative:** Signal shared-city economy, Phase 3  
**Prerequisites:** Economy Phases 1–2; save format v8; user-confirmed 316/316 EditMode tests (2026-11-19)

## Player-facing outcome

The player can visit a new discreet seller in the existing shared city and exchange physical dirty-money bundles for one generic contraband item. This is a small, repeatable local interaction that proves the ownership, custody, transfer and transaction foundations in a real gameplay loop.

The seller is a new NPC, not Milo and not part of the Milo Parcel opportunity. The seller and exchange exist in the ordinary city space; there is no separate underground map or new district.

## Approved design decisions

- Dirty money is represented by one physical item definition: **Dirty Cash Bundle**.
- Each bundle has a fixed face value of **100**.
- Each bundle is an indivisible physical asset: MaxStack = 1. Multiple bundles occupy separate inventory slots and have separate stable item-instance GUIDs.
- The first merchandise is one generic **Contraband** item definition. Its specific identity, use and effects are intentionally unspecified.
- The counterparty is a newly authored discreet seller NPC with a distinct economic actor identity.
- Dirty money is accepted only by this illicit exchange. It never enters CleanMoneyAccount and cannot pay for legitimate Shop purchases.
- Clean card balance remains unchanged by this exchange.

## First-slice content boundaries

- One seller, one contraband definition, one authored seller-stock unit initially.
- Seller stock is a real inventory asset owned by the seller, held in a seller-custodied inventory container.
- The seller is placed at an authored interaction point in an existing city region. Do not create a separate underground location, new district or new scene.
- The exact placement and the authored price (an integer number of 100-value bundles) remain content-tuning inputs; implementation must not invent them. The exchange must be disabled with clear configuration diagnostics if the price or placement is invalid.
- The stock does not automatically replenish. No supply/demand simulation or restock schedule is included.
- The seller may use the existing NPC presentation/interaction conventions, but this slice does not add autonomous criminal agendas, rumour chains or a new social scenario.

## Interaction flow

1. Player approaches the seller within the normal interaction range and presses the existing interaction key.
2. A compact confirmation panel displays the contraband item, its authored bundle price, and available stock.
3. Confirm attempts the exchange; Cancel closes the panel without changing any state.
4. On success, the contraband enters the player's carried inventory and the exact required Dirty Cash Bundle assets transfer into the seller's inventory.
5. The UI reports success and explicitly indicates physical dirty cash was used; the CARD balance is not involved.
6. Failure reports a clear reason and leaves the exchange state unchanged.

## Technical design and safeguards

### Physical item and identity

- Author Dirty Cash Bundle as a normal ItemDefinition with stable asset identity, one-cell footprint unless existing item conventions require otherwise, MaxStack = 1, and a fixed authored face value of 100 represented as explicit dirty-cash metadata or a narrowly scoped definition field.
- Do not infer its face value from ItemDefinition.BaseValue; base value is not the currency contract.
- Do not add a dirty-money account, aggregate balance, partial bundle, change-making or split-stack transfer.
- Author Contraband as a normal item definition. Do not assign unapproved effects, recipes, consumption, resale value or criminal progression.

### Seller and custody

- Give the seller a stable EconomicActorId distinct from the player and all existing actors.
- Register the seller stock inventory with EconomicContainerRegistry as seller-custodied. Register the player's carried inventory as player-custodied using the existing GameSession composition.
- Register the initial contraband asset as seller-owned. Each dirty bundle must be player-owned before payment can be accepted.
- Do not treat physical container custody as proof of asset ownership; validate both independently.

### Atomic exchange

The current AssetTransferService handles whole-stack transfers one at a time; it is not sufficient by itself for a two-sided purchase. Add a narrowly scoped Core exchange operation/service on top of the existing ownership and transfer foundation.

Before mutation, validate all of the following:
- valid seller/actor/container configuration and registered custody;
- player is still in interaction range and the seller is available;
- seller owns the offered contraband and it is present in seller stock;
- player owns at least the configured number of Dirty Cash Bundle assets;
- each payment asset is exactly one valid bundle with face value 100;
- player inventory can fit the contraband without rearranging or deleting existing items;
- seller inventory can receive all payment bundles;
- all item identities are unique and all ownership/container mappings are consistent.

Commit both directions as one logical operation:
- transfer the contraband seller → player;
- transfer exactly N whole dirty bundles player → seller;
- record the completed economic transfer records only after both directions succeed.

If any step fails, restore both inventories, the ownership book and transaction ledger to their pre-exchange state. No partial payment, free merchandise, duplicated/lost item GUID, orphaned ownership record or partial ledger history may survive. Prefer preflight plus a staged/batched commit; rollback must cover every mutable participant, not just the inventory grids. The API must return a structured failure reason and must not silently retry or mutate unrelated inventory contents.

### Persistence

- Existing save v8 persists player clean balance, player-held ownership mappings and economic transfer history, but does not yet cover this new seller stock container.
- Add a versioned migration to save v9 for the seller's persistent stock inventory and any seller-owned item state needed to reconstruct it. Preserve all v1–v8 migration behavior.
- On load, reconstruct/register the seller actor and seller-custodied stock container before restoring or validating its ownership mappings. Ensure the contraband's ownership and seller stock agree.
- Save/load after a successful purchase must not restock the contraband, recreate spent dirty bundles, duplicate assets or alter the clean card balance.
- Malformed/missing seller stock data must fail safely with a clear diagnostic; never silently mint a replacement item into a save that records the stock as sold.

### UI and runtime integration

- Reuse the current interaction prompt, UGUI style and GameSession composition patterns.
- Keep the purchase panel limited to the one offered item, price in bundles, stock availability, Confirm and Cancel.
- Revalidate all conditions on Confirm; do not trust values captured when the panel opened.
- Prevent double submission while a transaction is in progress.
- Do not open a second panel or permit the same stock item to be sold twice.
- No new global singleton, scene architecture or external package.

## Tests and acceptance criteria

Core EditMode tests:
- valid exchange transfers contraband and every required bundle with stable GUID/state preserved;
- correct ownership changes for both directions;
- insufficient bundles, wrong bundle definition/value, unowned payment, missing/unowned stock and unregistered containers fail without mutation;
- full player inventory and full seller inventory fail without mutation;
- duplicate identities, malformed configuration and invalid price fail safely;
- injected/forced failure during the second transfer restores both inventories, ownership and ledger;
- ledger contains all and only completed transfers for a successful exchange;
- repeated confirm cannot sell the same stock twice.

Save tests:
- v8 → v9 migration preserves all existing clean balance, player inventory/storage, ownership and transaction data;
- seller stock and ownership round-trip correctly;
- a completed purchase remains completed after save/load and does not restock;
- malformed seller stock records do not mint assets or corrupt unrelated player data.

Unity verification by the user:
- compile in Unity;
- run the full EditMode suite;
- Play Mode: approach seller, open/cancel panel, complete purchase, verify inventory and seller stock, verify CARD unchanged, save/load and verify the purchase remains completed.

## Explicit exclusions

No laundering/cleaning interaction, direct dirty-money deposit, legitimate purchase with dirty money, change-making, partial bundle transfer, multiple vendors, broad market/business framework, dynamic prices, restocking simulation, heat, police, witnesses, exposure, bribes, criminal progression, new quest/opportunity, or new drug-specific gameplay rules.

## Approval gate

This specification is ready for review. **Do not implement any gameplay code until the user explicitly approves this final specification.** Any change to the denomination, seller, merchandise category, dirty/clean separation or scope requires another design confirmation.
