# Progress — read this first

Reconciled 2026-11-20 from the append-only project log and the current code on `chore/collaboration-workflow-and-audit`. This is the short current-state summary; historical detail belongs in `docs/progress/LOG-2026-11.md`.

## 1. Where we are
- Unity 6000.6.9f1, 2D URP. Active architecture: `Signal.Core` (plain C#, no UnityEngine) + `Signal.Game` (Unity layer). `Assets/_Project/_Prototype/` and `Main.unity` are legacy reference only.
- Save schema **v14** (v12/v13 saves migrate).
- User reported **392/392 EditMode tests passed on 2026-11-20 after the rebuilt Slice B contact UI**. Four further regression tests have since been added; the current branch is expected to contain **396 tests**, not yet rerun by the user.
- **Active initiative: SLICE-B-HARDENING-001** — replay/retail/shop regression tests, GameSession constants and neutral item-name fallback. Changes are committed; Unity verification is pending.
- **Slice B Play Mode verification remains pending:** Unity compile and full contact exchange → cleaning → save/reload loop.
- Test content, not final: street bins A/B/C, ITEM_A/B/C, loot profiles A/B/C, Bread + Milo parcel, CornerShop prices, Rhea's wardrobe contents, unnamed contact and offer values. Third-party packs are test-only under decision 0004.

## 2. ALREADY IMPLEMENTED — do not rebuild, duplicate or re-propose
Extend only when a task card names a concrete gap. Search the code before claiming something is missing.

| Area | What exists | Where | Status |
|---|---|---|---|
| Hideout, city, save | Walkable hideout/workbench, connected city block, home storage, JSON save F5/F9 | Game: `RoomBootstrap`, `CityBlockBootstrap`, `StoragePanel`, `SaveFileStore`; Core: `SaveData` | user-verified |
| Grid inventory | Placement, rotation, whole-stack transfers, snapshots, drag math | Core: `GridInventory`, `InventoryTransfer`, `InventoryDragMath` | user-verified |
| Item identity/state/knowledge | Stable stack IDs; quality, condition, authenticity, ownership state incl. Stolen; knowledge book and viewer | Core: `ItemInstanceState`, `ItemKnowledgeBook`; Game: `ItemViewerPanel` | user-verified |
| Time, rent, heat, stats | Pausable clock, rent ledger, HeatMeter 0–100, stats | Core: `GameClock`, `RentLedger`, `HeatMeter`, `PlayerStats` | tested; heat currently awarded by witnessed theft only |
| Crafting core | Recipe in → item out, atomic rollback; no recipe assets | Core: `Crafting`; Game: `RecipeDefinition`, `WorkbenchWorkspace` | tested; no content |
| NPC framework | Shared social world, perception, knowledge/rumours, relationships, utility brain, routines, encounters and A* navigation | Core: `NpcSocialWorld`, `NpcPerception`, `NpcUtilityBrain`; Game: `NpcPresenter`, `CityNavigation*` | user-verified |
| World pacing | Permission gate for consequential developments; Core only, not wired/tuned | Core: `WorldPacingDirector` | tested |
| Discovery and relationships | Discovery ledger, player-NPC relationships, interaction conditions | Core: `PlayerDiscoveryLedger`, `PlayerRelationshipBook`, `InteractionConditionSet` | tested |
| Opportunity 001 — Milo's Parcel | Opportunity lifecycle + one authored scenario; soft delivery, no police/progression | Core: `OpportunityDefinition`, `OpportunityState`; Game: `MiloParcelOpportunityController` | user-verified (284/284 + Play Mode) |
| Economy foundation | Ownership independent of container, atomic transfer, append-only ledgers, clean CARD accounts incl. residents | Core: `AssetOwnershipBook`, `AssetTransferService`, `EconomicTransactionLedger`, `CleanMoneyAccount`, `EconomicAccountBook` | user-verified |
| Retail + sell | Fixed-price BUY catalogue, per-business buyback, BUY/SELL panel, revenue to owner | Core: `RetailService`, `Shop`; Game: `BusinessDefinition`, `RetailPanel` | user-verified |
| Scavenging | Stable-ID containers, one-time generation, progressive reveal, persistence, transactional transfer, dual inventory UI | Core: `SearchableContainer`; Game: `ContainerLootProfile`, `SearchableContainerPanel` | user-verified (368/368 + Play Mode) |
| Theft | Reusable resident-owned container; Stolen state; crime ledger and global progression; nearest witness receives character-scoped event | Core: `CrimeOutcome`, `CriminalProgressionTrack`; Game: `ResidentPropertyContainerDefinition`, `GameSession.RecordTheft` | user-verified; witnessed/unwitnessed Play Mode retest after heat fix still needed |
| Dirty Cash / Slice B | Physical Dirty Cash (10/unit); unnamed `Clean_Dirtycash` contact; ITEM_A ×2 → 3 units; 20% cleaning fee (8 CARD/unit player, 2 contact); atomic services, separate ledgers, persistent contact stash; rebuilt dual-inventory UI | Core: `DirtyCashTransactions.cs`; Game: `DirtyCashContactOfferDefinition`, `DirtyCashContactPanel`, `GameSession.ExchangeWithContact/CleanDirtyCash` | 392/392 user-reported after UI correction; Play Mode pending |
| Dev tooling | Debug menu, navigation overlay, F1 theft test controls | `CityBlockBootstrap`, `GameSession`, `NpcPresenter` | in use |

## 3. NOT implemented and NOT approved — do not assume, do not start
Propose first; wait for explicit user approval.
- Police response, heat consequences, broader crime catalogue, NPC theft/pickpocketing, aggression, weapons, stealth, factions, reputation, quest rules and rent penalties.
- Cleaning risk/delay/witness/heat, dynamic fence prices, more contacts/fences/items/rates, passive laundering.
- Production (Slice C): no workspace, recipe inputs/outputs or timing approved. Recipe content, water grades, food production and crops remain open.
- Dynamic pricing, supply/demand, NPC shopping, finite stock, barter, currency exchange, business ownership.
- Final NPC cast, NPC memory persistence, second opportunity, scenario owner for `WorldPacingDirector`, final art/audio/font and new packages/third-party code.

## 4. Binding decisions
- Design answers: `docs/OPEN_QUESTIONS.md`. Decisions: `docs/decisions/` (0004 permits test-only art packs).
- Economy: event-driven transactions; fixed authored prices/offers; clean CARD and physical Dirty Cash remain separate.
- New mechanics require explicit approval; one active gameplay initiative at a time. See `AGENTS.md` and `docs/DEVELOPMENT_WORKFLOW.md`.

## 5. Verification gaps
- **Current branch:** rerun the full EditMode suite (expected 396 tests) after SLICE-B-HARDENING-001. The user's last report was 392/392 before these four new tests.
- **Slice B:** Unity compilation and Play Mode check of the rebuilt contact panel; test exact selected fence stack, reject legitimate item/no capacity, clean exact stack, verify 24/6 split, save/reload contact stash, ownership, accounts and ledgers.
- **Theft:** witnessed vs unwitnessed Play Mode retest after the heat correction.
- Game-layer `GameSession.ExchangeWithContact/CleanDirtyCash` and the contact panel do not yet have automated Game-layer tests.
- Repository cleanup: Tier 1 steps 1–7 are applied except M2-card extraction. The source archive contains seven M2 cards (M2-01–M2-07), not eleven; it is preserved at `docs/tasks/archive/signal-docs-M2-cards-source.zip` pending reconciliation.
- Parked: locker capacity edge cases, actual v1-save migration, automated city-transition tests, platform save replacement.

## 6. Next action
User runs the full **396-test EditMode suite** in Unity, then the Slice B Play Mode exchange → cleaning → save/reload checklist. Do not start Slice C until the current slice is verified and the user chooses the next initiative.

## 7. Maintaining this file
Rewrite this summary when reality changes; keep it short and cite code paths. Append one dated entry per session to `docs/progress/LOG-YYYY-MM.md`. Never claim tests or gameplay verified without the user's report; move unapproved ideas to section 3.
