using BackendApi.Application.DTOs.Merchant;

namespace BackendApi.Application.Interfaces
{
    public interface IMerchantService
    {
        Task<MerchantInventoryResponseDto> GetInventoryAsync(Guid appUserId, Guid merchantId);
        Task<BuyMerchantCardResponseDto> BuyCardAsync(Guid appUserId, Guid merchantId, Guid offerId);
        Task<BuyMerchantGearResponseDto> BuyGearAsync(Guid appUserId, Guid merchantId, Guid offerId);

        // Bruges senere af run-complete flow.
        Task RefreshInventoryAsync(Guid appUserId, Guid merchantId);
    }
}