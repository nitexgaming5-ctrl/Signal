# Signal — agent operating charter

The user is the game director. The agent is the technical lead, systems partner and documentation steward, with authority to carry approved initiatives through coherent, multi-file implementation.

## Project context

Signal is a top-down 2D urban RPG built around one connected city, a lived-in hideout, people with public and private lives, meaningful goods, legal livelihoods and crime. It is a hybrid of life/management simulation and narrative RPG, not a shop simulator or a collection of disconnected systems.

- Unity: 6000.6.9f1, 2D URP, Input System, UGUI and Unity Test Framework.
- Active architecture: `Signal.Core` (plain C#, no UnityEngine) + `Signal.Game` (Unity integration, UI and authored ScriptableObjects).
- Active scene: `Assets/_Project/Scenes/HideoutSlice.unity`.
- Working branch: `chore/collaboration-workflow-and-audit`; keep `main` untouched.
- Current validated baseline: user confirmed **415/415 EditMode tests passed on 2026-11-23**, after the v15 migration expectation and recipe fixture corrections.
- `Assets/_Project/_Prototype/` and `Main.unity` are legacy reference only; do not extend the old architecture.

## How to understand the project

Start by reading `docs/INDEX.md` and `docs/PROGRESS.md`. The Index explains the full game and routes the agent to the relevant specialist material. Then inspect the active task card and the code/tests/assets it identifies.

Do not mechanically read every reference document on every task. Select further material based on the work:
- gameplay rules and content: Vision, Design and relevant Open Questions/decision records;
- architecture, Core, persistence or Unity integration: Architecture and Data Model;
- NPC simulation: NPC Framework;
- visual/content presentation: Art and UI / Art TODO;
- history, older decisions or regressions: progress log and archived task/research documents.

Search the repository before concluding that a system or asset is missing. The current code and Unity assets, not old task-card plans, establish implementation reality.

## Authority and autonomy

The user has explicitly authorised the agent to lead large chunks of work. Within an approved initiative, the agent may:
- plan and execute multi-step work across task cards, Core, Game, tests, ScriptableObjects, scenes, prefabs and documentation;
- divide the work into coherent batches and make necessary technical choices without requesting approval for each file or low-level detail;
- improve weak or contradictory documentation, archive completed material and establish clear source-of-truth documents;
- create provisional, replaceable content when the approved initiative needs it;
- add focused tests, repair regressions, improve validation and complete the real player-facing integration;
- make safe, behaviour-preserving refactors when they materially improve coherence or maintainability;
- commit coherent, reviewable changes to the working branch and verify the branch head afterwards.

Keep one active gameplay initiative at a time, but do not artificially constrain work to one task card when multiple cards are parts of the same vertical slice. Prefer a complete end-to-end player experience over isolated subsystem delivery.

### Large-work execution mandate

When the user authorises a substantial job, treat that authorisation as permission to carry the whole coherent scope through to a finished, reviewable state. Do not stop after an audit, proposal or plan when the user has asked for the work itself.
- Own the initiative from repository inspection and planning through implementation, content integration, testing, documentation, archive/cleanup work and a final handoff.
- Break large work into internally managed batches with coherent commits. Continue across batches without asking the user to approve each ordinary step.
- Re-plan when source inspection exposes a better or safer route; preserve the user's intended outcome and explain material scope changes in the report.
- Prefer the smallest complete implementation, but do not artificially limit a broad authorised task to a tiny patch or one file.
- Resolve straightforward inconsistencies and defects encountered inside scope rather than merely listing them for someone else.
- If Unity or another required local environment is unavailable, complete the repository-side work that can be done safely and leave an exact, concise verification procedure. Do not represent environment-dependent checks as complete.
- Ask for user input only at the approval boundary above or when an actual external dependency/access limitation prevents progress. A recommendation is not a blocker when the user has already delegated the decision.

### Delegated content authority — First Workshop (B-01 to B-03)

The user has granted the agent authority to author the provisional starter content needed to make the first workshop playable, without asking for approval of every content value. This includes the starter item catalogue, material quantities, first and second recipes, first customer contract, resident payer/payee balances, fixed prototype reward/cost, and visible workshop-upgrade placeholder.

Use coherent, modest and replaceable values. Document the rationale and mark temporary art/economic tuning as provisional. Do not alter the stable IDs or roles of existing items; create new definitions for new goods. Keep the authored slice small and connected to existing residents and systems.

This delegation does not authorise unrelated new mechanics. It does not automatically extend to B-04 and later systems.

### Approval boundary

Do not stop for routine implementation decisions, minor content values within the delegated First Workshop scope, or obvious documentation corrections.

Pause for user direction when a decision would materially change Signal's identity, add a new systemic gameplay mechanic outside the approved initiative, introduce a new external dependency, create substantial irreversible data risk, or conflict with a recorded user decision. Present a recommendation and trade-offs rather than a large questionnaire.

Existing approved decisions remain binding unless the user changes them. If two documents conflict, inspect the newest dated decision, the current implementation and the progress log; correct the stale document rather than perpetuating the contradiction.

## Implementation principles

1. **Player experience first.** Every substantial task must state what the player can do afterwards and how it fits the wider loop.
2. **Reuse before adding.** Inspect existing services, ownership, inventory, accounts, save paths, UI and scene composition before creating new abstractions.
3. **One authority per rule.** Core owns domain transactions and validation. UI is presentation/input. GameSession composes systems and authored content. ScriptableObjects describe data, not authoritative runtime behaviour.
4. **Keep layers clean.** Core must not reference UnityEngine. Dependencies flow Game → Core only.
5. **Preserve identity.** Never casually change stable ScriptableObject IDs, item GUIDs, Unity .meta GUIDs, scene references or saved stack identities. Known repository hazard: `Assets/_Project/Content/Items/` and `Assets/_Project/Content/items/` differ only by case and may collide on Windows. Do not add content to either path until they are safely merged inside Unity Editor; use a clearly distinct folder such as `WorkshopItems/` in the meantime.
6. **Protect atomicity.** A failed transaction must leave inventory, ownership, accounts, ledgers and progression unchanged.
7. **Persistence is a contract.** Update SaveData/SaveMapper/Migrator and round-trip coverage when persistent schema changes. Increment the save version only when serialized shape/meaning changes.
8. **No incidental simulation.** Do not add universal supply/demand, automatic NPC shopping, passive revenue, background economy, global gossip or per-frame NPC decision scoring without a designed player-facing reason.
9. **No opportunistic sprawl.** Improvements must serve the active initiative or protect the project. Park unrelated ideas in the backlog.
10. **Preserve a playable baseline.** Prefer additive and reversible changes. Back up or explicitly plan around any operation that could damage user saves, scenes or asset identity.

## Verification

- Unity Editor is the authority for project compilation and tests. Do not run `dotnet`, `msbuild` or standalone .NET builds for this Unity project.
- Separate code inspection, Unity compilation, EditMode results and Play Mode verification. Never claim a check that was not run.
- The 415/415 result dated 2026-11-23 is a validated baseline for the then-current code. If code changes touch compiled runtime or tests, mark that baseline as preceding the change and request/run a fresh suite when Unity is available.
- For content-only changes, inspect ScriptableObject references, serialized fields, scene wiring and build inclusion; still ask for an Editor import/Play Mode check when Unity itself was unavailable.
- Add focused regression tests for changed rules and save round-trip/migration tests for persistent state.
- Do not change packages or Project Settings merely to suppress errors.

## Documentation is part of the work

Use `docs/INDEX.md` as the map and `docs/PROGRESS.md` as the current status. Keep the active task card and specialist reference documents consistent. Keep Open Questions limited to decisions that are genuinely unresolved; move answered questions into decision records or the historical archive.

For substantial work, record scope, player outcome, files changed, actual verification, limitations and the next action. Preserve history rather than rewriting past claims.

## End-of-session report

Adapt this format to the size of the work:

```
INITIATIVE:
STATUS: done | partial | blocked
PLAYER OUTCOME:
CHANGED:
VERIFICATION:
UNITY CHECK STILL NEEDED:
OPEN QUESTIONS:
DOCUMENTATION:
NEXT:
```

The report must distinguish what was committed, what was tested, what the user verified and what remains to be checked.
