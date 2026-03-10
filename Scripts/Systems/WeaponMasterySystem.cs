using Godot;
using System;
using System.Collections.Generic;

namespace ClawRPG.Scripts.Systems
{
    /// <summary>
    /// Weapon type enumeration
    /// </summary>
    public enum WeaponType
    {
        Sword,
        Axe,
        Dagger,
        Staff,
        Bow,
        Hammer,
        Shield
    }

    /// <summary>
    /// Weapon mastery data for a single weapon type
    /// </summary>
    [System.Serializable]
    public class WeaponMasteryData
    {
        public WeaponType Type;
        public int Level = 1;
        public int Experience;
        
        // Damage bonus per level: +5%
        public float DamageBonus => (Level - 1) * 0.05f;
        
        // Experience needed for next level: 100 * level
        public int ExperienceForNextLevel => Level * 100;
        
        public void AddExperience(int amount)
        {
            Experience += amount;
            while (Experience >= ExperienceForNextLevel && Level < 20)
            {
                Experience -= ExperienceForNextLevel;
                Level++;
            }
        }
    }

    /// <summary>
    /// Special attack types
    /// </summary>
    public enum SpecialAttackType
    {
        None,
        HeavyStrike,    // 重击 - 按住攻击键蓄力
        QuickSlash,    // 快速斩 - 双击攻击键
        SpinAttack,    // 旋风斩 - 范围攻击
        Charge         // 冲锋 - 快速接近敌人
    }

    /// <summary>
    /// Weapon mastery and special attack system
    /// </summary>
    public partial class WeaponMasterySystem : Node
    {
        public static WeaponMasterySystem Instance { get; private set; }

        // Weapon masteries by type
        public Dictionary<WeaponType, WeaponMasteryData> Masteries { get; private set; } = new();

        // Current weapon type
        public WeaponType CurrentWeaponType { get; private set; } = WeaponType.Sword;

        // Special attack state
        public bool IsChargingHeavyStrike { get; private set; }
        public float ChargeTime { get; private set; }
        public float MaxChargeTime = 1.0f;  // Max charge duration
        
        // Heavy strike damage multiplier based on charge
        public float HeavyStrikeDamageMultiplier => 1.0f + (ChargeTime / MaxChargeTime) * 1.0f;  // 1x to 2x
        
        // Quick slash combo
        public int QuickSlashComboCount { get; private set; }
        public float QuickSlashCooldown { get; private set; }
        
        // Spin attack
        public bool IsSpinAttacking { get; private set; }
        public float SpinAttackDuration { get; private set; } = 0.5f;
        public float SpinAttackCooldown { get; private set; } = 3.0f;
        
        // Charge attack
        public bool IsCharging { get; private set; }
        public Vector2 ChargeDirection { get; private set; }
        public float ChargeDuration { get; private set; } = 0.3f;
        public float ChargeCooldown { get; private set; } = 2.0f;
        
        // Skill requirements
        public int MinSkillLevelForHeavyStrike = 5;
        public int MinSkillLevelForQuickSlash = 10;
        public int MinSkillLevelForSpinAttack = 15;
        public int MinSkillLevelForCharge = 20;

        public override void _Ready()
        {
            Instance = this;
            
            // Initialize masteries for all weapon types
            foreach (WeaponType type in Enum.GetValues(typeof(WeaponType)))
            {
                Masteries[type] = new WeaponMasteryData { Type = type };
            }
        }

        public override void _Process(double delta)
        {
            float dt = (float)delta;
            
            // Update charge time
            if (IsChargingHeavyStrike)
            {
                ChargeTime = Mathf.Min(ChargeTime + dt, MaxChargeTime);
            }
            
            // Update quick slash cooldown
            if (QuickSlashCooldown > 0)
            {
                QuickSlashCooldown -= dt;
            }
            
            // Update spin attack
            if (IsSpinAttacking)
            {
                SpinAttackDuration -= dt;
                if (SpinAttackDuration <= 0)
                {
                    IsSpinAttacking = false;
                    SpinAttackDuration = 0.5f;
                }
            }
            
            // Update charge cooldown
            if (ChargeCooldown > 0)
            {
                ChargeCooldown -= dt;
            }
        }

        /// <summary>
        /// Get damage bonus from weapon mastery
        /// </summary>
        public float GetMasteryDamageBonus(WeaponType type)
        {
            if (Masteries.TryGetValue(type, out var mastery))
            {
                return mastery.DamageBonus;
            }
            return 0f;
        }

        /// <summary>
        /// Add experience to weapon mastery
        /// </summary>
        public void AddMasteryExperience(WeaponType type, int amount)
        {
            if (Masteries.TryGetValue(type, out var mastery))
            {
                int oldLevel = mastery.Level;
                mastery.AddExperience(amount);
                
                // Notify if leveled up
                if (mastery.Level > oldLevel)
                {
                    GD.Print($"Weapon mastery increased! {type}: Level {oldLevel} -> {mastery.Level}");
                }
            }
        }

        /// <summary>
        /// Start charging heavy strike
        /// </summary>
        public void StartHeavyStrikeCharge()
        {
            if (!IsChargingHeavyStrike)
            {
                IsChargingHeavyStrike = true;
                ChargeTime = 0f;
            }
        }

        /// <summary>
        /// Release heavy strike (call when releasing attack key)
        /// </summary>
        public SpecialAttackType ReleaseHeavyStrike()
        {
            if (IsChargingHeavyStrike && ChargeTime >= 0.3f)  // Minimum charge time
            {
                IsChargingHeavyStrike = false;
                float multiplier = HeavyStrikeDamageMultiplier;
                ChargeTime = 0f;
                return SpecialAttackType.HeavyStrike;
            }
            IsChargingHeavyStrike = false;
            ChargeTime = 0f;
            return SpecialAttackType.None;
        }

        /// <summary>
        /// Attempt quick slash (double tap attack)
        /// </summary>
        public SpecialAttackType TryQuickSlash()
        {
            if (QuickSlashCooldown <= 0)
            {
                QuickSlashCooldown = 0.5f;
                QuickSlashComboCount++;
                if (QuickSlashComboCount > 3) QuickSlashComboCount = 1;
                return SpecialAttackType.QuickSlash;
            }
            return SpecialAttackType.None;
        }

        /// <summary>
        /// Reset quick slash combo
        /// </summary>
        public void ResetQuickSlashCombo()
        {
            QuickSlashComboCount = 0;
        }

        /// <summary>
        /// Perform spin attack
        /// </summary>
        public bool TrySpinAttack()
        {
            if (!IsSpinAttacking && SpinAttackCooldown <= 0)
            {
                IsSpinAttacking = true;
                SpinAttackDuration = 0.5f;
                SpinAttackCooldown = 3.0f;
                return true;
            }
            return false;
        }

        /// <summary>
        /// Perform charge attack
        /// </summary>
        public bool TryChargeAttack(Vector2 direction)
        {
            if (!IsCharging && ChargeCooldown <= 0 && direction.Length() > 0.1f)
            {
                IsCharging = true;
                ChargeDirection = direction.Normalized();
                ChargeDuration = 0.3f;
                ChargeCooldown = 2.0f;
                return true;
            }
            return false;
        }

        /// <summary>
        /// Switch weapon type
        /// </summary>
        public void SwitchWeapon(WeaponType type)
        {
            CurrentWeaponType = type;
            GD.Print($"Switched to weapon: {type}");
        }

        /// <summary>
        /// Get current weapon mastery level
        /// </summary>
        public int GetCurrentMasteryLevel()
        {
            if (Masteries.TryGetValue(CurrentWeaponType, out var mastery))
            {
                return mastery.Level;
            }
            return 1;
        }

        /// <summary>
        /// Get current weapon experience progress (0-1)
        /// </summary>
        public float GetCurrentMasteryProgress()
        {
            if (Masteries.TryGetValue(CurrentWeaponType, out var mastery))
            {
                return (float)mastery.Experience / mastery.ExperienceForNextLevel;
            }
            return 0f;
        }

        /// <summary>
        /// Serialize mastery data for saving
        /// </summary>
        public Dictionary<string, object> Serialize()
        {
            var data = new Dictionary<string, object>();
            data["currentWeapon"] = (int)CurrentWeaponType;
            
            var masteryList = new List<Dictionary<string, object>>();
            foreach (var kvp in Masteries)
            {
                masteryList.Add(new Dictionary<string, object>
                {
                    {"type", (int)kvp.Key},
                    {"level", kvp.Value.Level},
                    {"experience", kvp.Value.Experience}
                });
            }
            data["masteries"] = masteryList;
            
            return data;
        }

        /// <summary>
        /// Deserialize mastery data from save
        /// </summary>
        public void Deserialize(Dictionary<string, object> data)
        {
            if (data.ContainsKey("currentWeapon"))
            {
                CurrentWeaponType = (WeaponType)(int)data["currentWeapon"];
            }
            
            if (data.ContainsKey("masteries"))
            {
                var masteryList = (List<object>)data["masteries"];
                foreach (var masteryData in masteryList)
                {
                    var dict = (Dictionary<string, object>)masteryData;
                    var type = (WeaponType)(int)dict["type"];
                    var level = (int)dict["level"];
                    var experience = (int)dict["experience"];
                    
                    if (Masteries.ContainsKey(type))
                    {
                        Masteries[type].Level = level;
                        Masteries[type].Experience = experience;
                    }
                }
            }
        }
    }
}
