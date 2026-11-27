using System;
using System.Collections.Generic;

namespace Signal.Core
{
    public enum InventoryStorageKind
    {
        PlayerInventory = 0,
        PlayerBackpack = 1,
        PlayerPocket = 2,
        Hideout = 3,
        WorldContainer = 4,
        NpcPocket = 5,
        BusinessStock = 6,
        ContractEscrow = 7,
        ContactInventory = 8,
        Vehicle = 9,
        Workbench = 10,
        Other = 11
    }

    /// <summary>
    /// Stable identity of one physical/logical storage location. Identity is independent from
    /// item ownership, economic custody and whether the player currently has access to it.
    /// </summary>
    public readonly struct InventoryStorageId : IEquatable<InventoryStorageId>
    {
        public InventoryStorageId(InventoryStorageKind kind, string ownerId, string slotId = "")
        {
            if (!Enum.IsDefined(typeof(InventoryStorageKind), kind))
                throw new ArgumentOutOfRangeException(nameof(kind));
            if (string.IsNullOrWhiteSpace(ownerId))
                throw new ArgumentException("A stable storage owner ID is required.", nameof(ownerId));
            if (slotId == null)
                throw new ArgumentNullException(nameof(slotId));

            Kind = kind;
            OwnerId = ownerId.Trim();
            SlotId = slotId.Trim();
            Value = ((int)kind).ToString() + ":" + OwnerId.Length + ":" + OwnerId +
                ":" + SlotId.Length + ":" + SlotId;
        }

        public InventoryStorageKind Kind { get; }
        public string OwnerId { get; }
        public string SlotId { get; }
        public string Value { get; }
        public bool IsValid => !string.IsNullOrEmpty(Value);

        public bool Equals(InventoryStorageId other) =>
            string.Equals(Value, other.Value, StringComparison.Ordinal);

        public override bool Equals(object obj) =>
            obj is InventoryStorageId other && Equals(other);

        public override int GetHashCode() =>
            Value == null ? 0 : StringComparer.Ordinal.GetHashCode(Value);

        public override string ToString() => Value ?? string.Empty;
        public static bool operator ==(InventoryStorageId left, InventoryStorageId right) => left.Equals(right);
        public static bool operator !=(InventoryStorageId left, InventoryStorageId right) => !left.Equals(right);
    }

    /// <summary>
    /// Session-level authority mapping each stable storage identity to exactly one GridInventory.
    /// It deliberately does not grant access or determine item ownership/custody.
    /// Registrations are retained for the lifetime of the session, including while storage is
    /// dropped, hidden, locked or otherwise inaccessible.
    /// </summary>
    public sealed class InventoryStorageRegistry
    {
        private readonly Dictionary<InventoryStorageId, GridInventory> _byIdentity =
            new Dictionary<InventoryStorageId, GridInventory>();
        private readonly Dictionary<GridInventory, InventoryStorageId> _byInventory =
            new Dictionary<GridInventory, InventoryStorageId>();

        public int Count => _byIdentity.Count;

        public bool TryRegister(InventoryStorageId identity, GridInventory inventory)
        {
            if (!identity.IsValid || inventory == null ||
                _byIdentity.ContainsKey(identity) || _byInventory.ContainsKey(inventory))
                return false;

            _byIdentity.Add(identity, inventory);
            _byInventory.Add(inventory, identity);
            return true;
        }

        public bool TryGet(InventoryStorageId identity, out GridInventory inventory)
        {
            inventory = null;
            return identity.IsValid && _byIdentity.TryGetValue(identity, out inventory);
        }

        public bool TryGetIdentity(GridInventory inventory, out InventoryStorageId identity)
        {
            if (inventory == null)
            {
                identity = default;
                return false;
            }
            return _byInventory.TryGetValue(inventory, out identity);
        }

        public bool Owns(InventoryStorageId identity, GridInventory inventory) =>
            inventory != null && TryGet(identity, out GridInventory registered) &&
            ReferenceEquals(registered, inventory);

        public void Clear()
        {
            _byIdentity.Clear();
            _byInventory.Clear();
        }
    }
}