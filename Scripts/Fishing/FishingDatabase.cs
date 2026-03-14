using System;
using System.Collections.Generic;
using ClawRPG.Scripts.Fishing;

namespace ClawRPG.Scripts.Fishing
{
    /// <summary>
    /// 钓鱼数据库配置
    /// </summary>
    public static class FishingDatabase
    {
        // 鱼类配置
        public static Dictionary<string, FishData> Fish { get; private set; } = new Dictionary<string, FishData>();
        
        // 鱼竿配置
        public static Dictionary<RodType, RodConfig> Rods { get; private set; } = new Dictionary<RodType, RodConfig>();
        
        // 鱼饵配置
        public static Dictionary<BaitType, BaitConfig> Baits { get; private set; } = new Dictionary<BaitType, BaitConfig>();
        
        // 地点配置
        public static Dictionary<FishingLocationType, LocationConfig> Locations { get; private set; } = new Dictionary<FishingLocationType, LocationConfig>();
        
        // 等级经验配置
        public static int[] LevelThresholds { get; private set; } = new int[100];
        
        static FishingDatabase()
        {
            InitializeFish();
            InitializeRods();
            InitializeBaits();
            InitializeLocations();
            InitializeLevelThresholds();
        }
        
        private static void InitializeFish()
        {
            // 淡水鱼 - 河流/湖泊
            AddFish("fish_small_bass", "小鲈鱼", "一种常见的淡水鱼", FishType.Common, FishCategory.Freshwater, 10, 5, 100, 500, new List<FishingLocationType> { FishingLocationType.River, FishingLocationType.Lake });
            AddFish("fish_trout", "鳟鱼", "肉质鲜美的淡水鱼", FishType.Uncommon, FishCategory.Freshwater, 25, 10, 200, 800, new List<FishingLocationType> { FishingLocationType.River, FishingLocationType.Underground });
            AddFish("fish_salmon", "三文鱼", "著名的溯河产卵鱼", FishType.Rare, FishCategory.Freshwater, 50, 20, 500, 2000, new List<FishingLocationType> { FishingLocationType.River, FishingLocationType.Ocean });
            AddFish("fish_pike", "狗鱼", "凶猛的淡水掠食鱼", FishType.Uncommon, FishCategory.Freshwater, 30, 15, 800, 3000, new List<FishingLocationType> { FishingLocationType.Lake, FishingLocationType.River });
            AddFish("fish_catfish", "鲶鱼", "夜行性底层鱼类", FishType.Common, FishCategory.Freshwater, 15, 8, 300, 1500, new List<FishingLocationType> { FishingLocationType.River, FishingLocationType.Swamp });
            AddFish("fish_carp", "鲤鱼", "寓意吉祥的淡水鱼", FishType.Uncommon, FishCategory.Freshwater, 20, 10, 1000, 5000, new List<FishingLocationType> { FishingLocationType.Lake, FishingLocationType.River });
            AddFish("fish_koi", "锦鲤", "色彩斑斓的观赏鱼", FishType.Epic, FishCategory.Freshwater, 100, 30, 500, 3000, new List<FishingLocationType> { FishingLocationType.Lake }, true, "Spring");
            AddFish("fish_eel", "鳗鱼", "长条形的滑溜鱼类", FishType.Rare, FishCategory.Freshwater, 45, 20, 400, 2000, new List<FishingLocationType> { FishingLocationType.River, FishingLocationType.Swamp });
            
            // 海水鱼 - 海洋
            AddFish("fish_tuna", "金枪鱼", "速度极快的大型鱼类", FishType.Uncommon, FishCategory.Saltwater, 35, 15, 2000, 10000, new List<FishingLocationType> { FishingLocationType.Ocean });
            AddFish("fish_swordfish", "剑鱼", "拥有长剑般吻部的鱼类", FishType.Rare, FishCategory.Saltwater, 60, 25, 5000, 25000, new List<FishingLocationType> { FishingLocationType.Ocean });
            AddFish("fish_shark", "鲨鱼", "海洋顶级掠食者", FishType.Epic, FishCategory.Saltwater, 120, 40, 10000, 50000, new List<FishingLocationType> { FishingLocationType.Ocean });
            AddFish("fish_whale", "鲸鱼", "海洋中最大的生物", FishType.Legendary, FishCategory.Saltwater, 200, 60, 50000, 100000, new List<FishingLocationType> { FishingLocationType.Ocean }, true, "Winter");
            AddFish("fish_octopus", "章鱼", "拥有八条腕足的头足类", FishType.Rare, FishCategory.Crustacean, 55, 22, 300, 1500, new List<FishingLocationType> { FishingLocationType.Ocean, FishingLocationType.Underground });
            AddFish("fish_crab", "螃蟹", "带有硬壳的甲壳类", FishType.Common, FishCategory.Crustacean, 12, 5, 100, 500, new List<FishingLocationType> { FishingLocationType.Ocean, FishingLocationType.Swamp });
            AddFish("fish_lobster", "龙虾", "珍贵的甲壳类海鲜", FishType.Epic, FishCategory.Crustacean, 110, 35, 200, 1000, new List<FishingLocationType> { FishingLocationType.Ocean });
            AddFish("fish_eel_sea", "海鳗", "生活在珊瑚礁的海鳗", FishType.Uncommon, FishCategory.Saltwater, 28, 12, 200, 1000, new List<FishingLocationType> { FishingLocationType.Ocean, FishingLocationType.Volcanic });
            
            // 特殊地点鱼类
            AddFish("fish_swamp_green", "沼泽绿鱼", "发光的沼泽生物", FishType.Rare, FishCategory.Special, 40, 18, 100, 500, new List<FishingLocationType> { FishingLocationType.Swamp });
            AddFish("fish_swamp_troll", "沼泽巨魔鱼", "传说中的沼泽怪物", FishType.Legendary, FishCategory.Mythical, 180, 50, 2000, 10000, new List<FishingLocationType> { FishingLocationType.Swamp });
            AddFish("fish_waterfall_ancient", "瀑布古鱼", "瀑布下的远古生物", FishType.Epic, FishCategory.Mythical, 95, 30, 500, 2500, new List<FishingLocationType> { FishingLocationType.Waterfall });
            AddFish("fish_underground_blind", "盲眼鱼", "地下暗河的透明鱼类", FishType.Rare, FishCategory.Special, 50, 20, 50, 300, new List<FishingLocationType> { FishingLocationType.Underground });
            AddFish("fish_volcanic_lava", "熔岩鱼", "生活在火山温泉中", FishType.Epic, FishCategory.Mythical, 130, 40, 100, 800, new List<FishingLocationType> { FishingLocationType.Volcanic }, true, "Summer");
            AddFish("fish_mystical_spirit", "灵鱼", "神秘水域的灵魂之鱼", FishType.Mythic, FishCategory.Mythical, 300, 100, 1, 100, new List<FishingLocationType> { FishingLocationType.Mystical });
            AddFish("fish_mystical_dragon", "龙鱼", "传说中与龙相关的鱼类", FishType.Mythic, FishCategory.Mythical, 500, 150, 10000, 50000, new List<FishingLocationType> { FishingLocationType.Mystical }, true, "Night");
            
            // 爬行类
            AddFish("fish_turtle", "乌龟", "长寿的爬行动物", FishType.Uncommon, FishCategory.Reptile, 25, 12, 500, 3000, new List<FishingLocationType> { FishingLocationType.Lake, FishingLocationType.River });
            AddFish("fish_crocodile", "鳄鱼", "危险的淡水掠食者", FishType.Epic, FishCategory.Reptile, 150, 45, 5000, 20000, new List<FishingLocationType> { FishingLocationType.Swamp, FishingLocationType.River });
            
            // 甲壳类
            AddFish("fish_shrimp", "虾", "小型甲壳类", FishType.Common, FishCategory.Crustacean, 8, 3, 10, 100, new List<FishingLocationType> { FishingLocationType.Ocean, FishingLocationType.Lake });
            AddFish("fish_scorpion_fish", "蝎子鱼", "有毒的观赏鱼", FishType.Rare, FishCategory.Crustacean, 48, 20, 50, 300, new List<FishingLocationType> { FishingLocationType.Ocean });
        }
        
        private static void AddFish(string id, string name, string description, FishType rarity, FishCategory category, int value, int xp, int minWeight, int maxWeight, List<FishingLocationType> locations, bool seasonal = false, string season = "")
        {
            Fish[id] = new FishData
            {
                ID = id,
                Name = name,
                Description = description,
                Rarity = rarity,
                Category = category,
                BaseValue = value,
                ExperienceReward = xp,
                MinWeight = minWeight,
                MaxWeight = maxWeight,
                Locations = locations,
                IsSeasonal = seasonal,
                Season = season,
                IsTimeLimited = false
            };
        }
        
        private static void InitializeRods()
        {
            Rods[RodType.Bamboo] = new RodConfig
            {
                Type = RodType.Bamboo,
                Name = "竹制鱼竿",
                Description = "最简单的鱼竿，适合初学者",
                CastDistance = 5,
                ReelSpeed = 1.0f,
                Durability = 100,
                CatchBonus = 0.0f,
                RareBonus = 0.0f,
                Cost = 0
            };
            
            Rods[RodType.Fiberglass] = new RodConfig
            {
                Type = RodType.Fiberglass,
                Name = "玻璃钢鱼竿",
                Description = "，结实耐用的钓鱼竿",
                CastDistance = 8,
                ReelSpeed = 1.5f,
                Durability = 200,
                CatchBonus = 0.1f,
                RareBonus = 0.05f,
                Cost = 100
            };
            
            Rods[RodType.Carbon] = new RodConfig
            {
                Type = RodType.Carbon,
                Name = "碳纤维鱼竿",
                Description = "轻便且敏感的顶级鱼竿",
                CastDistance = 12,
                ReelSpeed = 2.0f,
                Durability = 350,
                CatchBonus = 0.2f,
                RareBonus = 0.1f,
                Cost = 500
            };
            
            Rods[RodType.Master] = new RodConfig
            {
                Type = RodType.Master,
                Name = "大师级鱼竿",
                Description = "大师工匠打造的精品鱼竿",
                CastDistance = 15,
                ReelSpeed = 2.5f,
                Durability = 500,
                CatchBonus = 0.3f,
                RareBonus = 0.15f,
                Cost = 2000
            };
            
            Rods[RodType.Legendary] = new RodConfig
            {
                Type = RodType.Legendary,
                Name = "传奇鱼竿",
                Description = "蕴含神秘力量的传说鱼竿",
                CastDistance = 20,
                ReelSpeed = 3.0f,
                Durability = 1000,
                CatchBonus = 0.4f,
                RareBonus = 0.2f,
                Cost = 10000
            };
            
            Rods[RodType.Mythic] = new RodConfig
            {
                Type = RodType.Mythic,
                Name = "神器鱼竿",
                Description = "的神器，传说能钓起任何生物",
                CastDistance = 30,
                ReelSpeed = 4.0f,
                Durability = 9999,
                CatchBonus = 0.5f,
                RareBonus = 0.3f,
                Cost = 100000
            };
        }
        
        private static void InitializeBaits()
        {
            Baits[BaitType.Worm] = new BaitConfig
            {
                Type = BaitType.Worm,
                Name = "蚯蚓",
                Description = "最常见的鱼饵，对淡水鱼效果好",
                Attractiveness = 1.0f,
                Cost = 1,
                Duration = 30,
                PreferredFish = new List<FishCategory> { FishCategory.Freshwater }
            };
            
            Baits[BaitType.Insect] = new BaitConfig
            {
                Type = BaitType.Insect,
                Name = "昆虫",
                Description = "各种水生昆虫",
                Attractiveness = 1.2f,
                Cost = 3,
                Duration = 25,
                PreferredFish = new List<FishCategory> { FishCategory.Freshwater, FishCategory.Crustacean }
            };
            
            Baits[BaitType.Fish] = new BaitConfig
            {
                Type = BaitType.Fish,
                Name = "小鱼",
                Description = "用小鱼作饵，吸引力更强",
                Attractiveness = 1.5f,
                Cost = 10,
                Duration = 20,
                PreferredFish = new List<FishCategory> { FishCategory.Saltwater, FishCategory.Reptile }
            };
            
            Baits[BaitType.Fruit] = new BaitConfig
            {
                Type = BaitType.Fruit,
                Name = "水果",
                Description = "芳香甜蜜的水果对某些鱼有奇效",
                Attractiveness = 1.3f,
                Cost = 5,
                Duration = 35,
                PreferredFish = new List<FishCategory> { FishCategory.Freshwater }
            };
            
            Baits[BaitType.Special] = new BaitConfig
            {
                Type = BaitType.Special,
                Name = "特殊鱼饵",
                Description = "特殊配方的魔法鱼饵",
                Attractiveness = 2.0f,
                Cost = 50,
                Duration = 15,
                PreferredFish = new List<FishCategory> { FishCategory.Mythical, FishCategory.Special }
            };
            
            Baits[BaitType.Lure] = new BaitConfig
            {
                Type = BaitType.Lure,
                Name = "拟饵",
                Description = "可以重复使用的假饵",
                Attractiveness = 1.4f,
                Cost = 20,
                Duration = 60,
                PreferredFish = new List<FishCategory> { FishCategory.Saltwater, FishCategory.Freshwater }
            };
            
            Baits[BaitType.Fly] = new BaitConfig
            {
                Type = BaitType.Fly,
                Name = "飞蝇",
                Description = "用于飞蝇钓的高级假饵",
                Attractiveness = 1.6f,
                Cost = 30,
                Duration = 40,
                PreferredFish = new List<FishCategory> { FishCategory.Freshwater }
            };
        }
        
        private static void InitializeLocations()
        {
            Locations[FishingLocationType.River] = new LocationConfig
            {
                Type = FishingLocationType.River,
                Name = "河流",
                Description = "流动的河水中有丰富的鱼类",
                BaseFishTypes = new List<FishType> { FishType.Common, FishType.Uncommon },
                RareFishTypes = new List<FishType> { FishType.Rare, FishType.Epic },
                Difficulty = 1.0f,
                RecommendedLevel = 1
            };
            
            Locations[FishingLocationType.Lake] = new LocationConfig
            {
                Type = FishingLocationType.Lake,
                Name = "湖泊",
                Description = "平静的湖泊中是大型鱼类的栖息地",
                BaseFishTypes = new List<FishType> { FishType.Common, FishType.Uncommon, FishType.Rare },
                RareFishTypes = new List<FishType> { FishType.Epic },
                Difficulty = 1.2f,
                RecommendedLevel = 5
            };
            
            Locations[FishingLocationType.Ocean] = new LocationConfig
            {
                Type = FishingLocationType.Ocean,
                Name = "海洋",
                Description = "广阔的大海中隐藏着巨大的生物",
                BaseFishTypes = new List<FishType> { FishType.Uncommon, FishType.Rare },
                RareFishTypes = new List<FishType> { FishType.Epic, FishType.Legendary },
                Difficulty = 1.5f,
                RecommendedLevel = 15
            };
            
            Locations[FishingLocationType.Swamp] = new LocationConfig
            {
                Type = FishingLocationType.Swamp,
                Name = "沼泽",
                Description = "神秘的沼泽中栖息着危险的生物",
                BaseFishTypes = new List<FishType> { FishType.Common, FishType.Uncommon },
                RareFishTypes = new List<FishType> { FishType.Rare, FishType.Epic, FishType.Legendary },
                Difficulty = 1.8f,
                RecommendedLevel = 20
            };
            
            Locations[FishingLocationType.Waterfall] = new LocationConfig
            {
                Type = FishingLocationType.Waterfall,
                Name = "瀑布",
                Description = "瀑布下的深潭中可能有远古生物",
                BaseFishTypes = new List<FishType> { FishType.Uncommon, FishType.Rare },
                RareFishTypes = new List<FishType> { FishType.Epic, FishType.Legendary },
                Difficulty = 2.0f,
                RecommendedLevel = 25
            };
            
            Locations[FishingLocationType.Underground] = new LocationConfig
            {
                Type = FishingLocationType.Underground,
                Name = "地下泉水",
                Description = "地下暗河中的奇特生物",
                BaseFishTypes = new List<FishType> { FishType.Rare },
                RareFishTypes = new List<FishType> { FishType.Epic },
                Difficulty = 2.2f,
                RecommendedLevel = 30
            };
            
            Locations[FishingLocationType.Volcanic] = new LocationConfig
            {
                Type = FishingLocationType.Volcanic,
                Name = "火山温泉",
                Description = "炽热的火山温泉中是耐高温的奇特生物",
                BaseFishTypes = new List<FishType> { FishType.Rare, FishType.Epic },
                RareFishTypes = new List<FishType> { FishType.Legendary, FishType.Mythic },
                Difficulty = 2.5f,
                RecommendedLevel = 40
            };
            
            Locations[FishingLocationType.Mystical] = new LocationConfig
            {
                Type = FishingLocationType.Mystical,
                Name = "神秘水域",
                Description = "传说中的神秘水域，可能钓起神灵",
                BaseFishTypes = new List<FishType> { FishType.Epic, FishType.Legendary },
                RareFishTypes = new List<FishType> { FishType.Mythic },
                Difficulty = 3.0f,
                RecommendedLevel = 50
            };
        }
        
        private static void InitializeLevelThresholds()
        {
            for (int i = 0; i < 100; i++)
            {
                LevelThresholds[i] = (i + 1) * 100 * (i + 1);
            }
        }
        
        public static int GetLevelForXP(int xp)
        {
            for (int i = 99; i >= 0; i--)
            {
                if (xp >= LevelThresholds[i])
                    return i + 1;
            }
            return 1;
        }
        
        public static int GetXPForNextLevel(int currentLevel)
        {
            if (currentLevel >= 100) return 0;
            return LevelThresholds[currentLevel];
        }
    }
    
    // 鱼竿配置
    public class RodConfig
    {
        public RodType Type { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int CastDistance { get; set; }
        public float ReelSpeed { get; set; }
        public int Durability { get; set; }
        public float CatchBonus { get; set; }
        public float RareBonus { get; set; }
        public int Cost { get; set; }
    }
    
    // 鱼饵配置
    public class BaitConfig
    {
        public BaitType Type { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public float Attractiveness { get; set; }
        public int Cost { get; set; }
        public int Duration { get; set; }
        public List<FishCategory> PreferredFish { get; set; }
    }
    
    // 地点配置
    public class LocationConfig
    {
        public FishingLocationType Type { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public List<FishType> BaseFishTypes { get; set; }
        public List<FishType> RareFishTypes { get; set; }
        public float Difficulty { get; set; }
        public int RecommendedLevel { get; set; }
    }
}
