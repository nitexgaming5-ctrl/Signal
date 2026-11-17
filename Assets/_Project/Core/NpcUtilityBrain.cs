using System;

namespace Signal.Core
{
    public enum NpcActivity
    {
        Home,
        Work,
        Rest,
        Socialize
    }

    /// <summary>
    /// Tiny deterministic utility brain for ambient NPCs. It evaluates four needs-driven
    /// activities every fifteen in-game minutes and advances needs once per game minute.
    /// No Unity objects, random numbers, background threads or model inference are involved.
    /// </summary>
    public sealed class NpcUtilityBrain
    {
        private const int MinutesPerDay = 1440;
        private const int DecisionInterval = 15;

        private float _energy = 1f;
        private float _social = 0.65f;
        private int _lastAbsoluteMinute = -1;

        public NpcActivity CurrentActivity { get; private set; } = NpcActivity.Home;
        public float Energy => _energy;
        public float Social => _social;

        /// <summary>Advances the NPC to a world-clock position. Rewinds rebuild that day's state deterministically.</summary>
        public NpcActivity AdvanceTo(int day, int minuteOfDay)
        {
            int safeDay = Math.Max(1, day);
            int safeMinute = Math.Max(0, Math.Min(MinutesPerDay - 1, minuteOfDay));
            int absoluteMinute = (safeDay - 1) * MinutesPerDay + safeMinute;

            if (_lastAbsoluteMinute < 0 || absoluteMinute < _lastAbsoluteMinute)
            {
                _energy = 1f;
                _social = 0.65f;
                CurrentActivity = NpcActivity.Home;
                _lastAbsoluteMinute = (safeDay - 1) * MinutesPerDay - 1;
            }

            while (_lastAbsoluteMinute < absoluteMinute)
            {
                _lastAbsoluteMinute++;
                int minute = _lastAbsoluteMinute % MinutesPerDay;

                if (minute % DecisionInterval == 0)
                    CurrentActivity = ChooseActivity(minute);

                ApplyNeedDrift();
            }

            return CurrentActivity;
        }

        private NpcActivity ChooseActivity(int minute)
        {
            float work = IsBetween(minute, 390, 1080) ? 20f + _energy * 40f : -10f;
            float rest = (1f - _energy) * 70f + (IsBetween(minute, 690, 810) ? 25f : 0f);
            float socialize = (1f - _social) * 55f + (IsBetween(minute, 1080, 1260) ? 40f : 0f);
            float home = minute >= 1260 || minute < 390 ? 100f : -10f;

            // A small inertia bonus prevents rapid switching when two needs have similar scores.
            switch (CurrentActivity)
            {
                case NpcActivity.Work: work += 5f; break;
                case NpcActivity.Rest: rest += 5f; break;
                case NpcActivity.Socialize: socialize += 5f; break;
                case NpcActivity.Home: home += 5f; break;
            }

            if (work >= rest && work >= socialize && work >= home) return NpcActivity.Work;
            if (rest >= socialize && rest >= home) return NpcActivity.Rest;
            if (socialize >= home) return NpcActivity.Socialize;
            return NpcActivity.Home;
        }

        private void ApplyNeedDrift()
        {
            switch (CurrentActivity)
            {
                case NpcActivity.Work:
                    _energy -= 0.0012f;
                    _social -= 0.00045f;
                    break;
                case NpcActivity.Rest:
                    _energy += 0.003f;
                    _social += 0.0001f;
                    break;
                case NpcActivity.Socialize:
                    _energy -= 0.0004f;
                    _social += 0.002f;
                    break;
                default:
                    _energy += 0.0015f;
                    _social += 0.0007f;
                    break;
            }

            _energy = Clamp01(_energy);
            _social = Clamp01(_social);
        }

        private static bool IsBetween(int value, int startInclusive, int endExclusive)
        {
            return value >= startInclusive && value < endExclusive;
        }

        private static float Clamp01(float value)
        {
            return Math.Max(0f, Math.Min(1f, value));
        }
    }
}