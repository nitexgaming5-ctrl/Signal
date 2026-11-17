namespace Signal.Core
{
    public enum NpcKnowledgeOrigin
    {
        DirectObservation,
        ToldByAnotherNpc
    }

    /// <summary>
    /// What one character currently believes about an event. Confidence and source are
    /// deliberately stored per character; the world does not give everyone the same truth.
    /// </summary>
    public sealed class NpcKnowledge
    {
        public string EventId { get; }
        public string Kind { get; }
        public string Description { get; }
        public string OriginalActorNpcId { get; }
        public string LastSourceNpcId { get; }
        public NpcKnowledgeOrigin Origin { get; }
        public float Confidence { get; }
        public int LearnedDay { get; }
        public int LearnedMinuteOfDay { get; }

        internal NpcKnowledge(string eventId, string kind, string description, string originalActorNpcId,
            string lastSourceNpcId, NpcKnowledgeOrigin origin, float confidence, int learnedDay, int learnedMinuteOfDay)
        {
            EventId = eventId;
            Kind = kind;
            Description = description;
            OriginalActorNpcId = originalActorNpcId;
            LastSourceNpcId = lastSourceNpcId;
            Origin = origin;
            Confidence = Clamp01(confidence);
            LearnedDay = learnedDay;
            LearnedMinuteOfDay = learnedMinuteOfDay;
        }

        internal NpcKnowledge Retell(string sourceNpcId, float confidence, int day, int minuteOfDay)
        {
            return new NpcKnowledge(EventId, Kind, Description, OriginalActorNpcId, sourceNpcId,
                NpcKnowledgeOrigin.ToldByAnotherNpc, confidence, day, minuteOfDay);
        }

        internal static NpcKnowledge Observe(NpcWorldEvent worldEvent)
        {
            return new NpcKnowledge(worldEvent.EventId, worldEvent.Kind, worldEvent.Description,
                worldEvent.ActorNpcId, worldEvent.ActorNpcId, NpcKnowledgeOrigin.DirectObservation,
                1f, worldEvent.Day, worldEvent.MinuteOfDay);
        }

        private static float Clamp01(float value)
        {
            return value < 0f ? 0f : value > 1f ? 1f : value;
        }
    }
}
