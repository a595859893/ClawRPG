using Godot;
using System;
using System.Collections.Generic;

namespace ClawRPG.Scripts.Systems
{
    /// <summary>
    /// 神秘商店数据库配置
    /// </summary>
    public static class MysteryMerchantDatabase
    {
        // 商人类型配置
        public static Dictionary<MysteryMerchantType, Dictionary<string, object>> MerchantTypeConfigs = new Dictionary<MysteryMerchantType, Dictionary<string, object>>
        {
            { MysteryMerchantType.TravelingMerchant, new Dictionary<string, object>
                {
                    { "name", "旅行商人" },
                    { "description", "来自远方的商人，携带各种普通商品" },
                    { "spawnChance", 0.30f },
                    { "minLevel", 1 },
                    { "itemCount", new Vector2(6, 10) },
                    { "priceRange", new Vector2(100, 1000) },
                    { "discount", new Vector2(0.8f, 1.0f) },
                    { "refreshCost", 50 }
                }
            },
            { MysteryMerchantType.BlackMarketDealer, new Dictionary<string, object>
                {
                    { "name", "黑市商人" },
                    { "description", "隐藏在暗处的商人，出售各种可疑物品" },
                    { "spawnChance", 0.15f },
                    { "minLevel", 10 },
                    { "itemCount", new Vector2(4, 8) },
                    { "priceRange", new Vector2(500, 5000) },
                    { "discount", new Vector2(0.5f, 0.8f) },
                    { "refreshCost", 200 }
                }
            },
            { MysteryMerchantType.AncientCollector, new Dictionary<string, object>
                {
                    { "name", "古代收藏家" },
                    { "description", "热衷于收集古老文物的神秘人物" },
                    { "spawnChance", 0.10f },
                    { "minLevel", 20 },
                    { "itemCount", new Vector2(3, 6) },
                    { "priceRange", new Vector2(1000, 10000) },
                    { "discount", new Vector2(0.6f, 0.9f) },
                    { "refreshCost", 500 }
                }
            },
            { MysteryMerchantType.DragonHoardKeeper, new Dictionary<string, object>
                {
                    { "name", "龙穴守护者" },
                    { "description", "守护巨龙宝藏的的神秘存在" },
                    { "spawnChance", 0.05f },
                    { "minLevel", 30 },
                    { "itemCount", new Vector2(2, 5) },
                    { "priceRange", new Vector2(5000, 50000) },
                    { "discount", new Vector2(0.3f, 0.6f) },
                    { "refreshCost", 1000 }
                }
            },
            { MysteryMerchantType.CursedItemDealer, new Dictionary<string, object>
                {
                    { "name", "诅咒物品商人" },
                    { "description", "出售带有强大力量但危险的诅咒物品" },
                    { "spawnChance", 0.08f },
                    { "minLevel", 15 },
                    { "itemCount", new Vector2(3, 6) },
                    { "priceRange", new Vector2(300, 3000) },
                    { "discount", new Vector2(0.4f, 0.7f) },
                    { "refreshCost", 150 }
                }
            },
            { MysteryMerchantType.LuckyCharmSeller, new Dictionary<string, object>
                {
                    { "name", "幸运护符卖家" },
                    { "description", "出售各种增加幸运值的护符和饰品" },
                    { "spawnChance", 0.12f },
                    { "minLevel", 5 },
                    { "itemCount", new Vector2(5, 8) },
                    { "priceRange", new Vector2(200, 2000) },
                    { "discount", new Vector2(0.7f, 1.0f) },
                    { "refreshCost", 100 }
                }
            },
            { MysteryMerchantType.RareMaterialVendor, new Dictionary<string, object>
                {
                    { "name", "稀有材料 vendor" },
                    { "description", "专门出售各种稀有锻造和炼金材料" },
                    { "spawnChance", 0.18f },
                    { "minLevel", 8 },
                    { "itemCount", new Vector2(6, 12) },
                    { "priceRange", new Vector2(150, 1500) },
                    { "discount", new Vector2(0.75f, 0.95f) },
                    { "refreshCost", 80 }
                }
            },
            { MysteryMerchantType.SecretArtifactDealer, new Dictionary<string, object>
                {
                    { "name", "秘密神器商人" },
                    { "description", "据说知道神器下落的神秘商人" },
                    { "spawnChance", 0.02f },
                    { "minLevel", 25 },
                    { "itemCount", new Vector2(1, 3) },
                    { "priceRange", new Vector2(10000, 100000) },
                    { "discount", new Vector2(0.2f, 0.5f) },
                    { "refreshCost", 2000 }
                }
            }
        };

        // 稀有度配置
        public static Dictionary<MerchantItemRarity, Dictionary<string, object>> RarityConfigs = new Dictionary<MerchantItemRarity, Dictionary<string, object>>
        {
            { MerchantItemRarity.Common, new Dictionary<string, object>
                {
                    { "name", "普通" },
                    { "color", "#FFFFFF" },
                    { "weight", 40 },
                    { "bonusAttributes", 0 },
                    { "secretChance", 0.0f }
                }
            },
            { MerchantItemRarity.Uncommon, new Dictionary<string, object>
                {
                    { "name", "优秀" },
                    { "color", "#1EFF00" },
                    { "weight", 30 },
                    { "bonusAttributes", 1 },
                    { "secretChance", 0.05f }
                }
            },
            { MerchantItemRarity.Rare, new Dictionary<string, object>
                {
                    { "name", "稀有" },
                    { "color", "#0070FF" },
                    { "weight", 18 },
                    { "bonusAttributes", 2 },
                    { "secretChance", 0.10f }
                }
            },
            { MerchantItemRarity.Epic, new Dictionary<string, object>
                {
                    { "name", "史诗" },
                    { "color", "#A335EE" },
                    { "weight", 8 },
                    { "bonusAttributes", 3 },
                    { "secretChance", 0.15f }
                }
            },
            { MerchantItemRarity.Legendary, new Dictionary<string, object>
                {
                    { "name", "传说" },
                    { "color", "#FF8000" },
                    { "weight", 3 },
                    { "bonusAttributes", 4 },
                    { "secretChance", 0.25f }
                }
            },
            { MerchantItemRarity.Mythical, new Dictionary<string, object>
                {
                    { "name", "神话" },
                    { "color", "#E6CC80" },
                    { "weight", 1 },
                    { "bonusAttributes", 5 },
                    { "secretChance", 0.40f }
                }
            }
        };

        // 商店区域配置
        public static Dictionary<string, List<MysteryMerchantType>> RegionMerchantTypes = new Dictionary<string, List<MysteryMerchantType>>
        {
            { "town", new List<MysteryMerchantType> { MysteryMerchantType.TravelingMerchant, MysteryMerchantType.RareMaterialVendor, MysteryMerchantType.LuckyCharmSeller } },
            { "forest", new List<MysteryMerchantType> { MysteryMerchantType.TravelingMerchant, MysteryMerchantType.AncientCollector, MysteryMerchantType.LuckyCharmSeller } },
            { "cave", new List<MysteryMerchantType> { MysteryMerchantType.BlackMarketDealer, MysteryMerchantType.CursedItemDealer } },
            { "mountain", new List<MysteryMerchantType> { MysteryMerchantType.DragonHoardKeeper, MysteryMerchantType.AncientCollector, MysteryMerchantType.SecretArtifactDealer } },
            { "desert", new List<MysteryMerchantType> { MysteryMerchantType.TravelingMerchant, MysteryMerchantType.AncientCollector } },
            { "volcano", new List<MysteryMerchantType> { MysteryMerchantType.DragonHoardKeeper, MysteryMerchantType.CursedItemDealer, MysteryMerchantType.SecretArtifactDealer } },
            { "snow", new List<MysteryMerchantType> { MysteryMerchantType.TravelingMerchant, MysteryMerchantType.LuckyCharmSeller, MysteryMerchantType.RareMaterialVendor } },
            { "swamp", new List<MysteryMerchantType> { MysteryMerchantType.BlackMarketDealer, MysteryMerchantType.CursedItemDealer } }
        };

        // 预定义商品列表（按类型和稀有度）
        public static Dictionary<string, List<Dictionary<string, object>>> ItemPool = new Dictionary<string, List<Dictionary<string, object>>>
        {
            { "weapon", new List<Dictionary<string, object>>
                {
                    new Dictionary<string, object> { { "id", "sword_basic" }, { "name", "铁剑" }, { "price", 100 } },
                    new Dictionary<string, object> { { "id", "sword_flame" }, { "name", "火焰剑" }, { "price", 500 } },
                    new Dictionary<string, object> { { "id", "axe_ice" }, { "name", "冰霜战斧" }, { "price", 800 } },
                    new Dictionary<string, object> { { "id", "dagger_shadow" }, { "name", "暗影匕首" }, { "price", 1200 } },
                    new Dictionary<string, object> { { "id", "hammer_thunder" }, { "name", "雷神之锤" }, { "price", 5000 } },
                    new Dictionary<string, object> { { "id", "sword_legendary" }, { "name", "王者之剑" }, { "price", 20000 } }
                }
            },
            { "armor", new List<Dictionary<string, object>>
                {
                    new Dictionary<string, object> { { "id", "armor_leather" }, { "name", "皮甲" }, { "price", 80 } },
                    new Dictionary<string, object> { { "id", "armor_chain" }, { "name", "锁甲" }, { "price", 200 } },
                    new Dictionary<string, object> { { "id", "armor_plate" }, { "name", "板甲" }, { "price", 600 } },
                    new Dictionary<string, object> { { "id", "robe_magic" }, { "name", "魔法长袍" }, { "price", 400 } },
                    new Dictionary<string, object> { { "id", "armor_dragon" }, { "name", "龙鳞甲" }, { "price", 8000 } },
                    new Dictionary<string, object> { { "id", "armor_legendary" }, { "name", "神圣战甲" }, { "price", 25000 } }
                }
            },
            { "accessory", new Dictionary<string, object>>
                {
                    { "id", "ring_power" }, { "name", "力量之戒" }, { "price", 150 }
                }
            },
            { "potion", new List<Dictionary<string, object>>
                {
                    new Dictionary<string, object> { { "id", "potion_health" }, { "name", "生命药水" }, { "price", 20 } },
                    new Dictionary<string, object> { { "id", "potion_mana" }, { "name", "魔法药水" }, { "price", 20 } },
                    new Dictionary<string, object> { { "id", "potion_strength" }, { "name", "力量药水" }, { "price", 100 } },
                    new Dictionary<string, object> { { "id", "potion_elixir" }, { "name", "万能精华" }, { "price", 500 } },
                    new Dictionary<string, object> { { "id", "potion_legendary" }, { "name", "龙血药剂" }, { "price", 3000 } }
                }
            },
            { "material", new List<Dictionary<string, object>>
                {
                    new Dictionary<string, object> { { "id", "material_iron" }, { "name", "铁矿" }, { "price", 10 } },
                    new Dictionary<string, object> { { "id", "material_gem" }, { "name", "宝石" }, { "price", 50 } },
                    new Dictionary<string, object> { { "id", "material_magic" }, { "name", "魔法水晶" }, { "price", 100 } },
                    new Dictionary<string, object> { { "id", "material_dragon" }, { "name", "龙鳞" }, { "price", 500 } },
                    new Dictionary<string, object> { { "id", "material_phoenix" }, { "name", "凤凰羽" }, { "price", 2000 } }
                }
            },
            { "charm", new List<Dictionary<string, object>>
                {
                    new Dictionary<string, object> { { "id", "charm_luck" }, { "name", "幸运护符" }, { "price", 100 } },
                    new Dictionary<string, object> { { "id", "charm_fortune" }, { "name", "财富硬币" }, { "price", 300 } },
                    new Dictionary<string, object> { { "id", "charm_protection" }, { "name", "保护符咒" }, { "price", 250 } },
                    new Dictionary<string, object> { { "id", "charm_ancient" }, { "name", "古代神符" }, { "price", 1500 } },
                    new Dictionary<string, object> { { "id", "charm_dragon" }, { "name", "龙之印记" }, { "price", 5000 } }
                }
            },
            { "relic", new List<Dictionary<string, object>>
                {
                    new Dictionary<string, object> { { "id", "relic_ancient" }, { "name", "古代遗物" }, { "price", 2000 } },
                    new Dictionary<string, object> { { "id", "relic_holy" }, { "name", "圣遗物" }, { "price", 8000 } },
                    new Dictionary<string, object> { { "id", "relic_chaos" }, { "name", "混沌神器" }, { "price", 15000 } },
                    new Dictionary<string, object> { { "id", "relic_power" }, { "name", "能量核心" }, { "price", 10000 } }
                }
            }
        };

        // 额外属性池
        public static string[] AttributePool = new string[]
        {
            "attack", "defense", "health", "magic", "speed", 
            "crit_rate", "crit_damage", "lifesteal", "dodge", "block",
            "fire_resist", "ice_resist", "lightning_resist", "dark_resist",
            "exp_bonus", "gold_bonus", "drop_bonus", "luck"
        };

        // 获取商人类型配置
        public static Dictionary<string, object> GetMerchantConfig(MysteryMerchantType type)
        {
            if (MerchantTypeConfigs.ContainsKey(type))
                return MerchantTypeConfigs[type];
            return null;
        }

        // 获取稀有度配置
        public static Dictionary<string, object> GetRarityConfig(MerchantItemRarity rarity)
        {
            if (RarityConfigs.ContainsKey(rarity))
                return RarityConfigs[rarity];
            return null;
        }

        // 根据权重随机获取稀有度
        public static MerchantItemRarity GetRandomRarity()
        {
            var random = new Random();
            int totalWeight = 0;
            foreach (var config in RarityConfigs)
            {
                totalWeight += (int)config.Value["weight"];
            }
            
            int roll = random.Next(totalWeight);
            int cumulative = 0;
            
            foreach (var config in RarityConfigs)
            {
                cumulative += (int)config.Value["weight"];
                if (roll < cumulative)
                    return config.Key;
            }
            
            return MerchantItemRarity.Common;
        }

        // 获取商人类型名称
        public static string GetMerchantTypeName(MysteryMerchantType type)
        {
            if (MerchantTypeConfigs.ContainsKey(type))
                return (string)MerchantTypeConfigs[type]["name"];
            return type.ToString();
        }

        // 获取稀有度名称
        public static string GetRarityName(MerchantItemRarity rarity)
        {
            if (RarityConfigs.ContainsKey(rarity))
                return (string)RarityConfigs[rarity]["name"];
            return rarity.ToString();
        }

        // 获取稀有度颜色
        public static string GetRarityColor(MerchantItemRarity rarity)
        {
            if (RarityConfigs.ContainsKey(rarity))
                return (string)RarityConfigs[rarity]["color"];
            return "#FFFFFF";
        }
    }
}
