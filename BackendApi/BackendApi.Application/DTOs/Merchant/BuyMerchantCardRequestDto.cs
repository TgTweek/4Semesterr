using System;

namespace BackendApi.Application.DTOs.Merchant
{
    public sealed class BuyMerchantCardRequestDto
    {
        public Guid OfferId { get; set; }
    }
}