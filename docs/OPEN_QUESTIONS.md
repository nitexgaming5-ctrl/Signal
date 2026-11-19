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
- Starting clean bank/card balance for the player and authored starting balances for named residents.
- Cleaning rate, prerequisites, risks and presentation.
- Which specific illegal goods require finite stock in the first neighbourhood.


## Economy — Phase 3 Business & Retail

**Status: approved; first implementation pass added (2026-11-19).** Full scope and acceptance criteria: `docs/tasks/ECONOMY-PHASE-3-BUSINESSES-RETAIL.md`.

Confirmed decisions:
- Every resident has a personal clean-money account.
- A business has authored merchandise and fixed authored prices; merchandise is unlimited and available whenever the business is open.
- Business sales credit the owner's personal account directly. No separate business revenue account.
- Existing player-to-shop selling behaviour is retained without new valuation/acceptance rules.
- No NPC shopping/spending AI, supplier or production simulation, finite stock/restocking, automatic sales, passive revenue or continuous/hourly economy simulation.
- Dirty money remains physical inventory items, excluded from clean accounts and legitimate retail.

Implementation now includes resident account/persistence support, authored business definitions, atomic retail purchases and the first CornerShop runtime integration. Save v9 stores resident balances and retail history. Remaining content input: choose the first Corner Bakery ItemDefinition(s) and fixed clean-money price(s); the assigned business asset currently has an empty catalogue rather than invented prices.


## Economy — player-facing sell-to-shop

**Q21 — Buyback acceptance and pricing model**

ANSWER: Each `BusinessDefinition` has its own separate authored buyback-offer list. Each offer references an `ItemDefinition` and a positive fixed clean-CARD unit price.

Approved rules:
- Each business buys only items explicitly listed in its own buyback offers.
- Buyback offers are separate from the business's player-facing merchandise catalogue. Selling an item does not imply the business buys it.
- Prices are fixed authored values; no dynamic valuation, `ItemValueCalculator` formula, condition/quality/authenticity/ownership modifiers, reputation or bargaining.
- No business budget/account debit, finite stock, restocking or NPC economy simulation.
- Only clean CARD is paid; dirty money remains physical inventory and is not part of legitimate retail.
- No item or price entries are implied by this approval; content offers require their own authored values.

Implementation specification: `docs/tasks/ECONOMY-PLAYER-SELLING-DESIGN.md`. The approved model is implemented and user-verified: the full EditMode suite passed 355/355, and the BUY → partial SELL → full SELL → save/reload Play Mode sequence works. The temporary Bread buyback offer at CARD $3 was used only for testing and has been removed from `Business_CornerShop.asset`. No final bakery buyback item or price has been approved. The existing Bread merchandise price of CARD $5 remains temporary test content, not final balancing.
