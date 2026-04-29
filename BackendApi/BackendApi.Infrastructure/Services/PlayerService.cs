using BackendApi.Application.DTOs.Player;
using BackendApi.Application.Interfaces;
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

            return new PlayerDto
            {
                PlayerId = player.PlayerId,
                PlayerName = player.PlayerName,
                Level = player.Level,
                Experience = player.Experience,
                ExperienceRequiredForNextLevel = GetRequiredExperienceForNextLevel(player.Level),
                DaluMoney = player.DaluMoney,
                DamageBonus = player.DamageBonus,
                BaseMaxHealth = player.BaseMaxHealth,
                BaseMaxMana = player.BaseMaxMana
            };
        }

        private static int GetRequiredExperienceForNextLevel(int currentLevel)
        {
            return 50 + ((currentLevel - 1) * 25);
        }
    }
}