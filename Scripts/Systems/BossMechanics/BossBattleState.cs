using System;

namespace ClawRPG.Scripts.Systems.BossMechanics
{
    /// <summary>
    /// Boss 战斗状态快照 - 用于 AI/技能/阶段决策
    /// </summary>
    public class BossBattleState
    {
        public string BossId { get; set; }
        public float CurrentHealth { get; set; }
        public float MaxHealth { get; set; }
        public int CurrentPhase { get; set; }
        public bool PhaseChanged { get; set; }
        public bool IsEnraged { get; set; }
        public float BattleTime { get; set; }
        public int ActiveMinionCount { get; set; }
        public bool IsRageTriggered { get; set; }

        public BossBattleState()
        {
            PhaseChanged = false;
            IsEnraged = false;
            IsRageTriggered = false;
            CurrentPhase = 1;
            BattleTime = 0f;
            ActiveMinionCount = 0;
        }
    }
}
