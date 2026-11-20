# Signal — project handoff

Signal is an immersive top-down 2D urban RPG. The user is the game director and final authority on game design.

## Read first
1. `AGENTS.md` — collaboration, architecture and verification rules.
2. `docs/PROGRESS.md` — current repository truth and next action.
3. The active task card linked from Progress.
4. Relevant design documents: `docs/VISION.md`, `docs/ROADMAP.md`, `docs/ARCHITECTURE.md`, `docs/DATA_MODEL.md`, `docs/OPEN_QUESTIONS.md` and `docs/DEVELOPMENT_WORKFLOW.md`.

## Open and verify
- Unity version: **6000.6.9f1**.
- Active scene: `Assets/_Project/Scenes/HideoutSlice.unity`.
- Open the project in Unity Hub, open the active scene, allow Unity to compile, then use the Unity Test Runner for the full EditMode suite.
- Do not use standalone `dotnet` or `msbuild` commands. The Unity Editor owns compilation and test execution.

## Historical detail
The full handoff snapshot is preserved at `docs/progress/ARCHIVE-PROJECT_HANDOFF-2026-11-20.md`. Current status always comes from `docs/PROGRESS.md`, not the archive.
