using System;

namespace Signal.Core
{
    public enum NpcActivity
    {
        Home,
        Work,
        Rest,
        Socialize
    }

    /// <summary>
    /// Deterministic utility brain for ambient NPCs. It evaluates authored routine/need
    /// preferences on a fixed interval, with social opportunities drawn only from the
    /// character's own perception snapshot.
    /// </summary>
    public sealed class NpcUtilityBrain
    {
        private const int MinutesPerDay = 1440;

        private readonly NpcUtilityProfile _profile;
        private float _energy = 1f;
        private float _social = 0.65f;
        private int _lastAbsoluteMinute = -1;

        public NpcActivity CurrentActivity { get; private set; } = NpcActivity.Home;
        public float Energy => _energy;
        public float Social => _social;

        public NpcUtilityBrain(NpcUtilityProfile profile = null)
        {
            _profile = profile ?? NpcUtilityProfile.CreateDefault();
            if (_profile.DecisionIntervalMinutes <= 0)
                throw new ArgumentOutOfRangeException(nameof(profile), "Decision interval must be positive.");
        }

        public NpcActivity AdvanceTo(int day, int minuteOfDay)
        {
            return AdvanceTo(day, minuteOfDay, null);
        }

        /// <summary>
        /// Advances the brain using a character-scoped perception snapshot. Nearby
        /// relationships influence the value of socialising; personally known information
        /// provides a small conversation-topic incentive. No global event journal is consulted.
        /// </summary>
        public NpcActivity AdvanceTo(int day, int minuteOfDay, NpcPerception perception)
        {
            int safeDay = Math.Max(1, day);
            int safeMinute = Math.Max(0, Math.Min(MinutesPerDay - 1, minuteOfDay));
            int absoluteMinute = (safeDay - 1) * MinutesPerDay + safeMinute;

            if (_lastAbsoluteMinute < 0 || absoluteMinute < _lastAbsoluteMinute)
            {
                _energy = 1f;
                _social = 0.65f;
                CurrentActivity = NpcActivity.Home;
                _lastAbsoluteMinute = (safeDay - 1) * MinutesPerDay - 1;
            }

            while (_lastAbsoluteMinute < absoluteMinute)
            {
                _lastAbsoluteMinute++;
                int minute = _lastAbsoluteMinute % MinutesPerDay;

                if (minute % _profile.DecisionIntervalMinutes == 0)
                    CurrentActivity = ChooseActivity(minute, perception);

                ApplyNeedDrift();
            }

            return CurrentActivity;
        }

        private NpcActivity ChooseActivity(int minute, NpcPerception perception)
        {
            float work = IsBetween(minute, _profile.WorkStartMinute, _profile.WorkEndMinute)
                ? _profile.WorkBaseScore + _energy * _profile.WorkEnergyWeight
                : _profile.WorkOffHoursScore;
            float rest = (1f - _energy) * _profile.RestEnergyNeedWeight +
                (IsBetween(minute, _profile.RestStartMinute, _profile.RestEndMinute) ? _profile.RestWindowBonus : 0f);
            float socialize = (1f - _social) * _profile.SocialNeedWeight +
                (IsBetween(minute, _profile.SocializeStartMinute, _profile.SocializeEndMinute) ? _profile.SocialWindowBonus : 0f);
            float home = IsBetween(minute, _profile.HomeStartMinute, _profile.HomeEndMinute)
                ? _profile.HomePriority
                : _profile.HomeOffHoursScore;

            socialize += GetSocialContextScore(perception, _profile);

            switch (CurrentActivity)
            {
                case NpcActivity.Work: work += _profile.ActivityInertiaBonus; break;
                case NpcActivity.Rest: rest += _profile.ActivityInertiaBonus; break;
                case NpcActivity.Socialize: socialize += _profile.ActivityInertiaBonus; break;
                case NpcActivity.Home: home += _profile.ActivityInertiaBonus; break;
            }

            if (work >= rest && work >= socialize && work >= home) return NpcActivity.Work;
            if (rest >= socialize && rest >= home) return NpcActivity.Rest;
            if (socialize >= home) return NpcActivity.Socialize;
            return NpcActivity.Home;
        }

        private static float GetSocialContextScore(NpcPerception perception, NpcUtilityProfile profile)
        {
            if (perception == null || perception.NearbyPeople.Count == 0)
                return 0f;

            float score = 0f;
            for (int i = 0; i < perception.NearbyPeople.Count; i++)
            {
                NpcRelationship relationship = perception.NearbyPeople[i].Relationship;
                score += relationship.Familiarity * profile.FamiliaritySocialWeight;
                score += relationship.Affinity * profile.AffinitySocialWeight;
            }

            int topicCount = Math.Min(profile.MaximumKnownTopicsForSocialising, perception.KnownEvents.Count);
            for (int i = 0; i < topicCount; i++)
                score += perception.KnownEvents[i].Confidence * profile.KnownInformationSocialWeight;

            return score;
        }

        private void ApplyNeedDrift()
        {
            switch (CurrentActivity)
            {
                case NpcActivity.Work:
                    _energy -= _profile.EnergyWorkDrain;
                    _social -= _profile.SocialWorkDrain;
                    break;
                case NpcActivity.Rest:
                    _energy += _profile.EnergyRestRecovery;
                    _social += _profile.SocialRestRecovery;
                    break;
                case NpcActivity.Socialize:
                    _energy -= _profile.EnergySocialDrain;
                    _social += _profile.SocialSocialRecovery;
                    break;
                default:
                    _energy += _profile.EnergyHomeRecovery;
                    _social += _profile.SocialHomeRecovery;
                    break;
            }

            _energy = Clamp01(_energy);
            _social = Clamp01(_social);
        }

        private static bool IsBetween(int value, int startInclusive, int endExclusive)
        {
            return startInclusive <= endExclusive
                ? value >= startInclusive && value < endExclusive
                : value >= startInclusive || value < endExclusive;
        }

        private static float Clamp01(float value)
        {
            return Math.Max(0f, Math.Min(1f, value));
        }
    }
}