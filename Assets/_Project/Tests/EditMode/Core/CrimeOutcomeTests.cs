using System;
using NUnit.Framework;

namespace Signal.Core.Tests
{
    public sealed class CrimeOutcomeTests
    {
        private static CrimeOutcome Outcome(Guid id, string witness = "rhea")
        {
            bool witnessed = !string.IsNullOrWhiteSpace(witness);
            return new CrimeOutcome(id, "apartment-container-theft", "player", Guid.NewGuid(), "Apartment",
                witness, witnessed ? "crime-" + id.ToString("N") : string.Empty,
                witnessed ? "Someone took something from the wardrobe." : string.Empty,
                1024.8f, 1003.2f, 8f, 1, 600, 2, 1);
        }

        [Test]
        public void LedgerRecordsCompletedOutcomesOnce()
        {
            var ledger = new CrimeOutcomeLedger();
            CrimeOutcome outcome = Outcome(Guid.NewGuid());

            Assert.That(ledger.TryRecord(outcome), Is.True);
            Assert.That(ledger.TryRecord(outcome), Is.False);
            Assert.That(ledger.Count, Is.EqualTo(1));
            Assert.That(ledger.Contains(outcome.OutcomeId), Is.True);
            Assert.That(ledger.Outcomes[0].WasWitnessed, Is.True);
            Assert.That(ledger.Outcomes[0].WitnessNpcId, Is.EqualTo("rhea"));
        }

        [Test]
        public void UnwitnessedOutcomeDoesNotInventWitnessKnowledge()
        {
            CrimeOutcome outcome = Outcome(Guid.NewGuid(), null);

            Assert.That(outcome.WasWitnessed, Is.False);
            Assert.That(outcome.WitnessNpcId, Is.Empty);
            Assert.That(outcome.WitnessEventId, Is.Empty);
            Assert.That(outcome.HeatAward, Is.EqualTo(2));
            Assert.That(outcome.ProgressionAward, Is.EqualTo(1));
        }

        [Test]
        public void InvalidWitnessPairAndNegativeAwardsAreRejected()
        {
            Guid id = Guid.NewGuid();
            Guid item = Guid.NewGuid();
            Assert.Throws<ArgumentException>(() => new CrimeOutcome(id, "theft", "player", item, "Apartment",
                "rhea", string.Empty, "Observed.", 0f, 0f, 2f, 1, 10, 1, 1));
            Assert.Throws<ArgumentOutOfRangeException>(() => new CrimeOutcome(id, "theft", "player", item, "Apartment",
                null, string.Empty, string.Empty, 0f, 0f, 2f, 1, 10, -1, 1));
        }
    }
}
