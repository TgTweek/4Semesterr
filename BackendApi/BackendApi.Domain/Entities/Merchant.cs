using System;
using System.Collections.Generic;

namespace BackendApi.Domain.Entities
{
    public sealed class Merchant
    {
        public Guid MerchantId { get; set; }
        public string Name { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;

        public ICollection<MerchantOffer> Offers { get; set; } = new List<MerchantOffer>();
    }
}