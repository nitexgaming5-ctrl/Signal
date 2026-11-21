# LIVELIHOOD-B1-B3 — First Workshop vertical slice

**Status:** design specification prepared; implementation not started  
**Direction:** approved Direction B — meaningful everyday livelihood  
**User decisions:** workshop model = both (player-operated first, NPC-owned compatible later); one active contract; first practical hideout upgrade unlocks another recipe/capability.

## Player outcome

The player operates a modest production workshop from the hideout, obtains materials, makes authored goods, accepts one customer contract, physically hands over the required goods, receives clean CARD, and invests in a practical workshop upgrade that unlocks another recipe. The architecture must not prevent an authored NPC-owned business from operating a production workspace later.

This is a connected livelihood loop, not a universal economy, idle business simulator, or generic quest framework.

## Scope and sequencing

1. **B-01 — Production foundation and player workshop:** authored production recipes, transactional crafting integration, one usable hideout production station and player-facing production UI.
2. **B-02 — Customer contracts:** one active accepted contract, authored customer/issuer, physical fulfilment and clean CARD payment.
3. **B-03 — Practical workshop upgrade:** one persistent practical upgrade whose concrete effect is unlocking one additional authored recipe/capability.

B-01 must be playable before B-02 is integrated. B-02 must be playable before B-03 is connected to the livelihood loop. Work may be implemented in substantial batches, but each task has its own tests and acceptance criteria.

## Approved design decisions

- The first operating workshop belongs to the player and is located in the existing hideout.
- The long-term model supports both player-operated workshops and NPC-owned production businesses. NPC-owned production is a compatibility requirement, not a requirement to ship a second complete business in this first slice.
- Production is authored: recipe assets describe exact inputs and outputs.
- Production creates real physical inventory items. It is not an abstract balance, job-progress number or instant reward detached from inventory.
- The first slice uses the existing clean CARD account and transaction architecture.
- Exactly one contract may be accepted/active at a time.
- The first practical hideout upgrade unlocks one additional authored recipe/capability.
- Existing Core crafting, item identity, inventory, ownership, account and save foundations must be inspected and reused.
- No new package, third-party code, global supply/demand simulation or automatic NPC shopping.

## Player loop

1. Obtain required materials through existing approved sources (starting inventory, scavenging or authored purchases).
2. Interact with the hideout production station.
3. Inspect an available recipe, its inputs and output.
4. Confirm production; validate inputs and output capacity before committing.
5. Consume required materials atomically and create the physical output.
6. Review the first authored customer contract and accept it if no other contract is active.
7. Produce the requested goods and physically submit eligible player-owned output.
8. Validate the contract, transfer goods and clean CARD payment atomically.
9. Use the existing hideout upgrade interaction to unlock the second recipe.
10. Save/reload and confirm workshop, unlock and contract state remain coherent.

## Technical direction

### Core and transaction boundaries
- Keep domain rules in plain C# under Signal.Core; no UnityEngine references.
- Reuse Recipe, Ingredient, Crafting.TryCraft, GridInventory, snapshots, stable ItemInstanceId, ownership services and economic account/ledger primitives.
- Audit the current crafting API before changing it. It currently consumes/creates by ItemType and inventory count; it does not itself validate AssetOwnershipBook, distinguish item-instance states for ingredient selection, or record a production transaction. Do not assume those guarantees already exist.
- Add only the smallest production transaction boundary needed for ownership-aware, atomic input consumption/output creation and a distinct production history if existing ledgers do not represent it.
- A failed production attempt must leave ingredients, outputs, ownership and history unchanged.
- Output creation uses the authored ItemDefinition's normal default state unless a later approved recipe rule says otherwise.
- No production duration, queue, quality roll, skill check, worker assignment, spoilage or recurring cost is included in the first slice.

### Authoring and Game integration
- Reuse RecipeDefinition and its ScriptableObject authoring pattern. Add validation and editor-facing error messages where needed.
- Reuse the existing WorkbenchWorkspace interaction surface, but replace/extend its current presentation-only placeholder behavior through explicit GameSession bindings. Do not create a parallel workshop scene or singleton.
- The first workshop is an authored player-operated workspace. Keep ownership/operator identity explicit in definitions/runtime composition so an NPC-owned workspace can be added later without rewriting Core production rules.
- UI is presentation only; GameSession/Core revalidate recipe, station, inputs, output capacity, ownership and upgrade unlock at commit time.

### Contracts and money
- A contract is authored content with stable identity, issuer/customer identity, required ItemDefinition and quantity, fixed clean CARD reward and a clear lifecycle.
- The first implementation supports one accepted/active contract at a time. It does not imply an unlimited procedural contract generator.
- Contract fulfilment is a physical handover from the player's carried inventory, not a counter increment. It validates exact required goods and player ownership, then transfers/consumes the goods and pays clean CARD.
- Use existing resident/business clean-money accounts and economic transaction patterns. Never mint money or bypass the payer's account. The first issuer must have a registered account and sufficient funds; define the first authored payer and balance only after the user approves the content.
- Item provenance/state acceptance, partial stack delivery, cancellation, expiry and contract penalties remain open design questions. Do not silently add them.
- A successful fulfilment is atomic: if goods cannot be accepted, the payer cannot pay, or any ledger step fails, inventory, ownership, balances, contract state and history roll back together.

### Persistence
- Persist accepted/active/completed contract state and stable contract/issuer identifiers as required by the final model.
- Persist recipe unlocks and workshop upgrade state.
- Persist production and contract transaction histories if new histories are introduced.
- Rebuild runtime workspace/container references from authored scene definitions; never serialize Unity object references.
- Determine the actual save-schema increment after identifying the minimum new serialized fields. If new fields are required, bump the current v14 schema and add round-trip/migration tests. Do not predeclare a version number in code or docs before inspecting SaveMapper.
- Preserve current-format save integrity and test the existing v14 migration path.

## Explicitly out of scope

- NPC-owned production gameplay/content in this first vertical slice (only keep the architecture compatible).
- Multiple simultaneous active contracts, procedural contract generation, recurring contracts or automatic customer demand.
- Repair/restoration, multiple professions, farming, water processing or fictional drug production.
- Production timers, queues, quality/skill rolls, production costs beyond authored ingredients, spoilage or staffing.
- Business stock simulation, automatic sales, NPC shopping, supply/demand, dynamic prices, passive revenue or business ownership progression.
- Relationship/trust/favour rewards for contracts; these belong to a later approved social slice.
- New districts, police response or additional crime systems.

## Acceptance criteria across B1-B3

1. The player can use the existing hideout workshop interaction to view and perform an available authored recipe.
2. Inputs are validated and consumed exactly once; outputs are physical inventory items.
3. Missing inputs and insufficient output space fail without mutation.
4. Production cannot duplicate output through repeated confirmation.
5. Production preserves correct player ownership and item identity/state semantics.
6. The first authored contract can be accepted only when no other contract is active.
7. The contract displays its issuer, required goods, quantity and fixed reward before acceptance.
8. The player can physically fulfil it with eligible carried goods; unrelated or insufficient goods are rejected.
9. Contract goods and clean CARD payment commit atomically; failed fulfilment changes nothing.
10. The player can complete the first practical hideout upgrade and see the second recipe become available.
11. Upgrade/unlock and contract state survive save/load.
12. Existing inventory, retail, selling, scavenging, theft, Dirty Cash and NPC behavior remain unchanged.
13. Core rules have focused EditMode tests; Unity Game integration has relevant tests where the current test architecture supports them.
14. Unity compilation, full EditMode suite and the complete Play Mode loop are user-verified before the initiative is marked complete.

## Verification script

- Start in HideoutSlice with the authored player workshop.
- Confirm the initial recipe is available and the locked recipe is unavailable.
- Attempt production with missing inputs; confirm no mutation.
- Attempt production with insufficient output space; confirm no mutation.
- Produce the required output; confirm exact input consumption, output count, ownership and one history record.
- Accept the first contract; verify a second acceptance is blocked while one is active.
- Attempt fulfilment with insufficient/wrong goods; confirm no item/account/contract mutation.
- Fulfil with the exact eligible goods; verify goods handover, payer debit, player clean CARD credit and completed state.
- Acquire and apply the first practical upgrade; verify the additional recipe unlocks.
- Save/reload during an active contract and after completion; verify state, balances, inventory and unlocks.
- Regression-check existing retail BUY/SELL, scavenging, theft, Dirty Cash contact and city NPC routines.

## Open decisions before implementation

See OPEN_QUESTIONS.md section **LIVELIHOOD-B1-B3**. Do not invent the first product, recipe ingredients, customer/issuer, reward, upgrade cost, item-state eligibility or partial-delivery policy.
