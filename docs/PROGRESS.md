# Progress — current repository truth

**Last reconciled:** 2026-11-27  
**Branch:** `chore/collaboration-workflow-and-audit` (keep `main` untouched)  
**Unity:** 6000.6.9f1 · 2D URP · active scene `Assets/_Project/Scenes/HideoutSlice.unity`  
**Architecture:** `Signal.Core` (plain C# domain) + `Signal.Game` (Unity integration/UI/content)

## Verification

- **Latest user-confirmed full EditMode suite: 580/580 passed (2026-11-27).** This includes the two independent-backpack persistence tests.
- P-04a: 557/557 confirmed.
- P-04b initial assignment foundation: 562/562 confirmed; follow-up brought the suite to 563/563.
- P-03.7: 555/555 plus Unity compile and Play Mode acceptance confirmed; closed.
- B-06: 456/456 plus successful Play Mode repair/return/payment loop confirmed.
- Unity Editor is authoritative for compile, tests and Play Mode. Never claim verification beyond what the user reports.

## Strategic direction

The user selected **Direction B — meaningful everyday livelihood**: repair, production, customer jobs, services, relationships and a practical hideout. Character development was reprioritised ahead of B-07 neighbourhood expansion. Keep one active initiative at a time and build connected player-facing loops.

## Current player-development status

### P-01 — Character foundation
Implemented and user-verified: modular Body_A renderer, directional Idle/Walk and local asset importer. The user imported the supplied pack and confirmed the enlarged presentation.

### P-02 / P-04a — Attributes and progression
Implemented and user-verified at **557/557**.
- Exactly three attributes: **Health** (primary; maximum HP), **Armor**, **Heat Protection**.
- Current HP remains separate in `PlayerStats`; effective Health determines maximum HP.
- F2 opens the player-facing Character panel. HUD shows current/max HP.
- Effective attributes combine equipment modifiers and authored skill modifiers.
- Temporary test fixtures only: `health_test` (+10 maximum Health/rank) and `heat_protection_test` (+1 Heat Protection/rank). These are not final balance or permanent skill identities.
- Actual Repair/Production/Trading/Scavenging/Social effects and comprehensive milestone trigger wiring remain future work.

### P-03 — Equipment, backpack and pockets
P-03.7 is closed and user-verified at **555/555**, including compile and Play Mode acceptance.
- Seven equipment slots: Head, Torso, Legs, Feet, Accessory 1–3; backpack uses an accessory slot.
- Backpack owns an 8×6 grid by stable item-instance ID. B drops it; E recovers/re-equips the same item. Dropped cargo remains inaccessible.
- Three persistent 1×1 pockets: P1/P2 ordinary; P3 hidden from ordinary-search enumeration.
- Stack split/merge, pocket transfers and pocket-aware service integrations are implemented.
- Automatic debug starting backpack/cargo was removed in a later cleanup; F1 debug spawner remains. That separate cleanup still needs fresh Unity verification (empty new game and F1 spawner). This does not reopen P-03.7.
- Wearable presentation P-03.6 remains paused.

### P-04 — Character playability (active track)

**P-04a — Attributes/development:** complete, user-verified at 557/557.

**P-04b — Six-slot quickbar assignment and HUD:** complete, user-verified at **563/563**.
- Exactly six general-purpose slots; eligible carried items can be assigned, including consumables, tools, weapons and backpacks.
- Quickbar stores stable item-instance references only; it never owns/copies physical items. A physical item can appear in at most one slot.
- Eligible locations: accessible backpack grid or any of three personal pockets. Equipped clothing/accessories are excluded.
- Tab inventory no longer renders a duplicate quickbar row. While Tab is open, the single bottom HUD quickbar is the drag/drop surface: inventory/pocket→HUD slot assigns or replaces, HUD slot→HUD slot swaps, and HUD slot→inventory unassigns. RMB Inspect / Equip to Quick Slot 1–6 remains available.
- RMB reassignment moves an existing reference to the chosen slot; it does not duplicate it.
- Dragging shows a semi-transparent item ghost following the pointer.
- The only six-slot bar is the always-visible bottom-centre HUD quickbar; it remains the drag/drop target while Tab inventory is open.
- If backpack is dropped, references to its contents persist but are disabled until that same backpack is recovered. Pocket references remain accessible.
- Save/load persists the six assignments and reconciles invalid/retired identities.
- User confirmed **563/563** after these changes. Play Mode acceptance should still cover the visible HUD and interactions as part of the next in-game check if not already completed.

**P-04c — Active hand and controls:** complete and user-verified at 571/571 plus Play Mode acceptance.
- Core `PlayerQuickbar` now owns selected-slot and holstered state. The held item is derived from the selected slot's existing stable item-instance reference; no duplicate item ID or inventory ownership is introduced.
- Mouse wheel up/down cycles next/previous through all six slots with wraparound, including empty slots. Number keys 1–6 directly select their matching slots.
- H toggles between holstered and unholstered while preserving the selected slot and assignment. Selecting an empty slot means empty hand.
- Wheel, number keys and H are ignored while any supported gameplay/menu panel is open.
- HUD slot clicks remain non-interactive; while Tab is open, the HUD slots accept drag/drop assignment, replacement, swapping and unassignment. Selected/active and selected/holstered states receive distinct feedback.
- If a backpack-backed held item becomes inaccessible when its backpack is dropped, the hand holsters while its quickbar reference remains assigned. Pocket-backed items remain accessible.
- Loading resets the active hand to no selected slot and holstered while restoring the six assignments.
- Seven new Core regression tests cover active selection/holster, reversible H toggling, empty-slot cycling/wraparound, invalidation/unassignment, backpack-style inaccessibility and load reset.
- Source commits: `c17a9f7e27d06caf6066d5fd010594d51d33fd85`, `3f8157afb5ca80e744ed83d9ea25922b33f99e50`, `e0ea41bdf2b11a30b4a99941f7153979fd04223e`.
- User verified the full P-04c EditMode suite at 571/571 and confirmed the active-hand/quickbar Play Mode behavior. Subsequent storage registry, allocator and backpack persistence tests are now user-verified at 580/580.

**P-04d — UI polish:** later follow-up after P-04c. The always-visible six-slot HUD and drag ghost have already been delivered in P-04b follow-up; do not duplicate them.

## Other tracks

- **B-01 to B-06 livelihood foundations:** implemented; B-06 user-verified at 456/456 plus Play Mode. Reuse rather than rebuild.
- **B-07 Neighbourhood Expansion:** paused while P-04 remains active.
- **W-01 Weather & Cold Exposure:** separate inactive future track. Heat Protection mitigates cold only; no overheating system.
- Crime expansion, broader city systems and additional social content remain later work.

## Reusable foundations — do not rebuild

| Area | Existing authority |
|---|---|
| Inventory | Spatial grid, stable stack/item GUIDs, item state, three pockets, portable backpack lifecycle |
| Ownership/economy | Ownership vs custody, atomic asset transfers, CARD accounts, service-payment ledger |
| Workshop | Actor-aware repair/production, authored recipes, one-active customer job |
| NPCs | Shared social world, local perception, individual knowledge, routines, directed relationships |
| Player | Attributes, skills, milestone ledger, equipment loadout, quickbar assignment and save mapping |

## Guardrails

- One active gameplay initiative at a time; complete a coherent player-facing loop.
- New gameplay mechanics must be explained and explicitly approved before implementation.
- Core remains plain C#; Game owns Unity integration and authored content.
- Reuse existing inventory, ownership, transaction, account and save authorities.
- Persistent state requires current-build F5/F9 round-trip coverage.
- No new Unity packages/dependencies without approval.
- Do not author new story cast/backstories until the main storyline/cast are designed.
- Art pause remains in force: no new sprites, wearable overlays, wardrobe presentation or character-art exploration.
- Do not run `dotnet` or `msbuild` for this Unity project.
- Historical detail belongs in [the November log](progress/LOG-2026-11.md); keep this status board concise.


### Debug test loadout (2026-11-27)
- F1 Item Spawner now includes a one-click **TEST LOADOUT** action; it is opt-in and does not change fresh-session starting inventory.
- It equips a Canvas Backpack if the player has none, then adds Bread ×5, Fabric Roll ×5, Thread Spool ×3, one Work Jacket and one Basic Patch Kit through the normal debug spawn/inventory path. Existing equipped backpacks are reused. Current known defect: a dropped backpack still occupies the single session-wide inventory grid and blocks a replacement backpack; per-backpack storage isolation is the next fix.
- Stack quantities are added one item at a time through normal inventory rules, so expected stack groupings are Bread 5, Fabric Roll 3+2, Thread Spool 2+1. This deliberately gives both stack top-up and multi-stack cases.
- No quickbar assignments are overwritten. This loadout only seeds inventory; it does not implement consumable activation/use. Item-use remains a separate gameplay slice.
- Added an EditMode callback test for the loadout action. User has since confirmed **571/571** for the quickbar/H-toggle work. The drag-layer follow-up and backpack lifecycle correction still require Unity verification.
- Commits: `c70907a557d48b16c5b9fb0e857693481b27d341`, `c3a9f4cf05b7afa316727c6a60765a24bf556fd7`.


### Follow-up bug report (2026-11-27)
- User confirmed the previous quickbar/H-toggle changes at **571/571**.
- Drag ghost now promotes itself above all HUD siblings while dragging (commit `d980ebba7d53a3ffbfb602b74adb9593b1e652e4`). Needs Unity verification.
- Backpack lifecycle defect identified: all backpacks currently share the single `_inventory` grid; dropping unregisters it but does not detach the session from that grid, and the debug loadout blocks replacement while a bag is dropped. Next correction is per-backpack grids, active-grid switching, independent dropped storage and save/load support. User approved automatic swap: the currently equipped bag drops at the player's feet and the recovered bag equips.


### Backpack isolation + drag-layer correction (2026-11-27)
- User approved automatic backpack swapping: equipping a dropped backpack while another is equipped drops the currently equipped backpack at the player's feet, equips the recovered backpack, and keeps each backpack's contents independent. The debug TEST LOADOUT must be able to equip a fresh backpack while another is dropped.
- Historical diagnosis at the start of this initiative: GameSession stored one `_droppedBackpackItem`, aliased every backpack to `_inventory`, and left the dropped grid active. The active-grid alias and access-detachment defects have since been corrected; multiple dropped-bag tracking and automatic swap remain open.
- Fixed the quickbar drag ghost layering by moving it to a dedicated override-sorting Canvas (sorting order 200), above the inventory/HUD canvas (100). Commit: `1971e800a08011e8f031ecd79fdd67cb56d3a17d`.
- Backpack isolation/swap, multi-dropped-backpack tracking, save/load of each backpack's own contents, and debug-loadout behavior remain in progress; do not mark complete until implemented and Unity-verified. Last confirmed EditMode suite: 571/571.


### Cross-world inventory ownership invariant (2026-11-27)
The user clarified that the current backpack defect is one symptom of a broader architecture requirement: **all inventories, containers and NPC pockets must be independent from one another**. This is now the governing invariant for the storage correction, not a backpack-only patch.

Every physical storage identity must own a unique mutable `GridInventory`: each backpack, player pocket, hideout storage, each world/searchable container, each NPC-owned pocket/inventory, each business stock inventory, and each contract/service escrow inventory. Access/custody changes must never share, retarget, merge or duplicate backing grids. Save/load must persist contents by stable storage identity.

Audit findings from `GameSession.cs`:
- Player backpack storage is currently initialized as one session-wide `_inventory` and backpack registration currently passes that same instance to `PortableStorageBook.TryRegister`; this defeats per-backpack isolation even though the Core data model describes a backpack-to-grid mapping.
- Other distinct inventories are instantiated for player home storage, player pockets, contact inventory, customer contract receiver inventory, and individual searchable containers. Searchable containers are created per container ID. These are separate instances at construction, but the full ownership/access/save audit must verify that no later registration or restore path aliases them.
- Current save data now stores backpack contents by physical backpack GUID, with legacy `items` reserved for staging. Dropped-item metadata still has a single-backpack compatibility field; multi-dropped-bag metadata is the next schema increment.
- NPC pockets are a required independent storage domain; audit whether a concrete NPC inventory currently exists or whether it is a future domain before claiming implementation.

Required regression suite: pairwise isolation between representative player/backpack/pocket/world-container/NPC/business/escrow inventories; explicit transfer-only movement; multiple backpacks with independent contents; drop/recover automatic swap; multiple dropped bags; save/load preserving every storage identity and its contents; legacy-save migration; no inaccessible inventory exposed through UI, services or quickbar.

### Storage identity registry foundation (2026-11-27)
- Added Core `InventoryStorageId` and `InventoryStorageRegistry`: stable typed identities map to one `GridInventory`, with reverse lookup and hard rejection of duplicate identities or grid aliasing. Registry entries remain for the session lifetime; access, item ownership and economic custody are intentionally separate.
- Registered current independent runtime domains in `GameSession`: all three player pockets, hideout storage, dirty-cash contact inventory, the first customer contract/repair receiver inventory, every street searchable container, and every resident-owned searchable property container.
- Added five Core regression tests for identity stability, duplicate/alias rejection, reverse lookup, and isolation between separate player/backpack/world test grids.
- Source commits: `cd4b182015525429b1ebc97c22628a862b28ef89`, `bf4d54e9be82dfd61760528a11a1f6a3186f93d8`, `13ff69d98d661cd0bfeecff8ee9c9bb3a8cbf50a`, `bea52619e72a6ebe0f5e0ac4b50d3e4a4c481585`, `ffa503494a62593e0ffe99d4e57f06a569b57387`.
- Follow-up `GameSession` commit `53e88a8a3d281469b484f4ef5e2d76b495bb237d` now registers the currently equipped/restored/dropped backpack grid under `PlayerBackpack/<instance-guid>/contents`, and verifies that the registered identity resolves to the same grid before granting economic access. The registry mapping remains while the bag is dropped.
- Runtime now allocates a distinct registry-backed grid for each equipped physical backpack, switches `_inventory` to that grid, and detaches to the staging grid on drop. F5/F9 now capture/restore per-backpack storage records, with deterministic migration of the legacy single-bag global `items` grid. Multiple dropped backpack metadata/visuals, automatic swap, business stock and NPC-owned pocket runtime inventories remain open. Do not mark the global invariant complete.
- User confirmed **576/576** after registry integration, then **578/578** after the per-backpack allocator, and most recently **580/580** after identity-keyed backpack persistence tests. Runtime Play Mode verification of the new backpack identity registration and the drag overlay fix remains pending.

### Per-backpack storage allocation — Core step (2026-11-27)
- Extended `PortableStorageBook` with a registry-backed registration overload. It creates a new `GridInventory` for each physical backpack GUID, derives the canonical `PlayerBackpack/<GUID>/contents` identity, and only commits the storage-book entry when the registry accepts the unique grid.
- Added `GetStorageIdentity(Guid)` so runtime and save/load code share one canonical identity format rather than reconstructing it independently.
- Added two EditMode tests: two backpacks receive different grids with bidirectional content isolation; duplicate registration leaves both book and registry unchanged.
- Commits: `bd9ed8c515265120e6d5ad98e26bc1007fe9cd56`, `7d374b16991da37d864999c9f560ba7a0fe5b704`.
- The registry-backed allocator is now wired into GameSession for the currently equipped bag. The compatibility overload remains only for deterministic migration of legacy saves. Multiple dropped physical items/visuals and automatic swap remain open.
- Expected EditMode count increase: +2 (576 to 578). Unity compile, EditMode and Play Mode verification are pending.



### Per-backpack persistence primitives — Core step (2026-11-27)
- Added PortableBackpackStorageSaveMapper to capture each physical backpack's contents under its stable backpack instance GUID, including grid dimensions and each item's stable identity/state.
- Restore allocates a distinct grid per saved backpack, registers it under PlayerBackpack/<GUID>/contents, rejects duplicate backpack identities, and uses a caller-supplied shared item-ID set to reject duplicate physical item identities across storage domains.
- Added precise registry unregistration and PortableStorageBook.Clear(registry) so a future restore can remove stale backpack identity bindings without clearing unrelated storage registrations. Added a storage-entry snapshot API for save capture.
- Added two EditMode regression tests: independent two-backpack round-trip with dimensions and contents preserved; duplicate backpack/item identity rejection.
- Commits: 2462112a32e88de815f53b0305ce3b4cf1bf63e5, 8156a03390ab1d2184f813530f34b229e783d7cb, 8ff1da85acca6de1b727f2d9dcd734eea709285d, 68eb632cbd4f46b85dbe88751335c2cdd6aa5369, 34330e52abf2c30b7b95314565284eda135db6c4, 3e45aa26826db7c91ba2baf1edc341acac8387f8.
- The persistence mapper is now called by GameSession F5/F9. The legacy `items` grid is reserved for the empty staging inventory, while each backpack's contents are captured under its own physical GUID. Legacy single-backpack saves migrate their old global items grid into the equipped/dropped backpack's private grid. Runtime still tracks one dropped-backpack item/visual; automatic swap and debug-loadout changes remain open.
- User has since confirmed **580/580** after these two persistence tests. GameSession active-grid and F5/F9 integration have since been implemented; Play Mode verification and multi-dropped-bag support remain open.


### Current active storage implementation step (2026-11-27)
- **Verified:** user confirmed 580/580 after the identity-keyed backpack save/restore mapper and duplicate-ID regression tests.
- **Completed in the current source step:** active-grid switching, immediate detachment on drop, rebinding InventoryPanel/Storage/Retail/Dirty Cash/Customer/Workbench/Milo consumers, identity-keyed F5/F9 persistence and deterministic legacy single-bag migration. **Next:** multi-dropped-bag metadata/visuals and the approved automatic swap.
- Current runtime limitation: GameSession now uses one independent grid per physical backpack and detaches the active reference on drop, but still tracks only one dropped backpack. NPC pocket and business stock runtime inventories remain future work. Do not claim the global storage invariant complete.
- Drag overlay and backpack runtime behavior still require fresh Unity Play Mode verification.


### GameSession per-backpack active-grid integration (2026-11-27)
- Equipped backpacks now receive their own registry-backed `GridInventory` from `PortableStorageBook`; the session's active `_inventory` reference switches to that grid only after allocation and registration succeed.
- Dropping immediately switches the player-facing inventory reference to the separate staging grid. The dropped bag's original grid remains registered to its physical GUID and inaccessible through carry panels/services.
- Rebound `InventoryPanel`, `StoragePanel`, `RetailPanel`, `DirtyCashContactPanel`, `CustomerContractPanel`, `WorkbenchWorkspace` and `MiloParcelOpportunityController` when the active grid changes.
- Quickbar validation now searches all physical backpack grids so assignments to dropped-bag contents remain assigned; active-hand access still checks only accessible carried inventories and therefore holsters correctly.
- F5/F9 now serialize per-backpack storage records separately from the legacy staging `items` grid. Load restores the active equipped bag's private grid; the legacy single-bag global grid migrates deterministically to the equipped or dropped backpack. Before backpack restore, item GUIDs are collected from staging, hideout, contact, contract escrow, pockets and searchable containers so duplicate physical identities across those domains are rejected. This does not yet migrate multiple dropped bags because runtime still tracks one dropped item.
- Added one regression test proving backpack registry cleanup preserves unrelated storage identities and permits the same bag identity to be registered again. Expected full EditMode suite: **581**; Unity verification pending. Last user-confirmed suite remains **580/580**.
- Commits: `3ef8447809dba93b58b67f210b8bdd9287f17b54`, `270d4427fb84a9ae7822c6bb34590403758a150a`, `7aa3f78c6e7ae7faf2872de91188e50da4f1fb61`, `cb2b3fe6a8cee57b258906601cc0fda1010e4d15`, `523b7199c3506f4de2609b0d196091c943bf6238`, `e0e44acb1cf06333b6f77cf9ab31e93b34af5245`, `24d34d773b8e6287103844ac0641f9299a95329a`, `c359058b1e47b3c87c76f3be0599d789fb198946`.
- Required verification: Unity compile + 581 EditMode tests; Play Mode equip/drop/recover, quickbar persistence, panel rebinding and F5/F9 round trips. No result is claimed yet.
