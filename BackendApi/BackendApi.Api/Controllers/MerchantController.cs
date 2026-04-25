using System.Security.Claims;
using BackendApi.Application.DTOs.Merchant;
using BackendApi.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BackendApi.Api.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/merchants")]
    public sealed class MerchantController : ControllerBase
    {
        private readonly IMerchantService _merchantService;

        public MerchantController(IMerchantService merchantService)
        {
            _merchantService = merchantService;
        }

        [HttpGet("{merchantId:guid}/inventory")]
        public async Task<ActionResult<MerchantInventoryResponseDto>> GetInventory(Guid merchantId)
        {
            if (!TryGetAppUserId(out var appUserId))
            {
                return Unauthorized("Invalid or missing user identifier.");
            }

            try
            {
                var result = await _merchantService.GetInventoryAsync(appUserId, merchantId);
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

        [HttpPost("{merchantId:guid}/cards/buy")]
        public async Task<ActionResult<BuyMerchantCardResponseDto>> BuyCard(
            Guid merchantId,
            [FromBody] BuyMerchantCardRequestDto request)
        {
            if (!TryGetAppUserId(out var appUserId))
            {
                return Unauthorized("Invalid or missing user identifier.");
            }

            var result = await _merchantService.BuyCardAsync(appUserId, merchantId, request.OfferId);

            if (!result.Success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }

        [HttpPost("{merchantId:guid}/gear/buy")]
        public async Task<ActionResult<BuyMerchantGearResponseDto>> BuyGear(
            Guid merchantId,
            [FromBody] BuyMerchantGearRequestDto request)
        {
            if (!TryGetAppUserId(out var appUserId))
            {
                return Unauthorized("Invalid or missing user identifier.");
            }

            var result = await _merchantService.BuyGearAsync(appUserId, merchantId, request.OfferId);

            if (!result.Success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }

        private bool TryGetAppUserId(out Guid appUserId)
        {
            appUserId = Guid.Empty;

            var appUserIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return Guid.TryParse(appUserIdClaim, out appUserId);
        }
    }
}