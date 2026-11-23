# Roadmap — build a lived-in city through connected activities

## North star

Signal is a top-down urban RPG about making a precarious living in one shared city. The player inhabits a practical hideout, meets residents with public and private lives, handles physical goods, pursues lawful and unlawful opportunities, and sees progress in possessions, relationships, equipment and places.

This roadmap describes the direction and dependencies. Current implementation and verification are authoritative in `PROGRESS.md`; detailed approved player-facing rules live in `DESIGN.md`.

## Active initiative — Direction B: First Workshop (B-01 to B-03)

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

A hands-on legal service built around the existing per-item condition model. Start with one repairable category, one clear station/tool requirement and a visible before/after result. Keep quality, authenticity and ownership unchanged unless a specific later rule justifies otherwise. The repair rules and failure model need design when B-04 becomes active.

### B-05 — Social life and recurring customers

Deepen player-driven familiarity, trust, affinity, requests and recurring customer context. Use the existing directed relationship and NPC knowledge foundations. Avoid ambient relationship farming, universal reputation or automatic city-wide gossip.

### B-06 — Livelihood expansion

Choose one additional coherent activity such as material processing, food preparation, repair service, trade or an authored NPC-owned production business. Build one complete loop at a time. Do not build a generic profession simulator.

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
- Authored fixed-price business merchandise and per-business fixed buyback offers.
- Dirty Cash is a physical item; illicit exchange and cleaning are explicit, atomic and separately recorded.
- Theft records a crime outcome and global criminal progression separately from heat.
- Universal supply/demand, auto-shopping, passive sales and dynamic prices are excluded from the initial economy.

## Other long-term game pillars

The wider vision includes additional crime categories, hidden NPC activities, services, investigation, legal and illicit trades, meaningful item provenance, richer consequences, personalisation and eventually more city blocks. These should be designed as connected scenarios that reuse the current world rather than as a checklist of isolated managers.

## Engineering and workflow guardrails

- One active gameplay initiative at a time.
- Implement the smallest complete player-facing loop through Core, Game integration, UI, content and save state.
- Reuse existing transaction and inventory rules; do not fork them.
- No new Unity packages or third-party systems without approval.
- Core remains plain C#; Game owns Unity integration and authored content.
- Persistent changes require round-trip and migration coverage.
- A system is not complete merely because the classes compile: integrate it into the active scene and verify the experience.

## Historical detail

Completed task specifications and their verification snapshots are archived under `tasks/archive/`. The append-only session history is in `progress/LOG-2026-11.md`. These files are for specific audits and regressions, not routine roadmap reading.
