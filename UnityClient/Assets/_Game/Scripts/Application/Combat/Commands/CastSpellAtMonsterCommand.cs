namespace Game.Application.Combat.Commands
{
    public sealed class CastSpellAtMonsterCommand
    {
        public string SpellName { get; set; } = string.Empty;
        public int ManaCost { get; set; }
        public int EffectValue { get; set; }
        public int Range { get; set; }
    }
}