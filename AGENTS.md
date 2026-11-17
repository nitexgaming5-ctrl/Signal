# Signal — Agent Rules (read this first, every session)

Unity 6000.6.9f1, 2D URP, Input System 1.20.1, UGUI, Test Framework 1.8.0, Tilemap.
Project root: X:\GameDev\Signal\UnityProject\Signal

## Read order (every session)
1. `docs/PROGRESS.md`: where we are and the one open task
2. The task card you were given (`docs/tasks/<id>.md`): this is your whole scope
3. `docs/OPEN_QUESTIONS.md`: read the ANSWER lines, they are binding decisions
4. `docs/ARCHITECTURE.md`: where code goes and how systems talk
5. `docs/DATA_MODEL.md`: only if your task touches data or state
6. Any `AGENTS.md` in the folder you are about to edit

If there is no task card, STOP and ask which task to do. Do not pick one yourself.

## The three rules that matter most
1. **Build only what the task card asks for.** No extra items, recipes, stats, NPCs, prices, or systems.
2. **Never invent game content.** Item names, prices, recipe steps, faction names, stat names, dialogue, and locations come from `docs/DESIGN.md`, `docs/OPEN_QUESTIONS.md` answers, or the user. If they are not written there, it is an open question.
3. **Foundations before features.** Core logic (plain C#, tested) comes before UI, and UI comes before world content. Do not build a feature whose data model does not exist yet.

## Hard rules
- Stay inside the task card's "Files you may touch" list. Create files only if the card allows it.
- Never delete or rename a file unless the card says so. Mark old code with `// DEPRECATED: <reason>` instead.
- No new Unity packages, no Asset Store or resource packs, no third-party code without asking.
- No `string` item or recipe ids in gameplay code. Reference definitions (ScriptableObjects) instead. See DATA_MODEL.md.
- No `FindObjectOfType`, `GameObject.Find`, or new singletons. Pass dependencies through serialized fields or constructors.
- Keep pure logic free of `UnityEngine` references so it can be tested in EditMode.
- Placeholder art is allowed (solid colors, simple shapes). Log every placeholder in `docs/ART_TODO.md`.
- Every change must compile. Check the Unity Console before reporting done. If it doesn't compile, fix it or stop and report. Do not move asmdefs or change project settings to get around an error.
- Do NOT run `dotnet`, `msbuild`, or any build command. Unity compiles the project, not you.
- Do NOT claim a file exists unless a command you ran shows it.
- Run EditMode tests for any logic you added. Report the result.

## Stop and ask when
- The task card conflicts with DESIGN.md or an OPEN_QUESTIONS answer
- You need a decision the docs don't cover
- Compile errors appear outside your files
- You would need to touch more files than the card lists

When you stop, don't decide the question yourself. Add it to `docs/OPEN_QUESTIONS.md` with two options and your recommendation, then end the session.

## Report format (end of every session, copy this block)
```
TASK: <id>
STATUS: done | partial | blocked
CHANGED: <list of files, created or modified>
VERIFY IN UNITY: <exact steps the user should do to see it work>
TESTS: <EditMode results, pass/fail counts>
PLACEHOLDERS ADDED: <yes/no, and ART_TODO lines added>
OPEN QUESTIONS ADDED: <list, or none>
PROGRESS.md: updated | not updated (why)
```

## Session end
1. Update `docs/PROGRESS.md`: mark the task done and set the next task. Rewrite the whole file, don't append.
2. Add any new questions to `docs/OPEN_QUESTIONS.md`.
3. Write the report block above.
4. Do not start the next task.
