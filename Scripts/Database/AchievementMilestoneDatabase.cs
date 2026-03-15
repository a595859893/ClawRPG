using System;
using System.Collections.Generic;

namespace ClawRPG.Scripts
{
    /// <summary>
    /// 成就里程碑系统 - 配置数据库
    /// </summary>
    public class AchievementMilestoneDatabase
    {
        // 成就配置
        public Dictionary<string, AchievementConfig> Achievements { get; private set; }
        
        // 里程碑配置
        public Dictionary<string, MilestoneConfig> Milestones { get; private set; }
        
        // 奖励配置
        public Dictionary<string, RewardConfig> Rewards { get; private set; }
        
        public AchievementMilestoneDatabase()
        {
            Achievements = new Dictionary<string, Achievement>();
            Milestones = new Dictionary<string, MilestoneConfig>();
            Rewards = new Dictionary<string, RewardConfig>();
            InitializeAchievements();
            InitializeMilestones();
            InitializeRewards();
        }
        
        private void InitializeAchievements()
        {
            // 战斗成就
            AddAchievement("ach_kill_100", "初出茅庐", "击杀100个敌人", AchievementType.Combat, AchievementRarity.Common, 100, "sword", new List<string> { "gold_100", "exp_50" });
            AddAchievement("ach_kill_500", "小有名气", "击杀500个敌人", AchievementType.Combat, AchievementRarity.Uncommon, 500, "sword", new List<string> { "gold_500", "exp_200" });
            AddAchievement("ach_kill_1000", "战斗老手", "击杀1000个敌人", AchievementType.Combat, AchievementRarity.Rare, 1000, "sword", new List<string> { "gold_1000", "exp_500" });
            AddAchievement("ach_kill_5000", "战场之魂", "击杀5000个敌人", AchievementType.Combat, AchievementRarity.Epic, 5000, "sword", new List<string> { "gold_5000", "exp_2000" });
            AddAchievement("ach_kill_10000", "传奇战士", "击杀10000个敌人", AchievementType.Combat, AchievementRarity.Legendary, 10000, "sword", new List<string> { "gold_10000", "exp_5000", "title_legendary_warrior" });
            
            // Boss成就
            AddAchievement("ach_boss_1", "Boss杀手", "击败1个Boss", AchievementType.Combat, AchievementRarity.Common, 1, "skull", new List<string> { "gold_200", "exp_100" });
            AddAchievement("ach_boss_10", "Boss猎手", "击败10个Boss", AchievementType.Combat, AchievementRarity.Uncommon, 10, "skull", new List<string> { "gold_1000", "exp_500" });
            AddAchievement("ach_boss_50", "Boss终结者", "击败50个Boss", AchievementType.Combat, AchievementRarity.Rare, 50, "skull", new List<string> { "gold_5000", "exp_2000" });
            AddAchievement("ach_boss_100", "Boss克星", "击败100个Boss", AchievementType.Combat, AchievementRarity.Epic, 100, "skull", new List<string> { "gold_10000", "exp_5000" });
            
            // 探索成就
            AddAchievement("ach_explore_5", "初探世界", "探索5个不同区域", AchievementType.Exploration, AchievementRarity.Common, 5, "compass", new List<string> { "gold_100", "exp_50" });
            AddAchievement("ach_explore_15", "地图绘制者", "探索15个不同区域", AchievementType.Exploration, AchievementRarity.Uncommon, 15, "compass", new List<string> { "gold_500", "exp_200" });
            AddAchievement("ach_explore_30", "探险达人", "探索30个不同区域", AchievementType.Exploration, AchievementRarity.Rare, 30, "compass", new List<string> { "gold_1000", "exp_500" });
            AddAchievement("ach_explore_all", "世界征服者", "探索所有区域", AchievementType.Exploration, AchievementRarity.Legendary, 50, "compass", new List<string> { "gold_5000", "exp_2000", "title_explorer" });
            
            // 制作成就
            AddAchievement("ach_craft_10", "新手匠人", "制作10件物品", AchievementType.Crafting, AchievementRarity.Common, 10, "hammer", new List<string> { "gold_100", "exp_50" });
            AddAchievement("ach_craft_50", "熟练工匠", "制作50件物品", AchievementType.Crafting, AchievementRarity.Uncommon, 50, "hammer", new List<string> { "gold_500", "exp_200" });
            AddAchievement("ach_craft_200", "工艺大师", "制作200件物品", AchievementType.Crafting, AchievementRarity.Rare, 200, "hammer", new List<string> { "gold_2000", "exp_1000" });
            AddAchievement("ach_craft_500", "传奇匠神", "制作500件物品", AchievementType.Crafting, AchievementRarity.Epic, 500, "hammer", new List<string> { "gold_5000", "exp_2000" });
            AddAchievement("ach_craft_1000", "天工巧匠", "制作1000件物品", AchievementType.Crafting, AchievementRarity.Legendary, 1000, "hammer", new List<string> { "gold_10000", "exp_5000", "title_master_crafter" });
            
            // 收集成就
            AddAchievement("ach_collect_50", "收藏爱好者", "收集50件物品", AchievementType.Collection, AchievementRarity.Common, 50, "chest", new List<string> { "gold_100", "exp_50" });
            AddAchievement("ach_collect_200", "收藏家", "收集200件物品", AchievementType.Collection, AchievementRarity.Uncommon, 200, "chest", new List<string> { "gold_500", "exp_200" });
            AddAchievement("ach_collect_500", "珍品收藏家", "收集500件物品", AchievementType.Collection, AchievementRarity.Rare, 500, "chest", new List<string> { "gold_2000", "exp_1000" });
            AddAchievement("ach_collect_1000", "稀世收藏家", "收集1000件物品", AchievementType.Collection, AchievementRarity.Epic, 1000, "chest", new List<string> { "gold_5000", "exp_2000" });
            AddAchievement("ach_collect_legendary", "传奇收藏家", "收集所有传说物品", AchievementType.Collection, AchievementRarity.Legendary, 50, "chest", new List<string> { "gold_10000", "exp_5000", "title_collector" });
            
            // 社交成就
            AddAchievement("ach_friend_5", "广交朋友", "添加5个好友", AchievementType.Social, AchievementRarity.Common, 5, "users", new List<string> { "gold_100", "exp_50" });
            AddAchievement("ach_friend_20", "社交达人", "添加20个好友", AchievementType.Social, AchievementRarity.Uncommon, 20, "users", new List<string> { "gold_500", "exp_200" });
            AddAchievement("ach_friend_50", "人脉广泛", "添加50个好友", AchievementType.Social, AchievementRarity.Rare, 50, "users", new List<string> { "gold_1000", "exp_500" });
            
            // 进度成就
            AddAchievement("ach_level_10", "初窥门径", "达到10级", AchievementType.Progress, AchievementRarity.Common, 10, "star", new List<string> { "gold_200", "exp_100" });
            AddAchievement("ach_level_30", "小有所成", "达到30级", AchievementType.Progress, AchievementRarity.Uncommon, 30, "star", new List<string> { "gold_1000", "exp_500" });
            AddAchievement("ach_level_50", "炉火纯青", "达到50级", AchievementType.Progress, AchievementRarity.Rare, 50, "star", new List<string> { "gold_3000", "exp_1500" });
            AddAchievement("ach_level_80", "登堂入室", "达到80级", AchievementType.Progress, AchievementRarity.Epic, 80, "star", new List<string> { "gold_8000", "exp_4000" });
            AddAchievement("ach_level_100", "超凡入圣", "达到100级", AchievementType.Progress, AchievementRarity.Legendary, 100, "star", new List<string> { "gold_20000", "exp_10000", "title_level_100" });
            
            // 金币成就
            AddAchievement("ach_gold_10000", "小有积蓄", "拥有10000金币", AchievementType.Progress, AchievementRarity.Common, 10000, "coin", new List<string> { "exp_100" });
            AddAchievement("ach_gold_100000", "富甲一方", "拥有100000金币", AchievementType.Progress, AchievementRarity.Uncommon, 100000, "coin", new List<string> { "exp_500" });
            AddAchievement("ach_gold_1000000", "腰缠万贯", "拥有1000000金币", AchievementType.Progress, AchievementRarity.Rare, 1000000, "coin", new List<string> { "exp_2000" });
            AddAchievement("ach_gold_10000000", "富可敌国", "拥有10000000金币", AchievementType.Progress, AchievementRarity.Epic, 10000000, "coin", new List<string> { "exp_5000" });
            
            // 挑战成就
            AddAchievement("ach_dungeon_1", "地下城初体验", "完成1个地下城", AchievementType.Challenge, AchievementRarity.Common, 1, "dungeon", new List<string> { "gold_200", "exp_100" });
            AddAchievement("ach_dungeon_25", "地下城探索者", "完成25个地下城", AchievementType.Challenge, AchievementRarity.Uncommon, 25, "dungeon", new List<string> { "gold_1000", "exp_500" });
            AddAchievement("ach_dungeon_100", "地下城大师", "完成100个地下城", AchievementType.Challenge, AchievementRarity.Rare, 100, "dungeon", new List<string> { "gold_5000", "exp_2000" });
            
            // 特殊成就
            AddAchievement("ach_first_blood", "第一滴血", "完成首次击杀", AchievementType.Special, AchievementRarity.Common, 1, "trophy", new List<string> { "gold_50", "exp_25" });
            AddAchievement("ach_first_dungeon", "初次探险", "完成第一个地下城", AchievementType.Special, AchievementRarity.Common, 1, "dungeon", new List<string> { "gold_100", "exp_50" });
            AddAchievement("ach_first_pvp", "战场初体验", "赢得第一场PVP", AchievementType.Special, AchievementRarity.Common, 1, "swords", new List<string> { "gold_100", "exp_50" });
            AddAchievement("ach_no_death", "不死传说", "连续100级无死亡", AchievementType.Special, AchievementRarity.Epic, 100, "shield", new List<string> { "gold_10000", "exp_5000", "title_immortal" });
            AddAchievement("ach_perfect_dungeon", "完美通关", "无伤完成地下城", AchievementType.Special, AchievementRarity.Rare, 1, "star", new List<string> { "gold_2000", "exp_1000" });
        }
        
        private void InitializeMilestones()
        {
            // 击杀里程碑
            AddMilestone("ms_kill_100", "百人斩", "击杀100个敌人", 100, "sword", new List<string> { "gold_500", "exp_200" });
            AddMilestone("ms_kill_500", "五百人斩", "击杀500个敌人", 500, "sword", new List<string> { "gold_2000", "exp_800" });
            AddMilestone("ms_kill_1000", "千人斩", "击杀1000个敌人", 1000, "sword", new List<string> { "gold_5000", "exp_2000" });
            AddMilestone("ms_kill_5000", "五千人斩", "击杀5000个敌人", 5000, "sword", new List<string> { "gold_10000", "exp_5000" });
            AddMilestone("ms_kill_10000", "万人斩", "击杀10000个敌人", 10000, "sword", new List<string> { "gold_20000", "exp_10000", "title_ten_thousand_kills" });
            
            // Boss击杀里程碑
            AddMilestone("ms_boss_10", "Boss猎人", "击败10个Boss", 10, "skull", new List<string> { "gold_2000", "exp_1000" });
            AddMilestone("ms_boss_50", "Boss杀手", "击败50个Boss", 50, "skull", new List<string> { "gold_5000", "exp_2500" });
            AddMilestone("ms_boss_100", "Boss克星", "击败100个Boss", 100, "skull", new List<string> { "gold_10000", "exp_5000" });
            
            // 地下城里程碑
            AddMilestone("ms_dungeon_10", "地下城新人", "完成10个地下城", 10, "dungeon", new List<string> { "gold_1000", "exp_500" });
            AddMilestone("ms_dungeon_50", "地下城老手", "完成50个地下城", 50, "dungeon", new List<string> { "gold_3000", "exp_1500" });
            AddMilestone("ms_dungeon_100", "地下城大师", "完成100个地下城", 100, "dungeon", new List<string> { "gold_8000", "exp_4000" });
            
            // 制作里程碑
            AddMilestone("ms_craft_100", "百炼成钢", "制作100件物品", 100, "hammer", new List<string> { "gold_1000", "exp_500" });
            AddMilestone("ms_craft_500", "巧夺天工", "制作500件物品", 500, "hammer", new List<string> { "gold_3000", "exp_1500" });
            AddMilestone("ms_craft_1000", "鬼斧神工", "制作1000件物品", 1000, "hammer", new List<string> { "gold_8000", "exp_4000" });
            
            // 等级里程碑
            AddMilestone("ms_level_25", "25级里程碑", "达到25级", 25, "star", new List<string> { "gold_1000", "exp_500" });
            AddMilestone("ms_level_50", "50级里程碑", "达到50级", 50, "star", new List<string> { "gold_3000", "exp_1500" });
            AddMilestone("ms_level_75", "75级里程碑", "达到75级", 75, "star", new List<string> { "gold_8000", "exp_4000" });
            AddMilestone("ms_level_100", "100级里程碑", "达到100级", 100, "star", new List<string> { "gold_20000", "exp_10000", "title_max_level" });
            
            // 金币里程碑
            AddMilestone("ms_gold_50000", "五万富翁", "拥有50000金币", 50000, "coin", new List<string> { "exp_500" });
            AddMilestone("ms_gold_500000", "五十万富翁", "拥有500000金币", 500000, "coin", new List<string> { "exp_2000" });
            AddMilestone("ms_gold_5000000", "五百万富翁", "拥有5000000金币", 5000000, "coin", new List<string> { "exp_5000" });
        }
        
        private void InitializeRewards()
        {
            // 金币奖励
            AddReward("gold_50", RewardType.Gold, 50, "50金币");
            AddReward("gold_100", RewardType.Gold, 100, "100金币");
            AddReward("gold_200", RewardType.Gold, 200, "200金币");
            AddReward("gold_500", RewardType.Gold, 500, "500金币");
            AddReward("gold_1000", RewardType.Gold, 1000, "1000金币");
            AddReward("gold_2000", RewardType.Gold, 2000, "2000金币");
            AddReward("gold_3000", RewardType.Gold, 3000, "3000金币");
            AddReward("gold_5000", RewardType.Gold, 5000, "5000金币");
            AddReward("gold_8000", RewardType.Gold, 8000, "8000金币");
            AddReward("gold_10000", RewardType.Gold, 10000, "10000金币");
            AddReward("gold_20000", RewardType.Gold, 20000, "20000金币");
            
            // 经验奖励
            AddReward("exp_25", RewardType.Experience, 25, "25经验");
            AddReward("exp_50", RewardType.Experience, 50, "50经验");
            AddReward("exp_100", RewardType.Experience, 100, "100经验");
            AddReward("exp_200", RewardType.Experience, 200, "200经验");
            AddReward("exp_500", RewardType.Experience, 500, "500经验");
            AddReward("exp_1000", RewardType.Experience, 1000, "1000经验");
            AddReward("exp_1500", RewardType.Experience, 1500, "1500经验");
            AddReward("exp_2000", RewardType.Experience, 2000, "2000经验");
            AddReward("exp_4000", RewardType.Experience, 4000, "4000经验");
            AddReward("exp_5000", RewardType.Experience, 5000, "5000经验");
            AddReward("exp_10000", RewardType.Experience, 10000, "10000经验");
        }
        
        private void AddAchievement(string id, string name, string description, AchievementType type, AchievementRarity rarity, int required, string icon, List<string> rewards)
        {
            var achievement = new Achievement
            {
                ID = id,
                Name = name,
                Description = description,
                Type = type,
                Rarity = rarity,
                RequiredValue = required,
                IconName = icon,
                Rewards = rewards
            };
            Achievements[id] = achievement;
        }
        
        private void AddMilestone(string id, string name, string description, int required, string icon, List<string> rewards)
        {
            var milestone = new Milestone
            {
                ID = id,
                Name = name,
                Description = description,
                RequiredCount = required,
                IconName = icon,
                Rewards = rewards
            };
            Milestones[id] = milestone;
        }
        
        private void AddReward(string id, RewardType type, int amount, string description)
        {
            var reward = new RewardConfig
            {
                ID = id,
                Type = type,
                Amount = amount,
                Description = description
            };
            Rewards[id] = reward;
        }
        
        public Achievement GetAchievementTemplate(string id)
        {
            return Achievements.ContainsKey(id) ? Achievements[id] : null;
        }
        
        public MilestoneConfig GetMilestoneTemplate(string id)
        {
            return Milestones.ContainsKey(id) ? Milestones[id] : null;
        }
        
        public RewardConfig GetRewardConfig(string id)
        {
            return Rewards.ContainsKey(id) ? Rewards[id] : null;
        }
        
        public List<Achievement> GetAchievementsByType(AchievementType type)
        {
            var result = new List<Achievement>();
            foreach (var ach in Achievements.Values)
            {
                if (ach.Type == type)
                    result.Add(ach);
            }
            return result;
        }
        
        public List<Milestone> GetMilestones()
        {
            return new List<Milestone>(Milestones.Values);
        }
    }
    
    // 配置数据结构
    public class AchievementConfig : Achievement { }
    public class MilestoneConfig : Milestone { }
    
    // 奖励类型
    public enum RewardType
    {
        Gold,
        Experience,
        Item,
        Title
    }
    
    // 奖励配置
    public class RewardConfig
    {
        public string ID { get; set; }
        public RewardType Type { get; set; }
        public int Amount { get; set; }
        public string Description { get; set; }
    }
}
