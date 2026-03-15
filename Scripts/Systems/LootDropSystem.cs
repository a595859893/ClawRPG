using Godot;
using System;
using System.Collections.Generic;

/// <summary>
/// Loot drop system that manages loot drops, luck bonuses, and pity mechanics.
/// Supports multiple loot pools, luck bonuses, critical drops, and guaranteed drop thresholds.
/// </summary>
public partial class LootDropSystem : BaseSystem
{
    private static LootDropSystem _instance;
    
    /// <summary>
    /// Gets the singleton instance of the LootDropSystem.
    /// </summary>
    /// <value>The global instance for loot drop operations.</value>
    public static LootDropSystem Instance
    {
        get
        {
            if (_instance == null) _instance = new LootDropSystem();
            return _instance;
        }
    }

    private LootDropData.PlayerLootData _playerData = new LootDropData.PlayerLootData();
    
    // Luck system - increases drop rate and quality
    private float _luckValue = 0f;
    private int _luckItems = 0;
    
    // Pity system - guarantees better drops after multiple attempts without good rewards
    private Dictionary<string, int> _pityCounters = new Dictionary<string, int>();
    private Dictionary<string, LootDropData.LootRarity> _pityThresholds = new Dictionary<string, LootDropData.LootRarity>();
    
    // Critical drop system - rare chance for double loot
    private float _criticalDropRate = 0.05f;
    
    /// <summary>
    /// Gets the current luck value affecting drop rates.
    /// </summary>
    /// <value>Current luck value from 0 upwards.</value>
    public float LuckValue => _luckValue;
    
    /// <summary>
    /// Gets the number of luck-boosting items used.
    /// </summary>
    /// <value>Count of luck items consumed.</value>
    public int LuckItems => _luckItems;
    
    // Signals
    
    /// <summary>
    /// Fired when loot is dropped: parameters are the loot entry and quantity.
    /// </summary>
    public Action<LootDropData.LootEntry, int> OnLootDropped;
    
    /// <summary>
    /// Fired when a rarity tier is dropped.
    /// </summary>
    public Action<LootDropData.LootRarity> OnRarityDropped;
    
    /// <summary>
    /// Fired when a lucky drop occurs (rare or better).
    /// </summary>
    public Action OnLuckyDrop;
    
    /// <summary>
    /// Fired when a critical (double) drop occurs.
    /// </summary>
    public Action OnCriticalDrop;

    public override void _Ready()
    {
        base._Ready();
    }

    /// <summary>
    /// Initializes the loot drop system.
    /// </summary>
    protected override void Initialize()
    {
        _instance = this;
        LoadPlayerData();
        InitializePitySystem();
        GD.Print("[LootDropSystem] Initialized");
    }

    /// <summary>
    /// Initializes the pity system with default thresholds.
    /// </summary>
    private void InitializePitySystem()
    {
        // Initialize pity counters for each pool
        _pityThresholds["enemy_drop"] = LootDropData.LootRarity.Rare;
        _pityThresholds["boss_drop"] = LootDropData.LootRarity.Epic;
        _pityThresholds["treasure"] = LootDropData.LootRarity.Epic;
        
        foreach (var poolId in _pityThresholds.Keys)
        {
            if (!_pityCounters.ContainsKey(poolId))
            {
                _pityCounters[poolId] = 0;
            }
        }
    }

    /// <summary>
    /// Saves player loot data to the save system.
    /// </summary>
    public void SavePlayerData()
    {
        if (SaveSystem.Instance != null)
        {
            SaveSystem.Instance.SaveLootDropData(_playerData);
        }
    }

    /// <summary>
    /// Loads player loot data from the save system.
    /// </summary>
    public void LoadPlayerData()
    {
        if (SaveSystem.Instance != null)
        {
            var data = SaveSystem.Instance.LoadLootDropData();
            if (data != null)
            {
                _playerData = data;
            }
        }
    }

    /// <summary>
    /// Rolls for loot from a specific loot pool.
    /// </summary>
    /// <param name="poolId">The loot pool ID to roll from.</param>
    /// <param name="count">Number of times to roll (default 1).</param>
    /// <returns>List of dropped loot entries.</returns>
    public List<LootDropData.LootEntry> RollLootFromPool(string poolId, int count = 1)
    {
        var results = new List<LootDropData.LootEntry>();
        var pool = LootDropDatabase.Instance.GetPool(poolId);
        
        if (pool == null)
        {
            GD.PrintErr($"[LootDropSystem] Pool not found: {poolId}");
            return results;
        }
        
        // Apply luck bonus
        float effectiveLuck = _luckValue;
        
        for (int i = 0; i < count; i++)
        {
            var loot = RollSingleLoot(pool, effectiveLuck);
            if (loot != null)
            {
                results.Add(loot);
                ApplyLoot(loot, poolId);
                
                // Check for critical drop (double loot)
                if (GD.Randf() < _criticalDropRate)
                {
                    results.Add(loot);
                    OnCriticalDrop?.Invoke();
                    _playerData.CriticalDrops++;
                }
            }
        }
        
        return results;
    }

    private LootDropData.LootEntry RollSingleLoot(LootDropData.LootPool pool, float luckBonus)
    {
        // Check pity system
        string poolId = pool.Id;
        if (_pityCounters.ContainsKey(poolId))
        {
            int pityCount = _pityCounters[poolId];
            LootDropData.LootRarity pityThreshold = _pityThresholds[poolId];
            
            // After enough attempts without a threshold drop, guarantee one
            if (pityCount >= 20 && pityThreshold <= LootDropData.LootRarity.Rare)
            {
                // Force a better drop
                luckBonus += (pityCount - 15) * 0.1f;
            }
            else if (pityCount >= 30 && pityThreshold <= LootDropData.LootRarity.Epic)
            {
                luckBonus += (pityCount - 20) * 0.15f;
            }
        }
        
        var loot = LootDropDatabase.Instance.RollLoot(pool, luckBonus);
        return loot;
    }

    private void ApplyLoot(LootDropData.LootEntry loot, string poolId)
    {
        // Update statistics
        _playerData.TotalDrops++;
        
        string rarityKey = loot.Rarity.ToString();
        if (!_playerData.RarityDrops.ContainsKey(rarityKey))
            _playerData.RarityDrops[rarityKey] = 0;
        _playerData.RarityDrops[rarityKey]++;
        
        string typeKey = loot.Type.ToString();
        if (!_playerData.TypeDrops.ContainsKey(typeKey))
            _playerData.TypeDrops[typeKey] = 0;
        _playerData.TypeDrops[typeKey]++;
        
        if (!_playerData.DropHistory.ContainsKey(loot.Id))
            _playerData.DropHistory[loot.Id] = 0;
        _playerData.DropHistory[loot.Id]++;
        
        // Reset pity counter on good drop
        if (_pityCounters.ContainsKey(poolId))
        {
            if (loot.Rarity >= _pityThresholds[poolId])
            {
                _pityCounters[poolId] = 0;
            }
            else
            {
                _pityCounters[poolId]++;
            }
        }
        
        // Check if this is a lucky drop
        bool isLucky = loot.Rarity >= LootDropData.LootRarity.Rare;
        if (isLucky)
        {
            _playerData.LuckyDrops++;
            OnLuckyDrop?.Invoke();
        }
        
        // Give items to player
        int quantity = GD.RandRange(loot.MinQuantity, loot.MaxQuantity);
        
        switch (loot.Type)
        {
            case LootDropData.LootType.Gold:
                if (Player.Instance != null)
                {
                    Player.Instance.AddGold(quantity);
                }
                break;
                
            case LootDropData.LootType.Currency:
            case LootDropData.LootType.Material:
                if (InventoryManager.Instance != null)
                {
                    InventoryManager.Instance.AddItem(loot.ItemId, quantity);
                }
                break;
                
            case LootDropData.LootType.Item:
            case LootDropData.LootType.Equipment:
                if (InventoryManager.Instance != null)
                {
                    InventoryManager.Instance.AddItem(loot.ItemId, 1);
                }
                break;
        }
        
        // Emit signals
        OnLootDropped?.Invoke(loot, quantity);
        OnRarityDropped?.Invoke(loot.Rarity);
    }

    /// <summary>
    /// Adds luck value from luck-boosting items or effects.
    /// </summary>
    /// <param name="amount">Amount of luck to add.</param>
    public void AddLuck(float amount)
    {
        _luckValue += amount;
        _playerData.TotalLuckValue += amount;
        
        // Track luck items
        if (amount > 0)
        {
            _luckItems++;
        }
    }

    /// <summary>
    /// Clears all active luck effects.
    /// </summary>
    public void ResetLuck()
    {
        _luckValue = 0f;
    }

    /// <summary>
    /// Gets the drop rate multiplier based on current luck.
    /// </summary>
    /// <returns>Multiplier applied to base drop rates.</returns>
    public float GetDropRateMultiplier()
    {
        return 1.0f + _luckValue * 0.1f;
    }

    /// <summary>
    /// Gets the quality bonus based on current luck.
    /// </summary>
    /// <returns>Bonus applied to loot quality rolls.</returns>
    public float GetQualityBonus()
    {
        return _luckValue * 0.2f;
    }

    /// <summary>
    /// Gets player loot statistics.
    /// </summary>
    /// <returns>PlayerLootData containing drop statistics.</returns>
    public LootDropData.PlayerLootData GetStatistics()
    {
        return _playerData;
    }

    /// <summary>
    /// Gets the rarity distribution as percentages.
    /// </summary>
    /// <returns>Dictionary mapping rarity names to percentages.</returns>
    public Dictionary<string, float> GetRarityDistribution()
    {
        var distribution = new Dictionary<string, float>();
        
        if (_playerData.TotalDrops == 0) return distribution;
        
        foreach (var kvp in _playerData.RarityDrops)
        {
            distribution[kvp.Key] = (float)kvp.Value / _playerData.TotalDrops * 100f;
        }
        
        return distribution;
    }

    /// <summary>
    /// Forces a drop from a pool with minimum rarity (for special events).
    /// </summary>
    /// <param name="poolId">The loot pool ID.</param>
    /// <param name="minRarity">Minimum rarity to drop.</param>
    /// <returns>The forced loot entry, or null if not possible.</returns>
    public LootDropData.LootEntry ForceDrop(string poolId, LootDropData.LootRarity minRarity)
    {
        var pool = LootDropDatabase.Instance.GetPool(poolId);
        if (pool == null) return null;
        
        // Find entries matching the minimum rarity
        var candidates = new List<LootDropData.LootEntry>();
        foreach (var entry in pool.Entries)
        {
            if (entry.Rarity >= minRarity)
            {
                candidates.Add(entry);
            }
        }
        
        if (candidates.Count == 0) return null;
        
        // Random selection from candidates
        int index = GD.RandRange(0, candidates.Count - 1);
        var loot = candidates[index];
        
        ApplyLoot(loot, poolId);
        return loot;
    }

    /// <summary>
    /// Gets the number of drops until the next pity threshold is reached.
    /// </summary>
    /// <param name="poolId">The loot pool ID.</param>
    /// <returns>Number of drops until pity triggers, or -1 if pool not found.</returns>
    public int GetDropsUntilPity(string poolId)
    {
        if (!_pityCounters.ContainsKey(poolId)) return -1;
        if (!_pityThresholds.ContainsKey(poolId)) return -1;
        
        var threshold = _pityThresholds[poolId];
        int targetCount = threshold == LootDropData.LootRarity.Rare ? 20 : 30;
        
        return Math.Max(0, targetCount - _pityCounters[poolId]);
    }

    public override Dictionary ExportSaveData()
    {
        var data = new Dictionary
        {
            { "totalDrops", _playerData.TotalDrops },
            { "luckyDrops", _playerData.LuckyDrops },
            { "criticalDrops", _playerData.CriticalDrops },
            { "totalLuckValue", _playerData.TotalLuckValue }
        };
        
        // Export rarity drops
        var rarityDrops = new Dictionary();
        foreach (var kvp in _playerData.RarityDrops)
        {
            rarityDrops[kvp.Key] = kvp.Value;
        }
        data["rarityDrops"] = rarityDrops;
        
        // Export pity counters
        var pityData = new Dictionary();
        foreach (var kvp in _pityCounters)
        {
            pityData[kvp.Key] = kvp.Value;
        }
        data["pityCounters"] = pityData;
        
        return data;
    }

    public override void ImportSaveData(Dictionary data)
    {
        if (data == null) return;
        
        if (data.Contains("totalDrops")) _playerData.TotalDrops = Convert.ToInt32(data["totalDrops"]);
        if (data.Contains("luckyDrops")) _playerData.LuckyDrops = Convert.ToInt32(data["luckyDrops"]);
        if (data.Contains("criticalDrops")) _playerData.CriticalDrops = Convert.ToInt32(data["criticalDrops"]);
        if (data.Contains("totalLuckValue")) _playerData.TotalLuckValue = Convert.ToSingle(data["totalLuckValue"]);
        
        // Import rarity drops
        if (data.Contains("rarityDrops") && data["rarityDrops"] is Dictionary rarityData)
        {
            _playerData.RarityDrops.Clear();
            foreach (var key in rarityData.Keys)
            {
                _playerData.RarityDrops[key.ToString()] = Convert.ToInt32(rarityData[key]);
            }
        }
        
        // Import pity counters
        if (data.Contains("pityCounters") && data["pityCounters"] is Dictionary pityData)
        {
            _pityCounters.Clear();
            foreach (var key in pityData.Keys)
            {
                _pityCounters[key.ToString()] = Convert.ToInt32(pityData[key]);
            }
        }
        
        GD.Print("[LootDropSystem] Data loaded");
    }
}
