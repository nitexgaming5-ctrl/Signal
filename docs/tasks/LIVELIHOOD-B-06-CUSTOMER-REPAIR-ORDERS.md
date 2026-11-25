# B-06 — Customer Repair Orders

**Status:** First implementation slice committed; Unity compilation, EditMode execution and Play Mode acceptance are pending.

## Goal

Extend the existing repair/restoration loop into a real customer service: a resident entrusts the player with one physical item, the player repairs it at the hideout, returns that exact item, and receives clean CARD payment.

This is a generic service foundation using Milo only as the current framework/test resident. Do not add story residents, backstories or cast content.

## Approved first content

- Issuer: existing resident Milo (milo).
- Item: one Work Jacket, seeded in Milo's issuer-owned physical inventory.
- Starting condition: Worn.
- Required return condition: Good (Pristine is also accepted as a better result).
- Repair material: one player-owned Basic Patch Kit; the existing repair service changes Worn → Good.
- Reward: 35 clean CARD, paid from Milo's existing resident account.
- Interaction: F opens the repair-order panel; existing E customer contract interaction remains unchanged.

## Binding rules

1. The entrusted jacket has one stable ItemInstanceId and remains owned by Milo throughout the job.
2. Accepting the order moves the exact stack from Milo's inventory to player custody. It does not change AssetOwnershipBook and does not create an asset-transfer transaction.
3. The player repairs the entrusted item through the existing RepairService; only a player-owned patch kit is consumed. The repair path is explicitly authorized for the registered issuer-owned target.
4. Returning the order moves the same physical instance back to Milo's inventory without changing its owner.
5. Payment is one atomic clean CARD transaction: debit Milo, credit the player, record one ServicePaymentTransaction, and complete the order. Failure restores both inventories and both balances.
6. The service-payment ledger is separate from EconomicTransactionLedger; a service fee is not an ownership-changing asset transfer.
7. Only one customer job may be active at a time across the existing customer contract and this repair order.
8. Active/completed order state, item custody location, ownership, resident/player balances and service-payment history survive F5/F9.
9. Replay cannot return the completed order or award a second payment. The payment ledger also rejects a second payment for the same order ID.

## Implemented files

- Assets/_Project/Core/CustomerRepairOrders.cs — order lifecycle, custody transfer, atomic return/payment and replay guards.
- Assets/_Project/Core/ServicePaymentTransactions.cs — dedicated service-payment transaction and ledger.
- Assets/_Project/Core/RepairService.cs — owner-aware repair overload; existing player-owned repair API remains compatible.
- Assets/_Project/Game/CustomerRepairOrderDefinition.cs and CustomerRepairOrderPanel.cs — authored order and focused UI.
- Assets/_Project/Game/WorkbenchWorkspace.cs — allows only the active entrusted target in addition to player-owned targets.
- Assets/_Project/Game/GameSession.cs — Milo composition, F interaction, order flow and save/load wiring.
- Assets/_Project/Content/Contracts/RepairOrder_Milo_WorkJacket.asset — first authored order.
- Assets/_Project/Scenes/HideoutSlice.unity — active scene assignment.
- Assets/_Project/Core/SaveData.cs — order and service-payment persistence.

## Focused EditMode coverage added

Ten tests cover:
- ownership remains with Milo during acceptance and repair;
- exact item return, issuer debit, player credit and one payment transaction;
- replay rejection;
- unrepaired item cannot be returned or paid;
- insufficient funds rollback;
- full receiver inventory rollback;
- duplicate acceptance and wrong-owner rejection;
- active order/entrusted ownership save-load;
- completed service payment and returned item save-load.

Expected suite count is 455 if the previous 445-test baseline is unchanged. This is a forecast only; the user must run Unity Test Runner before the result is called verified.

## Play Mode acceptance

- Open Milo's repair order with F; accept the Worn jacket.
- Inspect inventory/ownership: the same jacket is carried, but its owner is still Milo.
- At the hideout workbench, use one Basic Patch Kit; the jacket becomes Good and the kit is consumed.
- Return to Milo with F; confirm the exact jacket is back in Milo's inventory, Milo loses 35 CARD, the player gains 35 CARD and exactly one service-payment record exists.
- Attempt replay; no second payment is possible.
- Test insufficient funds/full receiver failure paths through EditMode tests.
- F5/F9 once while the order is active and once after completion; verify item location/owner, balances, order phase and payment transaction.
- Confirm an active repair order and active ordinary customer contract cannot overlap.

## Explicit exclusions

No new NPC cast, multiple concurrent customer jobs, partial item custody, repair queues/timers, repair quality rolls, automatic pricing, customer reputation reward tuning or universal profession simulator.
