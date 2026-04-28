using System;

namespace BackendApi.Application.DTOs.Auth
{
    public sealed class AuthResponseDto
    {
        public string AccessToken { get; set; } = string.Empty;
        public DateTime ExpiresAtUtc { get; set; }
        public Guid PlayerId { get; set; }
        public string PlayerName { get; set; } = string.Empty;
    }
}