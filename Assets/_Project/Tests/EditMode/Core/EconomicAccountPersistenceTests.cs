using System;
using System.Collections.Generic;
using NUnit.Framework;

namespace Signal.Core.Tests
{
    public sealed class EconomicAccountPersistenceTests
    {
        private sealed class World
        {
            public PlayerStats Stats = new PlayerStats();
            public GridInventory Inventory = new GridInventory();
            public GameClock Clock = new GameClock();
            public HeatMeter Heat = new HeatMeter();
            public RentLedger Rent = new RentLedger(50, 7);
        }

        private static Func<Guid, ItemType?> Resolver(ItemType type)
        {
            return id => id == type.Id ? (ItemType?)type : null;
        }

        [Test]
        public void VersionEightMigratesToNineWithEmptyResidentAndRetailCollections()
        {
            var data = new SaveData
            {
                version = 8,
                cleanBalance = 275,
                residentAccounts = null,
                retailTransactions = null
            };

            Assert.That(SaveMigrator.TryMigrate(data, out _), Is.True);
            Assert.That(data.version, Is.EqualTo(9));
            Assert.That(data.cleanBalance, Is.EqualTo(275));
            Assert.That(data.residentAccounts, Is.Empty);
            Assert.That(data.retailTransactions, Is.Empty);
        }

        [Test]
        public void ResidentAccountsAndRetailTransactionsRoundTripAndOverrideAuthoredBalances()
        {
            var source = new World();
            source.Stats.Restore(500, 80);
            var accounts = new EconomicAccountBook();
            Assert.That(accounts.TryRegister(EconomicActorId.Player, source.Stats.CleanMoney), Is.True);
            var owner = new EconomicActorId("resident:baker");
            accounts.GetOrCreate(owner, 100);

            ItemType bread = new ItemType(Guid.NewGuid(), 1, 1, 10);
            var ownership = new AssetOwnershipBook();
            var retailLedger = new RetailTransactionLedger();
            RetailPurchaseResult purchase = new RetailService(accounts, ownership, retailLedger)
                .TryPurchase("old-bakery", source.Inventory, EconomicActorId.Player, owner,
                    bread, ItemInstanceState.Default, 2, 15, 3, 610);

            Assert.That(purchase.Success, Is.True);
            Assert.That(accounts.TryGetAccount(owner, out CleanMoneyAccount sourceOwner), Is.True);
            Assert.That(sourceOwner.Balance, Is.EqualTo(130));

            SaveData data = SaveMapper.Capture(source.Stats, source.Inventory, null,
                source.Clock, source.Heat, source.Rent, "CornerShop", 1f, 2f,
                null, null, null, null, ownership, null, accounts, retailLedger);

            Assert.That(data.version, Is.EqualTo(9));
            Assert.That(data.residentAccounts.Count, Is.EqualTo(1));
            Assert.That(data.residentAccounts[0].actorId, Is.EqualTo(owner.Value));
            Assert.That(data.residentAccounts[0].balance, Is.EqualTo(130));
            Assert.That(data.retailTransactions.Count, Is.EqualTo(1));

            var target = new World();
            var restoredAccounts = new EconomicAccountBook();
            Assert.That(restoredAccounts.TryRegister(EconomicActorId.Player, target.Stats.CleanMoney), Is.True);
            restoredAccounts.GetOrCreate(owner, 999);
            var restoredOwnership = new AssetOwnershipBook();
            var restoredRetailLedger = new RetailTransactionLedger();

            LoadReport report = SaveMapper.Apply(data, target.Stats, target.Inventory, null,
                target.Clock, target.Heat, target.Rent, Resolver(bread),
                null, null, null, restoredOwnership, null, restoredAccounts, restoredRetailLedger);

            Assert.That(report.Success, Is.True);
            Assert.That(report.SkippedResidentAccounts, Is.Zero);
            Assert.That(report.SkippedRetailTransactions, Is.Zero);
            Assert.That(target.Stats.CleanMoney.Balance, Is.EqualTo(470));
            Assert.That(restoredAccounts.TryGetAccount(owner, out CleanMoneyAccount restoredOwner), Is.True);
            Assert.That(restoredOwner.Balance, Is.EqualTo(130));
            Assert.That(target.Inventory.CountOf(bread), Is.EqualTo(2));
            Assert.That(restoredOwnership.TryGetOwner(target.Inventory.Items[0].ItemInstanceId, out EconomicActorId itemOwner), Is.True);
            Assert.That(itemOwner, Is.EqualTo(EconomicActorId.Player));
            Assert.That(restoredRetailLedger.Count, Is.EqualTo(1));
            RetailTransaction transaction = restoredRetailLedger.Transactions[0];
            Assert.That(transaction.TransactionId, Is.EqualTo(purchase.Transaction.TransactionId));
            Assert.That(transaction.BusinessId, Is.EqualTo("old-bakery"));
            Assert.That(transaction.TotalAmount, Is.EqualTo(30));
            Assert.That(transaction.Quantity, Is.EqualTo(2));
            Assert.That(transaction.Customer, Is.EqualTo(EconomicActorId.Player));
            Assert.That(transaction.Owner, Is.EqualTo(owner));
        }

        [Test]
        public void MalformedResidentAndRetailRecordsAreSkippedIndividually()
        {
            var data = new SaveData();
            data.residentAccounts.Add(new SavedEconomicAccount { actorId = "resident:baker", balance = -1 });
            data.residentAccounts.Add(new SavedEconomicAccount { actorId = "player", balance = 999 });
            data.retailTransactions.Add(new SavedRetailTransaction
            {
                transactionId = "bad-id",
                businessId = "old-bakery",
                customerActorId = "player",
                ownerActorId = "resident:baker",
                itemDefinitionId = Guid.NewGuid().ToString(),
                quantity = 1,
                totalAmount = 10,
                day = 1,
                minuteOfDay = 600
            });

            var target = new World();
            var accounts = new EconomicAccountBook();
            accounts.TryRegister(EconomicActorId.Player, target.Stats.CleanMoney);
            var ledger = new RetailTransactionLedger();

            LoadReport report = SaveMapper.Apply(data, target.Stats, target.Inventory, null,
                target.Clock, target.Heat, target.Rent, Resolver(new ItemType(Guid.NewGuid(), 1, 1, 1)),
                null, null, null, null, null, accounts, ledger);

            Assert.That(report.Success, Is.True);
            Assert.That(report.SkippedResidentAccounts, Is.EqualTo(2));
            Assert.That(report.SkippedRetailTransactions, Is.EqualTo(1));
            Assert.That(ledger.Count, Is.Zero);
            Assert.That(accounts.Count, Is.EqualTo(1));
        }
    }
}
