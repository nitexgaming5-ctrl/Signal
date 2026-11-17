using NUnit.Framework;

namespace Signal.Core.Tests
{
    public sealed class NpcSocialEncounterSystemTests
    {
        [Test]
        public void ScheduledEncounterBuildsFamiliarityAndSharesAtMostOneFact()
        {
            var world = new NpcSocialWorld();
            world.RegisterNpc("resident-a", "Street", 0f, 0f, 8f);
            world.RegisterNpc("resident-b", "Street", 1f, 0f, 8f);
            world.RegisterNpc("distant", "Street", 20f, 0f, 8f);
            world.PublishEvent(new NpcWorldEvent(
                "delivery-a", "Delivery", "A sealed parcel arrived.", "resident-a",
                "Street", 0f, 0f, 0f, 1, 600));
            world.PublishEvent(new NpcWorldEvent(
                "repair-b", "Repair", "A window was repaired.", "resident-b",
                "Street", 1f, 0f, 0f, 1, 610));

            var encounters = new NpcSocialEncounterSystem();
            int created = encounters.AdvanceTo(world,
                new[] { "resident-a", "resident-b", "distant" }, 1, 700);

            Assert.That(created, Is.EqualTo(1));
            Assert.That(encounters.EncounterCount, Is.EqualTo(1));

            NpcRelationship aToB = world.GetRelationship("resident-a", "resident-b");
            NpcRelationship bToA = world.GetRelationship("resident-b", "resident-a");
            Assert.That(aToB.Familiarity, Is.EqualTo(0.01f).Within(0.0001f));
            Assert.That(bToA.Familiarity, Is.EqualTo(0.01f).Within(0.0001f));
            Assert.That(aToB.Trust, Is.EqualTo(0.5f));
            Assert.That(bToA.Trust, Is.EqualTo(0.5f));
            Assert.That(aToB.Affinity, Is.EqualTo(0f));
            Assert.That(bToA.Affinity, Is.EqualTo(0f));

            NpcPerception aView = world.BuildPerceptionFor("resident-a", 8f);
            NpcPerception bView = world.BuildPerceptionFor("resident-b", 8f);
            int sharedFacts = (aView.KnowsEvent("repair-b") ? 1 : 0) +
                (bView.KnowsEvent("delivery-a") ? 1 : 0);
            Assert.That(sharedFacts, Is.EqualTo(1));
            Assert.That(world.EventCount, Is.EqualTo(3));
        }

        [Test]
        public void EncounterCooldownAndSinglePartnerLimitKeepSimulationControlled()
        {
            var world = new NpcSocialWorld();
            world.RegisterNpc("a", "Street", 0f, 0f, 8f);
            world.RegisterNpc("b", "Street", 1f, 0f, 8f);
            world.RegisterNpc("c", "Street", 2f, 0f, 8f);
            var encounters = new NpcSocialEncounterSystem(pairCooldownMinutes: 720);

            Assert.That(encounters.AdvanceTo(world, new[] { "a", "b", "c" }, 1, 700), Is.EqualTo(1));
            Assert.That(encounters.AdvanceTo(world, new[] { "a", "b", "c" }, 1, 701), Is.EqualTo(0));
            Assert.That(encounters.AdvanceTo(world, new[] { "a", "b", "c" }, 1, 1419), Is.EqualTo(0));
            Assert.That(encounters.AdvanceTo(world, new[] { "a", "b", "c" }, 2, 0), Is.EqualTo(1));
            Assert.That(encounters.EncounterCount, Is.EqualTo(2));
        }

        [Test]
        public void ResidentsCannotEncounterAcrossLocationsOrOutsideMutualAwareness()
        {
            var world = new NpcSocialWorld();
            world.RegisterNpc("a", "Street", 0f, 0f, 8f);
            world.RegisterNpc("b", "Apartment", 1f, 0f, 8f);
            world.RegisterNpc("c", "Street", 20f, 0f, 1f);
            var encounters = new NpcSocialEncounterSystem();

            Assert.That(encounters.AdvanceTo(world, new[] { "a", "b", "c" }, 1, 700), Is.EqualTo(0));
            Assert.That(encounters.EncounterCount, Is.EqualTo(0));
        }
    }
}
