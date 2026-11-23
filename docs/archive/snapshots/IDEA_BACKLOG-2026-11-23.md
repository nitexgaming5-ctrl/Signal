# Idea backlog

This is a parking lot, not an implementation queue. Ideas are captured so they are not lost, but only one gameplay initiative is active at a time.

## How to add an idea
Record:
- **Idea:** one sentence.
- **Player fantasy:** what the player gets to do or feel.
- **Why it matters:** which Signal pillar or core loop it strengthens.
- **Likely dependencies:** only if known.
- **Status:** parked / needs design / approved / active / shipped / rejected.

Do not implement a parked idea until it is selected as the next initiative and its player-facing rules are agreed.

## Approved direction
- **Personal hideout:** a modest, walkable home with functional furniture, storage and decoration; the central hub and emotional anchor.
- **One shared city:** lawful businesses, ordinary residents and illegal opportunities coexist in the same streets, buildings, shops and social network. A separate criminal "underground" is not the default.
- **Parallel lifestyles:** players can freely combine legal and illegal activities; neither excludes the other.
- **Hidden dual lives:** NPCs can have public occupations and concealed private activities. A baker might also secretly sell illicit goods. The player discovers secrets through play rather than knowing every role immediately.
- **Crime as a core pillar:** fictional drug-related activity and a broad range of other illegal activities should be central content. Specific activities and rules need design.
- **Harsh criminal failure:** serious, potentially lasting consequences for criminal failure. Exact penalties and recovery rules need design.
- **Global crime progression:** all criminal activity contributes to one overall criminal level/progression track.
- **Varied legitimate livelihoods:** trade, services, gathering, processing, repair, making goods, small businesses and other income loops. Farming is one possible example, not a fixed path.
- **Goods with meaningful properties:** quality, condition, authenticity, origin/provenance and ownership status can influence inspection, use, trade and risk where they create interesting decisions.
- **Shared people and events:** NPCs, clients, suppliers, contacts, factions, rumours and world events create opportunities and consequences for both lawful and illegal activity in the same neighbourhoods.

- **Low-cost living NPCs:** approved direction is a hybrid of authored routines and a deterministic, needs-driven utility brain. Begin with a few meaningful activities and scale simulation detail by importance, not by running a heavyweight agent for every citizen.

## Candidate systems — needs design before implementation
- **NPC depth and persistence:** determine when relationships, memories, discovered secrets and off-screen activity need persistent state. The first utility-brain prototype is in place; deeper behaviour should be driven by a connected scenario.
- **Goods and item identity:** decide which goods need quality/condition/authenticity/provenance/ownership data, how players discover those properties, and how much information is hidden or uncertain.
- **Trade and customer system:** buyers and sellers with needs, budgets, preferences, trust and different reactions to goods or suspicious transactions.
- **NPC public/private lives:** occupations, routines, relationships, secrets, contact networks and discoverability clues.
- **Crime catalogue and global progression:** define crime activities, what grants criminal progress, thresholds, unlocks and whether reputation is separate from level.
- **Harsh failure and recovery:** define consequence tiers, what can be lost, temporary/permanent restrictions and recovery paths without making failure arbitrarily save-ruining.
- **Legal activity catalogue:** define varied trade, service, repair, gathering, processing, production and small-business loops. Do not assume farming is the main legal route.
- **World events:** define event triggers, duration, locality, player agency and persistence.
- **Shared economy:** define currencies, prices, supply/demand and how legal and illegal goods circulate through the same city.
- **Reusable activity framework:** define object/location interactions, character/request hooks, discovery, outcomes, rewards/failures and save-state requirements.

## Example content ideas (not fixed mechanics)
- **Water trade:** source, test, filter, bottle and sell different grades of water; quality and mislabelling may create trade-offs.
- **Food and everyday goods:** make, acquire, inspect, repair, package and sell goods.
- **Second-hand and stolen goods:** find or acquire household items, assess condition/provenance and resell them; legality and consequences need design.
- **A character with two lives:** an ordinary neighbourhood baker who also has a concealed illegal business, discoverable through clues and interactions.
- **Fictional drug-related activities:** part of the core crime catalogue, designed as game systems and not as real-world instructions.

These are inspiration and content directions, not approved detailed rules.

## Parked ideas
Future ideas about specific money-making activities, loot rules, police response, NPC schedules, faction structures, and furniture placement rules should be captured here and agreed before implementation.

## Economy Engine-inspired directions — parked (2026-11-19)

Reference reviewed: Unity Asset Store's Economy Engine 1.4. These are design inspirations only; Signal will retain its existing Core/Game architecture and transaction authority. No third-party asset or code is approved for adoption.

- **Shared trade framework** — explore a common, validated transaction foundation that can support distinct contexts such as ordinary shop purchases, fixed-price buybacks, item-for-item barter, specialist buyers and discreet exchanges. Each context/business/NPC defines its own acceptance and offer policy; sharing transaction safety must not make every NPC a shop or force every interaction through a retail UI.
  - Player fantasy: negotiate the practical problem of moving goods between people and places, with different kinds of exchange.
  - Why it matters: connects legal and illegal livelihoods through the same shared-city economy.
  - Likely dependencies: current item identity, ownership, inventory, account and transaction-ledger foundations; concrete player-facing slices before abstractions.
  - Status: parked / needs design.
- **Economy Library and authoring tools** — consider an editor-facing catalogue/workflow for browsing and validating item, currency, business, offer, trade-table and loot-table definitions as content grows. Prefer improving Signal's ScriptableObject authoring and validation over importing an external runtime framework.
  - Player fantasy: indirect; enables a richer, more consistent world economy and faster content iteration.
  - Why it matters: reduces authoring friction and definition errors once the number of goods and businesses grows.
  - Likely dependencies: stable definition schemas and demonstrated content-authoring pain.
  - Status: parked / tooling candidate.
- **Loot tables and item sources** — explore authored weighted outcomes and quantity ranges for specific containers, discoveries or activities. Keep source/context-specific rules; do not turn all item acquisition into random drops.
  - Player fantasy: search places and discover uncertain but contextually plausible goods.
  - Why it matters: supports scavenging, abandoned properties and environmental discovery.
  - Likely dependencies: item definitions, container/search interactions, deterministic testable random source and save-state rules where outcomes persist.
  - Status: parked / needs design.
- **Barter / item-for-item exchange** — explore offers that exchange one or more exact item quantities for other goods, with ownership and inventory-capacity safeguards. This is distinct from simply selling for CARD.
  - Player fantasy: trade useful goods when cash is unavailable or undesirable.
  - Why it matters: supports informal contacts, desperate residents and discreet exchanges.
  - Likely dependencies: shared trade transaction rules, exact-stack removal, atomic rollback and a clear player-facing offer/acceptance design.
  - Status: parked / needs design.
- **Explicit currency exchange** — consider narrowly authored exchange interactions only when a real scenario needs them. Preserve the approved distinction between clean CARD and dirty money as a physical inventory item; exchange must not silently turn dirty money into clean funds or bypass a separately designed laundering mechanic.
  - Player fantasy: access a specific financial service or contact with understandable terms and risks.
  - Why it matters: can add texture to different economic contexts without collapsing money systems.
  - Likely dependencies: currency/account definitions and a separately approved conversion policy.
  - Status: parked / needs design.

**Guardrails:** these ideas do not approve universal supply/demand, dynamic pricing, automatic currency conversion, continuous economy simulation, NPC shopping AI, finite stock/restocking, or a wholesale replacement of Signal's existing transaction systems. Build only the smallest player-facing vertical slice that proves a need.


## Direction B — approved livelihood initiative (2026-11-21)

- **Idea:** make the player's everyday life meaningful through authored production, customer contracts, practical hideout development, relationships and services.
  - **Player fantasy:** build a modest livelihood in the neighbourhood: obtain materials, make useful goods, fulfil customer requests, earn clean CARD and gradually improve a practical home workshop.
  - **Why it matters:** gives the player a satisfying legal path alongside crime; connects inventory, crafting, economy, NPCs and hideout progression into one daily loop.
  - **Approved choices:** both player-operated and NPC-owned workshops belong in the long-term model; start with the player's hideout workshop. One active customer contract at a time. The first practical hideout upgrade unlocks another authored recipe/capability.
  - **Implementation order:** B-01 production/workshop, B-02 contracts, B-03 practical recipe-unlocking upgrade.
  - **Future design placeholders:** B-04 repair/restoration; B-05 social life and recurring customers; B-06 additional livelihoods; B-07 neighbourhood expansion.
  - **Guardrails:** authored recipes and fixed contract rewards; physical goods; existing clean CARD accounts; no supply/demand, automatic NPC shopping, passive revenue, dynamic pricing or generic business simulator.
  - **Status:** approved strategic direction; detailed B1-B3 specification prepared; implementation not started. Exact product/recipe, issuer/reward, provenance policy, delivery semantics and upgrade cost/content remain open in OPEN_QUESTIONS.md.
