using System;
using System.Collections.Generic;

namespace Signal.Core
{
    /// <summary>Authoritative record of one completed crime action, separate from witness knowledge and heat.</summary>
    public sealed class CrimeOutcome
    {
        public Guid OutcomeId { get; }
        public string ActionId { get; }
        public string ActorId { get; }
        public Guid AssetInstanceId { get; }
        public string LocationId { get; }
        public string WitnessNpcId { get; }
        public string WitnessEventId { get; }
        public string WitnessDescription { get; }
        public float WitnessX { get; }
        public float WitnessY { get; }
        public float WitnessRadius { get; }
        public int Day { get; }
        public int MinuteOfDay { get; }
        public int HeatAward { get; }
        public int ProgressionAward { get; }
        public bool WasWitnessed => !string.IsNullOrWhiteSpace(WitnessNpcId);

        public CrimeOutcome(Guid outcomeId, string actionId, string actorId, Guid assetInstanceId, string locationId,
            string witnessNpcId, string witnessEventId, string witnessDescription,
            float witnessX, float witnessY, float witnessRadius, int day, int minuteOfDay,
            int heatAward, int progressionAward)
        {
            if (outcomeId == Guid.Empty) throw new ArgumentException("Outcome id is required.", nameof(outcomeId));
            if (string.IsNullOrWhiteSpace(actionId)) throw new ArgumentException("Crime action id is required.", nameof(actionId));
            if (string.IsNullOrWhiteSpace(actorId)) throw new ArgumentException("Crime actor id is required.", nameof(actorId));
            if (assetInstanceId == Guid.Empty) throw new ArgumentException("Asset instance id is required.", nameof(assetInstanceId));
            if (string.IsNullOrWhiteSpace(locationId)) throw new ArgumentException("Location id is required.", nameof(locationId));
            if (day < 1)
                throw new ArgumentOutOfRangeException(nameof(day), "Crime day must be positive.");
            if (minuteOfDay < 0 || minuteOfDay >= 1440)
                throw new ArgumentOutOfRangeException(nameof(minuteOfDay), "Crime minute must be within the day.");
            if (heatAward < 0)
                throw new ArgumentOutOfRangeException(nameof(heatAward), "Heat award cannot be negative.");
            if (progressionAward < 0)
                throw new ArgumentOutOfRangeException(nameof(progressionAward), "Progression award cannot be negative.");
            if (float.IsNaN(witnessRadius) || float.IsInfinity(witnessRadius) || witnessRadius < 0f)
                throw new ArgumentOutOfRangeException(nameof(witnessRadius), "Witness radius is invalid.");
            if (float.IsNaN(witnessX) || float.IsInfinity(witnessX))
                throw new ArgumentOutOfRangeException(nameof(witnessX), "Witness X position is invalid.");
            if (float.IsNaN(witnessY) || float.IsInfinity(witnessY))
                throw new ArgumentOutOfRangeException(nameof(witnessY), "Witness Y position is invalid.");
            bool witnessed = !string.IsNullOrWhiteSpace(witnessNpcId);
            if (witnessed != !string.IsNullOrWhiteSpace(witnessEventId))
                throw new ArgumentException("A witness and witness event id must either both be present or both be absent.");
            if (witnessed && string.IsNullOrWhiteSpace(witnessDescription))
                throw new ArgumentException("A witnessed crime requires an observable description.", nameof(witnessDescription));

            OutcomeId = outcomeId;
            ActionId = actionId;
            ActorId = actorId;
            AssetInstanceId = assetInstanceId;
            LocationId = locationId;
            WitnessNpcId = witnessed ? witnessNpcId : string.Empty;
            WitnessEventId = witnessed ? witnessEventId : string.Empty;
            WitnessDescription = witnessed ? witnessDescription ?? string.Empty : string.Empty;
            WitnessX = witnessX;
            WitnessY = witnessY;
            WitnessRadius = witnessRadius;
            Day = day;
            MinuteOfDay = minuteOfDay;
            HeatAward = heatAward;
            ProgressionAward = progressionAward;
        }
    }

    public sealed class CrimeOutcomeLedger
    {
        private readonly List<CrimeOutcome> _outcomes = new List<CrimeOutcome>();
        private readonly HashSet<Guid> _outcomeIds = new HashSet<Guid>();
        private readonly IReadOnlyList<CrimeOutcome> _readOnlyOutcomes;

        public CrimeOutcomeLedger() { _readOnlyOutcomes = _outcomes.AsReadOnly(); }
        public IReadOnlyList<CrimeOutcome> Outcomes => _readOnlyOutcomes;
        public int Count => _outcomes.Count;
        public bool Contains(Guid outcomeId) => _outcomeIds.Contains(outcomeId);

        public void Clear()
        {
            _outcomes.Clear();
            _outcomeIds.Clear();
        }

        public bool TryRecord(CrimeOutcome outcome)
        {
            if (outcome == null || !_outcomeIds.Add(outcome.OutcomeId))
                return false;
            _outcomes.Add(outcome);
            return true;
        }
    }
}
