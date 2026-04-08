using Game.Application.Abstractions;
using Game.Application.Merchant.Commands;
using Game.Application.Merchant.Results;
using Game.Domain.Common;
using Game.Domain.ValueObjects;

namespace Game.Application.Merchant.UseCases
{
    public sealed class BuyMerchantCardUseCase
    {
        private readonly IPlayerRepository _playerRepository;
        private readonly IMerchantRepository _merchantRepository;

        public BuyMerchantCardUseCase(
            IPlayerRepository playerRepository,
            IMerchantRepository merchantRepository)
        {
            _playerRepository = playerRepository;
            _merchantRepository = merchantRepository;
        }

        public BuyMerchantCardResult Execute(BuyMerchantCardCommand command)
        {
            try
            {
                var player = _playerRepository.GetById(new PlayerId(command.PlayerId));
                var merchant = _merchantRepository.GetById(new MerchantId(command.MerchantId));
                var offer = merchant.GetOffer(new OfferId(command.OfferId));

                player.SpendGold(offer.Price);
                player.AddCard(offer.CardDefinitionId);
                offer.MarkSold();

                _playerRepository.Save(player);
                _merchantRepository.Save(merchant);

                return new BuyMerchantCardResult
                {
                    Success = true,
                    RemainingGold = player.Gold.Value,
                    BoughtCardDefinitionId = offer.CardDefinitionId.Value
                };
            }
            catch (DomainException ex)
            {
                return new BuyMerchantCardResult
                {
                    Success = false,
                    Error = ex.Message
                };
            }
        }
    }
}