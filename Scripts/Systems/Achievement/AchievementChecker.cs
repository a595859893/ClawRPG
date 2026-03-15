using Godot;
using System;
using System.Collections.Generic;

namespace ClawRPG.Systems.Achievement
{
    /// <summary>
    /// 成就检测器 - 检查成就解锁条件
    /// </summary>
    public partial class AchievementChecker : BaseSystem
    {
        /// <summary>
        /// 检测结果
        /// </summary>
        public class CheckResult
        {
            public string AchievementId { get; set; }
            public bool ShouldUnlock { get; set; }
            public int CurrentProgress { get; set; }
            public int TargetProgress { get; set; }
        }
        
        private AchievementSystem _achievementSystem;
        
        public override void _Ready()
        {
            base._Ready();
        }
        
        /// <summary>
        /// 设置成就系统引用
        /// </summary>
        public void SetAchievementSystem(AchievementSystem system)
        {
            _achievementSystem = system;
        }
        
        /// <summary>
        /// 检查所有成就
        /// </summary>
        public List<CheckResult> CheckAllAchievements()
        {
            var results = new List<CheckResult>();
            
            // 检查击杀成就
            results.Add(CheckKillAchievements());
            
            // 检查探索成就
            results.Add(CheckZoneDiscoveryAchievements());
            
            // 检查收集成就
            results.Add(CheckCollectionAchievements());
            
            // 检查社交成就
            results.Add(CheckSocialAchievements());
            
            // 检查经济成就
            results.Add(CheckEconomyAchievements());
            
            return results;
        }
        
        /// <summary>
        /// 检查击杀成就
        /// </summary>
        private CheckResult CheckKillAchievements()
        {
            var result = new CheckResult { AchievementId = "" };
            
            // 简单击杀成就检测
            int totalKills = _achievementSystem.GetTotalKills();
            
            if (totalKills >= 10000)
                result.AchievementId = "killer_legend";
            else if (totalKills >= 1000)
                result.AchievementId = "killer_master";
            else if (totalKills >= 100)
                result.AchievementId = "killer_novice";
            
            result.CurrentProgress = totalKills;
            result.TargetProgress = 10000;
            result.ShouldUnlock = totalKills >= 100;
            
            return result;
        }
        
        /// <summary>
        /// 检查区域探索成就
        /// </summary>
        private CheckResult CheckZoneDiscoveryAchievements()
        {
            var result = new CheckResult { AchievementId = "" };
            
            int zonesDiscovered = _achievementSystem.GetZonesDiscovered();
            
            if (zonesDiscovered >= 50)
                result.AchievementId = "explorer_legend";
            else if (zonesDiscovered >= 20)
                result.AchievementId = "explorer_master";
            else if (zonesDiscovered >= 5)
                result.AchievementId = "explorer_novice";
            
            result.CurrentProgress = zonesDiscovered;
            result.TargetProgress = 50;
            result.ShouldUnlock = zonesDiscovered >= 5;
            
            return result;
        }
        
        /// <summary>
        /// 检查收集成就
        /// </summary>
        private CheckResult CheckCollectionAchievements()
        {
            var result = new CheckResult { AchievementId = "" };
            
            int pets = _achievementSystem.GetPetsCollected();
            int mounts = _achievementSystem.GetMountsCollected();
            int equipment = _achievementSystem.GetEquipmentCollected();
            
            // 宠物收集
            if (pets >= 50)
                result.AchievementId = "pet_collector_legend";
            else if (pets >= 20)
                result.AchievementId = "pet_collector_master";
            else if (pets >= 5)
                result.AchievementId = "pet_collector_novice";
            
            result.CurrentProgress = pets;
            result.TargetProgress = 50;
            result.ShouldUnlock = pets >= 5;
            
            return result;
        }
        
        /// <summary>
        /// 检查社交成就
        /// </summary>
        private CheckResult CheckSocialAchievements()
        {
            var result = new CheckResult { AchievementId = "" };
            
            int friends = _achievementSystem.GetFriendsMade();
            
            if (friends >= 100)
                result.AchievementId = "social_butterfly";
            else if (friends >= 50)
                result.AchievementId = "social_person";
            else if (friends >= 10)
                result.AchievementId = "social_novice";
            
            result.CurrentProgress = friends;
            result.TargetProgress = 100;
            result.ShouldUnlock = friends >= 10;
            
            return result;
        }
        
        /// <summary>
        /// 检查经济成就
        /// </summary>
        private CheckResult CheckEconomyAchievements()
        {
            var result = new CheckResult { AchievementId = "" };
            
            int goldSpent = _achievementSystem.GetGoldSpent();
            
            if (goldSpent >= 1000000)
                result.AchievementId = "shopaholic_legend";
            else if (goldSpent >= 100000)
                result.AchievementId = "shopaholic_master";
            else if (goldSpent >= 10000)
                result.AchievementId = "shopaholic_novice";
            
            result.CurrentProgress = goldSpent;
            result.TargetProgress = 1000000;
            result.ShouldUnlock = goldSpent >= 10000;
            
            return result;
        }
        
        /// <summary>
        /// 检查单个成就
        /// </summary>
        public bool CheckSingleAchievement(string achievementId)
        {
            switch (achievementId)
            {
                case "first_blood":
                    return _achievementSystem.GetTotalKills() >= 1;
                    
                case "killer_novice":
                    return _achievementSystem.GetTotalKills() >= 100;
                    
                case "killer_master":
                    return _achievementSystem.GetTotalKills() >= 1000;
                    
                case "killer_legend":
                    return _achievementSystem.GetTotalKills() >= 10000;
                    
                case "boss_slayer":
                    return _achievementSystem.GetBossKills() >= 10;
                    
                case "pvp_champion":
                    return _achievementSystem.GetPvpWins() >= 100;
                    
                case "explorer_novice":
                    return _achievementSystem.GetZonesDiscovered() >= 5;
                    
                case "tower_climber":
                    return _achievementSystem.GetSealedTowerFloor() >= 50;
                    
                default:
                    return false;
            }
        }
        
        /// <summary>
        /// 检查类别完成度
        /// </summary>
        public float CheckCategoryCompletion(AchievementData.AchievementCategory category)
        {
            // 简化实现
            return 0.0f;
        }
        
        public override Dictionary ExportSaveData()
        {
            var data = new Dictionary();
            return data;
        }
        
        public override void ImportSaveData(Dictionary data)
        {
            // 加载数据
        }
    }
}
