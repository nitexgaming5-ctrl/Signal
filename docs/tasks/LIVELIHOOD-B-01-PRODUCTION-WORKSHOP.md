# LIVELIHOOD-B-01 — Production foundation and player workshop

**Status:** specified; not started  
**Parent:** LIVELIHOOD-B1-B3-FIRST-WORKSHOP.md

## Player outcome

The player interacts with the existing hideout workbench, inspects an authored recipe and turns carried materials into a physical output through a safe production interaction.

## Implementation tasks

### B01.1 — Audit and protect existing crafting
- Inspect Crafting, Recipe, Ingredient, GridInventory, snapshots, ItemDefinition resolution, ownership registration and all current callers/tests.
- Record current behavior before changing APIs.
- Keep the existing simple Crafting.TryCraft contract compatible unless a concrete test demonstrates it must change.
- Add focused tests before expanding the transaction boundary.

### B01.2 — Production transaction boundary
- Introduce only the minimum Core service/model required to make production authoritative and atomic.
- Validate recipe inputs and output capacity before mutation.
- Validate player ownership of consumed stacks; do not consume a stack merely because a matching ItemType is present in a container.
- Preserve existing state semantics. If ingredient removal can select the wrong state or stack, add an exact and deterministic selection path rather than weakening item identity.
- Snapshot and roll back inventory, ownership and history on failure.
- Record a production transaction with stable recipe/actor/time identity if a dedicated ledger is justified by actual persistence/audit needs.
- Reject invalid/null recipes, invalid quantities and arithmetic overflow cleanly.

### B01.3 — Authored recipe definitions
- Extend RecipeDefinition only as needed for validation, stable identity and clear presentation.
- Keep inputs/outputs as ItemDefinition references, not string item IDs.
- Support one output definition and explicit positive integer input/output quantities for the first slice.
- No by-products, choice outputs, quality rolls, timing, queue or tool durability.
- Add editor validation or runtime validation messages so broken assets are diagnosable.

### B01.4 — Player workshop integration
- Reuse the existing hideout workbench and WorkbenchWorkspace; it is currently a tactile presentation-only surface with placeholder objects.
- Bind available recipes and production actions through GameSession.
- Show required inputs, output and current availability. Distinguish missing-input recipes from locked recipes.
- Confirm production explicitly; disable/reject duplicate or re-entrant confirmation.
- Revalidate station, recipe unlock, ownership, ingredients and output capacity at commit time.
- Keep UI state non-authoritative.

### B01.5 — First authored production content
- Add a small, coherent starter recipe and item content only after the exact product and inputs are approved in OPEN_QUESTIONS.
- Use existing placeholder/test items only if the user explicitly approves them as temporary fixture content.
- Add a second recipe definition as locked content for B-03 to unlock; exact content awaits approval.
- Do not add a large recipe catalogue.

## Acceptance criteria

1. Workbench opens from the existing physical hideout interaction and closes back to the same room.
2. At least one approved recipe is visible and craftable.
3. Production consumes exact required ingredients and creates the expected physical output.
4. Missing ingredients and insufficient capacity leave inventory/ownership/history unchanged.
5. Repeated confirmation cannot duplicate output.
6. UI never determines authoritative recipe, ownership or item counts.
7. Existing close/Escape, movement pause and inventory interactions remain correct.
8. Core production logic has focused tests for success, missing inputs, no-space rollback, duplicate/replay and ownership.
9. Unity integration is tested in the active HideoutSlice scene.

## Not included

Contracts, upgrade purchases, repair, production timing, multiple workstations, NPC production execution, item quality generation or economic sales.
