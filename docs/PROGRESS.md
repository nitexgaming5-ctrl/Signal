# Progress — read this first

Reconciled 2026-11-23 from the append-only project log, user verification reports and the current working-branch documentation. Historical detail remains in docs/progress/LOG-2026-11.md.

## 1. Where we are
- Unity 6000.6.9f1, 2D URP. Active architecture: Signal.Core (plain C#, no UnityEngine) + Signal.Game (Unity layer). Assets/_Project/_Prototype/ and Main.unity are legacy reference only.
- Save schema v15. Version 15 adds production history, customer contract state, workshop upgrade IDs and the issuer's physical receiving inventory; v14 saves migrate with empty new collections.
- User reported **396/396 EditMode tests passed on 2026-11-20** and subsequently reported the gameplay test passed. The report did not enumerate every individual negative/save-load scenario; do not infer a more detailed checklist than the user confirmed.
- **Most recent completed initiative: SLICE-B-HARDENING-001.** The user-confirmed test and gameplay report closes the hardening follow-up at the reported level.
- **Current implementation initiative: LIVELIHOOD-B1-B3 — First Workshop.** The B-01..B-03 Core/Game foundations are committed, including production, contracts, workshop progression and v15 persistence. No first recipe/customer/upgrade content has been assigned, and this commit has not been compiled or tested in Unity.
- Working branch: chore/collaboration-workflow-and-audit. Keep main untouched.

## 2. ALREADY IMPLEMENTED — do not rebuild, duplicate or re-propose
Extend only when a task card names a concrete gap. Search the code before claiming something is missing.

| Area | What exists | Where | Status |
|---|---|---|---|
| Hideout, city, save | Walkable hideout/workbench, connected city block, home storage, JSON save F5/F9 | Game: RoomBootstrap, CityBlockBootstrap, StoragePanel, SaveFileStore; Core: SaveData | user-verified |
| Grid inventory | Placement, rotation, whole-stack transfers, snapshots, drag math | Core: GridInventory, InventoryTransfer, InventoryDragMath | user-verified |
| Item identity/state/knowledge | Stable stack IDs; quality, condition, authenticity, ownership incl. Stolen; knowledge book/viewer | Core: ItemInstanceState, ItemKnowledgeBook; Game: ItemViewerPanel | user-verified |
| Time, rent, heat, stats | Pausable clock, rent ledger, HeatMeter 0–100, stats | Core: GameClock, RentLedger, HeatMeter, PlayerStats | tested; heat awarded by witnessed theft only |
| Crafting + production | Existing Crafting.TryCraft retained; actor-aware ProductionService, production history, authored RecipeDefinition and recipe-book workbench controls | Core: Crafting, ProductionService, ProductionTransaction; Game: RecipeDefinition, WorkbenchWorkspace, GameSession | implementation committed; Unity verification/content pending |
| NPC framework | Shared social world, perception, knowledge/rumours, directed relationships, utility brain, routines, encounters and A* navigation | Core: NpcSocialWorld, NpcPerception, NpcUtilityBrain; Game: NpcPresenter, CityNavigation* | user-verified |
| World pacing | Permission gate for consequential developments; Core only, not wired/tuned | Core: WorldPacingDirector | tested |
| Discovery/opportunities | Discovery ledger, relationship book, interaction conditions, opportunity lifecycle and Milo's Parcel | Core: PlayerDiscoveryLedger, PlayerRelationshipBook, Opportunity*; Game: MiloParcelOpportunityController | user-verified |
| Economy foundation | Ownership independent of container, atomic transfer, append-only ledgers, clean CARD accounts incl. residents | Core: AssetOwnershipBook, AssetTransferService, EconomicTransactionLedger, CleanMoneyAccount, EconomicAccountBook | user-verified |
| Retail + sell | Fixed-price BUY catalogue, per-business buyback, BUY/SELL panel, revenue to owner | Core: RetailService, Shop; Game: BusinessDefinition, RetailPanel | user-verified |
| Scavenging | Stable-ID containers, one-time generation, progressive reveal, persistence, transactional transfer, dual inventory UI | Core: SearchableContainer; Game: ContainerLootProfile, SearchableContainerPanel | user-verified (368/368 + Play Mode) |
| Theft | Reusable resident-owned container; Stolen state; crime ledger and global progression; nearest witness gets character-scoped event | Core: CrimeOutcome, CriminalProgressionTrack; Game: ResidentPropertyContainerDefinition, GameSession.RecordTheft | user-verified; witnessed/unwitnessed Play Mode retest after heat fix remains a separate gap |
| Dirty Cash / Slice B | Physical Dirty Cash (10/unit); unnamed Clean_Dirtycash contact; ITEM_A ×2 → 3 units; 20% cleaning fee; atomic services, separate ledgers, persistent contact stash; dual-inventory UI | Core: DirtyCashTransactions.cs; Game: DirtyCashContactOfferDefinition, DirtyCashContactPanel, GameSession | user-reported 396/396 and gameplay test passed |
| Customer contracts | Single-active contract book; whole-stack physical delivery to issuer inventory; issuer-funded clean CARD payment; customer panel | Core: CustomerContracts; Game: CustomerContractDefinition, CustomerContractPanel, GameSession | implementation committed; content/Unity verification pending |
| Workshop progression | One-time clean-CARD upgrade purchase, persisted acquisition and second-recipe unlock with visible prop hook | Core: WorkshopProgression; Game: WorkshopUpgradeDefinition, WorkbenchWorkspace, GameSession | implementation committed; content/Unity verification pending |
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
- The recipe-book production UI and Core transaction path now exist, but no recipe/customer/upgrade ScriptableObject content is assigned in HideoutSlice. The first playable livelihood loop is therefore not content-complete.
- Exact first product, recipes, inputs/outputs, customer/issuer, contract reward, upgrade appearance/cost/payee/prerequisite, provenance acceptance and partial-delivery policy remain open. Definitions deliberately require authored values rather than inventing them.
- Production timing/queues, quality rolls, staffing, spoilage, NPC production execution and procedural contracts are not approved.
- Repair/restoration, social/favour rewards, additional professions and a second district remain future design work.
- Police response, broader crime catalogue, cleaning risk/delay/heat, dynamic pricing, supply/demand, NPC shopping, finite stock/restocking, passive revenue and new packages remain outside the active initiative.

## 5. Verification and known gaps
- Slice B hardening: user reported 396/396 EditMode tests and a passing gameplay test on 2026-11-20. No exact sub-checklist beyond that report is inferred.
- Theft: witnessed vs unwitnessed Play Mode retest after the heat correction remains outstanding unless separately reported.
- The new Core production/contract/upgrade and save round-trip tests are committed but have not yet been run in Unity. Game-layer automated coverage for some GameSession transaction/panel paths remains limited.
- The user's 396/396 result dated 2026-11-20 is the previous Slice B baseline; it does not verify this new B-01..B-03 code.
- Repository cleanup: Tier 1 steps 1–7 are applied except M2-card extraction. The source ZIP contains seven M2 cards (M2-01–M2-07), not eleven; preserved at docs/tasks/archive/signal-docs-M2-cards-source.zip pending reconciliation.
- Parked reliability checks: locker capacity edge cases, actual v1-save migration, automated city-transition tests and target-platform save replacement.

## 6. Next action
Open HideoutSlice in Unity, compile, run the full EditMode suite, and exercise the new workbench/contract/upgrade paths after assigning temporary approved test content. Resolve the remaining authored-content questions in docs/OPEN_QUESTIONS.md before assigning final assets. Do not start strategic B-04+ work yet.

## 7. Maintaining this file
Rewrite this summary when reality changes; keep it short and cite code paths. Append one dated entry per session to docs/progress/LOG-YYYY-MM.md. Never claim tests or gameplay verified without the user's report; move unapproved ideas to section 4.
