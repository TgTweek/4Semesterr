using System;

namespace BackendApi.Application.DTOs.Merchant
{
    public sealed class BuyMerchantCardResponseDto
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public int RemainingDaluMoney { get; set; }
        public Guid CardDefinitionId { get; set; }
        public string CardKey { get; set; } = string.Empty;
    }
}