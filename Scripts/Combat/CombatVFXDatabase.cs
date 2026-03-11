using Godot;
using System;
using System.Collections.Generic;

namespace ClawRPG.Scripts.Combat {
    /// <summary>
    /// 战斗视觉特效配置数据库
    /// </summary>
    public static class CombatVFXDatabase {
        
        // 伤害数字颜色配置
        public static readonly Dictionary<DamageNumberType, Color> DamageNumberColors = new Dictionary<DamageNumberType, Color> {
            { DamageNumberType.Normal, new Color(1f, 1f, 1f) },      // 白色
            { DamageNumberType.Critical, new Color(1f, 0.84f, 0f) }, // 金色
            { DamageNumberType.Heal, new Color(0f, 1f, 0.5f) },     // 绿色
            { DamageNumberType.Block, new Color(0.5f, 0.5f, 1f) },   // 蓝色
            { DamageNumberType.Dodge, new Color(0.7f, 0.7f, 0.7f) },  // 灰色
            { DamageNumberType.Miss, new Color(0.7f, 0.7f, 0.7f) },  // 灰色
            { DamageNumberType.Absorb, new Color(0.5f, 0.8f, 1f) },  // 浅蓝
            { DamageNumberType.Reflect, new Color(1f, 0.5f, 0.5f) }  // 红色
        };
        
        // 伤害数字大小配置
        public static readonly Dictionary<DamageNumberType, float> DamageNumberSizes = new Dictionary<DamageNumberType, float> {
            { DamageNumberType.Normal, 24f },
            { DamageNumberType.Critical, 36f },
            { DamageNumberType.Heal, 28f },
            { DamageNumberType.Block, 22f },
            { DamageNumberType.Dodge, 22f },
            { DamageNumberType.Miss, 22f },
            { DamageNumberType.Absorb, 24f },
            { DamageNumberType.Reflect, 24f }
        };
        
        // 伤害数字动画配置
        public static readonly Dictionary<DamageNumberType, Vector2> DamageNumberVelocities = new Dictionary<DamageNumberType, Vector2> {
            { DamageNumberType.Normal, new Vector2(0, -80f) },
            { DamageNumberType.Critical, new Vector2(30f, -120f) },  // 向右上方
            { DamageNumberType.Heal, new Vector2(0, -60f) },
            { DamageNumberType.Block, new Vector2(0, -40f) },
            { DamageNumberType.Dodge, new Vector2(0, -40f) },
            { DamageNumberType.Miss, new Vector2(0, -40f) },
            { DamageNumberType.Absorb, new Vector2(0, -50f) },
            { DamageNumberType.Reflect, new Vector2(0, -60f) }
        };
        
        // 暴击伤害倍数
        public static float CriticalMultiplier { get; set; } = 1.5f;
        
        // 特效配置
        public static readonly Dictionary<VFXType, VFXConfig> VFXConfigs = new Dictionary<VFXType, VFXConfig> {
            { VFXType.Hit, new VFXConfig {
                ID = "hit",
                Name = "击中特效",
                Type = VFXType.Hit,
                Duration = VFXDuration.Instant,
                Position = VFXPosition.Target,
                Color = new Color(1f, 1f, 1f),
                Scale = 1f,
                Lifetime = 0.3f
            }},
            { VFXType.Critical, new VFXConfig {
                ID = "critical",
                Name = "暴击特效",
                Type = VFXType.Critical,
                Duration = VFXDuration.Short,
                Position = VFXPosition.Target,
                Color = new Color(1f, 0.84f, 0f),
                Scale = 1.5f,
                Lifetime = 0.8f
            }},
            { VFXType.Heal, new VFXConfig {
                ID = "heal",
                Name = "治疗特效",
                Type = VFXType.Heal,
                Duration = VFXDuration.Short,
                Position = VFXPosition.Target,
                Color = new Color(0f, 1f, 0.5f),
                Scale = 1f,
                Lifetime = 0.6f
            }},
            { VFXType.Block, new VFXConfig {
                ID = "block",
                Name = "格挡特效",
                Type = VFXType.Block,
                Duration = VFXDuration.Instant,
                Position = VFXPosition.Target,
                Color = new Color(0.5f, 0.5f, 1f),
                Scale = 1f,
                Lifetime = 0.3f
            }},
            { VFXType.Dodge, new VFXConfig {
                ID = "dodge",
                Name = "闪避特效",
                Type = VFXType.Dodge,
                Duration = VFXDuration.Instant,
                Position = VFXPosition.Target,
                Color = new Color(0.7f, 0.7f, 0.7f),
                Scale = 1f,
                Lifetime = 0.4f
            }},
            { VFXType.Death, new VFXConfig {
                ID = "death",
                Name = "死亡特效",
                Type = VFXType.Death,
                Duration = VFXDuration.Medium,
                Position = VFXPosition.Target,
                Color = new Color(0.5f, 0f, 0f),
                Scale = 1f,
                Lifetime = 1.5f
            }},
            { VFXType.Skill, new VFXConfig {
                ID = "skill",
                Name = "技能特效",
                Type = VFXType.Skill,
                Duration = VFXDuration.Medium,
                Position = VFXPosition.World,
                Color = new Color(0.8f, 0.8f, 1f),
                Scale = 1.2f,
                Lifetime = 2f
            }},
            { VFXType.Buff, new VFXConfig {
                ID = "buff",
                Name = "Buff特效",
                Type = VFXType.Buff,
                Duration = VFXDuration.Long,
                Position = VFXPosition.Target,
                Color = new Color(0f, 1f, 0f),
                Scale = 1f,
                Lifetime = 4f
            }},
            { VFXType.Debuff, new VFXConfig {
                ID = "debuff",
                Name = "Debuff特效",
                Type = VFXType.Debuff,
                Duration = VFXDuration.Medium,
                Position = VFXPosition.Target,
                Color = new Color(0.8f, 0f, 0.8f),
                Scale = 1f,
                Lifetime = 3f
            }}
        };
        
        // 屏幕特效配置
        public static readonly Dictionary<ScreenEffectType, float> ScreenEffectDurations = new Dictionary<ScreenEffectType, float> {
            { ScreenEffectType.Flash, 0.1f },
            { ScreenEffectType.RedTint, 0.3f },
            { ScreenEffectType.Shake, 0.2f },
            { ScreenEffectType.SlowMo, 0.5f },
            { ScreenEffectType.Chromatic, 0.3f }
        };
        
        // 屏幕特效强度配置
        public static readonly Dictionary<ScreenEffectType, float> ScreenEffectIntensities = new Dictionary<ScreenEffectType, float> {
            { ScreenEffectType.Flash, 0.5f },
            { ScreenEffectType.RedTint, 0.3f },
            { ScreenEffectType.Shake, 10f },
            { ScreenEffectType.SlowMo, 0.3f },  // 时间缩放比例
            { ScreenEffectType.Chromatic, 0.02f }
        };
        
        // 连击特效配置
        public static readonly Dictionary<int, string> ComboMilestones = new Dictionary<int, string> {
            { 5, "Nice!" },
            { 10, "Good!" },
            { 20, "Great!" },
            { 30, "Amazing!" },
            { 50, "Legendary!" },
            { 100, "UNSTOPPABLE!" }
        };
        
        // 连击特效颜色
        public static Color GetComboColor(int comboCount) {
            if (comboCount >= 50) return new Color(1f, 0f, 0.5f);  // 粉色
            if (comboCount >= 30) return new Color(1f, 0.84f, 0f);   // 金色
            if (comboCount >= 20) return new Color(1f, 0.5f, 0f);    // 橙色
            if (comboCount >= 10) return new Color(0.5f, 1f, 0f);     // 绿色
            if (comboCount >= 5) return new Color(0f, 0.8f, 1f);     // 蓝色
            return new Color(1f, 1f, 1f);                              // 白色
        }
        
        // 连击特效大小
        public static float GetComboSize(int comboCount) {
            return Mathf.Min(48f + comboCount * 0.5f, 72f);
        }
        
        // 暴击光效颜色
        public static Color GetCriticalGlowColor() {
            return new Color(1f, 0.84f, 0f, 0.8f);
        }
        
        // 暴击光效持续时间
        public static float GetCriticalGlowDuration() {
            return 0.4f;
        }
        
        // 暴击光效强度
        public static float GetCriticalGlowIntensity() {
            return 2f;
        }
        
        // 获取特效持续时间（秒）
        public static float GetVFXDuration(VFXDuration duration) {
            switch (duration) {
                case VFXDuration.Instant: return 0.3f;
                case VFXDuration.Short: return 1f;
                case VFXDuration.Medium: return 3f;
                case VFXDuration.Long: return 5f;
                default: return 1f;
            }
        }
    }
}
