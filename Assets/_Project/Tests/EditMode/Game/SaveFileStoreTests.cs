using System;
using System.IO;
using NUnit.Framework;
using Signal.Core;

namespace Signal.Game.Tests
{
    public sealed class SaveFileStoreTests
    {
        private string _folder;

        [SetUp]
        public void SetUp()
        {
            _folder = Path.Combine(Path.GetTempPath(), "signal-tests-" + Guid.NewGuid().ToString("N"));
        }

        [TearDown]
        public void TearDown()
        {
            if (Directory.Exists(_folder))
                Directory.Delete(_folder, true);
        }

        [Test]
        public void SaveThenLoadRoundTripsTheData()
        {
            string path = Path.Combine(_folder, "nested", "save.json");
            var data = new SaveData { money = 12, health = 80, day = 4, minuteOfDay = 500, heat = 20, missedRent = 1, playerX = 3.5f, playerY = -1f };
            data.items.Add(new SavedItem { typeId = Guid.NewGuid().ToString(), x = 2, y = 1, rotated = true, count = 3 });

            SaveFileStore.Save(data, path);
            bool loaded = SaveFileStore.TryLoad(path, out SaveData result, out string error);

            Assert.That(loaded, Is.True, error);
            Assert.That(result.money, Is.EqualTo(12));
            Assert.That(result.health, Is.EqualTo(80));
            Assert.That(result.day, Is.EqualTo(4));
            Assert.That(result.minuteOfDay, Is.EqualTo(500));
            Assert.That(result.heat, Is.EqualTo(20));
            Assert.That(result.missedRent, Is.EqualTo(1));
            Assert.That(result.playerX, Is.EqualTo(3.5f));
            Assert.That(result.items.Count, Is.EqualTo(1));
            Assert.That(result.items[0].typeId, Is.EqualTo(data.items[0].typeId));
            Assert.That(result.items[0].rotated, Is.True);
            Assert.That(result.items[0].count, Is.EqualTo(3));
        }

        [Test]
        public void LoadingAMissingFileFails()
        {
            bool loaded = SaveFileStore.TryLoad(Path.Combine(_folder, "nothing.json"), out SaveData result, out string error);

            Assert.That(loaded, Is.False);
            Assert.That(result, Is.Null);
            Assert.That(error, Is.Not.Empty);
        }

        [Test]
        public void LoadingCorruptJsonFails()
        {
            Directory.CreateDirectory(_folder);
            string path = Path.Combine(_folder, "broken.json");
            File.WriteAllText(path, "{ this is not json");

            bool loaded = SaveFileStore.TryLoad(path, out _, out string error);

            Assert.That(loaded, Is.False);
            Assert.That(error, Is.Not.Empty);
        }

        [Test]
        public void SavingAgainReplacesTheOldFile()
        {
            string path = Path.Combine(_folder, "save.json");

            SaveFileStore.Save(new SaveData { money = 1 }, path);
            SaveFileStore.Save(new SaveData { money = 2 }, path);
            SaveFileStore.TryLoad(path, out SaveData result, out _);

            Assert.That(result.money, Is.EqualTo(2));
            Assert.That(File.Exists(path + ".tmp"), Is.False);
        }
    }
}
