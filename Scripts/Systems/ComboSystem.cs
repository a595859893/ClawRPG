using Godot;
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using Framework;

// ========== JSON 配置数据结构 ==========
public class ComboConfigEntry
{
    [JsonPropertyName("comboId")] public string ComboId { get; set; }
    [JsonPropertyName("comboName")] public string ComboName { get; set; }
    [JsonPropertyName("description")] public string Description { get; set; }
    [JsonPropertyName("skillSequence")] public List<string> SkillSequence { get; set; }
    [JsonPropertyName("damageMultiplier")] public float DamageMultiplier { get; set; }
    [JsonPropertyName("cooldownReduction")] public float CooldownReduction { get; set; }
    [JsonPropertyName("comboPointReward")] public int ComboPointReward { get; set; }
    [JsonPropertyName("effectName")] public string EffectName { get; set; }
    [JsonPropertyName("requiredComboLevel")] public int RequiredComboLevel { get; set; }
    [JsonPropertyName("comboType")] public string ComboType { get; set; }
    [JsonPropertyName("comboRarity")] public string ComboRarity { get; set; }
}

public class ComboConfigFile
{
    [JsonPropertyName("version")] public string Version { get; set; }
    [JsonPropertyName("combos")] public List<ComboConfigEntry> Combos { get; set; }
}

/// <summary>
/// 连击数据资源 - 定义一个连击的完整配置
/// </summary>
public class ComboData : Resource
{
    /// <summary>
    /// 连击ID
    /// </summary>
    [Export] public string comboId;
    /// <summary>
    /// 连击名称
    /// </summary>
    [Export] public string comboName;
    /// <summary>
    /// 连击描述
    /// </summary>
    [Export] public string description;
    /// <summary>
    /// 技能序列 - 按顺序需要使用的技能ID列表
    /// </summary>
    [Export] public List<string> skillSequence = new List<string>();
    /// <summary>
    /// 伤害倍率
    /// </summary>
    [Export] public float damageMultiplier = 1.5f;
    /// <summary>
    /// 冷却缩减百分比
    /// </summary>
    [Export] public float cooldownReduction = 0.2f;
    /// <summary>
    /// 连击点奖励
    /// </summary>
    [Export] public int comboPointReward = 10;
    /// <summary>
    /// 特效名称
    /// </summary>
    [Export] public string effectName;
    /// <summary>
    /// 需要的连击等级
    /// </summary>
    [Export] public int requiredComboLevel = 1;
    /// <summary>
    /// 连击类型
    /// </summary>
    [Export] public ComboType comboType;
    /// <summary>
    /// 稀有度
    /// </summary>
    [Export] public Rarity comboRarity;
    
    /// <summary>
    /// 连击类型枚举
    /// </summary>
    public enum ComboType
    {
        Offensive,
        Defensive,
        Support,
        Utility,
        Special
    }
    
    /// <summary>
    /// 稀有度枚举
    /// </summary>
    public enum Rarity
    {
        Common,
        Uncommon,
        Rare,
        Epic,
        Legendary
    }
}

/// <summary>
/// 连击进度 - 追踪玩家当前连击的执行状态
/// </summary>
public class ComboProgress
{
    /// <summary>
    /// 连击ID
    /// </summary>
    public string comboId;
    /// <summary>
    /// 当前完成的步骤数
    /// </summary>
    public int currentStep = 0;
    /// <summary>
    /// 剩余时间（秒）
    /// </summary>
    public float timeRemaining = 0f;
    /// <summary>
    /// 连击是否激活
    /// </summary>
    public bool isActive = false;
    /// <summary>
    /// 已执行次数
    /// </summary>
    public int timesExecuted = 0;
}

/// <summary>
/// 连击系统 - 管理玩家连击技能的系统
/// 玩家按顺序使用特定技能可以触发强力的连击效果
/// </summary>
public class ComboSystem : BaseSystem
{
    /// <summary>
    /// 单例实例
    /// </summary>
    public static ComboSystem Instance { get; private set; }
    
    // Combo database
    private Dictionary<string, ComboData> _combos = new Dictionary<string, ComboData>();
    
    // Combo discovery — tracks which combos have been discovered by the player
    private HashSet<string> _discoveredComboIds = new HashSet<string>();
    
    // Player combo progress
    private Dictionary<string, ComboProgress> _playerCombos = new Dictionary<string, ComboProgress>();
    /// <summary>
    /// 当前连击点数
    /// </summary>
    private int _comboPoints = 0;
    /// <summary>
    /// 当前连击等级
    /// </summary>
    private int _comboLevel = 1;
    
    // Timing
    /// <summary>
    /// 完成连击的时间窗口（秒）
    /// </summary>
    private float _comboWindow = 3.0f;
    private float _deltaTime;
    
    // Signals
    /// <summary>
    /// 连击执行时触发
    /// </summary>
    public static Action<string, float, string> ComboExecuted;
    /// <summary>
    /// 连击进度更新时触发
    /// </summary>
    public static Action<string, int, float> ComboProgressUpdated;
    /// <summary>
    /// 连击点数变化时触发
    /// </summary>
    public static Action<int> ComboPointsChanged;
    /// <summary>
    /// 连击等级变化时触发
    /// </summary>
    public static Action<int> ComboLevelChanged;
    /// <summary>
    /// 发现新连击时触发
    /// </summary>
    public static Action<ComboData> NewComboDiscovered;
    
    public override void _Ready()
    {
        Instance = this;
        _InitializeComboDatabase();
    }
    
    public override void _Process(float delta)
    {
        _deltaTime = delta;
        _UpdateComboTimers(delta);
    }
    
    private void _InitializeComboDatabase()
    {
        // Try loading from JSON config first (data-driven)
        if (_LoadCombosFromJson())
        {
            GD.Print($"[ComboSystem] Loaded {_combos.Count} combos from JSON config");
        }
        else
        {
            GD.Print("[ComboSystem] JSON config not found or invalid, falling back to hardcoded combos");
            _LoadHardcodedCombos();
        }
        
        // Initialize progress for each combo
        foreach (var comboId in _combos.Keys)
        {
            _playerCombos[comboId] = new ComboProgress { comboId = comboId };
        }
        
        GD.Print($"[ComboSystem] Initialized {_combos.Count} combos");
    }
    
    private bool _LoadCombosFromJson()
    {
        string configPath = "res://Resources/Config/combos_config.json";
        if (!FileAccess.FileExists(configPath))
            return false;
        
        try
        {
            using var file = FileAccess.Open(configPath, FileAccess.ModeFlags.Read);
            if (file == null) return false;
            
            string json = file.GetAsText();
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var configFile = JsonSerializer.Deserialize<ComboConfigFile>(json, options);
            
            if (configFile?.Combos == null) return false;
            
            foreach (var entry in configFile.Combos)
            {
                var combo = new ComboData
                {
                    comboId = entry.ComboId,
                    comboName = entry.ComboName,
                    description = entry.Description,
                    skillSequence = entry.SkillSequence ?? new List<string>(),
                    damageMultiplier = entry.DamageMultiplier,
                    cooldownReduction = entry.CooldownReduction,
                    comboPointReward = entry.ComboPointReward,
                    effectName = entry.EffectName,
                    requiredComboLevel = entry.RequiredComboLevel,
                    comboType = _ParseComboType(entry.ComboType),
                    comboRarity = _ParseRarity(entry.ComboRarity)
                };
                _combos[combo.comboId] = combo;
            }
            return true;
        }
        catch (Exception ex)
        {
            GD.PrintErr($"[ComboSystem] Failed to load combos from JSON: {ex.Message}");
            return false;
        }
    }
    
    private ComboData.ComboType _ParseComboType(string type)
    {
        return type?.ToLowerInvariant() switch
        {
            "offensive" => ComboData.ComboType.Offensive,
            "defensive" => ComboData.ComboType.Defensive,
            "support" => ComboData.ComboType.Support,
            "utility" => ComboData.ComboType.Utility,
            "special" => ComboData.ComboType.Special,
            _ => ComboData.ComboType.Offensive
        };
    }
    
    private ComboData.Rarity _ParseRarity(string rarity)
    {
        return rarity?.ToLowerInvariant() switch
        {
            "common" => ComboData.Rarity.Common,
            "uncommon" => ComboData.Rarity.Uncommon,
            "rare" => ComboData.Rarity.Rare,
            "epic" => ComboData.Rarity.Epic,
            "legendary" => ComboData.Rarity.Legendary,
            _ => ComboData.Rarity.Common
        };
    }
    
    private void _LoadHardcodedCombos()
    {
        // Offensive Combos
        _RegisterCombo(new ComboData
        {
            comboId = "combo_double_strike",
            comboName = "Double Strike",
            description = "Strike twice in quick succession",
            skillSequence = new List<string> { "basic_attack", "basic_attack" },
            damageMultiplier = 1.8f,
            comboPointReward = 5,
            effectName = "Double Slash",
            comboType = ComboData.ComboType.Offensive,
            comboRarity = ComboData.Rarity.Common,
            requiredComboLevel = 1
        });
        
        _RegisterCombo(new ComboData
        {
            comboId = "combo_triple_slice",
            comboName = "Triple Slice",
            description = "Three rapid cuts dealing massive damage",
            skillSequence = new List<string> { "basic_attack", "basic_attack", "basic_attack" },
            damageMultiplier = 2.5f,
            comboPointReward = 10,
            effectName = "Triple Slash",
            comboType = ComboData.ComboType.Offensive,
            comboRarity = ComboData.Rarity.Uncommon,
            requiredComboLevel = 2
        });
        
        _RegisterCombo(new ComboData
        {
            comboId = "combo_whirlwind",
            comboName = "Whirlwind",
            description = "Spin attack hitting all nearby enemies",
            skillSequence = new List<string> { "basic_attack", "dodge", "basic_attack" },
            damageMultiplier = 2.2f,
            comboPointReward = 15,
            effectName = "Wind Blade",
            comboType = ComboData.ComboType.Offensive,
            comboRarity = ComboData.Rarity.Rare,
            requiredComboLevel = 3
        });
        
        _RegisterCombo(new ComboData
        {
            comboId = "combo_fury",
            comboName = "Fury Rush",
            description = "Berserker combo dealing overwhelming damage",
            skillSequence = new List<string> { "power_strike", "basic_attack", "power_strike" },
            damageMultiplier = 3.0f,
            cooldownReduction = 0.3f,
            comboPointReward = 25,
            effectName = "Fury Explosion",
            comboType = ComboData.ComboType.Offensive,
            comboRarity = ComboData.Rarity.Epic,
            requiredComboLevel = 5
        });
        
        // Defensive Combos
        _RegisterCombo(new ComboData
        {
            comboId = "combo_block_counter",
            comboName = "Block Counter",
            description = "Block and counterattack",
            skillSequence = new List<string> { "block", "basic_attack" },
            damageMultiplier = 1.5f,
            comboPointReward = 8,
            effectName = "Counter Strike",
            comboType = ComboData.ComboType.Defensive,
            comboRarity = ComboData.Rarity.Common,
            requiredComboLevel = 1
        });
        
        _RegisterCombo(new ComboData
        {
            comboId = "combo_shield_bash",
            comboName = "Shield Bash",
            description = "Stun enemies with shield bash combo",
            skillSequence = new List<string> { "block", "dodge", "basic_attack" },
            damageMultiplier = 1.8f,
            comboPointReward = 12,
            effectName = "Shield Impact",
            comboType = ComboData.ComboType.Defensive,
            comboRarity = ComboData.Rarity.Uncommon,
            requiredComboLevel = 2
        });
        
        _RegisterCombo(new ComboData
        {
            comboId = "combo_iron_will",
            comboName = "Iron Will",
            description = "Defensive stance that reflects damage",
            skillSequence = new List<string> { "block", "block", "block" },
            damageMultiplier = 1.0f,
            cooldownReduction = 0.4f,
            comboPointReward = 20,
            effectName = "Iron Reflection",
            comboType = ComboData.ComboType.Defensive,
            comboRarity = ComboData.Rarity.Rare,
            requiredComboLevel = 4
        });
        
        // Support Combos
        _RegisterCombo(new ComboData
        {
            comboId = "combo_healing_wave",
            comboName = "Healing Wave",
            description = "Chain healing skills for massive recovery",
            skillSequence = new List<string> { "heal", "heal" },
            damageMultiplier = 1.0f,
            comboPointReward = 15,
            effectName = "Wave of Life",
            comboType = ComboData.ComboType.Support,
            comboRarity = ComboData.Rarity.Rare,
            requiredComboLevel = 3
        });
        
        _RegisterCombo(new ComboData
        {
            comboId = "combo_blessing",
            comboName = "Divine Blessing",
            description = "Apply multiple buffs at once",
            skillSequence = new List<string> { "buff_attack", "buff_defense", "buff_speed" },
            damageMultiplier = 1.0f,
            cooldownReduction = 0.35f,
            comboPointReward = 30,
            effectName = "Divine Aura",
            comboType = ComboData.ComboType.Support,
            comboRarity = ComboData.Rarity.Epic,
            requiredComboLevel = 5
        });
        
        // Special Combos
        _RegisterCombo(new ComboData
        {
            comboId = "combo_ultimate",
            comboName = "Ultimate Combo",
            description = "The ultimate skill combination",
            skillSequence = new List<string> { "power_strike", "dodge", "basic_attack", "power_strike", "heal" },
            damageMultiplier = 4.0f,
            cooldownReduction = 0.5f,
            comboPointReward = 100,
            effectName = "Divine Wrath",
            comboType = ComboData.ComboType.Special,
            comboRarity = ComboData.Rarity.Legendary,
            requiredComboLevel = 10
        });
        
        _RegisterCombo(new ComboData
        {
            comboId = "combo_elemental_fusion",
            comboName = "Elemental Fusion",
            description = "Combine elements for explosive damage",
            skillSequence = new List<string> { "fire_skill", "ice_skill", "lightning_skill" },
            damageMultiplier = 3.5f,
            comboPointReward = 50,
            effectName = "Elemental Nova",
            comboType = ComboData.ComboType.Special,
            comboRarity = ComboData.Rarity.Legendary,
            requiredComboLevel = 8
        });
        
        // Utility Combos
        _RegisterCombo(new ComboData
        {
            comboId = "combo_swift_escape",
            comboName = "Swift Escape",
            description = "Quick dodge sequence for escape",
            skillSequence = new List<string> { "dodge", "dodge", "speed_buff" },
            damageMultiplier = 1.0f,
            cooldownReduction = 0.25f,
            comboPointReward = 10,
            effectName = "Shadow Step",
            comboType = ComboData.ComboType.Utility,
            comboRarity = ComboData.Rarity.Uncommon,
            requiredComboLevel = 2
        });
    }
    
    private void _RegisterCombo(ComboData combo)
    {
        if (!_combos.ContainsKey(combo.comboId))
        {
            _combos[combo.comboId] = combo;
        }
    }
    
    private void _UpdateComboTimers(float delta)
    {
        foreach (var progress in _playerCombos.Values)
        {
            if (progress.isActive && progress.timeRemaining > 0)
            {
                progress.timeRemaining -= delta;
                if (progress.timeRemaining <= 0)
                {
                    // Combo failed - reset progress
                    progress.currentStep = 0;
                    progress.isActive = false;
                }
            }
        }
    }
    
    // Called when player uses a skill
    /// <summary>
    /// 当玩家使用技能时调用 - 检查是否触发连击
    /// </summary>
    /// <param name="skillId">使用的技能ID</param>
    public void OnSkillUsed(string skillId)
    {
        // Check each combo to see if this skill continues the sequence
        foreach (var progress in _playerCombos.Values)
        {
            var combo = _combos[progress.comboId];
            if (combo == null) continue;
            
            // Skip if combo level requirement not met
            if (_comboLevel < combo.requiredComboLevel) continue;
            
            int expectedStep = progress.currentStep;
            
            // Check if this skill matches the expected skill in sequence
            if (expectedStep < combo.skillSequence.Count && 
                combo.skillSequence[expectedStep] == skillId)
            {
                // Good - advance to next step
                progress.currentStep++;
                progress.timeRemaining = _comboWindow;
                progress.isActive = true;
                
                ComboProgressUpdated?.Invoke(progress.comboId, progress.currentStep, progress.timeRemaining);
                
                // Check if combo is complete
                if (progress.currentStep >= combo.skillSequence.Count)
                {
                    _ExecuteCombo(progress.comboId);
                }
                return; // Only one combo can progress at a time
            }
            else if (expectedStep > 0 && combo.skillSequence[0] == skillId)
            {
                // Restart combo from beginning
                progress.currentStep = 1;
                progress.timeRemaining = _comboWindow;
                progress.isActive = true;
                ComboProgressUpdated?.Invoke(progress.comboId, progress.currentStep, progress.timeRemaining);
            }
        }
    }
    
    private void _ExecuteCombo(string comboId)
    {
        if (!_combos.TryGetValue(comboId, out var combo)) return;
        
        var progress = _playerCombos[comboId];
        
        // Calculate combo damage
        float baseDamage = 100f; // Would get from player stats
        float comboDamage = baseDamage * combo.damageMultiplier;
        
        // Award combo points
        _comboPoints += combo.comboPointReward;
        _CheckLevelUp();
        
        // Apply cooldown reduction
        // Would apply to skill cooldowns
        
        // Track execution
        progress.timesExecuted++;
        progress.currentStep = 0;
        progress.isActive = false;
        
        // Emit signals
        ComboExecuted?.Invoke(comboId, comboDamage, combo.effectName);
        ComboPointsChanged?.Invoke(_comboPoints);
        
        // Check for combo discovery
        _MaybeDiscoverCombo(comboId);
        
        GD.Print($"[ComboSystem] Executed combo: {combo.comboName} for {comboDamage} damage!");
    }
    
    private void _CheckLevelUp()
    {
        int pointsForLevel = _comboLevel * 50;
        int newLevel = 1 + (_comboPoints / pointsForLevel);
        
        if (newLevel > _comboLevel)
        {
            _comboLevel = newLevel;
            ComboLevelChanged?.Invoke(_comboLevel);
            GD.Print($"[ComboSystem] Combo Level up! Now level {_comboLevel}");
        }
    }
    
    private void _MaybeDiscoverCombo(string comboId)
    {
        if (!_discoveredComboIds.Contains(comboId))
        {
            _discoveredComboIds.Add(comboId);
            if (_combos.TryGetValue(comboId, out var combo))
            {
                NewComboDiscovered?.Invoke(combo);
                GD.Print($"[ComboSystem] New combo discovered: {combo.comboName} ({combo.comboRarity})!");
            }
        }
    }
    
    /// <summary>
    /// 获取已发现的连击ID列表
    /// </summary>
    public List<string> GetDiscoveredComboIds() => new List<string>(_discoveredComboIds);
    
    /// <summary>
    /// 检查某个连击是否已发现
    /// </summary>
    public bool IsComboDiscovered(string comboId) => _discoveredComboIds.Contains(comboId);
    
    /// <summary>
    /// 获取未发现的连击数量（用于显示"还有X个未知连击"）
    /// </summary>
    public int GetUndiscoveredCount() => _combos.Count - _discoveredComboIds.Count;
    
    // Getters
    
    /// <summary>
    /// 获取所有连击数据
    /// </summary>
    public Dictionary<string, ComboData> GetAllCombos() => _combos;
    
    /// <summary>
    /// 获取玩家连击进度
    /// </summary>
    public Dictionary<string, ComboProgress> GetPlayerProgress() => _playerCombos;
    
    /// <summary>
    /// 获取当前连击点数
    /// </summary>
    public int GetComboPoints() => _comboPoints;
    
    /// <summary>
    /// 获取当前连击等级
    /// </summary>
    public int GetComboLevel() => _comboLevel;
    
    /// <summary>
    /// 获取连击时间窗口
    /// </summary>
    public float GetComboWindow() => _comboWindow;
    
    /// <summary>
    /// 获取已解锁的连击列表
    /// </summary>
    public List<ComboData> GetUnlockedCombos()
    {
        var unlocked = new List<ComboData>();
        foreach (var combo in _combos.Values)
        {
            if (_comboLevel >= combo.requiredComboLevel)
            {
                unlocked.Add(combo);
            }
        }
        return unlocked;
    }
    
    /// <summary>
    /// 按类型获取连击列表
    /// </summary>
    /// <param name="type">连击类型</param>
    public List<ComboData> GetCombosByType(ComboData.ComboType type)
    {
        var filtered = new List<ComboData>();
        foreach (var combo in _combos.Values)
        {
            if (combo.comboType == type && _comboLevel >= combo.requiredComboLevel)
            {
                filtered.Add(combo);
            }
        }
        return filtered;
    }
    
    // Save/Load
    
    /// <summary>
    /// 导出存档数据
    /// </summary>
    public override Dictionary ExportSaveData()
    {
        var data = new Dictionary();
        data["comboPoints"] = _comboPoints;
        data["comboLevel"] = _comboLevel;
        
        var progressData = new Dictionary();
        foreach (var progress in _playerCombos)
        {
            progressData[progress.Key] = progress.Value.timesExecuted;
        }
        data["progress"] = progressData;
        
        // Save discovered combo IDs
        var discoveredList = new List<object>();
        foreach (var id in _discoveredComboIds)
            discoveredList.Add(id);
        data["discoveredCombos"] = discoveredList;
        
        return data;
    }
    
    /// <summary>
    /// 导入存档数据
    /// </summary>
    /// <param name="data">存档数据字典</param>
    public override void ImportSaveData(Dictionary data)
    {
        if (data == null) return;
        
        if (data.ContainsKey("comboPoints"))
            _comboPoints = (int)data["comboPoints"];
        if (data.ContainsKey("comboLevel"))
            _comboLevel = (int)data["comboLevel"];
        
        if (data.ContainsKey("progress"))
        {
            var progressData = (Dictionary)data["progress"];
            foreach (var entry in progressData)
            {
                if (_playerCombos.TryGetValue(entry.Key, out var progress))
                {
                    progress.timesExecuted = (int)entry.Value;
                }
            }
        }
        
        if (data.ContainsKey("discoveredCombos"))
        {
            _discoveredComboIds.Clear();
            var discoveredList = (List<object>)data["discoveredCombos"];
            foreach (var item in discoveredList)
                _discoveredComboIds.Add((string)item);
        }
    }
    
    // 旧的存档方法（保留兼容性）
    
    /// <summary>
    /// 获取存档数据（兼容性方法）
    /// </summary>
    public Dictionary<string, object> GetSaveData()
    {
        return ExportSaveData();
    }
    
    public void LoadSaveData(Dictionary<string, object> data)
    {
        ImportSaveData(new Dictionary(data));
    }
}
