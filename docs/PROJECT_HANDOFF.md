# Signal — project handoff

Signal is a top-down 2D urban RPG built on Unity 6000.6.9f1.

## Current entry points
- `AGENTS.md` — agent authority, architecture safeguards and verification rules.
- `docs/INDEX.md` — full game scope and documentation navigation.
- `docs/PROGRESS.md` — current repository state, validation and next action.
- `docs/ROADMAP.md` — active Direction B initiative and later sequence.
- `docs/tasks/LIVELIHOOD-B1-B3-FIRST-WORKSHOP.md` — First Workshop specification.
- `docs/tasks/LIVELIHOOD-B-05-GLOBAL-NPC-AWARENESS-AFFINITY.md` — completed shared NPC awareness, knowledge and all three relationship directions.
- `docs/tasks/LIVELIHOOD-B-06-CUSTOMER-REPAIR-ORDERS.md` — current entrusted repair order implementation and acceptance checklist.
- `docs/ROADMAP.md` — next B-06 livelihood expansion design.
- `docs/tasks/NPC-AGENDA-INTERACTION-THREADS.md` — active NPC agenda implementation and acceptance criteria.

## Open and verify
- Active scene: `Assets/_Project/Scenes/HideoutSlice.unity`.
- Open the project in Unity Hub and let Unity import/compile.
- Use Unity Test Runner for the full EditMode suite. Latest user-reported result: **445/445 EditMode tests passed on 2026-11-25**, including B-05 NPC→player directionality, generic agenda integration, contract outcomes and persistence. Do not report a passing result until Unity Test Runner confirms it. The Unity application version remains **0.01** and save snapshots are deliberately unversioned.
- B-05 EditMode verification is complete at 445/445. Any further Play Mode checks should use existing framework/test residents only (Rhea, Milo and the existing Dirty Cash contact). Verify all three relationship directions and F5/F9 if a targeted gameplay regression is suspected. Do not create additional NPCs or write character backstories until the main storyline and intended cast are designed.
- Current initiative: B-06 Customer Repair Orders. The first implementation slice is committed; Unity compilation, the full EditMode suite (expected 454 from the prior 445 baseline) and Play Mode acceptance are pending. Verify custody vs ownership, Worn→Good repair, customer debit/player credit, one dedicated service-payment transaction, replay rejection and F5/F9 before starting B-07.

Historical handoff snapshots and session history are retained under `docs/progress/`.
