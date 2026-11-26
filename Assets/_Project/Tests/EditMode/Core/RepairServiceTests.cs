using System;
using NUnit.Framework;

namespace Signal.Core.Tests
{
    public sealed class RepairServiceTests
    {
        private sealed class Fixture
        {
            public readonly ItemType Jacket = ItemType(Guid.NewGuid(), 2, 2, 1);
            public readonly ItemType BasicKit = ItemType(Guid.NewGuid(), 1, 1, 1);
            public readonly ItemType ReinforcedKit = ItemType(Guid.NewGuid(), 1, 1, 1);
            public readonly GridInventory Inventory = new GridInventory(8, 6);
            public readonly AssetOwnershipBook Ownership = new AssetOwnershipBook();
            public readonly EconomicContainerRegistry Containers = new EconomicContainerRegistry();
            public readonly ItemKnowledgeBook Knowledge = new ItemKnowledgeBook();
            public readonly EconomicActorId Player = EconomicActorId.Player;
            public readonly Guid JacketId;
            public readonly Guid BasicKitId;
            public readonly Guid ReinforcedKitId;

            public Fixture(ItemCondition condition, ItemOwnershipStatus provenance = ItemOwnershipStatus.Legitimate)
            {
                var state = new ItemInstanceState(ItemQuality.Fine, condition,
                    ItemAuthenticity.Counterfeit, provenance);
                Inventory.Place(Jacket, 0, 0, rotated: true, state: state);
                Inventory.Place(BasicKit, 2, 0);
                Inventory.Place(ReinforcedKit, 3, 0);
                JacketId = Inventory.Items[0].ItemInstanceId;
                BasicKitId = Inventory.Items[1].ItemInstanceId;
                ReinforcedKitId = Inventory.Items[2].ItemInstanceId;
                Ownership.TryRegister(JacketId, Player);
                Ownership.TryRegister(BasicKitId, Player);
                Ownership.TryRegister(ReinforcedKitId, Player);
                Containers.TryRegister(Inventory, Player);

                Knowledge.RecordFact(JacketId, ItemKnowledgeAspect.Quality, (int)ItemQuality.Fine,
                    ItemKnowledgeSource.Document, 0.8f, 1, 500);
                Knowledge.RecordFact(JacketId, ItemKnowledgeAspect.Authenticity, (int)ItemAuthenticity.Counterfeit,
                    ItemKnowledgeSource.Rumour, 0.5f, 1, 500);
                Knowledge.RecordFact(JacketId, ItemKnowledgeAspect.Ownership, (int)provenance,
                    ItemKnowledgeSource.Document, 0.95f, 1, 500);
                Knowledge.RecordFact(JacketId, ItemKnowledgeAspect.Condition, (int)condition,
                    ItemKnowledgeSource.Rumour, 0.35f, 1, 500);
            }

            public RepairResult Repair(Guid kitId, int day = 2, int minute = 615) =>
                RepairService.TryRepair(JacketId, kitId, Jacket, BasicKit, ReinforcedKit, Inventory,
                    Player, Containers, Ownership, Knowledge, day, minute);
        }

        private static ItemType ItemType(Guid id, int width, int height, int maxStack) =>
            new ItemType(id, width, height, maxStack);

        [Test]
        public void BasicKitRepairsWornToGoodAndRefreshesOnlyConditionKnowledge()
        {
            var fixture = new Fixture(ItemCondition.Worn, ItemOwnershipStatus.Stolen);
            PlacedItem before = fixture.Inventory.Items[0];

            RepairResult result = fixture.Repair(fixture.BasicKitId);

            Assert.That(result.Success, Is.True);
            Assert.That(result.ResultingCondition, Is.EqualTo(ItemCondition.Good));
            Assert.That(fixture.Inventory.TryGetItemInstance(fixture.JacketId, out PlacedItem repaired), Is.True);
            Assert.That(repaired.ItemInstanceId, Is.EqualTo(before.ItemInstanceId));
            Assert.That(repaired.InstanceId, Is.EqualTo(before.InstanceId));
            Assert.That(repaired.X, Is.EqualTo(before.X));
            Assert.That(repaired.Y, Is.EqualTo(before.Y));
            Assert.That(repaired.Rotated, Is.EqualTo(before.Rotated));
            Assert.That(repaired.Count, Is.EqualTo(before.Count));
            Assert.That(repaired.State.Quality, Is.EqualTo(ItemQuality.Fine));
            Assert.That(repaired.State.Condition, Is.EqualTo(ItemCondition.Good));
            Assert.That(repaired.State.Authenticity, Is.EqualTo(ItemAuthenticity.Counterfeit));
            Assert.That(repaired.State.Ownership, Is.EqualTo(ItemOwnershipStatus.Stolen));
            Assert.That(fixture.Inventory.TryGetItemInstance(fixture.BasicKitId, out _), Is.False);
            Assert.That(fixture.Ownership.TryGetOwner(fixture.BasicKitId, out _), Is.False);
            Assert.That(fixture.Ownership.TryGetOwner(fixture.JacketId, out EconomicActorId owner), Is.True);
            Assert.That(owner, Is.EqualTo(EconomicActorId.Player));

            Assert.That(fixture.Knowledge.FactCount, Is.EqualTo(4));
            Assert.That(fixture.Knowledge.TryGetFact(fixture.JacketId, ItemKnowledgeAspect.Condition, out ItemKnowledgeFact condition), Is.True);
            Assert.That(condition.Value, Is.EqualTo((int)ItemCondition.Good));
            Assert.That(condition.Source, Is.EqualTo(ItemKnowledgeSource.DirectInspection));
            Assert.That(condition.Confidence, Is.EqualTo(1f));
            Assert.That(condition.LearnedDay, Is.EqualTo(2));
            Assert.That(condition.LearnedMinuteOfDay, Is.EqualTo(615));
            Assert.That(fixture.Knowledge.TryGetFact(fixture.JacketId, ItemKnowledgeAspect.Quality, out ItemKnowledgeFact quality), Is.True);
            Assert.That(quality.Source, Is.EqualTo(ItemKnowledgeSource.Document));
            Assert.That(fixture.Knowledge.TryGetFact(fixture.JacketId, ItemKnowledgeAspect.Authenticity, out ItemKnowledgeFact authenticity), Is.True);
            Assert.That(authenticity.Source, Is.EqualTo(ItemKnowledgeSource.Rumour));
            Assert.That(fixture.Knowledge.TryGetFact(fixture.JacketId, ItemKnowledgeAspect.Ownership, out ItemKnowledgeFact ownership), Is.True);
            Assert.That(ownership.Value, Is.EqualTo((int)ItemOwnershipStatus.Stolen));
        }

        [Test]
        public void BasicKitRepairsDamagedToWorn()
        {
            var fixture = new Fixture(ItemCondition.Damaged);

            RepairResult result = fixture.Repair(fixture.BasicKitId);

            Assert.That(result.Status, Is.EqualTo(RepairStatus.Completed));
            Assert.That(fixture.Inventory.TryGetItemInstance(fixture.JacketId, out PlacedItem jacket), Is.True);
            Assert.That(jacket.State.Condition, Is.EqualTo(ItemCondition.Worn));
        }

        [Test]
        public void ReinforcedKitRepairsDamagedDirectlyToGood()
        {
            var fixture = new Fixture(ItemCondition.Damaged);

            RepairResult result = fixture.Repair(fixture.ReinforcedKitId);

            Assert.That(result.Status, Is.EqualTo(RepairStatus.Completed));
            Assert.That(fixture.Inventory.TryGetItemInstance(fixture.JacketId, out PlacedItem jacket), Is.True);
            Assert.That(jacket.State.Condition, Is.EqualTo(ItemCondition.Good));
            Assert.That(fixture.Inventory.TryGetItemInstance(fixture.ReinforcedKitId, out _), Is.False);
            Assert.That(fixture.Inventory.TryGetItemInstance(fixture.BasicKitId, out _), Is.True);
        }

        [Test]
        public void ReinforcedKitCannotRepairWornJacketAndFailureConsumesNothing()
        {
            var fixture = new Fixture(ItemCondition.Worn);
            int itemCountBefore = fixture.Inventory.Items.Count;

            RepairResult result = fixture.Repair(fixture.ReinforcedKitId);

            Assert.That(result.Status, Is.EqualTo(RepairStatus.KitCannotRepairCondition));
            Assert.That(fixture.Inventory.Items.Count, Is.EqualTo(itemCountBefore));
            Assert.That(fixture.Inventory.TryGetItemInstance(fixture.JacketId, out PlacedItem jacket), Is.True);
            Assert.That(jacket.State.Condition, Is.EqualTo(ItemCondition.Worn));
            Assert.That(fixture.Inventory.TryGetItemInstance(fixture.ReinforcedKitId, out _), Is.True);
            Assert.That(fixture.Ownership.TryGetOwner(fixture.ReinforcedKitId, out _), Is.True);
            Assert.That(fixture.Knowledge.TryGetFact(fixture.JacketId, ItemKnowledgeAspect.Condition, out ItemKnowledgeFact fact), Is.True);
            Assert.That(fact.Source, Is.EqualTo(ItemKnowledgeSource.Rumour));
        }

        [TestCase(ItemCondition.Good)]
        [TestCase(ItemCondition.Pristine)]
        public void AlreadyRestoredJacketsCannotBeRepaired(ItemCondition condition)
        {
            var fixture = new Fixture(condition);

            RepairResult result = fixture.Repair(fixture.BasicKitId);

            Assert.That(result.Status, Is.EqualTo(RepairStatus.ConditionNotRepairable));
            Assert.That(fixture.Inventory.TryGetItemInstance(fixture.BasicKitId, out _), Is.True);
            Assert.That(fixture.Inventory.TryGetItemInstance(fixture.JacketId, out PlacedItem jacket), Is.True);
            Assert.That(jacket.State.Condition, Is.EqualTo(condition));
        }

        [Test]
        public void RepairServiceRejectsNonPlayerOperatorsInTheFirstSlice()
        {
            var fixture = new Fixture(ItemCondition.Damaged);
            var resident = new EconomicActorId("resident:rhea");

            RepairResult result = RepairService.TryRepair(fixture.JacketId, fixture.BasicKitId,
                fixture.Jacket, fixture.BasicKit, fixture.ReinforcedKit, fixture.Inventory, resident,
                fixture.Containers, fixture.Ownership, fixture.Knowledge, 2, 615);

            Assert.That(result.Status, Is.EqualTo(RepairStatus.InvalidRequest));
            Assert.That(fixture.Inventory.TryGetItemInstance(fixture.JacketId, out PlacedItem jacket), Is.True);
            Assert.That(jacket.State.Condition, Is.EqualTo(ItemCondition.Damaged));
            Assert.That(fixture.Inventory.TryGetItemInstance(fixture.BasicKitId, out _), Is.True);
        }

        [Test]
        public void RepairRequiresPlayerOwnedInventoryAndBothOwnedItems()
        {
            var fixture = new Fixture(ItemCondition.Damaged);
            var otherActor = new EconomicActorId("resident:rhea");
            var foreignContainers = new EconomicContainerRegistry();
            foreignContainers.TryRegister(fixture.Inventory, otherActor);

            RepairResult wrongContainer = RepairService.TryRepair(fixture.JacketId, fixture.BasicKitId,
                fixture.Jacket, fixture.BasicKit, fixture.ReinforcedKit, fixture.Inventory, fixture.Player,
                foreignContainers, fixture.Ownership, fixture.Knowledge, 2, 615);

            Assert.That(wrongContainer.Status, Is.EqualTo(RepairStatus.ContainerNotOwned));
            Assert.That(fixture.Ownership.TryTransferOwnership(fixture.BasicKitId, fixture.Player, otherActor), Is.True);
            RepairResult wrongKitOwner = fixture.Repair(fixture.BasicKitId);
            Assert.That(wrongKitOwner.Status, Is.EqualTo(RepairStatus.OwnershipMismatch));
            Assert.That(fixture.Inventory.TryGetItemInstance(fixture.JacketId, out PlacedItem jacket), Is.True);
            Assert.That(jacket.State.Condition, Is.EqualTo(ItemCondition.Damaged));
            Assert.That(fixture.Inventory.TryGetItemInstance(fixture.BasicKitId, out _), Is.True);
        }
        [Test]
        public void RepairCanConsumePatchKitFromPocketWhileTargetRemainsInBackpack()
        {
            var fixture = new Fixture(ItemCondition.Worn);
            var pocket = new GridInventory(1, 1);
            pocket.Place(fixture.BasicKit, 0, 0);
            Guid pocketKitId = pocket.Items[0].ItemInstanceId;
            fixture.Ownership.TryRegister(pocketKitId, fixture.Player);
            fixture.Containers.TryRegister(pocket, fixture.Player);

            RepairResult result = RepairService.TryRepairForOwner(fixture.JacketId, pocketKitId,
                fixture.Jacket, fixture.BasicKit, fixture.ReinforcedKit, fixture.Inventory, pocket,
                fixture.Player, fixture.Player, fixture.Containers, fixture.Ownership,
                fixture.Knowledge, 2, 615);

            Assert.That(result.Success, Is.True);
            Assert.That(fixture.Inventory.TryGetItemInstance(fixture.JacketId, out PlacedItem jacket), Is.True);
            Assert.That(jacket.State.Condition, Is.EqualTo(ItemCondition.Good));
            Assert.That(pocket.TryGetItemInstance(pocketKitId, out _), Is.False);
            Assert.That(fixture.Ownership.TryGetOwner(pocketKitId, out _), Is.False);
            Assert.That(fixture.Ownership.TryGetOwner(fixture.JacketId, out EconomicActorId owner), Is.True);
            Assert.That(owner, Is.EqualTo(fixture.Player));
        }


        [Test]
        public void RepairNotifiesObserversOnlyAfterKitAndTargetCommit()
        {
            var fixture = new Fixture(ItemCondition.Worn);
            bool observerSawCommittedState = false;
            fixture.Inventory.Changed += changedInventory =>
            {
                observerSawCommittedState =
                    fixture.Inventory.TryGetItemInstance(fixture.JacketId, out PlacedItem jacket) &&
                    jacket.State.Condition == ItemCondition.Good &&
                    !fixture.Inventory.TryGetItemInstance(fixture.BasicKitId, out _) &&
                    !fixture.Ownership.TryGetOwner(fixture.BasicKitId, out _) &&
                    fixture.Ownership.TryGetOwner(fixture.JacketId, out EconomicActorId owner) &&
                    owner == fixture.Player;
            };

            RepairResult result = fixture.Repair(fixture.BasicKitId);

            Assert.That(result.Success, Is.True);
            Assert.That(observerSawCommittedState, Is.True);
        }

    }
}
