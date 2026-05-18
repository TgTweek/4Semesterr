using System;
using System.Text;
using System.Threading.Tasks;
using Game.Infrastructure.Api.Dtos.Player;
using Game.Infrastructure.Auth;
using UnityEngine;
using UnityEngine.Networking;

namespace Game.Infrastructure.Api.Player
{
    public sealed class PlayerApiGateway
    {
        private readonly string _baseUrl;
        private readonly AuthTokenStore _authTokenStore;

        public PlayerApiGateway(string baseUrl, AuthTokenStore authTokenStore)
        {
            _baseUrl = baseUrl.TrimEnd('/');
            _authTokenStore = authTokenStore;
        }

        public async Task<PlayerDto> GetCurrentPlayerAsync()
        {
            var playerId = _authTokenStore.GetPlayerId();

            if (string.IsNullOrWhiteSpace(playerId))
            {
                throw new InvalidOperationException("No player id found. Login first.");
            }

            var url = $"{_baseUrl}/api/player/{playerId}";

            using var request = UnityWebRequest.Get(url);
            ApplyHeaders(request);

            var operation = request.SendWebRequest();

            while (!operation.isDone)
            {
                await Task.Yield();
            }

            if (request.result != UnityWebRequest.Result.Success)
            {
                throw new InvalidOperationException(BuildErrorMessage(request));
            }

            return ParseResponse<PlayerDto>(request.downloadHandler.text);
        }

        public async Task<PlayerDto> SetDifficultyTierAsync(int difficultyTier)
        {
            var url = $"{_baseUrl}/api/player/difficulty-tier";

            var requestDto = new SetDifficultyTierRequestDto
            {
                difficultyTier = Math.Max(0, difficultyTier)
            };

            return await PostJsonAsync<PlayerDto, SetDifficultyTierRequestDto>(url, requestDto);
        }

        private async Task<TResponse> PostJsonAsync<TResponse, TRequest>(string url, TRequest requestDto)
        {
            var json = JsonUtility.ToJson(requestDto);
            var bodyRaw = Encoding.UTF8.GetBytes(json);

            using var request = new UnityWebRequest(url, UnityWebRequest.kHttpVerbPOST);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();

            ApplyHeaders(request);
            request.SetRequestHeader("Content-Type", "application/json");

            var operation = request.SendWebRequest();

            while (!operation.isDone)
            {
                await Task.Yield();
            }

            if (request.result != UnityWebRequest.Result.Success)
            {
                throw new InvalidOperationException(BuildErrorMessage(request));
            }

            return ParseResponse<TResponse>(request.downloadHandler.text);
        }

        private void ApplyHeaders(UnityWebRequest request)
        {
            request.SetRequestHeader("Accept", "application/json");

            var token = _authTokenStore.GetAccessToken();
            if (string.IsNullOrWhiteSpace(token))
            {
                throw new InvalidOperationException("No access token found. Login first.");
            }

            request.SetRequestHeader("Authorization", $"Bearer {token}");
        }

        private static TResponse ParseResponse<TResponse>(string json)
        {
            if (string.IsNullOrWhiteSpace(json))
            {
                throw new InvalidOperationException("Server returned an empty response.");
            }

            var dto = JsonUtility.FromJson<TResponse>(json);

            if (dto == null)
            {
                throw new InvalidOperationException("Failed to parse player response.");
            }

            return dto;
        }

        private static string BuildErrorMessage(UnityWebRequest request)
        {
            var body = request.downloadHandler?.text;

            if (!string.IsNullOrWhiteSpace(body))
            {
                return $"HTTP {(int)request.responseCode}: {body}";
            }

            if (!string.IsNullOrWhiteSpace(request.error))
            {
                return $"HTTP {(int)request.responseCode}: {request.error}";
            }

            return $"HTTP {(int)request.responseCode}: Request failed.";
        }
    }
}