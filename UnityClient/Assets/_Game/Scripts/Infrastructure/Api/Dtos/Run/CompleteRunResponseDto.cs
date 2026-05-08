using System;

namespace Game.Infrastructure.Api.Dtos.Run
{
    [Serializable]
    public sealed class CompleteRunResponseDto
    {
        public string outcome = "";

        public int goldEarned;
        public int experienceEarned;

        public int totalGold;

        public int newLevel;
        public int maxLevel;
        public int totalExperience;
        public int experienceRequiredForNextLevel;

        public int damageBonus;
        public int baseMaxHealth;
        public int baseMaxMana;
        public int movementTilesPerTurn;

        public int lostCardsCount;
        public bool shopRefreshed;

        public string message = "";
    }
}