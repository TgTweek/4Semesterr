using System.Collections.Generic;
using System.Linq;

namespace Game.Domain.Entities
{
    public sealed class BoardCombatState
    {
        public PlayerRuntimeState Player { get; }
        public IReadOnlyList<MonsterRuntimeState> Monsters => _monsters;

        private readonly List<MonsterRuntimeState> _monsters;

        public bool IsPlayerTurn { get; private set; }
        public int PendingRunGold { get; private set; }
        public int PendingRunExperience { get; private set; }

        public string LastLog { get; private set; } = string.Empty;

        public bool AreAllMonstersDead => _monsters.All(x => x.IsDead);
        public bool IsFinished => Player.IsDead || AreAllMonstersDead;

        public BoardCombatState(PlayerRuntimeState player, IEnumerable<MonsterRuntimeState> monsters)
        {
            Player = player;
            _monsters = monsters.ToList();
            IsPlayerTurn = true;
        }

        public void StartPlayerTurn()
        {
            IsPlayerTurn = true;
            Player.StartTurn();
            LastLog = "Player turn started.";
        }

        public void EndPlayerTurn()
        {
            IsPlayerTurn = false;
            LastLog = "Enemy turn started.";
        }

        public void SetLog(string log)
        {
            LastLog = log;
        }

        public void AddPendingRewards(MonsterRuntimeState monster)
        {
            if (!monster.IsDead) return;
            if (monster.RewardsClaimed) return;

            PendingRunGold += monster.GoldReward;
            PendingRunExperience += monster.ExperienceReward;
            monster.MarkRewardsClaimed();
        }
    }
}