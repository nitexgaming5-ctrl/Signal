# Roadmap — foundations first

Rule: a milestone is done only when its exit check passes in Unity. Do not start the next one early.
Each milestone is split into task cards in `docs/tasks/`. One card per agent session.

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
