# Signal — project handoff

Signal is a top-down 2D urban RPG built on Unity 6000.6.9f1.

## Current entry points
- `AGENTS.md` — agent authority, architecture safeguards and verification rules.
- `docs/INDEX.md` — full game scope and documentation navigation.
- `docs/PROGRESS.md` — current repository state, validation and next action.
- `docs/ROADMAP.md` — Direction B roadmap plus parallel Player Development and future Weather tracks.
- `docs/ARCHITECTURE.md` and `docs/DATA_MODEL.md` — current architecture and planned domain boundaries.
- `docs/tasks/PLAYER-P-01-CHARACTER-FOUNDATION-APPEARANCE.md` — proposed modular player visual foundation.
- `docs/tasks/PLAYER-P-02-ATTRIBUTES-SKILLS.md` — approved attribute/skill progression direction.
- `docs/tasks/WEATHER-W-01-DYNAMIC-WEATHER-COLD-EXPOSURE-DESIGN.md` — separate future weather/cold-exposure design.
- `docs/tasks/LIVELIHOOD-B-06-CUSTOMER-REPAIR-ORDERS.md` — completed and user-verified B-06 slice.

## Current verified baseline
- Working branch: `chore/collaboration-workflow-and-audit`; keep `main` untouched.
- Active scene: `Assets/_Project/Scenes/HideoutSlice.unity`.
- User confirmed **500/500 EditMode tests passing** after direct pocket-to-pocket moves/swaps (2026-11-26). P-03.5 source changes since that checkpoint cover Retail SELL, Dirty Cash selection, Home Storage, Searchable Container pocket display, workbench repair and multi-grid production across accessible pockets/backpack; three targeted EditMode tests were added, but these changes are not yet Unity-verified. Play Mode acceptance remains outstanding.
- B-06 Customer Repair Orders is complete: entrusted item custody remains distinct from ownership; exact item return and 35 CARD payment are atomic and replay-protected.
- B-04 repair/trading and the First Workshop have also been reported working in Play Mode.
- Unity remains the authority for compile, test and Play Mode verification. Never infer a pass from source edits.

## Player Development — P-01/P-02 implemented; P-03 active
The user chose the Player Development track before returning to B-07. B-07 remains Neighbourhood Expansion. P-03 now includes the physical backpack drop/recovery loop and three real personal pockets.

Confirmed: existing player character, fixed starting skill-point budget, milestone-earned points, free respecialization, exactly Armor and Heat Protection attributes, and cold-only environmental meaning for Heat Protection. P-01/P-02 first implementation is committed and user-verified at 467/467. The modular Body_A character pack is imported locally; P-03 equipment is the active next phase.

Candidate skills (Repair, Production, Trading, Scavenging, Social), starting budget amount, rank caps/costs and milestone rewards remain provisional/open. Activity-specific skill effects and actual milestone triggers remain future integration.

## Separate future phase — W-01 Weather
Dynamic seasonal weather is planned: temperature changes, rain, storms, snow and blizzards. Player exposure consequences are cold-focused and gradual. Heat Protection mitigates cold exposure; shelter supports recovery. No hot-weather/overheating gameplay is planned. W-01 is separate from P-01/P-02 and is not authorized for implementation.

## Next action — P-03.5 pocket-aware service-panel integration
The user has confirmed **500/500 EditMode tests passing** on 2026-11-26. Resume the original P-03 Equipment & Backpack track. The current P-03.5 implementation already integrates Retail SELL, Dirty Cash selection, Home Storage, Searchable Container pocket display, workbench repair and multi-grid production. Continue auditing any remaining 8×6-backpack-only assumptions, preserve atomic transactions and stable item identity/ownership, and keep dropped-backpack cargo strictly inaccessible. See the P-03 task card for the complete remaining sequence.

Current implementation:
- Seven equipment slots are approved; Canvas Backpack occupies Accessory 1 and owns the 8×6 contents grid.
- B drops the equipped backpack; E recovers the same item. The dropped bag keeps its cargo and is inaccessible until recovered.
- Exactly three independent 1×1 personal pockets now exist, each holding one stack of a 1×1 item. P moves a hovered backpack item into the first free pocket; clicking a pocket item moves it back into the equipped backpack.
- F5/F9 persists pocket items separately from backpack contents. Pocket item identity, quantity, state and owner are restored.
- NPC conversations and customer-contract acceptance are available without a backpack. CustomerContractPanel can list and fulfil pocket-held goods. Physical item receipt routes to pockets for repair-order acceptance, shop purchase, searchable-container pickup and Milo's parcel.
- When no backpack is accessible, Retail and Dirty Cash panels use an empty carry view rather than exposing the dropped backpack's cargo. Pocket-held items are not yet listed in Retail SELL or Dirty Cash selection.
- Home-storage transfer and workbench/production still require an equipped backpack. Clothing sprite overlays and broader service-panel pocket support remain future P-03 work.
- The user-confirmed EditMode suite is 503/503. Remaining P-03 work: finish the direct-grid assumption audit, wearable visuals, remove temporary debug starting-loadout seeding, and full Play Mode acceptance. Keep B-07 and W-01 as separate roadmap tracks.


## Latest implementation — P-01/P-02 first slice (2026-11-25)

Committed but not yet Unity-verified:
- P-01: modular five-part Body_A player visual with Idle/Walk and Down/Side/Top direction handling. RoomBootstrap retains physics/movement and falls back to the teal placeholder until assets are imported.
- To import the supplied archive in Unity: use **Signal → Import World Build Character Pack...**, select `World Build - Character Pack - 0.2(1).zip`, then enter Play Mode. This imports PNG sheets into Resources and preserves the bundled terms. The pack provides a single modular base body, not a full clothing/armor catalogue.
- P-02: Armor/Heat Protection base attributes, pure equipment modifier seam, point-based skill ranks, one-time milestone ledger, free respecialization, SaveData mapping, provisional catalog and F2 debug panel.
- Current prototype catalog: 8 starting points, rank cap 5, 1 point per rank. Candidate skills remain provisional. Activity-specific effects are not yet wired.
- Eleven focused tests were added. Previous user-verified baseline was 456/456 before this change; expected current total is 467. Unity compile, full EditMode suite and Play Mode must be run before calling this verified.
- F2 opens Player Development when other major panels are closed. F1 remains the existing debug menu.


## P-03 first Core implementation slice (historical; superseded by the second slice below)

- Core seven-slot PlayerEquipmentLoadout is committed with exact item-instance transfer, slot uniqueness and effective Armor/Heat Protection aggregation.
- PortableStorageBook associates independent GridInventory contents with a stable backpack item instance ID; non-empty storage cannot be discarded.
- Nine focused EditMode tests were added. User's 467/467 verification predates these tests; expected total is 476 pending Unity execution.
- Game item metadata, GameSession composition, SaveData persistence, equipment/backpack UI, wearable visuals and physical world drop/pickup are not implemented yet. Do not describe P-03 as complete until these are integrated and tested.


## P-03 second implementation slice — backpack equipped (2026-11-25)

- User confirmed 476/476 after the Core equipment/backpack foundation. Two additional focused tests were added in this slice; expected current suite is 478, pending Unity verification.
- Temporary player grid expanded to 8×6 for testing. Final design: 2–3 small pockets without a backpack; current large tabbed grid belongs to the equipped backpack.
- ItemDefinition optional metadata and three Resources fixtures are added: Canvas Backpack (Accessory 1, 8×6), Padded Work Jacket (Armor +2 / Heat Protection +1), Insulated Coat (Armor +0 / Heat Protection +3). All values are provisional test tuning.
- Inventory: hover item + E to equip; number keys 1–7 unequip ordinary slots. Backpack cannot yet be unequipped; that action belongs to the physical drop/pickup phase.
- Equipped identity/state/owner/modifiers persist through F5/F9. The existing 8×6 grid is attached to the backpack's stable identity without copying contents. A placeholder silhouette appears on the character.
- Next: Unity verify, then implement physical backpack drop/pickup and the 2–3 pocket fallback. Do not mark P-03 complete yet.

## Latest implementation — P-03.3 backpack drop and recovery (2026-11-25)

- P-03.3 now includes GameSession wiring: **B** drops the equipped backpack at the player's position; **E** beside the dropped bag recovers and automatically equips the same item instance.
- The character/inventory screen now has a left equipment sidebar (seven slots, three visual pocket placeholders) and a right BACKPACK STORAGE grid. A clickable **DROP BACKPACK [B]** button is available while equipped.
- F5/F9 capture and restore the dropped item's stable identity/state/owner/modifiers and world coordinates; the ground placeholder is recreated after load.
- Dropping closes the inventory and hides/blocks interaction with the backpack grid. The three pockets are not functional storage yet, and every other item-based interaction still needs an active-inventory accessibility audit.
- One additional Core test was added. The user-confirmed baseline was 485/485; expected current suite is **486**, pending user Unity compile/EditMode run. No Play Mode verification is claimed.
- Next: pull, compile, run EditMode (expected 486), then execute the documented drop/recover/save-load test game. Fix any compile/test errors before further expansion. P-03 remains incomplete until real 2–3 pocket inventory, item interaction gating, clothing visuals and full Play Mode acceptance are complete.

### Dropped-storage access rule

- While the bag is dropped, item-driven E/F interactions are temporarily blocked so workbench/shop/container flows cannot access the backpack's still-registered 8×6 grid. World navigation remains available: E at building/hideout doors and exits still transitions locations, with the matching HUD prompt. Movement, pickup, F5/F9 and sleep remain available. Replace this conservative gate with pocket-aware inventory routing when the real pocket inventory is implemented.

- HUD prompt follow-up: the exact-string door allowlist missed alternate wording such as “E Exit building”. `IsWorldTransitionPrompt` now recognizes E-prefixed enter/exit/leave/return prompts case-insensitively. This corrects prompt visibility without allowing item-based interactions while the backpack is dropped. Unity verification is pending.

## Latest correction — dropped-backpack exit HUD and API warning (2026-11-25)
- User clarified that the building exit interaction worked while the backpack was dropped, but the HUD did not display “press E”. The latest GameSession change preserves the raw CityBlock transition prompt before nearby interaction text can override it, recognises enter/exit/leave/return wording without requiring an existing E prefix, and displays an explicit E hint.
- InventoryPanel now uses `FindAnyObjectByType<EventSystem>()` instead of the obsolete `FindObjectOfType<EventSystem>()` API.
- User confirmed **486/486 EditMode tests passing** after pulling the prior implementation. Because these two cleanup commits were made afterward, Unity recompilation, another 486-test run and Play Mode confirmation remain pending.


## P-03 fresh-start inventory cleanup — 2026-11-25
- New sessions no longer seed the player inventory from the configured item definitions. Those definitions remain the catalogue source.
- The temporary 8×6 grid is inaccessible until a backpack is equipped. When the bag is dropped, its cargo remains with that physical bag; recovery restores access to the same contents.
- Item-driven E/F interactions are temporarily blocked when no backpack is equipped, while building navigation, recovery, sleep and save/load remain usable.
- Debug-spawned backpacks auto-equip on an empty start so the test flow can begin without hidden starting cargo.
- Existing save files are preserved; F9 may restore older test inventory contents. No save deletion/reset is performed.
- No Unity test or Play Mode result is claimed. Next verification: start fresh without F9, confirm empty/inaccessible backpack storage, debug-spawn Canvas Backpack and confirm auto-equip, then test drop/recovery and save/load.


## P-03.4a — social interactions without a backpack (2026-11-25)
- GameSession no longer globally blocks E/F interactions when the player has no equipped backpack. NPC conversations, agenda interactions, customer-request review/acceptance and city navigation should remain usable.
- Physical item receipt is guarded before transaction execution: repair-order acceptance, shop purchase, searchable loot pickup, Dirty Cash exchange reward and production output return an explicit capacity warning when no backpack is equipped. The rejected operation should not consume payment/inputs or change ownership.
- Shop browsing and container inspection remain available. Home storage transfers and workbench use are still blocked without a backpack until real pocket storage exists.
- HUD now preserves NPC/mission prompts instead of replacing them with a generic no-backpack message.
- No new automated tests were added. Latest user-confirmed suite was 486/486 before this code change; fresh Unity compilation, EditMode run and Play Mode test remain required.
- Next: P-03.4b functional three-slot 1×1 pockets, save/load, character UI, drop persistence and shared capacity routing. Current three pocket cells are still placeholders.


## P-03.4b — functional personal pockets (2026-11-25)
- The player now has three independent persistent 1×1 personal pockets, each holding one stack of a 1×1 item. They remain available while the backpack is dropped.
- PlayerPocketBook and PlayerPocketSaveMapper preserve item instance IDs, counts, instance state and owner through F5/F9.
- The character UI displays pocket contents. Press P while hovering a 1×1 backpack item to move it to the first free pocket; click an occupied pocket to move it back into backpack storage, or inspect it when no backpack is accessible. Ordinary 1×1 equipped gear can be unequipped into a free pocket.
- Pockets are registered as player economic containers. Repair-order acceptance, retail purchase, searchable loot pickup, Dirty Cash exchange output and Milo's parcel can route to available pocket capacity.
- Eight new Core tests are added; expected suite total is 494, pending Unity compile and test execution. The last user-confirmed baseline was 486/486 after P-03.4a.
- Customer Contract now lists pocket-held goods and can fulfil from the pocket's actual GridInventory. When no backpack is accessible, Retail and Dirty Cash panels switch to an empty carry view rather than exposing the dropped bag's cargo; they do not yet enumerate pocket-held items. Workbench/production and home-storage transfer still require an equipped backpack. Further service-panel integration and clothing visual overlays remain before P-03 completion.

## Latest P-03 UI follow-up — pending Unity verification (2026-11-25)
- User confirmed **494/494 EditMode tests passed** for P-03.4b before the latest UI-only follow-up.
- Backpack items can now be dragged directly onto one of the three personal pocket cells; invalid, rotated or oversized items remain in the source inventory because PlayerPocketBook validates the transfer.
- Press E while hovering an equippable item in a pocket to equip it. This uses the pocket itself as the source container and works without an equipped backpack.
- SearchableContainerPanel rebuilds its player-side grid to match the actual active inventory dimensions on each open. The equipped backpack's 8×6 grid should now visually match the underlying storage instead of showing the stale 6×4 layout.
- Next: pull latest, compile and rerun EditMode tests, then Play Mode verify drag/drop, pocket equipment and bin-search 8×6 alignment. Do not claim the latest UI fixes are verified until the user confirms.

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

## Latest P-03 pocket-role changes (2026-11-26)

- P1/P2 are ordinary pockets; P3 is the hidden pocket. P3 is tinted light red and labelled HIDDEN in the inventory UI.
- Ordinary-search enumeration is exposed by `PlayerPocketBook.GetItemsVisibleToOrdinarySearch()` and excludes P3. Player can still access and move items in P3 normally.
- Routine police-search/confiscation gameplay is not yet a complete system. When implemented, it must use the ordinary-search API; do not enumerate all three pocket grids directly. Advanced scanners/dogs/thorough searches remain undecided.
- Dragging a backpack item onto a pocket now targets that exact pocket. The automatic first-free-pocket shortcut was removed. Dragging a pocket item onto the backpack grid moves it to the exact target cell.
- Pocket item icons are rendered from ItemDefinition.Icon when present, with a name fallback when absent.
- New Core coverage added for hidden-pocket visibility. Current expected EditMode count is 497 (two exact-transfer tests plus one hidden-pocket test beyond the prior 494 baseline); pending Unity compile and execution. No pass is claimed yet.

## Latest test correction (2026-11-26)

- `MovesPocketItemToExactBackpackDropCell` failed because its test fixture attempted to place a 2×1 item into a 1×1 pocket. The fixture now uses a valid 1×1 item and still checks exact backpack coordinates. Production validation is unchanged. Pull commit `9a1244c` and rerun EditMode tests; do not claim a pass until Unity confirms it.


## Latest P-03 pocket interaction update (2026-11-26)
- Direct pocket-to-pocket drag-and-drop is now implemented. Dropping onto an empty pocket moves the stack; dropping onto an occupied pocket swaps the stacks atomically.
- Pocket transfers work with no backpack equipped and while the backpack is dropped. Stable item identity, stack count, item state and ownership are preserved. P3 remains the hidden pocket; this change does not alter ordinary-search visibility.
- Added three EditMode tests. Latest user-confirmed baseline is 494/494; expected suite is now 500, awaiting user Unity execution. No compile/test/Play Mode result is claimed.
- Next check: pull latest branch, run EditMode suite, then in Play Mode drag P1 to empty P3 and swap occupied P2/P3, both with an equipped backpack and without one. Confirm UI refreshes and P3 still shows HIDDEN.


## P-03 checkpoint update — 2026-11-26
- User confirmed **500/500 EditMode tests passing** after direct pocket-to-pocket move/swap support.
- Direct pocket moves to empty slots and swaps with occupied slots work through the shared pocket model; identity, count and item state are preserved. P3's ordinary-search exclusion remains unchanged.
- Next implementation slice is P-03.5: audit and integrate Retail SELL and Dirty Cash selection with pocket-held items, then home storage and workbench/production. Dropped backpack cargo must remain inaccessible in every panel.
- Remaining after service integration: wearable overlays constrained by supplied art, removal of temporary fresh-session debug loadout seeding, and full Play Mode acceptance. The 500/500 result is EditMode verification, not a claim that those Play Mode scenarios have passed.


- Next implementation action: continue P-03.5 with workbench repair and production across accessible backpack/pocket grids, then audit other direct inventory assumptions. Do not start B-07 or W-01 until P-03 is closed.


- P-03.5 now also includes multi-grid workbench repair and production: repair target/kit may occupy separate accessible grids; recipe inputs may span backpack/pockets and outputs route to a capacity-approved accessible grid. Two focused EditMode tests were added, but Unity compile/full-suite verification is still pending.
- Continue the direct backpack-grid assumption audit in search/loot and customer/service panels before wearable visuals and P-03 closeout.


- Latest debug convenience update: fresh Play Mode now starts with a Worn Work Jacket, a Basic Patch Kit, and Fabric Roll + Thread Spool + Adhesive Tube (one full Basic Patch Kit recipe), alongside the existing debug backpack and three extra footprint items. This is temporary and not permanent new-game inventory. Commit `858097a06aba6b70a672549b8e61aa4b09847083`; Unity verification pending.


## NPC rendering test (2026-11-26)

- Latest user-reported EditMode checkpoint: **503/503 passed**, before the visual-only NPC rendering addition below.
- To test the supplied sprites, pull the latest branch and run Unity menu `Signal > Import NPC Rendering Test Sprites...`, selecting `signal_npc_sprites.zip`. This imports the three full sheets locally under `Assets/_Project/Resources/NpcSprites`; the binary art is intentionally not committed.
- Once imported, CityBlockBootstrap adds exactly three generic city residents and randomly permutes the three appearances each session. They use existing generic schedules/movement and have no bespoke gameplay content. Other NPCs and the modular player renderer are untouched.
- Runtime reads the 48×72 cells at 32 PPU, with Idle/Walk animation and four directions. Unity compile/Play Mode verification of this new source is pending.


- NPC rendering-test clarification: the three sprite-sheet residents are strictly temporary visual tests and now remain outdoors in the Street region across all schedule activities. Their movement remains driven by the existing generic NPC schedule/pathfinding. No changes to Milo, Rhea, money cleaner, player visuals or gameplay. User confirmed 503/503 EditMode tests; Unity compile/visual check for this small routing adjustment is pending.


- Latest: three temporary NPC sprite-test residents now roam indefinitely outdoors via random street destinations and the existing pathfinder; they no longer wait at the bench or enter buildings. P-03.5 source audit reviewed Retail, Dirty Cash, Home Storage, Workbench repair/production, customer contracts/repair orders, searchable containers and GameSession item flows; no remaining direct-grid-only blocker found in those paths. Next P-03.6 is wearable presentation, but player sprite resources are user-local/not committed, so obtain their actual imported paths before wiring. Debug repair loadout remains temporary until P-03 final cleanup. Commit `bfcaecf0a98daabd17bf58abbeb14538959a38b9`; 503/503 user-confirmed previously, new roam and Play Mode acceptance pending.


- Outdoor rendering-test NPC follow-up: after the user reported all three walkers stopped at the bench, initial placement was changed to a clearance-safe sampled Street point rather than their schedule-derived location. New roaming destinations are sampled from the Street navigation region, checked with the pathfinder's clearance and spaced away from other residents. The A* pathfinder still handles obstacle avoidance. Unity compile and Play Mode confirmation are pending; 503/503 remains the last user-confirmed EditMode result.
- Latest commits: c000c6158535544f1bdc9dbc806000f87a0a571c, 8365baa42116c5f2819d4f229dedb2488db48de9.


- P-03 automatic receipt routing is now explicitly confirmed: use accessible backpack capacity first, then the first suitable empty pocket in P1 → P2 → P3 order; no destination prompt. Manual moves remain exact-pocket/player-directed. Added a Core regression test; last user-confirmed suite is 503/503 and expected count is 504, pending Unity verification.


## Latest test-result follow-up (2026-11-26)

- Uploaded test report: **500/504 passed, 4 failed**. All 4 failures were in CityBlockBootstrapTests; the Core assembly passed 424/424.
- Cause: temporary NPC sprite rendering-test residents were spawning in EditMode tests when local sprite assets existed, adding three unexpected residents and social interactions. This also changed which nearby resident supplied a talk prompt.
- Fix committed: 75d43bb4e140fa406ac631cd9f25ffacfef6848c. SpawnVisualTestResidents() is now gated by Application.isPlaying as well as sprite asset availability. The three roaming visuals remain a Play Mode-only test feature.
- Next action: rerun all 504 EditMode tests and share the fresh XML. Do not assume the fix passes until verified.
