using Godot;
using System;
using System.Collections.Generic;

namespace GameSystems
{
    public class PetExpeditionDatabase
    {
        private static PetExpeditionDatabase _instance;
        public static PetExpeditionDatabase Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new PetExpeditionDatabase();
                return _instance;
            }
        }
        
        public Dictionary<string, ExpeditionZone> Zones { get; private set; } = new Dictionary<string, ExpeditionZone>();
        
        public PetExpeditionDatabase()
        {
            InitializeZones();
        }
        
        private void InitializeZones()
        {
            // 草地花园 - 初级远征
            Zones["grassland_garden"] = new ExpeditionZone
            {
                Id = "grassland_garden",
                Name = "草地花园",
                Description = "宠物在宁静的花园中探险，寻找隐藏的宝藏",
                RecommendedLevel = 1,
                DurationMinutes = 30,
                PetSlotsRequired = 1,
                MinGoldReward = 10,
                MaxGoldReward = 50,
                MinExpReward = 20,
                MaxExpReward = 50,
                PossibleItems = new List<string> { "herb_green", "flower_red", "honey", "butterfly" },
                ItemDropChance = 0.3f,
                RequiredPower = 10
            };
            
            // 森林深处 - 中级远征
            Zones["deep_forest"] = new ExpeditionZone
            {
                Id = "deep_forest",
                Name = "森林深处",
                Description = "探索神秘的森林，发现稀有材料和宝物",
                RecommendedLevel = 15,
                DurationMinutes = 60,
                PetSlotsRequired = 1,
                MinGoldReward = 50,
                MaxGoldReward = 200,
                MinExpReward = 100,
                MaxExpReward = 250,
                PossibleItems = new List<string> { "mushroom_rare", "wood_Oak", "feather_eagle", "herb_mystic", "berry_gold" },
                ItemDropChance = 0.4f,
                RequiredPower = 50
            };
            
            // 山脉矿洞 - 中高级远征
            Zones["mountain_cave"] = new ExpeditionZone
            {
                Id = "mountain_cave",
                Name = "山脉矿洞",
                Description = "深入矿洞挖掘珍贵矿石和宝石",
                RecommendedLevel = 25,
                DurationMinutes = 90,
                PetSlotsRequired = 2,
                MinGoldReward = 150,
                MaxGoldReward = 500,
                MinExpReward = 250,
                MaxExpReward = 500,
                PossibleItems = new List<string> { "ore_iron", "ore_gold", "gem_ruby", "gem_sapphire", "crystal_blue" },
                ItemDropChance = 0.5f,
                RequiredPower = 100
            };
            
            // 古代遗迹 - 高级远征
            Zones["ancient_ruins"] = new ExpeditionZone
            {
                Id = "ancient_ruins",
                Name = "古代遗迹",
                Description = "探索古老文明的遗迹，寻找失落宝藏",
                RecommendedLevel = 35,
                DurationMinutes = 120,
                PetSlotsRequired = 2,
                MinGoldReward = 300,
                MaxGoldReward = 1000,
                MinExpReward = 500,
                MaxExpReward = 1000,
                PossibleItems = new List<string> { "relic_ancient", "scroll_magic", "artifact_gold", "gem_diamond", "orb_mystic" },
                ItemDropChance = 0.6f,
                RequiredPower = 200
            };
            
            // 龙之巢穴 - 顶级远征
            Zones["dragon_lair"] = new ExpeditionZone
            {
                Id = "dragon_lair",
                Name = "龙之巢穴",
                Description = "勇闯巨龙巢穴，获取传奇宝藏",
                RecommendedLevel = 50,
                DurationMinutes = 180,
                PetSlotsRequired = 3,
                MinGoldReward = 1000,
                MaxGoldReward = 5000,
                MinExpReward = 1500,
                MaxExpReward = 3000,
                PossibleItems = new List<string> { "scale_dragon", "claw_dragon", "treasure_legendary", "egg_dragon", "crown_king" },
                ItemDropChance = 0.7f,
                RequiredPower = 500
            };
            
            // 元素裂缝 - 专家远征
            Zones["elemental_rift"] = new ExpeditionZone
            {
                Id = "elemental_rift",
                Name = "元素裂缝",
                Description = "穿越元素裂缝，获取元素精华",
                RecommendedLevel = 40,
                DurationMinutes = 150,
                PetSlotsRequired = 2,
                MinGoldReward = 500,
                MaxGoldReward = 2000,
                MinExpReward = 800,
                MaxExpReward = 1500,
                PossibleItems = new List<string> { "essence_fire", "essence_ice", "essence_thunder", "orb_elemental", "core_mystic" },
                ItemDropChance = 0.55f,
                RequiredPower = 300
            };
            
            // 深海沉船 - 稀有远征
            Zones["sunken_ship"] = new ExpeditionZone
            {
                Id = "sunken_ship",
                Name = "深海沉船",
                Description = "探索海底沉没的古代船只，寻找海难宝藏",
                RecommendedLevel = 30,
                DurationMinutes = 100,
                PetSlotsRequired = 1,
                MinGoldReward = 200,
                MaxGoldReward = 800,
                MinExpReward = 400,
                MaxExpReward = 800,
                PossibleItems = new List<string> { "pearl", "coral_red", "trident_ancient", "coin_gold_ancient", "shell_mystic" },
                ItemDropChance = 0.45f,
                RequiredPower = 150
            };
            
            // 幽灵城堡 - 恐怖远征
            Zones["ghost_castle"] = new ExpeditionZone
            {
                Id = "ghost_castle",
                Name = "幽灵城堡",
                Description = "勇闯幽灵城堡，降服幽灵获得宝藏",
                RecommendedLevel = 45,
                DurationMinutes = 160,
                PetSlotsRequired = 3,
                MinGoldReward = 800,
                MaxGoldReward = 3000,
                MinExpReward = 1000,
                MaxExpReward = 2000,
                PossibleItems = new List<string> { "ghost_orb", "chain_spirit", "crown_ghost", "essence_dark", "mask_mystic" },
                ItemDropChance = 0.6f,
                RequiredPower = 400
            };
        }
        
        public ExpeditionZone GetZone(string zoneId)
        {
            if (Zones.ContainsKey(zoneId))
                return Zones[zoneId];
            return null;
        }
        
        public List<ExpeditionZone> GetZonesByLevel(int playerLevel)
        {
            var result = new List<ExpeditionZone>();
            foreach (var zone in Zones.Values)
            {
                if (zone.RecommendedLevel <= playerLevel + 10)
                    result.Add(zone);
            }
            return result;
        }
    }
}
