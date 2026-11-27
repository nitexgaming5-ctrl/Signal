using System;
using NUnit.Framework;

namespace Signal.Core.Tests
{
    public sealed class InventoryStorageRegistryTests
    {
        [Test]
        public void RegistersAndResolvesOneGridByStableIdentity()
        {
            var registry = new InventoryStorageRegistry();
            var identity = new InventoryStorageId(InventoryStorageKind.PlayerBackpack,
                Guid.NewGuid().ToString("N"));
            var grid = new GridInventory(8, 6);

            Assert.That(registry.TryRegister(identity, grid), Is.True);
            Assert.That(registry.Count, Is.EqualTo(1));
            Assert.That(registry.TryGet(identity, out GridInventory resolved), Is.True);
            Assert.That(resolved, Is.SameAs(grid));
            Assert.That(registry.TryGetIdentity(grid, out InventoryStorageId reverse), Is.True);
            Assert.That(reverse, Is.EqualTo(identity));
            Assert.That(registry.Owns(identity, grid), Is.True);
        }

        [Test]
        public void RejectsDuplicateIdentityAndGridAliasing()
        {
            var registry = new InventoryStorageRegistry();
            var firstId = new InventoryStorageId(InventoryStorageKind.PlayerBackpack, "backpack-a");
            var secondId = new InventoryStorageId(InventoryStorageKind.PlayerBackpack, "backpack-b");
            var firstGrid = new GridInventory(8, 6);
            var secondGrid = new GridInventory(8, 6);

            Assert.That(registry.TryRegister(firstId, firstGrid), Is.True);
            Assert.That(registry.TryRegister(firstId, secondGrid), Is.False);
            Assert.That(registry.TryRegister(secondId, firstGrid), Is.False);
            Assert.That(registry.TryRegister(secondId, secondGrid), Is.True);
            Assert.That(registry.Count, Is.EqualTo(2));
            Assert.That(registry.Owns(firstId, secondGrid), Is.False);
            Assert.That(registry.Owns(secondId, firstGrid), Is.False);
        }

        [Test]
        public void SeparateStorageIdentitiesNeverShareMutableContents()
        {
            var registry = new InventoryStorageRegistry();
            var player = new InventoryStorageId(InventoryStorageKind.PlayerInventory, "player");
            var backpack = new InventoryStorageId(InventoryStorageKind.PlayerBackpack, "backpack-a");
            var world = new InventoryStorageId(InventoryStorageKind.WorldContainer, "street-bin-01");
            var playerGrid = new GridInventory(4, 4);
            var backpackGrid = new GridInventory(8, 6);
            var worldGrid = new GridInventory(8, 6);
            ItemType item = new ItemType(Guid.NewGuid(), 1, 1, 10);

            Assert.That(registry.TryRegister(player, playerGrid), Is.True);
            Assert.That(registry.TryRegister(backpack, backpackGrid), Is.True);
            Assert.That(registry.TryRegister(world, worldGrid), Is.True);
            Assert.That(backpackGrid.Place(item, 0, 0), Is.Not.Null);

            Assert.That(playerGrid.Items, Is.Empty);
            Assert.That(worldGrid.Items, Is.Empty);
            Assert.That(backpackGrid.Items.Count, Is.EqualTo(1));
            Assert.That(registry.TryGet(backpack, out GridInventory sameBackpack), Is.True);
            Assert.That(sameBackpack.TryGetItemInstance(backpackGrid.Items[0].ItemInstanceId, out _), Is.True);
        }

        [Test]
        public void DistinguishesSameOwnerAcrossDifferentStorageKindsAndSlots()
        {
            var backpack = new InventoryStorageId(InventoryStorageKind.PlayerBackpack, "player", "item-guid");
            var pocketOne = new InventoryStorageId(InventoryStorageKind.PlayerPocket, "player", "1");
            var pocketTwo = new InventoryStorageId(InventoryStorageKind.PlayerPocket, "player", "2");

            Assert.That(backpack, Is.Not.EqualTo(pocketOne));
            Assert.That(pocketOne, Is.Not.EqualTo(pocketTwo));
            Assert.That(new InventoryStorageId(InventoryStorageKind.PlayerPocket, "player", "1"),
                Is.EqualTo(pocketOne));
        }

        [Test]
        public void RejectsInvalidIdentityAndNullInventory()
        {
            var registry = new InventoryStorageRegistry();
            Assert.Throws<ArgumentException>(() =>
                new InventoryStorageId(InventoryStorageKind.PlayerBackpack, " "));
            Assert.Throws<ArgumentOutOfRangeException>(() =>
                new InventoryStorageId((InventoryStorageKind)999, "owner"));
            Assert.That(registry.TryRegister(default, new GridInventory()), Is.False);
            Assert.That(registry.TryRegister(
                new InventoryStorageId(InventoryStorageKind.Hideout, "home"), null), Is.False);
            Assert.That(registry.TryGet(default, out _), Is.False);
            Assert.That(registry.TryGetIdentity(null, out _), Is.False);
            Assert.That(registry.Count, Is.Zero);
        }
    }
}