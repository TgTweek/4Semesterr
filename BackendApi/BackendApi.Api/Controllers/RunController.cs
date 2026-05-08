using System.Security.Claims;
using BackendApi.Application.DTOs.Run;
using BackendApi.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BackendApi.Api.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/run")]
    public sealed class RunController : ControllerBase
    {
        private readonly IRunRewardService _runRewardService;

        public RunController(IRunRewardService runRewardService)
        {
            _runRewardService = runRewardService;
        }

        [HttpPost("complete")]
        public async Task<ActionResult<CompleteRunResponseDto>> CompleteRun([FromBody] CompleteRunRequestDto request)
        {
            if (!TryGetAppUserId(out var appUserId))
            {
                return Unauthorized("Invalid or missing user identifier.");
            }

            try
            {
                var result = await _runRewardService.CompleteRunAsync(appUserId, request);
                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        private bool TryGetAppUserId(out Guid appUserId)
        {
            appUserId = Guid.Empty;

            var appUserIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return Guid.TryParse(appUserIdClaim, out appUserId);
        }
    }
}