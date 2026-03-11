using System;
using System.Collections.Generic;

namespace ClawRPG.Scripts.Systems.PetFoster
{
    /// <summary>
    /// 宠物寄养数据库
    /// </summary>
    public static class PetFosterDatabase
    {
        private static Dictionary<string, FosterConfig> _fosterConfigs;
        
        public static void Initialize()
        {
            _fosterConfigs = new Dictionary<string, FosterConfig>();
            LoadDefaultConfigs();
        }
        
        private static void LoadDefaultConfigs()
        {
            // 休息类型 - 恢复饱食度
            AddConfig(new FosterConfig
            {
                Id = "rest_short",
                Name = "短暂休息",
                Type = FosterType.Rest,
                Duration = 60,
                Cost = 10,
                MinPetLevel = 1,
                ExpReward = 5,
                GoldReward = 0,
                AffectionReward = 2
            });
            
            AddConfig(new FosterConfig
            {
                Id = "rest_medium",
                Name = "充分休息",
                Type = FosterType.Rest,
                Duration = 300,
                Cost = 40,
                MinPetLevel = 1,
                ExpReward = 20,
                GoldReward = 0,
                AffectionReward = 8
            });
            
            AddConfig(new FosterConfig
            {
                Id = "rest_long",
                Name = "深度休息",
                Type = FosterType.Rest,
                Duration = 600,
                Cost = 60,
                MinPetLevel = 5,
                ExpReward = 40,
                GoldReward = 0,
                AffectionReward = 15
            });
            
            // 训练类型 - 获得经验
            AddConfig(new FosterConfig
            {
                Id = "training_basic",
                Name = "基础训练",
                Type = FosterType.Training,
                Duration = 120,
                Cost = 25,
                MinPetLevel = 1,
                ExpReward = 50,
                GoldReward = 0,
                AffectionReward = 5,
                MaterialRewards = new List<string> { "training_manual" },
                MaterialDropChance = 0.3f
            });
            
            AddConfig(new FosterConfig
            {
                Id = "training_advanced",
                Name = "强化训练",
                Type = FosterType.Training,
                Duration = 300,
                Cost = 80,
                MinPetLevel = 10,
                ExpReward = 150,
                GoldReward = 0,
                AffectionReward = 10,
                MaterialRewards = new List<string> { "training_manual", "training_token" },
                MaterialDropChance = 0.5f
            });
            
            AddConfig(new FosterConfig
            {
                Id = "training_elite",
                Name = "精英训练",
                Type = FosterType.Training,
                Duration = 600,
                Cost = 200,
                MinPetLevel = 25,
                ExpReward = 400,
                GoldReward = 0,
                AffectionReward = 20,
                MaterialRewards = new List<string> { "training_token", "evolution_stone" },
                MaterialDropChance = 0.7f
            });
            
            // 采集类型 - 获得材料
            AddConfig(new FosterConfig
            {
                Id = "gathering_basic",
                Name = "基础采集",
                Type = FosterType.Gathering,
                Duration = 180,
                Cost = 15,
                MinPetLevel = 1,
                ExpReward = 30,
                GoldReward = 0,
                AffectionReward = 3,
                MaterialRewards = new List<string> { "herb_common", "ore_common" },
                MaterialDropChance = 0.8f
            });
            
            AddConfig(new FosterConfig
            {
                Id = "gathering_advanced",
                Name = "高级采集",
                Type = FosterType.Gathering,
                Duration = 360,
                Cost = 50,
                MinPetLevel = 15,
                ExpReward = 80,
                GoldReward = 0,
                AffectionReward = 8,
                MaterialRewards = new List<string> { "herb_rare", "ore_rare", "crystal_common" },
                MaterialDropChance = 0.7f
            });
            
            AddConfig(new FosterConfig
            {
                Id = "gathering_expert",
                Name = "专家采集",
                Type = FosterType.Gathering,
                Duration = 600,
                Cost = 120,
                MinPetLevel = 30,
                ExpReward = 150,
                GoldReward = 0,
                AffectionReward = 15,
                MaterialRewards = new List<string> { "herb_epic", "ore_epic", "crystal_rare", "dragon_scale" },
                MaterialDropChance = 0.6f
            });
            
            // 玩耍类型 - 提升好感度
            AddConfig(new FosterConfig
            {
                Id = "play_short",
                Name = "轻度玩耍",
                Type = FosterType.Play,
                Duration = 90,
                Cost = 20,
                MinPetLevel = 1,
                ExpReward = 15,
                GoldReward = 0,
                AffectionReward = 20
            });
            
            AddConfig(new FosterConfig
            {
                Id = "play_medium",
                Name = "开心玩耍",
                Type = FosterType.Play,
                Duration = 240,
                Cost = 45,
                MinPetLevel = 5,
                ExpReward = 40,
                GoldReward = 0,
                AffectionReward = 50
            });
            
            AddConfig(new FosterConfig
            {
                Id = "play_extensive",
                Name = "尽情玩耍",
                Type = FosterType.Play,
                Duration = 480,
                Cost = 80,
                MinPetLevel = 15,
                ExpReward = 80,
                GoldReward = 0,
                AffectionReward = 100
            });
            
            // 守护类型 - 获得金币
            AddConfig(new FosterConfig
            {
                Id = "guard_short",
                Name = "短期守护",
                Type = FosterType.Guard,
                Duration = 120,
                Cost = 10,
                MinPetLevel = 1,
                ExpReward = 10,
                GoldReward = 30,
                AffectionReward = 2
            });
            
            AddConfig(new FosterConfig
            {
                Id = "guard_medium",
                Name = "中期守护",
                Type = FosterType.Guard,
                Duration = 300,
                Cost = 30,
                MinPetLevel = 10,
                ExpReward = 30,
                GoldReward = 100,
                AffectionReward = 5
            });
            
            AddConfig(new FosterConfig
            {
                Id = "guard_long",
                Name = "长期守护",
                Type = FosterType.Guard,
                Duration = 600,
                Cost = 60,
                MinPetLevel = 20,
                ExpReward = 60,
                GoldReward = 300,
                AffectionReward = 10
            });
            
            AddConfig(new FosterConfig
            {
                Id = "guard_epic",
                Name = "传奇守护",
                Type = FosterType.Guard,
                Duration = 1200,
                Cost = 150,
                MinPetLevel = 35,
                ExpReward = 150,
                GoldReward = 1000,
                AffectionReward = 25
            });
        }
        
        private static void AddConfig(FosterConfig config)
        {
            _fosterConfigs[config.Id] = config;
        }
        
        public static FosterConfig GetConfig(string id)
        {
            if (_fosterConfigs.ContainsKey(id))
                return _fosterConfigs[id];
            return null;
        }
        
        public static List<FosterConfig> GetConfigsByType(FosterType type)
        {
            var result = new List<FosterConfig>();
            foreach (var config in _fosterConfigs.Values)
            {
                if (config.Type == type)
                    result.Add(config);
            }
            return result;
        }
        
        public static List<FosterConfig> GetAllConfigs()
        {
            return new List<FosterConfig>(_fosterConfigs.Values);
        }
        
        public static List<FosterConfig> GetAvailableConfigs(int petLevel)
        {
            var result = new List<FosterConfig>();
            foreach (var config in _fosterConfigs.Values)
            {
                if (config.MinPetLevel <= petLevel)
                    result.Add(config);
            }
            return result;
        }
    }
}
