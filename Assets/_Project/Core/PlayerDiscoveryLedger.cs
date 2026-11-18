using System;
using System.Collections.Generic;

namespace Signal.Core
{
    /// <summary>
    /// Player-only memory of discovery signals. It deliberately stores observations,
    /// conversations, evidence and rumours separately; it does not turn them into truth.
    /// </summary>
    public sealed class PlayerDiscoveryLedger
    {
        private readonly Dictionary<string, PlayerDiscoverySignal> _signals =
            new Dictionary<string, PlayerDiscoverySignal>(StringComparer.Ordinal);

        public event Action<PlayerDiscoveryLedger> Changed;
        public int Count => _signals.Count;

        public IReadOnlyList<PlayerDiscoverySignal> Signals
        {
            get
            {
                var result = new List<PlayerDiscoverySignal>(_signals.Values);
                result.Sort((left, right) =>
                {
                    int subject = string.CompareOrdinal(left.SubjectId, right.SubjectId);
                    if (subject != 0) return subject;
                    int day = left.LearnedDay.CompareTo(right.LearnedDay);
                    if (day != 0) return day;
                    int minute = left.LearnedMinuteOfDay.CompareTo(right.LearnedMinuteOfDay);
                    return minute != 0 ? minute : string.CompareOrdinal(left.SignalId, right.SignalId);
                });
                return result.AsReadOnly();
            }
        }

        /// <summary>
        /// Adds a signal once. Re-observing the same authored occurrence is idempotent;
        /// a genuinely new occurrence must have a different stable signal id.
        /// </summary>
        public bool Record(PlayerDiscoverySignal signal)
        {
            if (_signals.ContainsKey(signal.SignalId))
                return false;

            _signals.Add(signal.SignalId, signal);
            Changed?.Invoke(this);
            return true;
        }

        public bool TryGet(string signalId, out PlayerDiscoverySignal signal)
        {
            return _signals.TryGetValue(signalId, out signal);
        }

        public bool HasSignal(string signalId)
        {
            return !string.IsNullOrWhiteSpace(signalId) && _signals.ContainsKey(signalId);
        }

        public bool HasSignalType(string subjectId, DiscoverySignalType type, float minimumConfidence = 0f)
        {
            ValidateConfidence(minimumConfidence);
            foreach (PlayerDiscoverySignal signal in _signals.Values)
                if (signal.SubjectId == subjectId && signal.Type == type &&
                    signal.Confidence >= minimumConfidence)
                    return true;
            return false;
        }

        public int CountDistinctSignalTypes(string subjectId, float minimumConfidence = 0f)
        {
            ValidateConfidence(minimumConfidence);
            bool[] seen = new bool[Enum.GetValues(typeof(DiscoverySignalType)).Length];
            int count = 0;
            foreach (PlayerDiscoverySignal signal in _signals.Values)
            {
                int type = (int)signal.Type;
                if (signal.SubjectId == subjectId && signal.Confidence >= minimumConfidence && !seen[type])
                {
                    seen[type] = true;
                    count++;
                }
            }
            return count;
        }

        public IReadOnlyList<PlayerDiscoverySignal> GetForSubject(string subjectId)
        {
            var result = new List<PlayerDiscoverySignal>();
            foreach (PlayerDiscoverySignal signal in _signals.Values)
                if (signal.SubjectId == subjectId)
                    result.Add(signal);
            result.Sort((left, right) =>
            {
                int day = left.LearnedDay.CompareTo(right.LearnedDay);
                if (day != 0) return day;
                int minute = left.LearnedMinuteOfDay.CompareTo(right.LearnedMinuteOfDay);
                return minute != 0 ? minute : string.CompareOrdinal(left.SignalId, right.SignalId);
            });
            return result.AsReadOnly();
        }

        public void Clear()
        {
            if (_signals.Count == 0)
                return;
            _signals.Clear();
            Changed?.Invoke(this);
        }

        private static void ValidateConfidence(float confidence)
        {
            if (float.IsNaN(confidence) || float.IsInfinity(confidence) || confidence < 0f || confidence > 1f)
                throw new ArgumentOutOfRangeException(nameof(confidence));
        }
    }
}
