# Roadmap — build the place before the systems that fill it

Milestones are dependency notes, not a mandate to build isolated systems in order. Keep one active gameplay initiative at a time. Prefer a complete, player-facing vertical slice over a collection of disconnected subsystems. See `docs/DEVELOPMENT_WORKFLOW.md`, `docs/ARCHITECTURE.md` and `docs/PROJECT_HANDOFF.md`.

## Current foundation — hideout, connected block and first persistent loop

**Player outcome:** the player can inhabit a small hideout, leave it to explore a connected city block, enter simple buildings, carry items, store/retrieve them at the physical locker and save/load inventory plus world location.

### Slice A — establish the hideout
- [x] Consolidate the active runtime on Signal.Core + Signal.Game; preserve the old Main/prototype path as legacy reference.
- [x] Add HideoutSlice as the active build scene.
- [x] Establish a walkable room with floor, walls and player collision.
- [x] Add placeholder bed, table, chair, storage and a physical workbench.
- [x] Add E prompt and a close-up workspace with draggable/inspectable tabletop objects.
- [x] User verified compilation and reported the first-slice tests passed in Unity.
- [ ] Replace placeholder geometry with authored, cohesive hideout art and atmosphere.

### Slice B — connect the first city block
- [x] Build a compact outdoor street space using the Core/Game runtime and placeholder geometry.
- [x] Connect hideout ↔ street ↔ corner shop ↔ apartment without recreating the player or session.
- [x] Preserve the same inventory, clock and GameSession while changing active place and camera bounds.
- [x] User confirmed the connected travel loop works in Play Mode.
- [ ] Run/retain automated city-transition coverage in the full suite.
- [ ] Replace placeholder street/building geometry with cohesive authored city art.

### Slice C — home storage and location persistence
- [x] Add a separate 8×6 hideout locker inventory and physical E interaction.
- [x] Add capacity-aware STORE/TAKE transfers with rollback protection.
- [x] Save carried inventory, locker contents, current place and player coordinates in save format v2.
- [x] Add v1 → v2 migration (empty locker; restore at Hideout).
- [x] Allow save/load while away from home and restore the active city place.
- [x] User verified the normal locker and save/load loop in Play Mode with no new Console errors.
- [ ] Verify full/blocked destination edge cases and load an actual v1 save.
- [ ] Verify save replacement behavior on target platforms.
- [ ] Connect authored RecipeDefinition content to the workbench only after recipes and production rules are designed.
- [ ] Give the bed and other furniture meaningful gameplay interactions only after their rules are agreed.
- [ ] Replace placeholder hideout art and improve interaction feedback.

## Completed foundation — connected NPC framework v1

**Player/world outcome:** residents share one city context, have authored routines and individual incomplete knowledge, notice local events, and can meet when their schedules and locations overlap. This is a reusable simulation foundation, not a finished narrative or social gameplay loop.

### Shared world and perception
- [x] Core registry for NPC identity, location, position and awareness radius.
- [x] Local event perception constrained by same location, NPC awareness radius and event visibility radius.
- [x] Individual knowledge distinguishing direct observation from hearsay, including event identity, source and confidence.
- [x] Directed relationships separating familiarity, trust and affinity.
- [x] Explicit knowledge sharing; no automatic city-wide broadcast or omniscient character decisions.
- [x] Character-scoped `NpcPerception` snapshots for decisions; the global event journal is not an NPC's personal memory.
- [x] Off-screen event publishing at another city location.

### Decisions, content authoring and encounters
- [x] Deterministic, low-cost Core utility brain paired with authored routines; no per-NPC language model or network AI.
- [x] Reusable `NpcUtilityProfile` for schedule windows, utility weights and need drift.
- [x] Socializing is hard-gated to the authored social window; needs/relationships influence decisions only inside that window.
- [x] ScriptableObject `NpcCharacterDefinition` and one generic `NpcPresenter`; no per-character runtime scripts.
- [x] GameSession content assignment and multiple resident definitions in the city.
- [x] Temporary Milo and Rhea fallback profiles prove different routines through the same presenter. They are fixtures, not final cast/story commitments.
- [x] Restrained clock-driven NPC-to-NPC encounters: mutual proximity, same location, one encounter per resident per pass, three-hour resident cooldown, 12-hour pair cooldown.
- [x] Each ambient encounter adds 0.01 familiarity both ways, leaves trust/affinity unchanged, shares at most one previously unknown non-encounter event, and publishes a generic observable event.
- [x] User confirmed 219/219 EditMode tests passed after this milestone (2026-11-17).

### Still future within NPC initiative
- [ ] Build a first small player-facing NPC scenario using the framework; design and approve its clues, interaction rules and consequences first.
- [ ] Add player-driven relationship changes and authored scenario consequences; ambient routines must not create dramatic changes by themselves.
- [ ] Design stable-ID persistence for relationships, selected memories, discoveries and durable world changes when a real scenario requires them.
- [ ] Validate the ScriptableObject authoring workflow in the Unity Inspector.
- [ ] Establish population/area scaling limits before creating a large cast.
- [ ] Add event-specific NPC goals/reactions only when a playable scenario demonstrates the need.

## Approved world pacing — direction C

**Design principle:** the city continues to live, while meaningful developments become more restrained the longer the player remains in one scenario phase. Advancing the scenario gradually opens new possibilities without releasing a backlog.

- Everyday simulation (routines, scheduled meetings, familiarity and limited personal knowledge exchange) continues independently.
- Consequential developments share one global pacing authority, not separate social/business/discovery event budgets.
- A candidate must already be valid in its context and eligible under scenario logic before requesting pacing permission.
- Waiting never banks multiple developments. After a long idle period, the director authorizes at most one; there is no catch-up burst or queued backlog.
- The shared interval ramps from configured minimum to maximum as a phase continues. Changing phase begins a fresh quiet interval/ramp; it does not release pending developments.
- Major narrative events remain explicitly authored and gated by scenario logic.
- [x] Implement Core-only `WorldPacingDirector`: configured min/max interval and ramp duration, explicit phase ID, shared authorization gate, no backlog, clock-rewind rejection.
- [x] Add seven focused Core tests. The first Unity run was 225/226 because one test expected a frozen interval; the implementation correctly advanced from 60 to 64 after 30 minutes. Corrected the test expectation, with no production-code change.
- [x] User confirmed the follow-up full EditMode suite passed **226/226** (2026-11-17).
- [ ] Design the scenario owner, context-valid candidate contract, phase lifecycle and approved tuning values.
- [ ] Design save/load reconstruction for the director's phase start and last authorization.
- [ ] Integrate only when a real scenario has a valid consequential development to authorize. Do not connect it to routine NPC encounters.

## Short-term goals — next playable outcome

1. **[x] Verify Opportunity 001: Milo's Parcel.** The user confirmed 284/284 EditMode tests and successfully completed the scenario in Play Mode, including the parcel inventory/save/load/drop-off loop and a nearby-resident witness.
2. **Validate authored NPC content in Unity.** Create and assign one or two real `NpcCharacterDefinition` assets through the Inspector; confirm that schedules, destinations, appearance and activity dialogue work without character-specific scripts.
3. **Implement only the scenario's required persistence.** Use stable IDs and versioned migration. Avoid persisting transient perception/proximity if it can be rebuilt.
4. **Keep the current block playable and readable.** Replace the most visible placeholders with cohesive art only when it improves the slice; preserve the compact scale and hideout as the anchor.
5. **Close known reliability checks** when convenient: storage capacity/rollback, actual v1 migration, city transition tests and target-platform save replacement.

Milo's parcel scenario and save v7 are user-verified (284/284 EditMode tests). Obstacle-aware NPC navigation v1 is also user-verified: the user completed the Play Mode inspection and confirmed 289/289 EditMode tests on 2026-11-18. NPC interruption/recovery v1 and the follow-up game-clock pause correction are user-verified: 293/293 EditMode tests pass and the user confirmed Play Mode behaviour. Opportunity 001 fulfils the first-scenario validation requirement; a second opportunity is optional, not a framework prerequisite. Do not start a broad economy, crime catalogue or generic quest framework in parallel.

## Medium-term goals — deepen the shared-city loop

These are directions, not an implementation queue. Select one coherent player-facing scenario at a time.
- Expand the small neighbourhood with a few authored residents, businesses and enterable spaces whose routines overlap.
- Introduce a first meaningful player-driven relationship interaction (for example, a conversation, favour, trade or helping choice) only after its rules and consequences are agreed.
- Establish a small goods/trade loop with a limited set of authored items and meaningful decisions around quality, condition, provenance or ownership where relevant.
- Add one legitimate livelihood and one illegal opportunity in the same city/economy, demonstrating that players can combine them rather than choosing separate paths.
- Add controlled world developments through scenario logic and the shared pacing gate once phase ownership, candidate validity and persistence are designed.
- Add visual/audio identity and interaction feedback to make the hideout and street feel inhabited.

## Long-term goals — the full Signal experience

These are approved broad pillars, not detailed mechanics or promises of scope:
- **One layered city and economy:** homes, shops, services, ordinary residents, clients, suppliers and criminal dealings share locations, people and flows of goods/money. Dedicated criminal hideouts may exist later but are optional, not a separate default world.
- **Parallel lifestyles:** the player can freely combine legal and illegal activity; legitimate work remains a satisfying path rather than a forced tutorial or lesser fallback.
- **Varied legitimate livelihoods:** trade, services, repair, gathering, processing, making goods and small businesses. Farming/crops/wheat/bread are optional examples, not the core legal path.
- **Crime as a major pillar:** fictional drug-related activity and a broad range of other illegal activities, each designed as a player-facing loop rather than a list of menu buttons.
- **One global criminal progression track:** all criminal activity contributes to one overall criminal level/progression system, not one skill tree per crime.
- **Serious criminal failure:** meaningful, potentially lasting consequences. Exact thresholds, penalties and recovery options remain to be designed.
- **Goods with histories and qualities:** quality, condition, authenticity, source/provenance and ownership status where they create meaningful inspection, trade and risk decisions.
- **People with public and private lives:** residents have routines, needs, motives, relationships, contacts and secrets; players discover rather than receive omniscient labels.
- **A lived-in hideout:** storage, recovery, workspaces, preparation/production spaces and personalisation; visible improvements and emotional attachment.
- **A slow, controlled world:** everyday life continues, while consequential changes are paced globally and major narrative beats remain authored. No idle-time backlog release.
- **Scale by importance:** key residents can have richer simulation; distant/background populations use cheaper abstractions and event-driven updates.

## Design boundaries and not-yet-approved details

The following are examples or broad directions, not approved detailed mechanics:
- Water grades/testing/filtering/bottling/selling.
- Food production, crops, baking and recipes.
- Buying, fencing or reselling stolen household goods.
- Any specific NPC's secret business, identity, quest or relationship arc.
- Police response, heat consequences, rent consequences, faction structures, reputation and quest rules.
- Exact crime catalogue, criminal XP thresholds, unlocks, penalties and recovery.
- Exact quality/condition/provenance rules, prices, supply/demand and customer budgets.
- The detailed player interaction model for social relationships.

Explain the player-facing rules, interactions, progression/balance implications and alternatives before implementing a new gameplay mechanic. The user is the game director and final authority.

## Engineering safeguards

- Active architecture: `Signal.Core` (plain C#, no UnityEngine) + `Signal.Game` (Unity-facing runtime). Dependency direction is Game → Core.
- Active build scene: `Assets/_Project/Scenes/HideoutSlice.unity`.
- `Assets/_Project/Scenes/Main.unity` and `Assets/_Project/_Prototype/Scripts` are legacy reference only. Do not add new gameplay there or run both architectures in one scene.
- Preserve Unity asset GUIDs and scene references; no casual renames/deletions.
- No new packages, Asset Store packs or third-party code without asking.
- Keep Core rules testable; use ScriptableObjects for authored definitions; avoid global lookups/new singletons.
- Save changes require versioned migration and tests.
- No standalone dotnet/msbuild builds; Unity owns compilation and tests.
- Assistant cannot run the user's local Unity Editor through GitHub. Clearly distinguish code changes from user-reported Unity compilation, EditMode and Play Mode verification.

## Definition of a complete slice

A slice is complete only when the player-facing scenario works in Unity Play Mode, relevant tests pass, the Console has no new errors, save/load is checked where relevant, known limitations are recorded and `docs/PROGRESS.md` reflects actual verification.


## Item identity, per-stack state and inspection

The item identity, per-stack state and inspection foundation is implemented; the user confirmed **245/245 EditMode tests passed**. Player item knowledge v1 and save v5 were subsequently user-verified at **255/255 EditMode tests** (2026-11-18). The current Milo parcel scenario/save v7 changes await Unity verification.

### Verified Item Instance v1
- [x] Stable stack identity (`Guid ItemInstanceId`) is separate from grid placement/local integer IDs.
- [x] Identity survives movement, rotation, snapshots, whole-stack transfers and save/load.
- [x] Whole-stack transfers preserve identity/orientation, never merge distinct stacks and leave non-fitting stacks in the source.
- [x] Save version 3 and v1/v2 migration preserve inventory layouts and assign unique stack IDs.
- [x] User confirmed 235/235 EditMode tests passed.

### Item state and inspection v1 — implemented, verification pending
- [x] Add per-stack quality, condition, authenticity and ownership state.
- [x] Preserve state through inventory operations and transfers.
- [x] Merge only stacks with the same definition and matching state.
- [x] Add provisional Core reference valuation based on authored base value and state multipliers; this is not a trade/economy system.
- [x] Extend ItemDefinition with description, category, optional icon, base reference value and default state.
- [x] Add one read-only item viewer shared by carried inventory and home storage.
- [x] Add right-click inspection in carried inventory and per-stack INSPECT/STORE/TAKE actions in home storage.
- [x] Add targeted whole-stack transfer by stable identity.
- [x] Upgrade save format to v4; migrate v1/v2/v3 items to neutral default state.
- [x] Add Core tests for state-aware stacking, state-preserving transfer, valuation, targeted transfers and save migration/round-trip.
- [ ] Run Unity compilation and the full EditMode suite.
- [ ] In Play Mode, inspect two copies of one definition with different states in both carried inventory and locker.
- [ ] Verify single-stack STORE/TAKE preserves state and identity through repeated transfers.
- [ ] Verify a v3 save loads with neutral state and a v4 save preserves all state fields.

**Current boundaries:** item states remain authoritative; player item knowledge is separate and can be partial/uncertain. Milo's discovery ledger records scenario signals, not global truth. Valuation factors are provisional reference values only, not transaction prices. State is per stack, not per unit; there is no split-stack operation. No provenance chain, item use, equipment, crafting, weight, trade or generic quest-item rules are added.

## Quest and player progression foundations — later, scenario-led

- [ ] Build a small event-driven quest/opportunity contract alongside a real short scenario, after item identity and meaningful player interactions exist.
- [ ] Keep discovery separate from acceptance and completion where content needs it; do not make every opportunity a generic checklist or quest marker.
- [ ] Have objectives react to semantic actions from authoritative systems, not UI text, per-frame polling or arbitrary inventory snapshots.
- [ ] Keep NPC knowledge character-scoped; quest UI must not reveal facts the player has not discovered.
- [ ] Design player stats/skills only when actual activities prove the need. Preserve the approved single global criminal progression track; do not copy per-crime skill trees.
- [ ] Add cosmetic character customization independently when the player art/animation pipeline is ready; avoid legal/criminal classes that lock parallel lifestyles.


## Approved vertical slice — Opportunity 001: Milo's Parcel

Player outcome: discover that Milo is helping someone move stolen goods, decide whether to assist, and experience a reversible delivery situation with consequences shaped by timing, parcel condition and exposure. Milo's motive is helping someone he knows; the parcel goes to a discreet physical drop-off. Discovery is flexible across observation, contextual conversation, physical evidence and uncertain rumour.

- [x] User approved the complete implementation specification (2026-11-18).
- [x] Add Core player discovery records/ledger, preserving signal type, source, confidence, subject and learned time.
- [x] Add reusable contextual interaction conditions for NPC activity, relationship and discovery requirements.
- [x] Design and implement stable-ID opportunity definition/runtime state and permitted transitions.
- [x] Integrate authored discovery signals and contextual dialogue for Milo.
- [x] Add the physical stolen parcel and drop-off interaction.
- [x] Implement reversible accept/decline/withdraw paths and timing/condition/exposure outcomes.
- [x] Persist discovery/opportunity/player relationship state with save v7 and migrate v1-v6.
- [x] User confirmed compilation, 284/284 EditMode tests and successful Milo parcel Play Mode completion (2026-11-18).

Do not expand into a generic quest journal, second named recipient, police/criminal progression, general rumour engine or unrelated mechanics. See docs/tasks/OPPORTUNITY-001-MILO-PARCEL.md.


### Opportunity lifecycle and playable Milo slice — implementation added
- [x] Add stable-ID OpportunityDefinition with NPC association and contextual offer conditions.
- [x] Add Dormant → Available → Accepted → Resolved lifecycle, with return-to-Available withdrawal and non-terminal decline.
- [x] Ensure delivery resolves once, records timing/condition/exposure and expiry does not bank a backlog.
- [x] Add save v7 DTOs/migration for discoveries, player relationships and opportunity state; connect ownership through GameSession.
- [x] Connect authored Milo/Rhea interactions, physical parcel/evidence/drop-off and soft delivery consequences.
- [x] User confirmed 284/284 EditMode tests and successful Play Mode scenario completion (2026-11-18).


- [ ] If a later approved system introduces item damage, verify the Milo damaged-parcel outcome through that system; do not add a bespoke damage action solely to force this branch.


### Real-time NPC movement — implementation added
- [x] Move NPCs continuously between same-location activity destinations.
- [x] Route between shop/apartment interiors through their street doorways instead of teleporting on schedule changes.
- [x] Keep off-screen residents moving and synchronize their physical location/position with the shared social world.
- [x] Keep player interaction, ambient social encounters and Milo/Rhea scenario observations grounded in actual current location and arrival state.
- [x] User confirmed 284/284 EditMode tests and observed Milo walking between locations in Play Mode (2026-11-18).
- [ ] Revisit obstacle-aware pathfinding when city geometry grows beyond the current authored waypoint layout.


### City navigation geometry — first foundation added
- [x] Describe Hideout, Street, CornerShop and Apartment as connected navigation regions.
- [x] Register existing solid colliders as obstacle geometry and model current doorways as explicit bidirectional connections.
- [x] Add point-level walkability and location-graph queries without coupling Core to Unity.
- [x] Add an F1-toggleable runtime overlay for regions, obstacles, doorways, player/NPC positions and authored NPC route waypoints.
- [ ] Verify map accuracy and overlay visibility in Unity Play Mode; correct any geometry/coordinate mismatches found.
- [ ] Design character clearance and obstacle-aware pathfinding after inspecting the actual walkable geometry. Do not commit to a grid/navmesh algorithm before that review.
- [ ] Decide whether the shared map should become the movement authority for both player and NPCs; the current milestone is descriptive only.


## Next implementation — obstacle-aware NPC navigation v1

**Goal:** retain the existing walking NPCs and make their routes respect the generated city geometry.

- [x] Add clearance-aware walkability to the existing location/doorway map.
- [x] Add local A* pathfinding with diagonal corner protection, safe route smoothing and nearby safe endpoint selection.
- [x] Plan multi-location journeys through the shortest available doorway graph route.
- [x] Feed planned local route points into the existing NPC movement executor; keep schedules, walking speed, social-world position updates and doorway transitions intact.
- [x] Surface route failures in the F1 navigation overlay rather than silently accepting a straight-line route.
- [x] Add focused pathfinder and city integration tests.
- [x] User confirmed Unity compilation and 289/289 EditMode tests (2026-11-18).
- [x] User completed Play Mode inspection of the navigation overlay and NPC routes (2026-11-18).
- [x] User confirmed the existing Milo parcel scenario remains successful after navigation work.

**Scope boundary:** this is NPC route planning only. Player Rigidbody movement is unchanged. There is no moving-agent avoidance, crowd simulation, dynamic obstacle updates, click-to-move, navmesh, or new gameplay mechanic. The map is currently a build-time snapshot of static city geometry.


## Next implementation — NPC interruption and recovery v1

**Goal:** NPCs resume their authored lives reliably after developer intervention, simulation pauses or navigation failures.

- [x] Separate initial placement from normal schedule refresh; only initial binding may snap to a starting destination.
- [x] Resume a forced NPC activity by replanning from the resident's current physical location.
- [x] Preserve an in-progress route when simulation pauses and resumes without a schedule change.
- [x] If the schedule changes while paused, re-evaluate it and plan from the interruption point.
- [x] Add an explicit one-shot retry for failed routes, exposed in the F1 developer menu; do not retry every frame.
- [x] Add focused EditMode tests for forced activity recovery and pause/resume behavior.
- [x] User confirmed Unity compilation and 291/291 EditMode tests passed (2026-11-18).
- [x] User confirmed NPC movement/recovery works in Play Mode.
- [ ] Verify the follow-up game-clock pause movement correction; expected suite total is now 293.

**Scope boundary:** no new gameplay mechanic, autonomous obstacle avoidance, moving-agent avoidance, or dynamic navigation-map rebuilding. A failed route remains visibly failed until a deliberate retry or a later schedule change successfully replans it.


## Follow-up correction — game-clock pause and NPC movement

- [x] Add a shared transient pause state to GameClock and use it in the F1 developer clock toggle.
- [x] Stop real-time NPC movement while the game clock is paused, without coupling it to the separate NPC-simulation pause control.
- [x] Add tests for clock pause state and scheduled resident movement pause/resume.
- [ ] User verification in Unity; expected full EditMode suite: 293 tests.


## Roadmap clarification — second NPC opportunity is optional

Opportunity 001: Milo's Parcel satisfies the NPC initiative's requirement for a first player-facing scenario. The framework has been exercised through discovery, relationships, item knowledge, reversible commitment, delivery, witnesses and save/load. Do not schedule a second opportunity solely as a framework test. Revisit another authored opportunity when it serves a distinct player experience or naturally belongs in a larger gameplay loop.

**Recommended next initiative:** move toward the shared city's everyday gameplay loop, with a deliberately small goods/trade or service interaction that uses NPCs and item state/knowledge. First design the player-facing rules, value exchange, information asymmetry and consequences; do not implement an economy or commercial rules before the user approves the design. Extend Core only where this concrete loop demonstrates a need.

The authored `NpcCharacterDefinition` Inspector workflow remains a useful short validation task, but should not become a large standalone framework initiative.
## Economic system initiative — approved 2026-11-19

The shared-city economy is the active initiative. Its foundations are now in place; the next step is a deliberately small player-facing exchange, not more abstract economy infrastructure.

### Phase 1 — physical asset transfer foundation (implemented; 304/304 user-verified)
- [x] Stable economic actor identity.
- [x] Asset ownership independent of physical inventory location.
- [x] Whole-stack transfer with owner validation and rollback on failure.
- [x] Transaction records with participants, asset identity, quantity, reason and game timestamp.
- [x] Reuse existing GridInventory, stable item GUIDs and InventoryTransfer.
- [x] Core EditMode coverage for success, ownership mismatch, unregistered assets/containers, full destinations and same-owner storage moves.

### Phase 2 — clean account and economic persistence (implemented; 316/316 user-verified)
- [x] CleanMoneyAccount is the player's bank/card balance; existing PlayerStats money/rent/debug-sale APIs delegate to it.
- [x] Save v8 persists clean balance, carried/home-storage asset ownership mappings and completed economic transfer history.
- [x] Older v1-v7 saves migrate legacy money unchanged into cleanBalance and assign legacy held items to the player actor.
- [x] GameSession composes ownership, transaction, container-custody and transfer services.
- [x] HUD balance label explicitly reads CARD.
- [x] User confirmed the full EditMode suite **316/316 passed on 2026-11-19** after the save-version assertion correction.

### Phase 3 — first player-facing illicit exchange (design gate; not yet approved in detail)
**Recommended outcome:** the player can physically carry dirty-money items and use them in one discreet local exchange, while legitimate purchases continue to use only the clean card balance.

Before implementation, agree:
- The physical dirty-money item form and denomination/stack-value convention.
- The first exchange's context (who/where/why) and what the player receives.
- Whether the first slice includes cleaning or only illicit spending; cleaning is already approved as a future operation that consumes dirty-money items and credits the clean card balance directly, but its rate, prerequisites, risks and presentation remain undecided.
- Any immediate consequences or exposure risk. Do not invent police/heat consequences or criminal progression rules as part of the technical foundation.

Implementation should reuse the existing item identity/state, ownership and transaction services. Keep dirty money out of CleanMoneyAccount and out of legitimate purchase APIs. Prefer one complete local interaction over a generic market/business simulator.

### Phase 4 — bounded local economic activity (later)
- Simple authored availability for ordinary goods.
- Selective stock for gameplay-sensitive goods (e.g. drugs and illegal materials).
- No universal supply/demand, price fluctuation or continuous consumption simulation initially.

### Future consumers (separately designed)
- Resident/business/supplier financial profiles only when a concrete interaction needs them.
- World/economic events, stocks, lottery, betting, business ownership and laundering.
- Each consumer reuses the shared actor, asset and transaction foundations; none is automatically part of the first economy slice.

Stock markets, lottery, gambling, laundering, universal price simulation and business ownership are future consumers, not part of the initial economy foundation.


## Economy Phase 3 — first illicit exchange (specification finalized; awaiting approval)

- [x] Confirm physical dirty-cash denomination: one indivisible bundle worth 100 (MaxStack = 1).
- [x] Confirm a new discreet seller NPC, separate from Milo, in the existing shared city.
- [x] Confirm generic Contraband as the first merchandise.
- [x] Finalize atomic exchange, ownership/custody checks, save v9 seller-stock persistence, UI safeguards and test criteria in docs/tasks/ECONOMY-PHASE-3-ILLICIT-EXCHANGE.md.
- [ ] Obtain explicit user approval before implementation.
- [ ] Author exact bundle price and interaction placement; do not invent these values.

Scope excludes cleaning, legitimate dirty-money purchases, restocking simulation, dynamic prices, heat/police/exposure and criminal progression.
