using System;

namespace Game.Infrastructure.Api.Dtos.Player
{
    [Serializable]
    public sealed class PlayerDto
    {
        public string playerId = string.Empty;
        public string playerName = string.Empty;

        public int level;
        public int maxLevel;
        public int experience;
        public int experienceRequiredForNextLevel;

        public int daluMoney;

        public int damageBonus;
        public int baseMaxHealth;
        public int baseMaxMana;
        public int movementTilesPerTurn;
    }
}