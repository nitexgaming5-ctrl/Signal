# Art and UI

## Decision: no resource packs in the final game (test-only packs allowed; see decisions/0004)
Final art is made from scratch or from approved assets. Third-party packs may be used only as temporary test content, logged in `docs/ART_TODO.md`; no pack is final art.

## Placeholder phase (now)
- Floors, walls, items: solid colored squares or simple shapes
- Every placeholder logged in `docs/ART_TODO.md` with: asset name, purpose, size
- Colors used for placeholders come from the palette below, so the final pass doesn't shift the look

## Pixel and grid (decision needed, see OPEN_QUESTIONS)
- Recommended: **32×32 pixel tiles**, characters about 24–28 px tall, UI at native resolution or 2× integer scale
- Set Unity's Pixels Per Unit to 32 and Filter Mode to Point (no filter) on all pixel sprites
- Camera: orthographic, size set so the room fills about 60% of the screen height

## Palette (draft, to validate)
Keep each scene to ≤ 24 colors plus neon accents.
- Base: near-black blue `#0E1116`, concrete grey `#3A3F47`, mid grey `#6B7280`, light grey `#C9CED6`
- Warm: rust `#8B4A2B`, dull yellow `#C8A24A`
- Accent (neon, sparingly): cyan `#3EE6D2`, magenta `#E24FB5`

Neon only for signs, UI highlights, and dark-humour moments. Never for large areas.

## Tools
- **Aseprite** (paid) or **LibreSprite** (free fork) or **Piskel** (free, browser). Pick one and stay with it.
- Unity 6000.6.9f1 includes `com.unity.2d.aseprite`. Save source art as `.aseprite` in `Art/` and Unity imports it directly. Keep the `.aseprite` file as the master.

## UI
- Use UGUI (already installed). Build panels from Canvas + Image + TextMeshPro.
- Inventory grid: each cell is 32×32 display pixels at scale 1 (or 2× integer scale). Item rectangles fit their grid footprint.
- Fonts: pixel font, one family only. Decision needed, see OPEN_QUESTIONS.
- No gradients, no blur, no drop shadows that look like smooth vector art.

## Rules for agents
- Use placeholders unless the card says "art pass".
- Never add a new pack without asking. Test-only packs must be listed in `docs/ART_TODO.md` with their licence file.
- Never change the palette without an OPEN_QUESTIONS entry.
