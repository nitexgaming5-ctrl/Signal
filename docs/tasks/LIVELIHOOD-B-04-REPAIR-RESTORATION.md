# LIVELIHOOD-B-04 — Repair, restoration and the first textile business

**Status:** Design approved; hybrid buyback, Rhea textile content, repair service and workbench repair UI committed; Unity verification pending.  
**Parent:** Direction B livelihood roadmap  
**Acceptance dependency:** Complete the remaining B-01 to B-03 Play Mode checks as part of the end-to-end B-04 session, including Rhea's agenda threads, Compact Sewing Machine purchase/installation, Reinforced Patch Kit production and F5/F9 persistence.  
**Approved scope:** The first player-owned textile repair loop and one Rhea-operated textile buyer/supplier, using the existing hideout workbench, Compact Sewing Machine, physical patch kits, item condition and hybrid buyback catalogue.

## Player outcome

The player finds a worn Work Jacket, brings it home, uses the upgraded hideout workbench and a physical Patch Kit to restore it, then chooses whether to keep it or sell it to Rhea. Rhea also sells the raw materials needed to keep making repair kits. The same physical garment retains its identity and provenance throughout.

This is a small livelihood loop, not a generic durability system, customer-property repair service or simulated clothing market.

## Current implementation facts and hybrid catalogue foundation

The current branch was source-audited before these decisions were finalised:

- `ItemCondition` is declared as `Good = 0`, `Worn = 1`, `Damaged = 2`, `Pristine = 3`. Enum numeric order is not a repair ladder.
- `ItemInstanceState` and `PlacedItem` are readonly value types. State belongs to one physical stack.
- `GridInventory.TryUpdateItemState(Guid, ItemInstanceState)` replaces state on the exact existing stack while preserving its local ID, stable `ItemInstanceId`, definition, position, orientation and quantity, then raises `Changed`.
- `ItemKnowledgeBook` stores partial player knowledge separately from authoritative physical state. `RecordFact` replaces one aspect's known fact without wiping other aspects.
- `SaveMapper` persists physical item state and item-knowledge facts in the unversioned 0.01 save snapshot. No schema-version field or migration chain is to be introduced.
- `ContainerLootProfile` creates items from `ItemDefinition.DefaultInstanceState`; a dedicated Work Jacket definition authored Worn can be added to an existing loot profile without a new loot-state mechanism.
- `Shop.TryFulfilExact` receives the resolved unit price and records that exact price plus the sold stack's state in `ShopSaleTransaction`. The transaction format already supports the required audit trail; no new sale-history fields or save migration are needed.
- `BusinessDefinition` now exposes `BuybackPricingMode.Fixed` and `BuybackPricingMode.ByCondition`. Existing entries default to Fixed; the state-aware overload resolves the four explicit condition prices.
- `RetailPanel` quotes from the selected stack's state. `GameSession.SellToBusiness` resolves the offer again from the current stack state immediately before the transaction, so a stale UI quote cannot determine the committed price.
- The original fixed-only lookup remains available and does not silently choose a condition price. Merchandise purchases remain fixed-price.
- `ItemValueCalculator` remains an indicative inspection/reference valuation, not the source of the buyer's payout.
- `RepairService.TryRepair` is implemented in Core. It validates the player-owned carried container, exact Work Jacket and patch-kit instances, explicit kit transition and item ownership; it consumes the selected kit, changes only Condition, and records a full-confidence `DirectInspection` condition fact. It rolls back inventory/kit ownership if the state update fails.
- `WorkbenchWorkspace` now presents the repair controls alongside the existing recipe book. `GameSession` wires authored Work Jacket/kit definitions, requires the Compact Sewing Machine and the player to be at the hideout workbench, and routes the operation to `RepairService`.
- The hybrid code and focused `BusinessDefinitionTests` were committed in `79143557c26c3dd1cf291027ecbc677aaaec80e4`. They have not yet been compiled or run in the Unity Editor. The last user-reported 417/417 EditMode result predates that commit.

## Approved catalogue model

The catalogue has three deliberately separate layers:

| Layer | Rule | First textile-business use |
|---|---|---|
| Merchandise | Fixed authored unit price; existing unlimited catalogue behaviour | Rhea sells Fabric Roll, Thread Spool, Adhesive Tube and Fastener Pack |
| Fixed buyback | Fixed authored unit price for that exact item definition | All existing fixed buyback offers continue unchanged |
| Condition-based buyback | Per-business, per-item authored price schedule resolved from the physical stack's current condition | Rhea buys the Work Jacket at a different offer per condition |

Condition pricing is an offer policy, not a market simulation:

- Only a `BuybackEntry` explicitly set to `ByCondition` uses the state schedule. Existing/missing serialized mode fields retain the enum's zero/default `Fixed` behaviour.
- The schedule contains four independent positive CARD prices: Damaged, Worn, Good and Pristine.
- Price is resolved from `PlacedItem.State.Condition`, never from an enum arithmetic operation or item-definition default.
- The sell UI displays the resulting offer amount, not a hidden condition label. The offer can be Rhea's assessment of the garment; it does not automatically grant the player a `DirectInspection` knowledge fact.
- The sale boundary recalculates using the current selected stack. The existing sale transaction stores the realized unit price and complete item state.
- No time-based modifiers, stock levels, finite availability, restocking, demand, category-wide automatic rules or dynamic merchandise purchase prices.
- Do not use `ItemValueCalculator` to calculate CARD payout. Reference value and a particular buyer's offer are separate concepts.

## The first textile business — Rhea's Textile Counter

Rhea is the natural first operator: the existing Rhea definition gives her a 09:00–18:00 work routine and the line, “I still mend things for neighbours now and then. This old sewing machine has seen better days.” This extends established character context rather than creating a new NPC.

**Authored business profile (prototype content):**

| Field | Value |
|---|---|
| Display name | Rhea's Textile Counter |
| Business ID | `rhea-textile-counter` |
| Business type | Textile Repair & Resale |
| Owner | Rhea (`resident:rhea`) |
| Location | CornerShop (the existing bakery interior) |
| Hours | 09:00–18:00 |
| Catalogue | Fixed-price repair materials; one condition-based Work Jacket buyback |

Rhea's counter is authored inside the existing `CornerShop` interior, not on the Street. The business interaction point is `(996.7, 999.45)`, in front of the separate counter on the left side of the room. Rhea's work position is `(1001.2, 1000.4)` and the Bakery interaction point is `(1000, 1000.6)`, so the new prompt is outside both existing interaction radii. `CityBlockBootstrap` now builds a visible textile counter and cloth display. An EditMode regression test checks that the approach point is walkable and reachable from the shop entrance. The supplier/agenda interaction, Bakery prompt and Textile Counter prompt must still be confirmed together in Play Mode.

### Fixed merchandise — repeatable repair materials

| Item | Prototype unit price |
|---|---:|
| Fabric Roll | 6 CARD |
| Thread Spool | 4 CARD |
| Adhesive Tube | 4 CARD |
| Fastener Pack | 4 CARD |

These are authored fixed retail prices. Reuse the current `RetailService`: purchases debit the player and credit Rhea's resident account. Merchandise remains unlimited in this slice; do not add physical shop stock or automatic restocking.

### First repair target — Work Jacket

Create one new `ItemDefinition` under the distinct `Assets/_Project/Content/WorkshopItems/` content path (do not use the case-colliding `Items/` and `items/` folders):

| Field | Value |
|---|---|
| Display name | Work Jacket |
| Category | Clothing |
| Grid size | 2 × 2 |
| Max stack | 1 |
| Base reference value | 60 |
| Default quality | Standard |
| Default condition | Worn |
| Default authenticity | Genuine |
| Default ownership status | Legitimate |

Add it as a low-weight entry to the existing `LootProfile_A` used by a street scavenging container. It uses its own authored Worn default state. Do not create a new loot system, per-drop state override, respawn mechanic or guaranteed reward container. The item is unstackable so the first repair does not need generic partial-stack splitting.

### Condition-based buyback — Rhea's authored offer

Rhea buys only the Work Jacket in this first dynamic catalogue entry. Proposed prototype values:

| Work Jacket condition | Rhea's unit offer |
|---|---:|
| Damaged | 24 CARD |
| Worn | 36 CARD |
| Good | 52 CARD |
| Pristine | 58 CARD |

These are per-business offer prices, not multipliers and not a formula derived from BaseValue. They are replaceable balance values. Other businesses retain their existing fixed offers until a later approved scenario gives them a reason to use condition pricing.

The values create a small, legible reason to repair: Basic Worn → Good improves the buyer's offer by 16 CARD against 14 CARD of raw materials; Reinforced Damaged → Good improves it by 28 CARD against 24 CARD of recipe materials. A Basic Damaged → Worn step alone is not a profit step (12 CARD offer increase against 14 CARD of materials); two Basic kits take Damaged → Good at break-even material cost, while the Reinforced route is the more efficient one-step option. This is intentional prototype tuning, not a promise of a universal profit loop. The player's cost of acquiring the jacket itself is scenario-dependent.

## Approved repair rules

### Station and eligibility

- Repairs apply only to one physical item in the player's carried inventory.
- `AssetOwnershipBook` must identify the player as the item's current economic owner.
- The existing hideout workbench can repair only after the Compact Sewing Machine upgrade is installed.
- The first item must be the Work Jacket described above.
- Customer-owned or entrusted goods are not eligible. Custody, temporary possession and return contracts are a separate future slice.
- The operation is immediate and deterministic: no queue, timer, random failure, skill check, quality roll or tool durability.
- Validate the target, ownership, station/upgrade, explicit transition and selected kit before consuming anything. A failed repair changes nothing.

### Explicit kit transitions

Never use enum integer arithmetic. Apply only this table:

| Current condition | Basic Patch Kit | Reinforced Patch Kit |
|---|---|---|
| Damaged | Worn | Good |
| Worn | Good | Not accepted |
| Good | Not repairable | Not repairable |
| Pristine | Not repairable | Not repairable |

Basic restores one explicit step. Reinforced is the efficient one-step restoration from Damaged to Good; it is not a substitute for the Basic kit on Worn items.

### Atomic state change and provenance

- The Work Jacket is `MaxStack = 1`; target one stable `ItemInstanceId`.
- Consume exactly one physical kit and update the existing jacket stack with `GridInventory.TryUpdateItemState`.
- Change **Condition only**. Preserve Quality, Authenticity and Ownership exactly as they were, along with stable item identity, local ID, position, orientation and quantity.
- A stolen jacket may be repaired only if the player is its current registered owner after the theft transfer. Its `ItemOwnershipStatus.Stolen` marker must remain Stolen; repair must not launder provenance or alter the owner.
- No repair-history ledger is needed for the first slice unless implementation reveals a specific player-facing/persistence requirement.

### Player knowledge and persistence

- After a successful repair, record the new Condition fact for that same `ItemInstanceId` with `ItemKnowledgeSource.DirectInspection` and full confidence. Replace only the Condition fact; preserve known Quality, Authenticity and Ownership facts.
- The item viewer should immediately show the new known condition and updated reference valuation.
- Add a current-build save/load round-trip test covering stable ID, changed authoritative condition, DirectInspection condition knowledge and preservation of all other known facts.
- Keep the current unversioned 0.01 save shape. Do not add version fields or migrations.

## Implementation sequence after the First Workshop gate

1. Authored and wired: Work Jacket (`MaxStack = 1`, default Worn), low-weight `LootProfile_A` entry, Rhea's fixed-material/condition-buyback business, active scene references, and the visible CornerShop counter with a navigation reachability regression test.
2. Implemented: hybrid buyback pricing at quote/sale time; `RepairService` with explicit transitions, player ownership checks, exact kit consumption, condition-only state updates, provenance preservation and condition knowledge refresh; repair controls in the existing workbench UI; GameSession gate requiring the installed Compact Sewing Machine and hideout workbench.
3. Added focused EditMode tests for all repair transitions, invalid use/ownership, exact kit consumption, condition-knowledge preservation and a repaired-item save/knowledge round trip. These tests have not yet run in Unity.
4. Run Unity import/validation and the full EditMode suite. Fix any compile/test failures before beginning Play Mode; the old 417/417 report predates all B-04 changes.
5. Play Mode acceptance: finish the remaining First Workshop Rhea/machine/recipe/F5-F9 checks; acquire a Worn Work Jacket; inspect it; repair Damaged with Basic → Worn, Worn with Basic → Good, Damaged with Reinforced → Good; reject Reinforced on Worn and all repairs on Good/Pristine; verify only the selected kit is consumed and state/provenance/identity are preserved.
6. Confirm Rhea's textile counter, Bakery and Rhea supplier/agenda interactions do not overlap; purchase materials at fixed prices; sell Damaged/Worn/Good/Pristine jackets to verify each condition price and sale-history state/unit price; F5/F9 and verify item state, ownership, player knowledge, accounts and relevant ledgers.

## Out of scope

Global or demand-driven dynamic markets, time-based price fluctuation, finite textile-shop stock, restocking simulation, NPC shopping, universal condition-price rules, dynamic merchandise prices, prices derived from `ItemValueCalculator`, customer-owned repair contracts, additional garment families, stack splitting for repair, random repair failures, repair skills/quality rolls, tool durability, degradation over time, queues, provenance laundering and a generic profession/upgrade tree.