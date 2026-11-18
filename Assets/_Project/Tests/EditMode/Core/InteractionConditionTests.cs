using NUnit.Framework;

namespace Signal.Core.Tests
{
    public sealed class InteractionConditionTests
    {
        [Test]
        public void RequiresAuthoredActivityRelationshipAndDiscoveryTogether()
        {
            var ledger = new PlayerDiscoveryLedger();
            ledger.Record(new PlayerDiscoverySignal("routine-1", "milo",
                DiscoverySignalType.RoutineObservation, "milo", "Left after closing", 1f, 1, 800));
            var conditions = new InteractionConditionSet(NpcActivity.Rest,
                minimumFamiliarity: 0.4f, minimumTrust: 0.6f,
                requiredSignalTypes: new[] { DiscoverySignalType.RoutineObservation, DiscoverySignalType.Conversation });

            var context = new InteractionContext(NpcActivity.Rest,
                new NpcRelationship(0.5f, 0.7f, 0.1f), ledger);

            Assert.That(conditions.IsSatisfiedBy(context, "milo"), Is.False);

            ledger.Record(new PlayerDiscoverySignal("conversation-1", "milo",
                DiscoverySignalType.Conversation, "milo", "An evasive answer", 0.8f, 1, 810));

            Assert.That(conditions.IsSatisfiedBy(context, "milo"), Is.True);
        }

        [Test]
        public void WrongActivityOrInsufficientRelationshipKeepsInteractionUnavailable()
        {
            var ledger = new PlayerDiscoveryLedger();
            ledger.Record(new PlayerDiscoverySignal("evidence-1", "milo",
                DiscoverySignalType.PhysicalEvidence, "parcel-slip", "A marked slip", 1f, 1, 500));
            var conditions = new InteractionConditionSet(NpcActivity.Rest,
                minimumFamiliarity: 0.5f, requiredSignalTypes: new[] { DiscoverySignalType.PhysicalEvidence });

            Assert.That(conditions.IsSatisfiedBy(new InteractionContext(NpcActivity.Work,
                new NpcRelationship(0.8f, 0.5f, 0f), ledger), "milo"), Is.False);
            Assert.That(conditions.IsSatisfiedBy(new InteractionContext(NpcActivity.Rest,
                new NpcRelationship(0.2f, 0.5f, 0f), ledger), "milo"), Is.False);
        }

        [Test]
        public void RelationshipAndSignalRequirementsAreSubjectSpecific()
        {
            var ledger = new PlayerDiscoveryLedger();
            ledger.Record(new PlayerDiscoverySignal("rhea-rumour", "rhea",
                DiscoverySignalType.Rumour, "rhea", "A rumour", 0.8f, 1, 100));
            var conditions = new InteractionConditionSet(requiredSignalTypes:
                new[] { DiscoverySignalType.Rumour });

            Assert.That(conditions.IsSatisfiedBy(new InteractionContext(NpcActivity.Rest,
                NpcRelationship.Neutral, ledger), "milo"), Is.False);
        }

        [Test]
        public void FlexibleMinimumDistinctSignalTypesAcceptsDifferentDiscoveryCombinations()
        {
            var ledger = new PlayerDiscoveryLedger();
            ledger.Record(new PlayerDiscoverySignal("milo-observation", "milo",
                DiscoverySignalType.RoutineObservation, "street", "Milo left after closing.", 1f, 1, 1100));
            ledger.Record(new PlayerDiscoverySignal("milo-rumour", "milo",
                DiscoverySignalType.Rumour, "rhea", "A neighbour mentioned a late delivery.", 0.4f, 1, 1110));
            var conditions = new InteractionConditionSet(minimumDistinctSignalTypes: 2);
            var context = new InteractionContext(NpcActivity.Socialize, NpcRelationship.Neutral, ledger);

            Assert.That(conditions.IsSatisfiedBy(context, "milo"), Is.True);
            Assert.That(conditions.IsSatisfiedBy(context, "rhea"), Is.False);
        }
    }
}
