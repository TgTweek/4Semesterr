using BackendApi.Application.DTOs.Player;
using BackendApi.Application.Interfaces;
using BackendApi.Domain.Entities;
using BackendApi.Domain.Services;
using BackendApi.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BackendApi.Infrastructure.Services
{
    public sealed class PlayerService : IPlayerService
    {
        private readonly GameDbContext _dbContext;

        public PlayerService(GameDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<PlayerDto?> GetPlayerAsync(Guid playerId)
        {
            var player = await _dbContext.Players
                .FirstOrDefaultAsync(x => x.PlayerId == playerId);

            if (player is null)
            {
                return null;
            }

            return MapToDto(player);
        }

        public async Task<PlayerDto> SetDifficultyTierAsync(Guid appUserId, int difficultyTier)
        {
            if (difficultyTier < 0)
            {
                throw new InvalidOperationException("Difficulty tier cannot be negative.");
            }

            var player = await _dbContext.Players
                .FirstOrDefaultAsync(x => x.AppUserId == appUserId);

            if (player is null)
            {
                throw new KeyNotFoundException("Player not found.");
            }

            if (difficultyTier > player.HighestDifficultyTierReached)
            {
                throw new InvalidOperationException("You cannot select a locked difficulty tier.");
            }

            player.DifficultyTier = difficultyTier;

            await _dbContext.SaveChangesAsync();

            return MapToDto(player);
        }

        private static PlayerDto MapToDto(Player player)
        {
            return new PlayerDto
            {
                PlayerId = player.PlayerId,
                PlayerName = player.PlayerName,

                Level = player.Level,
                MaxLevel = PlayerProgressionRules.MaxLevel,
                Experience = player.Experience,
                ExperienceRequiredForNextLevel = PlayerProgressionRules.GetRequiredExperienceForNextLevel(player.Level),

                DaluMoney = player.DaluMoney,

                DamageBonus = PlayerProgressionRules.GetDamageBonus(player.Level),
                BaseMaxHealth = PlayerProgressionRules.GetBaseMaxHealth(player.Level),
                BaseMaxMana = PlayerProgressionRules.GetBaseMaxMana(player.Level),
                MovementTilesPerTurn = PlayerProgressionRules.GetMovementTilesPerTurn(player.Level),

                DifficultyTier = player.DifficultyTier,
                HighestDifficultyTierReached = player.HighestDifficultyTierReached,
                BossesDefeated = player.BossesDefeated
            };
        }
    }
}