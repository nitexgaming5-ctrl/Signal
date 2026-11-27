# Roadmap — connected livelihoods in one lived-in city

## North star

Signal is a top-down urban RPG about making a precarious living in one shared city. The player inhabits a practical hideout, meets residents with public and private lives, handles physical goods, pursues lawful and unlawful opportunities, and sees progress through possessions, capabilities, relationships and places.

Current implementation truth and verification live in [PROGRESS.md](PROGRESS.md). Approved player-facing rules and acceptance criteria live in design/task documents.

## Completed Direction B sequence — B-01 to B-06

- **B-01 Production/workbench:** authored immediate recipes create physical goods.
- **B-02 Customer contracts:** one active contract; exact whole-stack physical fulfilment; issuer-funded clean CARD.
- **B-03 Practical hideout upgrade:** Compact Sewing Machine unlocks the next recipe.
- **B-04 Repair/restoration:** repair kits, item-state preservation and textile buyer/supplier loop.
- **B-05 Global NPC awareness & affinity:** bounded individual knowledge and separate player→NPC, NPC→NPC and NPC→player relationship authority.
- **B-06 Customer Repair Orders:** entrusted Worn Work Jacket, repair, exact-item return and 35 CARD service payment.

The user confirmed B-06's **456/456** EditMode baseline and successful Play Mode repair/return/payment loop on 2026-11-25. Reuse these foundations; do not rebuild them without a concrete defect.

## Player Development — active priority

The user prioritised character development and carried-item usability ahead of neighbourhood expansion.

### P-01 — Character foundation
Implemented and user-verified: modular Body_A renderer, directional Idle/Walk and local asset importer.

### P-02 / P-04a — Attributes and progression
Complete and user-verified at **557/557**. Exactly three attributes: Health (primary/max HP), Armor and Heat Protection. Current HP remains separate. The F2 Character panel and current/max HP HUD are in place. Temporary `health_test` and `heat_protection_test` fixtures validate the modifier pipeline; they are not final skill design. Actual livelihood skill effects and full milestone trigger wiring remain future work.

### P-03 — Equipment, backpack and pockets
P-03.7 closed at **555/555** with compile and Play Mode acceptance. Equipment, stable item identity, portable backpack, pockets, stack operations and service integrations are existing foundations. A later removal of automatic debug starting cargo still needs a separate fresh-session/F1-spawner Unity check. Wearable presentation remains paused.

### P-04 — Character Playability

| Slice | Scope | Status |
|---|---|---|
| P-04a | Player-facing attributes, skills and modifier pipeline | Complete — 557/557 |
| P-04b | Six-slot assignment, identity safety, inventory UI and visible HUD | Complete — 563/563 |
| P-04c | Active hand, selection, holster, wheel and number-key controls | Implemented — Unity verification pending |
| P-04d | Remaining quickbar/interface polish | Later; avoid duplicating delivered HUD/drag ghost |

P-04b includes the six-slot general-purpose quickbar, RMB Inspect/Assign submenu, drag assignment/swap/unassign, reassignment movement, pointer-following drag ghost, persistent bottom-centre HUD bar, dropped-backpack access handling and save/load mapping. User confirmed **563/563** on 2026-11-27.

P-04b includes the six-slot general-purpose quickbar, RMB Inspect/Assign submenu, drag assignment/swap/unassign, reassignment movement, pointer-following drag ghost, persistent bottom-centre HUD bar, dropped-backpack access handling and save/load mapping. User confirmed **563/563** on 2026-11-27.

### P-04c — Active hand and controls (implementation committed; verification pending)

- The selected slot is the one item held in the player's hand. Core derives that item from the slot's stable item-instance reference; no duplicate held-item ID or ownership state exists.
- Mouse wheel up/down cycles next/previous through all six slots, including empty ones, with wraparound.
- Number keys 1–6 directly select the corresponding slot.
- H holsters without clearing the assignment. Selecting an empty slot means empty hand.
- All controls are ignored while any gameplay/menu panel is open.
- HUD remains display-only; selected active and selected holstered states have distinct visual feedback. The Tab quickbar remains the assignment interface.
- When a backpack-backed held item becomes inaccessible because the backpack is dropped, the active hand holsters but the quickbar reference remains assigned. Pocket-backed items remain accessible.
- Loading resets to no selected slot and holstered, while restoring all six quickbar assignments.
- Five new Core regression tests cover selection/holster, empty-slot cycling and wraparound, invalidation/unassignment, inaccessible held items and load reset.
- No consumable activation, tool action, weapon attack, combat, targeting, durability or art changes are included.
- Implementation commits: `c17a9f7e27d06caf6066d5fd010594d51d33fd85`, `3f8157afb5ca80e744ed83d9ea25922b33f99e50`, `e0ea41bdf2b11a30b4a99941f7153979fd04223e`.
- Unity compile, full EditMode suite and Play Mode acceptance are still pending. The last user-confirmed baseline remains 563/563; do not claim the new tests passed until run in Unity.

See [P-04 task card](tasks/PLAYER-P-04-CHARACTER-PLAYABILITY.md) for implementation details and the remaining verification checklist.

## Paused / later tracks

- **B-07 Neighbourhood Expansion:** paused until P-04 is completed or the user explicitly reprioritises it. The existing proposal remains in [B-07 task card](tasks/B-07-NEIGHBOURHOOD-EXPANSION.md).
- **W-01 Weather & Cold Exposure:** separate inactive track. Seasonal temperature/weather and gradual cold exposure may be designed later; Heat Protection mitigates cold only, with no overheating mechanic.
- Broader crime consequences, additional lawful/illicit activities, investigation, services, deeper social scenarios and hideout personalisation remain unscheduled.

## Engineering guardrails

- One active gameplay initiative at a time; complete a coherent player-facing loop.
- New mechanics require explanation and explicit user approval before implementation.
- Reuse inventory, ownership, transaction, account, NPC knowledge and persistence authorities.
- Core is plain C#; Game owns Unity integration and authored content.
- Persistent changes require current-build F5/F9 round-trip coverage.
- No new packages/dependencies without approval.
- No new story cast/backstories before the main storyline/cast are designed.
- The art pause remains active: no new sprites, wearable overlays or wardrobe presentation.
