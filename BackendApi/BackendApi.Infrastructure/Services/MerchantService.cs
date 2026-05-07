using System.Security.Cryptography;
using BackendApi.Application.DTOs.Merchant;
using BackendApi.Application.Interfaces;
using BackendApi.Domain.Entities;
using BackendApi.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BackendApi.Infrastructure.Services
{
    public sealed class MerchantService : IMerchantService
    {
        private const int CardOfferCount = 10;
        private const int GearOfferCountPerSlot = 2;

        // 5% chance. Vil først kunne trigge når der findes mindst 10 rare merchant cards.
        private const double RareOnlyShopChance = 0.05d;

        private readonly GameDbContext _dbContext;

        public MerchantService(GameDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<MerchantInventoryResponseDto> GetInventoryAsync(Guid appUserId, Guid merchantId)
        {
            var player = await GetPlayerByAppUserIdAsync(appUserId);

            await EnsureInventoryExistsAsync(player.PlayerId, merchantId);

            var cardOffers = await GetCardOffersInternalAsync(player.PlayerId, merchantId);
            var gearOffers = await GetGearOffersInternalAsync(player.PlayerId, merchantId);

            return new MerchantInventoryResponseDto
            {
                CardOffers = cardOffers,
                GearOffers = gearOffers
            };
        }

        public async Task<BuyMerchantCardResponseDto> BuyCardAsync(Guid appUserId, Guid merchantId, Guid offerId)
        {
            var player = await GetPlayerByAppUserIdAsync(appUserId);

            var offer = await _dbContext.PlayerMerchantCardOffers
                .Include(x => x.CardDefinition)
                .FirstOrDefaultAsync(x =>
                    x.PlayerId == player.PlayerId &&
                    x.MerchantId == merchantId &&
                    x.PlayerMerchantCardOfferId == offerId);

            if (offer is null)
            {
                return new BuyMerchantCardResponseDto
                {
                    Success = false,
                    Message = "Card offer not found."
                };
            }

            if (offer.IsSold)
            {
                return new BuyMerchantCardResponseDto
                {
                    Success = false,
                    Message = "This card offer has already been purchased."
                };
            }

            if (player.DaluMoney < offer.Price)
            {
                return new BuyMerchantCardResponseDto
                {
                    Success = false,
                    Message = "Not enough DaluMoney."
                };
            }

            player.DaluMoney -= offer.Price;
            offer.IsSold = true;

            _dbContext.PlayerCards.Add(new PlayerCard
            {
                PlayerCardId = Guid.NewGuid(),
                PlayerId = player.PlayerId,
                CardDefinitionId = offer.CardDefinitionId,
                Location = InventoryItemLocation.Stash,
                LoadoutOrder = null,
                AcquiredAtUtc = DateTime.UtcNow
            });

            try
            {
                await _dbContext.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                return new BuyMerchantCardResponseDto
                {
                    Success = false,
                    Message = "The offer changed during purchase. Refresh and try again."
                };
            }

            return new BuyMerchantCardResponseDto
            {
                Success = true,
                Message = "Card purchased successfully.",
                RemainingDaluMoney = player.DaluMoney,
                CardDefinitionId = offer.CardDefinitionId,
                CardKey = offer.CardDefinition.Key
            };
        }

        public async Task<BuyMerchantGearResponseDto> BuyGearAsync(Guid appUserId, Guid merchantId, Guid offerId)
        {
            var player = await GetPlayerByAppUserIdAsync(appUserId);

            var offer = await _dbContext.PlayerMerchantGearOffers
                .Include(x => x.GearDefinition)
                .FirstOrDefaultAsync(x =>
                    x.PlayerId == player.PlayerId &&
                    x.MerchantId == merchantId &&
                    x.PlayerMerchantGearOfferId == offerId);

            if (offer is null)
            {
                return new BuyMerchantGearResponseDto
                {
                    Success = false,
                    Message = "Gear offer not found."
                };
            }

            if (offer.IsSold)
            {
                return new BuyMerchantGearResponseDto
                {
                    Success = false,
                    Message = "This gear offer has already been purchased."
                };
            }

            if (player.DaluMoney < offer.Price)
            {
                return new BuyMerchantGearResponseDto
                {
                    Success = false,
                    Message = "Not enough DaluMoney."
                };
            }

            player.DaluMoney -= offer.Price;
            offer.IsSold = true;

            _dbContext.PlayerGears.Add(new PlayerGear
            {
                PlayerGearId = Guid.NewGuid(),
                PlayerId = player.PlayerId,
                GearDefinitionId = offer.GearDefinitionId,
                Location = InventoryItemLocation.Stash,
                AcquiredAtUtc = DateTime.UtcNow
            });

            try
            {
                await _dbContext.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                return new BuyMerchantGearResponseDto
                {
                    Success = false,
                    Message = "The offer changed during purchase. Refresh and try again."
                };
            }

            return new BuyMerchantGearResponseDto
            {
                Success = true,
                Message = "Gear purchased successfully.",
                RemainingDaluMoney = player.DaluMoney,
                GearDefinitionId = offer.GearDefinitionId,
                GearKey = offer.GearDefinition.Key
            };
        }

        public async Task RefreshInventoryAsync(Guid appUserId, Guid merchantId)
        {
            var player = await GetPlayerByAppUserIdAsync(appUserId);
            await ReplaceInventoryAsync(player.PlayerId, merchantId);
        }

        private async Task<Player> GetPlayerByAppUserIdAsync(Guid appUserId)
        {
            var player = await _dbContext.Players
                .FirstOrDefaultAsync(x => x.AppUserId == appUserId);

            if (player is null)
            {
                throw new KeyNotFoundException("Player not found.");
            }

            return player;
        }

        private async Task EnsureInventoryExistsAsync(Guid playerId, Guid merchantId)
        {
            var merchantExists = await _dbContext.Merchants
                .AnyAsync(x => x.MerchantId == merchantId && x.IsActive);

            if (!merchantExists)
            {
                throw new KeyNotFoundException("Merchant not found.");
            }

            var existingCardCount = await _dbContext.PlayerMerchantCardOffers
                .CountAsync(x => x.PlayerId == playerId && x.MerchantId == merchantId);

            var existingGearCount = await _dbContext.PlayerMerchantGearOffers
                .CountAsync(x => x.PlayerId == playerId && x.MerchantId == merchantId);

            if (existingCardCount == CardOfferCount && existingGearCount == GearOfferCountPerSlot * 3)
            {
                return;
            }

            await ReplaceInventoryAsync(playerId, merchantId);
        }

        private async Task ReplaceInventoryAsync(Guid playerId, Guid merchantId)
        {
            var existingCardOffers = await _dbContext.PlayerMerchantCardOffers
                .Where(x => x.PlayerId == playerId && x.MerchantId == merchantId)
                .ToListAsync();

            var existingGearOffers = await _dbContext.PlayerMerchantGearOffers
                .Where(x => x.PlayerId == playerId && x.MerchantId == merchantId)
                .ToListAsync();

            if (existingCardOffers.Count > 0)
            {
                _dbContext.PlayerMerchantCardOffers.RemoveRange(existingCardOffers);
            }

            if (existingGearOffers.Count > 0)
            {
                _dbContext.PlayerMerchantGearOffers.RemoveRange(existingGearOffers);
            }

            var generatedAtUtc = DateTime.UtcNow;

            var allMerchantCards = await _dbContext.CardDefinitions
                .Where(x => x.IsActive && x.IsMerchantAvailable)
                .ToListAsync();

            var commonCards = allMerchantCards.Where(x => x.Rarity == "Common").ToList();
            var uncommonCards = allMerchantCards.Where(x => x.Rarity == "Uncommon").ToList();
            var rareCards = allMerchantCards.Where(x => x.Rarity == "Rare").ToList();

            var selectedCards = BuildSelectedCardList(commonCards, uncommonCards, rareCards);
            ShuffleInPlace(selectedCards);

            for (var i = 0; i < selectedCards.Count; i++)
            {
                var card = selectedCards[i];

                _dbContext.PlayerMerchantCardOffers.Add(new PlayerMerchantCardOffer
                {
                    PlayerMerchantCardOfferId = Guid.NewGuid(),
                    PlayerId = playerId,
                    MerchantId = merchantId,
                    CardDefinitionId = card.CardDefinitionId,
                    Price = card.Price,
                    DisplayOrder = i + 1,
                    IsSold = false,
                    GeneratedAtUtc = generatedAtUtc
                });
            }

            var allMerchantGear = await _dbContext.GearDefinitions
                .Where(x => x.IsMerchantAvailable)
                .ToListAsync();

            var helmets = allMerchantGear.Where(x => x.Slot == "Helmet").ToList();
            var chests = allMerchantGear.Where(x => x.Slot == "Chest").ToList();
            var legs = allMerchantGear.Where(x => x.Slot == "Legs").ToList();

            if (helmets.Count < GearOfferCountPerSlot ||
                chests.Count < GearOfferCountPerSlot ||
                legs.Count < GearOfferCountPerSlot)
            {
                throw new InvalidOperationException("Not enough gear definitions to generate merchant gear offers.");
            }

            var selectedGear = new List<GearDefinition>();
            selectedGear.AddRange(TakeRandomDistinct(helmets, GearOfferCountPerSlot));
            selectedGear.AddRange(TakeRandomDistinct(chests, GearOfferCountPerSlot));
            selectedGear.AddRange(TakeRandomDistinct(legs, GearOfferCountPerSlot));
            ShuffleInPlace(selectedGear);

            for (var i = 0; i < selectedGear.Count; i++)
            {
                var gear = selectedGear[i];

                _dbContext.PlayerMerchantGearOffers.Add(new PlayerMerchantGearOffer
                {
                    PlayerMerchantGearOfferId = Guid.NewGuid(),
                    PlayerId = playerId,
                    MerchantId = merchantId,
                    GearDefinitionId = gear.GearDefinitionId,
                    Price = gear.Price,
                    DisplayOrder = i + 1,
                    IsSold = false,
                    GeneratedAtUtc = generatedAtUtc
                });
            }

            await _dbContext.SaveChangesAsync();
        }

        private List<CardDefinition> BuildSelectedCardList(
            List<CardDefinition> commonCards,
            List<CardDefinition> uncommonCards,
            List<CardDefinition> rareCards)
        {
            if (CanUseRareOnlyShop(rareCards.Count))
            {
                return TakeRandomDistinct(rareCards, CardOfferCount);
            }

            var composition = ChooseNormalCardComposition(
                commonCards.Count,
                uncommonCards.Count,
                rareCards.Count);

            var selectedCards = new List<CardDefinition>();

            selectedCards.AddRange(TakeRandomDistinct(rareCards, composition.RareCount));
            selectedCards.AddRange(TakeRandomDistinct(uncommonCards, composition.UncommonCount));

            var commonCount = CardOfferCount - composition.RareCount - composition.UncommonCount;
            selectedCards.AddRange(TakeRandomDistinct(commonCards, commonCount));

            return selectedCards;
        }

        private static bool CanUseRareOnlyShop(int rarePoolCount)
        {
            if (rarePoolCount < CardOfferCount)
            {
                return false;
            }

            return RollChance(RareOnlyShopChance);
        }

        private static (int RareCount, int UncommonCount) ChooseNormalCardComposition(
            int commonPoolCount,
            int uncommonPoolCount,
            int rarePoolCount)
        {
            var feasibleCompositions = new List<(int RareCount, int UncommonCount)>();

            for (var rareCount = 1; rareCount <= Math.Min(2, rarePoolCount); rareCount++)
            {
                for (var uncommonCount = 2; uncommonCount <= Math.Min(4, uncommonPoolCount); uncommonCount++)
                {
                    var commonCount = CardOfferCount - rareCount - uncommonCount;

                    if (commonCount >= 0 && commonCount <= commonPoolCount)
                    {
                        feasibleCompositions.Add((rareCount, uncommonCount));
                    }
                }
            }

            if (feasibleCompositions.Count == 0)
            {
                throw new InvalidOperationException(
                    "Not enough merchant-available card definitions to generate a valid 10-card merchant shop.");
            }

            var randomIndex = RandomNumberGenerator.GetInt32(feasibleCompositions.Count);
            return feasibleCompositions[randomIndex];
        }

        private async Task<IReadOnlyList<MerchantOfferResponseDto>> GetCardOffersInternalAsync(Guid playerId, Guid merchantId)
        {
            return await _dbContext.PlayerMerchantCardOffers
                .Where(x => x.PlayerId == playerId && x.MerchantId == merchantId)
                .Include(x => x.CardDefinition)
                .OrderBy(x => x.DisplayOrder)
                .Select(x => new MerchantOfferResponseDto
                {
                    OfferId = x.PlayerMerchantCardOfferId,
                    CardDefinitionId = x.CardDefinitionId,
                    CardKey = x.CardDefinition.Key,
                    CardName = x.CardDefinition.Name,
                    Description = x.CardDefinition.Description,
                    ManaCost = x.CardDefinition.ManaCost,
                    Price = x.Price,
                    EffectType = x.CardDefinition.EffectType,
                    EffectValue = x.CardDefinition.EffectValue,
                    Rarity = x.CardDefinition.Rarity,
                    IconKey = x.CardDefinition.IconKey,
                    IsSold = x.IsSold,
                    DisplayOrder = x.DisplayOrder
                })
                .ToListAsync();
        }

        private async Task<IReadOnlyList<MerchantGearOfferResponseDto>> GetGearOffersInternalAsync(Guid playerId, Guid merchantId)
        {
            var offers = await _dbContext.PlayerMerchantGearOffers
                .Where(x => x.PlayerId == playerId && x.MerchantId == merchantId)
                .Include(x => x.GearDefinition)
                .OrderBy(x => x.DisplayOrder)
                .ToListAsync();

            var setKeys = offers
                .Select(x => x.GearDefinition.SetKey)
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Distinct()
                .ToList();

            var gearSetsByKey = await _dbContext.GearSetDefinitions
                .Where(x => setKeys.Contains(x.SetKey))
                .ToDictionaryAsync(x => x.SetKey);

            var result = new List<MerchantGearOfferResponseDto>();

            foreach (var offer in offers)
            {
                GearSetDefinition? gearSet = null;

                if (!string.IsNullOrWhiteSpace(offer.GearDefinition.SetKey))
                {
                    gearSetsByKey.TryGetValue(offer.GearDefinition.SetKey!, out gearSet);
                }

                result.Add(new MerchantGearOfferResponseDto
                {
                    OfferId = offer.PlayerMerchantGearOfferId,
                    GearDefinitionId = offer.GearDefinitionId,
                    GearKey = offer.GearDefinition.Key,
                    GearName = offer.GearDefinition.Name,
                    Description = offer.GearDefinition.Description,
                    Slot = offer.GearDefinition.Slot,
                    Rarity = offer.GearDefinition.Rarity,
                    Price = offer.Price,
                    ArmorValue = offer.GearDefinition.ArmorValue,
                    SetKey = offer.GearDefinition.SetKey,
                    SetName = gearSet?.Name,
                    ThreePieceBonusDescription = gearSet?.ThreePieceBonusDescription,
                    IconKey = offer.GearDefinition.IconKey,
                    IsSold = offer.IsSold,
                    DisplayOrder = offer.DisplayOrder
                });
            }

            return result;
        }

        private static List<T> TakeRandomDistinct<T>(IReadOnlyList<T> source, int count)
        {
            if (count < 0 || count > source.Count)
            {
                throw new InvalidOperationException("Cannot take more random items than exist in the pool.");
            }

            var pool = source.ToList();
            var result = new List<T>();

            for (var i = 0; i < count; i++)
            {
                var index = RandomNumberGenerator.GetInt32(pool.Count);
                result.Add(pool[index]);
                pool.RemoveAt(index);
            }

            return result;
        }

        private static void ShuffleInPlace<T>(IList<T> list)
        {
            for (var i = list.Count - 1; i > 0; i--)
            {
                var j = RandomNumberGenerator.GetInt32(i + 1);
                (list[i], list[j]) = (list[j], list[i]);
            }
        }

        private static bool RollChance(double chance)
        {
            if (chance <= 0d)
            {
                return false;
            }

            if (chance >= 1d)
            {
                return true;
            }

            var roll = RandomNumberGenerator.GetInt32(0, 10_000);
            return roll < (int)(chance * 10_000);
        }
    }
}