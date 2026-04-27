using System;

namespace Game.Infrastructure.Api.Dtos.Merchant
{
    [Serializable]
    public sealed class MerchantGearOfferResponseDto
    {
        public string offerId = "";
        public string gearDefinitionId = "";
        public string gearKey = "";
        public string gearName = "";
        public string description = "";
        public string slot = "";
        public string rarity = "";
        public int price;
        public int armorValue;
        public string setKey = "";
        public string setName = "";
        public string threePieceBonusDescription = "";
        public string iconKey = "";
        public bool isSold;
        public int displayOrder;
    }
}