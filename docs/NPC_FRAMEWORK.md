# NPC framework — a connected neighbourhood, not isolated brains

## Purpose

NPCs are individuals living inside one shared simulation. Each character has their own needs, routines, relationships, memories and incomplete understanding of the city, but they do not inhabit a private simulation bubble. They share locations, time, world events, objects, activities and a social network.

The first Milo implementation is a disposable test character. Its purpose is to exercise the framework; his identity, schedule, dialogue, appearance and role are not production commitments.

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

### Game (Signal.Game)

- The city creates one NpcSocialWorld for its active shared-city session.
- NpcCharacterDefinition is a ScriptableObject content asset. It carries a stable ID, display name, public role, awareness radius, movement speed, NpcUtilityProfile, activity destinations, appearance colours and authored dialogue for each activity.
- Assets are created through **Assets → Create → Signal → NPC Character Definition** and assigned to the NPC Definitions array on GameSession. GameSession passes those definitions into CityBlockBootstrap at runtime.
- NpcPresenter is the one generic MonoBehaviour for residents. It reads the definition, runs a Core NpcUtilityBrain, walks between authored destinations in real time, builds a simple placeholder appearance, registers/moves in NpcSocialWorld and publishes conversation events.
- Schedule changes create waypoint routes through the connected street and the authored doorway of each interior. Residents walk to the exit in their current location, cross the doorway into the next location, and continue to their destination. Their logical location and shared-world position update as they move.
- Resident GameObjects remain active so off-screen residents continue their movement. Only their renderers are hidden when their physical location is not the player's active location. Changing the player's location never snaps a resident to a destination.
- Residents can be talked to only in their current physical location. Social encounters and Milo's contextual observations/rumours require residents to have physically arrived; travelling residents do not count as socializing participants or witnesses.
- Movement uses authored doorway waypoints and direct segments between destinations. It is real-time but is not yet a general obstacle-avoiding pathfinder; future map complexity may require authored path nodes or navigation.
- CityBlockBootstrap runs NpcSocialEncounterSystem after residents have processed a clock change, so encounter eligibility reflects their updated authored activities. The system does not run per frame and does not make NPCs seek one another outside their schedules.
- CityBlockBootstrap supports any number of definitions and routes prompts/interactions to the matching presenter. It does not contain per-character behaviour branches.
- When no definitions are assigned, the bootstrap creates two temporary fallback definitions (Milo and Rhea) to exercise different routines and the shared interaction path. They are test fixtures, not final narrative characters.
- CityBlockBootstrap.PublishWorldEvent publishes at the player's current location; PublishWorldEventAt can publish an event at another city location, allowing off-screen events to affect NPCs there.

## Important boundaries

- The event journal is a world-system record, not an NPC's personal memory. NPC decision-making should use that character's own knowledge and perceptions, not query the entire journal for hidden information.
- A publisher must describe what could be perceived, not reveal hidden causes, criminal roles or private motives to every witness.
- The current utility brain consumes the NPC's own NpcPerception snapshot for social decision scoring. Event-specific reactions and goals are not yet implemented; known information currently acts only as a modest reason to seek social contact.
- Ambient scheduled encounters currently increase familiarity only. Trust and affinity do not drift from proximity or routine conversation; those values are reserved for explicit player actions and authored scenario events. No friendship, loyalty, intimidation, romance, reputation or betrayal rules are being invented here.
- Ambient information exchange is deliberately capped at one known event per pair encounter, with the existing trust-based confidence calculation. This is not a free-running rumour cascade.
- NPC knowledge and relationships are not yet part of the save format. Persistence must be designed with stable IDs and explicit migration before it is required by a playable scenario.
- The utility brain is profile-driven at the Core level. Game character assets now serialize their own NpcUtilityProfile; the fallback Rhea profile intentionally starts work later than Milo to prove that schedules are data, not class-specific code.
- The former NeighbourNpc component has been removed. All residents use NpcPresenter; Milo is identified only by his temporary data definition and stable ID.

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
