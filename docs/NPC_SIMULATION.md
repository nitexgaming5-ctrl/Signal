# NPC simulation — shared world, routines and individual knowledge

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


### Agenda progress

The reusable, data-authored NPC agenda and conversation system is documented and tracked separately in `tasks/NPC-AGENDA-INTERACTION-THREADS.md`. It builds on this simulation model but does not replace resident routines, individual knowledge or player relationships.

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
- **Persistence gap tracked by B-05:** player-to-NPC relationship state currently has a separate persistence path, while general NPC-to-NPC relationships and ordinary personal knowledge are not broadly serialized. B-05 unifies these into one canonical directed relationship model and persists durable knowledge/relationships by stable actor, NPC and event IDs. Transient position, proximity and perception remain reconstructed from the live world. The resident-theft witness observation and agenda progress must migrate through the same shared knowledge path without losing their existing behaviour.
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

## Related runtime and narrative references

- City navigation, world pacing, schedule interruption and route recovery: `NPC_RUNTIME.md`.
- Authored agenda threads, choices, prerequisites and Rhea pilot: `tasks/NPC-AGENDA-INTERACTION-THREADS.md`.


## Durable social state

Each NPC’s personal knowledge and directed NPC-to-NPC relationships are persisted through the existing save mapper after authored residents are registered. Knowledge retains its event identity, original actor, latest source, observation/told origin, confidence, event context and occurred/learned time. The global event journal is transient and is not serialized as an omniscient truth source. On load, durable social state is cleared and restored by stable IDs; unknown residents and malformed/source-invalid records are skipped. The separate `PlayerRelationshipBook` remains the player-to-NPC authority and continues to use its existing save path. NPC position, nearby perception and encounter candidates are rebuilt from the live scene.
