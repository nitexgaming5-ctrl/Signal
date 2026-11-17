namespace Signal.Core
{
    /// <summary>A nearby character as perceived by one NPC, including that observer's social impression.</summary>
    public sealed class NpcPerceivedPerson
    {
        public string NpcId { get; }
        public NpcRelationship Relationship { get; }

        internal NpcPerceivedPerson(string npcId, NpcRelationship relationship)
        {
            NpcId = npcId;
            Relationship = relationship;
        }
    }
}
