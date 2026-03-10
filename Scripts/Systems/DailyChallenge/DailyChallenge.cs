using Godot;
using System;
using System.Collections.Generic;

namespace ClawRPG.Scripts.Systems {
    /// <summary>
    /// Daily challenge type definitions
    /// </summary>
    public enum ChallengeType {
        KillEnemies,      // 击杀敌人
        CollectItems,     // 收集物品
        UseSkills,        // 使用技能
        CompleteQuests,  //完成任务
        DealDamage,       // 造成伤害
        EarnGold,        // 赚取金币
        ExploreRegions,   // 探索区域
        SurvivalTime      // 生存时间
    }
    
    /// <summary>
    /// Difficulty level for challenges
    /// </summary>
    public enum ChallengeDifficulty {
        Easy,     // 简单
        Normal,   // 普通
        Hard,     // 困难
        Elite     // 精英
    }
    
    /// <summary>
    /// Daily challenge definition
    /// </summary>
    [Serializable]
    public class DailyChallenge {
        public string Id;
        public string Name;
        public string Description;
        public ChallengeType Type;
        public ChallengeDifficulty Difficulty;
        public int TargetCount;
        public int CurrentProgress;
        public bool IsCompleted;
        public int GoldReward;
        public int ExpReward;
        public List<int> ItemRewardIds;
        public DateTime ExpireTime;
        
        public DailyChallenge() {
            Id = "";
            Name = "";
            Description = "";
            ItemRewardIds = new List<int>();
        }
        
        public float GetProgressPercentage() {
            if (TargetCount <= 0) return 0f;
            return Mathf.Clamp((float)CurrentProgress / TargetCount, 0f, 1f);
        }
        
        public bool IsExpired() {
            return DateTime.Now > ExpireTime;
        }
    }
    
    /// <summary>
    /// Daily challenge database
    /// </summary>
    public class DailyChallengeDatabase {
        private static DailyChallengeDatabase _instance;
        public static DailyChallengeDatabase Instance {
            get {
                if (_instance == null) {
                    _instance = new DailyChallengeDatabase();
                    _instance.LoadChallenges();
                }
                return _instance;
            }
        }
        
        private List<DailyChallenge> _allChallenges = new();
        
        public void LoadChallenges() {
            // === KILL ENEMIES CHALLENGES ===
            _allChallenges.Add(new DailyChallenge {
                Id = "kill_goblins_10",
                Name = "哥布林杀手",
                Description = "击杀10只哥布林",
                Type = ChallengeType.KillEnemies,
                Difficulty = ChallengeDifficulty.Easy,
                TargetCount = 10,
                GoldReward = 50,
                ExpReward = 100,
                ItemRewardIds = new List<int> { 101 }
            });
            
            _allChallenges.Add(new DailyChallenge {
                Id = "kill_bosses_1",
                Name = "Boss杀手",
                Description = "击败1个Boss",
                Type = ChallengeType.KillEnemies,
                Difficulty = ChallengeDifficulty.Elite,
                TargetCount = 1,
                GoldReward = 500,
                ExpReward = 2000,
                ItemRewardIds = new List<int> { 201, 202 }
            });
            
            // === COLLECT ITEMS CHALLENGES ===
            _allChallenges.Add(new DailyChallenge {
                Id = "collect_gold_500",
                Name = "财富积累",
                Description = "赚取500金币",
                Type = ChallengeType.EarnGold,
                Difficulty = ChallengeDifficulty.Easy,
                TargetCount = 500,
                GoldReward = 0,
                ExpReward = 50,
                ItemRewardIds = new List<int>()
            });
            
            _allChallenges.Add(new DailyChallenge {
                Id = "collect_materials_20",
                Name = "材料收集者",
                Description = "收集20个材料",
                Type = ChallengeType.CollectItems,
                Difficulty = ChallengeDifficulty.Normal,
                TargetCount = 20,
                GoldReward = 150,
                ExpReward = 300,
                ItemRewardIds = new List<int> { 301 }
            });
            
            // === DEAL DAMAGE CHALLENGES ===
            _allChallenges.Add(new DailyChallenge {
                Id = "deal_damage_1000",
                Name = "战斗大师",
                Description = "造成1000点伤害",
                Type = ChallengeType.DealDamage,
                Difficulty = ChallengeDifficulty.Normal,
                TargetCount = 1000,
                GoldReward = 200,
                ExpReward = 400,
                ItemRewardIds = new List<int> { 102 }
            });
            
            _allChallenges.Add(new DailyChallenge {
                Id = "deal_damage_10000",
                Name = "毁灭者",
                Description = "造成10000点伤害",
                Type = ChallengeType.DealDamage,
                Difficulty = ChallengeDifficulty.Hard,
                TargetCount = 10000,
                GoldReward = 400,
                ExpReward = 1000,
                ItemRewardIds = new List<int> { 103, 104 }
            });
            
            // === USE SKILLS CHALLENGES ===
            _allChallenges.Add(new DailyChallenge {
                Id = "use_skills_30",
                Name = "技能大师",
                Description = "使用30次技能",
                Type = ChallengeType.UseSkills,
                Difficulty = ChallengeDifficulty.Easy,
                TargetCount = 30,
                GoldReward = 100,
                ExpReward = 200,
                ItemRewardIds = new List<int>()
            });
            
            // === COMPLETE QUESTS CHALLENGES ===
            _allChallenges.Add(new DailyChallenge {
                Id = "complete_quests_2",
                Name = "任务达人",
                Description = "完成2个任务",
                Type = ChallengeType.CompleteQuests,
                Difficulty = ChallengeDifficulty.Normal,
                TargetCount = 2,
                GoldReward = 300,
                ExpReward = 500,
                ItemRewardIds = new List<int> { 105 }
            });
            
            // === EXPLORE REGIONS CHALLENGES ===
            _allChallenges.Add(new DailyChallenge {
                Id = "explore_3_regions",
                Name = "探索者",
                Description = "探索3个不同区域",
                Type = ChallengeType.ExploreRegions,
                Difficulty = ChallengeDifficulty.Normal,
                TargetCount = 3,
                GoldReward = 250,
                ExpReward = 400,
                ItemRewardIds = new List<int> { 106 }
            });
            
            // === SURVIVAL CHALLENGES ===
            _allChallenges.Add(new DailyChallenge {
                Id = "survive_5_minutes",
                Name = "生存专家",
                Description = "生存5分钟",
                Type = ChallengeType.SurvivalTime,
                Difficulty = ChallengeDifficulty.Hard,
                TargetCount = 300, // 5 minutes in seconds
                GoldReward = 350,
                ExpReward = 800,
                ItemRewardIds = new List<int> { 107 }
            });
            
            GD.Print($"[DailyChallengeDatabase] Loaded {_allChallenges.Count} challenge templates");
        }
        
        public List<DailyChallenge> GetRandomChallenges(int count = 3) {
            var random = new Random();
            var shuffled = new List<DailyChallenge>(_allChallenges);
            
            // Shuffle using Fisher-Yates
            for (int i = shuffled.Count - 1; i > 0; i--) {
                int j = random.Next(i + 1);
                var temp = shuffled[i];
                shuffled[i] = shuffled[j];
                shuffled[j] = temp;
            }
            
            var result = new List<DailyChallenge>();
            for (int i = 0; i < Mathf.Min(count, shuffled.Count); i++) {
                var challenge = new DailyChallenge {
                    Id = shuffled[i].Id + "_" + DateTime.Now.ToString("yyyyMMdd"),
                    Name = shuffled[i].Name,
                    Description = shuffled[i].Description,
                    Type = shuffled[i].Type,
                    Difficulty = shuffled[i].Difficulty,
                    TargetCount = shuffled[i].TargetCount,
                    CurrentProgress = 0,
                    IsCompleted = false,
                    GoldReward = shuffled[i].GoldReward,
                    ExpReward = shuffled[i].ExpReward,
                    ItemRewardIds = new List<int>(shuffled[i].ItemRewardIds),
                    ExpireTime = DateTime.Today.AddDays(1)
                };
                result.Add(challenge);
            }
            
            return result;
        }
        
        public DailyChallenge GetChallengeTemplate(string id) {
            foreach (var c in _allChallenges) {
                if (c.Id == id) return c;
            }
            return null;
        }
    }
}
