# Approved gameplay design

This document records current, approved player-facing rules. It is not a development diary. Technical class layouts belong in Architecture/Data Model; historical implementation detail belongs in the progress log.

## Game identity and pillars

Signal is a top-down 2D urban RPG about a precarious life in one connected city. Lawful and unlawful activities coexist. The hideout is the emotional and practical anchor; people, goods, routines, discoveries and consequences make everyday decisions matter.

Detailed game identity and reference-game intent: `VISION.md`.

## Player and time foundations

- Player stats currently include clean Money/CARD and Health. No extra universal stat is implied.
- The world clock begins Day 1 at 08:00 in the original prototype tuning. NPC routines use world time; presentation movement is handled in Game.
- Rent, starting balances, clock speed and heat values remain prototype tuning until a specific playable scenario needs final balance.
- Heat and global criminal progression are separate: heat represents current pressure and can cool; criminal progression is a global non-decaying total. No broad consequence/unlock catalogue is approved yet.

## Items, inventory and knowledge

- Items are physical stacks in a spatial inventory. A stable ItemInstanceId follows a stack through transfer/save/load.
- Item state records quality, condition, authenticity and ownership (Legitimate/Stolen).
- Ownership is independent of the container holding the item; container custody is a separate concept.
- Inventory transfers move whole stacks in the current system. Partial sale is a separate explicit operation; general split-stack support is not implied.
- Player knowledge is partial and sourced. The item viewer must not reveal hidden state that the player has not learned.
- Item reference value is not a shop price. Buy/sell prices are authored per business/offer.

## NPCs, social knowledge and discovery

- Residents share one world context with authored routines and a deterministic, inexpensive utility brain.
- NPCs only directly observe local events within perception range. Knowledge is character-scoped; the world event log is not everyone's memory.
- Explicit communication can share knowledge with source/confidence. There is no automatic city-wide gossip.
- Player-to-NPC familiarity, trust and affinity are separate and directional. Ambient encounters may create small familiarity changes; trust/affinity and dramatic social consequences require player actions or authored scenarios.
- Milo and Rhea are authored neighbourhood residents. Dialogue and content should support their roles without hardcoding their identities into reusable systems.

Specialist implementation and social boundaries: `NPC_FRAMEWORK.md`.

## Shared economy

- Clean money is a resident/player CARD account and funds legitimate purchases/services.
- Dirty Cash is a physical inventory item. It cannot pay for legitimate retail or be deposited directly.
- Resident personal accounts own clean balances. Business revenue belongs to the resident owner; no independent business balance is created.
- Business merchandise and buyback offers are separate, authored, fixed-price lists. Ordinary merchandise has unlimited availability in the initial model.
- Transactions are explicit, validated and atomic. Do not invent dynamic prices, universal supply/demand, passive revenue or NPC shopping simulation.

## Scavenging and theft

### Public scavenging
- Physical bins have stable identities and authored loot profiles.
- Contents are generated once, revealed progressively and persisted. Closing/reopening preserves the same contents and search progress.
- The player manages carried inventory while the container is open and transfers discovered items individually.
- No respawn/replenishment is included in the first slice.

### Resident property theft
- Resident property containers have authored owners and fixed initial contents.
- A successful transfer marks the item Stolen and records ownership/economic transfer history atomically.
- Crime outcome, heat and global progression are distinct records/concepts.
- Only a resident selected as a local witness receives the character-scoped observation. **An unwitnessed theft adds no heat.**
- No automatic report, police response, relationship penalty or witness-sharing consequence is active in the first theft slice. Broader crime rules remain future design.

### Dirty Cash contact
- The contact is a normal resident in the shared city with internal ID `Clean_Dirtycash` and intentionally blank visible name.
- Dirty Cash is a physical item of denomination 10 per unit.
- The first fixture exchange accepts two player-owned stolen ITEM_A and pays three Dirty Cash units.
- Cleaning retains 20% and converts the remainder to the player's clean CARD; the contact receives the retained fee in their resident account.
- Exchange and cleaning are player-confirmed atomic transactions with separate histories. No risk, delay, heat, witness or police effect is included in this slice.

## Direction B — livelihood and First Workshop

The player should have a meaningful everyday livelihood alongside crime. The first workshop is player-operated in the hideout, with the long-term model compatible with NPC-owned production.

### Binding First Workshop rules

- Recipes are authored and contain explicit ItemDefinition input/output references and positive quantities.
- The first slice uses immediate production. There are no queues, timers, quality rolls, tool durability or staffing rules.
- Production consumes player-owned inputs and creates physical output in the same inventory.
- Only one customer contract can be active at once. Contract terms are authored and visible before acceptance.
- Contract fulfilment uses the exact required whole stack from player inventory, transfers goods to an issuer-owned physical receiving inventory and pays a fixed clean CARD reward from the issuer's existing resident account.
- Failed fulfilment does not mutate items, accounts, contract state or history.
- The first hideout upgrade is a one-time practical purchase; its tangible installed prop and acquired ID persist, and it unlocks one additional authored recipe.
- The workbench is a close-up workspace with tabletop props, a recipe book, recipe details, Produce and upgrade controls. It must remain attached to the existing hideout interaction rather than becoming a separate scene/system.

The current starter content and exact prototype values are listed in the active First Workshop task card and decision record `decisions/0005-first-workshop-content.md`. These content values are deliberately replaceable, not a new universal balancing policy.

## Explicit exclusions

No universal quest framework rewrite, procedural contract board, multiple active contracts, partial-stack handover, automatic NPC demand, recurring customer simulation, relationship rewards, production time/quality simulation, repair mechanics, police response or new districts are introduced by the First Workshop slice.
