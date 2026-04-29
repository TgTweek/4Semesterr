using System;

namespace BackendApi.Application.DTOs.Merchant
{
    public sealed class MerchantGearOfferResponseDto
    {
        public Guid OfferId { get; set; }
        public Guid GearDefinitionId { get; set; }

        public string GearKey { get; set; } = string.Empty;
        public string GearName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;

        public string Slot { get; set; } = string.Empty;
        public string Rarity { get; set; } = string.Empty;

        public int Price { get; set; }
        public int ArmorValue { get; set; }

        public string? SetKey { get; set; }
        public string? SetName { get; set; }
        public string? ThreePieceBonusDescription { get; set; }

        public string IconKey { get; set; } = string.Empty;

        public bool IsSold { get; set; }
        public int DisplayOrder { get; set; }
    }
}