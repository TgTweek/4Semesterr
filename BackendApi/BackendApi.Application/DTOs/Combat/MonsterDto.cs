namespace BackendApi.Application.DTOs.Combat
{
    public sealed class MonsterDto
    {
        public string MonsterKey { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;

        public int MaxHealth { get; set; }
        public int Damage { get; set; }
        public int Mana { get; set; }

        public int GoldReward { get; set; }
        public int ExperienceReward { get; set; }
    }
}