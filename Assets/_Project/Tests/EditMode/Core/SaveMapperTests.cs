using System;
using System.Collections.Generic;
using NUnit.Framework;

namespace Signal.Core.Tests
{
    public sealed class SaveMapperTests
    {
        private sealed class World
        {
            public PlayerStats Stats = new PlayerStats();
            public GridInventory Inventory = new GridInventory();
            public GameClock Clock = new GameClock();
            public HeatMeter Heat = new HeatMeter();
            public RentLedger Rent = new RentLedger(50, 7);
        }

        private static ItemType Item(int width, int height, int maxStack)
        {
            return new ItemType(Guid.NewGuid(), width, height, maxStack);
        }

        private static Func<Guid, ItemType?> Resolver(params ItemType[] known)
        {
            var map = new Dictionary<Guid, ItemType>();
            foreach (var type in known)
                map[type.Id] = type;

            return id => map.TryGetValue(id, out ItemType type) ? type : (ItemType?)null;
        }

        private static SaveData CaptureOf(World world, float x = 0f, float y = 0f)
        {
            return SaveMapper.Capture(world.Stats, world.Inventory, world.Clock, world.Heat, world.Rent, x, y);
        }

        private static LoadReport ApplyTo(World world, SaveData data, params ItemType[] known)
        {
            return SaveMapper.Apply(data, world.Stats, world.Inventory, world.Clock, world.Heat, world.Rent, Resolver(known));
        }

        [Test]
        public void CaptureWritesEveryField()
        {
            var world = new World();
            var type = Item(2, 1, 5);
            world.Stats.Restore(40, 70);
            world.Clock.Restore(5, 600);
            world.Heat.Restore(25);
            world.Rent.Restore(2);
            world.Inventory.Place(type, 3, 2, rotated: true, count: 4);

            SaveData data = CaptureOf(world, 1.5f, -2.5f);

            Assert.That(data.version, Is.EqualTo(SaveMigrator.CurrentVersion));
            Assert.That(data.money, Is.EqualTo(40));
            Assert.That(data.health, Is.EqualTo(70));
            Assert.That(data.day, Is.EqualTo(5));
            Assert.That(data.minuteOfDay, Is.EqualTo(600));
            Assert.That(data.heat, Is.EqualTo(25));
            Assert.That(data.missedRent, Is.EqualTo(2));
            Assert.That(data.inventoryWidth, Is.EqualTo(6));
            Assert.That(data.inventoryHeight, Is.EqualTo(4));
            Assert.That(data.playerX, Is.EqualTo(1.5f));
            Assert.That(data.playerY, Is.EqualTo(-2.5f));
            Assert.That(data.items.Count, Is.EqualTo(1));
            Assert.That(data.items[0].typeId, Is.EqualTo(type.Id.ToString()));
            Assert.That(data.items[0].x, Is.EqualTo(3));
            Assert.That(data.items[0].y, Is.EqualTo(2));
            Assert.That(data.items[0].rotated, Is.True);
            Assert.That(data.items[0].count, Is.EqualTo(4));
        }

        [Test]
        public void ApplyRestoresEverythingCaptured()
        {
            var source = new World();
            var type = Item(1, 3, 5);
            source.Stats.Restore(123, 45);
            source.Clock.Restore(8, 700);
            source.Heat.Restore(33);
            source.Rent.Restore(1);
            source.Inventory.Place(type, 2, 0, rotated: true, count: 3);
            SaveData data = CaptureOf(source);

            var target = new World();
            target.Inventory.Place(Item(1, 1, 1), 0, 0);

            LoadReport report = ApplyTo(target, data, type);

            Assert.That(report.Success, Is.True);
            Assert.That(report.SkippedItems, Is.EqualTo(0));
            Assert.That(target.Stats.Money, Is.EqualTo(123));
            Assert.That(target.Stats.Health, Is.EqualTo(45));
            Assert.That(target.Clock.Day, Is.EqualTo(8));
            Assert.That(target.Clock.MinuteOfDay, Is.EqualTo(700));
            Assert.That(target.Heat.Heat, Is.EqualTo(33));
            Assert.That(target.Rent.MissedPayments, Is.EqualTo(1));
            Assert.That(target.Inventory.Items.Count, Is.EqualTo(1));

            PlacedItem placed = target.Inventory.Items[0];
            Assert.That(placed.Type, Is.EqualTo(type));
            Assert.That(placed.X, Is.EqualTo(2));
            Assert.That(placed.Y, Is.EqualTo(0));
            Assert.That(placed.Rotated, Is.True);
            Assert.That(placed.Count, Is.EqualTo(3));
        }

        [Test]
        public void CaptureAndApplyPreserveHomeStorageAndWorldLocation()
        {
            var source = new World();
            var carried = Item(1, 1, 10);
            var stored = Item(2, 1, 10);
            var sourceStorage = new GridInventory(8, 6);
            source.Inventory.Place(carried, 0, 0, count: 3);
            sourceStorage.Place(stored, 2, 1, rotated: true, count: 4);

            SaveData data = SaveMapper.Capture(source.Stats, source.Inventory, sourceStorage,
                                               source.Clock, source.Heat, source.Rent,
                                               "CornerShop", 1001.5f, 997.25f);

            Assert.That(data.worldPlace, Is.EqualTo("CornerShop"));
            Assert.That(data.storageItems.Count, Is.EqualTo(1));
            Assert.That(data.storageItems[0].typeId, Is.EqualTo(stored.Id.ToString()));
            Assert.That(data.storageItems[0].count, Is.EqualTo(4));

            var target = new World();
            var targetStorage = new GridInventory(8, 6);
            LoadReport report = SaveMapper.Apply(data, target.Stats, target.Inventory, targetStorage,
                                                 target.Clock, target.Heat, target.Rent, Resolver(carried, stored));

            Assert.That(report.Success, Is.True);
            Assert.That(report.SkippedStorageItems, Is.EqualTo(0));
            Assert.That(target.Inventory.CountOf(carried), Is.EqualTo(3));
            Assert.That(targetStorage.CountOf(stored), Is.EqualTo(4));
        }

        [Test]
        public void VersionOneSaveMigratesWithEmptyHomeStorageAndHideoutLocation()
        {
            var data = new SaveData { version = 1, storageItems = null, worldPlace = null };

            Assert.That(SaveMigrator.TryMigrate(data, out _), Is.True);
            Assert.That(data.worldPlace, Is.EqualTo("Hideout"));
            Assert.That(data.storageItems, Is.Empty);
        }

        [Test]
        public void ApplySkipsItemsOfUnknownTypes()
        {
            var source = new World();
            var known = Item(1, 1, 5);
            var unknown = Item(1, 1, 5);
            source.Inventory.Place(known, 0, 0, count: 2);
            source.Inventory.Place(unknown, 1, 0, count: 2);
            SaveData data = CaptureOf(source);

            var target = new World();
            LoadReport report = ApplyTo(target, data, known);

            Assert.That(report.Success, Is.True);
            Assert.That(report.SkippedItems, Is.EqualTo(1));
            Assert.That(target.Inventory.Items.Count, Is.EqualTo(1));
            Assert.That(target.Inventory.CountOf(known), Is.EqualTo(2));
        }

        [Test]
        public void ApplySkipsItemsThatNoLongerFit()
        {
            var source = new World();
            var type = Item(1, 1, 5);
            source.Inventory.Place(type, 5, 3);
            SaveData data = CaptureOf(source);

            var target = new World();
            target.Inventory = new GridInventory(2, 2);
            LoadReport report = ApplyTo(target, data, type);

            Assert.That(report.Success, Is.True);
            Assert.That(report.SkippedItems, Is.EqualTo(1));
            Assert.That(target.Inventory.Items.Count, Is.EqualTo(0));
        }

        [Test]
        public void ApplySkipsAnItemWithAnInvalidTypeId()
        {
            var data = new SaveData();
            data.items.Add(new SavedItem { typeId = "not-a-guid", x = 0, y = 0, count = 1 });

            var target = new World();
            LoadReport report = ApplyTo(target, data);

            Assert.That(report.Success, Is.True);
            Assert.That(report.SkippedItems, Is.EqualTo(1));
        }

        [Test]
        public void ANewerSaveIsRejectedAndChangesNothing()
        {
            var data = new SaveData { version = SaveMigrator.CurrentVersion + 1, money = 999 };
            var target = new World();
            target.Stats.Restore(5, 50);

            LoadReport report = ApplyTo(target, data);

            Assert.That(report.Success, Is.False);
            Assert.That(report.Error, Is.Not.Empty);
            Assert.That(target.Stats.Money, Is.EqualTo(5));
        }

        [Test]
        public void MissingSaveDataIsRejected()
        {
            LoadReport report = ApplyTo(new World(), null);

            Assert.That(report.Success, Is.False);
        }

        [TestCase(1, true)]
        [TestCase(0, false)]
        [TestCase(-3, false)]
        public void MigrationAcceptsOnlyKnownVersions(int version, bool expected)
        {
            var data = new SaveData { version = version };

            Assert.That(SaveMigrator.TryMigrate(data, out _), Is.EqualTo(expected));
        }

        [Test]
        public void ItemKnowledgeRoundTripsWithStableItemIdentityAndEvidenceMetadata()
        {
            var source = new World();
            var type = Item(1, 1, 5);
            source.Inventory.Place(type, 0, 0);
            Guid itemId = source.Inventory.Items[0].ItemInstanceId;
            var sourceKnowledge = new ItemKnowledgeBook();
            sourceKnowledge.RecordFact(itemId, ItemKnowledgeAspect.Authenticity,
                (int)ItemAuthenticity.Genuine, ItemKnowledgeSource.Rumour, 0.45f, 4, 615);
            sourceKnowledge.RecordFact(itemId, ItemKnowledgeAspect.Condition,
                (int)ItemCondition.Worn, ItemKnowledgeSource.DirectInspection, 1f, 4, 620);

            SaveData data = SaveMapper.Capture(source.Stats, source.Inventory, null,
                source.Clock, source.Heat, source.Rent, "Hideout", 0f, 0f, sourceKnowledge);

            var target = new World();
            var targetKnowledge = new ItemKnowledgeBook();
            LoadReport report = SaveMapper.Apply(data, target.Stats, target.Inventory, null,
                target.Clock, target.Heat, target.Rent, Resolver(type), targetKnowledge);

            Assert.That(report.Success, Is.True);
            Assert.That(report.SkippedKnowledgeFacts, Is.Zero);
            Assert.That(data.version, Is.EqualTo(5));
            Assert.That(target.Inventory.Items[0].ItemInstanceId, Is.EqualTo(itemId));
            Assert.That(targetKnowledge.FactCount, Is.EqualTo(2));
            Assert.That(targetKnowledge.TryGetFact(itemId, ItemKnowledgeAspect.Authenticity, out ItemKnowledgeFact rumour), Is.True);
            Assert.That(rumour.Value, Is.EqualTo((int)ItemAuthenticity.Genuine));
            Assert.That(rumour.Source, Is.EqualTo(ItemKnowledgeSource.Rumour));
            Assert.That(rumour.Confidence, Is.EqualTo(0.45f));
            Assert.That(rumour.LearnedDay, Is.EqualTo(4));
            Assert.That(rumour.LearnedMinuteOfDay, Is.EqualTo(615));
            Assert.That(targetKnowledge.TryGetFact(itemId, ItemKnowledgeAspect.Condition, out ItemKnowledgeFact inspection), Is.True);
            Assert.That(inspection.Value, Is.EqualTo((int)ItemCondition.Worn));
            Assert.That(inspection.Source, Is.EqualTo(ItemKnowledgeSource.DirectInspection));
        }

        [Test]
        public void VersionFourSaveMigratesWithNoPlayerItemKnowledge()
        {
            var data = new SaveData { version = 4, itemKnowledge = null };

            Assert.That(SaveMigrator.TryMigrate(data, out _), Is.True);
            Assert.That(data.version, Is.EqualTo(5));
            Assert.That(data.itemKnowledge, Is.Empty);
        }

        [Test]
        public void InvalidSavedKnowledgeFactsAreSkippedWithoutRejectingTheSave()
        {
            var data = new SaveData();
            data.itemKnowledge.Add(new SavedItemKnowledge
            {
                instanceId = Guid.NewGuid().ToString(),
                aspect = (int)ItemKnowledgeAspect.Quality,
                value = 999,
                source = (int)ItemKnowledgeSource.DirectInspection,
                confidence = 1f,
                learnedDay = 1,
                learnedMinuteOfDay = 0
            });
            var knowledge = new ItemKnowledgeBook();
            var world = new World();

            LoadReport report = SaveMapper.Apply(data, world.Stats, world.Inventory, null,
                world.Clock, world.Heat, world.Rent, Resolver(), knowledge);

            Assert.That(report.Success, Is.True);
            Assert.That(report.SkippedKnowledgeFacts, Is.EqualTo(1));
            Assert.That(knowledge.FactCount, Is.Zero);
        }
    }
}
