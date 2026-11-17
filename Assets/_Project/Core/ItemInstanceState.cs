using System;

namespace Signal.Core
{
    public enum ItemQuality
    {
        Standard = 0,
        Poor = 1,
        Fine = 2,
        Exceptional = 3
    }

    public enum ItemCondition
    {
        Good = 0,
        Worn = 1,
        Damaged = 2,
        Pristine = 3
    }

    public enum ItemAuthenticity
    {
        Genuine = 0,
        Counterfeit = 1
    }

    public enum ItemOwnershipStatus
    {
        Legitimate = 0,
        Stolen = 1
    }

    /// <summary>
    /// Mutable facts belonging to one physical stack, not to its shared item definition.
    /// These are currently authoritative item facts; player discovery/knowledge is a separate future layer.
    /// </summary>
    public readonly struct ItemInstanceState : IEquatable<ItemInstanceState>
    {
        public ItemInstanceState(ItemQuality quality, ItemCondition condition,
                                 ItemAuthenticity authenticity, ItemOwnershipStatus ownership)
        {
            if (!Enum.IsDefined(typeof(ItemQuality), quality))
                throw new ArgumentOutOfRangeException(nameof(quality));
            if (!Enum.IsDefined(typeof(ItemCondition), condition))
                throw new ArgumentOutOfRangeException(nameof(condition));
            if (!Enum.IsDefined(typeof(ItemAuthenticity), authenticity))
                throw new ArgumentOutOfRangeException(nameof(authenticity));
            if (!Enum.IsDefined(typeof(ItemOwnershipStatus), ownership))
                throw new ArgumentOutOfRangeException(nameof(ownership));

            Quality = quality;
            Condition = condition;
            Authenticity = authenticity;
            Ownership = ownership;
        }

        public ItemQuality Quality { get; }
        public ItemCondition Condition { get; }
        public ItemAuthenticity Authenticity { get; }
        public ItemOwnershipStatus Ownership { get; }

        public static ItemInstanceState Default => new ItemInstanceState(
            ItemQuality.Standard, ItemCondition.Good, ItemAuthenticity.Genuine, ItemOwnershipStatus.Legitimate);

        public bool Equals(ItemInstanceState other)
        {
            return Quality == other.Quality && Condition == other.Condition &&
                   Authenticity == other.Authenticity && Ownership == other.Ownership;
        }

        public override bool Equals(object obj) => obj is ItemInstanceState other && Equals(other);

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = (int)Quality;
                hash = (hash * 397) ^ (int)Condition;
                hash = (hash * 397) ^ (int)Authenticity;
                hash = (hash * 397) ^ (int)Ownership;
                return hash;
            }
        }

        public static bool operator ==(ItemInstanceState left, ItemInstanceState right) => left.Equals(right);
        public static bool operator !=(ItemInstanceState left, ItemInstanceState right) => !left.Equals(right);
    }

    public readonly struct ItemValueBreakdown
    {
        public ItemValueBreakdown(int baseValue, float qualityMultiplier, float conditionMultiplier,
                                  float authenticityMultiplier, float ownershipMultiplier)
        {
            BaseValue = Math.Max(0, baseValue);
            QualityMultiplier = qualityMultiplier;
            ConditionMultiplier = conditionMultiplier;
            AuthenticityMultiplier = authenticityMultiplier;
            OwnershipMultiplier = ownershipMultiplier;
            FinalValue = (int)Math.Round(BaseValue * qualityMultiplier * conditionMultiplier *
                                         authenticityMultiplier * ownershipMultiplier,
                                         MidpointRounding.AwayFromZero);
        }

        public int BaseValue { get; }
        public float QualityMultiplier { get; }
        public float ConditionMultiplier { get; }
        public float AuthenticityMultiplier { get; }
        public float OwnershipMultiplier { get; }
        public int FinalValue { get; }
    }

    /// <summary>
    /// Central first-pass valuation. Multipliers are provisional tuning, not a buy/sell price or economy.
    /// </summary>
    public static class ItemValueCalculator
    {
        public static ItemValueBreakdown Calculate(int baseValue, ItemInstanceState state)
        {
            float quality;
            switch (state.Quality)
            {
                case ItemQuality.Poor: quality = 0.65f; break;
                case ItemQuality.Fine: quality = 1.35f; break;
                case ItemQuality.Exceptional: quality = 1.75f; break;
                default: quality = 1f; break;
            }

            float condition;
            switch (state.Condition)
            {
                case ItemCondition.Damaged: condition = 0.45f; break;
                case ItemCondition.Worn: condition = 0.7f; break;
                case ItemCondition.Pristine: condition = 1.1f; break;
                default: condition = 1f; break;
            }

            float authenticity = state.Authenticity == ItemAuthenticity.Counterfeit ? 0.1f : 1f;
            float ownership = state.Ownership == ItemOwnershipStatus.Stolen ? 0.6f : 1f;
            return new ItemValueBreakdown(baseValue, quality, condition, authenticity, ownership);
        }
    }
}