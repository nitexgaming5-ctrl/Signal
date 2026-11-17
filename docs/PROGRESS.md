# Progress

## Current initiative: first hideout and surrounding city block
Status: direction approved; focused scene and prototype audit in progress. Work is tracked on `chore/collaboration-workflow-and-audit`; `main` remains untouched.

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
- No gameplay code or Unity scene files have been modified on this branch during the direction change.
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

These are repository observations, not claims that the scene currently compiles or runs. The prototype code is in an archived area and must be inspected for dependencies before promotion or reuse.

## Important risks and constraints
- `SaveFileStore.Save` writes a temporary file, deletes the current save, then moves the temporary file into place. If the final move fails, the previous save may be lost. Fix with a recoverable replacement strategy and regression tests before expanding persistent world state.
- `Main.unity` mixes legacy/prototype-style scene wiring and script references. Do not overwrite it or migrate scripts blindly; inspect its dependencies and reconcile with the user's local Unity project first.
- The prototype scene is wired to a separate set of global-namespace components: `PlayerController`, `PlayerState`, `InventorySystem`, `DayCycle`, `GameSaveSystem`, and related systems. The newer `Signal.Core` / `Signal.Game` architecture is a parallel implementation, not a drop-in replacement. Do not combine the two casually; first decide which existing path should own the first hideout slice.
- The assistant cannot run Unity in the user's local editor through the GitHub repository connection. All Unity verification must be clearly delegated or run through an available Unity integration.

## Next actions
1. Finish the Main-scene dependency audit and identify the exact existing components and visual assets needed for the hideout slice.
2. Select the existing Main-scene prototype path as the likely short-term host, while keeping the newer Core/Game systems isolated until a deliberate integration decision is needed.
3. Implement the workbench close-up as an additive interaction that can be opened and closed without changing the underlying recipe/economy rules yet.
4. Improve the hideout's furniture, layout and atmosphere; then connect a compact city block and basic enterable interiors.
5. Fix save replacement safety and reconcile save ownership before adding persistent furniture placement or other new world-state data.

## Work log
- 2026-11-17: Confirmed GitHub access and created the working branch; documented the focus-friendly workflow and original hybrid RPG direction.
- 2026-11-17: User clarified the core fantasy: immersive urban hustling, travel, small interactions, money/resources, loot, risk and loss; shop management is not the central game.
- 2026-11-17: User approved a first-slice direction centered on a small playable hideout, visual furniture, a close-up interactive workbench, and a compact city with enterable buildings.
- 2026-11-17: Repository inspection found an existing Main scene with a walkable shelter/city layout, a sprite-backed workbench, bed, storage, entrance/exit, and prototype interaction scripts. The old prototype and newer Core/Game systems are parallel architectures. No code or scene files changed during this audit.
