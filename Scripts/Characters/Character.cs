using Godot;
using ClawRPG.Systems;

namespace ClawRPG.Scripts.Characters
{
    /// <summary>
    /// Base class for all character entities (player, enemy, boss).
    /// </summary>
    public partial class Character : Node2D
    {
        public float CurrentHealth { get; set; }
        public float MaxHealth { get; set; }
        public float Attack { get; set; }
        public float Defense { get; set; }

        // Status effect management - stub implementation
        private Godot.Collections.Array _characterStatusEffects = new();

        /// <summary>
        /// Apply a status effect from skill system (EffectType variant).
        /// </summary>
        public virtual void ApplyStatusEffect(StatusEffect.EffectType effectType, float value, float duration)
        {
            var effect = new StatusEffect(effectType, value, duration);
            _characterStatusEffects.Add(effect);
        }

        /// <summary>
        /// Apply a status effect from boss abilities (StatusEffectType variant).
        /// </summary>
        public virtual void ApplyStatusEffect(StatusEffectType effectType, float value, float duration)
        {
            var effect = StatusEffect.FromEffectType(effectType, value, duration);
            _characterStatusEffects.Add(effect);
        }
    }
}
