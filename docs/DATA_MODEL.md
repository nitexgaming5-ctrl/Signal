# Data model — current definitions and persistence

## Governing rule

Definitions describe what something is and belong in Game-layer ScriptableObjects. Runtime state describes what exists/has happened and belongs in plain C# Core models. Gameplay code references definitions; it must not invent string-based item IDs.

## Authored definitions (ScriptableObjects)

- **ItemDefinition:** stable Guid, display name/description/category, base reference value, footprint, max stack, optional icon and default ItemInstanceState.
- **RecipeDefinition:** stable Guid, display name/description, explicit Immediate timing, ItemDefinition input entries and one ItemDefinition output with positive quantities.
- **CustomerContractDefinition:** stable Guid, customer/issuer NPC ID, required ItemDefinition/quantity, fixed clean CARD reward, goods eligibility and whole-stack delivery mode.
- **WorkshopUpgradeDefinition:** stable Guid, description, unlocked recipe, installed prop prefab/offset, acquisition mode, prerequisite, positive CARD cost and resident payee NPC ID.
- **NpcCharacterDefinition:** stable NPC ID, display name, public role, awareness/movement settings, starting clean balance, utility routine profile, destinations, appearance and dialogue.
- **NpcAgendaDefinition:** stable NPC ID, prioritized independent threads, sequential stages, activity/place/discovery/relationship/opportunity/contract/upgrade conditions and typed player choices. Workshop offer choices reference existing WorkshopUpgradeDefinition assets.
- **BusinessDefinition:** stable business ID, resident owner, location/schedule, fixed merchandise catalogue and separate buyback offers.
- **ContainerLootProfile:** weighted ItemDefinition entries and quantity ranges for one-time generation.
- **ResidentPropertyContainerDefinition:** stable container identity, owner/location, fixed item content and initial crime/exposure tuning.
- Other narrative definitions should be introduced only when a real scenario needs them.

New ScriptableObjects generate their stable Guid once. Never change it after content has been saved or referenced.

## Runtime domains (Core)

- **GridInventory / PlacedItem:** spatial grid, definition ItemType, position, rotation, stack count, stable ItemInstanceId and ItemInstanceState.
- **ItemInstanceState:** quality, condition, authenticity and ownership. Ownership is a property of the item; storage custody is separate.
- **ItemKnowledgeBook:** player beliefs about item state, each with aspect, believed value, source, confidence and learned time. Unknown aspects remain unknown.
- **NpcSocialWorld:** resident presence, directed relationships, world events and individual knowledge. NPC belief is not world truth.
- **PlayerRelationshipBook / PlayerDiscoveryLedger:** player-scoped directed relationship impressions and learned discovery signals.
- **NpcAgendaProgressBook:** completed NPC/thread/stage IDs and selected choice IDs; dialogue content remains in Game ScriptableObjects.
- **CleanMoneyAccount / EconomicAccountBook:** clean CARD integer balances by actor. Dirty Cash is never an account field.
- **AssetOwnershipBook / EconomicContainerRegistry:** asset owner and container custodian are distinct registries.
- **AssetTransferService / EconomicTransactionLedger:** atomic physical ownership-changing transfer and append-only transaction history.
- **RetailTransactionLedger / ShopSaleTransactionLedger:** authored catalogue purchases and player sell-to-shop operations are separate from physical asset transfer.
- **Dirty Cash ledgers:** illicit exchange and cleaning histories are distinct from clean retail.
- **ProductionService / ProductionTransactionLedger:** actor-owned production and operator/time/recipe/output history.
- **CustomerContractBook:** one active authored contract state, acceptance/completion time, reward/payer and delivery transaction identity.
- **WorkshopProgressionBook:** acquired upgrade IDs; recipe definitions remain authored content.
- **SearchableContainer:** stable identity, generated inventory, reveal order, search-started state and discovered count.
- **CrimeOutcomeLedger / CriminalProgressionTrack / HeatMeter:** semantic crime outcomes, global criminal progression and current heat are separate concepts.

## Save/load policy for build 0.01

Save DTOs are serializable plain data. They store definition GUIDs, stack instance GUIDs, enum values, scalar state, actor IDs and transaction records. They do not store Unity object references.

The current `SaveData` snapshot persists:
- player carried inventory, home storage, player stats/CARD, clock, heat and rent;
- player item knowledge, discoveries, relationships and opportunity state;
- asset ownership and economic transactions;
- resident clean account balances and retail/shop-sale transactions;
- searchable container generation/content/reveal state;
- criminal progression and crime outcomes, including selected witness observation identity;
- physical Dirty Cash contact inventory and illicit exchange/cleaning ledgers;
- production transactions/operator, customer contract lifecycle, acquired workshop upgrade IDs, issuer receiver inventory and receiver actor identity;
- completed NPC agenda stage/choice progress by stable NPC/thread/stage IDs.

### Development save policy

- Build 0.01 has no serialized save-schema version, version gate or automatic migration chain.
- Save/load is expected to round-trip the current working build.
- Older experimental save files are not guaranteed to remain compatible after persisted data changes. Start a fresh development save when necessary.
- Keep current-build persistence reliable: reject a missing/unreadable file clearly and skip malformed or unknown records without silently corrupting unrelated state.

## Important inventory/economy invariants

- Same-owner container moves do not change ownership or create economic transactions.
- Ownership-changing transfers require registered source/destination custody, correct current owner, exact stack and destination capacity.
- Whole-stack transfers preserve stable identity and item state; partial operations must be explicit and preserve correct identity semantics.
- Clean CARD is a non-negative integer balance. Physical Dirty Cash is an inventory item and cannot fund legal retail.
- Business catalogue entries are authored unlimited availability; they are not a finite physical shop inventory.
- Contract rewards debit a real resident account; do not mint money or create a business account.
- Save the stable IDs and domain state required to reconstruct relationships; rebuild Unity references and runtime container registrations from authored content/live objects.
