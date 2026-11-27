# P-04 — Character Playability: Progression, Active Equipment & Quickbar

**Status:** Active specification — six-slot general-purpose quickbar direction confirmed; equipment layout, skill effects and item-action contracts still require decisions.  
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
3. Equip a weapon or practical tool into an intentional active-equipment slot and have that item represented by its exact physical instance.
4. Assign any suitable carried item—consumables (healing/energy), tools, weapons, backpacks and other item types—to one of six general-purpose quickbar slots; select and activate it through a validated item-action contract.
5. Save/load progression, equipped gear and quickbar assignments without duplicating items or exposing inaccessible dropped-backpack contents.

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

### P-04b — Active weapon/tool equipment
- Extend the current equipment model without repurposing clothing/accessory slots.
- Add authored equipment categories and compatible dedicated slot(s).
- Equip/unequip exact physical item instances through the existing ownership-aware inventory paths.
- Preserve item state, ownership, save/load and dropped-backpack isolation.
- Make active equipment visible in the character UI and available to the relevant player action.
- A weapon slot alone does not imply combat. Attack, damage, targeting, durability, ammunition and crime consequences remain out of scope unless separately designed and approved.
- Tools should only perform actions for which an existing or separately approved gameplay interaction exists.

### P-04c — Six-slot general-purpose quickbar
- Add a small always-visible six-slot quickbar to the HUD.
- Every slot can be assigned any supported carried item type: healing/energy consumables, tools, weapons, backpacks and other suitable items. The bar is not restricted to consumables or a dedicated weapon/tool category.
- Assign physical item instances/stacks by stable identity; assignment references the real item and never copies it or creates duplicate ownership.
- Provide clear selected-slot state. Keyboard mapping (proposed 1–6) and optional mouse-wheel selection remain input details to finalize.
- Use/activate only items with an explicitly supported action contract; do not consume arbitrary inventory items merely because they are assigned.
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
- Verify assignment and selection for different item categories, including healing/energy consumables, tools, weapons and a backpack.
- Keyboard selection and HUD selection state are clear.
- Only supported items/actions can be activated.
- Stack quantity changes are reflected immediately.
- Moving/splitting/merging/transferring/equipping/selling an assigned stack never creates duplicates or activates the wrong item.
- Stale references after save/load or backpack drop fail safely.
- Quickbar state round-trips through F5/F9.

### Regression
- Existing repair, production, retail, contracts, repair orders, storage, scavenging, pockets, stack lineage, NPC/social and backpack flows remain intact.
- Full EditMode suite and explicit Play Mode acceptance are required for each coherent slice.

## 8. Decisions required before implementation

1. **Equipment layout:** recommended starting point is one dedicated Weapon slot plus two dedicated Tool slots, separate from the seven existing clothing/accessory slots. Alternative: one shared Active Hand slot that accepts either a weapon or a tool.
2. **Quickbar:** six general-purpose slots accepting any suitable carried item type is confirmed. Proposed keyboard mapping is 1–6; confirm or revise the input mapping when the HUD/input slice is designed.
3. **Weapon scope:** equipping/visual selection only for the first slice, or do you want an actual attack/action loop designed now? The latter is a separate combat mechanic and requires a dedicated proposal.
4. **Tool scope:** identify the first one or two practical tools/actions to support. Existing repair/production tools are currently materials/ingredients; making them held, activated tools changes their gameplay contract.
5. **Skill effects:** approve or revise the candidate skill list and effect proposals before numerical formulas are coded.

P-04a can begin with an audit and UI/architecture refinement, but gameplay effects, new equipment categories and active-use mechanics must not be invented silently. Await explicit approval of the relevant decisions before implementing those mechanics.
