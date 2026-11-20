# Task M0-01-FIX — Move prototype code and verify

## Why this exists
M0-01a (moving old code into `_Prototype/`) was never done. A previous M0-01b report claimed work that the file system doesn't show. This task fixes that, and verifies with real command output.

## Before you start
- Ask the user to CLOSE the Unity Editor. Moving files while Unity is open can corrupt its cache. Do not continue until the user confirms Unity is closed.

## Steps
1. Create the folder `Assets/_Project/_Prototype/` if it does not exist.
2. Move these folders, using `mv` so each folder's `.meta` file moves with it:
   - `Assets/_Project/Scripts` → `Assets/_Project/_Prototype/Scripts`
   - `Assets/Tests` → `Assets/_Project/_Prototype/Tests`
3. Do NOT move:
   - `.unity` scene files
   - `Assets/Welcome/`
   - anything under `Packages/` or `ProjectSettings/`
4. Do NOT rename any file. Do NOT edit any `.cs` or `.asmdef` content.
5. Run this command and paste its full output into the report:
   ```
   find Assets -name "*.asmdef" | sort
   find Assets -name "*.asmdef.meta" | sort
   ```
6. Check that every `.asmdef` has a matching `.asmdef.meta`. If one is missing, STOP and report it. Do not create or copy meta files.

## Expected result
The `.asmdef` list must contain:
- `Assets/_Project/_Prototype/Scripts/Journal/Signal.Journal.asmdef`
- `Assets/_Project/_Prototype/Scripts/Shop/Signal.Shop.asmdef`
- `Assets/_Project/_Prototype/Tests/EditMode/Journal/Signal.Journal.EditModeTests.asmdef`
- `Assets/_Project/_Prototype/Tests/EditMode/Shop/Signal.Shop.EditModeTests.asmdef`
- `Assets/Welcome/Unity.2D.Welcome.asmdef`

It must NOT contain any `.asmdef` directly under `Assets/Tests/` or `Assets/_Project/Scripts/`.

## Files you may touch
- Folders under `Assets/_Project/` and `Assets/Tests/` (moves only)
- `docs/PROGRESS.md`
- `docs/OPEN_QUESTIONS.md` (only if you stop with a question)

## Do NOT
- Create the four new asmdefs (that is M0-01b, a separate task)
- Create any `.cs` file
- Change Project Settings or packages

## Report rules
- Report only what the command output shows. Do not list files you did not see in the output.
- If the output differs from "Expected result", STATUS is blocked, and say exactly what differs.

## Acceptance
- Command output matches "Expected result"
- Unity is still closed when you finish, and the user is asked to reopen it
- PROGRESS.md: M0-01-FIX done, next task M0-01b (redo)

## After this card
The user reopens Unity, checks the Console for errors, and then runs M0-01b as a separate session.