using System;

namespace Game.Infrastructure.Api.Dtos.Run
{
    [Serializable]
    public sealed class CompleteRunRequestDto
    {
        public int goldEarned;
        public int experienceEarned;

        public string outcome = "";
        public string merchantId = "";
    }
}