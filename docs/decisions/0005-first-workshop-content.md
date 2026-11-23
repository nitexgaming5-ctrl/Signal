# 0005 — First Workshop starter content

Date: 2026-11-23  
Status: accepted under the user's delegated content-authoring authority for B-01 to B-03.

## Purpose

Make the existing First Workshop code foundation into one small, connected, playable livelihood loop. All content values are replaceable prototype content, not a universal economy or final balance commitment.

## Starter catalogue and production

- Content paths: item assets in `Assets/_Project/Content/WorkshopItems/`; recipe, contract, upgrade and resident definitions live in their matching `Recipes/`, `Contracts/`, `WorkshopUpgrades/` and `NPCs/` folders.
- Materials and authored starting kit: Fabric Roll (1×2, max stack 3, start 3), Thread Spool (1×1, max stack 2, start 2), Adhesive Tube (1×1, max stack 2, start 2) and Fastener Pack (1×1, max stack 1, start 1).
- First product: Basic Patch Kit (1×1, max stack 1).
- Second product: Reinforced Patch Kit (1×1, max stack 1).
- Item base reference values are 5/3/3/3 for the four materials and 18/30 for the two products. These values are not shop prices, buyback offers or an implied economy catalogue.
- First recipe: Fabric Roll ×1 + Thread Spool ×1 + Adhesive Tube ×1 → Basic Patch Kit ×1.
- Second recipe, initially locked: Basic Patch Kit ×1 + Fabric Roll ×1 + Fastener Pack ×1 → Reinforced Patch Kit ×1.
- Both recipes use immediate production and create physical item stacks. Basic Patch Kits have max stack 1 so two individual kits can be made, one handed to a customer and one retained for the upgrade recipe.
- Hideout starter materials provide exactly enough supplies to make two separate Basic Patch Kits and then one Reinforced Patch Kit. Two separate stacks exist because product max stack is 1. The first is handed to Milo, the second is retained for the Reinforced recipe. This avoids a random scavenging roll soft-locking the first contract/upgrade demonstration.

The complete item table, exact recipe inputs/outputs, scene wiring and numbered playtest procedure live in `docs/content/STARTER_CATALOGUE.md`.

## First customer contract

- Customer/issuer: Milo, the existing neighbourhood baker.
- Request: one Basic Patch Kit so he can mend and keep a damaged work apron in the bakery.
- Reward: 40 clean CARD from Milo's existing resident account.
- Goods eligibility: LegitimateOnly.
- Delivery: one whole stack to Milo's physical receiving inventory; it is not silently deleted.
- Contract remains a single authored offer and is not procedural or recurring.

## First practical upgrade

- Upgrade: Compact Sewing Machine (second-hand unit supplied by Rhea, the existing neighbour).
- Acquisition: one-time clean CARD purchase for 90 CARD.
- Prerequisite: completion of Milo's first contract.
- Payee: Rhea's existing resident account.
- Effect: installs a visible replaceable prefab prop and unlocks Reinforced Patch Kit.
- Prototype resident balances: Milo 150 CARD; Rhea 250 CARD. These are authored test-slice balances, not final economy tuning.
- Prototype player starting balance: 70 CARD, so the 40 CARD contract reward makes the 90 CARD upgrade affordable. Rent remains 0 for this slice.

## Content boundaries

- Existing ITEM_A/B/C, Bread, Dirty Cash and Wrapped Parcel definitions retain their IDs and existing roles.
- No new NPC, crime mechanic, quality roll, durability, supplier simulation, procedural demand, dynamic price or business stock system is added.
- The first materials are supplied as a small authored starter kit. Future acquisition/sourcing can be designed when a later loop needs it.
- The sewing-machine prefab is a temporary placeholder and is listed for replacement in ART_TODO.

These decisions apply only to the first B-01 to B-03 content slice and can be replaced by later authored content without changing the underlying system contract.
