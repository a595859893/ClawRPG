using Godot;
using System;
using System.Collections.Generic;
using ClawRPG.Scripts.Skills;

namespace ClawRPG.Scripts.Systems {
    /// <summary>
    /// Skill Database V2 - Modular skill system using SkillData and SkillEffect
    /// </summary>
    public class SkillDatabaseV2
    {
        private static SkillDatabaseV2 _instance;
        public static SkillDatabaseV2 Instance => _instance ??= new SkillDatabaseV2();
        
        private Dictionary<int, SkillData> _skills = new();
        
        public SkillDatabaseV2()
        {
            Initialize();
        }
        
        private void Initialize()
        {
            // ===== OFFENSIVE SKILL TREE =====
            // Lightning Arrow
            AddSkill(new SkillData
            {
                Id = 1,
                Name = "闪电箭",
                Description = "发射一支闪电箭",
                Type = SkillData.SkillType.Attack,
                Tree = SkillTreeType.Offensive,
                ManaCost = 10,
                Cooldown = 3f,
                LevelRequired = 1,
                Effects = new List<SkillEffectData>
                {
                    new SkillEffectData { EffectType = SkillEffectType.Damage, Value = 30 }
                }
            });
            
            // Meteor
            AddSkill(new SkillData
            {
                Id = 2,
                Name = "陨石",
                Description = "召唤陨石砸向敌人",
                Type = SkillData.SkillType.Attack,
                Tree = SkillTreeType.Offensive,
                ManaCost = 30,
                Cooldown = 10f,
                LevelRequired = 3,
                RequiredSkillId = 1,
                Effects = new List<SkillEffectData>
                {
                    new SkillEffectData { EffectType = SkillEffectType.Damage, Value = 80, IsAOE = true, AOERadius = 100 }
                }
            });
            
            // Holy Strike
            AddSkill(new SkillData
            {
                Id = 3,
                Name = "圣光打击",
                Description = "神圣力量打击",
                Type = SkillData.SkillType.Attack,
                Tree = SkillTreeType.Offensive,
                ManaCost = 15,
                Cooldown = 4f,
                LevelRequired = 2,
                RequiredSkillId = 1,
                Effects = new List<SkillEffectData>
                {
                    new SkillEffectData { EffectType = SkillEffectType.Damage, Value = 40 },
                    new SkillEffectData { EffectType = SkillEffectType.Stun, Value = 0, Duration = 2f }
                }
            });
            
            // Shadow Arrow
            AddSkill(new SkillData
            {
                Id = 4,
                Name = "暗影箭",
                Description = "暗影箭矢",
                Type = SkillData.SkillType.Attack,
                Tree = SkillTreeType.Offensive,
                ManaCost = 12,
                Cooldown = 3f,
                LevelRequired = 2,
                Effects = new List<SkillEffectData>
                {
                    new SkillEffectData { EffectType = SkillEffectType.Damage, Value = 35 },
                    new SkillEffectData { EffectType = SkillEffectType.Debuff, Value = 0.5f, Duration = 3f, StatusEffect = StatusEffect.EffectType.Slow }
                }
            });
            
            // Whirlwind
            AddSkill(new SkillData
            {
                Id = 5,
                Name = "旋风斩",
                Description = "旋转攻击周围敌人",
                Type = SkillData.SkillType.Attack,
                Tree = SkillTreeType.Offensive,
                ManaCost = 20,
                Cooldown = 6f,
                LevelRequired = 3,
                RequiredSkillId = 4,
                Effects = new List<SkillEffectData>
                {
                    new SkillEffectData { EffectType = SkillEffectType.Damage, Value = 25, DamageMultiplier = 3f, IsAOE = true, AOERadius = 80 }
                }
            });
            
            // Poison Arrow
            AddSkill(new SkillData
            {
                Id = 6,
                Name = "毒箭",
                Description = "毒属性箭矢",
                Type = SkillData.SkillType.Attack,
                Tree = SkillTreeType.Offensive,
                ManaCost = 8,
                Cooldown = 2f,
                LevelRequired = 1,
                Effects = new List<SkillEffectData>
                {
                    new SkillEffectData { EffectType = SkillEffectType.Damage, Value = 15 },
                    new SkillEffectData { EffectType = SkillEffectType.DamageOverTime, Value = 5, Duration = 5f, StatusEffect = StatusEffect.EffectType.Poison }
                }
            });
            
            // Fire Bomb
            AddSkill(new SkillData
            {
                Id = 7,
                Name = "燃烧弹",
                Description = "投掷燃烧弹",
                Type = SkillData.SkillType.Attack,
                Tree = SkillTreeType.Offensive,
                ManaCost = 15,
                Cooldown = 5f,
                LevelRequired = 2,
                RequiredSkillId = 6,
                Effects = new List<SkillEffectData>
                {
                    new SkillEffectData { EffectType = SkillEffectType.Damage, Value = 30, IsAOE = true, AOERadius = 60 },
                    new SkillEffectData { EffectType = SkillEffectType.DamageOverTime, Value = 8, Duration = 4f, StatusEffect = StatusEffect.EffectType.Burn }
                }
            });
            
            // Frost Nova
            AddSkill(new SkillData
            {
                Id = 8,
                Name = "冰霜新星",
                Description = "冰冻周围敌人",
                Type = SkillData.SkillType.Attack,
                Tree = SkillTreeType.Offensive,
                ManaCost = 20,
                Cooldown = 8f,
                LevelRequired = 3,
                RequiredSkillId = 7,
                Effects = new List<SkillEffectData>
                {
                    new SkillEffectData { EffectType = SkillEffectType.Damage, Value = 25, IsAOE = true, AOERadius = 120 },
                    new SkillEffectData { EffectType = SkillEffectType.Stun, Value = 0, Duration = 2f, StatusEffect = StatusEffect.EffectType.Freeze }
                }
            });
            
            // Shadow Spike
            AddSkill(new SkillData
            {
                Id = 9,
                Name = "暗影之刺",
                Description = "暗影突刺",
                Type = SkillData.SkillType.Attack,
                Tree = SkillTreeType.Offensive,
                ManaCost = 18,
                Cooldown = 5f,
                LevelRequired = 3,
                Effects = new List<SkillEffectData>
                {
                    new SkillEffectData { EffectType = SkillEffectType.Damage, Value = 45 },
                    new SkillEffectData { EffectType = SkillEffectType.Debuff, Value = 0.5f, Duration = 4f, StatusEffect = StatusEffect.EffectType.Slow }
                }
            });
            
            // Chain Lightning
            AddSkill(new SkillData
            {
                Id = 10,
                Name = "链式闪电",
                Description = "连锁闪电攻击",
                Type = SkillData.SkillType.Attack,
                Tree = SkillTreeType.Offensive,
                ManaCost = 25,
                Cooldown = 7f,
                LevelRequired = 4,
                RequiredSkillId = 9,
                Effects = new List<SkillEffectData>
                {
                    new SkillEffectData { EffectType = SkillEffectType.Damage, Value = 35, IsAOE = true, AOERadius = 150 },
                    new SkillEffectData { EffectType = SkillEffectType.Stun, Value = 0, Duration = 1.5f, StatusEffect = StatusEffect.EffectType.Paralyze }
                }
            });
            
            // ===== MAGIC SKILL TREE - HEALING =====
            // Heal
            AddSkill(new SkillData
            {
                Id = 101,
                Name = "治疗术",
                Description = "恢复目标生命",
                Type = SkillData.SkillType.Healing,
                Tree = SkillTreeType.Magic,
                ManaCost = 15,
                Cooldown = 5f,
                LevelRequired = 1,
                Effects = new List<SkillEffectData>
                {
                    new SkillEffectData { EffectType = SkillEffectType.Heal, Value = 40 }
                }
            });
            
            // Group Heal
            AddSkill(new SkillData
            {
                Id = 102,
                Name = "群体治疗",
                Description = "恢复范围内友军生命",
                Type = SkillData.SkillType.Healing,
                Tree = SkillTreeType.Magic,
                ManaCost = 30,
                Cooldown = 10f,
                LevelRequired = 3,
                RequiredSkillId = 101,
                Effects = new List<SkillEffectData>
                {
                    new SkillEffectData { EffectType = SkillEffectType.Heal, Value = 60, IsAOE = true, AOERadius = 100 }
                }
            });
            
            // Regeneration
            AddSkill(new SkillData
            {
                Id = 103,
                Name = "再生",
                Description = "持续恢复生命",
                Type = SkillData.SkillType.Healing,
                Tree = SkillTreeType.Magic,
                ManaCost = 20,
                Cooldown = 15f,
                LevelRequired = 2,
                RequiredSkillId = 101,
                Effects = new List<SkillEffectData>
                {
                    new SkillEffectData { EffectType = SkillEffectType.HealOverTime, Value = 10, Duration = 10f }
                }
            });
            
            // ===== MAGIC SKILL TREE - BUFFS =====
            // Haste
            AddSkill(new SkillData
            {
                Id = 201,
                Name = "加速",
                Description = "提升移动速度",
                Type = SkillData.SkillType.Buff,
                Tree = SkillTreeType.Magic,
                ManaCost = 10,
                Cooldown = 20f,
                LevelRequired = 1,
                Effects = new List<SkillEffectData>
                {
                    new SkillEffectData { EffectType = SkillEffectType.SpeedBoost, Value = 1.5f, Duration = 10f }
                }
            });
            
            // Invincibility
            AddSkill(new SkillData
            {
                Id = 202,
                Name = "无敌",
                Description = "短时间无敌",
                Type = SkillData.SkillType.Buff,
                Tree = SkillTreeType.Magic,
                ManaCost = 50,
                Cooldown = 60f,
                LevelRequired = 5,
                RequiredSkillId = 201,
                Effects = new List<SkillEffectData>
                {
                    new SkillEffectData { EffectType = SkillEffectType.Invincibility, Value = 0, Duration = 3f }
                }
            });
            
            // Magic Shield
            AddSkill(new SkillData
            {
                Id = 203,
                Name = "魔法护盾",
                Description = "魔法护盾保护",
                Type = SkillData.SkillType.Buff,
                Tree = SkillTreeType.Magic,
                ManaCost = 25,
                Cooldown = 30f,
                LevelRequired = 3,
                RequiredSkillId = 201,
                Effects = new List<SkillEffectData>
                {
                    new SkillEffectData { EffectType = SkillEffectType.Shield, Value = 20, Duration = 15f }
                }
            });
            
            // Holy Shield
            AddSkill(new SkillData
            {
                Id = 204,
                Name = "圣光护盾",
                Description = "神圣护盾",
                Type = SkillData.SkillType.Buff,
                Tree = SkillTreeType.Magic,
                ManaCost = 30,
                Cooldown = 25f,
                LevelRequired = 4,
                RequiredSkillId = 203,
                Effects = new List<SkillEffectData>
                {
                    new SkillEffectData { EffectType = SkillEffectType.Shield, Value = 30, Duration = 10f },
                    new SkillEffectData { EffectType = SkillEffectType.HealOverTime, Value = 5, Duration = 10f }
                }
            });
            
            // ===== MAGIC SKILL TREE - DEBUFFS =====
            // Slow
            AddSkill(new SkillData
            {
                Id = 301,
                Name = "缓速",
                Description = "降低敌人速度",
                Type = SkillData.SkillType.Debuff,
                Tree = SkillTreeType.Magic,
                ManaCost = 10,
                Cooldown = 8f,
                LevelRequired = 2,
                Effects = new List<SkillEffectData>
                {
                    new SkillEffectData { EffectType = SkillEffectType.Debuff, Value = 0.5f, Duration = 5f, StatusEffect = StatusEffect.EffectType.Slow }
                }
            });
            
            // Stun
            AddSkill(new SkillData
            {
                Id = 302,
                Name = "眩晕",
                Description = "眩晕敌人",
                Type = SkillData.SkillType.Debuff,
                Tree = SkillTreeType.Magic,
                ManaCost = 20,
                Cooldown = 12f,
                LevelRequired = 4,
                RequiredSkillId = 301,
                Effects = new List<SkillEffectData>
                {
                    new SkillEffectData { EffectType = SkillEffectType.Stun, Value = 0, Duration = 3f, StatusEffect = StatusEffect.EffectType.Stun }
                }
            });
            
            // ===== DEFENSIVE SKILL TREE - PASSIVES =====
            // Iron Wall
            AddSkill(new SkillData
            {
                Id = 401,
                Name = "铁壁",
                Description = "提升防御力",
                Type = SkillData.SkillType.Passive,
                Tree = SkillTreeType.Defensive,
                IsPassive = true,
                LevelRequired = 1,
                MaxLevel = 5,
                Effects = new List<SkillEffectData>
                {
                    new SkillEffectData { EffectType = SkillEffectType.Buff, Value = 5 } // Defense bonus
                }
            });
            
            // Vitality
            AddSkill(new SkillData
            {
                Id = 402,
                Name = "生命强化",
                Description = "提升最大生命值",
                Type = SkillData.SkillType.Passive,
                Tree = SkillTreeType.Defensive,
                IsPassive = true,
                LevelRequired = 1,
                MaxLevel = 5,
                Effects = new List<SkillEffectData>
                {
                    new SkillEffectData { EffectType = SkillEffectType.Buff, Value = 20 } // HP bonus
                }
            });
            
            // Evasion
            AddSkill(new SkillData
            {
                Id = 403,
                Name = "闪避",
                Description = "提升闪避率",
                Type = SkillData.SkillType.Passive,
                Tree = SkillTreeType.Defensive,
                IsPassive = true,
                LevelRequired = 2,
                MaxLevel = 3,
                Effects = new List<SkillEffectData>
                {
                    new SkillEffectData { EffectType = SkillEffectType.Buff, Value = 0.05f } // Dodge bonus
                }
            });
            
            // ===== UTILITY SKILL TREE - PASSIVES =====
            // Wisdom
            AddSkill(new SkillData
            {
                Id = 501,
                Name = "智慧",
                Description = "提升法力上限",
                Type = SkillData.SkillType.Passive,
                Tree = SkillTreeType.Utility,
                IsPassive = true,
                LevelRequired = 1,
                MaxLevel = 5,
                Effects = new List<SkillEffectData>
                {
                    new SkillEffectData { EffectType = SkillEffectType.Buff, Value = 15 } // Mana bonus
                }
            });
            
            // Critical Strike
            AddSkill(new SkillData
            {
                Id = 502,
                Name = "暴击",
                Description = "提升暴击率",
                Type = SkillData.SkillType.Passive,
                Tree = SkillTreeType.Utility,
                IsPassive = true,
                LevelRequired = 2,
                MaxLevel = 5,
                Effects = new List<SkillEffectData>
                {
                    new SkillEffectData { EffectType = SkillEffectType.Buff, Value = 0.03f } // Crit bonus
                }
            });
            
            // Lucky
            AddSkill(new SkillData
            {
                Id = 503,
                Name = "幸运",
                Description = "提升掉落率",
                Type = SkillData.SkillType.Passive,
                Tree = SkillTreeType.Utility,
                IsPassive = true,
                LevelRequired = 3,
                MaxLevel = 3,
                Effects = new List<SkillEffectData>
                {
                    new SkillEffectData { EffectType = SkillEffectType.Buff, Value = 0.1f } // Drop rate bonus
                }
            });
            
            // Wealth
            AddSkill(new SkillData
            {
                Id = 504,
                Name = "财富",
                Description = "提升金币获取",
                Type = SkillData.SkillType.Passive,
                Tree = SkillTreeType.Utility,
                IsPassive = true,
                LevelRequired = 2,
                MaxLevel = 5,
                Effects = new List<SkillEffectData>
                {
                    new SkillEffectData { EffectType = SkillEffectType.Buff, Value = 0.15f } // Gold bonus
                }
            });
            
            // Experience Boost
            AddSkill(new SkillData
            {
                Id = 505,
                Name = "经验加成",
                Description = "提升经验获取",
                Type = SkillData.SkillType.Passive,
                Tree = SkillTreeType.Utility,
                IsPassive = true,
                LevelRequired = 3,
                MaxLevel = 5,
                Effects = new List<SkillEffectData>
                {
                    new SkillEffectData { EffectType = SkillEffectType.Buff, Value = 0.1f } // XP bonus
                }
            });
        }
        
        private void AddSkill(SkillData skill)
        {
            _skills[skill.Id] = skill;
        }
        
        public SkillData GetSkill(int id)
        {
            return _skills.TryGetValue(id, out var skill) ? skill : null;
        }
        
        public List<SkillData> GetAllSkills()
        {
            return new List<SkillData>(_skills.Values);
        }
        
        public List<SkillData> GetSkillsByTree(SkillTreeType tree)
        {
            var result = new List<SkillData>();
            foreach (var skill in _skills.Values)
            {
                if (skill.Tree == tree)
                    result.Add(skill);
            }
            return result;
        }
        
        public List<SkillData> GetAvailableSkills(List<int> learnedSkillIds, int playerLevel)
        {
            var result = new List<SkillData>();
            foreach (var skill in _skills.Values)
            {
                if (skill.LevelRequired > playerLevel) continue;
                if (learnedSkillIds.Contains(skill.Id)) continue;
                if (skill.RequiredSkillId > 0 && !learnedSkillIds.Contains(skill.RequiredSkillId)) continue;
                result.Add(skill);
            }
            return result;
        }
        
        public List<SkillData> GetPassiveSkills(List<int> learnedSkillIds)
        {
            var result = new List<SkillData>();
            foreach (var id in learnedSkillIds)
            {
                var skill = GetSkill(id);
                if (skill != null && skill.IsPassive)
                    result.Add(skill);
            }
            return result;
        }
    }
}
