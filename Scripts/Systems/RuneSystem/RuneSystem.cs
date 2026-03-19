using Godot;
using System.Collections.Generic;
using ClawRPG.Scripts.Database;

public class RuneSystem : BaseSystem
{
    private static RuneSystem _instance;
    public static RuneSystem Instance
    {
        get
        {
            if (_instance == null) _instance = new RuneSystem();
            return _instance;
        }
    }
    
    private RuneData _data;
    private RuneDatabase _db;
    
    public RuneData Data
    {
        get => _data;
        set => _data = value;
    }
    
    public RuneSystem()
    {
        _db = RuneDatabase.Instance;
        _data = new RuneData();
    }
    
    protected override void Initialize()
    {
        base.Initialize();
        if (_data == null)
        {
            _data = new RuneData();
        }
    }
    
    // Unlock a rune
    public bool UnlockRune(string runeId)
    {
        var rune = _db.GetRune(runeId);
        if (rune == null) return false;
        
        if (_data.UnlockedRunes.ContainsKey(runeId)) return false;
        
        _data.UnlockedRunes[runeId] = 1;
        _data.Statistics.TotalRunesUnlocked++;
        
        // Add history
        _data.RuneHistory.Add(new RuneHistoryEntry
        {
            RuneId = runeId,
            Action = "unlock",
            Level = 1,
            Timestamp = OS.GetUnixTime()
        });
        
        return true;
    }
    
    // Check if rune is unlocked
    public bool IsRuneUnlocked(string runeId)
    {
        return _data.UnlockedRunes.ContainsKey(runeId);
    }
    
    // Get rune level
    public int GetRuneLevel(string runeId)
    {
        return _data.UnlockedRunes.ContainsKey(runeId) ? _data.UnlockedRunes[runeId] : 0;
    }
    
    // Enhance rune
    public bool EnhanceRune(string runeId)
    {
        var rune = _db.GetRune(runeId);
        if (rune == null) return false;
        
        if (!_data.UnlockedRunes.ContainsKey(runeId)) return false;
        
        int currentLevel = _data.UnlockedRunes[runeId];
        if (currentLevel >= 5) return false; // Max level is 5
        
        int cost = rune.EnhanceCost * currentLevel;
        
        _data.UnlockedRunes[runeId] = currentLevel + 1;
        _data.Statistics.TimesEnhanced++;
        _data.Statistics.TotalGoldSpent += cost;
        
        // Add history
        _data.RuneHistory.Add(new RuneHistoryEntry
        {
            RuneId = runeId,
            Action = "enhance",
            Level = currentLevel + 1,
            Timestamp = OS.GetUnixTime()
        });
        
        return true;
    }
    
    // Equip rune to slot
    public bool EquipRune(string slot, string runeId)
    {
        if (!_data.UnlockedRunes.ContainsKey(runeId)) return false;
        
        var rune = _db.GetRune(runeId);
        if (rune == null) return false;
        
        // Check slot matches
        if (!System.Enum.TryParse<RuneDatabase.RuneSlot>(slot, true, out var runeSlot)) return false;
        if (rune.Slot != runeSlot) return false;
        
        _data.EquippedRunes[slot] = runeId;
        _data.Statistics.TotalRunesEquipped++;
        
        // Add history
        _data.RuneHistory.Add(new RuneHistoryEntry
        {
            RuneId = runeId,
            Action = "equip",
            Level = _data.UnlockedRunes[runeId],
            Timestamp = OS.GetUnixTime()
        });
        
        return true;
    }
    
    // Unequip rune from slot
    public bool UnequipRune(string slot)
    {
        if (!_data.EquippedRunes.ContainsKey(slot)) return false;
        
        string runeId = _data.EquippedRunes[slot];
        _data.EquippedRunes.Remove(slot);
        
        // Add history
        _data.RuneHistory.Add(new RuneHistoryEntry
        {
            RuneId = runeId,
            Action = "unequip",
            Level = GetRuneLevel(runeId),
            Timestamp = OS.GetUnixTime()
        });
        
        return true;
    }
    
    // Get equipped rune for slot
    public string GetEquippedRune(string slot)
    {
        return _data.EquippedRunes.ContainsKey(slot) ? _data.EquippedRunes[slot] : null;
    }
    
    // Calculate total attributes from equipped runes
    public Dictionary<string, float> GetTotalAttributes()
    {
        Dictionary<string, float> total = new Dictionary<string, float>();
        
        foreach (var equipped in _data.EquippedRunes)
        {
            string runeId = equipped.Value;
            var rune = _db.GetRune(runeId);
            if (rune == null) continue;
            
            int level = GetRuneLevel(runeId);
            float levelMultiplier = 1f + (level - 1) * 0.2f;
            
            foreach (var attr in rune.Attributes)
            {
                float value = attr.Value * levelMultiplier;
                if (total.ContainsKey(attr.Key))
                    total[attr.Key] += value;
                else
                    total[attr.Key] = value;
            }
        }
        
        return total;
    }
    
    // Get statistics
    public RuneStatistics GetStatistics()
    {
        return _data.Statistics;
    }
    
    // Get all unlocked runes
    public Dictionary<string, int> GetUnlockedRunes()
    {
        return _data.UnlockedRunes;
    }
    
    // Get equipped runes
    public Dictionary<string, string> GetEquippedRunes()
    {
        return _data.EquippedRunes;
    }
    
    // Get history
    public List<RuneHistoryEntry> GetHistory()
    {
        return _data.RuneHistory;
    }
    
    // Save data
    public Dictionary<string, object> Save()
    {
        Dictionary<string, object> data = new Dictionary<string, object>();
        
        var unlockedRunes = new Dictionary<string, int>();
        foreach (var kvp in _data.UnlockedRunes)
        {
            unlockedRunes[kvp.Key] = kvp.Value;
        }
        data["unlocked_runes"] = unlockedRunes;
        
        var equippedRunes = new Dictionary<string, string>();
        foreach (var kvp in _data.EquippedRunes)
        {
            equippedRunes[kvp.Key] = kvp.Value;
        }
        data["equipped_runes"] = equippedRunes;
        
        data["statistics"] = new Dictionary<string, object>
        {
            { "total_runes_unlocked", _data.Statistics.TotalRunesUnlocked },
            { "total_runes_equipped", _data.Statistics.TotalRunesEquipped },
            { "total_gold_spent", _data.Statistics.TotalGoldSpent },
            { "total_exp_gained", _data.Statistics.TotalExpGained },
            { "times_enhanced", _data.Statistics.TimesEnhanced },
            { "times_removed", _data.Statistics.TimesRemoved }
        };
        
        return data;
    }
    
    // Load data
    public void Load(Dictionary<string, object> data)
    {
        if (data == null) return;
        
        if (data.ContainsKey("unlocked_runes"))
        {
            var unlocked = (Godot.Collections.Dictionary)data["unlocked_runes"];
            _data.UnlockedRunes = new Dictionary<string, int>();
            foreach (var key in unlocked.Keys)
            {
                _data.UnlockedRunes[key.ToString()] = (int)unlocked[key];
            }
        }
        
        if (data.ContainsKey("equipped_runes"))
        {
            var equipped = (Godot.Collections.Dictionary)data["equipped_runes"];
            _data.EquippedRunes = new Dictionary<string, string>();
            foreach (var key in equipped.Keys)
            {
                _data.EquippedRunes[key.ToString()] = equipped[key].ToString();
            }
        }
        
        if (data.ContainsKey("statistics"))
        {
            var stats = (Godot.Collections.Dictionary)data["statistics"];
            _data.Statistics = new RuneStatistics
            {
                TotalRunesUnlocked = stats.Contains("total_runes_unlocked") ? (int)stats["total_runes_unlocked"] : 0,
                TotalRunesEquipped = stats.Contains("total_runes_equipped") ? (int)stats["total_runes_equipped"] : 0,
                TotalGoldSpent = stats.Contains("total_gold_spent") ? (int)stats["total_gold_spent"] : 0,
                TotalExpGained = stats.Contains("total_exp_gained") ? (int)stats["total_exp_gained"] : 0,
                TimesEnhanced = stats.Contains("times_enhanced") ? (int)stats["times_enhanced"] : 0,
                TimesRemoved = stats.Contains("times_removed") ? (int)stats["times_removed"] : 0
            };
        }
    }
    
    /// <summary>
    /// 导出保存数据 - 实现 BaseSystem 接口
    /// </summary>
    public override Dictionary ExportSaveData()
    {
        var data = new Dictionary();
        
        // 保存已解锁的符文及其等级
        var unlockedRunes = new Dictionary<string, int>();
        foreach (var kvp in _data.UnlockedRunes)
        {
            unlockedRunes[kvp.Key] = kvp.Value;
        }
        data["unlocked_runes"] = unlockedRunes;
        
        // 保存已装备的符文
        var equippedRunes = new Dictionary<string, string>();
        foreach (var kvp in _data.EquippedRunes)
        {
            equippedRunes[kvp.Key] = kvp.Value;
        }
        data["equipped_runes"] = equippedRunes;
        
        // 保存统计数据
        data["statistics"] = new Dictionary
        {
            { "total_runes_unlocked", _data.Statistics.TotalRunesUnlocked },
            { "total_runes_equipped", _data.Statistics.TotalRunesEquipped },
            { "total_gold_spent", _data.Statistics.TotalGoldSpent },
            { "total_exp_gained", _data.Statistics.TotalExpGained },
            { "times_enhanced", _data.Statistics.TimesEnhanced },
            { "times_removed", _data.Statistics.TimesRemoved }
        };
        
        return data;
    }
    
    /// <summary>
    /// 导入保存数据 - 实现 BaseSystem 接口
    /// </summary>
    public override void ImportSaveData(Dictionary data)
    {
        if (data == null) return;
        
        if (data.Contains("unlocked_runes"))
        {
            var unlocked = (Godot.Collections.Dictionary)data["unlocked_runes"];
            _data.UnlockedRunes = new Dictionary<string, int>();
            foreach (var key in unlocked.Keys)
            {
                _data.UnlockedRunes[key.ToString()] = (int)unlocked[key];
            }
        }
        
        if (data.Contains("equipped_runes"))
        {
            var equipped = (Godot.Collections.Dictionary)data["equipped_runes"];
            _data.EquippedRunes = new Dictionary<string, string>();
            foreach (var key in equipped.Keys)
            {
                _data.EquippedRunes[key.ToString()] = equipped[key].ToString();
            }
        }
        
        if (data.Contains("statistics"))
        {
            var stats = (Godot.Collections.Dictionary)data["statistics"];
            _data.Statistics = new RuneStatistics
            {
                TotalRunesUnlocked = stats.Contains("total_runes_unlocked") ? (int)stats["total_runes_unlocked"] : 0,
                TotalRunesEquipped = stats.Contains("total_runes_equipped") ? (int)stats["total_runes_equipped"] : 0,
                TotalGoldSpent = stats.Contains("total_gold_spent") ? (int)stats["total_gold_spent"] : 0,
                TotalExpGained = stats.Contains("total_exp_gained") ? (int)stats["total_exp_gained"] : 0,
                TimesEnhanced = stats.Contains("times_enhanced") ? (int)stats["times_enhanced"] : 0,
                TimesRemoved = stats.Contains("times_removed") ? (int)stats["times_removed"] : 0
            };
        }
    }
}
