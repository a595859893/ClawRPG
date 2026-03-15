using Godot;
using System;

namespace ClawRPG.Scripts.Characters {
    /// <summary>
    /// Boss special ability data structure
    /// </summary>
    [System.Serializable]
    public class BossAbility
    {
        public string AbilityId;
        public string AbilityName;
        public string Description;
        public float Cooldown;
        public float DamageMultiplier;
        public float Range;
        public float Duration;
        public StatusEffectType? ApplyStatus;
        public float StatusChance;
        public bool IsAoE;
        public float AoERadius;
        
        public BossAbility(string id, string name, string desc, float cooldown, float dmgMult, float range = 150f)
        {
            AbilityId = id;
            AbilityName = name;
            Description = desc;
            Cooldown = cooldown;
            DamageMultiplier = dmgMult;
            Range = range;
            Duration = 0f;
            IsAoE = false; 
            AoERadius = 0f;
            StatusChance = 0f;
        }
    }
    
    /// <summary>
    /// Boss AI behavior state enumeration
    /// </summary>
    public enum BossAIState
    {
        Idle,
        Chasing,
        Attacking,
        UsingAbility,
        Retreating,
        Stunned
    }
}
