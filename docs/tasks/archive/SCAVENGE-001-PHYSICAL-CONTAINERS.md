# SCAVENGE-001 — Physical containers and progressive scavenging

**Status:** complete; user verified 368/368 EditMode tests and final Play Mode interaction polish.  
**Approved:** 2026-11-20

## Player outcome
The player approaches physical bins in the shared city, searches them item by item, sees discoveries remain in the bin, and deliberately transfers individual finds into carried inventory.

## Approved rules
1. Each physical container has a stable authored ID and a distinct authored loot profile.
2. First interaction generates contents exactly once. Generated contents, stable item-instance IDs and randomized reveal order are saved.
3. Closing and reopening never rerolls or resets the container.
4. Pressing Search once starts an automatic reveal sequence, revealing one item at a time in randomized order.
5. Discovered items remain physically in the container. Undiscovered items remain concealed.
6. Container and player inventory panels are visible together.
7. Double-click or drag/drop transfers one discovered whole stack into carried inventory.
8. If the destination cannot fit the item, nothing is removed from the container.
9. Closing during the sequence stops presentation but preserves contents, order and discovered count. Reopening resumes at the next undiscovered item.
10. Empty bins stay empty. No respawn or replenishment.
11. The carried inventory remains fully manageable while the container panel is open, even when the bin is empty: player stacks can be repositioned, rotated and inspected without requiring a container item.
12. Drag feedback keeps the grabbed cell under the pointer and aligns the ghost with the snapped target/drop preview.

## First content slice
- Three authored loot profiles A/B/C using existing ItemDefinitions, initially ITEM_A, ITEM_B and ITEM_C.
- Three visible physical bins in Street.
- Each bin has one profile; its result is randomized once and persisted.
- Profiles are test fixtures, not a final city-wide loot economy or rarity system.

## Tarkov-inspired principles
Use experience references, not copied implementation:
- Container identity matters; different containers can have different contents.
- Search is a process, not an instant reveal.
- Inventory management can happen while searching.
- Discovery is distinct from taking an item.
- Examination/inspection is distinct from discovery.

References consulted:
- https://escapefromtarkov.fandom.com/wiki/Looting
- https://escapefromtarkov.fandom.com/wiki/Attention
- https://escapefromtarkov.fandom.com/wiki/Containers

Excluded: Tarkov skill progression, raid/extraction rules, timed respawn, secure containers, loot rarity balancing, dynamic world loot simulation and copied game code. No third-party dependency or Asset Store package is required.

## Architecture and dependencies
- Core owns persistent container/search state and validation.
- Game owns ScriptableObject loot definitions, random generation, physical presentation, input, UI and animation timing.
- Reuse GridInventory, PlacedItem, ItemInstanceId, ItemInstanceState, InventoryTransfer and the existing item-definition catalog.
- Do not add a parallel item/loot representation.
- Cross-inventory drag/drop must be transactional: source removal happens only after destination placement succeeds.
- Container contents are saved independently of carried inventory. Reveal order retains IDs of items already taken so search can resume correctly.
- Save schema v12 introduces SavedSearchableContainer.searchStarted. v10 saves have no container records and generate on first interaction; v11 generated contents/progress are retained with searchStarted defaulting false.
- No package or external code is required.

## Implementation sequence
- [x] Core SearchableContainer stable ID, one-time order initialization and progressive reveal.
- [x] Core save DTO and SearchableContainerSaveMapper foundation.
- [x] Loot profile definitions A/B/C and test assets.
- [x] Physical bins in Street with E interaction and stable IDs.
- [x] First-interaction generation with one-time randomized contents.
- [x] Side-by-side container and carried inventory UI.
- [x] Automatic one-by-one reveal presentation; close/reopen resumes.
- [x] Double-click and cross-panel drag/drop whole-stack transfer.
- [x] Player-owned stacks can be rearranged/rotated and inspected in the carried grid while the container panel is open, including for an empty bin.
- [x] Drag ghost anchoring aligns with the grabbed cell and snapped drop preview.
- [x] Register acquired items to player without losing identity/state.
- [x] GameSession capture/load integration and safe v10/v11 migration.
- [x] Tests for generation idempotency, search order, interruption, transfers, capacity rejection, state/identity preservation and save/load.
- [x] User confirmed Unity compilation, the latest full EditMode suite (368/368), and Play Mode checks for empty-bin carried-inventory management and grid snapping.

## Acceptance criteria
- Reopening never changes generated contents.
- Search reveals one item at a time automatically.
- Closing at any reveal count and reopening resumes with the next item.
- Undiscovered items are not visible or selectable.
- Discovered items remain in the bin until successful transfer.
- Double-click and drag/drop move the exact selected stack to carried inventory.
- Full inventory rejection leaves source unchanged.
- ItemInstanceId and ItemInstanceState survive transfer and save/load.
- Taken items never reappear after save/load.
- Empty bins remain empty and can still be reopened as an inventory-management context.
- Carried inventory can be rearranged, rotated and inspected while the container panel is open, even with zero container items.
- Drag ghost and drop preview agree on the snapped grid position.
- No respawn or timed reset.

## Current implementation status

The complete code/content integration and interaction polish are committed. The user confirmed the latest full Unity EditMode suite **368/368 passed** and confirmed in Play Mode that carried inventory remains manageable while an empty bin is open and that item snapping/drag feedback works correctly. SCAVENGE-001 is complete and user-verified; no assistant-side Unity execution is claimed.

Main implementation areas:
- Core: SearchableContainer, SearchableContainerSaveMapper, SaveData (save v12), GridInventory, InventoryTransfer, AssetTransferService, EconomicTransaction.
- Game: ContainerLootProfile, SearchableContainerPanel, GameSession, CityBlockBootstrap.
- Content: LootProfile_A/B/C assets and HideoutSlice scene references.
- Tests: SearchableContainerTests, AssetTransferServiceTests, ContainerLootProfileTests, CityBlockBootstrapTests.

No further SCAVENGE-001 verification is currently outstanding. Future loot balancing, respawn, broader loot-table tooling or new scavenging mechanics remain separate design decisions and are not implied by completion of this test-content slice.
