# Signal — project handoff

Signal is a top-down 2D urban RPG built on Unity 6000.6.9f1.

## Current entry points
- `AGENTS.md` — agent authority, architecture safeguards and verification rules.
- `docs/INDEX.md` — full game scope and documentation navigation.
- `docs/PROGRESS.md` — current repository state, validation and next action.
- `docs/ROADMAP.md` — active Direction B initiative and later sequence.
- `docs/tasks/LIVELIHOOD-B1-B3-FIRST-WORKSHOP.md` — First Workshop specification.
- `docs/tasks/LIVELIHOOD-B-05-GLOBAL-NPC-AWARENESS-AFFINITY.md` — shared NPC awareness/affinity implementation.
- `docs/tasks/NPC-AGENDA-INTERACTION-THREADS.md` — active NPC agenda implementation and acceptance criteria.

## Open and verify
- Active scene: `Assets/_Project/Scenes/HideoutSlice.unity`.
- Open the project in Unity Hub and let Unity import/compile.
- Use Unity Test Runner for the full EditMode suite. Latest user-reported pre-B-05 result: **431/431 passed on 2026-11-24**. B-05 persistence and regression tests have since been added and require a fresh full run. The Unity application version remains **0.01** and save snapshots are deliberately unversioned.
- Exercise the B-05 Play Mode flow: have Rhea, Milo and a third/fallback resident acquire different sourced knowledge; verify player↔NPC and NPC↔NPC impressions remain distinct; F5/F9 and confirm state survives without restoring the transient event journal.

Historical handoff snapshots and session history are retained under `docs/progress/`.
