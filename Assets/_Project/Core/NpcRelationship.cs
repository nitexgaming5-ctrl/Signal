namespace Signal.Core
{
    /// <summary>
    /// Directed social impression held by one NPC about another.
    /// Values are authored/simulated by later systems, not inferred from a global truth.
    /// </summary>
    public sealed class NpcRelationship
    {
        public float Familiarity { get; }
        public float Trust { get; }
        public float Affinity { get; }

        public NpcRelationship(float familiarity, float trust, float affinity)
        {
            Familiarity = Clamp01(familiarity);
            Trust = Clamp01(trust);
            Affinity = affinity < -1f ? -1f : affinity > 1f ? 1f : affinity;
        }

        public static NpcRelationship Neutral => new NpcRelationship(0f, 0.5f, 0f);

        private static float Clamp01(float value)
        {
            return value < 0f ? 0f : value > 1f ? 1f : value;
        }
    }
}
