using System;
using System.Runtime.CompilerServices;

namespace Signal.Core
{
    /// <summary>
    /// An item value for inventory logic.
    /// </summary>
    public readonly struct ItemType : IEquatable<ItemType>
    {
        private readonly Guid _id;
        private readonly int _width;
        private readonly int _height;
        private readonly int _maxStack;
        private readonly bool _equippable;
        private readonly bool _portableBackpack;

        public Guid Id => _id;
        public int Width => _width;
        public int Height => _height;
        public int MaxStack => _equippable || _portableBackpack ? 1 : _maxStack;
        public bool Equippable => _equippable;
        public bool PortableBackpack => _portableBackpack;
        public bool IsStackable => IsValid && MaxStack > 1;
        public bool IsValid => _id != Guid.Empty && _width > 0 && _height > 0 && _maxStack > 0;

        public ItemType(Guid id, int width, int height, int maxStack, bool equippable = false, bool portableBackpack = false)
        {
            if (id == Guid.Empty)
            {
                throw new ArgumentException("Id must not be Guid.Empty", nameof(id));
            }

            if (width < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(width), "Width must be greater than zero");
            }

            if (height < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(height), "Height must be greater than zero");
            }

            if (maxStack < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(maxStack), "MaxStack must be greater than zero");
            }

            _id = id;
            _width = width;
            _height = height;
            _maxStack = maxStack;
            _equippable = equippable;
            _portableBackpack = portableBackpack;
        }

        public bool Equals(ItemType other)
        {
            return _id == other._id;
        }

        public override bool Equals(object obj)
        {
            return obj is ItemType other && Equals(other);
        }

        public override int GetHashCode()
        {
            return _id.GetHashCode();
        }

        public static bool operator ==(ItemType left, ItemType right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(ItemType left, ItemType right)
        {
            return !(left == right);
        }
    }
}
