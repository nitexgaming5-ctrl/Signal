# Progress — current repository truth

Last reconciled: **2026-11-24**. Historical detail is preserved in `docs/progress/LOG-2026-11.md`. This file is the concise status board, not an implementation diary.

## Current state

- Unity 6000.6.9f1, 2D URP. Active architecture: `Signal.Core` (plain C#) + `Signal.Game` (Unity layer). Active scene: `Assets/_Project/Scenes/HideoutSlice.unity`.
- Working branch: `chore/collaboration-workflow-and-audit`. Keep `main` untouched.
- Unity application version: **0.01**. Save files are unversioned development snapshots: no schema version field, version gate or automatic migration chain. Older experimental saves are not guaranteed to remain compatible after data-shape changes.
- Latest completed full EditMode result reported by the user: **415/415 passed (2026-11-23)**. That run predates the Rhea supplier-panel integration now on this branch; rerun the full suite after importing the current patch. The earlier 413/415 report had two stale test defects corrected in `b6bd50a`.
- The user has reconfirmed **415/415 passed** and reports that the project is running, with the workbench, selling and customer-request flow working in Play Mode. The First Workshop is now beyond a code-only milestone; the remaining acceptance pass is to explicitly confirm the visible upgrade, Reinforced Patch Kit unlock/production and save/reload persistence before calling B-01 to B-03 complete.
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
- Existing `WorkbenchWorkspace` extended with recipe book and production feedback; separate Rhea-facing supplier offer panel handles upgrade purchase.
- `CustomerContractBook`, atomic fulfilment and customer interaction panel.
- `WorkshopProgressionBook`, one-time upgrade acquisition and visible installed-prop hook.
- Current-state save/load persistence for inventory, economy, contracts, workshop progression and NPC agenda progress, with current-build round-trip coverage.

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

## Current content reference

- Starter catalogue, exact recipes, contract/upgrade details and workbench playtest: `docs/content/STARTER_CATALOGUE.md`.
- Durable content decisions and replaceability rationale: `docs/decisions/0005-first-workshop-content.md`.

## Current verification

- **EditMode:** the user's 2026-11-24 run reported **434/435**. Its only failure was the obsolete `VersionFourteenSaveMigratesWithEmptyLivelihoodState` assertion, which expected schema v15 while the code had advanced to v16. The save-version field, migrator and migration-only tests are now removed; a fresh Unity compile and full EditMode run are required.
- **Unity compilation / asset import:** the sewing-machine placeholder's reserved root `fileID 100100000` was corrected to `100000`, including Transform/SpriteRenderer back-references and the upgrade asset reference. The prefab GUID is unchanged. The user subsequently reported the project running and the core workshop/customer loop working; no further prefab import blocker has been reported.
- **First Workshop Play Mode:** user-confirmed workbench, selling and customer-request flow. The Rhea supplier interaction now routes through her agenda conversation before opening the existing offer panel. Confirm `E Talk to Rhea`, choose to inspect the offer, purchase/install the Compact Sewing Machine, then speak to her again for mending advice. Unlock and craft the Reinforced Patch Kit, then F5/F9 save/reload and confirm balances, inventory, contract, agenda progress, installed upgrade and recipe unlock.
- **Theft witness correction:** witnessed and unwitnessed heat behavior should be retested in Play Mode after the prior heat fix if not already covered.
- Do not claim a Play Mode verification that the user has not reported.

## Newly approved initiative — NPC Agenda & Interaction Threads (2026-11-24)

- Approved reusable, data-authored NPC agenda/thread framework with Rhea as the first pilot.
- Rhea's three independent threads: sourced Milo tip; Compact Sewing Machine offer using the existing supplier/upgrade service; post-sale mending advice that foreshadows B-04 without implementing repairs.
- Preserve Milo parcel physical lifecycle mechanics. Migrate only Rhea's conversational tip and any conversation presentation that cleanly belongs in the reusable framework.
- Architecture contract and implementation sequence: `docs/NPC_FRAMEWORK.md`, section **NPC Agenda & Interaction Threads**. A dedicated task card tracks acceptance criteria.
- Core `NpcAgendaProgressBook`, current-state SaveMapper persistence, GameSession save/load wiring and focused round-trip tests are implemented.
- Added `NpcAgendaDefinition` authoring assets, deterministic `NpcAgendaInteractionDirector`, reusable `NpcAgendaConversationPanel`, and Rhea's three authored threads under `Resources/Agendas/RheaAgenda.asset`. Stage selection supports priority, activity/place, discovery, relationship thresholds, opportunity phase, contract completion and acquired/not-acquired upgrades.
- Rhea's old rumour branch has been removed from `MiloParcelOpportunityController`; physical parcel inspection, acceptance, return, delivery and outcome mechanics remain there. The authored rumour keeps `subjectId = milo` and `sourceId = rhea`, preserving Milo's existing distinct-signal offer condition.
- GameSession loads agenda definitions from its optional inspector array or `Resources/Agendas`, routes nearby conversations through the director, and delegates the machine offer to the existing supplier panel.
- The implementation and focused tests are committed, but the current branch has not yet been compiled or tested in Unity. Import/compile, run the full EditMode suite, then Play Mode verify Rhea's rumour at Street, her supplier offer after Milo's contract, her mending follow-up after purchase, and save/load progress.

## Current next action

Pull the current branch and let Unity import/compile the agenda framework and save-version cleanup. Run the full EditMode suite; the user's 434/435 report predates the removal of save-version code and migration-only tests, so it is not verification of this branch. The total test count will be lower because migration-only tests were deliberately removed; judge the new run by failures, not by matching the old total. Play Mode: talk to Rhea in the Street to test the sourced rumour choice; complete Milo's request, return to Rhea and verify the repeatable Compact Sewing Machine offer opens the existing supplier panel; buy/install it and confirm Rhea's mending-advice thread becomes available. Save/reload and verify completed conversation choices persist while the repeatable offer remains available until the upgrade is acquired. Also retest the separate supplier prompt/purchase path and First Workshop upgrade persistence. Do not implement B-04 repairs until its rules are approved.

## Known repository layout caveat

- Existing `Assets/_Project/Content/Items/` and `Assets/_Project/Content/items/` differ only by case and may collide on Windows. The new First Workshop definitions were deliberately placed in `Content/WorkshopItems/`. Do not merge/rename the existing folders outside Unity Editor; preserving all asset GUIDs and references is mandatory.

## Not included / deliberately deferred

No automatic NPC shopping, supplier simulation, finite universal stock, dynamic prices, supply/demand, passive income, production timers/queues, quality rolls, procedural contracts, multiple active contracts, partial-stack delivery, repair system, relationship/favour rewards, police response, global rumour cascades or new districts are part of the current initiative.

## Documentation maintenance

- Update this summary when implementation or verification state changes.
- Append dated session notes to `docs/progress/LOG-2026-11.md`.
- Keep Open Questions genuinely open; move answered decisions to `docs/decisions/` or the historical archive.
- Read `docs/INDEX.md` for the full game scope and documentation map.
