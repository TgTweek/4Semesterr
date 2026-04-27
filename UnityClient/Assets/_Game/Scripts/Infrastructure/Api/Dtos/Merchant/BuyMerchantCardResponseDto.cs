using System;

namespace Game.Infrastructure.Api.Dtos.Merchant
{
    [Serializable]
    public sealed class BuyMerchantCardResponseDto
    {
        public bool success;
        public string message = "";
        public int remainingDaluMoney;
        public string cardDefinitionId = "";
        public string cardKey = "";
    }
}