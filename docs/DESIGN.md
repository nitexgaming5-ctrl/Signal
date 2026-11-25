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
- Player-to-NPC, NPC-to-player and NPC-to-NPC familiarity, trust and affinity are separate and directional, using the same shared relationship model. PlayerRelationshipBook owns player→NPC state; each NPC owns its own NPC→player opinion in NpcSocialWorld/NpcAgentState; NPC→NPC state remains in that same NPC's relationship map. Every NPC has individual sourced knowledge. Information is learned through local observation, explicit communication or authored discovery; rumours retain source and confidence. Ambient NPC-to-NPC encounters may create small familiarity changes only; trust/affinity and dramatic social consequences require player actions or authored scenarios. Agenda stage gates and one-time choice outcomes may explicitly use either player→NPC or NPC→player perspective. Durable relationships and knowledge persist by stable IDs; transient perception is rebuilt.
- Milo and Rhea, plus the existing Dirty Cash contact, are current framework/test residents. Do not create additional NPCs or expand character backstories until the main storyline and intended cast have been designed. Dialogue and content must never hardcode their identities into reusable systems.

Specialist implementation and social boundaries: `NPC_FRAMEWORK.md`.

## Shared economy

- Clean money is a resident/player CARD account and funds legitimate purchases/services.
- Dirty Cash is a physical inventory item. It cannot pay for legitimate retail or be deposited directly.
- Resident personal accounts own clean balances. Business revenue belongs to the resident owner; no independent business balance is created.
- Business merchandise and buyback offers are separate authored catalogue layers. Merchandise remains fixed-price; each buyback entry is either fixed-price or explicitly condition-priced from the selected physical item's `ItemInstanceState`. Ordinary merchandise has unlimited availability in the initial model.
- Transactions are explicit, validated and atomic. Selective condition-based buyback is an opt-in authored exception, not a market simulator. Do not add time-, stock- or demand-driven price fluctuations, universal supply/demand, passive revenue or NPC shopping simulation.

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


## NPC awareness and relationships

- Every registered NPC uses the shared local-perception and individual-knowledge framework; authored residents do not receive character-specific runtime social systems.
- NPC-to-NPC relationships are directional and keep familiarity, trust and affinity separate. Player-to-NPC relationships remain in the existing `PlayerRelationshipBook`; NPC-to-NPC relationships remain in `NpcSocialWorld`.
- Personal knowledge is sourced and confidence-weighted. Direct observation and explicit communication/discovery are valid acquisition paths; the world event journal is not private NPC memory and is never a decision-making shortcut.
- Durable personal knowledge and NPC-to-NPC relationships survive save/load. Position, proximity and perception are reconstructed from the active scene.


## B-06 — Customer repair orders

- A customer may entrust one exact physical item to the player for repair. The existing Milo framework resident is the first authored test issuer; do not add new story residents or backstories.
- Accepting the order transfers custody from the issuer's inventory to the player's carried inventory, not asset ownership. The issuer remains the registered owner for the entire job.
- The entrusted item is repaired through the existing hideout workbench and RepairService. Only a player-owned patch kit is consumed; the active order authorizes repair of its exact issuer-owned item.
- The first order uses Milo's Worn Work Jacket, one Basic Patch Kit, a Good return condition and a fixed 35 clean CARD service fee.
- Returning the same item instance moves it back into the issuer's inventory and performs one atomic payment: debit the issuer's existing resident account, credit the player, record a ServicePaymentTransaction and complete the order.
- Service-payment history is distinct from ownership-changing asset-transfer history. Failed return leaves both inventories, both balances, payment history and order phase unchanged. A completed order cannot pay twice.
- Only one customer job may be active across ordinary customer contracts and repair orders. F5/F9 persists the active/completed order, item location/owner, balances and service-payment history.
- No repair queue, duration, quality roll, automatic pricing, concurrent customer jobs or new NPC cast is included in this first service slice.


## Player Development — approved direction (design stage)

The user has approved a parallel Player Development track. It customizes the existing player; it does not introduce a new-game character class/background flow.

### Character foundation and skills

- Character creation means customizing the existing player character.
- Starting skills are freely chosen from a fixed starting skill-point budget.
- Further skill points are awarded for important gameplay milestones, not repetitive use-based grinding.
- Skill points are manually allocated to improve skills.
- The player may freely redistribute all allocated skill points at any time, without cost, cooldown or penalty.
- The player has exactly two attributes: **Armor** and **Heat Protection**. Do not add generic Strength/Endurance/Dexterity/Intelligence/Charisma attributes.
- Armor is the physical-protection attribute.
- Heat Protection retains this exact name and protects against cold-related environmental exposure (low temperatures, wind chill and cold-weather precipitation). Despite its name, hot-weather exposure, overheating and heat damage are not planned.
- Clothing and armor are intended to provide both visual customization and gameplay modifiers. The authoritative equipment/loadout implementation is a later dedicated phase; appearance rendering must be designed to support it without duplicating inventory items.
- The candidate skill names Repair, Production, Trading, Scavenging and Social remain provisional until the user approves the final catalogue. Skill ranks, starting budget, point costs and milestone rewards are not yet numerically balanced.

### Dynamic weather and cold exposure direction

- Weather is intended to be dynamic across seasons, with changing temperatures, rain, storms, snow and blizzards.
- Player survival effects are focused on cold events. No hot-weather/overheating mechanics are planned.
- Cold exposure is gradual, affected by temperature, wind, rain/snow, outdoor duration, shelter and effective Heat Protection.
- Ordinary rain is not an instant damage event. Severe sustained exposure may eventually have meaningful consequences, subject to W-01 detailed design.
- Weather is a shared world state across connected locations; interiors/shelter protect the player and enable recovery.
- W-01 is a separate future system and is not part of P-01/P-02 implementation.

Detailed specifications: `tasks/PLAYER-P-01-CHARACTER-FOUNDATION-APPEARANCE.md`, `tasks/PLAYER-P-02-ATTRIBUTES-SKILLS.md`, and `tasks/WEATHER-W-01-DYNAMIC-WEATHER-COLD-EXPOSURE-DESIGN.md`.
