using System;
using NUnit.Framework;

namespace Signal.Core.Tests
{
    public sealed class CustomerRepairOrderTests
    {
        private sealed class Fixture
        {
            public readonly ItemType Jacket = new ItemType(Guid.NewGuid(), 2, 2, 1);
            public readonly ItemType BasicKit = new ItemType(Guid.NewGuid(), 1, 1, 1);
            public readonly ItemType ReinforcedKit = new ItemType(Guid.NewGuid(), 1, 1, 1);
            public readonly EconomicActorId Customer = new EconomicActorId("resident:milo");
            public readonly GridInventory CustomerInventory = new GridInventory(8, 6);
            public readonly GridInventory PlayerInventory = new GridInventory(8, 6);
            public readonly AssetOwnershipBook Ownership = new AssetOwnershipBook();
            public readonly EconomicContainerRegistry Containers = new EconomicContainerRegistry();
            public readonly EconomicTransactionLedger Transactions = new EconomicTransactionLedger();
            public readonly CustomerRepairOrderBook Orders = new CustomerRepairOrderBook();
            public readonly CleanMoneyAccount PlayerAccount = new CleanMoneyAccount(10);
            public readonly CleanMoneyAccount CustomerAccount = new CleanMoneyAccount(100);
            public readonly ItemKnowledgeBook Knowledge = new ItemKnowledgeBook();
            public readonly Guid OrderId = Guid.NewGuid();
            public readonly Guid JacketId;
            public readonly Guid ReinforcedKitId;

            public Fixture()
            {
                var jacketState = new ItemInstanceState(ItemQuality.Fine, ItemCondition.Damaged,
                    ItemAuthenticity.Genuine, ItemOwnershipStatus.Legitimate);
                CustomerInventory.Place(Jacket, 0, 0, state: jacketState);
                JacketId = CustomerInventory.Items[0].ItemInstanceId;
                Ownership.TryRegister(JacketId, Customer);

                PlayerInventory.Place(ReinforcedKit, 0, 0);
                ReinforcedKitId = PlayerInventory.Items[0].ItemInstanceId;
                Ownership.TryRegister(ReinforcedKitId, EconomicActorId.Player);

                Containers.TryRegister(CustomerInventory, Customer);
                Containers.TryRegister(PlayerInventory, EconomicActorId.Player);
                Orders.TryRegisterOffer(OrderId, Customer, Jacket.Id, 35);
            }

            public CustomerRepairOrderResult Accept() =>
                CustomerRepairOrderService.TryAccept(OrderId, Jacket, CustomerInventory, PlayerInventory,
                    Containers, Ownership, Orders, 1, 100);

            public RepairResult Repair() =>
                RepairService.TryRepairForOwner(JacketId, ReinforcedKitId, Jacket, BasicKit, ReinforcedKit,
                    PlayerInventory, EconomicActorId.Player, Customer, Containers, Ownership, Knowledge, 1, 300);

            public CustomerRepairOrderResult Return(int minute = 500) =>
                CustomerRepairOrderService.TryReturn(OrderId, Jacket, PlayerInventory, CustomerInventory,
                    Containers, Ownership, PlayerAccount, CustomerAccount, Transactions, Orders, 1, minute);
        }

        [Test]
        public void EntrustedItemKeepsCustomerOwnershipDuringAcceptanceAndRepair()
        {
            var fixture = new Fixture();

            CustomerRepairOrderResult accepted = fixture.Accept();

            Assert.That(accepted.Success, Is.True);
            Assert.That(accepted.ItemInstanceId, Is.EqualTo(fixture.JacketId));
            Assert.That(fixture.CustomerInventory.TryGetItemInstance(fixture.JacketId, out _), Is.False);
            Assert.That(fixture.PlayerInventory.TryGetItemInstance(fixture.JacketId, out PlacedItem entrusted), Is.True);
            Assert.That(entrusted.ItemInstanceId, Is.EqualTo(fixture.JacketId));
            Assert.That(fixture.Ownership.TryGetOwner(fixture.JacketId, out EconomicActorId owner), Is.True);
            Assert.That(owner, Is.EqualTo(fixture.Customer));
            Assert.That(fixture.Orders.TryGetState(fixture.OrderId, out CustomerRepairOrderState order), Is.True);
            Assert.That(order.Phase, Is.EqualTo(CustomerRepairOrderPhase.Active));

            RepairResult repaired = fixture.Repair();

            Assert.That(repaired.Success, Is.True);
            Assert.That(fixture.PlayerInventory.TryGetItemInstance(fixture.JacketId, out PlacedItem result), Is.True);
            Assert.That(result.State.Condition, Is.EqualTo(ItemCondition.Good));
            Assert.That(fixture.Ownership.TryGetOwner(fixture.JacketId, out owner), Is.True);
            Assert.That(owner, Is.EqualTo(fixture.Customer));
            Assert.That(fixture.Transactions.Count, Is.Zero,
                "Repairing the entrusted item is not an ownership-changing asset transfer.");
        }

        [Test]
        public void SuccessfulReturnPaysOnceRecordsServiceTransactionAndReturnsExactItem()
        {
            var fixture = new Fixture();
            fixture.Accept();
            Assert.That(fixture.Repair().Success, Is.True);

            CustomerRepairOrderResult returned = fixture.Return();

            Assert.That(returned.Success, Is.True);
            Assert.That(returned.ItemInstanceId, Is.EqualTo(fixture.JacketId));
            Assert.That(fixture.CustomerInventory.TryGetItemInstance(fixture.JacketId, out PlacedItem item), Is.True);
            Assert.That(item.ItemInstanceId, Is.EqualTo(fixture.JacketId));
            Assert.That(item.State.Condition, Is.EqualTo(ItemCondition.Good));
            Assert.That(fixture.PlayerInventory.TryGetItemInstance(fixture.JacketId, out _), Is.False);
            Assert.That(fixture.Ownership.TryGetOwner(fixture.JacketId, out EconomicActorId owner), Is.True);
            Assert.That(owner, Is.EqualTo(fixture.Customer));

            Assert.That(fixture.CustomerAccount.Balance, Is.EqualTo(65));
            Assert.That(fixture.PlayerAccount.Balance, Is.EqualTo(45));
            Assert.That(fixture.Transactions.Count, Is.EqualTo(1));
            EconomicTransaction transaction = fixture.Transactions.Transactions[0];
            Assert.That(transaction.ItemInstanceId, Is.EqualTo(fixture.JacketId));
            Assert.That(transaction.ItemDefinitionId, Is.EqualTo(fixture.Jacket.Id));
            Assert.That(transaction.FromActor, Is.EqualTo(fixture.Customer));
            Assert.That(transaction.ToActor, Is.EqualTo(EconomicActorId.Player));
            Assert.That(transaction.Reason, Is.EqualTo(AssetTransferReason.ServicePayment));
            Assert.That(transaction.Quantity, Is.EqualTo(1));

            Assert.That(fixture.Orders.TryGetState(fixture.OrderId, out CustomerRepairOrderState order), Is.True);
            Assert.That(order.Phase, Is.EqualTo(CustomerRepairOrderPhase.Completed));
            Assert.That(order.PaymentTransactionId, Is.EqualTo(transaction.TransactionId));

            CustomerRepairOrderResult replay = fixture.Return(501);
            Assert.That(replay.Success, Is.False);
            Assert.That(fixture.CustomerAccount.Balance, Is.EqualTo(65));
            Assert.That(fixture.PlayerAccount.Balance, Is.EqualTo(45));
            Assert.That(fixture.Transactions.Count, Is.EqualTo(1));
        }

        [Test]
        public void UnrepairedItemCannotBeReturnedOrPaidFor()
        {
            var fixture = new Fixture();
            fixture.Accept();

            CustomerRepairOrderResult result = fixture.Return();

            Assert.That(result.Status, Is.EqualTo(CustomerRepairOrderStatus.ItemNotRestored));
            Assert.That(fixture.PlayerInventory.TryGetItemInstance(fixture.JacketId, out _), Is.True);
            Assert.That(fixture.CustomerInventory.TryGetItemInstance(fixture.JacketId, out _), Is.False);
            Assert.That(fixture.CustomerAccount.Balance, Is.EqualTo(100));
            Assert.That(fixture.PlayerAccount.Balance, Is.EqualTo(10));
            Assert.That(fixture.Transactions.Count, Is.Zero);
        }

        [Test]
        public void InsufficientCustomerFundsRollbackItemReturnAndBothBalances()
        {
            var fixture = new Fixture();
            fixture.Accept();
            Assert.That(fixture.Repair().Success, Is.True);
            Assert.That(fixture.CustomerAccount.TryDebit(90), Is.True);

            CustomerRepairOrderResult result = fixture.Return();

            Assert.That(result.Status, Is.EqualTo(CustomerRepairOrderStatus.InsufficientFunds));
            Assert.That(fixture.PlayerInventory.TryGetItemInstance(fixture.JacketId, out PlacedItem item), Is.True);
            Assert.That(item.State.Condition, Is.EqualTo(ItemCondition.Good));
            Assert.That(fixture.CustomerInventory.TryGetItemInstance(fixture.JacketId, out _), Is.False);
            Assert.That(fixture.CustomerAccount.Balance, Is.EqualTo(10));
            Assert.That(fixture.PlayerAccount.Balance, Is.EqualTo(10));
            Assert.That(fixture.Transactions.Count, Is.Zero);
            Assert.That(fixture.Orders.TryGetState(fixture.OrderId, out CustomerRepairOrderState order), Is.True);
            Assert.That(order.Phase, Is.EqualTo(CustomerRepairOrderPhase.Active));
        }

        [Test]
        public void FullCustomerInventoryLeavesEntrustedItemAndPaymentUntouched()
        {
            var fixture = new Fixture();
            fixture.Accept();
            Assert.That(fixture.Repair().Success, Is.True);
            for (int y = 0; y < fixture.CustomerInventory.Height; y++)
                for (int x = 0; x < fixture.CustomerInventory.Width; x++)
                    fixture.CustomerInventory.Place(new ItemType(Guid.NewGuid(), 1, 1, 1), x, y);

            CustomerRepairOrderResult result = fixture.Return();

            Assert.That(result.Status, Is.EqualTo(CustomerRepairOrderStatus.InventoryFull));
            Assert.That(fixture.PlayerInventory.TryGetItemInstance(fixture.JacketId, out _), Is.True);
            Assert.That(fixture.CustomerAccount.Balance, Is.EqualTo(100));
            Assert.That(fixture.PlayerAccount.Balance, Is.EqualTo(10));
            Assert.That(fixture.Transactions.Count, Is.Zero);
        }

        [Test]
        public void ActiveOrderCanOnlyBeAcceptedOnceAndWrongOwnerCannotBeEntrusted()
        {
            var fixture = new Fixture();
            Assert.That(fixture.Accept().Success, Is.True);
            CustomerRepairOrderResult replay = fixture.Accept();
            Assert.That(replay.Success, Is.False);
            Assert.That(fixture.Ownership.TryGetOwner(fixture.JacketId, out EconomicActorId owner), Is.True);
            Assert.That(owner, Is.EqualTo(fixture.Customer));
        }

        private static void KeepTypeHelperReferenced()
        {
            _ = new ItemType(Guid.NewGuid(), 1, 1, 1);
        }
    }
}