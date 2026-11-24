# LIVELIHOOD-B1-B3 — First Workshop vertical slice

**Status:** implementation and starter content committed; user reports **417/417 EditMode tests passed (2026-11-24)**; partial Play Mode loop confirmed; final supplier/upgrade/save acceptance pending  
**Direction:** approved Direction B — meaningful everyday livelihood  
**User decisions:** player-operated workshop first (NPC-owned compatible later); one active contract; first practical hideout upgrade unlocks another recipe.

## Player outcome

The player operates a modest hideout workshop, produces useful physical goods from authored recipes, accepts one customer request, hands over the requested goods, receives clean CARD and invests in a practical upgrade that unlocks another recipe.

This is one connected livelihood loop, not a universal economy, idle business simulator or generic quest framework.

## Delivery packages

The implementation is divided into three linked cards. Their file-level work and acceptance criteria live in the child cards:

1. [B-01 — Production foundation and player workshop](LIVELIHOOD-B-01-PRODUCTION-WORKSHOP.md)
2. [B-02 — Single active customer contract](LIVELIHOOD-B-02-CUSTOMER-CONTRACTS.md)
3. [B-03 — Practical hideout workshop upgrade](LIVELIHOOD-B-03-PRACTICAL-WORKSHOP-UPGRADE.md)

B-01 provides the playable production station. B-02 connects an authored customer and physical handover. B-03 makes the first practical workshop upgrade part of the same loop.

## Binding design rules

- Core owns domain rules and transaction validation; GameSession composes services and authored content; UI is presentation/input only.
- Recipes are authored and contain explicit ItemDefinition inputs/outputs with positive integer quantities.
- Production is immediate and transactional. It consumes player-owned ingredients and creates physical output in the same inventory.
- Failed production leaves inventory, ownership and history unchanged.
- The first slice supports exactly one accepted/active contract. The contract terms are visible before acceptance.
- Fulfilment uses the exact required whole stack from the player's carried inventory. The goods transfer to the issuer's physical receiving inventory.
- The issuer pays a fixed clean CARD reward from their existing resident account. The first issuer is Milo; do not create a separate business account or mint funds.
- The first upgrade is a one-time practical purchase from Rhea. It installs a visible Compact Sewing Machine at the hideout workbench and unlocks the Reinforced Patch Kit recipe.
- NPC-owned production remains a future compatibility requirement, not a second workshop deliverable in this slice.

## Authored content

The active scene is wired to the starter catalogue: Fabric Roll, Thread Spool, Adhesive Tube and Fastener Pack; Basic Patch Kit and Reinforced Patch Kit; two immediate recipes; Milo's Basic Patch Kit request; and Rhea's Compact Sewing Machine offer.

Exact quantities, prototype values, account balances, item/recipe details and the user-facing verification script are maintained in [the Starter Catalogue](../content/STARTER_CATALOGUE.md). Content values are provisional, replaceable fixtures, not a universal price or stock policy.

## Persistence and save policy

Current unversioned 0.01 save snapshots preserve the persistent workshop state: inventories and item instance states, production history/operator, contract lifecycle, receiver inventory/identity, account balances, acquired upgrade IDs and NPC agenda progress.

There is deliberately no schema-version field, migration chain or old-save compatibility promise in this build. Current-build save/load round trips are mandatory for persistent changes; older experimental saves may be discarded when the data shape changes.

## Current verification

- The user reports **417/417 full EditMode tests passed on 2026-11-24**, including the latest supplier-panel, agenda and unversioned-save work.
- The user previously confirmed workbench access, production, selling and Milo's customer-request handover in Play Mode.
- Remaining acceptance: Rhea's rumour and supplier threads, machine purchase/installation, mending follow-up, Reinforced Patch Kit unlock/production, and F5/F9 persistence of balances, inventories, contract, agenda choices, installed upgrade and recipe unlock.
- Do not call B-01 to B-03 complete until the user confirms the remaining in-game flow.

## Exit criteria

1. The hideout workbench makes the Basic Patch Kit from exact player-owned materials and creates a physical output.
2. Missing inputs, insufficient output space, invalid ownership and duplicate confirmation fail without mutation.
3. Only one customer contract can be active; the player sees terms before accepting.
4. Exact eligible goods are handed over to the issuer's physical inventory; payer debit and player credit are atomic.
5. The Compact Sewing Machine can be purchased from Rhea once, visibly installed and persisted.
6. The Reinforced Patch Kit recipe remains locked until acquisition, then becomes craftable.
7. Current-build save/load preserves balances, inventories, contract, upgrade and unlock state.
8. Existing retail, scavenging, theft, Dirty Cash and NPC behaviour remain intact.
9. Full EditMode and the complete Play Mode loop are verified in Unity.

## Out of scope

NPC-owned production gameplay/content, multiple active or procedural contracts, partial-stack handover, production timers/queues/quality rolls, automatic NPC shopping, business stock simulation, dynamic prices, passive revenue, relationship/favour rewards, repair/restoration, new crime systems and new districts.

Repair/restoration is tracked separately in [B-04](LIVELIHOOD-B-04-REPAIR-RESTORATION.md). It must not be implemented as part of this vertical slice.
