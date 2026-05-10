using System;

namespace BackendApi.Application.DTOs.Player
{
    public sealed class PlayerDto
    {
        public Guid PlayerId { get; set; }
        public string PlayerName { get; set; } = string.Empty;

        public int Level { get; set; }
        public int MaxLevel { get; set; }
        public int Experience { get; set; }
        public int ExperienceRequiredForNextLevel { get; set; }

        public int DaluMoney { get; set; }

        public int DamageBonus { get; set; }
        public int BaseMaxHealth { get; set; }
        public int BaseMaxMana { get; set; }
        public int MovementTilesPerTurn { get; set; }
        public int DifficultyTier { get; set; }
        public int HighestDifficultyTierReached { get; set; }
        public int BossesDefeated { get; set; }
    }
}