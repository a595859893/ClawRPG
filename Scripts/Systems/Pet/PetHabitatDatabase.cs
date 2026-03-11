using Godot;
using System;
using System.Collections.Generic;

namespace GameSystems
{
    public static class PetHabitatDatabase
    {
        // 栖息地配置
        public static readonly Dictionary<string, HabitatConfig> Habitats = new Dictionary<string, HabitatConfig>
        {
            ["forest"] = new HabitatConfig
            {
                Id = "forest",
                Name = "森林栖息地",
                Description = "茂密的森林，适合木系和自然系宠物",
                Type = HabitatType.Forest,
                MaxSlots = 12,
                UnlockCost = 0,
                ComfortBonus = 10
            },
            ["meadow"] = new HabitatConfig
            {
                Id = "meadow",
                Name = "草原栖息地",
                Description = "广阔的草原，适合大多数宠物",
                Type = HabitatType.Meadow,
                MaxSlots = 10,
                UnlockCost = 500,
                ComfortBonus = 5
            },
            ["mountain"] = new HabitatConfig
            {
                Id = "mountain",
                Name = "山地栖息地",
                Description = "高耸的山地，适合岩系宠物",
                Type = HabitatType.Mountain,
                MaxSlots = 8,
                UnlockCost = 1500,
                ComfortBonus = 15
            },
            ["lake"] = new HabitatConfig
            {
                Id = "lake",
                Name = "湖泊栖息地",
                Description = "平静的湖泊，适合水系宠物",
                Type = HabitatType.Lake,
                MaxSlots = 10,
                UnlockCost = 2000,
                ComfortBonus = 12
            },
            ["desert"] = new HabitatConfig
            {
                Id = "desert",
                Name = "沙漠栖息地",
                Description = "炎热的沙漠，适合火系宠物",
                Type = HabitatType.Desert,
                MaxSlots = 8,
                UnlockCost = 3000,
                ComfortBonus = 20
            },
            ["jungle"] = new HabitatConfig
            {
                Id = "jungle",
                Name = "丛林栖息地",
                Description = "神秘的丛林，适合毒系宠物",
                Type = HabitatType.Jungle,
                MaxSlots = 14,
                UnlockCost = 5000,
                ComfortBonus = 25
            },
            ["tundra"] = new HabitatConfig
            {
                Id = "tundra",
                Name = "冻土栖息地",
                Description = "寒冷的冻土，适合冰系宠物",
                Type = HabitatType.Tundra,
                MaxSlots = 8,
                UnlockCost = 8000,
                ComfortBonus = 30
            },
            ["volcanic"] = new HabitatConfig
            {
                Id = "volcanic",
                Name = "火山栖息地",
                Description = "炽热的火山，适合传说宠物",
                Type = HabitatType.Volcanic,
                MaxSlots = 6,
                UnlockCost = 15000,
                ComfortBonus = 50
            }
        };
        
        // 装饰品配置
        public static readonly Dictionary<string, DecorationConfig> Decorations = new Dictionary<string, DecorationConfig>
        {
            // 植物类
            ["flower_red"] = new DecorationConfig
            {
                Id = "flower_red",
                Name = "红花",
                Description = "鲜艳的红色花朵",
                Type = DecorationType.Plant,
                Cost = 50,
                ComfortBonus = 2,
                AttractionBonus = 1,
                Icon = "🌺"
            },
            ["flower_blue"] = new DecorationConfig
            {
                Id = "flower_blue",
                Name = "蓝花",
                Description = "优雅的蓝色花朵",
                Type = DecorationType.Plant,
                Cost = 50,
                ComfortBonus = 2,
                AttractionBonus = 1,
                Icon = "🌸"
            },
            ["tree_oak"] = new DecorationConfig
            {
                Id = "tree_oak",
                Name = "橡树",
                Description = "高大的橡树提供良好遮蔽",
                Type = DecorationType.Plant,
                Cost = 200,
                ComfortBonus = 8,
                AttractionBonus = 3,
                Icon = "🌳"
            },
            ["tree_pine"] = new DecorationConfig
            {
                Id = "tree_pine",
                Name = "松树",
                Description = "常青的松树",
                Type = DecorationType.Plant,
                Cost = 180,
                ComfortBonus = 7,
                AttractionBonus = 3,
                Icon = "🌲"
            },
            ["bush"] = new DecorationConfig
            {
                Id = "bush",
                Name = "灌木丛",
                Description = "浓密的灌木，宠物喜欢在里面玩耍",
                Type = DecorationType.Plant,
                Cost = 100,
                ComfortBonus = 5,
                AttractionBonus = 2,
                Icon = "🌿"
            },
            ["grass"] = new DecorationConfig
            {
                Id = "grass",
                Name = "草地",
                Description = "柔软的草地",
                Type = DecorationType.Plant,
                Cost = 30,
                ComfortBonus = 3,
                AttractionBonus = 1,
                Icon = "🌱"
            },
            
            // 结构类
            ["house_wood"] = new DecorationConfig
            {
                Id = "house_wood",
                Name = "木屋",
                Description = "温馨的小木屋",
                Type = DecorationType.Structure,
                Cost = 500,
                ComfortBonus = 15,
                AttractionBonus = 5,
                Icon = "🏠"
            },
            ["house_stone"] = new DecorationConfig
            {
                Id = "house_stone",
                Name = "石屋",
                Description = "坚固的石制房屋",
                Type = DecorationType.Structure,
                Cost = 800,
                ComfortBonus = 20,
                AttractionBonus = 7,
                Icon = "🏛️"
            },
            ["fence"] = new DecorationConfig
            {
                Id = "fence",
                Name = "栅栏",
                Description = "漂亮的木质栅栏",
                Type = DecorationType.Structure,
                Cost = 150,
                ComfortBonus = 4,
                AttractionBonus = 2,
                Icon = "🚧"
            },
            ["bridge"] = new DecorationConfig
            {
                Id = "bridge",
                Name = "小桥",
                Description = "横跨小溪的小桥",
                Type = DecorationType.Structure,
                Cost = 300,
                ComfortBonus = 10,
                AttractionBonus = 4,
                Icon = "🌉"
            },
            ["tent"] = new DecorationConfig
            {
                Id = "tent",
                Name = "帐篷",
                Description = "野营帐篷",
                Type = DecorationType.Structure,
                Cost = 250,
                ComfortBonus = 8,
                AttractionBonus = 3,
                Icon = "⛺"
            },
            
            // 水景类
            ["pond"] = new DecorationConfig
            {
                Id = "pond",
                Name = "池塘",
                Description = "平静的小池塘",
                Type = DecorationType.WaterFeature,
                Cost = 400,
                ComfortBonus = 12,
                AttractionBonus = 8,
                Icon = "💧"
            },
            ["fountain"] = new DecorationConfig
            {
                Id = "fountain",
                Name = "喷泉",
                Description = "美丽的喷泉",
                Type = DecorationType.WaterFeature,
                Cost = 600,
                ComfortBonus = 15,
                AttractionBonus = 10,
                Icon = "⛲"
            },
            ["stream"] = new DecorationConfig
            {
                Id = "stream",
                Name = "小溪",
                Description = "潺潺流动的小溪",
                Type = DecorationType.WaterFeature,
                Cost = 350,
                ComfortBonus = 10,
                AttractionBonus = 6,
                Icon = "🌊"
            },
            
            // 照明类
            ["lamp_post"] = new DecorationConfig
            {
                Id = "lamp_post",
                Name = "路灯",
                Description = "温暖的灯光照亮夜晚",
                Type = DecorationType.Lighting,
                Cost = 180,
                ComfortBonus = 6,
                AttractionBonus = 2,
                Icon = "💡"
            },
            ["candle"] = new DecorationConfig
            {
                Id = "candle",
                Name = "蜡烛",
                Description = "温馨的蜡烛",
                Type = DecorationType.Lighting,
                Cost = 50,
                ComfortBonus = 3,
                AttractionBonus = 1,
                Icon = "🕯️"
            },
            ["torch"] = new DecorationConfig
            {
                Id = "torch",
                Name = "火把",
                Description = "明亮的火把",
                Type = DecorationType.Lighting,
                Cost = 100,
                ComfortBonus = 5,
                AttractionBonus = 2,
                Icon = "🔥"
            },
            
            // 玩具类
            ["ball"] = new DecorationConfig
            {
                Id = "ball",
                Name = "球",
                Description = "宠物喜欢玩耍的球",
                Type = DecorationType.Toy,
                Cost = 80,
                ComfortBonus = 4,
                AttractionBonus = 5,
                Icon = "⚽"
            },
            ["frisbee"] = new DecorationConfig
            {
                Id = "frisbee",
                Name = "飞盘",
                Description = "可以投掷的飞盘",
                Type = DecorationType.Toy,
                Cost = 120,
                ComfortBonus = 5,
                AttractionBonus = 6,
                Icon = "🥏"
            },
            ["rope"] = new DecorationConfig
            {
                Id = "rope",
                Name = "绳索",
                Description = "宠物喜欢的绳索玩具",
                Type = DecorationType.Toy,
                Cost = 60,
                ComfortBonus = 3,
                AttractionBonus = 4,
                Icon = "🪢"
            },
            ["tunnel"] = new DecorationConfig
            {
                Id = "tunnel",
                Name = "隧道",
                Description = "可以钻来钻去的隧道",
                Type = DecorationType.Toy,
                Cost = 200,
                ComfortBonus = 7,
                AttractionBonus = 8,
                Icon = "🕳️"
            },
            
            // 喂食站
            ["food_bowl"] = new DecorationConfig
            {
                Id = "food_bowl",
                Name = "食盆",
                Description = "宠物用餐的地方",
                Type = DecorationType.FoodStation,
                Cost = 100,
                ComfortBonus = 8,
                AttractionBonus = 10,
                Icon = "🍽️"
            },
            ["water_fountain"] = new DecorationConfig
            {
                Id = "water_fountain",
                Name = "饮水喷泉",
                Description = "干净的饮用水源",
                Type = DecorationType.FoodStation,
                Cost = 150,
                ComfortBonus = 10,
                AttractionBonus = 12,
                Icon = "🚰"
            },
            ["hay_rack"] = new DecorationConfig
            {
                Id = "hay_rack",
                Name = "草架",
                Description = "草食宠物的喂食架",
                Type = DecorationType.FoodStation,
                Cost = 120,
                ComfortBonus = 7,
                AttractionBonus = 8,
                Icon = "🌾"
            },
            
            // 床铺类
            ["bed_pet"] = new DecorationConfig
            {
                Id = "bed_pet",
                Name = "宠物床",
                Description = "柔软舒适的宠物床",
                Type = DecorationType.Bed,
                Cost = 250,
                ComfortBonus = 15,
                AttractionBonus = 5,
                Icon = "🛏️"
            },
            ["hammock"] = new DecorationConfig
            {
                Id = "hammock",
                Name = "吊床",
                Description = "悠闲的吊床",
                Type = DecorationType.Bed,
                Cost = 300,
                ComfortBonus = 18,
                AttractionBonus = 6,
                Icon = "🛖"
            },
            ["cushion"] = new DecorationConfig
            {
                Id = "cushion",
                Name = "坐垫",
                Description = "舒适的坐垫",
                Type = DecorationType.Bed,
                Cost = 80,
                ComfortBonus = 8,
                AttractionBonus = 3,
                Icon = "🧶"
            },
            
            // 装饰类
            ["rock"] = new DecorationConfig
            {
                Id = "rock",
                Name = "岩石",
                Description = "装饰用的大石头",
                Type = DecorationType.Decorative,
                Cost = 60,
                ComfortBonus = 3,
                AttractionBonus = 1,
                Icon = "🪨"
            },
            ["statue"] = new DecorationConfig
            {
                Id = "statue",
                Name = "雕像",
                Description = "装饰性雕像",
                Type = DecorationType.Decorative,
                Cost = 500,
                ComfortBonus = 12,
                AttractionBonus = 4,
                Icon = "🗿"
            },
            ["banner"] = new DecorationConfig
            {
                Id = "banner",
                Name = "旗帜",
                Description = "漂亮的装饰旗帜",
                Type = DecorationType.Decorative,
                Cost = 150,
                ComfortBonus = 5,
                AttractionBonus = 2,
                Icon = "🚩"
            },
            ["chest"] = new DecorationConfig
            {
                Id = "chest",
                Name = "宝箱",
                Description = "神秘的宝箱装饰",
                Type = DecorationType.Decorative,
                Cost = 350,
                ComfortBonus = 8,
                AttractionBonus = 15,
                Icon = "🎁"
            }
        };
        
        /// <summary>
        /// 获取所有可用的栖息地
        /// </summary>
        public static List<HabitatConfig> GetAllHabitats()
        {
            return new List<HabitatConfig>(Habitats.Values);
        }
        
        /// <summary>
        /// 获取所有可用的装饰品
        /// </summary>
        public static List<DecorationConfig> GetAllDecorations()
        {
            return new List<DecorationConfig>(Decorations.Values);
        }
        
        /// <summary>
        /// 根据类型获取装饰品
        /// </summary>
        public static List<DecorationConfig> GetDecorationsByType(DecorationType type)
        {
            List<DecorationConfig> result = new List<DecorationConfig>();
            foreach (var decoration in Decorations.Values)
            {
                if (decoration.Type == type)
                {
                    result.Add(decoration);
                }
            }
            return result;
        }
        
        /// <summary>
        /// 获取栖息地配置
        /// </summary>
        public static HabitatConfig GetHabitat(string id)
        {
            return Habitats.ContainsKey(id) ? Habitats[id] : null;
        }
        
        /// <summary>
        /// 获取装饰品配置
        /// </summary>
        public static DecorationConfig GetDecoration(string id)
        {
            return Decorations.ContainsKey(id) ? Decorations[id] : null;
        }
    }
}
