using NUnit.Framework;

namespace Signal.Core.Tests
{
    public sealed class CriminalProgressionTrackTests
    {
        [Test]
        public void AwardsAccumulateAcrossAllCrimeCategoriesWithoutDailyDecay()
        {
            var progression = new CriminalProgressionTrack();

            progression.Award(1);
            progression.Award(2);

            Assert.That(progression.Points, Is.EqualTo(3));
        }

        [Test]
        public void NonPositiveAwardsAreIgnoredAndRestoreClampsBelowZero()
        {
            var progression = new CriminalProgressionTrack();
            progression.Award(4);
            progression.Award(0);
            progression.Award(-3);
            progression.Restore(-10);

            Assert.That(progression.Points, Is.Zero);
        }

        [Test]
        public void AwardSaturatesInsteadOfOverflowing()
        {
            var progression = new CriminalProgressionTrack();
            progression.Restore(int.MaxValue - 1);
            progression.Award(5);

            Assert.That(progression.Points, Is.EqualTo(int.MaxValue));
        }
    }
}
