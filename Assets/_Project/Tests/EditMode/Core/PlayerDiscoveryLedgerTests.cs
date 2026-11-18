using System;
using NUnit.Framework;

namespace Signal.Core.Tests
{
    public sealed class PlayerDiscoveryLedgerTests
    {
        [Test]
        public void StoresDifferentSignalTypesWithoutConvertingThemIntoConclusions()
        {
            var ledger = new PlayerDiscoveryLedger();
            ledger.Record(Signal("routine", DiscoverySignalType.RoutineObservation, 1f));
            ledger.Record(Signal("rumour", DiscoverySignalType.Rumour, 0.35f));

            Assert.That(ledger.Count, Is.EqualTo(2));
            Assert.That(ledger.HasSignalType("milo", DiscoverySignalType.RoutineObservation), Is.True);
            Assert.That(ledger.HasSignalType("milo", DiscoverySignalType.Rumour, 0.5f), Is.False);
            Assert.That(ledger.CountDistinctSignalTypes("milo"), Is.EqualTo(2));
            Assert.That(ledger.TryGet("rumour", out PlayerDiscoverySignal rumour), Is.True);
            Assert.That(rumour.Type, Is.EqualTo(DiscoverySignalType.Rumour));
            Assert.That(rumour.Confidence, Is.EqualTo(0.35f));
        }

        [Test]
        public void RecordingSameAuthoredOccurrenceIsIdempotent()
        {
            var ledger = new PlayerDiscoveryLedger();
            PlayerDiscoverySignal signal = Signal("meeting-day-2", DiscoverySignalType.RoutineObservation, 1f);

            Assert.That(ledger.Record(signal), Is.True);
            Assert.That(ledger.Record(signal), Is.False);
            Assert.That(ledger.Count, Is.EqualTo(1));
        }

        [Test]
        public void DifferentOccurrencesOfSameTypeRemainDistinctButTypeCountDoesNotInflate()
        {
            var ledger = new PlayerDiscoveryLedger();
            ledger.Record(Signal("meeting-1", DiscoverySignalType.RoutineObservation, 1f));
            ledger.Record(Signal("meeting-2", DiscoverySignalType.RoutineObservation, 1f));

            Assert.That(ledger.Count, Is.EqualTo(2));
            Assert.That(ledger.CountDistinctSignalTypes("milo"), Is.EqualTo(1));
        }

        [Test]
        public void SignalsAreScopedToTheirSubject()
        {
            var ledger = new PlayerDiscoveryLedger();
            ledger.Record(new PlayerDiscoverySignal("rhea-rumour", "rhea",
                DiscoverySignalType.Rumour, "rhea", "Something heard", 0.5f, 1, 10));

            Assert.That(ledger.HasSignalType("milo", DiscoverySignalType.Rumour), Is.False);
            Assert.That(ledger.HasSignalType("rhea", DiscoverySignalType.Rumour), Is.True);
        }

        [Test]
        public void RejectsInvalidSignalIdentityConfidenceAndTime()
        {
            Assert.Throws<ArgumentException>(() => new PlayerDiscoverySignal("", "milo",
                DiscoverySignalType.Rumour, "rhea", "", 0.5f, 1, 0));
            Assert.Throws<ArgumentOutOfRangeException>(() => Signal("bad-confidence",
                DiscoverySignalType.Rumour, 1.1f));
            Assert.Throws<ArgumentOutOfRangeException>(() => new PlayerDiscoverySignal("bad-time",
                "milo", DiscoverySignalType.Rumour, "rhea", "", 0.5f, 1, 1440));
        }

        private static PlayerDiscoverySignal Signal(string id, DiscoverySignalType type, float confidence)
        {
            return new PlayerDiscoverySignal(id, "milo", type, "player", "Observed signal",
                confidence, 1, 120);
        }
    }
}
