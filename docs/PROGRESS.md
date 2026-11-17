# Progress

## Current initiative: first hideout and surrounding city block
Status: Slice A has been verified by the user (compilation and tests passed). Slice B source implementation is now in place and awaiting Unity Play Mode verification. Work remains on `chore/collaboration-workflow-and-audit`; `main` is untouched.

## Approved project direction
Signal is an immersive top-down 2D urban RPG about travelling through a city, performing small interactions for money and resources, taking risks, looting, sometimes losing things, and returning to a personal hideout. It is not primarily a shop-management game.
- The first player-facing focus is a small, walkable starter house/hideout.
- The hideout should contain real visual furniture and interactable objects.
- The workbench should open a dedicated close-up visual workspace, then return the player to the same room.
- A compact city block provides context; buildings should be enterable, with simple interiors initially.
- Visual direction takes top-down readability and lived-in interiors from Stardew Valley, adapted to a gritty contemporary urban setting.
- New detailed gameplay rules beyond this approved direction still require explanation and approval.

## Repository and branch
- Repository: `nitexgaming5-ctrl/Signal`
- Default branch: `main`
- Working branch: `chore/collaboration-workflow-and-audit`
- Unity version recorded in project: 6000.6.9f1.
- The working branch contains the Core/Game hideout and connected city-block implementation. No changes have been pushed to main.

## Existing foundations found
### Current Game layer
- `GameSession`, `PlayerMotor2D`, `RoomBootstrap`, `CameraFollow2D`, `InventoryPanel`, `HudPanel`, `SaveFileStore`.
- Core systems include player stats, grid inventory, clock, rent, heat, crafting, save DTOs and mapping.

### Existing Main scene and prototype content
The repository contains more of the intended hideout/city direction than the previous roadmap implied:
- `Assets/_Project/Scenes/Main.unity` includes objects named `ShelterEntrance`, `ShelterFloor`, `Workbench`, `StorageLocker`, `Bed`, `ExitDoor`, `--- CITY ---`, `--- SHELTER ---`, and street/building props.
- The scene references prototype components including `InteractionController`, `ShelterSpaceController`, `ShelterManager`, `ShelterVisualController`, `WorkbenchInteractable`, `StorageLockerInteractable`, and `BedInteractable`.
- `Assets/_Project/_Prototype/Scripts` contains a world interaction framework, shelter/city switching, bed/storage/workbench interactions, and related UI.
- The existing scene and scripts remain legacy reference only. No new gameplay is being added to that architecture.

## Slice A — hideout/workbench (user verified)
- Added HideoutSlice as the active build scene.
- Core/Game is the single active runtime.
- RoomBootstrap creates the starter room, player and placeholder furniture.
- E near the physical workbench opens a close-up UGUI workspace. Objects can be dragged and inspected; ESC/button closes it.
- The user reported that the project compiled and the first-slice tests passed in Unity.
- Authored art and meaningful bed/storage interactions remain future work.
- The assistant did not independently run Unity; verification is based on the user's report.

## Slice B — connected city block (implementation added; verification pending)
- Added `CityBlockBootstrap.cs` to build a compact street with sidewalks, road markings, two placeholder building facades and simple street props.
- Opened the hideout's south doorway so the player can physically walk out.
- The same player, inventory, clock and GameSession persist while transitioning between hideout, street, corner shop interior and apartment interior. The city system activates the current location's visual root and updates camera bounds.
- E at the street-facing hideout entrance returns home; E at either building entrance enters that interior; E at the interior exit returns to the street.
- Save, load and sleep are currently limited to the hideout because save data does not yet include the current world location. This avoids loading a city coordinate into the hideout scene state.
- Added `CityBlockBootstrapTests.cs` to cover the transition sequence. These new tests have NOT yet been run by the user or assistant.
- City/building visuals are placeholders. Existing art assets and legacy scene references were inspected, but no unverified bulk migration was performed.

## Verification status
- User confirmed compilation and the first hideout slice tests passed before Slice B work began.
- Slice B source and tests have been committed to the working branch, but the new code has not been compiled or run in Unity yet.
- Assistant cannot run the user's local Unity Editor through the GitHub repository connection.
- Next required check: pull the latest branch, let Unity compile, run EditMode tests, then test the full hideout → street → corner shop/apartment → street → hideout loop in Play Mode.

## Risks and constraints
- Core/Game SaveFileStore uses a temp file plus File.Replace and a separate `signal-core-save.json` path. The user has not yet reported platform-specific save replacement verification.
- Main.unity and its old prototype scripts remain because they contain useful scene content and references. They are not in the active build list and should not receive new gameplay features.
- Location persistence is not implemented; save/load/sleep are intentionally restricted to the hideout for now.
- The assistant cannot run the user's local Unity Editor through the GitHub repository connection.

## Next actions
1. User pulls the latest working branch and checks for compile errors.
2. Run EditMode tests, including `CityBlockBootstrapTests`.
3. Play from HideoutSlice: walk through the south doorway, enter and leave both buildings, return home, and check that the camera follows and collisions feel reasonable.
4. Fix any errors or layout/collision problems reported.
5. Once Slice B is verified, improve the visual quality of the hideout and city before introducing additional economy or production mechanics.

## Work log
- 2026-11-17: Confirmed GitHub access and created the working branch; documented the focus-friendly workflow and original hybrid RPG direction.
- 2026-11-17: User clarified the core fantasy: immersive urban hustling, travel, small interactions, money/resources, loot, risk and loss; shop management is not the central game.
- 2026-11-17: User approved a first-slice direction centered on a small playable hideout, visual furniture, a close-up interactive workbench, and a compact city with enterable buildings.
- 2026-11-17: Consolidated active runtime on Core/Game; kept Main and Prototype scripts as legacy references. Added HideoutSlice, starter furniture and the first draggable/inspectable workbench close-up.
- 2026-11-17: Hardened Core/Game save replacement with File.Replace and isolated its filename from the legacy prototype save.
- 2026-11-17: Fixed the HudPanel NewText argument-order compile error.
- 2026-11-17: User confirmed compilation and first-slice tests passed in Unity.
- 2026-11-17: Added the connected city street, hideout doorway transitions, corner shop and apartment placeholder interiors, camera-bound updates, and an EditMode transition test. Slice B verification pending.
