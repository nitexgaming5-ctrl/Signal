using System;
using NUnit.Framework;

namespace Signal.Core.Tests
{
    public sealed class ProductionServiceTests
    {
        private static ItemType Item(int width = 1, int height = 1, int maxStack = 10) =>
            new ItemType(Guid.NewGuid(), width, height, maxStack);

        private sealed class Fixture
        {
            public readonly ItemType Input = Item();
            public readonly ItemType Output = Item();
            public readonly GridInventory Inventory = new GridInventory();
            public readonly AssetOwnershipBook Ownership = new AssetOwnershipBook();
            public readonly EconomicContainerRegistry Containers = new EconomicContainerRegistry();
            public readonly ProductionTransactionLedger History = new ProductionTransactionLedger();
            public readonly EconomicActorId Operator = EconomicActorId.Player;

            public Fixture(int inputCount = 2)
            {
                Inventory.Place(Input, 0, 0, count: inputCount);
                Ownership.TryRegister(Inventory.Items[0].ItemInstanceId, Operator);
                Containers.TryRegister(Inventory, Operator);
            }

            public Recipe Recipe(int inputCount = 2, ItemType? output = null, int outputCount = 1)
            {
                return new Recipe(new[] { new Ingredient(Input, inputCount) },
                    new Ingredient(output ?? Output, outputCount));
            }

            public ProductionResult Produce(Recipe recipe) =>
                ProductionService.TryProduce(Guid.NewGuid(), recipe, Inventory, Operator,
                    Containers, Ownership, History, 1, 600);
        }

        [Test]
        public void ProductionConsumesOwnedInputsCreatesPhysicalOutputAndRecordsOperator()
        {
            var fixture = new Fixture();

            ProductionResult result = fixture.Produce(fixture.Recipe());

            Assert.That(result.Success, Is.True);
            Assert.That(fixture.Inventory.CountOf(fixture.Input), Is.Zero);
            Assert.That(fixture.Inventory.CountOf(fixture.Output), Is.EqualTo(1));
            Assert.That(fixture.History.Count, Is.EqualTo(1));
            Assert.That(result.Transaction.OperatorActor, Is.EqualTo(EconomicActorId.Player));
            Assert.That(fixture.Ownership.TryGetOwner(fixture.Inventory.Items[0].ItemInstanceId,
                out EconomicActorId owner), Is.True);
            Assert.That(owner, Is.EqualTo(EconomicActorId.Player));
        }

        [Test]
        public void MissingInputsLeaveInventoryOwnershipAndHistoryUnchanged()
        {
            var fixture = new Fixture(inputCount: 1);
            Guid instanceId = fixture.Inventory.Items[0].ItemInstanceId;

            ProductionResult result = fixture.Produce(fixture.Recipe(inputCount: 2));

            Assert.That(result.Status, Is.EqualTo(ProductionStatus.MissingInputs));
            Assert.That(fixture.Inventory.Items.Count, Is.EqualTo(1));
            Assert.That(fixture.Inventory.Items[0].ItemInstanceId, Is.EqualTo(instanceId));
            Assert.That(fixture.Ownership.TryGetOwner(instanceId, out EconomicActorId owner), Is.True);
            Assert.That(owner, Is.EqualTo(EconomicActorId.Player));
            Assert.That(fixture.History.Count, Is.Zero);
        }

        [Test]
        public void ProductionDoesNotConsumeGoodsOwnedByAnotherActor()
        {
            var fixture = new Fixture();
            Guid instanceId = fixture.Inventory.Items[0].ItemInstanceId;
            var otherOwner = new EconomicActorId("resident:someone-else");
            fixture.Ownership.TryTransferOwnership(instanceId, EconomicActorId.Player, otherOwner);

            ProductionResult result = fixture.Produce(fixture.Recipe());

            Assert.That(result.Status, Is.EqualTo(ProductionStatus.OwnershipMismatch));
            Assert.That(fixture.Inventory.Items.Count, Is.EqualTo(1));
            Assert.That(fixture.Inventory.Items[0].Count, Is.EqualTo(2));
            Assert.That(fixture.Ownership.TryGetOwner(instanceId, out EconomicActorId owner), Is.True);
            Assert.That(owner, Is.EqualTo(otherOwner));
            Assert.That(fixture.History.Count, Is.Zero);
        }

        [Test]
        public void OutputCapacityFailureRestoresIngredientsAndOwnership()
        {
            ItemType input = Item();
            ItemType oversizedOutput = Item(width: 2, height: 2);
            var inventory = new GridInventory(1, 1);
            inventory.Place(input, 0, 0, count: 2);
            Guid instanceId = inventory.Items[0].ItemInstanceId;
            var ownership = new AssetOwnershipBook();
            ownership.TryRegister(instanceId, EconomicActorId.Player);
            var containers = new EconomicContainerRegistry();
            containers.TryRegister(inventory, EconomicActorId.Player);
            var history = new ProductionTransactionLedger();
            var recipe = new Recipe(new[] { new Ingredient(input, 2) },
                new Ingredient(oversizedOutput, 1));

            ProductionResult result = ProductionService.TryProduce(Guid.NewGuid(), recipe, inventory,
                EconomicActorId.Player, containers, ownership, history, 1, 600);

            Assert.That(result.Status, Is.EqualTo(ProductionStatus.NoSpaceForOutput));
            Assert.That(inventory.Items.Count, Is.EqualTo(1));
            Assert.That(inventory.Items[0].ItemInstanceId, Is.EqualTo(instanceId));
            Assert.That(inventory.Items[0].Count, Is.EqualTo(2));
            Assert.That(ownership.TryGetOwner(instanceId, out EconomicActorId owner), Is.True);
            Assert.That(owner, Is.EqualTo(EconomicActorId.Player));
            Assert.That(history.Count, Is.Zero);
        }

        [Test]
        public void OperatorMustOwnTheProductionContainer()
        {
            var fixture = new Fixture();
            var other = new EconomicActorId("resident:workshop-owner");
            var wrongContainers = new EconomicContainerRegistry();
            wrongContainers.TryRegister(fixture.Inventory, other);

            ProductionResult result = ProductionService.TryProduce(Guid.NewGuid(), fixture.Recipe(),
                fixture.Inventory, EconomicActorId.Player, wrongContainers, fixture.Ownership,
                fixture.History, 1, 600);

            Assert.That(result.Status, Is.EqualTo(ProductionStatus.OwnershipMismatch));
            Assert.That(fixture.Inventory.CountOf(fixture.Input), Is.EqualTo(2));
            Assert.That(fixture.History.Count, Is.Zero);
        }

        [Test]
        public void NpcOwnedWorkshopProducesGoodsForItsOperator()
        {
            var actor = new EconomicActorId("resident:workshop-owner");
            ItemType input = Item();
            ItemType output = Item();
            var inventory = new GridInventory();
            inventory.Place(input, 0, 0, count: 2);
            Guid inputId = inventory.Items[0].ItemInstanceId;
            var ownership = new AssetOwnershipBook();
            ownership.TryRegister(inputId, actor);
            var containers = new EconomicContainerRegistry();
            containers.TryRegister(inventory, actor);
            var history = new ProductionTransactionLedger();
            var recipe = new Recipe(new[] { new Ingredient(input, 2) }, new Ingredient(output, 1));

            ProductionResult result = ProductionService.TryProduce(Guid.NewGuid(), recipe, inventory,
                actor, containers, ownership, history, 3, 480);

            Assert.That(result.Success, Is.True);
            Assert.That(inventory.CountOf(input), Is.Zero);
            Assert.That(inventory.CountOf(output), Is.EqualTo(1));
            Assert.That(result.Transaction.OperatorActor, Is.EqualTo(actor));
            Assert.That(ownership.TryGetOwner(inventory.Items[0].ItemInstanceId, out EconomicActorId owner), Is.True);
            Assert.That(owner, Is.EqualTo(actor));
        }

        [Test]
        public void DuplicateInputLinesAreAggregatedBeforeConsumption()
        {
            var fixture = new Fixture(inputCount: 4);
            var recipe = new Recipe(new[]
            {
                new Ingredient(fixture.Input, 1),
                new Ingredient(fixture.Input, 2)
            }, new Ingredient(fixture.Output, 1));

            ProductionResult result = fixture.Produce(recipe);

            Assert.That(result.Success, Is.True);
            Assert.That(fixture.Inventory.CountOf(fixture.Input), Is.EqualTo(1));
            Assert.That(fixture.Inventory.CountOf(fixture.Output), Is.EqualTo(1));
        }
        [Test]
        public void ProductionConsumesInputsAcrossBackpackAndPocketAndRoutesOutputToPocket()
        {
            var fixture = new Fixture(inputCount: 1);
            var pocket = new GridInventory(1, 1);
            pocket.Place(fixture.Input, 0, 0, count: 1);
            Guid pocketInputId = pocket.Items[0].ItemInstanceId;
            fixture.Ownership.TryRegister(pocketInputId, fixture.Operator);
            fixture.Containers.TryRegister(pocket, fixture.Operator);

            ProductionResult result = ProductionService.TryProduce(Guid.NewGuid(), fixture.Recipe(),
                new[] { fixture.Inventory, pocket }, pocket, fixture.Operator, fixture.Containers,
                fixture.Ownership, fixture.History, 1, 600);

            Assert.That(result.Success, Is.True);
            Assert.That(fixture.Inventory.CountOf(fixture.Input), Is.Zero);
            Assert.That(pocket.CountOf(fixture.Input), Is.Zero);
            Assert.That(pocket.CountOf(fixture.Output), Is.EqualTo(1));
            Assert.That(fixture.Ownership.TryGetOwner(pocket.Items[0].ItemInstanceId, out EconomicActorId owner), Is.True);
            Assert.That(owner, Is.EqualTo(fixture.Operator));
            Assert.That(fixture.History.Count, Is.EqualTo(1));
        }


    }
}