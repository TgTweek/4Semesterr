using BackendApi.Application.DTOs.Combat;
using BackendApi.Application.Interfaces;
using BackendApi.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BackendApi.Infrastructure.Services
{
    public sealed class MonsterService : IMonsterService
    {
        private readonly GameDbContext _dbContext;

        public MonsterService(GameDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<IEnumerable<MonsterDto>> GetAllAsync()
        {
            return await _dbContext.MonsterDefinitions
                .Select(x => new MonsterDto
                {
                    MonsterKey = x.MonsterKey,
                    Name = x.Name,
                    MaxHealth = x.MaxHealth,
                    Damage = x.Damage,
                    Mana = x.Mana,
                    GoldReward = x.GoldReward,
                    ExperienceReward = x.ExperienceReward
                })
                .ToListAsync();
        }

        public async Task<MonsterDto?> GetByKeyAsync(string monsterKey)
        {
            return await _dbContext.MonsterDefinitions
                .Where(x => x.MonsterKey == monsterKey)
                .Select(x => new MonsterDto
                {
                    MonsterKey = x.MonsterKey,
                    Name = x.Name,
                    MaxHealth = x.MaxHealth,
                    Damage = x.Damage,
                    Mana = x.Mana,
                    GoldReward = x.GoldReward,
                    ExperienceReward = x.ExperienceReward
                })
                .FirstOrDefaultAsync();
        }
    }
}