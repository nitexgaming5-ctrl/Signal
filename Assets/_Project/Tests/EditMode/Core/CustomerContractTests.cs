using System;
using NUnit.Framework;

namespace Signal.Core.Tests
{
    public sealed class CustomerContractTests
    {
        private static ItemType Item() => new ItemType(Guid.NewGuid(), 1, 1, 10);

        private sealed class Fixture
        {
            public readonly ItemType Goods = Item();
            public readonly GridInventory PlayerInventory = new GridInventory();
            public readonly GridInventory CustomerInventory = new GridInventory(4, 4);
            public readonly AssetOwnershipBook Ownership = new AssetOwnershipBook();
            public readonly EconomicContainerRegistry Containers = new EconomicContainerRegistry();
            public readonly EconomicTransactionLedger Transactions = new EconomicTransactionLedger();
            public readonly CustomerContractBook Contracts = new CustomerContractBook();
            public readonly EconomicActorId Customer = new EconomicActorId("resident:customer");
            public readonly CleanMoneyAccount PlayerAccount = new CleanMoneyAccount(10);
            public readonly CleanMoneyAccount CustomerAccount = new CleanMoneyAccount(100);
            public readonly AssetTransferService Transfers;

            public Fixture(int count = 2, ItemInstanceState? state = null)
            {
                PlayerInventory.Place(Goods, 0, 0, count: count, state: state);
                Ownership.TryRegister(PlayerInventory.Items[0].ItemInstanceId, EconomicActorId.Player);
                Containers.TryRegister(PlayerInventory, EconomicActorId.Player);
                Containers.TryRegister(CustomerInventory, Customer);
                Transfers = new AssetTransferService(Ownership, Containers, Transactions);
                Contracts.TryRegisterOffer(ContractId);
                Contracts.TryAccept(ContractId, 1, 100);
            }

            public Guid ContractId = System.Guid.NewGuid();

            public CustomerContractFulfilmentResult Fulfil(int quantity = 2, int reward = 20, bool acceptStolen = false)
            {
                return CustomerContractService.TryFulfil(ContractId, PlayerInventory.Items[0].ItemInstanceId,
                    Goods, quantity, acceptStolen, reward, Customer, Customer, PlayerInventory,
                    CustomerInventory, Ownership, Transfers, Transactions, PlayerAccount, CustomerAccount,
                    Contracts, 1, 200);
            }
        }

        [Test]
        public void OnlyOneContractCanBeActiveAtATime()
        {
            var contracts = new CustomerContractBook();
            Guid first = Guid.NewGuid();
            Guid second = Guid.NewGuid();
            Assert.That(contracts.TryRegisterOffer(first), Is.True);
            Assert.That(contracts.TryRegisterOffer(second), Is.True);
            Assert.That(contracts.TryAccept(first, 1, 100), Is.True);
            Assert.That(contracts.TryAccept(second, 1, 101), Is.False);
            Assert.That(contracts.ActiveContractId, Is.EqualTo(first));
        }

        [Test]
        public void FulfilmentTransfersPhysicalGoodsAndPaysExactlyOnce()
        {
            var fixture = new Fixture();
            Guid itemId = fixture.PlayerInventory.Items[0].ItemInstanceId;

            CustomerContractFulfilmentResult result = fixture.Fulfil();

            Assert.That(result.Success, Is.True);
            Assert.That(fixture.PlayerInventory.CountOf(fixture.Goods), Is.Zero);
            Assert.That(fixture.CustomerInventory.CountOf(fixture.Goods), Is.EqualTo(2));
            Assert.That(fixture.PlayerAccount.Balance, Is.EqualTo(30));
            Assert.That(fixture.CustomerAccount.Balance, Is.EqualTo(80));
            Assert.That(fixture.Ownership.TryGetOwner(itemId, out EconomicActorId owner), Is.True);
            Assert.That(owner, Is.EqualTo(fixture.Customer));
            Assert.That(fixture.Transactions.Count, Is.EqualTo(1));
            Assert.That(fixture.Transactions.Transactions[0].Reason, Is.EqualTo(AssetTransferReason.ContractDelivery));
            Assert.That(fixture.Contracts.TryGetState(fixture.ContractId, out CustomerContractState state), Is.True);
            Assert.That(state.Phase, Is.EqualTo(CustomerContractPhase.Completed));

            CustomerContractFulfilmentResult replay = CustomerContractService.TryFulfil(fixture.ContractId, itemId,
                fixture.Goods, 2, false, 20, fixture.Customer, fixture.Customer, fixture.PlayerInventory,
                fixture.CustomerInventory, fixture.Ownership, fixture.Transfers, fixture.Transactions,
                fixture.PlayerAccount, fixture.CustomerAccount, fixture.Contracts, 1, 201);
            Assert.That(replay.Success, Is.False);
            Assert.That(fixture.PlayerAccount.Balance, Is.EqualTo(30));
            Assert.That(fixture.CustomerAccount.Balance, Is.EqualTo(80));
        }

        [Test]
        public void WrongQuantityLeavesItemsAndAccountsUntouched()
        {
            var fixture = new Fixture(count: 1);
            CustomerContractFulfilmentResult result = fixture.Fulfil(quantity: 2);

            Assert.That(result.Status, Is.EqualTo(CustomerContractFulfilmentStatus.WrongQuantity));
            Assert.That(fixture.PlayerInventory.CountOf(fixture.Goods), Is.EqualTo(1));
            Assert.That(fixture.CustomerInventory.CountOf(fixture.Goods), Is.Zero);
            Assert.That(fixture.PlayerAccount.Balance, Is.EqualTo(10));
            Assert.That(fixture.CustomerAccount.Balance, Is.EqualTo(100));
            Assert.That(fixture.Transactions.Count, Is.Zero);
        }

        [Test]
        public void InsufficientCustomerFundsLeaveItemsUntouched()
        {
            var fixture = new Fixture();
            fixture.CustomerAccount.TryDebit(90);
            CustomerContractFulfilmentResult result = fixture.Fulfil();

            Assert.That(result.Status, Is.EqualTo(CustomerContractFulfilmentStatus.InsufficientFunds));
            Assert.That(fixture.PlayerInventory.CountOf(fixture.Goods), Is.EqualTo(2));
            Assert.That(fixture.CustomerInventory.CountOf(fixture.Goods), Is.Zero);
            Assert.That(fixture.Transactions.Count, Is.Zero);
            Assert.That(fixture.PlayerAccount.Balance, Is.EqualTo(10));
        }

        [Test]
        public void StolenGoodsAreRejectedUnlessTheOfferExplicitlyAcceptsThem()
        {
            var stolen = new ItemInstanceState(ItemQuality.Standard, ItemCondition.Good,
                ItemAuthenticity.Genuine, ItemOwnershipStatus.Stolen);
            var fixture = new Fixture(state: stolen);
            CustomerContractFulfilmentResult result = fixture.Fulfil(acceptStolen: false);

            Assert.That(result.Status, Is.EqualTo(CustomerContractFulfilmentStatus.StolenGoodsNotAccepted));
            Assert.That(fixture.PlayerInventory.CountOf(fixture.Goods), Is.EqualTo(2));
            Assert.That(fixture.CustomerInventory.CountOf(fixture.Goods), Is.Zero);
        }

        [Test]
        public void ContractCannotBeCompletedTwice()
        {
            var fixture = new Fixture();
            CustomerContractFulfilmentResult result = fixture.Fulfil();
            Assert.That(result.Success, Is.True);
            Assert.That(fixture.Contracts.TryAccept(fixture.ContractId, 1, 300), Is.False);
            Assert.That(fixture.Contracts.ActiveContractId, Is.EqualTo(System.Guid.Empty));
        }
    }
}
