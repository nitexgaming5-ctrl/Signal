# Signal — project index

Entry point for agents. Read this file, then `PROGRESS.md`, then only the specialist references and task card relevant to the assigned work.

## What Signal is
Signal is a top-down 2D urban RPG about making a precarious living in one connected city. The player explores a compact neighbourhood, meets residents with public and private lives, handles physical goods, pursues lawful and unlawful opportunities, and returns to a practical hideout that gradually becomes more useful and personal.

Pillars:
1. A lived-in hideout with storage, furniture, workspaces, equipment and practical upgrades.
2. One layered city where streets, buildings, residents, legitimate businesses and illicit contacts coexist.
3. People with bounded knowledge: NPCs observe, remember and communicate; they are not omniscient.
4. Physical goods with stable identity and condition, quality, authenticity, ownership and provenance.
5. Connected livelihoods: repair, production, customer work, scavenging, trade, services, social favours, investigation and crime.
6. A bounded economy based on resident CARD accounts, authored stock/prices and explicit transactions.
7. Consequences designed through connected scenarios, not incidental global side effects.
8. Progress through capabilities, equipment, access, relationships, possessions and places.

Signal is not primarily a shop simulator. Activities connect through the same player, city, inventory, people, economy and hideout.

## Current project truth
See [PROGRESS.md](PROGRESS.md) for implementation status, verification, blockers and next action. The strategic livelihood direction is **Direction B — meaningful everyday life and work**.

- B-01 through B-06 livelihood foundations are implemented; B-06 Customer Repair Orders is user-verified.
- P-01 Character Foundation and P-02 Attributes & Skills are implemented and user-verified.
- P-03 Equipment & Backpack has a substantial implementation: equipment slots, portable backpack lifecycle, three pockets, hidden-pocket visibility, pocket transfers/swaps and pocket-aware service integration.
- The current verification baseline and outstanding checks are in PROGRESS; do not infer current verification from older task-card history.
- **Sprite creation, wearable overlays, wardrobe presentation and character-art expansion are paused by the user.** Do not resume unless explicitly asked.
- B-07 Neighbourhood Expansion remains the next strategic livelihood/world track after P-03 non-art closeout.
- W-01 Dynamic Weather & Cold Exposure is a separate future track, not active.

## Documentation map
| Need | Source |
|---|---|
| Current status, verified baseline, next action | `PROGRESS.md` |
| Game identity and pillars | `VISION.md` |
| Approved player-facing rules | `DESIGN.md`, `decisions/` |
| Active and future sequence | `ROADMAP.md` |
| Genuine unresolved design questions | `OPEN_QUESTIONS.md` |
| Code boundaries and runtime composition | `ARCHITECTURE.md` |
| ScriptableObject definitions, runtime state and save schema | `DATA_MODEL.md` |
| Agent authority and workflow | `AGENTS.md`, `DEVELOPMENT_WORKFLOW.md` |
| NPC architecture and behaviour | `NPC_FRAMEWORK.md`, `NPC_SIMULATION.md`, `NPC_RUNTIME.md` |
| Visual/UI constraints (reference only while art is paused) | `ART_AND_UI.md`, `ART_TODO.md` |
| Authored starter content | `content/STARTER_CATALOGUE.md` |
| Current implementation acceptance | `tasks/` |
| Historical detail | `progress/LOG-2026-11.md`, `tasks/archive/` |
| Parked ideas | `IDEA_BACKLOG.md` |

## Task-directed reading
- Livelihood: relevant B task card and First Workshop parent card where applicable.
- Equipment/inventory/save: P-03 plus current Core/Game services and tests.
- NPC: NPC Framework, then relevant simulation/runtime reference and task card.
- World expansion: ROADMAP, VISION, ARCHITECTURE, NPC references and B-07 specification.
- Documentation: this index, PROGRESS and only files being changed. The historical log is for traceability, not routine planning.

## Repository map
- `Assets/_Project/Core/`: plain C# domain rules, inventory, transactions, NPC simulation, save DTOs and tests.
- `Assets/_Project/Game/`: Unity behaviours, GameSession composition, UI, ScriptableObject definitions and Unity integration.
- `Assets/_Project/Content/`: authored gameplay content.
- `Assets/_Project/Scenes/`: active Unity scenes; `HideoutSlice` is the current playable slice.
- `Assets/_Project/Tests/EditMode/`: Core and Game EditMode tests.
- `docs/tasks/`: active/future task cards; `docs/tasks/archive/`: completed/superseded cards.
- `docs/decisions/`: durable approved decisions; `docs/progress/`: append-only log and historical handoffs.

## Terminology
- **Direction B**: strategic life/livelihood path.
- **B-01 to B-06**: implementation packages within that track.
- **Clean CARD**: account-based currency.
- **Dirty Cash**: a physical item for explicitly illicit exchanges/cleaning.
- **Ownership**: who authoritatively owns an item.
- **Custody**: which actor/container currently holds an item; custody does not itself transfer ownership.
- **Player/NPC knowledge**: partial beliefs, not hidden world truth.

When documents disagree, prefer the newest explicit user decision and current implementation evidence, then correct stale documentation. Never let an obsolete task card override current repository truth.
