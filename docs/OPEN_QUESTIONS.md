# Open questions

Agents: add questions here with two options and your recommendation. Do not decide them yourself.
User: answer inline with `ANSWER:`. Agents read answers before the next session.
Decisions made in the chat with Claude are copied here so every agent sees them.

## Q1 — Player stats
ANSWER: a (Money and Health only)

## Q2 — Tile and pixel size
ANSWER: b (32×32)

## Q3 — UI framework
ANSWER: a (UGUI)

## Q4 — Grid inventory size for the starting character
ANSWER: a (6×4 cells)

## Q5 — Starting items
ANSWER: a (placeholders ITEM_A, ITEM_B, ITEM_C)

## Q6 — Fonts
ANSWER: a (a free pixel font, not chosen yet). Until then the built-in UGUI font is used (see Q10).

## Q7 — Sizes and stack limits for the placeholder items
ANSWER: a (default, change if you want)
- ITEM_A: 1×1, max stack 5
- ITEM_B: 1×2, max stack 1
- ITEM_C: 2×2, max stack 3

## Q8 — Inventory key
ANSWER: Tab (default; the old prototype used I)

## Q9 — Drag controls
ANSWER: left mouse drags an item, R turns it while dragging, Esc cancels the drag. An invalid drop leaves the item where it was.

## Q10 — M3 text rendering
ANSWER: UGUI `Text` with the built-in font for now. TextMeshPro needs an extra import step; switch in the pixel-font pass.

## Q11 — Where the inventory UI is tested
ANSWER: a new scene `Assets/_Project/Scenes/InventoryDemo.unity`, not the old prototype scene.

## Q12 — Slice scene
ANSWER: a new scene `Assets/_Project/Scenes/Slice.unity` holding one empty GameObject with the `GameSession` component (default, change if you want).

## Q13 — Slice controls
ANSWER: WASD or arrows move, Tab inventory, F6 sleep, F5 save, F9 load, F7 debug sell, F8 debug craft. Function keys are used for actions because letter keys sit in different places on AZERTY keyboards (default).

## Q14 — Clock
ANSWER: starts Day 1 at 08:00, one game minute per real second, sleeping wakes you at the next 06:00. If you sleep after midnight you wake the same calendar day (default, tunable).

## Q15 — Rent
ANSWER: placeholder 50 every 7 days. A missed payment is only counted for now. What it causes (warnings, eviction, a racket) is a design decision still to make.

## Q16 — Heat
ANSWER: 0 to 100, drops 10 per day. Nothing raises or reads heat yet. Decide what raises it and what it changes.

## Q17 — Room
ANSWER: one room, 20×12 tiles, walls around it, a gold door tile in the bottom wall that is solid for now (default).

## Q18 — Debug shop and crafting
ANSWER: F7 sells 1 of the first assigned item for 10. F8 crafts using an optional `RecipeDefinition` you create (Create > Signal > Recipe Definition). No recipe content is invented.

## Q19 — Save file
ANSWER: `SignalSave/signal-save.json` beside the project in the editor (git ignores it), the persistent data path in a build.

## Still undecided (needs your design)
- What items, recipes, customers and prices really exist
- Factions and reputation, relationships, quests
- What heat and missed rent actually cause
- The mixing/chemistry interaction (drag, heat, contamination, quality)


## Q20 — NPC life simulation approach
ANSWER: B — Lightweight utility simulation, paired with authored routines.

Approved approach:
- NPCs have authored identities, public roles, routines and private lives.
- A small deterministic utility brain chooses between a limited set of activities based on time and simple needs.
- The first prototype evaluates decisions every 15 in-game minutes and tracks only energy and social needs.
- Nearby NPC presentation uses ordinary Unity updates; world-clock progression drives the actual decision simulation.
- Core decision logic remains plain C# with no Unity dependency, random inference, background workers or language model.
- Start with one neighbourhood character, then expand only when a playable scenario needs more depth.
- Dialogue is authored and can respond to the NPC's current activity.
- This is a foundation, not a requirement to simulate every citizen in equal detail.

The first implementation is Milo, a neighbourhood baker with Work, Rest, Socialize and Home activities. His evening dialogue hints that his public occupation is not his entire life. Persistent relationships, discovered secrets and a concrete side activity remain future work and must be designed as part of a connected scenario.


## Economic details still undecided
- Currency name/symbol and how integer balance units are displayed.
- Starting clean bank/card balance for the actual game (current inspector value remains placeholder tuning).
- Dirty-money bundle denominations / stack-value convention.
- The first illicit exchange and future cleaning interaction's risks, rates and prerequisites.
- Which specific illegal goods require finite stock in the first neighbourhood.


## Economy — Phase 3 illicit exchange

**Status: design decisions resolved; implementation specification awaiting explicit approval.**

- Dirty-money item: one physical Dirty Cash Bundle, face value 100, indivisible, MaxStack 1.
- Seller: new discreet NPC, separate from Milo, in an existing shared-city region.
- Merchandise: one generic Contraband item; specific identity/effects are intentionally unspecified.
- First slice: one seller, one authored stock unit, no automatic restocking.
- Dirty bundles remain physical assets for illicit exchange only. They never enter CleanMoneyAccount and are not accepted by legitimate purchases.

Still to be authored as content inputs rather than guessed by implementation:
- Exact bundle count/price for the Contraband item.
- Exact interaction point within an existing city region.

Technical requirements and safeguards are specified in docs/tasks/ECONOMY-PHASE-3-ILLICIT-EXCHANGE.md. Save v9 is required to persist seller stock and prevent sold stock from reappearing after load.

Cleaning, dynamic prices, restocking, heat/police/exposure and criminal progression remain outside this slice.
