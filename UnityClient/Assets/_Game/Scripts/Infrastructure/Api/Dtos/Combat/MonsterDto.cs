using System;

namespace Game.Infrastructure.Api.Dtos.Combat
{
    [Serializable]
    public sealed class MonsterDto
    {
        public string monsterKey = string.Empty;
        public string name = string.Empty;

        public int maxHealth;
        public int damage;
        public int mana;

        public int goldReward;
        public int experienceReward;
    }
}