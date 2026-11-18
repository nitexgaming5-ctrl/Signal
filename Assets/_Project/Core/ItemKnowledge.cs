using System;

namespace Signal.Core
{
    public enum ItemKnowledgeAspect
    {
        Quality = 0,
        Condition = 1,
        Authenticity = 2,
        Ownership = 3
    }

    public enum ItemKnowledgeSource
    {
        DirectInspection = 0,
        Conversation = 1,
        Rumour = 2,
        Document = 3
    }

    /// <summary>
    /// One belief the player has about one physical item stack. Value is intentionally
    /// independent from the item's authoritative state: a rumour can be wrong.
    /// </summary>
    public readonly struct ItemKnowledgeFact : IEquatable<ItemKnowledgeFact>
    {
        public ItemKnowledgeFact(Guid itemInstanceId, ItemKnowledgeAspect aspect, int value,
            ItemKnowledgeSource source, float confidence, int learnedDay, int learnedMinuteOfDay)
        {
            if (itemInstanceId == Guid.Empty)
                throw new ArgumentException("An item knowledge fact requires a stable item identity.", nameof(itemInstanceId));
            if (!Enum.IsDefined(typeof(ItemKnowledgeAspect), aspect))
                throw new ArgumentOutOfRangeException(nameof(aspect));
            if (!Enum.IsDefined(typeof(ItemKnowledgeSource), source))
                throw new ArgumentOutOfRangeException(nameof(source));
            if (!IsValidValue(aspect, value))
                throw new ArgumentOutOfRangeException(nameof(value), "The value is not valid for this item knowledge aspect.");
            if (float.IsNaN(confidence) || float.IsInfinity(confidence) || confidence < 0f || confidence > 1f)
                throw new ArgumentOutOfRangeException(nameof(confidence));
            if (learnedDay < 1)
                throw new ArgumentOutOfRangeException(nameof(learnedDay));
            if (learnedMinuteOfDay < 0 || learnedMinuteOfDay >= 1440)
                throw new ArgumentOutOfRangeException(nameof(learnedMinuteOfDay));

            ItemInstanceId = itemInstanceId;
            Aspect = aspect;
            Value = value;
            Source = source;
            Confidence = confidence;
            LearnedDay = learnedDay;
            LearnedMinuteOfDay = learnedMinuteOfDay;
        }

        public Guid ItemInstanceId { get; }
        public ItemKnowledgeAspect Aspect { get; }
        public int Value { get; }
        public ItemKnowledgeSource Source { get; }
        public float Confidence { get; }
        public int LearnedDay { get; }
        public int LearnedMinuteOfDay { get; }

        public bool Equals(ItemKnowledgeFact other)
        {
            return ItemInstanceId == other.ItemInstanceId && Aspect == other.Aspect && Value == other.Value &&
                Source == other.Source && Confidence.Equals(other.Confidence) &&
                LearnedDay == other.LearnedDay && LearnedMinuteOfDay == other.LearnedMinuteOfDay;
        }

        public override bool Equals(object obj) => obj is ItemKnowledgeFact other && Equals(other);
        public override int GetHashCode()
        {
            unchecked
            {
                int hash = ItemInstanceId.GetHashCode();
                hash = (hash * 397) ^ (int)Aspect;
                hash = (hash * 397) ^ Value;
                hash = (hash * 397) ^ (int)Source;
                hash = (hash * 397) ^ Confidence.GetHashCode();
                hash = (hash * 397) ^ LearnedDay;
                return (hash * 397) ^ LearnedMinuteOfDay;
            }
        }

        private static bool IsValidValue(ItemKnowledgeAspect aspect, int value)
        {
            switch (aspect)
            {
                case ItemKnowledgeAspect.Quality: return Enum.IsDefined(typeof(ItemQuality), value);
                case ItemKnowledgeAspect.Condition: return Enum.IsDefined(typeof(ItemCondition), value);
                case ItemKnowledgeAspect.Authenticity: return Enum.IsDefined(typeof(ItemAuthenticity), value);
                case ItemKnowledgeAspect.Ownership: return Enum.IsDefined(typeof(ItemOwnershipStatus), value);
                default: return false;
            }
        }
    }
}
