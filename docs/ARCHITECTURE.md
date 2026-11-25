# Architecture — current runtime map

## Active architecture

Signal has one active gameplay architecture:

- **Signal.Core:** plain C# domain logic and DTOs. No UnityEngine references, scenes or MonoBehaviours. Rules are covered by EditMode tests.
- **Signal.Game:** Unity-facing composition, MonoBehaviours, UI, input, ScriptableObject definitions, scene integration and presentation.
- **Content:** authored ScriptableObject assets referenced by stable asset GUIDs. Runtime save data does not serialize Unity object references.

Dependency direction is Game → Core. Do not add gameplay to `Assets/_Project/_Prototype/` or legacy `Main.unity`.

## Composition and runtime flow

- `GameSession` is the active composition root. It builds player/runtime services, registers inventories and resident accounts, loads authored content, creates panels and connects world interactions.
- `RoomBootstrap` builds the hideout and exposes the physical workbench/storage/bed.
- `CityBlockBootstrap` builds the shared city spaces, navigation representation, resident presenters and interactions.
- `NpcPresenter` is the generic visible resident. Identity, routine profile, dialogue, positions, appearance and starting clean balance belong in `NpcCharacterDefinition`.
- `WorkbenchWorkspace` is presentation/input only. It binds inventory, ownership, recipe definitions and callbacks from GameSession. It does not own production rules.
- Customer and Dirty Cash panels similarly present authored offers and call GameSession/Core transaction paths.

## Core domains and key boundaries

| Domain | Core authority | Game composition/presentation |
|---|---|---|
| Inventory | `GridInventory`, `InventoryTransfer`, `InventoryDragMath` | InventoryPanel, StoragePanel, container/workbench panels |
| Item identity/state | `ItemType`, `ItemInstanceState`, stable ItemInstanceId, ItemKnowledgeBook | ItemDefinition, ItemViewerPanel |
| Ownership/custody | `AssetOwnershipBook`, `EconomicContainerRegistry`, `AssetTransferService` | GameSession registers live inventories with actors |
| Clean money | `CleanMoneyAccount`, `EconomicAccountBook` | player/resident account registration and UI |
| Retail | `RetailService`, `RetailTransactionLedger`, Shop sale boundary | BusinessDefinition, RetailPanel, GameSession |
| Dirty Cash | exchange/cleaning services and dedicated ledgers | DirtyCashContactOfferDefinition, DirtyCashContactPanel |
| Production | `ProductionService`, `ProductionTransactionLedger` | RecipeDefinition, WorkbenchWorkspace, GameSession |
| Customer contracts | `CustomerContractBook`, `CustomerContractService` | CustomerContractDefinition, CustomerContractPanel, issuer receiver inventory |
| Workshop upgrades | `WorkshopProgressionBook`, `WorkshopUpgradeService` | WorkshopUpgradeDefinition, installed prop, workbench panel |
| Searchable containers | `SearchableContainer` and persistent discovery state | ContainerLootProfile, SearchableContainerPanel, GameSession |
| Crime | `CrimeOutcomeLedger`, `CriminalProgressionTrack`, transfer semantics | ResidentPropertyContainerDefinition and local witness selection |
| Player development (planned) | PlayerCharacterProfile, PlayerAttributeSet, PlayerSkillBook, milestone ledger and pure attribute calculator (not implemented) | appearance/skill/milestone definitions, CharacterVisualRig and PlayerDevelopmentPanel (planned) |
| NPC/social | `NpcSocialWorld`, perception, knowledge, utility brain and encounters | NpcCharacterDefinition, NpcPresenter, CityBlockBootstrap |
| Narrative opportunity | discovery, relationship and opportunity Core models | authored scenario controllers such as MiloParcelOpportunityController |
| Save/load | Save DTOs, SaveMapper and dedicated mappers | SaveFileStore and GameSession |

## Transaction rules

- UI is not authoritative. GameSession revalidates current context and Core services validate transaction invariants.
- Ownership is not inferred from physical container custody.
- A successful ownership-changing transfer validates the exact stack, actor ownership, registered containers, destination capacity and transaction identity.
- Failed transactions must restore all snapshots and leave accounts, inventories, ownership and ledgers unchanged.
- Same-owner container movement is an inventory operation, not an economic ownership transfer.
- Clean account balance changes and physical Dirty Cash changes use separate APIs.
- Retail purchases instantiate authored merchandise; physical goods handovers use asset-transfer paths.

## Production and contracts

- Production takes an actor-owned registered inventory. Input stacks must be owned by the operator; outputs become owned by that operator.
- RecipeDefinition describes content only. ProductionService is the authority for consumption/output/history.
- Contract book enforces one active contract. CustomerContractService validates exact required whole stack and player ownership, transfers goods to the issuer's receiving inventory, debits the issuer's resident account and credits the player atomically.
- WorkshopProgressionService acquires a stable upgrade once, validates prerequisites and CARD accounts, then the Game layer applies its visible prop and the recipe unlock.
- Persistent state stores stable IDs and inventories, not references to ScriptableObjects or prefabs.

## Persistence

Build **0.01** uses an unversioned development save snapshot. `SaveData` is serialized directly and `SaveMapper` restores the current state; there is no save-version field, version gate or automatic migration layer.

Test current-build save/load round trips whenever persistent state changes. Older experimental saves are not guaranteed to remain compatible when the serialized data shape changes; start a fresh development save when needed. Malformed records should be skipped safely, and a missing or unreadable file must remain a clear load failure.

## Unity asset and refactoring safety

- Preserve .meta GUIDs and scene references. Folder casing is significant in the repository even though Windows filesystems can obscure case collisions.
- Avoid FindObjectOfType, GameObject.Find and new singletons; pass dependencies through serialized references or constructors.
- Split large classes only where it improves clarity and reduces coupling without changing behavior; do so in a coherent, tested initiative.
- Do not add packages or third-party code without user approval.


## Planned Player Development integration (P-01/P-02; not implemented)

- Preserve `RoomBootstrap` as the existing player composition entry point initially. Its current `CreatePlayer()` builds a placeholder SpriteRenderer, Rigidbody2D, BoxCollider2D and PlayerMotor2D.
- P-01 replaces the visual placeholder with a Game-layer modular rig. It must not replace movement/physics or move Unity types into Core.
- `PlayerMotor2D` currently reads input and writes velocity but does not expose a formal facing/animation contract. P-01 should introduce the smallest explicit movement/action state contract needed by the visual driver; avoid coupling the motor to SpriteRenderer or authored sprite assets.
- `GameSession` remains the composition root and passes authored definitions/profile state into the player/visual setup. Avoid a new singleton or scene-wide object search.
- Core owns persistent appearance IDs, the two base attributes (Armor, Heat Protection), skill ranks/points and one-time milestone awards. Game owns ScriptableObject definitions, Sprite composition, UI and event wiring.
- P-02's attribute calculator is pure Core logic. P-03 later supplies equipment modifiers from an authoritative loadout; W-01 later consumes effective Heat Protection. Neither P-02 nor the renderer should depend on a specific weather implementation.
- SaveData/SaveMapper persist stable IDs and primitive values only. Build 0.01 has no schema version/migration chain; current-build round-trip tests are required.

## Planned weather integration (W-01; separate and not implemented)

W-01 owns the shared world weather state and player cold-exposure simulation. It is driven by GameClock and composed by GameSession/world bootstrap. Core owns deterministic weather/exposure rules; Game owns authored seasonal patterns and weather presentation. It reads effective Heat Protection through a value interface and must not branch on item IDs or depend on the character renderer. Only cold-related exposure is planned; no hot-weather system.


## Player Development — first implementation slice

- `RoomBootstrap.CreatePlayer()` keeps the existing physics and `PlayerMotor2D`, then adds `PlayerCharacterVisual`. The visual rig creates five child SpriteRenderers and loads body-part sheets through Resources. If assets are absent, the existing teal placeholder remains as a child fallback.
- `CharacterPackImporter` is an Editor menu tool. It extracts the user-supplied archive and sets pixel-art texture import settings. The ZIP is not part of the repository.
- `GameSession` composes `PlayerDevelopmentState` from the Resources `PlayerDevelopmentCatalog`, creates the F2 debug panel, captures/restores progression with SaveData, and exposes an idempotent milestone-award bridge.
- Core remains Unity-free. Skill catalog/display content and visual sprite composition live in Game. The F2 panel is a debug-slice UI, not the final character-creation/wardrobe interface.
- The current skill list and progression values are provisional. Skill effects on activity calculations are not yet integrated; P-03 supplies equipment modifiers to the pure attribute calculator.
