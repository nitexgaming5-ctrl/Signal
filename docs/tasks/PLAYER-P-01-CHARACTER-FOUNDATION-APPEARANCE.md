# P-01 — Character Foundation & Appearance

**Status:** Design prepared; implementation not authorized.
**Track:** Player Development
**Dependencies:** Existing player composition in GameSession/RoomBootstrap; supplied modular character resource pack.
**Scope boundary:** This task establishes the player identity/visual foundation. It does not implement skill progression, a complete wardrobe, armor gameplay, or weather exposure.

## Player outcome

The existing player is represented by a recognizable, animated, customizable character rather than the current single-color placeholder. Appearance is stable across movement/actions and survives the current-build save/load round trip. The visual architecture is ready to display later clothing and armor without replacing the renderer.

## Current implementation facts

- `RoomBootstrap.CreatePlayer()` dynamically creates the Player GameObject, a single `SpriteRenderer`, `Rigidbody2D`, `BoxCollider2D` and `PlayerMotor2D`.
- `PlayerMotor2D` currently reads input and drives Rigidbody2D velocity; it does not expose a formal animation/facing contract.
- `GameSession` is the composition root and creates/configures the room.
- `SaveData` is a plain serializable DTO; build 0.01 has no schema version or migration chain.
- The supplied World Build character pack is modular (head, chest, legs, left/right arms and tool overlays) and includes multiple action/movement animation resources. It is a base-character resource, not a complete wardrobe: clothing and armor coverage must be sourced/created separately and checked for frame/direction compatibility before use.
- The pack documentation warns that draw order may vary by animation/action. A single fixed global layer order is therefore insufficient.

## Binding design

1. Preserve the existing Player GameObject, physics, collider, movement controller and camera-follow relationship. Replace only the visual implementation.
2. Build a Game-layer modular visual rig that composes body-part sprites from one shared animation state, direction and frame index.
3. Animation/facing state must be derived from movement/action state through an explicit contract. Do not make Core depend on Unity animation or SpriteRenderer APIs.
4. Each animation/action definition must describe its frame layout and per-part/per-direction sorting order (or equivalent authored layer recipe). Do not assume all sheets share frame counts or pivots.
5. Keep sprite composition deterministic: every body part in one rendered pose uses the same logical frame/time and facing direction. Missing optional overlays must not break the base body.
6. Appearance selections are stable content IDs, not serialized Sprite/Unity object references.
7. Keep visual equipment as a presentation concern. The rig may expose named clothing/armor layer slots, but P-01 does not implement the authoritative equipment loadout or its gameplay effects.
8. Keep tool overlays separate from wearable equipment. Existing action/tool visuals must not become permanent clothing layers.
9. No new character classes, backgrounds, attributes or skill rules are introduced here.
10. No new Unity package or third-party runtime dependency.

## Proposed architecture

### Core data
- `PlayerCharacterProfile`: stable appearance selection IDs and any player-owned visual choices. No UnityEngine types.
- `PlayerAppearanceState`: validated body/appearance option IDs, if separate state improves save mapping.
- Save DTO: scalar/string IDs only. Do not store Sprite, Texture, Animator, GameObject or ScriptableObject references.

### Game content/presentation
- `CharacterAppearanceDefinition`: authored appearance option and compatible sprite-part references.
- `CharacterAnimationSetDefinition`: animation/action clips, directional frame data, pivots/frame metadata and per-action layer ordering.
- `CharacterVisualRig`: composes the modular body and optional overlays.
- `PlayerAnimationDriver`: translates the player's movement/action state into rig animation state.
- A small customization panel/controller edits the profile through validated GameSession/Core APIs; it must not own persistent state.

These are proposed names, not a mandate to create every class if inspection finds an existing reusable equivalent.

## Animation integration

- At minimum validate idle, walking and running in every supported direction present in the pack.
- Validate every included action animation intended for the active game loop (holding/tool use/striking/watering only where the supplied pack provides compatible sheets).
- Respect each sheet's actual frame count, pivot, pixels-per-unit and alignment. Do not hardcode one universal frame count.
- Avoid animation drift between head, torso, arms and legs.
- Use animation-specific sorting/layer recipes so front/back arm ordering and held tools render correctly.
- Preserve pixel-art filtering, compression and sprite import settings required by the source pack.

## Customization scope

P-01 supports only options actually backed by compatible assets. It must not fabricate hair, skin, body, clothing or armor variants that are absent from the supplied resource pack.

The player can edit supported appearance selections on the existing player. A later P-03 equipment phase owns wearable item slots, item identity, inventory custody, clothing/armor modifiers and equipment-driven appearance. P-01 must provide a stable rendering seam for that future system.

## Save/load

- Persist the appearance selection IDs in SaveData through the existing mapper/store path.
- Restore only IDs that resolve to currently registered authored definitions; handle missing/unknown IDs safely with a documented default.
- Round-trip current-build appearance through F5/F9.
- Existing save files are not guaranteed compatible after DTO changes; no migration layer is in scope for build 0.01.

## Acceptance criteria

- The placeholder visual is replaced without changing player physics or interaction footprint unexpectedly.
- Movement, camera follow, collisions, location transitions and existing interactions remain functional.
- All supported directions and required animations display correctly.
- Body-part alignment and per-action draw order are correct; no visible frame desynchronization.
- Appearance changes are visible immediately and persist through F5/F9.
- Missing optional visual layers fail safely.
- No Unity object references are serialized into Core or SaveData.
- Existing gameplay and the full EditMode suite remain green; focused tests cover profile validation and save mapping.
- A Play Mode walkthrough confirms the character remains visually correct while moving through hideout/street/interiors and performing supported actions.

## Suggested focused test matrix

1. Valid appearance IDs are accepted; unknown IDs are rejected or defaulted safely.
2. Profile copy/restore preserves selected IDs.
3. SaveMapper round-trips appearance selections.
4. SaveMapper safely handles a missing/unknown appearance definition ID.
5. Animation-state adapter maps idle/movement/facing states deterministically (Game tests where feasible).
6. Existing movement/composition tests remain unchanged and pass.

## Explicit exclusions

No full clothing/armor catalogue, equipment inventory slots, combat system, cold exposure, weather simulation, character classes, backgrounds, skill point allocation, skill milestone awards, or new NPC cast. Those belong to P-02, P-03 or W-01 as specified separately.
