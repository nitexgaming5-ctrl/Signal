# Progress — current repository truth

Last reconciled: **2026-11-25**. This is the short status board. Session history is in [the November log](progress/LOG-2026-11.md); approved rules and implementation detail live in the linked decision/task documents.

## Project baseline

- Unity **6000.6.9f1**, 2D URP. Active architecture: `Signal.Core` (plain C#) + `Signal.Game` (Unity integration). Active scene: `Assets/_Project/Scenes/HideoutSlice.unity`.
- Working branch: `chore/collaboration-workflow-and-audit`. Keep `main` untouched.
- Application version: **0.01**. Save snapshots are deliberately unversioned; there is no schema-version field, version gate or automatic migration chain. Older experimental saves may be incompatible after data-shape changes.
- Latest user-reported full EditMode result: **439/439 passed (2026-11-25)**, including the debug item spawner and its EditMode cleanup fix. The latest B-05 NPC→player relationship work adds three focused tests; these still require a fresh Unity run (expected 442/442 if no other tests changed).
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

**EditMode:** the user reports **439/439 passed** on the item-spawner fix. The latest B-05 NPC→player relationship implementation adds three focused tests and has not yet been run in Unity; rerun the full suite on current HEAD.

**Play Mode:** the user reports the First Workshop and repair/trading features work well. The Work Jacket was not found in scavenging, so its loot availability is being made deterministic for testing.

**Current patch acceptance:**
1. Verify player movement feels right at the new 6 units/second speed (1.5× the previous 4).
2. Open a newly generated Street bin using `LootProfile_A`; it should contain a Work Jacket for repair testing. Existing generated bins retain their saved contents, so use a fresh/reset test bin or a fresh test save.
3. Confirm interaction pauses freeze NPC movement without blocking clock-driven schedule/activity changes; verify the resident replans toward the latest destination when the panel closes. Explicit debug-time movement must bypass only the short dialogue grace timer, not panel pauses, simulation pauses or a paused game clock.
4. Run the full EditMode suite and the combined B-01–B-04 Play Mode loop again.

The guaranteed Work Jacket entry is explicitly temporary test scaffolding and should be removed or changed back to a low-weight normal entry once the repair loop is fully validated.

## Active initiative — B-05 Global NPC Awareness & Affinity

B-05 is a generic NPC framework phase, not a character/cast-writing phase. Rhea, Milo and the existing Dirty Cash contact are framework/test residents only. Do not add residents, invent backstories or author the final cast until the main storyline and intended cast have been deliberately designed.

Relationship ownership is explicitly directional:
- Player→NPC: existing `PlayerRelationshipBook`, unchanged.
- NPC→NPC: each observer's relationship map inside `NpcAgentState` / `NpcSocialWorld`.
- NPC→player: a separate per-NPC opinion stored on `NpcAgentState` / `NpcSocialWorld`.
- All three directions use the same `NpcRelationship` value model, but never share storage authority or register the player as an NPC.

**Implemented in the current branch (latest directionality slice awaits Unity verification):**
- Shared local perception, individual sourced knowledge, explicit knowledge sharing and directed NPC-to-NPC relationships.
- F5/F9 persistence for each NPC's personal knowledge and directed NPC-to-NPC relationships.
- New independent NPC→player opinion with neutral per-NPC default and validated get/set/adjust APIs.
- F5/F9 persistence for NPC→player opinion, validated against registered stable NPC IDs and finite bounded values. Restore clears prior timeline state and rebuilds the transient event journal/presence separately.
- Customer contracts now support authored outcomes in both directions: player→issuer through PlayerRelationshipBook and issuer→player through the issuer's own NpcAgentState. Both apply only after successful goods transfer, payment and contract completion; replay cannot award twice.
- Added tests for independent directionality, persistence round-trip, contract success/replay and target validation. These latest tests have not run in Unity.

The user reports **439/439** after the debug item spawner fix. The latest slice adds three tests, so expect **442/442** if no other test files changed. Do not claim that result until the user runs the Unity suite.

Task specification: `tasks/LIVELIHOOD-B-05-GLOBAL-NPC-AWARENESS-AFFINITY.md`.

## Developer debug item spawner

- F1 opens the existing developer debug menu. Its Open item spawner action opens a searchable, scrollable catalogue panel with item icons, category, grid footprint and stack limit.
- Clicking an entry adds one instance to the real player inventory using that definition's default instance state. The existing inventory capacity/stack rules apply; full inventory returns a visible message and does not silently discard the item.
- Successful spawns are registered as player-owned through the existing ownership book and are ordinary inventory items, so the existing save/load path handles them.
- The picker uses the same shared item-definition catalogue as inventory, inspection, shops and containers. That includes the 100 local survival-pack definitions plus definitions referenced by the scene, recipes, contracts, businesses, loot profiles, resident property and special-item fields. The active HideoutSlice scene explicitly assigns ITEM_A/B/C; the Work Jacket is registered through the repair target and loot/business content.
- The current GitHub source contains no separate ItemDefinition named Wool. If Wool exists only in a local uncommitted asset, it will appear once added to a source catalogue the session loads; the spawner does not fabricate a duplicate definition.
- Two focused EditMode tests cover panel binding/open-close and routing a selected definition through the spawn callback.
- **EditMode cleanup fix:** `DebugItemSpawnerPanel.RefreshTiles` now uses deferred `Destroy` while playing and `DestroyImmediate` outside Play Mode. This avoids Unity's edit-mode destruction warning/error when the panel rebuilds its tiles during EditMode tests; runtime behaviour remains deferred and safe.
 The user subsequently confirmed **439/439** after the EditMode cleanup fix. The latest B-05 directionality tests are newer and remain pending; expected current suite count is 442 if no other tests were added.

## Repository hazards and deferred scope



- `Assets/_Project/Content/Items/` and `Assets/_Project/Content/items/` differ only by case and may collide on Windows. New authored content currently uses distinct folders such as `Content/WorkshopItems/`; preserve GUIDs and do not merge/rename those folders outside Unity Editor.
- Deliberately deferred: universal supply/demand, automatic NPC shopping, finite stock simulation, market-wide/time-/demand-driven price changes, passive income, production timers/queues, procedural/multiple contracts, partial-stack contract delivery, customer-owned repairs, random repair failure, broad relationship rewards, police response, global rumour cascades and new districts. Selective authored condition-based buyback is the approved narrow exception.

## Documentation maintenance

Keep this file concise. Append session history to `progress/LOG-2026-11.md`; keep approved rules in `DESIGN.md` / `decisions/`; keep implementation acceptance in the active task cards. The root `PROGRESS.md` is an intentional compatibility pointer to this canonical file.


### B-05 naming and save-test correction
- Shortened the new NPC social test names to describe one behavior each, renamed the restored knowledge local for clarity, and corrected the knowledge-sharing confidence expectation to `0.70` (listener trust `0.70` yields `1.0 * (0.35 + 0.70 * 0.5)`).
- This is a focused test/readability correction; no runtime behavior or public API was changed. Unity verification remains required.


## Current B-05 integration — customer relationship outcomes

- Customer contracts now support authored player→issuer familiarity, trust and affinity deltas. The GameSession passes the issuer's stable NPC ID and authored values into the generic Core fulfilment service.
- Player→issuer relationship changes occur only after the exact item transfer, customer payment and contract completion all succeed. The contract's Completed state blocks replay, so the outcome cannot be applied twice.
- The generic contract hook currently targets the player→issuer relationship book. Milo's current contract keeps all deltas at zero until the separate NPC→player opinion owner is designed; a customer's successful delivery should not be misrepresented as the player's trust in that customer.
- Added regression tests for one-time issuer-only changes and invalid delta rejection. Unity verification is pending.

## Free Pixel Survival Items Pack — local licensed import

- The user supplied 100 individual 32×32 icons. The pack licence permits game use but prohibits uploading the graphics to public repositories; Signal's GitHub repository is public.
- The PNGs are therefore excluded by `.gitignore` at `Assets/_Project/Resources/SurvivalItems/Icons/*.png`. The importer and text manifest remain in the repository.
- Local install: copy the ZIP's `Icons` folder into `Assets/_Project/Resources/SurvivalItems/`, then run **Tools → Signal → Import Survival Items Pack** in Unity. The editor importer creates 100 `ItemDefinition` assets under `Resources/SurvivalItems/Definitions`.
- `GameSession` loads these definitions into the shared catalogue without granting them as starter items. `InventoryPanel` now displays authored sprites (including drag previews); `ItemViewerPanel` already displays the definition icon.
- Names, categories, grid footprints, stack sizes and base reference values are first-pass interpretations. Base values are inspection/reference values, not shop prices. No new consumable, equipment, weapon, recipe or loot behaviour is assigned. Items are not automatically added to loot profiles or shop stock.
- Unity import/compile/Play Mode verification remains pending.


### B-05 directionality gap

`PlayerRelationshipBook` is player→NPC; `NpcSocialWorld` is NPC→NPC. Neither currently stores an NPC's own opinion of the player. Do not overload either direction or register the player as an NPC. Proposed next design: a player-relationship value owned by each `NpcAgentState`, persisted with that NPC's durable social state. Awaiting confirmation before implementing this distinct owner and wiring NPC response/agenda gates to it.
