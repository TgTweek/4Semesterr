using System;

namespace Game.Infrastructure.Api.Dtos.Merchant
{
    [Serializable]
    public sealed class BuyMerchantGearResponseDto
    {
        public bool success;
        public string message = "";
        public int remainingDaluMoney;
        public string gearDefinitionId = "";
        public string gearKey = "";
    }
}