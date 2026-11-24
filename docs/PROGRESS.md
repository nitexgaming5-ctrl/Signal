# Progress — current repository truth

Last reconciled: **2026-11-24**. This is the short status board. Session history is in [the November log](progress/LOG-2026-11.md); approved rules and implementation detail live in the linked decision/task documents.

## Project baseline

- Unity **6000.6.9f1**, 2D URP. Active architecture: `Signal.Core` (plain C#) + `Signal.Game` (Unity integration). Active scene: `Assets/_Project/Scenes/HideoutSlice.unity`.
- Working branch: `chore/collaboration-workflow-and-audit`. Keep `main` untouched.
- Application version: **0.01**. Save snapshots are deliberately unversioned; there is no schema-version field, version gate or automatic migration chain. Older experimental saves may be incompatible after data-shape changes.
- Latest user-reported full EditMode result: **417/417 passed (2026-11-24)**, before all B-04 hybrid pricing, textile content, counter geometry, repair service/UI and test additions. This is the last verified baseline, not verification of current HEAD; a fresh full suite is required.
- The user confirmed the workbench, selling and Milo's customer-request flow working in Play Mode. The supplier purchase/installation, Rhea agenda sequence, second recipe and full F5/F9 acceptance remain to be verified.

## Active initiative — Direction B: First Workshop (B-01 to B-03)

**Player outcome:** make physical goods at the hideout, fulfil one authored customer request for clean CARD, then purchase a practical upgrade that unlocks another recipe.

Approved rules:
- Player-operated hideout workshop first; keep Core compatible with NPC-owned production later.
- Authored immediate recipes create physical inventory items.
- Exactly one accepted/active customer contract at a time.
- Fulfilment transfers one exact whole player-owned stack to the issuer's physical receiving inventory.
- The issuer pays a fixed clean CARD reward from their existing resident account; no business account or minted money.
- The Compact Sewing Machine is a one-time practical upgrade and unlocks the Reinforced Patch Kit recipe.

Implementation and authored content are committed. Do not rebuild these systems without a concrete defect:
- [B-01 Production/workbench](tasks/LIVELIHOOD-B-01-PRODUCTION-WORKSHOP.md)
- [B-02 Customer contracts](tasks/LIVELIHOOD-B-02-CUSTOMER-CONTRACTS.md)
- [B-03 Practical upgrade](tasks/LIVELIHOOD-B-03-PRACTICAL-WORKSHOP-UPGRADE.md)
- [Starter catalogue and Play Mode script](content/STARTER_CATALOGUE.md)
- [Shared First Workshop overview](tasks/LIVELIHOOD-B1-B3-FIRST-WORKSHOP.md)

## Reusable implemented foundations

| Area | Existing foundation |
|---|---|
| World | Hideout, Street, CornerShop, Apartment and one persistent session |
| Inventory/items | Grid inventory, stable stack GUIDs, quality/condition/authenticity/ownership, item knowledge and inspection |
| Economy | Resident CARD accounts, ownership/custody, atomic transfers, fixed-price merchandise, fixed or condition-based authored buyback offers (Unity verification pending) |
| Workshop | Actor-aware production, one-active contract, physical fulfilment, issuer-paid reward, upgrade/recipe unlock |
| Scavenging/crime | One-time searchable physical bins, resident-owned containers, theft outcomes and witness-scoped knowledge |
| NPCs | Shared social world, local perception, individual knowledge, authored schedules, generic presenter and agenda threads |
| Dirty Cash | Physical currency, explicit contact exchange/cleaning and separate transaction history |

Specialist references: [Architecture](ARCHITECTURE.md), [Data Model](DATA_MODEL.md), [NPC framework](NPC_FRAMEWORK.md), [NPC simulation](NPC_SIMULATION.md), [NPC runtime](NPC_RUNTIME.md).

## Current verification and next action

**EditMode:** the user's **417/417 result on 2026-11-24** predates the hybrid buyback code and new tests. It remains the last verified baseline; rerun the full suite in Unity on current HEAD.

**Play Mode already confirmed:** workbench opens, production/selling works, Milo's request can be completed.

**Remaining First Workshop acceptance:**
1. Talk to Rhea on the Street and exercise the sourced Milo-rumour choices.
2. Complete Milo's request; talk to Rhea in CornerShop and choose to inspect her Compact Sewing Machine offer.
3. Buy/install the machine; verify payment, installed prop and Rhea's post-purchase mending advice.
4. Confirm Reinforced Patch Kit unlock and production.
5. F5/F9 save/reload; verify balances, inventories, contract, agenda choices, installed upgrade and recipe unlock.

The full agenda implementation and acceptance checklist are in [NPC Agenda & Interaction Threads](tasks/NPC-AGENDA-INTERACTION-THREADS.md). No Play Mode result is claimed for steps not reported by the user.

## Next strategic feature

**B-04 Repair & Restoration** now has its approved hybrid catalogue, Rhea/Work Jacket content, Core repair service and workbench repair UI committed. Focused EditMode coverage has been added for fixed/condition-based buyback, repair transitions, ownership/provenance, knowledge refresh and current-save persistence. Unity has not yet imported/compiled or run the current branch changes; the old 417/417 report predates this work. Next action: pull the branch, run a fresh full EditMode suite, then complete the combined B-01–B-04 Play Mode path (Rhea's supplier/agenda, machine upgrade, recipes, textile counter, condition-priced sales, repair transitions and F5/F9 persistence). See [B-04](tasks/LIVELIHOOD-B-04-REPAIR-RESTORATION.md) and [Decision 0006](decisions/0006-hybrid-buyback-and-textile-business.md).

## Repository hazards and deferred scope

- `Assets/_Project/Content/Items/` and `Assets/_Project/Content/items/` differ only by case and may collide on Windows. New authored content currently uses distinct folders such as `Content/WorkshopItems/`; preserve GUIDs and do not merge/rename those folders outside Unity Editor.
- Deliberately deferred: universal supply/demand, automatic NPC shopping, finite stock simulation, market-wide/time-/demand-driven price changes, passive income, production timers/queues, procedural/multiple contracts, partial-stack contract delivery, customer-owned repairs, random repair failure, broad relationship rewards, police response, global rumour cascades and new districts. Selective authored condition-based buyback is the approved narrow exception.

## Documentation maintenance

Keep this file concise. Append session history to `progress/LOG-2026-11.md`; keep approved rules in `DESIGN.md` / `decisions/`; keep implementation acceptance in the active task cards. The root `PROGRESS.md` is an intentional compatibility pointer to this canonical file.
