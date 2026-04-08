using System.Collections.Generic;
using System.Linq;
using Game.Application.Abstractions;
using Game.Application.Merchant.Queries;
using Game.Domain.ValueObjects;

namespace Game.Application.Merchant.UseCases
{
    public sealed class GetMerchantOffersUseCase
    {
        private readonly IMerchantRepository _merchantRepository;
        private readonly ICardDefinitionCatalog _cardDefinitionCatalog;

        public GetMerchantOffersUseCase(
            IMerchantRepository merchantRepository,
            ICardDefinitionCatalog cardDefinitionCatalog)
        {
            _merchantRepository = merchantRepository;
            _cardDefinitionCatalog = cardDefinitionCatalog;
        }

        public IReadOnlyList<MerchantOfferDto> Execute(string merchantId)
        {
            var merchant = _merchantRepository.GetById(new MerchantId(merchantId));

            return merchant.Offers
                .Select(offer => new MerchantOfferDto
                {
                    OfferId = offer.Id.Value,
                    CardDefinitionId = offer.CardDefinitionId.Value,
                    CardName = _cardDefinitionCatalog.GetName(offer.CardDefinitionId.Value),
                    Price = offer.Price.Value,
                    IsSold = offer.IsSold
                })
                .ToList();
        }
    }
}