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
- [x] User verified compilation and reported the first-slice tests passed in Unity.
- [ ] Replace placeholder geometry with authored, cohesive hideout art and atmosphere.
- [ ] Add meaningful interactions to bed, storage and other furniture after agreeing their player-facing rules.

**Exit check:** the player can move around without clipping through furniture/walls, open the workbench from the physical bench, inspect/rearrange tabletop objects, close the workspace and resume movement. The first playable hideout/workbench loop has been verified by the user; the visual/art pass remains open.

### Slice B — the surrounding city block
- [x] Build a compact outdoor street space using the existing Core/Game runtime and placeholder geometry.
- [x] Connect the hideout entrance to the street and back without recreating the player or session.
- [x] Add two enterable building shells with simple placeholder interiors (corner shop and apartment).
- [x] Keep the first block compact, with short routes and a readable street layout.
- [ ] Verify doorway reachability, collision, camera tracking and return paths in Unity Play Mode.
- [ ] Replace placeholder street/building geometry with cohesive authored city art.

**Exit check:** the player can move between the hideout and street, enter and exit the initial buildings, and reliably return to the correct location. Source implementation is in place; Unity verification is pending.

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
- Protect save compatibility; Core/Game now uses safe replacement and a separate filename from the legacy prototype. Verify on the target Unity platform before relying on persistence for more world state.
- Saving, loading and sleeping are currently restricted to the hideout because location persistence is not part of this slice yet.
- Do not claim a scene, build or test is verified until it has actually been run in Unity.

## Definition of a complete slice
A slice is complete only when the player-facing scenario works in Unity Play Mode, relevant tests pass, the Console has no new errors, save/load behavior is checked where relevant, and `docs/PROGRESS.md` records what was and was not verified.
