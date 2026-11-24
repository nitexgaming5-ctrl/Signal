# B-05 — Global NPC Awareness & Affinity

**Status:** Approved for implementation; architecture audit complete  
**Approved:** 2026-11-24  
**Priority:** High — shared social foundation  
**Pilot content:** Rhea and Milo; neither may receive bespoke runtime logic  
**Architecture references:** `NPC_FRAMEWORK.md`, `NPC_SIMULATION.md`, `DATA_MODEL.md`

## Player outcome

Residents remember what they personally witness or are explicitly told, form individual opinions of the player and one another, and respond consistently across conversations, customer work, favours and future scenarios. The same system applies to every authored NPC.

## Binding design rules

1. **One global social domain.** Extend the existing `NpcSocialWorld`, knowledge and relationship foundations. Do not create a parallel Rhea/social manager or duplicate relationship books.
2. **Directed relationships.** A relationship from A to B is independent of B to A. The existing PlayerRelationshipBook remains the canonical player-to-NPC store; NpcSocialWorld remains the canonical NPC-to-NPC store. Both use the same NpcRelationship value model, but are not merged into duplicate or ambiguous authority.
3. **Separate dimensions.** Familiarity, trust and affinity remain separate values with documented ranges, defaults and mutation rules. No generic friendship score.
4. **Individual awareness.** Every NPC has a private knowledge store. A world event journal records what happened; it is never a shortcut for NPC decision-making.
5. **Grounded acquisition.** Knowledge enters a character's memory only through valid local perception, explicit communication, or a specific authored discovery. Preserve event identity, original actor, latest source, confidence, event context and learning time.
6. **No automatic gossip cascade.** Information is not broadcast globally. Transmission is an explicit interaction/system action and must not recursively spread without another valid interaction.
7. **Truth and belief stay distinct.** A rumour remains sourced, confidence-weighted belief; it must not silently become an objective fact.
8. **Meaningful affinity changes.** Routine proximity and ambient meetings do not farm trust or affinity. Meaningful player actions and authored outcomes may change them through an idempotent, auditable relationship-change path.
9. **Generic content.** NPC identity, dialogue, social responses and scenario-specific effects belong in authored data. No character-ID branches in shared runtime services or presenters.
10. **Durable state.** Persist stable-ID relationships and durable personal knowledge. Rebuild transient position, nearby perception and encounter candidates from the restored world.
11. **Safe scale.** Event-driven updates and local queries; no per-frame full-city scans or background relationship drift.

## Existing foundations to reuse

- `NpcSocialWorld`: shared registry, presence, directed relationships and world-event journal.
- `NpcAgentState`: individual awareness radius, knowledge and relationship access.
- `NpcKnowledge`: observation/told source, event identity, confidence and context.
- `NpcPerception`: character-scoped decision snapshot.
- PlayerRelationshipBook and its existing SavedPlayerRelationship save path for player-to-NPC state.
- NpcSocialWorld and NpcAgentState for NPC-to-NPC state and per-NPC knowledge; these are the B-05 persistence gap.
- Existing explicit knowledge sharing and controlled scheduled social encounters.

Before adding any new type, map these existing APIs and migrate their responsibilities into one canonical path.

## Implementation packages

### B05-01 — Canonical relationship model
- Preserve the existing distinct authority boundaries: PlayerRelationshipBook for player-to-NPC, NpcSocialWorld for NPC-to-NPC.
- Keep the shared NpcRelationship value model and consistent validation/ranges without forcing the player into the NPC registry.
- Store directed familiarity, trust and affinity consistently in each existing owner.
- Provide validated, explicit mutation operations with source/reason metadata where durable history is needed.
- Preserve current ambient encounter behaviour: familiarity only; no ambient trust/affinity drift.
- Keep existing consumers (perception, agenda conditions, social encounters) reading the canonical relationship state.

### B05-02 — Universal awareness and transmission
- Ensure every registered NPC receives the same individual knowledge store.
- Route direct observations through local perception and explicit discoveries through a validated knowledge service.
- Route told/rumour knowledge through explicit transmission, retaining source and confidence.
- Prevent duplicate event knowledge and invalid/unknown actor references.
- Do not expose the global event journal through NPC perception or utility decisions.

### B05-03 — Persistence and composition
- Preserve the existing player-to-NPC save path unchanged.
- Persist durable NPC knowledge and directed NPC-to-NPC relationship state using stable NPC/event IDs.
- Restore after NPC definitions and social world are registered; skip malformed or removed content records safely.
- Rebuild transient location, position and perception from the live world.
- Keep the current unversioned 0.01 save policy; do not introduce a save-version/migration framework.

### B05-04 — Shared gameplay integration
- Ensure agenda relationship conditions and outcomes query/mutate the canonical service.
- Ensure scheduled encounters update familiarity through the same service.
- Ensure witnessed world events and explicit information-sharing use the same knowledge path.
- Add a minimal generic player-facing response/condition hook only where needed to prove affinity matters; do not invent a broad friendship, romance, faction or reputation system.

### B05-05 — Verification
- Core tests: directed independence, separate dimensions, bounded values, mutation idempotency, local observation, source/confidence retention, no journal leakage, explicit-only transmission and no gossip cascade.
- Game tests: arbitrary authored NPC definitions receive identical social state wiring; no Rhea/Milo branches.
- Save tests: player↔NPC and NPC↔NPC relationships, durable knowledge, malformed records and unknown IDs round-trip safely.
- Play Mode: Rhea and Milo learn different facts from different circumstances; a third/fallback authored resident uses the same system; relationship/knowledge state survives F5/F9.
- Run the full EditMode suite and report exact user/CI result. Do not claim Unity verification before it runs.

## Acceptance criteria

- [ ] Player-to-NPC and NPC-to-NPC relationships retain their explicit ownership boundaries while sharing the same directed NpcRelationship value model.
- [ ] Familiarity, trust and affinity remain separate and directional.
- [ ] Every registered NPC has an individual knowledge store.
- [ ] Observation is local and communication/discovery is explicit and sourced.
- [ ] No NPC can query the global event journal for private information.
- [ ] No automatic city-wide gossip or ambient affinity farming exists.
- [ ] Agenda, perception, encounters and witness observations use the shared services.
- [ ] Durable relationship and knowledge state round-trips through the current save.
- [ ] Generic Game composition works for arbitrary authored NPC definitions.
- [ ] Focused tests and full EditMode suite pass; user confirms Play Mode loop.

## Explicit exclusions

No global reputation score, friendship/romance/loyalty system, faction standing, automatic gossip network, NPC language model, procedural dialogue, background relationship drift, autonomous social quest generation, or broad new consequence catalogue. Add such systems only through later explicit design.

## Implementation guardrail

This is a unification and completion of existing foundations, not permission to replace working NPC routines, agenda progression, event pacing, navigation, or scenario lifecycles. Preserve current behaviour unless a change is required by the rules above and covered by regression tests.
