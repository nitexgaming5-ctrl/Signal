using NUnit.Framework;

namespace Signal.Core.Tests
{
    public sealed class InventoryDragMathTests
    {
        [TestCase(10f, 10f, 0, 0)]
        [TestCase(32f, 0f, 1, 0)]
        [TestCase(63.9f, 64f, 1, 2)]
        [TestCase(191.9f, 127.9f, 5, 3)]
        public void TryGetCellMapsPointsToCells(float x, float y, int expectedX, int expectedY)
        {
            bool found = InventoryDragMath.TryGetCell(x, y, 32f, 6, 4, out int cellX, out int cellY);

            Assert.That(found, Is.True);
            Assert.That(cellX, Is.EqualTo(expectedX));
            Assert.That(cellY, Is.EqualTo(expectedY));
        }

        [TestCase(-0.1f, 0f)]
        [TestCase(0f, -0.1f)]
        [TestCase(192f, 0f)]
        [TestCase(0f, 128f)]
        [TestCase(float.NaN, 0f)]
        public void TryGetCellRejectsPointsOutsideTheGrid(float x, float y)
        {
            Assert.That(InventoryDragMath.TryGetCell(x, y, 32f, 6, 4, out _, out _), Is.False);
        }

        [Test]
        public void TryGetCellRejectsANonPositiveCellSize()
        {
            Assert.That(InventoryDragMath.TryGetCell(10f, 10f, 0f, 6, 4, out _, out _), Is.False);
        }

        [Test]
        public void RotateGrabOffsetMovesTheRightCellOfADominoToTheBottom()
        {
            InventoryDragMath.RotateGrabOffsetClockwise(1, 0, 2, 1, out int x, out int y);

            Assert.That(x, Is.EqualTo(0));
            Assert.That(y, Is.EqualTo(1));
        }

        [Test]
        public void RotateGrabOffsetMovesTheBottomCellOfAVerticalBarToTheLeft()
        {
            InventoryDragMath.RotateGrabOffsetClockwise(0, 2, 1, 3, out int x, out int y);

            Assert.That(x, Is.EqualTo(0));
            Assert.That(y, Is.EqualTo(0));
        }

        [Test]
        public void RotateGrabOffsetClampsOffsetsOutsideTheFootprint()
        {
            InventoryDragMath.RotateGrabOffsetClockwise(9, 9, 2, 1, out int x, out int y);

            Assert.That(x, Is.EqualTo(0));
            Assert.That(y, Is.EqualTo(1));
        }
    }
}
