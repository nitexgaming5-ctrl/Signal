using System;
using NUnit.Framework;

namespace Signal.Core.Tests
{
    public sealed class ShopSaleTests
    {
        private static readonly EconomicActorId Player = EconomicActorId.Player;
        private static ItemType Item() => new ItemType(Guid.NewGuid(), 1, 1, 10);

        private static FulfilResult Sell(GridInventory inventory, AssetOwnershipBook ownership,
            CleanMoneyAccount account, ShopSaleTransactionLedger ledger, Guid stackId,
            ItemType item, int quantity, int price)
        {
            return Shop.TryFulfilExact(new ShopOrder(item, quantity, price), stackId, "test-business",
                Player, inventory, ownership, account, ledger, 2, 615);
        }

        [Test]
        public void FullStackSaleConsumesExactStackRemovesOwnershipCreditsCardAndRecordsSale()
        {
            ItemType item = Item();
            var inventory = new GridInventory();
            inventory.Place(item, 0, 0, count: 4);
            PlacedItem stack = inventory.Items[0];
            var ownership = new AssetOwnershipBook();
            Assert.That(ownership.TryRegister(stack.ItemInstanceId, Player), Is.True);
            var account = new CleanMoneyAccount(10);
            var ledger = new ShopSaleTransactionLedger();

            FulfilResult result = Sell(inventory, ownership, account, ledger, stack.ItemInstanceId, item, 4, 7);

            Assert.That(result, Is.EqualTo(FulfilResult.Done));
            Assert.That(inventory.Items, Is.Empty);
            Assert.That(ownership.TryGetOwner(stack.ItemInstanceId, out _), Is.False);
            Assert.That(account.Balance, Is.EqualTo(38));
            Assert.That(ledger.Count, Is.EqualTo(1));
            ShopSaleTransaction sale = ledger.Transactions[0];
            Assert.That(sale.BusinessId, Is.EqualTo("test-business"));
            Assert.That(sale.Seller, Is.EqualTo(Player));
            Assert.That(sale.ItemInstanceId, Is.EqualTo(stack.ItemInstanceId));
            Assert.That(sale.ItemDefinitionId, Is.EqualTo(item.Id));
            Assert.That(sale.Quantity, Is.EqualTo(4));
            Assert.That(sale.UnitPrice, Is.EqualTo(7));
            Assert.That(sale.TotalAmount, Is.EqualTo(28));
            Assert.That(sale.ItemState, Is.EqualTo(stack.State));
            Assert.That(sale.Day, Is.EqualTo(2));
            Assert.That(sale.MinuteOfDay, Is.EqualTo(615));
        }

        [Test]
        public void PartialSalePreservesExactStackIdentityStateAndOwnership()
        {
            ItemType item = Item();
            var state = new ItemInstanceState(ItemQuality.Fine, ItemCondition.Worn,
                ItemAuthenticity.Genuine, ItemOwnershipStatus.Stolen);
            var inventory = new GridInventory();
            inventory.Place(item, 0, 0, count: 6, state: state);
            PlacedItem original = inventory.Items[0];
            var ownership = new AssetOwnershipBook();
            ownership.TryRegister(original.ItemInstanceId, Player);
            var account = new CleanMoneyAccount();
            var ledger = new ShopSaleTransactionLedger();

            FulfilResult result = Sell(inventory, ownership, account, ledger, original.ItemInstanceId, item, 2, 9);

            Assert.That(result, Is.EqualTo(FulfilResult.Done));
            Assert.That(inventory.Items.Count, Is.EqualTo(1));
            PlacedItem remaining = inventory.Items[0];
            Assert.That(remaining.ItemInstanceId, Is.EqualTo(original.ItemInstanceId));
            Assert.That(remaining.InstanceId, Is.EqualTo(original.InstanceId));
            Assert.That(remaining.Count, Is.EqualTo(4));
            Assert.That(remaining.State, Is.EqualTo(state));
            Assert.That(ownership.TryGetOwner(original.ItemInstanceId, out EconomicActorId owner), Is.True);
            Assert.That(owner, Is.EqualTo(Player));
            Assert.That(account.Balance, Is.EqualTo(18));
            Assert.That(ledger.Count, Is.EqualTo(1));
        }

        [Test]
        public void SaleNeverConsumesAnotherStackOfTheSameItemType()
        {
            ItemType item = Item();
            var inventory = new GridInventory(2, 1);
            inventory.Place(item, 0, 0, count: 3);
            inventory.Place(item, 1, 0, count: 5);
            Guid selectedId = inventory.Items[0].ItemInstanceId;
            Guid otherId = inventory.Items[1].ItemInstanceId;
            var ownership = new AssetOwnershipBook();
            ownership.TryRegister(selectedId, Player);
            ownership.TryRegister(otherId, Player);
            var account = new CleanMoneyAccount();
            var ledger = new ShopSaleTransactionLedger();

            Assert.That(Sell(inventory, ownership, account, ledger, selectedId, item, 1, 4),
                Is.EqualTo(FulfilResult.Done));

            Assert.That(inventory.TryGetItemInstance(selectedId, out PlacedItem selected), Is.True);
            Assert.That(selected.Count, Is.EqualTo(2));
            Assert.That(inventory.TryGetItemInstance(otherId, out PlacedItem other), Is.True);
            Assert.That(other.Count, Is.EqualTo(5));
        }

        [Test]
        public void MissingOrWrongOwnershipRejectsSaleWithoutMutation()
        {
            ItemType item = Item();
            var inventory = new GridInventory();
            inventory.Place(item, 0, 0, count: 3);
            Guid stackId = inventory.Items[0].ItemInstanceId;
            var ownership = new AssetOwnershipBook();
            ownership.TryRegister(stackId, new EconomicActorId("resident:milo"));
            var account = new CleanMoneyAccount(12);
            var ledger = new ShopSaleTransactionLedger();

            Assert.That(Sell(inventory, ownership, account, ledger, stackId, item, 1, 5),
                Is.EqualTo(FulfilResult.InvalidOrder));
            Assert.That(inventory.CountOf(item), Is.EqualTo(3));
            Assert.That(account.Balance, Is.EqualTo(12));
            Assert.That(ledger.Count, Is.Zero);
        }

        [Test]
        public void InvalidQuantityAndOversizedTotalChangeNothing()
        {
            ItemType item = Item();
            var inventory = new GridInventory();
            inventory.Place(item, 0, 0, count: 2);
            Guid stackId = inventory.Items[0].ItemInstanceId;
            var ownership = new AssetOwnershipBook();
            ownership.TryRegister(stackId, Player);
            var account = new CleanMoneyAccount(20);
            var ledger = new ShopSaleTransactionLedger();

            Assert.That(Shop.TryFulfilExact(default(ShopOrder), stackId, "test-business", Player,
                inventory, ownership, account, ledger, 1, 100), Is.EqualTo(FulfilResult.InvalidOrder));
            Assert.That(Sell(inventory, ownership, account, ledger, stackId, item, 3, 5),
                Is.EqualTo(FulfilResult.NotEnoughStock));
            Assert.That(Sell(inventory, ownership, account, ledger, stackId, item, 2, int.MaxValue),
                Is.EqualTo(FulfilResult.InvalidOrder));
            Assert.That(inventory.CountOf(item), Is.EqualTo(2));
            Assert.That(account.Balance, Is.EqualTo(20));
            Assert.That(ledger.Count, Is.Zero);
        }

        [Test]
        public void AccountOverflowRestoresInventoryAndFullStackOwnership()
        {
            ItemType item = Item();
            var inventory = new GridInventory();
            inventory.Place(item, 0, 0, count: 2);
            PlacedItem stack = inventory.Items[0];
            var ownership = new AssetOwnershipBook();
            ownership.TryRegister(stack.ItemInstanceId, Player);
            var account = new CleanMoneyAccount(int.MaxValue);
            var ledger = new ShopSaleTransactionLedger();

            Assert.That(Sell(inventory, ownership, account, ledger, stack.ItemInstanceId, item, 2, 1),
                Is.EqualTo(FulfilResult.InvalidOrder));

            Assert.That(inventory.Items.Count, Is.EqualTo(1));
            Assert.That(inventory.Items[0].ItemInstanceId, Is.EqualTo(stack.ItemInstanceId));
            Assert.That(inventory.Items[0].Count, Is.EqualTo(2));
            Assert.That(ownership.TryGetOwner(stack.ItemInstanceId, out EconomicActorId owner), Is.True);
            Assert.That(owner, Is.EqualTo(Player));
            Assert.That(account.Balance, Is.EqualTo(int.MaxValue));
            Assert.That(ledger.Count, Is.Zero);
        }

        [Test]
        public void ExactStackRemovalRejectsInvalidQuantityWithoutChangingInventory()
        {
            ItemType item = Item();
            var inventory = new GridInventory();
            inventory.Place(item, 0, 0, count: 3);
            Guid id = inventory.Items[0].ItemInstanceId;
            int notifications = 0;
            inventory.Changed += _ => notifications++;

            Assert.That(inventory.TryRemoveItemInstance(id, 0), Is.False);
            Assert.That(inventory.TryRemoveItemInstance(id, 4), Is.False);
            Assert.That(inventory.TryRemoveItemInstance(Guid.NewGuid(), 1), Is.False);
            Assert.That(inventory.Items[0].Count, Is.EqualTo(3));
            Assert.That(notifications, Is.Zero);
        }
    }
}
