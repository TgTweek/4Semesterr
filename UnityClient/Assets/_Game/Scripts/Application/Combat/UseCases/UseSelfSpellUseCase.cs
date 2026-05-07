using Game.Domain.Entities;

namespace Game.Application.Combat.UseCases
{
    public sealed class UseSelfSpellUseCase
    {
        public bool Execute(
            BoardCombatState combatState,
            string spellName,
            int manaCost,
            string effectType,
            int effectValue)
        {
            if (combatState.IsFinished) return false;
            if (!combatState.IsPlayerTurn) return false;

            if (!combatState.Player.TrySpendMana(manaCost))
            {
                combatState.SetLog("Not enough mana.");
                return false;
            }

            switch (effectType)
            {
                case "Block":
                    combatState.Player.GainBlock(effectValue);
                    combatState.SetLog($"{spellName} gave {effectValue} block.");
                    return true;

                case "Heal":
                    combatState.Player.Heal(effectValue);
                    combatState.SetLog($"{spellName} healed {effectValue} health.");
                    return true;

                default:
                    combatState.SetLog($"Unsupported self spell effect: {effectType}");
                    return false;
            }
        }
    }
}