# Progress

## Current initiative: repository baseline audit and focus-friendly development workflow

Status: in progress. GitHub access is confirmed. Work is tracked on `chore/collaboration-workflow-and-audit`; `main` remains untouched.

## Approved project direction
- Signal is a hybrid management/simulation RPG and narrative-driven RPG.
- Simulation systems should create meaningful pressure and emergent situations; characters, relationships, choices, and consequences give those situations narrative weight.
- New gameplay mechanics may be proposed freely, but their player-facing rules must be explained and explicitly approved by the user before implementation.

## Repository baseline (GitHub `main`, inspected 2026-11-17)
- Default branch: `main`.
- The repository currently has one commit: `Initial commit - Signal Unity project`.
- Unity version: 6000.6.9f1.
- Project uses 2D URP, Input System 1.20.1, UGUI, Tilemap, Test Framework 1.8.0.
- The repository contains the Core/Game split, EditMode test assemblies, current Core systems, Unity-facing slice components, an archived `_Prototype` code area, and a sizeable vendored Unity MCP package.
- Current scene assets include `Assets/_Project/Scenes/Main.unity` and `InventoryDemo.unity`. The build settings currently point to `Main.unity`; there is no `Slice.unity` file in the inspected tree.
- No root `README.md`, `docs/DESIGN.md`, or `docs/roadmap.md` exists. The roadmap file is `docs/ROADMAP.md`.

## Previously reported verification (not independently rerun in this audit)
- The previous progress record reports M0–M3 verified and 109/109 tests passed.
- M4–M9 code was reported as written but awaiting the user's Unity test run.
- The audit has not run Unity compilation, EditMode tests, or manual play-mode verification. Those statuses remain unverified by this session.

## Implemented systems found in the current source tree
### Core
- PlayerStats (money and health)
- ItemType and GridInventory (placement, stacking, rotation, removal, snapshots)
- InventoryDragMath
- GameClock
- RentLedger
- HeatMeter
- Shop order fulfilment
- Crafting recipes and rollback
- Save DTOs, migration gate, and SaveMapper

### Unity/Game layer
- ItemDefinition and RecipeDefinition assets
- PlayerMotor2D, CameraFollow2D, RoomBootstrap
- InventoryPanel and HudPanel
- GameSession composition root
- SaveFileStore

These are source-presence observations, not claims that the full gameplay loop is complete or verified.

## Initial audit findings
- `AGENTS.md` still required one task card per session and prohibited new mechanics. That conflicts with the user's updated direction; revised collaboration rules are being introduced on this branch.
- `docs/PROGRESS.md` describes a user-side zip application and a future Slice scene, while the current repository already contains the M4–M9 source files and the build settings still reference Main.unity. This needs reconciliation with the user's local Unity project state.
- `.gitignore` does not currently list the `SignalSave/` directory, although the roadmap/data model say save data should be ignored.
- The current `Main.unity` scene contains legacy/prototype-style scene objects. Scene composition and script references need a dedicated inspection before changing or replacing it.
- The current code and tests have not been executed in Unity during this audit.

## Focus and delivery workflow
- One active gameplay initiative at a time; new ideas go to `docs/IDEA_BACKLOG.md` rather than interrupting active work.
- Build connected vertical slices with player-facing acceptance criteria, not isolated systems for their own sake.
- Protect the known-good Core, save compatibility, Unity asset GUIDs, and `main` branch. Use reversible initiative branches.
- The user has explicitly asked for a focus-friendly process: keep one current objective and one recommended next action visible, and handle technical sequencing without repeatedly asking them to manage it.
- `docs/DEVELOPMENT_WORKFLOW.md` defines the lifecycle and definition of done.

## Focused source-audit findings (2026-11-17)
- `SaveFileStore.Save` writes a temporary file but deletes the current save before moving the temporary file into place. If the final move fails, the previous save may be lost. Treat this as a high-priority save-integrity defect; fix with a recoverable replacement strategy and regression tests before expanding save-dependent gameplay.
- `GameSession` currently exposes selling and crafting through debug function keys (F7/F8), not through an in-world customer/shop interaction. This is a major integration gap between implemented systems and a compelling player-facing loop.
- `GameSession` is not covered by a `GameSessionTests` file in the current test tree. Existing Game tests cover inventory UI, definitions, room bootstrap, and save-file storage.
- The current build settings point to `Main.unity`, whose scene contains legacy shelter/door objects. `InventoryDemo.unity` contains the standalone inventory bootstrap. The documented `Slice.unity` scene is absent. Do not overwrite or repurpose these scenes until their local Unity state and references are reconciled.
- The shop and crafting Core rules have focused tests, but those systems are not yet integrated into a normal player interaction.

## Next
1. Finish the risk-focused audit, especially save/load mapping, scene wiring, and existing test coverage.
2. Fix the save replacement risk with tests before changing the save format or expanding dependent systems.
3. Reconcile the remote repository state with the user's local Unity project before scene migration or destructive changes.
4. Build one complete, player-facing shop interaction using approved rules; keep any new customer behavior or economic rules as a design proposal until agreed.

## Work log
- 2026-11-17: Confirmed GitHub repository access; inspected repository tree, core documentation, task cards, project settings, and a first pass of Core/Game code.
- 2026-11-17: Created branch `chore/collaboration-workflow-and-audit`; aligned agent instructions with the user's expanded collaboration model and documented the approved hybrid RPG direction.
