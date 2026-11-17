# Roadmap — build the place before the systems that fill it

Milestones are dependency notes, not a mandate to build isolated systems in order. Work is organised around one active, player-facing initiative at a time. Combine technical tasks whenever that produces a coherent playable result. See `docs/DEVELOPMENT_WORKFLOW.md` and `docs/ARCHITECTURE.md`.

## Current initiative — the first hideout and city block
**Player outcome:** the player can walk around a small starter house, interact with its furniture, use a tactile workbench workspace, leave the house, explore a compact city block, and enter/exit basic buildings.

### Slice A — make the hideout feel like a place
- [x] Consolidate the active runtime on Signal.Core + Signal.Game; preserve the old Main/prototype path as legacy reference.
- [x] Add HideoutSlice as the active build scene.
- [x] Establish a walkable room with floor, walls and player collision.
- [x] Add placeholder bed, table, chair, storage and a physical workbench.
- [x] Add E prompt and a close-up workspace with draggable/inspectable tabletop objects.
- [ ] Verify compilation, controls, UI layout and EditMode tests in Unity.
- [ ] Replace placeholder geometry with authored, cohesive hideout art and atmosphere.
- [ ] Add meaningful interactions to bed, storage and other furniture after agreeing their player-facing rules.

**Exit check:** in Unity Play Mode, the player can move around without clipping through furniture/walls, open the workbench from the physical bench, inspect/rearrange tabletop objects, close the workspace and resume movement. The room should feel like a modest lived-in home rather than a debug room.

### Slice B — the surrounding city block
- Build a compact outdoor street space using the existing city art where appropriate.
- Connect the hideout entrance to the street and back without losing player state.
- Add a small number of enterable building shells/interiors. The first pass can use simple placeholder rooms.
- Keep navigation readable and distances short; the block is a playable context, not a large empty map.

**Exit check:** the player can move between the hideout and street, enter and exit the initial buildings, and reliably return to the correct location.

### Slice C — make the hideout useful
- Connect existing inventory and crafting capabilities to the physical storage/workbench interactions where appropriate.
- Make important objects provide clear feedback and a satisfying interaction response.
- Add decoration/furniture placement only after the basic room and interaction loop is stable; decide its exact rules with the user before implementation.
- Ensure save/load preserves location and relevant hideout state before introducing persistent furnishing or world changes.

**Exit check:** the player can use the hideout as a practical home base, and changes that are intended to persist survive a save/load cycle.

## Later experience layers (not active yet)
These are broad directions, not approved detailed mechanics:
- Small city activities and interactions that provide money or resources.
- Outings with meaningful time, inventory, risk and possible loss.
- Looting, carrying and bringing things back to the hideout.
- Characters, relationships, factions and narrative events.
- A broader economy and hands-on production systems.

Each layer should be introduced through a complete playable scenario, not as an isolated subsystem.

## Engineering safeguards
- Preserve Unity asset GUIDs and existing scene references.
- Keep Main and its prototype scripts as legacy reference until valuable content is migrated and verified; do not add new features there.
- Keep Core logic independent from UnityEngine where possible.
- Protect save compatibility; fix and test the existing save replacement risk before relying on persistence for more world state.
- Do not claim a scene, build or test is verified until it has actually been run in Unity.

## Definition of a complete slice
A slice is complete only when the player-facing scenario works in Unity Play Mode, relevant tests pass, the Console has no new errors, save/load behavior is checked where relevant, and `docs/PROGRESS.md` records what was and was not verified.
