# Progress

## Current initiative: first hideout and surrounding city block
Status: first implementation in progress. Core/Game is now the single active runtime; the furnished HideoutSlice and visual workbench are wired into a new build scene. Unity verification is pending. Work remains on `chore/collaboration-workflow-and-audit`; `main` is untouched.

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
- The working branch now includes the first Core/Game hideout implementation and a new active build scene. No changes have been pushed to main.
- No Unity compilation, EditMode test run, or Play Mode verification has been performed by the assistant.

## Existing foundations found
### Current Game layer
- `GameSession`, `PlayerMotor2D`, `RoomBootstrap`, `CameraFollow2D`, `InventoryPanel`, `HudPanel`, `SaveFileStore`.
- Core systems include player stats, grid inventory, clock, rent, heat, crafting, save DTOs and mapping.

### Existing Main scene and prototype content
The repository contains more of the intended hideout/city direction than the previous roadmap implied:
- `Assets/_Project/Scenes/Main.unity` includes objects named `ShelterEntrance`, `ShelterFloor`, `Workbench`, `StorageLocker`, `Bed`, `ExitDoor`, `--- CITY ---`, `--- SHELTER ---`, and street/building props.
- The scene references prototype components including `InteractionController`, `ShelterSpaceController`, `ShelterManager`, `ShelterVisualController`, `WorkbenchInteractable`, `StorageLockerInteractable`, and `BedInteractable`.
- `Assets/_Project/_Prototype/Scripts` contains a world interaction framework, shelter/city switching, bed/storage/workbench interactions, and related UI.
- The existing `Workbench` scene object already has a visible sprite, trigger collider, and `WorkbenchInteractable`. Its current interaction performs a simple inventory conversion and displays text feedback; it is not yet the requested close-up, visual, object-manipulation workspace.
- The existing scene also has a bed, storage locker, shelter entrance/exit, a small city root, a few city building sprites, and a player interaction prompt/HUD. This is a more useful starting point than the previous audit summary suggested.
- Existing city art includes `Assets/_Project/Art/City` and `ArtSource/RoguelikeModernCity`; the latter includes its own source pack and license file. Check asset provenance and intended use before importing more art.

These are repository observations, not claims that the scene currently compiles or runs. The prototype code is legacy reference only. Do not add new gameplay there; migrate only specific valuable behavior into Core/Game when required.

## Important risks and constraints
- `SaveFileStore.Save` writes a temporary file, deletes the current save, then moves the temporary file into place. If the final move fails, the previous save may be lost. Fix with a recoverable replacement strategy and regression tests before expanding persistent world state.
- `Main.unity` mixes legacy/prototype-style scene wiring and script references. Do not overwrite it or migrate scripts blindly; inspect its dependencies and reconcile with the user's local Unity project first.
- The prototype scene is wired to a separate set of global-namespace components: `PlayerController`, `PlayerState`, `InventorySystem`, `DayCycle`, `GameSaveSystem`, and related systems. Core/Game is now the chosen active architecture. The legacy Main scene and its prototype components are preserved but are no longer the active build path. See docs/ARCHITECTURE.md.
- The assistant cannot run Unity in the user's local editor through the GitHub repository connection. All Unity verification must be clearly delegated or run through an available Unity integration.

## First implementation delivered

- Added HideoutSlice as the active build scene and changed EditorBuildSettings to launch it. Main.unity remains intact as a legacy reference.
- Consolidated new runtime work on Signal.Core + Signal.Game. The old Prototype scripts are no longer extended.
- RoomBootstrap now builds a furnished placeholder hideout with bed, table, chair, storage and a physical workbench.
- E near the workbench opens a close-up visual workspace. The scale, jars, notebook, tools and tray can be dragged around and clicked to inspect. ESC and the on-screen button close it; movement and inventory input are suspended while it is open.
- Added EditMode tests for the furniture bootstrap and workspace focus/close behavior.
- The workbench is visual only for now. No recipes, drug-production rules, money rules or new gameplay economy behavior were introduced.

## Verification status

- Source and scene YAML inspected through the repository.
- Unity compilation, EditMode tests, Play Mode and build have NOT been run by the assistant.
- New tests exist but remain unexecuted. Scene GUID references have only been checked textually.
- Unity Editor import/compile and Play Mode verification are the next required checks.

## Risks and constraints

- Core/Game SaveFileStore still deletes the current save before moving the temporary file into place. Fix and test this before adding persistent furniture or world state.
- Main.unity and its old prototype scripts remain because they contain useful scene content and references. They are not in the active build list and should not receive new gameplay features.
- The assistant cannot run the user's local Unity Editor through the GitHub repository connection.

## Next actions

1. Open HideoutSlice in Unity 6000.6.9f1; confirm scripts import and compile, then run EditMode tests.
2. Play the scene: walk to the bench, press E, drag/click objects, close with ESC and the button, and confirm movement resumes.
3. Fix any compile, UI layout, pointer input or collision issues found.
4. After the hideout is verified, build the compact city block and enterable buildings using Core/Game.
5. Fix save replacement safety before introducing persistent room customization.

## Work log
- 2026-11-17: Confirmed GitHub access and created the working branch; documented the focus-friendly workflow and original hybrid RPG direction.
- 2026-11-17: User clarified the core fantasy: immersive urban hustling, travel, small interactions, money/resources, loot, risk and loss; shop management is not the central game.
- 2026-11-17: User approved a first-slice direction centered on a small playable hideout, visual furniture, a close-up interactive workbench, and a compact city with enterable buildings.
- 2026-11-17: Repository inspection found an existing Main scene with a walkable shelter/city layout, a sprite-backed workbench, bed, storage, entrance/exit, and prototype interaction scripts. The old prototype and newer Core/Game systems are parallel architectures. No code or scene files changed during this audit.

- 2026-11-17: Consolidated active runtime on Core/Game; kept Main and Prototype scripts as legacy references. Added HideoutSlice, starter furniture and the first draggable/inspectable workbench close-up. Unity verification pending.
