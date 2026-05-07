using System;

namespace Game.Domain.Entities
{
    public sealed class PlayerRuntimeState
    {
        public int Level { get; }

        public int MaxHealth { get; }
        public int CurrentHealth { get; private set; }

        public int MaxMana { get; }
        public int CurrentMana { get; private set; }

        public int DamageBonus { get; }

        public int MovementTilesPerTurn { get; }
        public int RemainingMovementTiles { get; private set; }

        public int Block { get; private set; }

        public bool IsDead => CurrentHealth <= 0;

        public PlayerRuntimeState(
            int level,
            int maxHealth,
            int maxMana,
            int damageBonus,
            int movementTilesPerTurn)
        {
            if (level <= 0) throw new ArgumentOutOfRangeException(nameof(level));
            if (maxHealth <= 0) throw new ArgumentOutOfRangeException(nameof(maxHealth));
            if (maxMana < 0) throw new ArgumentOutOfRangeException(nameof(maxMana));
            if (damageBonus < 0) throw new ArgumentOutOfRangeException(nameof(damageBonus));
            if (movementTilesPerTurn <= 0) throw new ArgumentOutOfRangeException(nameof(movementTilesPerTurn));

            Level = level;
            MaxHealth = maxHealth;
            CurrentHealth = maxHealth;

            MaxMana = maxMana;
            CurrentMana = maxMana;

            DamageBonus = damageBonus;

            MovementTilesPerTurn = movementTilesPerTurn;
            RemainingMovementTiles = movementTilesPerTurn;

            Block = 0;
        }

        public void StartTurn()
        {
            CurrentMana = MaxMana;
            RemainingMovementTiles = MovementTilesPerTurn;
        }

        public bool TrySpendMana(int manaCost)
        {
            if (manaCost < 0) return false;
            if (CurrentMana < manaCost) return false;

            CurrentMana -= manaCost;
            return true;
        }

        public bool TrySpendMovement(int movementCost)
        {
            if (movementCost <= 0) return true;
            if (RemainingMovementTiles < movementCost) return false;

            RemainingMovementTiles -= movementCost;
            return true;
        }

        public void GainBlock(int amount)
        {
            if (amount <= 0) return;
            Block += amount;
        }

        public void Heal(int amount)
        {
            if (amount <= 0) return;
            CurrentHealth = Math.Min(MaxHealth, CurrentHealth + amount);
        }

        public void TakeDamage(int amount)
        {
            if (amount <= 0) return;

            var absorbed = Math.Min(Block, amount);
            Block -= absorbed;

            var remaining = amount - absorbed;
            CurrentHealth = Math.Max(0, CurrentHealth - remaining);
        }
    }
}