using System;
using System.Collections.Generic;

namespace Signal.Core
{
    /// <summary>Persistent contents and progressive discovery state for one physical world container.</summary>
    public sealed class SearchableContainer
    {
        private readonly List<Guid> _searchOrder = new List<Guid>();
        private readonly HashSet<Guid> _knownOrderIds = new HashSet<Guid>();
        private readonly IReadOnlyList<Guid> _readOnlySearchOrder;

        public SearchableContainer(string containerId, int width = 8, int height = 6)
        {
            if (string.IsNullOrWhiteSpace(containerId))
                throw new ArgumentException("A stable container ID is required.", nameof(containerId));

            ContainerId = containerId;
            Inventory = new GridInventory(width, height);
            _readOnlySearchOrder = _searchOrder.AsReadOnly();
        }

        public string ContainerId { get; }
        public GridInventory Inventory { get; }
        public bool IsGenerated { get; private set; }
        public int DiscoveredCount { get; private set; }
        public bool HasSearchStarted { get; private set; }
        public int TotalGeneratedCount => _searchOrder.Count;
        public bool IsSearchComplete => IsGenerated && DiscoveredCount >= _searchOrder.Count;
        public IReadOnlyList<Guid> SearchOrder => _readOnlySearchOrder;

        /// <summary>Freezes the one-time reveal order after the caller has generated the contents.</summary>
        public bool TryInitializeSearchOrder(IList<Guid> itemOrder)
        {
            if (IsGenerated || itemOrder == null || itemOrder.Count != Inventory.Items.Count)
                return false;

            _searchOrder.Clear();
            _knownOrderIds.Clear();
            for (int i = 0; i < itemOrder.Count; i++)
            {
                Guid id = itemOrder[i];
                if (id == Guid.Empty || !_knownOrderIds.Add(id) ||
                    !Inventory.TryGetItemInstance(id, out _))
                {
                    _searchOrder.Clear();
                    _knownOrderIds.Clear();
                    return false;
                }

                _searchOrder.Add(id);
            }

            IsGenerated = true;
            DiscoveredCount = 0;
            return true;
        }

        /// <summary>Marks the container as actively searched; this flag survives closing and saving.</summary>
        public bool TryStartSearch()
        {
            if (!IsGenerated || IsSearchComplete)
                return false;
            HasSearchStarted = true;
            return true;
        }

        /// <summary>Reveals the next item in the saved order. The item remains in the container.</summary>
        public bool TryRevealNext(out Guid itemInstanceId)
        {
            if (!IsGenerated || !HasSearchStarted || IsSearchComplete)
            {
                itemInstanceId = Guid.Empty;
                return false;
            }

            itemInstanceId = _searchOrder[DiscoveredCount];
            DiscoveredCount++;
            return true;
        }

        public bool IsDiscovered(Guid itemInstanceId)
        {
            if (itemInstanceId == Guid.Empty)
                return false;

            for (int i = 0; i < DiscoveredCount; i++)
                if (_searchOrder[i] == itemInstanceId)
                    return true;

            return false;
        }

        public Guid[] GetDiscoveredItemIds()
        {
            var result = new Guid[DiscoveredCount];
            for (int i = 0; i < DiscoveredCount; i++)
                result[i] = _searchOrder[i];
            return result;
        }

        /// <summary>Restores saved progress without generating or rerolling contents.</summary>
        public bool TryRestoreSearchState(bool generated, IList<Guid> itemOrder, int discoveredCount)
        {
            return TryRestoreSearchState(generated, itemOrder, discoveredCount, false);
        }

        public bool TryRestoreSearchState(bool generated, IList<Guid> itemOrder, int discoveredCount, bool searchStarted)
        {
            if (itemOrder == null || discoveredCount < 0 || discoveredCount > itemOrder.Count)
                return false;
            if (!generated && (itemOrder.Count != 0 || discoveredCount != 0 || searchStarted))
                return false;
            if (discoveredCount > 0 && !searchStarted)
                return false;

            var unique = new HashSet<Guid>();
            for (int i = 0; i < itemOrder.Count; i++)
                if (itemOrder[i] == Guid.Empty || !unique.Add(itemOrder[i]))
                    return false;
            if (!generated && Inventory.Items.Count != 0)
                return false;
            foreach (PlacedItem item in Inventory.Items)
                if (!unique.Contains(item.ItemInstanceId))
                    return false;

            _searchOrder.Clear();
            _knownOrderIds.Clear();
            for (int i = 0; i < itemOrder.Count; i++)
            {
                _searchOrder.Add(itemOrder[i]);
                _knownOrderIds.Add(itemOrder[i]);
            }

            IsGenerated = generated;
            DiscoveredCount = discoveredCount;
            HasSearchStarted = searchStarted;
            return true;
        }

        /// <summary>Used when loading an older save that has no state for this container.</summary>
        public void Reset()
        {
            Inventory.Clear();
            _searchOrder.Clear();
            _knownOrderIds.Clear();
            IsGenerated = false;
            DiscoveredCount = 0;
            HasSearchStarted = false;
        }
    }
}
