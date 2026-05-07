using BackendApi.Application.Interfaces;
using BackendApi.Domain.Entities;
using BackendApi.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BackendApi.Infrastructure.Services
{
    public sealed class StarterInventoryService : IStarterInventoryService
    {
        private const string StarterStrikeKey = "starter_strike";
        private const string StarterBlockKey = "starter_block";

        private readonly GameDbContext _dbContext;

        public StarterInventoryService(GameDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task GrantStarterCardsAsync(Guid playerId)
        {
            var alreadyHasCards = await _dbContext.PlayerCards
                .AnyAsync(x => x.PlayerId == playerId);

            if (alreadyHasCards)
            {
                return;
            }

            var strikeDefinition = await _dbContext.CardDefinitions
                .FirstOrDefaultAsync(x => x.Key == StarterStrikeKey && x.IsActive);

            var blockDefinition = await _dbContext.CardDefinitions
                .FirstOrDefaultAsync(x => x.Key == StarterBlockKey && x.IsActive);

            if (strikeDefinition is null)
            {
                throw new InvalidOperationException(
                    $"Starter card definition '{StarterStrikeKey}' was not found.");
            }

            if (blockDefinition is null)
            {
                throw new InvalidOperationException(
                    $"Starter card definition '{StarterBlockKey}' was not found.");
            }

            var loadoutOrder = 1;
            var acquiredAtUtc = DateTime.UtcNow;

            for (var i = 0; i < 3; i++)
            {
                _dbContext.PlayerCards.Add(new PlayerCard
                {
                    PlayerCardId = Guid.NewGuid(),
                    PlayerId = playerId,
                    CardDefinitionId = strikeDefinition.CardDefinitionId,
                    Location = InventoryItemLocation.Loadout,
                    LoadoutOrder = loadoutOrder,
                    AcquiredAtUtc = acquiredAtUtc
                });

                loadoutOrder++;
            } 

            for (var i = 0; i < 3; i++)
            {
                _dbContext.PlayerCards.Add(new PlayerCard
                {
                    PlayerCardId = Guid.NewGuid(),
                    PlayerId = playerId,
                    CardDefinitionId = blockDefinition.CardDefinitionId,
                    Location = InventoryItemLocation.Loadout,
                    LoadoutOrder = loadoutOrder,
                    AcquiredAtUtc = acquiredAtUtc
                });

                loadoutOrder++;
            }

            await _dbContext.SaveChangesAsync();
        }
    }
}