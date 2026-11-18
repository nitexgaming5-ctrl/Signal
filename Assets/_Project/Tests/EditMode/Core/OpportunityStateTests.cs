using System;
using NUnit.Framework;

namespace Signal.Core.Tests
{
    public sealed class OpportunityStateTests
    {
        [Test]
        public void OfferRequiresMatchingDefinitionAvailableStateAndContext()
        {
            var conditions = new InteractionConditionSet(NpcActivity.Rest, minimumFamiliarity: 0.3f);
            var definition = new OpportunityDefinition("milo-parcel", "milo", conditions);
            var state = new OpportunityState("milo-parcel");
            var context = new InteractionContext(NpcActivity.Rest,
                new NpcRelationship(0.5f, 0.5f, 0f), new PlayerDiscoveryLedger());

            Assert.That(definition.CanOffer(state, context), Is.False);
            Assert.That(state.MakeAvailable(), Is.True);
            Assert.That(definition.CanOffer(state, context), Is.True);
            Assert.That(new OpportunityDefinition("other", "milo", conditions).CanOffer(state, context), Is.False);
        }

        [Test]
        public void DecliningLeavesOpportunityAvailableAndOnlyRecordsTheChoice()
        {
            var state = AvailableState();

            Assert.That(state.RecordDecline(2, 600), Is.True);
            Assert.That(state.Phase, Is.EqualTo(OpportunityPhase.Available));
            Assert.That(state.DeclineCount, Is.EqualTo(1));
            Assert.That(state.LastDeclinedDay, Is.EqualTo(2));
            Assert.That(state.LastDeclinedMinuteOfDay, Is.EqualTo(600));
        }

        [Test]
        public void AcceptingAndReturningParcelReleasesCommitmentWithoutResolvingOpportunity()
        {
            var state = AvailableState();
            Guid parcelId = Guid.NewGuid();

            Assert.That(state.TryAccept(parcelId, 2, 610), Is.True);
            Assert.That(state.Phase, Is.EqualTo(OpportunityPhase.Accepted));
            Assert.That(state.ParcelInstanceId, Is.EqualTo(parcelId));
            Assert.That(state.TryReturnParcel(2, 620), Is.True);
            Assert.That(state.Phase, Is.EqualTo(OpportunityPhase.Available));
            Assert.That(state.AcceptanceCount, Is.EqualTo(1));
            Assert.That(state.Resolution, Is.EqualTo(OpportunityResolution.None));
        }

        [Test]
        public void DeliveredOpportunityResolvesExactlyOnce()
        {
            var state = AvailableState();
            state.TryAccept(Guid.NewGuid(), 1, 500);

            Assert.That(state.TryResolveDelivered(1, 560), Is.True);
            Assert.That(state.Phase, Is.EqualTo(OpportunityPhase.Resolved));
            Assert.That(state.Resolution, Is.EqualTo(OpportunityResolution.Delivered));
            Assert.That(state.TryResolveDelivered(1, 570), Is.False);
            Assert.That(state.TryAccept(Guid.NewGuid(), 1, 580), Is.False);
        }

        [Test]
        public void ExpiringAvailableOpportunityDoesNotCreateQueuedBacklog()
        {
            var state = AvailableState();

            Assert.That(state.TryExpire(), Is.True);
            Assert.That(state.Phase, Is.EqualTo(OpportunityPhase.Dormant));
            Assert.That(state.TryExpire(), Is.False);
            Assert.That(state.TryAccept(Guid.NewGuid(), 1, 600), Is.False);
        }

        [Test]
        public void InvalidTransitionsAndTimesAreRejected()
        {
            var state = new OpportunityState("milo-parcel");

            Assert.That(state.RecordDecline(1, 0), Is.False);
            Assert.That(state.TryReturnParcel(1, 0), Is.False);
            Assert.That(state.TryResolveDelivered(1, 0), Is.False);
            Assert.That(state.TryAccept(Guid.Empty, 1, 0), Is.False);
            Assert.Throws<ArgumentOutOfRangeException>(() => { state.MakeAvailable(); state.RecordDecline(0, 0); });
        }

        private static OpportunityState AvailableState()
        {
            var state = new OpportunityState("milo-parcel");
            state.MakeAvailable();
            return state;
        }

        [Test]
        public void RestoresOnlyConsistentOpportunitySnapshots()
        {
            Guid parcelId = Guid.NewGuid();

            Assert.That(OpportunityState.TryRestore("milo-parcel", OpportunityPhase.Accepted,
                OpportunityResolution.None, parcelId, 1, 0, 2, 610, 0, 0, 0, 0,
                out OpportunityState accepted), Is.True);
            Assert.That(accepted.ParcelInstanceId, Is.EqualTo(parcelId));
            Assert.That(accepted.Phase, Is.EqualTo(OpportunityPhase.Accepted));

            Assert.That(OpportunityState.TryRestore("milo-parcel", OpportunityPhase.Resolved,
                OpportunityResolution.None, parcelId, 1, 0, 2, 610, 0, 0, 2, 620,
                out _), Is.False);
            Assert.That(OpportunityState.TryRestore("milo-parcel", OpportunityPhase.Accepted,
                OpportunityResolution.Delivered, parcelId, 1, 0, 2, 610, 0, 0, 0, 0,
                out _), Is.False);
        }
    }
}
