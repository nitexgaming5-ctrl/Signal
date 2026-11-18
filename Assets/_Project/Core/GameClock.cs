using System;

namespace Signal.Core
{
    /// <summary>Day counter plus time of day, in whole minutes.</summary>
    public sealed class GameClock
    {
        public const int MinutesPerDay = 1440;
        public const int DefaultStartMinute = 480; // 08:00
        public const int DefaultWakeMinute = 360;  // 06:00

        public GameClock(int day = 1, int minuteOfDay = DefaultStartMinute)
        {
            Day = Math.Max(1, day);
            MinuteOfDay = ClampMinute(minuteOfDay);
        }

        public bool IsPaused { get; private set; }

        public int Day { get; private set; }

        public int MinuteOfDay { get; private set; }

        public int Hour => MinuteOfDay / 60;

        public int Minute => MinuteOfDay % 60;

        public string Label => string.Format("Day {0} {1:00}:{2:00}", Day, Hour, Minute);

        /// <summary>Fires once for every midnight crossed, after Day has increased.</summary>
        public event Action<GameClock> DayStarted;

        public event Action<GameClock> Changed;

        public void SetPaused(bool paused)
        {
            IsPaused = paused;
        }

        public void Advance(int minutes)
        {
            if (minutes <= 0)
                return;

            long total = (long)MinuteOfDay + minutes;
            int daysPassed = (int)(total / MinutesPerDay);
            MinuteOfDay = (int)(total % MinutesPerDay);

            for (int i = 0; i < daysPassed; i++)
            {
                Day++;
                DayStarted?.Invoke(this);
            }

            Changed?.Invoke(this);
        }

        /// <summary>Skips ahead to the next time the clock reads wakeMinute (a full day if it already does).</summary>
        public void Sleep(int wakeMinute = DefaultWakeMinute)
        {
            int wake = ClampMinute(wakeMinute);
            int delta = (wake - MinuteOfDay + MinutesPerDay) % MinutesPerDay;
            if (delta == 0)
                delta = MinutesPerDay;

            Advance(delta);
        }

        /// <summary>Sets the clock directly (used when loading a save). Does not fire DayStarted.</summary>
        public void Restore(int day, int minuteOfDay)
        {
            Day = Math.Max(1, day);
            MinuteOfDay = ClampMinute(minuteOfDay);
            Changed?.Invoke(this);
        }

        private static int ClampMinute(int minute)
        {
            return Math.Max(0, Math.Min(minute, MinutesPerDay - 1));
        }
    }
}
