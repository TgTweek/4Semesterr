using System;
using System.Collections.Generic;

namespace Game.Application.Movement.Results
{
    public sealed class FindPathResult
    {
        public bool IsSuccess { get; }
        public string Error { get; }
        public IReadOnlyList<PathPointDto> Path { get; }

        private FindPathResult(bool isSuccess, string error, IReadOnlyList<PathPointDto> path)
        {
            IsSuccess = isSuccess;
            Error = error;
            Path = path;
        }

        public static FindPathResult CreateSuccess(IReadOnlyList<PathPointDto> path)
        {
            return new FindPathResult(true, string.Empty, path);
        }

        public static FindPathResult CreateFailure(string error)
        {
            return new FindPathResult(false, error, Array.Empty<PathPointDto>());
        }
    }
}