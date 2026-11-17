using System;

namespace Signal.Core
{
    /// <summary>
    /// Authored tuning for a general-purpose NPC utility brain. Values describe routine
    /// preferences and need drift, not a character's identity or narrative role.
    /// </summary>
    [Serializable]
    public sealed class NpcUtilityProfile
    {
        public int DecisionIntervalMinutes = 15;

        public int WorkStartMinute = 390;
        public int WorkEndMinute = 1080;
        public int RestStartMinute = 690;
        public int RestEndMinute = 810;
        public int SocializeStartMinute = 1080;
        public int SocializeEndMinute = 1260;
        public int HomeStartMinute = 1260;
        public int HomeEndMinute = 390;

        public float WorkBaseScore = 20f;
        public float WorkEnergyWeight = 40f;
        public float WorkOffHoursScore = -10f;
        public float RestEnergyNeedWeight = 70f;
        public float RestWindowBonus = 25f;
        public float SocialNeedWeight = 55f;
        public float SocialWindowBonus = 40f;
        public float HomePriority = 100f;
        public float HomeOffHoursScore = -10f;
        public float ActivityInertiaBonus = 5f;

        public float FamiliaritySocialWeight = 8f;
        public float AffinitySocialWeight = 12f;
        public float KnownInformationSocialWeight = 2.5f;
        public int MaximumKnownTopicsForSocialising = 3;

        public float EnergyWorkDrain = 0.0012f;
        public float SocialWorkDrain = 0.00045f;
        public float EnergyRestRecovery = 0.003f;
        public float SocialRestRecovery = 0.0001f;
        public float EnergySocialDrain = 0.0004f;
        public float SocialSocialRecovery = 0.002f;
        public float EnergyHomeRecovery = 0.0015f;
        public float SocialHomeRecovery = 0.0007f;

        public static NpcUtilityProfile CreateDefault()
        {
            return new NpcUtilityProfile();
        }
    }
}