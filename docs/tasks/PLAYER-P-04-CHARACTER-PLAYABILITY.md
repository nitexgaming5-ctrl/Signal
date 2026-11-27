# P-04 — Character Playability: Progression, Active Equipment & Quickbar

**Status:** Active specification — six-slot quickbar/active-hand controls confirmed; Health is primary attribute; two temporary skill-effect tests approved; tool actions and weapon behavior deferred.  
**Track:** Player Development  
**Strategic priority:** Ahead of B-07 Neighbourhood Expansion.  
**Dependencies:** P-01 character foundation, P-02 attributes/skills, P-03 equipment/inventory/backpack.

## 1. Why this phase comes first

A larger city is not valuable if the player character cannot meaningfully progress, prepare for activities or access useful carried items during play. P-04 makes the character a playable RPG entity before expanding geography.

B-07 Neighbourhood Expansion is paused and must not be implemented while P-04 is active.

## 2. Current repository baseline (audit)

### Already implemented
- P-02 Core progression: exactly Armor and Heat Protection; skill ranks; starting point budget; milestone point awards; free respecialisation; stable-ID save/load.
- Current catalog is explicitly provisional: 8 starting points, rank cap 5, one point per rank; candidate skills Repair, Production, Trading, Scavenging and Social.
- F2 Player Development panel can allocate, deallocate and freely reset skill points.
- P-03 equipment loadout supports Head, Torso, Legs, Feet and three generic accessory slots. Backpack occupies an accessory slot.
- Exact-instance equipment transfer, item state and ownership preservation, backpack persistence, pockets and stack-aware inventory are implemented.
- Inventory panel supports item movement, splitting, merging and current equipment/backpack shortcuts.

### Missing or incomplete
- Skill ranks are stored but **do not yet affect the corresponding gameplay activities**.
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
- Show Armor and Heat Protection separately from skills.
- Show each skill's current rank, next-rank cost, unspent points and an understandable description of its actual effect.
- Preserve free respecialisation and the total-point invariant.
- Finalize the skill catalogue and define a small, testable effect for each selected skill.
- Integrate effects into existing authoritative services rather than adding duplicate calculations to UI.
- Award milestone points only after successful, committed gameplay events; use stable event IDs and existing idempotent progression authority.
- Persist all progression and milestone state.

**Candidate existing-activity integrations (not yet approved as formulas):**
- Repair → RepairService outcome or material efficiency.
- Production → ProductionService output/input efficiency.
- Trading → RetailService purchase/sale terms.
- Scavenging → searchable-container discovery/loot handling.
- Social → existing relationship/customer interactions.

No numerical bonus, chance, price formula, quality roll or extra reward is implied by this list. Each effect needs an explicit design decision before implementation.

### P-04b — Active hand item (quickbar-driven; no separate weapon/tool slots)
- The six-slot quickbar is the authoritative active-item selector. The currently selected slot's assigned item is the item equipped/held in the player's hand.
- Do not add a separate Weapon slot, Tool slot or duplicate Active Hand equipment slot. Do not repurpose the existing clothing/accessory slots.
- Selecting another occupied quickbar slot draws that slot's item into the active hand. Selecting an empty/unavailable slot safely leaves the hand empty. Selection never activates or consumes an item.
- **H** holsters the currently held item: active hand becomes empty, while all six assignments remain intact. Selecting another populated slot draws that item again.
- Preserve exact physical item identity, item state, ownership, save/load and dropped-backpack isolation. The active hand is a reference/state over the real item, not a second copy or a second owner.
- A weapon being held does not imply combat. Attack, damage, targeting, durability, ammunition and crime consequences remain out of scope unless separately designed and approved.
- Selecting an item never activates or consumes it. Healing items, energy drinks and other supported consumables require an intentional **left-click** to use, preventing accidental use while selecting/cycling slots. The exact effect/eligibility rules remain tied to each consumable's action contract.
- Selecting a tool or weapon simply holds it in the active hand; selection does not perform an action. Actual tool actions and weapon/combat behavior are later, separately designed interactions.

### P-04c — Six-slot general-purpose quickbar
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
- No new generic attributes; only Armor and Heat Protection.
- No paid/cooldown/penalty respecialisation.
- No changes to backpack/pocket rules, stack sizes or economy balance without separate approval.
- No art/sprite creation or wearable overlays while the art pause remains active.

## 7. Proposed acceptance gates

### Progression
- Character UI accurately reflects Core ranks and point accounting.
- Every approved skill has a demonstrable effect in its corresponding existing activity.
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
5. **Tool scope:** selecting a tool only holds it. Identify the first one or two practical tool actions separately; existing repair/production tools are currently materials/ingredients.
6. **Skill effects:** approve or revise the candidate skill list and effect proposals before numerical formulas are coded.

P-04a can proceed with the approved Health attribute extension and the two temporary skill-effect tests once their skill IDs and rank formulas are defined. Do not invent combat or tool-use mechanics. The active-hand, quickbar controls and consumable activation contract above are confirmed. Weapon behavior remains a separately scoped future challenge.
