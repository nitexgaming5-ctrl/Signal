# REPO-CLEANUP-001 — remove clutter, shrink the docs, no gameplay change

**Approved by the user:** 2026-11-20. Tier 1 only. Tier 2 items require a separate user decision; report them, do not perform them.
**Rules:** no gameplay, save-schema, content or economy change. Do not delete anything not listed. Do not run dotnet/msbuild. Do not claim Unity results.

## Tier 1 — approved cleanup
1. Apply the documentation drop-in: `AGENTS.md`, `docs/PROGRESS.md`, `docs/progress/LOG-2026-11.md`, decisions 0001 and 0004, `ART_AND_UI.md`, `ART_TODO.md`, `DEVELOPMENT_WORKFLOW.md`, `tasks/_TEMPLATE.md`, corrected Slice B card, this card and `SLICE-B-HARDENING-001.md`. The root `PROGRESS.md` pointer stays. Move the previous long `docs/PROGRESS.md` into the log verbatim before appending a dated entry.
2. Shrink `docs/PROJECT_HANDOFF.md` (696 lines): move it verbatim to `docs/progress/ARCHIVE-PROJECT_HANDOFF-2026-11-20.md`; replace it with at most 40 lines containing purpose, links to `AGENTS.md`, `docs/PROGRESS.md`, active task card, and how to open/run tests. Reuse only text already in the old handoff; make no status claims.
3. Stop tracking the local save: `.gitignore` already lists `/SignalSave/`; remove `SignalSave/` from Git without deleting the user's local files.
4. Remove stray metadata files outside `Assets/`: `docs/NPC_FRAMEWORK.md.meta` and `docs/tasks/SCAVENGE-001-PHYSICAL-CONTAINERS.md.meta`.
5. Remove orphan metadata files only after confirming their folders are absent: `Assets/Editor.meta`, `Assets/Tests.meta`, `Assets/_Project/ScriptableObjects.meta`, `Assets/_Project/Scripts.meta`, `Assets/_Project/Audio.meta`, `Assets/_Project/Settings.meta`, `Assets/_Project/Prefabs.meta`.
6. Add `*.slnx` to `.gitignore` and stop tracking generated `Signal.slnx`.
7. Archive finished/superseded task cards under `docs/tasks/archive/`: `M0-01.md`, `M0-01-FIX.md`, `M0-01a-prototype-move.md`, `M0-01b-asmdefs.md`, `M0-01c-first-test.md`, `M1-01.md`, and `ECONOMY-PHASE-3-ILLICIT-EXCHANGE.md`. Extract the 11 old M2 cards from `signal-docs-M2-cards.zip` into the archive and remove the zip. Repair links before moving.

## Tier 2 — do NOT do; report and ask the user
- `Assets/_Project/Content/items/` and `Content/Items/` differ only by case. Merge inside Unity Editor so GUIDs survive.
- `Content/Containers/` has no `.meta`; Unity should generate it when the project is opened.
- `unity-mcp` is both in `Packages/manifest.json` and embedded in `Packages/com.coplaydev.unity-mcp`; embedded copy wins. Also review `com.unity.ai.assistant` / learn framework against AGENTS rule 6.
- `_Prototype/` asmdefs remain `autoReferenced` and compile into the project; `Main.unity` is legacy.
- `ArtSource/` pack zips / Git LFS for art.
- Loose assets at `Content/` root (`NPC_Clean_Dirtycash`, `DirtyCashContactOffer_SliceB`) vs subfolders.
- `GameSession.cs` and `CityBlockBootstrap.cs` are growing by accretion; any behaviour-neutral split needs its own approved task card.

## Acceptance
- Git changes are limited to the listed docs, moves and removals; no `.cs`, `.asset`, `.unity` or save-schema changes as part of cleanup.
- `docs/PROGRESS.md` ≤120 lines; `docs/PROJECT_HANDOFF.md` ≤40 lines; no broken relative links among touched docs.
- Unity opens the project and EditMode count remains unchanged by cleanup; user verifies. Report files changed, not Unity success.
- One commit per numbered step, message prefix `chore(cleanup):`.
