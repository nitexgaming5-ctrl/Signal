# P-04 — Character Playability: Progression, Active Equipment & Quickbar

**Status:** P-04a implemented and user-verified at 557/557. P-04b interaction specification approved; implementation not started. P-04c active-hand/HUD controls remain specified, not implemented. Health is primary; temporary skill tests are authored; tool actions and weapon behavior deferred.  
**Track:** Player Development  
**Strategic priority:** Ahead of B-07 Neighbourhood Expansion.  
**Dependencies:** P-01 character foundation, P-02 attributes/skills, P-03 equipment/inventory/backpack.

## 1. Why this phase comes first

A larger city is not valuable if the player character cannot meaningfully progress, prepare for activities or access useful carried items during play. P-04 makes the character a playable RPG entity before expanding geography.

B-07 Neighbourhood Expansion is paused and must not be implemented while P-04 is active.

## 2. Current repository baseline (audit)

### Already implemented
- P-02 Core now stores base Health, Armor and Heat Protection. Health is the maximum-health attribute; current HP remains the separate PlayerStats resource. Effective maximum Health is applied to PlayerStats without changing damage/healing rules. Save mapping persists base Health and current HP separately.
- Current catalog is explicitly provisional: 8 starting points, rank cap 5, one point per rank; candidate skills Repair, Production, Trading, Scavenging and Social.
- F2 now opens the player-facing Character panel: it displays current/maximum Health, Armor, Heat Protection, skill ranks, descriptions, points and free respecialisation. The former F2-only debug presentation has been replaced.
- P-03 equipment loadout supports Head, Torso, Legs, Feet and three generic accessory slots. Backpack occupies an accessory slot.
- Exact-instance equipment transfer, item state and ownership preservation, backpack persistence, pockets and stack-aware inventory are implemented.
- Inventory panel supports item movement, splitting, merging and current equipment/backpack shortcuts.

### Missing or incomplete
- The two temporary attribute-test skills now affect effective maximum Health and Heat Protection through the authoritative calculation path. Other skill ranks remain stored but do not yet affect their corresponding livelihood activities.
- Milestone award API exists, but successful gameplay completion events are not comprehensively wired to authored milestone rewards.
- F2 is a prototype/debug allocation panel, not a complete character-development experience.
- Current equipment slots do not include a dedicated weapon or tool slot/category.
- No authoritative quickbar/hotbar assignment, selection, use or persistence system currently exists.
- Existing equipment metadata is provisional; no weapon combat/attack system should be assumed to exist.

## 3. Player outcome

The player can:
1. Open a clear character-development interface and understand attributes, skills, ranks, points and what each skill actually improves.
2. Earn skill points through important completed milestones, allocate them freely and respecialise without penalty.
3. Assign any suitable carried item—healing items, energy drinks, tools, weapons, backpacks and other supported items—to one of six general-purpose quickbar slots. The currently selected slot is the item equipped in the player's hand.
4. Holster the currently held item with **H** without deleting its quickbar assignment. Scroll-wheel up/down selects the next/previous quickbar item when no menu is open.
5. Save/load progression, active-hand selection, holstered state and quickbar assignments without duplicating items or exposing inaccessible dropped-backpack contents.

## 4. Proposed implementation sequence

### P-04a — Character development becomes gameplay-relevant
- Replace the F2-only debug presentation with a proper player-facing Character panel while retaining a development/debug route if useful.
- Show Health (primary), Armor and Heat Protection separately from skills.
- Show each skill's current rank, point allocation controls, unspent points and an understandable description of its current effect. Next-rank cost is the catalog's fixed points-per-rank value.
- Preserve free respecialisation and the total-point invariant.
- For initial skill-effect testing only, the authored catalog now includes `health_test` (Vitality test: +10 maximum Health per rank) and `heat_protection_test` (Cold Readiness test: +1 Heat Protection per rank). Both are explicitly temporary, replaceable pipeline fixtures, not final skill identities or balance.
- Integrate skill modifiers through the authoritative character-attribute calculation path rather than adding duplicate calculations to UI. Health is primary; Armor and Heat Protection remain distinct attributes.
- Award milestone points only after successful, committed gameplay events; use stable event IDs and existing idempotent progression authority.
- Persist all progression and milestone state.

**Candidate existing-activity integrations (not yet approved as formulas):**
- Repair → RepairService outcome or material efficiency.
- Production → ProductionService output/input efficiency.
- Trading → RetailService purchase/sale terms.
- Scavenging → searchable-container discovery/loot handling.
- Social → existing relationship/customer interactions.

No numerical bonus, chance, price formula, quality roll or extra reward is implied by this list. Each effect needs an explicit design decision before implementation.

### P-04b — Quickbar assignment foundation (context menu + inventory drag/drop)

**Approved interaction design (2026-11-27; specification only, no implementation yet)**

- The Tab inventory displays a dedicated six-slot quickbar row, visually separate from the inventory grid but within the same inventory mode.
- Right-clicking an inventory item opens a contextual menu instead of immediately opening Inspect. The menu contains **Inspect** and **Equip to Quick Slot**; the latter opens a numbered submenu for slots 1–6.
- Selecting **Inspect** opens the existing inspection experience. Selecting a numbered quick slot assigns the item's stable physical identity to that slot.
- Reuse the existing inventory drag/drop framework. Do not create a parallel drag system.
- Drag an inventory item onto a quickbar slot to assign it. Dropping onto an occupied slot replaces that slot's assignment; the previously assigned item remains in its normal inventory location.
- Drag one quickbar slot onto another to swap their assignments. Drag an assigned quickbar entry back into the inventory area to unassign it; the physical item remains in inventory.
- Context-menu assignment and drag/drop assignment must call the same authoritative Core assignment operation and produce identical results.
- Quickbar entries are references to stable item-instance identities, never item copies and never a second inventory owner.
- Moving an assigned item out of the accessible carried inventory (including selling, transferring, equipping, or moving it into an inaccessible dropped backpack) must not leave a usable stale assignment. Identity-changing stack operations must reconcile references against surviving item IDs; retired/consumed identities must be cleared safely.
- The quickbar assignment UI does not select/use the item, consume it, or perform a tool/weapon action. Active-hand selection and controls remain P-04c.
- Context-menu dismissal should follow existing UI conventions (outside click and Escape) after the code audit; do not introduce a new global input layer.

**P-04b implementation breakdown**
1. Audit `InventoryPanel`, current RMB inspect handling, drag-origin/drag-target abstractions, inventory layout sizing and GameSession/Core transfer callbacks. Document reusable entry points before editing.
2. Add a Core-owned six-slot assignment model/service using stable item-instance IDs, validation and explicit assign/replace/swap/unassign operations. Keep Core Unity-free.
3. Integrate assignment validity with existing inventory/equipment/backpack accessibility and stack-lineage authorities. No stale reference may grant access to dropped-backpack contents.
4. Add the six-slot row to Tab inventory and extend the existing drag/drop path for inventory→quickbar, quickbar→quickbar swap and quickbar→inventory unassign.
5. Add the RMB contextual menu with Inspect and Equip to Quick Slot→1–6; route both menu and drag/drop through the same assignment API.
6. Persist the six assignments through the existing SaveData/GameSession path and restore only identities that remain valid and accessible.
7. Add Core regression tests for assign, replace, swap, unassign, invalid identity, moved/sold/equipped item invalidation, stack split/merge/retirement and save/load. Add UI/Play Mode acceptance for both assignment paths and menu dismissal.

**P-04b decisions still requiring confirmation before implementation:** whether the same physical item may be assigned to more than one quickbar slot, and whether a quickbar slot should be allowed to reference an item currently equipped in a clothing/accessory slot. The safe default is to avoid duplicate assignments and reference only accessible carried items unless the user chooses otherwise. Do not silently decide these edge cases in code.

### P-04c — Active hand item (quickbar-driven; no separate weapon/tool slots)
- The six-slot quickbar is the authoritative active-item selector. The currently selected slot's assigned item is the item equipped/held in the player's hand.
- Do not add a separate Weapon slot, Tool slot or duplicate Active Hand equipment slot. Do not repurpose the existing clothing/accessory slots.
- Selecting another occupied quickbar slot draws that slot's item into the active hand. Selecting an empty/unavailable slot safely leaves the hand empty. Selection never activates or consumes an item.
- **H** holsters the currently held item: active hand becomes empty, while all six assignments remain intact. Selecting another populated slot draws that item again.
- Preserve exact physical item identity, item state, ownership, save/load and dropped-backpack isolation. The active hand is a reference/state over the real item, not a second copy or a second owner.
- A weapon being held does not imply combat. Attack, damage, targeting, durability, ammunition and crime consequences remain out of scope unless separately designed and approved.
- Selecting an item never activates or consumes it. Healing items, energy drinks and other supported consumables require an intentional **left-click** to use, preventing accidental use while selecting/cycling slots. The exact effect/eligibility rules remain tied to each consumable's action contract.
- Selecting a tool or weapon simply holds it in the active hand; selection does not perform an action. Actual tool actions and weapon/combat behavior are later, separately designed interactions.

### P-04d — Six-slot quickbar HUD and controls
- Add a small always-visible six-slot quickbar to the HUD.
- Every slot can be assigned any supported carried item type: healing/energy consumables, tools, weapons, backpacks and other suitable items. The bar is not restricted to consumables or a dedicated weapon/tool category.
- Assign physical item instances/stacks by stable identity; assignment references the real item and never copies it or creates duplicate ownership.
- The currently selected slot is the active hand item. Show the selected slot and held/holstered state clearly.
- Scroll-wheel up selects the next slot; scroll-wheel down selects the previous slot, wrapping from slot 6 to 1 and slot 1 to 6. These inputs are active only while no menu is open.
- **H** holsters the active-hand item without clearing or changing the quickbar assignments. Selecting a populated slot draws its item again.
- Number-key selection is not currently part of the confirmed control scheme.
- Slot selection/drawing never uses or consumes an item. Supported consumables require an intentional left-click to activate; arbitrary assigned items cannot be consumed merely by selection.
- Tools and weapons are held on selection only. Their later use/attack inputs and effects are outside this confirmed interaction rule and need separate approval.
- Handle depleted, moved, merged, split, sold, transferred, equipped or inaccessible items safely. Invalid assignments should clear or visibly become unavailable according to a documented policy.
- Backpack drop must never make its contents usable through stale quickbar references.
- Persist slot assignments and restore them safely against current inventory identities.

## 5. Architecture guardrails

- Core owns skill progression, equipment-slot state, quickbar assignment identity and validation.
- Game owns ScriptableObject definitions, input bindings, HUD/character presentation and action adapters.
- Reuse GridInventory, PlayerPocketBook, PlayerEquipmentLoadout, AssetOwnershipBook, AssetTransferService, StackTransferService and SaveData authorities.
- Never duplicate item data into the quickbar. Store stable instance IDs and resolve against accessible authoritative inventory/equipment.
- Never serialize Unity object references.
- Equipment and quickbar operations must be transactional and preserve stack/instance identity.
- No Core dependency on UnityEngine.
- No new Unity package or external dependency.

## 6. Explicit exclusions

- No city expansion during P-04.
- No new NPC cast, story content or backstories.
- No general combat system, enemy AI, targeting, damage model or police/combat consequences by implication.
- No item durability/repair degradation beyond existing ItemInstanceState.
- No skill XP grind or repeated-action point farming.
- No attributes beyond the now-confirmed three: Health (primary), Armor and Heat Protection.
- No paid/cooldown/penalty respecialisation.
- No changes to backpack/pocket rules, stack sizes or economy balance without separate approval.
- No art/sprite creation or wearable overlays while the art pause remains active.

## 7. Proposed acceptance gates

### Progression
- Character UI accurately reflects Core ranks and point accounting.
- The two approved test skills demonstrably modify Health and Heat Protection through the authoritative attribute path; their temporary test formulas are documented and clearly marked non-final.
- Milestone rewards occur only after successful completion and cannot duplicate on replay/save-load.
- Respecialisation preserves all earned points and resets all allocated ranks.
- F5/F9 round-trip preserves base attributes, skill ranks, point totals and milestone history.

### Equipment
- Only compatible item types can enter each slot.
- Equip/unequip preserves exact item ID, state and ownership.
- No item can simultaneously exist in inventory, equipment and quickbar as separate physical copies.
- Save/load restores equipped items exactly.
- Dropped backpack contents remain inaccessible.

### Quickbar
- Six slots; assign, replace, clear and select slots.
- The selected slot's item is the active-hand item; no separate weapon/tool equipment slot duplicates it.
- Scroll up/down cycles next/previous slot only when no menu is open; verify wraparound at both ends.
- H holsters the active item but preserves slot assignments; selecting a populated slot draws it again.
- Verify assignment and selection for different item categories, including healing/energy consumables, tools, weapons and a backpack.
- Selecting any slot never activates or consumes its item; left-click is required for a supported consumable use.
- Tools/weapons are held without automatically performing an action; verify later action systems do not trigger on selection.
- Stack quantity changes are reflected immediately.
- Moving/splitting/merging/transferring/equipping/selling an assigned stack never creates duplicates or activates the wrong item.
- Stale references after save/load or backpack drop fail safely.
- Quickbar state round-trips through F5/F9.

### Regression
- Existing repair, production, retail, contracts, repair orders, storage, scavenging, pockets, stack lineage, NPC/social and backpack flows remain intact.
- Full EditMode suite and explicit Play Mode acceptance are required for each coherent slice.

## 8. Decisions required before implementation

1. **Active hand:** confirmed — the currently selected quickbar slot is the item equipped in the player's hand. There are no separate weapon/tool slots.
2. **Controls:** confirmed — scroll up/down selects next/previous slot only when no menu is open; H holsters the held item while preserving assignments. Number keys are not currently specified.
3. **Consumable use:** confirmed — selecting/drawing a consumable never consumes it; an intentional left-click is required to use it.
4. **Weapon scope:** selecting a weapon only holds it. Attack/action behavior remains a separate design decision and requires a dedicated proposal before implementation.
5. **Tool scope:** no tool actions for now. Tools may be held through the quickbar, but they have no action behavior in this phase.
6. **Initial skill-effect test:** confirmed in principle — one skill increases Health and one skill increases Heat Protection, for testing only. Skill identities and temporary rank formulas remain to be specified; do not invent them.

P-04a is implemented and user-verified: **557/557 EditMode tests passed**, with the user confirming the relevant connections and functionality. P-04b's context-menu and drag/drop assignment design is approved, but no implementation has started. Before P-04b coding, resolve the two edge decisions recorded in its section (duplicate item assignment across slots; references to currently equipped clothing/accessory items). Do not invent combat or tool-use mechanics. The active-hand, quickbar controls and consumable activation contract above are confirmed. Weapon behavior remains a separately scoped future challenge.


## P-04a — implemented source checkpoint (2026-11-27)

**Scope delivered**
- Added Health to `PlayerAttributeSet` and `EffectiveCharacterAttributes`, retaining exactly three attributes: Health, Armor and Heat Protection.
- Health is implemented as maximum health, with current HP retained in `PlayerStats`. Effective maximum Health is recalculated when skill allocation changes; reducing the maximum clamps current HP safely. Existing damage behavior is unchanged.
- The equipment loadout and temporary skill modifiers feed the same effective-attribute result. Core owns calculation; GameSession composes authored skill modifiers.
- Added temporary authored skill fixtures: `health_test` gives +10 maximum Health per rank; `heat_protection_test` gives +1 Heat Protection per rank. Both are marked provisional/test-only and can be removed or replaced without changing the progression engine.
- Updated the Character panel, HUD current/max HP display, base-attribute save mapping and load ordering. On load, the effective maximum is restored before the saved current HP is reapplied.
- Added two Core regression tests. The prior user-confirmed full suite was 555/555; expected suite after these tests is **557**, pending Unity compile/full EditMode and Play Mode acceptance.

**Not yet verified**
- No Unity Editor compile or test run was performed by the assistant.
- The current expected test count is not a pass claim.
- Actual skill effects for Repair, Production, Trading, Scavenging and Social, and milestone-trigger wiring, remain future work.
