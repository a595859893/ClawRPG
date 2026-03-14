using System;
using System.Collections.Generic;

namespace ClawRPG.Scripts.Combat
{
    /// <summary>
    /// Combat UI data structures for polished combat interface
    /// </summary>
    
    // ============================================
    // Damage Display Types
    // ============================================
    
    public enum DamageDisplayType
    {
        Normal,         // White - standard damage
        Critical,       // Gold - critical hits
        Healing,        // Green - healing effects
        Miss,           // Gray - missed attacks
        Blocked,        // Blue - blocked damage
        Absorbed,       // Purple - damage absorption
        DoT,            // Red - damage over time
        TrueDamage      // Orange - true damage (bypasses defenses)
    }
    
    // ============================================
    // Health Bar Styles
    // ============================================
    
    public enum HealthBarStyle
    {
        Classic,        // Traditional red bar
        Gradient,       // Color gradient based on health %
        Pulsing,        // Pulse effect when low health
        Shielded,       // Shows shield overlay
        Animated        // Smooth animation transitions
    }
    
    // ============================================
    // Combat Indicator Types
    // ============================================
    
    public enum CombatIndicatorType
    {
        Damage,         // Damage received
        Healing,        // Health restored
        Buff,           // Buff applied
        Debuff,         // Debuff applied
        Combo,          // Combo counter
        Kill,           // Enemy killed
        BossPhase,      // Boss phase change
        Stun,           // Stun indicator
        Shield,         // Shield break
        Immunity        // Immunity activation
    }
    
    // ============================================
    // Data Structures
    // ============================================
    
    /// <summary>
    /// Represents a damage/healing floating text event
    /// </summary>
    public class DamageTextData
    {
        public float Amount { get; set; }
        public DamageDisplayType DisplayType { get; set; }
        public Vector3 Position { get; set; }
        public float Lifetime { get; set; } = 1.0f;
        public float Scale { get; set; } = 1.0f;
        public bool IsPlayerSource { get; set; }
    }
    
    /// <summary>
    /// Represents a combat indicator icon
    /// </summary>
    public class CombatIndicatorData
    {
        public CombatIndicatorType Type { get; set; }
        public string Message { get; set; }
        public float Duration { get; set; } = 2.0f;
        public string IconPath { get; set; }
        public int Priority { get; set; }
    }
    
    /// <summary>
    /// Health bar configuration
    /// </summary>
    public class HealthBarConfig
    {
        public HealthBarStyle Style { get; set; } = HealthBarStyle.Gradient;
        public float Width { get; set; } = 200f;
        public float Height { get; set; } = 20f;
        public bool ShowNumbers { get; set; } = true;
        public bool ShowPercent { get; set; } = true;
        public bool EnablePulse { get; set; } = true;
        public float PulseThreshold { get; set; } = 0.25f;
    }
    
    /// <summary>
    /// Combat statistics for current session
    /// </summary>
    public class CombatStatistics
    {
        public int TotalDamageDealt { get; set; }
        public int TotalDamageTaken { get; set; }
        public int TotalHealing { get; set; }
        public int EnemiesKilled { get; set; }
        public int CriticalHits { get; set; }
        public int Blocks { get; set; }
        public int Dodges { get; set; }
        public float HighestDamage { get; set; }
        public float SessionDuration { get; set; }
        public float DPS { get; set; }
    }
    
    /// <summary>
    /// Combo chain tracking
    /// </summary>
    public class ComboChainData
    {
        public int CurrentCombo { get; set; }
        public int MaxCombo { get; set; }
        public float ComboTimer { get; set; }
        public float MaxComboTime { get; set; } = 3.0f;
        public int ComboHits { get; set; }
        public float ComboDamage { get; set; }
    }
    
    /// <summary>
    /// UI layout preferences
    /// </summary>
    public class UILayoutPreferences
    {
        public bool ShowDamageNumbers { get; set; } = true;
        public bool ShowHealthBars { get; set; } = true;
        public bool ShowComboCounter { get; set; } = true;
        public bool ShowCombatIndicators { get; set; } = true;
        public bool ShowDPS { get; set; } = false;
        public float UIScale { get; set; } = 1.0f;
        public string DamageNumberPosition { get; set; } = "above_target";
    }
    
    /// <summary>
    /// Player combat state for UI tracking
    /// </summary>
    public class PlayerCombatState
    {
        public float CurrentHealth { get; set; }
        public float MaxHealth { get; set; }
        public float CurrentShield { get; set; }
        public float CurrentEnergy { get; set; }
        public float MaxEnergy { get; set; }
        public List<string> ActiveBuffs { get; set; } = new List<string>();
        public List<string> ActiveDebuffs { get; set; } = new List<string>();
    }
    
    /// <summary>
    /// Enemy combat state for UI tracking
    /// </summary>
    public class EnemyCombatState
    {
        public string EnemyId { get; set; }
        public string EnemyName { get; set; }
        public float CurrentHealth { get; set; }
        public float MaxHealth { get; set; }
        public bool IsBoss { get; set; }
        public int CurrentPhase { get; set; }
        public int TotalPhases { get; set; }
    }
    
    /// <summary>
    /// Damage number animation config
    /// </summary>
    public class DamageNumberConfig
    {
        public float FloatSpeed { get; set; } = 50f;
        public float FadeStart { get; set; } = 0.6f;
        public float ScaleOnHit { get; set; } = 1.3f;
        public float ScaleReturnSpeed { get; set; } = 3.0f;
        public float RotationRandomness { get; set; } = 15f;
    }
    
    /// <summary>
    /// Screen effect triggers
    /// </summary>
    public class ScreenEffectTrigger
    {
        public string EffectName { get; set; }
        public float Intensity { get; set; } = 1.0f;
        public float Duration { get; set; } = 0.5f;
        public bool IsScreenShake { get; set; }
        public bool IsFlash { get; set; }
    }
}
