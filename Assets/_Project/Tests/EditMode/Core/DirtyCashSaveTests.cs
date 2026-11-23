using System;
using System.Collections.Generic;
using NUnit.Framework;

namespace Signal.Core.Tests
{
    public sealed class DirtyCashSaveTests
    {
        [Test]
        public void ContactInventoryAndBothTransactionLedgersRoundTrip()
        {
            ItemType goods = new ItemType(Guid.NewGuid(), 1, 1, 5);
            ItemType cash = new ItemType(Guid.NewGuid(), 1, 1, 50);
            EconomicActorId contactActor = new EconomicActorId("resident:Clean_Dirtycash");
            var player = new GridInventory();
            player.Place(goods, 0, 0, count: 2, state: new ItemInstanceState(
                ItemQuality.Standard, ItemCondition.Good, ItemAuthenticity.Genuine, ItemOwnershipStatus.Stolen));
            PlacedItem stolen = player.Items[0];
            var contact = new GridInventory(8, 6);
            var ownership = new AssetOwnershipBook();
            ownership.TryRegister(stolen.ItemInstanceId, EconomicActorId.Player);
            var economicTransactions = new EconomicTransactionLedger();
            var exchanges = new IllicitExchangeTransactionLedger();
            var cleanings = new DirtyCashCleaningTransactionLedger();

            IllicitExchangeResult exchange = IllicitExchangeService.TryExchange(player, contact,
                stolen.ItemInstanceId, goods, 2, cash, 3, contactActor, ownership, economicTransactions, exchanges, 2, 600);
            Assert.That(exchange.Success, Is.True);
            PlacedItem cashStack = player.Items[0];
            var account = new CleanMoneyAccount();
            var sourceAccounts = new EconomicAccountBook();
            CleanMoneyAccount contactAccount = sourceAccounts.GetOrCreate(contactActor);
            DirtyCashCleaningResult cleaning = DirtyCashCleaningService.TryClean(player,
                cashStack.ItemInstanceId, cash, EconomicActorId.Player, ownership, account,
                contactAccount, contactActor, 20, cleanings, 2, 610);
            Assert.That(cleaning.Success, Is.True);

            var stats = new PlayerStats();
            stats.Restore(account.Balance, 100);
            var clock = new GameClock();
            var heat = new HeatMeter();
            var rent = new RentLedger(0, 7);
            SaveData data = SaveMapper.Capture(stats, player, null, clock, heat, rent,
                "Street", 0f, 0f, assetOwnership: ownership, economicTransactions: economicTransactions,
                economicAccounts: sourceAccounts, contactInventory: contact,
                illicitExchanges: exchanges, dirtyCashCleanings: cleanings);

            var targetStats = new PlayerStats();
            var targetPlayer = new GridInventory();
            var targetContact = new GridInventory(8, 6);
            var targetOwnership = new AssetOwnershipBook();
            var targetEconomicTransactions = new EconomicTransactionLedger();
            var targetAccounts = new EconomicAccountBook();
            var targetExchanges = new IllicitExchangeTransactionLedger();
            var targetCleanings = new DirtyCashCleaningTransactionLedger();
            LoadReport report = SaveMapper.Apply(data, targetStats, targetPlayer, null,
                new GameClock(), new HeatMeter(), new RentLedger(0, 7),
                id => id == goods.Id ? (ItemType?)goods : id == cash.Id ? (ItemType?)cash : null,
                assetOwnership: targetOwnership, economicTransactions: targetEconomicTransactions,
                economicAccounts: targetAccounts, illicitExchanges: targetExchanges,
                dirtyCashCleanings: targetCleanings, contactInventory: targetContact);

            Assert.That(report.Success, Is.True);
            Assert.That(report.SkippedContactItems, Is.Zero);
            Assert.That(targetContact.TryGetItemInstance(stolen.ItemInstanceId, out PlacedItem restored), Is.True);
            Assert.That(restored.Count, Is.EqualTo(2));
            Assert.That(restored.State.Ownership, Is.EqualTo(ItemOwnershipStatus.Stolen));
            Assert.That(targetOwnership.TryGetOwner(stolen.ItemInstanceId, out EconomicActorId owner), Is.True);
            Assert.That(owner, Is.EqualTo(contactActor));
            Assert.That(targetStats.CleanMoney.Balance, Is.EqualTo(24));
            Assert.That(targetAccounts.TryGetAccount(contactActor, out CleanMoneyAccount restoredContactAccount), Is.True);
            Assert.That(restoredContactAccount.Balance, Is.EqualTo(6));
            Assert.That(targetEconomicTransactions.Count, Is.EqualTo(1));
            Assert.That(targetEconomicTransactions.Transactions[0].Reason, Is.EqualTo(AssetTransferReason.IllicitExchange));
            Assert.That(targetExchanges.Count, Is.EqualTo(1));
            Assert.That(targetCleanings.Count, Is.EqualTo(1));
            Assert.That(targetCleanings.Transactions[0].CleanPayout, Is.EqualTo(24));
            Assert.That(targetCleanings.Transactions[0].FeeAmount, Is.EqualTo(6));
            Assert.That(targetCleanings.Transactions[0].FeeRecipient, Is.EqualTo(contactActor));
        }

        [Test]
        public void VersionThirteenMigratesWithEmptyDirtyCashState()
        {
            var data = new SaveData { version = 13, contactItems = null,
                illicitExchangeTransactions = null, dirtyCashCleaningTransactions = null };

            Assert.That(SaveMigrator.TryMigrate(data, out _), Is.True);
            Assert.That(data.version, Is.EqualTo(SaveMigrator.CurrentVersion));
            Assert.That(data.contactItems, Is.Empty);
            Assert.That(data.illicitExchangeTransactions, Is.Empty);
            Assert.That(data.dirtyCashCleaningTransactions, Is.Empty);
        }
    }
}
