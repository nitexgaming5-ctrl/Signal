# Roadmap — build the place before the systems that fill it

Milestones are dependency notes, not a mandate to build isolated systems in order. Work is organised around one active, player-facing initiative at a time. Combine technical tasks whenever that produces a coherent playable result. See `docs/DEVELOPMENT_WORKFLOW.md` and `docs/ARCHITECTURE.md`.

## Current foundation — hideout and connected starter block

**Player outcome:** the player can leave home, explore the connected block, bring items back, store and retrieve them at the physical locker, and save/load carried items, home storage and location.

### Slice A — establish the hideout
- [x] Consolidate the active runtime on Signal.Core + Signal.Game; preserve the old Main/prototype path as legacy reference.
- [x] Add HideoutSlice as the active build scene.
- [x] Establish a walkable room with floor, walls and player collision.
- [x] Add placeholder bed, table, chair, storage and a physical workbench.
- [x] Add E prompt and a close-up workspace with draggable/inspectable tabletop objects.
- [x] User verified compilation and reported the first-slice tests passed in Unity.
- [ ] Replace placeholder geometry with authored, cohesive hideout art and atmosphere.

**Exit check:** the player can move around the room, interact with the workbench and return to movement. This first pass was verified by the user; visual polish remains open.

### Slice B — connect the first city block
- [x] Build a compact outdoor street space using the existing Core/Game runtime and placeholder geometry.
- [x] Connect the hideout entrance to the street and back without recreating the player or session.
- [x] Add two enterable building shells with simple placeholder interiors (corner shop and apartment).
- [x] Keep the first block compact, with short routes and a readable street layout.
- [x] User confirmed the hideout/street/building travel loop is working in Play Mode.
- [ ] Run the new EditMode transition test in Unity (manual travel was confirmed in Play Mode).
- [ ] Replace placeholder street/building geometry with cohesive authored city art.

### Slice C — make the hideout useful
- [x] Expose the physical storage locker as an E interaction.
- [x] Add a home-storage screen that transfers available quantities between carried inventory and a separate locker inventory, respecting destination capacity through a tested Core transfer service.
- [x] Extend save format to version 2 with separate locker contents and the current world location/player coordinates.
- [x] Add version-1 save migration so existing saves load with an empty locker and the player at the hideout.
- [x] Allow save/load while in the city and restore the saved street/interior location.
- [x] Wire the existing ITEM_A/B/C sample definitions into HideoutSlice so the storage interaction can be exercised; these remain temporary test content, not final item design.
- [x] Verify the normal locker transfer and save/load loop in Unity Play Mode (user confirmed 2026-11-17).
- [ ] Verify full/blocked destination edge cases and version-1 save migration in Unity.
- [ ] Connect authored RecipeDefinition assets to the physical workbench. No recipe assets currently exist, so do not invent recipes or production rules.
- [ ] Give bed and other furniture meaningful interactions only after their player-facing rules are agreed.
- [ ] Replace placeholder hideout art and improve interaction feedback.

**Exit check:** the player can take items into the city, return, store/retrieve them, and save/load carried inventory, locker contents and current location without losing the connected-world context.

## Approved game direction — one shared city, overlapping lives

Signal is a balanced urban RPG where legitimate work, trade, crime, relationships, opportunities and unpredictable events coexist in the same city. There is no default split between a legitimate world and a physically separate underground world. The hideout remains the player's central hub and emotional anchor.

Approved design constraints:
- **Parallel lifestyles:** players can freely combine lawful and illegal activities; neither path locks the other.
- **One shared city and economy:** ordinary shops, homes, streets, customers, suppliers and criminal dealings inhabit the same locations and social network. Dedicated criminal hideouts or special spaces may exist later, but are not the default framing or a prerequisite.
- **Hidden dual lives:** an NPC may have a public occupation and a concealed illegal role. For example, an apparently ordinary baker may secretly sell illicit goods. The player does not automatically know every NPC's hidden activities; discovery can come through conversation, observation, requests, rumours and consequences.
- **Goods beyond item names:** products may have meaningful quality, condition, authenticity, origin/provenance and ownership status. Water is one example: different grades can be tested, processed, packaged and sold. Food, electronics, clothing, household goods, stolen items and fictional drugs can support different trade and activity loops.
- **Crime is a core pillar:** fictional drug-related activities and a broad range of other illegal activities are central content, alongside legitimate daily life. Specific activities and rules need individual design.
- **Harsh criminal failure:** crime should carry serious, potentially lasting consequences. Exact penalties, thresholds and recovery rules still need design.
- **Global crime progression:** criminal activities contribute to one overall criminal level/progression track, not separate skill trees for every crime.
- **Legitimate livelihoods are varied and satisfying:** gathering, processing, making, inspecting, repairing, trading, services and small businesses are possible directions. Farming is only one example, not a required path or defining legal loop.
- **Shared people and events:** NPCs, clients, contacts, factions, rumours and world events affect both lawful and illegal activities in the same neighbourhoods.
- **Hideout supports the whole life:** storage, preparation, recovery, workspaces, production spaces and personalisation can support any mix of activities.

## Next initiative — build the connected NPC framework

The first Milo implementation proved that an NPC can have a routine and activity-aware conversation, but it is a temporary prototype character. Do not spend the next initiative writing Milo's personal story or treating his hard-coded component as production content. His data and presentation may be replaced when the narrative cast is authored.

Build the reusable NPC foundation first. NPCs must not be isolated agents: they share one world context and can become aware of other people, nearby activity, environmental events and information circulating through their social network.

The initiative is defined in detail in [NPC_FRAMEWORK.md](NPC_FRAMEWORK.md). Its core requirements are:

1. **Shared world context:** stable NPC identity, current location/position, world time and a common event stream.
2. **Local perception:** NPCs notice people and events only within relevant locations and awareness/visibility ranges.
3. **Individual knowledge:** direct observations, hearsay and confidence/source information are stored per character; no automatic omniscience.
4. **Social connections:** directed relationships distinguish familiarity, trust and affinity. Characters can explicitly exchange information, and what they believe can differ.
5. **Context-aware decisions:** needs, routines and personal goals are evaluated alongside relevant people, known information and world events.
6. **Data-driven authoring:** generic runtime systems should accept character definitions; creating the eventual cast should primarily be content authoring, not writing bespoke scripts.
7. **Persistence and scale:** preserve meaningful social knowledge and relationships when needed, while rebuilding transient perception and keeping distant NPC simulation inexpensive.

### Delivery order

- [x] Add the Core shared-world registry, local event perception, individual knowledge, directed relationships, explicit knowledge exchange and character-scoped perception snapshots.
- [x] Integrate one shared world context with the active city runtime; register Milo as a temporary fixture and provide event publishing for the current or another city location.
- [x] Author Core and Game EditMode coverage for local presence, event visibility, knowledge exchange, trust confidence, character-scoped perception and off-screen events (execution still pending).
- [x] Run the original Core/Game EditMode suites in Unity and resolve the reported failures (user confirmed 214/214 pass on 2026-11-17); rerun after the generic-presenter changes is pending.
- [x] Feed each NPC's own perception snapshot into utility decisions; nearby relationship impressions and personally known information influence socialising without exposing the global event journal.
- [x] Move routine windows, need drift and utility weights into a reusable Core NpcUtilityProfile; verify alternate work hours through a Core test.
- [x] Replace Milo-specific runtime assumptions with ScriptableObject character definitions and a generic presenter. GameSession exposes an assignable NPC definitions array; the presenter owns generic movement, visuals, social registration and conversation publishing.
- [x] Add a second temporary test profile (Rhea) with a different work schedule, appearance and dialogue; both profiles use the same generic component. Extended Game integration tests; rerun in Unity after the latest changes is pending.
- [x] Add restrained schedule-driven social encounters: mutual proximity, one encounter per NPC per clock pass, pair cooldown, slow familiarity growth, at most one shared fact and an observable generic event. Unity verification pending.
- [ ] Add player-driven relationship changes and controlled scenario advancement for major social consequences; ambient routines must not independently create dramatic trust/affinity changes, deaths, disappearances or wealth shifts.
- [ ] Design persistence for stable NPC IDs, relationships, selected memories and discovered information, with save migration tests.
- [ ] Verify performance boundaries and event-driven updates before scaling the population.
- [ ] Run the full Core/Game EditMode suite after the scheduled-social-encounter implementation; the previous 214/214 result predates these changes.

**Exit check:** two or more temporary NPC profiles can inhabit the same shared city, perceive local events, encounter one another, exchange information according to their relationships, and make decisions from their own incomplete knowledge. The same event can produce different beliefs in different characters. Adding a new character does not require a new character-specific runtime class.

This is a framework milestone, not a narrative milestone. Do not create the final cast in bulk, write a Milo quest, invent a relationship progression system, or add a new crime/economy activity as part of this initiative.

## Later experience layers (not active yet)
- **Crime catalogue and global progression:** varied fictional illegal activities, one global criminal progression track, escalating opportunities and serious consequences.
- **Goods and trade simulation:** quality/condition/authenticity/provenance, inspection, appraisal, packaging, customer needs and price trade-offs where they create interesting play.
- **Legitimate livelihood catalogue:** varied production, services, repair, gathering, trade and small businesses; farming is optional among many possibilities.
- **NPCs with public and private lives:** occupations, routines, needs, relationships, hidden roles, customers, suppliers and contacts.
- **World events:** incidents and changing conditions that affect opportunities, locations, NPCs and player plans.
- **Shared economy:** one interconnected flow of goods, money, supply, demand and risk across lawful and illegal activity.

These are broad approved directions, not detailed mechanics. Each should be introduced through a complete playable scenario, not as an isolated subsystem. New detailed rules for economy, production, police, risk, progression and specific crimes require explanation and user approval before implementation.

## Engineering safeguards
- Preserve Unity asset GUIDs and existing scene references.
- Keep Main and its prototype scripts as legacy reference until valuable content is migrated and verified; do not add new features there.
- Keep Core logic independent from UnityEngine where possible.
- Protect save compatibility; Core/Game uses a separate filename from the legacy prototype and now migrates version-1 saves to version 2. Verify file replacement on the target Unity platform.
- Do not claim a scene, build or test is verified until it has actually been run in Unity.

## Definition of a complete slice
A slice is complete only when the player-facing scenario works in Unity Play Mode, relevant tests pass, the Console has no new errors, save/load behavior is checked where relevant, and `docs/PROGRESS.md` records what was and was not verified.
