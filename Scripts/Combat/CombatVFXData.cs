using Godot;
using System;
using System.Collections.Generic;

namespace ClawRPG.Scripts.Combat {
    /// <summary>
    /// 战斗视觉特效数据类型定义
    /// </summary>
    
    // 伤害数字类型
    public enum DamageNumberType {
        Normal,       // 普通伤害
        Critical,      // 暴击伤害
        Heal,          // 治疗
        Block,         // 格挡
        Dodge,         // 闪避
        Miss,          // MISS
        Absorb,        // 吸收
        Reflect        // 反射
    }
    
    // 特效类型
    public enum VFXType {
        Hit,              // 击中特效
        Critical,          // 暴击特效
        Heal,              // 治疗特效
        Block,             // 格挡特效
        Dodge,             // 闪避特效
        Death,             // 死亡特效
        Skill,             // 技能特效
        Buff,              // buff特效
        Debuff             // debuff特效
    }
    
    // 特效持续类型
    public enum VFXDuration {
        Instant,   // 瞬时
        Short,     // 短时 (1秒)
        Medium,    // 中时 (3秒)
        Long       // 长时 (5秒)
    }
    
    // 特效位置类型
    public enum VFXPosition {
        Target,        // 目标位置
        World,         // 世界坐标
        Screen,        // 屏幕中心
        Camera         // 相机跟随
    }
    
    // 伤害数字数据
    public class DamageNumber {
        public float Value { get; set; }
        public DamageNumberType Type { get; set; }
        public Vector3 Position { get; set; }
        public Vector3 Velocity { get; set; }
        public float LifeTime { get; set; }
        public float CurrentTime { get; set; }
        public bool IsEnemy { get; set; }  // 是否是敌人造成的
    }
    
    // 特效实例数据
    public class VFXInstance {
        public string ID { get; set; }
        public VFXType Type { get; set; }
        public VFXDuration Duration { get; set; }
        public Vector3 Position { get; set; }
        public Color Color { get; set; }
        public float Scale { get; set; }
        public float LifeTime { get; set; }
        public float CurrentTime { get; set; }
        public Node3D Target { get; set; }
    }
    
    // 屏幕特效数据
    public class ScreenEffect {
        public string ID { get; set; }
        public ScreenEffectType Type { get; set; }
        public float Intensity { get; set; }
        public float Duration { get; set; }
        public float CurrentTime { get; set; }
        public Color Color { get; set; }
    }
    
    public enum ScreenEffectType {
        Flash,           // 闪白
        RedTint,         // 红晕（受伤）
        Shake,           // 震动
        SlowMo,          // 慢动作
        Chromatic        // 色差
    }
    
    // 连击特效数据
    public class ComboEffect {
        public int ComboCount { get; set; }
        public Vector3 Position { get; set; }
        public float LifeTime { get; set; }
        public float CurrentTime { get; set; }
    }
    
    // 暴击光效数据
    public class CriticalGlow {
        public Node3D Target { get; set; }
        public Color GlowColor { get; set; }
        public float Intensity { get; set; }
        public float Duration { get; set; }
        public float CurrentTime { get; set; }
    }
    
    // 玩家战斗视觉数据
    public class PlayerCombatVFXData {
        public int TotalDamageNumbers { get; set; }
        public int CriticalHits { get; set; }
        public int Heals { get; set; }
        public int Blocks { get; set; }
        public int Dodges { get; set; }
        public int MaxCombo { get; set; }
        public int ScreenEffects { get; set; }
        public int VFXPlayed { get; set; }
        
        public PlayerCombatVFXData() {
            TotalDamageNumbers = 0;
            CriticalHits = 0;
            Heals = 0;
            Blocks = 0;
            Dodges = 0;
            MaxCombo = 0;
            ScreenEffects = 0;
            VFXPlayed = 0;
        }
    }
    
    // 特效配置
    public class VFXConfig {
        public string ID { get; set; }
        public string Name { get; set; }
        public VFXType Type { get; set; }
        public VFXDuration Duration { get; set; }
        public VFXPosition Position { get; set; }
        public string ParticleSystem { get; set; }  // 粒子系统名称
        public string SoundEffect { get; set; }     // 音效名称
        public Color Color { get; set; }
        public float Scale { get; set; }
        public float Lifetime { get; set; }
    }
}
