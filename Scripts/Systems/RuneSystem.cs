using System;
using System.Collections.Generic;
using Godot;

/// <summary>
/// Rune data structure - individual rune definition
/// </summary>
public class RuneData
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public RuneType Type { get; set; }
    public RuneRarity Rarity { get; set; }
    public RuneSlotType SlotType { get; set; }
    
    // Attribute bonuses
    public int AttackBonus { get; set; }
    public int DefenseBonus { get; set; }
    public int HealthBonus { get; set; }
    public int SpeedBonus { get; set; }
    public float CritRateBonus { get; set; }
    public float CritDamageBonus { get; set; }
    public int LifeStealBonus { get; set; }
    public int DodgeBonus { get; set; }
    public int BlockBonus { get; set; }
    
    // Special effects
    public string SpecialEffect { get; set; }
    public float SpecialEffectValue { get; set; }
    
    // Requirements
    public int RequiredLevel { get; set; }
    
    public string IconPath { get; set; }
}

public enum RuneType
{
    Offensive,    // Attack-focused
    Defensive,    // Defense-focused  
    Utility,      // Utility/economic
    Special       // Special effects
}

public enum RuneRarity
{
    Common,
    Uncommon,
    Rare,
    Epic,
    Legendary
}

public enum RuneSlotType
{
    Helmet,
    Chestplate,
    Weapon,
    Shield,
    Boots,
    Ring,
    Amulet,
    Any
}

/// <summary>
/// Player's rune inventory and equipped runes
/// </summary>
public class PlayerRuneData
{
    public List<RuneData> OwnedRunes { get; set; } = new List<RuneData>();
    public Dictionary<RuneSlotType, RuneData> EquippedRunes { get; set; } = new Dictionary<RuneSlotType, RuneData>();
    public int TotalRuneSlots { get; set; } = 10; // Starting slots
    
    public PlayerRuneData()
    {
        // Initialize empty slots for each slot type
        foreach (RuneSlotType slot in Enum.GetValues(typeof(RuneSlotType)))
        {
            if (slot != RuneSlotType.Any)
            {
                EquippedRunes[slot] = null;
            }
        }
    }
}

/// <summary>
/// Rune System - Equipment rune enhancement system
/// </summary>
public class RuneSystem : BaseSystem
{
    private static RuneSystem _instance;
    public static RuneSystem Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = GetNode<RuneSystem>("/root/RuneSystem");
                if (_instance == null)
                {
                    var node = new RuneSystem();
                    node.Name = "RuneSystem";
                    Engine.GetMainLoop().Root.AddChild(node);
                }
            }
            return _instance;
        }
    }
    
    private PlayerRuneData _playerRuneData;
    private RuneDatabase _runeDatabase;
    
    // Signals
    public Action<RuneData> RuneEquipped;
    public Action<RuneData> RuneUnequipped;
    public Action<RuneData> RuneAcquired;
    public Action<string> RuneSlotUnlocked;
    
    protected override void Initialize()
    {
        base.Initialize();
        
        _runeDatabase = new RuneDatabase();
        _playerRuneData = new PlayerRuneData();
        
        // 注册到保存系统
        SaveSystem.Instance?.Register(this);
        
        GD.Print("[RuneSystem] Initialized");
    }
    
    /// <summary>
    /// Get all available runes
    /// </summary>
    public List<RuneData> GetAllRunes()
    {
        return _runeDatabase.GetAllRunes();
    }
    
    /// <summary>
    /// Get runes by type
    /// </summary>
    public List<RuneData> GetRunesByType(RuneType type)
    {
        return _runeDatabase.GetRunesByType(type);
    }
    
    /// <summary>
    /// Get runes by rarity
    /// </summary>
    public List<RuneData> GetRunesByRarity(RuneRarity rarity)
    {
        return _runeDatabase.GetRunesByRarity(rarity);
    }
    
    /// <summary>
    /// Get player's owned runes
    /// </summary>
    public List<RuneData> GetOwnedRunes()
    {
        return _playerRuneData.OwnedRunes;
    }
    
    /// <summary>
    /// Get equipped rune for a slot
    /// </summary>
    public RuneData GetEquippedRune(RuneSlotType slotType)
    {
        if (_playerRuneData.EquippedRunes.ContainsKey(slotType))
        {
            return _playerRuneData.EquippedRunes[slotType];
        }
        return null;
    }
    
    /// <summary>
    /// Get all equipped runes
    /// </summary>
    public Dictionary<RuneSlotType, RuneData> GetAllEquippedRunes()
    {
        return _playerRuneData.EquippedRunes;
    }
    
    /// <summary>
    /// Equip a rune to a slot
    /// </summary>
    public bool EquipRune(RuneData rune, RuneSlotType slotType)
    {
        if (rune == null) return false;
        
        // Check level requirement
        if (rune.RequiredLevel > GetPlayerLevel())
        {
            return false;
        }
        
        // Check slot compatibility
        if (rune.SlotType != RuneSlotType.Any && rune.SlotType != slotType)
        {
            return false;
        }
        
        // Check if player owns the rune
        if (!_playerRuneData.OwnedRunes.Contains(rune))
        {
            return false;
        }
        
        // Unequip current rune if any
        var currentRune = GetEquippedRune(slotType);
        if (currentRune != null)
        {
            _playerRuneData.EquippedRunes[slotType] = rune;
        }
        else
        {
            _playerRuneData.EquippedRunes[slotType] = rune;
        }
        
        RuneEquipped?.Invoke(rune);
        return true;
    }
    
    /// <summary>
    /// Unequip a rune from a slot
    /// </summary>
    public bool UnequipRune(RuneSlotType slotType)
    {
        var currentRune = GetEquippedRune(slotType);
        if (currentRune == null) return false;
        
        _playerRuneData.EquippedRunes[slotType] = null;
        RuneUnequipped?.Invoke(currentRune);
        return true;
    }
    
    /// <summary>
    /// Add a rune to player's inventory
    /// </summary>
    public void AddRune(RuneData rune)
    {
        if (rune == null) return;
        
        _playerRuneData.OwnedRunes.Add(rune);
        RuneAcquired?.Invoke(rune);
    }
    
    /// <summary>
    /// Remove a rune from player's inventory
    /// </summary>
    public bool RemoveRune(RuneData rune)
    {
        if (rune == null) return false;
        
        // Can't remove if equipped
        foreach (var kvp in _playerRuneData.EquippedRunes)
        {
            if (kvp.Value == rune)
            {
                return false;
            }
        }
        
        return _playerRuneData.OwnedRunes.Remove(rune);
    }
    
    /// <summary>
    /// Unlock additional rune slot
    /// </summary>
    public bool UnlockSlot(int cost = 1000)
    {
        var player = GetPlayer();
        if (player == null) return false;
        
        int playerGold = (int)player.Get("gold", 0);
        if (playerGold < cost) return false;
        
        player.Set("gold", playerGold - cost);
        _playerRuneData.TotalRuneSlots++;
        
        RuneSlotUnlocked?.Invoke($"Slot #{_playerRuneData.TotalRuneSlots}");
        return true;
    }
    
    /// <summary>
    /// Get total equipped rune slots
    /// </summary>
    public int GetTotalSlots()
    {
        return _playerRuneData.TotalRuneSlots;
    }
    
    /// <summary>
    /// Get used rune slots count
    /// </summary>
    public int GetUsedSlots()
    {
        int count = 0;
        foreach (var kvp in _playerRuneData.EquippedRunes)
        {
            if (kvp.Value != null) count++;
        }
        return count;
    }
    
    /// <summary>
    /// Calculate total bonuses from equipped runes
    /// </summary>
    public Dictionary<string, object> GetTotalBonuses()
    {
        var bonuses = new Dictionary<string, object>
        {
            ["attack"] = 0,
            ["defense"] = 0,
            ["health"] = 0,
            ["speed"] = 0,
            ["crit_rate"] = 0f,
            ["crit_damage"] = 0f,
            ["lifesteal"] = 0,
            ["dodge"] = 0,
            ["block"] = 0
        };
        
        foreach (var kvp in _playerRuneData.EquippedRunes)
        {
            var rune = kvp.Value;
            if (rune == null) continue;
            
            bonuses["attack"] = (int)bonuses["attack"] + rune.AttackBonus;
            bonuses["defense"] = (int)bonuses["defense"] + rune.DefenseBonus;
            bonuses["health"] = (int)bonuses["health"] + rune.HealthBonus;
            bonuses["speed"] = (int)bonuses["speed"] + rune.SpeedBonus;
            bonuses["crit_rate"] = (float)bonuses["crit_rate"] + rune.CritRateBonus;
            bonuses["crit_damage"] = (float)bonuses["crit_damage"] + rune.CritDamageBonus;
            bonuses["lifesteal"] = (int)bonuses["lifesteal"] + rune.LifeStealBonus;
            bonuses["dodge"] = (int)bonuses["dodge"] + rune.DodgeBonus;
            bonuses["block"] = (int)bonuses["block"] + rune.BlockBonus;
        }
        
        return bonuses;
    }
    
    /// <summary>
    /// Get player level (placeholder - should connect to actual player data)
    /// </summary>
    private int GetPlayerLevel()
    {
        // This should be connected to actual player level
        return 1; // Default to level 1
    }
    
    /// <summary>
    /// Get player reference (placeholder)
    /// </summary>
    private Node GetPlayer()
    {
        var tree = Engine.GetMainLoop();
        if (tree is SceneTree sceneTree) {
            var nodes = sceneTree.GetNodesInGroup("player");
            if (nodes.Count > 0) return nodes[0];
        }
        return null;
    }
    
    /// <summary>
    /// Get rarity color
    /// </summary>
    public static string GetRarityColor(RuneRarity rarity)
    {
        return rarity switch
        {
            RuneRarity.Common => "#9E9E9E",      // Gray
            RuneRarity.Uncommon => "#4CAF50",    // Green
            RuneRarity.Rare => "#2196F3",        // Blue
            RuneRarity.Epic => "#9C27B0",        // Purple
            RuneRarity.Legendary => "#FF9800",   // Orange
            _ => "#FFFFFF"
        };
    }
    
    /// <summary>
    /// Get rarity display name
    /// </summary>
    public static string GetRarityName(RuneRarity rarity)
    {
        return rarity switch
        {
            RuneRarity.Common => "普通",
            RuneRarity.Uncommon => "优秀",
            RuneRarity.Rare => "稀有",
            RuneRarity.Epic => "史诗",
            RuneRarity.Legendary => "传说",
            _ => "未知"
        };
    }
    
    /// <summary>
    /// Get type display name
    /// </summary>
    public static string GetTypeName(RuneType type)
    {
        return type switch
        {
            RuneType.Offensive => "攻击",
            RuneType.Defensive => "防御",
            RuneType.Utility => "工具",
            RuneType.Special => "特殊",
            _ => "未知"
        };
    }
    
    /// <summary>
    /// Get slot type display name
    /// </summary>
    public static string GetSlotName(RuneSlotType slotType)
    {
        return slotType switch
        {
            RuneSlotType.Helmet => "头盔",
            RuneSlotType.Chestplate => "胸甲",
            RuneSlotType.Weapon => "武器",
            RuneSlotType.Shield => "盾牌",
            RuneSlotType.Boots => "靴子",
            RuneSlotType.Ring => "戒指",
            RuneSlotType.Amulet => "护符",
            RuneSlotType.Any => "通用",
            _ => "未知"
        };
    }
    
    /// <summary>
    /// 导出保存数据
    /// </summary>
    public override Dictionary ExportSaveData()
    {
        var saveData = new Dictionary();
        
        var ownedRunes = new List<Dictionary>();
        foreach (var rune in _playerRuneData.OwnedRunes)
        {
            ownedRunes.Add(new Dictionary { ["id"] = rune.Id });
        }
        saveData["owned_runes"] = ownedRunes;
        
        var equippedRunes = new Dictionary<string, string>();
        foreach (var kvp in _playerRuneData.EquippedRunes)
        {
            equippedRunes[kvp.Key.ToString()] = kvp.Value?.Id ?? "";
        }
        saveData["equipped_runes"] = equippedRunes;
        
        saveData["total_rune_slots"] = _playerRuneData.TotalRuneSlots;
        
        return saveData;
    }
    
    /// <summary>
    /// 导入保存数据
    /// </summary>
    public override void ImportSaveData(Dictionary data)
    {
        if (data == null) return;
        
        if (data.Contains("owned_runes"))
        {
            var runesList = data["owned_runes"] as List<object>;
            if (runesList != null)
            {
                _playerRuneData.OwnedRunes.Clear();
                foreach (var runeObj in runesList)
                {
                    var runeDict = runeObj as Dictionary;
                    if (runeDict != null)
                    {
                        var rune = _runeDatabase.GetRuneById(runeDict["id"] as string);
                        if (rune != null)
                        {
                            _playerRuneData.OwnedRunes.Add(rune);
                        }
                    }
                }
            }
        }
        
        if (data.Contains("equipped_runes"))
        {
            var equippedDict = data["equipped_runes"] as Dictionary;
            if (equippedDict != null)
            {
                foreach (var kvp in equippedDict)
                {
                    if (Enum.TryParse<RuneSlotType>(kvp.Key.ToString(), out var slotType))
                    {
                        var runeId = kvp.Value as string;
                        if (!string.IsNullOrEmpty(runeId))
                        {
                            var rune = _runeDatabase.GetRuneById(runeId);
                            _playerRuneData.EquippedRunes[slotType] = rune;
                        }
                    }
                }
            }
        }
        
        if (data.Contains("total_rune_slots"))
        {
            _playerRuneData.TotalRuneSlots = (int)data["total_rune_slots"];
        }
        
        GD.Print("[RuneSystem] Data loaded");
    }
    
    /// <summary>
    /// 获取系统ID
    /// </summary>
    public override string GetId()
    {
        return "RuneSystem";
    }
}
