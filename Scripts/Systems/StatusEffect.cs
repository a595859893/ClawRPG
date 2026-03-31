using System;
using ClawRPG.Scripts.Characters;

namespace ClawRPG.Systems
{
    /// <summary>
    /// Status effect types used by the skill/combat system.
    /// This nested enum mirrors ClawRPG.Systems.StatusEffectType for code that
    /// references StatusEffect.EffectType (e.g. SkillModules.cs).
    /// </summary>
    public class StatusEffect
    {
        /// <summary>
        /// Nested enum aliasing StatusEffectType values.
        /// Used by SkillModules, SkillSystem, etc.
        /// </summary>
        public enum EffectType
        {
            None = 0,
            Poison = 1,
            Burn = 2,
            Freeze = 3,
            Stun = 4,
            Slow = 5,
            Silence = 6,
            Shield = 7,
            Buff = 8,
            Debuff = 9,
            Regeneration = 10,
            Bleeding = 11,
            Cursed = 12
        }

        /// <summary>
        /// The type of this status effect.
        /// </summary>
        public EffectType Type { get; set; }

        /// <summary>
        /// Secondary value used by the effect (damage, healing, shield amount, etc.).
        /// </summary>
        public float Value { get; set; }

        /// <summary>
        /// Total duration in seconds.
        /// </summary>
        public float Duration { get; set; }

        /// <summary>
        /// Elapsed time since the effect was applied.
        /// </summary>
        public float Elapsed { get; private set; }

        /// <summary>
        /// Whether this effect has expired.
        /// </summary>
        public bool IsExpired => Elapsed >= Duration;

        /// <summary>
        /// Create a status effect with the given type, value, and duration.
        /// </summary>
        public StatusEffect(EffectType type, float value, float duration)
        {
            Type = type;
            Value = value;
            Duration = duration;
            Elapsed = 0f;
        }

        /// <summary>
        /// Update the effect. Call each physics frame.
        /// </summary>
        public virtual void Update(Character target, double delta)
        {
            Elapsed += (float)delta;
        }

        /// <summary>
        /// Convenience factory to create an effect from a StatusEffectType enum value.
        /// </summary>
        public static StatusEffect FromEffectType(StatusEffectType type, float value, float duration)
        {
            return new StatusEffect((EffectType)(int)type, value, duration);
        }
    }
}
