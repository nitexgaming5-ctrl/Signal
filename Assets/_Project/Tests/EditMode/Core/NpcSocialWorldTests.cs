using NUnit.Framework;

namespace Signal.Core.Tests
{
    public sealed class NpcSocialWorldTests
    {
        [Test]
        public void RelationshipChangesAreDirectional()
        {
            var world = new NpcSocialWorld();
            world.RegisterNpc("milo", "Street", 0f, 0f, 8f);
            world.RegisterNpc("rhea", "Street", 1f, 0f, 8f);
            world.SetRelationship("milo", "rhea", new NpcRelationship(0.4f, 0.6f, -0.2f));
            world.AdjustRelationship("milo", "rhea", familiarityDelta: 0.2f, affinityDelta: 0.1f);
            world.AdjustRelationship("rhea", "milo", familiarityDelta: 0.1f);
            Assert.That(world.GetRelationship("milo", "rhea").Familiarity, Is.EqualTo(0.6f).Within(0.0001f));
            Assert.That(world.GetRelationship("milo", "rhea").Trust, Is.EqualTo(0.6f).Within(0.0001f));
            Assert.That(world.GetRelationship("milo", "rhea").Affinity, Is.EqualTo(-0.1f).Within(0.0001f));
            Assert.That(world.GetRelationship("rhea", "milo").Familiarity, Is.EqualTo(0.1f).Within(0.0001f));
            Assert.That(world.GetRelationship("rhea", "milo").Trust, Is.EqualTo(0.5f));
            Assert.Throws<System.ArgumentOutOfRangeException>(() =>
                world.AdjustRelationship("milo", "rhea", trustDelta: float.NaN));
            Assert.Throws<System.ArgumentOutOfRangeException>(() =>
                new NpcRelationship(float.NaN, 0.5f, 0f));
        }
        [Test]
        public void NearbyNpcsMustShareLocation()
        {
            var world = new NpcSocialWorld();
            world.RegisterNpc("milo", "CornerShop", 0f, 0f, 8f);
            world.RegisterNpc("customer", "CornerShop", 3f, 0f, 8f);
            world.RegisterNpc("neighbour", "Apartment", 1f, 0f, 8f);
            world.RegisterNpc("distant", "CornerShop", 20f, 0f, 8f);

            CollectionAssert.AreEqual(new[] { "customer" }, world.GetNearbyNpcIds("milo", 5f));
        }

        [Test]
        public void EventsReachActorsAndNearbyWitnesses()
        {
            var world = new NpcSocialWorld();
            world.RegisterNpc("milo", "Street", 0f, 0f, 8f);
            world.RegisterNpc("customer", "Street", 4f, 0f, 8f);
            world.RegisterNpc("neighbour", "Apartment", 0f, 0f, 8f);
            world.RegisterNpc("distant", "Street", 20f, 0f, 8f);
            world.RegisterNpc("shortSighted", "Street", 4f, 0f, 2f);

            world.PublishEvent(new NpcWorldEvent(
                "delivery-1", "Delivery", "A sealed parcel changed hands.",
                "milo", "Street", 0f, 0f, 5f, 1, 600));


            NpcPerception actorView = world.BuildPerceptionFor("milo", 8f);
            NpcPerception witnessView = world.BuildPerceptionFor("customer", 8f);
            Assert.That(actorView.KnowsEvent("delivery-1"), Is.True);
            Assert.That(witnessView.KnowsEvent("delivery-1"), Is.True);
            Assert.That(witnessView.FindEvent("delivery-1").Origin, Is.EqualTo(NpcKnowledgeOrigin.DirectObservation));
            Assert.That(witnessView.FindEvent("delivery-1").LastSourceNpcId, Is.Empty);
            Assert.That(witnessView.FindEvent("delivery-1").EventLocationId, Is.EqualTo("Street"));
            Assert.That(witnessView.FindEvent("delivery-1").OccurredMinuteOfDay, Is.EqualTo(600));
            Assert.That(world.BuildPerceptionFor("neighbour", 8f).KnowsEvent("delivery-1"), Is.False);
            Assert.That(world.BuildPerceptionFor("distant", 8f).KnowsEvent("delivery-1"), Is.False);
            Assert.That(world.BuildPerceptionFor("shortSighted", 8f).KnowsEvent("delivery-1"), Is.False);
            Assert.That(world.EventCount, Is.EqualTo(1));
        }

        [Test]
        public void SharedKnowledgeUsesListenerTrust()
        {
            var world = new NpcSocialWorld();
            world.RegisterNpc("milo", "Street", 0f, 0f, 8f);
            world.RegisterNpc("trusted", "Apartment", 0f, 0f, 8f);
            world.RegisterNpc("sceptic", "Apartment", 1f, 0f, 8f);

            world.PublishEvent(new NpcWorldEvent(
                "argument-1", "Argument", "Milo saw two neighbours arguing.",
                "milo", "Street", 0f, 0f, 0f, 1, 900));

            world.SetRelationship("trusted", "milo", new NpcRelationship(0.8f, 1f, 0.2f));
            world.SetRelationship("sceptic", "milo", new NpcRelationship(0.2f, 0f, -0.4f));

            Assert.That(world.ShareKnowledge("milo", "trusted", "argument-1", 1, 910), Is.True);
            Assert.That(world.ShareKnowledge("milo", "sceptic", "argument-1", 1, 910), Is.True);

            NpcKnowledge trustedBelief = world.BuildPerceptionFor("trusted", 8f).FindEvent("argument-1");
            NpcKnowledge scepticBelief = world.BuildPerceptionFor("sceptic", 8f).FindEvent("argument-1");

            Assert.That(trustedBelief.Origin, Is.EqualTo(NpcKnowledgeOrigin.ToldByAnotherNpc));
            Assert.That(trustedBelief.LastSourceNpcId, Is.EqualTo("milo"));
            Assert.That(trustedBelief.Confidence, Is.EqualTo(0.85f).Within(0.0001f));
            Assert.That(scepticBelief.Confidence, Is.EqualTo(0.35f).Within(0.0001f));
            Assert.That(world.ShareKnowledge("milo", "trusted", "argument-1", 1, 920), Is.False);
        }

        [Test]
        public void MovingNpcsUpdatesLocalAwareness()
        {
            var world = new NpcSocialWorld();
            world.RegisterNpc("milo", "Street", 0f, 0f, 8f);
            world.RegisterNpc("customer", "Street", 2f, 0f, 8f);

            Assert.That(world.GetNearbyNpcIds("milo", 3f).Count, Is.EqualTo(1));
            world.MoveNpc("customer", "CornerShop", 2f, 0f);
            Assert.That(world.GetNearbyNpcIds("milo", 3f).Count, Is.EqualTo(0));
        }

        [Test]
        public void PerceptionUsesPersonalKnowledge()
        {
            var world = new NpcSocialWorld();
            world.RegisterNpc("observer", "Street", 0f, 0f, 8f);
            world.RegisterNpc("witness", "Street", 1f, 0f, 8f);
            world.RegisterNpc("stranger", "Street", 2f, 0f, 8f);

            world.SetRelationship("observer", "witness", new NpcRelationship(0.7f, 0.9f, 0.2f));

            world.PublishEvent(new NpcWorldEvent(
                "private-observation", "Exchange", "A quiet exchange happened.",
                "witness", "Street", 1f, 0f, 0f, 1, 700));

            NpcPerception observerView = world.BuildPerceptionFor("observer", 5f);
            NpcPerception witnessView = world.BuildPerceptionFor("witness", 5f);

            CollectionAssert.Contains(observerView.NearbyNpcIds, "witness");
            CollectionAssert.Contains(observerView.NearbyNpcIds, "stranger");
            Assert.That(observerView.NearbyPeople.Count, Is.EqualTo(2));
            NpcPerceivedPerson perceivedWitness = null;
            for (int i = 0; i < observerView.NearbyPeople.Count; i++)
                if (observerView.NearbyPeople[i].NpcId == "witness")
                    perceivedWitness = observerView.NearbyPeople[i];
            Assert.That(perceivedWitness, Is.Not.Null);
            Assert.That(perceivedWitness.Relationship.Trust, Is.EqualTo(0.9f).Within(0.0001f));
            Assert.That(observerView.KnowsEvent("private-observation"), Is.False);
            Assert.That(witnessView.KnowsEvent("private-observation"), Is.True);
            Assert.That(witnessView.KnownEvents[0].EventLocationId, Is.EqualTo("Street"));
            Assert.That(witnessView.KnownEvents[0].OccurredMinuteOfDay, Is.EqualTo(700));
            Assert.That(observerView.KnownEvents, Is.Empty);
        }

        [Test]
        public void WitnessKnowledgeCanBeRestored()
        {
            var world = new NpcSocialWorld();
            world.RegisterNpc("rhea", "Apartment", 0f, 0f, 8f);
            world.RegisterNpc("milo", "Apartment", 1f, 0f, 8f);

            var occurrence = new NpcWorldEvent(
                "crime-1", "crime-observed", "Someone took something from the wardrobe.",
                null, "Apartment", 0f, 0f, 8f, 1, 600);

            Assert.That(world.PublishEventToNpc(occurrence, "rhea"), Is.True);
            Assert.That(world.BuildPerceptionFor("rhea", 8f).KnowsEvent("crime-1"), Is.True);
            Assert.That(world.BuildPerceptionFor("milo", 8f).KnowsEvent("crime-1"), Is.False);
            Assert.That(world.PublishEventToNpc(occurrence, "rhea"), Is.True);
            Assert.That(world.EventCount, Is.EqualTo(1));
        }

        [Test]
        public void DuplicateNpcAndEventIdsAreRejected()
        {
            var world = new NpcSocialWorld();
            world.RegisterNpc("milo", "Street", 0f, 0f, 8f);
            Assert.Throws<System.InvalidOperationException>(() =>
                world.RegisterNpc("milo", "Street", 1f, 0f, 8f));

            var occurrence = new NpcWorldEvent("event-1", "Noise", "A noise.", null, "Street", 0f, 0f, 2f, 1, 10);
            world.PublishEvent(occurrence);
            Assert.Throws<System.InvalidOperationException>(() => world.PublishEvent(occurrence));
        }
    }
}
