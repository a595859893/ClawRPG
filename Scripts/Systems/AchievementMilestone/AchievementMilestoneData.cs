using Godot;
using System;
using System.Collections.Generic;

namespace ClawRPG.Scripts.Systems
{
    /// <summary>
    /// 成就里程碑数据
    /// </summary>
    public class AchievementMilestoneData
    {
        // 里程碑记录: 成就ID -> 已达成里程碑等级
        public Dictionary<string, int> Milestones { get; set; } = new Dictionary<string, int>();
        
        // 历史记录
        public List<MilestoneHistoryEntry> History { get; set; } = new List<MilestoneHistoryEntry>();
        
        // 统计
        public int TotalMilestonesReached { get; set; }
        public int HighestMilestoneLevel { get; set; }
    }
    
    public class MilestoneHistoryEntry
    {
        public string AchievementId { get; set; }
        public string AchievementName { get; set; }
        public int MilestoneLevel { get; set; }
        public int Timestamp { get; set; }
    }
    
    /// <summary>
    /// 成就里程碑配置数据库
    /// </summary>
    public class AchievementMilestoneDatabase
    {
        private static AchievementMilestoneDatabase _instance;
        public static AchievementMilestoneDatabase Instance => _instance ??= new AchievementMilestoneDatabase();
        
        // 里程碑配置: 成就ID -> 里程碑等级列表
        public Dictionary<string, List<MilestoneConfig>> Milestones { get; private set; } = new Dictionary<string, List<MilestoneConfig>>();
        
        // 里程碑奖励模板
        public Dictionary<string, MilestoneReward> RewardTemplates { get; private set; } = new Dictionary<string, MilestoneReward>();
        
        private AchievementMilestoneDatabase()
        {
            InitializeMilestones();
            InitializeRewardTemplates();
        }
        
        private void InitializeMilestones()
        {
            // 击杀类成就里程碑
            Milestones["kill_enemies"] = new List<MilestoneConfig>
            {
                new MilestoneConfig { Level = 1, Threshold = 100, Reward = "kill_enemies_100" },
                new MilestoneConfig { Level = 2, Threshold = 500, Reward = "kill_enemies_500" },
                new MilestoneConfig { Level = 3, Threshold = 1000, Reward = "kill_enemies_1000" },
                new MilestoneConfig { Level = 4, Threshold = 5000, Reward = "kill_enemies_5000" },
                new MilestoneConfig { Level = 5, Threshold = 10000, Reward = "kill_enemies_10000" }
            };
            
            // 等级类成就里程碑
            Milestones["reach_level"] = new List<MilestoneConfig>
            {
                new MilestoneConfig { Level = 1, Threshold = 10, Reward = "reach_level_10" },
                new MilestoneConfig { Level = 2, Threshold = 25, Reward = "reach_level_25" },
                new MilestoneConfig { Level = 3, Threshold = 50, Reward = "reach_level_50" },
                new MilestoneConfig { Level = 4, Threshold = 75, Reward = "reach_level_75" },
                new MilestoneConfig { Level = 5, Threshold = 100, Reward = "reach_level_100" }
            };
            
            // 金币类成就里程碑
            Milestones["earn_gold"] = new List<MilestoneConfig>
            {
                new MilestoneConfig { Level = 1, Threshold = 1000, Reward = "earn_gold_1k" },
                new MilestoneConfig { Level = 2, Threshold = 10000, Reward = "earn_gold_10k" },
                new MilestoneConfig { Level = 3, Threshold = 100000, Reward = "earn_gold_100k" },
                new MilestoneConfig { Level = 4, Threshold = 1000000, Reward = "earn_gold_1m" },
                new MilestoneConfig { Level = 5, Threshold = 10000000, Reward = "earn_gold_10m" }
            };
            
            // Boss击杀类成就里程碑
            Milestones["kill_bosses"] = new List<MilestoneConfig>
            {
                new MilestoneConfig { Level = 1, Threshold = 5, Reward = "kill_bosses_5" },
                new MilestoneConfig { Level = 2, Threshold = 25, Reward = "kill_bosses_25" },
                new MilestoneConfig { Level = 3, Threshold = 50, Reward = "kill_bosses_50" },
                new MilestoneConfig { Level = 4, Threshold = 100, Reward = "kill_bosses_100" },
                new MilestoneConfig { Level = 5, Threshold = 500, Reward = "kill_bosses_500" }
            };
            
            // 地下城楼层类成就里程碑
            Milestones["reach_dungeon_floor"] = new List<MilestoneConfig>
            {
                new MilestoneConfig { Level = 1, Threshold = 10, Reward = "floor_10" },
                new MilestoneConfig { Level = 2, Threshold = 25, Reward = "floor_25" },
                new MilestoneConfig { Level = 3, Threshold = 50, Reward = "floor_50" },
                new MilestoneConfig { Level = 4, Threshold = 75, Reward = "floor_75" },
                new MilestoneConfig { Level = 5, Threshold = 100, Reward = "floor_100" }
            };
            
            // 宠物数量类成就里程碑
            Milestones["collect_pets"] = new List<MilestoneConfig>
            {
                new MilestoneConfig { Level = 1, Threshold = 5, Reward = "collect_pets_5" },
                new MilestoneConfig { Level = 2, Threshold = 15, Reward = "collect_pets_15" },
                new MilestoneConfig { Level = 3, Threshold = 30, Reward = "collect_pets_30" },
                new MilestoneConfig { Level = 4, Threshold = 50, Reward = "collect_pets_50" },
                new MilestoneConfig { Level = 5, Threshold = 100, Reward = "collect_pets_100" }
            };
            
            // 装备收集类成就里程碑
            Milestones["collect_equipment"] = new List<MilestoneConfig>
            {
                new MilestoneConfig { Level = 1, Threshold = 10, Reward = "collect_equip_10" },
                new MilestoneConfig { Level = 2, Threshold = 50, Reward = "collect_equip_50" },
                new MilestoneConfig { Level = 3, Threshold = 100, Reward = "collect_equip_100" },
                new MilestoneConfig { Level = 4, Threshold = 250, Reward = "collect_equip_250" },
                new MilestoneConfig { Level = 5, Threshold = 500, Reward = "collect_equip_500" }
            };
            
            // 技能树节点解锁类成就里程碑
            Milestones["unlock_skill_nodes"] = new List<MilestoneConfig>
            {
                new MilestoneConfig { Level = 1, Threshold = 10, Reward = "skill_nodes_10" },
                new MilestoneConfig { Level = 2, Threshold = 25, Reward = "skill_nodes_25" },
                new MilestoneConfig { Level = 3, Threshold = 50, Reward = "skill_nodes_50" },
                new MilestoneConfig { Level = 4, Threshold = 75, Reward = "skill_nodes_75" },
                new MilestoneConfig { Level = 5, Threshold = 100, Reward = "skill_nodes_100" }
            };
        }
        
        private void InitializeRewardTemplates()
        {
            // 金币奖励
            RewardTemplates["gold_100"] = new MilestoneReward { Type = "gold", Value = 100 };
            RewardTemplates["gold_500"] = new MilestoneReward { Type = "gold", Value = 500 };
            RewardTemplates["gold_1000"] = new MilestoneReward { Type = "gold", Value = 1000 };
            RewardTemplates["gold_5000"] = new MilestoneReward { Type = "gold", Value = 5000 };
            RewardTemplates["gold_10000"] = new MilestoneReward { Type = "gold", Value = 10000 };
            
            // 经验奖励
            RewardTemplates["exp_100"] = new MilestoneReward { Type = "exp", Value = 100 };
            RewardTemplates["exp_500"] = new MilestoneReward { Type = "exp", Value = 500 };
            RewardTemplates["exp_1000"] = new MilestoneReward { Type = "exp", Value = 1000 };
            RewardTemplates["exp_5000"] = new MilestoneReward { Type = "exp", Value = 5000 };
            
            // 钻石奖励
            RewardTemplates["gem_10"] = new MilestoneReward { Type = "gem", Value = 10 };
            RewardTemplates["gem_25"] = new MilestoneReward { Type = "gem", Value = 25 };
            RewardTemplates["gem_50"] = new MilestoneReward { Type = "gem", Value = 50 };
            RewardTemplates["gem_100"] = new MilestoneReward { Type = "gem", Value = 100 };
        }
        
        public List<MilestoneConfig> GetMilestones(string achievementId)
        {
            return Milestones.ContainsKey(achievementId) ? Milestones[achievementId] : new List<MilestoneConfig>();
        }
        
        public MilestoneReward GetReward(string rewardId)
        {
            return RewardTemplates.ContainsKey(rewardId) ? RewardTemplates[rewardId] : null;
        }
    }
    
    public class MilestoneConfig
    {
        public int Level { get; set; }
        public int Threshold { get; set; }
        public string Reward { get; set; }
    }
    
    public class MilestoneReward
    {
        public string Type { get; set; }  // gold, exp, gem, item
        public int Value { get; set; }
        public string ItemId { get; set; }
    }
}
