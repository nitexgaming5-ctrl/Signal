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
Continue P-03 Equipment, Wearables & Portable Backpack. The seven-slot layout is approved: Head, Torso, Legs, Feet and three generic accessory slots. The backpack is a physical accessory/container whose contents remain attached to its stable item identity when equipped, dropped or recovered. The first Core foundation is committed; Unity verification and Game/save/UI/world integration remain pending. See docs/tasks/PLAYER-P-03-EQUIPMENT-BACKPACK.md. Keep B-07 and W-01 as separate roadmap tracks.


## Latest implementation — P-01/P-02 first slice (2026-11-25)

Committed but not yet Unity-verified:
- P-01: modular five-part Body_A player visual with Idle/Walk and Down/Side/Top direction handling. RoomBootstrap retains physics/movement and falls back to the teal placeholder until assets are imported.
- To import the supplied archive in Unity: use **Signal → Import World Build Character Pack...**, select `World Build - Character Pack - 0.2(1).zip`, then enter Play Mode. This imports PNG sheets into Resources and preserves the bundled terms. The pack provides a single modular base body, not a full clothing/armor catalogue.
- P-02: Armor/Heat Protection base attributes, pure equipment modifier seam, point-based skill ranks, one-time milestone ledger, free respecialization, SaveData mapping, provisional catalog and F2 debug panel.
- Current prototype catalog: 8 starting points, rank cap 5, 1 point per rank. Candidate skills remain provisional. Activity-specific effects are not yet wired.
- Eleven focused tests were added. Previous user-verified baseline was 456/456 before this change; expected current total is 467. Unity compile, full EditMode suite and Play Mode must be run before calling this verified.
- F2 opens Player Development when other major panels are closed. F1 remains the existing debug menu.


## P-03 first implementation slice (2026-11-25)

- Core seven-slot PlayerEquipmentLoadout is committed with exact item-instance transfer, slot uniqueness and effective Armor/Heat Protection aggregation.
- PortableStorageBook associates independent GridInventory contents with a stable backpack item instance ID; non-empty storage cannot be discarded.
- Nine focused EditMode tests were added. User's 467/467 verification predates these tests; expected total is 476 pending Unity execution.
- Game item metadata, GameSession composition, SaveData persistence, equipment/backpack UI, wearable visuals and physical world drop/pickup are not implemented yet. Do not describe P-03 as complete until these are integrated and tested.
