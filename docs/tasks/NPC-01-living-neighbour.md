# NPC-01 — First living neighbourhood character

## Player outcome
The starter street contains one identifiable resident who follows a daily routine and responds differently depending on what they are doing. The player can approach and talk to them.

## Scope
- Add a deterministic, hardware-friendly Core utility brain.
- Give the first NPC four activities: Work, Rest, Socialize and Home.
- Let the Game layer move and present the NPC at locations in the existing shared street.
- Connect the NPC to the existing E interaction prompt and HUD message.
- Add tests for decision behaviour and the Game integration.

## Design constraints
- No per-NPC language model, external AI service, random inference, background thread or per-frame decision scoring.
- No separate criminal map or NPC-only activity menu.
- The NPC has a public occupation and activity-aware dialogue; the evening line only hints at a wider private life.
- Do not invent persistent relationships, secret discovery or economic rewards in this first prototype.

## Implementation
- `Assets/_Project/Core/NpcUtilityBrain.cs`
- `Assets/_Project/Game/NeighbourNpc.cs`
- `Assets/_Project/Game/CityBlockBootstrap.cs`
- `Assets/_Project/Game/GameSession.cs`
- Core and Game EditMode tests.

## Verification status
Code and tests authored; not yet run in Unity. User must verify compilation, EditMode tests, street movement, E prompt, activity-specific dialogue and behaviour after spending time at home / sleeping.

## Follow-up
Connect this character to a complete shared-city activity with discovery, meaningful outcome and persistence. Add memory/relationship data only when that scenario requires it.
