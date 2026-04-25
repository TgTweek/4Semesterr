using System.Collections.Generic;

namespace BackendApi.Application.DTOs.Merchant
{
    public sealed class MerchantInventoryResponseDto
    {
        public IReadOnlyList<MerchantOfferResponseDto> CardOffers { get; set; }
            = new List<MerchantOfferResponseDto>();

        public IReadOnlyList<MerchantGearOfferResponseDto> GearOffers { get; set; }
            = new List<MerchantGearOfferResponseDto>();
    }
}