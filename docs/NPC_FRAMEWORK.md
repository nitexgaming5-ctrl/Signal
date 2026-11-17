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
- NpcUtilityProfile contains configurable routine windows, utility weights, need drift and social-context weights. The brain uses a default day-worker profile when none is supplied, but the same algorithm can run different schedules without character-specific code.
- Explicit knowledge sharing uses the listener's trust in the speaker to adjust confidence. It does not make a rumour true and does not automatically broadcast information.

### Game (Signal.Game)

- The city creates one NpcSocialWorld for its active shared-city session.
- Milo registers as an NPC and his simulated activity/location/movement updates his shared-world presence.
- Talking to Milo publishes a small conversation event into the same world context.
- CityBlockBootstrap.PublishWorldEvent publishes at the player's current location; PublishWorldEventAt can publish an event at another city location, allowing off-screen events to affect NPCs there.

The current scene still has one visible NPC. The shared Core model is intentionally ready for additional registrations, but bulk character content is deferred until narrative design is ready.

## Important boundaries

- The event journal is a world-system record, not an NPC's personal memory. NPC decision-making should use that character's own knowledge and perceptions, not query the entire journal for hidden information.
- A publisher must describe what could be perceived, not reveal hidden causes, criminal roles or private motives to every witness.
- The current utility brain consumes the NPC's own NpcPerception snapshot for social decision scoring. Event-specific reactions and goals are not yet implemented; known information currently acts only as a modest reason to seek social contact.
- Relationship values are data only. No friendship, loyalty, intimidation, romance, reputation or betrayal rules are being invented here.
- NPC knowledge and relationships are not yet part of the save format. Persistence must be designed with stable IDs and explicit migration before it is required by a playable scenario.
- The utility brain is now profile-driven at the Core level. The current Milo fixture still uses the default day-worker profile; authoring and assigning per-character definitions/profiles through the Game layer remains future work.
- The current Unity presenter remains Milo-specific. It must be separated from identity, appearance, dialogue and activity definitions before bulk character creation.

## Planned development sequence

1. **Shared awareness foundation — first implementation complete, tests pending.** Registry, location-aware presence, local event perception, individual knowledge, directed relationships and explicit information sharing.
2. **Decision context — first pass implemented.** The utility brain receives a character-scoped perception snapshot and lets nearby relationships and personally known information influence socialising. Event-specific goals and reactions remain future work.
3. **Routine configuration — Core first pass implemented.** NpcUtilityProfile configures routine windows, need drift and utility weights; the active Game fixture still uses the default profile.
4. **Data-driven character definitions.** Separate stable identity, public role, appearance, routine preferences, needs and authored dialogue from generic runtime components. Adding a character should mostly be content authoring, not a new MonoBehaviour.
4. **Interactions and social behaviour.** Conversations, requests, helping, avoiding, exchanging information and reactions should use shared event/relationship/knowledge services.
5. **Persistence.** Save only meaningful long-lived state: stable NPC IDs, relationships, discoveries, selected memories and world changes. Rebuild transient proximity/perception from the restored world.
6. **Scale and performance.** Use location/area partitions and event-driven updates. Do not run expensive full-city reasoning for every citizen every frame. Important NPCs may receive richer simulation than distant background residents.

## Completion target for this initiative

A second temporary test character can be registered without adding another character-specific runtime class. Two characters in the same place can perceive a shared event, exchange information with different confidence based on their relationship, and retain distinct knowledge. A character in another location or outside the perception radius does not receive that knowledge automatically. The utility decision layer consumes the character-scoped context for its first social decision pass; event-specific goals remain future work.

This is a framework milestone, not a narrative milestone. No new story arc, final NPC cast, crime activity, economy rule or relationship progression system is included.
