- [ ] Audit all inventory construction, registration, service access, UI binding, save/load and stack-lineage paths for aliased grid instances.
- [ ] Give each physical backpack its own persistent grid; support multiple dropped backpacks and the approved automatic swap.
- [ ] Preserve independent player pockets, home storage, world/searchable containers, resident/NPC-owned pockets, business stock and contract/service escrow inventories.
- [ ] Make all cross-inventory movement explicit, validated and atomic; no implicit shared backing grids.
- [ ] Serialize contents by stable storage identity and migrate legacy saves deterministically.
- [ ] Add pairwise isolation, transfer, access-control, save/load and regression tests for each storage domain.
- [x] Quickbar drag ghost uses a dedicated high-sorting overlay Canvas (commit `1971e800a08011e8f031ecd79fdd67cb56d3a17d`).

### Storage identity registry foundation — implementation update (2026-11-27)
- [x] Add typed stable storage identity and a session registry that rejects duplicate IDs and aliased `GridInventory` references.
- [x] Register current independent player pockets, hideout storage, contact inventory, contract receiver inventory, street searchable containers and resident-owned searchable property containers.
- [x] Add five Core regression tests for identity and alias invariants.
- [x] Wire the registry-backed `PortableStorageBook` allocator into GameSession: equipped physical backpacks receive distinct grids and the active player inventory view switches to the equipped bag. Unity compile/EditMode/Play Mode verification pending.
- [ ] Convert dropped backpack state/visual tracking to multiple instances and implement automatic swap without merging storage.
- [ ] Register future NPC pockets and business stock as their own inventories when those runtime systems are introduced.
- [x] Move backpack save/load to stable storage identities and migrate legacy single-backpack global `items` into the equipped/dropped bag. Unity F5/F9 verification pending.
- [ ] Verify all transfer, service and UI paths use only explicitly accessible inventories; run Unity compile, EditMode and Play Mode.



### Storage isolation — persistence primitive added (2026-11-27)
- [x] Add Core save records and a mapper that captures/restores every physical backpack's grid by stable backpack GUID; include dimensions and item instance/state data.
- [x] Reject duplicate backpack storage IDs and duplicate physical item IDs during restore; unregister stale backpack registry mappings without clearing unrelated storage domains.
- [x] Wire per-backpack persistence into GameSession with a separate staging inventory for legacy `items`, preventing double serialization. Unity F5/F9 verification pending.
- [ ] Replace the single dropped-backpack item/visual with multi-instance tracking and implement the approved automatic swap.
- [ ] Enable TEST LOADOUT with other backpacks dropped only after multi-bag swap and persistence are safe.
- [ ] Extend runtime storage registry coverage to NPC pockets and business stock; audit every UI/service access path.
- [x] Add deterministic migration for legacy single-backpack saves from the old global `items` grid. [ ] Verify F5/F9 round trips in Play Mode.


### Current storage implementation sequence — 2026-11-27
- [x] Core independent backpack grid allocation and identity-keyed persistence primitives — user verified 580/580.
- [x] GameSession active-grid switching and immediate detachment on drop; all relevant UI/service consumers rebind. Unity verification pending.
- [x] Rebind InventoryPanel, StoragePanel, RetailPanel, DirtyCashContactPanel, CustomerContractPanel, WorkbenchWorkspace and MiloParcelOpportunityController whenever the active backpack grid changes.
- [ ] Support multiple dropped physical backpacks and the approved automatic swap, preserving each bag's own grid.
- [x] Integrate per-backpack records into F5/F9 and deterministic legacy single-backpack migration; collect existing storage item GUIDs to reject cross-domain duplicates. Unity F5/F9 verification pending.
- [ ] Enable TEST LOADOUT with dropped bags only after the preceding runtime and persistence paths are safe.
- [ ] Continue the universal inventory identity audit for NPC pockets and business stock; backpack work alone does not complete global storage independence.


- [ ] Replace the singular dropped-backpack item/visual with GUID-keyed collections and persist every dropped bag's equipment metadata.
- [ ] Implement the approved automatic swap: equipped bag drops at player's feet, recovered bag equips, each retains its own contents.
- [ ] Remove the TEST LOADOUT dropped-backpack restriction only after multi-bag swap and persistence are verified.
- [ ] Verify current runtime integration with Unity compile, 581 EditMode tests and Play Mode F5/F9/equip/drop/recover/quickbar checks (latest user-confirmed baseline: 580/580).