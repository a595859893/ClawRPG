using System;
using System.Collections.Generic;

namespace ClawRPG.Systems
{
    /// <summary>
    /// 宠物探险配置数据库
    /// </summary>
    public static class PetExpeditionDatabase
    {
        public static readonly Dictionary<ExpeditionType, ExpeditionConfig> Expeditions = new Dictionary<ExpeditionType, ExpeditionConfig>
        {
            // 森林探险 - 容易，低奖励
            { ExpeditionType.Forest, new ExpeditionConfig
                {
                    Name = "Forest Expedition",
                    Description = "Explore the nearby forest for treasures",
                    DurationMinutes = 30,
                    MinLevel = 1,
                    SuccessRate = 0.8f,
                    GoldReward = new int[] { 50, 150 },
                    ExpReward = new int[] { 30, 80 },
                    ItemPool = new string[] { "Herb", "Mushroom", "Berry", "Wood", "Insect" },
                    RarityWeights = new float[] { 0.50f, 0.30f, 0.15f, 0.04f, 0.01f }
                }
            },
            
            // 山脉探险 - 中等难度
            { ExpeditionType.Mountain, new ExpeditionConfig
                {
                    Name = "Mountain Expedition",
                    Description = "Climb the mountain to find hidden treasures",
                    DurationMinutes = 60,
                    MinLevel = 10,
                    SuccessRate = 0.7f,
                    GoldReward = new int[] { 150, 350 },
                    ExpReward = new int[] { 80, 180 },
                    ItemPool = new string[] { "Iron Ore", "Silver Ore", "Gemstone", "Crystal", "Mountain Flower" },
                    RarityWeights = new float[] { 0.40f, 0.35f, 0.18f, 0.06f, 0.01f }
                }
            },
            
            // 沙漠探险 - 中等难度
            { ExpeditionType.Desert, new ExpeditionConfig
                {
                    Name = "Desert Expedition",
                    Description = "Cross the desert to discover ancient secrets",
                    DurationMinutes = 90,
                    MinLevel = 20,
                    SuccessRate = 0.65f,
                    GoldReward = new int[] { 250, 500 },
                    ExpReward = new int[] { 150, 300 },
                    ItemPool = new string[] { "Sand Crystal", "Desert Flower", "Ancient Coin", "Golden Idol", "Cactus" },
                    RarityWeights = new float[] { 0.35f, 0.35f, 0.20f, 0.08f, 0.02f }
                }
            },
            
            // 海洋探险 - 较难
            { ExpeditionType.Ocean, new ExpeditionConfig
                {
                    Name = "Ocean Expedition",
                    Description = "Set sail for oceanic adventures",
                    DurationMinutes = 120,
                    MinLevel = 30,
                    SuccessRate = 0.6f,
                    GoldReward = new int[] { 400, 800 },
                    ExpReward = new int[] { 250, 450 },
                    ItemPool = new string[] { "Pearl", "Coral", "Seaweed", "Trident", "Ocean Gem" },
                    RarityWeights = new float[] { 0.30f, 0.35f, 0.22f, 0.10f, 0.03f }
                }
            },
            
            // 火山探险 - 困难
            { ExpeditionType.Volcano, new ExpeditionConfig
                {
                    Name = "Volcano Expedition",
                    Description = "Brave the volcanic terrain for rare minerals",
                    DurationMinutes = 180,
                    MinLevel = 40,
                    SuccessRate = 0.5f,
                    GoldReward = new int[] { 600, 1200 },
                    ExpReward = new int[] { 400, 700 },
                    ItemPool = new string[] { "Obsidian", "Fire Gem", "Magma Core", "Dragon Scale", "Phoenix Feather" },
                    RarityWeights = new float[] { 0.25f, 0.35f, 0.25f, 0.12f, 0.03f }
                }
            },
            
            // 冰峰探险 - 困难
            { ExpeditionType.IcePeak, new ExpeditionConfig
                {
                    Name = "Ice Peak Expedition",
                    Description = "Scale the frozen peaks for icy treasures",
                    DurationMinutes = 180,
                    MinLevel = 45,
                    SuccessRate = 0.5f,
                    GoldReward = new int[] { 650, 1300 },
                    ExpReward = new int[] { 420, 720 },
                    ItemPool = new string[] { "Ice Crystal", "Frost Gem", "Snow Flower", "Ice Dragon Scale", "Winter Essence" },
                    RarityWeights = new float[] { 0.25f, 0.35f, 0.25f, 0.12f, 0.03f }
                }
            },
            
            // 远古遗迹 - 非常困难
            { ExpeditionType.AncientRuins, new ExpeditionConfig
                {
                    Name = "Ancient Ruins Expedition",
                    Description = "Explore mysterious ancient ruins",
                    DurationMinutes = 240,
                    MinLevel = 55,
                    SuccessRate = 0.4f,
                    GoldReward = new int[] { 1000, 2000 },
                    ExpReward = new int[] { 600, 1000 },
                    ItemPool = new string[] { "Ancient Artifact", "Mystic Scroll", "Rune Stone", "Lost Knowledge", "Ancient Weapon" },
                    RarityWeights = new float[] { 0.20f, 0.30f, 0.28f, 0.17f, 0.05f }
                }
            },
            
            // 巨龙巢穴 - 极难
            { ExpeditionType.DragonLair, new ExpeditionConfig
                {
                    Name = "Dragon Lair Expedition",
                    Description = "Venture into the dragon's lair",
                    DurationMinutes = 300,
                    MinLevel = 65,
                    SuccessRate = 0.3f,
                    GoldReward = new int[] { 2000, 4000 },
                    ExpReward = new int[] { 1000, 1800 },
                    ItemPool = new string[] { "Dragon Scale", "Dragon Fang", "Dragon Heart", "Dragon Egg", "Ancient Dragon Bone" },
                    RarityWeights = new float[] { 0.15f, 0.25f, 0.30f, 0.20f, 0.10f }
                }
            },
            
            // 暗影领域 - 史诗难度
            { ExpeditionType.ShadowRealm, new ExpeditionConfig
                {
                    Name = "Shadow Realm Expedition",
                    Description = "Enter the realm of shadows",
                    DurationMinutes = 360,
                    MinLevel = 75,
                    SuccessRate = 0.25f,
                    GoldReward = new int[] { 3000, 6000 },
                    ExpReward = new int[] { 1500, 2500 },
                    ItemPool = new string[] { "Shadow Essence", "Dark Crystal", "Void Stone", "Soul Gem", "Shadow Dragon Scale" },
                    RarityWeights = new float[] { 0.12f, 0.23f, 0.30f, 0.25f, 0.10f }
                }
            },
            
            // 天界领域 - 传说难度
            { ExpeditionType.CelestialRealm, new ExpeditionConfig
                {
                    Name = "Celestial Realm Expedition",
                    Description = "Ascend to the celestial heavens",
                    DurationMinutes = 480,
                    MinLevel = 85,
                    SuccessRate = 0.15f,
                    GoldReward = new int[] { 5000, 10000 },
                    ExpReward = new int[] { 2500, 4000 },
                    ItemPool = new string[] { "Celestial Crystal", "Divine Essence", "Holy Grail", "Angel Feather", "Divine Artifact" },
                    RarityWeights = new float[] { 0.10f, 0.20f, 0.28f, 0.27f, 0.15f }
                }
            }
        };
        
        /// <summary>
        /// 稀有度配置
        /// </summary>
        public static readonly string[] Rarities = { "Common", "Uncommon", "Rare", "Epic", "Legendary" };
        
        /// <summary>
        /// 稀有度颜色
        /// </summary>
        public static readonly string[] RarityColors = { "#FFFFFF", "#1EFF00", "#0070DD", "#A335EE", "#FF8000" };
        
        /// <summary>
        /// 稀有度名称（中文）
        /// </summary>
        public static readonly string[] RarityNamesCN = { "普通", "优秀", "稀有", "史诗", "传说" };
    }
}
