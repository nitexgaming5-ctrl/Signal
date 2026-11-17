# Task M0-01a — Move prototype (HUMAN task, agent verifies only)

## Who does this
The user, in the Unity Project window. Do NOT move files in Windows Explorer: that breaks .meta GUIDs and scene references.

## Steps for the user
1. Create `Assets/_Project/` if it doesn't exist.
2. Create `Assets/_Project/_Prototype/`.
3. Drag `Assets/_Project/Scripts/` into `_Prototype/`.
4. Drag `Assets/Tests/` into `_Prototype/`.
5. Leave all `.unity` scenes where they are.
6. Click Assets > Refresh and check the Console.

## Agent task (verification only)
- Run `find Assets -name "*.asmdef"` and list each file's path and "name" field.
- Report: any asmdef still outside `_Prototype/`, and any compile error in the Console the user pastes.
- Do NOT rename, move, or edit any file.

## Acceptance
- Console has no new errors caused by the move
- Report block filled in (AGENTS.md)
- PROGRESS.md: M0-01a done, next task M0-01b
