# NPC framework — a connected neighbourhood, not isolated brains

## Purpose

NPCs are individuals inside one shared city simulation. They occupy the same places, follow authored routines, perceive local events, build directional relationships and hold incomplete personal knowledge. The framework is designed for a compact lived-in neighbourhood, not isolated character scripts or omniscient global gossip.

## Core rules

1. One shared world context; characters move through the same connected places.
2. Local perception determines direct observation. A world event is not everyone's memory.
3. Knowledge belongs to individuals and preserves source/confidence when explicitly shared.
4. Rumour and belief are not promoted to objective truth.
5. Familiarity, trust and affinity are directional and distinct. Player→NPC, NPC→player and NPC→NPC are separate directions with explicit owners.
6. Core simulation is deterministic, plain C# and event-driven; no LLM, background worker or per-frame full-city reasoning.
7. NPC identity, schedules, appearance and dialogue are authored data. Runtime presenters remain generic.

## Where the detail lives

- [NPC simulation](NPC_SIMULATION.md): Core models, resident routines, perception, knowledge, social encounters, Game integration and design boundaries.
- [NPC runtime](NPC_RUNTIME.md): world pacing, city navigation geometry, route execution and interruption/recovery.
- [NPC Agenda & Interaction Threads](tasks/NPC-AGENDA-INTERACTION-THREADS.md): stable thread/stage/choice IDs, conditions, outcomes, save progress and Rhea's first three threads.

## Important boundaries

- Never let an NPC query the global event journal to learn hidden information.
- Do not broadcast rumours automatically across the city.
- Ambient scheduled encounters currently provide restrained NPC→NPC familiarity/social contact; trust and affinity changes belong to explicit player actions or authored scenarios.
- PlayerRelationshipBook owns player→NPC impressions. Each NpcAgentState owns that NPC's opinion of the player plus its directed relationships with other NPCs. Do not register the player as an NPC or merge these owners.
- Customer contracts may author issuer→player outcomes, applied only after successful delivery, payment and contract completion. Future agenda/scenario integrations must use generic NPC IDs and authored deltas, never character-specific runtime branches.
- Current residents such as Rhea, Milo and the Dirty Cash contact are test/framework content. Do not expand the cast or write new character backstories before the main storyline and cast are designed.
- Keep one generic `NpcPresenter`; character-specific narrative belongs in authored definitions and agenda content.
- Add persistence only for meaningful durable state with stable IDs and current-build round-trip tests.
- Broader relationship progression, event-specific reactions and autonomous scenario goals require their own player-facing design.

The active NPC agenda implementation and current verification status are maintained in its task card and `docs/PROGRESS.md`.
