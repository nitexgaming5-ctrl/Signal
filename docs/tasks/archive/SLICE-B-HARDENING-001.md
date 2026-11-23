# SLICE-B-HARDENING-001 — close review gaps in the Dirty Cash slice

**Status: completed (user-reported verification, 2026-11-20).** The user reported 396/396 EditMode tests passed and subsequently reported the gameplay test passed. The exact individual Play Mode subcases were not enumerated; this card is closed at the level of that report, without claiming unreported steps.

**Parent:** `CRIME-ECONOMY-SLICE-B-DIRTY-CASH.md`. **Origin:** 2026-11-20 code review of save schema v14.
**Scope:** no change to economy rules, content values, save fields or UI behaviour. If a test exposes a real bug, stop and report it before fixing.

## H1 — tests for acceptance criteria that lacked coverage
Inspect the real APIs (`DirtyCashTransactions.cs`, `RetailService.TryPurchase`, `Shop.TryFulfilExact`) and write meaningful assertions:
1. A second cleaning attempt with the consumed cash instance ID returns `ItemNotFound`; player/contact balances and ledger count stay unchanged.
2. Re-exchanging the sold stack ID fails with no second reward and ledger count stays unchanged.
3. Confirm the fee split: 3 Dirty Cash units → player +24 CARD, contact +6 CARD (existing cleaning test already asserts this).
4. Each new ledger rejects a duplicate `TransactionId`.
5. A carried Dirty Cash stack cannot fund a retail purchase; the clean account is the only payment source and the physical stack/ownership remain untouched.
6. `Shop.TryFulfilExact` cannot sell a Dirty Cash stack against an offer for another item; the stack remains unchanged.

Implemented in this batch: four new regression tests cover replay protection, duplicate IDs in both ledgers, retail payment separation and shop-sale item matching. The existing cleaning test already asserts the exact 24/6 split.

## H2 — one source of truth in `Game/GameSession.cs` (behaviour-identical)
- Centralize the contact NPC ID and resident account actor ID in named constants/readonly identity.
- Centralize the shared `1.85f` interaction distance in one named constant.

## H3 — no asset names shown to the player
The contact panel must use neutral `Unknown item` when an item's display name is blank or its definition cannot be resolved; never show the ScriptableObject asset filename.

## Out of scope / not approved
Splitting `GameSession`, any new contact/offer/rate, cleaning risk/heat/delay, or UI redesign.

## Verification (user) — completed 2026-11-20
- User reported the full EditMode suite passed **396/396**.
- User subsequently reported the gameplay test passed.
- No Unity test execution was performed by the assistant. The user's report is the verification authority for this closure.
- The user did not enumerate every negative-case or save/reload step; do not claim those individual cases were tested unless separately confirmed.
