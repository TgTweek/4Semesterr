namespace Game.Application.Merchant.Results
{
    public sealed class BuyMerchantCardResult
    {
        public bool Success { get; set; }
        public string Error { get; set; } = "";
        public int RemainingGold { get; set; }
        public string BoughtCardDefinitionId { get; set; } = "";
    }
}