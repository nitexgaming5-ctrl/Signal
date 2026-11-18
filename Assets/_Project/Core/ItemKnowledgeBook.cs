using System;
using System.Collections.Generic;

namespace Signal.Core
{
    /// <summary>
    /// Player-scoped knowledge about physical item stacks. This records beliefs, not truth;
    /// authoritative item state remains on PlacedItem and may disagree with a rumour.
    /// </summary>
    public sealed class ItemKnowledgeBook
    {
        private readonly Dictionary<Guid, Dictionary<ItemKnowledgeAspect, ItemKnowledgeFact>> _facts =
            new Dictionary<Guid, Dictionary<ItemKnowledgeAspect, ItemKnowledgeFact>>();

        public event Action<ItemKnowledgeBook> Changed;

        public int FactCount
        {
            get
            {
                int count = 0;
                foreach (var entry in _facts.Values)
                    count += entry.Count;
                return count;
            }
        }

        public IReadOnlyList<ItemKnowledgeFact> Facts
        {
            get
            {
                var result = new List<ItemKnowledgeFact>(FactCount);
                foreach (var entry in _facts.Values)
                    result.AddRange(entry.Values);
                result.Sort((left, right) =>
                {
                    int identity = string.CompareOrdinal(left.ItemInstanceId.ToString("N"), right.ItemInstanceId.ToString("N"));
                    return identity != 0 ? identity : left.Aspect.CompareTo(right.Aspect);
                });
                return result.AsReadOnly();
            }
        }

        /// <summary>Records or replaces the player's latest belief about one item property.</summary>
        public void RecordFact(Guid itemInstanceId, ItemKnowledgeAspect aspect, int value,
            ItemKnowledgeSource source, float confidence, int learnedDay, int learnedMinuteOfDay)
        {
            var fact = new ItemKnowledgeFact(itemInstanceId, aspect, value, source,
                confidence, learnedDay, learnedMinuteOfDay);

            if (!_facts.TryGetValue(itemInstanceId, out var itemFacts))
            {
                itemFacts = new Dictionary<ItemKnowledgeAspect, ItemKnowledgeFact>();
                _facts.Add(itemInstanceId, itemFacts);
            }

            if (itemFacts.TryGetValue(aspect, out ItemKnowledgeFact existing) && existing.Equals(fact))
                return;

            itemFacts[aspect] = fact;
            Changed?.Invoke(this);
        }

        public bool TryGetFact(Guid itemInstanceId, ItemKnowledgeAspect aspect, out ItemKnowledgeFact fact)
        {
            if (_facts.TryGetValue(itemInstanceId, out var itemFacts) && itemFacts.TryGetValue(aspect, out fact))
                return true;

            fact = default;
            return false;
        }

        public bool TryGetKnownState(Guid itemInstanceId, out ItemInstanceState state)
        {
            if (TryGetFact(itemInstanceId, ItemKnowledgeAspect.Quality, out ItemKnowledgeFact quality) &&
                TryGetFact(itemInstanceId, ItemKnowledgeAspect.Condition, out ItemKnowledgeFact condition) &&
                TryGetFact(itemInstanceId, ItemKnowledgeAspect.Authenticity, out ItemKnowledgeFact authenticity) &&
                TryGetFact(itemInstanceId, ItemKnowledgeAspect.Ownership, out ItemKnowledgeFact ownership))
            {
                state = new ItemInstanceState((ItemQuality)quality.Value, (ItemCondition)condition.Value,
                    (ItemAuthenticity)authenticity.Value, (ItemOwnershipStatus)ownership.Value);
                return true;
            }

            state = default;
            return false;
        }

        /// <summary>
        /// Explicitly records all four properties as known. Call only when an authored
        /// interaction genuinely establishes the complete state for the player.
        /// </summary>
        public void RecordFullState(Guid itemInstanceId, ItemInstanceState state, ItemKnowledgeSource source,
            float confidence, int learnedDay, int learnedMinuteOfDay)
        {
            RecordFact(itemInstanceId, ItemKnowledgeAspect.Quality, (int)state.Quality,
                source, confidence, learnedDay, learnedMinuteOfDay);
            RecordFact(itemInstanceId, ItemKnowledgeAspect.Condition, (int)state.Condition,
                source, confidence, learnedDay, learnedMinuteOfDay);
            RecordFact(itemInstanceId, ItemKnowledgeAspect.Authenticity, (int)state.Authenticity,
                source, confidence, learnedDay, learnedMinuteOfDay);
            RecordFact(itemInstanceId, ItemKnowledgeAspect.Ownership, (int)state.Ownership,
                source, confidence, learnedDay, learnedMinuteOfDay);
        }

        public void Clear()
        {
            if (_facts.Count == 0)
                return;

            _facts.Clear();
            Changed?.Invoke(this);
        }
    }
}
