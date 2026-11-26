# P-03.6 — Wearable Presentation

**Status: PAUSED — user-directed art pause (2026-11-26)**  
**Track:** Player Development  
**Parent:** [P-03 Equipment, Wearables & Portable Backpack](PLAYER-P-03-EQUIPMENT-BACKPACK.md)

## User direction
The user explicitly ended the current sprite/art effort and asked to return to the Signal roadmap and functional development. Do not resume this task unless the user explicitly re-authorises it.

The pause includes:
- creating or editing player sprite sheets/templates;
- authoring clothing or armor overlays;
- implementing wearable rendering layers;
- building a wardrobe/wardrobe-preview panel;
- further character-art exploration or sprite import workflows.

Existing player rendering, imported assets and already-committed temporary visual tests are preserved. This card remains so the work is not lost or mistaken for an immediate next step.

## Original intent (parked)
Eventually show equipped clothing/armor on the modular player character, with equipment state remaining authoritative in the existing P-03 loadout. The original preparation noted five Body_A render layers (Legs, Arm_L, Chest, Arm_R, Head), Idle/Walk, directional poses, shared frame timing, side mirroring and a separate backpack layer. These are historical notes, not an instruction to change the renderer.

## Reactivation requirements
Only if the user reopens this phase:
1. Confirm desired scope and inspect actual local asset paths/formats.
2. Verify frame dimensions, directions, pivots, animation layout and compatibility.
3. Preserve equipment/item authority; rendering is a view of equipped state, not a second equipment system.
4. Propose a bounded implementation and acceptance plan before coding.

Until reactivated, focus on non-art P-03 closeout and the approved roadmap.
