using System;
using UnityEngine;

namespace Game.Infrastructure.Run
{
    public sealed class RunSessionStore
    {
        public const string OutcomeVictory = "Victory";
        public const string OutcomeDefeat = "Defeat";
        public const string OutcomeReturnedHome = "ReturnedHome";
        public const string OutcomeBossVictory = "BossVictory";

        private const string TotalGoldKey = "run.total_gold";
        private const string TotalExperienceKey = "run.total_experience";
        private const string BattlesWonKey = "run.battles_won";
        private const string LastOutcomeKey = "run.last_outcome";
        private const string CurrentFightIsBossKey = "run.current_fight_is_boss";

        public void StartNewRun()
        {
            PlayerPrefs.SetInt(TotalGoldKey, 0);
            PlayerPrefs.SetInt(TotalExperienceKey, 0);
            PlayerPrefs.SetInt(BattlesWonKey, 0);
            PlayerPrefs.SetString(LastOutcomeKey, string.Empty);
            PlayerPrefs.SetInt(CurrentFightIsBossKey, 0);
            PlayerPrefs.Save();
        }

        public void StartNormalFight()
        {
            PlayerPrefs.SetInt(CurrentFightIsBossKey, 0);
            PlayerPrefs.Save();
        }

        public void StartBossFight()
        {
            PlayerPrefs.SetInt(CurrentFightIsBossKey, 1);
            PlayerPrefs.Save();
        }

        public bool IsCurrentFightBoss()
        {
            return PlayerPrefs.GetInt(CurrentFightIsBossKey, 0) == 1;
        }

        public bool IsNextFightBoss()
        {
            var battlesWon = GetBattlesWon();
            return battlesWon > 0 && battlesWon % 9 == 0;
        }

        public void RegisterVictory(int goldEarned, int experienceEarned)
        {
            AddRewards(goldEarned, experienceEarned);

            var battlesWon = GetBattlesWon();
            PlayerPrefs.SetInt(BattlesWonKey, battlesWon + 1);

            var outcome = IsCurrentFightBoss()
                ? OutcomeBossVictory
                : OutcomeVictory;

            PlayerPrefs.SetString(LastOutcomeKey, outcome);
            PlayerPrefs.SetInt(CurrentFightIsBossKey, 0);

            PlayerPrefs.Save();
        }

        public void RegisterDefeat(int goldEarned, int experienceEarned)
        {
            AddRewards(goldEarned, experienceEarned);

            PlayerPrefs.SetString(LastOutcomeKey, OutcomeDefeat);
            PlayerPrefs.SetInt(CurrentFightIsBossKey, 0);
            PlayerPrefs.Save();
        }

        public int GetTotalGold()
        {
            return PlayerPrefs.GetInt(TotalGoldKey, 0);
        }

        public int GetTotalExperience()
        {
            return PlayerPrefs.GetInt(TotalExperienceKey, 0);
        }

        public int GetBattlesWon()
        {
            return PlayerPrefs.GetInt(BattlesWonKey, 0);
        }

        public string GetLastOutcome()
        {
            return PlayerPrefs.GetString(LastOutcomeKey, string.Empty);
        }

        public bool CanReturnHome()
        {
            var battlesWon = GetBattlesWon();
            return battlesWon > 0 && battlesWon % 3 == 0;
        }

        public void Clear()
        {
            PlayerPrefs.DeleteKey(TotalGoldKey);
            PlayerPrefs.DeleteKey(TotalExperienceKey);
            PlayerPrefs.DeleteKey(BattlesWonKey);
            PlayerPrefs.DeleteKey(LastOutcomeKey);
            PlayerPrefs.DeleteKey(CurrentFightIsBossKey);
            PlayerPrefs.Save();
        }

        private void AddRewards(int goldEarned, int experienceEarned)
        {
            var safeGold = Math.Max(0, goldEarned);
            var safeExperience = Math.Max(0, experienceEarned);

            PlayerPrefs.SetInt(TotalGoldKey, GetTotalGold() + safeGold);
            PlayerPrefs.SetInt(TotalExperienceKey, GetTotalExperience() + safeExperience);
        }
    }
}