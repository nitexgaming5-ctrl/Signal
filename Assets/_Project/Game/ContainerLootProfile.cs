using System;
using System.Collections.Generic;
using Signal.Core;
using UnityEngine;

namespace Signal.Game
{
    [CreateAssetMenu(menuName = "Signal/Container Loot Profile", fileName = "ContainerLootProfile")]
    public sealed class ContainerLootProfile : ScriptableObject
    {
        [Serializable]
        private sealed class LootEntry
        {
            public ItemDefinition item;
            [Min(1)] public int weight = 1;
            [Min(1)] public int minimumCount = 1;
            [Min(1)] public int maximumCount = 1;
        }

        [SerializeField, Min(1)] private int minimumRolls = 2;
        [SerializeField, Min(1)] private int maximumRolls = 4;
        [SerializeField] private LootEntry[] entries;

        public IEnumerable<ItemDefinition> Items
        {
            get
            {
                if (entries == null) yield break;
                foreach (LootEntry entry in entries)
                    if (entry != null && entry.item != null)
                        yield return entry.item;
            }
        }

        public bool TryGenerate(SearchableContainer container, out string error)
        {
            error = null;
            if (container == null || container.IsGenerated || container.Inventory.Items.Count != 0)
            {
                error = "The target container is unavailable or has already been generated.";
                return false;
            }
            if (entries == null || entries.Length == 0 || minimumRolls < 1 || maximumRolls < minimumRolls)
            {
                error = "The loot profile has no valid entries or roll range.";
                return false;
            }

            int totalWeight = 0;
            foreach (LootEntry entry in entries)
            {
                if (entry == null || entry.item == null || entry.weight < 1 ||
                    entry.minimumCount < 1 || entry.maximumCount < entry.minimumCount)
                    continue;
                try { entry.item.ToItemType(); }
                catch (InvalidOperationException)
                {
                    error = "The loot profile contains an item without a valid definition ID.";
                    return false;
                }
                totalWeight += entry.weight;
            }
            if (totalWeight <= 0)
            {
                error = "The loot profile contains no usable weighted entries.";
                return false;
            }

            var random = new System.Random(Guid.NewGuid().GetHashCode());
            int stackCount = random.Next(minimumRolls, maximumRolls + 1);
            for (int i = 0; i < stackCount; i++)
            {
                LootEntry selected = SelectEntry(random, totalWeight);
                ItemType type = selected.item.ToItemType();
                int count = Math.Min(type.MaxStack, random.Next(selected.minimumCount, selected.maximumCount + 1));
                int remaining = container.Inventory.Add(type, count, selected.item.DefaultInstanceState);
                if (remaining > 0)
                {
                    container.Reset();
                    error = "The loot profile does not fit inside the container inventory.";
                    return false;
                }
            }

            var order = new List<Guid>();
            foreach (PlacedItem item in container.Inventory.Items)
                order.Add(item.ItemInstanceId);
            for (int i = order.Count - 1; i > 0; i--)
            {
                int swap = random.Next(i + 1);
                Guid value = order[i];
                order[i] = order[swap];
                order[swap] = value;
            }

            if (!container.TryInitializeSearchOrder(order))
            {
                container.Reset();
                error = "The generated contents could not be assigned a search order.";
                return false;
            }
            return true;
        }

        private LootEntry SelectEntry(System.Random random, int totalWeight)
        {
            int roll = random.Next(totalWeight);
            foreach (LootEntry entry in entries)
            {
                if (entry == null || entry.item == null || entry.weight < 1 ||
                    entry.minimumCount < 1 || entry.maximumCount < entry.minimumCount)
                    continue;
                roll -= entry.weight;
                if (roll < 0)
                    return entry;
            }
            throw new InvalidOperationException("A validated loot profile failed to resolve a weighted entry.");
        }
    }
}