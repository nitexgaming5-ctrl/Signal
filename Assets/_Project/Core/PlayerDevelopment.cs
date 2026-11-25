using System;
using System.Collections.Generic;

namespace Signal.Core
{
    /// <summary>Base character attributes. Equipment modifiers are applied by the pure calculator.</summary>
    public sealed class PlayerAttributeSet
    {
        public int Armor { get; private set; }
        public int HeatProtection { get; private set; }

        public PlayerAttributeSet(int armor = 0, int heatProtection = 0)
        {
            if (!TrySetBaseValues(armor, heatProtection))
                throw new ArgumentOutOfRangeException(nameof(armor), "Player attributes cannot be negative.");
        }

        public bool TrySetBaseValues(int armor, int heatProtection)
        {
            if (armor < 0 || heatProtection < 0)
                return false;
            Armor = armor;
            HeatProtection = heatProtection;
            return true;
        }
    }

    public struct EffectiveCharacterAttributes
    {
        public int Armor { get; }
        public int HeatProtection { get; }

        public EffectiveCharacterAttributes(int armor, int heatProtection)
        {
            Armor = Math.Max(0, armor);
            HeatProtection = Math.Max(0, heatProtection);
        }
    }

    /// <summary>Pure calculation seam. Equipment/loadout ownership is implemented by P-03.</summary>
    public static class CharacterAttributeCalculator
    {
        public static EffectiveCharacterAttributes Calculate(PlayerAttributeSet baseAttributes,
            int equipmentArmorModifier, int equipmentHeatProtectionModifier)
        {
            if (baseAttributes == null)
                throw new ArgumentNullException(nameof(baseAttributes));
            return new EffectiveCharacterAttributes(
                AddClamped(baseAttributes.Armor, equipmentArmorModifier),
                AddClamped(baseAttributes.HeatProtection, equipmentHeatProtectionModifier));
        }

        private static int AddClamped(int baseValue, int modifier)
        {
            long result = (long)baseValue + modifier;
            if (result <= 0) return 0;
            return result >= int.MaxValue ? int.MaxValue : (int)result;
        }
    }

    /// <summary>Stable skill IDs and ranks. Definitions/display metadata live in Game content.</summary>
    public sealed class PlayerSkillBook
    {
        private readonly Dictionary<string, int> _ranks =
            new Dictionary<string, int>(StringComparer.Ordinal);

        public IEnumerable<KeyValuePair<string, int>> Ranks => _ranks;

        public PlayerSkillBook(IEnumerable<string> skillIds)
        {
            if (skillIds == null)
                throw new ArgumentNullException(nameof(skillIds));
            foreach (string id in skillIds)
            {
                if (!string.IsNullOrWhiteSpace(id) && !_ranks.ContainsKey(id))
                    _ranks.Add(id, 0);
            }
        }

        public int GetRank(string skillId)
        {
            return !string.IsNullOrWhiteSpace(skillId) && _ranks.TryGetValue(skillId, out int rank) ? rank : 0;
        }

        public bool Contains(string skillId)
        {
            return !string.IsNullOrWhiteSpace(skillId) && _ranks.ContainsKey(skillId);
        }

        internal bool TrySetRank(string skillId, int rank)
        {
            if (string.IsNullOrWhiteSpace(skillId) || rank < 0 || !_ranks.ContainsKey(skillId))
                return false;
            _ranks[skillId] = rank;
            return true;
        }

        public int AllocatedRankPoints
        {
            get
            {
                int total = 0;
                foreach (int rank in _ranks.Values)
                    total += rank;
                return total;
            }
        }

        internal void Reset()
        {
            var keys = new List<string>(_ranks.Keys);
            foreach (string key in keys)
                _ranks[key] = 0;
        }
    }

    public sealed class PlayerMilestoneAward
    {
        public string MilestoneId { get; }
        public int Points { get; }
        public string SourceId { get; }

        public PlayerMilestoneAward(string milestoneId, int points, string sourceId)
        {
            MilestoneId = milestoneId;
            Points = points;
            SourceId = sourceId ?? string.Empty;
        }
    }

    /// <summary>Idempotency ledger for important, authored progression milestones.</summary>
    public sealed class PlayerMilestoneLedger
    {
        private readonly Dictionary<string, PlayerMilestoneAward> _awards =
            new Dictionary<string, PlayerMilestoneAward>(StringComparer.Ordinal);

        public IEnumerable<PlayerMilestoneAward> Awards => _awards.Values;
        public int TotalAwardedPoints
        {
            get
            {
                int total = 0;
                foreach (PlayerMilestoneAward award in _awards.Values)
                    total += award.Points;
                return total;
            }
        }

        public bool Contains(string milestoneId)
        {
            return !string.IsNullOrWhiteSpace(milestoneId) && _awards.ContainsKey(milestoneId);
        }

        internal bool TryAward(string milestoneId, int points, string sourceId)
        {
            if (string.IsNullOrWhiteSpace(milestoneId) || points <= 0 ||
                string.IsNullOrWhiteSpace(sourceId) || _awards.ContainsKey(milestoneId))
                return false;
            _awards.Add(milestoneId, new PlayerMilestoneAward(milestoneId, points, sourceId));
            return true;
        }
    }

    /// <summary>
    /// Owns point accounting. Prototype tuning is supplied by the caller; no action-based XP exists.
    /// Each rank currently costs a configurable fixed number of points.
    /// </summary>
    public sealed class PlayerSkillProgression
    {
        public int StartingPoints { get; }
        public int MaxRank { get; }
        public int PointsPerRank { get; }
        public int EarnedMilestonePoints { get; private set; }
        public int TotalPoints => StartingPoints + EarnedMilestonePoints;
        public int AllocatedPoints { get; private set; }
        public int UnspentPoints => Math.Max(0, TotalPoints - AllocatedPoints);

        public PlayerSkillProgression(int startingPoints, int maxRank, int pointsPerRank)
        {
            if (startingPoints < 0) throw new ArgumentOutOfRangeException(nameof(startingPoints));
            if (maxRank <= 0) throw new ArgumentOutOfRangeException(nameof(maxRank));
            if (pointsPerRank <= 0) throw new ArgumentOutOfRangeException(nameof(pointsPerRank));
            StartingPoints = startingPoints;
            MaxRank = maxRank;
            PointsPerRank = pointsPerRank;
        }

        public bool TrySpendRank(PlayerSkillBook skills, string skillId)
        {
            if (skills == null || !skills.Contains(skillId))
                return false;
            int current = skills.GetRank(skillId);
            if (current >= MaxRank || UnspentPoints < PointsPerRank)
                return false;
            if (!skills.TrySetRank(skillId, current + 1))
                return false;
            AllocatedPoints += PointsPerRank;
            return true;
        }

        public bool TryAwardMilestone(PlayerMilestoneLedger ledger, string milestoneId, int points, string sourceId)
        {
            if (ledger == null || points <= 0 || string.IsNullOrWhiteSpace(milestoneId) ||
                string.IsNullOrWhiteSpace(sourceId))
                return false;
            if (!ledger.TryAward(milestoneId, points, sourceId))
                return false;
            EarnedMilestonePoints += points;
            return true;
        }

        public void Respecialize(PlayerSkillBook skills)
        {
            if (skills == null)
                return;
            skills.Reset();
            AllocatedPoints = 0;
        }
    }

    /// <summary>Aggregate runtime state for the existing player character.</summary>
    public sealed class PlayerDevelopmentState
    {
        public PlayerAttributeSet Attributes { get; }
        public PlayerSkillBook Skills { get; }
        public PlayerMilestoneLedger Milestones { get; }
        public PlayerSkillProgression Progression { get; }

        public PlayerDevelopmentState(IEnumerable<string> skillIds, int startingPoints, int maxRank, int pointsPerRank)
        {
            Attributes = new PlayerAttributeSet();
            Skills = new PlayerSkillBook(skillIds);
            Milestones = new PlayerMilestoneLedger();
            Progression = new PlayerSkillProgression(startingPoints, maxRank, pointsPerRank);
        }
    }

    /// <summary>Save boundary for player development; stores IDs and primitive values only.</summary>
    public static class PlayerDevelopmentSaveMapper
    {
        public static void Capture(PlayerDevelopmentState state, SaveData destination)
        {
            if (state == null) throw new ArgumentNullException(nameof(state));
            if (destination == null) throw new ArgumentNullException(nameof(destination));

            destination.playerDevelopmentInitialized = true;
            destination.playerArmor = state.Attributes.Armor;
            destination.playerHeatProtection = state.Attributes.HeatProtection;
            destination.playerStartingSkillPoints = state.Progression.StartingPoints;
            if (destination.playerSkillRanks == null)
                destination.playerSkillRanks = new List<SavedPlayerSkillRank>();
            else
                destination.playerSkillRanks.Clear();
            if (destination.playerMilestoneAwards == null)
                destination.playerMilestoneAwards = new List<SavedPlayerMilestoneAward>();
            else
                destination.playerMilestoneAwards.Clear();

            foreach (KeyValuePair<string, int> rank in state.Skills.Ranks)
                destination.playerSkillRanks.Add(new SavedPlayerSkillRank { skillId = rank.Key, rank = rank.Value });

            foreach (PlayerMilestoneAward award in state.Milestones.Awards)
                destination.playerMilestoneAwards.Add(new SavedPlayerMilestoneAward
                {
                    milestoneId = award.MilestoneId,
                    points = award.Points,
                    sourceId = award.SourceId
                });
        }

        public static PlayerDevelopmentState Restore(SaveData source, IEnumerable<string> skillIds,
            int defaultStartingPoints, int maxRank, int pointsPerRank)
        {
            int startingPoints = source != null && source.playerDevelopmentInitialized
                ? Math.Max(0, source.playerStartingSkillPoints) : defaultStartingPoints;
            var state = new PlayerDevelopmentState(skillIds, startingPoints, maxRank, pointsPerRank);
            if (source == null || !source.playerDevelopmentInitialized)
                return state;

            state.Attributes.TrySetBaseValues(Math.Max(0, source.playerArmor), Math.Max(0, source.playerHeatProtection));

            if (source.playerMilestoneAwards != null)
            {
                foreach (SavedPlayerMilestoneAward saved in source.playerMilestoneAwards)
                {
                    if (saved != null)
                        state.Progression.TryAwardMilestone(state.Milestones, saved.milestoneId,
                            saved.points, saved.sourceId);
                }
            }

            if (source.playerSkillRanks != null)
            {
                var desiredRanks = new SortedDictionary<string, int>(StringComparer.Ordinal);
                foreach (SavedPlayerSkillRank saved in source.playerSkillRanks)
                {
                    if (saved == null || !state.Skills.Contains(saved.skillId) || saved.rank < 0)
                        continue;
                    int targetRank = Math.Min(saved.rank, state.Progression.MaxRank);
                    if (!desiredRanks.TryGetValue(saved.skillId, out int existing) || targetRank > existing)
                        desiredRanks[saved.skillId] = targetRank;
                }

                foreach (KeyValuePair<string, int> desired in desiredRanks)
                {
                    while (state.Skills.GetRank(desired.Key) < desired.Value)
                    {
                        if (!state.Progression.TrySpendRank(state.Skills, desired.Key))
                            break;
                    }
                }
            }

            return state;
        }
    }
}
