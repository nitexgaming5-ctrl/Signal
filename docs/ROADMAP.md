# Roadmap — build a lived-in city through connected activities

## North star

Signal is a top-down urban RPG about making a precarious living in one shared city. The player inhabits a practical hideout, meets residents with public and private lives, handles physical goods, pursues lawful and unlawful opportunities, and sees progress in possessions, relationships, equipment and places.

This roadmap describes the direction and dependencies. Current implementation and verification are authoritative in `PROGRESS.md`; detailed approved player-facing rules live in `DESIGN.md`.

## Completed sequence — Direction B: First Workshop through Customer Repair Orders (B-01 to B-06)

B-01–B-04 establish the player-operated workshop, one active customer contract, practical hideout upgrade, repair/restoration and the first textile buyer/supplier loop. B-05 adds generic individual NPC awareness and all three independent relationship directions. The user confirmed **445/445 EditMode tests passing on 2026-11-25**. These foundations are the baseline for the next livelihood activity; do not rebuild them without a concrete defect.

**B-06 — Customer Repair Orders: complete and user-verified.** The user confirmed 456/456 EditMode tests and successful in-game acceptance → repair → return/payment testing on 2026-11-25. See [B-06 task card](tasks/LIVELIHOOD-B-06-CUSTOMER-REPAIR-ORDERS.md).

**Player outcome:** produce useful physical goods in the hideout, fulfil one authored customer request, receive clean CARD and invest in a practical upgrade that unlocks another recipe.

Parent specification: `tasks/LIVELIHOOD-B1-B3-FIRST-WORKSHOP.md`.

Approved model:
- First workspace belongs to the player and is located in the existing hideout.
- Long-term model is compatible with NPC-owned production businesses, but those are not a second deliverable in this slice.
- Recipes are authored ScriptableObjects with explicit inputs and outputs.
- Production is immediate and transactional; output is a physical inventory item.
- Exactly one accepted/active customer contract at a time.
- Contract goods are handed over physically as a whole stack to an issuer-owned receiving inventory.
- The issuer pays a fixed clean CARD reward from an existing resident account.
- A one-time practical hideout upgrade unlocks one additional authored recipe/capability.
- Use existing inventory, ownership, account, transfer and save foundations.

### Delivery packages

1. **B-01 — Production/workbench:** author the first recipe, make the current workbench's recipe-book flow playable, and preserve atomic ownership-aware production.
2. **B-02 — Customer contract:** author one existing resident as the customer, show terms before acceptance and fulfil by physical handover.
3. **B-03 — Practical upgrade:** install one visible improvement, transfer its fixed CARD cost to an existing resident and unlock the second recipe.

The implementation foundation and starter content for all three packages are committed. The active scene references the catalogue, two recipes, contract, upgrade prefab and resident definitions. The active task cards provide file-level acceptance criteria.

**Verification update (2026-11-25):** the user reports **445/445 EditMode tests passing** on current HEAD. B-01–B-05 code and the B-05 relationship/agenda integration are included in that result. Continue to treat Play Mode acceptance separately from EditMode verification; do not infer unreported gameplay checks.

### Exit criteria

- The starter materials, recipes, contract, upgrade and authored residents are assigned to the active scene.
- Workbench shows the available recipe, exact ingredients/output and current availability.
- Production consumes the correct player-owned materials exactly once and creates physical output.
- The first contract can be reviewed/accepted; only one active contract is possible.
- Exact eligible goods are handed over; issuer account is debited and player account credited atomically.
- The visible upgrade is acquired once; its cost goes to the authored resident payee; the second recipe becomes available.
- Active/completed contract state, acquired upgrade, inventories and balances survive save/load.
- Existing inventory, scavenging, retail, theft, Dirty Cash and NPC behavior remain intact.
- Full EditMode suite and Play Mode loop are verified in Unity.

## Strategic sequence after the First Workshop

These are approved direction placeholders, not implementation permission for un-designed mechanics.

### B-04 — Repair and restoration

**Status: implemented and user-reported working in Play Mode; included in the later 445/445 full EditMode result.** See [LIVELIHOOD-B-04](tasks/LIVELIHOOD-B-04-REPAIR-RESTORATION.md) and [Decision 0006](decisions/0006-hybrid-buyback-and-textile-business.md).

The approved first loop uses Rhea as a textile buyer/supplier, a Worn unstackable Work Jacket from an existing scavenging loot profile, fixed-price raw repair materials, and one per-condition buyback schedule for that jacket. The existing hideout workbench/Compact Sewing Machine and Basic/Reinforced Patch Kits provide the repair path. Repairs change Condition only, preserve stable identity and all other state/provenance, refresh the player's Condition knowledge as direct inspection and persist through the current unversioned save round trip.

The catalogue model is hybrid but bounded: merchandise remains fixed-price; each buyback entry is either fixed or explicitly condition-based for one authored item and business. Existing fixed offers remain unchanged. Do not derive CARD payouts from `ItemValueCalculator`, which remains an indicative reference value. This is not demand pricing or a global market simulator. The generic pricing code, Work Jacket, Rhea business asset, loot-profile entry, active-scene registration, visible CornerShop counter, repair service and workbench repair UI are committed. Focused tests cover pricing, repair, ownership/provenance and save/knowledge persistence, but have not run in Unity. Complete the combined First Workshop/B-04 EditMode and Play Mode acceptance before calling the initiative verified.

### B-05 — Global NPC awareness & affinity

**Status: implemented and user-verified; 445/445 EditMode tests passed (2026-11-25).** Every registered NPC has individual sourced knowledge, directed NPC-to-NPC relationships and a separate opinion of the player. The player's opinion of each NPC remains in `PlayerRelationshipBook`; NPC opinions of the player and NPC-to-NPC relationships live with each `NpcAgentState` inside `NpcSocialWorld`. All three directions use the same `NpcRelationship` value model and persist independently through F5/F9. Customer-contract definitions can author outcomes in either direction, applied only after successful delivery, payment and completion.

Knowledge remains local or explicitly communicated; there is no omniscience, automatic city-wide gossip, ambient trust/affinity farming or universal reputation. **This phase builds reusable NPC infrastructure, not the cast:** Rhea, Milo and the existing Dirty Cash contact are framework/test residents only. Do not add residents, personalities, backstories or story content until the main storyline and cast are deliberately designed. See [B-05 task card](tasks/LIVELIHOOD-B-05-GLOBAL-NPC-AWARENESS-AFFINITY.md).

### B-06 — Customer repair orders

**Status: implemented and user-verified; 456/456 EditMode tests and in-game acceptance confirmed by the user on 2026-11-25.** The existing Milo framework resident entrusts one Worn Work Jacket. The player accepts it into custody (ownership remains Milo's), repairs it with the existing workbench/Basic Patch Kit flow, returns the same physical instance and receives 35 clean CARD from Milo's resident account.

The payment uses a dedicated ServicePaymentTransactionLedger, separate from ownership-changing asset transfers. The return operation is atomic across the two inventories, player/customer balances, payment history and order completion; replay is blocked. Active order and payment history are included in F5/F9. Only one customer job may be active across the existing contract and repair-order flows. No new story NPCs are introduced.

Eleven focused EditMode tests cover custody/ownership, successful payment, replay, failed returns, insufficient funds, player-account overflow, missing issuer account, receiver capacity and save/load. The user confirmed the complete suite at 456/456 and reported the in-game loop passing. See [B-06 task card](tasks/LIVELIHOOD-B-06-CUSTOMER-REPAIR-ORDERS.md).

### B-07 — Neighbourhood expansion

Add a compact connected area only when the current neighbourhood has enough meaningful interactions to justify more space. Density and revisit value matter more than map size. Reuse the same player, clock, inventory, accounts, NPC world and save context.

## Existing foundations — do not rebuild

### Place and movement
- Hideout room, walkable street, CornerShop and apartment interiors.
- One persistent GameSession/player across location transitions.
- Physical bed, storage, workbench and interaction prompts.
- Placeholder geometry and city art are still due for a cohesive art pass.

### Inventory, goods and discovery
- Spatial grid inventory, home locker, stable stack identities and transactional whole-stack transfers.
- Per-stack quality, condition, authenticity and ownership state.
- Player knowledge book and inspection UI; unknown properties remain unknown.
- Searchable street bins with authored loot profiles, one-time generation, progressive reveal and persistence.
- Resident-owned containers and theft use the same container/inventory foundations.

### NPC and narrative
- Shared NpcSocialWorld with local perception and character-scoped knowledge.
- Deterministic utility brain plus authored routines; one generic NpcPresenter.
- Directed player-to-NPC relationship book, discovery signals and interaction conditions.
- Milo parcel opportunity framework and crime-observation event foundation.
- Deeper player-driven social consequences, scenario persistence and secret discovery remain future work.

### Economy and crime
- Ownership separate from container custody.
- Atomic asset transfer and economic transaction history.
- Clean CARD accounts for player and residents.
- Authored fixed-price business merchandise and per-business buyback offers that may be fixed or selectively condition-based for explicitly configured physical item states.
- Dirty Cash is a physical item; illicit exchange and cleaning are explicit, atomic and separately recorded.
- Theft records a crime outcome and global criminal progression separately from heat.
- Universal supply/demand, auto-shopping, passive sales, finite stock simulation and market-wide/time-/demand-driven prices are excluded from the initial economy. Selective condition-based buyback is an authored, per-entry exception.

## Other long-term game pillars

The wider vision includes additional crime categories, hidden NPC activities, services, investigation, legal and illicit trades, meaningful item provenance, richer consequences, personalisation and eventually more city blocks. These should be designed as connected scenarios that reuse the current world rather than as a checklist of isolated managers.

## Engineering and workflow guardrails

- One active gameplay initiative at a time.
- Implement the smallest complete player-facing loop through Core, Game integration, UI, content and save state.
- Reuse existing transaction and inventory rules; do not fork them.
- No new Unity packages or third-party systems without approval.
- Core remains plain C#; Game owns Unity integration and authored content.
- Persistent changes require current-build save/load round-trip coverage. Build 0.01 intentionally has no schema-version field or migration chain; incompatible experimental saves may be discarded.
- A system is not complete merely because the classes compile: integrate it into the active scene and verify the experience.

## Historical detail

Completed task specifications and their verification snapshots are archived under `tasks/archive/`. The append-only session history is in `progress/LOG-2026-11.md`. These files are for specific audits and regressions, not routine roadmap reading.


## Active social foundation — B-05 Global NPC Awareness & Affinity

B-05 preserves three distinct directions: player→NPC in `PlayerRelationshipBook`; NPC→NPC and NPC→player on each `NpcAgentState` in `NpcSocialWorld`. Each uses the shared `NpcRelationship` value model. Individual knowledge and all NPC-owned relationship directions persist through F5/F9. Customer contracts now support authored outcomes for both the player's impression of the issuer and the issuer's impression of the player. The user confirmed **445/445** after the NPC→player and generic agenda integration. No new story NPCs were authored during this framework phase.

## Parallel track — Player Development (P-01 onward)

The user has approved intercalating character development before returning to the B-07 neighbourhood-expansion work. This is a parallel track; it does not renumber or replace B-07.

- **P-01 — Character Foundation & Appearance:** modular Body_A renderer, directional idle/walk animation, resource-pack importer and fallback are implemented. User imported the pack and confirmed the enlarged character works. Full wardrobe remains constrained by the pack's single base body.
- **P-02 — Attributes & Skills:** Core attributes, skill ranks, milestone ledger, save mapping, provisional catalog and F2 allocation panel are implemented. User confirmed 467/467 after this track's first slice. Prototype tuning is 8 starting points, rank cap 5, 1 point/rank; activity-specific skill effects and actual milestone triggers remain open.
- **P-03 — Equipment & Clothing:** active implementation. Seven slots are approved (Head, Torso, Legs, Feet, three generic accessories). The Canvas Backpack owns its 8×6 grid and can be dropped/recovered with B/E; backpack and pocket contents persist through F5/F9. The player has three persistent 1×1 pockets: P1/P2 ordinary, P3 hidden from ordinary-search enumeration. Backpack↔pocket drag/drop and direct pocket move/swap are implemented. User confirmed **503/503 EditMode tests passing (2026-11-26)** after the three focused P-03.5 tests. P-03.5 source integration now covers Retail SELL, Dirty Cash selection, Home Storage, Searchable Container pocket display, workbench repair and multi-grid production across accessible pockets/backpack; three focused tests were added and the user subsequently confirmed 503/503; later NPC sprite-rendering work is a separate unverified change. Next is the remaining direct-grid assumption audit, then wearable sprite overlays, temporary debug-loadout cleanup and end-to-end Play Mode acceptance. See the P-03 task card for sequencing.

See [P-01](tasks/PLAYER-P-01-CHARACTER-FOUNDATION-APPEARANCE.md), [P-02](tasks/PLAYER-P-02-ATTRIBUTES-SKILLS.md) and [P-03](tasks/PLAYER-P-03-EQUIPMENT-BACKPACK.md).

## Separate future world-simulation track — W-01

**W-01 — Dynamic Weather & Cold Exposure** is a separate future phase, not part of P-01/P-02. The user wants dynamic seasonal weather (temperature changes, rain, storms, snow and blizzards), while player exposure consequences are cold-focused only. The attribute name remains **Heat Protection**; it protects against cold exposure. Exposure is gradual, with shelter and effective Heat Protection reducing risk. No hot-weather or overheating gameplay is planned. See [W-01 design](tasks/WEATHER-W-01-DYNAMIC-WEATHER-COLD-EXPOSURE-DESIGN.md). Detailed mechanics and balance remain open until W-01 is activated.

