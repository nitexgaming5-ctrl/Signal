using System;
using System.Collections.Generic;

namespace Signal.Core
{
    /// <summary>
    /// Player-to-NPC social impressions. This is separate from NPC-to-NPC relationships
    /// held by NpcSocialWorld; values are directed from the player toward each resident.
    /// </summary>
    public sealed class PlayerRelationshipBook
    {
        private readonly Dictionary<string, NpcRelationship> _relationships =
            new Dictionary<string, NpcRelationship>(StringComparer.Ordinal);

        public event Action<PlayerRelationshipBook> Changed;
        public int Count => _relationships.Count;

        public IReadOnlyList<PlayerNpcRelationshipEntry> Entries
        {
            get
            {
                var result = new List<PlayerNpcRelationshipEntry>(_relationships.Count);
                foreach (var pair in _relationships)
                    result.Add(new PlayerNpcRelationshipEntry(pair.Key, pair.Value));
                result.Sort((left, right) => string.CompareOrdinal(left.NpcId, right.NpcId));
                return result.AsReadOnly();
            }
        }

        public NpcRelationship Get(string npcId)
        {
            if (string.IsNullOrWhiteSpace(npcId))
                throw new ArgumentException("An NPC id is required.", nameof(npcId));
            return _relationships.TryGetValue(npcId, out NpcRelationship relationship)
                ? relationship
                : NpcRelationship.Neutral;
        }

        public void Adjust(string npcId, float familiarityDelta = 0f,
            float trustDelta = 0f, float affinityDelta = 0f)
        {
            if (string.IsNullOrWhiteSpace(npcId))
                throw new ArgumentException("An NPC id is required.", nameof(npcId));
            ValidateDelta(familiarityDelta, nameof(familiarityDelta));
            ValidateDelta(trustDelta, nameof(trustDelta));
            ValidateDelta(affinityDelta, nameof(affinityDelta));

            NpcRelationship current = Get(npcId);
            var updated = new NpcRelationship(current.Familiarity + familiarityDelta,
                current.Trust + trustDelta, current.Affinity + affinityDelta);
            if (updated.Familiarity.Equals(current.Familiarity) &&
                updated.Trust.Equals(current.Trust) && updated.Affinity.Equals(current.Affinity))
                return;

            _relationships[npcId] = updated;
            Changed?.Invoke(this);
        }

        public void Set(string npcId, float familiarity, float trust, float affinity)
        {
            if (string.IsNullOrWhiteSpace(npcId))
                throw new ArgumentException("An NPC id is required.", nameof(npcId));
            ValidateDelta(familiarity, nameof(familiarity));
            ValidateDelta(trust, nameof(trust));
            ValidateDelta(affinity, nameof(affinity));

            var updated = new NpcRelationship(familiarity, trust, affinity);
            _relationships[npcId] = updated;
            Changed?.Invoke(this);
        }

        public void Clear()
        {
            if (_relationships.Count == 0)
                return;
            _relationships.Clear();
            Changed?.Invoke(this);
        }

        private static void ValidateDelta(float value, string name)
        {
            if (float.IsNaN(value) || float.IsInfinity(value))
                throw new ArgumentOutOfRangeException(name);
        }
    }

    public readonly struct PlayerNpcRelationshipEntry
    {
        public string NpcId { get; }
        public NpcRelationship Relationship { get; }

        public PlayerNpcRelationshipEntry(string npcId, NpcRelationship relationship)
        {
            if (string.IsNullOrWhiteSpace(npcId))
                throw new ArgumentException("An NPC id is required.", nameof(npcId));
            NpcId = npcId;
            Relationship = relationship ?? throw new ArgumentNullException(nameof(relationship));
        }
    }
}
