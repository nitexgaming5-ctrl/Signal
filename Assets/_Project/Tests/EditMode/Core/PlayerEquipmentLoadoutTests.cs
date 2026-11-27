using System;
using NUnit.Framework;

namespace Signal.Core.Tests
{
    public sealed class PlayerEquipmentLoadoutTests
    {
        private static ItemType CreateType(int width = 1, int height = 1, int maxStack = 1)
        {
            return new ItemType(Guid.NewGuid(), width, height, maxStack);
        }

        [Test]
        public void LoadoutSupportsExactlyFourClothingAndThreeAccessorySlots()
        {
            var loadout = new PlayerEquipmentLoadout();
            Assert.That(Enum.GetValues(typeof(EquipmentSlot)).Length, Is.EqualTo(7));
            Assert.That(loadout.Count, Is.Zero);
            foreach (EquipmentSlot slot in Enum.GetValues(typeof(EquipmentSlot)))
                Assert.That(loadout.IsOccupied(slot), Is.False);
        }

        [Test]
        public void EquipTransfersExactSingleItemAndPreservesItsStableIdentityAndState()
        {
            var inventory = new GridInventory(2, 2);
            ItemType type = CreateType();
            Guid id = Guid.NewGuid();
            var state = new ItemInstanceState(ItemQuality.Exceptional, ItemCondition.Good,
                ItemAuthenticity.Genuine, ItemOwnershipStatus.Legitimate);
            Assert.That(inventory.Place(type, 0, 0, false, 1, id, state), Is.Not.Null);

            var loadout = new PlayerEquipmentLoadout();
            Assert.That(loadout.TryEquip(inventory, id, EquipmentSlot.Torso, 2, 1), Is.True);
            Assert.That(inventory.TryGetItemInstance(id, out _), Is.False);
            Assert.That(loadout.TryGet(EquipmentSlot.Torso, out EquippedItem equipped), Is.True);
            Assert.That(equipped.Item.ItemInstanceId, Is.EqualTo(id));
            Assert.That(equipped.Item.State, Is.EqualTo(state));
            Assert.That(equipped.ArmorModifier, Is.EqualTo(2));
        }

        [Test]
        public void EquipRejectsOccupiedSlotUnknownItemAndStackedItemsWithoutChangingInventory()
        {
            var inventory = new GridInventory(3, 2);
            ItemType single = CreateType();
            ItemType stackable = CreateType(maxStack: 5);
            Guid first = Guid.NewGuid();
            Guid stacked = Guid.NewGuid();
            Assert.That(inventory.Place(single, 0, 0, false, 1, first), Is.Not.Null);
            Assert.That(inventory.Place(stackable, 1, 0, false, 2, stacked), Is.Not.Null);

            var loadout = new PlayerEquipmentLoadout();
            Assert.That(loadout.TryEquip(inventory, first, EquipmentSlot.Head), Is.True);
            Assert.That(loadout.TryEquip(inventory, stacked, EquipmentSlot.Head), Is.False);
            Assert.That(loadout.TryEquip(inventory, Guid.NewGuid(), EquipmentSlot.Feet), Is.False);
            Assert.That(inventory.TryGetItemInstance(stacked, out PlacedItem remains), Is.True);
            Assert.That(remains.Count, Is.EqualTo(2));
            Assert.That(loadout.Count, Is.EqualTo(1));
        }

        [Test]
        public void SamePhysicalItemCannotBeEquippedInTwoSlots()
        {
            var inventory = new GridInventory(1, 1);
            Guid id = Guid.NewGuid();
            Assert.That(inventory.Place(CreateType(), 0, 0, false, 1, id), Is.Not.Null);
            var loadout = new PlayerEquipmentLoadout();
            Assert.That(loadout.TryEquip(inventory, id, EquipmentSlot.Accessory1), Is.True);
            Assert.That(loadout.TryEquip(inventory, id, EquipmentSlot.Accessory2), Is.False);
            Assert.That(loadout.Count, Is.EqualTo(1));
        }

        [Test]
        public void UnequipReturnsExactItemAndLeavesLoadoutUntouchedWhenInventoryIsFull()
        {
            var source = new GridInventory(1, 1);
            Guid equippedId = Guid.NewGuid();
            Assert.That(source.Place(CreateType(), 0, 0, false, 1, equippedId), Is.Not.Null);
            var loadout = new PlayerEquipmentLoadout();
            Assert.That(loadout.TryEquip(source, equippedId, EquipmentSlot.Legs), Is.True);

            var full = new GridInventory(1, 1);
            Guid blockerId = Guid.NewGuid();
            Assert.That(full.Place(CreateType(), 0, 0, false, 1, blockerId), Is.Not.Null);
            Assert.That(loadout.TryUnequip(full, EquipmentSlot.Legs), Is.False);
            Assert.That(loadout.IsOccupied(EquipmentSlot.Legs), Is.True);
            Assert.That(full.TryGetItemInstance(blockerId, out _), Is.True);

            full.Clear();
            Assert.That(loadout.TryUnequip(full, EquipmentSlot.Legs), Is.True);
            Assert.That(full.TryGetItemInstance(equippedId, out _), Is.True);
            Assert.That(loadout.IsOccupied(EquipmentSlot.Legs), Is.False);
        }

        [Test]
        public void TakingEquippedBackpackForWorldDropPreservesExactItemWithoutPuttingItInInventory()
        {
            var inventory = new GridInventory(2, 2);
            ItemType type = CreateType(2, 2);
            Guid backpackId = Guid.NewGuid();
            var state = new ItemInstanceState(ItemQuality.Fine, ItemCondition.Good,
                ItemAuthenticity.Genuine, ItemOwnershipStatus.Legitimate);
            Assert.That(inventory.Place(type, 0, 0, false, 1, backpackId, state), Is.Not.Null);

            var loadout = new PlayerEquipmentLoadout();
            Assert.That(loadout.TryEquip(inventory, backpackId, EquipmentSlot.Accessory1), Is.True);
            Assert.That(loadout.TryTakeEquipped(EquipmentSlot.Accessory1, out EquippedItem dropped), Is.True);
            Assert.That(dropped.Item.ItemInstanceId, Is.EqualTo(backpackId));
            Assert.That(dropped.Item.State, Is.EqualTo(state));
            Assert.That(loadout.IsOccupied(EquipmentSlot.Accessory1), Is.False);
            Assert.That(inventory.TryGetItemInstance(backpackId, out _), Is.False);
            Assert.That(loadout.TryTakeEquipped(EquipmentSlot.Accessory1, out _), Is.False);
        }

        [Test]
        public void EquipmentModifiersAggregateAcrossAllSevenSlotsAndClampSafely()
        {
            var inventory = new GridInventory(7, 1);
            var loadout = new PlayerEquipmentLoadout();
            EquipmentSlot[] slots = (EquipmentSlot[])Enum.GetValues(typeof(EquipmentSlot));
            for (int i = 0; i < slots.Length; i++)
            {
                Guid id = Guid.NewGuid();
                Assert.That(inventory.Place(CreateType(), i, 0, false, 1, id), Is.Not.Null);
                Assert.That(loadout.TryEquip(inventory, id, slots[i], 1, 2), Is.True);
            }

            EffectiveCharacterAttributes effective =
                loadout.CalculateEffectiveAttributes(new PlayerAttributeSet(3, 4));
            Assert.That(effective.Armor, Is.EqualTo(10));
            Assert.That(effective.HeatProtection, Is.EqualTo(18));

            var clamped = new PlayerEquipmentLoadout();
            var overflowInventory = new GridInventory(1, 1);
            Guid overflowId = Guid.NewGuid();
            Assert.That(overflowInventory.Place(CreateType(), 0, 0, false, 1, overflowId), Is.Not.Null);
            Assert.That(clamped.TryEquip(overflowInventory, overflowId, EquipmentSlot.Head, int.MaxValue, 0), Is.True);
            Assert.That(clamped.CalculateEffectiveAttributes(new PlayerAttributeSet(10, 0)).Armor,
                Is.EqualTo(int.MaxValue));
        }

        [Test]
        public void RestoreEquippedBackpackDoesNotRequireAnInventoryCellAndKeepsStableIdentity()
        {
            var loadout = new PlayerEquipmentLoadout();
            ItemType backpackType = CreateType(2, 2);
            Guid backpackId = Guid.NewGuid();
            var state = new ItemInstanceState(ItemQuality.Fine, ItemCondition.Good,
                ItemAuthenticity.Genuine, ItemOwnershipStatus.Legitimate);

            Assert.That(loadout.TryRestoreEquipped(backpackId, backpackType, state,
                EquipmentSlot.Accessory1, 0, 0), Is.True);
            Assert.That(loadout.TryGet(EquipmentSlot.Accessory1, out EquippedItem equipped), Is.True);
            Assert.That(equipped.Item.ItemInstanceId, Is.EqualTo(backpackId));
            Assert.That(equipped.Item.State, Is.EqualTo(state));
            Assert.That(loadout.ContainsInstance(backpackId), Is.True);
            Assert.That(loadout.TryRestoreEquipped(backpackId, backpackType, state,
                EquipmentSlot.Accessory2), Is.False);
        }

        [Test]
        public void BackpackContentsRemainBoundToStableBackpackIdentity()
        {
            var storage = new PortableStorageBook();
            Guid backpackId = Guid.NewGuid();
            Assert.That(storage.TryRegister(backpackId, 5, 4), Is.True);
            Assert.That(storage.TryRegister(backpackId, 5, 4), Is.False);
            Assert.That(storage.TryGetContents(backpackId, out GridInventory contents), Is.True);

            Guid cargoId = Guid.NewGuid();
            Assert.That(contents.Place(CreateType(), 0, 0, false, 1, cargoId), Is.Not.Null);
            Assert.That(storage.TryGetContents(backpackId, out GridInventory sameContents), Is.True);
            Assert.That(sameContents.TryGetItemInstance(cargoId, out _), Is.True);
            Assert.That(storage.TryRemoveEmpty(backpackId), Is.False);
        }

        [Test]
        public void BackpackCanAttachTheExistingMainGridWithoutCopyingOrLosingItems()
        {
            var mainInventory = new GridInventory(8, 6);
            Guid cargoId = Guid.NewGuid();
            Assert.That(mainInventory.Place(CreateType(), 0, 0, false, 1, cargoId), Is.Not.Null);

            var storage = new PortableStorageBook();
            Guid backpackId = Guid.NewGuid();
            Assert.That(storage.TryRegister(backpackId, mainInventory), Is.True);
            Assert.That(storage.TryRegister(Guid.NewGuid(), mainInventory), Is.False);
            Assert.That(storage.TryGetContents(backpackId, out GridInventory attached), Is.True);
            Assert.That(attached, Is.SameAs(mainInventory));
            Assert.That(attached.TryGetItemInstance(cargoId, out _), Is.True);

            Guid laterCargoId = Guid.NewGuid();
            Assert.That(attached.Place(CreateType(), 1, 0, false, 1, laterCargoId), Is.Not.Null);
            Assert.That(mainInventory.TryGetItemInstance(laterCargoId, out _), Is.True);
        }


        [Test]
        public void RegistryBackedRegistrationCreatesDistinctGridForEveryPhysicalBackpack()
        {
            var storage = new PortableStorageBook();
            var registry = new InventoryStorageRegistry();
            Guid firstBackpack = Guid.NewGuid();
            Guid secondBackpack = Guid.NewGuid();

            Assert.That(storage.TryRegister(firstBackpack, 8, 6, registry), Is.True);
            Assert.That(storage.TryRegister(secondBackpack, 8, 6, registry), Is.True);
            Assert.That(storage.TryGetContents(firstBackpack, out GridInventory first), Is.True);
            Assert.That(storage.TryGetContents(secondBackpack, out GridInventory second), Is.True);
            Assert.That(first, Is.Not.SameAs(second));
            Assert.That(registry.Owns(PortableStorageBook.GetStorageIdentity(firstBackpack), first), Is.True);
            Assert.That(registry.Owns(PortableStorageBook.GetStorageIdentity(secondBackpack), second), Is.True);

            Guid firstItem = Guid.NewGuid();
            Guid secondItem = Guid.NewGuid();
            Assert.That(first.Place(CreateType(), 0, 0, false, 1, firstItem), Is.Not.Null);
            Assert.That(second.Place(CreateType(), 0, 0, false, 1, secondItem), Is.Not.Null);
            Assert.That(first.TryGetItemInstance(secondItem, out _), Is.False);
            Assert.That(second.TryGetItemInstance(firstItem, out _), Is.False);
        }

        [Test]
        public void RegistryBackedBackpackRegistrationRejectsDuplicateWithoutCreatingAnAlias()
        {
            var storage = new PortableStorageBook();
            var registry = new InventoryStorageRegistry();
            Guid backpackId = Guid.NewGuid();

            Assert.That(storage.TryRegister(backpackId, 8, 6, registry), Is.True);
            Assert.That(storage.TryRegister(backpackId, 8, 6, registry), Is.False);
            Assert.That(storage.Count, Is.EqualTo(1));
            Assert.That(registry.Count, Is.EqualTo(1));
        }

        [Test]
        public void EmptyBackpackStorageCanBeRemovedButNonEmptyStorageCannotBeDiscarded()
        {
            var storage = new PortableStorageBook();
            Guid emptyBackpack = Guid.NewGuid();
            Guid loadedBackpack = Guid.NewGuid();
            Assert.That(storage.TryRegister(emptyBackpack, 4, 3), Is.True);
            Assert.That(storage.TryRegister(loadedBackpack, 4, 3), Is.True);
            Assert.That(storage.TryGetContents(loadedBackpack, out GridInventory contents), Is.True);
            Assert.That(contents.Place(CreateType(), 0, 0), Is.Not.Null);

            Assert.That(storage.TryRemoveEmpty(loadedBackpack), Is.False);
            Assert.That(storage.TryRemoveEmpty(emptyBackpack), Is.True);
            Assert.That(storage.TryGetContents(emptyBackpack, out _), Is.False);
            Assert.That(storage.TryGetContents(loadedBackpack, out _), Is.True);
        }

        [Test]
        public void InvalidBackpackIdentityOrDimensionsAreRejected()
        {
            var storage = new PortableStorageBook();
            Assert.That(storage.TryRegister(Guid.Empty, 4, 3), Is.False);
            Assert.That(storage.TryRegister(Guid.NewGuid(), 0, 3), Is.False);
            Assert.That(storage.TryRegister(Guid.NewGuid(), 4, -1), Is.False);
            Assert.That(storage.Count, Is.Zero);
        }
        [Test]
        public void EquipmentTransfersNotifyOnlyAfterLoadoutAndInventoryAgree()
        {
            var inventory = new GridInventory(2, 2);
            ItemType type = CreateType();
            Guid itemId = Guid.NewGuid();
            Assert.That(inventory.Place(type, 0, 0, false, 1, itemId), Is.Not.Null);
            var loadout = new PlayerEquipmentLoadout();
            bool equipObserverSawCommittedState = false;
            inventory.Changed += changedInventory =>
            {
                equipObserverSawCommittedState =
                    !inventory.TryGetItemInstance(itemId, out _) &&
                    loadout.TryGet(EquipmentSlot.Torso, out EquippedItem equipped) &&
                    equipped.Item.ItemInstanceId == itemId;
            };

            Assert.That(loadout.TryEquip(inventory, itemId, EquipmentSlot.Torso), Is.True);
            Assert.That(equipObserverSawCommittedState, Is.True);

            var destination = new GridInventory(2, 2);
            bool unequipObserverSawCommittedState = false;
            destination.Changed += changedInventory =>
            {
                unequipObserverSawCommittedState =
                    destination.TryGetItemInstance(itemId, out _) &&
                    !loadout.IsOccupied(EquipmentSlot.Torso);
            };

            Assert.That(loadout.TryUnequip(destination, EquipmentSlot.Torso), Is.True);
            Assert.That(unequipObserverSawCommittedState, Is.True);
        }

    }
}
