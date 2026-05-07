namespace BackendApi.Domain.Services
{
    public static class PlayerProgressionRules
    {
        public const int MaxLevel = 20;

        public static int GetRequiredExperienceForNextLevel(int currentLevel)
        {
            if (currentLevel >= MaxLevel)
            {
                return 0;
            }

            var oldXp = 50 + ((currentLevel - 1) * 25);
            return (int)Math.Ceiling(oldXp * 1.5);
        }

        public static int GetDamageBonus(int level)
        {
            var safeLevel = Math.Clamp(level, 1, MaxLevel);
            return safeLevel / 2;
        }

        public static int GetBaseMaxHealth(int level)
        {
            var safeLevel = Math.Clamp(level, 1, MaxLevel);
            return 30 + ((safeLevel - 1) * 4);
        }

        public static int GetBaseMaxMana(int level)
        {
            var safeLevel = Math.Clamp(level, 1, MaxLevel);
            return 3 + (safeLevel / 4);
        }

        public static int GetMovementTilesPerTurn(int level)
        {
            var safeLevel = Math.Clamp(level, 1, MaxLevel);
            return 4 + (safeLevel / 3);
        }
    }
}