# Art and UI — approved working conventions

## Visual identity

Signal uses readable top-down 2D pixel-art presentation with a contemporary, worn urban atmosphere. The hideout should feel modest, personal and inhabited; the city should be readable and layered. Warm interiors and restrained neon accents can contrast with concrete, steel and dark streets. Reference games inform interaction quality, not literal asset/style copying.

## Pixel and grid decisions

- Tile / reference pixel size: **32×32** (approved Q2).
- Unity Pixels Per Unit and point filtering should remain consistent with the authored sprite scale. Existing placeholder sprites may use different import settings; inspect before changing.
- Inventory: starting grid is **6×4 cells** (approved Q4). Current presentation sizing is provisional and should be checked at the target resolution.
- Characters and props need clear silhouettes and readable interaction points.

## Palette and presentation

The current palette is a draft implementation guide, not a locked final art bible:
- near-black blue `#0E1116`, concrete `#3A3F47`, mid-grey `#6B7280`, light grey `#C9CED6`;
- rust `#8B4A2B`, dull yellow `#C8A24A`;
- cyan `#3EE6D2` and magenta `#E24FB5` reserved for sparse accents and feedback.

No gradients, blur or smooth-vector drop shadows in pixel-art scenes. Preserve point filtering and integer scaling where appropriate.

## UI

- UI framework: **UGUI** (approved Q3).
- Text: UGUI `Text` using the built-in font for now (approved Q10). Do not silently migrate to TextMeshPro. A pixel-font pass can select a font and rendering approach later.
- Inventory key: Tab. Drag with left mouse; R rotates while dragging; Esc cancels; invalid drops leave the item in place.
- Keep UI direct, readable and contextual. Important transactions require explicit review/confirmation and clear results.
- The current WorkbenchWorkspace is a close-up workspace with draggable placeholder objects and a recipe-book/production panel. Treat it as the existing interaction surface, not a disposable mock-up.

## Assets and licences

Final art must be original or explicitly approved. Third-party art packs may be used only as temporary test content (decision 0004); record each pack and licence in ART_TODO. Do not add a new pack without approval. Keep source files and Unity importer metadata intact.

## Tools

Aseprite, LibreSprite or Piskel are acceptable art-authoring choices. The project uses Unity 6000.6.9f1. Keep source art and any generated placeholder assets identifiable and replaceable.
