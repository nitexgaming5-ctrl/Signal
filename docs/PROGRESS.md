# Progress — current repository truth

Last reconciled: **2026-11-23**. Historical detail is preserved in `docs/progress/LOG-2026-11.md`. This file is the concise status board, not an implementation diary.

## Current state

- Unity 6000.6.9f1, 2D URP. Active architecture: `Signal.Core` (plain C#) + `Signal.Game` (Unity layer). Active scene: `Assets/_Project/Scenes/HideoutSlice.unity`.
- Working branch: `chore/collaboration-workflow-and-audit`. Keep `main` untouched.
- Save schema: **v15**. It persists production history, contract lifecycle, workshop upgrade IDs, contract receiver inventory and receiver actor identity in addition to previous player/world/economy state.
- Latest user-validated EditMode result: **415/415 passed (2026-11-23)**. The earlier 413/415 report had two stale test defects; both fixtures/assertions were corrected in commit `b6bd50a`, and the user confirmed the subsequent full run passed.
- The 415/415 result verifies the code/test state at that run. The subsequent First Workshop assets and scene wiring do not alter C# code, but Unity import and Play Mode verification of those new references/content remain a separate check.
- Active gameplay initiative: **LIVELIHOOD-B1-B3 — First Workshop**, following approved Direction B.

## Active player-facing initiative

**Outcome:** the player uses the hideout workbench to produce physical goods, accepts one authored customer request, hands over the requested goods for clean CARD, and improves the workbench to unlock another recipe.

Approved structure:
- Player-operated hideout workshop first; Core remains compatible with NPC-owned production later.
- Authored recipes and physical inventory outputs.
- Immediate production for the first slice.
- One accepted/active customer contract at a time.
- Whole-stack physical delivery to the issuer's receiving inventory.
- Fixed clean CARD payment from the issuer's existing resident account.
- One practical upgrade unlocks one additional authored recipe/capability.

Implementation foundation already exists:
- Core actor-aware `ProductionService` and production history.
- Existing `WorkbenchWorkspace` extended with recipe book, production feedback and upgrade panel.
- `CustomerContractBook`, atomic fulfilment and customer interaction panel.
- `WorkshopProgressionBook`, one-time upgrade acquisition and visible installed-prop hook.
- Save v15 persistence, v14 migration and focused Core/EditMode coverage.

Starter content is now authored and wired into HideoutSlice: four workshop materials, two patch-kit products, two recipes, Milo's single customer contract, Milo/Rhea resident definitions and the Compact Sewing Machine upgrade. The assets and scene references are committed; do not rebuild the systems above unless source inspection shows a concrete defect.

## Implemented foundations — reuse them

| Area | Existing foundation | Status |
|---|---|---|
| Hideout / city | Walkable hideout, Street, CornerShop, Apartment, same player/session across transitions | user-tested foundation |
| Inventory | Spatial grid, rotation, stack movement, home storage, dual-panel container interaction | user-tested foundation |
| Item state | Stable stack GUIDs; quality, condition, authenticity, ownership; partial player knowledge and item viewer | implemented |
| Time / rent / heat | GameClock, RentLedger, HeatMeter, PlayerStats / clean money account | implemented; some tuning/consequences remain provisional |
| NPCs | Shared social world, authored routine profiles, deterministic utility brain, generic presenter, navigation and scoped knowledge | implemented foundation; scenario depth remains future |
| Discovery / narrative | Player discovery ledger, relationships, interaction conditions, Milo parcel opportunity | implemented foundation |
| Economy | Asset ownership, container custody, atomic transfer, resident accounts, retail BUY and fixed SELL offers | user-tested foundations |
| Scavenging | Stable physical bins, authored loot profiles, progressive reveal, save/load and transfer UI | user-tested (368/368 milestone) |
| Theft | Resident-owned physical container, Stolen state, crime outcomes, witness-scoped knowledge | implemented; post-fix witness/heat Play Mode retest is a parked verification item |
| Dirty Cash | Physical currency item, unnamed contact, fixed exchange and cleaning, separate histories and persistent contact inventory | user-tested (396/396 milestone) |
| First Workshop | Production, one-active contract, physical delivery, issuer-paid reward, practical upgrade and recipe unlock | code foundation + 415/415 EditMode baseline; content/Play Mode completion tracked separately |

## Current verification

- **EditMode:** 415/415 passed, user-validated 2026-11-23 after commit `b6bd50a`.
- **Unity compilation:** user ran the test suite successfully after the corrections; no compile blocker was reported.
- **First Workshop Play Mode:** still needs direct end-to-end exercise with the authored assets: craft, accept, fulfil, acquire upgrade, observe second recipe, save/reload.
- **Theft witness correction:** witnessed and unwitnessed heat behavior should be retested in Play Mode after the prior heat fix if not already covered.
- Do not claim a Play Mode verification that the user has not reported.

## Current next action

Open the updated project in Unity, allow all new ScriptableObjects/prefab references to import, run the full EditMode suite, then exercise craft twice → deliver one kit to Milo → buy the sewing machine from Rhea → craft the reinforced kit → save/reload. Keep the user-validated 415/415 result recorded as the last full suite; rerun after any future code changes.

## Not included / deliberately deferred

No automatic NPC shopping, supplier simulation, finite universal stock, dynamic prices, supply/demand, passive income, production timers/queues, quality rolls, procedural contracts, multiple active contracts, partial-stack delivery, repair system, relationship/favour rewards, police response, global rumour cascades or new districts are part of the current initiative.

## Documentation maintenance

- Update this summary when implementation or verification state changes.
- Append dated session notes to `docs/progress/LOG-2026-11.md`.
- Keep Open Questions genuinely open; move answered decisions to `docs/decisions/` or the historical archive.
- Read `docs/INDEX.md` for the full game scope and documentation map.
