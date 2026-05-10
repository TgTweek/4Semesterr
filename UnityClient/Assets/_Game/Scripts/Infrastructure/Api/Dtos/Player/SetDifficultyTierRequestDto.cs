using System;

namespace Game.Infrastructure.Api.Dtos.Player
{
    [Serializable]
    public sealed class SetDifficultyTierRequestDto
    {
        public int difficultyTier;
    }
}