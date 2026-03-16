using Godot;
using System;
using System.Collections.Generic;
using ClawRPG.Scripts.Framework;

namespace ClawRPG.Scripts.Skills {
    /// <summary>
    /// Skill mastery levels and progression
    /// </summary>
    public enum MasteryRank
    {
        Novice,      // 0-100 XP
        Apprentice,  // 100-500 XP
        Expert,      // 500-1500 XP
        Master,      // 1500-5000 XP
        GrandMaster  // 5000+ XP
    }
    
    /// <summary>
    /// Skill combo types
    /// </summary>
    public enum ComboType
    {
        Sequential,  // Must use skills in order
        Parallel,   // Any combination within time window
        Chain,      // Chain reactions
        Fusion      // Combined effects
    }
    
    /// <summary>
    /// Skill rune slot types
    /// </summary>
    public enum RuneSlotType
    {
        Damage,      // 伤害增强
        Cooldown,   // 冷却缩减
        Range,      // 范围扩展
        Duration,   // 持续时间
        Cost        // 能量消耗
    }
    
    /// <summary>
    /// Skill rune configuration
    /// </summary>
    public class SkillRune
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public string Description { get; set; } = "";
        public RuneSlotType SlotType { get; set; }
        public float Value { get; set; } // Percentage or flat bonus
        public int Rarity { get; set; } // 1-5 (Common to Legendary)
    }
    
    /// <summary>
    /// Skill mastery record
    /// </summary>
    public class SkillMastery
    {
        public int SkillId { get; set; }
        public int CurrentLevel { get; set; } = 1;
        public int CurrentXP { get; set; } = 0;
        public int TotalXP { get; set; } = 0;
        public MasteryRank Rank { get; set; } = MasteryRank.Novice;
        public int RuneSlots { get; set; } = 0; // Unlocked slots (0-3)
        public Dictionary<int, int> EquippedRunes { get; set; } = new(); // slot -> rune id
        
        // Bonuses from mastery
        public float DamageBonus { get; set; } = 0;
        public float CooldownReduction { get; set; } = 0;
        public float RangeBonus { get; set; } = 0;
        public float CostReduction { get; set; } = 0;
    }
    
    /// <summary>
    /// Skill combo definition
    /// </summary>
    public class SkillCombo
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public string Description { get; set; } = "";
        public ComboType Type { get; set; }
        public List<int> SkillSequence { get; set; } = new(); // Skill IDs in order
        public float TimeWindow { get; set; } = 3f; // Time to complete combo
        public int ManaCost { get; set; }
        public float DamageMultiplier { get; set; } = 1.5f;
        public float Cooldown { get; set; } = 15f;
        public int RequiredMasteryLevel { get; set; } = 1;
        public List<int> RequiredSkillIds { get; set; } = new(); // Must have these skills
    }
    
    /// <summary>
    /// Active combo tracking
    /// </summary>
    public class ActiveCombo
    {
        public int ComboId { get; set; }
        public int CurrentStep { get; set; } = 0;
        public float TimeRemaining { get; set; }
        public bool IsComplete { get; set; } = false;
    }
    
    /// <summary>
    /// Skill progression data
    /// </summary>
    public class SkillProgressionData
    {
        public Dictionary<int, SkillMastery> Masteries { get; set; } = new(); // skill id -> mastery
        public Dictionary<int, int> ComboUsages { get; set; } = new(); // combo id -> usage count
        public int TotalMasteryXP { get; set; } = 0;
        public int HighestMasteryRank { get; set; } = 0; // 0=Novice, 4=GrandMaster
    }
    
    /// <summary>
    /// Skill Mastery and Combo System
    /// Manages skill progression, combos, and rune equipment
    /// </summary>
    public partial class SkillMasterySystem : BaseSystem
    {
        private static SkillMasterySystem _instance;
        public static SkillMasterySystem Instance => _instance ??= new SkillMasterySystem();
        
        private Dictionary<int, SkillRune> _runes = new();
        private Dictionary<int, SkillCombo> _combos = new();
        private Dictionary<int, SkillMastery> _masteries = new();
        private ActiveCombo _activeCombo = null;
        private Dictionary<int, float> _comboCooldowns = new();
        
        // Progression data
        public int TotalMasteryXP { get; private set; } = 0;
        public int HighestMasteryRank { get; private set; } = 0;
        
        protected override void Initialize()
        {
            InitializeRunes();
            InitializeCombos();
            IsInitialized = true;
        }
        
        private void InitializeRunes()
        {
            // Damage runes
            AddRune(new SkillRune { Id = 1, Name = "锋利", Description = "+10% 技能伤害", SlotType = RuneSlotType.Damage, Value = 0.10f, Rarity = 1 });
            AddRune(new SkillRune { Id = 2, Name = "锐利", Description = "+20% 技能伤害", SlotType = RuneSlotType.Damage, Value = 0.20f, Rarity = 2 });
            AddRune(new SkillRune { Id = 3, Name = "致命", Description = "+35% 技能伤害", SlotType = RuneSlotType.Damage, Value = 0.35f, Rarity = 3 });
            AddRune(new SkillRune { Id = 4, Name = "毁灭", Description = "+50% 技能伤害", SlotType = RuneSlotType.Damage, Value = 0.50f, Rarity = 4 });
            AddRune(new SkillRune { Id = 5, Name = "天罚", Description = "+75% 技能伤害", SlotType = RuneSlotType.Damage, Value = 0.75f, Rarity = 5 });
            
            // Cooldown runes
            AddRune(new SkillRune { Id = 11, Name = "迅捷", Description = "-5% 冷却时间", SlotType = RuneSlotType.Cooldown, Value = 0.05f, Rarity = 1 });
            AddRune(new SkillRune { Id = 12, Name = "高效", Description = "-10% 冷却时间", SlotType = RuneSlotType.Cooldown, Value = 0.10f, Rarity = 2 });
            AddRune(new SkillRune { Id = 13, Name = "加速", Description = "-15% 冷却时间", SlotType = RuneSlotType.Cooldown, Value = 0.15f, Rarity = 3 });
            AddRune(new SkillRune { Id = 14, Name = "时空", Description = "-25% 冷却时间", SlotType = RuneSlotType.Cooldown, Value = 0.25f, Rarity = 4 });
            AddRune(new SkillRune { Id = 15, Name = "法则", Description = "-35% 冷却时间", SlotType = RuneSlotType.Cooldown, Value = 0.35f, Rarity = 5 });
            
            // Range runes
            AddRune(new SkillRune { Id = 21, Name = "扩展", Description = "+10% 范围", SlotType = RuneSlotType.Range, Value = 0.10f, Rarity = 1 });
            AddRune(new SkillRune { Id = 22, Name = "延伸", Description = "+20% 范围", SlotType = RuneSlotType.Range, Value = 0.20f, Rarity = 2 });
            AddRune(new SkillRune { Id = 23, Name = "扩散", Description = "+35% 范围", SlotType = RuneSlotType.Range, Value = 0.35f, Rarity = 3 });
            AddRune(new SkillRune { Id = 24, Name = "领域", Description = "+50% 范围", SlotType = RuneSlotType.Range, Value = 0.50f, Rarity = 4 });
            AddRune(new SkillRune { Id = 25, Name = "虚空", Description = "+75% 范围", SlotType = RuneSlotType.Range, Value = 0.75f, Rarity = 5 });
            
            // Duration runes
            AddRune(new SkillRune { Id = 31, Name = "延长", Description = "+10% 持续时间", SlotType = RuneSlotType.Duration, Value = 0.10f, Rarity = 1 });
            AddRune(new SkillRune { Id = 32, Name = "持久", Description = "+20% 持续时间", SlotType = RuneSlotType.Duration, Value = 0.20f, Rarity = 2 });
            AddRune(new SkillRune { Id = 33, Name = "永恒", Description = "+35% 持续时间", SlotType = RuneSlotType.Duration, Value = 0.35f, Rarity = 3 });
            AddRune(new SkillRune { Id = 34, Name = "永续", Description = "+50% 持续时间", SlotType = RuneSlotType.Duration, Value = 0.50f, Rarity = 4 });
            AddRune(new SkillRune { Id = 35, Name = "无尽", Description = "+75% 持续时间", SlotType = RuneSlotType.Duration, Value = 0.75f, Rarity = 5 });
            
            // Cost runes
            AddRune(new SkillRune { Id = 41, Name = "节流", Description = "-5% 能量消耗", SlotType = RuneSlotType.Cost, Value = 0.05f, Rarity = 1 });
            AddRune(new SkillRune { Id = 42, Name = "节约", Description = "-10% 能量消耗", SlotType = RuneSlotType.Cost, Value = 0.10f, Rarity = 2 });
            AddRune(new SkillRune { Id = 43, Name = "精炼", Description = "-15% 能量消耗", SlotType = RuneSlotType.Cost, Value = 0.15f, Rarity = 3 });
            AddRune(new SkillRune { Id = 44, Name = "冥想", Description = "-25% 能量消耗", SlotType = RuneSlotType.Cost, Value = 0.25f, Rarity = 4 });
            AddRune(new SkillRune { Id = 45, Name = "共鸣", Description = "-35% 能量消耗", SlotType = RuneSlotType.Cost, Value = 0.35f, Rarity = 5 });
        }
        
        private void InitializeCombos()
        {
            // Sequential combos
            AddCombo(new SkillCombo
            {
                Id = 1,
                Name = "闪电连击",
                Description = "闪电箭 → 链式闪电",
                Type = ComboType.Sequential,
                SkillSequence = new List<int> { 1, 10 },
                TimeWindow = 4f,
                ManaCost = 35,
                DamageMultiplier = 1.8f,
                Cooldown = 20f,
                RequiredMasteryLevel = 3,
                RequiredSkillIds = new List<int> { 1, 10 }
            });
            
            AddCombo(new SkillCombo
            {
                Id = 2,
                Name = "冰火两重天",
                Description = "燃烧弹 → 冰霜新星",
                Type = ComboType.Sequential,
                SkillSequence = new List<int> { 7, 8 },
                TimeWindow = 4f,
                ManaCost = 35,
                DamageMultiplier = 2.0f,
                Cooldown = 25f,
                RequiredMasteryLevel = 4,
                RequiredSkillIds = new List<int> { 7, 8 }
            });
            
            AddCombo(new SkillCombo
            {
                Id = 3,
                Name = "暗影爆发",
                Description = "暗影箭 → 暗影之刺",
                Type = ComboType.Sequential,
                SkillSequence = new List<int> { 4, 9 },
                TimeWindow = 3f,
                ManaCost = 30,
                DamageMultiplier = 1.6f,
                Cooldown = 18f,
                RequiredMasteryLevel = 2,
                RequiredSkillIds = new List<int> { 4, 9 }
            });
            
            // Chain combos
            AddCombo(new SkillCombo
            {
                Id = 4,
                Name = "治疗链",
                Description = "治疗术 → 群体治疗 → 再生",
                Type = ComboType.Chain,
                SkillSequence = new List<int> { 101, 102, 103 },
                TimeWindow = 6f,
                ManaCost = 50,
                DamageMultiplier = 2.5f, // Healing multiplier
                Cooldown = 30f,
                RequiredMasteryLevel = 5,
                RequiredSkillIds = new List<int> { 101, 102, 103 }
            });
            
            AddCombo(new SkillCombo
            {
                Id = 5,
                Name = "护盾链",
                Description = "魔法护盾 → 圣光护盾",
                Type = ComboType.Chain,
                SkillSequence = new List<int> { 203, 204 },
                TimeWindow = 5f,
                ManaCost = 55,
                DamageMultiplier = 1.4f,
                Cooldown = 35f,
                RequiredMasteryLevel = 4,
                RequiredSkillIds = new List<int> { 203, 204 }
            });
            
            // Parallel combos
            AddCombo(new SkillCombo
            {
                Id = 6,
                Name = "元素风暴",
                Description = "在时间窗口内使用3个元素技能",
                Type = ComboType.Parallel,
                SkillSequence = new List<int> { 1, 4, 7 }, // Lightning, Shadow, Fire
                TimeWindow = 5f,
                ManaCost = 45,
                DamageMultiplier = 2.2f,
                Cooldown = 40f,
                RequiredMasteryLevel = 4,
                RequiredSkillIds = new List<int> { 1, 4, 7 }
            });
            
            // Fusion combos
            AddCombo(new SkillCombo
            {
                Id = 7,
                Name = "终极陨石",
                Description = "燃烧弹 + 陨石 = 毁灭性打击",
                Type = ComboType.Fusion,
                SkillSequence = new List<int> { 7, 2 },
                TimeWindow = 3f,
                ManaCost = 60,
                DamageMultiplier = 3.0f,
                Cooldown = 60f,
                RequiredMasteryLevel = 5,
                RequiredSkillIds = new List<int> { 7, 2 }
            });
            
            AddCombo(new SkillCombo
            {
                Id = 8,
                Name = "圣光审判",
                Description = "圣光打击 + 治疗术 = 审判",
                Type = ComboType.Fusion,
                SkillSequence = new List<int> { 3, 101 },
                TimeWindow = 3f,
                ManaCost = 40,
                DamageMultiplier = 2.5f,
                Cooldown = 45f,
                RequiredMasteryLevel = 5,
                RequiredSkillIds = new List<int> { 3, 101 }
            });
        }
        
        private void AddRune(SkillRune rune)
        {
            _runes[rune.Id] = rune;
        }
        
        private void AddCombo(SkillCombo combo)
        {
            _combos[combo.Id] = combo;
        }
        
        #region Mastery Methods
        
        /// <summary>
        /// Get or create mastery for a skill
        /// </summary>
        public SkillMastery GetMastery(int skillId)
        {
            if (!_masteries.ContainsKey(skillId))
            {
                _masteries[skillId] = new SkillMastery { SkillId = skillId };
            }
            return _masteries[skillId];
        }
        
        /// <summary>
        /// Add XP to skill mastery
        /// </summary>
        public void AddMasteryXP(int skillId, int xp)
        {
            var mastery = GetMastery(skillId);
            mastery.CurrentXP += xp;
            mastery.TotalXP += xp;
            TotalMasteryXP += xp;
            
            // Check for level up
            CheckMasteryLevelUp(mastery);
            
            // Update highest rank
            if ((int)mastery.Rank > HighestMasteryRank)
            {
                HighestMasteryRank = (int)mastery.Rank;
            }
            
            GD.Print($"Skill {skillId} mastery: {mastery.CurrentXP} XP, Level {mastery.CurrentLevel}, Rank {mastery.Rank}");
        }
        
        private void CheckMasteryLevelUp(SkillMastery mastery)
        {
            int[] xpThresholds = { 100, 500, 1500, 5000 };
            int[] levelUpXP = { 100, 400, 1000, 3500 };
            
            while (mastery.CurrentLevel < 10)
            {
                int threshold = levelUpXP[Math.Min(mastery.CurrentLevel - 1, 3)];
                if (mastery.CurrentXP >= threshold)
                {
                    mastery.CurrentLevel++;
                    ApplyMasteryBonuses(mastery);
                    
                    // Unlock rune slot at levels 3, 6, 9
                    if (mastery.CurrentLevel == 3) mastery.RuneSlots = 1;
                    if (mastery.CurrentLevel == 6) mastery.RuneSlots = 2;
                    if (mastery.CurrentLevel == 9) mastery.RuneSlots = 3;
                }
                else
                {
                    break;
                }
            }
            
            // Update rank based on total XP
            if (mastery.TotalXP >= 5000) mastery.Rank = MasteryRank.GrandMaster;
            else if (mastery.TotalXP >= 1500) mastery.Rank = MasteryRank.Master;
            else if (mastery.TotalXP >= 500) mastery.Rank = MasteryRank.Expert;
            else if (mastery.TotalXP >= 100) mastery.Rank = MasteryRank.Apprentice;
            else mastery.Rank = MasteryRank.Novice;
        }
        
        private void ApplyMasteryBonuses(SkillMastery mastery)
        {
            // Level-based bonuses
            mastery.DamageBonus = mastery.CurrentLevel * 0.05f; // +5% per level
            mastery.CooldownReduction = Math.Min(0.30f, mastery.CurrentLevel * 0.03f); // +3% per level, max 30%
            mastery.RangeBonus = mastery.CurrentLevel * 0.02f; // +2% per level
            mastery.CostReduction = mastery.CurrentLevel * 0.02f; // +2% per level
            
            GD.Print($"Mastery bonuses for skill {mastery.SkillId}: Damage +{mastery.DamageBonus*100}%, " +
                    $"CDR -{mastery.CooldownReduction*100}%, Range +{mastery.RangeBonus*100}%, Cost -{mastery.CostReduction*100}%");
        }
        
        /// <summary>
        /// Equip a rune to a skill mastery slot
        /// </summary>
        public bool EquipRune(int skillId, int slot, int runeId)
        {
            var mastery = GetMastery(skillId);
            
            if (slot < 0 || slot >= mastery.RuneSlots)
            {
                GD.Print($"Invalid slot {slot}. Available slots: {mastery.RuneSlots}");
                return false;
            }
            
            if (!_runes.ContainsKey(runeId))
            {
                GD.Print($"Rune {runeId} not found");
                return false;
            }
            
            mastery.EquippedRunes[slot] = runeId;
            GD.Print($"Equipped rune {runeId} to skill {skillId} slot {slot}");
            return true;
        }
        
        /// <summary>
        /// Unequip a rune from a skill mastery slot
        /// </summary>
        public bool UnequipRune(int skillId, int slot)
        {
            var mastery = GetMastery(skillId);
            
            if (mastery.EquippedRunes.ContainsKey(slot))
            {
                mastery.EquippedRunes.Remove(slot);
                return true;
            }
            return false;
        }
        
        /// <summary>
        /// Calculate total bonuses from mastery and runes
        /// </summary>
        public (float damage, float cdr, float range, float cost) GetTotalBonuses(int skillId)
        {
            var mastery = GetMastery(skillId);
            float damage = mastery.DamageBonus;
            float cdr = mastery.CooldownReduction;
            float range = mastery.RangeBonus;
            float cost = mastery.CostReduction;
            
            // Add rune bonuses
            foreach (var kvp in mastery.EquippedRunes)
            {
                var rune = _runes[kvp.Value];
                switch (rune.SlotType)
                {
                    case RuneSlotType.Damage: damage += rune.Value; break;
                    case RuneSlotType.Cooldown: cdr += rune.Value; break;
                    case RuneSlotType.Range: range += rune.Value; break;
                    case RuneSlotType.Cost: cost += rune.Value; break;
                }
            }
            
            return (damage, cdr, range, cost);
        }
        
        #endregion
        
        #region Combo Methods
        
        /// <summary>
        /// Start a combo by using first skill
        /// </summary>
        public bool StartCombo(int skillId, int playerMana)
        {
            foreach (var combo in _combos.Values)
            {
                if (combo.SkillSequence.Count > 0 && combo.SkillSequence[0] == skillId)
                {
                    // Check requirements
                    if (!HasComboRequirements(combo)) continue;
                    if (IsComboOnCooldown(combo.Id)) continue;
                    if (playerMana < combo.ManaCost) continue;
                    
                    _activeCombo = new ActiveCombo
                    {
                        ComboId = combo.Id,
                        CurrentStep = 0,
                        TimeRemaining = combo.TimeWindow,
                        IsComplete = false
                    };
                    
                    GD.Print($"Started combo: {combo.Name}");
                    return true;
                }
            }
            return false;
        }
        
        /// <summary>
        /// Continue combo with next skill
        /// </summary>
        public bool ContinueCombo(int skillId, int playerMana)
        {
            if (_activeCombo == null || _activeCombo.IsComplete) return false;
            
            var combo = _combos[_activeCombo.ComboId];
            
            // Check if this is the correct next skill
            int expectedSkillId = combo.SkillSequence[_activeCombo.CurrentStep + 1];
            if (skillId != expectedSkillId)
            {
                // For parallel combos, check if skill is in sequence
                if (combo.Type == ComboType.Parallel)
                {
                    if (!combo.SkillSequence.Contains(skillId)) return false;
                    // Check if already used in this combo
                    for (int i = 0; i <= _activeCombo.CurrentStep; i++)
                    {
                        if (combo.SkillSequence[i] == skillId)
                        {
                            GD.Print("Skill already used in parallel combo");
                            return false;
                        }
                    }
                }
                else
                {
                    GD.Print($"Wrong skill for combo. Expected {expectedSkillId}, got {skillId}");
                    CancelCombo();
                    return false;
                }
            }
            
            // Check mana
            if (playerMana < combo.ManaCost)
            {
                GD.Print("Not enough mana for combo");
                CancelCombo();
                return false;
            }
            
            _activeCombo.CurrentStep++;
            _activeCombo.TimeRemaining = combo.TimeWindow; // Reset timer
            
            // Check if combo complete
            if (_activeCombo.CurrentStep >= combo.SkillSequence.Count - 1)
            {
                CompleteCombo();
                return true;
            }
            
            GD.Print($"Combo progress: {_activeCombo.CurrentStep + 1}/{combo.SkillSequence.Count}");
            return true;
        }
        
        private bool HasComboRequirements(SkillCombo combo)
        {
            var skillManager = new SkillManager();
            
            foreach (var reqSkillId in combo.RequiredSkillIds)
            {
                if (!skillManager.HasLearned(reqSkillId))
                {
                    return false;
                }
            }
            
            return true;
        }
        
        private bool IsComboOnCooldown(int comboId)
        {
            return _comboCooldowns.ContainsKey(comboId) && _comboCooldowns[comboId] > 0;
        }
        
        private void CompleteCombo()
        {
            if (_activeCombo == null) return;
            
            var combo = _combos[_activeCombo.ComboId];
            _activeCombo.IsComplete = true;
            
            // Start combo cooldown
            _comboCooldowns[_activeCombo.ComboId] = combo.Cooldown;
            
            // Track usage
            if (!ComboUsages.ContainsKey(_activeCombo.ComboId))
                ComboUsages[_activeCombo.ComboId] = 0;
            ComboUsages[_activeCombo.ComboId]++;
            
            // Grant mastery XP
            int xpGain = 50 * combo.SkillSequence.Count;
            foreach (var skillId in combo.SkillSequence)
            {
                AddMasteryXP(skillId, xpGain);
            }
            
            GD.Print($"Combo complete: {combo.Name}! Damage multiplier: {combo.DamageMultiplier}x");
            
            _activeCombo = null;
        }
        
        private void CancelCombo()
        {
            if (_activeCombo != null)
            {
                GD.Print($"Combo cancelled: {_combos[_activeCombo.ComboId].Name}");
                _activeCombo = null;
            }
        }
        
        /// <summary>
        /// Get current active combo info
        /// </summary>
        public ActiveCombo GetActiveCombo() => _activeCombo;
        
        /// <summary>
        /// Get all available combos for player
        /// </summary>
        public List<SkillCombo> GetAvailableCombos()
        {
            var result = new List<SkillCombo>();
            var skillManager = new SkillManager();
            
            foreach (var combo in _combos.Values)
            {
                if (HasComboRequirements(combo))
                {
                    result.Add(combo);
                }
            }
            
            return result;
        }
        
        /// <summary>
        /// Get combo by ID
        /// </summary>
        public SkillCombo GetCombo(int comboId)
        {
            return _combos.ContainsKey(comboId) ? _combos[comboId] : null;
        }
        
        #endregion
        
        #region Update
        
        public void Update(float delta)
        {
            // Update combo timer
            if (_activeCombo != null)
            {
                _activeCombo.TimeRemaining -= delta;
                if (_activeCombo.TimeRemaining <= 0)
                {
                    CancelCombo();
                }
            }
            
            // Update combo cooldowns
            foreach (var key in _comboCooldowns.Keys)
            {
                _comboCooldowns[key] = Math.Max(0, _comboCooldowns[key] - delta);
            }
        }
        
        #endregion
        
        #region Save/Load
        
        public override Dictionary<string, object> ExportSaveData()
        {
            var data = new Dictionary<string, object>();
            
            // 精通数据
            data["masteries"] = _masteries;
            
            // Combo 使用次数
            data["comboUsages"] = ComboUsages;
            
            // 精通总经验
            data["totalMasteryXP"] = TotalMasteryXP;
            
            // 最高精通等级
            data["highestMasteryRank"] = HighestMasteryRank;
            
            return data;
        }
        
        public override void ImportSaveData(Dictionary<string, object> data)
        {
            if (data == null) return;
            
            // 精通数据
            if (data.TryGetValue("masteries", out var m))
            {
                _masteries.Clear();
                var dict = m as Dictionary<object, object>;
                if (dict != null)
                {
                    foreach (var kvp in dict)
                    {
                        var mastery = kvp.Value as SkillMastery;
                        if (mastery != null)
                        {
                            _masteries[Convert.ToInt32(kvp.Key)] = mastery;
                        }
                    }
                }
            }
            
            // Combo 使用次数
            if (data.TryGetValue("comboUsages", out var cu))
            {
                ComboUsages.Clear();
                var dict = cu as Dictionary<object, object>;
                if (dict != null)
                {
                    foreach (var kvp in dict)
                    {
                        ComboUsages[Convert.ToInt32(kvp.Key)] = Convert.ToInt32(kvp.Value);
                    }
                }
            }
            
            // 精通总经验
            if (data.TryGetValue("totalMasteryXP", out var tm))
                TotalMasteryXP = Convert.ToInt32(tm);
            
            // 最高精通等级
            if (data.TryGetValue("highestMasteryRank", out var hmr))
                HighestMasteryRank = Convert.ToInt32(hmr);
        }
        
        #endregion
        
        /// <summary>
        /// Get all runes of a specific type
        /// </summary>
        public List<SkillRune> GetRunesByType(RuneSlotType type)
        {
            var result = new List<SkillRune>();
            foreach (var rune in _runes.Values)
            {
                if (rune.SlotType == type)
                    result.Add(rune);
            }
            return result;
        }
        
        /// <summary>
        /// Get rune by ID
        /// </summary>
        public SkillRune GetRune(int runeId)
        {
            return _runes.ContainsKey(runeId) ? _runes[runeId] : null;
        }
        
        /// <summary>
        /// Get all available combos
        /// </summary>
        public List<SkillCombo> GetAllCombos() => new List<SkillCombo>(_combos.Values);
        
        /// <summary>
        /// Get mastery for all skills
        /// </summary>
        public Dictionary<int, SkillMastery> GetAllMasteries() => _masteries;
    }
    
    /// <summary>
    /// Track combo usage statistics
    /// </summary>
    public Dictionary<int, int> ComboUsages { get; private set; } = new Dictionary<int, int>();
}
