using System;
using System.Collections.Generic;
using NUnit.Framework;

namespace Signal.Core.Tests
{
    public sealed class CrimeOutcomeSaveTests
    {
        private sealed class World
        {
            public PlayerStats Stats = new PlayerStats();
            public GridInventory Inventory = new GridInventory();
            public GameClock Clock = new GameClock();
            public HeatMeter Heat = new HeatMeter();
            public RentLedger Rent = new RentLedger(50, 7);
        }

        private static Func<Guid, ItemType?> Resolver(ItemType known)
        {
            return id => id == known.Id ? (ItemType?)known : null;
        }

        [Test]
        public void CaptureAndApplyPreserveProgressionAndWitnessedCrimeOutcome()
        {
            var source = new World();
            var type = new ItemType(Guid.NewGuid(), 1, 1, 5);
            source.Inventory.Place(type, 0, 0);
            var progression = new CriminalProgressionTrack();
            progression.Award(3);
            var outcomes = new CrimeOutcomeLedger();
            var outcome = new CrimeOutcome(Guid.NewGuid(), "apartment-container-theft", "player",
                source.Inventory.Items[0].ItemInstanceId, "Apartment", "rhea", "crime-rhea-1",
                "Someone took something from the wardrobe.", 1024.8f, 1003.2f, 8f,
                2, 615, 2, 1);
            Assert.That(outcomes.TryRecord(outcome), Is.True);

            SaveData data = SaveMapper.Capture(source.Stats, source.Inventory, null, source.Clock,
                source.Heat, source.Rent, "Apartment", 1020f, 1000f,
                criminalProgression: progression, crimeOutcomes: outcomes);

            var target = new World();
            var restoredProgression = new CriminalProgressionTrack();
            var restoredOutcomes = new CrimeOutcomeLedger();
            LoadReport report = SaveMapper.Apply(data, target.Stats, target.Inventory, null,
                target.Clock, target.Heat, target.Rent, Resolver(type),
                criminalProgression: restoredProgression, crimeOutcomes: restoredOutcomes);

            Assert.That(report.Success, Is.True);
            Assert.That(report.SkippedCrimeOutcomes, Is.Zero);
            Assert.That(restoredProgression.Points, Is.EqualTo(3));
            Assert.That(restoredOutcomes.Count, Is.EqualTo(1));
            Assert.That(restoredOutcomes.Outcomes[0].ActorId, Is.EqualTo("player"));
            Assert.That(restoredOutcomes.Outcomes[0].WitnessNpcId, Is.EqualTo("rhea"));
            Assert.That(restoredOutcomes.Outcomes[0].WitnessEventId, Is.EqualTo("crime-rhea-1"));
            Assert.That(restoredOutcomes.Outcomes[0].HeatAward, Is.EqualTo(2));
            Assert.That(restoredOutcomes.Outcomes[0].ProgressionAward, Is.EqualTo(1));
        }

        [Test]
        public void VersionTwelveSaveMigratesWithNoCrimeHistoryOrProgression()
        {
            var data = new SaveData
            {
                version = 12,
                crimeOutcomes = null,
                criminalProgressionPoints = 0
            };

            Assert.That(SaveMigrator.TryMigrate(data, out _), Is.True);
            Assert.That(data.version, Is.EqualTo(SaveMigrator.CurrentVersion));
            Assert.That(data.crimeOutcomes, Is.Not.Null);
            Assert.That(data.crimeOutcomes, Is.Empty);
            Assert.That(data.criminalProgressionPoints, Is.Zero);
        }
    }
}
