# ECONOMY-FOUNDATION — shared city economy

**Status:** Approved initiative; Phase 1 Core foundation added 2026-11-19  
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
- Pull the branch, allow Unity to compile, then run the full EditMode suite.
- Report any compiler or test failures before proceeding to Phase 2.


## Phase 2 — clean account and economic persistence (implemented; verification pending)

- Added CleanMoneyAccount as the explicit clean bank/card balance. Existing PlayerStats Money/Earn/TrySpend APIs delegate to it; rent and current debug sale therefore remain clean-money operations.
- Dirty money is not part of this account and has no direct deposit path. It remains a physical item direction for later illicit payments and cleaning.
- Save v8 stores cleanBalance, carried/storage asset owner mappings and completed economic transfer records.
- v1-v7 migration copies legacy money to cleanBalance unchanged. Legacy carried/storage assets default to player ownership.
- GameSession owns/composes AssetOwnershipBook, EconomicTransactionLedger, EconomicContainerRegistry and AssetTransferService; player inventory and home storage are registered as player-custodied containers.
- HUD balance label now explicitly reads CARD.
- Added tests for clean account operations, v7 migration, economic state round-trip and malformed economic records.
- User confirmed 304/304 tests for Phase 1. Phase 2 code is not yet Unity-verified; run compilation and the full EditMode suite after pulling.

### Phase 2 boundaries / remaining work
- No dirty-money ItemDefinition or denomination convention has been authored.
- No cleaning, shady market, bribe, shop purchase, supplier, finite stock or player-facing economy interaction is implemented.
- Currency symbol/denomination and actual starting balance remain undecided.
