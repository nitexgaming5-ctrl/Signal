# Signal — current agent handoff

**Last reconciled:** 2026-11-27  
**Repository:** `nitexgaming5-ctrl/Signal`  
**Working branch:** `chore/collaboration-workflow-and-audit`  
**Do not modify:** `main`

Signal is a Unity 2D top-down urban RPG. Unity 6000.6.9f1; active scene `Assets/_Project/Scenes/HideoutSlice.unity`; architecture is `Signal.Core` (plain C# domain) + `Signal.Game` (Unity integration/UI/content).

**Latest user-confirmed full EditMode suite:** 583/583 (2026-11-27). Unity verification remains authoritative for all subsequent changes.

**Current small integration task:** user-supplied three-character city NPC sprite import is wired in code; the user must run `Signal → Import NPC Rendering Test Sprites...` with `TEST.zip`, then verify Unity compilation and Play Mode.

## Read first

1. `AGENTS.md` — authority and workflow.
2. `docs/INDEX.md` — documentation map.
3. `docs/PROGRESS.md` — concise implementation and verification truth.
4. `docs/ROADMAP.md` — active sequence and next proposal.
5. The relevant task card and specialist references.

Historical chronology belongs in `docs/progress/LOG-2026-11.md`; do not read it wholesale unless needed.

## Direction and workflow

The user selected **Direction B — meaningful everyday livelihood**: repair, production, customers, services, relationships and a practical hideout. Character development was reprioritised ahead of neighbourhood expansion. Keep one active initiative at a time and avoid isolated systems.

**Gameplay mechanics must be explained and explicitly approved before implementation.** User expects substantial coherent work after a green light, accurate documentation, and no false test claims. Keep `docs/PROGRESS.md`, `docs/ROADMAP.md`, `docs/PROJECT_HANDOFF.md`, the active task card and monthly log reconciled.

## Verification snapshot

- **Latest full EditMode suite: 580/580 passed**, user-confirmed 2026-11-27 after P-04b usability follow-up.
- P-04a: 557/557 confirmed.
- P-04b initial assignment foundation: 562/562 confirmed; usability follow-up: 563/563 confirmed.
- P-03.7: 555/555 plus Unity compile and Play Mode acceptance confirmed; closed.
- B-06: 456/456 plus successful Play Mode repair/return/payment loop confirmed.
- Unity Editor is authoritative for compile, EditMode and Play Mode. Do not infer Play Mode acceptance for P-04b solely from the test count.

## Current implementation

### Player attributes / P-04a

Exactly three attributes: Health (primary/max HP), Armor and Heat Protection. Current HP remains separate in PlayerStats. F2 opens the Character panel; HUD shows current/max HP. Effective attributes combine equipment and skill modifiers. Temporary test fixtures only: `health_test` (+10 max Health/rank) and `heat_protection_test` (+1 Heat Protection/rank). Actual Repair/Production/Trading/Scavenging/Social effects and comprehensive milestone trigger wiring remain future work.

### Equipment / P-03

Seven slots: Head, Torso, Legs, Feet and Accessory 1–3; backpack uses an accessory slot. Backpack owns an 8×6 grid by stable item ID; B drops it and E recovers/re-equips the same item. Three persistent 1×1 pockets exist, with P3 hidden from ordinary-search enumeration. Stack split/merge and pocket-aware service integrations are in place. P-03.7 is closed at 555/555 with Play Mode acceptance.

A later cleanup removed automatic fresh-session debug backpack/cargo; F1 spawner remains. That cleanup still needs a separate fresh Unity verification (empty new game and F1 spawner). Do not reopen P-03.7. Wearable presentation is paused.

### Quickbar / P-04b — verified

- Six general-purpose slots, stable item-instance references only; never duplicate/copy physical items.
- One physical item can appear in at most one slot.
- Assignable from accessible backpack grid or any personal pocket; equipped clothing/accessories excluded.
- Tab inventory has no duplicate quickbar row. The single bottom HUD quickbar is the drag/drop target while Tab is open: inventory/pocket→HUD slot assigns/replaces, HUD slot→HUD slot swaps, and HUD slot→inventory unassigns. RMB Inspect / Equip to Quick Slot 1–6 remains.
- RMB reassignment moves the existing reference to the new slot.
- Semi-transparent pointer-following drag ghost.
- The only quickbar display is the always-visible bottom-centre six-slot HUD bar; it accepts drag/drop while Tab is open.
- Dropped-backpack references persist but are unavailable until the same backpack is recovered; pocket entries remain available.
- Save/load mapping persists and reconciles assignments.
- User confirmed **563/563** after these changes.

### P-04c active hand and controls — implementation committed, verification pending

- Core `PlayerQuickbar` owns selected-slot and holstered state. The held item is derived from the selected slot's existing GUID; inventory remains ownership authority.
- Mouse wheel up/down cycles next/previous through all six positions, including empty slots, with wraparound.
- Number keys 1–6 directly select slots 1–6. H toggles between holstered and unholstered without clearing the selected slot or assignment. Selecting an empty slot means empty hand.
- All three controls are ignored while any supported menu/panel is open.
- HUD slot clicks remain non-interactive, but while Tab is open the HUD slots are the drag/drop assignment surface. The HUD highlights the selected slot differently when active versus holstered.
- If a held item in the backpack grid becomes inaccessible because the backpack is dropped, the active hand holsters but the slot assignment remains. Pocket items remain accessible.
- F9/load resets the active hand to no selected slot and holstered; six assignments are restored as before.
- No consumable use, tool action, weapon attack, combat, targeting, durability, or new art is included.
- Seven Core P-04c regression tests plus one debug-loadout callback test have been added since the last user-confirmed 563/563 baseline. Expected current EditMode total: 571; verify in Unity rather than assuming it.
- Source commits: `c17a9f7e27d06caf6066d5fd010594d51d33fd85`, `3f8157afb5ca80e744ed83d9ea25922b33f99e50`, `e0ea41bdf2b11a30b4a99941f7153979fd04223e`.
- Next action: run Unity compile, all EditMode tests and the Play Mode acceptance checklist in the task card; do not mark P-04c complete until those pass.

## Project-wide constraints

- Core must not reference UnityEngine; dependencies flow Game → Core.
- Reuse canonical inventory, ownership, custody, transaction, account and save authorities.
- Preserve stable ScriptableObject IDs, item GUIDs and Unity .meta GUIDs.
- No new packages/dependencies without approval.
- Do not author new story cast/backstories until the main storyline/cast are designed.
- Art pause remains active: no new sprites, wearable overlays, wardrobe presentation or character-art exploration.
- Do not run `dotnet` or `msbuild` for this Unity project.
- B-07 Neighbourhood Expansion is paused while P-04 is active.


### Debug test loadout (2026-11-27)
- F1 Item Spawner now includes a one-click **TEST LOADOUT** action; it is opt-in and does not change fresh-session starting inventory.
- It equips a Canvas Backpack if the player has none, then adds Bread ×5, Fabric Roll ×5, Thread Spool ×3, one Work Jacket and one Basic Patch Kit through the normal debug spawn/inventory path. Existing equipped backpacks are reused. Known defect: a dropped backpack still occupies the single session-wide grid and blocks replacement; per-backpack storage isolation is pending.
- Stack quantities are added one item at a time through normal inventory rules, so expected stack groupings are Bread 5, Fabric Roll 3+2, Thread Spool 2+1. This deliberately gives both stack top-up and multi-stack cases.
- No quickbar assignments are overwritten. This loadout only seeds inventory; it does not implement consumable activation/use. Item-use remains a separate gameplay slice.
- Added an EditMode callback test for the loadout action. Unity compilation, tests and Play Mode verification are pending.
- Commits: `c70907a557d48b16c5b9fb0e857693481b27d341`, `c3a9f4cf05b7afa316727c6a60765a24bf556fd7`.


### Current follow-up — 2026-11-27
- User confirmed the quickbar/H-toggle work at **571/571**.
- Drag ghost z-order follow-up committed as `d980ebba7d53a3ffbfb602b74adb9593b1e652e4`: it is promoted to the last sibling on the overlay canvas during dragging so it appears above the HUD quickbar. Unity verification pending.
- Backpack bug diagnosis: `GameSession` uses one `_inventory` grid for every physical backpack. Dropping unregisters the grid but keeps the same reference as the session inventory; debug loadout refuses a replacement while a dropped bag exists. Do not merely remove that guard. Correct design is one distinct grid per backpack identity, active-grid switching on equip/drop, multiple independently recoverable dropped bags, and save/load persistence for every bag.
- Await user decision: if the player recovers an old dropped bag while wearing another, should this automatically drop the currently worn bag at the player's feet and swap, or require the player to drop the current bag first?
- No backpack lifecycle correction has been implemented yet; the storage identity registry foundation is now in place. Preserve the confirmed 571/571 baseline and re-run Unity tests after the pending fixes.


## Active follow-up — backpack isolation and swap (2026-11-27)
User approved automatic swap: when near dropped Backpack A while Backpack B is equipped, pressing E drops B at the player's feet and equips A; each backpack retains independent contents. Dropping a backpack must immediately revoke access to its contents. The debug TEST LOADOUT must be able to equip a fresh backpack while another is dropped. Current code still has a single `_droppedBackpackItem` and maps every backpack to the same `_inventory`; debug loadout blocks while one is dropped. Do not remove that guard without introducing independent per-instance storage and persistence. The quickbar drag ghost was moved to a dedicated override-sorting Canvas (sorting order 200) in commit `1971e800a08011e8f031ecd79fdd67cb56d3a17d`. Backpack correction remains open and requires save/load coverage plus Unity verification; last user-confirmed suite is 571/571.


## Global inventory isolation requirement (2026-11-27)
The user clarified that the approved backpack correction must enforce a broader invariant: **every inventory, world container and NPC pocket owns independent contents**. Treat this as a cross-cutting storage architecture task, not a backpack-only patch.

Required storage domains: each physical backpack (stable item-instance ID), each player pocket, hideout/home storage, each searchable/world container, each NPC/resident-owned pocket or carried inventory, each shop/business stock inventory, and each contract/service escrow inventory. One storage identity must map to one unique mutable `GridInventory` instance. Access/custody is separate from ownership; dropping or switching access must never retarget, merge, clear or duplicate contents. Cross-inventory movement only occurs through explicit validated atomic transfer operations.

Audit note: GameSession currently registers the same session-wide `_inventory` grid for a backpack, so the apparent PortableStorageBook mapping is not sufficient for independent backpack storage. GameSession also creates separate player storage, pockets, contact inventory, contract receiver inventory and per-searchable-container grids; all registration, service, UI, save/load and stack-lineage paths still need an aliasing audit. NPC pockets are a required domain; verify whether they currently exist as runtime inventories before describing them as implemented.

Persistence must be keyed by stable storage identity, with deterministic migration for legacy global/single-backpack save fields. Regression tests must check pairwise isolation and explicit transfers across player, backpack, pocket, world container, NPC, business and escrow inventories.

### 2026-11-27 — Storage identity registry foundation
- Added `InventoryStorageId` + `InventoryStorageRegistry` in Core. One stable typed storage identity maps to exactly one unique `GridInventory`; duplicate identities and shared grid instances are rejected. Reverse lookup supports auditing. Registry identity is separate from access, item ownership and economic custody.
- GameSession now registers the three player pockets, hideout storage, dirty-cash contact inventory, customer receiver/escrow inventory, all street searchable containers and all resident-owned searchable property containers.
- Added five EditMode Core tests covering stable IDs, duplicate/alias rejection and cross-grid isolation.
- Commits: `cd4b182015525429b1ebc97c22628a862b28ef89`, `bf4d54e9be82dfd61760528a11a1f6a3186f93d8`, `13ff69d98d661cd0bfeecff8ee9c9bb3a8cbf50a`, `bea52619e72a6ebe0f5e0ac4b50d3e4a4c481585`, `ffa503494a62593e0ffe99d4e57f06a569b57387`.
- Follow-up commit `53e88a8a3d281469b484f4ef5e2d76b495bb237d` registers the current backpack grid under its stable `PlayerBackpack/<GUID>/contents` identity and checks identity-to-grid consistency before economic access. The identity persists while dropped.
- Runtime now allocates a distinct registry-backed grid per equipped physical backpack, switches the active inventory reference, detaches on drop, rebinds all inventory panels/services, and persists per-backpack storage records through F5/F9. Still incomplete: multiple dropped backpack item/visual tracking, automatic swap, business stock inventories and NPC-owned pocket runtime inventories. Keep the TEST LOADOUT dropped-bag guard until multi-bag swapping is complete.
- User confirmed **576/576 EditMode tests** after this integration and the preceding compile correction. Play Mode verification remains pending; check startup logs for duplicate/invalid storage identity errors.
- Next Core increment: `PortableStorageBook.TryRegister(Guid, width, height, InventoryStorageRegistry)` now allocates a distinct grid per physical backpack and binds it to the canonical stable identity. `GetStorageIdentity(Guid)` centralizes identity construction. Two regression tests cover distinct grids, bidirectional content isolation and duplicate-registration rollback. Commits: `bd9ed8c515265120e6d5ad98e26bc1007fe9cd56`, `7d374b16991da37d864999c9f560ba7a0fe5b704`.
- The allocator is now wired into GameSession for normal equip/drop/recovery. The compatibility overload remains for deterministic migration of legacy single-bag saves. F5/F9 use the per-backpack mapper; multi-dropped-bag state and automatic swap remain open. Latest user-confirmed baseline: 580/580; the newest registry-cleanup test brings the expected total to 581, pending Unity verification.



## 2026-11-27 — Per-backpack persistence Core primitives

- Added SavedPortableBackpackStorage records and PortableBackpackStorageSaveMapper to capture/restore one independent grid per physical backpack GUID, including dimensions, item instance IDs, placement and item state.
- Restore binds each newly allocated grid to PlayerBackpack/<GUID>/contents in InventoryStorageRegistry. Duplicate backpack IDs and duplicate item instance IDs (using a shared caller-owned set) are skipped; separate backpack grids remain isolated.
- Added InventoryStorageRegistry.TryUnregister and PortableStorageBook.Clear(registry) to remove stale backpack bindings without clearing unrelated registry domains.
- Added two regression tests for independent backpack round-trip and duplicate identity rejection. User confirmed **580/580**. GameSession active-grid and F5/F9 integration have since been implemented; runtime Play Mode verification and multi-dropped-bag support remain pending.
- Commits: 2462112a32e88de815f53b0305ce3b4cf1bf63e5, 8156a03390ab1d2184f813530f34b229e783d7cb, 8ff1da85acca6de1b727f2d9dcd734eea709285d, 68eb632cbd4f46b85dbe88751335c2cdd6aa5369, 34330e52abf2c30b7b95314565284eda135db6c4, 3e45aa26826db7c91ba2baf1edc341acac8387f8.
- Historical note, superseded by the later active-grid integration below: at this earlier point the mapper was not yet called by GameSession. It is now wired to F5/F9 with a separate staging inventory to avoid double serialization.


## Current next implementation — multi-dropped-backpack lifecycle (2026-11-27)

- Verified latest EditMode suite: **580/580**, user-confirmed.
- Core and GameSession now use distinct backpack grids and identity-keyed save/restore. The active inventory reference switches on equip/recovery and detaches on drop. Runtime still tracks one dropped-backpack item/visual.
- Next slice: replace the single dropped-backpack item/visual with GUID-keyed collections, persist full metadata for every dropped bag, and implement the approved automatic swap (drop currently equipped bag at the player's feet, then equip the recovered bag). Preserve every bag's private grid and allow TEST LOADOUT to equip another bag while previous bags remain dropped.
- The active grid no longer aliases the legacy `data.items` save path: F5/F9 reserve that path for staging and use per-backpack storage records for bag contents. Unity compile/Play Mode verification is pending.
- User must run Unity compilation and EditMode tests, then Play Mode acceptance for upcoming runtime changes.


## GameSession active-grid integration — committed, verification pending (2026-11-27)

- Equipped backpacks allocate and activate a distinct registry-backed grid; dropping switches the active player inventory reference to the staging grid while the dropped bag retains its own inaccessible contents.
- InventoryPanel, StoragePanel, RetailPanel, DirtyCashContactPanel, CustomerContractPanel, WorkbenchWorkspace and MiloParcelOpportunityController now rebind when the active grid changes.
- Quickbar identity validation searches all registered backpack grids so dropped-bag assignments persist, while active-hand access remains limited to accessible grids.
- F5/F9 now call `PortableBackpackStorageSaveMapper`; the staging inventory is passed to the legacy SaveMapper path to avoid double serialization. Legacy single-backpack global items migrate into the relevant bag. Restore collects existing item GUIDs from staging, hideout, contact, contract escrow, pockets and searchable containers before restoring backpack grids, rejecting cross-domain duplicate physical IDs. Missing/invalid per-bag records fall back to attaching the legacy staging grid to the affected bag rather than leaving it without contents.
- Added one registry cleanup regression test. Expected suite count 581; latest user-confirmed result is now 581/581.
- Source commits: `3ef8447809dba93b58b67f210b8bdd9287f17b54`, `270d4427fb84a9ae7822c6bb34590403758a150a`, `7aa3f78c6e7ae7faf2872de91188e50da4f1fb61`, `cb2b3fe6a8cee57b258906601cc0fda1010e4d15`, `523b7199c3506f4de2609b0d196091c943bf6238`, `e0e44acb1cf06333b6f77cf9ab31e93b34af5245`, `24d34d773b8e6287103844ac0641f9299a95329a`, `c359058b1e47b3c87c76f3be0599d789fb198946`, `2b4edd2bdbfbe000746859554ce96df604201e3d`.


### Multi-backpack proximity foundation — 2026-11-27
- `PortableBackpackWorldBook.TryGetNearestDropped` now chooses the closest dropped bag within a supplied radius and uses stable GUID tie-breaking for equal distances. Invalid positions/radii are rejected.
- Two regression tests cover nearest/range behavior and deterministic tie-breaking.
- This does not yet make GameSession multi-bag capable: `_droppedBackpackItem` and `_droppedBackpackVisual` remain singular. Next: GUID-keyed dropped-item/visual collections, multi-bag save metadata, automatic swap, then TEST LOADOUT unlock.
- Commits: `0781acd6e90d7197cef8581c7a87610d8598a2b2`, `66db3032d0af00dd50d389fb7ac5413076c3f19a`.
- Expected EditMode suite: 583; latest user-confirmed baseline: 581/581.


## 2026-11-28 — User-supplied city NPC sprite integration

- The user supplied `TEST.zip` with three composed character animation exports. Each character provides 3 idle and 4 walk frames for Down/Left/Right/Up, at 128×160 per frame.
- Updated the editor importer to build the three Resources sheets directly from the archive's `Frames` directories, with strict path/frame/dimension validation. Updated the runtime visual to animate the actual source frame counts and align the foot baseline; it remains attached to the existing three randomized street-only test residents.
- No NPC gameplay, identity, schedule, route or movement logic was changed. This imports only the composed frame images; source layer exports are not needed at runtime.
- Commits: `658793768936308f83185767b0905588014e33a0`, `32abe8a9d38d596fc55568c1ef93228079cbb2a7`, `322227bdd0cefa185e4f11bf419acb1b9989d73d`.
- Unity verification pending. Run **Signal → Import NPC Rendering Test Sprites...**, select `TEST.zip`, then compile and enter Play Mode. Check relative scale, foot alignment, all four facings and animation playback.
