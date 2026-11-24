using System;
using NUnit.Framework;

namespace Signal.Core.Tests
{
    public sealed class EconomicAccountSaveTests
    {
        [Test]
        [Test]
        public void ResidentAccountsAndRetailTransactionsRoundTrip()
        {
            var stats = new PlayerStats(250);
            var inventory = new GridInventory();
            var clock = new GameClock();
            var heat = new HeatMeter();
            var rent = new RentLedger(50, 7);
            var accounts = new EconomicAccountBook();
            accounts.TryRegister(EconomicActorId.Player, stats.CleanMoney);
            EconomicActorId owner = new EconomicActorId("resident:baker");
            accounts.GetOrCreate(owner, 80);
            var ownership = new AssetOwnershipBook();
            var assetLedger = new EconomicTransactionLedger();
            var retailLedger = new RetailTransactionLedger();
            var item = new ItemType(Guid.NewGuid(), 1, 1, 5);
            RetailPurchaseResult purchase = new RetailService(accounts, ownership, retailLedger).TryPurchase(
                "old-bakery", inventory, EconomicActorId.Player, owner, item,
                ItemInstanceState.Default, 2, 25, 2, 615);
            Assert.That(purchase.Success, Is.True);
            Assert.That(accounts.TryGetAccount(owner, out CleanMoneyAccount ownerAccount), Is.True);
            Assert.That(ownerAccount.Balance, Is.EqualTo(130));

            SaveData data = SaveMapper.Capture(stats, inventory, null, clock, heat, rent,
                "Hideout", 0f, 0f, null, null, null, null, ownership, assetLedger, accounts, retailLedger);
            Assert.That(data.residentAccounts.Count, Is.EqualTo(1));
            Assert.That(data.retailTransactions.Count, Is.EqualTo(1));

            var restoredStats = new PlayerStats();
            var restoredInventory = new GridInventory();
            var restoredAccounts = new EconomicAccountBook();
            restoredAccounts.TryRegister(EconomicActorId.Player, restoredStats.CleanMoney);
            restoredAccounts.GetOrCreate(owner, 999);
            var restoredOwnership = new AssetOwnershipBook();
            var restoredAssetLedger = new EconomicTransactionLedger();
            var restoredRetailLedger = new RetailTransactionLedger();

            LoadReport report = SaveMapper.Apply(data, restoredStats, restoredInventory, null,
                new GameClock(), new HeatMeter(), new RentLedger(50, 7),
                id => id == item.Id ? item : (ItemType?)null,
                null, null, null, restoredOwnership, restoredAssetLedger, restoredAccounts, restoredRetailLedger);

            Assert.That(report.Success, Is.True);
            Assert.That(report.SkippedResidentAccounts, Is.Zero);
            Assert.That(report.SkippedRetailTransactions, Is.Zero);
            Assert.That(restoredStats.CleanMoney.Balance, Is.EqualTo(200));
            Assert.That(restoredAccounts.TryGetAccount(owner, out CleanMoneyAccount restoredOwner), Is.True);
            Assert.That(restoredOwner.Balance, Is.EqualTo(130));
            Assert.That(restoredInventory.CountOf(item), Is.EqualTo(2));
            Assert.That(restoredRetailLedger.Count, Is.EqualTo(1));
            RetailTransaction transaction = restoredRetailLedger.Transactions[0];
            Assert.That(transaction.BusinessId, Is.EqualTo("old-bakery"));
            Assert.That(transaction.TotalAmount, Is.EqualTo(50));
            Assert.That(transaction.Customer, Is.EqualTo(EconomicActorId.Player));
            Assert.That(transaction.Owner, Is.EqualTo(owner));
        }

        [Test]
        public void MissingNewResidentKeepsItsAuthoredStartingBalanceAfterSaveLoad()
        {
            var stats = new PlayerStats();
            var data = new SaveData { money = 40, cleanBalance = 40 };

            var accounts = new EconomicAccountBook();
            accounts.TryRegister(EconomicActorId.Player, stats.CleanMoney);
            EconomicActorId newResident = new EconomicActorId("resident:new-neighbour");
            accounts.GetOrCreate(newResident, 75);

            LoadReport report = SaveMapper.Apply(data, stats, new GridInventory(), null,
                new GameClock(), new HeatMeter(), new RentLedger(50, 7), id => null,
                null, null, null, new AssetOwnershipBook(), new EconomicTransactionLedger(),
                accounts, new RetailTransactionLedger());

            Assert.That(report.Success, Is.True);
            Assert.That(accounts.TryGetAccount(newResident, out CleanMoneyAccount account), Is.True);
            Assert.That(account.Balance, Is.EqualTo(75));
        }

        [Test]
        public void SaveWithoutResidentAccountRecordsPreservesAuthoredStartingBalances()
        {
            var stats = new PlayerStats();
            var accounts = new EconomicAccountBook();
            accounts.TryRegister(EconomicActorId.Player, stats.CleanMoney);
            EconomicActorId resident = new EconomicActorId("resident:baker");
            CleanMoneyAccount residentAccount = accounts.GetOrCreate(resident, 60);
            residentAccount.TryDebit(20);

            var data = new SaveData { money = 10, cleanBalance = 10 };
            LoadReport report = SaveMapper.Apply(data, stats, new GridInventory(), null,
                new GameClock(), new HeatMeter(), new RentLedger(50, 7), id => null,
                null, null, null, new AssetOwnershipBook(), new EconomicTransactionLedger(),
                accounts, new RetailTransactionLedger());

            Assert.That(report.Success, Is.True);
            Assert.That(residentAccount.Balance, Is.EqualTo(60));
        }

        [Test]
        public void MalformedResidentAndRetailRecordsAreSkippedIndividually()
        {
            var data = new SaveData();
            data.residentAccounts.Add(new SavedEconomicAccount { actorId = "resident:good", balance = 25 });
            data.residentAccounts.Add(new SavedEconomicAccount { actorId = "resident:bad", balance = -1 });
            data.retailTransactions.Add(new SavedRetailTransaction
            {
                transactionId = "bad-guid",
                businessId = "old-bakery",
                customerActorId = "player",
                ownerActorId = "resident:baker",
                itemDefinitionId = Guid.NewGuid().ToString(),
                quantity = 1,
                totalAmount = 5,
                day = 1,
                minuteOfDay = 600
            });

            var stats = new PlayerStats();
            var accounts = new EconomicAccountBook();
            accounts.TryRegister(EconomicActorId.Player, stats.CleanMoney);
            var retail = new RetailTransactionLedger();
            LoadReport report = SaveMapper.Apply(data, stats, new GridInventory(), null,
                new GameClock(), new HeatMeter(), new RentLedger(50, 7), id => null,
                null, null, null, new AssetOwnershipBook(), new EconomicTransactionLedger(), accounts, retail);

            Assert.That(report.Success, Is.True);
            Assert.That(report.SkippedResidentAccounts, Is.EqualTo(1));
            Assert.That(report.SkippedRetailTransactions, Is.EqualTo(1));
            Assert.That(accounts.TryGetAccount(new EconomicActorId("resident:good"), out CleanMoneyAccount good), Is.True);
            Assert.That(good.Balance, Is.EqualTo(25));
            Assert.That(retail.Count, Is.Zero);
        }
    }
}
