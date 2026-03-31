using Godot;
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using Framework;

// Data structures moved to ComboData.cs

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
    // === REQ-167: Chaos Combo 信号 ===
    /// <summary>
    /// Chaos Combo 进度更新时触发（技能被收集）
    /// </summary>
    public static Action<string, int, List<string>, float> ChaosComboProgressUpdated;
    /// <summary>
    /// Chaos Combo 执行时触发，参数：comboId, 本次随机选中的技能列表
    /// </summary>
    public static Action<string, List<string>> ChaosComboExecuted;
    /// <summary>
    /// 发现新连击时触发
    /// </summary>
    public static Action<ComboData> NewComboDiscovered;
    /// <summary>
    /// 当可用 combo 列表因遗忘/唤醒而变化时触发
    /// </summary>
    public static Action ComboAvailabilityChanged;
    /// <summary>
    /// 连击失败时触发（超时或按错技能）
    /// </summary>
    public static Action<string> ComboFailed;
    
    public override void _Ready()
    {
        Instance = this;
        _InitializeComboDatabase();
        
        // 订阅遗忘状态变化（当 combo 从休眠唤醒时通知 UI 刷新）
        if (ComboForgetData.Instance != null)
        {
            ComboForgetData.ComboForgetStateChanged += OnForgetStateChanged;
            ComboForgetData.ComboRediscovered += OnComboRediscovered;
        }
    }
    
    private void OnForgetStateChanged(string comboId, bool isNowDormant)
    {
        // 休眠状态变化时通知 UI 刷新 combo 列表
        ComboAvailabilityChanged?.Invoke();
    }
    
    private void OnComboRediscovered(string comboId)
    {
        // combo 重新发现时通知 UI（信号由 ComboRediscoveredNotification 处理）
        ComboAvailabilityChanged?.Invoke();
    }
    
    public override void _ExitTree()
    {
        if (ComboForgetData.Instance != null)
        {
            ComboForgetData.ComboForgetStateChanged -= OnForgetStateChanged;
            ComboForgetData.ComboRediscovered -= OnComboRediscovered;
        }
    }
    
    public override void _Process(double delta)
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
                    comboRarity = _ParseRarity(entry.ComboRarity),
                    // REQ-167: Chaos Combo 字段
                    skillPool = entry.SkillPool ?? new List<string>(),
                    poolSizeMin = entry.PoolSizeMin > 0 ? entry.PoolSizeMin : 2,
                    poolSizeMax = entry.PoolSizeMax > 0 ? entry.PoolSizeMax : 4,
                    rarityWeights = entry.RarityWeights ?? new Dictionary<string, float>()
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
            "chaos" => ComboData.ComboType.Chaos,  // REQ-167
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
                    // REQ-168: Emit ComboFailed signal if there was active progress before timeout
                    if (progress.currentStep > 0)
                    {
                        ComboFailed?.Emit(progress.comboId);
                    }
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
            
            // === REQ-167: Chaos Combo 处理 ===
            if (combo.comboType == ComboData.ComboType.Chaos)
            {
                _HandleChaosComboSkill(progress, combo, skillId);
                continue;
            }
            
            // === Regular combo handling ===
            int expectedStep = progress.currentStep;
            
            // Check if this skill matches the expected skill in sequence
            if (expectedStep < combo.skillSequence.Count && 
                combo.skillSequence[expectedStep] == skillId)
            {
                // Good - advance to next step
                progress.currentStep++;
                progress.timeRemaining = _comboWindow;
                progress.isActive = true;
                
                ComboProgressUpdated?.Emit(progress.comboId, progress.currentStep, progress.timeRemaining);
                
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
                ComboProgressUpdated?.Emit(progress.comboId, progress.currentStep, progress.timeRemaining);
            }
        }
    }

    // === REQ-167: Chaos Combo 技能处理 ===
    private void _HandleChaosComboSkill(ComboProgress progress, ComboData combo, string skillId)
    {
        // Ignore if skill not in the pool
        if (combo.skillPool == null || !combo.skillPool.Contains(skillId))
            return;
        
        // Ignore if already collected (no duplicates in pool)
        if (progress.collectedPoolSkills != null && progress.collectedPoolSkills.Contains(skillId))
            return;
        
        // Add to collected pool
        progress.collectedPoolSkills ??= new List<string>();
        progress.collectedPoolSkills.Add(skillId);
        progress.timeRemaining = _comboWindow;
        progress.isActive = true;
        progress.currentStep = progress.collectedPoolSkills.Count;
        
        ChaosComboProgressUpdated?.Emit(combo.comboId, progress.collectedPoolSkills.Count, 
            progress.collectedPoolSkills, progress.timeRemaining);
        
        // Check if we've collected enough skills from the pool
        int minPool = Math.Max(combo.poolSizeMin, 1);
        if (progress.collectedPoolSkills.Count >= minPool)
        {
            _ExecuteChaosCombo(progress.comboId);
        }
    }
    
    // === REQ-167: Chaos Combo 随机抽取执行 ===
    private void _ExecuteChaosCombo(string comboId)
    {
        if (!_combos.TryGetValue(comboId, out var combo)) return;
        
        var progress = _playerCombos[comboId];
        
        // Randomly select skills from pool
        var selectedSkills = _SelectRandomSkillsFromPool(combo);
        
        // Store selected skills for execution
        progress.collectedPoolSkills = selectedSkills;
        
        // Emit signal with selected skills for UI display (REQ-167-05)
        ChaosComboExecuted?.Emit(comboId, selectedSkills);
        
        // Award combo points
        _comboPoints += combo.comboPointReward;
        _CheckLevelUp();
        
        // Apply rarity multiplier to cooldown reduction (consistency with _ExecuteCombo, REQ-167-03)
        float rarityMultiplier = _GetRarityDamageMultiplier(combo.comboRarity);
        float effectiveCdReduction = combo.cooldownReduction * rarityMultiplier;

        // Apply cooldown reduction
        try
        {
            var bonus = new ClawRPG.Scripts.Systems.ComboBonus
            {
                Name = combo.comboName,
                CooldownReduction = effectiveCdReduction,
                DamageMultiplier = 1f,
                Duration = 5f
            };
            ClawRPG.Scripts.Systems.SkillComboSystem.Instance?.ApplyComboBonus(bonus);
        }
        catch (Exception)
        {
            // SkillComboSystem may not be available
        }
        
        // Skills are already executed by the player during collection phase.
        // ChaosComboExecuted signal (emitted above) drives the UI display (REQ-167-05).
        GD.Print($"[ComboSystem] Chaos Combo '{combo.comboName}' executed with skills: {string.Join(", ", selectedSkills)}");
        
        // Reset progress for potential re-trigger
        progress.collectedPoolSkills.Clear();
        progress.currentStep = 0;
        progress.isActive = false;
        progress.timesExecuted++;
    }
    
    // === REQ-167: 加权随机抽取算法 ===
    private List<string> _SelectRandomSkillsFromPool(ComboData combo)
    {
        if (combo.skillPool == null || combo.skillPool.Count == 0)
            return new List<string>();
        
        var pool = combo.skillPool;
        int minCount = Math.Max(combo.poolSizeMin, 1);
        int maxCount = Math.Max(combo.poolSizeMax, minCount);
        int targetCount = GD.RandRange(minCount, maxCount);
        targetCount = Math.Min(targetCount, pool.Count);
        
        // Build weighted list
        var weightedPool = new List<(string skillId, float weight)>();
        foreach (var skillId in pool)
        {
            float weight = 1.0f;
            if (combo.rarityWeights != null && combo.rarityWeights.TryGetValue(skillId, out var w))
                weight = w;
            weightedPool.Add((skillId, weight));
        }
        
        // Weighted random selection without duplicates
        var selected = new List<string>();
        var available = new List<(string skillId, float weight)>(weightedPool);
        var rng = new RandomNumberGenerator();
        rng.Randomize();
        
        while (selected.Count < targetCount && available.Count > 0)
        {
            // Calculate total weight
            float totalWeight = 0f;
            foreach (var item in available)
                totalWeight += item.weight;
            
            // Select by weighted random
            float roll = (float)rng.RandDouble() * totalWeight;
            float cumulative = 0f;
            int selectedIdx = 0;
            for (int i = 0; i < available.Count; i++)
            {
                cumulative += available[i].weight;
                if (roll <= cumulative)
                {
                    selectedIdx = i;
                    break;
                }
            }
            
            selected.Add(available[selectedIdx].skillId);
            available.RemoveAt(selectedIdx);
        }
        
        return selected;
    }
    
    private void _ExecuteCombo(string comboId)
    {
        if (!_combos.TryGetValue(comboId, out var combo)) return;
        
        var progress = _playerCombos[comboId];
        
        // Calculate combo damage — use player's weapon damage instead of hardcoded 100f (REQ-151 Fix #1)
        float baseDamage = 100f;
        try
        {
            var weapon = Player.Instance?.Equipment?.GetCurrentWeapon();
            if (weapon != null) baseDamage = weapon.Damage;
        }
        catch (Exception)
        {
            // Fallback if player/equipment not available at call time
        }
        
        // Apply rarity multiplier (REQ-151 Fix #3: rarity now affects damage)
        float rarityMultiplier = _GetRarityDamageMultiplier(combo.comboRarity);
        float comboDamage = baseDamage * combo.damageMultiplier * rarityMultiplier;
        
        // Award combo points
        _comboPoints += combo.comboPointReward;
        _CheckLevelUp();
        
        // Apply cooldown reduction via SkillComboSystem (REQ-151 Fix #2: cooldownReduction actually applied)
        float effectiveCdReduction = combo.cooldownReduction * rarityMultiplier;
        try
        {
            var bonus = new ClawRPG.Scripts.Systems.ComboBonus
            {
                Name = combo.comboName,
                CooldownReduction = effectiveCdReduction,
                DamageMultiplier = 1f,
                Duration = 5f
            };
            ClawRPG.Scripts.Systems.SkillComboSystem.Instance?.ApplyComboBonus(bonus);
        }
        catch (Exception)
        {
            // SkillComboSystem may not be available
        }
        
        // Track execution
        progress.timesExecuted++;
        progress.currentStep = 0;
        progress.isActive = false;
        
        // Emit signals
        ComboExecuted?.Invoke(comboId, comboDamage, combo.effectName);
        ComboPointsChanged?.Invoke(_comboPoints);
        
        // Check for combo discovery
        _MaybeDiscoverCombo(comboId);
        
        // 记录使用（唤醒休眠 combo 或重置遗忘计时器）
        ComboForgetSystem.Instance?.RecordComboUsage(comboId);
        
        GD.Print($"[ComboSystem] Executed combo: {combo.comboName} (rarity={combo.comboRarity}) for {comboDamage} damage!");
    }
    
    /// <summary>
    /// Returns a damage multiplier based on combo rarity (REQ-151 Fix #3)
    /// </summary>
    private float _GetRarityDamageMultiplier(ComboData.Rarity rarity)
    {
        return rarity switch
        {
            ComboData.Rarity.Common => 1.0f,
            ComboData.Rarity.Uncommon => 1.1f,
            ComboData.Rarity.Rare => 1.25f,
            ComboData.Rarity.Epic => 1.5f,
            ComboData.Rarity.Legendary => 2.0f,
            _ => 1.0f
        };
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
        // 注册到遗忘系统（即使已发现也会更新 wasEverDiscovered）
        ComboForgetSystem.Instance?.RegisterCombo(comboId);
    }
    
    /// <summary>
    /// 获取已发现的连击ID列表（排除休眠 combo，用于 UI 展示）
    /// </summary>
    public List<string> GetDiscoveredComboIds()
    {
        var result = new List<string>();
        foreach (var id in _discoveredComboIds)
        {
            // 排除休眠的 combo（它们仍然可以被执行，但不显示在列表中）
            if (ComboForgetSystem.Instance == null || !ComboForgetSystem.Instance.IsDormant(id))
            {
                result.Add(id);
            }
        }
        return result;
    }
    
    /// <summary>
    /// 检查某个连击是否已发现（无论是否休眠）
    /// </summary>
    public bool IsComboDiscovered(string comboId) => _discoveredComboIds.Contains(comboId);
    
    /// <summary>
    /// 获取未发现的连击数量（用于显示"还有X个未知连击"）
    /// </summary>
    public int GetUndiscoveredCount() => _combos.Count - _discoveredComboIds.Count;

    /// <summary>
    /// 强制发现一个 combo（用于狂暴奖励等系统强制给予 combo 的场景）
    /// </summary>
    public void ForceDiscoverCombo(string comboId)
    {
        if (_combos.ContainsKey(comboId))
        {
            _MaybeDiscoverCombo(comboId);
            GD.Print($"[ComboSystem] Force-discovered combo: {comboId}");
        }
        else
        {
            GD.PrintErr($"[ComboSystem] ForceDiscoverCombo: comboId '{comboId}' not found in database!");
        }
    }

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

    // === REQ-168: Combo Intent Display ===
    /// <summary>
    /// 获取指定 combo 的下一个预期技能 ID（REQ-168）
    /// 返回 null 如果 combo 不存在、已完成或未激活
    /// </summary>
    public string GetExpectedSkill(string comboId)
    {
        if (!_playerCombos.TryGetValue(comboId, out var progress))
            return null;

        if (!progress.isActive || progress.currentStep == 0)
            return null;

        if (!_combos.TryGetValue(comboId, out var combo))
            return null;

        // Chaos Combo 不适用此方法
        if (combo.comboType == ComboData.ComboType.Chaos)
            return null;

        if (progress.currentStep >= combo.skillSequence.Count)
            return null;

        return combo.skillSequence[progress.currentStep];
    }

    /// <summary>
    /// 获取当前激活的 combo 中进度最高的一个的预期技能（用于 UI 显示）
    /// </summary>
    public (string comboId, string expectedSkillId) GetActiveComboIntent()
    {
        string bestComboId = null;
        string bestSkillId = null;
        int bestStep = 0;

        foreach (var progress in _playerCombos.Values)
        {
            if (progress.isActive && progress.currentStep > bestStep)
            {
                string skillId = GetExpectedSkill(progress.comboId);
                if (skillId != null)
                {
                    bestStep = progress.currentStep;
                    bestComboId = progress.comboId;
                    bestSkillId = skillId;
                }
            }
        }

        return (bestComboId, bestSkillId);
    }
    
    /// <summary>
    /// 获取已解锁的连击列表（排除休眠 combo）
    /// </summary>
    public List<ComboData> GetUnlockedCombos()
    {
        var unlocked = new List<ComboData>();
        foreach (var combo in _combos.Values)
        {
            if (_comboLevel >= combo.requiredComboLevel)
            {
                // 排除休眠的 combo
                if (ComboForgetSystem.Instance != null && ComboForgetSystem.Instance.IsDormant(combo.comboId))
                    continue;
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
                // 排除休眠的 combo
                if (ComboForgetSystem.Instance != null && ComboForgetSystem.Instance.IsDormant(combo.comboId))
                    continue;
                filtered.Add(combo);
            }
        }
        return filtered;
    }
    
    // Save/Load
    
    /// <summary>
    /// 导出存档数据
    /// </summary>
    public override Dictionary<string, object> ExportSaveData()
    {
        var data = new Dictionary<string, object>();
        data["comboPoints"] = _comboPoints;
        data["comboLevel"] = _comboLevel;
        
        var progressData = new Dictionary<string, object>();
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
    public override void ImportSaveData(Dictionary<string, object> data)
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
