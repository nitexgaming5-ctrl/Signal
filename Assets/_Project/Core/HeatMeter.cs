using System;

namespace Signal.Core
{
    /// <summary>Attention from the wrong people, 0 to 100. Cools down a little each new day.</summary>
    public sealed class HeatMeter
    {
        public const int MaxHeat = 100;

        private readonly int _decayPerDay;
        private int _heat;

        public HeatMeter(int decayPerDay = 10)
        {
            if (decayPerDay < 0)
                throw new ArgumentOutOfRangeException(nameof(decayPerDay), "Decay cannot be negative.");

            _decayPerDay = decayPerDay;
        }

        public int Heat => _heat;

        public event Action<HeatMeter> Changed;

        public void Add(int amount)
        {
            if (amount <= 0)
                return;

            Set(_heat + amount);
        }

        public void Cool(int amount)
        {
            if (amount <= 0)
                return;

            Set(_heat - amount);
        }

        public void NewDay()
        {
            Cool(_decayPerDay);
        }

        public void Restore(int heat)
        {
            Set(heat);
        }

        private void Set(int value)
        {
            int clamped = Math.Max(0, Math.Min(value, MaxHeat));
            if (clamped == _heat)
                return;

            _heat = clamped;
            Changed?.Invoke(this);
        }
    }
}
