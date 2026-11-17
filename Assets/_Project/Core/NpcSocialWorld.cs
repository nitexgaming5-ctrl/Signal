using System;
using System.Collections.Generic;

namespace Signal.Core
{
    /// <summary>
    /// Shared, deterministic social context for all NPCs in the simulation.
    /// Events are perceived locally; knowledge is held by individuals; information only
    /// spreads when a system explicitly asks one NPC to tell another.
    /// </summary>
    public sealed class NpcSocialWorld
    {
        private readonly Dictionary<string, NpcAgentState> _npcs = new Dictionary<string, NpcAgentState>();
        private readonly List<NpcWorldEvent> _eventJournal = new List<NpcWorldEvent>();

        public int NpcCount => _npcs.Count;
        public int EventCount => _eventJournal.Count;

        public NpcAgentState RegisterNpc(string id, string locationId, float x, float y, float awarenessRadius)
        {
            if (string.IsNullOrWhiteSpace(id)) throw new ArgumentException("NPC id is required.", nameof(id));
            if (string.IsNullOrWhiteSpace(locationId)) throw new ArgumentException("Location id is required.", nameof(locationId));
            if (awarenessRadius < 0f) throw new ArgumentOutOfRangeException(nameof(awarenessRadius));
            if (_npcs.ContainsKey(id)) throw new InvalidOperationException("NPC is already registered: " + id);

            var state = new NpcAgentState(id, locationId, x, y, awarenessRadius);
            _npcs.Add(id, state);
            return state;
        }

        public bool TryGetNpc(string id, out NpcAgentState state)
        {
            return _npcs.TryGetValue(id, out state);
        }

        public void MoveNpc(string id, string locationId, float x, float y)
        {
            if (!_npcs.TryGetValue(id, out NpcAgentState state))
                throw new KeyNotFoundException("NPC is not registered: " + id);
            if (string.IsNullOrWhiteSpace(locationId))
                throw new ArgumentException("Location id is required.", nameof(locationId));

            state.MoveTo(locationId, x, y);
        }

        public IList<string> GetNearbyNpcIds(string observerNpcId, float radius)
        {
            if (radius < 0f) throw new ArgumentOutOfRangeException(nameof(radius));
            if (!_npcs.TryGetValue(observerNpcId, out NpcAgentState observer))
                throw new KeyNotFoundException("NPC is not registered: " + observerNpcId);

            var nearby = new List<string>();
            float radiusSquared = radius * radius;
            foreach (KeyValuePair<string, NpcAgentState> pair in _npcs)
            {
                NpcAgentState other = pair.Value;
                if (other.Id == observerNpcId || other.LocationId != observer.LocationId)
                    continue;

                float dx = other.X - observer.X;
                float dy = other.Y - observer.Y;
                if (dx * dx + dy * dy <= radiusSquared)
                    nearby.Add(other.Id);
            }

            nearby.Sort(StringComparer.Ordinal);
            return nearby;
        }

        public void SetRelationship(string observerNpcId, string otherNpcId, NpcRelationship relationship)
        {
            if (observerNpcId == otherNpcId)
                throw new ArgumentException("An NPC cannot have a relationship with itself.", nameof(otherNpcId));
            if (relationship == null) throw new ArgumentNullException(nameof(relationship));
            if (!_npcs.TryGetValue(observerNpcId, out NpcAgentState observer))
                throw new KeyNotFoundException("NPC is not registered: " + observerNpcId);
            if (!_npcs.ContainsKey(otherNpcId))
                throw new KeyNotFoundException("NPC is not registered: " + otherNpcId);

            observer.SetRelationship(otherNpcId, relationship);
        }

        public void PublishEvent(NpcWorldEvent worldEvent)
        {
            if (worldEvent == null) throw new ArgumentNullException(nameof(worldEvent));
            for (int i = 0; i < _eventJournal.Count; i++)
                if (_eventJournal[i].EventId == worldEvent.EventId)
                    throw new InvalidOperationException("World event id is already in use: " + worldEvent.EventId);

            _eventJournal.Add(worldEvent);

            foreach (NpcAgentState npc in _npcs.Values)
            {
                bool isActor = npc.Id == worldEvent.ActorNpcId;
                bool canObserve = npc.LocationId == worldEvent.LocationId &&
                    DistanceSquared(npc.X, npc.Y, worldEvent.X, worldEvent.Y) <= worldEvent.VisibilityRadius * worldEvent.VisibilityRadius;

                if (isActor || canObserve)
                    npc.Remember(NpcKnowledge.Observe(worldEvent));
            }
        }

        /// <summary>
        /// Explicitly passes an event from one NPC to another. The listener's trust in the
        /// speaker affects confidence; it does not change the event itself or create certainty.
        /// </summary>
        public bool ShareKnowledge(string speakerNpcId, string listenerNpcId, string eventId, int day, int minuteOfDay)
        {
            if (!_npcs.TryGetValue(speakerNpcId, out NpcAgentState speaker))
                throw new KeyNotFoundException("NPC is not registered: " + speakerNpcId);
            if (!_npcs.TryGetValue(listenerNpcId, out NpcAgentState listener))
                throw new KeyNotFoundException("NPC is not registered: " + listenerNpcId);
            if (speakerNpcId == listenerNpcId)
                throw new ArgumentException("An NPC cannot tell itself something.", nameof(listenerNpcId));

            NpcKnowledge known = speaker.FindKnowledge(eventId);
            if (known == null || listener.KnowsEvent(eventId))
                return false;

            float trust = listener.GetRelationship(speakerNpcId).Trust;
            float confidence = known.Confidence * (0.35f + trust * 0.65f);
            var retold = known.Retell(speakerNpcId, confidence, Math.Max(1, day),
                Math.Max(0, Math.Min(1439, minuteOfDay)));
            return listener.Remember(retold);
        }

        public IList<NpcWorldEvent> GetEventJournal()
        {
            return _eventJournal.ToArray();
        }

        private static float DistanceSquared(float ax, float ay, float bx, float by)
        {
            float dx = ax - bx;
            float dy = ay - by;
            return dx * dx + dy * dy;
        }
    }
}
