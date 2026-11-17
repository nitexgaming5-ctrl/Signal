# Progress

## Current initiative: plan the first connected activity slice
Status: The hideout, connected starter city block and home-storage loop are implemented. The user has verified Slice A compilation/tests, manual city travel, and the normal locker/transfer/save/load sequence in Unity Play Mode. Remaining verification includes locker capacity edge cases, actual version-1 migration, automated city transition tests and target-platform save replacement. The next player-facing initiative is to design a reusable activity foundation and deliver one connected scenario; no new activity mechanics have been implemented yet. Work remains on `chore/collaboration-workflow-and-audit`; `main` is untouched.

## Approved project direction
Signal is an immersive top-down 2D urban RPG about travelling through a city, performing activities for money and resources, taking risks, sometimes losing things, and returning to a personal hideout. It combines a living city, opportunities and danger.
- The first player-facing focus is a small, walkable starter house/hideout.
- The hideout is the central hub and emotional anchor: storage, preparation, recovery, workspaces, production spaces and personalisation.
- A compact city block provides context; buildings should be enterable, with simple interiors initially.
- The hideout and city should form one loop: go out, bring things back, use the home base, and resume from a saved state.
- **Parallel lifestyles:** legitimate and criminal activities can be freely combined; neither excludes the other.
- **Harsh criminal failure:** crime should involve serious, potentially lasting consequences. Exact penalties, thresholds and recovery rules require design approval.
- **Global crime progression:** criminal activities contribute to one overall criminal level/progression track, rather than separate skill trees for each crime.
- **Crime is a core pillar:** fictional drug-related activity and a broad catalogue of other illegal activities are central to the game. Specific activity rules must be designed individually.
- **Legitimate life is a satisfying alternative:** farming, gathering, processing, crafting, services and trading should offer lower-risk progression and economic loops. Example direction: grow wheat, process it and bake bread; passive/delayed production should still require meaningful setup and collection.
- NPCs, clients, contacts, factions and world events should connect both lifestyles and make the city feel alive.
- New detailed gameplay rules beyond these approved directions still require explanation and approval before implementation.

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

## Slice C — functional home storage and persistent location
- Added a separate 8×6 `GridInventory` for the hideout locker.
- Exposed the physical locker from `RoomBootstrap`; E near it opens `StoragePanel`.
- The storage screen lists item types present in either inventory and offers STORE/TAKE transfers. The Core `InventoryTransfer` service moves as much as the destination can fit and uses snapshots to roll back an unexpected source-removal failure.
- HideoutSlice references ITEM_A/B/C sample definitions as temporary fixtures, not final item design.
- Extended `SaveData` to version 2 with locker contents and current place. Version-1 saves migrate to an empty locker and Hideout location.
- Save/load is available in the city; `CityBlockBootstrap.TryRestorePlace` restores the saved street/interior and player coordinates.
- Added Core tests for capacity-aware transfers, locker/location capture and restore, and version-1 migration; Game tests cover city-location restoration and JSON round-tripping.
- User confirmed on 2026-11-17 that the project has no new Console errors and the instructed locker/transfer/save/load sequence works in Play Mode.
- Not yet confirmed: full/blocked destination edge cases, loading an actual version-1 save, automated test-suite results, and platform-specific file replacement behavior.
- The workbench remains a visual inspect/rearrange workspace. No recipes or production rules have been invented.

## Verification status
- User confirmed Slice A compilation and EditMode tests passed.
- User confirmed the connected city travel loop works in Play Mode.
- User confirmed the Slice C locker and save/load sequence works in Play Mode with no Console errors.
- Assistant cannot run the user's local Unity Editor through the GitHub repository connection.
- Remaining verification: locker capacity edge cases, an actual version-1 save migration, automated city transition tests and platform-specific file replacement.

## Next actions
1. Keep the agreed lifestyle, failure and progression decisions as design constraints; do not implement a crime, economy or production system from assumptions.
2. Design the reusable activity model: activity definitions/objectives, world interactions, character/request hooks, event hooks, outcomes and persistence boundaries.
3. Select one compact vertical-slice scenario that demonstrates the activity model and connects city activity to the hideout. Agree its concrete reward, repeatability and failure rules before implementation.
4. Later, build out crime and legal activity catalogues as connected content using the agreed parallel-lifestyle model.
5. At a convenient verification pass, run EditMode tests and check storage edge cases, version-1 migration and target-platform save replacement.

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
- 2026-11-17: Added physical home storage transfers, version-2 saves for carried and stored items plus world location, version-1 migration, and tests for storage/location persistence.
- 2026-11-17: User confirmed the locker transfer and save/load sequence works in Play Mode with no Console errors; documentation updated to distinguish verified normal flow from remaining edge cases.
- 2026-11-17: User approved the broader Signal direction: balanced living city, opportunities and danger; hideout remains the central hub and emotional anchor.
- 2026-11-17: User approved parallel legal/criminal lifestyles, harsh criminal failure and one global crime progression track. Updated roadmap and idea backlog before implementation; no new gameplay mechanics implemented.
