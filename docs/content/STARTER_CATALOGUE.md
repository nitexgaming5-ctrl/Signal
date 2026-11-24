# First Workshop — starter catalogue and usage

**Scope:** B-01 to B-03 only  
**Status:** authored content committed and wired into `HideoutSlice.unity` on 2026-11-23  
**Authority:** the ScriptableObject assets are the runtime source of truth. This page is the human-readable index.

All values below are provisional First Workshop content authored under the user's delegated content authority. Item `baseValue` is an inspection/reference value, not a shop price. No buy/sell offer is implied.

## Starter item catalogue

Assets are in `Assets/_Project/Content/WorkshopItems/`. New items use default instance state: Standard quality, Good condition, Genuine authenticity and Legitimate ownership.

| Item | Role | Grid | Max stack | Starting quantity | Base reference value |
|---|---|---:|---:|---:|---:|
| Fabric Roll | Material | 1×2 | 3 | 3 | 5 |
| Thread Spool | Material | 1×1 | 2 | 2 | 3 |
| Adhesive Tube | Material | 1×1 | 2 | 2 | 3 |
| Fastener Pack | Material | 1×1 | 1 | 1 | 3 |
| Basic Patch Kit | Product | 1×1 | 1 | 0 | 18 |
| Reinforced Patch Kit | Product | 1×1 | 1 | 0 | 30 |

Existing ITEM_A, ITEM_B and ITEM_C remain unchanged as prototype/crime/scavenging fixtures. Bread, Dirty Cash and Wrapped Parcel retain their existing definitions and roles.

The current GameSession `items` array is also used by the existing starter-item grant, which grants each assigned definition at its max-stack quantity. The four materials are included there intentionally to provide a small deterministic workshop starter kit. This is not a general supply system.

## Recipes

### Basic Patch Kit

Asset: `Recipe_Workshop_BasicPatch.asset`

| Input | Quantity |
|---|---:|
| Fabric Roll | 1 |
| Thread Spool | 1 |
| Adhesive Tube | 1 |

Output: **Basic Patch Kit ×1**.

The player starts with enough materials to make two separate kits. Basic Patch Kit max stack is 1, so the two outputs remain separate stack identities: one can be delivered to Milo and one retained.

### Reinforced Patch Kit

Asset: `Recipe_Workshop_ReinforcedPatch.asset`

<badge color="warning">Locked until upgrade</badge>

| Input | Quantity |
|---|---:|
| Basic Patch Kit | 1 |
| Fabric Roll | 1 |
| Fastener Pack | 1 |

Output: **Reinforced Patch Kit ×1**.

The starter kit contains the remaining Fabric Roll and Fastener Pack needed after two Basic Patch Kit productions.

Both recipes are immediate. No timed crafting, queues, quality rolls, tool durability or by-products are authored.

## Customer contract

Asset: `Assets/_Project/Content/Contracts/Contract_Milo_BasicPatchKit.asset`.

- Customer/issuer: **Milo**, the existing neighbourhood baker.
- Request: one Basic Patch Kit to keep at the bakery and use to mend his work apron.
- Reward: **40 clean CARD**, debited from Milo's resident account.
- Milo's authored starting clean balance: **150 CARD**.
- Eligibility: `LegitimateOnly`.
- Delivery: the exact whole stack is transferred to Milo's physical receiving inventory.
- Only one active contract is allowed. This is one authored offer, not a procedural contract board.

## Practical upgrade

Asset: `Assets/_Project/Content/WorkshopUpgrades/Upgrade_CompactSewingMachine.asset`.

- Upgrade: **Compact Sewing Machine**.
- Supplier/payee: **Rhea**, the existing neighbour.
- Cost: **90 clean CARD**, paid to Rhea's resident account.
- Rhea's authored starting clean balance: **250 CARD**.
- Prerequisite: complete Milo's first contract.
- Result: a visible placeholder sewing-machine prop is installed at the workbench and the Reinforced Patch Kit recipe is unlocked.
- The prop is a replaceable composition using the existing PlayerSquare sprite. Final art remains on ART_TODO.

The player starts with **70 CARD**. Completing Milo's request raises the player's balance to 110 CARD, allowing the 90 CARD upgrade and leaving 20 CARD. Rent is set to 0 for this prototype slice.

## Workbench usage — playtest script

1. Start a new game in HideoutSlice. Open the inventory and confirm the four material stacks are present.
2. Walk to the physical workbench and press **E**. The close-up WorkbenchWorkspace opens.
3. Select Basic Patch Kit. Confirm the input/output details and that the recipe is available.
4. Press Produce twice. Each action consumes one Fabric Roll, one Thread Spool and one Adhesive Tube, creating one separate Basic Patch Kit stack.
5. Close the workspace with **Esc**. Travel to CornerShop while Milo is at work (his authored work window begins at 06:30; the world starts at 08:00).
6. Approach Milo and press **E**. Review and accept “A Repair Kit for the Bakery”.
7. Select one Basic Patch Kit stack and fulfil the contract. Confirm 40 CARD is credited, Milo's account is debited, the delivered kit is in his receiving inventory and the other kit remains with the player.
8. When Rhea is socializing on the Street, approach her and press **E**. Choose one of the three responses to her cautious, second-hand tip about Milo. This thread is independent: it can be completed before or after Milo's request and does not require buying the machine. Confirm the discovery is recorded as a **Rumour** about Milo with source Rhea and confidence 0.4; it must not be presented as verified fact. If Milo is the nearest resident, move closer to Rhea before interacting.
9. After Milo's contract is complete, find Rhea during her CornerShop work window (begins at **09:00**). Approach until the prompt reads **E  Talk to Rhea**, press **E**, then choose **Take a look**. The existing supplier panel opens; the upgrade still requires the completed contract and costs **90 CARD**. Select **BUY & INSTALL**. Confirm 90 CARD is paid to Rhea, the installed prop appears at the hideout workbench and Reinforced Patch Kit becomes available.
10. Speak to Rhea again after the machine is acquired. Her mending-advice thread should become available, with no repair contract or repair mechanic created yet.
11. Return to the hideout workbench and produce the Reinforced Patch Kit using the retained Basic Patch Kit, remaining Fabric Roll and Fastener Pack.
12. Save with **F5**, reload with **F9**, and confirm balances, retained/output items, completed contract, completed agenda stages, acquired upgrade, installed prop and recipe unlock remain coherent.

This script is the acceptance plan for Editor verification. The user has confirmed the workbench, selling and customer-request portions in Play Mode; Rhea's agenda conversations, supplier purchase/installation, mending follow-up, second recipe and save/reload path remain unverified until tested.

## Scene wiring

`HideoutSlice.unity` references:
- Four starter material ItemDefinitions in GameSession's existing `items` array.
- Authored Milo and Rhea NpcCharacterDefinitions in `npcDefinitions`.
- Basic Patch Kit in `startingWorkshopRecipes`.
- Milo's request in `firstCustomerContract`.
- Compact Sewing Machine in `firstWorkshopUpgrade`.

The second recipe is registered from the upgrade definition. The contract's required output and all recipe inputs/outputs are added to the runtime item catalog by GameSession.
