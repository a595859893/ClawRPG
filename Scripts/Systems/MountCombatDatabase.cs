using Godot;
using System;
using System.Collections.Generic;

namespace ClawRPG.Scripts.Mounts {
    /// <summary>
    /// 坐骑战斗数据库 - 定义所有坐骑的战斗技能
    /// </summary>
    public class MountCombatDatabase {
        public static MountCombatDatabase Instance { get; private set; }

        private Dictionary<string, List<MountCombatData.MountCombatSkill>> _mountSkills = new Dictionary<string, List<MountCombatData.MountCombatSkill>>();
        private Dictionary<string, MountCombatData.MountCombatStats> _mountCombatStats = new Dictionary<string, MountCombatData.MountCombatStats>();

        public MountCombatDatabase() {
            Instance = this;
            InitializeMountCombatData();
        }

        /// <summary>
        /// 初始化坐骑战斗数据
        /// </summary>
        private void InitializeMountCombatData() {
            // === 马类坐骑 ===
            _mountSkills["horse_white"] = new List<MountCombatData.MountCombatSkill> {
                new MountCombatData.MountCombatSkill {
                    Id = "horse_charge",
                    Name = "冲锋",
                    Description = "快速冲向敌人，造成大量伤害",
                    Cooldown = 8,
                    ManaCost = 30,
                    DamageMultiplier = 1.8f,
                    Range = 200f,
                    SkillType = MountCombatData.MountSkillType.Charge,
                    KnockbackForce = 150f,
                },
                new MountCombatData.MountCombatSkill {
                    Id = "horse_slam",
                    Name = "践踏",
                    Description = "用力践踏地面，对周围敌人造成伤害",
                    Cooldown = 12,
                    ManaCost = 40,
                    DamageMultiplier = 1.2f,
                    Range = 150f,
                    SkillType = MountCombatData.MountSkillType.Slam,
                    IsAOE = true,
                    AOERadius = 120f,
                    StunDuration = 0.5f,
                },
            };
            _mountCombatStats["horse_white"] = new MountCombatData.MountCombatStats {
                AttackDamage = 25f,
                AttackSpeed = 1.2f,
                CritChance = 0.05f,
                CritDamage = 1.5f,
                ArmorPenetration = 10f,
            };

            _mountSkills["horse_black"] = new List<MountCombatData.MountCombatSkill> {
                new MountCombatData.MountCombatSkill {
                    Id = "horse_dark_charge",
                    Name = "暗影冲锋",
                    Description = "暗影之力包裹的冲锋攻击",
                    Cooldown = 7,
                    ManaCost = 35,
                    DamageMultiplier = 2.0f,
                    Range = 220f,
                    SkillType = MountCombatData.MountSkillType.Charge,
                    KnockbackForce = 180f,
                },
                new MountCombatData.MountCombatSkill {
                    Id = "horse_dark_sweep",
                    Name = "暗影横扫",
                    Description = "暗影能量横扫面前所有敌人",
                    Cooldown = 10,
                    ManaCost = 45,
                    DamageMultiplier = 1.4f,
                    Range = 160f,
                    SkillType = MountCombatData.MountSkillType.Sweep,
                    IsAOE = true,
                    AOERadius = 140f,
                    ApplySlow = true,
                    SlowDuration = 3f,
                    SlowAmount = 0.3f,
                },
            };
            _mountCombatStats["horse_black"] = new MountCombatData.MountCombatStats {
                AttackDamage = 35f,
                AttackSpeed = 1.1f,
                CritChance = 0.08f,
                CritDamage = 1.6f,
                ArmorPenetration = 15f,
                LifeSteal = 0.05f,
            };

            // === 狼类坐骑 ===
            _mountSkills["wolf_snow"] = new List<MountCombatData.MountCombatSkill> {
                new MountCombatData.MountCombatSkill {
                    Id = "wolf_fang",
                    Name = "利齿撕咬",
                    Description = "锋利的狼牙撕咬敌人",
                    Cooldown = 5,
                    ManaCost = 20,
                    DamageMultiplier = 1.5f,
                    Range = 80f,
                    SkillType = MountCombatData.MountSkillType.Charge,
                    ApplyBleed = true,
                    BleedDamage = 5,
                    BleedDuration = 5f,
                },
                new MountCombatData.MountCombatSkill {
                    Id = "wolf_howl",
                    Name = "嚎叫",
                    Description = "威慑敌人的嚎叫，降低敌人防御",
                    Cooldown = 15,
                    ManaCost = 50,
                    DamageMultiplier = 0.5f,
                    Range = 200f,
                    SkillType = MountCombatData.MountSkillType.Roar,
                    IsAOE = true,
                    AOERadius = 180f,
                },
            };
            _mountCombatStats["wolf_snow"] = new MountCombatData.MountCombatStats {
                AttackDamage = 30f,
                AttackSpeed = 1.4f,
                CritChance = 0.12f,
                CritDamage = 1.7f,
                DodgeChance = 0.08f,
            };

            _mountSkills["wolf_shadow"] = new List<MountCombatData.MountCombatSkill> {
                new MountCombatData.MountCombatSkill {
                    Id = "wolf_shadow_fang",
                    Name = "暗影之牙",
                    Description = "从阴影中发动的致命攻击",
                    Cooldown = 6,
                    ManaCost = 25,
                    DamageMultiplier = 2.2f,
                    Range = 100f,
                    SkillType = MountCombatData.MountSkillType.Charge,
                    ApplyBleed = true,
                    BleedDamage = 8,
                    BleedDuration = 6f,
                },
                new MountCombatData.MountCombatSkill {
                    Id = "wolf_shadow_tear",
                    Name = "暗影撕裂",
                    Description = "撕碎敌人，造成持续伤害",
                    Cooldown = 10,
                    ManaCost = 40,
                    DamageMultiplier = 1.3f,
                    Range = 90f,
                    SkillType = MountCombatData.MountSkillType.Bleed,
                    IsAOE = true,
                    AOERadius = 100f,
                    ApplyBleed = true,
                    BleedDamage = 10,
                    BleedDuration = 8f,
                },
            };
            _mountCombatStats["wolf_shadow"] = new MountCombatData.MountCombatStats {
                AttackDamage = 40f,
                AttackSpeed = 1.3f,
                CritChance = 0.15f,
                CritDamage = 1.8f,
                ArmorPenetration = 20f,
                LifeSteal = 0.08f,
            };

            // === 熊类坐骑 ===
            _mountSkills["bear_brown"] = new List<MountCombatData.MountCombatSkill> {
                new MountCombatData.MountCombatSkill {
                    Id = "bear_smash",
                    Name = "粉碎打击",
                    Description = "沉重的打击，击晕敌人",
                    Cooldown = 8,
                    ManaCost = 35,
                    DamageMultiplier = 2.0f,
                    Range = 100f,
                    SkillType = MountCombatData.MountSkillType.Slam,
                    StunDuration = 1.0f,
                    KnockbackForce = 100f,
                },
                new MountCombatData.MountCombatSkill {
                    Id = "bear_trample",
                    Name = "狂暴踩踏",
                    Description = "巨大的身体踩踏敌人",
                    Cooldown = 14,
                    ManaCost = 60,
                    DamageMultiplier = 1.5f,
                    Range = 150f,
                    SkillType = MountCombatData.MountSkillType.Trample,
                    IsAOE = true,
                    AOERadius = 150f,
                    StunDuration = 0.8f,
                },
            };
            _mountCombatStats["bear_brown"] = new MountCombatData.MountCombatStats {
                AttackDamage = 50f,
                AttackSpeed = 0.8f,
                CritChance = 0.05f,
                CritDamage = 1.4f,
                BlockChance = 0.15f,
                Tenacity = 0.2f,
            };

            // === 鹰类坐骑 ===
            _mountSkills["eagle_golden"] = new List<MountCombatData.MountCombatSkill> {
                new MountCombatData.MountCombatSkill {
                    Id = "eagle_dive",
                    Name = "俯冲击",
                    Description = "从高空俯冲击打敌人",
                    Cooldown = 6,
                    ManaCost = 25,
                    DamageMultiplier = 2.5f,
                    Range = 150f,
                    SkillType = MountCombatData.MountSkillType.Charge,
                    KnockbackForce = 200f,
                },
                new MountCombatData.MountCombatSkill {
                    Id = "eagle_talons",
                    Name = "利爪撕裂",
                    Description = "锋利的爪子撕裂敌人",
                    Cooldown = 9,
                    ManaCost = 35,
                    DamageMultiplier = 1.6f,
                    Range = 120f,
                    SkillType = MountCombatData.MountSkillType.Bleed,
                    ApplyBleed = true,
                    BleedDamage = 12,
                    BleedDuration = 5f,
                },
            };
            _mountCombatStats["eagle_golden"] = new MountCombatData.MountCombatStats {
                AttackDamage = 45f,
                AttackSpeed = 1.5f,
                CritChance = 0.18f,
                CritDamage = 1.9f,
                ArmorPenetration = 25f,
            };

            // === 龙类坐骑 ===
            _mountSkills["dragon_red"] = new List<MountCombatData.MountCombatSkill> {
                new MountCombatData.MountCombatSkill {
                    Id = "dragon_fire_breath",
                    Name = "火焰吐息",
                    Description = "喷吐火焰灼烧敌人",
                    Cooldown = 10,
                    ManaCost = 60,
                    DamageMultiplier = 1.8f,
                    Range = 250f,
                    SkillType = MountCombatData.MountSkillType.Burn,
                    IsAOE = true,
                    AOERadius = 100f,
                    ApplySlow = true,
                    SlowDuration = 2f,
                    SlowAmount = 0.4f,
                },
                new MountCombatData.MountCombatSkill {
                    Id = "dragon_claw",
                    Name = "龙爪撕裂",
                    Description = "巨大的龙爪撕裂敌人",
                    Cooldown = 7,
                    ManaCost = 40,
                    DamageMultiplier = 2.8f,
                    Range = 130f,
                    SkillType = MountCombatData.MountSkillType.Charge,
                    KnockbackForce = 250f,
                },
                new MountCombatData.MountCombatSkill {
                    Id = "dragon_tail_sweep",
                    Name = "龙尾横扫",
                    Description = "巨大的龙尾横扫所有敌人",
                    Cooldown = 15,
                    ManaCost = 80,
                    DamageMultiplier = 1.6f,
                    Range = 200f,
                    SkillType = MountCombatData.MountSkillType.Sweep,
                    IsAOE = true,
                    AOERadius = 180f,
                    StunDuration = 1.2f,
                },
            };
            _mountCombatStats["dragon_red"] = new MountCombatData.MountCombatStats {
                AttackDamage = 80f,
                AttackSpeed = 1.0f,
                CritChance = 0.12f,
                CritDamage = 2.0f,
                ArmorPenetration = 30f,
                LifeSteal = 0.15f,
            };

            _mountSkills["dragon_blue"] = new List<MountCombatData.MountCombatSkill> {
                new MountCombatData.MountCombatSkill {
                    Id = "dragon_ice_breath",
                    Name = "寒冰吐息",
                    Description = "喷吐寒冰冻结敌人",
                    Cooldown = 10,
                    ManaCost = 55,
                    DamageMultiplier = 1.7f,
                    Range = 240f,
                    SkillType = MountCombatData.MountSkillType.Freeze,
                    IsAOE = true,
                    AOERadius = 90f,
                    StunDuration = 2.0f,
                    ApplySlow = true,
                    SlowDuration = 4f,
                    SlowAmount = 0.5f,
                },
                new MountCombatData.MountCombatSkill {
                    Id = "dragon_frost_claw",
                    Name = "冰霜龙爪",
                    Description = "冰霜之力包裹的龙爪",
                    Cooldown = 7,
                    ManaCost = 35,
                    DamageMultiplier = 2.4f,
                    Range = 120f,
                    SkillType = MountCombatData.MountSkillType.Charge,
                    ApplyBleed = true,
                    BleedDamage = 8,
                    BleedDuration = 5f,
                },
            };
            _mountCombatStats["dragon_blue"] = new MountCombatData.MountCombatStats {
                AttackDamage = 70f,
                AttackSpeed = 1.1f,
                CritChance = 0.10f,
                CritDamage = 1.8f,
                ArmorPenetration = 25f,
                Tenacity = 0.15f,
            };

            // === 麒麟 ===
            _mountSkills["qilin"] = new List<MountCombatData.MountCombatSkill> {
                new MountCombatData.MountCombatSkill {
                    Id = "qilin_horn",
                    Name = "圣角冲击",
                    Description = "使用圣角冲击敌人",
                    Cooldown = 5,
                    ManaCost = 20,
                    DamageMultiplier = 1.8f,
                    Range = 150f,
                    SkillType = MountCombatData.MountSkillType.Charge,
                    KnockbackForce = 120f,
                    HealCaster = true,
                    HealAmount = 15,
                },
                new MountCombatData.MountCombatSkill {
                    Id = "qilin_shield",
                    Name = "神圣护盾",
                    Description = "召唤神圣护盾保护自己",
                    Cooldown = 20,
                    ManaCost = 50,
                    DamageMultiplier = 0f,
                    Range = 0f,
                    SkillType = MountCombatData.MountSkillType.Shield,
                },
                new MountCombatData.MountCombatSkill {
                    Id = "qilin_healing_aura",
                    Name = "治愈光环",
                    Description = "释放治愈光环恢复生命",
                    Cooldown = 25,
                    ManaCost = 70,
                    DamageMultiplier = 0f,
                    Range = 200f,
                    SkillType = MountCombatData.MountSkillType.Roar,
                    IsAOE = true,
                    AOERadius = 180f,
                    HealCaster = true,
                    HealAmount = 50,
                },
            };
            _mountCombatStats["qilin"] = new MountCombatData.MountCombatStats {
                AttackDamage = 55f,
                AttackSpeed = 1.3f,
                CritChance = 0.10f,
                CritDamage = 1.6f,
                LifeSteal = 0.10f,
                Tenacity = 0.25f,
            };

            GD.Print("[MountCombatDatabase] Initialized mount combat data for " + _mountSkills.Count + " mounts");
        }

        /// <summary>
        /// 获取坐骑的战斗技能列表
        /// </summary>
        public List<MountCombatData.MountCombatSkill> GetMountSkills(string mountId) {
            if (_mountSkills.ContainsKey(mountId)) {
                return _mountSkills[mountId];
            }
            return new List<MountCombatData.MountCombatSkill>();
        }

        /// <summary>
        /// 获取坐骑的战斗属性
        /// </summary>
        public MountCombatData.MountCombatStats GetMountCombatStats(string mountId) {
            if (_mountCombatStats.ContainsKey(mountId)) {
                return _mountCombatStats[mountId];
            }
            return new MountCombatData.MountCombatStats();
        }

        /// <summary>
        /// 获取所有可用技能
        /// </summary>
        public List<MountCombatData.MountCombatSkill> GetAllSkills(string mountId, int level) {
            var allSkills = GetMountSkills(mountId);
            var unlockedSkills = new List<MountCombatData.MountCombatSkill>();
            
            foreach (var skill in allSkills) {
                // 根据坐骑等级解锁技能
                int requiredLevel = GetSkillRequiredLevel(skill);
                if (level >= requiredLevel) {
                    unlockedSkills.Add(skill);
                }
            }
            
            return unlockedSkills;
        }

        /// <summary>
        /// 获取技能需要的等级
        /// </summary>
        private int GetSkillRequiredLevel(MountCombatData.MountCombatSkill skill) {
            // 简单规则：第一个技能1级解锁，后续技能每2级解锁一个
            var skills = GetMountSkills(skill.Id.Split('_')[0] + "_" + skill.Id.Split('_')[1]);
            if (skills == null) return 1;
            
            for (int i = 0; i < skills.Count; i++) {
                if (skills[i].Id == skill.Id) {
                    return 1 + (i * 2);
                }
            }
            return 1;
        }

        /// <summary>
        /// 获取坐骑是否有战斗能力
        /// </summary>
        public bool HasCombatAbility(string mountId) {
            return _mountSkills.ContainsKey(mountId) && _mountSkills[mountId].Count > 0;
        }
    }
}
