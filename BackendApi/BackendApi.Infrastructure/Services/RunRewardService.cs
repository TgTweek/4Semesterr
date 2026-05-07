using BackendApi.Application.DTOs.Run;
using BackendApi.Application.Interfaces;
using BackendApi.Domain.Services;
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

            if (player.Level < PlayerProgressionRules.MaxLevel)
            {
                player.Experience += request.ExperienceEarned;

                while (player.Level < PlayerProgressionRules.MaxLevel &&
                       player.Experience >= PlayerProgressionRules.GetRequiredExperienceForNextLevel(player.Level))
                {
                    var requiredXp = PlayerProgressionRules.GetRequiredExperienceForNextLevel(player.Level);
                    player.Experience -= requiredXp;
                    player.Level++;
                }

                if (player.Level >= PlayerProgressionRules.MaxLevel)
                {
                    player.Level = PlayerProgressionRules.MaxLevel;
                    player.Experience = 0;
                }
            }

            await _dbContext.SaveChangesAsync();

            return new BankRunRewardsResponseDto
            {
                GoldEarned = request.GoldEarned,
                ExperienceEarned = request.ExperienceEarned,
                TotalGold = player.DaluMoney,
                NewLevel = player.Level,
                MaxLevel = PlayerProgressionRules.MaxLevel,
                TotalExperience = player.Experience,
                ExperienceRequiredForNextLevel = PlayerProgressionRules.GetRequiredExperienceForNextLevel(player.Level),
                DamageBonus = PlayerProgressionRules.GetDamageBonus(player.Level),
                BaseMaxHealth = PlayerProgressionRules.GetBaseMaxHealth(player.Level),
                BaseMaxMana = PlayerProgressionRules.GetBaseMaxMana(player.Level),
                MovementTilesPerTurn = PlayerProgressionRules.GetMovementTilesPerTurn(player.Level)
            };
        }
    }
}