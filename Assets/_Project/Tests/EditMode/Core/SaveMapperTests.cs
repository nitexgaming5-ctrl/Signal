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
        public void AgendaProgressSurvivesSaveCaptureAndRestore()
        {
            var source = new World();
            var progress = new NpcAgendaProgressBook();
            progress.TryCompleteStage("rhea", "milo-tip", "opening", "ask-source");
            progress.TryCompleteStage("rhea", "machine-offer", "opening", "decline");

            SaveData data = SaveMapper.Capture(source.Stats, source.Inventory, null,
                source.Clock, source.Heat, source.Rent, "Hideout", 0f, 0f,
                npcAgendaProgress: progress);
            Assert.That(data.npcAgendaProgress.Count, Is.EqualTo(2));

            var restored = new NpcAgendaProgressBook();
            LoadReport report = SaveMapper.Apply(data, new World().Stats, new GridInventory(),
                null, new GameClock(), new HeatMeter(), new RentLedger(50, 7),
                Resolver(), npcAgendaProgress: restored);

            Assert.That(report.Success, Is.True);
            Assert.That(report.SkippedNpcAgendaProgress, Is.Zero);
            Assert.That(restored.CompletedStageCount, Is.EqualTo(2));
            Assert.That(restored.TryGetChoice("rhea", "milo-tip", "opening", out string choice), Is.True);
            Assert.That(choice, Is.EqualTo("ask-source"));
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
        public void MissingSaveDataIsRejected()
        {
            LoadReport report = ApplyTo(new World(), null);

            Assert.That(report.Success, Is.False);
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
        public void RepairedJacketStateAndConditionKnowledgeRoundTripTogether()
        {
            var source = new World();
            ItemType jacket = Item(2, 2, 1);
            Guid itemId = Guid.NewGuid();
            var repairedState = new ItemInstanceState(ItemQuality.Fine, ItemCondition.Good,
                ItemAuthenticity.Genuine, ItemOwnershipStatus.Stolen);
            source.Inventory.Place(jacket, 0, 0, itemInstanceId: itemId, state: repairedState);
            var sourceKnowledge = new ItemKnowledgeBook();
            sourceKnowledge.RecordFact(itemId, ItemKnowledgeAspect.Quality, (int)ItemQuality.Fine,
                ItemKnowledgeSource.Document, 0.8f, 2, 615);
            sourceKnowledge.RecordFact(itemId, ItemKnowledgeAspect.Authenticity, (int)ItemAuthenticity.Genuine,
                ItemKnowledgeSource.Rumour, 0.6f, 2, 615);
            sourceKnowledge.RecordFact(itemId, ItemKnowledgeAspect.Ownership, (int)ItemOwnershipStatus.Stolen,
                ItemKnowledgeSource.Document, 0.95f, 2, 615);
            sourceKnowledge.RecordFact(itemId, ItemKnowledgeAspect.Condition, (int)ItemCondition.Good,
                ItemKnowledgeSource.DirectInspection, 1f, 2, 615);
            var sourceOwnership = new AssetOwnershipBook();
            sourceOwnership.TryRegister(itemId, EconomicActorId.Player);

            SaveData data = SaveMapper.Capture(source.Stats, source.Inventory, null, source.Clock,
                source.Heat, source.Rent, "Hideout", 1f, 2f, sourceKnowledge,
                assetOwnership: sourceOwnership);

            var target = new World();
            var targetKnowledge = new ItemKnowledgeBook();
            var targetOwnership = new AssetOwnershipBook();
            LoadReport report = SaveMapper.Apply(data, target.Stats, target.Inventory, null,
                target.Clock, target.Heat, target.Rent, Resolver(jacket), targetKnowledge,
                assetOwnership: targetOwnership);

            Assert.That(report.Success, Is.True);
            Assert.That(report.SkippedKnowledgeFacts, Is.Zero);
            Assert.That(target.Inventory.Items.Count, Is.EqualTo(1));
            Assert.That(target.Inventory.Items[0].ItemInstanceId, Is.EqualTo(itemId));
            Assert.That(target.Inventory.Items[0].State, Is.EqualTo(repairedState));
            Assert.That(targetOwnership.TryGetOwner(itemId, out EconomicActorId owner), Is.True);
            Assert.That(owner, Is.EqualTo(EconomicActorId.Player));
            Assert.That(targetKnowledge.FactCount, Is.EqualTo(4));
            Assert.That(targetKnowledge.TryGetFact(itemId, ItemKnowledgeAspect.Condition, out ItemKnowledgeFact condition), Is.True);
            Assert.That(condition.Value, Is.EqualTo((int)ItemCondition.Good));
            Assert.That(condition.Source, Is.EqualTo(ItemKnowledgeSource.DirectInspection));
            Assert.That(condition.Confidence, Is.EqualTo(1f));
            Assert.That(targetKnowledge.TryGetFact(itemId, ItemKnowledgeAspect.Ownership, out ItemKnowledgeFact ownership), Is.True);
            Assert.That(ownership.Value, Is.EqualTo((int)ItemOwnershipStatus.Stolen));
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

        [Test]
        public void ItemKnowledgePersistsAfterThePhysicalItemLeavesInventory()
        {
            var source = new World();
            var knowledge = new ItemKnowledgeBook();
            Guid formerItemId = Guid.NewGuid();
            knowledge.RecordFact(formerItemId, ItemKnowledgeAspect.Ownership,
                (int)ItemOwnershipStatus.Stolen, ItemKnowledgeSource.Document, 0.95f, 6, 300);

            SaveData data = SaveMapper.Capture(source.Stats, source.Inventory, null,
                source.Clock, source.Heat, source.Rent, "Hideout", 0f, 0f, knowledge);

            var restoredKnowledge = new ItemKnowledgeBook();
            LoadReport report = SaveMapper.Apply(data, source.Stats, source.Inventory, null,
                source.Clock, source.Heat, source.Rent, Resolver(), restoredKnowledge);

            Assert.That(report.Success, Is.True);
            Assert.That(source.Inventory.Items, Is.Empty);
            Assert.That(restoredKnowledge.TryGetFact(formerItemId, ItemKnowledgeAspect.Ownership, out ItemKnowledgeFact fact), Is.True);
            Assert.That(fact.Value, Is.EqualTo((int)ItemOwnershipStatus.Stolen));
            Assert.That(fact.Source, Is.EqualTo(ItemKnowledgeSource.Document));
        }

        [Test]
        public void RoundTripPreservesPlayerDiscoveriesAndOpportunityLifecycle()
        {
            var source = new World();
            var discoveries = new PlayerDiscoveryLedger();
            discoveries.Record(new PlayerDiscoverySignal("milo-routine-day-2", "milo",
                DiscoverySignalType.RoutineObservation, "player-observation", "Milo left the shop after closing.",
                1f, 2, 1260));
            discoveries.Record(new PlayerDiscoverySignal("milo-rumour-rhea-day-2", "milo",
                DiscoverySignalType.Rumour, "rhea", "Rhea heard Milo was helping someone.", 0.45f, 2, 1270));
            var opportunity = new OpportunityState("milo-parcel");
            opportunity.MakeAvailable();
            Guid parcelId = Guid.NewGuid();
            opportunity.TryAccept(parcelId, 2, 1280);
            opportunity.TryReturnParcel(2, 1290);
            opportunity.RecordDecline(2, 1300);

            SaveData data = SaveMapper.Capture(source.Stats, source.Inventory, null, source.Clock,
                source.Heat, source.Rent, "Street", 4f, -12f, null, discoveries, opportunity);
            Assert.That(data.playerDiscoveries.Count, Is.EqualTo(2));
            Assert.That(data.opportunity.parcelInstanceId, Is.EqualTo(parcelId.ToString()));

            var target = new World();
            var restoredDiscoveries = new PlayerDiscoveryLedger();
            LoadReport report = SaveMapper.Apply(data, target.Stats, target.Inventory, null,
                target.Clock, target.Heat, target.Rent, Resolver(), null, restoredDiscoveries);

            Assert.That(report.Success, Is.True);
            Assert.That(report.SkippedDiscoverySignals, Is.Zero);
            Assert.That(restoredDiscoveries.Count, Is.EqualTo(2));
            Assert.That(restoredDiscoveries.TryGet("milo-rumour-rhea-day-2", out PlayerDiscoverySignal rumour), Is.True);
            Assert.That(rumour.SourceId, Is.EqualTo("rhea"));
            Assert.That(rumour.Confidence, Is.EqualTo(0.45f));
            Assert.That(report.RestoredOpportunity, Is.Not.Null);
            Assert.That(report.RestoredOpportunity.Phase, Is.EqualTo(OpportunityPhase.Available));
            Assert.That(report.RestoredOpportunity.ParcelInstanceId, Is.EqualTo(parcelId));
            Assert.That(report.RestoredOpportunity.AcceptanceCount, Is.EqualTo(1));
            Assert.That(report.RestoredOpportunity.DeclineCount, Is.EqualTo(1));
        }

        [Test]
        public void InvalidDiscoverySignalsAreSkippedWithoutFailingSaveLoad()
        {
            var data = new SaveData();
            data.playerDiscoveries.Add(new SavedDiscoverySignal
            {
                signalId = "bad-confidence", subjectId = "milo",
                type = (int)DiscoverySignalType.Rumour, sourceId = "rhea",
                description = "Rumour", confidence = 2f, learnedDay = 1, learnedMinuteOfDay = 10
            });

            var discoveries = new PlayerDiscoveryLedger();
            LoadReport report = SaveMapper.Apply(data, new World().Stats, new GridInventory(), null,
                new GameClock(), new HeatMeter(), new RentLedger(50, 7), Resolver(), null, discoveries);

            Assert.That(report.Success, Is.True);
            Assert.That(report.SkippedDiscoverySignals, Is.EqualTo(1));
            Assert.That(discoveries.Count, Is.Zero);
        }

        [Test]
        public void RoundTripPreservesPlayerRelationshipImpressions()
        {
            var source = new World();
            var relationships = new PlayerRelationshipBook();
            relationships.Set("milo", 0.35f, 0.7f, 0.2f);
            relationships.Set("rhea", 0.1f, 0.45f, -0.1f);

            SaveData data = SaveMapper.Capture(source.Stats, source.Inventory, null, source.Clock,
                source.Heat, source.Rent, "Hideout", 1f, 2f, null, null, null, relationships);

            var restored = new PlayerRelationshipBook();
            LoadReport report = SaveMapper.Apply(data, new World().Stats, new GridInventory(), null,
                new GameClock(), new HeatMeter(), new RentLedger(50, 7), Resolver(), null, null, restored);

            Assert.That(report.Success, Is.True);
            Assert.That(report.SkippedPlayerRelationships, Is.Zero);
            Assert.That(restored.Count, Is.EqualTo(2));
            Assert.That(restored.Get("milo").Familiarity, Is.EqualTo(0.35f));
            Assert.That(restored.Get("milo").Trust, Is.EqualTo(0.7f));
            Assert.That(restored.Get("rhea").Affinity, Is.EqualTo(-0.1f));
        }

        [Test]
        public void SaveRoundTripPreservesResolvedDeliveryAssessment()
        {
            var source = new World();
            var opportunity = new OpportunityState("milo-parcel");
            opportunity.MakeAvailable();
            Guid parcelId = Guid.NewGuid();
            opportunity.SetParcelInstanceId(parcelId);
            opportunity.TryAccept(parcelId, 3, 1200);
            opportunity.TryResolveDelivered(3, 1320, false, false, true);

            SaveData data = SaveMapper.Capture(source.Stats, source.Inventory, null, source.Clock,
                source.Heat, source.Rent, "Street", 22.5f, -18.5f, null, null, opportunity);

            LoadReport report = SaveMapper.Apply(data, new World().Stats, new GridInventory(), null,
                new GameClock(), new HeatMeter(), new RentLedger(50, 7), Resolver());

            Assert.That(report.Success, Is.True);
            Assert.That(report.RestoredOpportunity, Is.Not.Null);
            Assert.That(report.RestoredOpportunity.Phase, Is.EqualTo(OpportunityPhase.Resolved));
            Assert.That(report.RestoredOpportunity.HasDeliveryAssessment, Is.True);
            Assert.That(report.RestoredOpportunity.DeliveredOnTime, Is.False);
            Assert.That(report.RestoredOpportunity.DeliveredIntact, Is.False);
            Assert.That(report.RestoredOpportunity.DeliveryWitnessed, Is.True);
        }

        [Test]
        public void EconomicAccountOwnershipAndTransactionHistoryRoundTrip()
        {
            var source = new World();
            source.Stats.Restore(321, 73);
            ItemType type = Item(1, 1, 5);
            var residentContainer = new GridInventory();
            residentContainer.Place(type, 0, 0, count: 2);
            Guid itemId = residentContainer.Items[0].ItemInstanceId;
            var ownership = new AssetOwnershipBook();
            EconomicActorId resident = new EconomicActorId("resident:milo");
            Assert.That(ownership.TryRegister(itemId, resident), Is.True);
            var containers = new EconomicContainerRegistry();
            Assert.That(containers.TryRegister(residentContainer, resident), Is.True);
            Assert.That(containers.TryRegister(source.Inventory, EconomicActorId.Player), Is.True);
            var ledger = new EconomicTransactionLedger();
            var transfer = new AssetTransferService(ownership, containers, ledger);
            AssetTransferResult moved = transfer.TransferStack(residentContainer, source.Inventory, itemId,
                resident, EconomicActorId.Player, AssetTransferReason.Purchase, 2, 615);
            Assert.That(moved.Success, Is.True);

            var sourceStorage = new GridInventory(8, 6);
            SaveData data = SaveMapper.Capture(source.Stats, source.Inventory, sourceStorage,
                source.Clock, source.Heat, source.Rent, "Hideout", 2f, 3f,
                null, null, null, null, ownership, ledger);
            var target = new World();
            var targetStorage = new GridInventory(8, 6);
            var restoredOwnership = new AssetOwnershipBook();
            var restoredLedger = new EconomicTransactionLedger();
            LoadReport report = SaveMapper.Apply(data, target.Stats, target.Inventory, targetStorage,
                target.Clock, target.Heat, target.Rent, Resolver(type), null, null, null,
                restoredOwnership, restoredLedger);

            Assert.That(report.Success, Is.True);
            Assert.That(report.SkippedAssetOwners, Is.Zero);
            Assert.That(report.SkippedEconomicTransactions, Is.Zero);
            Assert.That(target.Stats.CleanMoney.Balance, Is.EqualTo(321));
            Assert.That(target.Stats.Health, Is.EqualTo(73));
            Assert.That(target.Inventory.Items.Count, Is.EqualTo(1));
            Assert.That(target.Inventory.Items[0].ItemInstanceId, Is.EqualTo(itemId));
            Assert.That(restoredOwnership.TryGetOwner(itemId, out EconomicActorId restoredOwner), Is.True);
            Assert.That(restoredOwner, Is.EqualTo(EconomicActorId.Player));
            Assert.That(restoredLedger.Count, Is.EqualTo(1));
            EconomicTransaction transaction = restoredLedger.Transactions[0];
            Assert.That(transaction.TransactionId, Is.EqualTo(moved.Transaction.TransactionId));
            Assert.That(transaction.ItemInstanceId, Is.EqualTo(itemId));
            Assert.That(transaction.ItemDefinitionId, Is.EqualTo(type.Id));
            Assert.That(transaction.Quantity, Is.EqualTo(2));
            Assert.That(transaction.FromActor, Is.EqualTo(resident));
            Assert.That(transaction.ToActor, Is.EqualTo(EconomicActorId.Player));
            Assert.That(transaction.Reason, Is.EqualTo(AssetTransferReason.Purchase));
            Assert.That(transaction.Day, Is.EqualTo(2));
            Assert.That(transaction.MinuteOfDay, Is.EqualTo(615));
        }

        [Test]
        public void InvalidEconomicRecordsAreSkippedWithoutRejectingTheSave()
        {
            var data = new SaveData();
            data.assetOwners.Add(new SavedAssetOwner { instanceId = "not-a-guid", actorId = "resident:milo" });
            data.economicTransactions.Add(new SavedEconomicTransaction
            {
                transactionId = "bad-id", itemInstanceId = Guid.NewGuid().ToString(),
                itemDefinitionId = Guid.NewGuid().ToString(), quantity = 1,
                fromActorId = "resident:milo", toActorId = "player",
                reason = (int)AssetTransferReason.Gift, day = 1, minuteOfDay = 10
            });
            var world = new World();
            var ownership = new AssetOwnershipBook();
            var ledger = new EconomicTransactionLedger();
            LoadReport report = SaveMapper.Apply(data, world.Stats, world.Inventory, null,
                world.Clock, world.Heat, world.Rent, Resolver(), null, null, null, ownership, ledger);
            Assert.That(report.Success, Is.True);
            Assert.That(report.SkippedAssetOwners, Is.EqualTo(1));
            Assert.That(report.SkippedEconomicTransactions, Is.EqualTo(1));
            Assert.That(ownership.Count, Is.Zero);
            Assert.That(ledger.Count, Is.Zero);
        }

        [Test]
        public void ShopSaleHistoryAndItemStateRoundTripInCurrentSave()
        {
            ItemType type = Item(1, 1, 5);
            var state = new ItemInstanceState(ItemQuality.Fine, ItemCondition.Worn,
                ItemAuthenticity.Genuine, ItemOwnershipStatus.Stolen);
            var sourceStats = new PlayerStats(25);
            var sourceInventory = new GridInventory();
            sourceInventory.Place(type, 0, 0, count: 3, state: state);
            PlacedItem sourceStack = sourceInventory.Items[0];
            var sourceOwnership = new AssetOwnershipBook();
            sourceOwnership.TryRegister(sourceStack.ItemInstanceId, EconomicActorId.Player);
            var sourceSales = new ShopSaleTransactionLedger();

            Assert.That(Shop.TryFulfilExact(new ShopOrder(type, 1, 6), sourceStack.ItemInstanceId,
                "test-bakery", EconomicActorId.Player, sourceInventory, sourceOwnership,
                sourceStats.CleanMoney, sourceSales, 3, 720), Is.EqualTo(FulfilResult.Done));

            SaveData data = SaveMapper.Capture(sourceStats, sourceInventory, null,
                new GameClock(), new HeatMeter(), new RentLedger(50, 7), "Hideout", 0f, 0f,
                null, null, null, null, sourceOwnership, null, null, null, sourceSales);
            Assert.That(data.shopSaleTransactions.Count, Is.EqualTo(1));

            var targetStats = new PlayerStats();
            var targetInventory = new GridInventory();
            var targetOwnership = new AssetOwnershipBook();
            var targetSales = new ShopSaleTransactionLedger();
            LoadReport report = SaveMapper.Apply(data, targetStats, targetInventory, null,
                new GameClock(), new HeatMeter(), new RentLedger(50, 7), Resolver(type),
                null, null, null, targetOwnership, null, null, null, targetSales);

            Assert.That(report.Success, Is.True);
            Assert.That(report.SkippedShopSaleTransactions, Is.Zero);
            Assert.That(targetStats.CleanMoney.Balance, Is.EqualTo(31));
            Assert.That(targetInventory.Items.Count, Is.EqualTo(1));
            Assert.That(targetInventory.Items[0].ItemInstanceId, Is.EqualTo(sourceStack.ItemInstanceId));
            Assert.That(targetInventory.Items[0].Count, Is.EqualTo(2));
            Assert.That(targetInventory.Items[0].State, Is.EqualTo(state));
            Assert.That(targetOwnership.TryGetOwner(sourceStack.ItemInstanceId, out EconomicActorId owner), Is.True);
            Assert.That(owner, Is.EqualTo(EconomicActorId.Player));
            Assert.That(targetSales.Count, Is.EqualTo(1));
            ShopSaleTransaction restored = targetSales.Transactions[0];
            Assert.That(restored.BusinessId, Is.EqualTo("test-bakery"));
            Assert.That(restored.ItemInstanceId, Is.EqualTo(sourceStack.ItemInstanceId));
            Assert.That(restored.Quantity, Is.EqualTo(1));
            Assert.That(restored.UnitPrice, Is.EqualTo(6));
            Assert.That(restored.ItemState, Is.EqualTo(state));
        }

        [Test]
        public void InvalidShopSaleRecordsAreSkippedWithoutRejectingTheSave()
        {
            var data = new SaveData();
            data.shopSaleTransactions.Add(new SavedShopSaleTransaction
            {
                transactionId = "not-a-guid",
                businessId = "test-business",
                sellerActorId = "player",
                itemInstanceId = Guid.NewGuid().ToString(),
                itemDefinitionId = Guid.NewGuid().ToString(),
                quantity = 1,
                unitPrice = 5,
                totalAmount = 5,
                day = 1,
                minuteOfDay = 10
            });
            var sales = new ShopSaleTransactionLedger();
            var world = new World();

            LoadReport report = SaveMapper.Apply(data, world.Stats, world.Inventory, null,
                world.Clock, world.Heat, world.Rent, Resolver(), null, null, null,
                null, null, null, null, sales);

            Assert.That(report.Success, Is.True);
            Assert.That(report.SkippedShopSaleTransactions, Is.EqualTo(1));
            Assert.That(sales.Count, Is.Zero);
        }

        [Test]
        public void CaptureAndApplyRoundTripProductionContractsWorkshopUpgradeAndCustomerInventory()
        {
            var playerStats = new PlayerStats(50);
            var playerInventory = new GridInventory();
            var playerOwnership = new AssetOwnershipBook();
            var productionHistory = new ProductionTransactionLedger();
            var contracts = new CustomerContractBook();
            var workshop = new WorkshopProgressionBook();
            var receiverInventory = new GridInventory(4, 4);
            var input = Item(1, 1, 10);
            var received = Item(1, 1, 10);
            playerInventory.Place(input, 0, 0, count: 2);
            Guid receiverStackId = Guid.NewGuid();
            receiverInventory.Place(received, 0, 0, count: 3, itemInstanceId: receiverStackId);
            playerOwnership.TryRegister(playerInventory.Items[0].ItemInstanceId, EconomicActorId.Player);
            var customerActor = new EconomicActorId("resident:test-customer");
            playerOwnership.TryRegister(receiverStackId, customerActor);

            Guid recipeId = Guid.NewGuid();
            productionHistory.TryRecord(new ProductionTransaction(Guid.NewGuid(), recipeId,
                received.Id, 2, EconomicActorId.Player, 2, 500));
            Guid contractId = Guid.NewGuid();
            contracts.TryRegisterOffer(contractId);
            contracts.TryAccept(contractId, 2, 450);
            Guid upgradeId = Guid.NewGuid();
            workshop.TryAcquire(upgradeId);

            var clock = new GameClock();
            clock.Restore(2, 500);
            var data = SaveMapper.Capture(playerStats, playerInventory, null, clock,
                new HeatMeter(), new RentLedger(50, 7), "Hideout", 0f, 0f,
                assetOwnership: playerOwnership,
                productionTransactions: productionHistory,
                customerContracts: contracts,
                workshopProgression: workshop,
                contractReceiverInventory: receiverInventory,
                contractReceiverActorId: customerActor.Value);

            var targetStats = new PlayerStats();
            var targetInventory = new GridInventory();
            var targetOwnership = new AssetOwnershipBook();
            var targetProduction = new ProductionTransactionLedger();
            var targetContracts = new CustomerContractBook();
            targetContracts.TryRegisterOffer(contractId);
            var targetWorkshop = new WorkshopProgressionBook();
            var targetReceiver = new GridInventory(4, 4);

            LoadReport report = SaveMapper.Apply(data, targetStats, targetInventory, null,
                new GameClock(), new HeatMeter(), new RentLedger(50, 7), Resolver(input, received),
                assetOwnership: targetOwnership,
                productionTransactions: targetProduction,
                customerContracts: targetContracts,
                workshopProgression: targetWorkshop,
                contractReceiverInventory: targetReceiver);

            Assert.That(report.Success, Is.True);
            Assert.That(report.SkippedProductionTransactions, Is.Zero);
            Assert.That(report.SkippedCustomerContracts, Is.Zero);
            Assert.That(report.SkippedWorkshopUpgradeIds, Is.Zero);
            Assert.That(report.SkippedContractReceiverItems, Is.Zero);
            Assert.That(targetProduction.Count, Is.EqualTo(1));
            Assert.That(targetProduction.Transactions[0].OperatorActor, Is.EqualTo(EconomicActorId.Player));
            Assert.That(targetContracts.ActiveContractId, Is.EqualTo(contractId));
            Assert.That(targetWorkshop.HasAcquired(upgradeId), Is.True);
            Assert.That(targetReceiver.CountOf(received), Is.EqualTo(3));
            Assert.That(targetOwnership.TryGetOwner(receiverStackId, out EconomicActorId restoredOwner), Is.True);
            Assert.That(restoredOwner, Is.EqualTo(customerActor));
            Assert.That(targetInventory.CountOf(input), Is.EqualTo(2));
        }
        [Test]
        public void MalformedAgendaProgressEntriesAreSkippedWithoutFailingSaveLoad()
        {
            var data = new SaveData();
            data.npcAgendaProgress.Add(new SavedNpcAgendaStageProgress
            {
                npcId = "rhea",
                threadId = "thread",
                stageId = "stage",
                choiceId = ""
            });

            var restored = new NpcAgendaProgressBook();
            LoadReport report = SaveMapper.Apply(data, new World().Stats, new GridInventory(),
                null, new GameClock(), new HeatMeter(), new RentLedger(50, 7),
                Resolver(), npcAgendaProgress: restored);

            Assert.That(report.Success, Is.True);
            Assert.That(report.SkippedNpcAgendaProgress, Is.EqualTo(1));
            Assert.That(restored.CompletedStageCount, Is.Zero);
        }

        [Test]
        public void NpcSocialStateSurvivesSaveLoad()
        {
            var sourceWorld = new NpcSocialWorld();
            sourceWorld.RegisterNpc("milo", "Street", 1f, 2f, 8f);
            sourceWorld.RegisterNpc("rhea", "CornerShop", 3f, 4f, 8f);
            sourceWorld.RegisterNpc("customer", "Street", 5f, 6f, 8f);
            sourceWorld.PublishEvent(new NpcWorldEvent("parcel-1", "Parcel", "Milo handled a parcel.",
                "milo", "Street", 1f, 2f, 4f, 2, 620));
            sourceWorld.SetRelationship("rhea", "milo", new NpcRelationship(0.8f, 0.7f, -0.2f));
            sourceWorld.SetRelationship("milo", "rhea", new NpcRelationship(0.2f, 0.4f, 0.6f));
            sourceWorld.SetPlayerRelationship("rhea", new NpcRelationship(0.6f, 0.8f, 0.3f));
            sourceWorld.SetPlayerRelationship("milo", new NpcRelationship(0.1f, 0.2f, -0.4f));
            Assert.That(sourceWorld.ShareKnowledge("milo", "rhea", "parcel-1", 2, 630), Is.True);
            var source = new World();
            SaveData data = SaveMapper.Capture(source.Stats, source.Inventory, null, source.Clock,
                source.Heat, source.Rent, "Hideout", 0f, 0f, socialWorld: sourceWorld);
            Assert.That(data.npcKnowledge.Count, Is.EqualTo(2));
            Assert.That(data.npcSocialRelationships.Count, Is.EqualTo(2));
            Assert.That(data.npcPlayerRelationships.Count, Is.EqualTo(3));
            var restoredWorld = new NpcSocialWorld();
            restoredWorld.RegisterNpc("milo", "Street", 0f, 0f, 8f);
            restoredWorld.RegisterNpc("rhea", "CornerShop", 0f, 0f, 8f);
            restoredWorld.RegisterNpc("customer", "Street", 0f, 0f, 8f);
            restoredWorld.PublishEvent(new NpcWorldEvent("stale-event", "Noise", "Later timeline noise.",
                "customer", "Street", 0f, 0f, 0f, 3, 900));
            restoredWorld.SetRelationship("rhea", "customer", new NpcRelationship(0.9f, 0.9f, 0.9f));
            LoadReport report = SaveMapper.Apply(data, new World().Stats, new GridInventory(), null,
                new GameClock(), new HeatMeter(), new RentLedger(50, 7), Resolver(), socialWorld: restoredWorld);
            Assert.That(report.Success, Is.True);
            Assert.That(report.SkippedNpcKnowledge, Is.Zero);
            Assert.That(report.SkippedNpcSocialRelationships, Is.Zero);
            Assert.That(restoredWorld.EventCount, Is.Zero, "The transient world journal must be cleared and not restored.");
            Assert.That(restoredWorld.BuildPerceptionFor("rhea", 8f).KnowsEvent("stale-event"), Is.False);
            Assert.That(restoredWorld.GetRelationship("rhea", "customer").Affinity, Is.Zero);
            Assert.That(restoredWorld.GetPlayerRelationship("rhea").Familiarity, Is.EqualTo(0.6f).Within(0.0001f));
            Assert.That(restoredWorld.GetPlayerRelationship("rhea").Trust, Is.EqualTo(0.8f).Within(0.0001f));
            Assert.That(restoredWorld.GetPlayerRelationship("rhea").Affinity, Is.EqualTo(0.3f).Within(0.0001f));
            Assert.That(restoredWorld.GetPlayerRelationship("milo").Affinity, Is.EqualTo(-0.4f).Within(0.0001f));
            NpcKnowledge sharedKnowledge = restoredWorld.BuildPerceptionFor("rhea", 8f).FindEvent("parcel-1");
            Assert.That(sharedKnowledge, Is.Not.Null);
            Assert.That(sharedKnowledge.Origin, Is.EqualTo(NpcKnowledgeOrigin.ToldByAnotherNpc));
            Assert.That(sharedKnowledge.LastSourceNpcId, Is.EqualTo("milo"));
            // ShareKnowledge uses listener trust: 1.0 * (0.35 + 0.7 * 0.5) = 0.70.
            Assert.That(sharedKnowledge.Confidence, Is.EqualTo(0.7f).Within(0.0001f));
            Assert.That(sharedKnowledge.LearnedMinuteOfDay, Is.EqualTo(630));
            Assert.That(restoredWorld.GetRelationship("rhea", "milo").Affinity, Is.EqualTo(-0.2f));
            Assert.That(restoredWorld.GetRelationship("milo", "rhea").Affinity, Is.EqualTo(0.6f));
        }

        [Test]
        public void NpcSocialLoadSkipsInvalidEntries()
        {
            var data = new SaveData();
            data.npcKnowledge.Add(new SavedNpcKnowledge { npcId = "removed-resident", eventId = "event", kind = "Rumour",
                eventLocationId = "Street", origin = (int)NpcKnowledgeOrigin.DirectObservation, confidence = 1f, occurredDay = 1, learnedDay = 1 });
            data.npcSocialRelationships.Add(new SavedNpcSocialRelationship { observerNpcId = "milo", otherNpcId = "removed-resident",
                familiarity = 0.5f, trust = 0.5f, affinity = 0f });
            data.npcPlayerRelationships.Add(new SavedNpcPlayerRelationship { npcId = "removed-resident",
                familiarity = 0.5f, trust = 0.5f, affinity = 0f });
            var world = new NpcSocialWorld();
            world.RegisterNpc("milo", "Street", 0f, 0f, 8f);
            LoadReport report = SaveMapper.Apply(data, new World().Stats, new GridInventory(), null,
                new GameClock(), new HeatMeter(), new RentLedger(50, 7), Resolver(), socialWorld: world);
            Assert.That(report.Success, Is.True);
            Assert.That(report.SkippedNpcKnowledge, Is.EqualTo(1));
            Assert.That(report.SkippedNpcSocialRelationships, Is.EqualTo(1));
            Assert.That(report.SkippedNpcPlayerRelationships, Is.EqualTo(1));
            Assert.That(world.BuildPerceptionFor("milo", 8f).KnownEvents, Is.Empty);
        }
        [Test]
        public void SaveDataStartsWithDefaultHideoutAndInitializedCollections()
        {
            var data = new SaveData();

            Assert.That(data.worldPlace, Is.EqualTo("Hideout"));
            Assert.That(data.items, Is.Not.Null);
            Assert.That(data.storageItems, Is.Not.Null);
            Assert.That(data.playerDiscoveries, Is.Not.Null);
            Assert.That(data.npcAgendaProgress, Is.Not.Null);
            Assert.That(data.npcPlayerRelationships, Is.Not.Null);
        }

        [Test]
        public void ApplyRejectsDuplicateStableItemIdAcrossPlayerInventoryAndHomeStorage()
        {
            ItemType type = Item(1, 1, 5);
            Guid duplicateId = Guid.NewGuid();
            var data = new SaveData();
            data.items.Add(new SavedItem
            {
                typeId = type.Id.ToString(), instanceId = duplicateId.ToString(),
                x = 0, y = 0, count = 1
            });
            data.storageItems.Add(new SavedItem
            {
                typeId = type.Id.ToString(), instanceId = duplicateId.ToString(),
                x = 0, y = 0, count = 1
            });

            var world = new World();
            var storage = new GridInventory();
            var ownership = new AssetOwnershipBook();
            LoadReport report = SaveMapper.Apply(data, world.Stats, world.Inventory, storage,
                world.Clock, world.Heat, world.Rent, Resolver(type), assetOwnership: ownership);

            Assert.That(report.Success, Is.True);
            Assert.That(report.SkippedItems, Is.Zero);
            Assert.That(report.SkippedStorageItems, Is.EqualTo(1));
            Assert.That(world.Inventory.TryGetItemInstance(duplicateId, out _), Is.True);
            Assert.That(storage.TryGetItemInstance(duplicateId, out _), Is.False);
            Assert.That(ownership.TryGetOwner(duplicateId, out EconomicActorId owner), Is.True);
            Assert.That(owner, Is.EqualTo(EconomicActorId.Player));
        }

        [Test]
        public void ApplySkipsOrphanedOwnershipRecordsWithoutPhysicalInventoryItems()
        {
            var data = new SaveData();
            data.assetOwners.Add(new SavedAssetOwner
            {
                instanceId = Guid.NewGuid().ToString(),
                actorId = EconomicActorId.Player.Value
            });
            var world = new World();
            var ownership = new AssetOwnershipBook();

            LoadReport report = SaveMapper.Apply(data, world.Stats, world.Inventory, null,
                world.Clock, world.Heat, world.Rent, Resolver(), assetOwnership: ownership);

            Assert.That(report.Success, Is.True);
            Assert.That(report.SkippedAssetOwners, Is.EqualTo(1));
            Assert.That(ownership.Count, Is.Zero);
        }

        [Test]
        public void ApplyRejectsAliasedInventoryRolesBeforeClearingAnyGrid()
        {
            var world = new World();
            ItemType existing = Item(1, 1, 1);
            world.Inventory.Place(existing, 0, 0);
            var data = new SaveData();

            Assert.Throws<ArgumentException>(() => SaveMapper.Apply(data, world.Stats,
                world.Inventory, world.Inventory, world.Clock, world.Heat, world.Rent, Resolver(existing)));

            Assert.That(world.Inventory.Items.Count, Is.EqualTo(1));
            Assert.That(world.Inventory.Items[0].Type, Is.EqualTo(existing));
        }

    }
}
