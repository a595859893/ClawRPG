using Godot;
using System;
using System.Collections.Generic;
using ClawRPG.Scripts.Database;

namespace ClawRPG.Scripts.Data
{
    /// <summary>
    /// Pet combat companion data - tracks combat coordination between pet and player
    /// </summary>
    public class PetCombatCompanionData
    {
        public Dictionary<string, PetCompanionState> PetStates { get; set; } = new Dictionary<string, PetCompanionState>();
        public Dictionary<string, List<CombatComboRecord>> ComboHistory { get; set; } = new Dictionary<string, List<CombatComboRecord>>();
        public Dictionary<string, PetLearningData> LearningData { get; set; } = new Dictionary<string, PetLearningData>();
        public int TotalCombos { get; set; }
        public float TotalComboDamage { get; set; }
        public int HighestComboChain { get; set; }

        // Active companion tracking (REQ-136)
        public string ActivePetId { get; set; } = "";
        public string CurrentRole { get; set; } = "Attacker";
        public float SyncLevel { get; set; } = 0.5f;
        public int ComboCount { get; set; }
        public int MaxComboCount { get; set; }
        public int TotalAttacksAssisted { get; set; }
        public int TotalDamageDealt { get; set; }
        public int TotalEnemiesDefeated { get; set; }
        public List<string> LearnedSkills { get; set; } = new List<string>();

        // Pet obituary data (REQ-191): keyed by petId
        public Dictionary<string, PetObituaryData> ObituaryData { get; set; } = new Dictionary<string, PetObituaryData>();
    }

    public class PetCompanionState
    {
        public string PetId { get; set; }
        public string CurrentRole { get; set; } = "Attacker"; // Attacker/Support/Tank/Scout
        public int ComboChain { get; set; }
        public float LastAttackTime { get; set; }
        public float ComboWindow { get; set; } = 2.0f;
        public bool IsInCombo { get; set; }
        public string LastPlayerAction { get; set; }
        public Vector2 LastPlayerPosition { get; set; }
        public float SyncLevel { get; set; } = 0.5f; // 0-1, how well pet coordinates with player
    }

    public class CombatComboRecord
    {
        public string PetId { get; set; }
        public string ComboType { get; set; }
        public float Damage { get; set; }
        public float Duration { get; set; }
        public int HitCount { get; set; }
        public DateTime Timestamp { get; set; }
    }

    public partial class PetLearningData
    {
        public string PetId { get; set; }
        public Dictionary<string, int> EnemyTypeKills { get; set; } = new Dictionary<string, int>();
        public Dictionary<string, float> PlayerAttackPattern { get; set; } = new Dictionary<string, float>(); // Time between attacks
        public float AveragePlayerAttackInterval { get; set; } = 1.0f;
        public int SuccessfulDodges { get; set; }
        public int FailedDodges { get; set; }
        public float DodgeSuccessRate { get; set; }
        public List<string> PreferredBehaviors { get; set; } = new List<string>();
        public float AdaptationLevel { get; set; } = 0f; // 0-100
        public DateTime LastLearningUpdate { get; set; }
    }

    public enum PetCompanionRole
    {
        Attacker,
        Support,
        Tank,
        Scout
    }

    public enum ComboType
    {
        Basic,
        Chain,
        Counter,
        Support,
        Ultimate
    }

    /// <summary>
    /// 宠物讣告数据 — 记录宠物一生，供死亡时生成叙事讣告（REQ-191）
    /// </summary>
    public class PetObituaryData
    {
        // 参战统计
        public int TotalBattles { get; set; }           // 总参战场次
        public string MostUsedCombo { get; set; }        // 最常用的 combo 起手（如 "heavy→slash"）
        public int MostUsedComboCount { get; set; }     // 该 combo 使用次数
        public int TotalEnemiesKilled { get; set; }      // 总击杀数（复用现有数据）
        public double TotalBattleTimeSeconds { get; set; } // 累计战斗时长（秒）
        public int PeakStreak { get; set; }              // 最佳连胜记录
        public long FirstBattleTimestamp { get; set; }   // 第一次参战时间戳
        public long LastBattleTimestamp { get; set; }    // 最后一次参战时间戳

        // 本次战斗内实时数据（不持久化）
        public string CurrentBattleFirstCombo { get; set; } // 本场战斗第一个 combo 起手
        public double CurrentBattleStartTime { get; set; } // 本场战斗开始时间

        /// <summary>
        /// 战斗开始时调用，重置本次战斗数据
        /// </summary>
        public void OnBattleStarted()
        {
            CurrentBattleFirstCombo = "";
            CurrentBattleStartTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        }

        /// <summary>
        /// 战斗结束时调用，结算本次战斗数据
        /// </summary>
        public void OnBattleEnded(string firstCombo)
        {
            if (string.IsNullOrEmpty(firstCombo))
                firstCombo = "basic_attack";

            // 记录第一个 combo
            if (string.IsNullOrEmpty(CurrentBattleFirstCombo))
                CurrentBattleFirstCombo = firstCombo;

            TotalBattles++;

            // 更新最常用 combo
            if (!string.IsNullOrEmpty(CurrentBattleFirstCombo))
            {
                if (CurrentBattleFirstCombo == MostUsedCombo)
                {
                    MostUsedComboCount++;
                }
                else if (MostUsedComboCount == 0 || 
                         CurrentBattleFirstCombo != MostUsedCombo)
                {
                    // 第一次记录，或者被新的 combo 超过
                    if (string.IsNullOrEmpty(MostUsedCombo) || 
                        (CurrentBattleFirstCombo != MostUsedCombo && 
                         GetComboFrequency(CurrentBattleFirstCombo) > GetComboFrequency(MostUsedCombo)))
                    {
                        MostUsedCombo = CurrentBattleFirstCombo;
                        MostUsedComboCount = 1;
                    }
                }
            }

            // 更新时长
            if (CurrentBattleStartTime > 0)
            {
                double elapsed = DateTimeOffset.UtcNow.ToUnixTimeSeconds() - CurrentBattleStartTime;
                TotalBattleTimeSeconds += elapsed;
            }

            // 更新时间戳
            long now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            if (FirstBattleTimestamp == 0)
                FirstBattleTimestamp = now;
            LastBattleTimestamp = now;

            // 重置本次战斗数据
            CurrentBattleFirstCombo = "";
            CurrentBattleStartTime = 0;
        }

        private int GetComboFrequency(string combo)
        {
            // 估算频率（实际使用 MostUsedComboCount）
            return MostUsedCombo == combo ? MostUsedComboCount : 0;
        }
    }
}
