using NUnit.Framework;

namespace Signal.Core.Tests
{
    public sealed class GameClockTests
    {
        [Test]
        public void StartsOnDayOneAtEightInTheMorning()
        {
            var clock = new GameClock();

            Assert.That(clock.Day, Is.EqualTo(1));
            Assert.That(clock.MinuteOfDay, Is.EqualTo(480));
        }

        [Test]
        public void AdvanceWithinADayKeepsTheDay()
        {
            var clock = new GameClock();
            int days = 0;
            clock.DayStarted += _ => days++;

            clock.Advance(90);

            Assert.That(clock.Day, Is.EqualTo(1));
            Assert.That(clock.Hour, Is.EqualTo(9));
            Assert.That(clock.Minute, Is.EqualTo(30));
            Assert.That(days, Is.EqualTo(0));
        }

        [Test]
        public void AdvancePastMidnightStartsANewDay()
        {
            var clock = new GameClock(1, 23 * 60 + 30);
            int days = 0;
            clock.DayStarted += _ => days++;

            clock.Advance(60);

            Assert.That(clock.Day, Is.EqualTo(2));
            Assert.That(clock.MinuteOfDay, Is.EqualTo(30));
            Assert.That(days, Is.EqualTo(1));
        }

        [Test]
        public void AdvanceSeveralDaysFiresDayStartedForEach()
        {
            var clock = new GameClock();
            int days = 0;
            clock.DayStarted += _ => days++;

            clock.Advance(GameClock.MinutesPerDay * 2 + 10);

            Assert.That(clock.Day, Is.EqualTo(3));
            Assert.That(clock.MinuteOfDay, Is.EqualTo(490));
            Assert.That(days, Is.EqualTo(2));
        }

        [TestCase(0)]
        [TestCase(-5)]
        public void AdvanceByZeroOrLessDoesNothing(int minutes)
        {
            var clock = new GameClock();
            int fired = 0;
            clock.Changed += _ => fired++;

            clock.Advance(minutes);

            Assert.That(clock.MinuteOfDay, Is.EqualTo(480));
            Assert.That(fired, Is.EqualTo(0));
        }

        [Test]
        public void SleepingAtNightWakesTheNextMorning()
        {
            var clock = new GameClock(4, 22 * 60);
            int days = 0;
            clock.DayStarted += _ => days++;

            clock.Sleep();

            Assert.That(clock.Day, Is.EqualTo(5));
            Assert.That(clock.MinuteOfDay, Is.EqualTo(GameClock.DefaultWakeMinute));
            Assert.That(days, Is.EqualTo(1));
        }

        [Test]
        public void SleepingAfterMidnightWakesTheSameDay()
        {
            var clock = new GameClock(4, 60);
            int days = 0;
            clock.DayStarted += _ => days++;

            clock.Sleep();

            Assert.That(clock.Day, Is.EqualTo(4));
            Assert.That(clock.MinuteOfDay, Is.EqualTo(GameClock.DefaultWakeMinute));
            Assert.That(days, Is.EqualTo(0));
        }

        [Test]
        public void SleepingExactlyAtWakeTimeSkipsAFullDay()
        {
            var clock = new GameClock(2, GameClock.DefaultWakeMinute);

            clock.Sleep();

            Assert.That(clock.Day, Is.EqualTo(3));
            Assert.That(clock.MinuteOfDay, Is.EqualTo(GameClock.DefaultWakeMinute));
        }

        [Test]
        public void RestoreSetsDayAndTimeWithoutFiringDayStarted()
        {
            var clock = new GameClock();
            int days = 0;
            clock.DayStarted += _ => days++;

            clock.Restore(9, 125);

            Assert.That(clock.Day, Is.EqualTo(9));
            Assert.That(clock.MinuteOfDay, Is.EqualTo(125));
            Assert.That(days, Is.EqualTo(0));
        }

        [Test]
        public void ConstructorClampsOutOfRangeValues()
        {
            var clock = new GameClock(0, 5000);

            Assert.That(clock.Day, Is.EqualTo(1));
            Assert.That(clock.MinuteOfDay, Is.EqualTo(GameClock.MinutesPerDay - 1));
        }

        [Test]
        public void PauseStateCanBeToggledWithoutChangingClockTime()
        {
            var clock = new GameClock();
            int changed = 0;
            clock.Changed += _ => changed++;

            clock.SetPaused(true);
            Assert.That(clock.IsPaused, Is.True);
            Assert.That(clock.MinuteOfDay, Is.EqualTo(480));

            clock.SetPaused(false);
            Assert.That(clock.IsPaused, Is.False);
            Assert.That(clock.MinuteOfDay, Is.EqualTo(480));
            Assert.That(changed, Is.EqualTo(0));
        }

        [Test]
        public void LabelShowsDayAndTime()
        {
            var clock = new GameClock(3, 7 * 60 + 5);

            Assert.That(clock.Label, Is.EqualTo("Day 3 07:05"));
        }
    }
}
