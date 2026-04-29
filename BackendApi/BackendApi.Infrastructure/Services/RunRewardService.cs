using BackendApi.Application.DTOs.Run;
using BackendApi.Application.Interfaces;
using BackendApi.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BackendApi.Infrastructure.Services
{
    public sealed class RunRewardService : IRunRewardService
    {
        private readonly GameDbContext _dbContext;

        public RunRewardService(GameDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<BankRunRewardsResponseDto> BankRewardsAsync(Guid playerId, BankRunRewardsRequestDto request)
        {
            if (request.GoldEarned < 0)
                throw new InvalidOperationException("GoldEarned cannot be negative.");

            if (request.ExperienceEarned < 0)
                throw new InvalidOperationException("ExperienceEarned cannot be negative.");

            var player = await _dbContext.Players
                .FirstOrDefaultAsync(x => x.PlayerId == playerId);

            if (player is null)
                throw new InvalidOperationException("Player not found.");

            player.DaluMoney += request.GoldEarned;
            player.Experience += request.ExperienceEarned;

            while (player.Experience >= GetRequiredExperienceForNextLevel(player.Level))
            {
                var requiredXp = GetRequiredExperienceForNextLevel(player.Level);
                player.Experience -= requiredXp;
                player.Level++;

                ApplyLevelUp(player);
            }

            await _dbContext.SaveChangesAsync();

            return new BankRunRewardsResponseDto
            {
                GoldEarned = request.GoldEarned,
                ExperienceEarned = request.ExperienceEarned,
                TotalGold = player.DaluMoney,
                NewLevel = player.Level,
                TotalExperience = player.Experience,
                ExperienceRequiredForNextLevel = GetRequiredExperienceForNextLevel(player.Level),
                DamageBonus = player.DamageBonus,
                BaseMaxHealth = player.BaseMaxHealth,
                BaseMaxMana = player.BaseMaxMana
            };
        }

        private static void ApplyLevelUp(BackendApi.Domain.Entities.Player player)
        {
            player.BaseMaxHealth += 4;

            if (player.Level % 2 == 0)
            {
                player.DamageBonus += 1;
            }

            if (player.Level % 4 == 0)
            {
                player.BaseMaxMana += 1;
            }
        }

        private static int GetRequiredExperienceForNextLevel(int currentLevel)
        {
            return 50 + ((currentLevel - 1) * 25);
        }
    }
}