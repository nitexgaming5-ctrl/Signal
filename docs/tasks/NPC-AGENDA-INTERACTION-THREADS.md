# NPC Agenda & Interaction Threads

**Status:** Approved; implementation not yet started  
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

- [ ] Core progress model and rule tests cover stable IDs, independent threads, stage transitions, choice recording, invalid content and idempotent outcomes.
- [ ] Authored assets validate missing/duplicate IDs and invalid references.
- [ ] Generic director selects the correct eligible interaction for a nearby resident.
- [ ] Reusable conversation panel presents dialogue and choices and respects modal input locking.
- [ ] Rhea Thread A preserves source/confidence and never implies rumour is confirmed truth.
- [ ] Rhea Thread B opens the existing offer panel and purchases through the existing upgrade service only.
- [ ] Rhea Thread C appears only after the intended purchase milestone and does not create repair gameplay.
- [ ] A second NPC fixture uses the same framework without new character-specific runtime code.
- [ ] Save/reload preserves meaningful stage/choice state without replaying one-time effects.
- [ ] Milo parcel physical gameplay and its existing tests remain unchanged.
- [ ] Full EditMode suite passes; user verifies Play Mode interactions and save/load.

## Out of scope

Procedural/LLM dialogue, autonomous quest generation, broad NPC relationship simulation, repair mechanics, multiple active customer contracts, new economy rules, and wholesale migration of Milo's parcel opportunity.
