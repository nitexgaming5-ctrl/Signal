# Design — NPC life simulation v1

## Goal

NPCs should feel like people with lives that continue when the player is not interacting with them. Their public occupation should not reveal everything about them. The simulation must be inexpensive and deterministic on ordinary hardware.

## Approved technical approach

Use authored character content plus a small deterministic utility brain. Do not run a language model or remote AI service for each NPC.

- Core owns decision logic and need values as plain C#.
- Game owns the visible character, movement, proximity interaction and dialogue presentation.
- The brain evaluates possible activities every 15 in-game minutes.
- Need values drift once per elapsed in-game minute.
- The first prototype tracks only energy and social needs.
- No random decisions, background worker, networking or per-frame decision scoring.
- A character's richer simulation is justified by their importance and the current playable scenario; not every citizen needs equal detail.

## First character: Milo, neighbourhood baker

Milo is a working baker in the corner shop. The player sees him at work, taking a break, spending time with neighbours after closing, and heading home.

Activities:
- **Work:** preferred between 06:30 and 18:00.
- **Rest:** becomes more attractive as energy falls, with an additional midday preference.
- **Socialize:** becomes more attractive as social need falls, especially between 18:00 and 21:00.
- **Home:** strongly preferred from 21:00 to 06:30.

The utility brain has a small inertia bonus so it does not switch activities whenever two scores are nearly equal. The exact scores and need drift are implementation tuning, not a promise of final balance.

Milo's dialogue changes with his current activity. His evening line hints that the bakery is not his whole life, without exposing a complete secret or forcing the player into a criminal path.

## First prototype boundaries

Included:
- Deterministic activity selection from world time and two simple needs.
- Activity-specific destination points across the shared city: bakery interior, street bench, evening street corner and apartment interior.
- Movement while the street is active; when the player returns after time has passed elsewhere, Milo resumes at the location appropriate to the current activity.
- Proximity prompt and activity-aware conversation.
- Core tests for routine choices, determinism and clock rewind; a Game integration test for talking and observing an activity change.

Not yet included:
- Persistent relationship values or memory.
- Secret discovery flags, rumours or evidence.
- A concrete side business, request, delivery, trade, reward or failure outcome.
- NPC schedules for the entire population.
- Simulation of distant districts or a crowd manager.

The next playable slice should connect Milo's ordinary public role to a discoverable private opportunity and a meaningful player outcome, while keeping both in the same city and social network. Detailed consequences should be chosen to support that scenario, not added as generic subsystems.

## Verification

The implementation and tests are committed to the shared working branch. Unity compilation, EditMode tests and Play Mode behaviour have not yet been verified in the Editor.
## Economic system — approved foundation (2026-11-19)

Signal has one connected city economy shared by residents, businesses, suppliers, institutions and the player. Economic actors own assets; ownership is independent of the inventory or storage location currently holding an asset. Transfers are authoritative operations, atomic across affected state, and recorded for later accounting, investigation and world-event use.

### Money rules
- Clean money exists as a bank/card balance. Legitimate purchases use this balance; ordinary clean physical banknotes are not part of the initial design.
- Dirty money exists as a physical inventory asset. It is excluded from legitimate bank/card payments and cannot be deposited directly.
- Dirty money can be used for explicitly illicit cash-based transactions, including shady markets and bribes.
- A future cleaning operation consumes dirty-money items and credits the clean bank/card balance directly. Cleaning is a separate mechanic and is not part of the first transfer milestone.
- The initial city uses one currency. Monetary amounts will use integer minor units.

### Economic simulation scope
- Do not implement universal supply/demand, automatic price fluctuations or continuous consumption simulation initially.
- Ordinary goods do not need continuously simulated stock.
- Use authored/simple availability for ordinary shop goods.
- Finite stock is not part of Phase 3. Which goods, if any, should have finite stock and how they restock remain undecided until a concrete gameplay need is designed.
- Businesses, suppliers and residents will eventually participate in the same shared economy. The simulation should be bounded and event-driven rather than simulating every purchase.

### Future systems
Stock markets, lottery, gambling/betting, business ownership, laundering and richer economic events are future systems that must reuse the shared actor, asset, account, transfer and transaction foundations. They are not included in the initial implementation.


## Clean money and dirty money separation (approved 2026-11-19)

- Clean money is held in the player's bank/card account and is the payment source for legitimate purchases and ordinary clean expenses.
- Dirty money is represented by physical inventory items. It is not a balance in the bank/card account, cannot pay for legitimate purchases and cannot be deposited directly.
- A future cleaning operation will consume dirty-money items and credit the clean account directly. It is not implemented in this phase.
- Dirty money will have separate future uses in explicitly illicit exchanges such as shady markets and bribes.
- The account and physical item paths are separate APIs. A clean-account credit does not accept an inventory item or infer value from one.
- The prototype retains integer money units and existing placeholder values; currency denomination and display formatting remain undecided.
- Universal supply/demand, automatic price movement and continuous consumption remain out of scope. Ordinary business merchandise is an authored unlimited catalogue for now; whether any goods later need finite stock remains undecided.


## Physical containers and progressive scavenging — approved 2026-11-20

### Player experience
The player finds physical bins in the city and searches them like someone rummaging through a real container. Each bin has a distinct authored loot profile. Contents are unknown until searched, and the player chooses what to keep.

### Rules
- First interaction generates contents once. Item stacks, stable IDs and randomized reveal order are saved; closing/reopening does not reroll.
- Pressing Search starts an automatic sequence, revealing one item at a time until the container is fully searched or the player closes the interface.
- Revealed items remain physically in the container and can be taken individually. Undiscovered contents stay hidden.
- Closing mid-search preserves the exact next reveal. Reopening resumes without restarting or regenerating.
- Container and player inventory panels are open together. Double-clicking or dragging/dropping a discovered item transfers that stack to carried inventory.
- A failed transfer (for example, insufficient space) leaves the item in the bin.
- No respawn or replenishment in this first slice.

### First test slice
- Three authored loot profiles A/B/C.
- Two or three physical bins placed in Street.
- Existing ItemDefinitions and GridInventory are reused; loot profiles are test fixtures, not a final economy or rarity system.

### Boundaries
Tarkov is an experience reference for progressive container discovery, inventory management during search and separate item examination. Do not import its skill progression, timed container resets, raid rules, secure-container rules or loot economy. No third-party package, Asset Store dependency or copied game code is required.


## Physical containers and progressive scavenging — implementation details

- Street contains three physical bins with stable IDs and authored profile assignments A/B/C.
- The first interaction generates each bin once. The actual generated stacks and randomized reveal order are persisted; profile randomness is not replayed on load.
- The player presses Search once. The UI reveals one item stack at a time (current placeholder interval: 0.85 real seconds). This interval is presentation tuning, not an in-game search skill or timed respawn.
- The container and carried inventory are visible side by side. Only discovered container stacks are rendered. Right-click opens the existing read-only item viewer; double-click transfers a discovered stack to the first free carried slot; drag/drop places it at the selected carried-inventory cell.
- A successful take is an economic asset transfer from the bin's world actor to Player. The item instance ID, state and ownership record are preserved/updated through the shared transfer service. If the selected placement does not fit, no source or ownership mutation occurs.
- Search start, reveal order and discovered count persist. Closing pauses the sequence; reopening resumes automatically. F5 can save while the panel is open; F9 closes it and restores the saved state.
- Save schema is v12. No respawn or replenishment is included.


## First crime action — resident-owned apartment container theft (2026-11-20)

The user chose theft from a physical container inside an apartment as the first crime action. Stealing directly from an NPC (pickpocketing, torn references, etc.) is deferred until interaction, stealth, aggression and related foundations exist.

- The first content fixture is Rhea's wardrobe. Rhea is not hardcoded into the crime system: a reusable `ResidentPropertyContainerDefinition` names the owner and location, and future `StealingHouse_NPC` content can reuse the same framework.
- The wardrobe contains fixed authored items (ITEM_A ×2 and ITEM_C ×1), not a randomized scavenging profile. It uses the existing progressive container search UI and never respawns its contents.
- A successful take is a Theft asset transfer from `resident:rhea` to Player. The transfer preserves ItemInstanceId and all non-ownership state, marks `ItemOwnershipStatus.Stolen` atomically and records an economic transaction with reason Theft. Public street bins remain Scavenging.
- Only a successful theft emits a separate `CrimeOutcome`. The first editable prototype tuning is +2 heat and +1 global criminal-progression point per stolen stack.
- At most one nearest resident who is physically present in the authored location and within the 8-unit witness/awareness radius receives the anonymous observation. The witness's knowledge is character-scoped and persists through save/load. Ambient social encounters do not share crime observations until witness-sharing rules are approved.
- No automatic report, police response, heat-band consequence, relationship penalty or suspicion is introduced. Unwitnessed theft still receives its authored prototype heat/progression awards but creates no witness knowledge.
- Heat and long-term criminal progression are separate: the existing HeatMeter cools using its current placeholder daily decay; progression is a global non-decaying total with no unlock thresholds yet.
- These values are testable prototype tuning, not final balance. Dirty-money exchange/cleaning and production are not part of this first theft slice.

Implementation and acceptance criteria: `docs/tasks/CRIME-THEFT-001-RESIDENT-CONTAINER-THEFT.md`. Unity verification remains pending.


## Crime economy — Slice B: physical Dirty Cash and contact-specific cleaning (approved 2026-11-20)

- Add one discreet contact to the existing shared city. The stable internal NPC ID is `Clean_Dirtycash`; the visible display name is intentionally blank. The internal ID must never appear in player-facing text.
- Dirty Cash is a physical inventory item, not an account balance. One unit has face value 10.
- The contact offers one fixed fence purchase for exactly 2 stolen, player-owned ITEM_A items, paying 3 Dirty Cash units. A legitimate copy is not accepted. These are authored fixture values in `DirtyCashContactOffer_SliceB.asset`.
- The same contact offers a separate cleaning action. Its offer asset sets a 20% fee; each Dirty Cash unit credits 8 clean CARD to the player and 2 CARD to the contact's existing NPC account.
- The player sees the exact accepted item, Dirty Cash reward, fee and clean payout before confirming.
- Fence and cleaning transactions are atomic and separately recorded. Failed capacity, ownership, validation or account operations leave the original items and balances unchanged.
- Dirty Cash cannot be used for legal retail or credited directly to CARD. Existing legal retail and buyback rules do not change.
- The first slice has no cleaning delay, risk, heat, witness, police or progression effect. Additional contacts/offers/rates and consequences require separate design approval.
- No black-market stock simulation, dynamic prices, bargaining, passive laundering or separate underground city.

Detailed implementation and test specification: `docs/tasks/CRIME-ECONOMY-SLICE-B-DIRTY-CASH.md`.


## Direction B — meaningful everyday livelihood (approved 2026-11-21)

**Strategic direction:** make the player's everyday life meaningful through production, customer contracts, practical hideout development, relationships and services. Legal livelihood is a satisfying path alongside crime, not a replacement for the shared-city crime pillar.

### B1-B3 first workshop decisions

- **Workshop model:** both player-operated and NPC-owned production are part of the intended model. Start with a player-operated workshop in the hideout; keep the technical model compatible with explicitly authored NPC-owned production businesses later.
- **Contracts:** one accepted/active customer contract at a time in the first implementation.
- **First hideout upgrade:** a practical upgrade whose first gameplay effect is unlocking one additional authored recipe/capability.
- **Production output:** physical inventory goods created through authored recipes, not abstract currency or a progress counter.
- **Production economy:** authored recipes and fixed authored contract rewards; reuse existing clean CARD accounts and transaction boundaries.
- **No universal economy simulation:** no supply/demand, automatic NPC shopping, passive sales/revenue, finite stock simulation or dynamic pricing is introduced by this direction.

### Design boundaries still requiring answers

- First product, recipe ingredients, output quantities and item definitions.
- Whether production has any duration in the first slice. Current recommendation: immediate, with no queue/timer until a demonstrated gameplay need.
- First contract issuer, required goods, quantity and fixed reward.
- Issuer account funding and whether the issuer is a resident or authored business owner.
- Whether contracts accept only legitimate goods or any player-owned goods.
- Whole-stack versus partial-stack contract delivery and the receiving/consumption destination.
- Upgrade acquisition condition, cost, exact physical change and second recipe.
- Whether a dedicated production or contract ledger is needed after auditing existing transaction records.

No detailed recipe, contract content, cost, item-provenance acceptance or deadline/penalty rule is approved until the user answers these questions. See **docs/tasks/LIVELIHOOD-B1-B3-FIRST-WORKSHOP.md** and **docs/OPEN_QUESTIONS.md**.
