using System;
using NUnit.Framework;

namespace Signal.Core.Tests
{
    public sealed class ShopTests
    {
        private static ItemType Item()
        {
            return new ItemType(Guid.NewGuid(), 1, 1, 5);
        }

        [Test]
        public void FulfilRemovesStockAndPaysTheTotal()
        {
            var item = Item();
            var grid = new GridInventory();
            grid.Place(item, 0, 0, count: 5);
            var stats = new PlayerStats();

            FulfilResult result = Shop.TryFulfil(new ShopOrder(item, 3, 12), grid, stats);

            Assert.That(result, Is.EqualTo(FulfilResult.Done));
            Assert.That(stats.Money, Is.EqualTo(36));
            Assert.That(grid.CountOf(item), Is.EqualTo(2));
        }

        [Test]
        public void NotEnoughStockChangesNothing()
        {
            var item = Item();
            var grid = new GridInventory();
            grid.Place(item, 0, 0, count: 5);
            var stats = new PlayerStats();

            FulfilResult result = Shop.TryFulfil(new ShopOrder(item, 6, 12), grid, stats);

            Assert.That(result, Is.EqualTo(FulfilResult.NotEnoughStock));
            Assert.That(stats.Money, Is.EqualTo(0));
            Assert.That(grid.CountOf(item), Is.EqualTo(5));
        }

        [Test]
        public void SaleDoesNotConsumeStockWhenCleanAccountCannotCredit()
        {
            var item = Item();
            var grid = new GridInventory();
            grid.Place(item, 0, 0, count: 2);
            var stats = new PlayerStats(int.MaxValue);

            FulfilResult result = Shop.TryFulfil(new ShopOrder(item, 1, 1), grid, stats);

            Assert.That(result, Is.EqualTo(FulfilResult.InvalidOrder));
            Assert.That(grid.CountOf(item), Is.EqualTo(2));
            Assert.That(stats.CleanMoney.Balance, Is.EqualTo(int.MaxValue));
        }

        [Test]
        public void ADefaultOrderIsInvalid()
        {
            var result = Shop.TryFulfil(default(ShopOrder), new GridInventory(), new PlayerStats());

            Assert.That(result, Is.EqualTo(FulfilResult.InvalidOrder));
        }

        [TestCase(0, 1)]
        [TestCase(1, -1)]
        public void AnOrderRejectsInvalidValues(int quantity, int unitPrice)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new ShopOrder(Item(), quantity, unitPrice));
        }

        [Test]
        public void TheTotalDoesNotOverflow()
        {
            var order = new ShopOrder(Item(), int.MaxValue, 2);

            Assert.That(order.Total, Is.EqualTo(4294967294L));
        }

        [Test]
        public void AnOversizedTotalIsAnInvalidOrder()
        {
            var item = Item();
            var grid = new GridInventory();
            grid.Place(item, 0, 0, count: 5);
            var stats = new PlayerStats();

            FulfilResult result = Shop.TryFulfil(new ShopOrder(item, int.MaxValue, 2), grid, stats);

            Assert.That(result, Is.EqualTo(FulfilResult.InvalidOrder));
            Assert.That(grid.CountOf(item), Is.EqualTo(5));
        }
    }
}
