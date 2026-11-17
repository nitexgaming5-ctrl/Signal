using System.Collections.Generic;

namespace Signal.Core
{
    /// <summary>Runtime social-world state for one NPC, independent of its Unity presentation.</summary>
    public sealed class NpcAgentState
    {
        private readonly List<NpcKnowledge> _knowledge = new List<NpcKnowledge>();
        private readonly Dictionary<string, NpcRelationship> _relationships = new Dictionary<string, NpcRelationship>();

        public string Id { get; }
        public string LocationId { get; private set; }
        public float X { get; private set; }
        public float Y { get; private set; }
        public float AwarenessRadius { get; }

        internal NpcAgentState(string id, string locationId, float x, float y, float awarenessRadius)
        {
            Id = id;
            LocationId = locationId;
            X = x;
            Y = y;
            AwarenessRadius = awarenessRadius;
        }

        public IList<NpcKnowledge> GetKnowledge()
        {
            return _knowledge.ToArray();
        }

        public bool KnowsEvent(string eventId)
        {
            for (int i = 0; i < _knowledge.Count; i++)
                if (_knowledge[i].EventId == eventId)
                    return true;
            return false;
        }

        public NpcKnowledge FindKnowledge(string eventId)
        {
            for (int i = 0; i < _knowledge.Count; i++)
                if (_knowledge[i].EventId == eventId)
                    return _knowledge[i];
            return null;
        }

        public NpcRelationship GetRelationship(string otherNpcId)
        {
            return _relationships.TryGetValue(otherNpcId, out NpcRelationship relationship)
                ? relationship
                : NpcRelationship.Neutral;
        }

        internal void MoveTo(string locationId, float x, float y)
        {
            LocationId = locationId;
            X = x;
            Y = y;
        }

        internal void SetRelationship(string otherNpcId, NpcRelationship relationship)
        {
            _relationships[otherNpcId] = relationship;
        }

        internal bool Remember(NpcKnowledge knowledge)
        {
            if (KnowsEvent(knowledge.EventId))
                return false;

            _knowledge.Add(knowledge);
            return true;
        }
    }
}
