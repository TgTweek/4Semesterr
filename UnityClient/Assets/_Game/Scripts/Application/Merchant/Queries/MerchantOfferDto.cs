namespace Game.Application.Merchant.Queries
{
    public sealed class MerchantOfferDto
    {
        public string OfferId { get; set; } = "";
        public string CardDefinitionId { get; set; } = "";
        public string CardName { get; set; } = "";
        public int Price { get; set; }
        public bool IsSold { get; set; }
    }
}