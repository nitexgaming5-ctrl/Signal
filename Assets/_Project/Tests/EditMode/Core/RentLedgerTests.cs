using System;
using NUnit.Framework;

namespace Signal.Core.Tests
{
    public sealed class RentLedgerTests
    {
        [Test]
        public void NothingIsDueOnOtherDays()
        {
            var rent = new RentLedger(50, 7);
            var stats = new PlayerStats(100);

            for (int day = 1; day <= 6; day++)
                Assert.That(rent.Settle(day, stats), Is.EqualTo(RentOutcome.NotDue));

            Assert.That(stats.Money, Is.EqualTo(100));
        }

        [Test]
        public void RentIsPaidWhenDueAndAffordable()
        {
            var rent = new RentLedger(50, 7);
            var stats = new PlayerStats(80);

            RentOutcome outcome = rent.Settle(7, stats);

            Assert.That(outcome, Is.EqualTo(RentOutcome.Paid));
            Assert.That(stats.Money, Is.EqualTo(30));
            Assert.That(rent.MissedPayments, Is.EqualTo(0));
        }

        [Test]
        public void MissedRentIsCountedAndMoneyIsUntouched()
        {
            var rent = new RentLedger(50, 7);
            var stats = new PlayerStats(10);

            Assert.That(rent.Settle(7, stats), Is.EqualTo(RentOutcome.Missed));
            Assert.That(rent.Settle(14, stats), Is.EqualTo(RentOutcome.Missed));

            Assert.That(rent.MissedPayments, Is.EqualTo(2));
            Assert.That(stats.Money, Is.EqualTo(10));
        }

        [Test]
        public void FreeRentIsAlwaysPaid()
        {
            var rent = new RentLedger(0, 3);

            Assert.That(rent.Settle(3, new PlayerStats()), Is.EqualTo(RentOutcome.Paid));
        }

        [TestCase(-1, 7)]
        [TestCase(50, 0)]
        public void ConstructorRejectsInvalidValues(int amount, int intervalDays)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new RentLedger(amount, intervalDays));
        }

        [Test]
        public void DaysBelowOneAreNeverDue()
        {
            var rent = new RentLedger(50, 7);

            Assert.That(rent.IsDue(0), Is.False);
            Assert.That(rent.IsDue(-7), Is.False);
        }

        [Test]
        public void RestoreSetsMissedPaymentsAndIgnoresNegatives()
        {
            var rent = new RentLedger(50, 7);

            rent.Restore(3);
            Assert.That(rent.MissedPayments, Is.EqualTo(3));

            rent.Restore(-4);
            Assert.That(rent.MissedPayments, Is.EqualTo(0));
        }
    }
}
