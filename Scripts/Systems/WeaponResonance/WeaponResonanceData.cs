using System;
using Godot;

namespace ClawRPG.Scripts.Systems.WeaponResonance
{
    /// <summary>
    /// 武器共鸣运行时数据 — 跟踪当前共鸣激活状态
    /// </summary>
    [System.Serializable]
    public class WeaponResonanceData
    {
        /// <summary>当前激活的武器类型（字符串名）</summary>
        public string ActiveWeaponType { get; set; } = "";

        /// <summary>共鸣是否激活</summary>
        public bool IsActive { get; set; }

        /// <summary>共鸣激活持续时间（秒）</summary>
        public float ActiveDuration { get; set; }

        /// <summary>共鸣激活的时间戳（用于调试/记录）</summary>
        public float ActivatedAt { get; set; }

        /// <summary>
        /// 激活指定类型的共鸣
        /// </summary>
        public void Activate(string weaponType)
        {
            ActiveWeaponType = weaponType;
            IsActive = true;
            ActivatedAt = Time.GetTicksMsec() / 1000f;
            ActiveDuration = 0f;
        }

        /// <summary>
        /// 停用共鸣
        /// </summary>
        public void Deactivate()
        {
            IsActive = false;
            ActiveWeaponType = "";
            ActiveDuration = 0f;
        }

        /// <summary>
        /// 更新共鸣持续时间（在 _Process 中调用）
        /// </summary>
        public void Update(double delta)
        {
            if (IsActive)
            {
                ActiveDuration += (float)delta;
            }
        }

        /// <summary>
        /// 获取当前共鸣效果配置
        /// </summary>
        public ResonanceEffect GetActiveEffect()
        {
            if (!IsActive || string.IsNullOrEmpty(ActiveWeaponType))
                return null;
            return WeaponResonanceConfig.GetEffect(ActiveWeaponType);
        }

        /// <summary>
        /// 获取伤害加成（直接用于战斗计算）
        /// </summary>
        public float GetDamageBonus()
        {
            var effect = GetActiveEffect();
            return effect?.DamageBonus ?? 0f;
        }

        /// <summary>
        /// 获取暴击率加成
        /// </summary>
        public float GetCritBonus()
        {
            var effect = GetActiveEffect();
            return effect?.CritBonus ?? 0f;
        }

        /// <summary>
        /// 获取暴击伤害加成
        /// </summary>
        public float GetCritDamageBonus()
        {
            var effect = GetActiveEffect();
            return effect?.CritDamageBonus ?? 0f;
        }

        /// <summary>
        /// 获取攻击速度加成
        /// </summary>
        public float GetAttackSpeedBonus()
        {
            var effect = GetActiveEffect();
            return effect?.AttackSpeedBonus ?? 0f;
        }

        /// <summary>
        /// 获取共鸣名称
        /// </summary>
        public string GetResonanceName()
        {
            var effect = GetActiveEffect();
            return effect?.Name ?? "";
        }

        /// <summary>
        /// 获取共鸣描述
        /// </summary>
        public string GetResonanceDescription()
        {
            var effect = GetActiveEffect();
            return effect?.Description ?? "";
        }
    }
}
