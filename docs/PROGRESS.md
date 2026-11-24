# Progress — current repository truth

Last reconciled: **2026-11-24**. This is the short status board. Session history is in [the November log](progress/LOG-2026-11.md); approved rules and implementation detail live in the linked decision/task documents.

## Project baseline

- Unity **6000.6.9f1**, 2D URP. Active architecture: `Signal.Core` (plain C#) + `Signal.Game` (Unity integration). Active scene: `Assets/_Project/Scenes/HideoutSlice.unity`.
- Working branch: `chore/collaboration-workflow-and-audit`. Keep `main` untouched.
- Application version: **0.01**. Save snapshots are deliberately unversioned; there is no schema-version field, version gate or automatic migration chain. Older experimental saves may be incompatible after data-shape changes.
- Latest user-reported full EditMode result before B-05: **431/431 passed (2026-11-24)** after the B-04 implementation. The user also confirmed the First Workshop and repair/trading gameplay works in Play Mode; the remaining issue was that the Work Jacket was not found during scavenging. This 430/430 result predates the movement, guaranteed-loot and NPC-interaction-pause changes in the current patch.
- The user reports the First Workshop and repair/trading flow working in Play Mode. The Work Jacket still needs to be acquired through scavenging to complete the repair loop.

## Active initiative — Direction B: First Workshop and Repair (B-01 to B-04)

**Player outcome:** make physical goods at the hideout, fulfil one authored customer request for clean CARD, then purchase a practical upgrade that unlocks another recipe.

Approved rules:
- Player-operated hideout workshop first; keep Core compatible with NPC-owned production later.
- Authored immediate recipes create physical inventory items.
- Exactly one accepted/active customer contract at a time.
- Fulfilment transfers one exact whole player-owned stack to the issuer's physical receiving inventory.
- The issuer pays a fixed clean CARD reward from their existing resident account; no business account or minted money.
- The Compact Sewing Machine is a one-time practical upgrade and unlocks the Reinforced Patch Kit recipe.

Implementation and authored content are committed. Do not rebuild these systems without a concrete defect:
- [B-01 Production/workbench](tasks/LIVELIHOOD-B-01-PRODUCTION-WORKSHOP.md)
- [B-02 Customer contracts](tasks/LIVELIHOOD-B-02-CUSTOMER-CONTRACTS.md)
- [B-03 Practical upgrade](tasks/LIVELIHOOD-B-03-PRACTICAL-WORKSHOP-UPGRADE.md)
- [Starter catalogue and Play Mode script](content/STARTER_CATALOGUE.md)
- [Shared First Workshop overview](tasks/LIVELIHOOD-B1-B3-FIRST-WORKSHOP.md)

## Reusable implemented foundations

| Area | Existing foundation |
|---|---|
| World | Hideout, Street, CornerShop, Apartment and one persistent session |
| Inventory/items | Grid inventory, stable stack GUIDs, quality/condition/authenticity/ownership, item knowledge and inspection |
| Economy | Resident CARD accounts, ownership/custody, atomic transfers, fixed-price merchandise, fixed or condition-based authored buyback offers (Unity verification pending) |
| Workshop | Actor-aware production, one-active contract, physical fulfilment, issuer-paid reward, upgrade/recipe unlock |
| Scavenging/crime | One-time searchable physical bins, resident-owned containers, theft outcomes and witness-scoped knowledge |
| NPCs | Shared social world, local perception, individual knowledge, authored schedules, generic presenter and agenda threads |
| Dirty Cash | Physical currency, explicit contact exchange/cleaning and separate transaction history |

Specialist references: [Architecture](ARCHITECTURE.md), [Data Model](DATA_MODEL.md), [NPC framework](NPC_FRAMEWORK.md), [NPC simulation](NPC_SIMULATION.md), [NPC runtime](NPC_RUNTIME.md).

## Test convenience and item inspection

- `HideoutSlice.unity` now starts a fresh player session with **5000 CARD** (serialized `startingMoney`). Existing saves restore their saved balance and are not overwritten by this starting value.
- Item inspection is already wired: open inventory with Tab, then right-click an item stack. The same viewer is also passed to searchable-container and storage item interactions. The viewer shows authored description/category/reference value and only item properties the player has learned; unknown properties remain hidden. Press Esc or the viewer's Close button to dismiss it.

## Current verification and next action

**EditMode:** the user's **431/431 result on 2026-11-24** predates B-05. It remains the last user-verified baseline; rerun the full suite in Unity on current HEAD.

**Play Mode:** the user reports the First Workshop and repair/trading features work well. The Work Jacket was not found in scavenging, so its loot availability is being made deterministic for testing.

**Current patch acceptance:**
1. Verify player movement feels right at the new 6 units/second speed (1.5× the previous 4).
2. Open a newly generated Street bin using `LootProfile_A`; it should contain a Work Jacket for repair testing. Existing generated bins retain their saved contents, so use a fresh/reset test bin or a fresh test save.
3. Confirm interaction pauses freeze NPC movement without blocking clock-driven schedule/activity changes; verify the resident replans toward the latest destination when the panel closes. Explicit debug-time movement must bypass only the short dialogue grace timer, not panel pauses, simulation pauses or a paused game clock.
4. Run the full EditMode suite and the combined B-01–B-04 Play Mode loop again.

The guaranteed Work Jacket entry is explicitly temporary test scaffolding and should be removed or changed back to a low-weight normal entry once the repair loop is fully validated.

## Active initiative — B-05 Global NPC Awareness & Affinity

The user approved a shared social foundation for every current and future NPC. Relationship ownership is intentionally split: `PlayerRelationshipBook` is the existing player→NPC authority and is already persisted; `NpcSocialWorld` owns NPC→NPC relationships and per-NPC knowledge. Both use the same `NpcRelationship` value model.

**Implemented in the current branch (Unity verification pending):**
- `SaveData` captures each registered NPC’s personal knowledge, including event identity, original actor, latest source, observation/told origin, confidence, event location/coordinates and occurred/learned timestamps.
- Directed NPC→NPC familiarity/trust/affinity is captured independently for each observer→target pair.
- F5/F9 restores social state after scene residents are registered. Unknown residents, invalid source references and malformed values are skipped and counted in `LoadReport`.
- Loading clears previous NPC knowledge/relationships and the transient event journal before applying the save. The journal is not serialized as private memory; positions and perception remain rebuilt from the live scene.
- `NpcSocialWorld.AdjustRelationship` is the validated directed NPC mutation path. Scheduled encounters use it for familiarity only; trust and affinity do not drift from ambient meetings.
- `PlayerRelationshipBook` and its existing save format remain unchanged. Agenda gates/outcomes already read/write that generic player→NPC book; the social encounter system uses the generic NPC→NPC world for all registered residents.
- Added focused tests for sourced knowledge and directional relationship round-trip, transient journal exclusion, unknown residents, relationship dimensions and non-finite values.

Task specification: `tasks/LIVELIHOOD-B-05-GLOBAL-NPC-AWARENESS-AFFINITY.md`. No Unity compile or EditMode result has been run/claimed for this patch. The latest user-reported baseline before B-05 remains **431/431**; rerun the full suite on this branch head.

## Repository hazards and deferred scope



- `Assets/_Project/Content/Items/` and `Assets/_Project/Content/items/` differ only by case and may collide on Windows. New authored content currently uses distinct folders such as `Content/WorkshopItems/`; preserve GUIDs and do not merge/rename those folders outside Unity Editor.
- Deliberately deferred: universal supply/demand, automatic NPC shopping, finite stock simulation, market-wide/time-/demand-driven price changes, passive income, production timers/queues, procedural/multiple contracts, partial-stack contract delivery, customer-owned repairs, random repair failure, broad relationship rewards, police response, global rumour cascades and new districts. Selective authored condition-based buyback is the approved narrow exception.

## Documentation maintenance

Keep this file concise. Append session history to `progress/LOG-2026-11.md`; keep approved rules in `DESIGN.md` / `decisions/`; keep implementation acceptance in the active task cards. The root `PROGRESS.md` is an intentional compatibility pointer to this canonical file.
