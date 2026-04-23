using System;

namespace BackendApi.Domain.Entities
{
    public sealed class PlayerMerchantCardOffer
    {
        public Guid PlayerMerchantCardOfferId { get; set; }

        public Guid PlayerId { get; set; }
        public Player Player { get; set; } = null!;

        public Guid MerchantId { get; set; }
        public Merchant Merchant { get; set; } = null!;

        public Guid CardDefinitionId { get; set; }
        public CardDefinition CardDefinition { get; set; } = null!;

        public int Price { get; set; }
        public int DisplayOrder { get; set; }

        public bool IsSold { get; set; }

        public DateTime GeneratedAtUtc { get; set; }

        public byte[] RowVersion { get; set; } = Array.Empty<byte>();
    }
}