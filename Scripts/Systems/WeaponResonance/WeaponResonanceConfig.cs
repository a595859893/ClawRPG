using System;
using System.Collections.Generic;
using ClawRPG.Scripts.Systems;

namespace ClawRPG.Scripts.Systems.WeaponResonance
{
    /// <summary>
    /// 共鸣效果配置 — 数据驱动，每个武器类型定义自己的共鸣效果
    /// </summary>
    [System.Serializable]
    public class ResonanceEffect
    {
        /// <summary>共鸣名称（如"双剑流"、"双斧狂暴"）</summary>
        public string Name { get; set; } = "";

        /// <summary>攻击速度加成（如0.15 = +15%）</summary>
        public float AttackSpeedBonus { get; set; }

        /// <summary>暴击率加成（如0.10 = +10%）</summary>
        public float CritBonus { get; set; }

        /// <summary>暴击伤害加成（如0.25 = +25%）</summary>
        public float CritDamageBonus { get; set; }

        /// <summary>额外效果名称（如"ThirdStrike"表示第3次攻击触发额外斩击）</summary>
        public string ExtraEffectName { get; set; } = "";

        /// <summary>伤害倍率加成（如0.20 = +20%）</summary>
        public float DamageBonus { get; set; }

        /// <summary>描述文本（用于UI显示）</summary>
        public string Description { get; set; } = "";
    }

    /// <summary>
    /// 武器共鸣配置 — 所有武器类型的共鸣效果定义
    /// </summary>
    public static class WeaponResonanceConfig
    {
        /// <summary>
        /// WeaponType 到共鸣效果的映射
        /// Key 使用 WeaponMasterySystem.WeaponType 枚举名（字符串）
        /// </summary>
        public static readonly Dictionary<string, ResonanceEffect> ResonanceByType = new()
        {
            // 双剑：攻击速度+15%，第3次攻击额外触发斩击
            ["Sword"] = new ResonanceEffect
            {
                Name = "双剑流",
                AttackSpeedBonus = 0.15f,
                CritBonus = 0.0f,
                CritDamageBonus = 0.0f,
                DamageBonus = 0.0f,
                ExtraEffectName = "ThirdStrike",
                Description = "攻击速度+15%，第3次攻击额外触发斩击"
            },

            // 双斧：暴击伤害+25%，暴击率+10%
            ["Axe"] = new ResonanceEffect
            {
                Name = "双斧狂暴",
                AttackSpeedBonus = 0.0f,
                CritBonus = 0.10f,
                CritDamageBonus = 0.25f,
                DamageBonus = 0.0f,
                ExtraEffectName = "",
                Description = "暴击率+10%，暴击伤害+25%"
            },

            // 双匕首：背刺伤害+40%，闪避后下一次攻击必暴击
            ["Dagger"] = new ResonanceEffect
            {
                Name = "双刃舞",
                AttackSpeedBonus = 0.10f,
                CritBonus = 0.05f,
                CritDamageBonus = 0.0f,
                DamageBonus = 0.15f,
                ExtraEffectName = "BackstabBonus",
                Description = "背刺伤害+40%，闪避后下一次攻击必暴击"
            },

            // 双杖：法术伤害+20%，冷却速度+10%
            ["Staff"] = new ResonanceEffect
            {
                Name = "双杖奥术",
                AttackSpeedBonus = 0.0f,
                CritBonus = 0.0f,
                CritDamageBonus = 0.0f,
                DamageBonus = 0.20f,
                ExtraEffectName = "CooldownReduction",
                Description = "法术伤害+20%，冷却速度+10%"
            },

            // 双弓：射击速度+20%，射击间隔-15%
            ["Bow"] = new ResonanceEffect
            {
                Name = "双弓连射",
                AttackSpeedBonus = 0.20f,
                CritBonus = 0.05f,
                CritDamageBonus = 0.10f,
                DamageBonus = 0.0f,
                ExtraEffectName = "",
                Description = "射击速度+20%，暴击率+5%，暴击伤害+10%"
            },

            // 双锤：攻击速度-10%但伤害+30%，击退效果翻倍
            ["Hammer"] = new ResonanceEffect
            {
                Name = "双锤粉碎",
                AttackSpeedBonus = -0.10f,
                CritBonus = 0.0f,
                CritDamageBonus = 0.0f,
                DamageBonus = 0.30f,
                ExtraEffectName = "DoubleKnockback",
                Description = "攻击速度-10%但伤害+30%，击退效果翻倍"
            },

            // 双盾：防御+30%，格挡效率+20%（虽然是防御装备，但可作为副手）
            ["Shield"] = new ResonanceEffect
            {
                Name = "双盾壁垒",
                AttackSpeedBonus = 0.0f,
                CritBonus = 0.0f,
                CritDamageBonus = 0.0f,
                DamageBonus = 0.0f,
                ExtraEffectName = "BlockEfficiency",
                Description = "防御+30%，格挡效率+20%"
            }
        };

        /// <summary>
        /// 根据武器类型获取共鸣效果（无配置返回 null）
        /// </summary>
        public static ResonanceEffect GetEffect(string weaponTypeName)
        {
            if (ResonanceByType.TryGetValue(weaponTypeName, out var effect))
                return effect;
            return null;
        }

        /// <summary>
        /// 根据 WeaponType 获取共鸣效果
        /// </summary>
        public static ResonanceEffect GetEffect(WeaponType type)
        {
            return GetEffect(type.ToString());
        }
    }
}
