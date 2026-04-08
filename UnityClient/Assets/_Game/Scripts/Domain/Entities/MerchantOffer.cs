using Game.Domain.Common;
using Game.Domain.ValueObjects;

namespace Game.Domain.Entities
{
    public sealed class MerchantOffer
    {
        public OfferId Id { get; }
        public CardDefinitionId CardDefinitionId { get; }
        public GoldAmount Price { get; }
        public bool IsSold { get; private set; }

        public MerchantOffer(
            OfferId id,
            CardDefinitionId cardDefinitionId,
            GoldAmount price,
            bool isSold = false)
        {
            Id = id;
            CardDefinitionId = cardDefinitionId;
            Price = price;
            IsSold = isSold;
        }

        public void MarkSold()
        {
            if (IsSold)
                throw new DomainException("Offer has already been sold.");

            IsSold = true;
        }
    }
}