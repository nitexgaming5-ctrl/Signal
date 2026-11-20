# Task M0-01c — First passing test

## Prerequisite
M0-01b done and Console clean.

## Steps
1. Create `Assets/_Project/Core/CoreInfo.cs`:
```csharp
namespace Signal.Core
{
    public static class CoreInfo
    {
        public const string Name = "Signal.Core";
    }
}
```
2. Create `Assets/_Project/Tests/EditMode/Core/CoreInfoTests.cs`:
```csharp
using NUnit.Framework;

namespace Signal.Core.Tests
{
    public sealed class CoreInfoTests
    {
        [Test]
        public void NameIsSignalCore()
        {
            Assert.That(CoreInfo.Name, Is.EqualTo("Signal.Core"));
        }
    }
}
```
3. Click Assets > Refresh. Open Window > General > Test Runner > EditMode and click Run All.

## Files you may touch
- Assets/_Project/Core/CoreInfo.cs
- Assets/_Project/Tests/EditMode/Core/CoreInfoTests.cs
- docs/PROGRESS.md

## Acceptance
- Test Runner: 1 passed, 0 failed
- Console clean
- Report block filled in; PROGRESS.md: M0 complete, next task M1-01 (needs user go-ahead)
