using System.Security.Claims;
using BackendApi.Application.DTOs.Player;
using BackendApi.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BackendApi.Api.Controllers
{
    [ApiController]
    [Route("api/player")]
    public sealed class PlayerController : ControllerBase
    {
        private readonly IPlayerService _playerService;

        public PlayerController(IPlayerService playerService)
        {
            _playerService = playerService;
        }

        [HttpGet("{playerId:guid}")]
        public async Task<IActionResult> GetById(Guid playerId)
        {
            var result = await _playerService.GetPlayerAsync(playerId);
            if (result is null)
            {
                return NotFound();
            }

            return Ok(result);
        }

        [Authorize]
        [HttpPost("difficulty-tier")]
        public async Task<ActionResult<PlayerDto>> SetDifficultyTier([FromBody] SetDifficultyTierRequestDto request)
        {
            if (!TryGetAppUserId(out var appUserId))
            {
                return Unauthorized("Invalid or missing user identifier.");
            }

            try
            {
                var result = await _playerService.SetDifficultyTierAsync(
                    appUserId,
                    request.DifficultyTier);

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