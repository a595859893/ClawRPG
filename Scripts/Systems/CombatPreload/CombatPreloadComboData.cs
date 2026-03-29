using Godot;
using System;
using System.Collections.Generic;

namespace ClawRPG.Scripts.Systems.CombatPreload
{
    /// <summary>
    /// 战斗前Combo预览数据结构
    /// </summary>
    public class CombatPreloadComboEntry
    {
        public string ComboId;
        public string ComboName;
        public string Description;
        public List<string> SkillSequence = new List<string>();
        public float DamageMultiplier;
        public int ComboPointReward;
        public string EffectName;
        public CombatPreloadComboType ComboType;
        public CombatPreloadComboRarity Rarity;
        public int RequiredComboLevel;
        public bool IsUnlocked;
        public int CurrentProgress; // 0 = not started, n = completed n steps
        
        // REQ-128: 疲劳状态（来自 ComboFatigueSystem）
        public string FatigueStatus = "Fresh";       // Fresh / Slightly Familiar / Adapted / Highly Adapted / Fully Adapted
        public float FatigueLevel = 0f;              // 0.0–1.0，1.0 = 完全疲劳
        public float EffectiveDamageMultiplier;      // base DamageMultiplier × fatigue multiplier
        public Color FatigueColor = new Color(0.3f, 1f, 0.3f); // UI 颜色
    }

    public enum CombatPreloadComboType
    {
        Offensive,
        Defensive,
        Support,
        Utility,
        Special
    }

    public enum CombatPreloadComboRarity
    {
        Common,
        Uncommon,
        Rare,
        Epic,
        Legendary
    }

    /// <summary>
    /// 预览面板状态
    /// </summary>
    public enum CombatPreloadState
    {
        Hidden,
        Showing,
        CountingDown, // REQ-121: 确认后倒计时中，可换Combo（消耗Combo Point）
        Confirmed,
        Cancelled
    }
}
