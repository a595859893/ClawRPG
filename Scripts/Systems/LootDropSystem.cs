using Godot;
using System;
using System.Collections.Generic;

public class LootDropSystem
{
    private static LootDropSystem _instance;
    public static LootDropSystem Instance
    {
        get
        {
            if (_instance == null) _instance = new LootDropSystem();
            return _instance;
        }
    }

    private LootDropData.PlayerLootData _playerData = new LootDropData.PlayerLootData();
    
    // Lucky drop system - increases drop rate and quality
    private float _luckValue = 0f;
    private int _luckItems = 0;
    
    // Pity system - guarantees better drops after many failed attempts
    private Dictionary<string, int> _pityCounters = new Dictionary<string, int>();
    private Dictionary<string, LootDropData.LootRarity> _pityThresholds = new Dictionary<string, LootDropData.LootRarity>();
    
    // Critical drop system - rare chance for double drops
    private float _criticalDropRate = 0.05f;
    
    public float LuckValue => _luckValue;
    public int LuckItems => _luckItems;
    
    // Signals
    public Action<LootDropData.LootEntry, int> OnLootDropped;
    public Action<LootDropData.LootRarity> OnRarityDropped;
    public Action OnLuckyDrop;
    public Action OnCriticalDrop;

    public void Initialize()
    {
        LoadPlayerData();
        InitializePitySystem();
        GD.Print("[LootDropSystem] Initialized");
    }

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

    public void SavePlayerData()
    {
        if (SaveSystem.Instance != null)
        {
            SaveSystem.Instance.SaveLootDropData(_playerData);
        }
    }

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
    /// Roll for loot from a specific pool
    /// </summary>
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
    /// Add luck value (from luck-boosting items/effects)
    /// </summary>
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
    /// Clear all luck effects
    /// </summary>
    public void ResetLuck()
    {
        _luckValue = 0f;
    }

    /// <summary>
    /// Get drop rate multiplier based on luck
    /// </summary>
    public float GetDropRateMultiplier()
    {
        return 1.0f + _luckValue * 0.1f;
    }

    /// <summary>
    /// Get drop quality bonus based on luck
    /// </summary>
    public float GetQualityBonus()
    {
        return _luckValue * 0.2f;
    }

    /// <summary>
    /// Get player loot statistics
    /// </summary>
    public LootDropData.PlayerLootData GetStatistics()
    {
        return _playerData;
    }

    /// <summary>
    /// Get rarity distribution as percentages
    /// </summary>
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
    /// Check if a drop is guaranteed (for special events)
    /// </summary>
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
    /// Get the number of drops until next pity threshold
    /// </summary>
    public int GetDropsUntilPity(string poolId)
    {
        if (!_pityCounters.ContainsKey(poolId)) return -1;
        if (!_pityThresholds.ContainsKey(poolId)) return -1;
        
        var threshold = _pityThresholds[poolId];
        int targetCount = threshold == LootDropData.LootRarity.Rare ? 20 : 30;
        
        return Math.Max(0, targetCount - _pityCounters[poolId]);
    }
}
