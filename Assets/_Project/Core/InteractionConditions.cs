using System;
using System.Collections.Generic;

namespace Signal.Core
{
    /// <summary>Current character/player context used to evaluate an authored interaction.</summary>
    public sealed class InteractionContext
    {
        public NpcActivity NpcActivity { get; }
        public NpcRelationship Relationship { get; }
        public PlayerDiscoveryLedger Discoveries { get; }

        public InteractionContext(NpcActivity npcActivity, NpcRelationship relationship,
            PlayerDiscoveryLedger discoveries)
        {
            NpcActivity = npcActivity;
            Relationship = relationship ?? NpcRelationship.Neutral;
            Discoveries = discoveries ?? throw new ArgumentNullException(nameof(discoveries));
        }
    }

    /// <summary>
    /// Reusable, data-oriented conditions for early NPC interactions. Conditions are
    /// conjunctive: every configured requirement must be satisfied.
    /// </summary>
    public sealed class InteractionConditionSet
    {
        private readonly string[] _requiredSignalIds;
        private readonly DiscoverySignalType[] _requiredSignalTypes;

        public NpcActivity? RequiredActivity { get; }
        public float MinimumFamiliarity { get; }
        public float MinimumTrust { get; }
        public float MinimumAffinity { get; }

        public InteractionConditionSet(NpcActivity? requiredActivity = null,
            float minimumFamiliarity = 0f, float minimumTrust = 0f, float minimumAffinity = -1f,
            IEnumerable<string> requiredSignalIds = null,
            IEnumerable<DiscoverySignalType> requiredSignalTypes = null)
        {
            ValidateRange(minimumFamiliarity, 0f, 1f, nameof(minimumFamiliarity));
            ValidateRange(minimumTrust, 0f, 1f, nameof(minimumTrust));
            ValidateRange(minimumAffinity, -1f, 1f, nameof(minimumAffinity));
            if (requiredActivity.HasValue && !Enum.IsDefined(typeof(NpcActivity), requiredActivity.Value))
                throw new ArgumentOutOfRangeException(nameof(requiredActivity));

            RequiredActivity = requiredActivity;
            MinimumFamiliarity = minimumFamiliarity;
            MinimumTrust = minimumTrust;
            MinimumAffinity = minimumAffinity;
            _requiredSignalIds = CopyIds(requiredSignalIds);
            _requiredSignalTypes = CopyTypes(requiredSignalTypes);
        }

        public bool IsSatisfiedBy(InteractionContext context, string subjectId)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));
            if (string.IsNullOrWhiteSpace(subjectId))
                throw new ArgumentException("An interaction subject id is required.", nameof(subjectId));

            if (RequiredActivity.HasValue && context.NpcActivity != RequiredActivity.Value)
                return false;
            if (context.Relationship.Familiarity < MinimumFamiliarity ||
                context.Relationship.Trust < MinimumTrust ||
                context.Relationship.Affinity < MinimumAffinity)
                return false;

            for (int i = 0; i < _requiredSignalIds.Length; i++)
                if (!context.Discoveries.HasSignal(_requiredSignalIds[i]))
                    return false;
            for (int i = 0; i < _requiredSignalTypes.Length; i++)
                if (!context.Discoveries.HasSignalType(subjectId, _requiredSignalTypes[i]))
                    return false;
            return true;
        }

        private static string[] CopyIds(IEnumerable<string> values)
        {
            if (values == null) return Array.Empty<string>();
            var result = new List<string>();
            var seen = new HashSet<string>(StringComparer.Ordinal);
            foreach (string value in values)
            {
                if (string.IsNullOrWhiteSpace(value))
                    throw new ArgumentException("Required signal ids cannot be empty.", nameof(values));
                if (seen.Add(value)) result.Add(value);
            }
            return result.ToArray();
        }

        private static DiscoverySignalType[] CopyTypes(IEnumerable<DiscoverySignalType> values)
        {
            if (values == null) return Array.Empty<DiscoverySignalType>();
            var result = new List<DiscoverySignalType>();
            var seen = new HashSet<DiscoverySignalType>();
            foreach (DiscoverySignalType value in values)
            {
                if (!Enum.IsDefined(typeof(DiscoverySignalType), value))
                    throw new ArgumentOutOfRangeException(nameof(values));
                if (seen.Add(value)) result.Add(value);
            }
            return result.ToArray();
        }

        private static void ValidateRange(float value, float minimum, float maximum, string name)
        {
            if (float.IsNaN(value) || float.IsInfinity(value) || value < minimum || value > maximum)
                throw new ArgumentOutOfRangeException(name);
        }
    }
}
