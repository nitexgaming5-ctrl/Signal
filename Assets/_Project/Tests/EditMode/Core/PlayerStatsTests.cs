using System;
using NUnit.Framework;

namespace Signal.Core.Tests
{
    public sealed class PlayerStatsTests
    {
        [Test]
        public void DefaultsAreZeroMoneyAndFullHealth()
        {
            var stats = new PlayerStats();

            Assert.That(stats.Money, Is.EqualTo(0));
            Assert.That(stats.Health, Is.EqualTo(100));
        }

        [Test]
        public void Earn_AddsMoney()
        {
            var stats = new PlayerStats();

            stats.Earn(12);

            Assert.That(stats.Money, Is.EqualTo(12));
        }

        [TestCase(0)]
        [TestCase(-5)]
        public void Earn_IgnoresZeroAndNegative(int amount)
        {
            var stats = new PlayerStats(money: 10);

            stats.Earn(amount);

            Assert.That(stats.Money, Is.EqualTo(10));
        }

        [Test]
        public void TrySpend_SucceedsWithEnoughMoney()
        {
            var stats = new PlayerStats(money: 20);

            bool ok = stats.TrySpend(12);

            Assert.That(ok, Is.True);
            Assert.That(stats.Money, Is.EqualTo(8));
        }

        [Test]
        public void TrySpend_FailsWithoutChangingMoney()
        {
            var stats = new PlayerStats(money: 5);

            bool ok = stats.TrySpend(12);

            Assert.That(ok, Is.False);
            Assert.That(stats.Money, Is.EqualTo(5));
        }

        [Test]
        public void MoneyCompatibilityApiUsesTheCleanBankCardAccount()
        {
            var stats = new PlayerStats(100);
            Assert.That(stats.CleanMoney.Balance, Is.EqualTo(100));
            Assert.That(stats.TrySpend(25), Is.True);
            Assert.That(stats.CleanMoney.Balance, Is.EqualTo(75));
            stats.Earn(10);
            Assert.That(stats.Money, Is.EqualTo(85));
        }

        [Test]
        public void Damage_ReducesHealth()
        {
            var stats = new PlayerStats();

            stats.Damage(30);

            Assert.That(stats.Health, Is.EqualTo(70));
        }

        [Test]
        public void Damage_ClampsAtZero()
        {
            var stats = new PlayerStats();

            stats.Damage(500);

            Assert.That(stats.Health, Is.EqualTo(0));
        }

        [TestCase(0)]
        [TestCase(-3)]
        public void Damage_IgnoresZeroAndNegative(int amount)
        {
            var stats = new PlayerStats(health: 50);

            stats.Damage(amount);

            Assert.That(stats.Health, Is.EqualTo(50));
        }

        [Test]
        public void Constructor_ClampsHealthToRange()
        {
            Assert.That(new PlayerStats(health: 250).Health, Is.EqualTo(100));
            Assert.That(new PlayerStats(health: -10).Health, Is.EqualTo(0));
        }

        [Test]
        public void Changed_FiresOncePerRealChange()
        {
            var stats = new PlayerStats(money: 10);
            int count = 0;
            stats.Changed += _ => count++;

            stats.Earn(5);
            stats.TrySpend(999);
            stats.Damage(0);
            stats.Damage(10);

            Assert.That(count, Is.EqualTo(2));
        }

        [Test]
        public void RestoreSetsBothValuesAndFiresOnce()
        {
            var stats = new PlayerStats();
            int fired = 0;
            stats.Changed += _ => fired++;

            stats.Restore(40, 55);

            Assert.That(stats.Money, Is.EqualTo(40));
            Assert.That(stats.Health, Is.EqualTo(55));
            Assert.That(fired, Is.EqualTo(1));
        }

        [TestCase(-5, 150, 0, 100)]
        [TestCase(7, -3, 7, 0)]
        public void RestoreClampsValues(int money, int health, int expectedMoney, int expectedHealth)
        {
            var stats = new PlayerStats();

            stats.Restore(money, health);

            Assert.That(stats.Money, Is.EqualTo(expectedMoney));
            Assert.That(stats.Health, Is.EqualTo(expectedHealth));
        }

        [Test]
        public void RestoreWithTheSameValuesFiresNothing()
        {
            var stats = new PlayerStats(10, 80);
            int fired = 0;
            stats.Changed += _ => fired++;

            stats.Restore(10, 80);

            Assert.That(fired, Is.EqualTo(0));
        }
    }
}
