# LIVELIHOOD-B-02 — Single active customer contract

**Status:** specified; not started  
**Parent:** LIVELIHOOD-B1-B3-FIRST-WORKSHOP.md  
**Depends on:** B-01 production loop; NPC identities, inventories, clean accounts, ownership and transaction ledgers.

## Player outcome

The player accepts one authored customer contract, produces the requested physical goods, hands them over and receives the agreed clean CARD reward.

## Contract model

The first contract is authored content, not generated demand. It contains:
- Stable contract identity.
- Issuer/customer actor identity and player-facing display information.
- Required ItemDefinition and positive quantity.
- Fixed integer clean CARD reward.
- Authored availability/context.
- Runtime lifecycle state: Offered, Active, Completed; cancellation is excluded unless separately approved.
- Stable record of acceptance and completion needed for save/load and audit.

Only one contract may be Active at a time. The first slice contains one authored offer; it does not imply a procedural contract board.

## Implementation tasks

### B02.1 — Core contract state and validation
- Define a small Core contract state model with stable IDs and explicit legal transitions.
- Reject acceptance if another contract is already active or the offer is invalid/already completed.
- Repeated acceptance must not create duplicate active state.
- Keep authored definitions separate from mutable runtime state.

### B02.2 — Issuer and payment authority
- Resolve the issuer to an existing resident/business economic actor and registered clean account.
- Contract reward is a fixed clean CARD amount.
- Do not mint funds or create implicit overdraft. The issuer must have sufficient balance at fulfilment.
- Credit player and debit issuer atomically with the goods handover.
- If a dedicated contract ledger is introduced, justify it against existing economic ledgers; record contract ID, issuer, player, goods, quantity, reward and time.

### B02.3 — Physical fulfilment
- Fulfilment uses actual carried goods, not an abstract checkbox/counter.
- Validate required ItemDefinition, exact quantity, player ownership and carried-container custody.
- First slice should require exact whole-stack delivery; do not add stack splitting/partial delivery incidentally. If exact quantity across stacks or partial stack handling is needed, resolve it as an explicit question first.
- Validate item-state/provenance acceptance only after the user answers the open question; do not assume stolen goods are accepted or rejected.
- Goods transfer to an authored receiving inventory or are consumed by a clearly defined transaction. Do not silently destroy goods without specifying the receiver/consumption rule.
- Commit goods transfer, account debit/credit, contract state and history atomically.

### B02.4 — Player-facing contract interaction
- Present customer/issuer, requested item and quantity, reward and conditions before acceptance.
- Show one active contract clearly in a compact contract panel or interaction surface.
- Provide deliberate Accept and Fulfil actions; reviewing must never commit.
- Revalidate active contract, issuer identity, carried goods, account balance and interaction context at commit time.
- On success, show physical handover and exact payment result.
- No generic quest journal rewrite.

### B02.5 — Persistence and tests
- Persist active/completed contract state and transaction identity as required.
- Add round-trip and migration tests for the actual serialized shape.
- Test second active contract rejection, repeated acceptance, wrong/insufficient goods, wrong ownership, insufficient issuer funds, success, duplicate fulfilment and rollback.

## Acceptance criteria

1. Exactly one authored customer contract is available in the first content slice.
2. Only one contract can be Active at a time.
3. Player sees exact goods, quantity and reward before accepting.
4. Goods are physically handed over from carried inventory.
5. No payment or goods are lost on failed validation.
6. Successful fulfilment transfers correct goods and clean CARD atomically.
7. Repeated fulfilment cannot duplicate payment.
8. Active and completed contract state survive save/load.
9. Existing retail and NPC account behavior remains unchanged.

## Not included

Procedural contracts, multiple active contracts, deadlines, penalties, reputation/favour changes, recurring customers, partial delivery, negotiation, dynamic rewards or a general-purpose quest system.
