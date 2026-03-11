using Godot;
using System;
using System.Collections.Generic;

namespace ClawRPG.Scripts.Mounts {
    /// <summary>
    /// 坐骑战斗数据结构 - 定义坐骑的战斗技能和特殊能力
    /// </summary>
    public class MountCombatData {
        /// <summary>
        /// 坐骑战斗技能
        /// </summary>
        public class MountCombatSkill {
            public string Id { get; set; }
            public string Name { get; set; }
            public string Description { get; set; }
            public int Cooldown { get; set; } // 秒
            public int ManaCost { get; set; }
            public float DamageMultiplier { get; set; } = 1.0f;
            public float Range { get; set; } = 100f;
            public MountSkillType SkillType { get; set; }
            public string AnimationName { get; set; }
            public float KnockbackForce { get; set; }
            public float StunDuration { get; set; }
            public bool IsAOE { get; set; }
            public float AOERadius { get; set; }
            
            // 特殊效果
            public bool ApplySlow { get; set; }
            public float SlowDuration { get; set; }
            public float SlowAmount { get; set; }
            
            public bool ApplyBleed { get; set; }
            public int BleedDamage { get; set; }
            public float BleedDuration { get; set; }
            
            public bool HealCaster { get; set; }
            public int HealAmount { get; set; }
        }

        /// <summary>
        /// 坐骑技能类型
        /// </summary>
        public enum MountSkillType {
            Charge,      // 冲锋
            Slam,       // 践踏
            Sweep,      // 横扫
            Trample,    // 踩踏
            Roar,       // 咆哮
            Shield,     // 护盾
            Dash,       // 冲刺
            Bleed,      // 撕裂
            Burn,       // 灼烧
            Freeze,     // 冰冻
        }

        /// <summary>
        /// 坐骑战斗属性
        /// </summary>
        public class MountCombatStats {
            public float AttackDamage { get; set; }
            public float AttackSpeed { get; set; }
            public float CritChance { get; set; }
            public float CritDamage { get; set; }
            public float ArmorPenetration { get; set; }
            public float LifeSteal { get; set; }
            public float DodgeChance { get; set; }
            public float BlockChance { get; set; }
            public float Tenacity { get; set; } // 韧性，减少控制时间
        }

        /// <summary>
        /// 坐骑实例战斗数据
        /// </summary>
        public class MountCombatInstance {
            public string MountId { get; set; }
            public int Level { get; set; }
            public int Experience { get; set; }
            public List<string> UnlockedSkills { get; set; } = new List<string>();
            public Dictionary<string, int> SkillCooldowns { get; set; } = new Dictionary<string, int>();
            public float LastAttackTime { get; set; }
            public bool IsInCombat { get; set; }
            public int CombatKills { get; set; }
            public int CombatDamageDealt { get; set; }
            public int CombatDamageTaken { get; set; }
        }

        /// <summary>
        /// 获取技能冷却剩余时间
        /// </summary>
        public static int GetSkillCooldownRemaining(MountCombatInstance instance, string skillId) {
            if (instance.SkillCooldowns.ContainsKey(skillId)) {
                return Mathf.Max(0, instance.SkillCooldowns[skillId]);
            }
            return 0;
        }

        /// <summary>
        /// 检查技能是否可用
        /// </summary>
        public static bool IsSkillReady(MountCombatInstance instance, MountCombatSkill skill, int currentMana) {
            if (currentMana < skill.ManaCost) return false;
            if (GetSkillCooldownRemaining(instance, skill.Id) > 0) return false;
            return true;
        }
    }
}
