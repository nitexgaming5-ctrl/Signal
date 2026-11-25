# Progress — current repository truth

Last reconciled: **2026-11-25**. This is the short status board. Session history is in [the November log](progress/LOG-2026-11.md); approved rules and implementation detail live in the linked decision/task documents.

## Project baseline

- Unity **6000.6.9f1**, 2D URP. Active architecture: `Signal.Core` (plain C#) + `Signal.Game` (Unity integration). Active scene: `Assets/_Project/Scenes/HideoutSlice.unity`.
- Working branch: `chore/collaboration-workflow-and-audit`. Keep `main` untouched.
- Application version: **0.01**. Save snapshots are deliberately unversioned; there is no schema-version field, version gate or automatic migration chain. Older experimental saves may be incompatible after data-shape changes.
- Latest user-reported full EditMode result: **445/445 passed (2026-11-25)**, including B-05 NPC→player directionality, generic agenda integration, contract outcomes and persistence. B-05 is now user-verified.
- The user reports the First Workshop and B-04 repair/trading loop working in Play Mode. B-06 uses a separate Worn jacket seeded in Milo's inventory for the entrusted-service test.

## Completed livelihood foundations — B-01 to B-05; active B-06 implementation

The first workshop, customer contract, practical hideout upgrade, repair/restoration loop and generic NPC awareness/relationship foundation are implemented. B-05 is now verified at 445/445 EditMode tests. B-06 Customer Repair Orders is now approved and in its first implementation slice; Unity verification is pending.

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
| Economy | Resident CARD accounts, ownership/custody, atomic transfers, fixed-price merchandise, condition-based buyback offers, separate service-payment ledger |
| Workshop | Actor-aware production, one-active contract, physical fulfilment, issuer-paid reward, upgrade/recipe unlock |
| Scavenging/crime | One-time searchable physical bins, resident-owned containers, theft outcomes and witness-scoped knowledge |
| NPCs | Shared social world, local perception, individual knowledge, authored schedules, generic presenter and agenda threads |
| Dirty Cash | Physical currency, explicit contact exchange/cleaning and separate transaction history |

Specialist references: [Architecture](ARCHITECTURE.md), [Data Model](DATA_MODEL.md), [NPC framework](NPC_FRAMEWORK.md), [NPC simulation](NPC_SIMULATION.md), [NPC runtime](NPC_RUNTIME.md).

## Test convenience and item inspection

- `HideoutSlice.unity` now starts a fresh player session with **5000 CARD** (serialized `startingMoney`). Existing saves restore their saved balance and are not overwritten by this starting value.
- Item inspection is already wired: open inventory with Tab, then right-click an item stack. The same viewer is also passed to searchable-container and storage item interactions. The viewer shows authored description/category/reference value and only item properties the player has learned; unknown properties remain hidden. Press Esc or the viewer's Close button to dismiss it.

## Current verification and next action

**EditMode:** the user reports **445/445 passed** on current HEAD (2026-11-25). B-05's six new directionality/agenda tests are included in that result.

**Play Mode:** the user reports the First Workshop and repair/trading features work well. B-06 entrusted jacket is seeded in Milo's inventory; B-06 Play Mode acceptance is pending.

**Next B-06 acceptance:** compile the current branch in Unity, run the full EditMode suite (nine focused repair-order tests added; expected 454/454 from the previous 445 baseline), then exercise acceptance → workshop repair → exact-item return → customer debit/player credit → replay rejection and F5/F9 persistence. B-04 scavenging repair acceptance remains a separate existing loop.

The guaranteed Work Jacket entry is explicitly temporary test scaffolding and should be removed or changed back to a low-weight normal entry once the repair loop is fully validated.

## Completed initiative — B-05 Global NPC Awareness & Affinity

B-05 is a generic NPC framework phase, not a character/cast-writing phase. Rhea, Milo and the existing Dirty Cash contact are framework/test residents only. Do not add residents, invent backstories or author the final cast until the main storyline and intended cast have been deliberately designed.

Relationship ownership is explicitly directional:
- Player→NPC: existing `PlayerRelationshipBook`, unchanged.
- NPC→NPC: each observer's relationship map inside `NpcAgentState` / `NpcSocialWorld`.
- NPC→player: a separate per-NPC opinion stored on `NpcAgentState` / `NpcSocialWorld`.
- All three directions use the same `NpcRelationship` value model, but never share storage authority or register the player as an NPC.

**Implemented and user-verified on 2026-11-25 (445/445 EditMode tests):**
- Shared local perception, individual sourced knowledge, explicit knowledge sharing and directed NPC-to-NPC relationships.
- F5/F9 persistence for each NPC's personal knowledge and directed NPC-to-NPC relationships.
- New independent NPC→player opinion with neutral per-NPC default and validated get/set/adjust APIs.
- F5/F9 persistence for NPC→player opinion, validated against registered stable NPC IDs and finite bounded values. Restore clears prior timeline state and rebuilds the transient event journal/presence separately.
- Customer contracts now support authored outcomes in both directions: player→issuer through PlayerRelationshipBook and issuer→player through the issuer's own NpcAgentState. Both apply only after successful goods transfer, payment and contract completion; replay cannot award twice.
- Generic agenda stages can gate on either relationship perspective, and agenda choices can award one-time outcomes in either direction. GameSession passes the shared NpcSocialWorld to the generic director; repeatable choices are forbidden from applying relationship deltas.
- Added tests for independent directionality, persistence round-trip, contract success/replay/target validation, agenda gates/outcomes and repeatable-choice safeguards. These tests are included in the user's confirmed 445/445 full EditMode result.

The user has now confirmed **445/445 EditMode tests passed (2026-11-25)** on the current B-05 implementation.

Task specification: `tasks/LIVELIHOOD-B-05-GLOBAL-NPC-AWARENESS-AFFINITY.md`.

## Developer debug item spawner

- F1 opens the existing developer debug menu. Its Open item spawner action opens a searchable, scrollable catalogue panel with item icons, category, grid footprint and stack limit.
- Clicking an entry adds one instance to the real player inventory using that definition's default instance state. The existing inventory capacity/stack rules apply; full inventory returns a visible message and does not silently discard the item.
- Successful spawns are registered as player-owned through the existing ownership book and are ordinary inventory items, so the existing save/load path handles them.
- The picker uses the same shared item-definition catalogue as inventory, inspection, shops and containers. That includes the 100 local survival-pack definitions plus definitions referenced by the scene, recipes, contracts, businesses, loot profiles, resident property and special-item fields. The active HideoutSlice scene explicitly assigns ITEM_A/B/C; the Work Jacket is registered through the repair target and loot/business content.
- The current GitHub source contains no separate ItemDefinition named Wool. If Wool exists only in a local uncommitted asset, it will appear once added to a source catalogue the session loads; the spawner does not fabricate a duplicate definition.
- Two focused EditMode tests cover panel binding/open-close and routing a selected definition through the spawn callback.
- **EditMode cleanup fix:** `DebugItemSpawnerPanel.RefreshTiles` now uses deferred `Destroy` while playing and `DestroyImmediate` outside Play Mode. This avoids Unity's edit-mode destruction warning/error when the panel rebuilds its tiles during EditMode tests; runtime behaviour remains deferred and safe.
 The user subsequently confirmed **439/439** after the EditMode cleanup fix; the later B-05 directionality and agenda integration tests are now included in the confirmed **445/445** result.

## Repository hazards and deferred scope



- `Assets/_Project/Content/Items/` and `Assets/_Project/Content/items/` differ only by case and may collide on Windows. New authored content currently uses distinct folders such as `Content/WorkshopItems/`; preserve GUIDs and do not merge/rename those folders outside Unity Editor.
- Deliberately deferred: universal supply/demand, automatic NPC shopping, finite stock simulation, market-wide/time-/demand-driven price changes, passive income, production timers/queues, procedural or concurrent customer jobs, partial-stack contract delivery, repair queues/random failure, broad relationship rewards, police response, global rumour cascades and new districts. Selective authored condition-based buyback is the approved narrow exception.

## Documentation maintenance

Keep this file concise. Append session history to `progress/LOG-2026-11.md`; keep approved rules in `DESIGN.md` / `decisions/`; keep implementation acceptance in the active task cards. The root `PROGRESS.md` is an intentional compatibility pointer to this canonical file.


### B-05 naming and save-test correction
- Shortened the new NPC social test names to describe one behavior each, renamed the restored knowledge local for clarity, and corrected the knowledge-sharing confidence expectation to `0.70` (listener trust `0.70` yields `1.0 * (0.35 + 0.70 * 0.5)`).
- This is a focused test/readability correction; no runtime behavior or public API was changed. Unity verification remains required.


## B-05 relationship outcome integration (completed)

- Customer contracts route authored player→issuer and issuer→player outcomes to their distinct canonical owners. Both apply only after successful transfer/payment/completion and cannot be replayed.
- Generic agenda gates and choices can read/write either player→NPC or NPC→player perspective; repeatable choices cannot farm relationship deltas.
- These changes are included in the user's confirmed 445/445 full EditMode result.

## Free Pixel Survival Items Pack — local licensed import

- The user supplied 100 individual 32×32 icons. The pack licence permits game use but prohibits uploading the graphics to public repositories; Signal's GitHub repository is public.
- The PNGs are therefore excluded by `.gitignore` at `Assets/_Project/Resources/SurvivalItems/Icons/*.png`. The importer and text manifest remain in the repository.
- Local install: copy the ZIP's `Icons` folder into `Assets/_Project/Resources/SurvivalItems/`, then run **Tools → Signal → Import Survival Items Pack** in Unity. The editor importer creates 100 `ItemDefinition` assets under `Resources/SurvivalItems/Definitions`.
- `GameSession` loads these definitions into the shared catalogue without granting them as starter items. `InventoryPanel` now displays authored sprites (including drag previews); `ItemViewerPanel` already displays the definition icon.
- Names, categories, grid footprints, stack sizes and base reference values are first-pass interpretations. Base values are inspection/reference values, not shop prices. No new consumable, equipment, weapon, recipe or loot behaviour is assigned. Items are not automatically added to loot profiles or shop stock.
- Unity import/compile/Play Mode verification remains pending.





## Active initiative — B-06 Customer Repair Orders

- The user approved the entrusted-item repair service. Existing Milo is the first framework/test issuer; no new NPC cast or story content is added.
- The authored order seeds one Worn Work Jacket into Milo's registered inventory. F opens the repair-order panel; E remains the existing customer-contract interaction.
- Acceptance moves the exact item into player custody while Milo remains its registered owner. The shared workbench repairs that active entrusted target with a player-owned Basic Patch Kit; Worn becomes Good.
- Returning the exact item moves it back to Milo and atomically transfers 35 clean CARD from Milo to the player. A dedicated ServicePaymentTransactionLedger records the payment separately from physical asset transfers. Failed returns roll back item movement and both balances; one active customer job is enforced across contract and repair-order acceptance.
- F5/F9 persists order phase, item location/owner, accounts and service-payment history.
- Nine focused EditMode tests added for ownership, repair, payment, replay, insufficient funds, receiver capacity and active/completed save/load. Expected suite count **454** from the prior 445 baseline; Unity compilation and execution are pending.
- Task card: tasks/LIVELIHOOD-B-06-CUSTOMER-REPAIR-ORDERS.md.
