using BackendApi.Application.DTOs.Player;

namespace BackendApi.Application.Interfaces
{
    public interface IPlayerService
    {
        Task<PlayerDto?> GetPlayerAsync(Guid playerId);
    }
}