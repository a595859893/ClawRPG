using Godot;
using System;
using System.Collections.Generic;

namespace ClawRPG.Systems.CombatUI
{
    /// <summary>
    /// Combat UI Enhancement System - 战斗UI增强系统
    /// Provides dynamic health bars, skill cooldown animations, combat state indicators, combo counters
    /// 应用 Advanced Shader Effects 学习的视觉增强技术
    /// </summary>
    public partial class CombatUIEnhancementData : Resource
    {
        [Export] public bool Enabled { get; set; } = true;
        
        // Dynamic Health Bar Settings
        [Export] public float HealthBarWidth { get; set; } = 200f;
        [Export] public float HealthBarHeight { get; set; } = 20f;
        [Export] public Color HealthBarColor { get; set; } = new Color(0.2f, 0.8f, 0.2f);
        [Export] public Color HealthBarCriticalColor { get; set; } = new Color(0.9f, 0.2f, 0.2f);
        [Export] public Color HealthBarLowColor { get; set; } = new Color(0.9f, 0.6f, 0.2f);
        
        // Skill Cooldown Settings
        [Export] public float CooldownAnimationSpeed { get; set; } = 1.0f;
        [Export] public bool CooldownSpiralEnabled { get; set; } = true;
        [Export] public bool CooldownNumberEnabled { get; set; } = true;
        
        // Combat State Indicator Settings
        [Export] public float IndicatorPulseSpeed { get; set; } = 2.0f;
        [Export] public bool StatusEffectsEnabled { get; set; } = true;
        
        // Combo Counter Settings
        [Export] public float ComboCounterScale { get; set; } = 1.0f;
        [Export] public float ComboTimeout { get; set; } = 3.0f;
        [Export] public bool ComboShakeEnabled { get; set; } = true;
        
        // Screen Effects Integration
        [Export] public bool ScreenFlashOnCritical { get; set; } = true;
        [Export] public bool ScreenShakeOnHit { get; set; } = true;
        [Export] public float ScreenShakeIntensity { get; set; } = 0.5f;
        
        // Statistics
        [Export] public int TotalCombosTriggered { get; set; } = 0;
        [Export] public int HighestComboCount { get; set; } = 0;
        [Export] public int TotalCriticals { get; set; } = 0;
        [Export] public float TotalDamageMitigated { get; set; } = 0f;
    }
    
    public partial class CombatStateData : Resource
    {
        public enum CombatState
        {
            Idle,
            Fighting,
            Defending,
            Casting,
            Stunned,
            Dodging,
            Countering
        }
        
        public CombatState CurrentState { get; set; } = CombatState.Idle;
        public float StateTimer { get; set; } = 0f;
        public int ComboCount { get; set; } = 0;
        public float LastHitTime { get; set; } = 0f;
        public bool IsCritical { get; set; } = false;
        public bool IsBlocking { get; set; } = false;
        public bool IsDodging { get; set; } = false;
    }
    
    public partial class SkillCooldownData : Resource
    {
        public string SkillId { get; set; } = "";
        public string SkillName { get; set; } = "";
        public float CurrentCooldown { get; set; } = 0f;
        public float MaxCooldown { get; set; } = 0f;
        public bool IsReady => CurrentCooldown <= 0f;
        
        public float GetCooldownPercent() => MaxCooldown > 0 ? CurrentCooldown / MaxCooldown : 0f;
    }
    
    public partial class StatusEffectData : Resource
    {
        public enum EffectType
        {
            Poison,
            Burn,
            Freeze,
            Stun,
            Slow,
            Bleed,
            Blind,
            Silence,
            Taunt,
            Shield
        }
        
        public EffectType Type { get; set; }
        public string DisplayName { get; set; } = "";
        public float Duration { get; set; } = 0f;
        public float RemainingTime { get; set; } = 0f;
        public float Intensity { get; set; } = 1.0f;
        public Color EffectColor { get; set; } = Colors.White;
        
        public float GetRemainingPercent() => Duration > 0 ? RemainingTime / Duration : 0f;
    }
}
