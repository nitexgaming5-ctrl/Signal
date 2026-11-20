# Signal — project handoff for the next agent

> Read this document together with `AGENTS.md`, `docs/PROGRESS.md`, `docs/ROADMAP.md`, `docs/VISION.md`, `docs/ARCHITECTURE.md`, `docs/NPC_FRAMEWORK.md`, `docs/DESIGN.md`, `docs/OPEN_QUESTIONS.md` and `docs/DEVELOPMENT_WORKFLOW.md` before substantial work. This is the consolidated design and implementation handoff from the discussions and user-confirmed decisions through 2026-11-20. The user is the game director.

## Latest verification update — 2026-11-20

- SCAVENGE-001 physical containers and progressive search are complete. The user confirmed the latest full EditMode suite **368/368 passed** after UI polish and confirmed empty-bin carried-inventory management plus grid snapping in Play Mode.
- Opportunity 001: Milo's Parcel is user-verified: 284/284 EditMode tests and successful end-to-end Play Mode completion were confirmed by the user. Any older pending-verification wording in historical notes is superseded.
- Player-facing sell-to-shop is user-verified: 355/355 EditMode tests and the BUY/partial SELL/full SELL/save-reload Play Mode path were confirmed; the user also observed Milo receiving business revenue.
- CRIME-THEFT-001 resident-owned apartment container theft is implemented and user-tested. The user confirmed 382/382 EditMode tests and that Play Mode works except unwitnessed theft incorrectly raised heat. A follow-up correction is committed: the apartment theft fixture awards +2 heat only when a local witness is selected; unwitnessed theft still awards +1 global progression and records the crime, but adds zero heat. The regression assertion is added to an existing test, so the expected suite remains 382. Post-fix Unity compilation/test rerun and quick witnessed/unwitnessed Play Mode retest are pending.
- Slice B implementation is committed on the collaboration branch at `12b62e82196b18603f20f039a3f122221726b757`: blank-name `Clean_Dirtycash` contact, ITEM_A ×2 → Dirty Cash ×3 fence offer, contact-specific 20% cleaning fee (24 CARD to player, 6 CARD to contact), persistent contact stash, dedicated ledgers and save schema v14. The user's first full 392-case EditMode run reported 391/392: all Core tests passed and one Game test failed because its blank-name contact fixture omitted the SocialDialogue value that the assertion expected. Both Dirty Cash save tests passed. A test-only fixture correction is committed/pending rerun; Play Mode verification remains pending.
- Verification is based on user reports. The assistant has not run the local Unity Editor.

## Current next action — Slice B verification

Pull the latest test-fixture correction on `chore/collaboration-workflow-and-audit` and rerun the full 392-case EditMode suite. Then verify the exact fence exchange, 24/6 cleaning split and save/reload of the contact stash, ownership, accounts and transaction histories in Play Mode. Do not claim this slice complete before those checks are reported.

## 1. What Signal is

**Signal is an immersive top-down 2D urban RPG about making a precarious living in one layered city.** The player goes out into the neighbourhood, meets people, performs small activities for money or resources, trades/acquires goods, takes calculated risks, sometimes loses things, discovers what is happening around them, and returns to a personal hideout that gradually becomes more useful and personal.

Signal blends:
- Urban life and hustle simulation.
- Legal and illegal livelihoods that coexist in the same city.
- Narrative-driven role-playing through people, places, relationships, secrets and consequences.
- Hands-on spatial interaction with objects, workspaces, shops, homes and residents, rather than everything being an abstract menu.

The player is **not primarily managing a shop**. A shop or counter can be one activity, but the broader loop is about moving through the city, doing things, acquiring/handling goods, meeting people, making decisions under pressure and returning home.

The hideout is the emotional anchor. It is a modest place the player physically inhabits, improves and personalises. It should feel like home, not a sterile management screen.

## 2. The shared-city rule — especially important

There is one shared city, one overlapping social world and eventually one connected economy.

- Legitimate businesses, ordinary residents, criminal opportunities and concealed dealings coexist in the same streets, homes, shops and social networks.
- Do not build a separate default "legal city" and "underground city". The underground is not a second world or a mandatory separate map. Specific criminal hideouts or discreet locations may exist later, but only as optional places inside the same city.
- A person can have a normal public occupation and a concealed private activity. The user's example: a baker might be selling drugs without the player knowing. The player sees the ordinary baker first and discovers more through play.
- Legal and illegal activities are parallel lifestyles. The player can combine them freely; choosing one does not lock out the other.
- The same NPCs, contacts, customers, suppliers, rumours and events can connect lawful and unlawful activity.

Do not make NPCs or entire locations automatically labelled "underground". Hidden activity should be discovered, not revealed by a map category or omniscient UI.

## 3. Approved gameplay pillars and direction

### Hideout and physical life
- Walkable home with furniture and props.
- Storage, preparation, recovery, workspaces, production spaces and personalisation can grow over time.
- Improvements should be visible in the room.
- The hideout remains the central hub between outings.

### Goods and handling
Goods may have properties beyond a name and stack count: quality, condition, authenticity, origin/provenance, ownership status and risk. Add these only where they create useful player decisions (inspection, appraisal, use, trade, concealment, resale, etc.), not for complexity alone.

Examples discussed include water of different qualities, food/everyday goods, electronics/clothing/household goods and stolen items. These are examples, not a final catalogue or approved detailed rules.

### Legal livelihoods
The player should have satisfying ways to make a living without crime. Possible directions include trade, services, repair, gathering, processing, making goods and small businesses. Farming is optional. Wheat/crops/bread were an example, not a commitment to make farming the central legal loop.

### Crime
- Crime is a major pillar, including fictional drug-related activity and a broad range of other illegal activities.
- All criminal activity contributes to one global criminal progression track, rather than separate skill trees for each crime.
- Serious criminal failure should have harsh, potentially lasting consequences.
- The first action is now selected and implemented as a prototype: stealing a fixed authored item from a resident-owned apartment container. Rhea's wardrobe is the first content fixture; the owner/location are data-driven, and the flow is reusable for future `StealingHouse_NPC` content.
- The first fixture records successful theft, marks the item Stolen, awards editable prototype heat/progression, and gives at most one local resident direct-observation knowledge. It has no automatic report or police response.
- Other crime actions, final progression thresholds, penalties, police response, heat bands/cooling and recovery rules remain open. The current +2 heat/+1 progression values are prototype tuning, not final balance.

### People and narrative
NPCs are people first, not quest dispensers. They have public roles, private motives, routines, needs, relationships, incomplete knowledge, secrets and social networks. Their lives should not exist only when the player is nearby. The player discovers opportunities and truths through observation, conversation, requests, rumours, inspection and consequences.

## 4. Visual and interaction direction

- Top-down 2D.
- Readability and approachable spatial clarity inspired by Stardew Valley, adapted to a gritty contemporary urban setting. This is not a request to copy Stardew's exact art style.
- Clear silhouettes, readable rooms, expressive props, layered interiors, restrained neon accents and atmospheric lighting.
- Hideout should feel modest, personal and lived-in.
- Important activities should use world objects and physical-feeling interactions when they strengthen immersion.
- Reference games are experience references, not templates: Schedule I (compact city/contact loop), Drug Dealer Simulator (hands-on workspace/storage atmosphere), Probably Stolen (goods quality/appraisal/trade-offs), Torn Crimes 2.0 (variety of crime categories/global progression inspiration), Stardew Valley (readable cozy spaces) and Stalker/Tarkov (scarcity, inventory pressure and meaningful loot choices). Do not copy their exact systems.

## 5. Approved NPC simulation approach

The user explicitly chose a lightweight utility simulation paired with authored routines:
- Deterministic, affordable Core logic.
- Authored character identity, public role, routine, dialogue and profile data.
- A small utility brain chooses among a limited activity set based on time, needs and relevant local context.
- No per-NPC language model, remote AI service, random inference, background worker or per-frame full-population scoring.
- Important NPCs can receive richer simulation than distant/background residents.
- NPCs must share awareness of people, events and surroundings. The framework should be connected, not a collection of isolated brains.
- Milo was initially used as a baker prototype, but the user later clarified that the priority is a reusable connected framework, not Milo's personal story. Milo and Rhea are disposable fallback fixtures, not confirmed narrative characters.

### Current NPC activity and decision model
- Activities: Home, Work, Rest, Socialize.
- Default day-worker profile retains the initial routine structure; profiles can change their work/social/home windows and utility/need weights without changing brain code.
- Decisions are evaluated on a 15 in-game-minute cadence; needs drift with elapsed in-game time.
- Socialize is hard-gated to the authored social window. Need and relationship context can influence activity selection only inside that window.
- Decisions use an NPC's own `NpcPerception` snapshot, never the global event journal as an omniscient shortcut.

### Shared social world already implemented
Core types include `NpcSocialWorld`, `NpcAgentState`, `NpcWorldEvent`, `NpcKnowledge`, `NpcRelationship`, `NpcPerception` and `NpcPerceivedPerson`.
- NPCs register with location, position and awareness radius.
- Events are location/time/radius scoped and should describe only observable facts.
- Direct observation requires same location and both NPC awareness and event visibility.
- Each character has their own knowledge. A world event is not automatically known by all.
- Knowledge sharing is explicit, retains original event identity and records who last told the listener. Trust influences confidence; hearsay is not objective truth.
- Relationships are directed and distinguish familiarity, trust and affinity. One person can trust another more than the reverse.
- `NpcPerception` exposes nearby people and their directed relationship impressions plus only the observer's knowledge. It deliberately excludes the global journal.
- `ShareKnowledge` is an explicit communication operation. Do not add arbitrary proximity/channel restrictions without design; its caller is responsible for ensuring a valid conversation/contact.
- Event descriptions must not reveal hidden causes, criminal identities or motives to witnesses.

### Controlled ambient NPC-to-NPC encounters
User-approved rules:
- NPCs meet only when their authored schedules make them Socialize and they are mutually nearby in the same location.
- Player dialogue remains simple for now; build NPC-to-NPC simulation first.
- NPCs do not autonomously initiate conversations with the player.
- Ambient meetings may happen even while the player is absent, but effects must remain restrained and predictable.
Implemented first pass:
- Clock-driven, deterministic encounter pass after NPC schedule updates.
- Each NPC can encounter at most once per pass and once every 3 in-game hours; a pair has a 12-hour cooldown.
- Encounter requires same location and mutual awareness/proximity.
- Familiarity increases by 0.01 in each direction.
- Trust and affinity do not change from ambient meetings.
- At most one previously unknown event is exchanged; social-encounter events are excluded to prevent rumour chains.
- One generic observable event is published. No autonomous agenda, dialogue generation or dramatic consequence.

## 6. Approved world-pacing direction C

The user raised the risk that a player who spends a long time farming/doing activities could accumulate many world events, then see them all trigger at once when story progression advances. The desired result is gradual and very slow toward the end, not a burst of queued content.

Approved direction:
- Everyday simulation continues (routines, scheduled meetings, small familiarity changes, limited knowledge exchange).
- Meaningful/consequential world developments share one global pacing authority. Do not create separate independent cooldowns/event budgets for social, business, discovery, etc.
- Scenario advancement gradually opens new possibilities but does not release a backlog.
- A candidate must already be valid because of its context and scenario eligibility before requesting permission.
- Time alone is insufficient; valid circumstances and pacing permission are both required.
- Long idle periods never bank multiple authorizations. At most one can be granted; no catch-up burst.
- The shared interval becomes progressively longer as the player remains in one scenario phase.
- Major narrative beats remain explicitly authored and gated by scenario logic.
- This pacing layer should protect narration, not generate story by itself.

### WorldPacingDirector status
Implemented in Core as a policy gate only:
- Configured minimum interval, maximum interval and ramp duration (no guessed default tuning).
- Requires an explicit scenario phase ID.
- Same phase reapplication does not reset phase start; changing phase starts a fresh quiet interval/ramp.
- Linear interval ramp from minimum to maximum as the phase continues.
- One shared authorization across callers/categories.
- Valid-context check happens before authorization; invalid context does not consume permission.
- Long idle grants at most one; no backlog.
- Clock rewind is rejected.
- Does not create/select/queue events, own scenario logic, or connect to ambient NPC activity.
- Not yet integrated with a scenario owner or save system. Do not wire it into gameplay until those responsibilities and real candidates are designed.

The first Unity test run was 225/226. The only failure was a test expectation: after the same phase advanced 30 minutes, expected interval should have been 64, not 60. Test expectation was corrected; no production code changed. User confirmed the follow-up full EditMode suite **226/226 passed on 2026-11-17**.

## 7. Current playable/technical state

### Architecture
- Active architecture: `Signal.Core` + `Signal.Game`.
- Core is plain C# with no UnityEngine dependency; Game owns MonoBehaviours, UI, input, scene composition and Unity-facing presentation.
- Content definitions belong in ScriptableObjects.
- Dependency direction is Game → Core.
- Active build scene: `Assets/_Project/Scenes/HideoutSlice.unity`.
- `Assets/_Project/Scenes/Main.unity` and `Assets/_Project/_Prototype/Scripts` are legacy reference only. Preserve them; do not add new gameplay there or run both architectures in the same active scene.
- Do not bulk-migrate old prototype scripts. Rebuild only specific useful behavior against Core/Game.

### Verified slices
**Slice A — hideout/workbench**
- Walkable procedural starter hideout with bed, table, chair, storage, workbench and placeholder props.
- E opens a close-up UGUI tabletop workspace; props can be dragged/inspected; ESC/back closes.
- Visual-only prototype: no crafting/production rules implied.
- User confirmed compilation and tests passed.

**Slice B — connected city block**
- Compact procedural street, shop interior and apartment interior.
- Same player Rigidbody2D, inventory, clock and GameSession persist through transitions.
- E transitions hideout ↔ street ↔ shop/apartment; camera bounds update.
- User confirmed Play Mode travel works.

**Slice C — storage and persistence**
- Separate 8×6 home locker inventory.
- Physical locker interaction and STORE/TAKE panel.
- Capacity-aware Core transfer service with rollback protection.
- Save format v2 includes carried inventory, locker inventory, current place and player coordinates.
- v1 migration defaults to empty locker and Hideout.
- User confirmed normal locker transfer and save/load location restoration work with no new Console errors.
- Still worth checking full/blocked capacity, actual v1 migration, city transition automated tests and platform-specific save replacement.

### NPC framework
See `docs/NPC_FRAMEWORK.md` for the technical contract. Two fallback profiles (Milo/Rhea) use one generic presenter. The authored-content workflow exists but should still be validated by creating/assigning real ScriptableObject definitions in the Inspector.

### Test verification
- User-reported progression: 214/214 passed after earlier RoomBootstrap test corrections; 219/219 passed after generic presenter and scheduled social encounter; latest 226/226 passed after WorldPacingDirector test correction.
- These are Unity EditMode results reported by the user. The assistant cannot run the local Unity Editor through GitHub.
- Do not claim Play Mode verification of the WorldPacingDirector: it is Core-only and not wired into gameplay.
- The user confirmed HideoutSlice, city travel and storage/save-load normal flow in Play Mode as described above.

## 8. Immediate goals and recommended sequencing

### Immediate technical action: verify player item knowledge v1 in Unity
The user confirmed **255/255 EditMode tests passed** after player item-knowledge v1 and save v5 work (2026-11-18). The current shared branch adds Opportunity 001: Milo's Parcel and advances save format to v7. Pull the shared branch, allow Unity to compile, run the full EditMode suite, then verify the Milo discovery/offer/return/drop-off paths and save/load at Dormant, Available, Accepted and Resolved states. See `docs/PROGRESS.md`, `docs/ROADMAP.md` and `docs/tasks/OPPORTUNITY-001-MILO-PARCEL.md`. Fix reported failures before adding more systems.

### Next recommended player-facing initiative: one small NPC-driven playable scenario
The framework is now broad enough to support a first scenario. Do not expand abstract simulation indefinitely. Design a small complete experience in which:
1. The player encounters a resident in their public everyday role.
2. The player can notice or learn something through authored observation, conversation, a request, a clue or a rumour.
3. The player chooses whether/how to engage.
4. The choice produces a clear, contained outcome or consequence.
5. The event uses the same city, NPC network and places as ordinary life.

This was a proposed scenario shape. The user has since approved Opportunity 001: Milo's Parcel with exact design decisions recorded in `docs/tasks/OPPORTUNITY-001-MILO-PARCEL.md`. Do not broaden it into a generic quest system or add excluded mechanics.

### Supporting short-term work
- Validate one or two real `NpcCharacterDefinition` assets through Unity Inspector and active scene.
- Define which NPC state must persist for the selected scenario (stable IDs, relationship values, selected memories, discovery flags or durable world changes) and add versioned migration/tests only for those needs.
- Design the scenario phase owner and pacing-candidate contract if the scenario requires consequential background developments.
- Close the known storage/save edge-case checks when convenient.
- Improve placeholder art only when it materially improves the chosen playable slice.

### Medium term
- Add a few meaningful residents and locations with overlapping routines.
- Add player-driven relationship interactions with explicit rules.
- Build a small goods/trade loop with a limited authored item catalogue and meaningful quality/provenance/ownership decisions.
- Demonstrate one legal livelihood and one illegal opportunity within the same block and economy.
- Integrate controlled world developments through the shared pacing gate after phase ownership and persistence are designed.
- Improve hideout/city visuals, sound and feedback.

### Long term
- A connected city economy with homes, shops, services, clients, suppliers, residents and criminal dealings sharing the same spaces and networks.
- Parallel lawful and unlawful lifestyles with free mixing.
- Broad legal livelihood choices (trade, services, repair, gathering, processing, making goods, small businesses; farming optional).
- Varied fictional crime activities under one global criminal progression track.
- Serious potentially lasting criminal failure, with carefully designed thresholds and recovery.
- Goods whose quality, condition, authenticity, origin/provenance and ownership status create meaningful decisions.
- NPCs with public/private lives, relationships, secrets, contacts, incomplete knowledge and ongoing routines.
- A slow evolving city where everyday life continues but consequential developments remain globally paced and major narrative events authored.
- Scale simulation fidelity by importance and distance rather than simulating every resident equally.

## 9. Design approval gate — never skip

The user welcomes ideas and wants the agent to act as a technical lead and creative systems partner, but remains the final game director.

Before implementing a new gameplay mechanic or consequential design choice, explain:
- What the player will do/experience.
- The rules and outcomes.
- How it interacts with existing systems.
- Progression/balance implications.
- Technical dependencies and meaningful alternatives.

Wait for explicit user confirmation. Record approved decisions in the appropriate docs and unresolved questions in `docs/OPEN_QUESTIONS.md`.

Already approved broad directions are not permission to invent their detailed rules. Examples:
- Water grades/testing/filtering/bottling/selling are possibilities, not a designed loop.
- Food, crops, wheat, bread and farming are possibilities, not a fixed legal path.
- Stolen household goods circulating in the market are an example, not a finalized ownership/provenance system.
- The baker with a concealed illicit business is an example, not a committed character or quest.
- Crime is a pillar, but exact activities/progression/penalties/police rules remain open.
- NPCs may have secrets, but no specific secret, faction or storyline is established.
- World pacing direction C is approved, but tuning and scenario integration are not.

## 10. Collaboration and repository workflow

- Repository: `nitexgaming5-ctrl/Signal`.
- Shared working branch: `chore/collaboration-workflow-and-audit`.
- `main` is untouched; no PR workflow is required.
- User normally runs `git pull` locally while already on the shared branch. If switching branches is necessary, explain carefully and warn about uncommitted changes.
- Fetch current files and blob SHAs before updating; create new files with `create_file`.
- Update `docs/PROGRESS.md` after every substantial initiative. Keep `docs/ROADMAP.md` and relevant design docs consistent.
- User uses Unity 6000.6.9f1. Do not use standalone `dotnet`/`msbuild` commands; Unity owns compilation.
- Assistant cannot run local Unity tests or Play Mode. Report only what code changed and what the user actually verified.
- Prefer coherent end-to-end work over unnecessary micro-steps. User appreciates exact, practical explanations and clear file/Inspector locations when local work is needed.
- Keep one active gameplay initiative. Capture tangents in `docs/IDEA_BACKLOG.md`; do not implement parked ideas automatically.
- Preserve Unity .meta GUIDs, scenes and references. No new packages, asset packs or third-party code without permission.

## 11. Files worth reading first

- `AGENTS.md` — authority, safety, workflow and verification rules.
- `docs/VISION.md` — concise game identity and pillars.
- `docs/PROGRESS.md` — actual repository state and verification provenance.
- `docs/ROADMAP.md` — short/medium/long-term milestones.
- `docs/NPC_FRAMEWORK.md` — detailed NPC framework contract and boundaries.
- `docs/ARCHITECTURE.md` — Core/Game boundary and legacy decision.
- `docs/DESIGN.md` — first NPC utility design (some early Milo-specific prose is historical; current generic framework details are in NPC_FRAMEWORK.md).
- `docs/OPEN_QUESTIONS.md` — decisions still requiring the user's input.
- `docs/DEVELOPMENT_WORKFLOW.md` — one active initiative and vertical-slice-first process.
- `docs/DATA_MODEL.md` — definitions, runtime state and save conventions.
- `docs/IDEA_BACKLOG.md` — parked ideas, not an implementation queue.

## 12. One-sentence instruction to the next agent

**First verify the item state and inspection foundation in Unity; then return to one small, meaningful player experience in the shared city using the connected NPC foundation. Keep simulation restrained, consequential change gradual, and never turn an illustrative idea into a mechanic or story without the user's approval.**


## 13. Inventory identity, item state and inspection — current handoff state (2026-11-18)

### Verified Item Instance v1
The user confirmed **235/235 EditMode tests passed** after pulling the stable stack identity implementation.

- Every stack has a persistent `Guid ItemInstanceId`, separate from the inventory-local integer `InstanceId`.
- Identity survives movement, rotation, snapshots, whole-stack transfers and save/load.
- Transfers preserve orientation and identity, never merge distinct stack identities, and leave a stack in its source if the complete footprint does not fit.
- Save v3 stores stable identity; v1/v2 migrations assign unique IDs across carried and home storage while preserving old inventory layout and v1 Hideout/empty-storage defaults.
- No split-stack action exists. Identity is per stack, not per unit.

### Item state and inspection v1 — user-verified (245/245 EditMode tests, 2026-11-18)
The user explicitly approved proper inspection/interaction, an item viewer and different state values for different copies of the same definition. The current implementation adds:

- `ItemInstanceState` per stack: quality (Poor/Standard/Fine/Exceptional), condition (Damaged/Worn/Good/Pristine), authenticity (Genuine/Counterfeit), and ownership (Legitimate/Stolen).
- `ItemDefinition` authoring fields for description, category, optional icon, base reference value and default instance state.
- A Core `ItemValueCalculator` with provisional multipliers. Its output is an indicative reference value, not a buy/sell price or economy.
- State-aware stack merging: only same-definition stacks with matching state can top up one another.
- State preservation through movement, rotation, snapshots, targeted transfers and save/load. Core also exposes `TryUpdateItemState` to change the state while retaining the physical stack identity.
- A shared read-only item viewer. Right-click an item in the carried inventory or use INSPECT in the home locker.
- Home storage now lists individual stacks with all four state dimensions and supports inspecting or transferring a selected complete stack; it no longer hides distinct states behind one aggregate count.
- Save v7 persists item knowledge, player discovery signals, player-to-NPC relationships, opportunity lifecycle and delivery assessment. v1-v6 saves migrate with empty new discovery/relationship collections and no active opportunity; v1-v3 receive neutral item-state defaults (Standard, Good, Genuine, Legitimate), and existing stable stack identities are preserved.
- New tests cover state-aware stacking, valuation, targeted stack transfer and v3 migration defaults.

### Verification still required
The user confirmed **255/255 EditMode tests passed** after player item-knowledge v1/save v5. The current Milo scenario and save v7 changes are not yet Unity-verified. Pull the latest shared branch, let Unity compile, run the full EditMode suite, then test:
1. Right-click inspection of a carried item.
2. INSPECT from the home storage panel.
3. Two stacks of the same definition with different states remain separate and display different reference values.
4. STORE/TAKE moves only the selected stack and preserves its identity/state.
5. A v4 save migrates with no item knowledge; v7 preserves item facts, discovery sources/confidence/times, player relationships, parcel identity and delivery assessment.

### Important boundaries
- Authoritative item state is separate from player knowledge. The v1 knowledge book records partial beliefs with source/confidence/time; the viewer shows only known facts and withholds state-based valuation until all four properties are known. No player-facing inspect action, appraisal minigame, contradiction resolution or automatic NPC-to-player knowledge transfer exists yet.
- Value multipliers are provisional tuning only; no transaction prices, customer offers, supply/demand, bargaining or market economy.
- No provenance chain, durability decay, item use, equipment, crafting, weight, quest-item rules or split-stack behavior.
- Existing ITEM_A/B/C assets remain temporary fixtures; descriptions/icons/base values/default states can be authored in the Inspector later.
- Do not add a broad quest framework or item catalogue before a real player-facing scenario proves what is needed.


## 14. Developer debug menu — current testing support (2026-11-18)

- Press **F1** in Play Mode to open/close the in-game developer panel.
- Pause/resume all NPC schedules and movement independently of the game clock; pause/resume the clock separately.
- Inspect current clock, location, Milo/Rhea activity and location, Milo opportunity phase, discovery count and player-to-Milo familiarity.
- Focus the player beside Milo, force Milo to Work/Rest/Socialize/Home, or return him to his authored schedule.
- **Trigger Milo Offer Now** pauses NPC simulation, forces Milo to Socialize, focuses the player on him, opens the normal offer-choice UI without fabricating discovery or relationship facts. This bypasses discovery requirements for testing, not the parcel inventory/accept/return/delivery logic.
- The user confirmed Unity compilation and that the parcel appears in Play Mode. A 279/280 EditMode run had one stale evening-dialogue assertion; it was corrected on the shared branch. Rerun the full suite after pulling the latest changes.
- These are developer-only test controls, not player-facing mechanics. Do not treat debug-created discovery facts as authored story content.


## 15. Real-time NPC movement — implementation added (2026-11-18)

- Schedule changes no longer teleport residents between Work, Rest, Socialize and Home locations.
- NpcPresenter now follows a waypoint route: current interior exit → street-side doorway → destination street doorway → destination interior entry → authored activity position. Same-location movement walks directly to the destination.
- NPCs continue moving when their location is not currently displayed. Their GameObjects stay active; only child SpriteRenderers are hidden based on the player's active location. Social-world location/position follows the resident throughout travel.
- Player location transitions no longer snap NPC transforms.
- Social encounter eligibility and Milo/Rhea scenario observations require physical arrival, not just a scheduled activity label.
- The F1 debug menu's NPC summaries now report a travelling status where applicable. Deterministic movement stepping is available to EditMode tests.
- Movement currently follows authored doorway waypoints and direct segments. There is no general obstacle avoidance or dynamic pathfinding.
- This change has been committed to the shared branch, but Unity compilation, the full EditMode suite and Play Mode verification are still pending. Do not report it as tested until the user confirms.


## 16. City navigation geometry and diagnostics — implementation added (2026-11-18)

- `CityNavigationMap` describes the four current coordinate spaces (Hideout, Street, CornerShop, Apartment), their bounds, actual solid colliders and named doorway links.
- The street is one region for road plus both sidewalks; shop and apartment interiors are separate regions. Hideout connects to the street, which connects to each interior.
- Press **F1** and use **Toggle navigation geometry / NPC route overlay**. Close the menu to inspect the overlay in the game view. It marks the active region, solid obstacles, doorway endpoints, player, visible residents and their current authored travel waypoints.
- The map provides point-level geometry and a connected-location graph; it is not a pathfinder. No player/NPC movement behavior was changed. NPC routes remain authored waypoints until the geometry is inspected and validated.
- Unity compile, EditMode tests and Play Mode overlay validation are pending user verification.


## Current navigation milestone — 2026-11-18

The existing NPC movement system is already active: residents walk continuously between authored destinations, cross doorways between separate coordinate spaces, and continue moving off-screen. Obstacle-aware navigation v1 now layers route planning onto that movement system.

- CityNavigationMap records location regions, solid obstacle rectangles and doorway links; it now supports circular clearance queries.
- CityNavigationPathfinder uses cached local grids and A* with diagonal corner protection and line-of-sight route smoothing.
- CityBlockBootstrap.BuildNpcTravelRoute finds a route through the doorway graph and plans each local segment around obstacles.
- NpcPresenter still executes the route at its authored walking speed and preserves physical location/social-world updates. Failed routes are surfaced to the F1 overlay.
- The player controller is unchanged.
- The user should run Unity compilation, the full EditMode suite and Play Mode route inspection. Do not claim these new changes are verified until the user reports results.


## Current handoff — NPC interruption and recovery v1 (2026-11-18)

- Latest completed and user-verified navigation milestone: obstacle-aware NPC navigation v1, commit 3099aa202a411c2a121d007af0308987f97b72be; user confirmed 289/289 EditMode tests and completed Play Mode inspection.
- Latest implementation adds NPC interruption/recovery. Returning from a forced debug activity now replans from the resident's physical position; resuming a paused simulation preserves an unchanged route, while schedule changes during the pause replan from the interruption point.
- Presenter re-enabling no longer uses the initial-placement snap. First-time Bind remains the only normal initialization snap.
- Failed navigation has an explicit one-shot retry exposed in F1. There is no per-frame retry loop. The city map remains a static build-time snapshot; dynamic obstacle updates and moving-agent avoidance are not included.
- Added two focused EditMode tests to CityBlockBootstrapTests: forced-activity + pause/resume recovery, and schedule changes during a pause. The expected full-suite count is now 291 (previously 289); Unity compilation and tests for this latest change are pending user verification.
- Branch remains chore/collaboration-workflow-and-audit. Latest commit will be supplied in the implementation handoff.


## Latest update — game-clock pause correction (2026-11-18)

- User verified NPC interruption/recovery v1: **291/291 EditMode tests passed**, and the behaviour works in Play Mode.
- Follow-up testing found the F1 **Pause game clock** control stopped game-time accumulation but did not stop NPCs walking, because their movement advanced from Unity real time.
- Added `GameClock.IsPaused` / `SetPaused(bool)`. GameSession uses this shared state for the debug clock control and clock ticking; NpcPresenter gates real-time movement on the same state. The independent NPC-simulation pause remains separate.
- Added two tests (one Core pause-state test and one city movement integration test). Expected suite count is **293**. This latest correction has been pushed but is awaiting user Unity compile, EditMode and Play Mode verification.
- Branch: `chore/collaboration-workflow-and-audit`. Pull latest with `git pull`.


## Roadmap decision — second NPC opportunity deferred (2026-11-18)

- User confirmed the game-clock pause correction works and the full EditMode suite passes **293/293**. NPC interruption/recovery and clock pause are verified.
- Opportunity 001 (Milo's Parcel) satisfies the roadmap's first player-facing NPC scenario requirement. A second opportunity is not required to prove the framework and is deferred until it adds a genuinely different player experience.
- Recommended next direction: design a small shared-city everyday gameplay loop (goods/trade or a service interaction) that uses NPCs, item identity/state/knowledge and relationships. Design the rules and player experience first; obtain approval before implementing new gameplay mechanics. Extend Core only where this loop demonstrates a concrete need.
- The authored NPC ScriptableObject Inspector workflow remains a small validation item, not a blocker.
## Economic system — approved direction and current implementation (2026-11-19)

The user approved a full shared-city economic system. Retain the full approved direction: asset ownership, transfers, atomic transactions and records, clean accounts, dirty money, economic actors, businesses, suppliers, selective stock, world events and future financial activities (stocks, lottery, betting, business ownership). The following refinements are binding:

- Clean money is a bank/card balance used for legitimate purchases. Ordinary clean physical cash is omitted initially.
- Dirty money is a physical inventory asset. It cannot be used for legitimate card/bank purchases or deposited directly. It is reserved for explicitly illicit cash-based uses such as shady markets and bribes.
- A future cleaning operation consumes dirty-money assets and credits the clean bank/card balance directly.
- No universal supply/demand, price fluctuations or continuous consumption simulation initially. Ordinary goods use simple authored availability; stock tracking is selective for gameplay-sensitive goods such as drugs and illegal materials.
- Legal and illegal activities remain in the same shared city economy.

### Phase 1 status

Added runtime Core primitives:
- EconomicActorId
- AssetOwnershipBook
- EconomicContainerRegistry (runtime container custodian mapping, distinct from asset ownership)
- EconomicTransaction and EconomicTransactionLedger
- AssetTransferService

The service requires registered source and destination containers, validates the current asset owner, checks that the destination container is maintained by the receiving actor, updates ownership, preserves stable item identity/state, and records successful transfers with a reason and in-game timestamp. Source-container custody is intentionally independent from asset ownership. Same-owner storage moves remain ordinary InventoryTransfer operations. Core EditMode tests were added for success and failure paths.

This is not yet a persistent player-facing feature. Ownership and transaction state are runtime-only and are not connected to GameSession or save v7. Phase 2 must add versioned persistence before using these primitives in saved gameplay. Unity compilation, EditMode tests and Play Mode verification remain pending the user's Editor run.

Branch: chore/collaboration-workflow-and-audit.


## Economy initiative handoff — 2026-11-19

### Confirmed design
- One shared city economy for player, residents, businesses and suppliers.
- Clean money is the player's bank/card balance for legitimate purchases and ordinary clean expenses.
- Dirty money is physical inventory items only. It is excluded from legitimate card/bank payments and cannot be deposited directly. Future cleaning consumes those items and credits the clean account; illicit uses include shady markets and bribes.
- No universal supply/demand, automatic price fluctuation or continuous consumption simulation initially. Business merchandise is an authored unlimited catalogue for now; which goods, if any, need finite stock remains undecided.
- Stocks, lottery, betting, laundering, business ownership and richer economic events are future consumers of the shared foundation, not current features.

### Phase 1 and Phase 2 code state
- Phase 1 Core asset ownership, container custody, transfer service and transaction ledger were added. User confirmed 304/304 EditMode tests on that code.
- Phase 2 adds CleanMoneyAccount, preserves PlayerStats compatibility APIs, upgrades saves to v8, persists player-held asset owner mappings and completed transfer history, and wires the runtime economic services into GameSession.
- The GameSession container registry is reconstructed for carried inventory and home storage. The ownership book and transaction ledger are captured/restored with saves.
- Legacy v1-v7 money migrates unchanged to the clean balance; legacy carried/storage items default to player ownership when no explicit owner exists.
- Dirty-money content definitions, denominations and cleaning mechanics are not implemented yet.
- User confirmed the full EditMode suite **316/316 passed on 2026-11-19** after the Phase 2 save-version assertion correction. Economy-specific Play Mode verification has not yet been separately reported.


## 13. Latest economy status — 2026-11-19

The user confirmed **316/316 EditMode tests passed** after Economy Phase 2 and the three stale save-version test expectation fixes.

### Verified economy foundations
- Core asset ownership is independent of container custody.
- Whole-stack owner-to-owner transfers validate owner and registered custody, preserve item identity/state and append completed transaction records.
- `CleanMoneyAccount` is the player's clean bank/card balance. Existing `PlayerStats.Money`, `Earn` and `TrySpend` compatibility paths delegate to it.
- Dirty money is not an account balance and has no direct deposit or legitimate-payment path. It must be represented by physical inventory items for illicit exchanges.
- Save v8 persists clean balance, carried/storage asset ownership and completed economic transactions. v1-v7 legacy money migrates unchanged into cleanBalance.
- GameSession composes the ownership book, transaction ledger, container registry and transfer service; player carried inventory and home storage are player-custodied.
- HUD labels the clean balance as CARD.

### Verification provenance
The user's Unity Test Runner reported 313/316 initially. The three failures were old assertions expecting save version 7 in `ItemInstanceIdentityTests`; they were corrected to assert `SaveMigrator.CurrentVersion`. The user then confirmed **316/316**. The assistant did not run Unity independently. Economy-specific Play Mode checks have not yet been separately reported.

### Historical next step — superseded 2026-11-19
The earlier suggestion to implement a first illicit exchange was superseded when the user redirected Phase 3 to the shared business and retail economy. The active Phase 3 implementation and remaining Unity verification/content steps are documented in the latest handoff section below.



## Economy Phase 3 handoff — Business & Retail (first implementation pass added 2026-11-19)

The old Phase 3 discreet-seller / dirty-cash / contraband proposal is superseded. The active specification is docs/tasks/ECONOMY-PHASE-3-BUSINESSES-RETAIL.md.

User-approved rules:
- Every resident has a personal clean-money account.
- Businesses use predefined merchandise authored according to business/NPC roles.
- Prices are fixed authored values initially; reputation-based fluctuations may be considered later.
- Business sales credit the owner's personal account directly; no separate business revenue balance.
- Existing player-to-shop selling behaviour is retained without new acceptance or valuation rules.
- Merchandise is unlimited for now and available whenever the business is open.
- No supplier/delivery/production chain, finite stock/restocking, NPC shopping AI, automatic sales/revenue or continuous/hourly simulation.
- Dirty money remains physical inventory items, separate from clean accounts and legitimate retail.

### Code now on the branch
- Core resident account registry, atomic retail purchase service and retail transaction ledger.
- Save v9 resident account balances and retail transaction history; v1–v8 migration preserved and legacy account initialization made deterministic.
- NPC authored starting clean balance, BusinessDefinition ScriptableObject, click-driven RetailPanel (product rows with BUY buttons) and GameSession integration.
- Core and Game EditMode tests authored; not run by the assistant.
- F1 debug view shows each spawned resident's clean-money balance and the retail transaction count.

### Unity content authoring still required
The Corner Bakery definition has already been created at `Assets/_Project/Content/Businesses/Business_CornerShop.asset` and assigned to `GameSession.Businesses` in `HideoutSlice.unity`. It targets Milo, `CornerShop`, 06:30–18:00 and interaction point (1000, 1000.6).

The catalogue now contains `Item_Bread.asset` at a temporary CARD $5 test price, explicitly chosen only to exercise the purchase path. The item uses a 1×1 footprint and stack limit 10 with ordinary default state. No final price balancing is implied. Next, compile, run the full EditMode suite and verify a Bread purchase plus save/load in Play Mode. The assistant cannot run Unity. Existing Core `Shop` / `ShopOrder` selling logic remains unchanged.


### Retail UI test follow-up — 2026-11-19
- The retail panel is now click-driven: each catalogue row displays the item and fixed price with an adjacent BUY button. Paging and close are clickable; ESC also closes the panel. It uses runtime UGUI with an EventSystem and InputSystemUIInputModule.
- `HideoutSlice.unity` seeds the player's clean CARD balance at $5,000 for testing. This is only a test starting value. Loading an existing save restores the saved balance, so use a fresh game to observe the seed.
- This UI is still a first-pass test interface, not the final visual/UI design. Unity compile, tests and Play Mode purchase verification remain outstanding.


## Latest handoff correction — 2026-11-19

### Verified economy state
- Economy Phase 3 Business & Retail is complete. The user confirmed **335/335 EditMode tests passed** and all Phase 3 functionality works as intended in Unity.
- Purchases continue to use `RetailService` as the sole Core transaction authority. Do not bypass its account checks, quantity/price/overflow validation, inventory capacity handling, ownership registration or retail transaction recording from UI or GameSession.
- Existing `Shop` / `ShopOrder` sell valuation and acceptance rules remain unchanged.

### Transaction-safety audit follow-up
- `Shop.TryFulfil` now rejects uninitialized item definitions and zero-value orders before touching inventory. It retains the existing inventory snapshot/restore behavior if account credit fails.
- New regression cases cover zero-value and default-item sale orders, plus RetailService zero/negative quantity, zero/negative price and oversized totals. These new tests have not yet been run by the user; expected full suite is 342 test cases.
- The legacy sale helper is currently used only by the developer sale control. It does not write an economic transaction record or consult AssetOwnershipBook. This is not silently expanded here because player-facing sale acceptance/ownership/recording needs a deliberate integration design.
- No save schema change was made in this audit.

### Persistence policy — updated user decision
- Make the current save format reliable and test its round-trip whenever persistent state changes.
- Do not spend excessive time preserving obsolete development saves or maintaining migrations for every historical intermediate format.
- A substantial change may deliberately invalidate old development saves and require a fresh game.
- Do not silently corrupt or lose current-format progress; make intentional incompatibility/reset explicit and keep writes reliable.


## 2026-11-19 — Transaction audit verified; player-facing selling design

- User confirmed **342/342 EditMode tests passed**. The transaction-safety audit is closed.
- `Shop.TryFulfil` is a Core primitive receiving `ShopOrder(item, quantity, unitPrice)`; it validates and executes the sale by removing goods and crediting player clean CARD. It does not derive a unit price or define per-business acceptance.
- A player-facing sale flow is not implemented. Do not mistake the old developer test value ($10) for a real buyback policy.
- Draft design and technical acceptance criteria: `docs/tasks/ECONOMY-PLAYER-SELLING-DESIGN.md`.
- The only pending design approval is the buyback offer model. Recommendation: separate per-business authored buyback entries (item + fixed unit price), distinct from the business's retail catalogue. Do not implement until the user approves this or chooses an alternative.
- Proposed transaction must validate the exact player-owned stack, preserve state/identity on partial sales, remove ownership only when the stack is fully consumed, credit clean CARD atomically, and persist a sale-specific transaction record. The old physical-transfer ledger is not a sale ledger.


## 2026-11-19 — Player-facing sell-to-shop specification finalized

Q21 is resolved: the user approved **separate fixed-price buyback offers per `BusinessDefinition`**. Each business accepts only explicitly listed `ItemDefinition` entries at authored positive clean-CARD unit prices. The offer list is separate from its merchandise catalogue; catalogue inclusion does not imply buyback acceptance. No specific new offer items or prices are approved by this decision.

The finalized implementation contract is `docs/tasks/ECONOMY-PLAYER-SELLING-DESIGN.md`. Key constraints:
- Reuse the existing business interaction and `RetailPanel`; add SELL while preserving BUY through `RetailService`.
- Sell only from carried inventory, one exact selected stack at a time. Partial sales preserve stack identity/state/ownership; full sales remove the ownership entry.
- Validate player ownership and revalidate the business offer/opening hours at confirmation.
- Preserve existing `Shop.TryFulfil` safety and clean-account-credit semantics, but do not use its current type-wide stack removal for a selected-stack sale. Refactor/reuse the Core transaction boundary to target exact `ItemInstanceId`.
- Add a dedicated sale transaction ledger, separate from physical asset transfers and retail purchases; persist it in current save v10 with v9 initialization.
- No business budget debit, finite stock, dynamic valuation, dirty-money payment or automatic buyback content.

The specification is finalized, but implementation has **not** started. Wait for the user's explicit go-ahead before changing code.


## 2026-11-19 — Player-facing sell-to-shop implementation

The approved player-selling initiative has been implemented in code on the working branch; Unity verification is pending.

- `BusinessDefinition` has a separate `BuybackEntry` list (item reference + positive fixed clean-CARD unit price), independent of the merchandise catalogue. A temporary test-only Bread buyback offer at CARD $3 was used for Play Mode verification and has since been removed; do not treat it as final balancing.
- `GridInventory.TryRemoveItemInstance(Guid, int)` removes from one exact stack; partial sales preserve the same stack identity and state.
- `Shop.TryFulfilExact` validates the exact stack and seller ownership, removes the selected quantity, removes ownership only on full-stack sale, credits clean CARD and records one dedicated `ShopSaleTransaction`. Failure paths restore the inventory/account/ownership snapshot.
- `ShopSaleTransactionLedger` is separate from `EconomicTransactionLedger` and `RetailTransactionLedger`.
- `RetailPanel` now has BUY/SELL tabs and per-stack quantity controls. BUY still calls `RetailService`; SELL lists only carried, player-owned stacks accepted by the current business.
- `GameSession` revalidates the current nearby business, location, opening schedule, authored offer, exact stack and ownership at confirmation.
- Save v10 persists the sale ledger. v9 initializes an empty sale collection. Current-format round-trip and malformed-record recovery tests have been added.
- 12 new EditMode test cases have been authored; expected full suite is 354 from the last user-verified 342 baseline. They have not yet been run. No Unity compilation or Play Mode verification has been performed by the assistant.

Next: user pulls the branch, compiles, runs the full EditMode suite, then tests both partial and full sales plus save/load in Play Mode. Do not mark this initiative complete until those results are reported.


## 2026-11-19 — Save-version test maintenance follow-up

The recurring version failure was caused by a stale test expectation, not by a runtime migration defect: the v8 migration test expected version 9 after the current format had advanced to v10 for the dedicated shop-sale ledger.

- `SaveMigrator.CurrentVersion` is authoritative. The v8 migration test now expects that constant and checks initialization of resident accounts, retail purchases and shop-sale history.
- The v9 migration test now expects `SaveMigrator.CurrentVersion` rather than hardcoded 10.
- Historical input versions remain explicit in fixtures; only the expected migrated/current version uses the constant.
- The rule is documented beside `SaveMigrator.CurrentVersion` and in `docs/DATA_MODEL.md`: increment only for serialized persistent-schema changes, not ordinary UI/runtime/gameplay changes.
- No production migration behavior or serialized fields changed in this maintenance pass. Unity compile and EditMode verification remain pending.


## 2026-11-19 — 354/354 confirmed; sale boundary hardening

The user confirmed **354/354 EditMode tests passed** after the save-version test correction. The save-version maintenance issue is closed.

A further review of the active player-selling initiative found two defensive improvements, now committed:
- `Shop.TryFulfilExact` now rejects any seller other than `EconomicActorId.Player`, matching its player-facing contract. A new test verifies that even a resident who owns a stack cannot use the player sale operation; no inventory, ownership, account or ledger state changes.
- `GameSession.SellToBusiness` revalidates the exact active `BusinessDefinition` object reference, not merely its ID, to avoid stale or duplicate-ID content references being accepted.

The new regression test has not yet been run; expected full suite is now 355. Unity compilation and SELL Play Mode checks remain pending. The user explicitly authorized test content: `Business_CornerShop.asset` now buys the existing Bread ItemDefinition at CARD $3 (retail sale price remains $5). This is disposable test content, not a final economy decision. The next expected EditMode count is 355; only 354 has been user-confirmed so far.


### SELL test fixture update — 2026-11-19

The user authorized temporary test content: `Business_CornerShop.asset` briefly had a separate buyback offer for `Item_Bread.asset` at CARD $3 per unit, while its merchandise offer remained CARD $5. After the user confirmed the full SELL Play Mode flow and 355/355 EditMode tests, the temporary buyback offer was removed. The CARD $5 merchandise price remains temporary and unbalanced.

## Economy Engine inspiration captured (2026-11-19)

The user asked us to review Unity Asset Store's Economy Engine 1.4 as inspiration. Five directions are parked in docs/IDEA_BACKLOG.md and sequenced as future candidates in docs/ROADMAP.md: context-specific shared trade foundations, an Economy Library/editor workflow, contextual loot tables, item-for-item barter and narrowly authored currency exchange. These are not approved mechanics or a commitment to adopt the asset/package. Preserve existing Signal Core/Game architecture and current economy constraints. The SELL slice has since been fully verified and closed.


## Latest verified handoff — SELL milestone closed (2026-11-19)

The user confirmed that the full player-facing BUY → partial SELL → full SELL → save/reload sequence works in Unity Play Mode, and confirmed **355/355 EditMode tests passed**. The user also observed Milo receiving the Corner Bakery's purchase revenue in his personal account, validating the resident-owner credit path in the live scene.

- The SELL initiative is complete: exact carried-stack selection, player ownership validation, per-business authored buyback rules, partial/full sale behavior, clean CARD credit, dedicated sale history and save v10 persistence.
- The temporary Bread buyback offer at CARD $3 was removed from `Business_CornerShop.asset` after testing. It was disposable verification content, not approved final pricing. The temporary CARD $5 retail catalogue test price remains as before; do not treat it as balanced content.
- No final bakery buyback items/prices have been approved. The SELL system is implemented, but a business must have authored offers to accept items.
- Unity compilation, EditMode results and Play Mode behavior are user-reported. No assistant-side Unity execution is claimed.

**Next:** begin with a design proposal for the next compact player-facing everyday city loop. Use the current roadmap and parked idea backlog; explain the player experience, rules, information/relationship hooks, economic exchange and consequences, then wait for explicit approval before implementing new gameplay.


## 2026-11-20 — Active initiative: physical containers and scavenging

The user approved a physical-container scavenging slice. The player approaches a physical street bin, presses E, opens container and carried-inventory panels together, presses Search once, watches one item at a time become discovered, then double-clicks or drag/drops discovered items into carried inventory. Discovered items remain in the bin until transferred. Closing during search pauses it; reopening resumes at the next undiscovered item. Contents are generated once, saved, never rerolled, and do not respawn.

Three authored test loot profiles A/B/C and 2–3 street bins are the first content slice. Use existing ItemDefinitions, GridInventory, stable item IDs/state, safe transfer and save/load. Do not add third-party packages or copy Tarkov code. Tarkov is only an experience reference for progressive discovery, container-specific loot identity, concurrent inventory management and separate examination. Exclude Tarkov skills, raid rules, timed respawns and rarity economy.

Core groundwork now committed:
- Assets/_Project/Core/SearchableContainer.cs owns stable container identity, inventory, generated reveal order and discovered count.
- Assets/_Project/Core/SearchableContainerSaveMapper.cs captures/restores generated contents and reveal progress.
- Save format is v11.
- Initial Core tests cover reveal progression, one-time initialization, empty bins, interruption/reopen and taking a discovered item.

Not yet complete: Game-layer loot profile definitions/assets, physical bin placement, E interaction, side-by-side UI, automatic search animation, double-click and cross-panel drag/drop, GameSession save/load integration, complete tests and Unity Play Mode verification. Do not describe the scavenging loop as playable until those are finished.

The SELL milestone is now fully user-verified in Play Mode, including Milo receiving the business revenue; older pending notes in the historical progress section are superseded.


## 2026-11-20 — SCAVENGE-001 implementation committed; Unity verification pending

The complete first playable implementation batch for physical-container scavenging is now in the branch.

**Content and world**
- `Assets/_Project/Content/Containers/LootProfile_A.asset`, `LootProfile_B.asset`, `LootProfile_C.asset` are assigned to the active `HideoutSlice.unity` GameSession. They use the existing ITEM_A/B/C definitions with weighted test distributions.
- `CityBlockBootstrap` creates three physical solid bin props in Street, with stable IDs `street-bin-a/b/c`, prompts and E interaction.

**Runtime and UI**
- `ContainerLootProfile` generates actual GridInventory item stacks and randomizes a stable reveal order once.
- `SearchableContainerPanel` presents bin and carried inventory together. Search starts once and automatically reveals one stack at a time. Only discovered items are visible. Right-click inspects; double-click takes; drag/drop places the stack at the selected inventory cell.
- Closing the panel stops the animation only. The persisted started flag means reopening automatically resumes. There is no respawn.
- GameSession owns the runtime containers, registers bin custody and world ownership, uses AssetTransferService to take discovered stacks, and captures/restores all bin states.
- Scavenging transfers preserve ItemInstanceId and ItemInstanceState, change owner from the bin world actor to Player, and append an economic transaction with reason Scavenging. Capacity failure leaves the item in the bin.

**Persistence and tests**
- Save schema is now v12. SearchableContainerSaveMapper stores contents, reveal order, discovered count and searchStarted.
- v10 saves have no container records and generate bins on first interaction. v11 records retain their generated contents and progress but default searchStarted to false.
- Core tests cover one-time search start, save/resume, v11 migration, transfer identity/state/ownership and destination-full rejection. Game tests cover profile generation and physical-bin prompt/discovery.

**Unity verification still required**
1. Open `Assets/_Project/Scenes/HideoutSlice.unity` and confirm no compile errors.
2. Run the full EditMode suite.
3. Enter Street, approach each bin and press E. Confirm each profile yields contents once.
4. Press Search, wait for several reveals, close with ESC, reopen and confirm it resumes with the next item.
5. Double-click a discovered item; repeat using drag/drop to a chosen cell. Verify item identity/state and ownership.
6. Fill the player inventory and confirm a failed take leaves the stack in the bin.
7. F5 save while a search is partly complete, close/reload with F9, and confirm contents, discoveries, started state and taken items are unchanged.

No assistant-side Unity compile, EditMode or Play Mode run has occurred. Do not call this verified until those checks pass.


## Latest handoff — CRIME-THEFT-001 implementation committed (2026-11-20)

The user approved multi-slice implementation within the previously agreed crime/economy boundaries and selected theft from a physical apartment container as the first crime action. Rhea's wardrobe is the first authored fixture; the reusable property definition and Core crime systems reference owner/location data rather than hardcoding Rhea.

- Fixed contents: ITEM_A ×2 and ITEM_C ×1. Existing searchable-container UI and persistence are reused; no random loot, respawn or scavenging semantics are applied.
- Successful transfers use Theft, mark the item Stolen inside the atomic AssetTransferService operation, preserve stable identity and other item-state fields, and record the existing economic transaction.
- A separate CrimeOutcomeLedger records successful thefts with the player actor; CriminalProgressionTrack is one global non-decaying total. Initial editable fixture awards are +2 heat and +1 progression point per stolen stack.
- At most one nearest resident physically in the authored location and within the 8-unit witness/awareness radius receives an anonymous character-scoped event. Ambient social encounters do not share crime observations while witness-sharing/reporting rules remain open. No automatic reporting, police response, suspicion or relationship penalty.
- Crime outcomes, progression and selected witness identity/event persist in save v13. Loading restores the event to that same NPC only.
- F1 debug controls can force Rhea Home + focus, return her to schedule, and show heat/progression/outcome count.
- Parent design and acceptance details: `docs/tasks/CRIME-ECONOMY-001-SHARED-CITY-DESIGN.md`; implementation checklist: `docs/tasks/CRIME-THEFT-001-RESIDENT-CONTAINER-THEFT.md`.

**Verification:** implementation and tests are committed but Unity compilation, EditMode and Play Mode remain pending. The last user-confirmed baseline was 368/368; 14 new test cases make 382 the expected suite count. No assistant-side Unity execution is claimed.

**Next:** pull the branch, open HideoutSlice, run the full EditMode suite, then verify street-bin regression, wardrobe theft, item Stolen state, transaction/heat/progression, witness and no-witness paths, absence of automatic reporting, and save/load. Only after Slice A is verified should we choose whether dirty-money payment/cleaning or authored production comes next.


## Latest design decision — CRIME-ECONOMY-SLICE-B (2026-11-20)

The user approved Slice B and confirmed:
- New discreet shared-city contact, internal NPC ID `Clean_Dirtycash`, visible display name blank.
- Physical Dirty Cash denomination 10 per unit.
- Cleaning retains 20%, paying 8 clean CARD per unit immediately.
- One fixed fence purchase for one specifically authored stolen item; one separate contact-specific cleaning offer.
- Both transactions must be explicit, player-confirmed, atomic, separately recorded and save/load-safe.
- Dirty Cash cannot be used in legal retail or deposited directly. No cleaning risk/delay, heat, witness, police or passive laundering in this first slice.

Full specification: `docs/tasks/CRIME-ECONOMY-SLICE-B-DIRTY-CASH.md`.

**Before implementation:** the exact accepted stolen ItemDefinition and fixed Dirty Cash payout quantity remain unapproved. Ask the user to choose/approve that one fence offer. Do not invent a new narrative item, price or contact identity. The prior superseded Phase 3 illicit-vendor proposal remains historical and is not the active specification; Slice B is the newly approved, narrowly scoped transaction loop.


## Latest status — Slice B UI correction (2026-11-20)

The user confirmed **392/392 EditMode tests passed**. Save-version naming/numbering policy is deferred until a release/compatibility milestone; do not spend further effort on it now.

The user found the first Dirty Cash contact panel confusing and reported that the fence appeared executable without the required ITEM_A ×2, the review control behaved like a transaction confirmation, and the player's carried inventory was not visible beside the contact inventory. The UI has now been rebuilt:
- Side-by-side contact inventory (read-only) and player carried inventory (selectable), using the same grid-workspace visual language as SearchableContainerPanel.
- Separate Fence Offer and Clean Dirty Cash modes.
- The player must select the exact eligible stack. Fence review is a non-transactional step; confirm fence exchange is a separate control in a different screen position.
- Cleaning has its own selected Dirty Cash preview and Confirm Cleaning control. Fee and exact CARD payout are visible before execution.
- GameSession now receives the selected ItemInstanceId and validates that exact stack for accepted type, exact quantity, Stolen state and Player ownership. It no longer auto-selects any qualifying stack.

**Verification status:** code and docs are committed, but this latest UI correction has not yet been compiled or tested in Unity. The 392/392 result predates this UI-only change. User should pull the branch, confirm compilation, and test: no selection / wrong item / legitimate ITEM_A cannot fence; a valid stolen ITEM_A ×2 can be reviewed without executing; only Confirm Fence Exchange performs the exchange; Dirty Cash appears in carried inventory; selecting it in Clean mode shows the 20% fee and exact payout; only Confirm Cleaning cleans it; both inventory grids remain visible throughout. Then save/reload and regression-test scavenging, theft, and retail.
