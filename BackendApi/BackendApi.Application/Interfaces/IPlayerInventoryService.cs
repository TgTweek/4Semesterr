using BackendApi.Application.DTOs.Inventory;

namespace BackendApi.Application.Interfaces
{
    public interface IPlayerInventoryService
    {
        Task<PlayerInventoryResponseDto> GetInventoryAsync(Guid appUserId);

        Task<PlayerInventoryResponseDto> EquipCardAsync(Guid appUserId, Guid playerCardId);
        Task<PlayerInventoryResponseDto> StashCardAsync(Guid appUserId, Guid playerCardId);

        Task<PlayerInventoryResponseDto> EquipGearAsync(Guid appUserId, Guid playerGearId);
        Task<PlayerInventoryResponseDto> StashGearAsync(Guid appUserId, Guid playerGearId);
    }
}