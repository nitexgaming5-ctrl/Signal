# Architecture

## Layers
1. **Signal.Core** (asmdef, no UnityEngine, no scene) — stats, inventory grid, rules, save DTOs, clock. All testable in EditMode.
2. **Signal.Game** (asmdef, references Core) — MonoBehaviours, UI, tilemap, input, glue to Unity.
3. **Content** (ScriptableObject assets) — items, recipes, factions, placed in `Assets/_Project/Content/`. Data only, no logic.

Dependencies go one way: Game → Core. Core never references Game.

## Folder layout
```
Assets/_Project/
  Core/            Signal.Core.asmdef     pure C# logic (no UnityEngine)
  Game/            Signal.Game.asmdef     MonoBehaviours, glue
  UI/              (inside Game) inventory, HUD, panels
  World/           (inside Game) tilemaps, room, camera, player
  Content/         ScriptableObject assets only
    Items/
    Recipes/
  Art/             Aseprite sources, imported sprites, tilesets
  Tests/
    EditMode/      Signal.Core.Tests.asmdef, Signal.Game.Tests.asmdef
  _Prototype/      old prototype code, read-only reference, do not extend
```

Prototype code lives in `_Prototype/`. New work does not build on it. Its string ids and hardcoded values are the reason for this rebuild.

## Assembly definitions
- `Signal.Core`: no references, `noEngineReferences: true`
- `Signal.Core.Tests`: references `Signal.Core`, EditMode only
- `Signal.Game`: references `Signal.Core`, `UnityEngine.UI`, `Unity.InputSystem`
- `Signal.Game.Tests`: references `Signal.Game`, `Signal.Core`, EditMode only

Rule: one `.asmdef` per folder, and each test folder has its own subfolder. (This caused the earlier compile errors.)

## UI conventions
- UGUI panels are built from code at runtime; a scene needs only the composition-root GameObject.
- World input is read through the Input System (`Keyboard.current`, `Mouse.current`). Add an EventSystem with InputSystemUIInputModule only when a UI needs pointer-driven buttons or dragging.
- Every rule (can this item go there?) lives in Core. UI asks Core objects and presents the result.
- Text uses UGUI `Text` with Unity's built-in font until the pixel font pass (see ART_TODO.md).

## Communication between systems
- Systems expose C# events for state changes. UI subscribes and refreshes; it does not duplicate domain state.
- Save/load goes through DTOs, not runtime classes.
- `GameSession` is the composition root that wires Core state to Game presentation and world objects.

## Coding conventions
- Namespaces: `Signal.Core.*`, `Signal.Game.*` matching folders.
- One main public type per file; tiny value types may share their owning file.
- Serialized fields are private with `[SerializeField]`. No public fields.
- Prefer immutable definitions and explicit runtime state.
- Comments explain why, not what.

## Active architecture decision (2026-11-17)

**Signal.Core + Signal.Game are the single active gameplay architecture.** Core owns pure domain state and rules; Game owns Unity runtime, input, scene composition, world objects, UI and persistence adapters.

The old `Assets/_Project/Scenes/Main.unity` and `Assets/_Project/_Prototype/Scripts` are retained as legacy references because the scene contains useful city/shelter art and gameplay experiments. They are not the active build path, and no new gameplay should be added there. The active build scene is `Assets/_Project/Scenes/HideoutSlice.unity`, composed by `Signal.Game.GameSession`.

Why Core/Game: clearer boundaries, reusable domain rules, and focused EditMode coverage. The prototype contains useful ideas (outing/loot risk, shelter switching, factions and NPC interactions), but duplicates state and uses global lookups and tightly coupled scene wiring. Migrate only useful behavior required by a slice; rewrite it against Core/Game rather than bulk-copying it.

Do not run both architectures in the same active scene. Preserve Main and its asset GUIDs until valuable content has been migrated and verified. Remove legacy scripts only in a separate cleanup after their scene references are gone.

## First hideout implementation

The active HideoutSlice is a code-built first-pass room. It contains placeholder bed, table, chair, storage and a physical workbench. The workbench opens a close-up UGUI tabletop with draggable and inspectable props. It remains a visual interaction prototype until recipe assets are authored and connected; no production/economy rules are implied by the tabletop props.

## Connected city and home storage

`CityBlockBootstrap` composes the compact outdoor street and two simple interior spaces under the active `GameSession`. It reuses the same player Rigidbody2D and keeps inventory, clock and other session state alive while changing the active visual root and camera bounds.

The home locker is a second `GridInventory` owned by `GameSession`. `StoragePanel` lists individual stacks from both containers, exposes inspection and moves a selected whole stack with Core `InventoryTransfer.MoveStack`. The bulk `MoveAll` API remains available for system-level use. Transfers preserve stable identity, state and orientation; distinct stacks are never merged during transfer, and a stack that does not fit remains in the source. Transfer rollback uses inventory snapshots. HideoutSlice currently references ITEM_A/B/C as temporary test fixtures from the existing inventory demo; these are not final content.

Each `PlacedItem` has a persistent `Guid ItemInstanceId` identifying its stack, separate from the inventory-local integer `InstanceId` used by the UI. `ItemInstanceState` stores quality, condition, authenticity and ownership status per stack. `GridInventory.Add` merges only stacks whose definition and state match. Save format version 4 persists these state fields; v1/v2/v3 migration preserves existing identities where present and defaults new state to Standard/Good/Genuine/Legitimate. `CityBlockBootstrap.TryRestorePlace` restores the saved active world root and camera bounds. The user confirmed **235/235 EditMode tests passed** after Item Instance v1; the new state/viewer implementation and tests are awaiting Unity verification.

## Item instance state and inspection

- Core owns `ItemInstanceState` and `ItemValueCalculator`; Unity does not calculate item values itself.
- A stack currently carries quality, condition, authenticity and ownership status. These are the item's actual facts, not yet the player's discovered knowledge. A future inspection/appraisal skill or uncertainty model must be a separate layer so the UI cannot accidentally expose secrets.
- `ItemDefinition` authors the item's descriptive metadata, icon, base reference value and default state. Runtime stacks can differ from those defaults.
- The viewer is a read-only presentation of the selected physical stack. Right-clicking an item in the carried grid or choosing INSPECT in home storage opens it; both paths use the same viewer.
- The value calculation is provisional reference valuation only. It is not a transaction price, resale offer, market simulation or economy rule.
- Save format v4 persists all four state enums. Existing v1/v2/v3 saves migrate to the neutral default state.

## Connected NPC simulation

NPC runtime belongs to the shared city simulation, not to isolated character-specific scripts.

- Core owns the deterministic social-world model: NPC registration and presence, local perception, world-event records, individual knowledge, directed relationships and explicit knowledge exchange. NpcPerception builds a character-scoped snapshot for decision systems.
- Game owns Unity presentation, spatial movement, scene/location integration and publishes player-facing/world events into the shared Core context.
- An NPC's knowledge is not the same as the world event journal. Decision-making must consume the individual agent's NpcPerception snapshot (nearby people plus its own knowledge), never the global journal as an omniscient shortcut.
- World events must contain only information that could be perceived by their witnesses. Hidden motives, identities and causes must not leak through event descriptions.
- The current Milo component is a temporary integration fixture. It is not the production character-authoring pattern. Move identity, appearance, routine preferences and dialogue into data-driven definitions before bulk NPC creation.
- NPC-to-NPC social knowledge/relationships and transient perception remain runtime-only in general. The resident-property theft slice is a narrow exception: its persisted CrimeOutcome stores the selected witness and event so that one character's direct-observation knowledge can be rehydrated after load. This is not a general social-memory save system. Player-to-NPC relationship impressions used by the Milo scenario are persisted in save v7.

The implementation contract and phased delivery plan are documented in [NPC_FRAMEWORK.md](NPC_FRAMEWORK.md). Keep the simulation deterministic and event-driven; do not add per-frame full-population reasoning, external AI services or separate legal/criminal social worlds.


## Player discovery and contextual interactions

- PlayerDiscoveryLedger is player-scoped and separate from ItemKnowledgeBook, NPC NpcKnowledge and authoritative world state.
- PlayerDiscoverySignal records one learned signal with stable occurrence ID, subject ID, signal type (routine observation, conversation, physical evidence or rumour), source ID, observable/communicated description, confidence and in-game learned time. The ledger is a provenance-preserving record, not a truth evaluator or conclusion generator.
- Re-recording the same stable signal ID is idempotent. A new occurrence requires a new ID; repeating one observation cannot inflate the number of distinct signal types.
- InteractionConditionSet evaluates current NPC activity, the player's directed relationship impression and required discovery signal IDs/types. Conditions are conjunctive and evaluated against an explicit InteractionContext; no global lookups or character-specific branches are introduced.
- The ledger does not infer what the player believes about Milo. Authored opportunity logic evaluates combinations separately. Item facts continue to use ItemKnowledgeBook; NPC facts remain character-scoped in NpcKnowledge.
- PlayerRelationshipBook stores the player's directed familiarity/trust/affinity toward residents. It is separate from NPC-to-NPC relationships in NpcSocialWorld.
- MiloParcelOpportunityController is a Game-layer, scenario-specific composition of Core discovery/relationship/opportunity state and existing inventory/item-knowledge systems. It owns the physical parcel, evidence prop, interaction prompts, choices and drop-off presentation; it does not introduce a generic quest journal or dialogue framework.
- Save v7 persists player discovery signals, player-to-NPC relationships, opportunity state and delivery assessment. NPC-to-NPC memories/relationships and transient perception remain runtime-only. Keep Core plain C# and Game responsible for scene objects, input and presentation.


## Authored opportunity lifecycle
- OpportunityDefinition is an immutable Core definition keyed by stable opportunity ID and associated NPC ID; its offer gate reuses InteractionConditionSet.
- OpportunityState is the small runtime lifecycle: Dormant, Available, Accepted and Resolved. Decline is recorded without failing the opportunity. Returning the parcel moves Accepted back to Available. Delivery resolves once. Expiry returns an available opportunity to Dormant and does not queue a future release.
- The state model stores the parcel's stable item-stack identity rather than a separate quest-item copy. The Game layer must move the actual item through existing inventory/world interaction services.
- This is not a generic quest system. It has no journal, objective tree, reward catalogue or autonomous backlog. Save integration and GameSession ownership are pending.


## Milo parcel vertical slice integration

- GameSession owns PlayerDiscoveryLedger, PlayerRelationshipBook and OpportunityState; it supplies them to the scenario controller and SaveMapper.
- Item_MiloParcel is a dedicated ItemDefinition referenced by the active HideoutSlice scene and catalogued for save/load resolution, but is not granted as a starting item.
- The parcel is represented by one stable PlacedItem.ItemInstanceId when accepted. Its authoritative state is Stolen; player knowledge remains separate and can be partial/uncertain.
- MiloParcelOpportunityController is intentionally specific to Opportunity 001. Do not promote its authored dialogue, coordinates or story conditions into generic Core abstractions.
- Delivery assessment records same-day timing, damaged/intact condition and nearby third-party exposure. Complications alter Milo's relationship and dialogue softly; they do not invoke law enforcement or global criminal progression.
- Save format v7 migrates v1-v6. User verification of compilation, tests and Play Mode remains pending.


- StoragePanel displays only facts present in ItemKnowledgeBook. It must not expose PlacedItem.State directly; ItemViewerPanel remains the place to inspect known source/confidence and partial knowledge.
## Economic domain (approved direction; Phase 1 Core foundation added 2026-11-19)

Economic rules belong in Signal.Core as plain C#; Unity presentation, authoring and runtime composition belong in Signal.Game.

- Actor identity and asset ownership are explicit. A physical inventory is a location/container, not proof of ownership.
- AssetTransferService composes the existing stable stack identity and InventoryTransfer behavior with ownership validation and an append-only transaction ledger.
- Moving an item between containers of the same owner remains an inventory operation. A change of owner is an economic asset transfer.
- Monetary accounts are distinct from physical inventories. Dirty money is a physical inventory asset and is never accepted by legitimate bank/card operations.
- Avoid a monolithic EconomyManager. EconomicAccountBook, RetailService and future market services compose the shared actor, asset and transaction primitives.
- Save v8 persists the player clean balance, held-asset ownership and physical asset-transfer history. Save v9 adds resident account balances and retail purchase history; runtime container custody is reconstructed from live GridInventory references.
- No universal supply/demand simulation is planned for the initial economy; stock simulation is selective and gameplay-driven.


EconomicContainerRegistry records the custodian for each runtime inventory container. Asset ownership is deliberately not inferred from container custody. AssetTransferService validates the asset's current owner and requires the destination container to belong to the receiving actor; this allows assets to be held by a third-party custodian without confusing storage with ownership.


## Economic account and persistence composition

- `CleanMoneyAccount` is a Core account owned by `PlayerStats`. Legacy `Money`, `Earn` and `TrySpend` APIs delegate to it, so rent and debug-sale paths remain clean-account operations.
- `EconomicAccountBook` registers the same player account under actor `player` and one independent account for each resident under `resident:<npc-id>`. Authored resident starting balances initialize once; saved v9 balances override them.
- Dirty money is never a field in PlayerStats, CleanMoneyAccount or EconomicAccountBook. It remains a physical inventory asset; future illicit payment/cleaning services must consume or transfer those stacks explicitly.
- GameSession composes AssetOwnershipBook, EconomicTransactionLedger, EconomicContainerRegistry, AssetTransferService, EconomicAccountBook, RetailService and RetailTransactionLedger. It registers carried inventory and home storage as player-custodied containers and registers spawned residents as financial actors.
- SaveMapper v8 persists the player clean balance, player-held asset owner mappings and physical asset-transfer records. Save v9 additionally persists resident account balances and retail transaction history. Older saves preserve the player balance and initialize the new resident/retail collections safely.
- The container registry is rebuilt from live GridInventory references; Unity object/container references are not serialized.


## Player-to-business sale transaction path

- `BusinessDefinition` is the authority for each business's separate fixed-price buyback offers. The list is independent from its merchandise catalogue.
- GameSession validates active counter proximity, business identity, opening schedule, current offer, selected carried stack and `AssetOwnershipBook` owner before calling the Core sale boundary.
- `Shop.TryFulfilExact` is the Core transaction boundary for a selected stack. It reuses the existing `ShopOrder` validation/account-credit semantics but removes quantity from the exact `ItemInstanceId`, never from another matching stack.
- Full-stack sale removes the player ownership entry; partial sale retains it. Clean CARD credit and sale ledger recording are atomic with inventory mutation and rollback.
- `ShopSaleTransactionLedger` is distinct from physical asset-transfer and retail-purchase ledgers. SaveMapper persists it in v10.
- RetailPanel owns presentation only. BUY remains delegated to `RetailService`; SELL delegates a selected stack/quantity to GameSession and never makes UI state authoritative.

## Business and retail runtime (Phase 3 first pass)

- `BusinessDefinition` is authored Game content: stable business ID, display/type, resident owner ID, city location, interaction point, opening schedule and an unlimited catalogue of `ItemDefinition` references with fixed clean-money prices. It is not serialized into save data.
- `RetailPanel` is a shared keyboard-driven catalogue UI. GameSession resolves the nearby business, checks its schedule and owner account, and delegates each confirmed one-unit purchase to Core `RetailService`.
- `RetailService` validates customer/owner accounts, clean balance, inventory capacity and ownership of any matching stack that may be topped up. It grants the item using the authored default state, registers new stack ownership, transfers clean money from customer to owner and appends a `RetailTransaction`. It restores the pre-operation state on failure.
- `RetailTransactionLedger` is distinct from the physical asset-transfer ledger because unlimited catalogue merchandise is instantiated for the customer rather than transferred from a finite business inventory.
- The Corner Bakery definition is assigned in the active HideoutSlice scene. Its catalogue is intentionally empty pending user-approved merchandise and prices. Existing Core `Shop` / `ShopOrder` selling logic remains unchanged.


## Physical searchable containers and cross-inventory transfer

- Core `SearchableContainer` owns persistent state only: stable container ID, normal `GridInventory`, immutable reveal order, discovery count and search-started flag. It has no Unity dependencies or random-generation policy.
- Game `ContainerLootProfile` ScriptableObjects author weighted item definitions and quantity ranges. `GameSession` invokes generation once on first interaction and registers generated instances to the bin's world actor.
- `CityBlockBootstrap` owns the physical street placement and proximity prompt for three stable bin IDs. `GameSession` maps those IDs to the three authored profiles and runtime Core containers.
- `SearchableContainerPanel` owns only presentation/input: it renders discovered stacks in a side-by-side grid UI, animates the reveal sequence, pauses it on close and resumes from Core state on reopen. It delegates transfers to GameSession.
- Cross-inventory transfer is transactional. Core first validates destination placement, then removes the source stack; AssetTransferService changes ownership and records the `Scavenging` transaction, rolling back inventory/ownership if the transaction cannot be committed.
- Bin inventories are registered as world-custodied containers under `world:<container-id>`. The player inventory remains registered to Player. Ownership and custody remain separate concepts.
- SaveMapper handles player inventory/ownership; SearchableContainerSaveMapper separately handles bin contents and search state. Runtime registry entries are rebuilt from live inventory references, not serialized Unity objects.
- Save v12 persists `searchStarted` in addition to generated contents, reveal order and discovery count. No external package or third-party code is required.


## Crime outcomes and resident-property theft

- The crime action is emitted by the authoritative item-transfer operation, not inferred from UI text or arbitrary inventory changes. Its CrimeOutcome records the action ID, player actor, transferred ItemInstanceId, location/time, awards and optional selected witness. A failed move creates no crime outcome.
- `AssetTransferService` owns the atomic theft-state transition: reason `Theft` changes the moved item's ownership-status state to `Stolen` before ownership and transaction commit; snapshot rollback restores both inventories if any commit step fails.
- `CrimeOutcome` / `CrimeOutcomeLedger` records the semantic crime result separately from the economic transfer ledger. `CriminalProgressionTrack` is a single global, non-decaying total. `HeatMeter` remains current pressure; it is not a witness or evidence database.
- `ResidentPropertyContainerDefinition` is reusable authored Game content. It supplies the resident owner, location, interaction position, fixed contents and initial crime/exposure tuning. Rhea's wardrobe is a first content fixture; future `StealingHouse_NPC` containers should reuse this definition and flow rather than add Rhea-specific branches.
- A Game-layer witness selection chooses at most one resident physically present in the property's location and within the minimum of the authored witness radius and that NPC's awareness radius. Core `NpcSocialWorld.PublishEventToNpc` gives the event only to that explicit observer. The description is observable and does not reveal the player's identity.
- Generic ambient social encounters explicitly skip `crime-observed` events. Witness sharing/reporting is disabled until its rules are separately designed and approved.
- Crime outcome persistence stores the selected witness identity and observation event. On load, only that NPC's knowledge is restored; no event is broadcast to newly nearby residents. There is no automatic report, police response, suspicion or relationship penalty in this first slice.
- Save v13 stores the global progression total and crime outcomes; the existing searchable-container mapper stores the physical resident-property inventory and reveal state. Runtime container custody is reconstructed from the authored owner and live inventory.
- Dirty-money exchange/cleaning and authored production remain later slices. Do not build a universal crime, transaction, production or police framework ahead of a concrete repeated use case.
