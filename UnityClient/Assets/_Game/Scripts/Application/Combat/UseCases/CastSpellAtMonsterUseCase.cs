using Game.Application.Combat.Commands;
using Game.Domain.Entities;
using Game.Domain.ValueObjects;

namespace Game.Application.Combat.UseCases
{
    public sealed class CastSpellAtMonsterUseCase
    {
        private readonly CheckTargetInRangeUseCase _checkTargetInRangeUseCase;

        public CastSpellAtMonsterUseCase(CheckTargetInRangeUseCase checkTargetInRangeUseCase)
        {
            _checkTargetInRangeUseCase = checkTargetInRangeUseCase;
        }

        public bool Execute(
            BoardCombatState combatState,
            MonsterRuntimeState targetMonster,
            CellPosition playerCell,
            CellPosition targetCell,
            CastSpellAtMonsterCommand command)
        {
            if (combatState.IsFinished) return false;
            if (!combatState.IsPlayerTurn) return false;
            if (targetMonster.IsDead) return false;

            var inRange = _checkTargetInRangeUseCase.Execute(playerCell, targetCell, command.Range);
            if (!inRange)
            {
                combatState.SetLog($"{command.SpellName} is out of range.");
                return false;
            }

            if (!combatState.Player.TrySpendMana(command.ManaCost))
            {
                combatState.SetLog("Not enough mana.");
                return false;
            }

            var finalDamage = command.EffectValue + combatState.Player.DamageBonus;
            targetMonster.TakeDamage(finalDamage);

            if (targetMonster.IsDead)
            {
                combatState.AddPendingRewards(targetMonster);
                combatState.SetLog($"{command.SpellName} killed {targetMonster.Name} for {finalDamage} damage.");
            }
            else
            {
                combatState.SetLog($"{command.SpellName} hit {targetMonster.Name} for {finalDamage} damage.");
            }

            return true;
        }
    }
}