# Opportunity 001 — Milo's Parcel

## Approval and scope

Approved by the user on 2026-11-18. This is a small NPC/item opportunity proving only the reusable infrastructure it needs, not a broad quest system.

### Locked narrative decisions
- Milo is a neighbourhood baker.
- Milo knowingly helps someone he knows by moving goods; his motive is helping, not primarily money or coercion.
- The parcel contains stolen goods. Its authoritative ownership state is Stolen, independently of what the player knows.
- Destination is a discreet physical drop-off point in the existing city. No second named recipient NPC is required.
- Discovery is flexible across routine observation, contextual conversation, physical evidence and rumour. No fixed clue checklist or mandatory order.
- Commitment is reversible: the player can accept, investigate, decline or withdraw/return the parcel. Consequences should be proportional rather than binary.
- Delivery outcomes vary by timing, parcel condition and exposure/witnessing.

## Player-facing experience

The player encounters Milo in his ordinary public role. His private activity is inferred through several independent signals:
1. Routine deviation witnessed by the player.
2. Conversation whose availability/content depends on Milo's current activity and the relationship.
3. Physical evidence such as a delivery slip or package marking.
4. Rumour from another resident, retaining source and uncertainty.

Signals are evidence, not conclusions. The player's understanding can develop through different combinations. The player may accept an opportunity while still uncertain.

Milo may offer a favour to move a parcel to a physical drop-off point. Player choices: accept, ask questions, inspect first, decline, or withdraw after accepting. A successful delivery is not the only valid outcome.

The delivery is evaluated across three dimensions: timing (within the intended window or late), condition (intact or damaged), and exposure (witnessed or unnoticed).

Consequences are contained: Milo's relationship/impression, player and NPC knowledge where a character genuinely observes or is told something, parcel location/state, and opportunity state. No automatic police response, criminal progression, major scenario phase transition or backlog is included.

## Technical milestones

### 1. Discovery and contextual interaction foundation
- Player-only discovery ledger, separate from item knowledge and NPC knowledge.
- Preserve signal identity, subject, type, source, confidence and learned time.
- Duplicate authored signal IDs are idempotent.
- Contextual interaction conditions for NPC activity, directed relationship values and required discovery signals.
- No conclusions or truth inferred by the ledger itself.

### 2. Opportunity definition and state
- Small stable-ID authored definition and runtime state.
- Explicit permitted transitions and repeat prevention.
- Persist state only as required by this scenario.
- No generic journal, quest tracker, objective tree or reward catalogue.

### 3. Milo discovery and interaction
- Authored content for the four signal types.
- Contextual dialogue and player choices.
- One physical parcel with stable stack identity and Stolen ownership state.
- Knowledge remains distinct from authoritative item state.

### 4. Delivery and consequences
- Physical drop-off point in the existing city.
- Evaluate timing, condition and exposure.
- Return/withdraw path remains valid.
- Persist relevant opportunity, parcel and discovery state.

## Existing foundations to reuse

NpcUtilityBrain, NpcSocialWorld, NpcKnowledge, NpcRelationship, ItemInstanceState, ItemKnowledgeBook, GridInventory, InventoryTransfer and save migration. Keep Core plain C# and Game responsible for Unity interactions/presentation.

## Explicit exclusions

No generic quest journal/tracker, full dialogue editor, second named recipient, police simulation, criminal progression, general rumour engine, item provenance chain, crafting/trading/production, unrelated NPC secrets or opportunity catalogue.

## Verification requirements

- Core EditMode tests for ledger identity, source/confidence/time, contextual conditions, state transitions and outcomes.
- Save migration tests preserving v5/v6 data while adding opportunity/discovery/relationship persistence (save v7).
- Unity compilation and full EditMode suite.
- Play Mode: discover through multiple combinations; verify source/confidence distinctions; accept/decline/withdraw; deliver on time/late, damaged and witnessed; save/load at relevant points.
- Do not claim Unity verification until the user reports it.

## Milestone 2 — opportunity lifecycle foundation (implemented; verification pending)

- Added immutable OpportunityDefinition with stable opportunity ID, associated NPC ID and offer conditions.
- Added OpportunityState with Dormant, Available, Accepted and Resolved phases.
- Declining records the choice but leaves the opportunity Available; investigation/questions do not require a lifecycle transition.
- Accepting associates the physical parcel's stable stack identity. Returning the parcel releases the commitment and makes the opportunity Available again.
- Delivery resolves the opportunity exactly once. An available opportunity can expire back to Dormant without queueing a future release.
- Added Core EditMode tests for offer gating, reversible acceptance/return, decline, one-time resolution, expiry and invalid transitions.
- Save persistence and GameSession integration were added in the subsequent implementation milestone; see the implementation record below.


## Implementation added — 2026-11-18 (Unity verification pending)

### Reusable Core pieces
- PlayerDiscoverySignal / PlayerDiscoveryLedger retain source, confidence, subject and time for the four independent signal types. Stable occurrence IDs are idempotent. Flexible interaction gates can require a minimum number of distinct types, without requiring a fixed set.
- PlayerRelationshipBook holds player-to-NPC familiarity/trust/affinity, distinct from NPC-to-NPC social relationships.
- OpportunityDefinition and OpportunityState provide stable identity, offer conditions, reversible acceptance/return, non-terminal decline and one-time resolution.
- Save v7 persists discovery signals, player relationships, parcel identity, opportunity lifecycle and delivery assessment. Save v1-v6 migrate with empty new data.

### Authored Milo implementation
- The fallback Milo dialogue no longer states his private activity directly.
- Routine observation: while Milo is Socialize/after closing, the player can see him with a wrapped parcel; approaching him records an observation.
- Conversation: ordinary bakery conversations build familiarity. The private offer only appears during Milo's Socialize activity, with familiarity at least 0.2 and at least two distinct discovery signal types. This allows different clue combinations.
- Physical evidence: a delivery slip at the bakery counter and the parcel's removed label.
- Rumour: Rhea can share an uncertain, sourced rumour while socializing (confidence 0.4).
- Offer wording: “Listen, could you do me a small favour? There's a parcel I need moved. Just take it to the drop-off point. I'd rather not leave the shop.”
- Choices: accept, ask why, inspect first, decline. After acceptance, interacting with Milo returns the parcel; interacting with the street drop-off delivers it.
- Parcel: dedicated ItemDefinition, Standard / Good / Genuine / Stolen. It is not a starter item. Its stack identity remains stable through acceptance, return, save/load and delivery assessment.
- Delivery: same in-game day is on time; a damaged parcel is not intact; an active nearby resident other than Milo counts as a possible witness. The result is recorded. Complications remain soft and do not invoke police/criminal progression.

### Verification status and boundaries
- Core tests have been added/updated for flexible discovery, relationships, opportunity transitions, save v7 migration/round-trip and delivery assessment.
- Unity compilation, full EditMode suite and Play Mode verification are not yet confirmed.
- The parcel has no independent contents simulation, the drop-off has no named recipient, and no generic quest journal/tracker, police system, criminal progression, rumour engine, provenance system or economy has been added.


- StoragePanel now respects the player knowledge boundary: unknown parcel quality/authenticity/ownership aspects remain unknown in storage rows, and the item viewer shows only recorded facts with evidence source/confidence.


- Condition assessment reads the parcel's authoritative ItemCondition. The parcel begins Good; this slice does not add a new action or general damage mechanic to make it Damaged. The damaged branch is ready for existing/future systems that legitimately change item condition.
