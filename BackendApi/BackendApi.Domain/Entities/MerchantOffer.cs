using System;

namespace BackendApi.Domain.Entities
{
    public sealed class MerchantOffer
    {
        public Guid MerchantOfferId { get; set; }

        public Guid MerchantId { get; set; }
        public Merchant Merchant { get; set; } = null!;

        public Guid CardDefinitionId { get; set; }
        public CardDefinition CardDefinition { get; set; } = null!;

        public int Price { get; set; }
        public int Stock { get; set; }
        public bool IsActive { get; set; } = true;

        public byte[] RowVersion { get; set; } = Array.Empty<byte>();
    }
}