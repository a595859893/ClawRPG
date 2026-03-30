using System;

namespace ClawRPG.Scripts.Systems.BossMechanics {
    /// <summary>
    /// Intent type classification for boss attacks.
    /// Maps to BossSkillType for display purposes.
    /// </summary>
    public enum BossIntentType {
        Damage,   // Red — direct damage abilities
        Buff,     // Yellow — self buffs, shields, heals
        Debuff,   // Purple — debuffs applied to players
        Defend,   // Blue — defensive abilities
        Special   // Dark purple — summons, teleports, enrage
    }

    /// <summary>
    /// Data payload for boss intent display.
    /// Emitted as a signal when BossDecisionMaker selects an ability.
    /// </summary>
    public class BossIntentData {
        /// <summary>The ability identifier selected by the boss.</summary>
        public string AbilityId { get; set; }

        /// <summary>Human-readable ability name.</summary>
        public string AbilityName { get; set; }

        /// <summary>Classified intent type for color/icon coding.</summary>
        public BossIntentType IntentType { get; set; }

        /// <summary>Minimum damage (for damage-type intents).</summary>
        public int MinDamage { get; set; }

        /// <summary>Maximum damage (for damage-type intents).</summary>
        public int MaxDamage { get; set; }

        /// <summary>Whether this is a multi-target ability.</summary>
        public bool IsAoE { get; set; }

        /// <summary>Whether the boss is currently enraged.</summary>
        public bool IsEnraged { get; set; }

        /// <summary>
        /// Classify a BossSkillType into a BossIntentType.
        /// </summary>
        public static BossIntentType ClassifySkillType(BossSkillType skillType) {
            return skillType switch {
                BossSkillType.MeleeAttack or
                BossSkillType.RangedAttack or
                BossSkillType.AreaOfEffect or
                BossSkillType.Charge or
                BossSkillType.SpinAttack or
                BossSkillType.LaserBeam or
                BossSkillType.Projectile or
                BossSkillType.Knockback or
                BossSkillType.Stun
                    => BossIntentType.Damage,

                BossSkillType.Buff or
                BossSkillType.Heal or
                BossSkillType.Shield
                    => BossIntentType.Buff,

                BossSkillType.Debuff
                    => BossIntentType.Debuff,

                BossSkillType.Teleport
                    => BossIntentType.Defend,

                BossSkillType.Summon or
                BossSkillType.Enrage
                    => BossIntentType.Special,

                _ => BossIntentType.Damage
            };
        }

        /// <summary>
        /// Build a display string like "18–24 Damage" or "Heal: 300".
        /// </summary>
        public string GetDisplayString() {
            return IntentType switch {
                BossIntentType.Damage when MinDamage > 0 && MaxDamage > 0
                    => IsAoE ? $"{MinDamage}–{MaxDamage} AoE" : $"{MinDamage}–{MaxDamage}",

                BossIntentType.Buff when AbilityName.Contains("Heal")
                    => $"Heal: {MinDamage}",

                BossIntentType.Buff when AbilityName.Contains("Shield")
                    => $"Shield: {MinDamage}",

                BossIntentType.Special when AbilityName.Contains("Summon")
                    => "Summon",

                BossIntentType.Special when AbilityName.Contains("Enrage")
                    => "ENRAGE",

                _ => AbilityName
            };
        }
    }
}
