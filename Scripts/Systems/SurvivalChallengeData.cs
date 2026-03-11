using Godot;
using System;
using System.Collections.Generic;

namespace ClawRPG.Scripts.Systems
{
    /// <summary>
    /// 生存挑战数据结构
    /// </summary>
    public class SurvivalChallengeData
    {
        // 挑战类型
        public enum ChallengeType
        {
            EndlessWaves,      // 无尽波次
            TimeAttack,        // 时间 attack
            BossRush,          // Boss rush
            ArenaSurvival,     // 竞技场生存
            DungeonEndless     // 无尽地下城
        }
        
        // 难度等级
        public enum ChallengeDifficulty
        {
            Easy,       // 简单
            Normal,     // 普通
            Hard,       // 困难
            Epic,       // 史诗
            Legendary   // 传奇
        }
        
        // 挑战状态
        public enum ChallengeState
        {
            NotStarted,    // 未开始
            InProgress,    // 进行中
            Completed,     // 完成
            Failed         // 失败
        }
        
        // 挑战配置
        public class ChallengeConfig
        {
            public string Id { get; set; }
            public string Name { get; set; }
            public string Description { get; set; }
            public ChallengeType Type { get; set; }
            public ChallengeDifficulty Difficulty { get; set; }
            public int RecommendedLevel { get; set; }
            public int TimeLimit { get; set; }           // 时间限制（秒），0=无限制
            public int WaveCount { get; set; }           // 波次数量，0=无限
            public int EnemiesPerWave { get; set; }       // 每波敌人数量
            public float EnemySpawnInterval { get; set; }  // 敌人生成间隔
            public float EnemyScale { get; set; }         // 敌人属性缩放
            public int EntryFee { get; set; }             // 参加费用
            public int BaseGoldReward { get; set; }       // 基础金币奖励
            public int BaseExpReward { get; set; }        // 基础经验奖励
            public float GoldMultiplier { get; set; }     // 金币奖励乘数
            public float ExpMultiplier { get; set; }      // 经验奖励乘数
        }
        
        // 活跃挑战实例
        public class ActiveChallenge
        {
            public string InstanceId { get; set; }
            public string ConfigId { get; set; }
            public ChallengeState State { get; set; }
            public int CurrentWave { get; set; }
            public int EnemiesKilled { get; set; }
            public int DamageDealt { get; set; }
            public int DamageTaken { get; set; }
            public int EnemiesRemaining { get; set; }
            public float ElapsedTime { get; set; }
            public float LastSpawnTime { get; set; }
            public int Score { get; set; }
            public bool IsWaveInProgress { get; set; }
        }
        
        // 玩家挑战数据
        public class PlayerChallengeData
        {
            public Dictionary<string, int> BestWaves { get; set; } = new();      // 最佳波次
            public Dictionary<string, int> BestScores { get; set; } = new();    // 最高分
            public Dictionary<string, float> BestTimes { get; set; } = new();  // 最佳时间
            public Dictionary<string, int> CompletionCount { get; set; } = new(); // 完成次数
            public Dictionary<string, int> TotalKills { get; set; } = new();   // 总击杀数
            public Dictionary<string, int> TotalGoldEarned { get; set; } = new(); // 总获得金币
            public List<ActiveChallenge> ActiveChallenges { get; set; } = new(); // 活跃挑战
        }
        
        // 挑战结果
        public class ChallengeResult
        {
            public string ConfigId { get; set; }
            public bool Success { get; set; }
            public int WaveReached { get; set; }
            public int EnemiesKilled { get; set; }
            public int DamageDealt { get; set; }
            public int DamageTaken { get; set; }
            public float TimeElapsed { get; set; }
            public int Score { get; set; }
            public int GoldReward { get; set; }
            public int ExpReward { get; set; }
            public string Grade { get; set; }  // S/A/B/C/D
        }
    }
}
