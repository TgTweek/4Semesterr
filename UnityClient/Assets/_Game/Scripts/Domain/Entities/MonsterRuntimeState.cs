using System;

namespace Game.Domain.Entities
{
    public sealed class MonsterRuntimeState
    {
        public string MonsterKey { get; }
        public string Name { get; }

        public int MaxHealth { get; }
        public int CurrentHealth { get; private set; }

        public int Damage { get; }
        public int Mana { get; }

        public int GoldReward { get; }
        public int ExperienceReward { get; }

        public bool RewardsClaimed { get; private set; }
        public bool IsDead => CurrentHealth <= 0;

        public MonsterRuntimeState(
            string monsterKey,
            string name,
            int maxHealth,
            int damage,
            int mana,
            int goldReward,
            int experienceReward)
        {
            if (string.IsNullOrWhiteSpace(monsterKey)) throw new ArgumentException("Monster key is required.", nameof(monsterKey));
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Monster name is required.", nameof(name));
            if (maxHealth <= 0) throw new ArgumentOutOfRangeException(nameof(maxHealth));
            if (damage < 0) throw new ArgumentOutOfRangeException(nameof(damage));
            if (mana < 0) throw new ArgumentOutOfRangeException(nameof(mana));
            if (goldReward < 0) throw new ArgumentOutOfRangeException(nameof(goldReward));
            if (experienceReward < 0) throw new ArgumentOutOfRangeException(nameof(experienceReward));

            MonsterKey = monsterKey;
            Name = name;
            MaxHealth = maxHealth;
            CurrentHealth = maxHealth;
            Damage = damage;
            Mana = mana;
            GoldReward = goldReward;
            ExperienceReward = experienceReward;
        }

        public void TakeDamage(int amount)
        {
            if (amount <= 0) return;
            CurrentHealth = Math.Max(0, CurrentHealth - amount);
        }

        public void MarkRewardsClaimed()
        {
            RewardsClaimed = true;
        }
    }
}