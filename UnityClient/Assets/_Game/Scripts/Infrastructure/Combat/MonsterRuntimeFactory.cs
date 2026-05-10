using System;
using Game.Domain.Entities;
using Game.Infrastructure.Api.Dtos.Combat;

namespace Game.Infrastructure.Combat
{
    public sealed class MonsterRuntimeFactory
    {
        private const int HealthPerDifficultyTier = 6;
        private const int DamagePerDifficultyTier = 2;

        private const int GoldRewardPerDifficultyTier = 0;
        private const int ExperienceRewardPerDifficultyTier = 0;

        public MonsterRuntimeState Create(MonsterDto monsterDto, int difficultyTier)
        {
            var safeTier = Math.Max(0, difficultyTier);

            var scaledMaxHealth = monsterDto.maxHealth + (safeTier * HealthPerDifficultyTier);
            var scaledDamage = monsterDto.damage + (safeTier * DamagePerDifficultyTier);
            var scaledGoldReward = monsterDto.goldReward + (safeTier * GoldRewardPerDifficultyTier);
            var scaledExperienceReward = monsterDto.experienceReward + (safeTier * ExperienceRewardPerDifficultyTier);

            return new MonsterRuntimeState(
                monsterDto.monsterKey,
                monsterDto.name,
                scaledMaxHealth,
                scaledDamage,
                monsterDto.mana,
                scaledGoldReward,
                scaledExperienceReward);
        }

        public string BuildDifficultyDescription(int difficultyTier)
        {
            var safeTier = Math.Max(0, difficultyTier);

            return
                $"World Tier {safeTier + 1}\n" +
                $"Enemy HP +{safeTier * HealthPerDifficultyTier}\n" +
                $"Enemy Damage +{safeTier * DamagePerDifficultyTier}";
        }

        public string BuildDifficultyRow(int difficultyTier, int currentDifficultyTier, int highestDifficultyTierReached)
        {
            var safeTier = Math.Max(0, difficultyTier);
            var safeCurrentTier = Math.Max(0, currentDifficultyTier);
            var safeHighestTier = Math.Max(0, highestDifficultyTierReached);

            var status = safeTier == safeCurrentTier
                ? "Current"
                : safeTier <= safeHighestTier
                    ? "Unlocked"
                    : "Locked";

            return
                $"World Tier {safeTier + 1}  -  {status}\n" +
                $"+{safeTier * HealthPerDifficultyTier} HP   |   +{safeTier * DamagePerDifficultyTier} DMG";
        }
    }
}