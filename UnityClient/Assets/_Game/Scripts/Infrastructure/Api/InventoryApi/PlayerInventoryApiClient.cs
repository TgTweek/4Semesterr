using System;
using System.Threading.Tasks;
using Game.Infrastructure.Api.Dtos.Inventory;
using Game.Infrastructure.Auth;
using UnityEngine;
using UnityEngine.Networking;

namespace Game.Infrastructure.Api.Inventory
{
    public sealed class PlayerInventoryApiClient
    {
        private readonly string _baseUrl;
        private readonly AuthTokenStore _authTokenStore;

        public PlayerInventoryApiClient(string baseUrl, AuthTokenStore authTokenStore)
        {
            _baseUrl = baseUrl.TrimEnd('/');
            _authTokenStore = authTokenStore;
        }

        public async Task<PlayerInventoryResponseDto> GetInventoryAsync()
        {
            var url = $"{_baseUrl}/api/player/inventory";
            return await GetJsonAsync<PlayerInventoryResponseDto>(url);
        }

        public async Task<PlayerInventoryResponseDto> EquipCardAsync(string playerCardId)
        {
            var url = $"{_baseUrl}/api/player/inventory/cards/{playerCardId}/equip";
            return await PostEmptyAsync<PlayerInventoryResponseDto>(url);
        }

        public async Task<PlayerInventoryResponseDto> StashCardAsync(string playerCardId)
        {
            var url = $"{_baseUrl}/api/player/inventory/cards/{playerCardId}/stash";
            return await PostEmptyAsync<PlayerInventoryResponseDto>(url);
        }

        public async Task<PlayerInventoryResponseDto> EquipGearAsync(string playerGearId)
        {
            var url = $"{_baseUrl}/api/player/inventory/gear/{playerGearId}/equip";
            return await PostEmptyAsync<PlayerInventoryResponseDto>(url);
        }

        public async Task<PlayerInventoryResponseDto> StashGearAsync(string playerGearId)
        {
            var url = $"{_baseUrl}/api/player/inventory/gear/{playerGearId}/stash";
            return await PostEmptyAsync<PlayerInventoryResponseDto>(url);
        }

        private async Task<TResponse> GetJsonAsync<TResponse>(string url)
        {
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

            return ParseResponse<TResponse>(request.downloadHandler.text);
        }

        private async Task<TResponse> PostEmptyAsync<TResponse>(string url)
        {
            using var request = new UnityWebRequest(url, UnityWebRequest.kHttpVerbPOST);
            request.downloadHandler = new DownloadHandlerBuffer();

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
                throw new InvalidOperationException("Failed to parse server response.");
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