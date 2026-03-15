using Godot;
using System;
using System.Collections.Generic;

/// <summary>
/// 战利品掉落数据 - 定义掉落相关的枚举和数据结构
/// </summary>
public class LootDropData
{
    /// <summary>
    /// 掉落稀有度枚举
    /// </summary>
    public enum LootRarity
    {
        Common = 0,    // 普通
        Uncommon = 1,  // 优秀
        Rare = 2,      // 稀有
        Epic = 3,      // 史诗
        Legendary = 4  // 传说
    }

    /// <summary>
    /// 战利品类型枚举
    /// </summary>
    public enum LootType
    {
        Gold,       // 金币
        Item,       // 物品
        Equipment,  // 装备
        Material,   // 材料
        Pet,        // 宠物
        Mount,      // 坐骑
        Currency    // 货币
    }

    [Serializable]
    /// <summary>
    /// 战利品条目定义
    /// </summary>
    public class LootEntry
    {
        public string Id;             // 唯一标识
        public string Name;           // 名称
        public string Description;    // 描述
        public LootType Type;         // 类型
        public LootRarity Rarity;     // 稀有度
        public int BaseWeight;        // 基础权重
        public int MinDropLevel;      // 最低掉落等级
        public int MaxDropLevel;      // 最高掉落等级
        public string IconPath;       // 图标路径
        
        // For items/equipment
        public string ItemId;         // 物品ID
        public int MinQuantity;       // 最小数量
        public int MaxQuantity;       // 最大数量
    }

    [Serializable]
    /// <summary>
    /// 战利品池定义
    /// </summary>
    public class LootPool
    {
        public string Id;                     // 池ID
        public string Name;                   // 池名称
        public string Description;            // 描述
        public List<LootEntry> Entries;       // 条目列表
        public int TotalWeight;               // 总权重
        public float DropRate = 1.0f;         // 掉落率
    }

    [Serializable]
    /// <summary>
    /// 玩家战利品数据
    /// </summary>
    public class PlayerLootData
    {
        public int TotalDrops;                                    // 总掉落次数
        public Dictionary<string, int> RarityDrops = new Dictionary<string, int>();    // 按稀有度统计
        public Dictionary<string, int> TypeDrops = new Dictionary<string, int>();      // 按类型统计
        public Dictionary<string, int> DropHistory = new Dictionary<string, int>();    // 掉落历史
        public int LuckyDrops;                                    // 幸运掉落次数
        public int CriticalDrops;                                 // 暴击掉落次数
        public float TotalLuckValue;                              // 总幸运值
    }
}

/// <summary>
/// 战利品掉落数据库 - 管理所有战利品池和掉落规则
/// </summary>
public class LootDropDatabase
{
    private static LootDropDatabase _instance;
    /// <summary>
    /// 获取数据库单例实例
    /// </summary>
    public static LootDropDatabase Instance
    {
        get
        {
            if (_instance == null) _instance = new LootDropDatabase();
            return _instance;
        }
    }

    /// <summary>
    /// 所有战利品池列表
    /// </summary>
    public List<LootDropData.LootPool> Pools = new List<LootDropData.LootPool>();
    
    // 稀有度权重: 普通50%, 优秀30%, 稀有15%, 史诗4%, 传说1%
    private readonly int[] RarityWeights = { 50, 30, 15, 4, 1 };
    private readonly int TotalRarityWeight = 100;

    public LootDropDatabase()
    {
        InitializePools();
    }

    /// <summary>
    /// 初始化所有战利品池
    /// </summary>
    private void InitializePools()
    {
        // Enemy drops pool
        var enemyPool = new LootDropData.LootPool
        {
            Id = "enemy_drop",
            Name = "Enemy Drop",
            Description = "Standard enemy drop pool",
            Entries = new List<LootDropData.LootEntry>
            {
                // Common drops
                new LootDropData.LootEntry { Id = "gold_small", Name = "Small Gold Pouch", Description = "A small pouch of gold coins", Type = LootDropData.LootType.Gold, Rarity = LootDropData.LootRarity.Common, BaseWeight = 30, MinQuantity = 5, MaxQuantity = 20 },
                new LootDropData.LootEntry { Id = "herb_green", Name = "Green Herb", Description = "A common healing herb", Type = LootDropData.LootType.Material, Rarity = LootDropData.LootRarity.Common, BaseWeight = 25, MinQuantity = 1, MaxQuantity = 3 },
                new LootDropData.LootEntry { Id = "bone_ordinary", Name = "Ordinary Bone", Description = "A simple animal bone", Type = LootDropData.LootType.Material, Rarity = LootDropData.LootRarity.Common, BaseWeight = 20, MinQuantity = 1, MaxQuantity = 2 },
                new LootDropData.LootEntry { Id = "cloth_scrap", Name = "Cloth Scrap", Description = "A piece of torn cloth", Type = LootDropData.LootType.Material, Rarity = LootDropData.LootRarity.Common, BaseWeight = 20, MinQuantity = 1, MaxQuantity = 3 },
                
                // Uncommon drops
                new LootDropData.LootEntry { Id = "gold_medium", Name = "Medium Gold Pouch", Description = "A medium pouch of gold coins", Type = LootDropData.LootType.Gold, Rarity = LootDropData.LootRarity.Uncommon, BaseWeight = 20, MinQuantity = 20, MaxQuantity = 50 },
                new LootDropData.LootEntry { Id = "herb_red", Name = "Red Herb", Description = "A rare healing herb", Type = LootDropData.LootType.Material, Rarity = LootDropData.LootRarity.Uncommon, BaseWeight = 15, MinQuantity = 1, MaxQuantity = 2 },
                new LootDropData.LootEntry { Id = "iron_ore", Name = "Iron Ore", Description = "A piece of iron ore", Type = LootDropData.LootType.Material, Rarity = LootDropData.LootRarity.Uncommon, BaseWeight = 12, MinQuantity = 1, MaxQuantity = 3 },
                new LootDropData.LootEntry { Id = "magic_stone", Name = "Magic Stone", Description = "A stone imbued with magic", Type = LootDropData.LootType.Material, Rarity = LootDropData.LootRarity.Uncommon, BaseWeight = 10, MinQuantity = 1, MaxQuantity = 2 },
                
                // Rare drops
                new LootDropData.LootEntry { Id = "gold_large", Name = "Large Gold Pouch", Description = "A large pouch of gold coins", Type = LootDropData.LootType.Gold, Rarity = LootDropData.LootRarity.Rare, BaseWeight = 10, MinQuantity = 50, MaxQuantity = 100 },
                new LootDropData.LootEntry { Id = "gem_ruby", Name = "Ruby", Description = "A precious ruby gem", Type = LootDropData.LootType.Currency, Rarity = LootDropData.LootRarity.Rare, BaseWeight = 5, MinQuantity = 1, MaxQuantity = 1 },
                new LootDropData.LootEntry { Id = "gem_sapphire", Name = "Sapphire", Description = "A precious sapphire gem", Type = LootDropData.LootType.Currency, Rarity = LootDropData.LootRarity.Rare, BaseWeight = 5, MinQuantity = 1, MaxQuantity = 1 },
                
                // Epic drops
                new LootDropData.LootEntry { Id = "gold_precious", Name = "Precious Gold Chest", Description = "A chest filled with gold", Type = LootDropData.LootType.Gold, Rarity = LootDropData.LootRarity.Epic, BaseWeight = 4, MinQuantity = 100, MaxQuantity = 200 },
                new LootDropData.LootEntry { Id = "gem_diamond", Name = "Diamond", Description = "A rare diamond gem", Type = LootDropData.LootType.Currency, Rarity = LootDropData.LootRarity.Epic, BaseWeight = 2, MinQuantity = 1, MaxQuantity = 2 },
                new LootDropData.LootEntry { Id = "enhancement_stone", Name = "Enhancement Stone", Description = "A stone used for equipment enhancement", Type = LootDropData.LootType.Currency, Rarity = LootDropData.LootRarity.Epic, BaseWeight = 2, MinQuantity = 1, MaxQuantity = 3 },
                
                // Legendary drops
                new LootDropData.LootEntry { Id = "legendary_chest", Name = "Legendary Treasure Chest", Description = "An ancient chest containing legendary treasures", Type = LootDropData.LootType.Gold, Rarity = LootDropData.LootRarity.Legendary, BaseWeight = 1, MinQuantity = 500, MaxQuantity = 1000 },
                new LootDropData.LootEntry { Id = "evolution_stone", Name = "Evolution Stone", Description = "A mystical stone for evolution", Type = LootDropData.LootType.Currency, Rarity = LootDropData.LootRarity.Legendary, BaseWeight = 1, MinQuantity = 1, MaxQuantity = 1 },
            }
        };
        CalculatePoolWeights(enemyPool);
        Pools.Add(enemyPool);

        // Boss drops pool
        var bossPool = new LootDropData.LootPool
        {
            Id = "boss_drop",
            Name = "Boss Drop",
            Description = "Boss enemy drop pool with better rewards",
            DropRate = 2.0f,
            Entries = new List<LootDropData.LootEntry>
            {
                // Common
                new LootDropData.LootEntry { Id = "gold_boss", Name = "Boss Gold Pouch", Description = "Gold from a defeated boss", Type = LootDropData.LootType.Gold, Rarity = LootDropData.LootRarity.Common, BaseWeight = 25, MinQuantity = 100, MaxQuantity = 200 },
                
                // Uncommon
                new LootDropData.LootEntry { Id = "boss_material_1", Name = "Boss Essence", Description = "Essence of a powerful boss", Type = LootDropData.LootType.Material, Rarity = LootDropData.LootRarity.Uncommon, BaseWeight = 20, MinQuantity = 1, MaxQuantity = 3 },
                
                // Rare
                new LootDropData.LootEntry { Id = "rare_equipment", Name = "Rare Equipment", Description = "A rare piece of equipment", Type = LootDropData.LootType.Equipment, Rarity = LootDropData.LootRarity.Rare, BaseWeight = 15, MinQuantity = 1, MaxQuantity = 1 },
                new LootDropData.LootEntry { Id = "gem_epic", Name = "Epic Gem", Description = "A rare gem", Type = LootDropData.LootType.Currency, Rarity = LootDropData.LootRarity.Rare, BaseWeight = 10, MinQuantity = 1, MaxQuantity = 2 },
                
                // Epic
                new LootDropData.LootEntry { Id = "epic_equipment", Name = "Epic Equipment", Description = "An epic piece of equipment", Type = LootDropData.LootType.Equipment, Rarity = LootDropData.LootRarity.Epic, BaseWeight = 8, MinQuantity = 1, MaxQuantity = 1 },
                new LootDropData.LootEntry { Id = "evolution_stone_epic", Name = "Epic Evolution Stone", Description = "A powerful evolution stone", Type = LootDropData.LootType.Currency, Rarity = LootDropData.LootRarity.Epic, BaseWeight = 5, MinQuantity = 1, MaxQuantity = 2 },
                
                // Legendary
                new LootDropData.LootEntry { Id = "legendary_equipment", Name = "Legendary Equipment", Description = "A legendary piece of equipment", Type = LootDropData.LootType.Equipment, Rarity = LootDropData.LootRarity.Legendary, BaseWeight = 3, MinQuantity = 1, MaxQuantity = 1 },
                new LootDropData.LootEntry { Id = "pet_scroll", Name = "Pet Summon Scroll", Description = "A scroll to summon a random pet", Type = LootDropData.LootType.Pet, Rarity = LootDropData.LootRarity.Legendary, BaseWeight = 2, MinQuantity = 1, MaxQuantity = 1 },
                new LootDropData.LootEntry { Id = "mount_scroll", Name = "Mount Summon Scroll", Description = "A scroll to summon a random mount", Type = LootDropData.LootType.Mount, Rarity = LootDropData.LootRarity.Legendary, BaseWeight = 1, MinQuantity = 1, MaxQuantity = 1 },
            }
        };
        CalculatePoolWeights(bossPool);
        Pools.Add(bossPool);

        // Treasure chest pool
        var treasurePool = new LootDropData.LootPool
        {
            Id = "treasure",
            Name = "Treasure",
            Description = "Treasure chest drop pool",
            DropRate = 3.0f,
            Entries = new List<LootDropData.LootEntry>
            {
                new LootDropData.LootEntry { Id = "treasure_gold_1", Name = "Bronze Treasure", Description = "A small treasure", Type = LootDropData.LootType.Gold, Rarity = LootDropData.LootRarity.Common, BaseWeight = 30, MinQuantity = 50, MaxQuantity = 100 },
                new LootDropData.LootEntry { Id = "treasure_gold_2", Name = "Silver Treasure", Description = "A moderate treasure", Type = LootDropData.LootType.Gold, Rarity = LootDropData.LootRarity.Uncommon, BaseWeight = 25, MinQuantity = 100, MaxQuantity = 250 },
                new LootDropData.LootEntry { Id = "treasure_gold_3", Name = "Gold Treasure", Description = "A valuable treasure", Type = LootDropData.LootType.Gold, Rarity = LootDropData.LootRarity.Rare, BaseWeight = 20, MinQuantity = 250, MaxQuantity = 500 },
                new LootDropData.LootEntry { Id = "treasure_gold_4", Name = "Platinum Treasure", Description = "A precious treasure", Type = LootDropData.LootType.Gold, Rarity = LootDropData.LootRarity.Epic, BaseWeight = 15, MinQuantity = 500, MaxQuantity = 1000 },
                new LootDropData.LootEntry { Id = "treasure_gold_5", Name = "Diamond Treasure", Description = "An invaluable treasure", Type = LootDropData.LootType.Gold, Rarity = LootDropData.LootRarity.Legendary, BaseWeight = 10, MinQuantity = 1000, MaxQuantity = 2500 },
            }
        };
        CalculatePoolWeights(treasurePool);
        Pools.Add(treasurePool);
    }

    /// <summary>
    /// 计算战利品池权重
    /// </summary>
    private void CalculatePoolWeights(LootDropData.LootPool pool)
    {
        pool.TotalWeight = 0;
        foreach (var entry in pool.Entries)
        {
            pool.TotalWeight += entry.BaseWeight;
        }
    }

    /// <summary>
    /// 获取指定ID的战利品池
    /// </summary>
    public LootDropData.LootPool GetPool(string poolId)
    {
        foreach (var pool in Pools)
        {
            if (pool.Id == poolId) return pool;
        }
        return null;
    }

    /// <summary>
    /// 随机掉落稀有度
    /// </summary>
    /// <param name="luckBonus">幸运加成</param>
    /// <returns>掉落的稀有度</returns>
    public LootDropData.LootRarity RollRarity(float luckBonus = 0f)
    {
        // Apply luck bonus to improve drop rarity
        var adjustedWeights = (int[])RarityWeights.Clone();
        int totalAdjusted = TotalRarityWeight;
        
        // Luck affects the roll
        float luckRoll = (float)GD.RandRange(0, totalAdjusted + luckBonus * 10);
        
        int cumulative = 0;
        for (int i = 0; i < adjustedWeights.Length; i++)
        {
            cumulative += adjustedWeights[i];
            if (luckRoll < cumulative)
            {
                return (LootDropData.LootRarity)i;
            }
        }
        return LootDropData.LootRarity.Common;
    }

    public LootDropData.LootEntry RollLoot(LootDropData.LootPool pool, float luckBonus = 0f)
    {
        if (pool == null || pool.Entries.Count == 0) return null;
        
        // Roll for drop
        if (GD.Randf() > pool.DropRate * (1.0f + luckBonus * 0.1f))
        {
            return null;
        }
        
        // Filter by rarity based on luck
        var targetRarity = RollRarity(luckBonus);
        
        // Get entries of target rarity or better
        var candidates = new List<LootDropData.LootEntry>();
        foreach (var entry in pool.Entries)
        {
            if (entry.Rarity <= targetRarity)
            {
                candidates.Add(entry);
            }
        }
        
        if (candidates.Count == 0) candidates = pool.Entries;
        
        // Weighted random selection
        int totalWeight = 0;
        foreach (var entry in candidates)
        {
            // Higher luck increases weight for better items
            int weight = entry.BaseWeight;
            if (entry.Rarity == targetRarity)
            {
                weight = (int)(weight * (1.0f + luckBonus * 0.2f));
            }
            totalWeight += weight;
        }
        
        int roll = GD.RandRange(0, totalWeight);
        int cumulative = 0;
        
        foreach (var entry in candidates)
        {
            int weight = entry.BaseWeight;
            if (entry.Rarity == targetRarity)
            {
                weight = (int)(weight * (1.0f + luckBonus * 0.2f));
            }
            cumulative += weight;
            if (roll < cumulative)
            {
                return entry;
            }
        }
        
        return candidates[candidates.Count - 1];
    }
}
