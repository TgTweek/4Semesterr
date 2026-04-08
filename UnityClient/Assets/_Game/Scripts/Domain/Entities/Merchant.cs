using System.Collections.Generic;
using System.Linq;
using Game.Domain.Common;
using Game.Domain.ValueObjects;

namespace Game.Domain.Entities
{
    public sealed class Merchant
    {
        private readonly List<MerchantOffer> _offers = new();

        public MerchantId Id { get; }

        public IReadOnlyCollection<MerchantOffer> Offers => _offers.AsReadOnly();

        public Merchant(MerchantId id, IEnumerable<MerchantOffer> offers)
        {
            Id = id;
            _offers.AddRange(offers);
        }

        public MerchantOffer GetOffer(OfferId offerId)
        {
            var offer = _offers.FirstOrDefault(x => x.Id.Value == offerId.Value);

            if (offer is null)
                throw new DomainException("Offer not found.");

            return offer;
        }
    }
}