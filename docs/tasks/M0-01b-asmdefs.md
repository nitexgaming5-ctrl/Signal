# Task M0-01b — Create the four assembly definitions

## Prerequisite
M0-01a done.

## Steps
1. Create folders if missing: `Assets/_Project/Core/`, `Assets/_Project/Game/`, `Assets/_Project/Tests/EditMode/Core/`, `Assets/_Project/Tests/EditMode/Game/`.
2. Create these files with exactly this content:

`Assets/_Project/Core/Signal.Core.asmdef`
```json
{
  "name": "Signal.Core",
  "rootNamespace": "Signal.Core",
  "references": [],
  "includePlatforms": [],
  "excludePlatforms": [],
  "allowUnsafeCode": false,
  "overrideReferences": false,
  "precompiledReferences": [],
  "autoReferenced": true,
  "defineConstraints": [],
  "versionDefines": [],
  "noEngineReferences": true
}
```

`Assets/_Project/Game/Signal.Game.asmdef`
```json
{
  "name": "Signal.Game",
  "rootNamespace": "Signal.Game",
  "references": ["Signal.Core"],
  "includePlatforms": [],
  "excludePlatforms": [],
  "allowUnsafeCode": false,
  "overrideReferences": false,
  "precompiledReferences": [],
  "autoReferenced": true,
  "defineConstraints": [],
  "versionDefines": [],
  "noEngineReferences": false
}
```

`Assets/_Project/Tests/EditMode/Core/Signal.Core.Tests.asmdef`
```json
{
  "name": "Signal.Core.Tests",
  "rootNamespace": "Signal.Core.Tests",
  "references": ["Signal.Core"],
  "includePlatforms": ["Editor"],
  "excludePlatforms": [],
  "allowUnsafeCode": false,
  "overrideReferences": true,
  "precompiledReferences": ["nunit.framework.dll"],
  "autoReferenced": false,
  "defineConstraints": ["UNITY_INCLUDE_TESTS"],
  "versionDefines": [],
  "noEngineReferences": false
}
```

`Assets/_Project/Tests/EditMode/Game/Signal.Game.Tests.asmdef`
```json
{
  "name": "Signal.Game.Tests",
  "rootNamespace": "Signal.Game.Tests",
  "references": ["Signal.Core", "Signal.Game"],
  "includePlatforms": ["Editor"],
  "excludePlatforms": [],
  "allowUnsafeCode": false,
  "overrideReferences": true,
  "precompiledReferences": ["nunit.framework.dll"],
  "autoReferenced": false,
  "defineConstraints": ["UNITY_INCLUDE_TESTS"],
  "versionDefines": [],
  "noEngineReferences": false
}
```

3. Click Assets > Refresh. Check the Console.

## Do NOT
- Put any .cs file in these folders yet (that's M0-01c)
- Touch `_Prototype/`
- Change Project Settings

## Acceptance
- Console has zero errors
- Each asmdef is listed by name in the Project window
- Report block filled in; PROGRESS.md: M0-01b done, next M0-01c
