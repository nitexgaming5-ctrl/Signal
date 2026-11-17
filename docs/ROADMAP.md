# Roadmap — build the place before the systems that fill it

Milestones are dependency notes, not a mandate to build isolated systems in order. Work is organised around one active, player-facing initiative at a time. Combine technical tasks whenever that produces a coherent playable result. See `docs/DEVELOPMENT_WORKFLOW.md` and `docs/ARCHITECTURE.md`.

## Current initiative — make the hideout the centre of the city loop

**Player outcome:** the player can leave home, explore the connected block, bring items back, store and retrieve them at the physical locker, and save/load the same carried items, home storage and location.

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

**Exit check:** the player can move between the hideout and street, enter and exit the initial buildings, and reliably return to the correct location. The user has confirmed the route works; automated test execution remains pending.

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

## Approved game direction — parallel lives in a living, dangerous city

Signal is a balanced urban-life RPG where the city supports legitimate work, underground crime, relationships, opportunities and unpredictable events. The hideout remains the player's central hub and emotional anchor.

These are approved design constraints:
- **Parallel lifestyles:** legal and criminal activities are both available. Players may combine them freely; neither path locks the other.
- **Harsh criminal failure:** crime should carry serious, potentially lasting consequences. Depending on the eventual activity and severity, failure may cause major losses or restrictions. Exact penalties and recovery rules still need design.
- **Global crime progression:** criminal activities contribute to one overall criminal level/progression track, not separate skill trees for every crime. Individual activity mastery may be considered later, but is not approved as a required system.
- **Legitimate progression is a real alternative:** farming, gathering, processing, crafting, services and selling goods (for example, growing wheat and making bread) should form satisfying lower-risk economic loops, not a consolation path.
- **Crime is a core pillar:** fictional drug-related activity and other illegal activity are central game content, alongside theft, fraud, cybercrime, vandalism and other suitable fictional crime categories. Specific activities and rules need individual design.
- **The city is shared by both lifestyles:** NPCs, clients, contacts, factions, locations and world events should create opportunities and consequences across legal and illegal play.
- **The hideout supports both:** storage, preparation, recovery, workspaces, production spaces and personalisation should grow into a meaningful home base.

## Next initiative — design and build the first connected activity slice

Do not implement a standalone searchable container as the whole activity system. First design a small reusable activity framework that can support character requests, world interactions and event-driven opportunities. Then deliver one complete playable scenario that connects:
1. A character, contact or discoverable opportunity.
2. A clear activity objective and world interaction.
3. A meaningful item/resource/reward outcome.
4. A return-to-hideout or follow-up step.
5. Persistence so completion and important state do not reset on reload.

The first scenario can be modest, but should demonstrate the foundations rather than become a one-off hardcoded feature. The exact scenario, item, reward, repeatability and failure rules still need agreement before implementation.

## Later experience layers (not active yet)

- **Crime catalogue and progression:** a broad range of fictional illegal activities, with one global criminal progression level, escalating opportunities and serious consequences.
- **Legal livelihood catalogue:** farming, gathering, processing, crafting, services, trade and small businesses, including delayed/passive production that still requires meaningful setup and collection.
- **People and relationships:** NPC identities, routines, needs, client expectations, contacts, reputation and factions.
- **World events:** incidents and changing conditions that affect opportunities, locations, NPCs and player plans.
- **Economy and production:** coherent resource chains, item definitions, pricing and workbench/production interactions.

These are broad approved directions, not detailed mechanics. Each should be introduced through a complete playable scenario, not as an isolated subsystem. New detailed rules for economy, production, police, risk, progression and specific crimes require explanation and user approval before implementation.

## Engineering safeguards
- Preserve Unity asset GUIDs and existing scene references.
- Keep Main and its prototype scripts as legacy reference until valuable content is migrated and verified; do not add new features there.
- Keep Core logic independent from UnityEngine where possible.
- Protect save compatibility; Core/Game uses a separate filename from the legacy prototype and now migrates version-1 saves to version 2. Verify file replacement on the target Unity platform.
- Do not claim a scene, build or test is verified until it has actually been run in Unity.

## Definition of a complete slice
A slice is complete only when the player-facing scenario works in Unity Play Mode, relevant tests pass, the Console has no new errors, save/load behavior is checked where relevant, and `docs/PROGRESS.md` records what was and was not verified.
