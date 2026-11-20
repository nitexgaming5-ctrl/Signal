# CRIME-THEFT-001 — Resident-owned apartment container theft

**Status:** implementation committed; Unity compilation, EditMode and Play Mode verification pending  
**Date:** 2026-11-20  
**Parent design:** [CRIME-ECONOMY-001](CRIME-ECONOMY-001-SHARED-CITY-DESIGN.md)

## Player outcome

The player can enter an occupied apartment, search a resident-owned wardrobe and take a discovered item. The action is meaningfully different from searching a public street bin: the contents belong to a named resident, the transfer is recorded as theft, the physical item is marked stolen, and a nearby resident may personally witness the act.

This is the first crime action only. Pickpocketing, torn references, NPC theft, aggression, weapons and stealth are explicitly deferred until their interaction foundations exist.

## Authored first scenario

- Target: Rhea's existing apartment wardrobe.
- Stable container ID: `rhea-wardrobe`.
- Owner identity: `resident:rhea`, referenced through authored data. The crime/container framework is not hardwired to Rhea and can be reused for future `StealingHouse_NPC` residents.
- Location: `Apartment`.
- Interaction point matches the existing wardrobe prop at world position (1024.8, 1003.2).
- Contents are fixed, not rolled: ITEM_A ×2 and ITEM_C ×1. These are existing placeholder definitions, not final loot/economy balance.
- The existing side-by-side searchable-container UI is reused. Search reveals authored contents progressively; the player can double-click or drag/drop a discovered stack into carried inventory.

## Authoritative theft semantics

1. The runtime container inventory is registered under the resident owner actor, separately from container custody.
2. Only a successful whole-stack transfer from this resident-owned container uses `AssetTransferReason.Theft`. Street bins continue to use `Scavenging`.
3. The Core `AssetTransferService` applies `ItemOwnershipStatus.Stolen` as part of the same rollback-safe transfer operation. Other item-state fields and ItemInstanceId are preserved.
4. The existing economic transaction ledger records the exact stack, owner change, reason and in-game timestamp.
5. Only after a successful transfer does GameSession record one semantic `CrimeOutcome` with the player actor identity. A failed capacity/placement attempt produces no theft outcome, heat, progression or witness event.
6. The prototype's authored outcome awards **+2 heat** and **+1 global criminal-progression point** per successfully stolen stack. These are deliberately small, data-authored prototype tuning values, not final balancing.
7. Heat continues to use the existing HeatMeter and its current daily cooling placeholder. Criminal progression is a separate global total and does not decay with heat. No progression unlocks or heat-band consequences are introduced here.

## Exposure and witnesses

- At most one local witness is selected: the nearest registered resident who is physically in the property's authored location and within both the property's witness radius and the NPC's own awareness radius.
- The first wardrobe uses an 8-unit witness radius to match the existing resident awareness range and apartment layout. Rhea can witness while at home; being the owner does not make her omniscient.
- The selected NPC receives one character-scoped direct-observation event with an anonymous observable description. Other residents do not receive the event merely because they exist in the city.
- If nobody is in range, the theft still receives its authored heat/progression awards but creates no witness knowledge.
- The existing ambient social-encounter exchange deliberately skips `crime-observed` events. Witness sharing/reporting is not enabled until its rules are designed and approved.
- There is no automatic report, police response, relationship penalty or suspicion merely because the theft occurred. Later NPC sharing or authored consequences must use valid social/context gates.
- The selected witness identity and observation event are persisted with the crime outcome and restored to that NPC after loading.

## Core and Game model

- Core `CrimeOutcome` / `CrimeOutcomeLedger`: semantic successful-crime history including actor identity, independent of the economic transfer ledger and NPC knowledge.
- Core `CriminalProgressionTrack`: one global, non-decaying integer total shared by future criminal activities.
- Core `NpcSocialWorld.PublishEventToNpc`: explicit single-observer event delivery and idempotent witness-memory restoration.
- Core `AssetTransferService`: theft reason marks item state stolen inside the atomic transfer/rollback boundary.
- Game `ResidentPropertyContainerDefinition`: reusable ScriptableObject with stable owner/location/container IDs, interaction position, fixed ItemDefinition contents and authored crime/exposure tuning.
- GameSession composes the property container, ownership/custody, crime outcome, heat, progression and witness event.
- Save schema v13 persists criminal progression points and crime outcomes. SearchableContainerSaveMapper continues to persist the physical wardrobe contents/search progress independently.

## Verification checklist

1. Open `Assets/_Project/Scenes/HideoutSlice.unity`; confirm the new content asset resolves and there are no compiler errors.
2. Run the full EditMode suite. The previous user-confirmed baseline was 368/368; this batch adds 14 test cases, so 382 is the expected count if no other tests changed.
3. Confirm all three street bins still use `Scavenging`, retain their existing contents and do not award crime outcomes/heat/progression.
4. Enter the apartment, approach Rhea's wardrobe and press E. Confirm the fixed ITEM_A ×2 / ITEM_C ×1 contents appear once and do not reroll after closing/reopening.
5. Search and take a discovered item using both double-click and drag/drop. Confirm its ItemInstanceId is unchanged, ItemOwnershipStatus becomes Stolen, ownership changes from `resident:rhea` to Player, and the economic transaction reason is Theft.
6. Confirm a successful stolen stack adds one CrimeOutcome, +2 heat and +1 progression. A failed destination placement must leave the item in the wardrobe and change none of those values.
7. Use F1 → “Force Rhea: Home + focus”, move to the wardrobe and steal an item. Confirm Rhea alone receives the `theft-observed` knowledge event. Use “Return Rhea to authored schedule” afterwards.
8. Repeat while no resident is in the authored witness radius. Confirm no NPC learns the event, while the authored heat/progression still apply.
9. Confirm there is no police/reporting response and no automatic relationship change.
10. Save with F5, load with F9, and verify wardrobe contents/search progress, stolen item state, ownership, economic transaction, heat, progression, crime outcome and the selected witness's knowledge all survive. Loading the same save again must not duplicate the witness event.
11. Re-run the full EditMode suite after any fixes.

## Exclusions and open follow-up

- No NPC theft/pickpocketing, torn references, stealing worn/carried items, aggression, weapons or stealth.
- No automatic reporting, police AI, pursuit, arrest, investigation, heat bands or authored consequences.
- No dirty-money item, illicit exchange, cleaning offer or production recipe in this slice; those remain later slices under CRIME-ECONOMY-001.
- Prototype heat/progression values, daily cooling and witness radius remain tunable and should be revisited after Play Mode testing.
- No Unity compile/test/Play Mode success is claimed until the user verifies it in the Editor.
