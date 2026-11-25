# Signal — project handoff

Signal is a top-down 2D urban RPG built on Unity 6000.6.9f1.

## Current entry points
- `AGENTS.md` — agent authority, architecture safeguards and verification rules.
- `docs/INDEX.md` — full game scope and documentation navigation.
- `docs/PROGRESS.md` — current repository state, validation and next action.
- `docs/ROADMAP.md` — active Direction B initiative and later sequence.
- `docs/tasks/LIVELIHOOD-B1-B3-FIRST-WORKSHOP.md` — First Workshop specification.
- `docs/tasks/LIVELIHOOD-B-05-GLOBAL-NPC-AWARENESS-AFFINITY.md` — shared NPC awareness, knowledge and all three relationship directions.
- `docs/tasks/NPC-AGENDA-INTERACTION-THREADS.md` — active NPC agenda implementation and acceptance criteria.

## Open and verify
- Active scene: `Assets/_Project/Scenes/HideoutSlice.unity`.
- Open the project in Unity Hub and let Unity import/compile.
- Use Unity Test Runner for the full EditMode suite. Latest user-reported result: **439/439 EditMode tests passed on 2026-11-25**, including the debug item spawner. The latest B-05 NPC→player and generic agenda integration slices add six focused tests; run the full suite again (expected 445/445 if no other tests changed). Do not report a passing result until Unity Test Runner confirms it. The Unity application version remains **0.01** and save snapshots are deliberately unversioned.
- Exercise the B-05 Play Mode flow with existing framework/test residents only (Rhea, Milo and the existing Dirty Cash contact): verify player→NPC, NPC→player and NPC→NPC impressions remain independent; author test contract and agenda outcomes in the issuer→player direction; verify agenda gates can read that direction; F5/F9 and confirm all durable state survives without restoring the transient event journal. Do not create additional NPCs or write character backstories until the main storyline and intended cast are designed.

Historical handoff snapshots and session history are retained under `docs/progress/`.
