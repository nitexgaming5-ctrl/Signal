using Signal.Core;
using UnityEngine;

namespace Signal.Game
{
    /// <summary>
    /// Authored, reusable content for one resident. Create assets with
    /// Assets/Create/Signal/NPC Character Definition; runtime behaviour lives in NpcPresenter.
    /// </summary>
    [CreateAssetMenu(menuName = "Signal/NPC Character Definition", fileName = "NPC_Character")]
    public sealed class NpcCharacterDefinition : ScriptableObject
    {
        [Header("Identity")]
        public string NpcId = "resident";
        public string DisplayName = "Resident";
        public string PublicRole = "Neighbour";
        [Min(0.1f)] public float AwarenessRadius = 8f;
        [Min(0.1f), Tooltip("Movement speed in world units per real-time second.")]
        public float WalkingSpeed = 1.6f;

        [Header("Routine")]
        public NpcUtilityProfile UtilityProfile = NpcUtilityProfile.CreateDefault();
        public Vector2 WorkPosition = new Vector2(1000f, 1000.4f);
        public Vector2 RestPosition = new Vector2(3.5f, -17.5f);
        public Vector2 SocialPosition = new Vector2(18.2f, -11.6f);
        public Vector2 HomePosition = new Vector2(1016.5f, 1000f);

        [Header("Appearance")]
        public Color CoatColor = new Color32(65, 78, 83, 255);
        public Color ClothingColor = new Color32(190, 178, 151, 255);
        public Color SkinColor = new Color32(190, 145, 111, 255);
        public Color HairColor = new Color32(57, 49, 43, 255);
        public Color DetailColor = new Color32(218, 213, 195, 255);

        [Header("Dialogue")]
        [TextArea] public string WorkDialogue = "I'm working right now.";
        [TextArea] public string RestDialogue = "Taking a short break.";
        [TextArea] public string SocialDialogue = "Good to see someone from the neighbourhood.";
        [TextArea] public string HomeDialogue = "I'm done for today.";

        public string DialogueFor(NpcActivity activity)
        {
            switch (activity)
            {
                case NpcActivity.Work: return WorkDialogue;
                case NpcActivity.Rest: return RestDialogue;
                case NpcActivity.Socialize: return SocialDialogue;
                default: return HomeDialogue;
            }
        }

        public Vector2 PositionFor(NpcActivity activity)
        {
            switch (activity)
            {
                case NpcActivity.Work: return WorkPosition;
                case NpcActivity.Rest: return RestPosition;
                case NpcActivity.Socialize: return SocialPosition;
                default: return HomePosition;
            }
        }
    }
}
