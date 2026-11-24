# LIVELIHOOD-B-04 — Repair and restoration

**Status:** Design draft; not approved for implementation  
**Parent:** Direction B livelihood roadmap  
**Prerequisite:** Complete the First Workshop Play Mode acceptance (Rhea threads, machine purchase/installation, Reinforced Patch Kit production and F5/F9 persistence).  
**Scope:** One small, player-owned textile-repair loop using the existing hideout workbench and installed Compact Sewing Machine.

## Player outcome

The player finds or acquires a worn textile item, prepares a physical patch kit at the workbench, repairs the item in place, sees its condition improve, and retains the same physical item with its identity and other properties intact.

This extends the existing livelihood loop. It is not a new profession, a generic durability system or a customer-property contract.

## Source audit — current implementation facts

These facts were checked against the current branch:

- `ItemCondition` is declared as `Good = 0`, `Worn = 1`, `Damaged = 2`, `Pristine = 3`. The enum's numeric order is not a repair ladder.
- `ItemInstanceState` is a readonly value type. `PlacedItem` is also a readonly value type, and its state belongs to one physical stack.
- `GridInventory.TryUpdateItemState(Guid, ItemInstanceState)` already replaces the state on an existing stack, preserves its local ID, stable `ItemInstanceId`, definition, position, orientation and quantity, then raises `Changed`. A new split-stack system is not needed for the first target.
- The current Basic Patch Kit and Reinforced Patch Kit definitions both have `MaxStack = 1`. The First Workshop starter grant contains only enough materials for the two Basic kits already reserved by Milo's delivery and the Reinforced recipe.
- `ItemKnowledgeBook` stores the player's known facts separately from authoritative item state. `RecordFact` replaces the fact for one aspect; the other known aspects remain. The item viewer reads this book, not the hidden authoritative state.
- `SaveMapper` already persists physical item state and item-knowledge facts in the unversioned 0.01 save snapshot. A repair round-trip test can use the existing save shape; no save-version field or migration is needed.
- `ContainerLootProfile` creates loot using each `ItemDefinition.DefaultInstanceState`; it has no per-loot state override. A dedicated worn garment definition can therefore be authored with default condition Worn and added to an existing loot profile without adding a new loot-state mechanism.
- Shop buyback prices are currently fixed per item in `BusinessDefinition.BuybackEntry`. `Shop.TryFulfilExact` pays the supplied fixed `ShopOrder.UnitPrice`; the sale record stores item state but does not calculate payment from condition. `ItemValueCalculator` currently provides an indicative reference valuation used by item inspection, not a dynamic sell-price rule.
- `AssetTransferService` handles theft by changing the item's state to `ItemOwnershipStatus.Stolen` and transferring the `AssetOwnershipBook` owner from the previous actor to the thief. A stolen item can therefore be repaired under the proposed player-owned rule when the player is its current registered owner; repair must preserve the Stolen marker and must not change the owner.

## Proposed first-slice rules

These are recommendations for approval, not implementation instructions.

### Repair eligibility and station

- Repair only a physical item currently in the player's carried inventory.
- `AssetOwnershipBook` must identify the player as the item's current owner.
- Allow an item whose `ItemOwnershipStatus` is `Stolen` when the player is its current economic owner. Repair must preserve Stolen status; it must not launder provenance or change ownership.
- Repair is available at the existing hideout workbench only after the Compact Sewing Machine is installed.
- Customer-owned/entrusted items are deferred. They require custody, temporary possession and return rules and should not be mixed into this first loop.
- No timers, queues, random failure, skill checks or tool durability. Invalid attempts fail without changing inventory, ownership, knowledge, money or item state.

### Explicit condition ladder

Never calculate the next condition by subtracting or adding enum integers. Use explicit allowed transitions:

| Current condition | Basic Patch Kit | Reinforced Patch Kit |
|---|---|---|
| Damaged | Proposed: Worn | Proposed: Good |
| Worn | Proposed: Good | Needs decision: allow Good, or reserve Reinforced for Damaged only |
| Good | Not repairable | Not repairable |
| Pristine | Not repairable | Not repairable |

The Basic kit would restore one step. The Reinforced kit would offer the larger Damaged → Good restoration. A successful repair changes **Condition only**; Quality, Authenticity and Ownership remain exactly as they were.

### Identity, stacks and transaction

- The first repairable garment must be authored with `MaxStack = 1`. This prevents accidentally repairing every unit in a state-sharing stack and avoids building general split-stack support for one feature.
- The repair operation targets one stable `ItemInstanceId`, consumes one selected physical kit and uses `GridInventory.TryUpdateItemState` to replace only the target's condition.
- Preserve item identity, inventory position, quality, authenticity, ownership, count and all unrelated inventory state.
- Validate station/upgrade, target, explicit condition transition, player ownership and kit before committing. Failed validation changes nothing.
- Do not add a dedicated repair history ledger unless the implementation audit identifies a real player-facing or persistence need.

### Player knowledge and persistence

- On successful repair, update the player's Condition fact for that same `ItemInstanceId` using `ItemKnowledgeSource.DirectInspection` and full confidence. This replaces a stale Worn/Damaged belief without wiping knowledge of quality, authenticity or ownership.
- The item viewer must immediately display the new condition and refreshed known reference valuation.
- Add a current-build save/load round-trip test: repair a known item, capture/apply the save, and assert the same stable ID, new authoritative condition and matching DirectInspection knowledge fact. Also assert the other known facts remain unchanged.
- Preserve the existing unversioned 0.01 save policy.

### First content and repeatable supply

- Add one dedicated textile garment definition with `MaxStack = 1` and default condition Worn (for example, a second-hand work jacket). It does not currently exist in `WorkshopItems/`.
- Place the first test garment in an authored scavenging profile or sell it through an existing resident/business. A scavenging drop is the smaller first fixture; the profile already uses the item's authored default state.
- The initial First Workshop starter materials are already committed to the two Basic kits and Reinforced recipe. They are not a renewable repair supply.
- For a repeatable repair loop, expose the required raw materials (Fabric Roll, Thread Spool and Adhesive Tube) through an existing authored retail catalogue with fixed prices, using the current unlimited-merchandise `RetailService`. This reuses existing retail and does not add stock simulation, automatic restocking or a new supplier system.
- The actual retailer, prices and first garment source are content choices to settle before implementation.

## Economic value — important unresolved design point

The current `ItemValueCalculator` condition multipliers are:

- Damaged: ×0.45
- Worn: ×0.70
- Good: ×1.00
- Pristine: ×1.10

A Worn → Good repair therefore raises the **reference valuation** by 0.30 × the item's base value (about 42.9% relative to the Worn valuation). However, this does **not** currently increase the CARD paid by a shop: buyback offers use fixed authored prices per item definition. The Basic Patch Kit's base value of 18 is also a reference value, not a verified purchase cost. The project currently has no condition-based sale formula from which to calculate repair profit.

Two possible directions:

1. **Preserve the approved fixed-price economy:** repair improves item condition and inspection/reference value, but does not automatically increase the shop's buyback payment. Profit is not claimed as a feature of this first slice.
2. **Approve a narrow condition-sensitive buyback rule:** one authored buyer pays according to condition for the first repairable textile category. This changes the existing fixed-price-only sell rule and needs separate explicit approval, transaction tests and balance values.

Do not silently change all business buyback offers or treat BaseValue as a shop price.

## Decisions required before implementation

1. Does B-04 need to produce direct CARD profit in the first slice, or should the fixed-price economy remain unchanged for now?
2. Confirm the proposed kit mapping: Basic repairs Damaged → Worn and Worn → Good; Reinforced repairs Damaged → Good. Should Reinforced also be allowed for Worn → Good?
3. Approve the first worn garment and supply source (recommended: one worn garment in a scavenging profile, with repeatable raw repair materials sold through an existing authored retail catalogue).

## Out of scope

Customer-owned repairs, repair contracts, multiple textile types, general-purpose stack splitting, repair skill/quality rolls, random failure, tool durability, repair queues, item degradation over time, condition-based prices for every business, provenance laundering and a new profession/upgrade tree.
