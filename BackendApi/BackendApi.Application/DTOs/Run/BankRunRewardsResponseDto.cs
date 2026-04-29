namespace BackendApi.Application.DTOs.Run
{
    public sealed class BankRunRewardsResponseDto
    {
        public int GoldEarned { get; set; }
        public int ExperienceEarned { get; set; }

        public int TotalGold { get; set; }

        public int NewLevel { get; set; }
        public int TotalExperience { get; set; }
        public int ExperienceRequiredForNextLevel { get; set; }

        public int DamageBonus { get; set; }
        public int BaseMaxHealth { get; set; } 
        public int BaseMaxMana { get; set; }
    }
}