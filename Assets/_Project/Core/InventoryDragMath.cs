using System;

namespace Signal.Core
{
    /// <summary>Pure helpers for the inventory UI, kept out of Unity code so they can be tested.</summary>
    public static class InventoryDragMath
    {
        /// <summary>
        /// Converts a point, measured in pixels from the grid's top-left corner (x to the right,
        /// y downwards), into a cell. Returns false when the point is outside the grid.
        /// </summary>
        public static bool TryGetCell(float pointX, float pointY, float cellSize, int gridWidth, int gridHeight,
                                      out int cellX, out int cellY)
        {
            cellX = 0;
            cellY = 0;

            // The "!(a > b)" form also rejects NaN.
            if (!(cellSize > 0f) || gridWidth < 1 || gridHeight < 1)
                return false;
            if (!(pointX >= 0f) || !(pointY >= 0f))
                return false;
            if (pointX >= gridWidth * cellSize || pointY >= gridHeight * cellSize)
                return false;

            cellX = Math.Min((int)(pointX / cellSize), gridWidth - 1);
            cellY = Math.Min((int)(pointY / cellSize), gridHeight - 1);
            return true;
        }

        /// <summary>
        /// Where the held cell of an item ends up after the item is turned a quarter turn clockwise.
        /// footprintWidth and footprintHeight describe the item before the turn.
        /// </summary>
        public static void RotateGrabOffsetClockwise(int offsetX, int offsetY, int footprintWidth, int footprintHeight,
                                                     out int newOffsetX, out int newOffsetY)
        {
            int x = Math.Max(0, Math.Min(offsetX, footprintWidth - 1));
            int y = Math.Max(0, Math.Min(offsetY, footprintHeight - 1));

            newOffsetX = footprintHeight - 1 - y;
            newOffsetY = x;
        }
    }
}
