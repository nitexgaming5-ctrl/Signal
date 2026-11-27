# P-04 — Character Playability: Progression, Active Hand & Quickbar

**Status:** P-04a complete (557/557); P-04b complete (563/563). P-04c is the next proposal and is not yet authorised for implementation.  
**Track:** Player Development  
**Priority:** Ahead of B-07 Neighbourhood Expansion.  
**Dependencies:** P-01 character foundation, P-02 attributes/skills, P-03 equipment/inventory/backpack.

## 1. Purpose

Make the player a coherent RPG character before expanding city geography: meaningful attributes/progression, reliable carried-item preparation, and one general-purpose quickbar that doubles as the active hand.

B-07 remains paused while P-04 is active.

## 2. Verified implementation

### P-04a — Attributes and development

- Exactly three attributes: **Health** (primary; determines maximum HP), **Armor**, **Heat Protection**.
- Current HP remains a separate PlayerStats resource; damage/healing behavior is unchanged.
- Effective attributes combine equipment modifiers and authored skill modifiers through the authoritative calculation path.
- F2 opens the player-facing Character panel; HUD shows current/max HP.
- Base attributes, skills and milestone ledger are persisted; load restores effective maximum Health before saved current HP.
- Temporary test fixtures: `health_test` (+10 maximum Health/rank) and `heat_protection_test` (+1 Heat Protection/rank). These are not final skill identities or balance.
- User verified **557/557** and confirmed the connections/functionality tested good.
- Actual Repair, Production, Trading, Scavenging and Social effects, plus comprehensive milestone trigger wiring, remain future work.

### P-04b — Six-slot assignment and visible HUD

**Complete; user verified 563/563 on 2026-11-27.**

- Exactly six general-purpose quickbar slots. Suitable carried items include consumables, tools, weapons, backpacks and other supported items.
- Quickbar entries are stable item-instance references only. They do not copy/move items or create a second owner. A physical item can occupy at most one slot.
- Eligible sources: accessible backpack grid or any of three personal pockets. Equipped clothing/accessories are excluded.
- Tab inventory supports RMB Inspect / Equip to Quick Slot 1–6, drag assignment, slot-to-slot swap and drag-to-unassign.
- Reassigning an already assigned item moves its reference from its old slot to the chosen slot. An occupied destination loses only its former quickbar reference.
- Drag ghost follows the pointer in screen space.
- Always-visible six-slot HUD bar appears bottom-centre outside Tab inventory; the Tab inventory row remains.
- Dropped-backpack references persist but are unavailable until the same backpack is recovered. Pocket entries remain accessible.
- Save/load persists assignments and reconciles invalid/retired identities.
- User confirmed the follow-up at **563/563**.

## 3. Confirmed active-hand rules

These are already user-approved and should not be reopened as broad design questions:

1. The currently selected quickbar slot is the item held in the player's hand.
2. There are no dedicated weapon/tool slots and no duplicated active-equipment state.
3. **H** holsters the held item without clearing its quickbar assignment.
4. Mouse-wheel up/down selects next/previous quickbar slot, with wraparound, only when no menu is open.
5. Selecting a slot only draws/holds its item. It never consumes or activates it.
6. Supported consumables require an intentional left-click to use.
7. Tools/weapons are held only. Tool actions and weapon attacks are explicitly out of scope for this phase.
8. Number keys 1–6 have not been approved.
9. The quickbar remains a reference to carried physical inventory; it never duplicates ownership.

## 4. Next proposal — P-04c Active Hand & Controls

**Status: proposal only. Do not implement until the user explicitly approves the scope and resolves the open decisions below.**

### Intended player outcome

The six-slot HUD becomes functional as the player's active-hand selector. The player can draw an assigned item, holster it, and cycle through slots without opening inventory. This is an item-selection/presentation loop only, not an item-use/combat system.

### Proposed implementation breakdown

**P-04c.1 — Core active-hand state**
- Extend the existing `PlayerQuickbar` authority with a selected slot and holstered/active state, or introduce a minimal adjacent Core value object if separation is cleaner.
- Do not store a second item ID for the held item. Resolve the held item from the selected slot's existing stable item-instance reference.
- Selecting a populated slot marks it active and resolves its physical item through the existing accessible inventory/pocket authorities.
- Selecting an empty slot must never resolve a stale or inaccessible item. Exact cycling behavior is an open decision below.
- If an assigned item is retired, transferred, sold, consumed or otherwise becomes unavailable, reconcile the active-hand state safely without creating a phantom held item.
- If the backpack is dropped, an item referenced from its contents must not remain accessible/held. Pocket-backed entries remain usable as references.

**P-04c.2 — Input and UI integration**
- Mouse-wheel up/down cycles next/previous with wraparound only when no menu is open.
- H holsters: active-hand presentation is cleared; all six assignments remain.
- Selecting a populated slot again draws it.
- Proposed direct HUD interaction: left-clicking a visible quickbar slot selects it. This is not the same as clicking in the world to activate a supported consumable. Please confirm this interaction.
- No number-key shortcuts in this slice unless separately approved.
- Show a clear active-slot highlight and distinguish holstered from active state without adding another item panel or duplicating inventory state.

**P-04c.3 — Persistence**
- Persist only the minimal selected-slot/holstered state if the user wants the active hand restored after F5/F9.
- Alternative safe policy: assignments persist as today, but every loaded session starts holstered. This avoids restoring an unexpected held item.
- Either policy must resolve against current item identity/accessibility; never restore a phantom item.

**P-04c.4 — Regression and acceptance**
- Core tests: selecting populated/empty slots, H holster, reselect after holster, wheel wraparound in both directions, selected item retirement/reconciliation, duplicate/reference invariants.
- Game/input tests: wheel and H are ignored while any menu is open; wheel direction is correct; HUD active-slot/holster indicator matches Core state.
- Play Mode: draw from backpack and each pocket, holster, reselect, cycle across slot 6→1 and 1→6, test empty slots per approved policy, drop backpack while holding a backpack-backed item, and verify pocket-backed references remain available.
- F5/F9 acceptance follows the approved persistence policy.
- Full EditMode suite and Play Mode acceptance required.

### Open decisions — please confirm before implementation

1. **HUD click:** Should left-clicking one of the six visible HUD slots select/draw that item? (Recommended: yes. It provides direct selection without adding unapproved number-key controls.)
2. **Empty slots during wheel cycling:** Should the wheel visit all six positions, including empty slots, or skip empty slots and select the next populated slot? (Recommended: visit all six positions; it makes wheel movement deterministic and keeps slot positions meaningful. An empty selected slot means empty hand.)
3. **F5/F9 active-hand persistence:** Should the selected/holstered state be saved and restored, or should every loaded session start holstered while keeping all six assignments? (Recommended: load holstered for safety; quickbar assignments remain persistent.)

### Explicit exclusions

- No number-key mapping 1–6.
- No consumable activation/use implementation in this slice; left-click use is a confirmed later interaction contract, not permission to build a use system now.
- No tool actions, weapon attacks, combat, targeting, durability, reload, animation/art work or new item categories.
- No new sprites, wearable overlays or wardrobe presentation while the art pause is active.
- No city expansion, NPC cast or story content.
- No changes to inventory ownership, backpack/pocket rules, stack behavior or economy.

## 5. Later work

- **P-04d:** remaining interface polish after active-hand selection is verified. The always-visible HUD bar and drag ghost are already delivered; do not duplicate them.
- Actual livelihood skill effects and milestone event wiring require their own connected proposals.
- B-07 Neighbourhood Expansion remains paused until P-04 is complete or the user reprioritises it.
