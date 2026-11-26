using System;
using NUnit.Framework;

namespace Signal.Core.Tests
{
    public sealed class RetailServiceTests
    {
        private static readonly EconomicActorId Customer = EconomicActorId.Player;
        private static readonly EconomicActorId Owner = new EconomicActorId("resident:baker");

        private static RetailService CreateService(EconomicAccountBook accounts, AssetOwnershipBook ownership,
            RetailTransactionLedger ledger)
        {
            return new RetailService(accounts, ownership, ledger);
        }

        [Test]
        public void AccountBookCreatesEachResidentAccountOnlyOnce()
        {
            var accounts = new EconomicAccountBook();
            CleanMoneyAccount first = accounts.GetOrCreate(Owner, 125);
            CleanMoneyAccount second = accounts.GetOrCreate(Owner, 900);

            Assert.That(second, Is.SameAs(first));
            Assert.That(second.Balance, Is.EqualTo(125));
            Assert.That(accounts.Count, Is.EqualTo(1));
        }

        [Test]
        public void AccountBookCanShareTheExistingPlayerAccount()
        {
            var accounts = new EconomicAccountBook();
            var stats = new PlayerStats(250);

            Assert.That(accounts.TryRegister(Customer, stats.CleanMoney), Is.True);
            Assert.That(accounts.TryGetAccount(Customer, out CleanMoneyAccount resolved), Is.True);
            Assert.That(resolved, Is.SameAs(stats.CleanMoney));
        }

        [Test]
        public void AccountBookRejectsSharingOneBalanceBetweenDifferentActors()
        {
            var accounts = new EconomicAccountBook();
            var shared = new CleanMoneyAccount(50);
            Assert.That(accounts.TryRegister(Customer, shared), Is.True);
            Assert.That(accounts.TryRegister(Owner, shared), Is.False);
            Assert.That(accounts.Count, Is.EqualTo(1));
        }


        [Test]
        public void PurchaseDebitsCustomerCreditsOwnerGrantsGoodsAndRecordsTransaction()
        {
            var accounts = new EconomicAccountBook();
            var customerAccount = new CleanMoneyAccount(100);
            var ownerAccount = new CleanMoneyAccount(25);
            accounts.TryRegister(Customer, customerAccount);
            accounts.TryRegister(Owner, ownerAccount);
            var ownership = new AssetOwnershipBook();
            var ledger = new RetailTransactionLedger();
            var service = CreateService(accounts, ownership, ledger);
            var inventory = new GridInventory();
            var item = new ItemType(Guid.NewGuid(), 1, 1, 10);
            var state = ItemInstanceState.Default;

            RetailPurchaseResult result = service.TryPurchase("old-bakery", inventory, Customer, Owner,
                item, state, 2, 15, 3, 610);

            Assert.That(result.Success, Is.True);
            Assert.That(customerAccount.Balance, Is.EqualTo(70));
            Assert.That(ownerAccount.Balance, Is.EqualTo(55));
            Assert.That(inventory.CountOf(item, state), Is.EqualTo(2));
            Assert.That(ledger.Count, Is.EqualTo(1));
            Assert.That(result.Transaction.TotalAmount, Is.EqualTo(30));
            Assert.That(result.Transaction.BusinessId, Is.EqualTo("old-bakery"));
            Assert.That(result.Transaction.Customer, Is.EqualTo(Customer));
            Assert.That(result.Transaction.Owner, Is.EqualTo(Owner));
            Assert.That(result.Transaction.ItemDefinitionId, Is.EqualTo(item.Id));
            Assert.That(result.Transaction.Quantity, Is.EqualTo(2));
            Assert.That(result.Transaction.Day, Is.EqualTo(3));
            Assert.That(result.Transaction.MinuteOfDay, Is.EqualTo(610));
            Assert.That(ownership.TryGetOwner(inventory.Items[0].ItemInstanceId, out EconomicActorId itemOwner), Is.True);
            Assert.That(itemOwner, Is.EqualTo(Customer));
        }

        [Test]
        public void PurchaseCanTopUpAnAlreadyOwnedMatchingStack()
        {
            var accounts = new EconomicAccountBook();
            accounts.TryRegister(Customer, new CleanMoneyAccount(50));
            accounts.TryRegister(Owner, new CleanMoneyAccount(0));
            var ownership = new AssetOwnershipBook();
            var ledger = new RetailTransactionLedger();
            var inventory = new GridInventory();
            var item = new ItemType(Guid.NewGuid(), 1, 1, 10);
            inventory.Place(item, 0, 0, count: 2);
            Guid existingId = inventory.Items[0].ItemInstanceId;
            ownership.TryRegister(existingId, Customer);

            RetailPurchaseResult result = CreateService(accounts, ownership, ledger).TryPurchase(
                "old-bakery", inventory, Customer, Owner, item, ItemInstanceState.Default, 3, 5, 1, 600);

            Assert.That(result.Success, Is.True);
            Assert.That(inventory.Items.Count, Is.EqualTo(1));
            Assert.That(inventory.Items[0].ItemInstanceId, Is.EqualTo(existingId));
            Assert.That(inventory.CountOf(item), Is.EqualTo(5));
        }

        [Test]
        public void InsufficientFundsChangesNothing()
        {
            var accounts = new EconomicAccountBook();
            var customer = new CleanMoneyAccount(4);
            var owner = new CleanMoneyAccount(10);
            accounts.TryRegister(Customer, customer);
            accounts.TryRegister(Owner, owner);
            var inventory = new GridInventory();
            var item = new ItemType(Guid.NewGuid(), 1, 1, 5);

            RetailPurchaseResult result = CreateService(accounts, new AssetOwnershipBook(),
                new RetailTransactionLedger()).TryPurchase("old-bakery", inventory, Customer, Owner,
                item, ItemInstanceState.Default, 1, 5, 1, 600);

            Assert.That(result.Status, Is.EqualTo(RetailPurchaseStatus.InsufficientFunds));
            Assert.That(customer.Balance, Is.EqualTo(4));
            Assert.That(owner.Balance, Is.EqualTo(10));
            Assert.That(inventory.Items, Is.Empty);
        }

        [Test]
        public void FullInventoryRollsBackBothAccounts()
        {
            var accounts = new EconomicAccountBook();
            var customer = new CleanMoneyAccount(100);
            var owner = new CleanMoneyAccount(0);
            accounts.TryRegister(Customer, customer);
            accounts.TryRegister(Owner, owner);
            var inventory = new GridInventory(1, 1);
            inventory.Place(new ItemType(Guid.NewGuid(), 1, 1, 1), 0, 0);
            var ledger = new RetailTransactionLedger();

            RetailPurchaseResult result = CreateService(accounts, new AssetOwnershipBook(), ledger).TryPurchase(
                "old-bakery", inventory, Customer, Owner, new ItemType(Guid.NewGuid(), 1, 1, 1),
                ItemInstanceState.Default, 1, 20, 1, 600);

            Assert.That(result.Status, Is.EqualTo(RetailPurchaseStatus.InventoryFull));
            Assert.That(customer.Balance, Is.EqualTo(100));
            Assert.That(owner.Balance, Is.EqualTo(0));
            Assert.That(inventory.Items.Count, Is.EqualTo(1));
            Assert.That(ledger.Count, Is.Zero);
        }

        [Test]
        public void OwnerAccountOverflowRollsBackCustomerDebit()
        {
            var accounts = new EconomicAccountBook();
            var customer = new CleanMoneyAccount(100);
            var owner = new CleanMoneyAccount(int.MaxValue);
            accounts.TryRegister(Customer, customer);
            accounts.TryRegister(Owner, owner);
            var inventory = new GridInventory();
            var ledger = new RetailTransactionLedger();

            RetailPurchaseResult result = CreateService(accounts, new AssetOwnershipBook(), ledger).TryPurchase(
                "old-bakery", inventory, Customer, Owner, new ItemType(Guid.NewGuid(), 1, 1, 1),
                ItemInstanceState.Default, 1, 10, 1, 600);

            Assert.That(result.Status, Is.EqualTo(RetailPurchaseStatus.AccountOverflow));
            Assert.That(customer.Balance, Is.EqualTo(100));
            Assert.That(owner.Balance, Is.EqualTo(int.MaxValue));
            Assert.That(inventory.Items, Is.Empty);
            Assert.That(ledger.Count, Is.Zero);
        }

        [Test]
        public void ExistingMatchingStackWithDifferentOwnerIsRejected()
        {
            var accounts = new EconomicAccountBook();
            accounts.TryRegister(Customer, new CleanMoneyAccount(100));
            accounts.TryRegister(Owner, new CleanMoneyAccount(0));
            var inventory = new GridInventory();
            var item = new ItemType(Guid.NewGuid(), 1, 1, 5);
            inventory.Place(item, 0, 0);
            var ownership = new AssetOwnershipBook();
            ownership.TryRegister(inventory.Items[0].ItemInstanceId, Owner);

            RetailPurchaseResult result = CreateService(accounts, ownership, new RetailTransactionLedger())
                .TryPurchase("old-bakery", inventory, Customer, Owner, item, ItemInstanceState.Default, 1, 10, 1, 600);

            Assert.That(result.Status, Is.EqualTo(RetailPurchaseStatus.OwnershipInvalid));
            Assert.That(inventory.CountOf(item), Is.EqualTo(1));
        }

        [TestCase(0, 10)]
        [TestCase(-1, 10)]
        [TestCase(1, 0)]
        [TestCase(1, -1)]
        [TestCase(int.MaxValue, 2)]
        public void InvalidQuantityOrPriceNeverChangesPurchaseState(int quantity, int unitPrice)
        {
            var accounts = new EconomicAccountBook();
            var customer = new CleanMoneyAccount(100);
            var owner = new CleanMoneyAccount(25);
            accounts.TryRegister(Customer, customer);
            accounts.TryRegister(Owner, owner);
            var ownership = new AssetOwnershipBook();
            var ledger = new RetailTransactionLedger();
            var inventory = new GridInventory();
            var item = new ItemType(Guid.NewGuid(), 1, 1, 10);

            RetailPurchaseResult result = CreateService(accounts, ownership, ledger).TryPurchase(
                "old-bakery", inventory, Customer, Owner, item, ItemInstanceState.Default,
                quantity, unitPrice, 1, 600);

            Assert.That(result.Status, Is.EqualTo(RetailPurchaseStatus.InvalidRequest));
            Assert.That(customer.Balance, Is.EqualTo(100));
            Assert.That(owner.Balance, Is.EqualTo(25));
            Assert.That(inventory.Items, Is.Empty);
            Assert.That(ledger.Count, Is.Zero);
            Assert.That(ownership.Count, Is.Zero);
        }

        [Test]
        public void InvalidRequestAndMissingAccountsAreRejected()
        {
            var accounts = new EconomicAccountBook();
            var service = CreateService(accounts, new AssetOwnershipBook(), new RetailTransactionLedger());
            var inventory = new GridInventory();
            var item = new ItemType(Guid.NewGuid(), 1, 1, 5);

            Assert.That(service.TryPurchase("shop", inventory, Customer, Owner, item,
                ItemInstanceState.Default, 1, 10, 1, 600).Status, Is.EqualTo(RetailPurchaseStatus.AccountNotFound));
            Assert.That(service.TryPurchase("", inventory, Customer, Owner, item,
                ItemInstanceState.Default, 1, 10, 1, 600).Status, Is.EqualTo(RetailPurchaseStatus.InvalidRequest));
            Assert.That(service.TryPurchase("shop", inventory, Customer, Owner, item,
                ItemInstanceState.Default, 1, int.MaxValue, 1, 600).Status, Is.EqualTo(RetailPurchaseStatus.AccountNotFound));
        }

        [Test]
        public void PhysicalDirtyCashCannotFundRetailPurchaseAndRemainsUntouched()
        {
            var accounts = new EconomicAccountBook();
            var customer = new CleanMoneyAccount();
            var owner = new CleanMoneyAccount();
            accounts.TryRegister(Customer, customer);
            accounts.TryRegister(Owner, owner);
            var ownership = new AssetOwnershipBook();
            var ledger = new RetailTransactionLedger();
            var inventory = new GridInventory();
            var dirtyCash = new ItemType(Guid.NewGuid(), 1, 1, 50);
            var retailGood = new ItemType(Guid.NewGuid(), 1, 1, 5);
            inventory.Place(dirtyCash, 0, 0, count: 3);
            PlacedItem cashStack = inventory.Items[0];
            ownership.TryRegister(cashStack.ItemInstanceId, Customer);

            RetailPurchaseResult result = CreateService(accounts, ownership, ledger).TryPurchase(
                "old-bakery", inventory, Customer, Owner, retailGood, ItemInstanceState.Default,
                1, 1, 1, 600);

            Assert.That(result.Status, Is.EqualTo(RetailPurchaseStatus.InsufficientFunds));
            Assert.That(customer.Balance, Is.Zero);
            Assert.That(owner.Balance, Is.Zero);
            Assert.That(inventory.TryGetItemInstance(cashStack.ItemInstanceId, out PlacedItem unchanged), Is.True);
            Assert.That(unchanged.Count, Is.EqualTo(3));
            Assert.That(unchanged.Type, Is.EqualTo(dirtyCash));
            Assert.That(ownership.TryGetOwner(cashStack.ItemInstanceId, out EconomicActorId cashOwner), Is.True);
            Assert.That(cashOwner, Is.EqualTo(Customer));
            Assert.That(ledger.Count, Is.Zero);
        }

        [Test]
        public void PurchaseNotifiesInventoryOnlyAfterBalancesOwnershipAndLedgerCommit()
        {
            var accounts = new EconomicAccountBook();
            var customerAccount = new CleanMoneyAccount(100);
            var ownerAccount = new CleanMoneyAccount(25);
            accounts.TryRegister(Customer, customerAccount);
            accounts.TryRegister(Owner, ownerAccount);
            var ownership = new AssetOwnershipBook();
            var ledger = new RetailTransactionLedger();
            var inventory = new GridInventory();
            var item = new ItemType(Guid.NewGuid(), 1, 1, 10);
            bool observerSawCommittedState = false;
            inventory.Changed += _ =>
            {
                observerSawCommittedState =
                    inventory.CountOf(item) == 2 &&
                    customerAccount.Balance == 70 &&
                    ownerAccount.Balance == 55 &&
                    ledger.Count == 1 &&
                    inventory.Items.Count == 1 &&
                    ownership.TryGetOwner(inventory.Items[0].ItemInstanceId, out EconomicActorId owner) &&
                    owner == Customer;
            };

            RetailPurchaseResult result = CreateService(accounts, ownership, ledger).TryPurchase(
                "old-bakery", inventory, Customer, Owner, item, ItemInstanceState.Default, 2, 15, 3, 610);

            Assert.That(result.Success, Is.True);
            Assert.That(observerSawCommittedState, Is.True);
        }

    }
}
