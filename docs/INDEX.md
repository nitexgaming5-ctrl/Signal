# Signal — project index and world guide

This is the entry point for agents working on Signal. It is both a compact explanation of the full game and a map of the repository. It should let an agent understand the whole project before deciding which specialist files and source paths are relevant to the current job.

## What Signal is

Signal is an immersive top-down 2D urban RPG about making a precarious living in one layered city. The player meets people, trades and handles goods, works legitimate livelihoods, commits crimes, uncovers secrets, takes risks and returns to a hideout that gradually becomes more useful and personal.

The game combines:
- **Life and hustle simulation:** a compact explorable city, practical everyday activities, limited carrying capacity, money pressure, workspaces and meaningful possessions.
- **Legal and illegal livelihoods in one world:** lawful work, trade, services and production coexist with theft and other fictional crime. They are parallel options, not separate city maps or mutually exclusive character classes.
- **Narrative RPG:** authored residents, relationships, secrets, requests, rumours and opportunities make the simulation matter.
- **Hands-on interaction:** places, people, containers, furniture, tools and physical goods should matter. Menus support the world rather than replace it.
- **Visible personal progression:** the hideout, equipment, discoveries, relationships and possessions can reflect the player's choices.

The player is not primarily running a shop. Shopping and selling are only two activities in a larger cycle of going out, meeting people, gathering or acquiring goods, making decisions, taking opportunities and returning home.

## The intended game scope

The following is the full direction, not a claim that every feature is already implemented.

### 1. A lived-in hideout

A modest walkable house/planque is the emotional anchor. It should support storage, functional furniture, workspaces, equipment, personalisation and visible upgrades. The player returns here between city activities. The current hideout contains a bed, storage and a physical workbench; the workbench now has a recipe-book/production foundation.

### 2. One shared, layered city

Ordinary residents, legitimate businesses, services, suppliers, criminal contacts and hidden dealings share the same streets and buildings. The city should expand through connected blocks and interiors, with density and reasons to revisit taking priority over map size.

### 3. People with public and private lives

Residents have identities, roles, routines, needs, partial knowledge, relationships and potentially concealed activities. They are not omniscient. Their knowledge should come from observation or explicit communication. Important residents may receive deeper simulation; background characters should remain inexpensive. Dialogue, requests and consequences are authored to support scenarios.

### 4. Goods with identity and history

Items are physical inventory stacks with stable identity and state. Quality, condition, authenticity and ownership/provenance can distinguish copies of the same definition. The player may have incomplete knowledge of those properties. Goods can be gathered, inspected, produced, repaired, delivered, bought, sold, stolen or exchanged where the relevant player loop is designed.

### 5. Many connected livelihoods and activities

The intended activity range includes scavenging, buying and selling, production, customer work, repair/restoration, services, delivery, social favours, investigation and criminal opportunities. Farming is optional rather than the assumed centre of the game. Activities should combine through the same city, inventory, people and hideout.

### 6. A bounded, event-driven economy

Residents own clean CARD accounts; businesses are authored catalogues and their revenue belongs to resident owners. Dirty Cash is a physical item for explicitly illicit uses and cleaning. Transactions are explicit, validated and atomic. Do not turn this into a universal stock/supply-demand simulator, automatic NPC shopping loop or passive-income spreadsheet.

### 7. Crime, risk and consequences

Crime is a core pillar alongside lawful livelihoods. Theft and dirty-money foundations exist. Global criminal progression and local heat are separate concepts. Serious consequences, police response, crime categories, witness-sharing and progression unlocks need connected scenario design; do not invent them as incidental side effects.

### 8. Progression through play

Progress should emerge through practical equipment, new authored capabilities, access to people and places, discoveries, relationships, possessions and scenario outcomes. Avoid a generic feature tree without a clear player fantasy.

## Current project snapshot

The current implementation status, branch head, verification and next action live in `PROGRESS.md`. The current strategic gameplay direction is **Direction B — meaningful everyday livelihood**.

- The connected hideout, street, corner bakery and apartment foundations exist.
- Inventory, item identity/state, knowledge, save/load, ownership transfers, clean accounts, retail, scavenging, theft, Dirty Cash, NPC routines and contextual narrative opportunity foundations exist.
- The First Workshop B-01 to B-03 foundation and starter content are committed: Fabric Roll/Thread Spool/Adhesive Tube/Fastener Pack → Basic Patch Kit; Milo requests one kit for 40 CARD; Rhea sells the Compact Sewing Machine for 90 CARD after contract completion; the upgrade unlocks Reinforced Patch Kit.
- The latest user-reported full EditMode result is **417/417 passed on 2026-11-24**, before the hybrid buyback code/test additions. A fresh full run is required for current HEAD.
- The full EditMode suite passes. Remaining First Workshop Play Mode acceptance covers Rhea's three agenda threads, the Compact Sewing Machine purchase/installation, Reinforced Patch Kit production and F5/F9 persistence.

### Current and future strategic direction

1. **B-01 to B-03 — First Workshop:** make authored physical goods, fulfil one customer request and improve the hideout workshop. This is the active vertical slice.
2. **B-04 — Repair, restoration and textile business:** design approved; hybrid buyback framework committed; finish First Workshop Play Mode acceptance before integrating the Work Jacket, Rhea's textile counter and repair interaction.
3. **B-05 — Social life and recurring customers:** future design placeholder; deepen player-driven relationships and authored requests.
4. **B-06 — Livelihood expansion:** choose another coherent legitimate activity, not a universal profession simulator.
5. **B-07 — Neighbourhood expansion:** add connected geography only when existing places have meaningful reasons to revisit.
6. **Crime depth and long-term progression:** continue as separate, scenario-led design that reuses the shared city, item and economy foundations.

B-05 to B-07 remain future placeholders. B-04 has an approved textile-repair design and a committed hybrid buyback foundation; the remaining B-04 gate is completion of the First Workshop Play Mode acceptance, not another design approval.

## How to navigate this repository

The agent should use the task and the nature of the change to select relevant references. Do not read every document by default.

| Need | Source of truth |
|---|---|
| Current status, verified baseline, blockers, next action | `PROGRESS.md` |
| Full game identity, pillars and player fantasy | `VISION.md` |
| Approved player-facing rules and mechanics | `DESIGN.md` |
| What is active, future and deliberately deferred | `ROADMAP.md` |
| Decisions that still require design | `OPEN_QUESTIONS.md` |
| Code boundaries, dependencies and runtime composition | `ARCHITECTURE.md` |
| ScriptableObject definitions, runtime state and save schema | `DATA_MODEL.md` |
| Collaboration workflow and vertical-slice standard | `DEVELOPMENT_WORKFLOW.md` |
| NPC architecture overview | `NPC_FRAMEWORK.md` |
| NPC simulation, perception, routines, knowledge and social boundaries | `NPC_SIMULATION.md` |
| NPC pacing, navigation and interruption/recovery | `NPC_RUNTIME.md` |
| Visual style, UI conventions and asset constraints | `ART_AND_UI.md`, `ART_TODO.md` |
| Parked ideas only | `IDEA_BACKLOG.md` |
| Authored First Workshop item/recipe/contract/upgrade catalogue and playtest steps | `content/STARTER_CATALOGUE.md` |
| Current initiative implementation checklist | `tasks/` (active cards are linked from Progress/Roadmap) |
| Durable user decisions | `decisions/` |
| Historical milestones and prior handoffs | `progress/` and `tasks/archive/` |

### Task-directed reading

- A production, contract, upgrade or content task should use the First Workshop parent card plus the relevant B-01/B-02/B-03 card, then inspect the corresponding Game/Core code and tests.
- An inventory, retail, account or save task should start from the relevant classes/tests and consult Architecture/Data Model; older economy cards are historical audit material.
- An NPC task should use the short NPC Framework overview, then the relevant NPC_SIMULATION or NPC_RUNTIME reference and the specific scenario/agenda card.
- An art/UI task should use Art and UI and its asset TODO.
- A documentation task should consult Index, Progress and only the documents being changed. Historical logs are for traceability, not normal planning.

## Repository map

- `Assets/_Project/Core/`: plain C# domain rules, inventories, transactions, NPC simulation, save DTOs and tests.
- `Assets/_Project/Game/`: MonoBehaviours, GameSession composition, Unity integration, ScriptableObject definitions and UI.
- `Assets/_Project/Content/`: authored ScriptableObject content (items, NPCs, businesses, containers, recipes, contracts and upgrades).
- `Assets/_Project/Scenes/`: active Unity scenes; HideoutSlice is the current playable slice.
- `Assets/_Project/Tests/EditMode/`: Core and Game EditMode tests.
- `docs/tasks/`: active task cards and future design placeholders.
- `docs/content/`: concise human-readable catalogue of authored gameplay content.
- `docs/tasks/archive/`: completed/superseded task cards.
- `docs/progress/`: append-only log and archived handoff snapshots.
- `docs/decisions/`: durable approved decisions.

## Important terminology

- **Direction B** refers to the strategic livelihood path.
- **B-01, B-02 and B-03** are implementation packages inside the First Workshop initiative; they are not the same as strategic B1/B2/B3 names used in older planning.
- **Clean CARD** is the player's/residents' clean account currency.
- **Dirty Cash** is a physical inventory asset, not a bank balance.
- **Ownership** is independent of which inventory currently stores an item.
- **Custody** describes which actor operates a container; it does not determine asset ownership.
- **Player knowledge** and **NPC knowledge** are partial beliefs, not the hidden truth of the world.

When documents disagree, use the newest explicit decision and current implementation as evidence, then repair the stale documentation. Never let an obsolete task card silently override the current source of truth.
