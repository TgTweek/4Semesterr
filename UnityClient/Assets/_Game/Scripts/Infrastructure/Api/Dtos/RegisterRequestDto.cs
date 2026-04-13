using System;

namespace Game.Infrastructure.Api.Dtos
{
    [Serializable]
    public sealed class RegisterRequestDto
    {
        public string email = "";
        public string password = "";
        public string playerName = "";
    }
}