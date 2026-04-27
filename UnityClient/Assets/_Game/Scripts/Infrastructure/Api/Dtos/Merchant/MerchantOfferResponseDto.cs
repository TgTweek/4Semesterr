using System;

namespace Game.Infrastructure.Api.Dtos.Merchant
{
    [Serializable]
    public sealed class MerchantOfferResponseDto
    {
        public string offerId = "";
        public string cardDefinitionId = "";
        public string cardKey = "";
        public string cardName = "";
        public string description = "";
        public int manaCost;
        public int price;
        public string effectType = "";
        public int effectValue;
        public string rarity = "";
        public string iconKey = "";
        public bool isSold;
        public int displayOrder;
    }
}