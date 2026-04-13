using System.Security.Claims;
using BackendApi.Application.DTOs.Merchant;
using BackendApi.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BackendApi.Api.Controllers
{
    [ApiController]
    [Route("api/merchants")]
    public sealed class MerchantController : ControllerBase
    {
        private readonly IMerchantService _merchantService;

        public MerchantController(IMerchantService merchantService)
        {
            _merchantService = merchantService;
        }

        [HttpGet("{merchantId:guid}/offers")]
        public async Task<ActionResult<IReadOnlyList<MerchantOfferResponseDto>>> GetOffers(Guid merchantId)
        {
            var offers = await _merchantService.GetOffersAsync(merchantId);
            return Ok(offers);
        }

        [Authorize]
        [HttpPost("{merchantId:guid}/buy")]
        public async Task<ActionResult<BuyMerchantCardResponseDto>> Buy(
            Guid merchantId,
            [FromBody] BuyMerchantCardRequestDto request)
        {
            var appUserIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (!Guid.TryParse(appUserIdClaim, out var appUserId))
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
    }
}