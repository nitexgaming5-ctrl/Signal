# Signal — project handoff

Signal is a top-down 2D urban RPG built on Unity 6000.6.9f1.

## Current entry points
- `AGENTS.md` — agent authority, architecture safeguards and verification rules.
- `docs/INDEX.md` — full game scope and documentation navigation.
- `docs/PROGRESS.md` — current repository state, validation and next action.
- `docs/ROADMAP.md` — active Direction B initiative and later sequence.
- `docs/tasks/LIVELIHOOD-B1-B3-FIRST-WORKSHOP.md` — First Workshop specification.
- `docs/tasks/NPC-AGENDA-INTERACTION-THREADS.md` — active NPC agenda implementation and acceptance criteria.

## Open and verify
- Active scene: `Assets/_Project/Scenes/HideoutSlice.unity`.
- Open the project in Unity Hub and let Unity import/compile.
- Use Unity Test Runner for the full EditMode suite. Latest user-reported result: **417/417 passed on 2026-11-24**, after removing the final stale save-version/migrator test references. The Unity application version remains **0.01** and save snapshots are deliberately unversioned.
- Exercise the remaining First Workshop and Rhea agenda Play Mode flow: rumour, customer request prerequisite, supplier offer, machine purchase/installation, recipe unlock/production, and save/reload.

Historical handoff snapshots and session history are retained under `docs/progress/`.
