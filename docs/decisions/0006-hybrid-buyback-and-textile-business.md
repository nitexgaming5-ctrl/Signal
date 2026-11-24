# 0006 — Hybrid buyback catalogue and first textile business

Date: 2026-11-24  
Status: approved by the user; authored prototype values remain tunable  
Scope: bounded B-04 hybrid catalogue foundation and the first Rhea-operated textile repair/restoration loop.

## Catalogue decision

Business catalogues have separate merchandise and buyback layers:

- Merchandise purchase prices remain fixed and authored. Existing unlimited catalogue availability remains.
- A buyback entry is either `Fixed` (the default and unchanged legacy behaviour) or `ByCondition` (four authored CARD offers for Damaged, Worn, Good and Pristine).
- Condition-based prices are configured per business and per exact `ItemDefinition`; they are resolved from the selected physical stack's authoritative `ItemInstanceState`.
- The retail sell list resolves the offer for each stack. The actual sale re-resolves the current state before committing. The existing `ShopSaleTransaction` records the realized unit price and sold state.
- Existing fixed offers stay fixed. `ItemValueCalculator` remains indicative reference valuation and does not set shop payout.

This does not approve a general dynamic market: no time, stock, demand or global-category pricing; no finite stock/restocking simulation; no automatic NPC shopping; no condition-based merchandise purchase price.

The generic code foundation was committed in `79143557c26c3dd1cf291027ecbc677aaaec80e4`. The implementation needs a fresh Unity compile and full EditMode run before it is treated as verified.

## First textile business

Rhea operates the first textile buyer/supplier using her existing resident identity and work routine:

- Name: Rhea's Textile Counter
- Business ID: `rhea-textile-counter`
- Owner: `resident:rhea`
- Location: `CornerShop`, inside the existing bakery interior; interaction point `(996.7, 999.45)` is separate from Rhea's work point `(1001.2, 1000.4)` and the Bakery interaction point `(1000, 1000.6)`.
- Hours: 09:00–18:00, aligned with her authored work routine.

Fixed merchandise prices (prototype tuning):

| Item | Price |
|---|---:|
| Fabric Roll | 6 CARD |
| Thread Spool | 4 CARD |
| Adhesive Tube | 4 CARD |
| Fastener Pack | 4 CARD |

First repair target: one Work Jacket `ItemDefinition`, Clothing category, 2×2, `MaxStack = 1`, Standard quality, Worn condition, Genuine, Legitimate, base reference value 60. Add it at low weight to the existing `LootProfile_A` street scavenging source, using the item's default state.

Rhea's condition-based buyback offer (prototype tuning):

| Physical condition | Offer |
|---|---:|
| Damaged | 24 CARD |
| Worn | 36 CARD |
| Good | 52 CARD |
| Pristine | 58 CARD |

The schedule is an authored buyer policy, not a formula derived from BaseValue. Only the Work Jacket uses it in the first slice.

## Repair rules

- Station: existing hideout workbench, available only once Compact Sewing Machine is installed.
- Eligibility: one carried physical item whose current registered economic owner is the player. Customer-owned/entrusted items are excluded.
- Basic Patch Kit: Damaged → Worn; Worn → Good.
- Reinforced Patch Kit: Damaged → Good only.
- Good and Pristine items cannot be repaired.
- Repair is immediate and deterministic, consumes exactly one physical kit and changes Condition only. Preserve stable item identity, placement, count, Quality, Authenticity and Ownership.
- A stolen garment can be repaired only after its current registered owner is the player; preserve the Stolen provenance marker. Repair never launders provenance.
- On success, refresh only the player's Condition fact for the same instance ID using `DirectInspection` at full confidence. Preserve other known aspects.
- Use current-build save/load round-trip tests; keep the unversioned 0.01 save model.

The raw material cost of a Basic kit is 14 CARD at these prototype retail prices; the Reinforced recipe's ingredient replacement cost is 24 CARD. Worn → Good raises Rhea's offer by 16 CARD; Damaged → Good raises it by 28 CARD. The numbers create a small repair incentive while keeping the market bounded. They are tuning values, not final game balance.

## Delivery gate

The hybrid lookup/UI/sale framework, Work Jacket asset, Rhea business asset, `LootProfile_A` entry, active-scene registration, visible CornerShop counter, Core `RepairService`, workbench repair UI and focused pricing/repair/save tests are committed. Unity verification is pending; the 417/417 user report predates these changes. Run a fresh full EditMode suite, complete the remaining First Workshop Play Mode checks and then verify the full repair/material purchase/condition-priced sale/save-load loop as specified in `tasks/LIVELIHOOD-B-04-REPAIR-RESTORATION.md`.

## Rationale

A condition-sensitive buyback gives the repair activity a visible economic consequence without changing every shop. Keeping purchase prices and legacy buyback entries fixed preserves the current authored-economy model. Rhea already has a mending identity, work routine, account and relationship/narrative context, so the textile counter extends an existing character instead of adding an isolated business.