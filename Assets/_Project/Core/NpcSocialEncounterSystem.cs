using System;
using System.Collections.Generic;

namespace Signal.Core
{
    /// <summary>
    /// Resolves restrained, schedule-driven encounters between residents. It does not
    /// choose activities or create autonomous agendas; callers supply only NPCs whose
    /// current authored activity is Socialize.
    /// </summary>
    public sealed class NpcSocialEncounterSystem
    {
        private readonly int _pairCooldownMinutes;
        private readonly float _maximumDistance;
        private readonly float _familiarityGain;
        private readonly Dictionary<string, int> _lastEncounterByPair = new Dictionary<string, int>();
        private int _lastProcessedMinute = -1;
        private int _encounterSequence;

        public int EncounterCount { get; private set; }

        public NpcSocialEncounterSystem(int pairCooldownMinutes = 720,
            float maximumDistance = 3f, float familiarityGain = 0.01f)
        {
            if (pairCooldownMinutes <= 0)
                throw new ArgumentOutOfRangeException(nameof(pairCooldownMinutes));
            if (maximumDistance <= 0f)
                throw new ArgumentOutOfRangeException(nameof(maximumDistance));
            if (familiarityGain < 0f || familiarityGain > 1f)
                throw new ArgumentOutOfRangeException(nameof(familiarityGain));

            _pairCooldownMinutes = pairCooldownMinutes;
            _maximumDistance = maximumDistance;
            _familiarityGain = familiarityGain;
        }

        /// <summary>
        /// Processes one deterministic encounter pass. Every supplied NPC can take part
        /// in at most one encounter in this pass; each pair is then held by a cooldown.
        /// </summary>
        public int AdvanceTo(NpcSocialWorld world, IList<string> socializingNpcIds,
            int day, int minuteOfDay)
        {
            if (world == null) throw new ArgumentNullException(nameof(world));
            if (socializingNpcIds == null) throw new ArgumentNullException(nameof(socializingNpcIds));

            int safeDay = Math.Max(1, day);
            int safeMinute = Math.Max(0, Math.Min(1439, minuteOfDay));
            int absoluteMinute = (safeDay - 1) * 1440 + safeMinute;
            if (absoluteMinute == _lastProcessedMinute)
                return 0;
            if (_lastProcessedMinute >= 0 && absoluteMinute < _lastProcessedMinute)
                _lastEncounterByPair.Clear();
            _lastProcessedMinute = absoluteMinute;

            var candidates = new List<string>();
            for (int i = 0; i < socializingNpcIds.Count; i++)
            {
                string id = socializingNpcIds[i];
                if (!string.IsNullOrWhiteSpace(id) && !candidates.Contains(id))
                    candidates.Add(id);
            }
            candidates.Sort(StringComparer.Ordinal);

            var occupiedThisPass = new HashSet<string>(StringComparer.Ordinal);
            int created = 0;
            for (int i = 0; i < candidates.Count; i++)
            {
                string firstId = candidates[i];
                if (occupiedThisPass.Contains(firstId) ||
                    !world.TryGetNpc(firstId, out NpcAgentState first))
                    continue;

                IList<string> nearby = world.GetNearbyNpcIds(firstId, _maximumDistance);
                for (int j = i + 1; j < candidates.Count; j++)
                {
                    string secondId = candidates[j];
                    if (occupiedThisPass.Contains(secondId) || !nearby.Contains(secondId))
                        continue;

                    // Both residents must be close enough to notice one another.
                    if (!world.GetNearbyNpcIds(secondId, _maximumDistance).Contains(firstId))
                        continue;

                    string pairKey = firstId + "|" + secondId;
                    if (_lastEncounterByPair.TryGetValue(pairKey, out int lastMinute) &&
                        absoluteMinute - lastMinute < _pairCooldownMinutes)
                        continue;

                    if (!world.TryGetNpc(secondId, out NpcAgentState second))
                        continue;

                    ResolveEncounter(world, first, second, safeDay, safeMinute, absoluteMinute);
                    _lastEncounterByPair[pairKey] = absoluteMinute;
                    occupiedThisPass.Add(firstId);
                    occupiedThisPass.Add(secondId);
                    created++;
                    break;
                }
            }

            EncounterCount += created;
            return created;
        }

        private void ResolveEncounter(NpcSocialWorld world, NpcAgentState first,
            NpcAgentState second, int day, int minuteOfDay, int absoluteMinute)
        {
            IncreaseFamiliarity(world, first.Id, second.Id);
            IncreaseFamiliarity(world, second.Id, first.Id);
            ExchangeAtMostOneKnownEvent(world, first.Id, second.Id, day, minuteOfDay);

            float midpointX = (first.X + second.X) * 0.5f;
            float midpointY = (first.Y + second.Y) * 0.5f;
            float dx = first.X - second.X;
            float dy = first.Y - second.Y;
            float visibilityRadius = (float)Math.Sqrt(dx * dx + dy * dy) * 0.5f + 0.25f;

            _encounterSequence++;
            world.PublishEvent(new NpcWorldEvent(
                "social-encounter-" + absoluteMinute + "-" + _encounterSequence,
                "SocialEncounter",
                "Two residents stopped to talk.",
                string.Empty,
                first.LocationId,
                midpointX,
                midpointY,
                visibilityRadius,
                day,
                minuteOfDay));
        }

        private void IncreaseFamiliarity(NpcSocialWorld world, string observerId, string otherId)
        {
            NpcRelationship current = world.GetRelationship(observerId, otherId);
            world.SetRelationship(observerId, otherId, new NpcRelationship(
                current.Familiarity + _familiarityGain,
                current.Trust,
                current.Affinity));
        }

        private static void ExchangeAtMostOneKnownEvent(NpcSocialWorld world,
            string firstId, string secondId, int day, int minuteOfDay)
        {
            NpcPerception firstView = world.BuildPerceptionFor(firstId, float.MaxValue);
            NpcPerception secondView = world.BuildPerceptionFor(secondId, float.MaxValue);
            for (int i = 0; i < firstView.KnownEvents.Count; i++)
            {
                string eventId = firstView.KnownEvents[i].EventId;
                if (!secondView.KnowsEvent(eventId))
                {
                    world.ShareKnowledge(firstId, secondId, eventId, day, minuteOfDay);
                    return;
                }
            }

            for (int i = 0; i < secondView.KnownEvents.Count; i++)
            {
                string eventId = secondView.KnownEvents[i].EventId;
                if (!firstView.KnowsEvent(eventId))
                {
                    world.ShareKnowledge(secondId, firstId, eventId, day, minuteOfDay);
                    return;
                }
            }
        }
    }
}
