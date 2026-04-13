using System;

namespace Game.Infrastructure.Api.Dtos
{
    [Serializable]
    public sealed class LoginRequestDto
    {
        public string email = "";
        public string password = "";
    }
}