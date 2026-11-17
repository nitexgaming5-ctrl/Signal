# Design — NPC life simulation v1

## Goal

NPCs should feel like people with lives that continue when the player is not interacting with them. Their public occupation should not reveal everything about them. The simulation must be inexpensive and deterministic on ordinary hardware.

## Approved technical approach

Use authored character content plus a small deterministic utility brain. Do not run a language model or remote AI service for each NPC.

- Core owns decision logic and need values as plain C#.
- Game owns the visible character, movement, proximity interaction and dialogue presentation.
- The brain evaluates possible activities every 15 in-game minutes.
- Need values drift once per elapsed in-game minute.
- The first prototype tracks only energy and social needs.
- No random decisions, background worker, networking or per-frame decision scoring.
- A character's richer simulation is justified by their importance and the current playable scenario; not every citizen needs equal detail.

## First character: Milo, neighbourhood baker

Milo is a working baker in the corner shop. The player sees him at work, taking a break, spending time with neighbours after closing, and heading home.

Activities:
- **Work:** preferred between 06:30 and 18:00.
- **Rest:** becomes more attractive as energy falls, with an additional midday preference.
- **Socialize:** becomes more attractive as social need falls, especially between 18:00 and 21:00.
- **Home:** strongly preferred from 21:00 to 06:30.

The utility brain has a small inertia bonus so it does not switch activities whenever two scores are nearly equal. The exact scores and need drift are implementation tuning, not a promise of final balance.

Milo's dialogue changes with his current activity. His evening line hints that the bakery is not his whole life, without exposing a complete secret or forcing the player into a criminal path.

## First prototype boundaries

Included:
- Deterministic activity selection from world time and two simple needs.
- Activity-specific destination points in the shared street.
- Movement while the street is active; when the player returns after time has passed elsewhere, Milo resumes at the location appropriate to the current activity.
- Proximity prompt and activity-aware conversation.
- Core tests for routine choices, determinism and clock rewind; a Game integration test for talking and observing an activity change.

Not yet included:
- Persistent relationship values or memory.
- Secret discovery flags, rumours or evidence.
- A concrete side business, request, delivery, trade, reward or failure outcome.
- NPC schedules for the entire population.
- Simulation of distant districts or a crowd manager.

The next playable slice should connect Milo's ordinary public role to a discoverable private opportunity and a meaningful player outcome, while keeping both in the same city and social network. Detailed consequences should be chosen to support that scenario, not added as generic subsystems.

## Verification

The implementation and tests are committed to the shared working branch. Unity compilation, EditMode tests and Play Mode behaviour have not yet been verified in the Editor.
