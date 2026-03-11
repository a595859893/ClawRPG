using System;
using System.Collections.Generic;
using ClawRPG.Systems;

namespace ClawRPG.Database
{
    /// <summary>
    /// 坐骑远征数据库
    /// </summary>
    public static class MountExpeditionDatabase
    {
        public static Dictionary<string, MountExpeditionData.ExpeditionZone> Zones { get; private set; }
        
        static MountExpeditionDatabase()
        {
            InitializeZones();
        }
        
        private static void InitializeZones()
        {
            Zones = new Dictionary<string, MountExpeditionData.ExpeditionZone>();
            
            // 草地悠闲赛道 - Lv.1
            Zones["grassland_race"] = new MountExpeditionData.ExpeditionZone
            {
                Id = "grassland_race",
                Name = "草地悠闲赛道",
                Description = "适合新手的轻松赛道，沿途风景优美",
                RecommendedLevel = 1,
                DurationMinutes = 30,
                MountSlots = 1,
                MinGoldReward = 10,
                MaxGoldReward = 50,
                MinExpReward = 20,
                MaxExpReward = 50,
                BaseSuccessRate = 0.9f,
                ItemRewards = new List<string> { "speed_potion", "small_health_potion" }
            };
            
            // 森林小径 - Lv.15
            Zones["forest_trail"] = new MountExpeditionData.ExpeditionZone
            {
                Id = "forest_trail",
                Name = "森林小径",
                Description = "穿越神秘的森林，寻找隐藏的宝藏",
                RecommendedLevel = 15,
                DurationMinutes = 60,
                MountSlots = 1,
                MinGoldReward = 50,
                MaxGoldReward = 200,
                MinExpReward = 100,
                MaxExpReward = 250,
                BaseSuccessRate = 0.8f,
                ItemRewards = new List<string> { "forest_herb", "rare_herb", "wood" }
            };
            
            // 山地赛道 - Lv.25
            Zones["mountain_track"] = new MountExpeditionData.ExpeditionZone
            {
                Id = "mountain_track",
                Name = "山地赛道",
                Description = "陡峭的山路考验坐骑的耐力和速度",
                RecommendedLevel = 25,
                DurationMinutes = 90,
                MountSlots = 1,
                MinGoldReward = 150,
                MaxGoldReward = 500,
                MinExpReward = 250,
                MaxExpReward = 500,
                BaseSuccessRate = 0.75f,
                ItemRewards = new List<string> { "iron_ore", "silver_ore", "stone" }
            };
            
            // 沙漠探险 - Lv.30
            Zones["desert_expedition"] = new MountExpeditionData.ExpeditionZone
            {
                Id = "desert_expedition",
                Name = "沙漠探险",
                Description = "穿越炽热的沙漠，寻找古代遗迹",
                RecommendedLevel = 30,
                DurationMinutes = 100,
                MountSlots = 2,
                MinGoldReward = 200,
                MaxGoldReward = 800,
                MinExpReward = 400,
                MaxExpReward = 800,
                BaseSuccessRate = 0.7f,
                ItemRewards = new List<string> { "sand_pearl", "ancient_coin", "desert_crystal" }
            };
            
            // 火山边缘 - Lv.35
            Zones["volcano_edge"] = new MountExpeditionData.ExpeditionZone
            {
                Id = "volcano_edge",
                Name = "火山边缘",
                Description = "沿着火山口行进，获取炽热宝藏",
                RecommendedLevel = 35,
                DurationMinutes = 120,
                MountSlots = 1,
                MinGoldReward = 300,
                MaxGoldReward = 1000,
                MinExpReward = 500,
                MaxExpReward = 1000,
                BaseSuccessRate = 0.65f,
                ItemRewards = new List<string> { "fire_essence", "obsidian", "magma_crystal" }
            };
            
            // 冰霜之巅 - Lv.40
            Zones["frozen_peaks"] = new MountExpeditionData.ExpeditionZone
            {
                Id = "frozen_peaks",
                Name = "冰霜之巅",
                Description = "攀登寒冷的雪山，寻找冰雪宝藏",
                RecommendedLevel = 40,
                DurationMinutes = 140,
                MountSlots = 1,
                MinGoldReward = 400,
                MaxGoldReward = 1500,
                MinExpReward = 700,
                MaxExpReward = 1400,
                BaseSuccessRate = 0.6f,
                ItemRewards = new List<string> { "ice_crystal", "frozen_herb", "snow_pearl" }
            };
            
            // 幽灵城堡 - Lv.45
            Zones["ghost_castle"] = new MountExpeditionData.ExpeditionZone
            {
                Id = "ghost_castle",
                Name = "幽灵城堡",
                Description = "探索闹鬼的城堡，寻找失落的珍宝",
                RecommendedLevel = 45,
                DurationMinutes = 160,
                MountSlots = 2,
                MinGoldReward = 800,
                MaxGoldReward = 3000,
                MinExpReward = 1000,
                MaxExpReward = 2000,
                BaseSuccessRate = 0.55f,
                ItemRewards = new List<string> { "ghost_orb", "shadow_essence", "cursed_gem" }
            };
            
            // 龙之巢穴 - Lv.50
            Zones["dragon_lair"] = new MountExpeditionData.ExpeditionZone
            {
                Id = "dragon_lair",
                Name = "龙之巢穴",
                Description = "深入巨龙的巢穴，获取无上宝藏",
                RecommendedLevel = 50,
                DurationMinutes = 180,
                MountSlots = 1,
                MinGoldReward = 1000,
                MaxGoldReward = 5000,
                MinExpReward = 1500,
                MaxExpReward = 3000,
                BaseSuccessRate = 0.5f,
                ItemRewards = new List<string> { "dragon_scale", "dragon_blood", "fire_dragon_egg" }
            };
        }
        
        /// <summary>
        /// 获取所有区域
        /// </summary>
        public static List<MountExpeditionData.ExpeditionZone> GetAllZones()
        {
            return new List<MountExpeditionData.ExpeditionZone>(Zones.Values);
        }
        
        /// <summary>
        /// 获取区域
        /// </summary>
        public static MountExpeditionData.ExpeditionZone GetZone(string zoneId)
        {
            if (Zones.ContainsKey(zoneId))
                return Zones[zoneId];
            return null;
        }
    }
}
