using System.Security.Claims;
using BackendApi.Application.DTOs.Inventory;
using BackendApi.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BackendApi.Api.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/player/inventory")]
    public sealed class PlayerInventoryController : ControllerBase
    {
        private readonly IPlayerInventoryService _playerInventoryService;

        public PlayerInventoryController(IPlayerInventoryService playerInventoryService)
        {
            _playerInventoryService = playerInventoryService;
        }

        [HttpGet]
        public async Task<ActionResult<PlayerInventoryResponseDto>> GetInventory()
        {
            if (!TryGetAppUserId(out var appUserId))
            {
                return Unauthorized("Invalid or missing user identifier.");
            }

            try
            {
                var result = await _playerInventoryService.GetInventoryAsync(appUserId);
                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }

        [HttpPost("cards/{playerCardId:guid}/equip")]
        public async Task<ActionResult<PlayerInventoryResponseDto>> EquipCard(Guid playerCardId)
        {
            if (!TryGetAppUserId(out var appUserId))
            {
                return Unauthorized("Invalid or missing user identifier.");
            }

            try
            {
                var result = await _playerInventoryService.EquipCardAsync(appUserId, playerCardId);
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

        [HttpPost("cards/{playerCardId:guid}/stash")]
        public async Task<ActionResult<PlayerInventoryResponseDto>> StashCard(Guid playerCardId)
        {
            if (!TryGetAppUserId(out var appUserId))
            {
                return Unauthorized("Invalid or missing user identifier.");
            }

            try
            {
                var result = await _playerInventoryService.StashCardAsync(appUserId, playerCardId);
                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }

        [HttpPost("gear/{playerGearId:guid}/equip")]
        public async Task<ActionResult<PlayerInventoryResponseDto>> EquipGear(Guid playerGearId)
        {
            if (!TryGetAppUserId(out var appUserId))
            {
                return Unauthorized("Invalid or missing user identifier.");
            }

            try
            {
                var result = await _playerInventoryService.EquipGearAsync(appUserId, playerGearId);
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

        [HttpPost("gear/{playerGearId:guid}/stash")]
        public async Task<ActionResult<PlayerInventoryResponseDto>> StashGear(Guid playerGearId)
        {
            if (!TryGetAppUserId(out var appUserId))
            {
                return Unauthorized("Invalid or missing user identifier.");
            }

            try
            {
                var result = await _playerInventoryService.StashGearAsync(appUserId, playerGearId);
                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
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