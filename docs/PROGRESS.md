# Progress

## Current initiative: connected NPC framework
Status: NPC framework foundation implemented; the user confirmed 219/219 EditMode tests passed on 2026-11-17. A first Core-only WorldPacingDirector and focused tests have now been added, but this new work has NOT yet been run in Unity. The director is intentionally not wired into gameplay or scenario content: it is a shared permission gate awaiting authored scenario ownership and approved tuning. Next: user pulls the branch, compiles and runs the full EditMode suite; then resolve any failures. Storage edge cases, actual version-1 migration and target-platform save replacement remain follow-up verification. Work remains on `chore/collaboration-workflow-and-audit`; `main` is untouched.

## Approved project direction
Signal is an immersive top-down 2D urban RPG about making a living in one layered city: meeting people, doing activities for money and resources, trading goods, taking risks, sometimes losing things, discovering secrets and returning to a personal hideout.
- The hideout is the central hub and emotional anchor: storage, preparation, recovery, workspaces, production spaces and personalisation.
- The city is shared by legitimate businesses, ordinary residents, criminal opportunities and hidden dealings. There is no default separate "legal world" and "underground world"; dedicated criminal hideouts may exist later, but are optional content.
- NPCs can have public occupations and concealed private activities. The player should not automatically know every person's secrets; discovery can happen through conversation, observation, requests, rumours and consequences. Example: a baker may also secretly sell illicit goods.
- NPCs should feel like people with lives beyond the player. The approved technical direction is a low-cost, deterministic utility brain paired with authored routines; no per-NPC language model is required.
- Legal and illegal activities are parallel and can be freely combined.
- Crime is a core pillar, including fictional drug-related activity and a broad range of other illegal activities.
- Criminal failure should be harsh and potentially lasting; exact penalties, thresholds and recovery rules need design approval.
- Criminal progression is global: criminal activities contribute to one overall criminal level/progression track, rather than separate skill trees for each crime.
- Legitimate life is a satisfying alternative and can take many forms: trading, selling different grades of water, food, repair, services, gathering, processing, making goods, small businesses and potentially farming. Farming/wheat/bread was an example, not a fixed design requirement.
- Goods can have meaningful properties such as quality, condition, authenticity, origin/provenance and ownership status. These should be implemented when they create useful player decisions, not as complexity for its own sake.
- NPCs, clients, contacts, factions, rumours and world events should connect both lawful and illegal activities in the same neighbourhoods and economy.
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
- Added Core tests for capacity-aware transfers, locker/location capture and restore, and Game tests for city-location restoration and JSON round-tripping.
- User confirmed on 2026-11-17 that the project has no new Console errors and the instructed locker/transfer/save/load sequence works in Play Mode.
- Not yet confirmed: full/blocked destination edge cases, loading an actual version-1 save, automated test-suite results, and platform-specific file replacement behavior.
- The workbench remains a visual inspect/rearrange workspace. No recipes or production rules have been invented.

## Connected NPC framework (in progress)

- The original Milo baker is explicitly a temporary fixture. His name, appearance, dialogue, role and hard-coded schedule are not final narrative content.
- Added Core social-world types:
  - `NpcSocialWorld`: shared registry, location-aware presence, nearby-NPC queries, event journal, directed relationships and explicit knowledge sharing.
  - `NpcWorldEvent`: a time/location/radius-scoped occurrence with an observable description.
  - `NpcAgentState`: each NPC's current location, position, awareness radius, knowledge and relationships.
  - `NpcKnowledge`: per-character memory distinguishing direct observation from information told by another NPC, including source and confidence.
  - `NpcRelationship`: directed familiarity, trust and affinity values.
- NPCs perceive events only in the same location and within both their awareness radius and the event's visibility radius. Knowledge is not broadcast to every character.
- Explicit knowledge sharing preserves the original event identity and changes confidence according to the listener's trust in the speaker. A rumour remains a belief, not objective truth.
- NpcPerception provides a safe decision snapshot containing nearby NPC IDs and their directed relationship impressions, plus the observing character's own knowledge (including event location/time); it excludes the global event journal.
- CityBlockBootstrap.PublishWorldEventAt can publish events in locations other than the player's current location, supporting off-screen events in the shared city.
- Connected the active CityBlockBootstrap to one shared NpcSocialWorld. Residents register, update simulated location/position and publish conversation events. Added PublishWorldEvent as the common integration point for future environmental and gameplay events.
- Added Game-layer NpcCharacterDefinition ScriptableObject assets and a generic NpcPresenter. GameSession exposes an NPC definitions array for authored assets; CityBlockBootstrap can spawn multiple definitions without per-character components. With no assets assigned, two temporary fallback definitions (Milo and Rhea) prove distinct routine schedules, appearance, dialogue and interaction through the same presenter. Residents are visible/interactable only in their current city location. Removed the superseded NeighbourNpc component.
- Added Core EditMode tests for local presence, event perception, knowledge sharing, trust-based confidence, movement and duplicate IDs. Extended the city integration test to check registration and conversation event publication. The user's EditMode report confirms the full Core test assembly passes and the CityBlockBootstrap fixture passes; two unrelated legacy RoomBootstrap assertions required correction.
- The social world is currently runtime-only. Save persistence and event-specific decision goals remain follow-up work. Character definitions now carry authored NpcUtilityProfile data and use the generic presenter; final narrative character assets are intentionally deferred.
- Added the first controlled social-encounter pass: only residents currently following their authored Socialize activity are eligible, and the utility brain now hard-gates that activity to its authored social window; encounters require mutual proximity in the same location, each NPC can participate once per clock pass and once every three in-game hours, and pairs have a 12-hour cooldown. Each encounter adds 0.01 familiarity in both directions, leaves trust/affinity unchanged, exchanges at most one previously unknown event using existing trust-based confidence, and publishes a generic observable event. Added Core tests and a city-clock integration test. Unity verification is pending.
- Detailed design and sequencing: docs/NPC_FRAMEWORK.md.

## Verification status
- User confirmed Slice A compilation and EditMode tests passed.
- User confirmed the connected city travel loop works in Play Mode.
- User confirmed the Slice C locker and save/load sequence works in Play Mode with no Console errors.
- Assistant cannot run the user's local Unity Editor through the GitHub repository connection. The user's 2026-11-17 XML report initially showed 181/181 Core tests and 23/25 Game tests; both Game failures were stale RoomBootstrap expectations. After correction and the later generic-presenter/social-encounter work, the user confirmed 219/219 EditMode tests pass on 2026-11-17. This result predates the new WorldPacingDirector tests.
- Remaining verification: locker capacity edge cases, an actual version-1 save migration, automated city transition tests and platform-specific file replacement.

## Next actions

1. Pull the latest shared branch, compile in Unity and rerun the full Core/Game EditMode suite. The confirmed 219/219 result predates the WorldPacingDirector addition.
2. Create and assign one or more NpcCharacterDefinition assets through the GameSession Inspector to verify the content-authoring workflow in the active scene.
3. Design stable-ID persistence for meaningful relationships, discoveries and selected memories with save migration tests.
4. Verify population scale and event-driven simulation boundaries before adding many residents.
5. Keep the shared-city model: lawful and illegal activity, public life and concealed dealings coexist in the same locations and social network.
6. At a convenient verification pass, run the existing storage edge-case, version-1 migration, city transition and target-platform save replacement checks.

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
- 2026-11-17: User approved parallel legal/criminal lifestyles, harsh criminal failure and one global crime progression track.
- 2026-11-17: User clarified that all activities share one city and that NPCs may have concealed illegal lives; wheat/bread was only an example, with water, food and second-hand/stolen goods among possible trade loops. Updated vision, roadmap, progress and backlog; no gameplay mechanics implemented.
- 2026-11-17: User gave a white card for the next initiative. Approved and implemented the first low-cost NPC life prototype (Milo the baker), with Core utility decisions, activity-aware dialogue and street movement. Added tests and design/task documentation; Unity verification remains pending.

- 2026-11-17: User clarified that the next priority is a reusable, connected NPC framework rather than Milo-specific narrative content. Added the first shared social-world Core model (NPC presence, local event perception, individual knowledge, directed relationships and explicit information sharing), integrated Milo as a temporary fixture, added Core/Game tests, and documented the framework contract and revised roadmap. Unity tests remain unrun.

- 2026-11-17: User confirmed the connected NPC framework compiles without errors. Integrated character-scoped perception into the utility brain so nearby relationship impressions and personally known information can influence social decisions; added a deterministic Core test. Unity EditMode execution remains pending.

- 2026-11-17: Extracted NPC routine windows, utility weights, social-context weights and need drift into the reusable Core NpcUtilityProfile. Added coverage for a different work schedule; the active Game fixture still uses the default profile. Unity compilation and EditMode execution for these latest changes are pending user verification.

- 2026-11-17: User reran the corrected EditMode suite and confirmed 214/214 tests pass.
- 2026-11-17: Replaced the Milo-specific Game component with NpcCharacterDefinition ScriptableObject content and one generic NpcPresenter. Added GameSession asset assignment, multi-resident city spawning and a second temporary profile with a different work start, dialogue and appearance. Extended CityBlockBootstrap tests for Rhea and Milo using the same presenter. Latest changes await Unity compilation and test execution.
- 2026-11-17: Reviewed the user's initial EditMode XML report: 212/214 tests passed; all 181 Core tests passed, while the Game assembly had two failures in legacy RoomBootstrapTests. Corrected the floor assertions to check occupied cells and distinct floor tiles (GetUsedTilesCount counts distinct tile assets), and aligned the doorway assertion with RoomBootstrap's intentional open south entrance. The user reran the full EditMode suite and confirmed 214/214 tests pass; no Unity test execution was available through the repository connection.

- 2026-11-17: Added a controlled scheduled NPC social layer. Clock changes trigger one deterministic encounter pass after NPC routine updates; only mutually nearby residents currently in Socialize can meet. Pair cooldowns and a one-encounter-per-NPC pass limit keep the simulation restrained. Encounters build familiarity slowly, preserve trust/affinity, exchange at most one known event and publish a generic observable event. Added Core coverage and a CityBlockBootstrap integration test; Unity verification is pending.
