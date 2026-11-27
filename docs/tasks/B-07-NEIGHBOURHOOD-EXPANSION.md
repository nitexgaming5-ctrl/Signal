# B-07 — Neighbourhood Expansion

**Status:** Specification proposal — awaiting explicit user approval before implementation.  
**Strategic parent:** Direction B — meaningful everyday livelihood.  
**Branch:** `chore/collaboration-workflow-and-audit`  
**Prerequisites:** B-01–B-06 livelihood foundations; P-03 stack/inventory foundations. P-03.7 is user-verified closed.

## 1. Player outcome

Expand Signal's compact starter block into a small, connected neighbourhood that gives the player practical reasons to leave the hideout, visit places repeatedly, source materials, scavenge and use existing livelihood systems.

The objective is **not** a larger map for its own sake. The expansion must create a useful loop:

1. Leave the hideout and travel through a recognisable connected street.
2. Reach a new destination that supplies or accepts goods using existing retail/inventory rules.
3. Revisit outdoor spaces and their persistent searchable containers.
4. Return home to repair, produce, store, fulfil work and develop the hideout.

Keep the district compact and authored. Do not add a generic city generator, universal economy simulation or empty buildings.

## 2. Current world baseline

The current `CityBlockBootstrap` builds a compact connected world with these location IDs:
- `Hideout`
- `Street`
- `CornerShop`
- `Apartment`

The bootstrap owns the visible location roots and transitions. `CityNavigationMap` models location regions, obstacles and explicit door connections; `CityNavigationPathfinder` handles local routes. NPC travel routes use the same place/connection graph. The player, GameSession, inventory, clock and world simulation persist while visible roots change.

The current outdoor street includes the hideout, corner-shop and apartment doors, a bench, trees and three persistent searchable street bins. The existing architecture should be extended rather than replaced.

## 3. Recommended first expansion

### Proposed geography: Market Lane and its courtyard

Add one compact outdoor extension attached directly to the existing `Street`, provisionally named `MarketLane`. It should feel like the next block around the corner, not a disconnected scene or a large open map.

The recommended vertical slice contains:

1. **Market Lane outdoor segment**
   - A short connected continuation of the existing street.
   - A small courtyard/alcove that creates a distinct place to walk through and revisit.
   - Clear pedestrian routes, readable entrances and a few grounded environmental landmarks.
   - No decorative maze, inaccessible frontage or oversized empty space.

2. **One enterable second-hand/materials shop**
   - A practical destination for repair and production supplies.
   - Uses the existing authored business stock, retail transaction and inventory transfer systems.
   - Stock is predefined by role; no ingredient supply chain, dynamic demand or automatic restocking simulation.
   - Its exact name, stock list and whether it buys player goods are content decisions to confirm before implementation. The recommended default is a modest fixed stock of existing repair/production materials and a small set of ordinary second-hand goods.

3. **Two persistent public searchable containers**
   - Reuse the current searchable-container and stack/ownership/persistence systems.
   - Give them deliberate, accessible placements that reward a return visit without making scavenging the centre of the expansion.
   - Use unique stable container IDs and authored loot profiles; no respawn loop or random loot economy.

This gives the player a reason to travel for supplies, a reason to revisit the lane, and a practical route back to the workshop. It does not require a new gameplay mechanic.

## 4. Existing systems to reuse

| Existing system | B-07 use |
|---|---|
| `CityBlockBootstrap` | Add location roots, entrances, transitions and outdoor composition |
| `CityNavigationMap` / pathfinder | Register the new region, obstacles and connected door graph |
| `NpcPresenter` travel routing | Keep routes valid across the expanded location graph; do not author new schedules or residents in this phase |
| `RetailService` / authored stock | Run the new shop with fixed, role-appropriate stock |
| `GridInventory`, `AssetTransferService`, `StackTransferService` | Preserve physical item movement, ownership, quantities and lineage |
| Searchable containers and save mapper | Persistent public scavenging points with stable IDs |
| GameSession and F5/F9 | Preserve player location and relevant world/container state across save/load |
| Existing repair, production and customer-work systems | Make sourced materials useful after the player returns home |

Do not create parallel inventory, ownership, retail, transaction, NPC-awareness or save systems.

## 5. Explicit non-goals and guardrails

- No new named residents, backstories, dialogue arcs or story cast. The user wants the main storyline/cast designed before expanding authored character narratives.
- No new customer-contract or repair-order rules. Existing B-02/B-06 systems remain authoritative.
- No automatic NPC shopping, universal supply/demand, passive income, business simulation or global market pricing.
- No new crime, police, witness, heat or confiscation mechanics.
- No weather/cold-exposure implementation; W-01 remains separate.
- No new skill effects or milestone triggers.
- No changes to stack sizes, item balance or the one-active-contract rule.
- No art-pack or wearable-presentation work; the art boundary remains in force.
- No disconnected interior scenes, map generator, additive-scene architecture or new external dependencies.
- No new gameplay mechanic may be implemented until it has been explained and explicitly approved by the user.

## 6. Technical implementation proposal

### B-07a — World graph and layout
- Add a single new outdoor place ID and a compact root built by the existing bootstrap.
- Extend the current street bounds/camera handling only as much as required.
- Add one explicit connection from `Street` to `MarketLane`.
- Add one explicit connection from `MarketLane` to the proposed shop interior.
- Keep all connection endpoints walkable with the pathfinder's actual clearance.
- Ensure local and cross-place NPC routes can traverse the new graph even though no new NPC schedules are introduced.
- Preserve existing Hideout/Street/CornerShop/Apartment IDs and transitions.

### B-07b — Practical destination
- Add one shop interior using the existing interior/bootstrap conventions.
- Connect it to the existing retail authority and authored stock definitions.
- Reuse existing item definitions only. Any genuinely new item, service, business rule or stock behavior is a separate proposal.
- Do not make the shop depend on a newly authored named NPC. Confirm the existing retail interaction entry point before choosing its trigger.

### B-07c — Persistent public scavenging
- Add two stable container definitions and unique IDs.
- Reuse current searchable-container initialization, ownership, stack transfer and save/load paths.
- Ensure loot persists after partial search, close/reopen, save/load and return to the area.
- Do not introduce respawn or time-based replenishment.

### B-07d — Integration and polish
- Add modest, readable environmental composition and entrance cues using existing project assets/procedural primitives.
- Ensure camera bounds, collision, navigation obstacles, door prompts, player placement and return positions are coherent.
- Keep the current test-resident rendering experiment separate from actual resident identity and schedules.
- Update documentation and handoff after each coherent implementation slice.

## 7. Persistence and lifecycle requirements

- Current place and player position must restore to a valid walkable point after F5/F9.
- Existing locations must remain compatible with their current location IDs.
- New searchable-container state must be keyed by stable unique IDs and survive save/load without duplicate item identities.
- Shop stock must follow the existing authored-stock persistence policy; do not invent a second stock ledger.
- NPC social state, relationship state and schedules must not be reset by changing the visible location.
- Returning from the new shop must place the player at the correct Market Lane entrance; returning to the current street must preserve the existing door semantics.
- If any location is missing or malformed during restore, fail safely to the established fallback rather than placing the player outside walkable bounds.

## 8. Acceptance criteria

### World and navigation
- The player can walk continuously from Hideout → Street → MarketLane → shop and return without a scene reload or loss of session state.
- Every doorway has a correct prompt, entry point, exit point and walkable navigation connection.
- Existing Hideout, CornerShop and Apartment routes still work.
- Camera bounds do not reveal unintended empty space or cut off the new area.
- No collision traps or pathfinding dead ends.

### Retail and inventory
- The shop opens through the existing retail interaction path.
- Buying/selling uses existing accounts, ownership and transaction ledgers.
- Purchases route to accessible backpack/pockets correctly, including when the backpack is dropped.
- Stack quantities, item states and ownership remain correct.

### Searchable containers
- Both new containers can be searched, closed and resumed.
- Contents persist across F5/F9 and revisits.
- Partial stacks, split/merge and pocket routing preserve stable IDs, ownership and quantity.
- No respawn occurs unless a separate future design is approved.

### Regression
- Existing repair, production, customer contract and repair-order loops remain intact.
- Existing resident schedules, travel routes and social state remain intact.
- Existing backpack drop/recovery and dropped-content isolation remain intact.
- Full EditMode suite and Play Mode acceptance are required before closing B-07.

## 9. Test plan

### EditMode
- New place/connection graph validity and route reachability.
- Existing and new location IDs survive save/load.
- Searchable-container definitions have unique IDs and valid loot profiles.
- Container state capture/restore preserves partial-search state and item identities.
- Retail stock and transaction integration use existing authorities.
- Regression coverage for dropped-backpack access and stack ownership/lineage.

### Play Mode
1. Start a fresh game: no automatic debug cargo; use F1 only when test items are needed.
2. Walk from Hideout through Street into MarketLane and the shop; return through every doorway.
3. Verify collision, camera framing, prompts and pathfinding.
4. Buy a repair/production material; confirm the correct destination inventory and ownership.
5. Sell an eligible item if the shop is configured to buy goods; confirm accounts and inventory.
6. Search each new container, take a partial stack, close/reopen, save/load and revisit.
7. Repeat relevant receipt and search tests with the backpack dropped; no hidden backpack cargo may become accessible.
8. Run existing repair, production, contract, repair-order, storage and retail regression flows.

## 10. Decisions required before implementation

The recommended proposal above is not yet approved. Confirm or change these content choices:

1. **Geography:** one connected Market Lane plus a small courtyard, attached to the existing Street.
2. **Main destination:** one enterable second-hand/materials shop using existing retail and fixed authored stock.
3. **Scavenging:** two persistent public searchable containers in the new outdoor area.
4. **Residents:** no new named residents, backstories or schedule changes in B-07.
5. **Mechanics:** reuse existing systems only; no new gameplay mechanics in this phase.

Once the user approves the scope, implementation can proceed in B-07a through B-07d. Any proposed mechanic outside the existing systems must be presented separately and wait for explicit approval.
