using System;

namespace BackendApi.Domain.Entities
{
    public sealed class MonsterDefinition
    {
        public Guid MonsterDefinitionId { get; set; }

        public string MonsterKey { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;

        public int MaxHealth { get; set; }
        public int Damage { get; set; }
        public int Mana { get; set; }

        public int GoldReward { get; set; }
        public int ExperienceReward { get; set; }
        public bool IsBoss { get; set; }

    }
}