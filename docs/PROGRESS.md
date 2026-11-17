# Progress

## Current initiative: make the hideout the centre of the city loop
Status: Slice A was verified by the user (compilation and tests passed). The user has now confirmed that hideout/street/building travel works in Play Mode. Slice C implementation has begun: locker transfers, persistent locker contents, and saved world location are committed to the shared working branch. The new code and tests still require Unity compilation and verification. Work remains on `chore/collaboration-workflow-and-audit`; `main` is untouched.

## Approved project direction
Signal is an immersive top-down 2D urban RPG about travelling through a city, performing small interactions for money and resources, taking risks, looting, sometimes losing things, and returning to a personal hideout. It is not primarily a shop-management game.
- The first player-facing focus is a small, walkable starter house/hideout.
- The hideout should contain real visual furniture and interactable objects.
- The workbench should open a dedicated close-up visual workspace, then return the player to the same room.
- A compact city block provides context; buildings should be enterable, with simple interiors initially.
- The hideout and city should form one loop: go out, bring things back, use the home base, and resume from a saved state.
- New detailed gameplay rules beyond this approved direction still require explanation and approval.

## Repository and branch
- Repository: `nitexgaming5-ctrl/Signal`
- Default branch: `main`
- Working branch: `chore/collaboration-workflow-and-audit`
- Unity version recorded in project: 6000.6.9f1.
- The working branch contains the Core/Game hideout, connected city block and first home-storage implementation. No changes have been pushed to main.

## Existing foundations found
### Current Game layer
- `GameSession`, `PlayerMotor2D`, `RoomBootstrap`, `CameraFollow2D`, `InventoryPanel`, `HudPanel`, `SaveFileStore`.
- Core systems include player stats, grid inventory, clock, rent, heat, crafting, save DTOs and mapping.
- `RecipeDefinition` exists, but there are currently no recipe assets in Content.

### Existing Main scene and prototype content
The repository contains more of the intended hideout/city direction than the previous roadmap implied:
- `Assets/_Project/Scenes/Main.unity` includes objects named `ShelterEntrance`, `ShelterFloor`, `Workbench`, `StorageLocker`, `Bed`, `ExitDoor`, city/shelter groups, and street/building props.
- The scene references prototype components including `InteractionController`, `ShelterSpaceController`, `ShelterManager`, `ShelterVisualController`, `WorkbenchInteractable`, `StorageLockerInteractable`, and `BedInteractable`.
- `Assets/_Project/_Prototype/Scripts` contains a world interaction framework, shelter/city switching, bed/storage/workbench interactions, and related UI.
- The existing scene and scripts remain legacy reference only. No new gameplay is being added to that architecture.

## Slice A — hideout/workbench (user verified)
- Added HideoutSlice as the active build scene.
- Core/Game is the single active runtime.
- RoomBootstrap creates the starter room, player and placeholder furniture.
- E near the physical workbench opens a close-up UGUI workspace. Objects can be dragged and inspected; ESC/button closes it.
- The user reported that the project compiled and the first-slice tests passed in Unity.
- Authored art and meaningful bed interactions remain future work.
- The assistant did not independently run Unity; verification is based on the user's report.

## Slice B — connected city block (travel verified by user)
- Added `CityBlockBootstrap.cs` to build a compact street with sidewalks, road markings, two placeholder building facades and simple street props.
- Opened the hideout's south doorway so the player can physically walk out.
- The same player, inventory, clock and GameSession persist while transitioning between hideout, street, corner shop interior and apartment interior.
- E at the street-facing hideout entrance returns home; E at either building entrance enters that interior; E at the interior exit returns to the street.
- The user explicitly confirmed that travelling around the connected route is working in Play Mode.
- Added `CityBlockBootstrapTests.cs`; the test has not yet been run by the user or assistant.
- City/building visuals are placeholders. Existing art assets and legacy scene references were inspected, but no unverified bulk migration was performed.

## Slice C — functional home storage and persistent location (implementation committed; verification pending)
- Added a separate 8×6 `GridInventory` for the hideout locker.
- Exposed the physical locker from `RoomBootstrap`; E near it opens `StoragePanel`.
- The storage screen lists item types present in either inventory and offers STORE/TAKE transfers. The Core `InventoryTransfer` service moves as much as the destination can fit and uses snapshots to roll back an unexpected source-removal failure.
- HideoutSlice now references the existing ITEM_A/B/C sample definitions so the inventory and locker can be exercised immediately. They are temporary fixtures, not final item design.
- Extended `SaveData` to version 2 with locker contents and the current place. Version-1 saves migrate to an empty locker and Hideout location.
- Save/load is no longer restricted to the hideout. `CityBlockBootstrap.TryRestorePlace` restores the street or either interior and the saved player coordinates.
- Added Core tests for capacity-aware transfers (including partial transfer and rollback), locker/location capture and restore, and version-1 migration; added Game tests for restoring city locations and JSON round-tripping of locker contents/location.
- These changes have NOT yet been compiled or run in Unity. Storage UI layout/input, transfers, file replacement and location restoration require Play Mode verification.
- The workbench remains a visual inspect/rearrange workspace. RecipeDefinition exists, but no recipe assets are currently authored; no new recipes or production rules have been invented.

## Verification status
- User confirmed Slice A compilation and EditMode tests passed.
- User confirmed the connected city travel loop works in Play Mode.
- Slice C source and tests have been committed, but the latest changes have not yet been compiled or run in Unity.
- Assistant cannot run the user's local Unity Editor through the GitHub repository connection.
- Next check: pull the working branch, allow Unity to compile, run EditMode tests, then verify locker transfers and save/load from multiple locations.

## Risks and constraints
- Core/Game SaveFileStore uses a temp file plus File.Replace and a separate `signal-core-save.json` path. The user has not yet reported platform-specific save replacement verification.
- Save format is now version 2; version-1 migration has unit coverage but still needs execution in Unity.
- The locker has a fixed 8×6 grid for this first pass.
- Main.unity and its old prototype scripts remain because they contain useful scene content and references. They are not in the active build list and should not receive new gameplay features.
- The assistant cannot run the user's local Unity Editor through the GitHub repository connection.

## Next actions
1. User pulls the latest working branch and checks for compile errors.
2. Run EditMode tests, especially `SaveMapperTests`, `CityBlockBootstrapTests`, and `SaveFileStoreTests`.
3. Play from HideoutSlice: approach the locker, store and retrieve items, test full/blocked destination behavior, and close the panel with ESC.
4. Save while at home, move into the street or an interior, then load and confirm the correct place, coordinates, carried items and locker contents return.
5. If all passes, connect existing `RecipeDefinition` assets to the workbench. Authoring actual recipes and their gameplay meaning remains a design decision, not an assumed implementation detail.

## Work log
- 2026-11-17: Confirmed GitHub access and created the working branch; documented the focus-friendly workflow and original hybrid RPG direction.
- 2026-11-17: User clarified the core fantasy: immersive urban hustling, travel, small interactions, money/resources, loot, risk and loss; shop management is not the central game.
- 2026-11-17: User approved a first-slice direction centered on a small playable hideout, visual furniture, a close-up interactive workbench, and a compact city with enterable buildings.
- 2026-11-17: Consolidated active runtime on Core/Game; kept Main and Prototype scripts as legacy references. Added HideoutSlice, starter furniture and the first draggable/inspectable workbench close-up.
- 2026-11-17: Hardened Core/Game save replacement with File.Replace and isolated its filename from the legacy prototype save.
- 2026-11-17: Fixed the HudPanel NewText argument-order compile error.
- 2026-11-17: User confirmed compilation and first-slice tests passed in Unity.
- 2026-11-17: Added the connected city street, hideout doorway transitions, corner shop and apartment placeholder interiors, camera-bound updates, and an EditMode transition test.
- 2026-11-17: User confirmed connected hideout/street/building travel works in Play Mode.
- 2026-11-17: Added physical home storage transfers, version-2 saves for carried and stored items plus world location, version-1 migration, and tests for storage/location persistence. Unity verification pending.
