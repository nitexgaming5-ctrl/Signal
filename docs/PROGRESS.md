# Progress

## Current initiative: repository baseline audit and collaboration workflow

Status: in progress. The GitHub connection is confirmed and the current `main` branch has been inspected read-only. Work is being tracked on `chore/collaboration-workflow-and-audit`.

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

## Next
1. Finish the source, scene, test, package, and documentation audit.
2. Reconcile the remote repository state with the user's local Unity project before any scene migration or destructive changes.
3. Produce the technical assessment and proposed development sequence.
4. Propose the first batch of new gameplay mechanics with full rules and wait for user approval before implementing them.

## Work log
- 2026-11-17: Confirmed GitHub repository access; inspected repository tree, core documentation, task cards, project settings, and a first pass of Core/Game code.
- 2026-11-17: Created branch `chore/collaboration-workflow-and-audit`; aligned agent instructions with the user's expanded collaboration model and documented the approved hybrid RPG direction.
