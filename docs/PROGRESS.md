# Progress — current repository truth

Last reconciled: **2026-11-25**. This is the short status board. Session history is in [the November log](progress/LOG-2026-11.md); approved rules and implementation detail live in the linked decision/task documents.

## Project baseline

- Unity **6000.6.9f1**, 2D URP. Active architecture: `Signal.Core` (plain C#) + `Signal.Game` (Unity integration). Active scene: `Assets/_Project/Scenes/HideoutSlice.unity`.
- Working branch: `chore/collaboration-workflow-and-audit`. Keep `main` untouched.
- Application version: **0.01**. Save snapshots are deliberately unversioned; there is no schema-version field, version gate or automatic migration chain. Older experimental saves may be incompatible after data-shape changes.
- Latest user-reported full EditMode result: **486/486 passed (2026-11-25)** after P-03.4a. P-03.4b adds eight Core tests; expected total is **494**, pending Unity compilation and test execution. B-05/B-06 and the backpack drop/recovery/fresh-start UI are user-verified.
- The user reports the First Workshop and B-04 repair/trading loop working in Play Mode. B-06 uses a separate Worn jacket seeded in Milo's inventory for the entrusted-service test.

## Completed livelihood foundations — B-01 to B-06

The first workshop, customer contract, practical hideout upgrade, repair/restoration loop, generic NPC awareness/relationship foundation and customer repair orders are implemented. The user confirmed **456/456 EditMode tests** and successful B-06 in-game testing on 2026-11-25.

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

**EditMode:** the user confirms **486/486 passed (2026-11-25)** after P-03.4a. P-03.4b adds eight Core tests; expected total is **494**, pending Unity execution.

**Play Mode:** the user reports the First Workshop, repair/trading features and B-06 entrusted repair-order loop work correctly.

**Active initiative:** P-03 Equipment, Wearables & Portable Backpack. Seven equipment slots, equipped backpack identity, physical B/drop + E/recovery, F5/F9 backpack world state and three persistent 1×1 personal pockets are integrated. The character UI displays real pocket contents; P moves a hovered 1×1 backpack item into a pocket and clicking an occupied pocket moves it back. Selected item receipts route into available pockets. User confirmed 486/486 after P-03.4a; eight new Core tests bring the expected total to 494, pending Unity execution. Remaining work: service-panel pocket coverage, clothing overlays and full Play Mode acceptance. See [P-03 task card](tasks/PLAYER-P-03-EQUIPMENT-BACKPACK.md). B-07 Neighbourhood Expansion remains intact; W-01 remains separate.

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
- Eleven focused EditMode tests cover ownership, repair, payment, replay, insufficient funds, player-account overflow, missing issuer account, receiver capacity and active/completed save/load. The user confirmed the full suite at **456/456** and reported the in-game flow passing.
- Task card: tasks/LIVELIHOOD-B-06-CUSTOMER-REPAIR-ORDERS.md.


## Player Development and weather design preparation (2026-11-25)

- The user chose to intercalate character/skills customization before returning to the B-07 roadmap. B-07 remains Neighbourhood Expansion; Player Development is a parallel track and does not renumber Direction B.
- P-01 Character Foundation & Appearance and P-02 Attributes & Skills specifications have been created. The current player is dynamically created by `RoomBootstrap.CreatePlayer()` with a placeholder SpriteRenderer; `PlayerMotor2D` currently owns input/velocity and has no formal animation/facing contract; `GameSession` is the composition root.
- Confirmed player design: customize the existing player; fixed starting skill-point budget freely distributed; additional points awarded for important milestones; points manually spent; free redistribution at any time.
- Exactly two attributes are approved: Armor and Heat Protection. Heat Protection keeps its name and protects against cold-related exposure only. No hot-weather/overheating mechanics are planned.
- Clothing and armor are intended to have visual and gameplay effects. Full authoritative equipment/loadout is reserved for P-03; P-01 prepares visual layer seams and P-02 prepares attribute modifier calculation.
- Candidate skills Repair, Production, Trading, Scavenging and Social remain provisional; numeric starting budget, rank caps/costs and milestone rewards remain open.
- W-01 Dynamic Weather & Cold Exposure is documented as a separate future phase: dynamic seasonal weather including temperature changes, rain, storms, snow and blizzards; gradual cold exposure; shelter and effective Heat Protection reduce exposure. No implementation is authorized.
- No gameplay code, assets, scenes, packages or tests were changed in this documentation milestone.


## Player Development implementation slice — 2026-11-25

- **P-01 code committed:** modular five-part Body_A player visual, shared idle/walk frame clock, Down/Side/Top facing and side mirroring. RoomBootstrap preserves existing physics/movement and uses a fallback placeholder until the supplied ZIP is imported.
- **Character resource import:** Unity menu `Signal > Import World Build Character Pack...` extracts all Body_A PNG sheets into Resources and configures pixel-art texture settings. The archive is not checked into the repository; the user must select their supplied ZIP once in the Unity Editor. The supplied pack provides one modular base body, not a full clothing/armor wardrobe.
- **P-02 code committed:** Armor and Heat Protection Core attributes; pure effective-attribute modifier calculator; skill book; configurable point progression; one-time milestone ledger; free respecialization; SaveData capture/restore; authored provisional skill catalog; F2 debug allocation panel.
- Prototype catalog tuning is currently **8 starting points, rank cap 5, 1 point per rank**. These are editable prototype defaults, not final balance. Repair, Production, Trading, Scavenging and Social remain provisional. Skill ranks are persisted and allocated, but activity-specific skill effects have not been wired pending approval of their exact effects.
- GameSession exposes `TryAwardPlayerMilestone`; no automatic milestone awards have been attached to activities yet. Only successful, stable-ID gameplay events should call it.
- Eleven focused EditMode tests were added for attributes, modifier calculation, allocation, overspending, rank cap, respecialization, milestone idempotency, save/load and invalid data handling.
- **Verification boundary:** the user-confirmed 456/456 baseline predates this code slice. Expected current suite count is **467**, but Unity compilation, tests and Play Mode have not yet been run. Do not mark this slice verified until the user reports the results.
- B-07 Neighbourhood Expansion remains the original Direction B phase; Player Development is intercalated as a parallel track. W-01 remains a separate future weather/cold-exposure phase.


## 2026-11-25 — Player character visual scale adjustment

- Increased the modular character's five visual sprite layers to **1.8×** their previous local scale.
- Applied scaling to the visual child layers only. The player root transform, Rigidbody2D, BoxCollider2D, movement speed, interaction range and camera configuration are unchanged; gameplay collision/physics sizing is preserved.
- User should verify the larger appearance in Play Mode. Unity compilation and visual verification are pending.


## P-03 first implementation slice — 2026-11-25

- Added Core seven-slot equipment loadout: Head, Torso, Legs, Feet and three generic accessory slots. Equipment transfers preserve exact item identity/state and do not change ownership.
- Added summed Armor/Heat Protection modifiers using the existing effective-attribute value model.
- Added PortableStorageBook keyed by stable backpack item instance ID. Backpack contents remain in an independent GridInventory and a non-empty container cannot be discarded.
- Added nine focused EditMode tests and Unity metadata for new files.
- This is a Core foundation only: no Game item definitions, UI, F5/F9 persistence, wearable visuals or dropped-world backpack object yet. The user's 467/467 verification predates these nine tests; expected suite is 476, pending Unity.
- Task card: tasks/PLAYER-P-03-EQUIPMENT-BACKPACK.md.


## P-03 second implementation slice — backpack equipped (2026-11-25)

- Expanded the temporary player inventory grid to 8×6 for test capacity. Final design remains 2–3 small personal pockets without a backpack; the large tabbed grid is intended to belong to the equipped backpack.
- Added optional equipment metadata to ItemDefinition and prototype Resources assets: Canvas Backpack (Accessory 1, 8×6), Padded Work Jacket (Torso, Armor +2 / Heat Protection +1), Insulated Coat (Torso, Armor +0 / Heat Protection +3). These modifiers are provisional test values, not final balance.
- Inventory controls: hover an item and press E to equip; press 1–7 to unequip ordinary equipment slots. Backpack unequip is intentionally blocked until physical drop/pickup is implemented.
- Equipping Canvas Backpack attaches the existing 8×6 GridInventory to the backpack's stable item instance ID without copying contents. A provisional backpack silhouette is shown on the player.
- F5/F9 capture and restore equipped item identity, definition, slot, modifiers, owner actor and item instance state. The current inventory contents continue to use the existing inventory save path.
- Added two focused tests for attaching an existing inventory grid and restoring an equipped item without a free inventory cell. User's confirmed 476/476 baseline predates these additions; expected total 478, pending Unity compile/test.
- Physical world backpack drop/pickup, no-backpack pocket restriction and final clothing visuals remain pending. See tasks/PLAYER-P-03-EQUIPMENT-BACKPACK.md.


## 2026-11-25 — Unity folder metadata reconciliation

- Corrected the two newly added SurvivalItems folder metadata files to preserve the GUIDs already generated in the user's local Unity project: SurvivalItems = dc080c683bb17b448ad72d35956ae8f4; Definitions = 0c275d1a215b0684e8fb75e43c9ca3c1.
- The earlier repository values were different and blocked the user's pull because the matching local files were untracked. The repository now matches the user's supplied local metadata exactly. No local files were deleted or changed.
- Repository housekeeping only; no gameplay code, assets, scenes or tests were changed.


## 2026-11-25 — GameSession duplicate resolver compile fix

- Removed the redundant second `FindItemDefinition(Guid)` method from `GameSession`. The existing earlier resolver remains the single canonical implementation and continues to search the registered item-definition catalogue.
- This resolves the reported CS0111 duplicate-member declaration. Repository source was updated; Unity compilation and EditMode tests have not been run by the assistant and remain pending user verification.


## P-03.3 Core backpack world-state foundation — 2026-11-25

- Added `PortableBackpackWorldBook` with explicit Equipped/Dropped states and persistent world coordinates. A backpack can only enter this registry if the matching `PortableStorageBook` contents grid already exists under the same stable item instance ID.
- Drop/recover transitions change only the physical state; the same contents grid and cargo item identities remain attached to the backpack.
- Added `SavedPortableBackpack` plus a focused save mapper that rejects malformed, duplicate, invalid or unbacked records.
- Added seven EditMode tests. The previous user-confirmed suite was 478/478; **485 is the expected count pending Unity compilation and test execution**, not yet verified.
- This is a Core foundation only. GameSession integration, physical world object, interaction, active-inventory switching, no-backpack pockets and full F5/F9 wiring remain next. No Play Mode acceptance is claimed.
- Next test game is specified in the P-03 task card; it must validate equipped → dropped → save/load while dropped → recovered → save/load while equipped, with cargo identity/content unchanged throughout.

## P-03.3 Game integration — backpack drop and pickup (2026-11-25)

- Added `PlayerEquipmentLoadout.TryTakeEquipped` so the backpack is removed from the equipment slot without being placed into its own storage grid.
- GameSession now tracks Equipped/Dropped state, spawns a visible ground placeholder, drops with **B**, and recovers with **E** nearby. Recovery automatically equips the same stable backpack item instance.
- Added a clickable **DROP BACKPACK [B]** button and a character/equipment sidebar beside the right-hand BACKPACK STORAGE grid. Seven equipment slots are shown; three pocket cells are visual placeholders only, with functional pocket storage still pending.
- Dropping closes the inventory, hides the backpack grid and blocks pointer interaction with its contents. A full audit/gating of all other item-based interactions while storage is dropped remains necessary alongside the active pocket-inventory work.
- F5/F9 now capture/restore the dropped item identity/state/owner/modifiers and its world coordinates, then recreate the dropped visual. Recovery after restore re-equips the same item.
- Added one focused EditMode test for exact equipped-item extraction. Previous user-confirmed suite was 485/485; expected total is **486**, pending Unity compilation and test execution. No Play Mode acceptance is claimed yet.
- User test sequence: equip and load cargo, B/button drop, verify inaccessible grid, E pickup/auto-equip, F5 while dropped, F9 and recover, then repeat save/load while equipped. Verify cargo identity/state/ownership.

### Dropped-backpack access guard

- While the backpack is dropped, GameSession now applies a conservative temporary gate to normal E/F interactions that could inspect, consume or receive items. Movement, pickup, F5/F9 and sleep remain available. This prevents workbench/shop/container flows from indirectly exposing the dropped bag's 8×6 contents.
- This is deliberately temporary until the 2–3 pocket inventory is functional; then gameplay systems should route through the active carried inventory rather than globally blocking interactions.
- Follow-up correction: dropped-backpack mode permits E at authored building/hideout doors and interior exits, and retains the navigation prompt. The temporary gate blocks item interactions, not world transitions.

- HUD follow-up: the initial navigation allowlist matched only five exact door labels, so an authored prompt such as “E Exit building” could still be hidden even though the E interaction worked. Replaced the brittle exact-string list with a case-insensitive E-action check for enter/exit/leave/return wording. Unity verification remains pending.

### P-03 HUD and API cleanup — 2026-11-25
- Corrected the dropped-backpack HUD path: keep the raw CityBlock world prompt separate from modified nearby-interaction text, recognise transition wording even when the source prompt lacks an E prefix, and display an explicit `E` hint. This addresses the user's report that exiting worked but the HUD did not tell them to press E.
- Replaced obsolete `FindObjectOfType<EventSystem>()` with `FindAnyObjectByType<EventSystem>()` in InventoryPanel (CS0618 cleanup).
- User confirms **486/486 EditMode tests passing** after pulling the previous code. Latest cleanup commits are pending Unity recompilation, test rerun and Play Mode confirmation.


### P-03 fresh-start inventory cleanup — 2026-11-25
- Removed the automatic starting-item grant from new sessions. The configured `items` list remains the item catalogue source; it no longer seeds player cargo.
- The temporary 8×6 player grid is now hidden/inaccessible unless a backpack is equipped. Its contents remain attached to the same backpack identity when dropped, recovered, saved or loaded.
- Item-based E/F interactions are gated while no backpack is equipped; authored world navigation, sleep and F5/F9 remain available. The inventory panel explains that storage requires an equipped/recovered backpack.
- To keep the fresh-start test path usable, a backpack spawned through the debug item spawner auto-equips when no backpack is equipped.
- Existing save files are deliberately preserved. Loading an older save with F9 may restore its previous inventory contents; this cleanup does not erase player saves.
- No new tests were added and no Unity compile/EditMode/Play Mode result is claimed. User should verify the new empty start, debug-spawned backpack auto-equip, storage visibility, and dropped-bag recovery.


## P-03.4a — Backpack-independent social/mission interactions (2026-11-25)
- Removed the global no-backpack E/F early return. NPC dialogue, agenda conversations, customer-request review/acceptance, and world navigation are no longer blocked by the absence of a backpack.
- Added preflight capacity warnings for physical item receipts: repair-order acceptance, shop purchases, searchable-container loot, illicit-exchange Dirty Cash rewards, and production output. These checks run before the transactional service call, preserving money, ownership, inputs and source items on rejection.
- Retail browsing and container inspection remain available without a backpack. Home-storage transfers and workbench access remain blocked until pocket storage exists, because those flows currently depend on the 8×6 inventory grid.
- No-backpack HUD no longer overwrites useful NPC/mission prompts with a generic backpack warning.
- No new tests were added in this subphase. The last user-confirmed suite is 486/486 before these code changes; Unity compile, EditMode rerun and Play Mode checks remain pending.
- Next: P-03.4b functional three-slot 1×1 pocket storage, persistence, UI and pocket-aware routing. Pockets remain placeholders; do not mark P-03 complete.


## P-03.4b — functional personal pockets (2026-11-25)
- Added PlayerPocketBook: exactly three independent 1×1 pockets, each holding one stack of one 1×1 item. Larger footprints cannot be stored in pockets.
- Added exact-stack transfer between backpack and pockets using InventoryTransfer, preserving stable item identity, count and instance state. These are internal player-container moves; ownership remains Player.
- Registered pocket grids with EconomicContainerRegistry as Player containers.
- Added SavedPocketItem and PlayerPocketSaveMapper; GameSession F5/F9 now captures/restores pocket index, stable item identity, stack quantity, item condition/quality/authenticity/ownership state and owner.
- Character inventory UI now displays actual pocket contents. Hover a backpack item and press P to move it to the first free pocket; click an occupied pocket to move it into backpack storage (or inspect if no backpack is available). Ordinary 1×1 equipment can be unequipped into a free pocket without a backpack.
- Item-receive routing now supports pockets for repair-order acceptance, retail purchase, searchable-container pickups and Dirty Cash exchange rewards. Milo's parcel opportunity can be accepted and tracked in pockets without a backpack; parcels inside a dropped backpack remain inaccessible.
- Added eight focused Core tests for pocket capacity, footprint rules, identity/state-preserving transfers, failed-transfer atomicity and save/load validation. User confirmed 486/486 after P-03.4a; expected new total is 494, pending Unity execution. No Unity compile/test/Play Mode result is claimed.
- Customer Contract now lists pocket-held goods, allowing qualifying contract fulfilment without a backpack. When no backpack is accessible, Retail and Dirty Cash panels switch to an empty carry view so dropped-bag cargo is not exposed; pocket-held items are not yet listed in Retail SELL or Dirty Cash selection. Home-storage transfer and workbench/production still require the backpack. P-03 remains active.

## P-03.4b verification and UI follow-up (2026-11-25)
- User confirmed **494/494 EditMode tests passed** for the functional-pocket implementation.
- Follow-up UI fixes committed: drag-and-drop a backpack item directly onto a personal pocket; equip equippable items directly from a pocket with E; size the searchable-container player grid from the active GridInventory dimensions (8×6 for the current backpack rather than the old hardcoded 6×4 display).
- The UI follow-up is not yet Unity-verified. Next: pull latest, compile, run EditMode suite, then Play Mode test pocket drops, pocket equipment, invalid footprint rejection and searchable-bin 8×6 visual alignment.

## Debug-only fresh-session starting loadout (2026-11-26)
- Added a temporary GameSession startup bootstrap for Play Mode testing. Each fresh session finds a compatible portable backpack definition (8×6), creates a unique backpack item instance, equips it through the normal equipment/storage/world-state path, then seeds up to three non-backpack items with distinct grid footprints into the backpack.
- The seed items are selected from the loaded item catalogue, sorted largest footprint first, and placed through normal GridInventory placement. Ownership is registered to the Player. Existing item definitions and save schemas are unchanged.
- This is explicitly debug/test scaffolding, not intended starting gear or final game balance. Remove this helper before establishing the real new-game starting inventory.
- If no compatible backpack is available, the helper logs a warning and leaves the session otherwise unchanged. If fewer than three distinct item footprints exist in the catalogue, it seeds however many are available.
- No Unity compile, EditMode or Play Mode result is claimed for this commit. User should pull and confirm the character spawns with an equipped backpack and 2–3 differently-sized items visible in its 8×6 grid.

## Pocket UI follow-up and hidden-pocket design (2026-11-26)
- Pocket cells now render the corresponding ItemDefinition sprite when one exists; when no sprite is authored, the cell displays the item name as a readable fallback rather than only an empty-looking slot.
- Pocket-to-backpack drag/drop is now supported. Dragging an occupied pocket onto a valid backpack cell transfers the exact item stack to that precise cell, preserving stable instance identity and item state. Invalid/occupied destinations leave the item in its pocket. The existing click action remains available for inspecting a pocket item.
- Added Core coverage for exact-position pocket transfer and atomic failure when the target cell is occupied. These tests have been added but have NOT yet been run in Unity; the last user-confirmed baseline remains 494/494.
- User clarified the intended pocket layout: two ordinary personal pockets plus one hidden pocket, with the hidden pocket intended to preserve at least one object from police/search confiscation. This changes the current three-equivalent-pocket design. Do not implement hidden-pocket search/concealment rules until the user explicitly confirms the proposed rules. Proposed baseline for confirmation: all three pockets remain player-accessible; only the first two are exposed to ordinary inventory searches; the hidden slot is excluded from routine police searches and preserves its single 1x1 item. A special thorough/strip search could be a separate future rule if desired.

### P-03 pocket roles and UI follow-up — 2026-11-26

- Confirmed design: P1/P2 are ordinary pockets; P3 is hidden and visually distinguished with a light-red background and “HIDDEN” label.
- PlayerPocketBook exposes only P1/P2 through its ordinary-search enumeration API. P3 remains accessible to the player but is omitted from routine-search results. Advanced searches (scanner/dogs/thorough search) remain undecided.
- Backpack items are moved into the exact pocket the player drops them onto; there is no automatic first-free-pocket assignment. Pocket-to-backpack drag/drop targets the exact backpack cell.
- Pocket cells render ItemDefinition icons where available, otherwise a readable text fallback.
- Added a focused Core test for ordinary-search visibility. Unity compilation and test execution are pending; last user-confirmed baseline remains 494/494 before these additions.
- Police search/confiscation is not yet a complete implemented gameplay flow. Future routine police-search code must query `GetItemsVisibleToOrdinarySearch()` and must not directly enumerate all pocket grids.

### P-03 exact pocket-to-backpack test correction — 2026-11-26

- Fixed `MovesPocketItemToExactBackpackDropCell`: its fixture incorrectly created a 2×1 item, which cannot legally enter a 1×1 pocket. The test now uses a valid 1×1 pocket item and continues to assert placement at the exact requested backpack coordinates. Production transfer validation is unchanged; larger/rotated items remain ineligible for pockets.
