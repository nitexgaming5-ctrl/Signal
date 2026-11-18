using System;
using System.Collections.Generic;
namespace Signal.Core
{
    /// <summary>Authoritative ownership of physical item stacks, independent of their current container.</summary>
    public sealed class AssetOwnershipBook
    {
        private readonly Dictionary<Guid, EconomicActorId> _owners = new Dictionary<Guid, EconomicActorId>();
        public int Count => _owners.Count;
        public bool TryRegister(Guid itemInstanceId, EconomicActorId owner)
        {
            if (itemInstanceId == Guid.Empty || !owner.IsValid || _owners.ContainsKey(itemInstanceId)) return false;
            _owners.Add(itemInstanceId, owner);
            return true;
        }
        public bool TryGetOwner(Guid itemInstanceId, out EconomicActorId owner) => _owners.TryGetValue(itemInstanceId, out owner);
        public bool TryTransferOwnership(Guid itemInstanceId, EconomicActorId expectedOwner, EconomicActorId newOwner)
        {
            if (itemInstanceId == Guid.Empty || !expectedOwner.IsValid || !newOwner.IsValid || expectedOwner == newOwner ||
                !_owners.TryGetValue(itemInstanceId, out EconomicActorId currentOwner) || currentOwner != expectedOwner) return false;
            _owners[itemInstanceId] = newOwner;
            return true;
        }
        public bool TryRemove(Guid itemInstanceId, EconomicActorId expectedOwner)
        {
            if (itemInstanceId == Guid.Empty || !expectedOwner.IsValid ||
                !_owners.TryGetValue(itemInstanceId, out EconomicActorId currentOwner) || currentOwner != expectedOwner) return false;
            return _owners.Remove(itemInstanceId);
        }
    }
}
