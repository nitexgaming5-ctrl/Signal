# Signal — Agent Rules

These rules govern technical work in the Signal repository. The user is the game director and final authority on game design.

## Project baseline
- Follow `docs/DEVELOPMENT_WORKFLOW.md`: one active gameplay initiative, vertical-slice outcomes, and a parked idea backlog in `docs/IDEA_BACKLOG.md`.
- Unity 6000.6.9f1, 2D URP, Input System 1.20.1, UGUI, Test Framework 1.8.0, Tilemap.
- Project root on the user's machine: `X:\GameDev\Signal\UnityProject\Signal`.
- Read `docs/PROGRESS.md`, `docs/VISION.md`, `docs/OPEN_QUESTIONS.md`, `docs/ARCHITECTURE.md`, and `docs/DATA_MODEL.md` before substantial work. Read relevant task cards and folder-level AGENTS.md files when present.

## Scope and authority
The user has authorized the agent to act as a technical lead and gameplay-systems partner, not merely as a task-card executor.
- Work may span multiple task cards and files when that is the coherent way to complete an initiative.
- Work outside an existing task card is allowed. Track it explicitly in `docs/PROGRESS.md`; create or update a task/initiative note under `docs/tasks/` when the work is substantial.
- Keep changes within the agreed initiative. Do not make unrelated opportunistic changes.
- Technical implementation and architecture decisions may be made autonomously when they preserve the approved direction and project constraints.
- Do not claim work is verified unless the relevant verification was actually performed.

## Game-design approval gate
The game is a hybrid of a deep management/simulation RPG and a narrative-driven RPG.
- Propose new mechanics freely. The user explicitly wants new mechanics to be developed.
- Before implementing a new gameplay mechanic or making a consequential game-design decision, explain its purpose, player-facing rules, interactions, progression/balance implications, technical dependencies, and meaningful alternatives.
- Wait for the user's explicit confirmation before implementing that mechanic or treating the design as approved.
- Record approved decisions in the appropriate design documentation and unresolved decisions in `docs/OPEN_QUESTIONS.md`.
- Existing approved decisions in `docs/OPEN_QUESTIONS.md` and decision records remain binding until the user changes them.
- Do not confuse a technical implementation choice with a gameplay-design decision.

## Architecture and implementation rules

**Active architecture:** Signal.Core + Signal.Game are the single active gameplay architecture. Treat Assets/_Project/_Prototype/Scripts and Main.unity as legacy reference only: do not add new gameplay there, do not run both architectures in one active scene, and migrate only specific content needed by the current slice. See docs/ARCHITECTURE.md.

1. **Foundations before dependent features.** Core logic (plain C#, tested) comes before UI, and UI comes before world content. Do not build a feature whose required data model does not exist.
2. **Keep layers clean.** `Signal.Core` has no `UnityEngine` references. `Signal.Game` contains Unity-facing components and references Core. Content definitions belong in ScriptableObjects.
3. **No string item or recipe IDs in gameplay code.** Reference definitions and stable asset IDs as specified in `docs/DATA_MODEL.md`.
4. **No `FindObjectOfType`, `GameObject.Find`, or new singletons.** Pass dependencies through serialized fields or constructors.
5. **Preserve Unity asset identity.** Do not casually move, rename, or delete Unity assets; preserve their `.meta` GUIDs and scene references.
6. **No new Unity packages, Asset Store packs, external resource packs, or third-party code without asking.** Temporary third-party art packs are allowed only as test content under decision 0004; every pack must be listed with its licence in `docs/ART_TODO.md`, and no pack is final art.
7. **Comments explain why, not what.** Keep public APIs focused and avoid unnecessary abstractions.
8. **Avoid unrelated refactors.** Refactor when required by the initiative or when a concrete defect/risk justifies it; document material scope changes.

## Verification rules
- Every code change must be checked for compilation and relevant tests when the necessary environment is available.
- Run EditMode tests for logic added or changed when Unity is available.
- Do NOT run `dotnet`, `msbuild`, or other standalone build commands for this Unity project. Unity compiles the project.
- Do not claim a file exists unless a repository listing or command confirms it.
- Separate verification states clearly: code inspected, automated tests run, Unity compilation checked, and in-Editor gameplay manually verified are different claims.
- If Unity is unavailable, state exactly what remains for the user to verify in the Editor.
- Do not modify Project Settings or package versions merely to work around errors.

## Documentation is part of the implementation
Keep these documents consistent:
- `docs/VISION.md`: approved high-level identity and pillars.
- `docs/DESIGN.md`: detailed approved gameplay mechanics and rules (create when the first substantial mechanics are approved).
- `docs/ROADMAP.md`: milestones, dependencies, and exit criteria.
- `docs/PROGRESS.md`: actual repository state, completed work, verification status, blockers, and next recommended action.
- `docs/OPEN_QUESTIONS.md`: unresolved design/technical decisions with options and a recommendation.
- `docs/ARCHITECTURE.md`: technical boundaries and communication patterns.
- `docs/DATA_MODEL.md`: persistent entities, definitions, and runtime state.
- `docs/tasks/`: task cards and substantial ad-hoc initiatives.
- `docs/ART_TODO.md`: placeholders and outstanding art.

For every substantial initiative, record:
- scope and reason;
- files changed;
- implementation status;
- tests and verification actually performed;
- known limitations and follow-up work;
- any user decisions still required.

Do not rewrite progress history to make work appear more complete. Preserve previous results with their provenance and dates when known.

## End-of-session report
Use this format, adapting TASK to an initiative when work is not tied to a task card:
```
TASK / INITIATIVE: <id and title>
STATUS: done | partial | blocked
CHANGED: <files created or modified>
VERIFICATION: <commands/tests actually run and results>
VERIFY IN UNITY: <exact user steps, or not required>
OPEN QUESTIONS: <list, or none>
PROGRESS.md: updated | not updated (why)
NEXT: <single recommended next action>
```

## Session end
1. Update `docs/PROGRESS.md` for every substantial initiative.
2. Record approved mechanics and unresolved questions in their designated documents.
3. Report verification honestly and list any Unity Editor steps still required.
4. Do not silently start a separate, unrelated initiative.
