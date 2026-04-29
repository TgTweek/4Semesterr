using BackendApi.Application.DTOs.Inventory;
using BackendApi.Application.Interfaces;
using BackendApi.Domain.Entities;
using BackendApi.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BackendApi.Infrastructure.Services
{
    public sealed class PlayerInventoryService : IPlayerInventoryService
    {
        private const int MaxLoadoutCards = 10;

        private readonly GameDbContext _dbContext;

        public PlayerInventoryService(GameDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<PlayerInventoryResponseDto> GetInventoryAsync(Guid appUserId)
        {
            var player = await GetPlayerByAppUserIdAsync(appUserId);
            return await BuildInventoryResponseAsync(player.PlayerId);
        }

        public async Task<PlayerInventoryResponseDto> EquipCardAsync(Guid appUserId, Guid playerCardId)
        {
            var player = await GetPlayerByAppUserIdAsync(appUserId);

            var card = await _dbContext.PlayerCards
                .FirstOrDefaultAsync(x =>
                    x.PlayerCardId == playerCardId &&
                    x.PlayerId == player.PlayerId);

            if (card is null)
            {
                throw new KeyNotFoundException("Card was not found in this player's inventory.");
            }

            if (card.Location == InventoryItemLocation.Loadout)
            {
                return await BuildInventoryResponseAsync(player.PlayerId);
            }

            var loadoutCount = await _dbContext.PlayerCards
                .CountAsync(x =>
                    x.PlayerId == player.PlayerId &&
                    x.Location == InventoryItemLocation.Loadout);

            if (loadoutCount >= MaxLoadoutCards)
            {
                throw new InvalidOperationException("Card loadout is full. Maximum is 10 cards.");
            }

            var usedOrders = await _dbContext.PlayerCards
                .Where(x =>
                    x.PlayerId == player.PlayerId &&
                    x.Location == InventoryItemLocation.Loadout &&
                    x.LoadoutOrder != null)
                .Select(x => x.LoadoutOrder!.Value)
                .ToListAsync();

            var nextOrder = Enumerable.Range(1, MaxLoadoutCards)
                .First(x => !usedOrders.Contains(x));

            card.Location = InventoryItemLocation.Loadout;
            card.LoadoutOrder = nextOrder;

            await _dbContext.SaveChangesAsync();

            return await BuildInventoryResponseAsync(player.PlayerId);
        }

        public async Task<PlayerInventoryResponseDto> StashCardAsync(Guid appUserId, Guid playerCardId)
        {
            var player = await GetPlayerByAppUserIdAsync(appUserId);

            var card = await _dbContext.PlayerCards
                .FirstOrDefaultAsync(x =>
                    x.PlayerCardId == playerCardId &&
                    x.PlayerId == player.PlayerId);

            if (card is null)
            {
                throw new KeyNotFoundException("Card was not found in this player's inventory.");
            }

            card.Location = InventoryItemLocation.Stash;
            card.LoadoutOrder = null;

            await _dbContext.SaveChangesAsync();

            return await BuildInventoryResponseAsync(player.PlayerId);
        }

        public async Task<PlayerInventoryResponseDto> EquipGearAsync(Guid appUserId, Guid playerGearId)
        {
            var player = await GetPlayerByAppUserIdAsync(appUserId);

            var gear = await _dbContext.PlayerGears
                .Include(x => x.GearDefinition)
                .FirstOrDefaultAsync(x =>
                    x.PlayerGearId == playerGearId &&
                    x.PlayerId == player.PlayerId);

            if (gear is null)
            {
                throw new KeyNotFoundException("Gear was not found in this player's inventory.");
            }

            var slot = gear.GearDefinition.Slot;

            if (string.IsNullOrWhiteSpace(slot))
            {
                throw new InvalidOperationException("Gear slot is missing.");
            }

            var equippedGearInSameSlot = await _dbContext.PlayerGears
                .Include(x => x.GearDefinition)
                .Where(x =>
                    x.PlayerId == player.PlayerId &&
                    x.PlayerGearId != gear.PlayerGearId &&
                    x.Location == InventoryItemLocation.Loadout &&
                    x.GearDefinition.Slot == slot)
                .ToListAsync();

            foreach (var equippedGear in equippedGearInSameSlot)
            {
                equippedGear.Location = InventoryItemLocation.Stash;
            }

            gear.Location = InventoryItemLocation.Loadout;

            await _dbContext.SaveChangesAsync();

            return await BuildInventoryResponseAsync(player.PlayerId);
        }

        public async Task<PlayerInventoryResponseDto> StashGearAsync(Guid appUserId, Guid playerGearId)
        {
            var player = await GetPlayerByAppUserIdAsync(appUserId);

            var gear = await _dbContext.PlayerGears
                .FirstOrDefaultAsync(x =>
                    x.PlayerGearId == playerGearId &&
                    x.PlayerId == player.PlayerId);

            if (gear is null)
            {
                throw new KeyNotFoundException("Gear was not found in this player's inventory.");
            }

            gear.Location = InventoryItemLocation.Stash;

            await _dbContext.SaveChangesAsync();

            return await BuildInventoryResponseAsync(player.PlayerId);
        }

        private async Task<Player> GetPlayerByAppUserIdAsync(Guid appUserId)
        {
            var player = await _dbContext.Players
                .FirstOrDefaultAsync(x => x.AppUserId == appUserId);

            if (player is null)
            {
                throw new KeyNotFoundException("Player profile was not found.");
            }

            return player;
        }

        private async Task<PlayerInventoryResponseDto> BuildInventoryResponseAsync(Guid playerId)
        {
            var cards = await _dbContext.PlayerCards
                .AsNoTracking()
                .Include(x => x.CardDefinition)
                .Where(x => x.PlayerId == playerId)
                .OrderBy(x => x.Location == InventoryItemLocation.Loadout ? 0 : 1)
                .ThenBy(x => x.LoadoutOrder ?? 999)
                .ThenBy(x => x.CardDefinition.Name)
                .Select(x => new PlayerCardInventoryItemDto
                {
                    PlayerCardId = x.PlayerCardId,
                    CardDefinitionId = x.CardDefinitionId,
                    CardKey = x.CardDefinition.Key,
                    CardName = x.CardDefinition.Name,
                    Description = x.CardDefinition.Description,
                    ManaCost = x.CardDefinition.ManaCost,
                    EffectType = x.CardDefinition.EffectType,
                    EffectValue = x.CardDefinition.EffectValue,
                    Rarity = x.CardDefinition.Rarity,
                    IconKey = x.CardDefinition.IconKey,
                    Location = x.Location,
                    LoadoutOrder = x.LoadoutOrder ?? 0
                })
                .ToListAsync();

            var playerGear = await _dbContext.PlayerGears
                .AsNoTracking()
                .Include(x => x.GearDefinition)
                .Where(x => x.PlayerId == playerId)
                .ToListAsync();

            var setKeys = playerGear
                .Select(x => x.GearDefinition.SetKey)
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Select(x => x!)
                .Distinct()
                .ToList();

            var gearSetsByKey = await _dbContext.GearSetDefinitions
                .Where(x => setKeys.Contains(x.SetKey))
                .ToDictionaryAsync(x => x.SetKey);

            var gear = playerGear
                .Select(x =>
                {
                    GearSetDefinition? gearSet = null;

                    if (!string.IsNullOrWhiteSpace(x.GearDefinition.SetKey))
                    {
                        gearSetsByKey.TryGetValue(x.GearDefinition.SetKey!, out gearSet);
                    }

                    return new PlayerGearInventoryItemDto
                    {
                        PlayerGearId = x.PlayerGearId,
                        GearDefinitionId = x.GearDefinitionId,
                        GearKey = x.GearDefinition.Key,
                        GearName = x.GearDefinition.Name,
                        Description = x.GearDefinition.Description,
                        Slot = x.GearDefinition.Slot,
                        Rarity = x.GearDefinition.Rarity,
                        ArmorValue = x.GearDefinition.ArmorValue,
                        SetKey = x.GearDefinition.SetKey ?? string.Empty,
                        SetName = gearSet?.Name ?? string.Empty,
                        ThreePieceBonusDescription = gearSet?.ThreePieceBonusDescription ?? string.Empty,
                        IconKey = x.GearDefinition.IconKey,
                        Location = x.Location
                    };
                })
                .OrderBy(x => x.Location == InventoryItemLocation.Loadout ? 0 : 1)
                .ThenBy(x => GetGearSlotSortOrder(x.Slot))
                .ThenBy(x => x.GearName)
                .ToList();

            return new PlayerInventoryResponseDto
            {
                Cards = cards,
                Gear = gear
            };
        }

        private static int GetGearSlotSortOrder(string slot)
        {
            return slot switch
            {
                "Helmet" => 1,
                "Chest" => 2,
                "Legs" => 3,
                _ => 99
            };
        }
    }
}