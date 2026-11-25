# P-03 — Equipment, Wearables & Portable Backpack

**Status:** Core equipment/backpack foundation committed; Unity verification and Game integration pending.
**Track:** Player Development
**Dependency:** P-01 modular player visual and P-02 Armor/Heat Protection modifier contract.

## Player outcome

The player equips physical clothing, protection and accessories. Equipment is authoritative item state, not a second copy of inventory data. A backpack is a physical item whose contents remain attached to that backpack instance when it is equipped, carried, dropped or recovered.

## Confirmed slot layout

Seven slots total:

| Slot | Intended category |
|---|---|
| Head | Hats, helmets and headwear |
| Torso | Shirts, jackets, body armor |
| Legs | Trousers and legwear |
| Feet | Shoes and boots |
| Accessory 1 | Generic accessory; backpack is compatible |
| Accessory 2 | Generic accessory |
| Accessory 3 | Generic accessory |

Accessory slots are generic. Do not reserve all three for hardcoded item categories. A backpack is one possible accessory and consumes one accessory slot.

## Binding rules

1. Each equipped item is one exact physical item instance; stacked quantities cannot be equipped as a single item.
2. Equip/unequip moves the same stable ItemInstanceId, ItemInstanceState and ItemType; it does not clone, recreate or change ownership.
3. Equipment custody is separate from ownership. Equipping a resident-owned item does not silently transfer ownership.
4. A slot holds at most one item. The same physical item cannot occupy multiple slots.
5. Failed equip/unequip operations leave both source and destination unchanged.
6. Effective Armor and Heat Protection are base attributes plus the sum of equipped item modifiers, clamped safely. No combat or weather effects are introduced here.
7. Backpack contents belong to the backpack's stable physical instance ID, not to the player inventory or the equipment slot. Dropping the backpack must not spill, duplicate or destroy its contents.
8. When a backpack is dropped, its contents remain inside the same portable container. Picking it up recovers that same container and its contents.
9. Backpack capacity is authored per item. The initial Core registry represents each backpack's contents as its own grid. UI presentation and how that grid appears alongside player inventory are a later Game integration.
10. If the backpack is lost, stolen or abandoned, its contents remain with it. Ownership and world custody continue to use existing asset/container authorities.
11. Do not introduce weight, movement penalties, pursuit penalties, or automatic backpack dropping without a separate approved rule. The player chooses when to abandon it.
12. No new NPCs, backstories or story-specific content.

## Architecture

### Core (implemented first slice)
- EquipmentSlot: Head, Torso, Legs, Feet, Accessory1, Accessory2, Accessory3.
- EquippedItem: exact PlacedItem plus authored Armor/Heat Protection modifiers.
- PlayerEquipmentLoadout: validated equip/unequip, slot uniqueness and effective-attribute aggregation.
- PortableStorageBook: maps stable backpack item instance IDs to independent GridInventory contents; refuses duplicate identities and refuses to discard a non-empty backpack container.

### Remaining integration
- Game EquipmentItemDefinition or equivalent authored metadata: compatible slot(s), Armor modifier, Heat Protection modifier, backpack dimensions, and visual part/sprite references. Keep definitions in Game; Core must not depend on Unity.
- GameSession composition, item catalogue registration and F5/F9 persistence for all seven slots and nested backpack contents.
- UI: equipment panel, drag/drop between inventory and slots, backpack grid, item inspection and clear failure feedback.
- Physical world representation: dropped backpack is a world container with the same stable backpack ID and contents. Reuse existing container registration, custody and save mechanisms; do not create a parallel loot-container authority.
- PlayerCharacterVisual integration: wearable layers/overlays must follow the existing body-part renderer and animation direction/frame contract. The supplied Body_A pack is only a base body; do not invent a full wardrobe from missing sprites.
- Authored starter wearable content and visual assets are a separate content pass.

## First implementation slice committed

- Added PlayerEquipmentLoadout.cs in Core with the seven slots, exact-instance equip/unequip transfers, slot uniqueness, and Armor/Heat Protection aggregation.
- Added PortableStorageBook keyed by backpack stable item ID. Contents remain in their own GridInventory; non-empty storage cannot be discarded.
- Added nine focused EditMode tests covering slot count, exact identity/state transfer, invalid/stacked items, duplicate slot identity, capacity failure/rollback, modifier aggregation/clamping, backpack identity/content continuity and safe empty-container removal.
- Added Unity .meta files for both new source files.

## Not yet implemented / verified

- No Game item-definition metadata, equipment UI, visual wearable overlays, GameSession composition, SaveData persistence or dropped-world backpack object yet.
- No Unity compilation or EditMode test run has been performed by the repository-edit pass. User's last verified full suite was 467/467 before P-03 tests; expected count is 476 if no other tests were added. Treat this as an expectation, not a passing result.
- P-03 is not complete until equipped items and nested backpack contents round-trip through F5/F9, the dropped backpack can be picked up with its exact contents, visual equipment is shown, and the full EditMode/Play Mode acceptance passes.
