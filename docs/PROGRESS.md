# Progress — current repository truth

**Last reconciled:** 2026-11-27  
**Branch:** `chore/collaboration-workflow-and-audit` (keep `main` untouched)  
**Unity:** 6000.6.9f1 · 2D URP · active scene `Assets/_Project/Scenes/HideoutSlice.unity`  
**Architecture:** `Signal.Core` (plain C# domain) + `Signal.Game` (Unity integration/UI/content)

## Verification

- **Latest user-confirmed full EditMode suite: 563/563 passed (2026-11-27).** This includes the P-04b quickbar usability follow-up.
- P-04a: 557/557 confirmed.
- P-04b initial assignment foundation: 562/562 confirmed; follow-up brought the suite to 563/563.
- P-03.7: 555/555 plus Unity compile and Play Mode acceptance confirmed; closed.
- B-06: 456/456 plus successful Play Mode repair/return/payment loop confirmed.
- Unity Editor is authoritative for compile, tests and Play Mode. Never claim verification beyond what the user reports.

## Strategic direction

The user selected **Direction B — meaningful everyday livelihood**: repair, production, customer jobs, services, relationships and a practical hideout. Character development was reprioritised ahead of B-07 neighbourhood expansion. Keep one active initiative at a time and build connected player-facing loops.

## Current player-development status

### P-01 — Character foundation
Implemented and user-verified: modular Body_A renderer, directional Idle/Walk and local asset importer. The user imported the supplied pack and confirmed the enlarged presentation.

### P-02 / P-04a — Attributes and progression
Implemented and user-verified at **557/557**.
- Exactly three attributes: **Health** (primary; maximum HP), **Armor**, **Heat Protection**.
- Current HP remains separate in `PlayerStats`; effective Health determines maximum HP.
- F2 opens the player-facing Character panel. HUD shows current/max HP.
- Effective attributes combine equipment modifiers and authored skill modifiers.
- Temporary test fixtures only: `health_test` (+10 maximum Health/rank) and `heat_protection_test` (+1 Heat Protection/rank). These are not final balance or permanent skill identities.
- Actual Repair/Production/Trading/Scavenging/Social effects and comprehensive milestone trigger wiring remain future work.

### P-03 — Equipment, backpack and pockets
P-03.7 is closed and user-verified at **555/555**, including compile and Play Mode acceptance.
- Seven equipment slots: Head, Torso, Legs, Feet, Accessory 1–3; backpack uses an accessory slot.
- Backpack owns an 8×6 grid by stable item-instance ID. B drops it; E recovers/re-equips the same item. Dropped cargo remains inaccessible.
- Three persistent 1×1 pockets: P1/P2 ordinary; P3 hidden from ordinary-search enumeration.
- Stack split/merge, pocket transfers and pocket-aware service integrations are implemented.
- Automatic debug starting backpack/cargo was removed in a later cleanup; F1 debug spawner remains. That separate cleanup still needs fresh Unity verification (empty new game and F1 spawner). This does not reopen P-03.7.
- Wearable presentation P-03.6 remains paused.

### P-04 — Character playability (active track)

**P-04a — Attributes/development:** complete, user-verified at 557/557.

**P-04b — Six-slot quickbar assignment and HUD:** complete, user-verified at **563/563**.
- Exactly six general-purpose slots; eligible carried items can be assigned, including consumables, tools, weapons and backpacks.
- Quickbar stores stable item-instance references only; it never owns/copies physical items. A physical item can appear in at most one slot.
- Eligible locations: accessible backpack grid or any of three personal pockets. Equipped clothing/accessories are excluded.
- Tab inventory supports RMB Inspect / Equip to Quick Slot 1–6, drag assignment, slot-to-slot swap and drag-to-unassign.
- RMB reassignment moves an existing reference to the chosen slot; it does not duplicate it.
- Dragging shows a semi-transparent item ghost following the pointer.
- A second six-slot bar is visible at bottom-centre HUD outside Tab inventory; the Tab inventory row remains.
- If backpack is dropped, references to its contents persist but are disabled until that same backpack is recovered. Pocket references remain accessible.
- Save/load persists the six assignments and reconciles invalid/retired identities.
- User confirmed **563/563** after these changes. Play Mode acceptance should still cover the visible HUD and interactions as part of the next in-game check if not already completed.

**P-04c — Active hand and controls:** implementation committed; Unity verification pending.
- Core `PlayerQuickbar` now owns selected-slot and holstered state. The held item is derived from the selected slot's existing stable item-instance reference; no duplicate item ID or inventory ownership is introduced.
- Mouse wheel up/down cycles next/previous through all six slots with wraparound, including empty slots. Number keys 1–6 directly select their matching slots.
- H holsters without clearing assignments. Selecting an empty slot means empty hand.
- Wheel, number keys and H are ignored while any supported gameplay/menu panel is open.
- The six-slot HUD is display-only; selected/active and selected/holstered states receive distinct slot feedback. The Tab quickbar remains an assignment UI.
- If a backpack-backed held item becomes inaccessible when its backpack is dropped, the hand holsters while its quickbar reference remains assigned. Pocket-backed items remain accessible.
- Loading resets the active hand to no selected slot and holstered while restoring the six assignments.
- Five new Core regression tests cover active selection/holster, empty-slot cycling/wraparound, invalidation/unassignment, backpack-style inaccessibility and load reset.
- Source commits: `c17a9f7e27d06caf6066d5fd010594d51d33fd85`, `3f8157afb5ca80e744ed83d9ea25922b33f99e50`, `e0ea41bdf2b11a30b4a99941f7153979fd04223e`.
- **Verification pending:** Unity compile, full EditMode suite (563/563 is the last user-confirmed baseline; five new tests have been added) and Play Mode acceptance.

**P-04d — UI polish:** later follow-up after P-04c. The always-visible six-slot HUD and drag ghost have already been delivered in P-04b follow-up; do not duplicate them.

## Other tracks

- **B-01 to B-06 livelihood foundations:** implemented; B-06 user-verified at 456/456 plus Play Mode. Reuse rather than rebuild.
- **B-07 Neighbourhood Expansion:** paused while P-04 remains active.
- **W-01 Weather & Cold Exposure:** separate inactive future track. Heat Protection mitigates cold only; no overheating system.
- Crime expansion, broader city systems and additional social content remain later work.

## Reusable foundations — do not rebuild

| Area | Existing authority |
|---|---|
| Inventory | Spatial grid, stable stack/item GUIDs, item state, three pockets, portable backpack lifecycle |
| Ownership/economy | Ownership vs custody, atomic asset transfers, CARD accounts, service-payment ledger |
| Workshop | Actor-aware repair/production, authored recipes, one-active customer job |
| NPCs | Shared social world, local perception, individual knowledge, routines, directed relationships |
| Player | Attributes, skills, milestone ledger, equipment loadout, quickbar assignment and save mapping |

## Guardrails

- One active gameplay initiative at a time; complete a coherent player-facing loop.
- New gameplay mechanics must be explained and explicitly approved before implementation.
- Core remains plain C#; Game owns Unity integration and authored content.
- Reuse existing inventory, ownership, transaction, account and save authorities.
- Persistent state requires current-build F5/F9 round-trip coverage.
- No new Unity packages/dependencies without approval.
- Do not author new story cast/backstories until the main storyline/cast are designed.
- Art pause remains in force: no new sprites, wearable overlays, wardrobe presentation or character-art exploration.
- Do not run `dotnet` or `msbuild` for this Unity project.
- Historical detail belongs in [the November log](progress/LOG-2026-11.md); keep this status board concise.


### Debug test loadout (2026-11-27)
- F1 Item Spawner now includes a one-click **TEST LOADOUT** action; it is opt-in and does not change fresh-session starting inventory.
- It equips a Canvas Backpack if the player has none, then adds Bread ×5, Fabric Roll ×5, Thread Spool ×3, one Work Jacket and one Basic Patch Kit through the normal debug spawn/inventory path. Existing equipped backpacks are reused; a dropped backpack must be recovered first.
- Stack quantities are added one item at a time through normal inventory rules, so expected stack groupings are Bread 5, Fabric Roll 3+2, Thread Spool 2+1. This deliberately gives both stack top-up and multi-stack cases.
- No quickbar assignments are overwritten. This loadout only seeds inventory; it does not implement consumable activation/use. Item-use remains a separate gameplay slice.
- Added an EditMode callback test for the loadout action. Unity compilation, tests and Play Mode verification are pending.
- Commits: `c70907a557d48b16c5b9fb0e857693481b27d341`, `c3a9f4cf05b7afa316727c6a60765a24bf556fd7`.
