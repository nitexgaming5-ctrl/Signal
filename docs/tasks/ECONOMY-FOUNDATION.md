# ECONOMY-FOUNDATION — shared city economy

**Status:** Phases 1–2 implemented and user-verified (316/316 EditMode tests); Phase 3 player-facing slice awaiting detailed design approval  
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


## Phase 3 — first player-facing illicit exchange (design gate)

The technical economy foundations should now be exercised by a small, complete player interaction rather than expanded into a broad business simulator.

### Recommended target
Let the player carry physical dirty-money items and spend them in one discreet local exchange. The clean card balance remains exclusively for legitimate purchases. The exchange should transfer real inventory assets and use the shared ownership/transaction foundation.

### Decisions required before implementation
- Physical form of dirty money and the denomination/stack-value convention.
- First exchange context, counterparty and received asset/service.
- Whether the slice includes cleaning now or only illicit spending. Cleaning is approved as a future operation that consumes dirty-money items and credits the clean card balance directly; rate, prerequisites, risk and presentation remain open.
- Whether the exchange has any immediate exposure/consequence. Do not invent heat, police response or criminal progression.

### Explicit exclusions
- No direct dirty-money deposit into CleanMoneyAccount.
- No dirty-money acceptance by legitimate shop/card purchase APIs.
- No universal supply/demand, automatic price fluctuation or continuous consumption.
- No general market/business simulator, stock market, lottery or betting feature in this slice.
- No denomination, cleaning rate or risk tuning invented by implementation.

### Exit criteria (once design is approved)
- Dirty-money items are ordinary physical inventory assets with stable identity and explicit owner.
- The approved illicit exchange consumes/transfers the physical item stack atomically and records the economic transaction.
- Save/load preserves the resulting inventory and ownership state.
- Player-facing feedback makes clear that the card balance was not used.
- Core rules have EditMode coverage; Unity compilation and a short Play Mode script are verified by the user.
