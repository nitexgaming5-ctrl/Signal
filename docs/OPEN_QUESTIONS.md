# Open questions — current design queue

This file contains only decisions that are still genuinely open. Answered setup questions and completed initiative decisions are preserved in `decisions/` and the historical archive. Do not reopen an approved decision without new evidence or a user request.

## Active First Workshop

The user delegated authority to the agent to author provisional B-01 to B-03 content. This authority has been exercised and recorded in decisions/0005-first-workshop-content.md. The starter catalogue, two recipes, Milo contract and Rhea upgrade are authored and wired; no further content choice is blocking the active slice.

The following are implementation/verification checks, not open design questions:
- Open the updated assets/scene in Unity and confirm there are no import/reference/validation errors.
- Exercise two productions, contract acceptance/fulfilment, CARD payment, upgrade purchase, visual installation, recipe unlock and save/load in Play Mode.
- Confirm Milo's account funds the 40 CARD reward and Rhea's account receives the 90 CARD upgrade payment.

## Future design — deliberately not blocking B-01 to B-03

### Repair/restoration (B-04)
- Which item categories can be repaired, and what does a repair change?
- Are repair materials consumed, and does the player need a specific tool/workspace?
- Can customers entrust their own goods, or are repairs limited to player-owned items?
- What are the failure, cost and presentation rules?

### Social life and recurring customers (B-05)
- Which player actions change familiarity, trust or affinity?
- How do recurring customer requests arise without procedural demand simulation?
- Which rewards can relationships provide: information, access, introductions, services or other authored outcomes?
- How do residents explicitly share observations and rumours without omniscient gossip?

### Broader crime design
- Which crime actions follow physical-container theft, and how are their risk/exposure rules different?
- What triggers heat, how does it cool, and what are the meaningful consequence bands?
- When/how can witnesses report or share observations, and what evidence/confidence is retained?
- What is the long-term global criminal progression threshold/unlock model?
- What failure and recovery rules are harsh but not arbitrary save deletion?

### Economy and city expansion
- What currency display/symbol and HUD format should be used for clean CARD?
- Which goods, if any, need selective finite stock and authored restocking?
- Which additional lawful or illicit activities justify another economy service?
- What resident cast, services and geography justify a second neighbourhood block?

### Art and presentation
- Which final pixel font and screen scaling should replace the built-in UGUI placeholder?
- What is the final authored palette/lighting treatment and item-icon direction?
- Which temporary city art packs/placeholders should be replaced first?

## Parked, not missing

The following are deliberately outside the active slice: production timers/queues, quality rolls, tool durability, procedural contracts, multiple active contracts, partial-stack contract delivery, relationship/favour rewards, automatic NPC shopping, universal supply/demand, dynamic prices, police response, global rumour cascades and additional districts.

Do not turn these into implementation tasks until a player-facing scenario demonstrates the need.
