using BackendApi.Application.DTOs.Merchant;
using BackendApi.Application.Interfaces;
using BackendApi.Domain.Entities;
using BackendApi.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BackendApi.Infrastructure.Services
{
    public sealed class MerchantService : IMerchantService
    {
        private readonly GameDbContext _dbContext;

        public MerchantService(GameDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<IReadOnlyList<MerchantOfferResponseDto>> GetOffersAsync(Guid merchantId)
        {
            var offers = await _dbContext.MerchantOffers
                .Include(x => x.CardDefinition)
                .Where(x => x.MerchantId == merchantId && x.IsActive && x.Stock > 0)
                .Select(x => new MerchantOfferResponseDto
                {
                    OfferId = x.MerchantOfferId,
                    CardDefinitionId = x.CardDefinitionId,
                    CardKey = x.CardDefinition.Key,
                    CardName = x.CardDefinition.Name,
                    Description = x.CardDefinition.Description,
                    ManaCost = x.CardDefinition.ManaCost,
                    Price = x.Price,
                    Rarity = x.CardDefinition.Rarity,
                    IconKey = x.CardDefinition.IconKey,
                    Stock = x.Stock
                })
                .ToListAsync();

            return offers;
        }

        public async Task<BuyMerchantCardResponseDto> BuyCardAsync(Guid appUserId, Guid merchantId, Guid offerId)
        {
            var player = await _dbContext.Players
                .FirstOrDefaultAsync(x => x.AppUserId == appUserId);

            if (player is null)
            {
                return new BuyMerchantCardResponseDto
                {
                    Success = false,
                    Message = "Player not found."
                };
            }

            var offer = await _dbContext.MerchantOffers
                .Include(x => x.CardDefinition)
                .FirstOrDefaultAsync(x =>
                    x.MerchantOfferId == offerId &&
                    x.MerchantId == merchantId);

            if (offer is null)
            {
                return new BuyMerchantCardResponseDto
                {
                    Success = false,
                    Message = "Offer not found."
                };
            }

            if (!offer.IsActive)
            {
                return new BuyMerchantCardResponseDto
                {
                    Success = false,
                    Message = "Offer is inactive."
                };
            }

            if (offer.Stock <= 0)
            {
                return new BuyMerchantCardResponseDto
                {
                    Success = false,
                    Message = "Offer is out of stock."
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
            offer.Stock -= 1;

            var playerCard = new PlayerCard
            {
                PlayerCardId = Guid.NewGuid(),
                PlayerId = player.PlayerId,
                CardDefinitionId = offer.CardDefinitionId,
                AcquiredAtUtc = DateTime.UtcNow
            };

            _dbContext.PlayerCards.Add(playerCard);

            try
            {
                await _dbContext.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                return new BuyMerchantCardResponseDto
                {
                    Success = false,
                    Message = "The offer or player was changed by another request. Refresh and try again."
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
    }
}