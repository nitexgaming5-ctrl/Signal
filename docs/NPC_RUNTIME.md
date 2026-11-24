# NPC runtime — pacing, navigation and interruption

This reference covers systems that affect how NPC simulation advances through the city: shared development pacing, navigation geometry and controlled interruption/recovery. Core perception, routines, relationships and individual knowledge are documented in [NPC_SIMULATION.md](NPC_SIMULATION.md).

## Shared world pacing

- `WorldPacingDirector` is the single shared permission gate for consequential world developments. It does not create events, choose outcomes, or queue candidates.
- A calling system must first establish that a development has valid context (for example, relevant circumstances and an eligible scenario condition), then call `TryAuthorizeDevelopment`. Invalid candidates do not consume pacing permission.
- All development systems share one interval. There are no independent social/business/discovery event budgets in this first pass, and unused permission never accumulates into multiple future releases.
- The director requires an authored scenario phase ID. Reapplying the same phase does not reset its clock. Changing phase starts a fresh restraint ramp and a quiet interval, so phase advancement cannot dump a backlog.
- The permitted interval grows linearly from a configured minimum to a configured maximum over a configured phase-ramp duration. Long periods in one phase therefore make meaningful developments progressively less frequent.
- The director is deliberately not connected to ambient NPC meetings, familiarity, routine decisions or ordinary perception. Those remain everyday simulation.
- The director has no default tuning values and is not yet wired to scenario content. A future scenario owner must supply approved pacing values and call it only after validating a real candidate. No dramatic event catalogue or automatic catch-up scheduler is introduced.
- Clock rewinds are rejected for authorization rather than treated as fresh time; persistence/reconstruction of the director's phase and last authorization will be designed alongside scenario/save state before this becomes live gameplay.

- The newly added WorldPacingDirector tests are a separate, later change and still require a fresh Unity run; the 219/219 result predates this pacing addition.


## City navigation geometry

- Added `CityNavigationMap` in the Game layer. It describes each coordinate-space location as a walkable rectangular region, records solid colliders as obstacles, and joins locations through named doorway connections.
- The current map registers Hideout, Street, CornerShop and Apartment. The street is one connected area containing the road and both sidewalks; shop/apartment interiors use their own coordinate spaces. Doorway links describe how those spaces connect.
- Street/interior solid geometry is snapshotted from generated BoxCollider2D bounds. The hideout's TilemapCollider2D perimeter is represented as separate wall rectangles, preserving its south doorway instead of treating the tilemap's aggregate bounds as one solid block. Point-level walkability queries combine region bounds and obstacle rectangles. Character radius/clearance is not yet part of the query.
- Added a runtime navigation debug overlay, toggled from F1. It shows the active region, solid geometry, doorway endpoints, player position, visible residents and their currently authored travel waypoints.
- This is a geometry/connectivity foundation only. It does not change player movement, NPC movement, collision, or route selection. NPCs still use their existing authored waypoint routes; obstacle-aware pathfinding is deliberately next, after the map is inspected and validated in Play Mode.
- The map currently models the existing four places and their existing doorways only. New buildings should add their region, collider obstacles and explicit doorway connections through the same representation.


## NPC interruption and recovery

- Initial binding is the only normal lifecycle operation allowed to snap a resident to an authored starting position. Re-enabling a presenter and resuming schedules use the current physical position.
- Debug-forced activities remain temporary overrides. Returning to the authored schedule clears the override and plans a route from the NPC's current location/position to the activity selected by the current game clock.
- Pausing simulation freezes movement and preserves the current route. On resume, an unchanged schedule keeps that route; a changed schedule replaces it with a newly planned route from the interruption point.
- Failed navigation remains explicit. A caller may request one retry through DebugRetryNavigation; the F1 menu exposes a one-shot retry across failed residents. There is no per-frame retry loop.
- The map is currently a static build-time snapshot, so retry is a controlled diagnostic hook for cases where the navigation representation has become usable again; dynamic obstacle/map rebuilding is future infrastructure, not part of this milestone.
- Focused EditMode tests cover forced-activity return, pause/resume route preservation and schedule changes while paused. Unity compilation, the full suite and Play Mode validation remain pending user verification.
