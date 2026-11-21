# Progress — read this first

Reconciled 2026-11-21 from the append-only project log, user verification reports and the current working-branch documentation. Historical detail remains in docs/progress/LOG-2026-11.md.

## 1. Where we are
- Unity 6000.6.9f1, 2D URP. Active architecture: Signal.Core (plain C#, no UnityEngine) + Signal.Game (Unity layer). Assets/_Project/_Prototype/ and Main.unity are legacy reference only.
- Save schema v14. The next initiative must inspect SaveMapper and increment only if new serialized state is required.
- User reported **396/396 EditMode tests passed on 2026-11-20** and subsequently reported the gameplay test passed. The report did not enumerate every individual negative/save-load scenario; do not infer a more detailed checklist than the user confirmed.
- **Most recent completed initiative: SLICE-B-HARDENING-001.** The user-confirmed test and gameplay report closes the hardening follow-up at the reported level.
- **Current design initiative: LIVELIHOOD-B1-B3 — First Workshop.** Direction B is selected. Detailed specification and B-01 through B-07 task cards are prepared; no gameplay implementation has started.
- Working branch: chore/collaboration-workflow-and-audit. Keep main untouched.

## 2. ALREADY IMPLEMENTED — do not rebuild, duplicate or re-propose
Extend only when a task card names a concrete gap. Search the code before claiming something is missing.

| Area | What exists | Where | Status |
|---|---|---|---|
| Hideout, city, save | Walkable hideout/workbench, connected city block, home storage, JSON save F5/F9 | Game: RoomBootstrap, CityBlockBootstrap, StoragePanel, SaveFileStore; Core: SaveData | user-verified |
| Grid inventory | Placement, rotation, whole-stack transfers, snapshots, drag math | Core: GridInventory, InventoryTransfer, InventoryDragMath | user-verified |
| Item identity/state/knowledge | Stable stack IDs; quality, condition, authenticity, ownership incl. Stolen; knowledge book/viewer | Core: ItemInstanceState, ItemKnowledgeBook; Game: ItemViewerPanel | user-verified |
| Time, rent, heat, stats | Pausable clock, rent ledger, HeatMeter 0–100, stats | Core: GameClock, RentLedger, HeatMeter, PlayerStats | tested; heat awarded by witnessed theft only |
| Crafting core | Recipe/Ingredient, Crafting.TryCraft with inventory rollback; RecipeDefinition asset; presentation-only WorkbenchWorkspace | Core: Crafting; Game: RecipeDefinition, WorkbenchWorkspace | Core tested; no production content/player loop |
| NPC framework | Shared social world, perception, knowledge/rumours, directed relationships, utility brain, routines, encounters and A* navigation | Core: NpcSocialWorld, NpcPerception, NpcUtilityBrain; Game: NpcPresenter, CityNavigation* | user-verified |
| World pacing | Permission gate for consequential developments; Core only, not wired/tuned | Core: WorldPacingDirector | tested |
| Discovery/opportunities | Discovery ledger, relationship book, interaction conditions, opportunity lifecycle and Milo's Parcel | Core: PlayerDiscoveryLedger, PlayerRelationshipBook, Opportunity*; Game: MiloParcelOpportunityController | user-verified |
| Economy foundation | Ownership independent of container, atomic transfer, append-only ledgers, clean CARD accounts incl. residents | Core: AssetOwnershipBook, AssetTransferService, EconomicTransactionLedger, CleanMoneyAccount, EconomicAccountBook | user-verified |
| Retail + sell | Fixed-price BUY catalogue, per-business buyback, BUY/SELL panel, revenue to owner | Core: RetailService, Shop; Game: BusinessDefinition, RetailPanel | user-verified |
| Scavenging | Stable-ID containers, one-time generation, progressive reveal, persistence, transactional transfer, dual inventory UI | Core: SearchableContainer; Game: ContainerLootProfile, SearchableContainerPanel | user-verified (368/368 + Play Mode) |
| Theft | Reusable resident-owned container; Stolen state; crime ledger and global progression; nearest witness gets character-scoped event | Core: CrimeOutcome, CriminalProgressionTrack; Game: ResidentPropertyContainerDefinition, GameSession.RecordTheft | user-verified; witnessed/unwitnessed Play Mode retest after heat fix remains a separate gap |
| Dirty Cash / Slice B | Physical Dirty Cash (10/unit); unnamed Clean_Dirtycash contact; ITEM_A ×2 → 3 units; 20% cleaning fee; atomic services, separate ledgers, persistent contact stash; dual-inventory UI | Core: DirtyCashTransactions.cs; Game: DirtyCashContactOfferDefinition, DirtyCashContactPanel, GameSession | user-reported 396/396 and gameplay test passed |
| Dev tooling | Debug menu, navigation overlay, F1 theft test controls | CityBlockBootstrap, GameSession, NpcPresenter | in use |

## 3. Current initiative — Direction B livelihood
**Parent specification:** docs/tasks/LIVELIHOOD-B1-B3-FIRST-WORKSHOP.md

Approved decisions:
- Workshop model supports both player-operated and NPC-owned production; begin with the player's hideout workshop.
- One accepted/active customer contract at a time.
- First practical hideout upgrade unlocks one additional authored recipe/capability.
- Production creates physical goods through authored recipes and reuses existing Core/Game foundations.

Task breakdown:
- B-01 Production foundation and player workshop.
- B-02 Single active customer contract and physical fulfilment.
- B-03 Practical hideout upgrade and recipe unlock.
- B-04 Repair/restoration (future design placeholder).
- B-05 Social life and recurring customers (future design placeholder).
- B-06 Livelihood expansion (future design placeholder).
- B-07 Neighbourhood expansion (future design placeholder).

Detailed task cards are under docs/tasks/LIVELIHOOD-B-*.md. B-04 through B-07 are roadmap placeholders, not approved mechanics or implementation permission.

## 4. NOT implemented / not approved
- No player-facing production workspace/recipe loop or authored production content yet. Existing Crafting and WorkbenchWorkspace are foundations only.
- Exact first product, recipes, inputs/outputs, customer/issuer, contract reward, upgrade cost/effect content, provenance acceptance and partial-delivery policy remain open.
- Production timing/queues, quality rolls, staffing, spoilage, NPC production execution and procedural contracts are not approved.
- Repair/restoration, social/favour rewards, additional professions and a second district remain future design work.
- Police response, broader crime catalogue, cleaning risk/delay/heat, dynamic pricing, supply/demand, NPC shopping, finite stock/restocking, passive revenue and new packages remain outside the active initiative.

## 5. Verification and known gaps
- Slice B hardening: user reported 396/396 EditMode tests and a passing gameplay test on 2026-11-20. No exact sub-checklist beyond that report is inferred.
- Theft: witnessed vs unwitnessed Play Mode retest after the heat correction remains outstanding unless separately reported.
- Game-layer automated coverage for some GameSession transaction/panel paths remains limited.
- Repository cleanup: Tier 1 steps 1–7 are applied except M2-card extraction. The source ZIP contains seven M2 cards (M2-01–M2-07), not eleven; preserved at docs/tasks/archive/signal-docs-M2-cards-source.zip pending reconciliation.
- Parked reliability checks: locker capacity edge cases, actual v1-save migration, automated city-transition tests and target-platform save replacement.

## 6. Next action
Resolve the remaining B1-B3 content/design questions in docs/OPEN_QUESTIONS.md, then implement B-01 as the first playable production loop. Do not start B-02 or B-03 implementation before their dependencies are playable. Keep one active gameplay initiative.

## 7. Maintaining this file
Rewrite this summary when reality changes; keep it short and cite code paths. Append one dated entry per session to docs/progress/LOG-YYYY-MM.md. Never claim tests or gameplay verified without the user's report; move unapproved ideas to section 4.
