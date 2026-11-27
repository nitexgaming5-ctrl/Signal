using System.Collections.Generic;
using NUnit.Framework;

namespace Signal.Core.Tests
{
    public sealed class PlayerDevelopmentTests
    {
        private static readonly string[] Skills = { "repair", "production", "trading" };

        [Test]
        public void AttributeSetContainsHealthArmorAndHeatProtectionAndRejectsNegativeValues()
        {
            var attributes = new PlayerAttributeSet(120, 2, 3);
            Assert.That(attributes.Health, Is.EqualTo(120));
            Assert.That(attributes.Armor, Is.EqualTo(2));
            Assert.That(attributes.HeatProtection, Is.EqualTo(3));
            Assert.That(attributes.TrySetBaseValues(-1, 0, 0), Is.False);
            Assert.That(attributes.TrySetBaseValues(0, 0, 0), Is.False);
            Assert.That(attributes.Health, Is.EqualTo(120));
            Assert.That(attributes.Armor, Is.EqualTo(2));
        }

        [Test]
        public void EffectiveAttributesCombineBaseAndEquipmentModifiersWithoutMutatingBase()
        {
            var attributes = new PlayerAttributeSet(2, 4);
            EffectiveCharacterAttributes effective = CharacterAttributeCalculator.Calculate(attributes, 3, -1);
            Assert.That(effective.Health, Is.EqualTo(100));
            Assert.That(effective.Armor, Is.EqualTo(5));
            Assert.That(effective.HeatProtection, Is.EqualTo(3));
            EffectiveCharacterAttributes clamped = CharacterAttributeCalculator.Calculate(
                new PlayerAttributeSet(int.MaxValue, 0), 1, -1);
            Assert.That(clamped.Health, Is.EqualTo(100));
            Assert.That(clamped.Armor, Is.EqualTo(int.MaxValue));
            Assert.That(attributes.Armor, Is.EqualTo(2));
            Assert.That(attributes.HeatProtection, Is.EqualTo(4));
        }

        [Test]
        public void TemporarySkillModifiersIncreaseEffectiveHealthAndHeatProtectionOnly()
        {
            var attributes = new PlayerAttributeSet(100, 2, 4);
            EffectiveCharacterAttributes effective = CharacterAttributeCalculator.Calculate(
                attributes, 3, -1, 20, 2);

            Assert.That(effective.Health, Is.EqualTo(120));
            Assert.That(effective.Armor, Is.EqualTo(5));
            Assert.That(effective.HeatProtection, Is.EqualTo(5));
            Assert.That(attributes.Health, Is.EqualTo(100));
            Assert.That(attributes.HeatProtection, Is.EqualTo(4));
        }

        [Test]
        public void PlayerStatsSupportsAttributeDrivenMaximumHealthWithoutChangingDamageRules()
        {
            var stats = new PlayerStats(0, 100);
            Assert.That(stats.MaximumHealth, Is.EqualTo(100));
            Assert.That(stats.TrySetMaximumHealth(120), Is.True);
            Assert.That(stats.MaximumHealth, Is.EqualTo(120));
            Assert.That(stats.Health, Is.EqualTo(100));
            stats.Restore(0, 120);
            stats.Damage(15);
            Assert.That(stats.Health, Is.EqualTo(105));
            Assert.That(stats.TrySetMaximumHealth(90), Is.True);
            Assert.That(stats.MaximumHealth, Is.EqualTo(90));
            Assert.That(stats.Health, Is.EqualTo(90));
        }

        [Test]
        public void StartingBudgetCanBeDistributedAmongKnownSkills()
        {
            var state = new PlayerDevelopmentState(Skills, 4, 5, 1);
            Assert.That(state.Progression.UnspentPoints, Is.EqualTo(4));
            Assert.That(state.Progression.TrySpendRank(state.Skills, "repair"), Is.True);
            Assert.That(state.Progression.TrySpendRank(state.Skills, "production"), Is.True);
            Assert.That(state.Skills.GetRank("repair"), Is.EqualTo(1));
            Assert.That(state.Skills.GetRank("production"), Is.EqualTo(1));
            Assert.That(state.Progression.UnspentPoints, Is.EqualTo(2));
        }

        [Test]
        public void RejectsUnknownSkillAndOverspending()
        {
            var state = new PlayerDevelopmentState(Skills, 1, 5, 1);
            Assert.That(state.Progression.TrySpendRank(state.Skills, "unknown"), Is.False);
            Assert.That(state.Progression.TrySpendRank(state.Skills, "repair"), Is.True);
            Assert.That(state.Progression.TrySpendRank(state.Skills, "repair"), Is.False);
            Assert.That(state.Progression.UnspentPoints, Is.Zero);
        }

        [Test]
        public void SkillRanksRespectConfiguredMaximum()
        {
            var state = new PlayerDevelopmentState(Skills, 8, 2, 1);
            Assert.That(state.Progression.TrySpendRank(state.Skills, "repair"), Is.True);
            Assert.That(state.Progression.TrySpendRank(state.Skills, "repair"), Is.True);
            Assert.That(state.Progression.TrySpendRank(state.Skills, "repair"), Is.False);
            Assert.That(state.Skills.GetRank("repair"), Is.EqualTo(2));
        }

        [Test]
        public void RespecializationIsFreeAndPreservesTotalEarnedPoints()
        {
            var state = new PlayerDevelopmentState(Skills, 4, 5, 1);
            state.Progression.TrySpendRank(state.Skills, "repair");
            state.Progression.TrySpendRank(state.Skills, "production");
            state.Progression.TryAwardMilestone(state.Milestones, "first-repair", 2, "repair-order-1");

            state.Progression.Respecialize(state.Skills);

            Assert.That(state.Progression.TotalPoints, Is.EqualTo(6));
            Assert.That(state.Progression.UnspentPoints, Is.EqualTo(6));
            Assert.That(state.Skills.GetRank("repair"), Is.Zero);
            Assert.That(state.Skills.GetRank("production"), Is.Zero);
            Assert.That(state.Milestones.Contains("first-repair"), Is.True);
        }

        [Test]
        public void MilestoneAwardIsOneTimeAndRejectsInvalidRewards()
        {
            var state = new PlayerDevelopmentState(Skills, 2, 5, 1);
            Assert.That(state.Progression.TryAwardMilestone(state.Milestones, "first-contract", 3, "contract-1"), Is.True);
            Assert.That(state.Progression.TryAwardMilestone(state.Milestones, "first-contract", 3, "contract-1"), Is.False);
            Assert.That(state.Progression.TryAwardMilestone(state.Milestones, "zero", 0, "event"), Is.False);
            Assert.That(state.Progression.TryAwardMilestone(state.Milestones, "missing-source", 2, string.Empty), Is.False);
            Assert.That(state.Progression.TotalPoints, Is.EqualTo(5));
            Assert.That(state.Milestones.TotalAwardedPoints, Is.EqualTo(3));
        }

        [Test]
        public void SaveRoundTripPreservesAttributesSkillsAndMilestoneHistory()
        {
            var state = new PlayerDevelopmentState(Skills, 4, 5, 1);
            state.Attributes.TrySetBaseValues(125, 3, 2);
            state.Progression.TrySpendRank(state.Skills, "repair");
            state.Progression.TrySpendRank(state.Skills, "trading");
            state.Progression.TryAwardMilestone(state.Milestones, "first-restoration", 2, "repair-order-42");
            var save = new SaveData();

            PlayerDevelopmentSaveMapper.Capture(state, save);
            PlayerDevelopmentState restored = PlayerDevelopmentSaveMapper.Restore(save, Skills, 4, 5, 1);

            Assert.That(restored.Attributes.Health, Is.EqualTo(125));
            Assert.That(restored.Attributes.Armor, Is.EqualTo(3));
            Assert.That(restored.Attributes.HeatProtection, Is.EqualTo(2));
            Assert.That(restored.Skills.GetRank("repair"), Is.EqualTo(1));
            Assert.That(restored.Skills.GetRank("trading"), Is.EqualTo(1));
            Assert.That(restored.Progression.TotalPoints, Is.EqualTo(6));
            Assert.That(restored.Progression.UnspentPoints, Is.EqualTo(4));
            Assert.That(restored.Milestones.Contains("first-restoration"), Is.True);
        }

        [Test]
        public void RestoreSkipsUnknownSkillsAndInvalidMilestoneAwards()
        {
            var save = new SaveData
            {
                playerDevelopmentInitialized = true,
                playerStartingSkillPoints = 2,
                playerSkillRanks = new List<SavedPlayerSkillRank>
                {
                    new SavedPlayerSkillRank { skillId = "missing-skill", rank = 1 },
                    new SavedPlayerSkillRank { skillId = "repair", rank = 1 }
                },
                playerMilestoneAwards = new List<SavedPlayerMilestoneAward>
                {
                    new SavedPlayerMilestoneAward { milestoneId = "bad", points = -10, sourceId = "bad-source" }
                }
            };

            PlayerDevelopmentState restored = PlayerDevelopmentSaveMapper.Restore(save, Skills, 4, 5, 1);
            Assert.That(restored.Skills.GetRank("repair"), Is.EqualTo(1));
            Assert.That(restored.Progression.TotalPoints, Is.EqualTo(2));
            Assert.That(restored.Progression.UnspentPoints, Is.EqualTo(1));
            Assert.That(restored.Milestones.TotalAwardedPoints, Is.Zero);
        }

        [Test]
        public void OlderSaveWithoutPlayerDevelopmentUsesConfiguredDefaults()
        {
            var restored = PlayerDevelopmentSaveMapper.Restore(new SaveData(), Skills, 7, 4, 1);
            Assert.That(restored.Progression.TotalPoints, Is.EqualTo(7));
            Assert.That(restored.Progression.UnspentPoints, Is.EqualTo(7));
            Assert.That(restored.Skills.GetRank("repair"), Is.Zero);
        }

        [Test]
        public void DuplicateMilestoneIdentityCannotBeRestoredTwice()
        {
            var save = new SaveData
            {
                playerDevelopmentInitialized = true,
                playerStartingSkillPoints = 1,
                playerMilestoneAwards = new List<SavedPlayerMilestoneAward>
                {
                    new SavedPlayerMilestoneAward { milestoneId = "first-job", points = 2, sourceId = "job-1" },
                    new SavedPlayerMilestoneAward { milestoneId = "first-job", points = 2, sourceId = "job-1" }
                }
            };

            var restored = PlayerDevelopmentSaveMapper.Restore(save, Skills, 1, 5, 1);
            Assert.That(restored.Progression.TotalPoints, Is.EqualTo(3));
            Assert.That(restored.Milestones.TotalAwardedPoints, Is.EqualTo(2));
        }
    }
}
