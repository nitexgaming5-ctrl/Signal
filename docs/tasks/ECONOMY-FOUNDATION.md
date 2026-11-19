# ECONOMY-FOUNDATION — shared city economy

**Status:** Phases 1–2 implemented and user-verified (316/316 EditMode tests); Phase 3 specification finalized, awaiting explicit implementation approval  
**Branch:** chore/collaboration-workflow-and-audit

## Approved design

The city has one connected economy shared by residents, businesses, suppliers, institutions and the player. Ownership is independent of storage location. Economic operations are atomic and recorded.

Money:
- Clean money is a bank/card balance used for legitimate purchases.
- Dirty money is a physical inventory asset for illicit cash-based uses such as shady markets and bribes.
- Dirty money cannot be used for legitimate card/bank purchases or deposited directly.
- A future cleaning operation consumes dirty-money items and credits the clean balance directly.
- Ordinary clean physical cash is omitted initially.

Simulation:
- No universal supply/demand, price changes or continuous consumption simulation initially.
- Ordinary goods use simple authored availability.
- Track stock selectively where gameplay needs it (e.g. drugs and illegal materials).
- Future stocks, lottery, betting, business ownership, laundering and economic world events reuse shared foundations.

## Phase 1 — asset transfer foundation

### Added
- EconomicActorId: stable Core actor identity.
- AssetOwnershipBook: explicit owner per stable item stack GUID, independent of the containing inventory.
- EconomicContainerRegistry: records which actor is responsible for each runtime inventory container; custody is distinct from asset ownership.
- EconomicTransaction: immutable completed-transfer record.
- EconomicTransactionLedger: append-only runtime ledger with duplicate-ID protection.
- AssetTransferService: whole-stack transfer that validates current ownership, requires registered source/destination containers and a destination custodian matching the recipient, uses existing InventoryTransfer, changes owner and records the result.
- AssetTransferServiceTests: success, preservation, ownership and failure coverage.

### Boundaries
- Same-owner inventory moves remain InventoryTransfer operations.
- Partial stack transfers are not implemented.
- No money, accounts, bank/card, dirty-money item, cleaning, shop, business or stock simulation is implemented in Phase 1.
- Ownership and transaction records are runtime-only. Versioned save integration is a prerequisite for persistent gameplay use and belongs to Phase 2.
- No new UI or Unity scene integration in this Core foundation pass.

### Verification
- Assistant cannot run the Unity Editor or Unity Test Runner.
- User confirmed **304/304 EditMode tests passed** for Phase 1 before Phase 2 began.
- Phase 1 is closed; its ownership/transfer contracts are now extended by the verified Phase 2 save/account composition.


## Phase 2 — clean account and economic persistence (implemented; 316/316 user-verified)

- Added CleanMoneyAccount as the explicit clean bank/card balance. Existing PlayerStats Money/Earn/TrySpend APIs delegate to it; rent and current debug sale therefore remain clean-money operations.
- Dirty money is not part of this account and has no direct deposit path. It remains a physical item direction for later illicit payments and cleaning.
- Save v8 stores cleanBalance, carried/storage asset owner mappings and completed economic transfer records.
- v1-v7 migration copies legacy money to cleanBalance unchanged. Legacy carried/storage assets default to player ownership.
- GameSession owns/composes AssetOwnershipBook, EconomicTransactionLedger, EconomicContainerRegistry and AssetTransferService; player inventory and home storage are registered as player-custodied containers.
- HUD balance label now explicitly reads CARD.
- Added tests for clean account operations, v7 migration, economic state round-trip and malformed economic records.
- User confirmed 304/304 tests for Phase 1, then confirmed **316/316 full EditMode tests** after pulling Phase 2 and the save-version test correction (2026-11-19).
- The initial Phase 2 report was 313/316 because three legacy migration tests still expected version 7. Those assertions now compare against `SaveMigrator.CurrentVersion`; no production code was changed by the correction.
- The assistant did not independently run Unity. No separate economy Play Mode verification has yet been reported.

### Phase 2 boundaries / remaining work
- No dirty-money ItemDefinition or denomination convention has been authored.
- No cleaning, shady market, bribe, shop purchase, supplier, finite stock or player-facing economy interaction is implemented.
- Currency symbol/denomination and actual starting balance remain undecided.


## Phase 3 — first player-facing illicit exchange (spec finalized; awaiting approval)

The detailed implementation specification is in ECONOMY-PHASE-3-ILLICIT-EXCHANGE.md.

User decisions:
- Dirty Cash Bundle is a physical, indivisible item worth 100; MaxStack = 1.
- The seller is a new discreet NPC, separate from Milo, located in the existing shared city.
- First merchandise is a generic Contraband item; its specific identity and effects remain intentionally undefined.

The slice is one seller, one contraband definition and one authored stock unit. Price (an integer number of bundles) and exact placement are content inputs and must not be guessed. No automatic restocking.

Technical safeguards include explicit seller actor/custody registration, whole-item ownership validation, two-sided atomic exchange with complete rollback, a seller stock inventory persisted through save v9, and revalidation at confirmation. The clean CARD balance is not read or changed by the illicit exchange.

**No gameplay code until the user explicitly approves the final specification.**
