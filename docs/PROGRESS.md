# Progress — current repository truth

Last reconciled: **2026-11-26**. This is the concise status board. Historical detail belongs in [the November log](progress/LOG-2026-11.md); approved rules and acceptance detail belong in linked design/task documents.

## Project baseline
- Unity **6000.6.9f1**, 2D URP.
- Active architecture: `Signal.Core` (plain C#) + `Signal.Game` (Unity integration).
- Active scene: `Assets/_Project/Scenes/HideoutSlice.unity`.
- Working branch: `chore/collaboration-workflow-and-audit`; keep `main` untouched.
- Application version **0.01**. Save data has no schema-version field or automatic migration chain; incompatible experimental saves may be discarded/recreated.
- **Latest user-confirmed full EditMode suite: 504/504 passed**, after the Play Mode-only guard for temporary NPC visual-test residents. One automatic-pocket-routing regression test was committed afterward; expected current suite is **505**, but 505 has not been confirmed. Do not report 505/505 as passing.
- Unity Editor is authoritative for compilation, EditMode tests and Play Mode acceptance. Source review is not Unity verification.

## Strategic direction and sequence
The user selected **Direction B — meaningful everyday livelihood**: repair, production, customers, services, relationships and a practical hideout. Keep one active gameplay initiative at a time.

### Completed livelihood foundations — B-01 to B-06
- **B-01 Production/workbench:** authored immediate recipes create physical goods.
- **B-02 Customer contracts:** one active contract, physical whole-stack fulfilment and issuer-funded clean CARD.
- **B-03 Practical upgrade:** Compact Sewing Machine unlocks the next recipe.
- **B-04 Repair/restoration:** repair kits, item-state preservation and Rhea's textile buyer/supplier loop.
- **B-05 Global NPC awareness & affinity:** bounded individual knowledge, local perception and separate relationship directions.
- **B-06 Customer Repair Orders:** Milo entrusts a Worn Work Jacket; player repairs and returns the exact item for 35 CARD. Ownership/custody remain distinct; payment and return are atomic and replay-protected.
- User confirmed **456/456** and successful B-06 Play Mode acceptance on 2026-11-25. Do not rebuild these foundations without a concrete defect. No additional story cast was authored.

### Player Development
- **P-01 Character Foundation & Appearance — implemented/user-verified.** Modular Body_A renderer, directional Idle/Walk, local ZIP importer and fallback. User imported the supplied pack and confirmed the enlarged character presentation.
- **P-02 Attributes & Skills — implemented/user-verified.** Exactly two attributes: Armor and Heat Protection. Fixed starting skill-point budget, milestone awards, free respecialisation, save mapping and F2 allocation panel. User confirmed **467/467** after this implementation slice. Catalog tuning and actual skill-effect/milestone-trigger integrations remain provisional/future work.
- **P-03 Equipment & Portable Backpack — core implementation largely complete; non-art closeout remains.** See [P-03 task card](tasks/PLAYER-P-03-EQUIPMENT-BACKPACK.md).

## P-03 current implementation truth
Implemented in source:
- Seven equipment slots: Head, Torso, Legs, Feet, Accessory 1–3. Backpack occupies one generic accessory slot.
- Exact-instance equip/unequip preserves stable identity, item state and ownership.
- Equipped backpack owns its 8×6 contents grid by stable backpack instance ID.
- B drops the backpack; E recovers and re-equips the same item. Dropped contents remain inaccessible until recovery.
- F5/F9 captures backpack equipped/dropped state, position, identity and contents.
- Three persistent 1×1 pockets: P1/P2 ordinary; P3 hidden from ordinary-search enumeration and visibly marked HIDDEN.
- Manual pocket moves are exact-target; pocket-to-pocket move/swap preserves identity/state/ownership.
- Automatic receipts choose accessible backpack capacity first, then first suitable empty pocket P1 → P2 → P3. No destination prompt; oversized/rotated items cannot use pockets.
- Pocket-aware integration/source audit covers Retail SELL, Dirty Cash selection, Home Storage, Searchable Container, workbench repair, multi-grid production, customer flows and GameSession receipt/transfer paths. Dropped-backpack cargo is excluded from accessible views.
- Temporary fresh-session debug loadout seeds a backpack and repair/production test cargo. It is test scaffolding, not intended new-game inventory.

Still outstanding:
1. Run the complete current EditMode suite (expected **505**) and inspect the XML.
2. Complete Play Mode acceptance: fresh session/debug loadout; equip/drop/recover; F5/F9 while equipped and dropped; pocket access while bag is dropped; manual exact-target moves/swaps; P3 ordinary-search exclusion contract; service flows with items split across backpack/pockets; ensure dropped cargo is inaccessible.
3. Keep temporary repair/production cargo until the user finishes acceptance. At final P-03 closeout, remove only automatic fresh-session cargo seeding; retain the F1 debug item spawner.
4. Record user-confirmed results. Do not mark P-03 complete before these checks.

**P-03.6 Wearable Presentation is paused/deferred.** The task card is historical preparation only, not the next implementation step. No new sprites, sprite templates, clothing overlays, wardrobe panel or art import work.

## Next strategic track — B-07 Neighbourhood Expansion
B-07 remains the next Direction B initiative after P-03 non-art closeout. Add a compact connected area only when it has meaningful destinations and repeatable reasons to visit. Reuse the existing player/session, clock, inventory, accounts, NPC social world and save context. Before implementation, prepare a dedicated task specification covering player outcome, geography, destinations, practical interactions, system reuse, NPC scope, navigation, save/load and acceptance. Explain any genuinely new mechanic and obtain user approval before coding it.

## Separate future track — W-01 Weather & Cold Exposure
W-01 is separate and inactive. Intended direction: seasonal temperature/weather, rain, storms, snow/blizzards and gradual cold exposure; Heat Protection retains its name and mitigates cold only. No hot-weather/overheating gameplay. Detailed mechanics/balance remain open until W-01 is activated.

## Reusable foundations — do not rebuild
| Area | Existing foundation |
|---|---|
| World | Hideout, Street, CornerShop, Apartment, persistent GameSession/player |
| Inventory | Spatial grid, stable stack GUIDs, item state, three pockets, portable backpack lifecycle |
| Ownership/economy | Ownership vs custody, atomic asset transfers, CARD accounts, service-payment ledger |
| Workshop | Actor-aware repair/production, authored recipes, one-active customer job |
| Scavenging/crime | One-time searchable bins, resident-owned containers, theft outcomes, witness-scoped knowledge |
| NPCs | Shared NpcSocialWorld, local perception, individual knowledge, routines, directed relationships |
| Dirty Cash | Physical currency, explicit exchange/cleaning and separate transaction history |
| Player Development | Armor, Heat Protection, skills, points, milestones and save mapping |

## Guardrails
- One active gameplay initiative at a time; complete a coherent player-facing loop.
- Core remains plain C#; Game owns Unity integration and authored content.
- Reuse existing inventory, transaction, ownership, account and save authorities.
- Persistent state needs current-build F5/F9 round-trip coverage.
- No new Unity packages/dependencies without approval.
- No universal supply/demand, automatic NPC shopping, passive revenue, global gossip or incidental simulation.
- Do not author new story cast/backstories until the main storyline/cast are designed.
- Do not run `dotnet` or `msbuild` for this Unity project.
- Keep this file concise; append history to the November log.
