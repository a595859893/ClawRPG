using System;
using System.Collections.Generic;

namespace ClawRPG.Scripts.Combat
{
    /// <summary>
    /// Combat UI database - configuration for damage numbers, health bars, indicators
    /// </summary>
    public class CombatUIDatabase
    {
        private static CombatUIDatabase _instance;
        public static CombatUIDatabase Instance => _instance ??= new CombatUIDatabase();
        
        // Damage number colors by type
        public Dictionary<DamageDisplayType, string> DamageColors { get; private set; }
        
        // Damage number fonts by type
        public Dictionary<DamageDisplayType, string> DamageFonts { get; private set; }
        
        // Combat indicator messages
        public Dictionary<CombatIndicatorType, List<string>> IndicatorMessages { get; private set; }
        
        // Screen effects configuration
        public Dictionary<string, ScreenEffectConfig> ScreenEffects { get; private set; }
        
        // UI animation presets
        public Dictionary<string, UIAnimationPreset> AnimationPresets { get; private set; }
        
        // Combo milestone rewards/messages
        public List<ComboMilestoneConfig> ComboMilestones { get; private set; }
        
        // Health bar color thresholds
        public List<HealthThresholdConfig> HealthThresholds { get; private set; }
        
        public CombatUIDatabase()
        {
            InitializeDamageColors();
            InitializeDamageFonts();
            InitializeIndicatorMessages();
            InitializeScreenEffects();
            InitializeAnimationPresets();
            InitializeComboMilestones();
            InitializeHealthThresholds();
        }
        
        private void InitializeDamageColors()
        {
            DamageColors = new Dictionary<DamageDisplayType, string>
            {
                { DamageDisplayType.Normal, "#FFFFFF" },
                { DamageDisplayType.Critical, "#FFD700" },
                { DamageDisplayType.Healing, "#00FF7F" },
                { DamageDisplayType.Miss, "#888888" },
                { DamageDisplayType.Blocked, "#4169E1" },
                { DamageDisplayType.Absorbed, "#9370DB" },
                { DamageDisplayType.DoT, "#FF4500" },
                { DamageDisplayType.TrueDamage, "#FF8C00" }
            };
        }
        
        private void InitializeDamageFonts()
        {
            DamageFonts = new Dictionary<DamageDisplayType, string>
            {
                { DamageDisplayType.Normal, "res://fonts/CombatNormal.tres" },
                { DamageDisplayType.Critical, "res://fonts/CombatCritical.tres" },
                { DamageDisplayType.Healing, "res://fonts/CombatHealing.tres" },
                { DamageDisplayType.Miss, "res://fonts/CombatSmall.tres" },
                { DamageDisplayType.Blocked, "res://fonts/CombatNormal.tres" },
                { DamageDisplayType.Absorbed, "res://fonts/CombatNormal.tres" },
                { DamageDisplayType.DoT, "res://fonts/CombatDoT.tres" },
                { DamageDisplayType.TrueDamage, "res://fonts/CombatTrue.tres" }
            };
        }
        
        private void InitializeIndicatorMessages()
        {
            IndicatorMessages = new Dictionary<CombatIndicatorType, List<string>>
            {
                { CombatIndicatorType.Damage, new List<string>
                    {
                        "Hit!", "Smack!", "Whack!", "Pow!", "Take that!",
                        "Critical!", "CRITICAL!", "BAM!", "SLAM!", "DIRECT HIT!"
                    }
                },
                { CombatIndicatorType.Healing, new List<string>
                    {
                        "Healed!", "Mending...", "+HP", "Recovered!", "Restored!",
                        "Full Restore!", "Regenerated!", "Refreshed!", "Renewed!", "Vitalized!"
                    }
                },
                { CombatIndicatorType.Buff, new List<string>
                    {
                        "Powered Up!", "Boosted!", "Enhanced!", "Fortified!",
                        "Accelerated!", "Strengthened!", "Empowered!", "Blessed!"
                    }
                },
                { CombatIndicatorType.Debuff, new List<string>
                    {
                        "Weakened!", "Slowed!", "Stunned!", "Poisoned!",
                        "Cursed!", "Burning!", "Frozen!", "Shocked!"
                    }
                },
                { CombatIndicatorType.Combo, new List<string>
                    {
                        "Combo x{N}!", "x{N} COMBO!", "{N} HITS!",
                        "COMBO!", "KILLER COMBO!", "AMAZING x{N}!"
                    }
                },
                { CombatIndicatorType.Kill, new List<string>
                    {
                        "Eliminated!", "Defeated!", "Slain!", "Finished!",
                        "VICTORY!", "Enemy Down!", "Destroyed!", "terminated"
                    }
                },
                { CombatIndicatorType.BossPhase, new List<string>
                    {
                        "PHASE CHANGE!", "Phase {N}!", "NEW PHASE!",
                        "BERSERK MODE!", "ENRAGED!", "FINAL PHASE!"
                    }
                },
                { CombatIndicatorType.Stun, new List<string>
                    {
                        "STUNNED!", "Knocked Out!", "Dazed!", "Unconscious!",
                        "Staggered!", "Off Balance!", "Winded!"
                    }
                },
                { CombatIndicatorType.Shield, new List<string>
                    {
                        "Shield Broken!", "Barrier Shattered!", "Defense Down!",
                        "Shield Depleted!", "Protection Failed!"
                    }
                },
                { CombatIndicatorType.Immunity, new List<string>
                    {
                        "Immune!", "No Effect!", "Resisted!", "Blocked!",
                        "Invulnerable!", "Protected!", "Deflected!"
                    }
                }
            };
        }
        
        private void InitializeScreenEffects()
        {
            ScreenEffects = new Dictionary<string, ScreenEffectConfig>
            {
                { "light_damage", new ScreenEffectConfig
                    {
                        Name = "light_damage",
                        ScreenShake = true,
                        ShakeIntensity = 2.0f,
                        ShakeDuration = 0.1f,
                        FlashColor = "#FF0000",
                        FlashAlpha = 0.1f,
                        FlashDuration = 0.05f
                    }
                },
                { "heavy_damage", new ScreenEffectConfig
                    {
                        Name = "heavy_damage",
                        ScreenShake = true,
                        ShakeIntensity = 8.0f,
                        ShakeDuration = 0.3f,
                        FlashColor = "#FF0000",
                        FlashAlpha = 0.3f,
                        FlashDuration = 0.15f
                    }
                },
                { "critical_hit", new ScreenEffectConfig
                    {
                        Name = "critical_hit",
                        ScreenShake = true,
                        ShakeIntensity = 5.0f,
                        ShakeDuration = 0.2f,
                        FlashColor = "#FFD700",
                        FlashAlpha = 0.2f,
                        FlashDuration = 0.1f,
                        ChromaticAberration = 2.0f
                    }
                },
                { "healing", new ScreenEffectConfig
                    {
                        Name = "healing",
                        ScreenShake = false,
                        FlashColor = "#00FF7F",
                        FlashAlpha = 0.15f,
                        FlashDuration = 0.2f,
                        VignetteColor = "#00FF7F",
                        VignetteAlpha = 0.1f
                    }
                },
                { "kill_streak", new ScreenEffectConfig
                    {
                        Name = "kill_streak",
                        ScreenShake = true,
                        ShakeIntensity = 3.0f,
                        ShakeDuration = 0.15f,
                        ChromaticAberration = 3.0f,
                        SpeedLines = true,
                        SpeedLineIntensity = 0.5f
                    }
                },
                { "boss_enter", new ScreenEffectConfig
                    {
                        Name = "boss_enter",
                        ScreenShake = true,
                        ShakeIntensity = 10.0f,
                        ShakeDuration = 0.5f,
                        FlashColor = "#FF0000",
                        FlashAlpha = 0.5f,
                        FlashDuration = 0.3f,
                        SlowMotion = true,
                        SlowMotionScale = 0.3f,
                        SlowMotionDuration = 0.5f
                    }
                },
                { "phase_change", new ScreenEffectConfig
                    {
                        Name = "phase_change",
                        ScreenShake = true,
                        ShakeIntensity = 6.0f,
                        ShakeDuration = 0.4f,
                        FlashColor = "#FFFFFF",
                        FlashAlpha = 0.4f,
                        FlashDuration = 0.2f,
                        ChromaticAberration = 5.0f
                    }
                },
                { "perfect_block", new ScreenEffectConfig
                    {
                        Name = "perfect_block",
                        ScreenShake = true,
                        ShakeIntensity = 4.0f,
                        ShakeDuration = 0.1f,
                        FlashColor = "#4169E1",
                        FlashAlpha = 0.25f,
                        FlashDuration = 0.1f
                    }
                }
            };
        }
        
        private void InitializeAnimationPresets()
        {
            AnimationPresets = new Dictionary<string, UIAnimationPreset>
            {
                { "bouncy", new UIAnimationPreset
                    {
                        Name = "bouncy",
                        ScaleInDuration = 0.15f,
                        ScaleInAmount = 1.4f,
                        ScaleOutDuration = 0.3f,
                        ScaleOutTarget = 0.8f,
                        FloatDuration = 0.8f,
                        FloatDistance = 40f,
                        RotationAmount = 10f,
                        EaseType = "elastic"
                    }
                },
                { "smooth", new UIAnimationPreset
                    {
                        Name = "smooth",
                        ScaleInDuration = 0.2f,
                        ScaleInAmount = 1.2f,
                        ScaleOutDuration = 0.4f,
                        ScaleOutTarget = 1.0f,
                        FloatDuration = 1.0f,
                        FloatDistance = 35f,
                        RotationAmount = 5f,
                        EaseType = "sine"
                    }
                },
                { "snappy", new UIAnimationPreset
                    {
                        Name = "snappy",
                        ScaleInDuration = 0.08f,
                        ScaleInAmount = 1.5f,
                        ScaleOutDuration = 0.15f,
                        ScaleOutTarget = 0.9f,
                        FloatDuration = 0.6f,
                        FloatDistance = 30f,
                        RotationAmount = 0f,
                        EaseType = "quad"
                    }
                },
                { "elegant", new UIAnimationPreset
                    {
                        Name = "elegant",
                        ScaleInDuration = 0.25f,
                        ScaleInAmount = 1.1f,
                        ScaleOutDuration = 0.5f,
                        ScaleOutTarget = 1.0f,
                        FloatDuration = 1.2f,
                        FloatDistance = 50f,
                        RotationAmount = 15f,
                        EaseType = "quint"
                    }
                }
            };
        }
        
        private void InitializeComboMilestones()
        {
            ComboMilestones = new List<ComboMilestoneConfig>
            {
                new ComboMilestoneConfig { ComboCount = 5, Message = "Nice Combo!", Color = "#FFFFFF", Scale = 1.2f },
                new ComboMilestoneConfig { ComboCount = 10, Message = "Great Combo!", Color = "#90EE90", Scale = 1.4f },
                new ComboMilestoneConfig { ComboCount = 15, Message = "Awesome!", Color = "#FFD700", Scale = 1.5f },
                new ComboMilestoneConfig { ComboCount = 20, Message = "AMAZING!", Color = "#FFA500", Scale = 1.6f },
                new ComboMilestoneConfig { ComboCount = 25, Message = "UNSTOPPABLE!", Color = "#FF4500", Scale = 1.7f },
                new ComboMilestoneConfig { ComboCount = 30, Message = "LEGENDARY!", Color = "#FF0000", Scale = 1.8f },
                new ComboMilestoneConfig { ComboCount = 40, Message = "GODLIKE!", Color = "#9400D3", Scale = 2.0f },
                new ComboMilestoneConfig { ComboCount = 50, Message = "MEGA LEGEND!", Color = "#00FFFF", Scale = 2.2f }
            };
        }
        
        private void InitializeHealthThresholds()
        {
            HealthThresholds = new List<HealthThresholdConfig>
            {
                new HealthThresholdConfig { Threshold = 1.0f, Color = "#00FF00", GlowIntensity = 0f },
                new HealthThresholdConfig { Threshold = 0.75f, Color = "#7FFF00", GlowIntensity = 0f },
                new HealthThresholdConfig { Threshold = 0.5f, Color = "#FFFF00", GlowIntensity = 0.2f },
                new HealthThresholdConfig { Threshold = 0.25f, Color = "#FF7F00", GlowIntensity = 0.5f },
                new HealthThresholdConfig { Threshold = 0.1f, Color = "#FF0000", GlowIntensity = 0.8f },
                new HealthThresholdConfig { Threshold = 0f, Color = "#8B0000", GlowIntensity = 1.0f }
            };
        }
        
        // Helper methods
        public string GetDamageColor(DamageDisplayType type)
        {
            return DamageColors.TryGetValue(type, out var color) ? color : "#FFFFFF";
        }
        
        public string GetRandomIndicatorMessage(CombatIndicatorType type)
        {
            if (IndicatorMessages.TryGetValue(type, out var messages) && messages.Count > 0)
            {
                var random = new Random();
                return messages[random.Next(messages.Count)];
            }
            return "";
        }
        
        public ScreenEffectConfig GetScreenEffect(string effectName)
        {
            return ScreenEffects.TryGetValue(effectName, out var effect) ? effect : null;
        }
        
        public UIAnimationPreset GetAnimationPreset(string presetName)
        {
            return AnimationPresets.TryGetValue(presetName, out var preset) ? preset : null;
        }
        
        public ComboMilestoneConfig GetComboMilestone(int comboCount)
        {
            ComboMilestoneConfig result = null;
            foreach (var milestone in ComboMilestones)
            {
                if (comboCount >= milestone.ComboCount)
                {
                    result = milestone;
                }
            }
            return result;
        }
        
        public HealthThresholdConfig GetHealthThreshold(float healthPercent)
        {
            foreach (var threshold in HealthThresholds)
            {
                if (healthPercent >= threshold.Threshold)
                {
                    return threshold;
                }
            }
            return HealthThresholds[HealthThresholds.Count - 1];
        }
    }
    
    // ============================================
    // Helper Config Classes
    // ============================================
    
    public class ScreenEffectConfig
    {
        public string Name { get; set; }
        public bool ScreenShake { get; set; }
        public float ShakeIntensity { get; set; }
        public float ShakeDuration { get; set; }
        public string FlashColor { get; set; }
        public float FlashAlpha { get; set; }
        public float FlashDuration { get; set; }
        public float ChromaticAberration { get; set; }
        public bool SpeedLines { get; set; }
        public float SpeedLineIntensity { get; set; }
        public bool SlowMotion { get; set; }
        public float SlowMotionScale { get; set; }
        public float SlowMotionDuration { get; set; }
        public string VignetteColor { get; set; }
        public float VignetteAlpha { get; set; }
    }
    
    public class UIAnimationPreset
    {
        public string Name { get; set; }
        public float ScaleInDuration { get; set; }
        public float ScaleInAmount { get; set; }
        public float ScaleOutDuration { get; set; }
        public float ScaleOutTarget { get; set; }
        public float FloatDuration { get; set; }
        public float FloatDistance { get; set; }
        public float RotationAmount { get; set; }
        public string EaseType { get; set; }
    }
    
    public class ComboMilestoneConfig
    {
        public int ComboCount { get; set; }
        public string Message { get; set; }
        public string Color { get; set; }
        public float Scale { get; set; }
    }
    
    public class HealthThresholdConfig
    {
        public float Threshold { get; set; }
        public string Color { get; set; }
        public float GlowIntensity { get; set; }
    }
}
