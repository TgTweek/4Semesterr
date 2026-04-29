using BackendApi.Application.DTOs.Run;

namespace BackendApi.Application.Interfaces
{
    public interface IRunRewardService
    {
        Task<BankRunRewardsResponseDto> BankRewardsAsync(Guid playerId, BankRunRewardsRequestDto request);
    }
}