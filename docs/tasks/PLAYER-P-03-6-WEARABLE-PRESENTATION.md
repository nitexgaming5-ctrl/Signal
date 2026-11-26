# P-03.6 — Wearable Presentation

**Status (2026-11-26):** Preparation complete; implementation is gated on a verified inventory of the user's locally imported player sprites and compatible wearable art. Do not guess asset paths, manufacture wardrobe sprites, or substitute item icons for character overlays.
**Track:** Player Development
**Dependency:** P-01 modular Body_A renderer; P-03 authoritative seven-slot equipment loadout; actual compatible wearable sprite assets.
**Parent:** [P-03 Equipment, Wearables & Portable Backpack](PLAYER-P-03-EQUIPMENT-BACKPACK.md)

## Player outcome

Equipped clothing and protection should be visible on the existing modular player character. Visuals must follow the same idle/walk pose, facing, mirroring, frame timing and layer ordering as the base body. Equipment state and modifiers remain authoritative even if a particular item has no compatible visual asset.

## Current verified source facts

- `PlayerCharacterVisual` composes five renderers: `Legs`, `Arm_L`, `Chest`, `Arm_R`, and `Head`.
- It currently loads Idle/Walk sheets for Down/Side/Top from `Resources/Character/Body_A/<Part>/<Action>/<Action>_<Direction>`.
- Each current base-body frame is expected to be 96×128 pixels; the renderer creates frames at 96 PPU and scales the visual rig by 1.8 without changing player physics.
- The current body layers share one frame clock and use facing-specific sorting. Side-facing uses horizontal mirroring.
- The backpack is currently a separate placeholder renderer behind the body. It is toggled through `SetBackpackEquipped(bool)`.
- `ItemDefinition` currently exposes a generic inventory `Icon` and equipment metadata (slot, Armor modifier, Heat Protection modifier, backpack flag and backpack dimensions). An inventory icon is not a wearable sprite and must never be rendered as a body overlay by assumption.
- The P-01 importer places base-body sheets under `Assets/_Project/Resources/Character/Body_A`; the imported user assets are local and are not committed in the repository.

## Preflight — required before visual implementation

1. Ask the user for the actual imported wearable asset paths or a Unity-exported listing/screenshot of the relevant Resources folders. Do not ask for the original base-body ZIP again unless the user says it changed.
2. Inspect actual files for each candidate wearable: slot/category, dimensions, frame layout, directions, animation set, pivot/alignment, PPU and transparency.
3. Map each candidate against the Body_A renderer's supported Idle/Walk and Down/Side/Top pose contract. Confirm whether it is a complete replacement body part, an overlay, or incompatible.
4. Record supported and unsupported slots/variants in this task card before adding runtime references.
5. If no compatible authored wearable sprites exist, keep this phase at the preparation checkpoint. Do not fabricate artwork or use a static inventory icon as a visual substitute.

## Approved presentation rules

1. Equipment remains authoritative in `PlayerEquipmentLoadout`; the visual rig reads the current equipped item and never owns item identity, inventory custody, ownership, or stat modifiers.
2. Preserve the existing Player GameObject, Rigidbody2D, collider, movement controller, camera relationship and 1.8 visual scale.
3. Visual overlays must use the same animation state, direction, frame clock and side-facing mirror as the corresponding base-body pose.
4. Draw ordering must follow the existing direction-aware body ordering. Do not use one fixed order for every facing if the source art requires different ordering.
5. Missing or unsupported wearable art is a safe no-op. The base character and equipment UI continue working.
6. The backpack remains its own accessory/back layer and must not be merged into torso clothing.
7. Head, Torso, Legs and Feet are the intended body-wearable slots. The three Accessory slots remain generic; do not invent a universal accessory renderer.
8. Do not add new gameplay effects, equipment slots, wardrobe choices, appearance customization, or save fields in this phase unless inspection proves a necessary, approved change.
9. Do not add third-party packages or generated placeholder wardrobe content.

## Implementation approach — only after preflight

- Prefer a small authored wearable visual definition that references compatible per-pose sprites or sheets. Keep Unity Sprite references in Game-layer content, not Core or serialized domain state.
- Resolve the visual from the exact equipped item's definition/visual key; do not infer from display names, item icons or slot alone.
- Extend `PlayerCharacterVisual` with optional wearable renderers/layers that reuse its current pose key, frame selection, flip and sorting recipe.
- If an asset is a full replacement for a body part rather than an overlay, explicitly define replacement semantics so the base part is hidden only while the compatible wearable is equipped.
- Rebuild or refresh the visual immediately after equip, unequip, backpack restore and F9 equipment restoration. The visual must not lag one frame or retain stale sprites.
- Keep missing definitions, missing sprites and unsupported directions safe and visible in logs without disabling the base character.

## Focused verification

- Supported wearable appears on the correct body region while idle and walking.
- All supplied directions render correctly; side-facing mirroring remains aligned.
- Wearable and base body do not visibly drift between frames.
- Direction changes preserve correct front/back ordering.
- Equip and unequip update visuals immediately, including equipping from a pocket.
- F5/F9 restores the same wearable presentation from the authoritative equipped item state.
- Missing optional art leaves the base body intact and does not throw or break movement.
- Backpack visual remains independent and correctly layered when equipped, dropped and recovered.
- Existing equipment identity, ownership, Armor and Heat Protection values are unchanged.
- Full EditMode suite and active-scene Play Mode walkthrough are run in Unity; only user-reported results count as verified.

## Explicit exclusions

No new clothing catalogue, item balance, armor/combat mechanics, cold exposure, weather, player appearance editor, NPC wardrobe, or new character content. W-01 remains separate. P-03.7 owns debug-loadout cleanup and final lifecycle/service acceptance.
