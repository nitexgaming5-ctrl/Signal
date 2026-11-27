using System;
using NUnit.Framework;

namespace Signal.Core.Tests
{
    public sealed class PortableBackpackWorldBookTests
    {
        private static ItemType CreateType() => new ItemType(Guid.NewGuid(), 1, 1, 1);
        private static PortableStorageBook Storage(Guid id, out GridInventory contents)
        {
            var storage = new PortableStorageBook();
            Assert.That(storage.TryRegister(id, 8, 6), Is.True);
            Assert.That(storage.TryGetContents(id, out contents), Is.True);
            return storage;
        }

        [Test]
        public void RegistrationRequiresAnExistingContentsGrid()
        {
            var book = new PortableBackpackWorldBook();
            Assert.That(book.TryRegisterEquipped(Guid.NewGuid(), new PortableStorageBook()), Is.False);
            Assert.That(book.TryRegisterEquipped(Guid.Empty, new PortableStorageBook()), Is.False);
            Assert.That(book.Count, Is.Zero);
        }

        [Test]
        public void DropAndRecoverPreserveBackpackIdentityAndContents()
        {
            Guid id = Guid.NewGuid();
            PortableStorageBook storage = Storage(id, out GridInventory contents);
            Guid cargo = Guid.NewGuid();
            Assert.That(contents.Place(CreateType(), 0, 0, false, 1, cargo), Is.Not.Null);
            var book = new PortableBackpackWorldBook();
            Assert.That(book.TryRegisterEquipped(id, storage), Is.True);
            Assert.That(book.TryDrop(id, 12.5f, -3.25f), Is.True);
            Assert.That(book.TryGet(id, out PortableBackpackWorldState dropped), Is.True);
            Assert.That(dropped.Status, Is.EqualTo(PortableBackpackWorldStatus.Dropped));
            Assert.That(dropped.WorldX, Is.EqualTo(12.5f));
            Assert.That(dropped.WorldY, Is.EqualTo(-3.25f));
            Assert.That(storage.TryGetContents(id, out GridInventory same), Is.True);
            Assert.That(same, Is.SameAs(contents));
            Assert.That(same.TryGetItemInstance(cargo, out _), Is.True);
            Assert.That(book.TryRecover(id), Is.True);
            Assert.That(book.TryGet(id, out PortableBackpackWorldState recovered), Is.True);
            Assert.That(recovered.Status, Is.EqualTo(PortableBackpackWorldStatus.Equipped));
            Assert.That(book.TryRecover(id), Is.False);
        }

        [Test]
        public void InvalidTransitionsAndNonFinitePositionsDoNotMutateState()
        {
            Guid id = Guid.NewGuid();
            PortableStorageBook storage = Storage(id, out _);
            var book = new PortableBackpackWorldBook();
            Assert.That(book.TryRegisterEquipped(id, storage), Is.True);
            Assert.That(book.TryRecover(id), Is.False);
            Assert.That(book.TryDrop(id, float.NaN, 1f), Is.False);
            Assert.That(book.TryDrop(id, 1f, float.PositiveInfinity), Is.False);
            Assert.That(book.TryGet(id, out PortableBackpackWorldState state), Is.True);
            Assert.That(state.Status, Is.EqualTo(PortableBackpackWorldStatus.Equipped));
            Assert.That(book.TryDrop(id, 1f, 2f), Is.True);
            Assert.That(book.TryDrop(id, 3f, 4f), Is.False);
        }

        [Test]
        public void DuplicateRegistrationAndMissingContentsAreRejected()
        {
            Guid id = Guid.NewGuid();
            PortableStorageBook storage = Storage(id, out _);
            var book = new PortableBackpackWorldBook();
            Assert.That(book.TryRegisterEquipped(id, storage), Is.True);
            Assert.That(book.TryRegisterEquipped(id, storage), Is.False);
            Assert.That(book.TryRegisterEquipped(Guid.NewGuid(), storage), Is.False);
            Assert.That(book.Count, Is.EqualTo(1));
        }

        [Test]
        public void SaveMapperRoundTripsDroppedStatePositionAndContentsReference()
        {
            Guid id = Guid.NewGuid();
            PortableStorageBook storage = Storage(id, out GridInventory contents);
            Guid cargo = Guid.NewGuid();
            Assert.That(contents.Place(CreateType(), 0, 0, false, 1, cargo), Is.Not.Null);
            var source = new PortableBackpackWorldBook();
            Assert.That(source.TryRegisterEquipped(id, storage), Is.True);
            Assert.That(source.TryDrop(id, 4.75f, 9.5f), Is.True);
            var data = new SaveData();
            PortableBackpackWorldSaveMapper.Capture(source, data);
            var restored = new PortableBackpackWorldBook();
            Assert.That(PortableBackpackWorldSaveMapper.Apply(data, restored, storage), Is.Zero);
            Assert.That(restored.TryGet(id, out PortableBackpackWorldState state), Is.True);
            Assert.That(state.Status, Is.EqualTo(PortableBackpackWorldStatus.Dropped));
            Assert.That(state.WorldX, Is.EqualTo(4.75f));
            Assert.That(state.WorldY, Is.EqualTo(9.5f));
            Assert.That(storage.TryGetContents(id, out GridInventory same), Is.True);
            Assert.That(same.TryGetItemInstance(cargo, out _), Is.True);
        }

        [Test]
        public void SaveMapperSkipsMalformedDuplicateAndUnregisteredBackpacks()
        {
            Guid id = Guid.NewGuid();
            PortableStorageBook storage = Storage(id, out _);
            var data = new SaveData();
            data.portableBackpacks.Add(new SavedPortableBackpack { instanceId = "bad", status = 1 });
            data.portableBackpacks.Add(new SavedPortableBackpack { instanceId = Guid.NewGuid().ToString(), status = 1 });
            data.portableBackpacks.Add(new SavedPortableBackpack { instanceId = id.ToString(), status = 1, worldX = 2f, worldY = 3f });
            data.portableBackpacks.Add(new SavedPortableBackpack { instanceId = id.ToString(), status = 1, worldX = 2f, worldY = 3f });
            var restored = new PortableBackpackWorldBook();
            Assert.That(PortableBackpackWorldSaveMapper.Apply(data, restored, storage), Is.EqualTo(3));
            Assert.That(restored.Count, Is.EqualTo(1));
        }

        [Test]
        public void NearestDroppedSelectionChoosesClosestBagWithinRadius()
        {
            Guid fartherId = Guid.NewGuid();
            Guid nearerId = Guid.NewGuid();
            PortableStorageBook storage = new PortableStorageBook();
            Assert.That(storage.TryRegister(fartherId, 8, 6), Is.True);
            Assert.That(storage.TryRegister(nearerId, 8, 6), Is.True);
            var book = new PortableBackpackWorldBook();
            Assert.That(book.TryRegisterEquipped(fartherId, storage), Is.True);
            Assert.That(book.TryRegisterEquipped(nearerId, storage), Is.True);
            Assert.That(book.TryDrop(fartherId, 1f, 0f), Is.True);
            Assert.That(book.TryDrop(nearerId, 0.25f, 0f), Is.True);

            Assert.That(book.TryGetNearestDropped(0f, 0f, 1f, out PortableBackpackWorldState nearest), Is.True);
            Assert.That(nearest.BackpackInstanceId, Is.EqualTo(nearerId));
            Assert.That(book.TryGetNearestDropped(0f, 0f, 0.1f, out _), Is.False);
        }

        [Test]
        public void NearestDroppedSelectionUsesStableGuidTieBreakAndRejectsInvalidRadius()
        {
            Guid first = Guid.NewGuid();
            Guid second = Guid.NewGuid();
            Guid expected = first.CompareTo(second) < 0 ? first : second;
            PortableStorageBook storage = new PortableStorageBook();
            Assert.That(storage.TryRegister(first, 8, 6), Is.True);
            Assert.That(storage.TryRegister(second, 8, 6), Is.True);
            var book = new PortableBackpackWorldBook();
            Assert.That(book.TryRegisterEquipped(first, storage), Is.True);
            Assert.That(book.TryRegisterEquipped(second, storage), Is.True);
            Assert.That(book.TryDrop(first, -1f, 0f), Is.True);
            Assert.That(book.TryDrop(second, 1f, 0f), Is.True);

            Assert.That(book.TryGetNearestDropped(0f, 0f, 1f, out PortableBackpackWorldState nearest), Is.True);
            Assert.That(nearest.BackpackInstanceId, Is.EqualTo(expected));
            Assert.That(book.TryGetNearestDropped(0f, 0f, -1f, out _), Is.False);
            Assert.That(book.TryGetNearestDropped(float.NaN, 0f, 1f, out _), Is.False);
        }

        [Test]
        public void EquippedRestoreNormalizesWorldCoordinates()
        {
            Guid id = Guid.NewGuid();
            PortableStorageBook storage = Storage(id, out _);
            var book = new PortableBackpackWorldBook();
            Assert.That(book.TryRestore(id, PortableBackpackWorldStatus.Equipped, 99f, -44f, storage), Is.True);
            Assert.That(book.TryGet(id, out PortableBackpackWorldState state), Is.True);
            Assert.That(state.WorldX, Is.Zero);
            Assert.That(state.WorldY, Is.Zero);
        }
    }
}
