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
| P-04c | Active hand, selection, holster and wheel controls | Next proposal — awaiting user approval |
| P-04d | Remaining quickbar/interface polish | Later; avoid duplicating delivered HUD/drag ghost |

P-04b includes the six-slot general-purpose quickbar, RMB Inspect/Assign submenu, drag assignment/swap/unassign, reassignment movement, pointer-following drag ghost, persistent bottom-centre HUD bar, dropped-backpack access handling and save/load mapping. User confirmed **563/563** on 2026-11-27.

P-04c's already-confirmed rules:
- The selected quickbar slot is the single item held in the player's hand; do not add separate weapon/tool equipment slots or duplicate active-item state.
- H holsters the held item but preserves its quickbar assignment.
- Mouse-wheel up/down selects next/previous slot with wraparound, only when no menu is open.
- Selecting a slot only draws/holds the item. It does not consume a consumable or trigger a tool/weapon action.
- Supported consumables require an intentional left-click to use. Tools/weapons are held only; action/attack behavior is out of scope.
- Number keys 1–6 are not approved.
- See [P-04 task card](tasks/PLAYER-P-04-CHARACTER-PLAYABILITY.md) for the proposed P-04c boundary and acceptance gates.

### P-04c implementation proposition (not yet authorised)

Deliver only the active-hand state and its two confirmed controls:
1. Add one authoritative selected-slot/holstered state to the existing Core quickbar model (or a tightly related Core value object), avoiding a second copy of the held item. The held item is resolved from the selected slot's stable item ID.
2. Selecting a populated slot makes that slot active and resolves its item as held. Selecting an empty slot should safely result in no held item; exact empty-slot cycling behavior is to be agreed in the task card.
3. H holsters: clear the active-hand presentation/state while leaving all six assignments untouched. Selecting a populated slot again draws it.
4. Mouse-wheel up/down cycles next/previous with wraparound only while no menu is open. Define whether empty slots are traversed or skipped before implementation.
5. Keep slot selection separate from item use. No consumable activation, tool action, weapon attack, durability, targeting or combat logic in P-04c.
6. Persist the minimal active selection/holstered state only if required by the save/load decision; otherwise establish a safe deterministic post-load state. No duplicate item ownership or physical movement.
7. Add Core tests for selection, holster, wraparound, empty slots, duplicate safety and save/load policy; Game tests/Play Mode checks for input gating and HUD active-slot indication.

**Open decisions to confirm before coding:** whether clicking a visible HUD slot selects it; whether wheel cycling visits empty slots or skips them; whether active/holstered state persists through F5/F9 or always loads holstered. Number keys remain excluded unless separately approved.

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
