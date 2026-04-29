using BackendApi.Application.DTOs.Run;
using BackendApi.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace BackendApi.Api.Controllers
{
    [ApiController]
    [Route("api/run")]
    public sealed class RunController : ControllerBase
    {
        private readonly IRunRewardService _runRewardService;

        public RunController(IRunRewardService runRewardService)
        {
            _runRewardService = runRewardService;
        }

        [HttpPost("bank-rewards/{playerId:guid}")]
        public async Task<IActionResult> BankRewards(Guid playerId, [FromBody] BankRunRewardsRequestDto request)
        {
            var result = await _runRewardService.BankRewardsAsync(playerId, request);
            return Ok(result);
        }
    }
}