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
    /// 技能精通与连招系统 - 协调者
    /// 委托给子系统: SkillProgressionSystem, SkillTreeSystem, SkillUnlockSystem
    /// </summary>
    public partial class SkillMasterySystem : BaseSystem
    {
        // Singleton
        private static SkillMasterySystem _instance;
        public static SkillMasterySystem Instance => _instance;
        
        #region Signals
public delegate void MasteryLevelUpEventHandler(int skillId, int newLevel, MasteryRank rank);
public delegate void MasteryXPChangedEventHandler(int skillId, int xp, int totalXP);
public delegate void ComboStartedEventHandler(int comboId, string comboName);
public delegate void ComboCompletedEventHandler(int comboId, string comboName, float damageMultiplier);
public delegate void ComboFailedEventHandler(int comboId, string reason);
public delegate void RuneEquippedEventHandler(int skillId, int slot, int runeId);
public delegate void RuneUnequippedEventHandler(int skillId, int slot);
public delegate void SkillUnlockedEventHandler(string skillId);
        
        #endregion
        
        #region Subsystems
        
        private SkillProgressionSystem _progressionSystem;
        private SkillTreeSystem _treeSystem;
        private SkillUnlockSystem _unlockSystem;
        
        #endregion
        
        #region Data
        
        private Dictionary<int, SkillRune> _runes = new();
        private Dictionary<int, SkillCombo> _combos = new();
        private Dictionary<int, SkillMastery> _masteries = new();
        private ActiveCombo _activeCombo = null;
        private Dictionary<int, float> _comboCooldowns = new();
        
        // Progression data
        public int TotalMasteryXP { get; private set; } = 0;
        public int HighestMasteryRank { get; private set; } = 0;
        
        // Combo usage tracking
        public Dictionary<int, int> ComboUsages { get; private set; } = new();
        
        #endregion
        
        protected override void Initialize()
        {
            _instance = this;
            
            // Get subsystem references
            _progressionSystem = SkillProgressionSystem.Instance;
            _treeSystem = SkillTreeSystem.Instance;
            _unlockSystem = SkillUnlockSystem.Instance;
            
            // Initialize data
            InitializeRunes();
            InitializeCombos();
            
            IsInitialized = true;
            GD.Print("[SkillMasterySystem] Initialized as coordinator");
        }
        
        #region Initialization
        
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
            AddCombo(new SkillCombo { Id = 1, Name = "闪电连击", Description = "闪电箭 → 链式闪电", Type = ComboType.Sequential, SkillSequence = new List<int> { 1, 10 }, TimeWindow = 4f, ManaCost = 35, DamageMultiplier = 1.8f, Cooldown = 20f, RequiredMasteryLevel = 3, RequiredSkillIds = new List<int> { 1, 10 } });
            AddCombo(new SkillCombo { Id = 2, Name = "冰火两重天", Description = "燃烧弹 → 冰霜新星", Type = ComboType.Sequential, SkillSequence = new List<int> { 7, 8 }, TimeWindow = 4f, ManaCost = 35, DamageMultiplier = 2.0f, Cooldown = 25f, RequiredMasteryLevel = 4, RequiredSkillIds = new List<int> { 7, 8 } });
            AddCombo(new SkillCombo { Id = 3, Name = "暗影爆发", Description = "暗影箭 → 暗影之刺", Type = ComboType.Sequential, SkillSequence = new List<int> { 4, 9 }, TimeWindow = 3f, ManaCost = 30, DamageMultiplier = 1.6f, Cooldown = 18f, RequiredMasteryLevel = 2, RequiredSkillIds = new List<int> { 4, 9 } });
            AddCombo(new SkillCombo { Id = 4, Name = "治疗链", Description = "治疗术 → 群体治疗 → 再生", Type = ComboType.Chain, SkillSequence = new List<int> { 101, 102, 103 }, TimeWindow = 6f, ManaCost = 50, DamageMultiplier = 2.5f, Cooldown = 30f, RequiredMasteryLevel = 5, RequiredSkillIds = new List<int> { 101, 102, 103 } });
            AddCombo(new SkillCombo { Id = 5, Name = "护盾链", Description = "魔法护盾 → 圣光护盾", Type = ComboType.Chain, SkillSequence = new List<int> { 203, 204 }, TimeWindow = 5f, ManaCost = 55, DamageMultiplier = 1.4f, Cooldown = 35f, RequiredMasteryLevel = 4, RequiredSkillIds = new List<int> { 203, 204 } });
            AddCombo(new SkillCombo { Id = 6, Name = "元素风暴", Description = "在时间窗口内使用3个元素技能", Type = ComboType.Parallel, SkillSequence = new List<int> { 1, 4, 7 }, TimeWindow = 5f, ManaCost = 45, DamageMultiplier = 2.2f, Cooldown = 40f, RequiredMasteryLevel = 4, RequiredSkillIds = new List<int> { 1, 4, 7 } });
            AddCombo(new SkillCombo { Id = 7, Name = "终极陨石", Description = "燃烧弹 + 陨石 = 毁灭性打击", Type = ComboType.Fusion, SkillSequence = new List<int> { 7, 2 }, TimeWindow = 3f, ManaCost = 60, DamageMultiplier = 3.0f, Cooldown = 60f, RequiredMasteryLevel = 5, RequiredSkillIds = new List<int> { 7, 2 } });
            AddCombo(new SkillCombo { Id = 8, Name = "圣光审判", Description = "圣光打击 + 治疗术 = 审判", Type = ComboType.Fusion, SkillSequence = new List<int> { 3, 101 }, TimeWindow = 3f, ManaCost = 40, DamageMultiplier = 2.5f, Cooldown = 45f, RequiredMasteryLevel = 5, RequiredSkillIds = new List<int> { 3, 101 } });
        }
        
        private void AddRune(SkillRune rune) => _runes[rune.Id] = rune;
        private void AddCombo(SkillCombo combo) => _combos[combo.Id] = combo;
        
        #endregion
        
        #region Mastery Methods (Delegates to Progression System)
        
        public SkillMastery GetMastery(int skillId)
        {
            if (!_masteries.ContainsKey(skillId))
                _masteries[skillId] = new SkillMastery { SkillId = skillId };
            return _masteries[skillId];
        }
        
        public void AddMasteryXP(int skillId, int xp)
        {
            var mastery = GetMastery(skillId);
            int oldLevel = mastery.CurrentLevel;
            MasteryRank oldRank = mastery.Rank;
            
            mastery.CurrentXP += xp;
            mastery.TotalXP += xp;
            TotalMasteryXP += xp;
            
            CheckMasteryLevelUp(mastery);
            
            if ((int)mastery.Rank > HighestMasteryRank)
                HighestMasteryRank = (int)mastery.Rank;
            
            // Emit signals
            EmitSignal(nameof(MasteryXPChanged), skillId, mastery.CurrentXP, mastery.TotalXP);
            
            if (mastery.CurrentLevel > oldLevel)
            {
                EmitSignal(nameof(MasteryLevelUp), skillId, mastery.CurrentLevel, mastery.Rank);
            }
            
            // Also delegate to progression system if available
            _progressionSystem?.AddXp("default", skillId.ToString(), xp);
            
            GD.Print($"Skill {skillId} mastery: {mastery.CurrentXP} XP, Level {mastery.CurrentLevel}, Rank {mastery.Rank}");
        }
        
        private void CheckMasteryLevelUp(SkillMastery mastery)
        {
            int[] levelUpXP = { 100, 400, 1000, 3500 };
            
            while (mastery.CurrentLevel < 10)
            {
                int threshold = levelUpXP[Math.Min(mastery.CurrentLevel - 1, 3)];
                if (mastery.CurrentXP >= threshold)
                {
                    mastery.CurrentLevel++;
                    ApplyMasteryBonuses(mastery);
                    
                    if (mastery.CurrentLevel == 3) mastery.RuneSlots = 1;
                    if (mastery.CurrentLevel == 6) mastery.RuneSlots = 2;
                    if (mastery.CurrentLevel == 9) mastery.RuneSlots = 3;
                }
                else break;
            }
            
            if (mastery.TotalXP >= 5000) mastery.Rank = MasteryRank.GrandMaster;
            else if (mastery.TotalXP >= 1500) mastery.Rank = MasteryRank.Master;
            else if (mastery.TotalXP >= 500) mastery.Rank = MasteryRank.Expert;
            else if (mastery.TotalXP >= 100) mastery.Rank = MasteryRank.Apprentice;
            else mastery.Rank = MasteryRank.Novice;
        }
        
        private void ApplyMasteryBonuses(SkillMastery mastery)
        {
            mastery.DamageBonus = mastery.CurrentLevel * 0.05f;
            mastery.CooldownReduction = Math.Min(0.30f, mastery.CurrentLevel * 0.03f);
            mastery.RangeBonus = mastery.CurrentLevel * 0.02f;
            mastery.CostReduction = mastery.CurrentLevel * 0.02f;
        }
        
        #endregion
        
        #region Rune Methods
        
        public bool EquipRune(int skillId, int slot, int runeId)
        {
            var mastery = GetMastery(skillId);
            if (slot < 0 || slot >= mastery.RuneSlots) return false;
            if (!_runes.ContainsKey(runeId)) return false;
            
            mastery.EquippedRunes[slot] = runeId;
            
            EmitSignal(nameof(RuneEquipped), skillId, slot, runeId);
            return true;
        }
        
        public bool UnequipRune(int skillId, int slot)
        {
            var mastery = GetMastery(skillId);
            if (mastery.EquippedRunes.ContainsKey(slot))
            {
                mastery.EquippedRunes.Remove(slot);
                EmitSignal(nameof(RuneUnequipped), skillId, slot);
                return true;
            }
            return false;
        }
        
        public (float damage, float cdr, float range, float cost) GetTotalBonuses(int skillId)
        {
            var mastery = GetMastery(skillId);
            float damage = mastery.DamageBonus;
            float cdr = mastery.CooldownReduction;
            float range = mastery.RangeBonus;
            float cost = mastery.CostReduction;
            
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
        
        public bool StartCombo(int skillId, int playerMana)
        {
            foreach (var combo in _combos.Values)
            {
                if (combo.SkillSequence.Count > 0 && combo.SkillSequence[0] == skillId)
                {
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
                    
                    EmitSignal(nameof(ComboStarted), combo.Id, combo.Name);
                    return true;
                }
            }
            return false;
        }
        
        public bool ContinueCombo(int skillId, int playerMana)
        {
            if (_activeCombo == null || _activeCombo.IsComplete) return false;
            
            var combo = _combos[_activeCombo.ComboId];
            int expectedSkillId = combo.SkillSequence[_activeCombo.CurrentStep + 1];
            
            if (skillId != expectedSkillId)
            {
                if (combo.Type == ComboType.Parallel)
                {
                    if (!combo.SkillSequence.Contains(skillId)) return false;
                    for (int i = 0; i <= _activeCombo.CurrentStep; i++)
                        if (combo.SkillSequence[i] == skillId) return false;
                }
                else 
                {
                    CancelCombo();
                    EmitSignal(nameof(ComboFailed), combo.Id, "Wrong skill order");
                    return false; 
                }
            }
            
            if (playerMana < combo.ManaCost) 
            { 
                CancelCombo();
                EmitSignal(nameof(ComboFailed), combo.Id, "Insufficient mana");
                return false; 
            }
            
            _activeCombo.CurrentStep++;
            _activeCombo.TimeRemaining = combo.TimeWindow;
            
            if (_activeCombo.CurrentStep >= combo.SkillSequence.Count - 1)
            {
                CompleteCombo();
                return true;
            }
            return true;
        }
        
        private bool HasComboRequirements(SkillCombo combo)
        {
            var skillManager = new SkillManager();
            foreach (var reqSkillId in combo.RequiredSkillIds)
                if (!skillManager.HasLearned(reqSkillId)) return false;
            return true;
        }
        
        private bool IsComboOnCooldown(int comboId) => _comboCooldowns.ContainsKey(comboId) && _comboCooldowns[comboId] > 0;
        
        private void CompleteCombo()
        {
            if (_activeCombo == null) return;
            
            var combo = _combos[_activeCombo.ComboId];
            _activeCombo.IsComplete = true;
            _comboCooldowns[_activeCombo.ComboId] = combo.Cooldown;
            
            if (!ComboUsages.ContainsKey(_activeCombo.ComboId))
                ComboUsages[_activeCombo.ComboId] = 0;
            ComboUsages[_activeCombo.ComboId]++;
            
            int xpGain = 50 * combo.SkillSequence.Count;
            foreach (var skillId in combo.SkillSequence)
                AddMasteryXP(skillId, xpGain);
            
            EmitSignal(nameof(ComboCompleted), combo.Id, combo.Name, combo.DamageMultiplier);
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
        
        public ActiveCombo GetActiveCombo() => _activeCombo;
        
        public List<SkillCombo> GetAvailableCombos()
        {
            var result = new List<SkillCombo>();
            var skillManager = new SkillManager();
            foreach (var combo in _combos.Values)
                if (HasComboRequirements(combo)) result.Add(combo);
            return result;
        }
        
        public SkillCombo GetCombo(int comboId) => _combos.ContainsKey(comboId) ? _combos[comboId] : null;
        
        #endregion
        
        #region Update
        
        public void Update(float delta)
        {
            if (_activeCombo != null)
            {
                _activeCombo.TimeRemaining -= delta;
                if (_activeCombo.TimeRemaining <= 0) 
                {
                    CancelCombo();
                    EmitSignal(nameof(ComboFailed), _activeCombo.ComboId, "Time expired");
                }
            }
            
            foreach (var key in _comboCooldowns.Keys)
                _comboCooldowns[key] = Math.Max(0, _comboCooldowns[key] - delta);
        }
        
        #endregion
        
        #region Save/Load
        
        public override Dictionary ExportSaveData()
        {
            var data = new Dictionary();
            
            // Mastery data
            var masteriesArray = new Array();
            foreach (var kvp in _masteries)
            {
                var m = new Dictionary
                {
                    ["skillId"] = kvp.Key,
                    ["currentLevel"] = kvp.Value.CurrentLevel,
                    ["currentXP"] = kvp.Value.CurrentXP,
                    ["totalXP"] = kvp.Value.TotalXP,
                    ["rank"] = (int)kvp.Value.Rank,
                    ["runeSlots"] = kvp.Value.RuneSlots,
                    ["equippedRunes"] = kvp.Value.EquippedRunes,
                    ["damageBonus"] = kvp.Value.DamageBonus,
                    ["cooldownReduction"] = kvp.Value.CooldownReduction,
                    ["rangeBonus"] = kvp.Value.RangeBonus,
                    ["costReduction"] = kvp.Value.CostReduction
                };
                masteriesArray.Add(m);
            }
            data["masteries"] = masteriesArray;
            
            // Combo usages
            var comboUsagesArray = new Array();
            foreach (var kvp in ComboUsages)
            {
                comboUsagesArray.Add(new Dictionary { ["comboId"] = kvp.Key, ["usages"] = kvp.Value });
            }
            data["comboUsages"] = comboUsagesArray;
            
            data["totalMasteryXP"] = TotalMasteryXP;
            data["highestMasteryRank"] = HighestMasteryRank;
            
            return data;
        }
        
        public override void ImportSaveData(Dictionary data)
        {
            if (data == null) return;
            
            // Import masteries
            if (data.TryGetValue("masteries", out var m))
            {
                _masteries.Clear();
                var arr = m as Array;
                if (arr != null)
                {
                    foreach (Dictionary entry in arr)
                    {
                        int skillId = Convert.ToInt32(entry["skillId"]);
                        _masteries[skillId] = new SkillMastery
                        {
                            SkillId = skillId,
                            CurrentLevel = Convert.ToInt32(entry["currentLevel"]),
                            CurrentXP = Convert.ToInt32(entry["currentXP"]),
                            TotalXP = Convert.ToInt32(entry["totalXP"]),
                            Rank = (MasteryRank)Convert.ToInt32(entry["rank"]),
                            RuneSlots = Convert.ToInt32(entry["runeSlots"]),
                            EquippedRunes = new Dictionary<int, int>(),
                            DamageBonus = Convert.ToSingle(entry["damageBonus"]),
                            CooldownReduction = Convert.ToSingle(entry["cooldownReduction"]),
                            RangeBonus = Convert.ToSingle(entry["rangeBonus"]),
                            CostReduction = Convert.ToSingle(entry["costReduction"])
                        };
                        
                        if (entry.Contains("equippedRunes"))
                        {
                            var runesDict = entry["equippedRunes"] as Dictionary<object, object>;
                            if (runesDict != null)
                            {
                                foreach (var runeKvp in runesDict)
                                {
                                    _masteries[skillId].EquippedRunes[Convert.ToInt32(runeKvp.Key)] = Convert.ToInt32(runeKvp.Value);
                                }
                            }
                        }
                    }
                }
            }
            
            // Import combo usages
            if (data.TryGetValue("comboUsages", out var cu))
            {
                ComboUsages.Clear();
                var arr = cu as Array;
                if (arr != null)
                {
                    foreach (Dictionary entry in arr)
                    {
                        int comboId = Convert.ToInt32(entry["comboId"]);
                        ComboUsages[comboId] = Convert.ToInt32(entry["usages"]);
                    }
                }
            }
            
            if (data.TryGetValue("totalMasteryXP", out var tm))
                TotalMasteryXP = Convert.ToInt32(tm);
            if (data.TryGetValue("highestMasteryRank", out var hmr))
                HighestMasteryRank = Convert.ToInt32(hmr);
        }
        
        #endregion
        
        #region Public API
        
        public List<SkillRune> GetRunesByType(RuneSlotType type)
        {
            var result = new List<SkillRune>();
            foreach (var rune in _runes.Values)
                if (rune.SlotType == type) result.Add(rune);
            return result;
        }
        
        public SkillRune GetRune(int runeId) => _runes.ContainsKey(runeId) ? _runes[runeId] : null;
        
        public List<SkillCombo> GetAllCombos() => new List<SkillCombo>(_combos.Values);
        
        public Dictionary<int, SkillMastery> GetAllMasteries() => _masteries;
        
        /// <summary>
        /// 获取进度系统引用 (用于委托)
        /// </summary>
        public SkillProgressionSystem GetProgressionSystem() => _progressionSystem;
        
        /// <summary>
        /// 获取技能树系统引用 (用于委托)
        /// </summary>
        public SkillTreeSystem GetTreeSystem() => _treeSystem;
        
        /// <summary>
        /// 获取解锁系统引用 (用于委托)
        /// </summary>
        public SkillUnlockSystem GetUnlockSystem() => _unlockSystem;
        
        #endregion
    }
}
