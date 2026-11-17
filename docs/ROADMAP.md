# Roadmap — foundations first

Rule: milestones are done only when their player-facing exit check passes in Unity. The roadmap is a dependency map, not a command to build isolated systems in sequence. Work is organized around one active vertical-slice initiative at a time; an initiative may span multiple task cards and sessions. See `docs/DEVELOPMENT_WORKFLOW.md`.
Task cards are implementation aids, not session boundaries. Combine cards when that produces a coherent playable outcome.

## M0 — Project foundation
Goal: a clean project the agent can work in safely.
- Git repo, `.gitignore` (Library, Temp, Obj, Logs, UserSettings, SignalSave)
- Folder layout from ARCHITECTURE.md
- Assembly definitions: `Signal.Core` (pure logic, no UnityEngine), `Signal.Core.Tests`, `Signal.Game` (MonoBehaviours)
- One passing EditMode test to prove the test runner works
- Exit check: Console clean; Test Runner shows 1 green test; PROGRESS.md updated

## M1 — Player data (no UI, no world)
Goal: the player has data that can be read, changed, validated and saved.
- `PlayerStats` as plain C#: Money, Health (0–100). Other stats are NOT added until listed in OPEN_QUESTIONS answered.
- Validation rules (no negative money, health clamped)
- Change events
- Exit check: EditMode tests cover every change path; no scene needed

## M2 — Items and grid inventory (logic)
Goal: items with size take space in a grid, like Tarkov.
- `ItemDefinition` ScriptableObject: id, display name, width, height, max stack
- `GridInventory` plain C#: width×height grid, place, remove, move, rotate, stacking, overlap rejection
- Tests for placement, overlap, rotation, stack limits, out-of-bounds
- Exit check: all tests green; 3 placeholder ItemDefinitions exist (names from DESIGN or marked PLACEHOLDER)

## M3 — Inventory UI
Goal: you can see and move items.
- UGUI grid panel, items drawn as colored rectangles with names
- Mouse drag and drop between grid cells; invalid drops snap back
- Open/close with one key (Input System)
- Exit check: play session, move items, closing and reopening keeps positions

## M4 — Basic world (one room)
Goal: the player walks inside a readable room.
- One tilemap room: floor, walls, a door tile (placeholder art)
- Player movement with collision, top-down
- Camera follows player, bounded to room
- Exit check: walk through room, collide with walls, cannot leave

## M5 — Save and load
Goal: money, health, inventory and position survive restart.
- Save DTO separate from runtime classes
- Versioned save file with a `version` field
- Exit check: change state, quit, reopen, state matches

## M6 — Time and schedule
Goal: days pass through the schedule, not a keypress.
- Clock with day/time-of-day
- Sleep action advances the day
- Exit check: sleep advances day; clock continues during play

## M7 — The shop (uses M2 inventory, no hardcoded items)
Goal: a customer order you can fulfil from inventory.
- Order definition, stock check against GridInventory, payment to PlayerStats
- Exit check: order fills, stock changes, money changes, all in tests

## M8 — Chemistry (first recipe)
Goal: the Drug Dealer-style mixing loop, minimal.
- Recipe definition: inputs, output, steps
- Mixing UI: drag ingredients into a flask slot, press mix
- One recipe working end to end. Quality and contamination come in M9.

## M9 — Consequences
Goal: actions have a cost.
- Heat meter, rent due each day, one racket
- Exit check: skipping rent changes state; heat affects one shop outcome

## M10 — Second area (outings)
Goal: leave the room to a street. Only after M1–M9 pass.

## Art track (parallel, optional)
Placeholder art is replaced in a single pass after M4, using Aseprite files imported natively (see ART_AND_UI.md). Never mix art passes into logic tasks.


## Integration rule — no isolated-system milestones
M1–M9 describe foundations and capabilities, but they are not all valuable as standalone deliverables. As soon as the relevant foundations are stable, integrate them into the smallest complete playable loop. The target is a player-facing scenario with a real interaction, meaningful cost/reward, clear feedback, and a consequence. Expand the scenario only after it works in the Unity Editor.

A milestone can be technically implemented but still be marked **not integrated** or **not verified**. Do not call the first vertical slice complete until a player can experience its full loop without debug-only shortcuts.
