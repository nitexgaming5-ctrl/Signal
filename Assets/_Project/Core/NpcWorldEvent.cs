using System;

namespace Signal.Core
{
    /// <summary>
    /// A world occurrence expressed only as information an observer could perceive.
    /// Hidden causes and unobserved facts belong elsewhere; this is not omniscient truth.
    /// </summary>
    public sealed class NpcWorldEvent
    {
        public string EventId { get; }
        public string Kind { get; }
        public string Description { get; }
        public string ActorNpcId { get; }
        public string LocationId { get; }
        public float X { get; }
        public float Y { get; }
        public float VisibilityRadius { get; }
        public int Day { get; }
        public int MinuteOfDay { get; }

        public NpcWorldEvent(string eventId, string kind, string description, string actorNpcId,
            string locationId, float x, float y, float visibilityRadius, int day, int minuteOfDay)
        {
            if (string.IsNullOrWhiteSpace(eventId)) throw new ArgumentException("Event id is required.", nameof(eventId));
            if (string.IsNullOrWhiteSpace(kind)) throw new ArgumentException("Event kind is required.", nameof(kind));
            if (string.IsNullOrWhiteSpace(locationId)) throw new ArgumentException("Location id is required.", nameof(locationId));
            if (visibilityRadius < 0f) throw new ArgumentOutOfRangeException(nameof(visibilityRadius));

            EventId = eventId;
            Kind = kind;
            Description = description ?? string.Empty;
            ActorNpcId = actorNpcId ?? string.Empty;
            LocationId = locationId;
            X = x;
            Y = y;
            VisibilityRadius = visibilityRadius;
            Day = Math.Max(1, day);
            MinuteOfDay = Math.Max(0, Math.Min(1439, minuteOfDay));
        }
    }
}
