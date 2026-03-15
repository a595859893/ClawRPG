using Godot;
using System;
using System.Collections.Generic;

/// <summary>
/// Loot drop data definitions including enums and data structures.
/// </summary>
public class LootDropData
{
    /// <summary>
    /// Defines the rarity tier of loot.
    /// </summary>
    public enum LootRarity
    {
        /// <summary>Common tier - most frequent drops.</summary>
        Common = 0,
        
        /// <summary>Uncommon tier - less frequent than common.</summary>
        Uncommon = 1,
        
        /// <summary>Rare tier - infrequent drops with better stats.</summary>
        Rare = 2,
        
        /// <summary>Epic tier - very rare drops with excellent stats.</summary>
        Epic = 3,
        
        /// <summary>Legendary tier - extremely rare, best possible drops.</summary>
        Legendary = 4
    }

    /// <summary>
    /// Defines the type of loot that can be dropped.
    /// </summary>
    public enum LootType
    {
        /// <summary>Gold currency.</summary>
        Gold,
        
        /// <summary>Regular item.</summary>
        Item,
        
        /// <summary>Equipment piece.</summary>
        Equipment,
        
        /// <summary>Crafting or enhancement material.</summary>
        Material,
        
        /// <summary>Pet companion.</summary>
        Pet,
        
        /// <summary>Mount or vehicle.</summary>
        Mount,
        
        /// <summary>Special currency (gems, tokens, etc.).</summary>
        Currency
    }

    /// <summary>
    /// Defines a single loot entry in a loot pool.
    /// </summary>
    [Serializable]
    public class LootEntry
    {
        /// <summary>Unique identifier for this loot entry.</summary>
        public string Id;
        
        /// <summary>Display name of the loot.</summary>
        public string Name;
        
        /// <summary>Description of the loot item.</summary>
        public string Description;
        
        /// <summary>Type of loot (Gold, Item, Equipment, etc.).</summary>
        public LootType Type;
        
        /// <summary>Rarity tier of this loot.</summary>
        public LootRarity Rarity;
        
        /// <summary>Base weight for random selection.</summary>
        public int BaseWeight;
        
        /// <summary>Minimum player level required for this drop.</summary>
        public int MinDropLevel;
        
        /// <summary>Maximum player level for this drop.</summary>
        public int MaxDropLevel;
        
        /// <summary>Path to the icon resource.</summary>
        public string IconPath;
        
        // For items/equipment
        
        /// <summary>Item database ID for this loot.</summary>
        public string ItemId;
        
        /// <summary>Minimum quantity when dropped.</summary>
        public int MinQuantity;
        
        /// <summary>Maximum quantity when dropped.</summary>
        public int MaxQuantity;
    }

    /// <summary>
    /// Defines a collection of loot entries that can be rolled from.
    /// </summary>
    [Serializable]
    public class LootPool
    {
        /// <summary>Unique identifier for this loot pool.</summary>
        public string Id;
        
        /// <summary>Display name of the pool.</summary>
        public string Name;
        
        /// <summary>Description of what this pool contains.</summary>
        public string Description;
        
        /// <summary>List of loot entries in this pool.</summary>
        public List<LootEntry> Entries;
        
        /// <summary>Total weight of all entries combined.</summary>
        public int TotalWeight;
        
        /// <summary>Base drop rate multiplier (default 1.0).</summary>
        public float DropRate = 1.0f;
    }

    /// <summary>
    /// Stores player-specific loot drop statistics.
    /// </summary>
    [Serializable]
    public class PlayerLootData
    {
        /// <summary>Total number of drops received.</summary>
        public int TotalDrops;
        
        /// <summary>Drop counts keyed by rarity name.</summary>
        public Dictionary<string, int> RarityDrops = new Dictionary<string, int>();
        
        /// <summary>Drop counts keyed by loot type.</summary>
        public Dictionary<string, int> TypeDrops = new Dictionary<string, int>();
        
        /// <summary>Drop counts keyed by loot item ID.</summary>
        public Dictionary<string, int> DropHistory = new Dictionary<string, int>();
        
        /// <summary>Number of drops that were considered lucky (rare or better).</summary>
        public int LuckyDrops;
        
        /// <summary>Number of critical (double) drops that occurred.</summary>
        public int CriticalDrops;
        
        /// <summary>Total accumulated luck value from items and effects.</summary>
        public float TotalLuckValue;
    }
}

/// <summary>
/// Loot drop database that manages all loot pools and drop rules.
/// </summary>
public class LootDropDatabase
{
    private static LootDropDatabase _instance;
    
    /// <summary>
    /// Gets the singleton instance of the LootDropDatabase.
    /// </summary>
    /// <value>The global instance for loot database operations.</value>
    public static LootDropDatabase Instance
    {
        get
        {
            if (_instance == null) _instance = new LootDropDatabase();
            return _instance;
        }
    }

    /// <summary>
    /// List of all defined loot pools.
    /// </summary>
    public List<LootDropData.LootPool> Pools = new List<LootDropData.LootPool>();
    
    // Default rarity weights: Common 50%, Uncommon 30%, Rare 15%, Epic 4%, Legendary 1%
    private readonly int[] RarityWeights = { 50, 30, 15, 4, 1 };
    private readonly int TotalRarityWeight = 100;

    public LootDropDatabase()
    {
        InitializePools();
    }

    /// <summary>
    /// Initializes all loot pools with their entries.
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
    /// Calculates total weight for a loot pool based on its entries.
    /// </summary>
    /// <param name="pool">The loot pool to calculate weights for.</param>
    private void CalculatePoolWeights(LootDropData.LootPool pool)
    {
        pool.TotalWeight = 0;
        foreach (var entry in pool.Entries)
        {
            pool.TotalWeight += entry.BaseWeight;
        }
    }

    /// <summary>
    /// Gets a loot pool by its ID.
    /// </summary>
    /// <param name="poolId">The pool identifier.</param>
    /// <returns>The matching LootPool, or null if not found.</returns>
    public LootDropData.LootPool GetPool(string poolId)
    {
        foreach (var pool in Pools)
        {
            if (pool.Id == poolId) return pool;
        }
        return null;
    }

    /// <summary>
    /// Rolls for a random rarity based on luck bonus.
    /// </summary>
    /// <param name="luckBonus">Additional luck value affecting the roll.</param>
    /// <returns>The rolled rarity tier.</returns>
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
