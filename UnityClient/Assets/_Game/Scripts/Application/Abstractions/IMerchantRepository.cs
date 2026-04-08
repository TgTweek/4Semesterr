using Game.Domain.ValueObjects;

namespace Game.Application.Abstractions
{
    public interface IMerchantRepository
    {
        Game.Domain.Entities.Merchant GetById(MerchantId merchantId);
        void Save(Game.Domain.Entities.Merchant merchant);
    }
}