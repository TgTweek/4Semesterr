using System;
using System.Collections.Generic;

namespace Game.Infrastructure.Api.Dtos.Merchant
{
    [Serializable]
    public sealed class MerchantInventoryResponseDto
    {
        public List<MerchantOfferResponseDto> cardOffers = new();
        public List<MerchantGearOfferResponseDto> gearOffers = new();
    }
}