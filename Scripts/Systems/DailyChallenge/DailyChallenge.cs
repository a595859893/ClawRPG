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
        CompleteQuests,   // 完成任务
        DealDamage,       // 造成伤害
        EarnGold,         // 赚取金币
        ExploreRegions,   // 探索区域
        SurvivalTime,     // 生存时间
        Fishing,          // 钓鱼
        Alchemy,          // 炼金
        MountCombat,      // 坐骑战斗
        PetBattle,        // 宠物战斗
        Trade,            // 交易
        CraftItem,        // 合成物品
        Reputation,       // 声望
        KillBoss,         // 击败Boss
        CriticalHits,     // 暴击
        Dodge,            // 闪避
        Heal,             // 治疗
        Stealth           // 潜行
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
            
            // === FISHING CHALLENGES ===
            _allChallenges.Add(new DailyChallenge {
                Id = "fish_5_times",
                Name = "休闲钓手",
                Description = "钓鱼5次",
                Type = ChallengeType.Fishing,
                Difficulty = ChallengeDifficulty.Easy,
                TargetCount = 5,
                GoldReward = 80,
                ExpReward = 150,
                ItemRewardIds = new List<int> { 401 }
            });
            
            _allChallenges.Add(new DailyChallenge {
                Id = "catch_legendary_fish",
                Name = "传说中的渔夫",
                Description = "钓到1条传说级鱼类",
                Type = ChallengeType.Fishing,
                Difficulty = ChallengeDifficulty.Elite,
                TargetCount = 1,
                GoldReward = 600,
                ExpReward = 1500,
                ItemRewardIds = new List<int> { 402 }
            });
            
            // === ALCHEMY CHALLENGES ===
            _allChallenges.Add(new DailyChallenge {
                Id = "craft_3_potions",
                Name = "炼金学徒",
                Description = "炼金3次",
                Type = ChallengeType.Alchemy,
                Difficulty = ChallengeDifficulty.Easy,
                TargetCount = 3,
                GoldReward = 100,
                ExpReward = 200,
                ItemRewardIds = new List<int> { 403 }
            });
            
            _allChallenges.Add(new DailyChallenge {
                Id = "craft_epic_potion",
                Name = "炼金大师",
                Description = "炼制1个史诗级药水",
                Type = ChallengeType.Alchemy,
                Difficulty = ChallengeDifficulty.Hard,
                TargetCount = 1,
                GoldReward = 450,
                ExpReward = 1200,
                ItemRewardIds = new List<int> { 404 }
            });
            
            // === MOUNT COMBAT CHALLENGES ===
            _allChallenges.Add(new DailyChallenge {
                Id = "mount_kill_10_enemies",
                Name = "骑战先锋",
                Description = "骑乘坐骑击杀10个敌人",
                Type = ChallengeType.MountCombat,
                Difficulty = ChallengeDifficulty.Normal,
                TargetCount = 10,
                GoldReward = 250,
                ExpReward = 500,
                ItemRewardIds = new List<int> { 405 }
            });
            
            _allChallenges.Add(new DailyChallenge {
                Id = "mount_skill_20_times",
                Name = "骑战大师",
                Description = "使用坐骑技能20次",
                Type = ChallengeType.MountCombat,
                Difficulty = ChallengeDifficulty.Hard,
                TargetCount = 20,
                GoldReward = 400,
                ExpReward = 900,
                ItemRewardIds = new List<int> { 406 }
            });
            
            // === PET BATTLE CHALLENGES ===
            _allChallenges.Add(new DailyChallenge {
                Id = "pet_kill_5_enemies",
                Name = "宠物训练师",
                Description = "宠物击杀5个敌人",
                Type = ChallengeType.PetBattle,
                Difficulty = ChallengeDifficulty.Easy,
                TargetCount = 5,
                GoldReward = 120,
                ExpReward = 250,
                ItemRewardIds = new List<int> { 407 }
            });
            
            _allChallenges.Add(new DailyChallenge {
                Id = "pet_level_up",
                Name = "宠物成长",
                Description = "宠物升级1次",
                Type = ChallengeType.PetBattle,
                Difficulty = ChallengeDifficulty.Normal,
                TargetCount = 1,
                GoldReward = 300,
                ExpReward = 600,
                ItemRewardIds = new List<int> { 408 }
            });
            
            // === TRADE CHALLENGES ===
            _allChallenges.Add(new DailyChallenge {
                Id = "sell_items_10",
                Name = "商人新手",
                Description = "出售10件物品",
                Type = ChallengeType.Trade,
                Difficulty = ChallengeDifficulty.Easy,
                TargetCount = 10,
                GoldReward = 80,
                ExpReward = 150,
                ItemRewardIds = new List<int>()
            });
            
            _allChallenges.Add(new DailyChallenge {
                Id = "earn_gold_1000",
                Name = "金币大亨",
                Description = "赚取1000金币",
                Type = ChallengeType.Trade,
                Difficulty = ChallengeDifficulty.Normal,
                TargetCount = 1000,
                GoldReward = 0,
                ExpReward = 300,
                ItemRewardIds = new List<int> { 409 }
            });
            
            // === CRAFT CHALLENGES ===
            _allChallenges.Add(new DailyChallenge {
                Id = "craft_equipment_2",
                Name = "装备匠师",
                Description = "合成2件装备",
                Type = ChallengeType.CraftItem,
                Difficulty = ChallengeDifficulty.Normal,
                TargetCount = 2,
                GoldReward = 200,
                ExpReward = 400,
                ItemRewardIds = new List<int> { 410 }
            });
            
            // === REPUTATION CHALLENGES ===
            _allChallenges.Add(new DailyChallenge {
                Id = "gain_reputation",
                Name = "、声望提升",
                Description = "提升任意阵营100点声望",
                Type = ChallengeType.Reputation,
                Difficulty = ChallengeDifficulty.Normal,
                TargetCount = 100,
                GoldReward = 250,
                ExpReward = 500,
                ItemRewardIds = new List<int> { 411 }
            });
            
            // === BOSS CHALLENGES ===
            _allChallenges.Add(new DailyChallenge {
                Id = "kill_elite_boss",
                Name = "精英杀手",
                Description = "击败1个精英Boss",
                Type = ChallengeType.KillBoss,
                Difficulty = ChallengeDifficulty.Hard,
                TargetCount = 1,
                GoldReward = 500,
                ExpReward = 1500,
                ItemRewardIds = new List<int> { 201, 202 }
            });
            
            // === CRITICAL HIT CHALLENGES ===
            _allChallenges.Add(new DailyChallenge {
                Id = "critical_hits_10",
                Name = "暴击达人",
                Description = "暴击10次",
                Type = ChallengeType.CriticalHits,
                Difficulty = ChallengeDifficulty.Normal,
                TargetCount = 10,
                GoldReward = 180,
                ExpReward = 350,
                ItemRewardIds = new List<int> { 412 }
            });
            
            _allChallenges.Add(new DailyChallenge {
                Id = "critical_hits_50",
                Name = "暴击大师",
                Description = "暴击50次",
                Type = ChallengeType.CriticalHits,
                Difficulty = ChallengeDifficulty.Hard,
                TargetCount = 50,
                GoldReward = 350,
                ExpReward = 800,
                ItemRewardIds = new List<int> { 413 }
            });
            
            // === DODGE CHALLENGES ===
            _allChallenges.Add(new DailyChallenge {
                Id = "dodge_5_attacks",
                Name = "闪避专家",
                Description = "闪避5次攻击",
                Type = ChallengeType.Dodge,
                Difficulty = ChallengeDifficulty.Normal,
                TargetCount = 5,
                GoldReward = 150,
                ExpReward = 300,
                ItemRewardIds = new List<int> { 414 }
            });
            
            // === HEAL CHALLENGES ===
            _allChallenges.Add(new DailyChallenge {
                Id = "heal_500_hp",
                Name = "治疗大师",
                Description = "恢复500点生命值",
                Type = ChallengeType.Heal,
                Difficulty = ChallengeDifficulty.Easy,
                TargetCount = 500,
                GoldReward = 100,
                ExpReward = 200,
                ItemRewardIds = new List<int> { 415 }
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
