# Development workflow — keep Signal playable and focused

## Purpose
This workflow exists to protect the project's momentum and the user's attention. The goal is a compelling, playable game, not an ever-growing collection of disconnected systems.

## The north star
Signal is a hybrid management/simulation RPG and narrative-driven RPG. Deep simulation creates pressure and emergent situations; characters, choices, relationships, and consequences make that pressure matter.

Every substantial task must answer:
1. What can the player do after this that they could not do before?
2. Which part of the core experience does it improve?
3. How will we see or feel the result in a playable build?
4. What existing systems does it depend on?
5. What is the smallest complete version that delivers the experience?

If a task has no clear player-facing payoff, defer it unless it is necessary to protect the project (bug, architecture, tooling, save integrity, build reliability).

## Work-in-progress limit
- There is **one active gameplay initiative** at a time.
- There may be small supporting engineering tasks only when they directly unblock or protect that initiative.
- Do not start a second feature because it is exciting while the active feature is unfinished.
- Keep the next action visible in `docs/PROGRESS.md`; the user should never have to reconstruct the plan from old chat messages.

## The idea parking lot
New ideas are welcome, including ambitious ones.
- Capture ideas in `docs/IDEA_BACKLOG.md` with a short description, player fantasy, likely system impact, and a rough priority.
- Do not turn a parked idea into active work automatically.
- At the end of the current initiative, review the backlog and choose at most one next initiative.
- Combine, simplify, or discard ideas that do not strengthen the core loop.
- The backlog is not a promise to implement everything.

## Vertical-slice-first development
Build connected player experiences, not isolated subsystem checklists.
- Define a small player-facing scenario.
- Identify only the minimum systems needed for that scenario.
- Implement the full path through Core, Unity integration, feedback/UI, and content.
- Add tests for the underlying rules.
- Verify the experience in the Unity Editor.
- Only then expand its depth, variety, or content.

A system is not "done" merely because its C# class exists. It is done when it contributes to a working player experience and its exit criteria are verified.

## Initiative lifecycle
1. **Choose:** state the player-facing outcome and why it is the best next step.
2. **Scope:** write the smallest end-to-end acceptance criteria; identify risks and dependencies.
3. **Protect:** inspect current behavior and relevant tests before changing existing foundations.
4. **Build:** work in coherent batches; prefer complete, reviewable changes over tiny conversational edits.
5. **Integrate:** connect the new behavior to the actual scene, UI, controls, and content.
6. **Verify:** run available tests and Unity checks; never claim checks that were not run.
7. **Demonstrate:** provide a short playtest script so the user can feel the result.
8. **Record:** update `docs/PROGRESS.md`, close/update task notes, and capture new ideas in the backlog.
9. **Choose next:** recommend one next action, not a long list of competing priorities.

## Protect the working core
The existing Core systems are the foundation, not disposable scaffolding.
- Prefer additive changes and small, reversible refactors.
- Before changing a Core API, inspect all callers and tests.
- Preserve save compatibility; any save-format change requires a versioned migration and tests.
- Preserve Unity asset GUIDs and scene references. Do not casually rename/move/delete scenes, scripts, prefabs, or ScriptableObjects.
- Keep a known-good branch and use initiative branches for substantial work.
- Do not replace a working implementation just because a different architecture looks cleaner.
- If a change risks data loss, save corruption, scene breakage, or broad regressions, stop and make a backup/compatibility plan first.
- If the user says "we can fix it", that permits initiative and experimentation, not careless destruction of the only known-good baseline.

## Focus-friendly collaboration
- Keep one clear current objective and one recommended next action.
- Explain decisions in plain language and keep implementation details available without forcing the user to hold them all in working memory.
- Do not ask the user to choose between many low-level technical options when a safe engineering default exists.
- Ask for user input only when a decision changes the game's identity, player-facing rules, tone, progression, or other meaningful creative direction.
- When a creative choice is needed, present a concise recommendation with the player experience, trade-offs, and a clear approve/change choice.
- Treat tangents as ideas to capture, not reasons to abandon the active initiative.
- At each handoff, summarize: what changed, what can now be played, what remains unverified, and the single next step.

## Definition of done
An initiative is complete only when:
- Its player-facing acceptance criteria are met.
- It is integrated into the real Unity scene and controls where applicable.
- Relevant automated tests pass.
- Unity compilation and Play Mode behavior have been checked when Unity is available.
- Known limitations are recorded.
- `docs/PROGRESS.md` is updated.
- The user receives a concise way to try the result.

If the assistant cannot run Unity, mark Editor verification as pending and give the user the shortest possible test procedure.

## Priority order
1. Protect data, save compatibility, build health, and known-good behavior.
2. Complete the current playable initiative.
3. Remove blockers that directly prevent it.
4. Improve usability and feedback for that experience.
5. Add depth and content after the loop works.
6. Explore ambitious optional ideas only when the current initiative is stable.
