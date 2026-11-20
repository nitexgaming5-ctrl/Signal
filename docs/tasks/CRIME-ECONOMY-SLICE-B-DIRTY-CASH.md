# CRIME-ECONOMY-SLICE-B — Dirty Cash, illicit exchange and contact-specific cleaning

**Status:** design specification; implementation not started  
**Parent:** [CRIME-ECONOMY-001 shared-city design](CRIME-ECONOMY-001-SHARED-CITY-DESIGN.md)  
**Prerequisite:** CRIME-THEFT-001 resident-owned container theft  
**User approval:** Slice B approved; contact identity convention, Dirty Cash denomination and cleaning fee confirmed on 2026-11-20.

## Player outcome

The player can take a stolen physical item to one discreet neighbourhood contact, exchange the specifically accepted item for physical Dirty Cash, then use that same contact's separate cleaning offer to convert Dirty Cash into clean CARD. The loop is visible, transactional, persistent and uses the existing shared city and inventory systems.

This is one authored vertical slice, not a general black market, vendor framework or city-wide laundering simulation.

## Confirmed design decisions

- Add one new resident/contact to the existing shared city and NPC framework.
- Internal NPC ID: `Clean_Dirtycash`.
- Visible `DisplayName`: empty string. Do not replace the blank name with the internal ID in player-facing UI.
- Dirty Cash is a physical inventory item, not an account balance.
- One Dirty Cash unit has a face value of 10 clean-money units.
- Cleaning retains 20% as the contact's fee. One Dirty Cash unit therefore credits 8 clean CARD.
- Cleaning credits the player's existing clean CARD account immediately.
- Legal retail and clean CARD purchasing remain unchanged. Dirty Cash cannot be used in legal retail or deposited directly.
- One fixed, authored fence offer and one fixed, contact-specific cleaning offer.
- No dynamic pricing, bargaining, stock simulation, passive laundering, automatic cleaning, or separate underground city.
- Transactions must be safe across save/load and atomic on failure.

## First authored illicit exchange — one remaining content value

The first fence offer must accept exactly one explicitly authored stolen item definition and pay a fixed whole number of Dirty Cash units. The target item and payout have **not** been approved yet. Do not silently reuse a generic placeholder as final content or invent a payout. The recommended implementation fixture is one item already present in the theft test content, with the offer isolated in a ScriptableObject so it can be replaced without changing Core rules.

The offer must require `ItemOwnershipStatus.Stolen`; a legitimate copy of the same ItemDefinition is not accepted. The initial transaction consumes the exact whole stack (matching the current whole-stack transfer foundation), transfers ownership of the sold asset to the contact, and creates the authored Dirty Cash quantity in the player's carried inventory. If the player cannot fit the Dirty Cash reward, the entire exchange is rejected with no item loss, reward, ownership change or ledger entry.

**Open user decision:** select the first accepted stolen ItemDefinition and its fixed Dirty Cash payout. The denomination is 10 per unit; the payout is expressed as a number of those units.

## Cleaning offer

- Accept only Dirty Cash item stacks.
- Show the exact clean CARD payout before confirmation.
- Consume the selected whole Dirty Cash stack and credit CARD immediately.
- Formula: `clean payout = dirty units × 10 × 80%`. Thus each unit pays 8 CARD.
- Use integer arithmetic with overflow/invalid-quantity checks. No partial/fractional unit conversion.
- If the clean account cannot accept the result, or any precondition fails, leave the Dirty Cash stack and CARD balance unchanged.
- Cleaning is a separate operation from the fence purchase and from legitimate retail.
- No risk, delay, witness event, heat award or progression award in this first slice. These can be designed later as explicit mechanics.

## Player-facing interaction

- The contact is an ordinary resident entity in the existing CityBlock/NPC framework. No separate map, underground district, vendor subsystem or new NPC simulation model.
- Internal ID is stable and non-localized. The player-facing name remains blank; interaction affordance can use neutral authored text such as an unnamed contact prompt without revealing the internal identifier.
- A compact contact panel exposes the fixed fence offer and the cleaning action. It must show accepted item/state, Dirty Cash quantity, cleaning fee and exact CARD payout before the player confirms.
- Revalidate proximity, offer identity, ownership/state, exact stack quantity, capacity and account state at commit time.
- No offer appears as available when its preconditions are not met; explain why the action is unavailable without exposing internal IDs.

## Technical implementation specification

### Core
- Add a small Dirty Cash denomination/value rule that cannot be confused with clean account balance.
- Add dedicated illicit-exchange and cleaning transaction result/ledger models; do not overload `ShopSaleTransaction`, `RetailTransaction` or clean-money retail services.
- Add services for the exact fence exchange and exact Dirty Cash cleaning operation.
- Snapshot and rollback all affected inventories, ownership records and clean account balance on any failed step.
- Preserve stable ItemInstanceIds and ItemInstanceState for the sold stolen item. The Dirty Cash reward is a new physical item instance with player ownership.
- Validate that the fence item is Stolen and Player-owned, and that the player owns the Dirty Cash being cleaned.
- Keep Core plain C# with no UnityEngine references.

### Game/content
- Add a Dirty Cash `ItemDefinition` with denomination documented in content metadata/description; it must be included in the catalog and save resolver.
- Add an authored contact/offer definition. Keep the internal NPC ID and blank visible display name distinct.
- Reuse the existing `NpcCharacterDefinition`, `CityBlockBootstrap`, `GameSession`, item catalog, carried `GridInventory`, `AssetOwnershipBook` and `EconomicContainerRegistry` patterns.
- Add a player-facing contact panel with exact preview and explicit confirmation; do not use a debug-only shortcut.
- No new package, third-party code or asset pack.

### Persistence
- Dirty Cash itself persists through the existing normal inventory item save path and stable item IDs.
- Persist transaction history for both exchange and cleaning in a dedicated save representation, with current-format round-trip coverage.
- Increment save schema only if new serialized fields are added; migrate v13 saves to empty new transaction histories without changing existing inventory, account, crime or searchable-container state.
- Rebuild ownership/custody registrations consistently on load.

## Acceptance criteria

1. The contact is present in the shared city and can be interacted with through the normal proximity path.
2. Its internal NPC ID is `Clean_Dirtycash`; its visible display name is blank and never leaks into UI.
3. A legitimate item of the accepted definition is rejected by the fence.
4. A stolen, player-owned accepted item can be exchanged for exactly the authored Dirty Cash quantity.
5. Insufficient carried-inventory capacity rejects the entire exchange without changing either side.
6. The sold item retains identity/state and ownership changes to the contact; Dirty Cash is a new player-owned physical stack.
7. Dirty Cash cannot pay for legal retail or be credited directly to CARD.
8. Cleaning preview shows the 20% fee and exact payout; each Dirty Cash unit credits 8 CARD.
9. Cleaning consumes the exact selected Dirty Cash stack and credits CARD atomically; failure leaves both unchanged.
10. Repeated confirmation cannot duplicate reward or payout.
11. Save/load preserves Dirty Cash inventory, clean CARD balance, ownership and both transaction histories.
12. v13 save migration remains valid and initializes new history collections empty.
13. Existing retail, sell-to-shop, scavenging and theft behaviour remains unchanged.
14. Tests cover validation, ownership, item state, arithmetic, capacity, rollback, duplicate attempts, transaction history and save/load.

## Out of scope

- Additional contacts, fences, accepted items or cleaning rates.
- Dynamic market prices, negotiation, reputation modifiers or business stock.
- Heat, witnesses, police, risk, exposure or cleaning delays.
- Passive/automatic laundering or periodic conversions.
- Dirty-money use in legal retail or clean CARD purchases.
- Generic black-market framework beyond what this one authored vertical slice requires.
- Production/workspaces/recipes (Slice C).

## Verification plan

- Unity compilation in the Editor.
- Full EditMode suite, recording the actual count.
- Play Mode: obtain a stolen item, meet the contact, preview and complete fence exchange, verify Dirty Cash identity/count/ownership, clean it, verify exact CARD change, then save/reload.
- Negative Play Mode checks: legitimate item rejection, no room for reward, empty/invalid cleaning selection, insufficient or invalid state, repeated confirmation.
- Regression checks: legal shop BUY/SELL, street-bin scavenging and apartment theft/witness heat.

## Remaining approval

Only the first fence offer's accepted ItemDefinition and fixed Dirty Cash unit payout remain unselected. All other Slice B boundaries above reflect the user's approval.
