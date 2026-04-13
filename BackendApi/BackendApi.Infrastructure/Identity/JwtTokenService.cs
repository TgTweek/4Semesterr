using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using BackendApi.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using BackendApi.Infrastructure.Identity;

namespace BackendApi.Infrastructure.Identity
{
    public sealed class JwtTokenService : IJwtTokenService
    {
        private readonly IConfiguration _configuration;

        public JwtTokenService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public string CreateAccessToken(Guid appUserId, string email, Guid playerId, string playerName)
        {
            var issuer = _configuration["Jwt:Issuer"]
                ?? throw new InvalidOperationException("Jwt:Issuer not configured.");

            var audience = _configuration["Jwt:Audience"]
                ?? throw new InvalidOperationException("Jwt:Audience not configured.");

            var key = _configuration["Jwt:Key"]
                ?? throw new InvalidOperationException("Jwt:Key not configured.");

            var accessTokenMinutesString = _configuration["Jwt:AccessTokenMinutes"]
                ?? throw new InvalidOperationException("Jwt:AccessTokenMinutes not configured.");

            if (!int.TryParse(accessTokenMinutesString, out var accessTokenMinutes))
            {
                throw new InvalidOperationException("Jwt:AccessTokenMinutes must be a valid integer.");
            }

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, appUserId.ToString()),
                new Claim("player_id", playerId.ToString()),
                new Claim("player_name", playerName),
                new Claim(ClaimTypes.Email, email ?? string.Empty)
            };

            var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
            var credentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(accessTokenMinutes),
                signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}