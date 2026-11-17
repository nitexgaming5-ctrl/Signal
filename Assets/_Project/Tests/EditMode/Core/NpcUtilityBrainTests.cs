using NUnit.Framework;

namespace Signal.Core.Tests
{
    public sealed class NpcUtilityBrainTests
    {
        [Test]
        public void BakerWorksDuringTheDayTakesABreakAndSocializesAfterClosing()
        {
            var brain = new NpcUtilityBrain();

            Assert.That(brain.AdvanceTo(1, 8 * 60), Is.EqualTo(NpcActivity.Work));
            Assert.That(brain.AdvanceTo(1, 12 * 60), Is.EqualTo(NpcActivity.Rest));
            Assert.That(brain.AdvanceTo(1, 13 * 60), Is.EqualTo(NpcActivity.Work));
            Assert.That(brain.AdvanceTo(1, 19 * 60), Is.EqualTo(NpcActivity.Socialize));
            Assert.That(brain.AdvanceTo(1, 22 * 60), Is.EqualTo(NpcActivity.Home));
        }

        [Test]
        public void SameWorldTimeProducesSameActivity()
        {
            var first = new NpcUtilityBrain();
            var second = new NpcUtilityBrain();

            NpcActivity firstActivity = first.AdvanceTo(3, 19 * 60);
            NpcActivity secondActivity = second.AdvanceTo(3, 19 * 60);

            Assert.That(firstActivity, Is.EqualTo(secondActivity));
            Assert.That(first.Energy, Is.EqualTo(second.Energy).Within(0.0001f));
            Assert.That(first.Social, Is.EqualTo(second.Social).Within(0.0001f));
        }

        [Test]
        public void ClockRewindRebuildsNpcStateWithoutNegativeNeeds()
        {
            var brain = new NpcUtilityBrain();
            brain.AdvanceTo(4, 22 * 60);

            NpcActivity rewound = brain.AdvanceTo(2, 8 * 60);

            Assert.That(rewound, Is.EqualTo(NpcActivity.Work));
            Assert.That(brain.Energy, Is.InRange(0f, 1f));
            Assert.That(brain.Social, Is.InRange(0f, 1f));
        }
    }
}