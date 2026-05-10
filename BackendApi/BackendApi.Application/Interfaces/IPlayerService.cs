using BackendApi.Application.DTOs.Player;

namespace BackendApi.Application.Interfaces
{
    public interface IPlayerService
    {
        Task<PlayerDto?> GetPlayerAsync(Guid playerId);
        Task<PlayerDto> SetDifficultyTierAsync(Guid appUserId, int difficultyTier);
    }
}