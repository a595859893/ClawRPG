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
        Confirmed,
        Cancelled
    }
}
