using System;
using System.Text;
using System.Threading.Tasks;
using Game.Infrastructure.Api.Dtos;
using UnityEngine;
using UnityEngine.Networking;

namespace Game.Infrastructure.Api.Auth
{
    public sealed class AuthApiClient
    {
        private readonly string _baseUrl;

        public AuthApiClient(string baseUrl)
        {
            _baseUrl = baseUrl.TrimEnd('/');
        }

        public async Task<AuthResponseDto> LoginAsync(LoginRequestDto requestDto)
        {
            var url = $"{_baseUrl}/api/auth/login";
            return await PostJsonAsync<AuthResponseDto, LoginRequestDto>(url, requestDto);
        }

        public async Task<AuthResponseDto> RegisterAsync(RegisterRequestDto requestDto)
        {
            var url = $"{_baseUrl}/api/auth/register";
            return await PostJsonAsync<AuthResponseDto, RegisterRequestDto>(url, requestDto);
        }

        private static async Task<TResponse> PostJsonAsync<TResponse, TRequest>(string url, TRequest requestDto)
        {
            var json = JsonUtility.ToJson(requestDto);
            var bodyRaw = Encoding.UTF8.GetBytes(json);

            using var request = new UnityWebRequest(url, UnityWebRequest.kHttpVerbPOST);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();

            request.SetRequestHeader("Content-Type", "application/json");
            request.SetRequestHeader("Accept", "application/json");

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

            var responseDto = JsonUtility.FromJson<TResponse>(responseJson);

            if (responseDto == null)
            {
                throw new InvalidOperationException("Failed to parse server response.");
            }

            return responseDto;
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