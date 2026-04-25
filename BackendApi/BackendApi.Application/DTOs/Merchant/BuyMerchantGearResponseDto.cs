using System;

namespace BackendApi.Application.DTOs.Merchant
{
    public sealed class BuyMerchantGearResponseDto
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public int RemainingDaluMoney { get; set; }

        public Guid GearDefinitionId { get; set; }
        public string GearKey { get; set; } = string.Empty;
    }
}