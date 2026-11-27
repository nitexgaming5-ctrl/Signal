# P-02 — Player Attributes & Skills

**Status:** Original progression foundation user-verified at 467/467; Health extension implemented in P-04a source, Unity verification pending.
**Track:** Player Development
**Dependency:** P-01 establishes the persistent player character profile and composition boundary. P-02 may be developed against the profile contract, but should not duplicate appearance/rendering responsibilities.

## Player outcome

The player can distribute a fixed starting skill-point budget among available skills, earn additional points from important authored gameplay milestones, spend points manually, and freely redistribute all allocated skill points at any time. The character has three attributes, with **Health as the primary attribute**, alongside Armor and Heat Protection.

## Confirmed player decisions

- Character creation is customization of the existing player, not a separate new-game class/character flow.
- Starting skills are freely chosen from a fixed point budget.
- Skill progression uses points the player allocates.
- New skill points are awarded by important milestones, not by repetitive action XP.
- Respecialization is free and available at any time.
- Three attributes: **Health** (primary), **Armor** and **Heat Protection**. This supersedes the earlier two-attribute decision.
- Armor is the physical-protection attribute.
- Heat Protection retains that exact name and is used for cold-weather/environmental exposure protection. No hot-weather/overheating gameplay is planned.
- Clothing and armor are intended to have both visual and gameplay effects; authoritative equipment implementation belongs to P-03. P-02 must define a clean modifier interface without inventing equipment slots or duplicating inventory state.

## Binding rules

### Attributes

1. The character has exactly three attributes: **Health**, **Armor** and **Heat Protection**. Health is the primary attribute. Do not add other generic RPG attributes without a separate decision.
2. Health is the primary **maximum-health attribute**. The existing `PlayerStats.Health` remains the current-health resource; its maximum is supplied by effective Health. The current-health resource is saved separately. This does not add or change damage/healing actions or formulas.
3. Armor represents protection against physical damage/threats when a supported damage source exists.
4. Heat Protection represents protection against cold exposure, including cold associated with low temperatures, wind, rain and snow. The name remains Heat Protection by explicit user decision.
5. No hot weather, heatwaves, overheating or heat exposure damage is in scope.
6. Store base attribute values separately from effective values. Effective values are calculated from base values plus validated equipment/skill modifiers through one authoritative calculation path.
7. The current prototype base maximum Health is 100 to match the existing player-health baseline. It is a replaceable starting value, not final balance. Armor/Heat Protection starting values and final attribute tuning remain open. Physical-damage formula is not in scope.
8. P-02 establishes the attribute data and calculation contract. It must not invent a combat system or apply speculative damage to the current game.

### Skills

1. Skills are learned capabilities, distinct from the two attributes and from equipment modifiers.
2. The player receives a fixed starting skill-point budget and distributes it freely.
3. Skill ranks are improved only by spending points; actions do not silently increase ranks.
4. Important authored milestones award additional skill points. Repeating/reloading the same milestone cannot award it twice.
5. A milestone award is an explicit domain transaction with a stable milestone ID, point amount, source/context and awarded state.
6. The player may redistribute all invested skill points freely at any time. Respecialization has no CARD cost, item cost, cooldown, penalty or limited token.
7. Redistribution preserves the total number of points: allocated + unspent equals the total starting and milestone points earned.
8. Skill definitions and IDs are authored content; runtime skill ranks and point balances are Core state.
9. Do not introduce classes, backgrounds, locked professions or mutually exclusive skill paths.

## Initial skill catalogue — still provisional

The following are candidate skills from the design discussion, not yet individually approved as the final catalogue:
- Repair
- Production
- Trading
- Scavenging
- Social

Keep the catalogue data-driven so the user can rename, remove, add or rebalance skills without rewriting the Core progression model. Do not hardcode skill-specific behavior into the generic point-allocation service. A skill only affects an activity when that activity explicitly consumes its value through a defined integration contract.

The exact starting budget, skill rank cap, point cost per rank, milestone list/rewards and individual skill gameplay formulas remain open tuning/content decisions. Do not invent final values during implementation without documenting them as temporary, replaceable test fixtures or obtaining direction where balance materially changes the experience.

## Proposed architecture

### Core (plain C#)
- `PlayerAttributeSet`: validated base Health, Armor and Heat Protection values.
- `PlayerSkillBook`: stable skill-ID → rank state, validation and allocation.
- `PlayerSkillProgression`: starting budget, total earned points, unspent points and point-spend operations.
- `PlayerMilestoneLedger`: stable milestone IDs and one-time point-award records.
- `CharacterAttributeCalculator`: derives effective Health, Armor and Heat Protection from base values and supplied equipment/skill modifiers.
- Optional immutable result/value objects for allocation previews and milestone award results.

### Game/content
- `SkillDefinition` ScriptableObject: stable ID, display name, description, icon and authored progression metadata.
- `PlayerDevelopmentPanel`: presentation for attributes, skills, unspent points and redistribution.
- `MilestoneDefinition` ScriptableObject or equivalent authored definition: stable ID, point reward and explicit trigger/source context. The actual milestone trigger must come from a successful existing gameplay event, not UI visibility or an attempted action.
- `GameSession`: composes the Core books/services, registers authored definitions and saves/restores their state.

These names are proposed; inspect for reusable current types before creating new classes.

## Milestone award boundaries

- Award only after the underlying activity has successfully completed (for example, after an accepted customer service transaction is fully committed).
- Do not award points merely for opening a panel, accepting an order, attempting a repair or repeating an already completed interaction.
- The ledger is the replay-protection authority. The source activity's transaction must succeed before the award is committed.
- Milestone awards must be atomic with their own progression state and safe against duplicate callbacks/reloads. If an activity and milestone are committed separately, use a stable event ID/idempotent award API and ensure a failed award can be retried without duplicating the activity reward.
- No passive XP, per-action grind, time-played rewards or random skill-point drops.

## Equipment integration boundary

P-02 owns base attribute state and a pure calculation contract. P-03 owns the actual equipment loadout and item-to-modifier mapping. W-01 consumes effective Heat Protection when calculating cold exposure. P-02 must not make Core depend on Game ScriptableObjects or Unity objects.

## Save/load

Persist:
- base Health, Armor and Heat Protection;
- skill ranks by stable skill ID;
- starting/total earned/unspent point state (or the minimal canonical values from which these are safely derived);
- stable milestone award IDs and their point amounts/source identity.

Do not serialize ScriptableObject references. Unknown skill/milestone IDs must be skipped or safely retained according to a documented policy without corrupting unrelated state. Add current-build SaveMapper round-trip tests. Build 0.01 has no save-schema version or migration chain; older experimental saves may need to be discarded.

## Acceptance criteria

- Exactly Health, Armor and Heat Protection exist as player attributes; Health is primary.
- The fixed starting skill budget can be freely distributed among valid authored skills.
- Invalid IDs, negative ranks, negative point amounts and overspending are rejected.
- A milestone can award points once; duplicate event delivery/reload cannot award twice.
- Spending points updates rank and unspent balance consistently.
- Free redistribution returns invested points and allows a new allocation without changing total earned points.
- Save/load preserves attributes, ranks, point balances and milestone history.
- Effective attribute calculation is deterministic and combines base values with supplied modifiers without mutating base values.
- Existing B-01–B-06 gameplay and transaction ledgers remain unaffected.
- Focused Core tests cover all invariants; full EditMode suite and Play Mode UI flow pass.

## Explicit exclusions

No character classes/backgrounds, automatic use-based skill growth, generic attributes beyond Health, Armor and Heat Protection, final balance values, full equipment loadout, combat/damage implementation, hot-weather systems, cold exposure simulation, weather simulation or NPC skill systems.


## Implementation slice committed (2026-11-25)

- Added Core `PlayerAttributeSet` with exactly Armor and Heat Protection, plus a pure `CharacterAttributeCalculator` seam for future equipment modifiers. This is the pre-Health implementation baseline and must be extended: Health is now the primary third attribute.
- Added `PlayerSkillBook`, `PlayerSkillProgression`, `PlayerMilestoneLedger` and `PlayerDevelopmentState`. Point spending validates skill IDs, budget and maximum rank. Milestone awards are stable-ID, one-time and reject invalid rewards. Free respecialization returns every allocated point.
- Added save capture/restore using primitive values and stable IDs only. Unknown skills and invalid/duplicate milestone awards are safely ignored; over-budget malformed ranks are restored deterministically within the available budget.
- Added an authored `PlayerDevelopmentCatalog` asset. Its current prototype values are **8 starting points, rank cap 5, and 1 point per rank**. Repair, Production, Trading, Scavenging and Social are explicitly flagged provisional and can be edited in the catalog.
- Added the in-game F2 Player Development debug panel for viewing the two attributes, allocating points and freely resetting allocations. GameSession exposes an idempotent milestone-award entry point for future successful gameplay events.
- Skill ranks are stored and allocated, but activity-specific gameplay effects are intentionally not invented or wired yet. The final skill catalogue and what each skill modifies still need design/tuning. Equipment modifiers are P-03's responsibility.

**Not yet verified:** Unity compilation, full EditMode suite and F2/F5/F9 Play Mode acceptance. The last user-confirmed baseline was 456/456 before these 11 new tests; expected suite count is now 467, pending Unity execution.


## Design update — Health promoted to primary attribute (2026-11-27)

The user approved replacing the previous two-attribute design with three attributes: **Health (primary), Armor and Heat Protection**. The existing Core implementation/save mapping contains only Armor and Heat Protection and must be extended. Health's exact current/max-health model, starting value, cap, damage/healing rules and formulas remain open; do not invent a combat or damage system. For initial skill-pipeline testing, one skill will increase Health and another will increase Heat Protection. The skill identities and temporary rank formulas remain to be selected/documented before implementation.


## P-04a Health extension — source implementation checkpoint (2026-11-27)

- `PlayerAttributeSet` now stores positive base maximum Health alongside Armor and Heat Protection. `EffectiveCharacterAttributes` and the pure calculator expose all three effective values.
- The existing current-health resource remains `PlayerStats.Health`; its effective maximum is updated from calculated Health. Damage and healing rules are unchanged. The HUD shows current/maximum Health.
- Equipped Armor/Heat Protection modifiers and authored skill modifiers are combined through the Core attribute calculation/loadout path. The Core layer remains independent of Unity content.
- The catalog now contains two explicitly temporary test skills: `health_test` (Vitality test: +10 maximum Health per rank) and `heat_protection_test` (Cold Readiness test: +1 Heat Protection per rank). These are pipeline fixtures only, not final skill identities or balance.
- Base Health, Armor, Heat Protection, skill ranks and milestone awards are captured/restored by the existing development save mapper. GameSession reapplies saved current HP after restoring the effective maximum so a higher-Health save is not clipped by the initial default cap.
- Two Core regression tests were added for temporary attribute modifiers and dynamic maximum Health. The previous user-verified suite was 555/555; expected current suite is 557, pending Unity compilation and full EditMode execution. No new Unity verification is claimed.
