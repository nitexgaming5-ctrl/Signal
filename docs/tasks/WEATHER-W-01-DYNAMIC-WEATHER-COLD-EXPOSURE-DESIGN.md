# W-01 — Dynamic Weather & Cold Exposure

**Status:** Separate future design phase; design proposal only; implementation not authorized.
**Track:** World Simulation
**Dependencies:** World clock and location context; P-02 Heat Protection attribute contract; P-03 equipment modifiers; player health and shelter/interior state.

## Purpose

Give the shared city a changing climate that affects atmosphere and outdoor decisions. Weather is dynamic across seasons, but player survival consequences are deliberately focused on cold events. The player experiences gradual cold exposure rather than instant punishment from ordinary rain.

## Confirmed direction

- Dynamic weather includes seasonal conditions, changing temperatures, rain, storms, snow and blizzards.
- Weather may affect visual atmosphere and outdoor conditions throughout the connected city.
- The player-facing protection attribute remains named **Heat Protection**.
- Heat Protection is interpreted as protection against cold exposure, including low temperature, wind chill and precipitation-related cooling.
- Only cold-related exposure events are planned. No heatwaves, overheating, hot-weather exposure or heat damage.
- Exposure severity is gradual: ordinary weather is not an instant health drain; meaningful danger is reserved for sustained/severe cold exposure.
- Weather should be shared world state, not a per-room random effect. Interiors/shelter should protect the player from outdoor conditions and support recovery.

## Proposed weather state

A single world weather state should describe, at minimum:
- season / seasonal phase;
- ambient temperature or authored temperature band;
- precipitation kind (none/rain/snow) and intensity;
- wind intensity;
- storm severity / lightning presentation state where relevant;
- current weather pattern ID and transition timing.

The state should be authored/data-driven and deterministic for a given saved world timeline. This is a game weather model, not a real meteorological simulation or live real-world forecast.

## Proposed weather progression

- Weather transitions are driven by the existing GameClock, not independent real-time timers.
- Seasonal pattern definitions describe allowed conditions and weighted/ordered transitions.
- A saved world seed plus timeline/pattern state should make weather reproducible across save/load.
- Weather transitions should not reroll unpredictably every frame or on scene changes.
- Weather may change while the player is indoors; leaving the hideout should reveal the current shared state.
- Visual/audio presentation is separate from gameplay weather state and must not become authoritative.

## Cold exposure model

A Core exposure model evaluates:
- ambient temperature band/value;
- rain/snow intensity;
- wind intensity;
- time spent exposed outdoors;
- effective Heat Protection;
- whether the player is in a protected interior/shelter.

Exposure accumulates and recovers gradually. Heat Protection reduces the rate or severity of cold accumulation; shelter stops or strongly reduces accumulation and enables recovery. Exact formulas, stage thresholds, durations and health consequences remain open balancing decisions.

Suggested player-facing stages:
1. Comfortable;
2. Cold;
3. Very Cold;
4. Severe Exposure.

Stages should communicate increasing discomfort/penalties. Health loss, if ultimately approved in balancing, belongs only at sustained severe exposure and must be clearly telegraphed. Rain in mild conditions should not automatically damage health.

## Equipment integration

- P-03 equipment definitions may author Heat Protection modifiers.
- W-01 reads the effective Heat Protection result from the character attribute calculation contract.
- W-01 must not inspect specific clothing names or contain jacket/armor item-ID branches.
- Waterproofing may be a separate future clothing property if gameplay needs it, but it is not a third player attribute. Do not silently rename Heat Protection or add a separate environmental-protection stat.
- Initial cold exposure can be driven by temperature, wind, precipitation and shelter without requiring a separate waterproofing stat.

## Scope proposal

1. Weather state and deterministic transitions.
2. Seasonal pattern definitions and temperature bands.
3. Rain, snow, wind and storm state/presentation.
4. Player outdoor exposure accumulation/recovery.
5. Heat Protection integration.
6. Interior/shelter protection.
7. Save/load of world weather timeline/seed and player exposure.
8. Weather visuals/audio for the active hideout/street slice.

The first W-01 vertical slice should demonstrate one clear-weather → rain/storm → cold/snow progression and player recovery indoors, not every conceivable weather effect.

## Deferred possibilities

- NPC weather reactions and schedule changes.
- Business/customer demand responses.
- Weather-dependent scavenging loot or crime visibility.
- Road/terrain movement penalties, flooding, snow accumulation and power outages.
- Weather forecasts, radio reports and forecast accuracy.
- Hot weather and heat exposure.
- Waterproofing as a separate equipment modifier.
- Full citywide visual/audio polish beyond the initial connected slice.

## Acceptance criteria for eventual implementation

- One authoritative weather state is shared across connected locations.
- Weather changes are driven by world time and survive save/load deterministically.
- Seasonal definitions constrain plausible weather patterns.
- Rain, storm, wind, snow and blizzard can be represented without adding unrelated player attributes.
- Player cold exposure changes gradually based on conditions, outdoor duration, shelter and effective Heat Protection.
- Ordinary rain does not cause immediate health loss.
- Severe exposure is telegraphed and recoverable through appropriate clothing/shelter.
- Interiors protect the player and recovery is consistent across location transitions.
- Weather visuals are presentation-only and cannot diverge from authoritative weather state.
- Core exposure tests cover accumulation, protection, shelter, recovery, thresholds and save/load; Play Mode verifies transitions across hideout/street.

## Design questions intentionally open

- Exact season length and calendar model.
- Whether weather patterns are authored schedules, weighted deterministic transitions, or a hybrid.
- Temperature units/ranges and exact accumulation/recovery formula.
- Whether severe exposure causes health damage, movement penalties or both.
- Whether precipitation wetness is represented as a separate temporary clothing state.
- How much weather is visible/heard indoors.
- NPC reactions and effects on city routines.

These should be decided in the W-01 detailed design before implementation. This phase is separate from P-01/P-02 and does not block their data-model preparation.
