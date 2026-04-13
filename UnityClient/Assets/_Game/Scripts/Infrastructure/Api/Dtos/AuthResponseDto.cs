using System;

namespace Game.Infrastructure.Api.Dtos
{
    [Serializable]
    public sealed class AuthResponseDto
    {
        public string accessToken = "";
        public string expiresAtUtc = "";
        public string playerId = "";
        public string playerName = "";
    }
}