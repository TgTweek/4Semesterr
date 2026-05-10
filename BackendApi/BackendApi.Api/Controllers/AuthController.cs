using BackendApi.Application.DTOs.Auth;
using BackendApi.Application.Interfaces;
using BackendApi.Domain.Entities;
using BackendApi.Infrastructure.Identity;
using BackendApi.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BackendApi.Api.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public sealed class AuthController : ControllerBase
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly GameDbContext _dbContext;
        private readonly IJwtTokenService _jwtTokenService;
        private readonly IStarterInventoryService _starterInventoryService;

        public AuthController(
            UserManager<AppUser> userManager,
            GameDbContext dbContext,
            IJwtTokenService jwtTokenService,
            IStarterInventoryService starterInventoryService)
        {
            _userManager = userManager;
            _dbContext = dbContext;
            _jwtTokenService = jwtTokenService;
            _starterInventoryService = starterInventoryService;
        }

        [HttpPost("register")]
        public async Task<ActionResult<AuthResponseDto>> Register([FromBody] RegisterRequestDto request)
        {
            var existingUser = await _userManager.FindByEmailAsync(request.Email);
            if (existingUser is not null)
            {
                return BadRequest("Email is already in use.");
            }

            var appUser = new AppUser
            {
                Id = Guid.NewGuid(),
                UserName = request.Email,
                Email = request.Email,
                EmailConfirmed = true
            };

            var createResult = await _userManager.CreateAsync(appUser, request.Password);

            if (!createResult.Succeeded)
            {
                var errors = createResult.Errors.Select(x => x.Description).ToList();
                return BadRequest(errors);
            }

            var player = new Player
            {
                PlayerId = Guid.NewGuid(),
                AppUserId = appUser.Id,
                PlayerName = request.PlayerName,
                Level = 1,
                DaluMoney = 200,
                Experience = 0,
                DamageBonus = 0,
                BaseMaxHealth = 30,
                BaseMaxMana = 3,
                DifficultyTier = 0,
                HighestDifficultyTierReached = 0,
                BossesDefeated = 0
            };

            _dbContext.Players.Add(player);
            await _dbContext.SaveChangesAsync();

            await _starterInventoryService.GrantStarterCardsAsync(player.PlayerId);

            var token = _jwtTokenService.CreateAccessToken(
                appUser.Id,
                appUser.Email ?? string.Empty,
                player.PlayerId,
                player.PlayerName);

            return Ok(new AuthResponseDto
            {
                AccessToken = token,
                ExpiresAtUtc = DateTime.UtcNow.AddMinutes(60),
                PlayerId = player.PlayerId,
                PlayerName = player.PlayerName
            });
        }

        [HttpPost("login")]
        public async Task<ActionResult<AuthResponseDto>> Login([FromBody] LoginRequestDto request)
        {
            var appUser = await _userManager.FindByEmailAsync(request.Email);
            if (appUser is null)
            {
                return Unauthorized("Invalid email or password.");
            }

            var passwordValid = await _userManager.CheckPasswordAsync(appUser, request.Password);
            if (!passwordValid)
            {
                return Unauthorized("Invalid email or password.");
            }

            var player = await _dbContext.Players
                .FirstOrDefaultAsync(x => x.AppUserId == appUser.Id);

            if (player is null)
            {
                return BadRequest("Player profile not found.");
            }

            var token = _jwtTokenService.CreateAccessToken(
                appUser.Id,
                appUser.Email ?? string.Empty,
                player.PlayerId,
                player.PlayerName);

            return Ok(new AuthResponseDto
            {
                AccessToken = token,
                ExpiresAtUtc = DateTime.UtcNow.AddMinutes(60),
                PlayerId = player.PlayerId,
                PlayerName = player.PlayerName
            });
        }
    }
}