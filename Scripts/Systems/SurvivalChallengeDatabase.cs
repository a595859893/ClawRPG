using Godot;
using System;
using System.Collections.Generic;

namespace ClawRPG.Scripts.Systems
{
    /// <summary>
    /// 生存挑战配置数据库
    /// </summary>
    public static class SurvivalChallengeDatabase
    {
        // 挑战配置列表
        private static readonly List<SurvivalChallengeData.ChallengeConfig> _challenges = new()
        {
            // 无尽波次
            new SurvivalChallengeData.ChallengeConfig
            {
                Id = "endless_easy",
                Name = "初级无尽波次",
                Description = "适合新手的入门挑战，敌人较弱",
                Type = SurvivalChallengeData.ChallengeType.EndlessWaves,
                Difficulty = SurvivalChallengeData.ChallengeDifficulty.Easy,
                RecommendedLevel = 1,
                TimeLimit = 300,
                WaveCount = 0,  // 无限
                EnemiesPerWave = 5,
                EnemySpawnInterval = 3f,
                EnemyScale = 0.5f,
                EntryFee = 0,
                BaseGoldReward = 100,
                BaseExpReward = 50,
                GoldMultiplier = 1.5f,
                ExpMultiplier = 1.5f
            },
            new SurvivalChallengeData.ChallengeConfig
            {
                Id = "endless_normal",
                Name = "普通无尽波次",
                Description = "标准难度的生存挑战",
                Type = SurvivalChallengeData.ChallengeType.EndlessWaves,
                Difficulty = SurvivalChallengeData.ChallengeDifficulty.Normal,
                RecommendedLevel = 15,
                TimeLimit = 600,
                WaveCount = 0,
                EnemiesPerWave = 8,
                EnemySpawnInterval = 2.5f,
                EnemyScale = 0.8f,
                EntryFee = 100,
                BaseGoldReward = 300,
                BaseExpReward = 150,
                GoldMultiplier = 1.2f,
                ExpMultiplier = 1.2f
            },
            new SurvivalChallengeData.ChallengeConfig
            {
                Id = "endless_hard",
                Name = "困难无尽波次",
                Description = "高难度生存挑战，敌人强力",
                Type = SurvivalChallengeData.ChallengeType.EndlessWaves,
                Difficulty = SurvivalChallengeData.ChallengeDifficulty.Hard,
                RecommendedLevel = 30,
                TimeLimit = 900,
                WaveCount = 0,
                EnemiesPerWave = 12,
                EnemySpawnInterval = 2f,
                EnemyScale = 1.2f,
                EntryFee = 500,
                BaseGoldReward = 800,
                BaseExpReward = 400,
                GoldMultiplier = 1.0f,
                ExpMultiplier = 1.0f
            },
            new SurvivalChallengeData.ChallengeConfig
            {
                Id = "endless_epic",
                Name = "史诗无尽波次",
                Description = "极具挑战性的生存模式",
                Type = SurvivalChallengeData.ChallengeType.EndlessWaves,
                Difficulty = SurvivalChallengeData.ChallengeDifficulty.Epic,
                RecommendedLevel = 45,
                TimeLimit = 1200,
                WaveCount = 0,
                EnemiesPerWave = 15,
                EnemySpawnInterval = 1.5f,
                EnemyScale = 1.5f,
                EntryFee = 2000,
                BaseGoldReward = 2000,
                BaseExpReward = 1000,
                GoldMultiplier = 0.8f,
                ExpMultiplier = 0.8f
            },
            new SurvivalChallengeData.ChallengeConfig
            {
                Id = "endless_legendary",
                Name = "传奇无尽波次",
                Description = "终极生存挑战，只有最强者能完成",
                Type = SurvivalChallengeData.ChallengeType.EndlessWaves,
                Difficulty = SurvivalChallengeData.ChallengeDifficulty.Legendary,
                RecommendedLevel = 60,
                TimeLimit = 1800,
                WaveCount = 0,
                EnemiesPerWave = 20,
                EnemySpawnInterval = 1f,
                EnemyScale = 2.0f,
                EntryFee = 5000,
                BaseGoldReward = 5000,
                BaseExpReward = 2500,
                GoldMultiplier = 0.5f,
                ExpMultiplier = 0.5f
            },
            
            // 时间 attack
            new SurvivalChallengeData.ChallengeConfig
            {
                Id = "time_attack_easy",
                Name = "初级时间 attack",
                Description = "在限定时间内击败尽可能多的敌人",
                Type = SurvivalChallengeData.ChallengeType.TimeAttack,
                Difficulty = SurvivalChallengeData.ChallengeDifficulty.Easy,
                RecommendedLevel = 5,
                TimeLimit = 120,
                WaveCount = 0,
                EnemiesPerWave = 3,
                EnemySpawnInterval = 2f,
                EnemyScale = 0.6f,
                EntryFee = 50,
                BaseGoldReward = 200,
                BaseExpReward = 100,
                GoldMultiplier = 2.0f,
                ExpMultiplier = 2.0f
            },
            new SurvivalChallengeData.ChallengeConfig
            {
                Id = "time_attack_normal",
                Name = "普通时间 attack",
                Description = "标准时间 attack 挑战",
                Type = SurvivalChallengeData.ChallengeType.TimeAttack,
                Difficulty = SurvivalChallengeData.ChallengeDifficulty.Normal,
                RecommendedLevel = 20,
                TimeLimit = 180,
                WaveCount = 0,
                EnemiesPerWave = 5,
                EnemySpawnInterval = 1.5f,
                EnemyScale = 0.9f,
                EntryFee = 200,
                BaseGoldReward = 500,
                BaseExpReward = 250,
                GoldMultiplier = 1.5f,
                ExpMultiplier = 1.5f
            },
            new SurvivalChallengeData.ChallengeConfig
            {
                Id = "time_attack_hard",
                Name = "困难时间 attack",
                Description = "高强度时间 attack",
                Type = SurvivalChallengeData.ChallengeType.TimeAttack,
                Difficulty = SurvivalChallengeData.ChallengeDifficulty.Hard,
                RecommendedLevel = 35,
                TimeLimit = 240,
                WaveCount = 0,
                EnemiesPerWave = 8,
                EnemySpawnInterval = 1f,
                EnemyScale = 1.3f,
                EntryFee = 800,
                BaseGoldReward = 1200,
                BaseExpReward = 600,
                GoldMultiplier = 1.2f,
                ExpMultiplier = 1.2f
            },
            new SurvivalChallengeData.ChallengeConfig
            {
                Id = "time_attack_epic",
                Name = "史诗时间 attack",
                Description = "极限时间 attack",
                Type = SurvivalChallengeData.ChallengeType.TimeAttack,
                Difficulty = SurvivalChallengeData.ChallengeDifficulty.Epic,
                RecommendedLevel = 50,
                TimeLimit = 300,
                WaveCount = 0,
                EnemiesPerWave = 12,
                EnemySpawnInterval = 0.8f,
                EnemyScale = 1.6f,
                EntryFee = 3000,
                BaseGoldReward = 3000,
                BaseExpReward = 1500,
                GoldMultiplier = 1.0f,
                ExpMultiplier = 1.0f
            },
            
            // Boss Rush
            new SurvivalChallengeData.ChallengeConfig
            {
                Id = "boss_rush_normal",
                Name = "普通Boss rush",
                Description = "连续击败多个Boss",
                Type = SurvivalChallengeData.ChallengeType.BossRush,
                Difficulty = SurvivalChallengeData.ChallengeDifficulty.Normal,
                RecommendedLevel = 25,
                TimeLimit = 600,
                WaveCount = 5,
                EnemiesPerWave = 1,
                EnemySpawnInterval = 0f,
                EnemyScale = 1.0f,
                EntryFee = 500,
                BaseGoldReward = 2000,
                BaseExpReward = 1000,
                GoldMultiplier = 1.5f,
                ExpMultiplier = 1.5f
            },
            new SurvivalChallengeData.ChallengeConfig
            {
                Id = "boss_rush_hard",
                Name = "困难Boss rush",
                Description = "连续击败强力Boss",
                Type = SurvivalChallengeData.ChallengeType.BossRush,
                Difficulty = SurvivalChallengeData.ChallengeDifficulty.Hard,
                RecommendedLevel = 40,
                TimeLimit = 900,
                WaveCount = 8,
                EnemiesPerWave = 1,
                EnemySpawnInterval = 0f,
                EnemyScale = 1.5f,
                EntryFee = 2000,
                BaseGoldReward = 5000,
                BaseExpReward = 2500,
                GoldMultiplier = 1.2f,
                ExpMultiplier = 1.2f
            },
            new SurvivalChallengeData.ChallengeConfig
            {
                Id = "boss_rush_legendary",
                Name = "传奇Boss rush",
                Description = "连续击败传奇Boss",
                Type = SurvivalChallengeData.ChallengeType.BossRush,
                Difficulty = SurvivalChallengeData.ChallengeDifficulty.Legendary,
                RecommendedLevel = 55,
                TimeLimit = 1200,
                WaveCount = 10,
                EnemiesPerWave = 1,
                EnemySpawnInterval = 0f,
                EnemyScale = 2.0f,
                EntryFee = 8000,
                BaseGoldReward = 15000,
                BaseExpReward = 7500,
                GoldMultiplier = 1.0f,
                ExpMultiplier = 1.0f
            },
            
            // 竞技场生存
            new SurvivalChallengeData.ChallengeConfig
            {
                Id = "arena_easy",
                Name = "初级竞技场",
                Description = "1v1竞技场对战",
                Type = SurvivalChallengeData.ChallengeType.ArenaSurvival,
                Difficulty = SurvivalChallengeData.ChallengeDifficulty.Easy,
                RecommendedLevel = 10,
                TimeLimit = 180,
                WaveCount = 0,
                EnemiesPerWave = 1,
                EnemySpawnInterval = 0f,
                EnemyScale = 0.7f,
                EntryFee = 100,
                BaseGoldReward = 500,
                BaseExpReward = 250,
                GoldMultiplier = 2.0f,
                ExpMultiplier = 2.0f
            },
            new SurvivalChallengeData.ChallengeConfig
            {
                Id = "arena_normal",
                Name = "普通竞技场",
                Description = "1v3竞技场对战",
                Type = SurvivalChallengeData.ChallengeType.ArenaSurvival,
                Difficulty = SurvivalChallengeData.ChallengeDifficulty.Normal,
                RecommendedLevel = 25,
                TimeLimit = 300,
                WaveCount = 0,
                EnemiesPerWave = 3,
                EnemySpawnInterval = 0f,
                EnemyScale = 1.0f,
                EntryFee = 500,
                BaseGoldReward = 1500,
                BaseExpReward = 750,
                GoldMultiplier = 1.5f,
                ExpMultiplier = 1.5f
            },
            new SurvivalChallengeData.ChallengeConfig
            {
                Id = "arena_hard",
                Name = "困难竞技场",
                Description = "1v5竞技场对战",
                Type = SurvivalChallengeData.ChallengeType.ArenaSurvival,
                Difficulty = SurvivalChallengeData.ChallengeDifficulty.Hard,
                RecommendedLevel = 40,
                TimeLimit = 420,
                WaveCount = 0,
                EnemiesPerWave = 5,
                EnemySpawnInterval = 0f,
                EnemyScale = 1.4f,
                EntryFee = 2000,
                BaseGoldReward = 4000,
                BaseExpReward = 2000,
                GoldMultiplier = 1.2f,
                ExpMultiplier = 1.2f
            },
            new SurvivalChallengeData.ChallengeConfig
            {
                Id = "arena_legendary",
                Name = "传奇竞技场",
                Description = "1v10竞技场车轮战",
                Type = SurvivalChallengeData.ChallengeType.ArenaSurvival,
                Difficulty = SurvivalChallengeData.ChallengeDifficulty.Legendary,
                RecommendedLevel = 55,
                TimeLimit = 600,
                WaveCount = 0,
                EnemiesPerWave = 10,
                EnemySpawnInterval = 0f,
                EnemyScale = 2.0f,
                EntryFee = 8000,
                BaseGoldReward = 12000,
                BaseExpReward = 6000,
                GoldMultiplier = 1.0f,
                ExpMultiplier = 1.0f
            }
        };
        
        /// <summary>
        /// 获取所有挑战配置
        /// </summary>
        public static List<SurvivalChallengeData.ChallengeConfig> GetAllChallenges()
        {
            return new List<SurvivalChallengeData.ChallengeConfig>(_challenges);
        }
        
        /// <summary>
        /// 根据ID获取挑战配置
        /// </summary>
        public static SurvivalChallengeData.ChallengeConfig GetChallenge(string id)
        {
            foreach (var challenge in _challenges)
            {
                if (challenge.Id == id)
                    return challenge;
            }
            return null;
        }
        
        /// <summary>
        /// 根据类型获取挑战列表
        /// </summary>
        public static List<SurvivalChallengeData.ChallengeConfig> GetChallengesByType(SurvivalChallengeData.ChallengeType type)
        {
            List<SurvivalChallengeData.ChallengeConfig> result = new();
            foreach (var challenge in _challenges)
            {
                if (challenge.Type == type)
                    result.Add(challenge);
            }
            return result;
        }
        
        /// <summary>
        /// 根据难度获取挑战列表
        /// </summary>
        public static List<SurvivalChallengeData.ChallengeConfig> GetChallengesByDifficulty(SurvivalChallengeData.ChallengeDifficulty difficulty)
        {
            List<SurvivalChallengeData.ChallengeConfig> result = new();
            foreach (var challenge in _challenges)
            {
                if (challenge.Difficulty == difficulty)
                    result.Add(challenge);
            }
            return result;
        }
        
        /// <summary>
        /// 根据难度颜色获取颜色值
        /// </summary>
        public static Color GetDifficultyColor(SurvivalChallengeData.ChallengeDifficulty difficulty)
        {
            return difficulty switch
            {
                SurvivalChallengeData.ChallengeDifficulty.Easy => new Color(0.3f, 0.8f, 0.3f),
                SurvivalChallengeData.ChallengeDifficulty.Normal => new Color(0.3f, 0.5f, 0.9f),
                SurvivalChallengeData.ChallengeDifficulty.Hard => new Color(0.9f, 0.5f, 0.2f),
                SurvivalChallengeData.ChallengeDifficulty.Epic => new Color(0.6f, 0.3f, 0.8f),
                SurvivalChallengeData.ChallengeDifficulty.Legendary => new Color(0.9f, 0.3f, 0.3f),
                _ => Colors.White
            };
        }
        
        /// <summary>
        /// 获取难度名称
        /// </summary>
        public static string GetDifficultyName(SurvivalChallengeData.ChallengeDifficulty difficulty)
        {
            return difficulty switch
            {
                SurvivalChallengeData.ChallengeDifficulty.Easy => "简单",
                SurvivalChallengeData.ChallengeDifficulty.Normal => "普通",
                SurvivalChallengeData.ChallengeDifficulty.Hard => "困难",
                SurvivalChallengeData.ChallengeDifficulty.Epic => "史诗",
                SurvivalChallengeData.ChallengeDifficulty.Legendary => "传奇",
                _ => "未知"
            };
        }
        
        /// <summary>
        /// 获取类型名称
        /// </summary>
        public static string GetTypeName(SurvivalChallengeData.ChallengeType type)
        {
            return type switch
            {
                SurvivalChallengeData.ChallengeType.EndlessWaves => "无尽波次",
                SurvivalChallengeData.ChallengeType.TimeAttack => "限时击杀",
                SurvivalChallengeData.ChallengeType.BossRush => "Boss Rush",
                SurvivalChallengeData.ChallengeType.ArenaSurvival => "竞技场",
                SurvivalChallengeData.ChallengeType.DungeonEndless => "无尽地下城",
                _ => "未知"
            };
        }
    }
}
