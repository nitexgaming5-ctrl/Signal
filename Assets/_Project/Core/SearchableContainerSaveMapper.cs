using System;
using System.Collections.Generic;

namespace Signal.Core
{
    /// <summary>Persists searchable container contents and reveal progress alongside the main save.</summary>
    public static class SearchableContainerSaveMapper
    {
        public static void Capture(IReadOnlyList<SearchableContainer> containers, SaveData destination)
        {
            if (destination == null) throw new ArgumentNullException(nameof(destination));
            destination.searchableContainers.Clear();
            if (containers == null) return;

            for (int i = 0; i < containers.Count; i++)
            {
                SearchableContainer container = containers[i];
                if (container == null) continue;

                var saved = new SavedSearchableContainer
                {
                    containerId = container.ContainerId,
                    generated = container.IsGenerated,
                    searchStarted = container.HasSearchStarted,
                    discoveredCount = container.DiscoveredCount
                };

                foreach (Guid id in container.SearchOrder)
                    saved.searchOrder.Add(id.ToString());

                foreach (PlacedItem item in container.Inventory.Items)
                {
                    saved.items.Add(new SavedItem
                    {
                        typeId = item.Type.Id.ToString(),
                        instanceId = item.ItemInstanceId.ToString(),
                        x = item.X,
                        y = item.Y,
                        rotated = item.Rotated,
                        count = item.Count,
                        quality = (int)item.State.Quality,
                        condition = (int)item.State.Condition,
                        authenticity = (int)item.State.Authenticity,
                        ownership = (int)item.State.Ownership
                    });
                }

                destination.searchableContainers.Add(saved);
            }
        }

        /// <returns>Number of saved container/item records that could not be restored.</returns>
        public static int Apply(SaveData source, IReadOnlyList<SearchableContainer> destinations,
            Func<Guid, ItemType?> resolve)
        {
            if (destinations == null) return 0;
            if (resolve == null) throw new ArgumentNullException(nameof(resolve));

            int skipped = 0;
            var savedById = new Dictionary<string, SavedSearchableContainer>(StringComparer.Ordinal);
            if (source != null && source.searchableContainers != null)
            {
                foreach (SavedSearchableContainer saved in source.searchableContainers)
                {
                    if (saved == null || string.IsNullOrWhiteSpace(saved.containerId) ||
                        savedById.ContainsKey(saved.containerId))
                    {
                        skipped++;
                        continue;
                    }
                    savedById.Add(saved.containerId, saved);
                }
            }

            for (int i = 0; i < destinations.Count; i++)
            {
                SearchableContainer destination = destinations[i];
                if (destination == null) continue;
                destination.Reset();

                if (!savedById.TryGetValue(destination.ContainerId, out SavedSearchableContainer saved))
                    continue;

                if (saved.items != null)
                {
                    foreach (SavedItem item in saved.items)
                    {
                        if (item == null || !Guid.TryParse(item.typeId, out Guid typeId) ||
                            !Guid.TryParse(item.instanceId, out Guid instanceId))
                        {
                            skipped++;
                            continue;
                        }

                        ItemType? type = resolve(typeId);
                        ItemInstanceState state;
                        try
                        {
                            state = new ItemInstanceState((ItemQuality)item.quality, (ItemCondition)item.condition,
                                (ItemAuthenticity)item.authenticity, (ItemOwnershipStatus)item.ownership);
                        }
                        catch (ArgumentOutOfRangeException)
                        {
                            skipped++;
                            continue;
                        }

                        if (!type.HasValue || !destination.Inventory.Place(type.Value, item.x, item.y,
                            item.rotated, item.count, instanceId, state).HasValue)
                            skipped++;
                    }
                }

                var order = new List<Guid>();
                if (saved.searchOrder != null)
                {
                    foreach (string value in saved.searchOrder)
                    {
                        if (!Guid.TryParse(value, out Guid id))
                        {
                            skipped++;
                            continue;
                        }
                        order.Add(id);
                    }
                }

                if (!destination.TryRestoreSearchState(saved.generated, order, saved.discoveredCount, saved.searchStarted))
                {
                    destination.Reset();
                    skipped++;
                }
            }

            return skipped;
        }
    }
}
