# Signal — current agent handoff

**Last reconciled:** 2026-11-27  
**Repository:** `nitexgaming5-ctrl/Signal`  
**Working branch:** `chore/collaboration-workflow-and-audit`  
**Do not modify:** `main`

Signal is a Unity 2D top-down urban RPG. Unity 6000.6.9f1; active scene `Assets/_Project/Scenes/HideoutSlice.unity`; architecture is `Signal.Core` (plain C# domain) + `Signal.Game` (Unity integration/UI/content).

## Read first

1. `AGENTS.md` — authority and workflow.
2. `docs/INDEX.md` — documentation map.
3. `docs/PROGRESS.md` — concise implementation and verification truth.
4. `docs/ROADMAP.md` — active sequence and next proposal.
5. The relevant task card and specialist references.

Historical chronology belongs in `docs/progress/LOG-2026-11.md`; do not read it wholesale unless needed.

## Direction and workflow

The user selected **Direction B — meaningful everyday livelihood**: repair, production, customers, services, relationships and a practical hideout. Character development was reprioritised ahead of neighbourhood expansion. Keep one active initiative at a time and avoid isolated systems.

**Gameplay mechanics must be explained and explicitly approved before implementation.** User expects substantial coherent work after a green light, accurate documentation, and no false test claims. Keep `docs/PROGRESS.md`, `docs/ROADMAP.md`, `docs/PROJECT_HANDOFF.md`, the active task card and monthly log reconciled.

## Verification snapshot

- **Latest full EditMode suite: 563/563 passed**, user-confirmed 2026-11-27 after P-04b usability follow-up.
- P-04a: 557/557 confirmed.
- P-04b initial assignment foundation: 562/562 confirmed; usability follow-up: 563/563 confirmed.
- P-03.7: 555/555 plus Unity compile and Play Mode acceptance confirmed; closed.
- B-06: 456/456 plus successful Play Mode repair/return/payment loop confirmed.
- Unity Editor is authoritative for compile, EditMode and Play Mode. Do not infer Play Mode acceptance for P-04b solely from the test count.

## Current implementation

### Player attributes / P-04a

Exactly three attributes: Health (primary/max HP), Armor and Heat Protection. Current HP remains separate in PlayerStats. F2 opens the Character panel; HUD shows current/max HP. Effective attributes combine equipment and skill modifiers. Temporary test fixtures only: `health_test` (+10 max Health/rank) and `heat_protection_test` (+1 Heat Protection/rank). Actual Repair/Production/Trading/Scavenging/Social effects and comprehensive milestone trigger wiring remain future work.

### Equipment / P-03

Seven slots: Head, Torso, Legs, Feet and Accessory 1–3; backpack uses an accessory slot. Backpack owns an 8×6 grid by stable item ID; B drops it and E recovers/re-equips the same item. Three persistent 1×1 pockets exist, with P3 hidden from ordinary-search enumeration. Stack split/merge and pocket-aware service integrations are in place. P-03.7 is closed at 555/555 with Play Mode acceptance.

A later cleanup removed automatic fresh-session debug backpack/cargo; F1 spawner remains. That cleanup still needs a separate fresh Unity verification (empty new game and F1 spawner). Do not reopen P-03.7. Wearable presentation is paused.

### Quickbar / P-04b — verified

- Six general-purpose slots, stable item-instance references only; never duplicate/copy physical items.
- One physical item can appear in at most one slot.
- Assignable from accessible backpack grid or any personal pocket; equipped clothing/accessories excluded.
- Tab inventory: RMB Inspect / Equip to Quick Slot 1–6; drag assignment, slot swap and drag-to-unassign.
- RMB reassignment moves the existing reference to the new slot.
- Semi-transparent pointer-following drag ghost.
- Always-visible bottom-centre six-slot HUD bar outside Tab, while the inventory row remains.
- Dropped-backpack references persist but are unavailable until the same backpack is recovered; pocket entries remain available.
- Save/load mapping persists and reconciles assignments.
- User confirmed **563/563** after these changes.

### P-04c active hand and controls — implementation committed, verification pending

- Core `PlayerQuickbar` owns selected-slot and holstered state. The held item is derived from the selected slot's existing GUID; inventory remains ownership authority.
- Mouse wheel up/down cycles next/previous through all six positions, including empty slots, with wraparound.
- Number keys 1–6 directly select slots 1–6. H holsters without clearing the assignment. Selecting an empty slot means empty hand.
- All three controls are ignored while any supported menu/panel is open.
- HUD remains display-only. It highlights the selected slot differently when active versus holstered; no HUD click interaction.
- If a held item in the backpack grid becomes inaccessible because the backpack is dropped, the active hand holsters but the slot assignment remains. Pocket items remain accessible.
- F9/load resets the active hand to no selected slot and holstered; six assignments are restored as before.
- No consumable use, tool action, weapon attack, combat, targeting, durability, or new art is included.
- Five Core regression tests were added. Last user-confirmed baseline is 563/563; the new tests and full suite still require Unity verification.
- Source commits: `c17a9f7e27d06caf6066d5fd010594d51d33fd85`, `3f8157afb5ca80e744ed83d9ea25922b33f99e50`, `e0ea41bdf2b11a30b4a99941f7153979fd04223e`.
- Next action: run Unity compile, all EditMode tests and the Play Mode acceptance checklist in the task card; do not mark P-04c complete until those pass.

## Project-wide constraints

- Core must not reference UnityEngine; dependencies flow Game → Core.
- Reuse canonical inventory, ownership, custody, transaction, account and save authorities.
- Preserve stable ScriptableObject IDs, item GUIDs and Unity .meta GUIDs.
- No new packages/dependencies without approval.
- Do not author new story cast/backstories until the main storyline/cast are designed.
- Art pause remains active: no new sprites, wearable overlays, wardrobe presentation or character-art exploration.
- Do not run `dotnet` or `msbuild` for this Unity project.
- B-07 Neighbourhood Expansion is paused while P-04 is active.
