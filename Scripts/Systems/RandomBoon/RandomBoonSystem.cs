using Godot;
using System;
using System.Collections.Generic;

public class RandomBoonSystem : BaseSystem
{
    public static RandomBoonSystem Instance { get; private set; }
    
    // Signals
    public Signal BoonOffered { get; }  // (List<BoonData> offeredBoons)
    public Signal BoonSelected { get; }  // (BoonData selectedBoon)
    public Signal BoonActivated { get; }  // (BoonData activatedBoon)
    public Signal BoonRemoved { get; }  // (string boonId)
    
    // Player data
    public PlayerBoonData PlayerData { get; private set; }
    
    // Current offer
    private List<BoonData> _currentOffer;
    private bool _isOffering;
    
    // Boon bonuses cache
    private Dictionary<string, int> _activeBonusCache;
    
    public override void _Ready()
    {
        Instance = this;
        PlayerData = new PlayerBoonData();
        _activeBonusCache = new Dictionary<string, int>();
        LoadData();
    }
    
    public void LoadData()
    {
        var saveData = SaveSystem.LoadProgress();
        if (saveData != null && saveData.ContainsKey("random_boon"))
        {
            var data = saveData["random_boon"] as Dictionary<string, object>;
            if (data != null)
            {
                var ownedBoons = data.GetValueOrDefault("owned_boons", null) as List<object>;
                var activeBoons = data.GetValueOrDefault("active_boons", null) as List<object>;
                
                if (ownedBoons != null)
                {
                    PlayerData.OwnedBoons = new List<string>();
                    foreach (var b in ownedBoons)
                        PlayerData.OwnedBoons.Add(b.ToString());
                }
                
                if (activeBoons != null)
                {
                    PlayerData.ActiveBoons = new List<string>();
                    foreach (var b in activeBoons)
                        PlayerData.ActiveBoons.Add(b.ToString());
                }
                
                PlayerData.TotalBoonsGained = (int)(data.GetValueOrDefault("total_boons", 0));
                PlayerData.TotalGoldEarned = (int)(data.GetValueOrDefault("total_gold", 0));
                PlayerData.TotalExpEarned = (int)(data.GetValueOrDefault("total_exp", 0));
            }
        }
        
        UpdateBonusCache();
    }
    
    public void SaveData()
    {
        var saveData = SaveSystem.LoadProgress() ?? new Dictionary<string, object>();
        
        saveData["random_boon"] = new Dictionary<string, object>
        {
            { "owned_boons", PlayerData.OwnedBoons },
            { "active_boons", PlayerData.ActiveBoons },
            { "total_boons", PlayerData.TotalBoonsGained },
            { "total_gold", PlayerData.TotalGoldEarned },
            { "total_exp", PlayerData.TotalExpEarned }
        };
        
        SaveSystem.SaveProgress(saveData);
    }
    
    // Offer random boons for player to choose
    public void OfferRandomBoons(int count = 3, BoonRarity? rarity = null)
    {
        _currentOffer = BoonDatabase.Instance.GetRandomBoonPool(count, rarity);
        _isOffering = true;
        BoonOffered.Emit(_currentOffer);
    }
    
    // Select a boon from current offer
    public bool SelectBoon(int index)
    {
        if (!_isOffering || _currentOffer == null || index < 0 || index >= _currentOffer.Count)
            return false;
        
        var selected = _currentOffer[index];
        return AddBoon(selected);
    }
    
    // Add a boon to player's collection
    public bool AddBoon(BoonData boon)
    {
        if (boon == null || PlayerData.OwnedBoons.Contains(boon.Id))
            return false;
        
        PlayerData.OwnedBoons.Add(boon.Id);
        PlayerData.TotalBoonsGained++;
        
        // Auto-activate if less than max active
        if (PlayerData.ActiveBoons.Count < GetMaxActiveBoons())
        {
            ActivateBoon(boon.Id);
        }
        
        SaveData();
        return true;
    }
    
    // Activate a boon
    public bool ActivateBoon(string boonId)
    {
        if (!PlayerData.OwnedBoons.Contains(boonId))
            return false;
        
        if (PlayerData.ActiveBoons.Count >= GetMaxActiveBoons())
            return false;
        
        if (PlayerData.ActiveBoons.Contains(boonId))
            return false;
        
        PlayerData.ActiveBoons.Add(boonId);
        UpdateBonusCache();
        
        var boon = BoonDatabase.Instance.GetBoon(boonId);
        if (boon != null)
            BoonActivated.Emit(boon);
        
        // Update player bonuses
        ApplyBonusesToPlayer();
        
        SaveData();
        return true;
    }
    
    // Deactivate a boon
    public bool DeactivateBoon(string boonId)
    {
        if (!PlayerData.ActiveBoons.Contains(boonId))
            return false;
        
        PlayerData.ActiveBoons.Remove(boonId);
        UpdateBonusCache();
        
        BoonRemoved.Emit(boonId);
        
        // Update player bonuses
        ApplyBonusesToPlayer();
        
        SaveData();
        return true;
    }
    
    // Apply bonuses to player
    private void ApplyBonusesToPlayer()
    {
        var player = GetTree().CurrentScene.GetNodeOrNull<Player>("%Player");
        if (player != null)
        {
            player.ApplyBoonBonuses();
        }
    }
    
    // Swap active boons
    public bool SwapBoon(string removeId, string addId)
    {
        if (!PlayerData.ActiveBoons.Contains(removeId))
            return false;
        
        if (!PlayerData.OwnedBoons.Contains(addId))
            return false;
        
        if (PlayerData.ActiveBoons.Contains(addId))
            return false;
        
        PlayerData.ActiveBoons.Remove(removeId);
        PlayerData.ActiveBoons.Add(addId);
        
        UpdateBonusCache();
        SaveData();
        return true;
    }
    
    // Get max active boons based on player level
    private int GetMaxActiveBoons()
    {
        var player = GetTree().CurrentScene.GetNode<Player>("%Player");
        if (player == null) return 3;
        
        int level = player.Level;
        if (level >= 50) return 8;
        if (level >= 40) return 7;
        if (level >= 30) return 6;
        if (level >= 20) return 5;
        if (level >= 10) return 4;
        return 3;
    }
    
    // Update bonus cache
    private void UpdateBonusCache()
    {
        _activeBonusCache.Clear();
        
        foreach (var boonId in PlayerData.ActiveBoons)
        {
            var boon = BoonDatabase.Instance.GetBoon(boonId);
            if (boon == null) continue;
            
            AddToCache("attack", boon.AttackBonus);
            AddToCache("defense", boon.DefenseBonus);
            AddToCache("health", boon.HealthBonus);
            AddToCache("magic", boon.MagicBonus);
            AddToCache("speed", boon.SpeedBonus);
            AddToCache("crit_rate", (int)(boon.CritRateBonus * 100));
            AddToCache("crit_damage", (int)(boon.CritDamageBonus * 100));
            AddToCache("lifesteal", (int)(boon.LifestealBonus * 100));
            AddToCache("dodge", (int)(boon.DodgeBonus * 100));
            AddToCache("gold_mult", boon.GoldMultiplier);
            AddToCache("exp_mult", boon.ExpMultiplier);
        }
    }
    
    private void AddToCache(string key, int value)
    {
        if (_activeBonusCache.ContainsKey(key))
            _activeBonusCache[key] += value;
        else
            _activeBonusCache[key] = value;
    }
    
    // Get active bonus values
    public int GetAttackBonus() => _activeBonusCache.GetValueOrDefault("attack", 0);
    public int GetDefenseBonus() => _activeBonusCache.GetValueOrDefault("defense", 0);
    public int GetHealthBonus() => _activeBonusCache.GetValueOrDefault("health", 0);
    public int GetMagicBonus() => _activeBonusCache.GetValueOrDefault("magic", 0);
    public int GetSpeedBonus() => _activeBonusCache.GetValueOrDefault("speed", 0);
    public float GetCritRateBonus() => _activeBonusCache.GetValueOrDefault("crit_rate", 0) / 100f;
    public float GetCritDamageBonus() => _activeBonusCache.GetValueOrDefault("crit_damage", 0) / 100f;
    public float GetLifestealBonus() => _activeBonusCache.GetValueOrDefault("lifesteal", 0) / 100f;
    public float GetDodgeBonus() => _activeBonusCache.GetValueOrDefault("dodge", 0) / 100f;
    public int GetGoldMultiplier() => _activeBonusCache.GetValueOrDefault("gold_mult", 0);
    public int GetExpMultiplier() => _activeBonusCache.GetValueOrDefault("exp_mult", 0);
    
    // Get current offer
    public List<BoonData> GetCurrentOffer() => _currentOffer;
    public bool IsOffering() => _isOffering;
    
    // Cancel current offer
    public void CancelOffer()
    {
        _currentOffer = null;
        _isOffering = false; 
    }
    
    // Get all owned boons
    public List<BoonData> GetOwnedBoons()
    {
        var boons = new List<BoonData>();
        foreach (var id in PlayerData.OwnedBoons)
        {
            var boon = BoonDatabase.Instance.GetBoon(id);
            if (boon != null)
                boons.Add(boon);
        }
        return boons;
    }
    
    // Get active boons
    public List<BoonData> GetActiveBoons()
    {
        var boons = new List<BoonData>();
        foreach (var id in PlayerData.ActiveBoons)
        {
            var boon = BoonDatabase.Instance.GetBoon(id);
            if (boon != null)
                boons.Add(boon);
        }
        return boons;
    }
    
    // Check if has specific boon type active
    public bool HasActiveBoonOfType(BoonType type)
    {
        foreach (var id in PlayerData.ActiveBoons)
        {
            var boon = BoonDatabase.Instance.GetBoon(id);
            if (boon != null && boon.Type == type)
                return true;
        }
        return false;
    }
    
    // Get statistics
    public Dictionary<string, int> GetStatistics()
    {
        return new Dictionary<string, int>
        {
            { "total_boons", PlayerData.TotalBoonsGained },
            { "owned_count", PlayerData.OwnedBoons.Count },
            { "active_count", PlayerData.ActiveBoons.Count },
            { "max_active", GetMaxActiveBoons() },
            { "total_gold", PlayerData.TotalGoldEarned },
            { "total_exp", PlayerData.TotalExpEarned }
        };
    }
    
    // Apply gold/exp multipliers
    public int ApplyGoldMultiplier(int baseGold)
    {
        int multiplier = GetGoldMultiplier();
        if (multiplier <= 0) return baseGold;
        return (int)(baseGold * (1 + multiplier / 100f));
    }
    
    public int ApplyExpMultiplier(int baseExp)
    {
        int multiplier = GetExpMultiplier();
        if (multiplier <= 0) return baseExp;
        return (int)(baseExp * (1 + multiplier / 100f));
    }
    
    // Reset all boons (for new game)
    public void ResetBoons()
    {
        PlayerData = new PlayerBoonData();
        _activeBonusCache.Clear();
        SaveData();
    }
    
    /// <summary>
    /// 导出保存数据
    /// </summary>
    public override Dictionary ExportSaveData()
    {
        var data = new Dictionary();
        
        if (PlayerData != null)
        {
            data["active_boons"] = new Array(PlayerData.ActiveBoons);
            data["boon_history"] = new Array(PlayerData.BoonHistory);
            data["reroll_count"] = PlayerData.RerollCount;
            data["total_boons_activated"] = PlayerData.TotalBoonsActivated;
        }
        
        return data;
    }
    
    /// <summary>
    /// 导入保存数据
    /// </summary>
    public override void ImportSaveData(Dictionary data)
    {
        if (data == null || PlayerData == null) return;
        
        if (data.Contains("active_boons"))
        {
            var boonsArray = (Array)data["active_boons"];
            PlayerData.ActiveBoons = new List<string>();
            foreach (string boon in boonsArray)
            {
                PlayerData.ActiveBoons.Add(boon);
            }
        }
        
        if (data.Contains("boon_history"))
        {
            var historyArray = (Array)data["boon_history"];
            PlayerData.BoonHistory = new List<string>();
            foreach (string boon in historyArray)
            {
                PlayerData.BoonHistory.Add(boon);
            }
        }
        
        PlayerData.RerollCount = (int)data.GetValueOrDefault("reroll_count", 0);
        PlayerData.TotalBoonsActivated = (int)data.GetValueOrDefault("total_boons_activated", 0);
    }
}
