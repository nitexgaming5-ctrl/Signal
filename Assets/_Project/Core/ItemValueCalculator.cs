using System;

namespace Signal.Core
{
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
