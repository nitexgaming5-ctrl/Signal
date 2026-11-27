using System;
using System.Collections.Generic;
using NUnit.Framework;

namespace Signal.Core.Tests
{
    public sealed class PlayerQuickbarTests
    {
        [Test]
        public void AssignReplaceAndUniqueIdentityAreEnforced()
        {
            var bar = new PlayerQuickbar();
            Guid first = Guid.NewGuid();
            Guid second = Guid.NewGuid();

            Assert.IsTrue(bar.TryAssign(0, first));
            Assert.IsFalse(bar.TryAssign(1, first));
            Assert.IsTrue(bar.TryReplace(0, second));
            Assert.AreEqual(second, bar.GetItem(0));
            Assert.AreEqual(-1, bar.FindSlot(first));
        }

        [Test]
        public void SwapAndUnassignOnlyChangeReferences()
        {
            var bar = new PlayerQuickbar();
            Guid first = Guid.NewGuid();
            Guid second = Guid.NewGuid();
            Assert.IsTrue(bar.TryAssign(0, first));
            Assert.IsTrue(bar.TryAssign(1, second));

            Assert.IsTrue(bar.TrySwap(0, 1));
            Assert.AreEqual(second, bar.GetItem(0));
            Assert.AreEqual(first, bar.GetItem(1));
            Assert.IsTrue(bar.TryUnassign(0));
            Assert.AreEqual(Guid.Empty, bar.GetItem(0));
            Assert.AreEqual(first, bar.GetItem(1));
        }

        [Test]
        public void RestoreRejectsMissingAndDuplicateIdentities()
        {
            var bar = new PlayerQuickbar();
            Guid valid = Guid.NewGuid();
            Guid missing = Guid.NewGuid();
            int skipped = bar.Restore(new[] { valid, missing, valid }, id => id == valid);

            Assert.AreEqual(2, skipped);
            Assert.AreEqual(valid, bar.GetItem(0));
            Assert.AreEqual(Guid.Empty, bar.GetItem(1));
            Assert.AreEqual(Guid.Empty, bar.GetItem(2));
        }

        [Test]
        public void SaveMapperRoundTripsSixSlotsAndClearsMissingItems()
        {
            var bar = new PlayerQuickbar();
            Guid first = Guid.NewGuid();
            Guid sixth = Guid.NewGuid();
            Assert.IsTrue(bar.TryAssign(0, first));
            Assert.IsTrue(bar.TryAssign(5, sixth));
            var data = new SaveData();
            PlayerQuickbarSaveMapper.Capture(bar, data);

            var restored = new PlayerQuickbar();
            int skipped = PlayerQuickbarSaveMapper.Apply(data, restored, id => id == first || id == sixth);
            Assert.AreEqual(0, skipped);
            Assert.AreEqual(first, restored.GetItem(0));
            Assert.AreEqual(sixth, restored.GetItem(5));

            skipped = PlayerQuickbarSaveMapper.Apply(data, restored, id => id == first);
            Assert.AreEqual(1, skipped);
            Assert.AreEqual(Guid.Empty, restored.GetItem(5));
        }

        [Test]
        public void ClearItemRemovesOnlyMatchingReference()
        {
            var bar = new PlayerQuickbar();
            Guid first = Guid.NewGuid();
            Guid second = Guid.NewGuid();
            Assert.IsTrue(bar.TryAssign(0, first));
            Assert.IsTrue(bar.TryAssign(1, second));

            bar.ClearItem(first);

            Assert.AreEqual(Guid.Empty, bar.GetItem(0));
            Assert.AreEqual(second, bar.GetItem(1));
        }
    }
}