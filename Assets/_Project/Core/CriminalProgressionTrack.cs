using System;

namespace Signal.Core
{
    /// <summary>One global, non-decaying progression total shared by all criminal activities.</summary>
    public sealed class CriminalProgressionTrack
    {
        private int _points;

        public int Points => _points;

        public void Award(int points)
        {
            if (points <= 0)
                return;
            _points = _points > int.MaxValue - points ? int.MaxValue : _points + points;
        }

        public void Restore(int points)
        {
            _points = Math.Max(0, points);
        }
    }
}
