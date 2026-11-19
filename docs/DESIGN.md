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
- Universal supply/demand, automatic price movement and continuous consumption remain out of scope. Ordinary goods use authored availability; stock is selectively tracked only where it matters.
