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
- Save migration tests preserving v5 data while adding opportunity/discovery persistence.
- Unity compilation and full EditMode suite.
- Play Mode: discover through multiple combinations; verify source/confidence distinctions; accept/decline/withdraw; deliver on time/late, damaged and witnessed; save/load at relevant points.
- Do not claim Unity verification until the user reports it.