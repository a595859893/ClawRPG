using Godot;
using System;
using System.Collections.Generic;

public class ComboData : Resource
{
    [Export] public string comboId;
    [Export] public string comboName;
    [Export] public string description;
    [Export] public List<string> skillSequence = new List<string>(); // Required skill IDs in order
    [Export] public float damageMultiplier = 1.5f;
    [Export] public float cooldownReduction = 0.2f; // 20% cooldown reduction
    [Export] public int comboPointReward = 10;
    [Export] public string effectName; // Special effect name
    [Export] public int requiredComboLevel = 1;
    [Export] public ComboType comboType;
    [Export] public Rarity comboRarity;
    
    public enum ComboType
    {
        Offensive,
        Defensive,
        Support,
        Utility,
        Special
    }
    
    public enum Rarity
    {
        Common,
        Uncommon,
        Rare,
        Epic,
        Legendary
    }
}

public class ComboProgress
{
    public string comboId;
    public int currentStep = 0;
    public float timeRemaining = 0f;
    public bool isActive = false;
    public int timesExecuted = 0;
}

public class ComboSystem : Node
{
    public static ComboSystem Instance { get; private set; }
    
    // Combo database
    private Dictionary<string, ComboData> _combos = new Dictionary<string, ComboData>();
    
    // Player combo progress
    private Dictionary<string, ComboProgress> _playerCombos = new Dictionary<string, ComboProgress>();
    private int _comboPoints = 0;
    private int _comboLevel = 1;
    
    // Timing
    private float _comboWindow = 3.0f; // Time to complete combo sequence
    private float _deltaTime;
    
    // Signals
    public static signal ComboExecuted(string comboId, float damage, string effectName);
    public static signal ComboProgressUpdated(string comboId, int currentStep, float timeRemaining);
    public static signal ComboPointsChanged(int newPoints);
    public static signal ComboLevelChanged(int newLevel);
    public static signal NewComboDiscovered(ComboData combo);
    
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
        
        // Initialize progress for each combo
        foreach (var comboId in _combos.Keys)
        {
            _playerCombos[comboId] = new ComboProgress { comboId = comboId };
        }
        
        GD.Print($"[ComboSystem] Initialized {_combos.Count} combos");
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
                
                ComboProgressUpdated?.Call(progress.comboId, progress.currentStep, progress.timeRemaining);
                
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
                ComboProgressUpdated?.Call(progress.comboId, progress.currentStep, progress.timeRemaining);
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
        ComboExecuted?.Call(comboId, comboDamage, combo.effectName);
        ComboPointsChanged?.Call(_comboPoints);
        
        GD.Print($"[ComboSystem] Executed combo: {combo.comboName} for {comboDamage} damage!");
    }
    
    private void _CheckLevelUp()
    {
        int pointsForLevel = _comboLevel * 50;
        int newLevel = 1 + (_comboPoints / pointsForLevel);
        
        if (newLevel > _comboLevel)
        {
            _comboLevel = newLevel;
            ComboLevelChanged?.Call(_comboLevel);
            GD.Print($"[ComboSystem] Combo Level up! Now level {_comboLevel}");
        }
    }
    
    // Getters
    public Dictionary<string, ComboData> GetAllCombos() => _combos;
    public Dictionary<string, ComboProgress> GetPlayerProgress() => _playerCombos;
    public int GetComboPoints() => _comboPoints;
    public int GetComboLevel() => _comboLevel;
    public float GetComboWindow() => _comboWindow;
    
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
    public Dictionary<string, object> GetSaveData()
    {
        var data = new Dictionary<string, object>();
        data["comboPoints"] = _comboPoints;
        data["comboLevel"] = _comboLevel;
        
        var progressData = new Dictionary<string, int>();
        foreach (var progress in _playerCombos)
        {
            progressData[progress.Key] = progress.Value.timesExecuted;
        }
        data["progress"] = progressData;
        
        return data;
    }
    
    public void LoadSaveData(Dictionary<string, object> data)
    {
        if (data.ContainsKey("comboPoints"))
            _comboPoints = (int)data["comboPoints"];
        if (data.ContainsKey("comboLevel"))
            _comboLevel = (int)data["comboLevel"];
        
        if (data.ContainsKey("progress"))
        {
            var progressData, object>)data = (Dictionary<string["progress"];
            foreach (var entry in progressData)
            {
                if (_playerCombos.TryGetValue(entry.Key, out var progress))
                {
                    progress.timesExecuted = (int)entry.Value;
                }
            }
        }
    }
}
