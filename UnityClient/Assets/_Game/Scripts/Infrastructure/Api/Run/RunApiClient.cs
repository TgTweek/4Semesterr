using System;
using System.Text;
using System.Threading.Tasks;
using Game.Infrastructure.Api.Dtos.Run;
using Game.Infrastructure.Auth;
using UnityEngine;
using UnityEngine.Networking;

namespace Game.Infrastructure.Api.Run
{
    public sealed class RunApiClient
    {
        private readonly string _baseUrl;
        private readonly AuthTokenStore _authTokenStore;

        public RunApiClient(string baseUrl, AuthTokenStore authTokenStore)
        {
            _baseUrl = baseUrl.TrimEnd('/');
            _authTokenStore = authTokenStore;
        }

        public async Task<CompleteRunResponseDto> CompleteRunAsync(CompleteRunRequestDto requestDto)
        {
            var url = $"{_baseUrl}/api/run/complete";
            return await PostJsonAsync<CompleteRunResponseDto, CompleteRunRequestDto>(url, requestDto);
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
                throw new InvalidOperationException("No access token found. Login first.");
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