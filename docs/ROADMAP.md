# Roadmap — connected livelihoods in one lived-in city

## North star
Signal is a top-down urban RPG about making a precarious living in one shared city. The player inhabits a practical hideout, meets residents with public and private lives, handles physical goods, pursues lawful and unlawful opportunities, and sees progress through possessions, capabilities, relationships and places.

Current implementation and verification are authoritative in [PROGRESS.md](PROGRESS.md). Approved player-facing rules live in DESIGN and decision records.

## Completed Direction B sequence — B-01 to B-06
- **B-01 Production/workbench:** authored immediate recipes create physical goods.
- **B-02 Customer contracts:** one active contract; exact whole-stack physical fulfilment; issuer-funded clean CARD.
- **B-03 Practical hideout upgrade:** Compact Sewing Machine unlocks the next recipe.
- **B-04 Repair/restoration:** repair kits, item-state preservation and textile buyer/supplier loop.
- **B-05 Global NPC awareness & affinity:** bounded individual knowledge and separate player→NPC, NPC→NPC and NPC→player relationship authority.
- **B-06 Customer Repair Orders:** entrusted Worn Work Jacket, repair, exact-item return and 35 CARD service payment.
The user confirmed B-06's **456/456** EditMode baseline and in-game repair/return/payment loop on 2026-11-25. Do not rebuild these foundations without a concrete defect. No additional story cast was authored.

## Parallel Player Development track
- **P-01 Character Foundation & Appearance:** modular Body_A renderer, directional Idle/Walk and local importer implemented; user imported the pack and confirmed presentation.
- **P-02 Attributes & Skills:** exactly Armor and Heat Protection; fixed starting points, milestone awards and free respecialisation. Core/save/UI implementation user-verified at 467/467.
- **P-03 Equipment & Portable Backpack:** equipment slots, backpack lifecycle, pockets, hidden-pocket visibility, pocket transfers/swaps and pocket-aware service integration implemented in source. Non-art acceptance/closeout remains. See [P-03](tasks/PLAYER-P-03-EQUIPMENT-BACKPACK.md).

**Art boundary:** the user has paused sprite creation and wearable/wardrobe presentation. P-03.6 is parked; do not resume overlays, sprite templates, wardrobe preview or character-art work without explicit re-authorisation.

## Immediate sequence

### 1. P-03 non-art closeout
- Run the current full EditMode suite (expected 505; last confirmed 504/504 before the additional automatic-pocket-routing test).
- Resolve regressions before further gameplay expansion.
- Complete Play Mode acceptance of backpack equip/drop/recovery, F5/F9 while equipped and dropped, pocket operations, hidden-pocket visibility contract and all pocket-aware service paths.
- Keep temporary fresh-session repair/production cargo until acceptance is complete. Then remove only that automatic debug seeding; keep F1 debug item spawner.
- Update P-03 status and record user verification. Wearable visuals are not a closeout dependency while art is paused.

### 2. B-07 Neighbourhood Expansion
This is the next strategic gameplay initiative after P-03 non-art closeout. The goal is not map size for its own sake. Add a compact, connected area only when it creates meaningful reasons to travel, revisit, meet people and use existing livelihoods.

Before implementation, prepare a detailed task specification covering:
- player outcome and how it extends the existing loop;
- geographic connection to current Street/CornerShop/Apartment/Hideout spaces;
- proposed destinations and their practical purpose;
- existing systems reused (repair, production, retail, storage, customer work, NPC routines/relationships, scavenging);
- whether any new mechanic is actually necessary (explain it and seek approval before coding);
- resident scope, with no invented story cast/backstories;
- travel/navigation, scene composition, save/load and performance implications;
- acceptance criteria and Unity test/Play Mode plan.

Do not implement an empty district, disconnected map, placeholder service or new mechanic merely to increase content count.

## Later / separate tracks

### W-01 — Dynamic Weather & Cold Exposure
A separate future world-simulation phase. Intended direction: seasonal temperature changes, rain, storms, snow and blizzards; gradual cold exposure; shelter and effective Heat Protection mitigate cold. Heat Protection keeps its name and has no hot-weather/overheating meaning. Design and balance remain open until W-01 is activated.

### Broader pillars (not scheduled)
Crime consequences, police response, additional lawful/illicit activities, investigation, services, social favours, deeper scenario persistence, more city blocks and hideout personalisation remain part of the wider vision. Design them as connected player-facing scenarios, not isolated managers.

## Engineering guardrails
- One active gameplay initiative at a time.
- Complete the smallest coherent player-facing loop through Core, Game integration, UI, content and save state.
- Reuse existing authorities for inventory, ownership, transactions, accounts, NPC knowledge and persistence.
- Core is plain C#; Game owns Unity integration and authored content.
- Persistent changes require current-build save/load round-trip coverage.
- No new Unity packages or external dependencies without approval.
- No universal supply/demand, automatic NPC shopping, passive income, global gossip or per-frame NPC decision scoring.
- Do not add story residents/backstories before the main storyline/cast is designed.
- Unity Editor is the authority for compile, EditMode and Play Mode verification.


## P-03 — dropped backpack isolation regression (2026-11-26)

- **Source fix committed; verification pending.** SearchableContainerPanel no longer retains or hit-tests the live backpack inventory while the bag is dropped. GameSession and DirtyCashContactPanel now receive/use only accessible backpack references or an isolated empty grid. Personal pockets and automatic loot routing remain available.
- Preserve the dropped bag's stable identity and exact contents; the fix does not mutate its storage.
- Required next gate before P-03 closeout: Unity EditMode run, then Play Mode acceptance using B to drop the bag, opening Rhea's wardrobe and a street bin/searchable container, and recovering the same bag with its cargo intact.
