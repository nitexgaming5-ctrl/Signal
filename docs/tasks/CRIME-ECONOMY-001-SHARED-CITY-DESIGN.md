# CRIME-ECONOMY-001 — Shared-city crime, dirty money and production

**Status:** broad design approved; Slice A resident-container theft implementation committed, awaiting Unity verification. Slices B–D remain design-stage.  
**Date:** 2026-11-20  
**Scope:** shared-city crime/economy architecture and staged implementation. The first crime action is specified in [CRIME-THEFT-001](CRIME-THEFT-001-RESIDENT-CONTAINER-THEFT.md).

## 1. Purpose and player outcome

Signal needs a connected livelihood model in which legal work, criminal activity, physical goods, NPC relationships and money flows coexist in one city. This initiative designs the seams between three areas before implementation:

1. Crime, exposure, heat and consequences.
2. Dirty money as a physical inventory asset, its illicit uses and a deliberately bounded cleaning path.
3. Legitimate production as an authored capability for selected businesses, not a universal requirement.

The target experience is a city activity where a player can take a risk, create information that may spread through people, receive a physical reward, decide how to use it, and eventually convert some proceeds into legitimate purchasing power.

The first implementation should be a small vertical slice. The full crime catalogue, full police simulation and broad production economy are explicitly not the target.

## 2. Binding decisions already approved

- Legal and illegal activities coexist in the same shared city and social network. They are parallel lifestyles; neither locks out the other.
- All criminal activity contributes to one global criminal progression track, not separate crime skill trees.
- Serious criminal failure may have harsh and lasting consequences; exact rules remain undecided.
- Dirty money is a physical inventory item. It is not a clean account balance and cannot pay for ordinary legitimate purchases or be deposited directly.
- Dirty money may be used in explicitly illicit exchanges, including shady markets and bribes.
- A cleaning operation consumes dirty-money items and credits the player's clean CARD account directly.
- Residents have personal clean-money accounts. Business revenue from ordinary retail credits the owner's account.
- Existing business retail is authored, fixed-price and unlimited for now. No universal supply/demand, dynamic pricing, automatic NPC shopping or continuous economy simulation.
- Consequential world developments must use the shared WorldPacingDirector only after valid scenario context and phase ownership are designed. It must not govern routine events.

## 3. Newly approved design boundaries

The user approved these boundaries on 2026-11-20.

### Crime and heat
- Consequences are gradual and authored, not an immediate full police-response simulator.
- Heat represents accumulated pressure; it is not proof that an authority knows about a particular incident.
- Exposure/knowledge and heat must remain distinct concepts.
- Specific thresholds, consequences and recovery rules remain proposals until separately approved.

### Dirty-money cleaning
- Cleaning is offered through authored contacts/business interactions.
- Each contact defines its own offer terms (rate and/or fee); there is no universal conversion rate.
- Cleaning is an explicit transaction that consumes physical dirty-money items and credits clean CARD.
- Exact offer structure, prerequisites, timing, risks and denomination remain open.

### Production
- Production exists only for explicitly authored businesses or workspaces that need it.
- A business that sells authored merchandise does not automatically require ingredients, skills, suppliers, stock or production.
- No city-wide supply/demand or continuous production simulation is introduced.

## 4. Conceptual model

### 4.1 Crime action → exposure → heat → consequence

Keep four separate layers:

1. **Crime action:** an authoritative gameplay operation with actor, action category, location and game time. It may succeed, fail or be interrupted.
2. **Exposure evidence:** what was observable and what a particular person or institution could learn. A crime does not automatically become universally known.
3. **Heat:** a bounded global pressure value/progression input derived from authored crime/exposure outcomes. Heat is not an evidence database and does not identify a witness.
4. **Consequence:** an authored scenario/business/contact/world response selected because its context is valid and its conditions are met. A heat band may make a consequence eligible, but does not itself spawn one.

An unwitnessed action may contribute to criminal progression or some heat if its authored risk says so, but must not create an omniscient witness record. A witnessed action creates only locally observable facts; the observer's knowledge remains character-scoped and may be shared later through existing social-world rules. Reporting, investigation or institutional awareness needs explicit authored pathways.

**Recommendation:** use semantic crime outcomes emitted by the authoritative activity, rather than inferring crime from item movement, UI text or arbitrary inventory differences. Existing economic transactions remain authoritative for ownership and money; they may be accompanied by a crime outcome when the activity's design says it is criminal.

### 4.2 Exposure is not a global broadcast

Reuse the current NPC social foundation:
- Publish only observable event descriptions.
- Preserve location, time, visibility and source.
- Give knowledge to eligible observers, not every NPC.
- Rumours retain source/confidence and do not become objective truth.
- A witness does not automatically report; reporting is a separate authored reaction/opportunity.
- Do not add autonomous police AI or continuous city-wide investigation in the first slice.

Institutional awareness should be explicit knowledge held by an institution/contact actor or scenario state, not by making every resident omniscient.

### 4.3 Heat and global criminal progression are different

- **Heat:** current accumulated pressure; it can cool down according to approved rules.
- **Criminal progression:** long-term advancement from criminal activity; it should not automatically decay with daily heat.
- **Exposure:** specific information and its spread; it may persist even after heat cools.

The existing Core HeatMeter is a simple 0–100 value with a default 10-point daily decay and currently has no gameplay readers. Treat the decay as placeholder tuning until the user approves how it should work. The old Q16 value is not a complete heat design.

### 4.4 Dirty money remains a physical asset

- Represent dirty money using an authored ItemDefinition and normal stable item-instance identity, ownership and inventory persistence.
- Its economic value must be explicit and integer-based; do not infer it from the provisional ItemValueCalculator.
- Illicit payments validate exact player-owned dirty-money quantity/value, recipient/context, and destination capacity or stock rules as applicable.
- Legitimate RetailService and CleanMoneyAccount must never accept dirty-money items as payment.
- Cleaning is a distinct transaction: validate exact input assets and offer, consume dirty items, credit clean CARD, and record a durable transaction atomically. Any failure leaves both inventory and account unchanged.
- Never silently convert dirty money during ordinary sale, deposit, save/load or account credit.

**Open design point:** whether one stack represents a fixed denomination or a variable value-per-unit item. Recommendation: fixed value per unit for the first slice, with partial quantity selection only if existing inventory/transaction APIs can support it safely; otherwise require whole-stack cleaning initially. No denomination is approved yet.

### 4.5 Context-specific illicit exchanges

Do not resurrect the superseded Phase 3 illicit-seller proposal. A future shady contact may offer a specific authored exchange, but it should use shared ownership/inventory foundations and have its own acceptance rules. It is not a generic vendor or universal black-market system.

For the first slice, keep the illicit payment simple: a specific authored contact accepts a defined dirty-money amount for a specific authored outcome. No dynamic haggling, universal stock simulation or NPC shopping.

### 4.6 Cleaning as an authored service

A cleaning offer belongs to a particular contact/business definition and may define:
- Accepted dirty-money item definition(s).
- Minimum/maximum accepted amount or whole-stack rule.
- Fixed fee or retained percentage, authored per offer.
- Clean CARD payout calculation using integer arithmetic.
- Any prerequisite, time delay or exposure condition.
- Whether the offer is repeatable and its scenario cooldown/limit.

The player must see input amount, fee/retention and exact clean CARD payout before confirming. The operation is explicit and irreversible after confirmation. No automatic laundering, passive conversion or global conversion table.

**Recommendation for first implementation:** fixed retained percentage per authored offer, immediate transaction, no random failure and no generic risk roll. Add time/exposure consequences only when a concrete scenario supports them. This is a recommendation, not an approved final rate or rule.

## 5. Production boundary

Production is a capability, not a mandatory business lifecycle.

A production-capable authored business/workspace may define:
- A stable production recipe ID.
- Exact ItemDefinition inputs and quantities.
- Exact ItemDefinition outputs and quantities.
- Optional duration or interaction steps only where the player-facing activity needs them.
- Output ownership and destination container.
- Whether the recipe is available at that location and under what authored conditions.

For the first production slice:
- Use an explicit player interaction at a physical workspace.
- Validate all inputs and output capacity before committing.
- Consume inputs and create outputs atomically.
- Record resulting asset/transaction changes through existing economic ownership foundations.
- Keep the recipe list authored and small.
- Do not add employee skills, automatic production queues, suppliers, delivery schedules, finite shop stock, passive revenue or universal business simulation.

A bakery may continue selling its authored Bread catalogue without flour or production. A later player-facing bakery production activity can be authored separately if it adds a meaningful loop.

## 6. Recommended vertical-slice sequence

Do not implement all three domains as parallel frameworks. Use this sequence:

### Slice A — crime/exposure foundation
**Player outcome:** complete one authored crime-related action and see a locally grounded exposure outcome without omniscient city knowledge.

**First action chosen by the user:** steal a physical item from a resident-owned container inside an apartment. NPC theft/pickpocketing, torn references, aggression, weapons and stealth are deferred until their interaction foundations exist.

The first implementation is Rhea's wardrobe, but the reusable property definition references its owner and location through authored data; the Core crime and transfer systems are not hardwired to Rhea.

Minimum:
- A semantic crime outcome contract and global criminal progression track in Core.
- One fixed-content resident property container using existing GridInventory, stable item identity, ownership/custody and save foundations.
- A successful transfer uses the existing Theft reason and marks the item Stolen within the atomic transfer boundary; street-bin Scavenging remains unchanged.
- A successful theft emits one CrimeOutcome, authored heat/progression awards and at most one local witness event.
- A witness is selected from residents physically present in the authored location and within the witness and NPC-awareness radii. Knowledge is delivered to that character only and is restored after load.
- No automatic reporting, police response, relationship penalty or suspicion from an unwitnessed event. The existing ambient social-encounter exchange skips `crime-observed` events until witness-sharing/reporting rules are designed and approved.
- Tests for successful/failed transfer, stolen item state, no-observer, one-observer, out-of-radius, duplicate outcome and current-format save/load.

Initial prototype tuning on Rhea's wardrobe is +2 heat, +1 global criminal-progression point per successfully stolen stack, and an 8-unit witness radius. These are serialized authoring values for playtesting, not final balance. Existing HeatMeter daily cooling remains placeholder tuning; criminal progression does not decay and has no unlock thresholds yet.

**Implementation status:** code/content/test batch committed; awaiting Unity compilation, EditMode and Play Mode verification. Full details and acceptance checks: [CRIME-THEFT-001](CRIME-THEFT-001-RESIDENT-CONTAINER-THEFT.md).

Exit: a small authored scenario produces a crime result, only the selected local observer learns the observable fact, and heat/progression outputs and save/load are deterministic and testable.

### Slice B — dirty-money payment and cleaning
**Player outcome:** receive dirty money from an authored illegal opportunity, spend some at an illicit contact, and clean an explicitly offered amount into CARD.

Minimum:
- One authored dirty-money ItemDefinition and value convention.
- Safe exact-quantity removal/transfer/payment path with ownership checks.
- One specific illicit exchange offer.
- One specific cleaning offer with visible terms and atomic item-to-CARD conversion.
- Separate durable transaction records for illicit exchange and cleaning; do not overload retail or asset-transfer ledgers with misleading semantics.
- Current-format save/load round-trip tests for physical dirty money and transaction history.
- No universal shady market or multiple contacts until the first flow works.

Exit: a complete end-to-end Play Mode loop with inventory, account, ownership and save/load integrity.

### Slice C — authored production
**Player outcome:** use one physical workspace to consume authored inputs and create an authored output.

Minimum:
- One recipe and one production-capable authored location/business.
- Core validation/transaction logic, Game interaction and visible result.
- Atomic input consumption/output creation and ownership records.
- Tests for missing inputs, insufficient capacity, invalid recipe, success, rollback and save/load if any new state persists.

Exit: one production interaction works; ordinary retail-only businesses remain unaffected.

### Slice D — integrated scenario
**Player outcome:** undertake one short opportunity that connects criminal activity, exposure, dirty-money reward/payment/cleaning, and an ordinary legal activity or production option.

The scenario should demonstrate that legal and illegal paths can be combined without locking the player into a class or faction. Use the existing opportunity/discovery framework where appropriate; do not create a generic quest journal as a prerequisite.

## 7. Architecture and reuse

Reuse before abstraction:
- HeatMeter is an existing Core primitive but currently has no readers; inspect its save/restore and day-start integration before changing it.
- NpcSocialWorld, NpcWorldEvent, NpcKnowledge, NpcPerception and directed relationships provide local awareness and uncertain knowledge.
- WorldPacingDirector is only a policy gate; do not integrate until a scenario phase owner and valid consequential candidate are defined.
- AssetOwnershipBook, EconomicContainerRegistry, AssetTransferService, EconomicTransactionLedger and stable item IDs provide ownership/custody/transfer safety.
- CleanMoneyAccount and EconomicAccountBook provide clean CARD accounts.
- RetailService, RetailTransactionLedger and ShopSaleTransactionLedger remain separate and unchanged unless a concrete requirement demands a carefully designed extension.
- GridInventory, InventoryTransfer, item instance state and save mapping remain authoritative for physical goods.
- GameSession remains the composition root; Core stays free of UnityEngine.

Do not introduce a universal transaction engine, generic crime framework or production scheduler until the first slice demonstrates a repeated concrete need.

## 8. Persistence and verification constraints

- Current-format save/load round-trip and transaction integrity are mandatory whenever new persistent state is introduced.
- Increment save version only for serialized schema changes; runtime-only/UI changes do not require it.
- Save physical dirty-money item identity/state/ownership through the existing item and ownership path.
- Save durable cleaning/illicit transaction records if those records are part of player-visible history, scenario state or future consequences.
- Reconstruct runtime container custody from actual inventory references; do not serialize Unity object references.
- Unity EditMode tests, compilation and Play Mode verification must be reported separately. The assistant cannot claim local Unity execution through GitHub.

## 9. Decisions still required

The user has since green-lit implementation of Slice A and selected the first action as theft from a physical apartment container. The following details remain open beyond the initial prototype fixture:

1. Additional crime categories/actions and the exact award rules for each.
2. Final heat values, daily cooling, heat bands and gradual consequences.
3. Whether/how witnesses later share or report information, who receives it, timing and confidence. The first slice only gives one local witness direct knowledge and does not auto-report.
4. Criminal progression award policy across future crime categories and what progression unlocks. The first slice awards one prototype point per successful stolen stack; no unlock thresholds exist.
5. Dirty-money denomination/value convention and first item definition.
6. First illicit contact, accepted amount and exchange outcome.
7. Cleaning fee/retention model, rate, prerequisites, delay and exposure consequences.
8. First production-capable business/workspace, recipe inputs/outputs and whether production is immediate or time-based.
9. Whether Slice B (dirty-money payment/cleaning) or Slice C (authored production) should follow the verified theft slice.

The first action's current serialized prototype tuning is +2 heat, +1 progression point per successful stolen stack, and an 8-unit witness radius. These values are intentionally small and editable; they are not a final balance decision. The existing HeatMeter daily cooling value remains placeholder tuning.

## 10. Explicit exclusions

- No full police AI, pursuits, arrests, raids or city-wide investigation simulator in the first crime slice.
- No omniscient witness or global NPC knowledge broadcast.
- No separate criminal city/map or player legal/criminal class.
- No universal supply/demand, dynamic prices, continuous consumption or automatic NPC shopping.
- No universal black-market vendor, dynamic haggling or generic illicit stock simulation.
- No automatic/passive money cleaning or dirty-money deposit into CARD.
- No mandatory production for ordinary retail businesses.
- No automatic factory queues, employees, skill trees, supplier logistics or passive business revenue.
- No new Unity packages, Asset Store packs or third-party code.
