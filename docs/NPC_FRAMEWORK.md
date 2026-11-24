# NPC framework — a connected neighbourhood, not isolated brains

## Purpose

NPCs are individuals living inside one shared simulation. Each character has their own needs, routines, relationships, memories and incomplete understanding of the city, but they do not inhabit a private simulation bubble. They share locations, time, world events, objects, activities and a social network.

Milo and Rhea began as fallback framework fixtures. They are now also represented by explicit authored NpcCharacterDefinition assets in the First Workshop scene composition; their current roles/routines are still small-slice content, not a locked final cast.

## Core principles

1. **One shared world context.** NPCs are registered in a common world model and occupy named locations with positions. Moving an NPC changes who they can encounter and what they can perceive.
2. **Local perception, not omniscience.** Events have a location and visibility radius. An NPC can directly observe an event only when it is in the same location and within both the event's visibility radius and the NPC's awareness radius.
3. **Individual knowledge.** Observations are stored on the observing character. A world event is not automatically known by every NPC.
4. **Social transmission.** Knowledge can spread when a character explicitly tells another. The listener records who told them, preserves the original event identity and receives a confidence value influenced by their trust in the speaker.
5. **Truth is not belief.** The world event journal represents occurrences; an NPC's knowledge represents what that NPC has observed or been told. Rumours are not promoted to objective truth. Future hidden identities and motives must not be leaked through public event descriptions.
6. **Relationships are directional.** One person may trust another more than the reverse. Familiarity, trust and affinity are separate values.
7. **Deterministic and affordable.** Core simulation is plain C#, advances from game/world events, and does not require a language model, network service, background worker or per-frame decision scoring.
8. **Shared-city continuity.** Legal and illegal activities, public life and concealed dealings use the same places and social network. No separate criminal-world simulation is implied.

## Current implementation

### Core (Signal.Core)

- NpcSocialWorld registers NPCs, tracks their current location/position, exposes nearby NPC queries, stores directed relationships and maintains a world-event journal.
- NpcWorldEvent records an occurrence and only the observable description supplied by the publisher, plus its location, time and visibility radius.
- NpcAgentState stores an individual's position, awareness radius, relationships and knowledge.
- NpcKnowledge distinguishes direct observation from information told by another NPC. It retains event identity, original actor, last source, confidence, event location/position/time and learning time.
- NpcRelationship stores familiarity, trust and affinity independently.
- NpcPerception creates a character-scoped decision snapshot: nearby NPC IDs and their directed relationship impressions, plus only that character's own knowledge. It deliberately excludes the global event journal.
- NpcUtilityBrain accepts that snapshot when evaluating its decisions. Familiarity/affinity with nearby people and personally known information can increase the value of socialising. It does not query global events or reveal information to other NPCs.
- NpcUtilityProfile contains configurable routine windows, utility weights, need drift and social-context weights. The brain uses a default day-worker profile when none is supplied, but the same algorithm can run different schedules without character-specific code. Socialize is only a valid activity inside the authored social window; needs and nearby relationships influence the choice within that window, not outside it.
- Explicit knowledge sharing uses the listener's trust in the speaker to adjust confidence. It does not make a rumour true and does not automatically broadcast information.
- NpcSocialEncounterSystem processes only residents whose authored activity is currently Socialize. It runs on clock changes, requires mutual local awareness and proximity, limits each resident to one encounter per pass and one encounter every three in-game hours, and applies a 12-hour pair cooldown.
- A scheduled encounter adds a small familiarity increment in both directions, leaves trust and affinity unchanged, and shares at most one previously unknown event. It publishes one generic observable social-encounter event; it does not author dialogue or invent a narrative consequence.

### Core (Signal.Core) — agenda progress first increment

- `NpcAgendaProgressBook` stores completed stages by stable NPC/thread/stage IDs and the selected choice ID. Independent threads and residents have independent progress; a stage can be completed only once, preventing replayed choices from applying their outcomes twice.
- The book exposes sorted progress snapshots and a restore hook for future SaveMapper integration. It stores no Unity objects or dialogue content.
- Focused EditMode tests cover one-time completion, independent threads, selected-choice retention, restore duplicate rejection and clearing. Save schema v16 now captures/restores agenda progress through SaveMapper and GameSession; v15 migration initializes the new collection and malformed entries are skipped.
- Game-layer authoring uses `NpcAgendaDefinition` ScriptableObjects with independent threads, priority ordering, sequential stages, optional activity/place, discovery, relationship, opportunity-phase, contract and acquired/not-acquired upgrade prerequisites, typed choices and repeatable choices for revisitable offers.
`NpcAgendaInteractionDirector` performs deterministic stage selection and applies one-time discovery/relationship outcomes. It opens an existing workshop offer only through a typed reference to its authored upgrade definition. `NpcAgendaConversationPanel` presents authored choices without putting story branches in NpcPresenter.
- Rhea's first three authored threads are loaded from Resources: a low-confidence second-hand Milo rumour (`subjectId=milo`, `sourceId=rhea`, confidence 0.4), the Compact Sewing Machine offer routed to the existing supplier panel, and post-upgrade mending advice. Keeping the rumour's subject as Milo is required by the parcel opportunity's existing distinct-discovery-type gate. The parcel controller no longer owns Rhea's dialogue; it retains physical parcel lifecycle mechanics.
- New Core and Game EditMode tests are added but none of these changes have been run in Unity yet.

### Game (Signal.Game)

- The city creates one NpcSocialWorld for its active shared-city session.
- NpcCharacterDefinition is a ScriptableObject content asset. It carries a stable ID, display name, public role, awareness radius, movement speed, NpcUtilityProfile, activity destinations, appearance colours and authored dialogue for each activity.
- Assets are created through **Assets → Create → Signal → NPC Character Definition** and assigned to the NPC Definitions array on GameSession. GameSession passes those definitions into CityBlockBootstrap at runtime.
- NpcPresenter is the one generic MonoBehaviour for residents. It reads the definition, runs a Core NpcUtilityBrain, walks between authored destinations in real time, builds a simple placeholder appearance, registers/moves in NpcSocialWorld and publishes conversation events.
- Schedule changes create waypoint routes through the connected street and the authored doorway of each interior. Residents walk to the exit in their current location, cross the doorway into the next location, and continue to their destination. Their logical location and shared-world position update as they move.
- Resident GameObjects remain active so off-screen residents continue their movement. Only their renderers are hidden when their physical location is not the player's active location. Changing the player's location never snaps a resident to a destination.
- Residents can be talked to only in their current physical location. Social encounters and Milo's contextual observations/rumours require residents to have physically arrived; travelling residents do not count as socializing participants or witnesses.
- Movement is real-time and uses the shared city navigation map: local A* routes account for static collider obstacles and character clearance, while a doorway graph connects coordinate-space locations. NpcPresenter executes the planned waypoints at authored walking speed. Dynamic obstacle updates and moving-agent avoidance are not implemented.
- CityBlockBootstrap runs NpcSocialEncounterSystem after residents have processed a clock change, so encounter eligibility reflects their updated authored activities. The system does not run per frame and does not make NPCs seek one another outside their schedules.
- CityBlockBootstrap supports any number of definitions and routes prompts/interactions to the matching presenter. It does not contain per-character behaviour branches.
- When no definitions are assigned, the bootstrap creates two temporary fallback definitions (Milo and Rhea). The active HideoutSlice now assigns authored Milo and Rhea definitions with resident starting balances, so the fallback path is not used in the main scene.
- CityBlockBootstrap.PublishWorldEvent publishes at the player's current location; PublishWorldEventAt can publish an event at another city location, allowing off-screen events to affect NPCs there.

## Important boundaries

- The event journal is a world-system record, not an NPC's personal memory. NPC decision-making should use that character's own knowledge and perceptions, not query the entire journal for hidden information.
- A publisher must describe what could be perceived, not reveal hidden causes, criminal roles or private motives to every witness.
- The current utility brain consumes the NPC's own NpcPerception snapshot for social decision scoring. Event-specific reactions and goals are not yet implemented; known information currently acts only as a modest reason to seek social contact.
- Ambient scheduled encounters currently increase familiarity only. Trust and affinity do not drift from proximity or routine conversation; those values are reserved for explicit player actions and authored scenario events. No friendship, loyalty, intimidation, romance, reputation or betrayal rules are being invented here.
- Ambient information exchange is deliberately capped at one known event per pair encounter, with the existing trust-based confidence calculation. This is not a free-running rumour cascade.
- General NPC-to-NPC social relationships and ordinary personal knowledge are not broadly serialized. The player-to-NPC relationship book is persisted, and the resident-theft crime outcome restores its specifically selected witness observation. Extend persistence only for a concrete scenario and stable-ID requirement.
- The utility brain is profile-driven at the Core level. Game character assets now serialize their own NpcUtilityProfile; the fallback Rhea profile intentionally starts work later than Milo to prove that schedules are data, not class-specific code.
- The former NeighbourNpc component has been removed. All residents use NpcPresenter; Milo and Rhea are identified by stable IDs and authored NpcCharacterDefinition assets in HideoutSlice, not character-specific runtime classes. Their current First Workshop roles are replaceable scenario content, not a lock on the final cast.

## Planned development sequence

1. **Shared awareness foundation — implemented.** The user confirmed the full 219/219 EditMode suite passed after the generic presenter and scheduled social encounter work on 2026-11-17. Registry, location-aware presence, local event perception, individual knowledge, directed relationships and explicit information sharing.
2. **Decision context — first pass implemented.** The utility brain receives a character-scoped perception snapshot and lets nearby relationships and personally known information influence socialising. Event-specific goals and reactions remain future work.
3. **Routine configuration — first pass implemented in Core and exposed in Game assets.** Each NpcCharacterDefinition serializes its own NpcUtilityProfile; the fallback Rhea profile demonstrates a later work start than Milo.
4. **Data-driven character definitions — first pass implemented.** ScriptableObject definitions, generic NpcPresenter, GameSession asset assignment and two fallback profiles now share one runtime path. Authored final cast/content remains deferred.
5. **Scheduled social encounters — first controlled pass implemented.** Residents whose authored routines overlap can meet when they are mutually nearby. Encounters slowly increase familiarity and may exchange one known event. Trust/affinity changes and player-driven relationship development remain future work.
6. **Player-driven relationships and authored scenario changes.** Meaningful trust, affinity, secrets, suspicion and major life changes should be driven by player actions and controlled scenario advancement, not background drift.
7. **Interactions and richer social behaviour.** Conversations, requests, helping, avoiding and event-specific reactions should use shared event/relationship/knowledge services.
8. **Persistence.** Save only meaningful long-lived state: stable NPC IDs, relationships, discoveries, selected memories and world changes. Rebuild transient proximity/perception from the restored world.
9. **Scale and performance.** Use location/area partitions and event-driven updates. Do not run expensive full-city reasoning for every citizen every frame. Important NPCs may receive richer simulation than distant background residents.

## Completion target for this initiative

Two temporary character definitions now use the same NpcPresenter and register in the same shared world; the Game integration test exercises Rhea's separate routine and conversation alongside Milo. The extended NPC integration tests were included in the user's confirmed 219/219 EditMode run on 2026-11-17. Two characters in the same place can perceive a shared event, exchange information with different confidence based on their relationship, and retain distinct knowledge. A character in another location or outside the perception radius does not receive that knowledge automatically. The utility decision layer consumes the character-scoped context for its first social decision pass; event-specific goals remain future work.

This is a framework milestone, not a narrative milestone. No new story arc, final NPC cast, crime activity, economy rule or relationship progression system is included.


## Shared world pacing — first Core foundation

- `WorldPacingDirector` is the single shared permission gate for consequential world developments. It does not create events, choose outcomes, or queue candidates.
- A calling system must first establish that a development has valid context (for example, relevant circumstances and an eligible scenario condition), then call `TryAuthorizeDevelopment`. Invalid candidates do not consume pacing permission.
- All development systems share one interval. There are no independent social/business/discovery event budgets in this first pass, and unused permission never accumulates into multiple future releases.
- The director requires an authored scenario phase ID. Reapplying the same phase does not reset its clock. Changing phase starts a fresh restraint ramp and a quiet interval, so phase advancement cannot dump a backlog.
- The permitted interval grows linearly from a configured minimum to a configured maximum over a configured phase-ramp duration. Long periods in one phase therefore make meaningful developments progressively less frequent.
- The director is deliberately not connected to ambient NPC meetings, familiarity, routine decisions or ordinary perception. Those remain everyday simulation.
- The director has no default tuning values and is not yet wired to scenario content. A future scenario owner must supply approved pacing values and call it only after validating a real candidate. No dramatic event catalogue or automatic catch-up scheduler is introduced.
- Clock rewinds are rejected for authorization rather than treated as fresh time; persistence/reconstruction of the director's phase and last authorization will be designed alongside scenario/save state before this becomes live gameplay.

- The newly added WorldPacingDirector tests are a separate, later change and still require a fresh Unity run; the 219/219 result predates this pacing addition.


## City navigation geometry — first shared representation (2026-11-18)

- Added `CityNavigationMap` in the Game layer. It describes each coordinate-space location as a walkable rectangular region, records solid colliders as obstacles, and joins locations through named doorway connections.
- The current map registers Hideout, Street, CornerShop and Apartment. The street is one connected area containing the road and both sidewalks; shop/apartment interiors use their own coordinate spaces. Doorway links describe how those spaces connect.
- Street/interior solid geometry is snapshotted from generated BoxCollider2D bounds. The hideout's TilemapCollider2D perimeter is represented as separate wall rectangles, preserving its south doorway instead of treating the tilemap's aggregate bounds as one solid block. Point-level walkability queries combine region bounds and obstacle rectangles. Character radius/clearance is not yet part of the query.
- Added a runtime navigation debug overlay, toggled from F1. It shows the active region, solid geometry, doorway endpoints, player position, visible residents and their currently authored travel waypoints.
- This is a geometry/connectivity foundation only. It does not change player movement, NPC movement, collision, or route selection. NPCs still use their existing authored waypoint routes; obstacle-aware pathfinding is deliberately next, after the map is inspected and validated in Play Mode.
- The map currently models the existing four places and their existing doorways only. New buildings should add their region, collider obstacles and explicit doorway connections through the same representation.


## NPC interruption and recovery v1 — implementation added (2026-11-18)

- Initial binding is the only normal lifecycle operation allowed to snap a resident to an authored starting position. Re-enabling a presenter and resuming schedules use the current physical position.
- Debug-forced activities remain temporary overrides. Returning to the authored schedule clears the override and plans a route from the NPC's current location/position to the activity selected by the current game clock.
- Pausing simulation freezes movement and preserves the current route. On resume, an unchanged schedule keeps that route; a changed schedule replaces it with a newly planned route from the interruption point.
- Failed navigation remains explicit. A caller may request one retry through DebugRetryNavigation; the F1 menu exposes a one-shot retry across failed residents. There is no per-frame retry loop.
- The map is currently a static build-time snapshot, so retry is a controlled diagnostic hook for cases where the navigation representation has become usable again; dynamic obstacle/map rebuilding is future infrastructure, not part of this milestone.
- Focused EditMode tests cover forced-activity return, pause/resume route preservation and schedule changes while paused. Unity compilation, the full suite and Play Mode validation remain pending user verification.


## NPC Agenda & Interaction Threads — approved initiative (2026-11-24)

The next narrative-interaction layer is a reusable, authored agenda system. It extends the shared-world and player-relationship foundations above; it does not replace NPC schedules, perception, knowledge, discovery, opportunities, contracts or economy services.

### Design contract

- Every thread has stable NPC, thread and stage IDs. Content is authored; runtime progress is deterministic and save-safe.
- Threads are independent. A resident may have several available conversations or offers without forcing one giant linear quest.
- Eligibility is evaluated from explicit context: resident identity/activity/location, player discoveries, relationship values, contract/opportunity state and acquired upgrades. Only configured requirements apply.
- A conversation stage may offer multiple player choices. Outcomes are typed, explicit, idempotent and applied once. Reopening a conversation must not duplicate relationship changes, discoveries, payments, items or unlocks.
- Dialogue and rumours preserve source and confidence. The player may learn what a resident believes or heard without the game declaring that belief objectively true.
- NPC knowledge is never inferred from the global event journal. A stage may reveal only information the resident actually observed, was told, or has explicitly authored knowledge of.
- Core owns stable progress and rule evaluation. Game owns ScriptableObject content, presentation and adapters to existing gameplay services. NpcPresenter remains generic and does not gain character-specific story branches.
- No procedural dialogue, LLM, free-running quest generator, background relationship drift or per-frame agenda evaluation.

### Rhea pilot threads

1. **A neighbour who notices things:** Rhea can share her limited, sourced information about Milo. The tip is a rumour with low confidence, not a verified conclusion. The player can ask how she knows, ask why she is telling them, or leave the subject.
2. **Something in the back room:** after the existing Milo customer-request prerequisite, Rhea can offer the Compact Sewing Machine. The agenda opens the existing supplier panel; the existing upgrade service remains authoritative for payment, ownership and installation. Declining does not permanently destroy the offer.
3. **Perhaps I need a favour myself:** after the machine transaction, Rhea can offer practical mending advice and establish a future repair-service/customer connection. This is narrative setup only; it does not implement B-04 Repair & Restoration.

The threads are independent wherever their prerequisites allow. Hearing the Milo tip does not require buying the machine; buying the machine does not require exhausting all dialogue.

### Milo parcel integration boundary

The parcel controller currently owns physical parcel props, inspection, acceptance, return, drop-off, delivery and outcome assessment. Preserve those mechanics and their stable IDs. Move only conversational content/choice presentation into the agenda framework where doing so produces a cleaner reusable path. The framework may call the existing opportunity service/controller through a narrow typed action; it must not duplicate parcel lifecycle rules or silently alter the opportunity's eligibility/outcomes.

### Implemented first pilot (2026-11-24)

- `NpcAgendaProgressBook` in Core stores completed choices by stable NPC/thread/stage IDs. The same stage cannot be completed twice; different residents and threads remain independent.
- Save schema v16 captures/restores agenda progress and migrates v15 saves by initializing an empty progress collection. Focused SaveMapper tests cover round-trip and malformed legacy entries.
- `NpcAgendaDefinition` ScriptableObjects author stable threads, ordered stages, priorities, choices, activity/location gates, discovery gates, relationship thresholds, opportunity phases, completed-contract gates and acquired/not-acquired upgrade gates.
- `NpcAgendaInteractionDirector` deterministically selects the highest-priority eligible thread, applies typed one-time outcomes, and delegates workshop offers to the existing supplier panel. It does not run a background AI loop.
- `NpcAgendaConversationPanel` is the reusable player-facing choice surface. `GameSession` routes the nearest eligible resident interaction through it, enforces modal input locking and revalidates the resident/stage before accepting a choice.
- Rhea's authored content is `Assets/_Project/Resources/Agendas/RheaAgenda.asset`. Her three independent threads are the sourced Milo rumour, Compact Sewing Machine offer, and post-purchase mending advice.
- The rumour is stored as a `Rumour` about subject `milo`, source `rhea`, confidence 0.4. The description explicitly says Rhea heard it second-hand and did not witness where the parcels went. All three dialogue choices still record the rumour the player has heard.
- The former Rhea rumour branch has been removed from `MiloParcelOpportunityController`. Parcel props, inspection, acceptance, return, delivery and outcome logic remain there.
- A second resident fixture in EditMode tests uses the same director; tests cover independent progress, deterministic thread priority, activity/location/contract/upgrade gates, relationship/opportunity gates and repeatable non-completing choices.
- Unity compilation, full EditMode execution and Play Mode validation are still pending. The previous 415/415 user result predates these additions.

### Implementation sequence

1. **Implemented:** Core progress book, stable IDs, idempotent completion and v16 save/load migration.
2. **Implemented:** Authored agenda definitions and validation for duplicate/missing IDs and invalid typed outcomes.
3. **Implemented:** Generic priority-based interaction director and reusable conversation panel; no character-specific logic in `NpcPresenter`.
4. **Implemented:** Rhea's three independent threads and existing supplier/contract service adapters.
5. **Implemented:** Rhea's sourced rumour moved into agenda content; Milo parcel lifecycle remains intact.
6. **Implemented:** A second resident fixture and focused tests for independent authoring, conditions and outcomes.
7. **Pending verification:** Import/compile, full EditMode suite, Play Mode thread flow and save/reload acceptance.

This initiative is approved. New gameplay mechanics outside these three Rhea threads remain subject to separate design approval.
