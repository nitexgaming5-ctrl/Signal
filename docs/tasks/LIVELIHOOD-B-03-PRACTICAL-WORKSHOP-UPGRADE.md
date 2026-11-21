# LIVELIHOOD-B-03 — Practical hideout workshop upgrade

**Status:** specified; not started  
**Parent:** LIVELIHOOD-B1-B3-FIRST-WORKSHOP.md  
**Depends on:** B-01 production and B-02 customer contract loop.

## Player outcome

The player improves the hideout workshop through one practical upgrade. The upgrade visibly changes or improves the workstation and unlocks one additional authored recipe/capability.

## Approved rule

The first practical hideout upgrade unlocks another production recipe/capability. It is not merely cosmetic or an unexplained percentage bonus.

## Implementation tasks

### B03.1 — Upgrade definition and runtime state
- Author one stable upgrade definition for the player workshop.
- Define its prerequisite, one-time acquisition rule and the recipe/capability it unlocks.
- Keep definition data separate from persisted acquired/unlocked state.
- Avoid a generic upgrade tree until a second concrete use case proves the need.

### B03.2 — Acquisition and transaction
- Add a deliberate player-facing upgrade interaction in the hideout.
- Use an explicitly approved acquisition condition and cost; do not invent a price or payment source.
- If clean CARD is chosen, use the existing account transaction path and validate funds atomically.
- Repeated purchase must not charge twice or duplicate unlocks.

### B03.3 — World and UI feedback
- Make the upgrade visible in the hideout through the existing scene/content approach.
- Explain what changed and which recipe/capability is now available.
- Locked recipe remains visible or discoverable as appropriate, but cannot be crafted before unlock.
- UI cannot authoritatively grant unlocks.

### B03.4 — Persistence and tests
- Persist acquired upgrade and unlocked recipe identity.
- Test prerequisite failure, insufficient funds if applicable, duplicate acquisition, correct unlock and save/load.
- Keep the authored NPC-owned workshop path compatible; this first upgrade belongs to the player hideout.

## Acceptance criteria

1. Player can acquire the one authored practical workshop upgrade.
2. Upgrade has a visible, understandable practical result.
3. Exactly the intended second recipe/capability becomes available.
4. Recipe cannot be used before upgrade.
5. Repeated interaction cannot duplicate payment or unlock.
6. Upgrade and unlock survive save/load.
7. Existing production and contract behavior remains intact.

## Not included

Multiple upgrade tiers, branching upgrade trees, furniture decoration systems, room expansion, passive bonuses, generic progression menus or NPC business upgrades.
