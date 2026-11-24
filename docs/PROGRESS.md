# Progress — current repository truth

Last reconciled: **2026-11-24**. This is the short status board. Session history is in [the November log](progress/LOG-2026-11.md); approved rules and implementation detail live in the linked decision/task documents.

## Project baseline

- Unity **6000.6.9f1**, 2D URP. Active architecture: `Signal.Core` (plain C#) + `Signal.Game` (Unity integration). Active scene: `Assets/_Project/Scenes/HideoutSlice.unity`.
- Working branch: `chore/collaboration-workflow-and-audit`. Keep `main` untouched.
- Application version: **0.01**. Save snapshots are deliberately unversioned; there is no schema-version field, version gate or automatic migration chain. Older experimental saves may be incompatible after data-shape changes.
- Latest user-reported full EditMode result: **434/434 passed (2026-11-24)** after the B-05 social persistence, relationship mutation and test-name/confidence corrections. This result predates the latest customer-contract relationship outcome, inventory icon rendering and survival-pack importer changes; rerun the suite on current HEAD.
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

**EditMode:** the user reports **434/434 passed** after the first B-05 persistence slice and test correction. The latest B-05-04 customer outcome tests and item UI/importer changes have not yet been run in Unity; rerun the full suite on current HEAD.

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

Task specification: `tasks/LIVELIHOOD-B-05-GLOBAL-NPC-AWARENESS-AFFINITY.md`. The user reports **434/434** after B05-01–03. The new customer-outcome integration is implemented but still needs Unity compilation and a fresh full-suite run.

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
