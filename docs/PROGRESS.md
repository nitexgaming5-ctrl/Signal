# Progress

## Current status: player-facing sell-to-shop implemented in code; Unity verification pending

The user confirmed **342/342 EditMode tests passed on 2026-11-19** after the transaction-safety audit changes. Phase 3 Business & Retail remains verified at 335/335. The user approved separate fixed-price buyback offers per BusinessDefinition and authorized implementation. The Core exact-stack sale path, business buyback authoring, SELL tab and save v10 ledger are now implemented on the working branch. Unity compilation, EditMode tests and Play Mode verification are pending.

The working branch is `chore/collaboration-workflow-and-audit`; `main` remains untouched. The active architecture is Signal.Core + Signal.Game. The hideout, connected city, inventory/storage, item identity/state/knowledge, NPC framework, Milo parcel opportunity and NPC navigation foundations remain in place.

The economy now has:
- Explicit asset ownership independent of the inventory/container holding an item.
- Whole-stack owner-to-owner transfer and an append-only transaction ledger.
- A clean bank/card account used by existing legitimate-money paths, plus passive personal clean accounts for residents and an atomic retail purchase service that credits business owners.
- Save v10 persistence for clean balance, resident account balances, player-held carried/storage asset ownership, economic transfer history, retail purchase history and the dedicated player-to-business sale ledger.
- Player-to-business sales use separate per-business fixed-price buyback offers, exact carried-stack identity and player ownership validation; sale history is separate from transfers and retail purchases.
- Dirty money explicitly excluded from the bank/card account; it is reserved for physical inventory items and future illicit uses.

Phase 2 is verified by the user's 316/316 report. No dirty-money item definition, cleaning interaction, shady-market exchange, bribe, supplier loop or finite-stock gameplay has been implemented. The Corner Bakery now has an authored Bread ItemDefinition and a one-item catalogue priced at CARD $5 as a temporary test value. The catalogue remains unlimited and event-driven.

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
- Added the first controlled social-encounter pass: only residents currently following their authored Socialize activity are eligible, and the utility brain hard-gates that activity to its authored social window; encounters require mutual proximity in the same location, each NPC can participate once per clock pass and once every three in-game hours, and pairs have a 12-hour cooldown. Each encounter adds 0.01 familiarity in both directions, leaves trust/affinity unchanged, exchanges at most one previously unknown event using existing trust-based confidence, and publishes a generic observable event. The user confirmed these tests as part of the 219/219 EditMode run.
- Detailed design and sequencing: docs/NPC_FRAMEWORK.md.

## World pacing foundation (first pass implemented; not gameplay-integrated)

- Added `WorldPacingDirector` in Core with explicit configuration for minimum interval, maximum interval and ramp duration. It requires an authored scenario phase ID; there are intentionally no guessed default tuning values.
- The interval ramps linearly from minimum to maximum as time passes within the same scenario phase. Reapplying the same phase does not reset the phase clock; changing phase starts a fresh quiet interval/ramp.
- `TryAuthorizeDevelopment` is a shared permission gate for consequential developments. Callers must establish valid context first. Invalid candidates do not consume permission.
- All categories/callers share one interval. Long idle time grants at most one authorization; no backlog or banked permissions accumulate. Clock rewinds cannot grant early authorization.
- The director does not create, queue, select or resolve events. It does not manage categories or scenario logic, and it is not connected to everyday NPC routines or scheduled social encounters.
- Added seven Core tests. The initial 225/226 run exposed an incorrect test expectation: the phase ramp correctly advanced from 60 to 64 after 30 minutes. Corrected the test only; production logic was unchanged. User confirmed the subsequent full suite is **226/226 passing** on 2026-11-17.
- Still deferred: scenario owner, approved tuning, save/load persistence or reconstruction, and integration with any real consequential candidate.

## Verification status
- User confirmed Slice A compilation and EditMode tests passed.
- User confirmed the connected city travel loop works in Play Mode.
- User confirmed the Slice C locker and save/load sequence works in Play Mode with no Console errors.
- Assistant cannot run the user's local Unity Editor through the GitHub repository connection. The user's 2026-11-17 XML report initially showed 181/181 Core tests and 23/25 Game tests; both Game failures were stale RoomBootstrap expectations. After correction, the user confirmed 214/214, then 219/219 after the generic presenter and scheduled-social work. The WorldPacingDirector test's first run was 225/226; the only failure was an incorrect expected ramp value in the test, not a production defect. The expectation was corrected and the user confirmed the full EditMode suite is now 226/226 passing (2026-11-17).
- Remaining verification: locker capacity edge cases, an actual version-1 save migration, automated city transition tests and platform-specific file replacement.

## Next actions and priorities

1. **Recommended next initiative — design a compact player-facing NPC scenario.** Use the existing connected NPC framework to create a small situation in which the player can notice, investigate or interact with a resident's public/private life and experience a meaningful outcome. Agree the exact player-facing rules, clues, risks and consequences with the user before implementation. Do not assume the baker example is the final story.
2. **Short technical validation:** create/assign one or more real `NpcCharacterDefinition` assets in the Unity Inspector and verify the authored-content path in `HideoutSlice`. The fallback Milo/Rhea definitions remain fixtures, not final cast.
3. **Scenario and pacing ownership design:** decide which scenario system owns the current phase, what makes a candidate context-valid, when the shared `WorldPacingDirector` is called, and how phase/last-authorization state is saved or reconstructed. Do not wire arbitrary events into the director.
4. **Persistence only when the scenario needs it:** design stable-ID persistence for meaningful relationships, selected memories, discoveries and durable world changes, with versioned save migration tests. Rebuild transient location/perception data after load.
5. **Scale boundaries:** verify event-driven updates and location/area partitioning before adding many residents. Do not simulate every citizen at equal fidelity.
6. **Parked verification:** storage capacity edge cases, actual version-1 save migration, automated city-transition tests and target-platform save replacement.
7. Keep the shared-city model: lawful and illegal activity, public life and concealed dealings coexist in the same locations and social network.

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


### World pacing test-run follow-up

- The first Unity EditMode run after adding WorldPacingDirector reported **225/226 passed**. The only failure was `WorldPacingDirectorTests.ChangingPhaseStartsFreshRampWithoutReleasingBacklog`: the test expected the interval to remain 60 minutes after observing the same phase 30 minutes later, while the implementation correctly reported 64 as the phase ramp progressed.
- Corrected the test expectation to 64. No production code change was needed.
- The user subsequently confirmed the follow-up full EditMode suite passed **226/226**. The pacing tests are now verified by the user's Unity run.


- 2026-11-17: Added the Core-only WorldPacingDirector and seven focused tests. The first Unity run was 225/226 due to an incorrect expected ramp value in one test; corrected the expectation (60 → 64, reflecting 30 minutes of same-phase ramp progress), with no production-code change. User confirmed the follow-up full EditMode suite passed 226/226.
- 2026-11-17: Refreshed the roadmap with verified milestones, short/medium/long-term goals and design boundaries. Added docs/PROJECT_HANDOFF.md as the detailed next-agent brief covering the game identity, confirmed design, technical architecture, NPC framework, world pacing, verification and collaboration rules.

## Handoff notes for the next agent

- Read `docs/PROJECT_HANDOFF.md` first for the full game brief, approved decisions, current implementation map, constraints and suggested sequencing.
- The 226/226 EditMode result is user-reported Unity verification, not an assistant-run test. No claim is made that a build or full Play Mode scenario was tested after the pacing addition.
- The first NPC framework is deliberately generic and restrained. Do not turn it into a complex autonomous-life simulator before a player-facing scenario proves which deeper rules are useful.
- The WorldPacingDirector is not a content scheduler. It is a shared authorization gate only; no event catalogue, backlog, auto-release or per-category cooldown system has been approved.


## Inventory, quest and player-system audit (2026-11-18)

Reviewed the active Core/Game implementation and the reference-game list before starting Item Instance v1. The full audit and proposed sequence are in `docs/ITEM_QUEST_PLAYER_FRAMEWORK_RESEARCH.md`. The following inventory findings describe the pre-v1 state; the implementation status is recorded in the later Item Instance v1 section.

### Pre-v1 inventory findings
- The project already has a spatial grid inventory: `GridInventory`, 6×4 default carried grid, 8×6 home locker, rectangular item footprints, rotation, stacking, add/remove/move/place, change notifications and snapshots.
- `InventoryTransfer.MoveAll` is capacity-aware and rollback-protected.
- `ItemDefinition` currently provides a stable GUID, display name, grid dimensions and max stack. Core `ItemType` contains only ID, footprint and stack limit.
- Placed stacks have inventory-local integer IDs. These are not stable physical-item identities and are not preserved across container transfer or save/load.
- Save format v2 stores definition ID, position, rotation and count for carried and stored stacks. It does not preserve per-copy state or a stable item-instance ID.
- Inventory UI is generated in code and currently uses placeholder item labels/colours rather than a complete authored icon/description presentation.
- Starting-item grant currently adds the maximum stack of every assigned item definition; this is bootstrap fixture behavior, not a final starting loadout.

### Quest and player-system findings
- No active quest definition/runtime/journal/objective framework was found in the reviewed Core/Game slice.
- NPC events, individual NPC knowledge, relationships and WorldPacingDirector are useful foundations, but are not a quest system.
- PlayerStats currently contains money and health; HeatMeter and RentLedger are separate. No complete player creation, customization, equipment or skill framework exists in the active slice.
- Do not build a broad quest framework before meaningful player actions exist. Future objectives should consume semantic events from authoritative systems so inventory rearrangement/transfer cannot falsely satisfy “acquire” or “deliver” objectives.
- Do not add a large generic stat/skill catalogue. The user approved one global criminal progression track, not a separate skill tree for every crime.

### Next step created — Item Instance v1 proposal
- Added `docs/ITEM_QUEST_PLAYER_FRAMEWORK_RESEARCH.md` with the inventory audit, quest architecture recommendation, player progression/customization sequencing and a feature map for Schedule I, Drug Dealer Simulator, Probably Stolen, Torn Crimes 2.0, Stardew Valley and S.T.A.L.K.E.R./Escape from Tarkov.
- Updated `docs/ROADMAP.md` to make the next technical milestone a narrow item-instance identity/persistence foundation: separate item identity from grid placement, preserve identity across container transfers, define safe stack compatibility, and migrate v2 saves to v3 with tests.
- This is a technical foundation proposal, not approval for quality/durability/counterfeit/stolen/provenance/price/equipment/crafting rules. Those remain separate design decisions.
- The quest framework is sequenced after item identity and meaningful player interactions, and should be proven through one short authored opportunity rather than built as an abstract feature catalogue.
- Player creation/customization and additional stats are deferred until their player-facing use cases are established.


## Item Instance v1 — implemented and user-verified (2026-11-18)

The user approved Item Instance v1 and confirmed **235/235 EditMode tests passed** after pulling the implementation.

- Each stack has a stable `Guid ItemInstanceId`, separate from the inventory-local integer `InstanceId`.
- Identity survives movement, rotation, snapshots, whole-stack transfers and save/load.
- Container transfers preserve orientation and identity, never merge distinct stacks, and leave stacks in the source when a complete footprint cannot fit.
- Save format v3 stores stable stack identity. v1/v2 migration assigns unique identities across carried and storage containers and preserves the prior v1 Hideout/empty-locker behavior.
- There is no split-stack operation. Identity is stack-level, not per individual unit.

## Item state and inspection v1 — user-verified (245/245 EditMode tests, 2026-11-18)

### Implemented
- Added Core `ItemInstanceState` per stack:
  - Quality: Poor, Standard, Fine, Exceptional.
  - Condition: Damaged, Worn, Good, Pristine.
  - Authenticity: Genuine, Counterfeit.
  - Ownership: Legitimate, Stolen.
- Added a central `ItemValueCalculator` that combines the item's authored base reference value with provisional multipliers for those four state dimensions. The result is an indicative reference value only; it is not a buy/sell price or an economy. Core exposes `TryUpdateItemState` to change an item's state while retaining its stable identity.
- Extended `ItemDefinition` with description, category, optional icon, base reference value and default instance-state fields. Runtime stacks may use different states from the definition's defaults.
- `GridInventory.Add` now tops up only stacks with the same definition **and** matching state. Different states remain separate stacks and keep separate stable identities.
- Moving, rotating, snapshotting and transferring preserve the full item state.
- Added `InventoryTransfer.MoveStack` for targeted whole-stack transfers by stable identity.
- Reworked home storage to list individual stacks, display all four state dimensions, inspect a selected stack, and STORE/TAKE only that stack. This avoids hiding different copies of the same definition behind one aggregate count.
- Added a shared read-only `ItemViewerPanel`. Right-click an item in the carried inventory or choose INSPECT in home storage. The viewer presents authored description/category/icon, stack quantity, all four state fields, reference-value multipliers, unit value and stack reference value.
- Save format is now v4. All four state fields are persisted. v1/v2/v3 saves migrate to neutral defaults (Standard, Good, Genuine, Legitimate); existing IDs and inventory layouts remain preserved.
- Added tests for state-aware stack compatibility, state-preserving movement/transfer, targeted stack transfers, provisional valuation, v3 migration defaults and save/load state round-trip.

### Player item knowledge v1 — user-verified (255/255 EditMode tests, 2026-11-18)
- Added Core `ItemKnowledgeFact` and `ItemKnowledgeBook`, keyed by stable stack identity and property aspect.
- Each fact records a believed enum value, evidence source (direct inspection, conversation, rumour or document), confidence and learned in-game day/time.
- Knowledge is deliberately separate from authoritative `ItemInstanceState`; a rumour may be wrong. New evidence replaces the current belief for that aspect, while other known aspects remain.
- The player may know some properties and not others. The book can reconstruct a fully known state only when all four facts exist.
- The item viewer now displays only player-known facts and their source/confidence. Unknown facts remain unknown; reference valuation is withheld until all four properties are known and uses the player's known values, never hidden truth.
- `GameSession.PlayerItemKnowledge` exposes the shared book for future authored scenario/interactions.
- Save format v5 persists item knowledge independently of inventory contents, so discoveries can remain after an item leaves a container. v1-v4 saves migrate with an empty knowledge book; malformed knowledge facts are skipped without rejecting the save.
- Added Core tests for partial/full knowledge, incorrect rumours, evidence replacement, validation and v4→v5/save-load behavior.

### Deliberate boundaries
- No player-facing inspection action or appraisal rule is added yet. The knowledge book is a reusable recording/persistence contract for future authored interactions; do not silently make every right-click reveal facts.
- Evidence-source labels are data provenance, not a detailed dialogue/rumour simulation. No contradiction-resolution, evidence decay, confidence updates or automatic NPC-to-player sharing is implemented.
- The viewer now shows UNKNOWN for unrecorded properties. Until a scenario records discoveries, the new layer intentionally avoids revealing authoritative state.
- The value factors remain provisional reference tuning, not transaction prices, offers, demand, bargaining or a functioning economy.
- State/knowledge is per stack, not per unit. No split-stack operation, provenance chain, evidence system, durability decay, item use, equipment, crafting, weight, trade or quest-item behavior was added.
- Existing ITEM_A/B/C assets remain temporary fixtures; their descriptions/icons/base values/default states still need authoring in the Inspector.

### Verification
- User confirmed **245/245 EditMode tests passed** after the previous item state/inspection work (2026-11-18).
- The player item-knowledge v1 and save v5 work were user-verified at 255/255 EditMode tests. The current Milo opportunity changes introduce save v7; Unity compilation, full EditMode suite and Play Mode scenario verification are pending.


## Current initiative: Milo Parcel — playable scenario implementation added; Unity verification pending

The user gave full approval on 2026-11-18 for Opportunity 001: Milo's Parcel. Locked decisions: Milo helps someone he knows; the parcel contains stolen goods; it goes to a discreet physical drop-off; discovery is flexible across routine observation, contextual conversation, physical evidence and uncertain rumour; player commitment is reversible. See docs/tasks/OPPORTUNITY-001-MILO-PARCEL.md.

The shared branch now contains the discovery ledger, flexible contextual conditions, opportunity lifecycle, player-to-NPC relationship book, save v7 persistence, and a Game-layer Milo parcel scenario with the physical parcel, evidence slip, after-hours observation, Rhea rumour, reversible choices and drop-off outcome assessment. Unity compilation, full EditMode tests and Play Mode verification remain pending; no assistant-side Unity test run is claimed.

## 2026-11-18 — Opportunity 001: Milo's Parcel (approved; milestone 1 implemented)
- Full user approval received for the consolidated specification. Narrative choices are locked as helping motive, stolen parcel, physical drop-off, flexible multi-signal discovery and reversible commitment.
- Added PlayerDiscoverySignal and PlayerDiscoveryLedger in Core. Signals retain stable occurrence ID, subject, type, source, description, confidence and learned in-game time. Re-recording the same authored occurrence is idempotent; distinct occurrences remain distinct. Routine observation, conversation, physical evidence and rumour remain separate signal types.
- Added InteractionContext and InteractionConditionSet for conjunctive checks across current NPC activity, directed relationship thresholds and required discovery signal IDs/types.
- Added focused EditMode tests for source/type retention, confidence, subject scoping, idempotency, multiple occurrences, and activity/relationship/discovery gating.
- Added docs/tasks/OPPORTUNITY-001-MILO-PARCEL.md as the approved initiative specification.
- The discovery ledger, player relationship book, opportunity state, save v7 integration and Milo/Rhea world interactions are now connected through GameSession and MiloParcelOpportunityController. Unity compilation, full EditMode suite and Play Mode scenario verification for these new changes remain pending.
- No legacy scripts or old iterations were deleted; none were demonstrated to be safely removable by this milestone.


### Milestone 2 — minimal opportunity lifecycle (implemented)
- Added immutable OpportunityDefinition with stable ID, associated NPC and offer conditions.
- Added OpportunityState phases Dormant, Available, Accepted and Resolved. Decline records the choice without failing the opportunity; returning the parcel releases commitment and makes it available again; delivery resolves exactly once; an available opportunity can expire without a backlog.
- Added tests for offer conditions, decline, accept/return, one-time delivery resolution, expiry and invalid transitions.
- Later milestones connected these models to GameSession, save v7 and the playable Milo parcel controller. Unity compilation and the full EditMode suite for the new scenario remain pending.


## 2026-11-18 — Opportunity 001: Milo's Parcel (implementation added; Unity verification pending)

- Added reusable Core player discovery signals and ledger for routine observation, conversation, physical evidence and rumour. Stable occurrence IDs are idempotent; source, confidence, subject and learned in-game time are preserved. Flexible interaction conditions can require a minimum number of distinct signal types rather than a fixed clue checklist.
- Added reusable Core player-to-NPC relationship impressions in PlayerRelationshipBook, separate from NPC-to-NPC relationships in NpcSocialWorld. Familiarity, trust and affinity are persisted.
- Added OpportunityDefinition and OpportunityState with Dormant, Available, Accepted and Resolved phases. Decline is non-terminal; returning the parcel releases commitment; delivery resolves once and records on-time/intact/witnessed assessment.
- Added save format v7. It persists player discovery signals, player-to-NPC relationships, opportunity phase, parcel stack identity, acceptance/decline history and delivery assessment. Older v1-v6 saves migrate with empty new collections and no active opportunity.
- Added the Item_MiloParcel definition and assigned it to active HideoutSlice without granting it as a starter item. The authoritative parcel state is Standard / Good / Genuine / Stolen.
- Added MiloParcelOpportunityController: routine observation when Milo is seen after closing with the parcel, contextual offer requiring Socialize + familiarity 0.2 + any two distinct discovery signal types, physical evidence at the bakery counter, uncertain Rhea rumour, question/inspect/accept/decline choices, parcel return, and a discreet physical street drop-off.
- Delivery is deliberately soft: timing (same in-game day), parcel condition (damaged or not) and nearby third-party exposure are recorded independently. Complications do not trigger police, criminal progression or a hard failure. Milo is excluded from counting as an outside witness.
- Updated temporary fallback Milo dialogue so it no longer directly reveals his private activity in a single line.
- Added/updated Core tests for discovery, flexible signal combinations, player relationships, opportunity lifecycle, save v7 migration/round-trip and delivery assessment. Unity compilation, full EditMode suite and all Play Mode paths remain to be run by the user.
- No old prototype scripts or legacy scenes were deleted. They remain protected as reference material.


- Updated StoragePanel to show only player-known item facts (unknown aspects remain `?`) instead of leaking authoritative item state; it refreshes when the player knowledge book changes.


- The parcel starts Good. Delivery records the damaged/intact condition branch from authoritative item state, but this slice deliberately adds no new parcel-damage action or general damage mechanic.


## 2026-11-18 — Developer debug menu for NPC and Milo scenario testing

- Added an in-game developer panel toggled with **F1**. It is separate from normal player interactions and disables player movement while open.
- The panel reports the game clock, current location, NPC simulation state, Milo/Rhea activity and location, Milo opportunity phase, discovery count and player-to-Milo familiarity.
- Added pause/resume for all NPC schedules and movement, plus an independent game-clock pause.
- Added Milo controls to focus the player near him, force Work/Rest/Socialize/Home, and return him to his authored schedule. Forced activity overrides the utility schedule until resumed; resuming all NPC simulation clears debug overrides and refreshes authored schedules.
- Added a direct **Trigger Milo Offer Now** test action. It pauses NPC simulation, forces Milo to Socialize, focuses the player on him, opens the ordinary offer-choice flow and moves a dormant opportunity to Available. It bypasses discovery/familiarity gates without fabricating discovery or relationship facts; parcel acceptance, inventory capacity, return and delivery still use the scenario's normal code paths.
- Debug controls are development tools, not player abilities or new gameplay mechanics. They do not change the authored NPC schedules or the approved opportunity design.
- The user confirmed Unity compiles and the parcel appears in Play Mode. The previous EditMode report was 279/280, with the sole failure being an outdated Milo dialogue assertion; that assertion has been updated. Full test rerun and debug-menu Play Mode verification are pending.


## 2026-11-18 — Real-time NPC travel between city locations

- Replaced schedule-change teleporting with waypoint-based walking between the NPC's current physical location and the next authored activity destination.
- Residents walk to the exit in their current interior, cross to the corresponding street doorway, walk along the street to the destination building, cross into that interior, then walk to the final activity position. Street-to-street and same-interior movement also remains continuous.
- NPCs remain active while off-screen; only their sprite renderers are hidden outside the player's current location. Their movement and NpcSocialWorld position/location continue updating even when the player is elsewhere.
- Changing the player's active location no longer resets NPC positions.
- Player conversations now require the NPC to be physically in the same active location. World events use the NPC's actual current location.
- Scheduled social encounters only consider residents who have reached their Socialize destination. Milo's routine observation, private offer and carried-parcel visual, plus Rhea's rumour, likewise wait until the relevant resident is physically on the street and no longer travelling.
- Added deterministic EditMode movement advancement and a regression test covering Milo's shop → street → apartment route. Existing integration tests now advance NPC movement before asserting arrival or social encounters.
- Movement follows authored doorway waypoints and direct path segments; it is not yet dynamic obstacle avoidance/pathfinding.
- Unity compilation, full EditMode tests and Play Mode observation are pending user verification.


## 2026-11-18 — City navigation geometry and debug overlay (implementation added; Unity verification pending)

- Inspected the generated street/interior geometry and current movement contracts before adding navigation data.
- Added `CityNavigationMap` with four location regions (Hideout, Street, CornerShop, Apartment), solid-collider obstacle records, named bidirectional doorway connections, point-level walkability checks and location-graph connectivity queries.
- The street remains one walkable region spanning the road and both sidewalks. Building interiors remain separate coordinate spaces joined to the street through explicit door connections.
- Navigation obstacles are collected from the actual generated colliders, so the geometry description follows the current scene bootstrap rather than duplicating a second hand-maintained obstacle list.
- Added an F1 developer-menu toggle for a runtime overlay. It displays the active region, solid collider geometry, doorway endpoints, player marker, visible resident positions and current authored NPC travel waypoints.
- Added Core-facing Game assembly EditMode tests for region bounds, obstacle exclusion, doorway graph traversal and duplicate registration.
- No pathfinding was added and no movement behavior was changed. Player movement still relies on Rigidbody2D collision; NPCs still follow authored waypoint segments. Character clearance, obstacle avoidance, dynamic route planning and a shared player/NPC pathfinder remain future work.
- Unity compilation, EditMode tests and Play Mode inspection of the overlay/map accuracy are pending user verification.


## 2026-11-18 — Obstacle-aware NPC navigation v1 (implementation added; Unity verification pending)

- Preserved the existing continuous NpcPresenter movement executor, schedule-driven activity changes, off-screen simulation and physical doorway transitions. This milestone changes how its waypoint routes are planned; it does not replace NPC movement.
- Extended CityNavigationMap with clearance-aware walkability. A circular character footprint is represented by expanding solid collider rectangles and insetting region boundaries.
- Added cached per-location grid data and an A* pathfinder. It uses 8-direction movement, prevents diagonal corner cutting, snaps authored destinations that sit on furniture to a nearby safe point, and smooths routes only where sampled line-of-sight remains walkable.
- Replaced the hard-coded shop/street/apartment route builder with a shortest doorway-connection route through the location graph. Each leg is planned locally around that location's obstacles.
- NPCs now expose a navigation-failure state. A failed route leaves the resident in place rather than silently treating a direct line through an obstacle as a valid journey; the F1 overlay marks the failure.
- Added pathfinder tests for obstacle avoidance, clearance, blocked destinations, unreachable regions and diagonal corner cases, plus a city integration test that checks a route around the street bench.
- Player movement remains on its existing Rigidbody2D collision path. Dynamic obstacles, moving-agent avoidance, crowd separation, player click-to-move and navigation persistence are not part of this pass.
- The user reported 284/284 EditMode tests and a working Milo parcel scenario before this implementation. This new code has not yet been compiled or tested in Unity; verify the full suite and Play Mode movement before treating navigation v1 as complete.


## NPC interruption and recovery v1 — implementation added (2026-11-18)

- Separated initial placement from ordinary schedule refresh. Only first-time binding may snap a resident to an authored starting destination; re-enabling a presenter, resuming the simulation or returning from a debug activity now preserves physical position.
- Returning Milo from a forced debug activity to his authored schedule replans a route from his current physical location instead of teleporting him to the scheduled destination.
- Pausing NPC simulation preserves the current route. If the game clock changes while paused, resuming evaluates the current schedule and replans from the interruption point.
- Added an explicit one-shot retry for failed navigation routes, exposed through F1. There is no per-frame retry loop.
- Added EditMode coverage for forced-activity recovery, route preservation across pause/resume and schedule changes during pause.
- [ ] Run Unity compilation and the full EditMode suite.
- [ ] Verify the new F1 retry control and recovery behaviour in Play Mode.
- [ ] Confirm a failed route remains stopped until a deliberate retry or schedule change; no silent teleport or endless retry loop.


## 2026-11-18 — NPC interruption/recovery v1 verified; game-clock pause correction added

- User confirmed the interruption/recovery implementation compiles and all **291/291 EditMode tests pass**. The user also confirmed the recovery and movement behaviour works in Play Mode.
- Follow-up Play Mode inspection found that the F1 **Pause game clock** control stopped in-game time but residents continued walking in real time. This happened because NPC movement used Unity's `Time.deltaTime` independently of the game clock.
- Added a shared transient pause state to `GameClock`. The developer menu now toggles that state; the normal clock tick and NPC movement both respect it. The separate **Pause all NPC schedules** control remains independent and still pauses NPC movement/behaviour regardless of clock state.
- Added Core coverage for pause-state toggling and a city integration test for scheduled NPC movement stopping/resuming with the game clock. The user subsequently confirmed the correction works in Play Mode and the full suite passes **293/293** (2026-11-18).
- Explicit clock advances (e.g. sleep or save restoration) remain explicit operations; the pause flag gates real-time ticking and resident movement rather than disabling intentional clock changes.


## Next direction — shared-city everyday gameplay loop

- Opportunity 001 (Milo's Parcel) fulfilled the roadmap requirement to build and verify a first player-facing NPC scenario. A second opportunity is optional content and is not a prerequisite for the NPC framework.
- Recommend shifting focus from additional scenario examples to the shared city's repeatable everyday loop: acquiring/handling goods, meaningful item information, and player-to-NPC commercial or service interactions. The exact activity and rules must be designed with the user before implementation.
- Keep the current NPC, item identity/state/knowledge, relationship and opportunity systems as supporting foundations. Extend them only when the chosen loop exposes a concrete missing capability.
- Validate authored NpcCharacterDefinition assets in the Inspector as a small content-authoring check, not as a reason to delay the next player-facing loop.
## 2026-11-19 — Economic system approved; Phase 1 Core foundation added

The user approved the shared city economy design, with two refinements:
- Dirty money is a physical inventory asset for illicit payments (shady markets, bribes, etc.). It is not accepted by legitimate bank/card purchases and cannot be deposited directly. A future cleaning operation consumes dirty-money items and credits the clean bank/card balance directly. Ordinary clean physical cash is omitted initially.
- Universal supply/demand, price fluctuations and continuous consumption are deferred. Ordinary goods use simple authored availability; stock is tracked selectively only where gameplay requires it (e.g. drugs and illegal materials).

### Phase 1 implementation added to the repository
- Added EconomicActorId, EconomicContainerRegistry, AssetOwnershipBook, EconomicTransaction / EconomicTransactionLedger, and AssetTransferService in Signal.Core.
- Whole-stack transfers validate the registered asset owner, require registered source/destination containers and ensure the destination container is maintained by the receiving actor. Source custody may differ from asset ownership. Successful transfers preserve the stack GUID and item state, update the owner, and record a completed transaction with reason and in-game timestamp.
- Full destinations, missing/unregistered assets, invalid requests and ownership mismatches leave the source asset, owner and ledger unchanged.
- Same-owner inventory/storage movement continues to use the existing InventoryTransfer and does not create an economic transaction.
- Added AssetTransferServiceTests covering successful transfer, identity/state preservation, ownership mismatch, unregistered assets/containers, destination actor mismatch, missing assets, full destination, invalid actor/timestamp, ownership book rules and same-owner container movement.
- Phase 1 state is runtime-only. It is not yet wired into GameSession or save data; Phase 2 must add versioned persistence before these services are used for persistent gameplay.
- No Unity Editor or test runner was available to the assistant. C# compilation, EditMode tests and Unity verification are pending user validation.

Next: pull the branch and run Unity compilation plus the full EditMode suite. Then address any reported failures before adding account/money mechanics.


## 2026-11-19 — Economy Phase 2 account and persistence foundation

### User verification of Phase 1
- User ran the full Unity EditMode suite and confirmed **304/304 passed** after the Phase 1 asset-transfer foundation.

### Added in Phase 2
- Added CleanMoneyAccount as the explicit clean bank/card balance. PlayerStats.Money/Earn/TrySpend remain compatibility APIs delegating to that account, keeping existing rent and debug-sale flows clean.
- Dirty money is not represented by an account balance or deposit API; it remains a physical inventory-item direction for future illicit exchanges and cleaning.
- Save format advanced from v7 to v8. Existing v1-v7 money values migrate unchanged into cleanBalance.
- Save v8 captures/restores carried/storage asset ownership mappings and economic transfer history.
- GameSession now owns/composes the ownership book, transaction ledger, player container registry and transfer service. Carried inventory and home storage are registered as player-custodied containers.
- HUD explicitly labels the existing balance as CARD.
- Added CleanMoneyAccount tests and SaveMapper migration/round-trip/invalid-record tests.

### Verification status
- User pulled the implementation and confirmed the full EditMode suite **316/316 passed on 2026-11-19**.
- The first Phase 2 test report was 313/316. All three failures were stale assertions in `ItemInstanceIdentityTests` expecting save version 7 after the save format advanced to v8. The assertions were changed to compare against `SaveMigrator.CurrentVersion`; no production code changed in that correction.
- The 316/316 result is the user's Unity Test Runner report. The assistant did not independently run Unity.
- Economy-specific Play Mode behavior has not yet been separately reported; the next in-Editor check should exercise the CARD display and save/load of the balance after any future player-facing economy interaction.


## 2026-11-19 — Economy Phase 2 verified and documentation reconciled

- User confirmed **316/316 EditMode tests passed** after pulling the Phase 2 implementation and the three stale save-version test expectation corrections.
- Phase 2 is now marked verified: CleanMoneyAccount, PlayerStats compatibility delegation, save v8 clean balance migration, player-held ownership persistence, economic transaction persistence, GameSession composition and CARD HUD label.
- No production-code changes were needed after the 313/316 report; the three failing tests were stale expected-version assertions, corrected to use `SaveMigrator.CurrentVersion`.
- Housekeeping: refreshed this progress summary, reconciled the economy roadmap/task status, and updated the handoff. Older work-log entries remain as historical records and are not rewritten.
- Next: design a compact player-facing illicit exchange/dirty-money slice. The physical item representation and its denomination/stack-value convention remain unresolved; do not invent them in implementation.



## 2026-11-19 — Economy Phase 3: Business & Retail first implementation pass

The approved shared-economy retail scope is in implementation. The old illicit-vendor proposal remains superseded.

### Added
- Core EconomicAccountBook registers the existing player clean account and idempotently creates one passive clean account per resident actor. It prevents two actors from sharing one account instance and remembers authored initial balances.
- Added RetailService for atomic catalogue purchases: validates actors, account availability, price/quantity/time, existing stack ownership and inventory capacity; debits the customer; credits the resident owner; grants merchandise; registers new item-stack ownership; and records one retail transaction. Failure restores both balances, inventory and newly registered ownership.
- Added RetailTransactionLedger and transaction data separate from the physical asset-transfer ledger. Unlimited catalogue purchases do not pretend to transfer a pre-existing finite stock stack.
- Save format advanced v8 → v9. v9 persists resident account balances and retail transaction history. v1–v8 migrations initialize these collections; legacy loads restore resident authored starting balances, while v9 saved balances override them. Player clean balance remains in its canonical cleanBalance field.
- Added StartingCleanBalance to authored NPC definitions and registers all spawned residents, including fallback fixtures, with their own account.
- Added BusinessDefinition ScriptableObject with stable business ID, owner NPC ID, location/interaction point, opening schedule and ItemDefinition-based fixed-price catalogue.
- Added a click-driven RetailPanel with merchandise rows, adjacent BUY buttons, clickable paging/close controls and ESC close.
- GameSession now resolves nearby authored businesses, opens the panel, revalidates opening hours and offers on purchase, and credits the owner's account. Business merchandise is included in the item-definition save resolver.
- Existing Core Shop / ShopOrder selling rules were not changed.
- Added Core tests for account registration/isolation, purchase success, insufficient funds, full inventory rollback, owner-account overflow, ownership validation, save v9 round-trip, legacy starting-balance reset and malformed-record handling. Added Game tests for the authored business schedule/default owner.

### Not yet complete / verification
- Created `Assets/_Project/Content/Businesses/Business_CornerShop.asset` and assigned it to `GameSession.Businesses` in `HideoutSlice.unity`. It targets owner `milo`, location `CornerShop`, hours 06:30–18:00 and interaction point (1000, 1000.6).
- Added `Assets/_Project/Content/Items/Item_Bread.asset` (stable item ID, 1×1 footprint, stack limit 10, ordinary default state) and assigned it to the Corner Bakery catalogue at CARD $5. The price is explicitly a temporary test value, not final balancing.
- Business definitions are validated on interaction and purchase. Existing Core `Shop` / `ShopOrder` sell-to-shop behavior remains unchanged; no new valuation or acceptance rules were added.
- F1 debug view exposes spawned resident clean balances and retail transaction count to support Play Mode verification. The active HideoutSlice GameSession now seeds the player's clean CARD balance at $5,000 for testing; existing saved balances still override this when a save is loaded.
- Tests are authored but have not been executed. The assistant cannot run the Unity Editor, so Unity compilation, full EditMode suite and Play Mode verification remain pending.
- Existing player-to-shop selling code remains unchanged; no new sell valuation, acceptance rules or shop budget were introduced.

Next: pull the branch, compile, run the full EditMode suite and verify a Bread purchase plus save/load in Play Mode. Confirm the player's CARD decreases by $5 and Milo's resident account increases by $5.

## 2026-11-19 — BusinessDefinitionTests compile correction

- Fixed the duplicate `System` and `System.Reflection` using directives in `Assets/_Project/Tests/EditMode/Game/BusinessDefinitionTests.cs`.
- Qualified Unity object cleanup calls as `UnityEngine.Object.DestroyImmediate` to resolve the `Object` ambiguity between `System.Object` and `UnityEngine.Object`.
- Consolidated overlapping catalogue-validation coverage while retaining schedule, owner identity, valid catalogue, duplicate item, missing owner and non-positive price assertions.
- This is a test-source correction only; no production gameplay code or design rules changed.
- Unity compilation and EditMode execution have not yet been rerun. Recompile and run the full suite in the Editor to confirm the correction and surface any subsequent errors.


## 2026-11-19 — Retail UI usability and test balance

- Replaced the temporary F2–F10 shop selection scheme with a mouse-clickable catalogue. Each merchandise entry has its own BUY button beside the product name/price; previous/next paging, close button and ESC close are available. The UI calls the existing GameSession purchase callback, so business-hours, catalogue, account, inventory-capacity, ownership and atomic-transaction validation remain authoritative.
- Added an EventSystem/InputSystemUIInputModule and GraphicRaycaster setup for runtime-created UGUI so mouse clicks work with the project's Input System.
- Changed `HideoutSlice.unity` GameSession `startingMoney` from 0 to **5000**. This is a temporary clean CARD test balance, not a final player-starting-money design. A loaded save keeps its persisted balance; start a fresh game to see the seed.
- Unity compilation, full EditMode suite and Play Mode click/purchase/save-load checks are still pending. The assistant cannot run Unity.


## 2026-11-19 — Economy Phase 3 verified; transaction-safety audit

- User confirmed the full Unity EditMode suite **335/335 passed** and reported that all Phase 3 Business & Retail functionality works as intended in Play Mode.
- Phase 3 is now verified: resident clean accounts, authored Corner Bakery catalogue, click-driven purchases through the existing RetailService, owner revenue, inventory ownership and retail transaction persistence.
- Audited the separate legacy `Shop.TryFulfil` sale operation. It already snapshots inventory, rejects insufficient stock and oversized totals, and restores inventory if the clean account cannot accept payment. `CleanMoneyAccount.TryCredit` rejects non-positive credits and integer overflow.
- Tightened sale preflight validation so default/uninitialized item definitions and zero-value orders are rejected before inventory mutation. Existing sale valuation and acceptance rules are unchanged.
- Added Shop regression coverage for zero-value fail-fast behavior (including no inventory change notification) and uninitialized item definitions.
- Added RetailService regression cases for zero/negative quantity, zero/negative unit price and total-cost overflow. These verify that account balances, inventory, ownership and the retail ledger remain unchanged.
- `Shop.TryFulfil` is not wired to a runtime player-facing sale UI; it is a Core transaction primitive exercised by tests. There is no player-facing sale flow yet. It does not currently create economic transaction records or validate AssetOwnershipBook ownership. Do not retrofit a new sale economy silently; design the player-facing sale integration before enabling it.
- Expected EditMode suite count after these additions: **342 test cases** (335 prior + 2 Shop tests + 5 RetailService parameter cases). This is an expected count, not a test result; Unity verification is pending.
- No save schema change was required.

### Persistence policy clarification (user-approved 2026-11-19)

- Prioritize reliable save/load for the current save format and round-trip tests whenever persistent systems change.
- Do not spend disproportionate effort preserving obsolete development saves or maintaining a long chain of historical migrations.
- A substantial schema change may deliberately invalidate older development saves and require starting a fresh game.
- Never silently corrupt or discard a current-format save; keep writes reliable/atomic and make any intentional reset/incompatibility explicit.


## 2026-11-19 — Transaction-safety audit verified; player-facing selling design

- User confirmed **342/342 EditMode tests passed** after the seven transaction-safety regression cases were added. The transaction-safety audit is closed.
- The existing `Shop.TryFulfil` helper remains a Core transaction primitive: it accepts a `ShopOrder` containing item, quantity and caller-supplied unit price; removes goods and credits the player's clean CARD balance atomically. It does not calculate resale prices or define a shop's accepted catalogue.
- The next proposed initiative is a player-facing sell-to-shop flow. Draft specification: `docs/tasks/ECONOMY-PLAYER-SELLING-DESIGN.md`.
- Key unresolved design point: how each business authors the items it will buy and the unit prices supplied to `ShopOrder`. Recommendation is a separate, explicit fixed-price buyback offer list per business, not reuse of the business's player-facing retail catalogue and not an automatic ItemValueCalculator formula.
- No player-facing sale code has been implemented. Await the user's approval of the buyback offer model before implementation.


## 2026-11-19 — Player-facing sell-to-shop specification finalized

- User approved Q21: each `BusinessDefinition` owns a separate authored buyback-offer list with fixed positive clean-CARD unit prices, independent of its merchandise catalogue.
- Finalized `docs/tasks/ECONOMY-PLAYER-SELLING-DESIGN.md` with the approved business policy, exact-stack/ownership contract, atomic transaction semantics, dedicated sale ledger, current-format save v10 plan, UI integration and acceptance tests.
- Important implementation constraint: the existing `Shop.TryFulfil` removes by item type and does not validate asset ownership or record a sale. The new path must preserve its safety/credit/rollback rules while targeting the exact selected `ItemInstanceId`; do not call its old type-wide removal path for a selected-stack sale.
- No gameplay code, business buyback entries or buyback prices were changed/created. Await explicit user approval before implementation.


## 2026-11-19 — Player-facing sell-to-shop implementation (Unity verification pending)

Implemented the approved player-facing sale slice on `chore/collaboration-workflow-and-audit`.

- Core `GridInventory.TryRemoveItemInstance(Guid, int)` removes quantity from one exact stable stack. Partial remainder preserves local/stable IDs, position, orientation and state.
- Core `Shop.TryFulfilExact` extends the established sale boundary with exact stack identity, seller ownership validation, clean CARD credit, full-stack ownership removal and rollback. The existing type-wide `Shop.TryFulfil` API remains available and unchanged for its existing callers/tests.
- Added `ShopSaleTransaction` and `ShopSaleTransactionLedger`, separate from physical asset transfers and unlimited retail purchases. Records retain business, seller, stack/item IDs, quantity, fixed price/total, item-state snapshot and in-game timestamp.
- `BusinessDefinition` now has an independent `BuybackEntry[]` with positive fixed clean-CARD prices, duplicate/invalid-item validation and `TryGetBuybackOffer`. Merchandise and buyback acceptance remain independent.
- `GameSession` composes the sale ledger, includes buyback-only definitions in the item resolver, validates counter proximity/business identity/open hours/current offer/exact stack/player ownership and routes sales through Core.
- `RetailPanel` now has BUY and SELL tabs. BUY continues to call the existing `RetailService`; SELL shows only carried player-owned stacks accepted by that business and provides per-stack quantity controls.
- Save format advanced from v9 to v10. The dedicated sale ledger is captured/restored; v9 saves initialize an empty sale list. Malformed individual sale records are skipped without rejecting the save.
- No Corner Bakery buyback items/prices were authored because the user approved the model, not content entries.
- Added 7 Core sale/inventory tests, 2 BusinessDefinition buyback tests and 3 save/migration tests (12 new test cases; expected suite 354 from the previously verified 342 baseline). These tests are authored but **not yet run in Unity**.
- No Unity compile, EditMode run or Play Mode verification has been performed by the assistant. User verification remains required.


## 2026-11-19 — Save-version test maintenance

- Audited the current save schema and migration tests after the v9 → v10 shop-sale ledger addition.
- `SaveMigrator.CurrentVersion` remains the single authoritative current-format number. It should change only when serialized persistent data changes; UI, runtime-only and ordinary gameplay logic changes do not require a save-version increment.
- Fixed the stale v8 migration test that expected literal version 9. It now expects `SaveMigrator.CurrentVersion` and also verifies that the migration initializes the v10 shop-sale ledger.
- Fixed the v9 → current migration test to assert `SaveMigrator.CurrentVersion` rather than literal 10. Historical source versions remain explicit in test fixtures.
- Added the versioning rule beside `SaveMigrator.CurrentVersion` and aligned the player-selling task status with the implemented code.
- These are test/documentation maintenance changes; no save schema or runtime migration behavior changed. Unity compilation and the full EditMode suite are pending user verification.
