using System;
using System.Text;
using System.Threading.Tasks;
using Game.Infrastructure.Api.Dtos.Merchant;
using Game.Infrastructure.Auth;
using UnityEngine;
using UnityEngine.Networking;

namespace Game.Infrastructure.Api.Merchant
{
    public sealed class MerchantApiClient
    {
        private readonly string _baseUrl;
        private readonly AuthTokenStore _authTokenStore;

        public MerchantApiClient(string baseUrl, AuthTokenStore authTokenStore)
        {
            _baseUrl = baseUrl.TrimEnd('/');
            _authTokenStore = authTokenStore;
        }

        public async Task<MerchantInventoryResponseDto> GetInventoryAsync(string merchantId)
        {
            var url = $"{_baseUrl}/api/merchants/{merchantId}/inventory";
            return await GetJsonAsync<MerchantInventoryResponseDto>(url);
        }

        public async Task<BuyMerchantCardResponseDto> BuyCardAsync(string merchantId, string offerId)
        {
            var url = $"{_baseUrl}/api/merchants/{merchantId}/cards/buy";

            var requestDto = new BuyMerchantCardRequestDto
            {
                offerId = offerId
            };

            return await PostJsonAsync<BuyMerchantCardResponseDto, BuyMerchantCardRequestDto>(url, requestDto);
        }

        public async Task<BuyMerchantGearResponseDto> BuyGearAsync(string merchantId, string offerId)
        {
            var url = $"{_baseUrl}/api/merchants/{merchantId}/gear/buy";

            var requestDto = new BuyMerchantGearRequestDto
            {
                offerId = offerId
            };

            return await PostJsonAsync<BuyMerchantGearResponseDto, BuyMerchantGearRequestDto>(url, requestDto);
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

            var json = request.downloadHandler.text;

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

            var responseJson = request.downloadHandler.text;

            if (string.IsNullOrWhiteSpace(responseJson))
            {
                throw new InvalidOperationException("Server returned an empty response.");
            }

            var dto = JsonUtility.FromJson<TResponse>(responseJson);
            if (dto == null)
            {
                throw new InvalidOperationException("Failed to parse server response.");
            }

            return dto;
        }

        private void ApplyHeaders(UnityWebRequest request)
        {
            request.SetRequestHeader("Accept", "application/json");

            var token = _authTokenStore.GetAccessToken();
            if (string.IsNullOrWhiteSpace(token))
            {
                throw new InvalidOperationException("No access token found.");
            }

            request.SetRequestHeader("Authorization", $"Bearer {token}");
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