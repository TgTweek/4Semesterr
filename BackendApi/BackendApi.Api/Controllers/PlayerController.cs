using BackendApi.Application.Interfaces;
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
                return NotFound();

            return Ok(result);
        }
    }
}