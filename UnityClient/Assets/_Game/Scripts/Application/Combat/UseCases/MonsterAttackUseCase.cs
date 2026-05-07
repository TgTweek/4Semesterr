using Game.Domain.Entities;
using Game.Domain.ValueObjects;

namespace Game.Application.Combat.UseCases
{
    public sealed class MonsterAttackUseCase
    {
        private readonly CheckTargetInRangeUseCase _checkTargetInRangeUseCase;

        public MonsterAttackUseCase(CheckTargetInRangeUseCase checkTargetInRangeUseCase)
        {
            _checkTargetInRangeUseCase = checkTargetInRangeUseCase;
        }

        public string Execute(
            PlayerRuntimeState player,
            MonsterRuntimeState monster,
            CellPosition monsterCell,
            CellPosition playerCell)
        {
            if (monster.IsDead)
            {
                return $"{monster.Name} is dead and skips its turn.";
            }

            var inRange = _checkTargetInRangeUseCase.Execute(monsterCell, playerCell, 1);
            if (!inRange)
            {
                return $"{monster.Name} is out of range and does nothing.";
            }

            player.TakeDamage(monster.Damage);
            return $"{monster.Name} hit the player for {monster.Damage}.";
        }
    }
}