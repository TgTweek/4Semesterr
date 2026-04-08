namespace Game.Application.Merchant.Commands
{
    public sealed class BuyMerchantCardCommand
    {
        public string PlayerId { get; set; } = "";
        public string MerchantId { get; set; } = "";
        public string OfferId { get; set; } = "";
    }
}