using System;
using System.Collections.Generic;

namespace Signal.Core
{
    public readonly struct PlacedItem
    {
        public PlacedItem(int instanceId, ItemType type, int x, int y, bool rotated, int count)
            : this(instanceId, Guid.NewGuid(), type, x, y, rotated, count, ItemInstanceState.Default)
        {
        }

        internal PlacedItem(int instanceId, Guid itemInstanceId, ItemType type, int x, int y, bool rotated, int count, ItemInstanceState state)
        {
            InstanceId = instanceId;
            ItemInstanceId = itemInstanceId;
            Type = type;
            X = x;
            Y = y;
            Rotated = rotated;
            Count = count;
            State = state;
        }

        public int InstanceId { get; }
        public Guid ItemInstanceId { get; }
        public ItemType Type { get; }
        public int X { get; }
        public int Y { get; }
        public bool Rotated { get; }
        public int Count { get; }
        public ItemInstanceState State { get; }

        public int FootprintWidth => Rotated ? Type.Height : Type.Width;

        public int FootprintHeight => Rotated ? Type.Width : Type.Height;
    }

    /// <summary>A saved copy of an inventory's contents, used to undo a multi-step change.</summary>
    public sealed class InventorySnapshot
    {
        internal InventorySnapshot(PlacedItem[] items, int nextInstanceId)
        {
            Items = items;
            NextInstanceId = nextInstanceId;
        }

        internal PlacedItem[] Items { get; }

        internal int NextInstanceId { get; }
    }

    public sealed class GridInventory
    {
        public const int DefaultWidth = 6;
        public const int DefaultHeight = 4;

        // Instance ids start at 1, so 0 never matches a real item.
        private const int NoInstance = 0;

        private readonly List<PlacedItem> _items = new List<PlacedItem>();
        private readonly IReadOnlyList<PlacedItem> _readOnlyItems;
        private int _nextInstanceId = 1;

        public GridInventory(int width = DefaultWidth, int height = DefaultHeight)
        {
            if (width < 1)
                throw new ArgumentOutOfRangeException(nameof(width), "Width must be at least 1.");
            if (height < 1)
                throw new ArgumentOutOfRangeException(nameof(height), "Height must be at least 1.");

            Width = width;
            Height = height;
            _readOnlyItems = _items.AsReadOnly();
        }

        public int Width { get; }

        public int Height { get; }

        public IReadOnlyList<PlacedItem> Items => _readOnlyItems;

        public event Action<GridInventory> Changed;

        public bool CanPlace(ItemType type, int x, int y, bool rotated)
        {
            return Fits(type, x, y, rotated, NoInstance);
        }

        public int? Place(ItemType type, int x, int y, bool rotated = false, int count = 1, Guid? itemInstanceId = null, ItemInstanceState? state = null)
        {
            if (count < 1 || count > type.MaxStack)
                return null;
            if (!CanPlace(type, x, y, rotated))
                return null;

            Guid stableId = itemInstanceId ?? Guid.NewGuid();
            if (stableId == Guid.Empty || _items.Exists(existing => existing.ItemInstanceId == stableId))
                return null;

            int id = _nextInstanceId++;
            _items.Add(new PlacedItem(id, stableId, type, x, y, rotated, count, state ?? ItemInstanceState.Default));
            Changed?.Invoke(this);
            return id;
        }

        public bool Remove(int instanceId)
        {
            int index = IndexOf(instanceId);
            if (index < 0)
                return false;

            _items.RemoveAt(index);
            Changed?.Invoke(this);
            return true;
        }

        internal bool TryRemoveItemInstance(Guid itemInstanceId)
        {
            int index = _items.FindIndex(item => item.ItemInstanceId == itemInstanceId);
            if (index < 0)
                return false;

            _items.RemoveAt(index);
            Changed?.Invoke(this);
            return true;
        }

        internal bool TryPlaceTransferred(PlacedItem item)
        {
            if (item.ItemInstanceId == Guid.Empty || item.Count < 1 || item.Count > item.Type.MaxStack)
                return false;
            if (_items.Exists(existing => existing.ItemInstanceId == item.ItemInstanceId))
                return false;
            if (!TryFindFreeSpot(item.Type, item.Rotated, out int x, out int y))
                return false;

            int localId = _nextInstanceId++;
            _items.Add(new PlacedItem(localId, item.ItemInstanceId, item.Type, x, y, item.Rotated, item.Count, item.State));
            Changed?.Invoke(this);
            return true;
        }

        public bool TryGetItemInstance(Guid itemInstanceId, out PlacedItem item)
        {
            int index = _items.FindIndex(candidate => candidate.ItemInstanceId == itemInstanceId);
            if (index < 0)
            {
                item = default;
                return false;
            }

            item = _items[index];
            return true;
        }

        public bool TryGet(int instanceId, out PlacedItem item)
        {
            int index = IndexOf(instanceId);
            if (index < 0)
            {
                item = default;
                return false;
            }

            item = _items[index];
            return true;
        }

        public bool TryMove(int instanceId, int x, int y, bool rotated)
        {
            int index = IndexOf(instanceId);
            if (index < 0)
                return false;

            PlacedItem current = _items[index];

            // The item being moved must not block its own new position.
            if (!Fits(current.Type, x, y, rotated, instanceId))
                return false;

            _items[index] = new PlacedItem(instanceId, current.ItemInstanceId, current.Type, x, y, rotated, current.Count, current.State);
            Changed?.Invoke(this);
            return true;
        }

        /// <summary>Total quantity of an item across all its stacks.</summary>
        public int CountOf(ItemType type)
        {
            int total = 0;
            foreach (var item in _items)
            {
                if (item.Type == type)
                    total += item.Count;
            }

            return total;
        }

        /// <summary>
        /// Removes a quantity of an item, taking from the most recently placed stacks first.
        /// Returns false and changes nothing if there is not enough.
        /// </summary>
        public bool TryRemove(ItemType type, int count)
        {
            if (count < 1 || CountOf(type) < count)
                return false;

            int remaining = count;
            for (int i = _items.Count - 1; i >= 0 && remaining > 0; i--)
            {
                PlacedItem item = _items[i];
                if (item.Type != type)
                    continue;

                int taken = Math.Min(item.Count, remaining);
                remaining -= taken;

                if (taken == item.Count)
                    _items.RemoveAt(i);
                else
                    _items[i] = new PlacedItem(item.InstanceId, item.ItemInstanceId, item.Type, item.X, item.Y, item.Rotated, item.Count - taken, item.State);
            }

            Changed?.Invoke(this);
            return true;
        }

        public void Clear()
        {
            if (_items.Count == 0)
                return;

            _items.Clear();
            Changed?.Invoke(this);
        }

        public InventorySnapshot CreateSnapshot()
        {
            return new InventorySnapshot(_items.ToArray(), _nextInstanceId);
        }

        public void Restore(InventorySnapshot snapshot)
        {
            if (snapshot == null)
                throw new ArgumentNullException(nameof(snapshot));

            _items.Clear();
            _items.AddRange(snapshot.Items);
            _nextInstanceId = snapshot.NextInstanceId;
            Changed?.Invoke(this);
        }

        /// <summary>Finds the item covering a cell, if any. Used by the UI to pick items up.</summary>
        public bool TryGetItemAt(int x, int y, out PlacedItem item)
        {
            foreach (var candidate in _items)
            {
                bool insideX = x >= candidate.X && x < candidate.X + candidate.FootprintWidth;
                bool insideY = y >= candidate.Y && y < candidate.Y + candidate.FootprintHeight;
                if (insideX && insideY)
                {
                    item = candidate;
                    return true;
                }
            }

            item = default;
            return false;
        }

        /// <summary>Same check as TryMove, but changes nothing. Used by the UI for drop previews.</summary>
        public bool CanMove(int instanceId, int x, int y, bool rotated)
        {
            int index = IndexOf(instanceId);
            return index >= 0 && Fits(_items[index].Type, x, y, rotated, instanceId);
        }

        public bool TryRotate(int instanceId)
        {
            if (!TryGet(instanceId, out PlacedItem current))
                return false;

            // Same position, flipped orientation. TryMove checks the fit and fires Changed.
            return TryMove(instanceId, current.X, current.Y, !current.Rotated);
        }

        /// <summary>
        /// Adds a quantity of an item: tops up existing stacks first, then places new stacks
        /// in the first free spot (rows top to bottom, left to right, unrotated).
        /// Returns how many could not be placed.
        /// </summary>
        public int Add(ItemType type, int count)
        {
            return Add(type, count, ItemInstanceState.Default);
        }

        public int Add(ItemType type, int count, ItemInstanceState state)
        {
            if (count < 1)
                return 0;
            if (type.MaxStack < 1)
                return count; // an uninitialised ItemType can never be stored

            int remaining = count;
            bool changed = false;

            for (int i = 0; i < _items.Count && remaining > 0; i++)
            {
                PlacedItem existing = _items[i];
                if (existing.Type != type || existing.State != state)
                    continue;

                int room = existing.Type.MaxStack - existing.Count;
                if (room <= 0)
                    continue;

                int moved = Math.Min(room, remaining);
                _items[i] = new PlacedItem(existing.InstanceId, existing.ItemInstanceId, existing.Type, existing.X, existing.Y,
                                           existing.Rotated, existing.Count + moved, existing.State);
                remaining -= moved;
                changed = true;
            }

            while (remaining > 0)
            {
                if (!TryFindFreeSpot(type, false, out int x, out int y))
                    break;

                int stack = Math.Min(remaining, type.MaxStack);
                _items.Add(new PlacedItem(_nextInstanceId++, Guid.NewGuid(), type, x, y, false, stack, state));
                remaining -= stack;
                changed = true;
            }

            if (changed)
                Changed?.Invoke(this);

            return remaining;
        }

        private bool TryFindFreeSpot(ItemType type, bool rotated, out int x, out int y)
        {
            for (int cy = 0; cy < Height; cy++)
            {
                for (int cx = 0; cx < Width; cx++)
                {
                    if (Fits(type, cx, cy, rotated, NoInstance))
                    {
                        x = cx;
                        y = cy;
                        return true;
                    }
                }
            }

            x = 0;
            y = 0;
            return false;
        }

        private bool Fits(ItemType type, int x, int y, bool rotated, int ignoreInstanceId)
        {
            int w = FootprintWidth(type, rotated);
            int h = FootprintHeight(type, rotated);

            // Written as "x > Width - w" so huge coordinates cannot overflow.
            if (x < 0 || y < 0 || x > Width - w || y > Height - h)
                return false;

            foreach (var other in _items)
            {
                if (other.InstanceId == ignoreInstanceId)
                    continue;

                int ow = FootprintWidth(other.Type, other.Rotated);
                int oh = FootprintHeight(other.Type, other.Rotated);

                // Strict comparisons: rectangles that only touch at an edge do not overlap.
                bool overlaps = x < other.X + ow && other.X < x + w
                             && y < other.Y + oh && other.Y < y + h;
                if (overlaps)
                    return false;
            }

            return true;
        }

        private int IndexOf(int instanceId)
        {
            for (int i = 0; i < _items.Count; i++)
            {
                if (_items[i].InstanceId == instanceId)
                    return i;
            }

            return -1;
        }

        private static int FootprintWidth(ItemType type, bool rotated) => rotated ? type.Height : type.Width;

        private static int FootprintHeight(ItemType type, bool rotated) => rotated ? type.Width : type.Height;
    }
}
