using Game.Infrastructure.Api.Dtos;
using UnityEngine;

namespace Game.Infrastructure.Auth
{
    public sealed class AuthTokenStore
    {
        private const string AccessTokenKey = "auth.access_token";
        private const string PlayerIdKey = "auth.player_id";
        private const string PlayerNameKey = "auth.player_name";

        public void Save(AuthResponseDto response)
        {
            PlayerPrefs.SetString(AccessTokenKey, response.accessToken ?? "");
            PlayerPrefs.SetString(PlayerIdKey, response.playerId ?? "");
            PlayerPrefs.SetString(PlayerNameKey, response.playerName ?? "");
            PlayerPrefs.Save();
        }

        public string GetAccessToken()
        {
            return PlayerPrefs.GetString(AccessTokenKey, "");
        }

        public string GetPlayerId()
        {
            return PlayerPrefs.GetString(PlayerIdKey, "");
        }

        public string GetPlayerName()
        {
            return PlayerPrefs.GetString(PlayerNameKey, "");
        }

        public bool HasAccessToken()
        {
            return !string.IsNullOrWhiteSpace(GetAccessToken());
        }

        public void Clear()
        {
            PlayerPrefs.DeleteKey(AccessTokenKey);
            PlayerPrefs.DeleteKey(PlayerIdKey);
            PlayerPrefs.DeleteKey(PlayerNameKey);
            PlayerPrefs.Save();
        }
    }
}