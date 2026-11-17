using System;
using NUnit.Framework;

namespace Signal.Core.Tests
{
    public sealed class HeatMeterTests
    {
        [Test]
        public void StartsAtZero()
        {
            Assert.That(new HeatMeter().Heat, Is.EqualTo(0));
        }

        [Test]
        public void AddRaisesHeatAndClampsAtTheMaximum()
        {
            var heat = new HeatMeter();

            heat.Add(60);
            heat.Add(60);

            Assert.That(heat.Heat, Is.EqualTo(HeatMeter.MaxHeat));
        }

        [Test]
        public void CoolLowersHeatAndClampsAtZero()
        {
            var heat = new HeatMeter();
            heat.Add(20);

            heat.Cool(50);

            Assert.That(heat.Heat, Is.EqualTo(0));
        }

        [Test]
        public void NewDayCoolsByTheConfiguredAmount()
        {
            var heat = new HeatMeter(10);
            heat.Add(25);

            heat.NewDay();
            Assert.That(heat.Heat, Is.EqualTo(15));

            heat.NewDay();
            heat.NewDay();
            Assert.That(heat.Heat, Is.EqualTo(0));
        }

        [TestCase(0)]
        [TestCase(-4)]
        public void AddingZeroOrLessChangesNothing(int amount)
        {
            var heat = new HeatMeter();
            heat.Add(10);
            int fired = 0;
            heat.Changed += _ => fired++;

            heat.Add(amount);

            Assert.That(heat.Heat, Is.EqualTo(10));
            Assert.That(fired, Is.EqualTo(0));
        }

        [Test]
        public void ChangedFiresOnlyForRealChanges()
        {
            var heat = new HeatMeter();
            int fired = 0;
            heat.Changed += _ => fired++;

            heat.Add(10);
            heat.Cool(0);
            heat.Restore(10);
            heat.Restore(30);

            Assert.That(fired, Is.EqualTo(2));
        }

        [Test]
        public void RestoreClampsToTheValidRange()
        {
            var heat = new HeatMeter();

            heat.Restore(500);
            Assert.That(heat.Heat, Is.EqualTo(HeatMeter.MaxHeat));

            heat.Restore(-5);
            Assert.That(heat.Heat, Is.EqualTo(0));
        }

        [Test]
        public void ConstructorRejectsNegativeDecay()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new HeatMeter(-1));
        }
    }
}
