using Godot;
using System;
using System.Collections.Generic;

/// <summary>
/// MysteryTreasureDatabase - 神秘宝藏数据库
/// 管理所有神秘宝箱的配置和奖励池
/// </summary>
public class MysteryTreasureDatabase
{
    private static MysteryTreasureDatabase _instance;
    public static MysteryTreasureDatabase Instance
    {
        get
        {
            if (_instance == null)
                _instance = new MysteryTreasureDatabase();
            return _instance;
        }
    }

    // 宝藏定义
    private Dictionary<string, MysteryTreasureData> _treasures = new Dictionary<string, MysteryTreasureData>();
    
    // 按稀有度索引
    private Dictionary<TreasureRarity, List<string>> _treasuresByRarity = new Dictionary<TreasureRarity, List<string>>();
    
    // 按类型索引
    private Dictionary<TreasureType, List<string>> _treasuresByType = new Dictionary<TreasureType, List<string>>();

    public MysteryTreasureDatabase()
    {
        InitializeTreasures();
    }

    private void InitializeTreasures()
    {
        // 初始化索引
        foreach (TreasureRarity rarity in Enum.GetValues(typeof(TreasureRarity)))
            _treasuresByRarity[rarity] = new List<string>();
        
        foreach (TreasureType type in Enum.GetValues(typeof(TreasureType)))
            _treasuresByType[type] = new List<string>();

        // 添加宝藏定义
        
        // ===== 普通宝藏 (Common) =====
        AddTreasure(new MysteryTreasureData
        {
            TreasureId = "small_chest",
            TreasureName = "小箱子",
            Description = "一个普通的小箱子，里面可能有一些零钱",
            Rarity = TreasureRarity.Common,
            Type = TreasureType.Chest,
            MinGold = 10,
            MaxGold = 50,
            ItemIds = new List<string> { "health_potion_small" },
            ItemCounts = new List<int> { 1 },
            ExpReward = 10,
            SpawnChance = 0.3f
        });
        
        AddTreasure(new MysteryTreasureData
        {
            TreasureId = "coin_pile",
            TreasureName = "钱币堆",
            Description = "地面上散落的一些钱币",
            Rarity = TreasureRarity.Common,
            Type = TreasureType.Hidden,
            MinGold = 20,
            MaxGold = 80,
            ItemIds = new List<string>(),
            ItemCounts = new List<int>(),
            ExpReward = 5,
            SpawnChance = 0.25f
        });
        
        AddTreasure(new MysteryTreasureData
        {
            TreasureId = "weak_enemy_drop",
            TreasureName = "怪物战利品",
            Description = "击败普通怪物后掉落的战利品",
            Rarity = TreasureRarity.Common,
            Type = TreasureType.Monster,
            MinGold = 5,
            MaxGold = 30,
            ItemIds = new List<string> { "monster_bone" },
            ItemCounts = new List<int> { 1 },
            ExpReward = 15,
            SpawnChance = 0.2f
        });

        // ===== 优秀宝藏 (Uncommon) =====
        AddTreasure(new MysteryTreasureData
        {
            TreasureId = "medium_chest",
            TreasureName = "中箱子",
            Description = "一个中等大小的箱子，里面有不错的宝贝",
            Rarity = TreasureRarity.Uncommon,
            Type = TreasureType.Chest,
            MinGold = 50,
            MaxGold = 150,
            ItemIds = new List<string> { "health_potion_medium", "mana_potion_small" },
            ItemCounts = new List<int> { 2, 1 },
            ExpReward = 30,
            SpawnChance = 0.2f
        });
        
        AddTreasure(new MysteryTreasureData
        {
            TreasureId = "hidden_cache",
            TreasureName = "隐藏补给",
            Description = "藏在角落里的补给箱",
            Rarity = TreasureRarity.Uncommon,
            Type = TreasureType.Hidden,
            MinGold = 40,
            MaxGold = 120,
            ItemIds = new List<string> { "enhancement_stone" },
            ItemCounts = new List<int> { 2 },
            ExpReward = 25,
            SpawnChance = 0.15f
        });
        
        AddTreasure(new MysteryTreasureData
        {
            TreasureId = "elite_enemy_drop",
            TreasureName = "精英怪物战利品",
            Description = "击败精英怪物后掉落的战利品",
            Rarity = TreasureRarity.Uncommon,
            Type = TreasureType.Monster,
            MinGold = 30,
            MaxGold = 100,
            ItemIds = new List<string> { "monster_core" },
            ItemCounts = new List<int> { 1 },
            ExpReward = 40,
            SpawnChance = 0.15f
        });

        // ===== 稀有宝藏 (Rare) =====
        AddTreasure(new MysteryTreasureData
        {
            TreasureId = "large_chest",
            TreasureName = "大箱子",
            Description = "一个华丽的大箱子，里面肯定有好东西",
            Rarity = TreasureRarity.Rare,
            Type = TreasureType.Chest,
            MinGold = 150,
            MaxGold = 400,
            ItemIds = new List<string> { "health_potion_large", "mana_potion_medium", "rare_gem" },
            ItemCounts = new List<int> { 3, 2, 1 },
            ExpReward = 80,
            SpawnChance = 0.12f
        });
        
        AddTreasure(new MysteryTreasureData
        {
            TreasureId = "ancient_ruins",
            TreasureName = "远古遗迹",
            Description = "古老的遗迹中隐藏的宝藏",
            Rarity = TreasureRarity.Rare,
            Type = TreasureType.Ancient,
            MinGold = 100,
            MaxGold = 300,
            ItemIds = new List<string> { "ancient_coin", "rare_material" },
            ItemCounts = new List<int> { 3, 1 },
            ExpReward = 100,
            SpawnChance = 0.1f
        });
        
        AddTreasure(new MysteryTreasureData
        {
            TreasureId = "boss_drop",
            TreasureName = "Boss 战利品",
            Description = "击败Boss后掉落的珍贵战利品",
            Rarity = TreasureRarity.Rare,
            Type = TreasureType.Monster,
            MinGold = 200,
            MaxGold = 500,
            ItemIds = new List<string> { "boss_token", "epic_gem" },
            ItemCounts = new List<int> { 1, 1 },
            ExpReward = 200,
            SpawnChance = 0.08f
        });

        // ===== 史诗宝藏 (Epic) =====
        AddTreasure(new MysteryTreasureData
        {
            TreasureId = "treasure_room",
            TreasureName = "宝藏室",
            Description = "隐藏房间里的宝藏室",
            Rarity = TreasureRarity.Epic,
            Type = TreasureType.Chest,
            MinGold = 400,
            MaxGold = 1000,
            ItemIds = new List<string> { "epic_weapon_fragment", "epic_armor_fragment", "legendary_gem" },
            ItemCounts = new List<int> { 1, 1, 1 },
            ExpReward = 300,
            SpawnChance = 0.06f
        });
        
        AddTreasure(new MysteryTreasureData
        {
            TreasureId = "ancient_temple",
            TreasureName = "远古神庙",
            Description = "远古神庙中的神秘宝藏",
            Rarity = TreasureRarity.Epic,
            Type = TreasureType.Ancient,
            MinGold = 300,
            MaxGold = 800,
            ItemIds = new List<string> { "holy_relic", "epic_material" },
            ItemCounts = new List<int> { 1, 2 },
            ExpReward = 250,
            SpawnChance = 0.05f
        });
        
        AddTreasure(new MysteryTreasureData
        {
            TreasureId = "secret_boss_drop",
            TreasureName = "秘密Boss战利品",
            Description = "击败秘密Boss后掉落的稀有战利品",
            Rarity = TreasureRarity.Epic,
            Type = TreasureType.Monster,
            MinGold = 500,
            MaxGold = 1200,
            ItemIds = new List<string> { "secret_boss_heart", "legendary_gem" },
            ItemCounts = new List<int> { 1, 2 },
            ExpReward = 500,
            SpawnChance = 0.04f
        });

        // ===== 传说宝藏 (Legendary) =====
        AddTreasure(new MysteryTreasureData
        {
            TreasureId = "dragon_hoard",
            TreasureName = "巨龙宝藏",
            Description = "传说中巨龙的巢穴宝藏",
            Rarity = TreasureRarity.Legendary,
            Type = TreasureType.Special,
            MinGold = 1000,
            MaxGold = 5000,
            ItemIds = new List<string> { "dragon_scale", "dragon_blood", "legendary_weapon", "legendary_armor" },
            ItemCounts = new List<int> { 3, 2, 1, 1 },
            ExpReward = 1000,
            SpawnChance = 0.02f
        });
        
        AddTreasure(new MysteryTreasureData
        {
            TreasureId = "ancient_kingdom_treasure",
            TreasureName = "远古王国宝藏",
            Description = "失落的远古王国遗留的无价之宝",
            Rarity = TreasureRarity.Legendary,
            Type = TreasureType.Special,
            MinGold = 2000,
            MaxGold = 8000,
            ItemIds = new List<string> { "crown_of_kingdom", "ancient_scepter", "legendary_gem", "holy_relic" },
            ItemCounts = new List<int> { 1, 1, 3, 2 },
            ExpReward = 2000,
            SpawnChance = 0.01f
        });
        
        AddTreasure(new MysteryTreasureData
        {
            TreasureId = "world_boss_drop",
            TreasureName = "世界Boss战利品",
            Description = "击败世界Boss后获得的传说战利品",
            Rarity = TreasureRarity.Legendary,
            Type = TreasureType.Monster,
            MinGold = 3000,
            MaxGold = 10000,
            ItemIds = new List<string> { "world_boss_heart", "legendary_weapon", "legendary_armor", "rare_enchantment" },
            ItemCounts = new List<int> { 1, 1, 1, 1 },
            ExpReward = 3000,
            SpawnChance = 0.008f
        });
    }

    private void AddTreasure(MysteryTreasureData treasure)
    {
        _treasures[treasure.TreasureId] = treasure;
        _treasuresByRarity[treasure.Rarity].Add(treasure.TreasureId);
        _treasuresByType[treasure.Type].Add(treasure.TreasureId);
    }

    // 获取所有宝藏
    public Dictionary<string, MysteryTreasureData> GetAllTreasures() => new Dictionary<string, MysteryTreasureData>(_treasures);

    // 根据ID获取宝藏
    public MysteryTreasureData GetTreasureById(string treasureId)
    {
        if (_treasures.ContainsKey(treasureId))
            return _treasures[treasureId];
        return null;
    }

    // 根据稀有度获取宝藏
    public List<MysteryTreasureData> GetTreasuresByRarity(TreasureRarity rarity)
    {
        List<MysteryTreasureData> result = new List<MysteryTreasureData>();
        if (_treasuresByRarity.ContainsKey(rarity))
        {
            foreach (var id in _treasuresByRarity[rarity])
            {
                result.Add(_treasures[id]);
            }
        }
        return result;
    }

    // 根据类型获取宝藏
    public List<MysteryTreasureData> GetTreasuresByType(TreasureType type)
    {
        List<MysteryTreasureData> result = new List<MysteryTreasureData>();
        if (_treasuresByType.ContainsKey(type))
        {
            foreach (var id in _treasuresByType[type])
            {
                result.Add(_treasures[id]);
            }
        }
        return result;
    }

    // 随机获取宝藏（考虑刷新几率）
    public MysteryTreasureData GetRandomTreasure()
    {
        List<MysteryTreasureData> availableTreasures = new List<MysteryTreasureData>();
        
        foreach (var treasure in _treasures.Values)
        {
            if (GD.Randf() < treasure.SpawnChance)
            {
                availableTreasures.Add(treasure);
            }
        }
        
        if (availableTreasures.Count == 0)
            return null;
        
        return availableTreasures[GD.Randi() % availableTreasures.Count];
    }

    // 根据稀有度随机获取宝藏
    public MysteryTreasureData GetRandomTreasureByRarity(TreasureRarity minRarity)
    {
        List<MysteryTreasureData> candidates = new List<MysteryTreasureData>();
        
        foreach (var treasure in _treasures.Values)
        {
            if (treasure.Rarity >= minRarity && GD.Randf() < treasure.SpawnChance)
            {
                candidates.Add(treasure);
            }
        }
        
        if (candidates.Count == 0)
            return null;
        
        return candidates[GD.Randi() % candidates.Count];
    }

    // 获取稀有度颜色
    public Color GetRarityColor(TreasureRarity rarity)
    {
        switch (rarity)
        {
            case TreasureRarity.Common: return new Color(0.7f, 0.7f, 0.7f);
            case TreasureRarity.Uncommon: return new Color(0.2f, 0.8f, 0.2f);
            case TreasureRarity.Rare: return new Color(0.2f, 0.5f, 1.0f);
            case TreasureRarity.Epic: return new Color(0.6f, 0.3f, 0.9f);
            case TreasureRarity.Legendary: return new Color(1.0f, 0.6f, 0.1f);
            default: return Colors.White;
        }
    }

    // 获取稀有度名称
    public string GetRarityName(TreasureRarity rarity)
    {
        switch (rarity)
        {
            case TreasureRarity.Common: return "普通";
            case TreasureRarity.Uncommon: return "优秀";
            case TreasureRarity.Rare: return "稀有";
            case TreasureRarity.Epic: return "史诗";
            case TreasureRarity.Legendary: return "传说";
            default: return "未知";
        }
    }

    // 获取类型名称
    public string GetTypeName(TreasureType type)
    {
        switch (type)
        {
            case TreasureType.Chest: return "宝箱";
            case TreasureType.Hidden: return "隐藏";
            case TreasureType.Ancient: return "远古";
            case TreasureType.Monster: return "怪物掉落";
            case TreasureType.Special: return "特殊";
            default: return "未知";
        }
    }
}
