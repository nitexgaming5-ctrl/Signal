using NUnit.Framework;

namespace Signal.Core.Tests
{
    public sealed class NpcUtilityBrainTests
    {
        [Test]
        public void DefaultDayWorkerUsesWorkRestSocialAndHomeWindows()
        {
            var brain = new NpcUtilityBrain();

            Assert.That(brain.AdvanceTo(1, 8 * 60), Is.EqualTo(NpcActivity.Work));
            Assert.That(brain.AdvanceTo(1, 12 * 60), Is.EqualTo(NpcActivity.Rest));
            Assert.That(brain.AdvanceTo(1, 13 * 60), Is.EqualTo(NpcActivity.Work));
            Assert.That(brain.AdvanceTo(1, 19 * 60), Is.EqualTo(NpcActivity.Socialize));
            Assert.That(brain.AdvanceTo(1, 22 * 60), Is.EqualTo(NpcActivity.Home));
        }

        [Test]
        public void FamiliarNearbyPeopleAndPersonallyKnownEventsCanInfluenceSocialising()
        {
            var aloneWorld = new NpcSocialWorld();
            aloneWorld.RegisterNpc("observer", "Street", 0f, 0f, 8f);

            var socialWorld = new NpcSocialWorld();
            socialWorld.RegisterNpc("observer", "Street", 0f, 0f, 8f);
            socialWorld.RegisterNpc("friend-a", "Street", 1f, 0f, 8f);
            socialWorld.RegisterNpc("friend-b", "Street", 2f, 0f, 8f);
            socialWorld.RegisterNpc("friend-c", "Street", 3f, 0f, 8f);
            socialWorld.SetRelationship("observer", "friend-a", new NpcRelationship(1f, 0.8f, 1f));
            socialWorld.SetRelationship("observer", "friend-b", new NpcRelationship(1f, 0.8f, 1f));
            socialWorld.SetRelationship("observer", "friend-c", new NpcRelationship(1f, 0.8f, 1f));
            socialWorld.PublishEvent(new NpcWorldEvent(
                "known-delivery", "Delivery", "A parcel arrived.", null,
                "Street", 0f, 0f, 0f, 1, 590));

            var aloneBrain = new NpcUtilityBrain();
            var socialBrain = new NpcUtilityBrain();
            aloneBrain.AdvanceTo(1, 599);
            socialBrain.AdvanceTo(1, 599);

            NpcActivity alone = aloneBrain.AdvanceTo(
                1, 600, aloneWorld.BuildPerceptionFor("observer", 8f));
            NpcActivity withFriendsAndKnowledge = socialBrain.AdvanceTo(
                1, 600, socialWorld.BuildPerceptionFor("observer", 8f));

            Assert.That(alone, Is.EqualTo(NpcActivity.Work));
            Assert.That(withFriendsAndKnowledge, Is.EqualTo(NpcActivity.Socialize));
        }

        [Test]
        public void AuthoredRoutineProfileCanChangeWorkHoursWithoutChangingBrainCode()
        {
            var profile = new NpcUtilityProfile
            {
                WorkStartMinute = 600,
                WorkEndMinute = 1080
            };
            var brain = new NpcUtilityBrain(profile);

            Assert.That(brain.AdvanceTo(1, 8 * 60), Is.Not.EqualTo(NpcActivity.Work));
            Assert.That(brain.AdvanceTo(1, 10 * 60), Is.EqualTo(NpcActivity.Work));
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