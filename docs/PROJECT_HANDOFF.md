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
- Use Unity Test Runner for the full EditMode suite; the latest user-reported result was 434/435 on 2026-11-24; the only failure was an obsolete save-version migration assertion. The save-version field, migrator and migration-only tests have since been removed, so the updated branch needs a fresh Unity compile and full EditMode run. The Unity application version remains 0.01.
- Exercise the First Workshop Play Mode flow and save/load path after importing the new content.

Historical handoff snapshots and session history are retained under `docs/progress/`.
