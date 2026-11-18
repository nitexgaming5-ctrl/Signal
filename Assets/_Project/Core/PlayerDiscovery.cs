using System;

namespace Signal.Core
{
    public enum DiscoverySignalType
    {
        RoutineObservation = 0,
        Conversation = 1,
        PhysicalEvidence = 2,
        Rumour = 3
    }

    /// <summary>
    /// A source-preserving record of something the player personally learned.
    /// The record describes the signal, not the conclusion the player draws from it.
    /// </summary>
    public readonly struct PlayerDiscoverySignal : IEquatable<PlayerDiscoverySignal>
    {
        public PlayerDiscoverySignal(string signalId, string subjectId, DiscoverySignalType type,
            string sourceId, string description, float confidence, int learnedDay, int learnedMinuteOfDay)
        {
            if (string.IsNullOrWhiteSpace(signalId))
                throw new ArgumentException("A stable signal id is required.", nameof(signalId));
            if (string.IsNullOrWhiteSpace(subjectId))
                throw new ArgumentException("A subject id is required.", nameof(subjectId));
            if (!Enum.IsDefined(typeof(DiscoverySignalType), type))
                throw new ArgumentOutOfRangeException(nameof(type));
            if (string.IsNullOrWhiteSpace(sourceId))
                throw new ArgumentException("A source id is required.", nameof(sourceId));
            if (float.IsNaN(confidence) || float.IsInfinity(confidence) || confidence < 0f || confidence > 1f)
                throw new ArgumentOutOfRangeException(nameof(confidence));
            if (learnedDay < 1)
                throw new ArgumentOutOfRangeException(nameof(learnedDay));
            if (learnedMinuteOfDay < 0 || learnedMinuteOfDay >= 1440)
                throw new ArgumentOutOfRangeException(nameof(learnedMinuteOfDay));

            SignalId = signalId;
            SubjectId = subjectId;
            Type = type;
            SourceId = sourceId;
            Description = description ?? string.Empty;
            Confidence = confidence;
            LearnedDay = learnedDay;
            LearnedMinuteOfDay = learnedMinuteOfDay;
        }

        public string SignalId { get; }
        public string SubjectId { get; }
        public DiscoverySignalType Type { get; }
        public string SourceId { get; }
        public string Description { get; }
        public float Confidence { get; }
        public int LearnedDay { get; }
        public int LearnedMinuteOfDay { get; }

        public bool Equals(PlayerDiscoverySignal other)
        {
            return SignalId == other.SignalId && SubjectId == other.SubjectId && Type == other.Type &&
                SourceId == other.SourceId && Description == other.Description &&
                Confidence.Equals(other.Confidence) && LearnedDay == other.LearnedDay &&
                LearnedMinuteOfDay == other.LearnedMinuteOfDay;
        }

        public override bool Equals(object obj) => obj is PlayerDiscoverySignal other && Equals(other);
        public override int GetHashCode()
        {
            unchecked
            {
                int hash = SignalId.GetHashCode();
                hash = (hash * 397) ^ SubjectId.GetHashCode();
                hash = (hash * 397) ^ (int)Type;
                hash = (hash * 397) ^ SourceId.GetHashCode();
                hash = (hash * 397) ^ Description.GetHashCode();
                hash = (hash * 397) ^ Confidence.GetHashCode();
                hash = (hash * 397) ^ LearnedDay;
                return (hash * 397) ^ LearnedMinuteOfDay;
            }
        }
    }
}
