using System;
using NUnit.Framework;

namespace Signal.Core.Tests
{
    public sealed class DirtyCashEconomyTests
    {
        private static readonly EconomicActorId Player = EconomicActorId.Player;
        private static readonly EconomicActorId Contact = new EconomicActorId("resident:Clean_Dirtycash");
        private static ItemType Item() => new ItemType(Guid.NewGuid(), 1, 1, 5);
        private static ItemInstanceState Stolen => new ItemInstanceState(ItemQuality.Standard,
            ItemCondition.Good, ItemAuthenticity.Genuine, ItemOwnershipStatus.Stolen);

        [Test]
        public void FenceExchangeTransfersExactStolenStackAndCreatesOwnedDirtyCash()
        {
            ItemType accepted = Item();
            ItemType cash = Item();
            var player = new GridInventory(3, 2);
            player.Place(accepted, 0, 0, count: 2, state: Stolen);
            PlacedItem stolen = player.Items[0];
            var contact = new GridInventory(3, 2);
            var ownership = new AssetOwnershipBook();
            ownership.TryRegister(stolen.ItemInstanceId, Player);
            var ledger = new IllicitExchangeTransactionLedger();
            var economicLedger = new EconomicTransactionLedger();

            IllicitExchangeResult result = IllicitExchangeService.TryExchange(player, contact,
                stolen.ItemInstanceId, accepted, 2, cash, 3, Contact, ownership, economicLedger, ledger, 2, 600);

            Assert.That(result.Success, Is.True);
            Assert.That(player.CountOf(accepted), Is.Zero);
            Assert.That(player.CountOf(cash), Is.EqualTo(3));
            Assert.That(contact.TryGetItemInstance(stolen.ItemInstanceId, out PlacedItem transferred), Is.True);
            Assert.That(transferred.State, Is.EqualTo(Stolen));
            Assert.That(transferred.Count, Is.EqualTo(2));
            Assert.That(ownership.TryGetOwner(stolen.ItemInstanceId, out EconomicActorId owner), Is.True);
            Assert.That(owner, Is.EqualTo(Contact));
            PlacedItem reward = player.Items[0];
            Assert.That(ownership.TryGetOwner(reward.ItemInstanceId, out owner), Is.True);
            Assert.That(owner, Is.EqualTo(Player));
            Assert.That(ledger.Count, Is.EqualTo(1));
            Assert.That(ledger.Transactions[0].DirtyCashUnits, Is.EqualTo(3));
            Assert.That(economicLedger.Count, Is.EqualTo(1));
            Assert.That(economicLedger.Transactions[0].Reason, Is.EqualTo(AssetTransferReason.IllicitExchange));
        }

        [Test]
        public void FenceRejectsLegitimateCopyWithoutMutation()
        {
            ItemType accepted = Item();
            ItemType cash = Item();
            var player = new GridInventory();
            player.Place(accepted, 0, 0, count: 2);
            PlacedItem stack = player.Items[0];
            var contact = new GridInventory();
            var ownership = new AssetOwnershipBook();
            ownership.TryRegister(stack.ItemInstanceId, Player);
            var ledger = new IllicitExchangeTransactionLedger();
            var economicLedger = new EconomicTransactionLedger();

            IllicitExchangeResult result = IllicitExchangeService.TryExchange(player, contact,
                stack.ItemInstanceId, accepted, 2, cash, 3, Contact, ownership, economicLedger, ledger, 1, 600);

            Assert.That(result.Status, Is.EqualTo(IllicitExchangeStatus.ItemNotStolen));
            Assert.That(player.CountOf(accepted), Is.EqualTo(2));
            Assert.That(contact.Items, Is.Empty);
            Assert.That(ledger.Count, Is.Zero);
            Assert.That(economicLedger.Count, Is.Zero);
        }

        [Test]
        public void FenceRejectsWhenContactCannotStoreItem()
        {
            ItemType accepted = Item();
            ItemType cash = Item();
            var player = new GridInventory();
            player.Place(accepted, 0, 0, count: 2, state: Stolen);
            PlacedItem stack = player.Items[0];
            var contact = new GridInventory(1, 1);
            contact.Place(Item(), 0, 0);
            var ownership = new AssetOwnershipBook();
            ownership.TryRegister(stack.ItemInstanceId, Player);
            var ledger = new IllicitExchangeTransactionLedger();
            var economicLedger = new EconomicTransactionLedger();

            IllicitExchangeResult result = IllicitExchangeService.TryExchange(player, contact,
                stack.ItemInstanceId, accepted, 2, cash, 3, Contact, ownership, economicLedger, ledger, 1, 600);

            Assert.That(result.Success, Is.False);
            Assert.That(player.TryGetItemInstance(stack.ItemInstanceId, out PlacedItem unchanged), Is.True);
            Assert.That(unchanged.Count, Is.EqualTo(2));
            Assert.That(ownership.TryGetOwner(stack.ItemInstanceId, out EconomicActorId owner), Is.True);
            Assert.That(owner, Is.EqualTo(Player));
            Assert.That(player.CountOf(cash), Is.Zero);
            Assert.That(ledger.Count, Is.Zero);
            Assert.That(economicLedger.Count, Is.Zero);
        }

        [Test]
        public void FenceRollsBackBothInventoriesWhenRewardCannotFit()
        {
            ItemType accepted = new ItemType(Guid.NewGuid(), 1, 1, 1);
            ItemType cash = new ItemType(Guid.NewGuid(), 2, 2, 5);
            ItemType filler = new ItemType(Guid.NewGuid(), 1, 1, 1);
            var player = new GridInventory(2, 2);
            player.Place(accepted, 0, 0, count: 1, state: Stolen);
            PlacedItem stolen = player.Items[0];
            player.Place(filler, 1, 0);
            player.Place(filler, 0, 1);
            player.Place(filler, 1, 1);
            var contact = new GridInventory(2, 2);
            var ownership = new AssetOwnershipBook();
            ownership.TryRegister(stolen.ItemInstanceId, Player);
            var ledger = new IllicitExchangeTransactionLedger();
            var economicLedger = new EconomicTransactionLedger();

            IllicitExchangeResult result = IllicitExchangeService.TryExchange(player, contact,
                stolen.ItemInstanceId, accepted, 1, cash, 1, Contact, ownership, economicLedger, ledger, 1, 600);

            Assert.That(result.Status, Is.EqualTo(IllicitExchangeStatus.RewardInventoryFull));
            Assert.That(player.TryGetItemInstance(stolen.ItemInstanceId, out PlacedItem restored), Is.True);
            Assert.That(restored.State.Ownership, Is.EqualTo(ItemOwnershipStatus.Stolen));
            Assert.That(contact.Items, Is.Empty);
            Assert.That(ownership.TryGetOwner(stolen.ItemInstanceId, out EconomicActorId owner), Is.True);
            Assert.That(owner, Is.EqualTo(Player));
            Assert.That(ledger.Count, Is.Zero);
            Assert.That(economicLedger.Count, Is.Zero);
        }

        [Test]
        public void CleaningConsumesWholeStackAndCreditsEightPerUnit()
        {
            ItemType cash = Item();
            var inventory = new GridInventory();
            inventory.Place(cash, 0, 0, count: 3);
            PlacedItem stack = inventory.Items[0];
            var ownership = new AssetOwnershipBook();
            ownership.TryRegister(stack.ItemInstanceId, Player);
            var account = new CleanMoneyAccount(11);
            var feeAccount = new CleanMoneyAccount();
            var ledger = new DirtyCashCleaningTransactionLedger();

            DirtyCashCleaningResult result = DirtyCashCleaningService.TryClean(inventory,
                stack.ItemInstanceId, cash, Player, ownership, account, feeAccount, Contact, 20, ledger, 3, 720);

            Assert.That(result.Success, Is.True);
            Assert.That(inventory.CountOf(cash), Is.Zero);
            Assert.That(ownership.TryGetOwner(stack.ItemInstanceId, out _), Is.False);
            Assert.That(account.Balance, Is.EqualTo(35));
            Assert.That(feeAccount.Balance, Is.EqualTo(6));
            Assert.That(ledger.Count, Is.EqualTo(1));
            Assert.That(ledger.Transactions[0].DirtyUnits, Is.EqualTo(3));
            Assert.That(ledger.Transactions[0].CleanPayout, Is.EqualTo(24));
            Assert.That(ledger.Transactions[0].FeePercent, Is.EqualTo(20));
            Assert.That(ledger.Transactions[0].FeeAmount, Is.EqualTo(6));
            Assert.That(ledger.Transactions[0].FeeRecipient, Is.EqualTo(Contact));
        }

        [Test]
        public void CleaningOverflowRollsBackItemOwnershipAndBalance()
        {
            ItemType cash = Item();
            var inventory = new GridInventory();
            inventory.Place(cash, 0, 0, count: 1);
            PlacedItem stack = inventory.Items[0];
            var ownership = new AssetOwnershipBook();
            ownership.TryRegister(stack.ItemInstanceId, Player);
            var account = new CleanMoneyAccount(10);
            var feeAccount = new CleanMoneyAccount(int.MaxValue);
            var ledger = new DirtyCashCleaningTransactionLedger();

            DirtyCashCleaningResult result = DirtyCashCleaningService.TryClean(inventory,
                stack.ItemInstanceId, cash, Player, ownership, account, feeAccount, Contact, 20, ledger, 1, 720);

            Assert.That(result.Status, Is.EqualTo(DirtyCashCleaningStatus.AccountOverflow));
            Assert.That(inventory.TryGetItemInstance(stack.ItemInstanceId, out PlacedItem unchanged), Is.True);
            Assert.That(unchanged.Count, Is.EqualTo(1));
            Assert.That(ownership.TryGetOwner(stack.ItemInstanceId, out EconomicActorId owner), Is.True);
            Assert.That(owner, Is.EqualTo(Player));
            Assert.That(account.Balance, Is.EqualTo(10));
            Assert.That(feeAccount.Balance, Is.EqualTo(int.MaxValue));
            Assert.That(ledger.Count, Is.Zero);
        }

        [Test]
        public void DirtyCashPayoutUsesIntegerArithmeticAndRejectsInvalidAmounts()
        {
            Assert.That(DirtyCashRules.TryGetCleaningAmounts(3, 20, out int payout, out int fee), Is.True);
            Assert.That(payout, Is.EqualTo(24));
            Assert.That(fee, Is.EqualTo(6));
            Assert.That(DirtyCashRules.TryGetCleaningAmounts(3, 30, out payout, out fee), Is.True);
            Assert.That(payout, Is.EqualTo(21));
            Assert.That(fee, Is.EqualTo(9));
            Assert.That(DirtyCashRules.TryGetCleaningAmounts(0, 20, out _, out _), Is.False);
            Assert.That(DirtyCashRules.TryGetCleaningAmounts(int.MaxValue, 20, out _, out _), Is.False);
            Assert.That(DirtyCashRules.TryGetCleaningAmounts(3, 100, out _, out _), Is.False);
        }

        [Test]
        public void FenceReplayCannotRewardTheSameStackTwiceAndLedgerRejectsDuplicateId()
        {
            ItemType accepted = Item();
            ItemType cash = Item();
            var player = new GridInventory(3, 2);
            player.Place(accepted, 0, 0, count: 2, state: Stolen);
            PlacedItem stolen = player.Items[0];
            var contact = new GridInventory(3, 2);
            var ownership = new AssetOwnershipBook();
            ownership.TryRegister(stolen.ItemInstanceId, Player);
            var ledger = new IllicitExchangeTransactionLedger();
            var economicLedger = new EconomicTransactionLedger();

            IllicitExchangeResult first = IllicitExchangeService.TryExchange(player, contact,
                stolen.ItemInstanceId, accepted, 2, cash, 3, Contact, ownership, economicLedger, ledger, 2, 600);
            IllicitExchangeResult replay = IllicitExchangeService.TryExchange(player, contact,
                stolen.ItemInstanceId, accepted, 2, cash, 3, Contact, ownership, economicLedger, ledger, 2, 601);

            Assert.That(first.Success, Is.True);
            Assert.That(replay.Status, Is.EqualTo(IllicitExchangeStatus.ItemNotFound));
            Assert.That(player.CountOf(cash), Is.EqualTo(3));
            Assert.That(contact.CountOf(accepted), Is.EqualTo(2));
            Assert.That(ledger.Count, Is.EqualTo(1));
            Assert.That(ledger.TryRecord(first.Transaction), Is.False);
            Assert.That(economicLedger.Count, Is.EqualTo(1));
        }

        [Test]
        public void CleaningReplayCannotPayTheSameStackTwiceAndLedgerRejectsDuplicateId()
        {
            ItemType cash = Item();
            var inventory = new GridInventory();
            inventory.Place(cash, 0, 0, count: 3);
            PlacedItem stack = inventory.Items[0];
            var ownership = new AssetOwnershipBook();
            ownership.TryRegister(stack.ItemInstanceId, Player);
            var playerAccount = new CleanMoneyAccount();
            var contactAccount = new CleanMoneyAccount();
            var ledger = new DirtyCashCleaningTransactionLedger();

            DirtyCashCleaningResult first = DirtyCashCleaningService.TryClean(inventory,
                stack.ItemInstanceId, cash, Player, ownership, playerAccount, contactAccount,
                Contact, 20, ledger, 3, 720);
            DirtyCashCleaningResult replay = DirtyCashCleaningService.TryClean(inventory,
                stack.ItemInstanceId, cash, Player, ownership, playerAccount, contactAccount,
                Contact, 20, ledger, 3, 721);

            Assert.That(first.Success, Is.True);
            Assert.That(replay.Status, Is.EqualTo(DirtyCashCleaningStatus.ItemNotFound));
            Assert.That(playerAccount.Balance, Is.EqualTo(24));
            Assert.That(contactAccount.Balance, Is.EqualTo(6));
            Assert.That(ledger.Count, Is.EqualTo(1));
            Assert.That(ledger.TryRecord(first.Transaction), Is.False);
            Assert.That(inventory.CountOf(cash), Is.Zero);
        }

        [Test]
        public void FenceExchangeNotifiesOnlyAfterGoodsCashOwnershipAndLedgersCommit()
        {
            ItemType accepted = Item();
            ItemType cash = Item();
            var player = new GridInventory(3, 2);
            player.Place(accepted, 0, 0, count: 2, state: Stolen);
            Guid stolenId = player.Items[0].ItemInstanceId;
            var contact = new GridInventory(3, 2);
            var ownership = new AssetOwnershipBook();
            ownership.TryRegister(stolenId, Player);
            var ledger = new IllicitExchangeTransactionLedger();
            var economicLedger = new EconomicTransactionLedger();
            bool observerSawCommittedState = false;
            player.Changed += changedInventory =>
            {
                observerSawCommittedState =
                    player.CountOf(accepted) == 0 &&
                    player.CountOf(cash) == 3 &&
                    contact.TryGetItemInstance(stolenId, out _) &&
                    ownership.TryGetOwner(stolenId, out EconomicActorId owner) &&
                    owner == Contact &&
                    ledger.Count == 1 &&
                    economicLedger.Count == 1;
            };

            IllicitExchangeResult result = IllicitExchangeService.TryExchange(player, contact,
                stolenId, accepted, 2, cash, 3, Contact, ownership, economicLedger, ledger, 2, 600);

            Assert.That(result.Success, Is.True);
            Assert.That(observerSawCommittedState, Is.True);
        }

        [Test]
        public void CleaningNotifiesInventoryOnlyAfterAccountsOwnershipAndLedgerCommit()
        {
            ItemType cash = Item();
            var inventory = new GridInventory();
            inventory.Place(cash, 0, 0, count: 3);
            Guid cashId = inventory.Items[0].ItemInstanceId;
            var ownership = new AssetOwnershipBook();
            Assert.That(ownership.TryRegister(cashId, Player), Is.True);
            var account = new CleanMoneyAccount(11);
            var feeAccount = new CleanMoneyAccount();
            var ledger = new DirtyCashCleaningTransactionLedger();
            bool observerSawCommittedState = false;
            inventory.Changed += _ =>
            {
                observerSawCommittedState =
                    inventory.CountOf(cash) == 0 &&
                    !ownership.TryGetOwner(cashId, out _) &&
                    account.Balance == 35 &&
                    feeAccount.Balance == 6 &&
                    ledger.Count == 1;
            };

            DirtyCashCleaningResult result = DirtyCashCleaningService.TryClean(inventory,
                cashId, cash, Player, ownership, account, feeAccount, Contact, 20, ledger, 3, 720);

            Assert.That(result.Success, Is.True);
            Assert.That(observerSawCommittedState, Is.True);
        }

    }
}
