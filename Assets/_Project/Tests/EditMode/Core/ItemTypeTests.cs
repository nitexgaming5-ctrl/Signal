using System;
using NUnit.Framework;

namespace Signal.Core.Tests
{
    public sealed class ItemTypeTests
    {
        [Test]
        public void ExposesTheValuesItWasBuiltWith()
        {
            var id = Guid.NewGuid();

            var item = new ItemType(id, 2, 3, 4);

            Assert.That(item.Id, Is.EqualTo(id));
            Assert.That(item.Width, Is.EqualTo(2));
            Assert.That(item.Height, Is.EqualTo(3));
            Assert.That(item.MaxStack, Is.EqualTo(4));
        }

        [Test]
        public void WidthZeroIsRejected()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new ItemType(Guid.NewGuid(), 0, 1, 1));
        }

        [Test]
        public void NegativeHeightIsRejected()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new ItemType(Guid.NewGuid(), 1, -1, 1));
        }

        [Test]
        public void MaxStackZeroIsRejected()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new ItemType(Guid.NewGuid(), 1, 1, 0));
        }

        [Test]
        public void EmptyGuidIsRejected()
        {
            Assert.Throws<ArgumentException>(() => new ItemType(Guid.Empty, 1, 1, 1));
        }

        [Test]
        public void SameIdIsEqualEvenWhenSizesDiffer()
        {
            var id = Guid.NewGuid();
            var small = new ItemType(id, 1, 1, 1);
            var large = new ItemType(id, 4, 4, 9);

            Assert.That(small.Equals(large), Is.True);
            Assert.That(small == large, Is.True);
            Assert.That(small != large, Is.False);
            Assert.That(small.GetHashCode(), Is.EqualTo(large.GetHashCode()));
        }

        [Test]
        public void DifferentIdsAreNotEqual()
        {
            var a = new ItemType(Guid.NewGuid(), 1, 1, 1);
            var b = new ItemType(Guid.NewGuid(), 1, 1, 1);

            Assert.That(a.Equals(b), Is.False);
            Assert.That(a == b, Is.False);
            Assert.That(a != b, Is.True);
        }
    }
}