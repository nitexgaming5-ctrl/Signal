using System.Collections.Generic;

namespace Signal.Core
{
    /// <summary>
    /// A character-scoped view of the shared world. It contains nearby people and only
    /// the knowledge held by this character; it never exposes the global event journal.
    /// </summary>
    public sealed class NpcPerception
    {
        public string ObserverNpcId { get; }
        public string LocationId { get; }
        public IList<string> NearbyNpcIds { get; }
        public IList<NpcPerceivedPerson> NearbyPeople { get; }
        public IList<NpcKnowledge> KnownEvents { get; }

        internal NpcPerception(string observerNpcId, string locationId,
            IList<string> nearbyNpcIds, IList<NpcPerceivedPerson> nearbyPeople, IList<NpcKnowledge> knownEvents)
        {
            ObserverNpcId = observerNpcId;
            LocationId = locationId;
            NearbyNpcIds = nearbyNpcIds;
            NearbyPeople = nearbyPeople;
            KnownEvents = knownEvents;
        }

        public NpcKnowledge FindEvent(string eventId)
        {
            for (int i = 0; i < KnownEvents.Count; i++)
                if (KnownEvents[i].EventId == eventId)
                    return KnownEvents[i];
            return null;
        }

        public bool KnowsEvent(string eventId)
        {
            for (int i = 0; i < KnownEvents.Count; i++)
                if (KnownEvents[i].EventId == eventId)
                    return true;
            return false;
        }
    }
}
