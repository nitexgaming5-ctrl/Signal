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
- EconomicTransaction: immutable completed-transfer record.
- EconomicTransactionLedger: append-only runtime ledger with duplicate-ID protection.
- AssetTransferService: whole-stack transfer that validates current ownership, uses existing InventoryTransfer, changes owner and records the result.
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
