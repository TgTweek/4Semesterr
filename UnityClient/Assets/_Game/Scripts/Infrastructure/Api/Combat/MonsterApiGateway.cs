using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Game.Infrastructure.Api.Dtos.Combat;
using UnityEngine;
using UnityEngine.Networking;

namespace Game.Infrastructure.Api.Combat
{
    public sealed class MonsterApiGateway
    {
        [Serializable]
        private sealed class MonsterDtoArrayWrapper
        {
            public MonsterDto[] items = Array.Empty<MonsterDto>();
        }

        private readonly string _baseUrl;

        public MonsterApiGateway(string baseUrl)
        {
            _baseUrl = baseUrl.TrimEnd('/');
        }

        public async Task<List<MonsterDto>> GetAllMonstersAsync()
        {
            var url = $"{_baseUrl}/api/monsters";

            using var request = UnityWebRequest.Get(url);
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

            var json = request.downloadHandler.text;

            if (string.IsNullOrWhiteSpace(json))
            {
                throw new InvalidOperationException("Server returned an empty monster list.");
            }

            var wrappedJson = $"{{\"items\":{json}}}";
            var wrapper = JsonUtility.FromJson<MonsterDtoArrayWrapper>(wrappedJson);

            if (wrapper == null || wrapper.items == null)
            {
                throw new InvalidOperationException("Failed to parse monster list.");
            }

            return new List<MonsterDto>(wrapper.items);
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