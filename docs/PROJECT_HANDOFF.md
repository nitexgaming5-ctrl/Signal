# Signal — project handoff

Signal is a top-down 2D urban RPG built on Unity 6000.6.9f1.

## Current entry points
- `AGENTS.md` — agent authority, architecture safeguards and verification rules.
- `docs/INDEX.md` — full game scope and documentation navigation.
- `docs/PROGRESS.md` — current repository state, validation and next action.
- `docs/ROADMAP.md` — Direction B roadmap plus parallel Player Development and future Weather tracks.
- `docs/ARCHITECTURE.md` and `docs/DATA_MODEL.md` — current architecture and planned domain boundaries.
- `docs/tasks/PLAYER-P-01-CHARACTER-FOUNDATION-APPEARANCE.md` — proposed modular player visual foundation.
- `docs/tasks/PLAYER-P-02-ATTRIBUTES-SKILLS.md` — approved attribute/skill progression direction.
- `docs/tasks/WEATHER-W-01-DYNAMIC-WEATHER-COLD-EXPOSURE-DESIGN.md` — separate future weather/cold-exposure design.
- `docs/tasks/LIVELIHOOD-B-06-CUSTOMER-REPAIR-ORDERS.md` — completed and user-verified B-06 slice.

## Current verified baseline
- Working branch: `chore/collaboration-workflow-and-audit`; keep `main` untouched.
- Active scene: `Assets/_Project/Scenes/HideoutSlice.unity`.
- User confirmed **456/456 EditMode tests passed** on 2026-11-25 and reported the B-06 in-game repair-order loop passed.
- B-06 Customer Repair Orders is complete: entrusted item custody remains distinct from ownership; exact item return and 35 CARD payment are atomic and replay-protected.
- B-04 repair/trading and the First Workshop have also been reported working in Play Mode.
- Unity remains the authority for compile, test and Play Mode verification. Never infer a pass from source edits.

## Current design initiative — Player Development
The user wants character/skills customization intercalated before returning to the B-07 roadmap. This is a parallel track; B-07 remains Neighbourhood Expansion.

Confirmed:
- Customize the existing player rather than introducing a new-game class/background flow.
- Fixed starting skill-point budget, freely distributed.
- Additional points from important milestones.
- Manual point allocation and free redistribution at any time.
- Exactly two attributes: Armor and Heat Protection.
- Heat Protection retains its name and protects against cold-related environmental exposure only.
- Clothing and armor should have both visual and gameplay effects. Full equipment loadout is a later P-03 phase.

P-01 and P-02 specifications are prepared. No implementation has been authorized. The current player is created in `RoomBootstrap.CreatePlayer()`; `PlayerMotor2D` currently owns movement input/velocity; `GameSession` is the composition root. Core remains plain C# and Game owns Unity presentation/content.

Candidate skills (Repair, Production, Trading, Scavenging, Social), starting budget amount, rank caps/costs and milestone rewards remain provisional/open.

## Separate future phase — W-01 Weather
Dynamic seasonal weather is planned: temperature changes, rain, storms, snow and blizzards. Player exposure consequences are cold-focused and gradual. Heat Protection mitigates cold exposure; shelter supports recovery. No hot-weather/overheating gameplay is planned. W-01 is separate from P-01/P-02 and is not authorized for implementation.

## Next action
Review/approve the P-01/P-02 specifications before implementation. Keep W-01 as a separate future design phase. Do not change gameplay code, assets or scenes until the user approves the implementation scope.
