using Godot;
using System;
using System.Collections.Generic;

namespace ClawRPG.Scripts.Skills {
    /// <summary>
    /// Skill data - static skill definition (read-only)
    /// </summary>
    public class SkillData
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public string Description { get; set; } = "";
        public SkillType Type { get; set; }
        public SkillTreeType Tree { get; set; }
        public bool IsPassive { get; set; }
        public int ManaCost { get; set; }
        public float Cooldown { get; set; } = 5f;
        public float CastTime { get; set; }
        public int LevelRequired { get; set; } = 1;
        public int MaxLevel { get; set; } = 5;
        public int RequiredSkillId { get; set; }
        public string IconPath { get; set; } = "";
        
        // Dynamic effects list (the core of modular design)
        public List<SkillEffectData> Effects { get; set; } = new();
        
        public enum SkillType { Attack, Healing, Buff, Debuff, Passive }
    }
    
    /// <summary>
    /// Skill effect data - defines what the skill does
    /// </summary>
    public class SkillEffectData
    {
        public SkillEffectType EffectType { get; set; }
        public float Value { get; set; }
        public float Duration { get; set; }
        public float Range { get; set; }
        public StatusEffect.EffectType? StatusEffect { get; set; }
        public float StatusEffectDamage { get; set; }
        public float StatusEffectDuration { get; set; }
        public bool IsAOE { get; set; }
        public float AOERadius { get; set; }
        public float DamageMultiplier { get; set; } = 1f;
    }
    
    /// <summary>
    /// Skill effect types - extensible enum
    /// </summary>
    public enum SkillEffectType
    {
        Damage,           // Direct damage
        Heal,             // Direct healing
        DamageOverTime,   // DOT effect
        HealOverTime,     // HOT effect
        Buff,             // Apply buff
        Debuff,           // Apply debuff
        Shield,           // Apply shield
        Knockback,        // Knockback effect
        Stun,             // Stun effect
        SpeedBoost,       // Speed increase
        Invincibility,    // Temporary invincibility
        resurrect         // Resurrect fallen target
    }
    
    /// <summary>
    /// Skill instance - tracks skill level and runtime state
    /// </summary>
    public class SkillInstance
    {
        public SkillData Data { get; }
        public int CurrentLevel { get; set; }
        public float LastUsedTime { get; set; }
        public bool IsOnCooldown => Time.TotalSeconds - LastUsedTime < CooldownRemaining;
        
        public float CooldownRemaining => Mathf.Max(0, Data.Cooldown - (float)(Time.TotalSeconds - LastUsedTime));
        
        public SkillInstance(SkillData data, int level = 1)
        {
            Data = data;
            CurrentLevel = level;
            LastUsedTime = -999f; // Ready at start
        }
        
        public bool CanUse(int playerLevel, int currentMana)
        {
            return CurrentLevel > 0 && 
                   playerLevel >= Data.LevelRequired && 
                   currentMana >= Data.ManaCost && 
                   !IsOnCooldown;
        }
        
        public void Use()
        {
            LastUsedTime = (float)Time.TotalSeconds;
        }
        
        public float GetScaledValue(SkillEffectType effectType)
        {
            float scaleFactor = 1f + (CurrentLevel - 1) * 0.2f; // +20% per level
            return scaleFactor;
        }
    }
    
    /// <summary>
    /// Skill executor - executes skill effects on targets
    /// </summary>
    public class SkillExecutor
    {
        // Singleton for global access
        private static SkillExecutor _instance;
        public static SkillExecutor Instance => _instance ??= new SkillExecutor();
        
        /// <summary>
        /// Execute all effects of a skill
        /// </summary>
        public void Execute(SkillInstance skill, Node source, Node target = null)
        {
            if (skill == null || source == null) return;
            
            // Get all potential targets (single or AOE)
            var targets = GetTargets(source, target, skill.Data);
            
            foreach (var effectData in skill.Data.Effects)
            {
                float scaledValue = effectData.Value * skill.GetScaledValue(effectData.EffectType);
                
                foreach (var t in targets)
                {
                    ApplyEffect(effectData.EffectType, scaledValue, effectData, source, t);
                }
            }
            
            skill.Use();
        }
        
        /// <summary>
        /// Get target list based on skill configuration
        /// </summary>
        private List<Node> GetTargets(Node source, Node primaryTarget, SkillData data)
        {
            var targets = new List<Node>();
            
            if (data.IsAOE)
            {
                // AOE - get all enemies in range
                var enemies = GetEnemiesInArea(source, data.Effects.Count > 0 ? data.Effects[0].AOERadius : 100f);
                targets.AddRange(enemies);
            }
            else if (primaryTarget != null)
            {
                targets.Add(primaryTarget);
            }
            
            return targets;
        }
        
        /// <summary>
        /// Get enemies in area around source
        /// </summary>
        private List<Node> GetEnemiesInArea(Node source, float radius)
        {
            var enemies = new List<Node>();
            var area = source.GetNodeOrNull<Area2D>("AttackArea");
            
            if (area != null)
            {
                var bodies = area.GetOverlappingBodies();
                foreach (var body in bodies)
                {
                    if (body is Enemy || body is Player)
                    {
                        float dist = source.GlobalPosition.DistanceTo(body.GlobalPosition);
                        if (dist <= radius)
                        {
                            enemies.Add(body);
                        }
                    }
                }
            }
            
            return enemies;
        }
        
        /// <summary>
        /// Apply a single effect to a target
        /// </summary>
        private void ApplyEffect(SkillEffectType effectType, float value, SkillEffectData effectData, Node source, Node target)
        {
            switch (effectType)
            {
                case SkillEffectType.Damage:
                    ApplyDamage(source, target, value, effectData);
                    break;
                    
                case SkillEffectType.Heal:
                    ApplyHeal(source, target, value);
                    break;
                    
                case SkillEffectType.DamageOverTime:
                    ApplyDOT(target, value, effectData);
                    break;
                    
                case SkillEffectType.HealOverTime:
                    ApplyHOT(target, value, effectData);
                    break;
                    
                case SkillEffectType.Buff:
                case SkillEffectType.Debuff:
                    ApplyStatusEffect(target, effectData);
                    break;
                    
                case SkillEffectType.Shield:
                    ApplyShield(target, value, effectData);
                    break;
                    
                case SkillEffectType.Knockback:
                    ApplyKnockback(source, target, value);
                    break;
                    
                case SkillEffectType.Stun:
                    ApplyStun(target, effectData.Duration);
                    break;
                    
                case SkillEffectType.SpeedBoost:
                    ApplySpeedBoost(target, value, effectData.Duration);
                    break;
                    
                case SkillEffectType.Invincibility:
                    ApplyInvincibility(target, effectData.Duration);
                    break;
            }
        }
        
        private void ApplyDamage(Node source, Node target, float damage, SkillEffectData data)
        {
            // Scale with damage multiplier from equipment/talents
            float multiplier = data.DamageMultiplier;
            
            if (source is Player player)
            {
                damage *= multiplier;
                
                // Critical hit
                if (GD.Randf() < player.CritChance)
                {
                    damage *= player.CritDamage;
                    ShowDamageNumber(target, damage, true);
                }
            }
            
            if (target is Enemy enemy)
            {
                enemy.TakeDamage((int)damage);
            }
            else if (target is Player p)
            {
                p.TakeDamage((int)damage);
            }
            
            ShowDamageNumber(target, damage, false);
        }
        
        private void ApplyHeal(Node source, Node target, float amount)
        {
            if (target is Player player)
            {
                player.Heal((int)amount);
            }
        }
        
        private void ApplyDOT(Node target, float damagePerSecond, SkillEffectData data)
        {
            if (target is Enemy enemy && data.StatusEffect.HasValue)
            {
                enemy.ApplyStatusEffect(data.StatusEffect.Value, damagePerSecond, data.Duration);
            }
            else if (target is Player player && data.StatusEffect.HasValue)
            {
                player.ApplyStatusEffect(data.StatusEffect.Value, damagePerSecond, data.Duration);
            }
        }
        
        private void ApplyHOT(Node target, float healPerSecond, SkillEffectData data)
        {
            // Apply regeneration status effect
            if (target is Player player)
            {
                player.ApplyStatusEffect(StatusEffect.EffectType.Regeneration, healPerSecond, data.Duration);
            }
        }
        
        private void ApplyStatusEffect(Node target, SkillEffectData data)
        {
            if (!data.StatusEffect.HasValue) return;
            
            if (target is Enemy enemy)
            {
                enemy.ApplyStatusEffect(data.StatusEffect.Value, data.StatusEffectDamage, data.StatusEffectDuration);
            }
            else if (target is Player player)
            {
                player.ApplyStatusEffect(data.StatusEffect.Value, data.StatusEffectDamage, data.StatusEffectDuration);
            }
        }
        
        private void ApplyShield(Node target, float shieldAmount, SkillEffectData data)
        {
            if (target is Player player)
            {
                player.ApplyStatusEffect(StatusEffect.EffectType.Shield, shieldAmount, data.Duration);
            }
        }
        
        private void ApplyKnockback(Node source, Node target, float force)
        {
            Vector2 direction = (target.GlobalPosition - source.GlobalPosition).Normalized();
            if (target is CharacterBody2D body)
            {
                body.Velocity = direction * force;
                body.MoveAndSlide();
            }
        }
        
        private void ApplyStun(Node target, float duration)
        {
            if (target is Enemy enemy)
            {
                enemy.ApplyStatusEffect(StatusEffect.EffectType.Stun, 0, duration);
            }
        }
        
        private void ApplySpeedBoost(Node target, float multiplier, float duration)
        {
            // Use slow (negative) for speed boost
            if (target is Player player)
            {
                player.ApplyStatusEffect(StatusEffect.EffectType.Slow, -multiplier, duration);
            }
        }
        
        private void ApplyInvincibility(Node target, float duration)
        {
            if (target is Player player)
            {
                player.ApplyStatusEffect(StatusEffect.EffectType.Shield, 9999, duration);
            }
        }
        
        private void ShowDamageNumber(Node target, float damage, bool isCrit)
        {
            var damagePopup = target.GetNodeOrNull<CanvasItem>("DamagePopup");
            if (damagePopup != null && damagePopup.HasMethod("ShowDamage"))
            {
                damagePopup.Call("ShowDamage", (int)damage, isCrit);
            }
        }
    }
}
