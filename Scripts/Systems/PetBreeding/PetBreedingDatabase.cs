using System;
using System.Collections.Generic;

namespace ClawRPG.Systems
{
    /// <summary>
    /// 宠物繁殖数据库 - 定义繁殖规则和配置
    /// </summary>
    public static class PetBreedingDatabase
    {
        // 繁殖配置
        public static Dictionary<PetBreedingData.BreedingType, BreedingConfig> BreedingConfigs = new Dictionary<PetBreedingData.BreedingType, BreedingConfig>();
        
        // 稀有度继承权重
        public static Dictionary<string, RarityInheritance> RarityInheritance = new Dictionary<string, RarityInheritance>();
        
        // 属性继承系数
        public static AttributeInheritance AttributeInheritance = new AttributeInheritance();
        
        // 繁殖类型
        public enum BreedingType
        {
            Basic,      // 基础繁殖
            Advanced,   // 高级繁殖  
            Legendary   // 传奇繁殖
        }
        
        public class BreedingConfig
        {
            public string Name { get; set; }
            public string Description { get; set; }
            public int BaseDuration { get; set; } // 秒
            public int GoldCost { get; set; }
            public float BaseSuccessRate { get; set; }
            public float LegendaryChance { get; set; }
            public int MinParentLevel { get; set; }
            public int OffspringMinLevel { get; set; }
        }
        
        public class RarityInheritance
        {
            public string Rarity { get; set; }
            public float Weight { get; set; }
        }
        
        public class AttributeInheritance
        {
            public float MinInheritRate { get; set; } = 0.3f;
            public float MaxInheritRate { get; set; } = 0.7f;
            public float MutationChance { get; set; } = 0.15f;
            public float MutationBonus { get; set; } = 0.2f;
        }
        
        static PetBreedingDatabase()
        {
            InitializeBreedingConfigs();
            InitializeRarityInheritance();
        }
        
        private static void InitializeBreedingConfigs()
        {
            // 基础繁殖
            BreedingConfigs[PetBreedingData.BreedingType.Basic] = new BreedingConfig
            {
                Name = "基础繁殖",
                Description = "普通的宠物繁殖方式，成功率较低",
                BaseDuration = 300, // 5分钟
                GoldCost = 100,
                BaseSuccessRate = 0.6f,
                LegendaryChance = 0.02f,
                MinParentLevel = 5,
                OffspringMinLevel = 1
            };
            
            // 高级繁殖
            BreedingConfigs[PetBreedingData.BreedingType.Advanced] = new BreedingConfig
            {
                Name = "高级繁殖",
                Description = "使用特殊道具提高成功率",
                BaseDuration = 180, // 3分钟
                GoldCost = 500,
                BaseSuccessRate = 0.75f,
                LegendaryChance = 0.08f,
                MinParentLevel = 10,
                OffspringMinLevel = 5
            };
            
            // 传奇繁殖
            BreedingConfigs[PetBreedingData.BreedingType.Legendary] = new BreedingConfig
            {
                Name = "传奇繁殖",
                Description = "使用传奇繁殖石，成功率最高",
                BaseDuration = 60, // 1分钟
                GoldCost = 2000,
                BaseSuccessRate = 0.9f,
                LegendaryChance = 0.2f,
                MinParentLevel = 20,
                OffspringMinLevel = 10
            };
        }
        
        private static void InitializeRarityInheritance()
        {
            // 普通
            RarityInheritance["Common"] = new RarityInheritance { Rarity = "Common", Weight = 0.5f };
            // 优秀
            RarityInheritance["Uncommon"] = new RarityInheritance { Rarity = "Uncommon", Weight = 0.3f };
            // 稀有
            RarityInheritance["Rare"] = new RarityInheritance { Rarity = "Rare", Weight = 0.15f };
            // 史诗
            RarityInheritance["Epic"] = new RarityInheritance { Rarity = "Epic", Weight = 0.04f };
            // 传说
            RarityInheritance["Legendary"] = new RarityInheritance { Rarity = "Legendary", Weight = 0.01f };
        }
        
        /// <summary>
        /// 获取繁殖配置
        /// </summary>
        public static BreedingConfig GetConfig(PetBreedingData.BreedingType type)
        {
            return BreedingConfigs.ContainsKey(type) ? BreedingConfigs[type] : null;
        }
        
        /// <summary>
        /// 计算繁殖成功率
        /// </summary>
        public static float CalculateSuccessRate(PetBreedingData.BreedingType type, int parent1Level, int parent2Level)
        {
            var config = GetConfig(type);
            if (config == null) return 0.5f;
            
            // 等级加成
            float levelBonus = (parent1Level + parent2Level) / 200f;
            
            return Math.Min(config.BaseSuccessRate + levelBonus, 0.95f);
        }
        
        /// <summary>
        /// 随机选择后代稀有度
        /// </summary>
        public static string SelectOffspringRarity(string parent1Rarity, string parent2Rarity, float legendaryChance)
        {
            // 父母稀有度平均值作为基础
            int parent1Rank = GetRarityRank(parent1Rarity);
            int parent2Rank = GetRarityRank(parent2Rarity);
            float avgRank = (parent1Rank + parent2Rank) / 2f;
            
            // 随机浮动
            float random = new Random().NextFloat();
            float rankOffset = (random - 0.5f) * 2f; // -1 到 1
            
            int finalRank = (int)(avgRank + rankOffset);
            finalRank = Math.Max(0, Math.Min(4, finalRank));
            
            // 传奇检查
            if (random < legendaryChance)
            {
                return "Legendary";
            }
            
            return GetRarityByRank(finalRank);
        }
        
        /// <summary>
        /// 计算后代属性
        /// </summary>
        public static void CalculateOffspringAttributes(
            PetBreedingData.ParentPet parent1, 
            PetBreedingData.ParentPet parent2, 
            out int attack, out int defense, out int health, out int speed)
        {
            var random = new Random();
            float inheritRate = AttributeInheritance.MinInheritRate + 
                (float)(random.NextDouble() * (AttributeInheritance.MaxInheritRate - AttributeInheritance.MinInheritRate));
            
            // 基础属性继承
            attack = (int)((parent1.Attack + parent2.Attack) / 2f * inheritRate);
            defense = (int)((parent1.Defense + parent2.Defense) / 2f * inheritRate);
            health = (int)((parent1.Health + parent2.Health) / 2f * inheritRate);
            speed = (int)((parent1.Speed + parent2.Speed) / 2f * inheritRate);
            
            // 突变检查
            if (random.NextDouble() < AttributeInheritance.MutationChance)
            {
                float bonus = 1f + AttributeInheritance.MutationBonus;
                switch (random.Next(4))
                {
                    case 0: attack = (int)(attack * bonus); break;
                    case 1: defense = (int)(defense * bonus); break;
                    case 2: health = (int)(health * bonus); break;
                    case 3: speed = (int)(speed * bonus); break;
                }
            }
            
            // 最小值保证
            attack = Math.Max(attack, 5);
            defense = Math.Max(defense, 5);
            health = Math.Max(health, 20);
            speed = Math.Max(speed, 3);
        }
        
        private static int GetRarityRank(string rarity)
        {
            switch (rarity.ToLower())
            {
                case "common": return 0;
                case "uncommon": return 1;
                case "rare": return 2;
                case "epic": return 3;
                case "legendary": return 4;
                default: return 0;
            }
        }
        
        private static string GetRarityByRank(int rank)
        {
            switch (rank)
            {
                case 0: return "Common";
                case 1: return "Uncommon";
                case 2: return "Rare";
                case 3: return "Epic";
                case 4: return "Legendary";
                default: return "Common";
            }
        }
    }
}
