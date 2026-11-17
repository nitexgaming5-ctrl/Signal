using NUnit.Framework;

namespace Signal.Core.Tests
{
    public sealed class WorldPacingDirectorTests
    {
        [Test]
        public void RequiresAnAuthoredPhaseAndValidContext()
        {
            var director = new WorldPacingDirector(60, 240, 1440);

            Assert.That(director.TryAuthorizeDevelopment(true, 1, 60), Is.False);
            Assert.That(director.SetScenarioPhase("opening", 1, 60), Is.True);
            Assert.That(director.TryAuthorizeDevelopment(false, 1, 120), Is.False);
            Assert.That(director.TryAuthorizeDevelopment(true, 1, 128), Is.False);
            Assert.That(director.TryAuthorizeDevelopment(true, 1, 129), Is.True);
        }

        [Test]
        public void OneSharedGateLimitsAllDevelopmentCallers()
        {
            var director = new WorldPacingDirector(60, 60, 600);
            director.SetScenarioPhase("opening", 1, 0);

            Assert.That(director.TryAuthorizeDevelopment(true, 1, 60), Is.True);
            // A second system asking at the same world time cannot use a separate allowance.
            Assert.That(director.TryAuthorizeDevelopment(true, 1, 60), Is.False);
            Assert.That(director.TryAuthorizeDevelopment(true, 1, 119), Is.False);
            Assert.That(director.TryAuthorizeDevelopment(true, 1, 120), Is.True);
        }

        [Test]
        public void DevelopmentsBecomeMoreRestrainedAsPhaseContinues()
        {
            var director = new WorldPacingDirector(60, 240, 1440);
            director.SetScenarioPhase("long-phase", 1, 0);

            Assert.That(director.CurrentIntervalMinutes, Is.EqualTo(60));
            Assert.That(director.TryAuthorizeDevelopment(true, 1, 70), Is.True);
            Assert.That(director.TryAuthorizeDevelopment(true, 1, 140), Is.False);
            Assert.That(director.CurrentIntervalMinutes, Is.EqualTo(78));
            Assert.That(director.TryAuthorizeDevelopment(true, 1, 153), Is.True);
            Assert.That(director.TryAuthorizeDevelopment(true, 2, 0), Is.True);
            Assert.That(director.CurrentIntervalMinutes, Is.EqualTo(240));
            Assert.That(director.TryAuthorizeDevelopment(true, 2, 239), Is.False);
            Assert.That(director.TryAuthorizeDevelopment(true, 2, 240), Is.True);
        }

        [Test]
        public void ChangingPhaseStartsFreshRampWithoutReleasingBacklog()
        {
            var director = new WorldPacingDirector(60, 240, 1440);
            director.SetScenarioPhase("phase-one", 1, 0);
            Assert.That(director.TryAuthorizeDevelopment(true, 2, 0), Is.True);

            Assert.That(director.SetScenarioPhase("phase-two", 5, 0), Is.True);
            Assert.That(director.SetScenarioPhase("phase-two", 5, 30), Is.False);
            Assert.That(director.CurrentIntervalMinutes, Is.EqualTo(64));
            Assert.That(director.TryAuthorizeDevelopment(true, 5, 59), Is.False);
            Assert.That(director.TryAuthorizeDevelopment(true, 5, 68), Is.False);
            Assert.That(director.TryAuthorizeDevelopment(true, 5, 69), Is.True);
        }

        [Test]
        public void LongIdlePeriodAllowsOnlyOneDevelopmentAndDoesNotBankCapacity()
        {
            var director = new WorldPacingDirector(60, 240, 1440);
            director.SetScenarioPhase("opening", 1, 0);

            Assert.That(director.TryAuthorizeDevelopment(true, 20, 0), Is.True);
            Assert.That(director.TryAuthorizeDevelopment(true, 20, 0), Is.False);
            Assert.That(director.TryAuthorizeDevelopment(true, 20, 239), Is.False);
            Assert.That(director.TryAuthorizeDevelopment(true, 20, 240), Is.True);
        }

        [Test]
        public void ClockRewindCannotGrantAnEarlyDevelopment()
        {
            var director = new WorldPacingDirector(60, 60, 600);
            director.SetScenarioPhase("opening", 2, 0);

            Assert.That(director.TryAuthorizeDevelopment(true, 2, 69), Is.True);
            Assert.That(director.TryAuthorizeDevelopment(true, 1, 1439), Is.False);
            Assert.That(director.TryAuthorizeDevelopment(true, 2, 128), Is.False);
            Assert.That(director.TryAuthorizeDevelopment(true, 2, 129), Is.True);
        }

        [Test]
        public void RejectsInvalidPacingConfiguration()
        {
            Assert.Throws<System.ArgumentOutOfRangeException>(
                () => new WorldPacingDirector(0, 60, 100));
            Assert.Throws<System.ArgumentOutOfRangeException>(
                () => new WorldPacingDirector(120, 60, 100));
            Assert.Throws<System.ArgumentOutOfRangeException>(
                () => new WorldPacingDirector(60, 120, 0));
        }
    }
}
