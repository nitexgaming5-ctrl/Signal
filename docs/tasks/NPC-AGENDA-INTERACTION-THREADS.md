# NPC Agenda & Interaction Threads

**Status:** Implementation in progress; Unity verification pending  
**Approved:** 2026-11-24  
**Priority:** High — shared narrative foundation for Direction B  
**Pilot:** Rhea  
**Architecture reference:** `docs/NPC_FRAMEWORK.md`

## Outcome

Create a reusable, data-authored interaction framework that lets any resident have multiple independent conversation/request/offer threads with authored stages, player choices, explicit conditions and one-time consequences. Rhea is the first complete pilot. A second fixture must prove the system is not Rhea-specific.

## Required capabilities

- Stable NPC/thread/stage/choice IDs.
- Multiple independent threads per NPC; deterministic selection and clear priority/tie-breaking.
- Stage eligibility from authored conditions: activity/location, player discoveries, relationship thresholds, opportunity/contract phases and acquired upgrades.
- Multi-choice stages with typed outcomes.
- Idempotent effects: no duplicate discovery, relationship adjustment, contract activation, item transfer, payment or upgrade purchase when revisiting/reloading.
- Sourced information and confidence. Rumour is belief, not objective truth.
- Save/load persistence for durable choices and stage progress; safe handling of unknown or removed content IDs.
- Generic Core rules/progress, Game ScriptableObject content, generic director and reusable conversation UI.
- No character-specific branches in NpcPresenter; no LLM/procedural conversation; no background relationship drift.

## Rhea pilot content

### Thread A — A neighbour who notices things
Rhea shares a low-confidence, sourced tip about Milo. Player can ask how she knows, ask why she is telling them, or leave. The player receives a rumour, not a factual verdict. Rhea must have a plausible observation/hearsay source.

### Thread B — Something in the back room
After the existing Milo customer-request prerequisite, Rhea offers the Compact Sewing Machine. The agenda opens the existing supplier offer panel. Existing upgrade service remains authoritative for payment, installation and recipe unlock. Decline remains revisitable.

### Thread C — Perhaps I need a favour myself
After the machine transaction, Rhea gives practical mending advice and is established as a possible future repair customer. This is narrative setup only; B-04 repair mechanics are out of scope.

Threads A and B remain independent. The player does not need to buy the machine to hear the tip, or exhaust all dialogue to buy it.

## Milo parcel boundary

Keep the parcel controller's physical world props, inspection, acceptance, return, delivery and outcome mechanics. Migrate Rhea's tip/conversation presentation only if it cleanly fits the framework. Do not duplicate or rewrite the parcel opportunity lifecycle.

## Acceptance criteria

- [x] Core progress model, save v16 migration/round-trip wiring and focused tests cover stable IDs, independent threads, selected choices, invalid records and idempotent stage completion (Unity execution pending).
- [x] Authored assets validate stable IDs, duplicate IDs, required references, typed outcomes and invalid discovery/relationship data.
- [x] Generic director selects eligible interactions using thread priority, activity, location, discovery, relationship, opportunity, contract and upgrade conditions.
- [x] Reusable conversation panel presents dialogue and choices with modal input locking.
- [x] Rhea Thread A is authored as a second-hand rumour with source `rhea` and confidence 0.4; the response explicitly states it is unverified.
- [x] Rhea Thread B references the existing Compact Sewing Machine upgrade and opens the existing supplier panel; the existing upgrade service remains the only purchase path.
- [x] Rhea Thread C requires the acquired sewing-machine upgrade and only provides narrative mending advice.
- [x] A second resident fixture (`milo`) is covered through the same director tests without character-specific runtime code.
- [x] Save v16 persists completed stage/choice IDs and restores them through GameSession; SaveMapper round-trip and v15 migration tests are added.
- [x] Milo parcel physical lifecycle remains in its existing controller; only Rhea's conversation/rumour path was removed.
- [ ] Full EditMode suite passes; user verifies Play Mode interactions and save/load.

## Current verification note\n\nAll implementation checkboxes above mean code/content is in place, not Unity-verified. The full EditMode suite and Play Mode acceptance remain pending. The prior 415/415 result predates these changes.\n\n## Out of scope

Procedural/LLM dialogue, autonomous quest generation, broad NPC relationship simulation, repair mechanics, multiple active customer contracts, new economy rules, and wholesale migration of Milo's parcel opportunity.
