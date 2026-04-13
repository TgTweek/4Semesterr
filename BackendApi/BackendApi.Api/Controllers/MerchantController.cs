using BackendApi.Application.DTOs.Merchant;
using BackendApi.Application.Interfaces;
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
    }
}