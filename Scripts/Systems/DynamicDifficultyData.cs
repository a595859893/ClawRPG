using Godot;
using System;
using System.Collections.Generic;

public class DynamicDifficultyData
{
    // 难度等级
    public enum DifficultyLevel
    {
        Easy = 0,       // 简单
        Normal = 1,     // 普通
        Hard = 2,       // 困难
        Epic = 3,       // 史诗
        Legendary = 4   // 传奇
    }

    // 玩家技能档案 - 记录玩家表现
    public class PlayerSkillProfile
    {
        public float WinRate { get; set; }              // 胜率 (0-1)
        public float ResourceEfficiency { get; set; }   // 资源效率 (0-1)
        public float SurvivalAbility { get; set; }      // 生存能力 (0-1)
        public float DamageOutput { get; set; }         // 输出能力 (0-1)
        public float TechnicalSkill { get; set; }       // 技术水平 (0-1)
        public float OverallScore { get; set; }         // 综合评分 (0-1)
        public int TotalSessions { get; set; }          // 总会话数
        public int Wins { get; set; }                    // 胜利次数
        public int Losses { get; set; }                  // 失败次数
        public float AverageClearTime { get; set; }      // 平均通关时间(分钟)
        public float AverageDeaths { get; set; }        // 平均死亡次数

        public PlayerSkillProfile()
        {
            WinRate = 0.5f;
            ResourceEfficiency = 0.5f;
            SurvivalAbility = 0.5f;
            DamageOutput = 0.5f;
            TechnicalSkill = 0.5f;
            OverallScore = 0.5f;
            TotalSessions = 0;
            Wins = 0;
            Losses = 0;
            AverageClearTime = 30f;
            AverageDeaths = 2f;
        }
    }

    // 难度修正值
    public class DifficultyModifiers
    {
        public float EnemyHealthMultiplier { get; set; }    // 敌人生命值乘数
        public float EnemyDamageMultiplier { get; set; }     // 敌人伤害乘数
        public float EnemySpeedMultiplier { get; set; }      // 敌人速度乘数
        public float DropRateMultiplier { get; set; }         // 掉落率乘数
        public float ExperienceMultiplier { get; set; }        // 经验值乘数
        public float GoldMultiplier { get; set; }             // 金币乘数
        public float EnemyCountMultiplier { get; set; }       // 敌人数量乘数
        public float BossHealthMultiplier { get; set; }       // Boss生命值乘数
        public float BossDamageMultiplier { get; set; }       // Boss伤害乘数

        public DifficultyModifiers()
        {
            SetForDifficulty(DifficultyLevel.Normal);
        }

        public void SetForDifficulty(DifficultyLevel level)
        {
            switch (level)
            {
                case DifficultyLevel.Easy:
                    EnemyHealthMultiplier = 0.7f;
                    EnemyDamageMultiplier = 0.7f;
                    EnemySpeedMultiplier = 0.8f;
                    DropRateMultiplier = 1.5f;
                    ExperienceMultiplier = 1.5f;
                    GoldMultiplier = 1.5f;
                    EnemyCountMultiplier = 0.8f;
                    BossHealthMultiplier = 0.7f;
                    BossDamageMultiplier = 0.7f;
                    break;
                case DifficultyLevel.Normal:
                    EnemyHealthMultiplier = 1.0f;
                    EnemyDamageMultiplier = 1.0f;
                    EnemySpeedMultiplier = 1.0f;
                    DropRateMultiplier = 1.2f;
                    ExperienceMultiplier = 1.2f;
                    GoldMultiplier = 1.2f;
                    EnemyCountMultiplier = 1.0f;
                    BossHealthMultiplier = 1.0f;
                    BossDamageMultiplier = 1.0f;
                    break;
                case DifficultyLevel.Hard:
                    EnemyHealthMultiplier = 1.3f;
                    EnemyDamageMultiplier = 1.3f;
                    EnemySpeedMultiplier = 1.1f;
                    DropRateMultiplier = 1.0f;
                    ExperienceMultiplier = 1.0f;
                    GoldMultiplier = 1.0f;
                    EnemyCountMultiplier = 1.2f;
                    BossHealthMultiplier = 1.3f;
                    BossDamageMultiplier = 1.3f;
                    break;
                case DifficultyLevel.Epic:
                    EnemyHealthMultiplier = 1.6f;
                    EnemyDamageMultiplier = 1.6f;
                    EnemySpeedMultiplier = 1.2f;
                    DropRateMultiplier = 0.8f;
                    ExperienceMultiplier = 0.9f;
                    GoldMultiplier = 0.9f;
                    EnemyCountMultiplier = 1.4f;
                    BossHealthMultiplier = 1.6f;
                    BossDamageMultiplier = 1.6f;
                    break;
                case DifficultyLevel.Legendary:
                    EnemyHealthMultiplier = 2.0f;
                    EnemyDamageMultiplier = 2.0f;
                    EnemySpeedMultiplier = 1.3f;
                    DropRateMultiplier = 0.6f;
                    ExperienceMultiplier = 0.8f;
                    GoldMultiplier = 0.8f;
                    EnemyCountMultiplier = 1.6f;
                    BossHealthMultiplier = 2.0f;
                    BossDamageMultiplier = 2.0f;
                    break;
            }
        }
    }

    // 会话统计数据
    public class SessionStats
    {
        public int EnemiesKilled { get; set; }
        public int BossesDefeated { get; set; }
        public int TimesDied { get; set; }
        public int ItemsCollected { get; set; }
        public int GoldEarned { get; set; }
        public int ExperienceGained { get; set; }
        public float SessionTime { get; set; }
        public int PotionsUsed { get; set; }
        public int CriticalHits { get; set; }
        public int Dodges { get; set; }

        public SessionStats()
        {
            Reset();
        }

        public void Reset()
        {
            EnemiesKilled = 0;
            BossesDefeated = 0;
            TimesDied = 0;
            ItemsCollected = 0;
            GoldEarned = 0;
            ExperienceGained = 0;
            SessionTime = 0f;
            PotionsUsed = 0;
            CriticalHits = 0;
            Dodges = 0;
        }
    }

    // 玩家动态难度数据
    public class PlayerDynamicDifficultyData
    {
        public DifficultyLevel CurrentDifficulty { get; set; }
        public DifficultyLevel RecommendedDifficulty { get; set; }
        public bool IsAutoAdjustment { get; set; }
        public PlayerSkillProfile SkillProfile { get; set; }
        public SessionStats CurrentSession { get; set; }
        public List<SessionStats> SessionHistory { get; set; }
        public DateTime LastAdjustmentTime { get; set; }
        public int SessionsSinceLastAdjustment { get; set; }

        public PlayerDynamicDifficultyData()
        {
            CurrentDifficulty = DifficultyLevel.Normal;
            RecommendedDifficulty = DifficultyLevel.Normal;
            IsAutoAdjustment = true;
            SkillProfile = new PlayerSkillProfile();
            CurrentSession = new SessionStats();
            SessionHistory = new List<SessionStats>();
            LastAdjustmentTime = DateTime.Now;
            SessionsSinceLastAdjustment = 0;
        }
    }
}
