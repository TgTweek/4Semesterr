using System;

namespace BackendApi.Application.DTOs.Merchant
{
    public sealed class MerchantOfferResponseDto
    {
        public Guid OfferId { get; set; }
        public Guid CardDefinitionId { get; set; }

        public string CardKey { get; set; } = string.Empty;
        public string CardName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;

        public int ManaCost { get; set; }
        public int Price { get; set; }

        public string EffectType { get; set; } = string.Empty;
        public int EffectValue { get; set; }

        public string Rarity { get; set; } = string.Empty;
        public string IconKey { get; set; } = string.Empty;

        public bool IsSold { get; set; }
        public int DisplayOrder { get; set; }
    }
}