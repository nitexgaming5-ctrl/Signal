# LIVELIHOOD-B-03 — Practical hideout workshop upgrade

**Status:** Core/Game and Compact Sewing Machine upgrade content committed; Rhea-facing supplier offer panel and nearest-NPC interaction routing added; awaiting Unity compilation and Play Mode verification  
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
- Add a deliberate player-facing supplier interaction at Rhea; the player reviews the offer and buys the upgrade through her NPC interaction.
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

## Implementation status — 2026-11-23

- Added a persistent Core WorkshopProgressionBook and one-time acquisition service. A successful clean-CARD purchase debits the player, credits the authored resident payee and records the upgrade ID; failed/repeated acquisition does not charge twice.
- Added a validated WorkshopUpgradeDefinition with an unlocked recipe, authored cost/payee/prerequisite, and visible installed-prop prefab hook.
- Integrated a dedicated supplier-facing offer panel opened by pressing E near Rhea. Purchase validation requires proximity to Rhea, the completed Milo contract and sufficient clean CARD. A successful purchase credits Rhea, installs the prop at the hideout workbench and unlocks the recipe.
- Added Core EditMode tests for contract prerequisite, no-prerequisite mode, duplicate purchase and insufficient funds.
- Authored the Compact Sewing Machine upgrade. It costs 90 clean CARD, is sold by Rhea (resident account starts at 250 CARD), requires Milo's first contract to be completed and unlocks Reinforced Patch Kit.
- The installed prop is a replaceable SpriteRenderer placeholder built from the existing PlayerSquare sprite; final art is tracked in ART_TODO.
- The player starts with 70 CARD; Milo's 40 CARD reward makes the upgrade affordable after fulfilling the request. Rent remains zero for this slice.
- The user reconfirmed 415/415 EditMode tests and confirmed workbench production, selling and Milo's customer request in Play Mode. The first supplier menu was missing because purchase had incorrectly been exposed only inside WorkbenchWorkspace; this has been replaced by the Rhea-facing panel. Unity compilation and the supplier purchase/installation, unlock and save/reload path still require Play Mode verification.

## Acceptance criteria

1. Approaching Rhea displays an `E Workshop offer` prompt and opens the supplier offer panel.
2. The panel shows the upgrade, cost, prerequisite and availability; it remains unavailable until Milo's contract is complete.
3. The player can buy the upgrade only while near Rhea and with sufficient CARD; the payment credits Rhea's resident account exactly once.
4. The purchase installs the visible prop at the hideout workbench and unlocks the intended second recipe.
5. Reopening Rhea's offer after purchase shows the already-purchased/installed state without charging again.
6. Recipe cannot be used before the upgrade is purchased.
7. Repeated interaction cannot duplicate payment or unlock.
8. Upgrade and unlock survive save/load.
9. Existing production and contract behavior remains intact.

## Not included

Multiple upgrade tiers, branching upgrade trees, furniture decoration systems, room expansion, passive bonuses, generic progression menus or NPC business upgrades.


## Supplier interaction correction — 2026-11-23

The user found that no menu opened when attempting to buy the machine from Rhea. Source inspection showed the offer had been incorrectly wired only to the workbench UI, while Rhea remained on generic NPC dialogue. The upgrade acquisition is now routed through a dedicated panel opened by approaching Rhea and pressing E. The workbench remains the production station; it no longer contains the purchase button. Because Milo and Rhea's CornerShop work positions are close together, E is routed to the nearest of the two residents when both are in range. Purchase still uses the existing atomic WorkshopUpgradeService and then installs the prop at the hideout workbench. Added a focused UI callback test; Unity must compile and run it before the change is considered verified.
