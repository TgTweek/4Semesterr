using BackendApi.Application.DTOs.Run;

namespace BackendApi.Application.Interfaces
{
    public interface IRunRewardService
    {
        Task<CompleteRunResponseDto> CompleteRunAsync(Guid appUserId, CompleteRunRequestDto request);
    }
}