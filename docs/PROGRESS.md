# Progress — current repository truth

Last reconciled: **2026-11-24**. Historical detail is preserved in `docs/progress/LOG-2026-11.md`. This file is the concise status board, not an implementation diary.

## Current state

- Unity 6000.6.9f1, 2D URP. Active architecture: `Signal.Core` (plain C#) + `Signal.Game` (Unity layer). Active scene: `Assets/_Project/Scenes/HideoutSlice.unity`.
- Working branch: `chore/collaboration-workflow-and-audit`. Keep `main` untouched.
- Unity application version: **0.01**. Save files are unversioned development snapshots: no schema version field, version gate or automatic migration chain. Older experimental saves are not guaranteed to remain compatible after data-shape changes.
- Latest completed full EditMode result reported by the user: **417/417 passed (2026-11-24)** after removing the final stale save-version/migration references. This confirms the current branch compiles and the full EditMode suite passes. The earlier 413/415 and 434/435 results are historical.
- The user reports **417/417 EditMode tests passing** and previously confirmed that the project runs with the workbench, selling and customer-request flow working in Play Mode. The remaining First Workshop acceptance pass is to explicitly confirm the Rhea agenda interactions, visible Compact Sewing Machine purchase/installation, Reinforced Patch Kit unlock/production and save/reload persistence before calling B-01 to B-03 complete.
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
| First Workshop | Production, one-active contract, physical delivery, issuer-paid reward, practical upgrade and recipe unlock | code/content committed; 417/417 EditMode passed; final Play Mode acceptance remains |

## Current content reference

- Starter catalogue, exact recipes, contract/upgrade details and workbench playtest: `docs/content/STARTER_CATALOGUE.md`.
- Durable content decisions and replaceability rationale: `docs/decisions/0005-first-workshop-content.md`.

## Current verification

- **EditMode:** the user reports **417/417 passed (2026-11-24)** after the final migration-only references were removed from `ItemInstanceIdentityTests.cs` and `SearchableContainerTests.cs`. Current item identity, current-save round-trip and searchable-container persistence coverage remain. The save-version removal is now compile/test verified; the application version remains 0.01.
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
- The implementation and focused tests are committed, and the user reports the full **417/417 EditMode suite passing** on the current branch. Play Mode must still verify Rhea's rumour at Street, her supplier offer after Milo's contract, her mending follow-up after purchase, and save/load progress.

## Current next action

The save-version cleanup and NPC agenda framework now pass the user's full **417/417 EditMode suite**. Return to the player-facing acceptance pass: (1) talk to Rhea on the Street and exercise the sourced Milo rumour choices; (2) complete Milo's request, return to Rhea at CornerShop, inspect and purchase/install the Compact Sewing Machine; (3) confirm Rhea's mending advice becomes available and the Reinforced Patch Kit recipe unlocks and can be crafted; (4) F5/F9 save/reload and verify balances, inventories, contract, agenda choices, installed upgrade and recipe unlock. After this, close B-01 to B-03 and begin the B-04 Repair & Restoration design. Do not implement repair mechanics until their rules are approved.

## Known repository layout caveat

- Existing `Assets/_Project/Content/Items/` and `Assets/_Project/Content/items/` differ only by case and may collide on Windows. The new First Workshop definitions were deliberately placed in `Content/WorkshopItems/`. Do not merge/rename the existing folders outside Unity Editor; preserving all asset GUIDs and references is mandatory.

## Not included / deliberately deferred

No automatic NPC shopping, supplier simulation, finite universal stock, dynamic prices, supply/demand, passive income, production timers/queues, quality rolls, procedural contracts, multiple active contracts, partial-stack delivery, repair system, relationship/favour rewards, police response, global rumour cascades or new districts are part of the current initiative.

## Documentation maintenance

- Update this summary when implementation or verification state changes.
- Append dated session notes to `docs/progress/LOG-2026-11.md`.
- Keep Open Questions genuinely open; move answered decisions to `docs/decisions/` or the historical archive.
- Read `docs/INDEX.md` for the full game scope and documentation map.
