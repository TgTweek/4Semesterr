using BackendApi.Application.DTOs.Merchant;

namespace BackendApi.Application.Interfaces
{
    public interface IMerchantService
    {
        Task<IReadOnlyList<MerchantOfferResponseDto>> GetOffersAsync(Guid merchantId);
        Task<BuyMerchantCardResponseDto> BuyCardAsync(Guid appUserId, Guid merchantId, Guid offerId);
    }
}