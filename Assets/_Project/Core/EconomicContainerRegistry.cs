using System.Collections.Generic;
namespace Signal.Core
{
    /// <summary>Maps physical inventory containers to the actor responsible for that location.</summary>
    public sealed class EconomicContainerRegistry
    {
        private readonly Dictionary<GridInventory, EconomicActorId> _custodians =
            new Dictionary<GridInventory, EconomicActorId>();
        public int Count => _custodians.Count;
        public bool TryRegister(GridInventory container, EconomicActorId custodian)
        {
            if (container == null || !custodian.IsValid || _custodians.ContainsKey(container)) return false;
            _custodians.Add(container, custodian);
            return true;
        }
        public bool TryGetCustodian(GridInventory container, out EconomicActorId custodian)
        {
            if (container == null) { custodian = default; return false; }
            return _custodians.TryGetValue(container, out custodian);
        }
        public bool TryUnregister(GridInventory container)
        {
            return container != null && _custodians.Remove(container);
        }
    }
}
