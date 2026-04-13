using System;

namespace BackendApi.Application.Interfaces
{
    public interface IJwtTokenService
    {
        string CreateAccessToken(Guid appUserId, string email, Guid playerId, string playerName);
    }
}