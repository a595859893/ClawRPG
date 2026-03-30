using Godot;
using System;
using System.Collections.Generic;

namespace ClawRPG.Systems.Achievement
{
    /// <summary>
    /// 成就奖励管理 - 处理成就奖励的发放
    /// </summary>
    public partial class AchievementRewards : BaseSystem
    {
        /// <summary>
        /// 奖励类型
        /// </summary>
        public enum RewardType
        {
            Gold,
            Item,
            Title,
            Badge,
            SkillPoint,
            Experience,
            Currency
        }
        
        /// <summary>
        /// 奖励数据
        /// </summary>
        public class RewardData
        {
            public RewardType Type { get; set; }
            public int Amount { get; set; }
            public string ItemId { get; set; }
            public string TitleId { get; set; }
            public string BadgeId { get; set; }
        }
        
        private Dictionary<string, List<RewardData>> _rewardTable = new Dictionary<string, List<RewardData>>();
        
        public override void _Ready()
        {
            base._Ready();
            InitializeRewardTable();
        }
        
        /// <summary>
        /// 初始化奖励表
        /// </summary>
        private void InitializeRewardTable()
        {
            // 击杀成就奖励
            _rewardTable["first_blood"] = new List<RewardData>
            {
                new RewardData { Type = RewardType.Gold, Amount = 100 },
                new RewardData { Type = RewardType.Experience, Amount = 50 }
            };
            
            _rewardTable["killer_novice"] = new List<RewardData>
            {
                new RewardData { Type = RewardType.Gold, Amount = 1000 },
                new RewardData { Type = RewardType.TitleId = "killer_novice" }
            };
            
            _rewardTable["killer_master"] = new List<RewardData>
            {
                new RewardData { Type = RewardType.Gold, Amount = 10000 },
                new RewardData { Type = RewardType.TitleId = "killer_master" }
            };
            
            _rewardTable["killer_legend"] = new List<RewardData>
            {
                new RewardData { Type = RewardType.Gold, Amount = 100000 },
                new RewardData { Type = RewardType.TitleId = "killer_legend" },
                new RewardData { Type = RewardType.BadgeId = "legendary_killer" }
            };
            
            // Boss击杀奖励
            _rewardTable["boss_slayer"] = new List<RewardData>
            {
                new RewardData { Type = RewardType.Gold, Amount = 5000 },
                new RewardData { Type = RewardType.TitleId = "boss_slayer" }
            };
            
            // PvP奖励
            _rewardTable["pvp_novice"] = new List<RewardData>
            {
                new RewardData { Type = RewardType.Gold, Amount = 500 }
            };
            
            _rewardTable["pvp_champion"] = new List<RewardData>
            {
                new RewardData { Type = RewardType.Gold, Amount = 50000 },
                new RewardData { Type = RewardType.TitleId = "pvp_champion" }
            };
            
            // 探索奖励
            _rewardTable["explorer_novice"] = new List<RewardData>
            {
                new RewardData { Type = RewardType.Gold, Amount = 500 },
                new RewardData { Type = RewardType.Experience, Amount = 200 }
            };
            
            // 爬塔奖励
            _rewardTable["tower_climber"] = new List<RewardData>
            {
                new RewardData { Type = RewardType.Gold, Amount = 5000 },
                new RewardData { Type = RewardType.ItemId = "tower_key" }
            };
            
            // 收集奖励
            _rewardTable["pet_collector_novice"] = new List<RewardData>
            {
                new RewardData { Type = RewardType.Gold, Amount = 1000 }
            };
            
            // 社交奖励
            _rewardTable["social_novice"] = new List<RewardData>
            {
                new RewardData { Type = RewardType.Gold, Amount = 200 }
            };
            
            // 经济奖励
            _rewardTable["shopaholic_novice"] = new List<RewardData>
            {
                new RewardData { Type = RewardType.Gold, Amount = 5000 }
            };
        }
        
        /// <summary>
        /// 发放成就奖励
        /// </summary>
        public void GrantRewards(string achievementId)
        {
            if (!_rewardTable.ContainsKey(achievementId))
            {
                GD.Print($"[AchievementRewards] No rewards for: {achievementId}");
                return;
            }
            
            var rewards = _rewardTable[achievementId];
            
            foreach (var reward in rewards)
            {
                GrantSingleReward(reward);
            }
            
            GD.Print($"[AchievementRewards] Rewards granted for: {achievementId}");
        }
        
        /// <summary>
        /// 发放单个奖励
        /// </summary>
        private void GrantSingleReward(RewardData reward)
        {
            switch (reward.Type)
            {
                case RewardType.Gold:
                    GrantGold(reward.Amount);
                    break;
                    
                case RewardType.Item:
                    GrantItem(reward.ItemId);
                    break;
                    
                case RewardType.Title:
                    GrantTitle(reward.TitleId);
                    break;
                    
                case RewardType.Badge:
                    GrantBadge(reward.BadgeId);
                    break;
                    
                case RewardType.SkillPoint:
                    GrantSkillPoint(reward.Amount);
                    break;
                    
                case RewardType.Experience:
                    GrantExperience(reward.Amount);
                    break;
                    
                case RewardType.Currency:
                    GrantCurrency(reward.Amount);
                    break;
            }
        }
        
        /// <summary>
        /// 发放金币
        /// </summary>
        private void GrantGold(int amount)
        {
            GD.Print($"[AchievementRewards] Granted {amount} gold");
            // 实际发放金币逻辑
        }
        
        /// <summary>
        /// 发放物品
        /// </summary>
        private void GrantItem(string itemId)
        {
            GD.Print($"[AchievementRewards] Granted item: {itemId}");
            // 实际发放物品逻辑
        }
        
        /// <summary>
        /// 发放称号
        /// </summary>
        private void GrantTitle(string titleId)
        {
            GD.Print($"[AchievementRewards] Granted title: {titleId}");
            // 实际发放称号逻辑
        }
        
        /// <summary>
        /// 发放徽章
        /// </summary>
        private void GrantBadge(string badgeId)
        {
            GD.Print($"[AchievementRewards] Granted badge: {badgeId}");
            // 实际发放徽章逻辑
        }
        
        /// <summary>
        /// 发放技能点
        /// </summary>
        private void GrantSkillPoint(int amount)
        {
            GD.Print($"[AchievementRewards] Granted {amount} skill points");
            // 实际发放技能点逻辑
        }
        
        /// <summary>
        /// 发放经验
        /// </summary>
        private void GrantExperience(int amount)
        {
            GD.Print($"[AchievementRewards] Granted {amount} experience");
            // 实际发放经验逻辑
        }
        
        /// <summary>
        /// 发放货币
        /// </summary>
        private void GrantCurrency(int amount)
        {
            GD.Print($"[AchievementRewards] Granted {amount} currency");
            // 实际发放货币逻辑
        }
        
        /// <summary>
        /// 获取成就奖励
        /// </summary>
        public List<RewardData> GetRewards(string achievementId)
        {
            return _rewardTable.ContainsKey(achievementId) 
                ? _rewardTable[achievementId] 
                : new List<RewardData>();
        }
        
        /// <summary>
        /// 添加自定义奖励
        /// </summary>
        public void AddCustomReward(string achievementId, RewardData reward)
        {
            if (!_rewardTable.ContainsKey(achievementId))
            {
                _rewardTable[achievementId] = new List<RewardData>();
            }
            
            _rewardTable[achievementId].Add(reward);
        }
        
        public override Dictionary<string, object> ExportSaveData()
        {
            var data = new Dictionary<string, object>();
            return data;
        }
        
        public override void ImportSaveData(Dictionary<string, object> data)
        {
            // 加载数据
        }
    }
}
