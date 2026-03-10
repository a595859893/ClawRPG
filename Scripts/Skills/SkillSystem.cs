using Godot;
using System;
using System.Collections.Generic;

namespace ClawRPG.Scripts.Skills {
    /// <summary>
    /// Skill base class
    /// </summary>
    public class Skill
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public string Description { get; set; } = "";
        public SkillType Type { get; set; }
        public int ManaCost { get; set; }
        public float Cooldown { get; set; } = 5f;
        public float CastTime { get; set; } // Instant if 0
        public int LevelRequired { get; set; } = 1;
        
        // Combat effects
        public float Damage { get; set; }
        public float DamageMultiplier { get; set; } = 1f; // Based on attack
        
        // Status effects
        public StatusEffect.EffectType? ApplyStatusEffect { get; set; }
        public float StatusEffectDamage { get; set; }
        public float StatusEffectDuration { get; set; }
        
        // Buffs
        public float AttackBoost { get; set; }
        public float DefenseBoost { get; set; }
        public float SpeedBoost { get; set; }
        public float DamageReduction { get; set; }
        public float BuffDuration { get; set; }
        
        // Special
        public bool IsAOE { get; set; }
        public float AOERadius { get; set; }
        public bool Heals { get; set; }
        public int HealAmount { get; set; }
        
        public enum SkillType { Attack, Healing, Buff, Debuff }
    }
    
    /// <summary>
    /// Skill database - contains all game skills
    /// </summary>
    public class SkillDatabase
    {
        private static SkillDatabase _instance;
        public static SkillDatabase Instance => _instance ??= new SkillDatabase();
        
        private Dictionary<int, Skill> _skills = new();
        
        public SkillDatabase()
        {
            Initialize();
        }
        
        private void Initialize()
        {
            // Attack Skills
            AddSkill(new Skill 
            { 
                Id = 1, Name = "闪电箭", Description = "发射一支闪电箭", 
                Type = Skill.SkillType.Attack, ManaCost = 10, Cooldown = 3f, 
                Damage = 30, LevelRequired = 1 
            });
            
            AddSkill(new Skill 
            { 
                Id = 2, Name = "陨石", Description = "召唤陨石砸向敌人", 
                Type = Skill.SkillType.Attack, ManaCost = 30, Cooldown = 10f, 
                Damage = 80, IsAOE = true, AOERadius = 100, LevelRequired = 3 
            });
            
            AddSkill(new Skill 
            { 
                Id = 3, Name = "圣光打击", Description = "神圣力量打击", 
                Type = Skill.SkillType.Attack, ManaCost = 15, Cooldown = 4f, 
                Damage = 40, ApplyStatusEffect = StatusEffect.EffectType.Stun, 
                StatusEffectDuration = 2f, LevelRequired = 2 
            });
            
            AddSkill(new Skill 
            { 
                Id = 4, Name = "暗影箭", Description = "暗影箭矢", 
                Type = Skill.SkillType.Attack, ManaCost = 12, Cooldown = 3f, 
                Damage = 35, ApplyStatusEffect = StatusEffect.EffectType.Slow, 
                StatusEffectDuration = 3f, LevelRequired = 2 
            });
            
            AddSkill(new Skill 
            { 
                Id = 5, Name = "旋风斩", Description = "旋转攻击周围敌人", 
                Type = Skill.SkillType.Attack, ManaCost = 20, Cooldown = 6f, 
                Damage = 25, DamageMultiplier = 3f, IsAOE = true, AOERadius = 80, 
                LevelRequired = 3 
            });
            
            AddSkill(new Skill 
            { 
                Id = 6, Name = "毒箭", Description = "毒属性箭矢", 
                Type = Skill.SkillType.Attack, ManaCost = 8, Cooldown = 2f, 
                Damage = 15, ApplyStatusEffect = StatusEffect.EffectType.Poison, 
                StatusEffectDamage = 5, StatusEffectDuration = 5f, LevelRequired = 1 
            });
            
            AddSkill(new Skill 
            { 
                Id = 7, Name = "燃烧弹", Description = "投掷燃烧弹", 
                Type = Skill.SkillType.Attack, ManaCost = 15, Cooldown = 5f, 
                Damage = 30, ApplyStatusEffect = StatusEffect.EffectType.Burn, 
                StatusEffectDamage = 8, StatusEffectDuration = 4f, IsAOE = true, 
                AOERadius = 60, LevelRequired = 2 
            });
            
            AddSkill(new Skill 
            { 
                Id = 8, Name = "冰霜新星", Description = "冰冻周围敌人", 
                Type = Skill.SkillType.Attack, ManaCost = 20, Cooldown = 8f, 
                Damage = 25, ApplyStatusEffect = StatusEffect.EffectType.Freeze, 
                StatusEffectDuration = 2f, IsAOE = true, AOERadius = 120, 
                LevelRequired = 3 
            });
            
            AddSkill(new Skill 
            { 
                Id = 9, Name = "暗影之刺", Description = "暗影突刺", 
                Type = Skill.SkillType.Attack, ManaCost = 18, Cooldown = 5f, 
                Damage = 45, ApplyStatusEffect = StatusEffect.EffectType.Slow, 
                StatusEffectDuration = 4f, LevelRequired = 3 
            });
            
            AddSkill(new Skill 
            { 
                Id = 10, Name = "链式闪电", Description = "连锁闪电攻击", 
                Type = Skill.SkillType.Attack, ManaCost = 25, Cooldown = 7f, 
                Damage = 35, ApplyStatusEffect = StatusEffect.EffectType.Paralyze, 
                StatusEffectDuration = 1.5f, IsAOE = true, AOERadius = 150, 
                LevelRequired = 4 
            });
            
            // Healing Skills
            AddSkill(new Skill 
            { 
                Id = 101, Name = "治疗术", Description = "恢复目标生命", 
                Type = Skill.SkillType.Healing, ManaCost = 15, Cooldown = 5f, 
                Heals = true, HealAmount = 40, LevelRequired = 1 
            });
            
            AddSkill(new Skill 
            { 
                Id = 102, Name = "群体治疗", Description = "恢复范围内友军生命", 
                Type = Skill.SkillType.Healing, ManaCost = 30, Cooldown = 10f, 
                Heals = true, HealAmount = 60, IsAOE = true, AOERadius = 100, 
                LevelRequired = 3 
            });
            
            AddSkill(new Skill 
            { 
                Id = 103, Name = "再生", Description = "持续恢复生命", 
                Type = Skill.SkillType.Healing, ManaCost = 20, Cooldown = 15f, 
                Heals = true, HealAmount = 10, BuffDuration = 10f, 
                LevelRequired = 2 
            });
            
            // Buff Skills
            AddSkill(new Skill 
            { 
                Id = 201, Name = "加速", Description = "提升移动速度", 
                Type = Skill.SkillType.Buff, ManaCost = 10, Cooldown = 20f, 
                SpeedBoost = 1.5f, BuffDuration = 10f, LevelRequired = 1 
            });
            
            AddSkill(new Skill 
            { 
                Id = 202, Name = "无敌", Description = "短时间无敌", 
                Type = Skill.SkillType.Buff, ManaCost = 50, Cooldown = 60f, 
                DamageReduction = 1f, BuffDuration = 3f, LevelRequired = 5 
            });
            
            AddSkill(new Skill 
            { 
                Id = 203, Name = "魔法护盾", Description = "魔法护盾保护", 
                Type = Skill.SkillType.Buff, ManaCost = 25, Cooldown = 30f, 
                DefenseBoost = 20, BuffDuration = 15f, LevelRequired = 3 
            });
            
            AddSkill(new Skill 
            { 
                Id = 204, Name = "圣光护盾", Description = "神圣护盾", 
                Type = Skill.SkillType.Buff, ManaCost = 30, Cooldown = 25f, 
                ApplyStatusEffect = StatusEffect.EffectType.Shield, 
                StatusEffectDuration = 10f, BuffDuration = 10f, LevelRequired = 4 
            });
            
            // Debuff Skills
            AddSkill(new Skill 
            { 
                Id = 301, Name = "缓速", Description = "降低敌人速度", 
                Type = Skill.SkillType.Debuff, ManaCost = 10, Cooldown = 8f, 
                ApplyStatusEffect = StatusEffect.EffectType.Slow, 
                StatusEffectDuration = 5f, LevelRequired = 2 
            });
            
            AddSkill(new Skill 
            { 
                Id = 302, Name = "眩晕", Description = "眩晕敌人", 
                Type = Skill.SkillType.Debuff, ManaCost = 20, Cooldown = 12f, 
                ApplyStatusEffect = StatusEffect.EffectType.Stun, 
                StatusEffectDuration = 3f, LevelRequired = 4 
            });
        }
        
        private void AddSkill(Skill skill)
        {
            _skills[skill.Id] = skill;
        }
        
        public Skill GetSkill(int id)
        {
            return _skills.ContainsKey(id) ? _skills[id] : null;
        }
        
        public List<Skill> GetAllSkills()
        {
            return new List<Skill>(_skills.Values);
        }
        
        public List<Skill> GetSkillsByType(Skill.SkillType type)
        {
            var result = new List<Skill>();
            foreach (var skill in _skills.Values)
            {
                if (skill.Type == type)
                    result.Add(skill);
            }
            return result;
        }
        
        public List<Skill> GetAvailableSkills(int playerLevel)
        {
            var result = new List<Skill>();
            foreach (var skill in _skills.Values)
            {
                if (skill.LevelRequired <= playerLevel)
                    result.Add(skill);
            }
            return result;
        }
    }
    
    /// <summary>
    /// Skill manager - handles skill usage and cooldowns
    /// </summary>
    public class SkillManager
    {
        private Dictionary<int, float> _cooldowns = new();
        private HashSet<int> _learnedSkills = new();
        
        public void LearnSkill(int skillId)
        {
            _learnedSkills.Add(skillId);
            GD.Print("Learned skill: " + SkillDatabase.Instance.GetSkill(skillId)?.Name);
        }
        
        public bool HasLearned(int skillId)
        {
            return _learnedSkills.Contains(skillId);
        }
        
        public List<Skill> GetLearnedSkills()
        {
            var result = new List<Skill>();
            foreach (var id in _learnedSkills)
            {
                var skill = SkillDatabase.Instance.GetSkill(id);
                if (skill != null) result.Add(skill);
            }
            return result;
        }
        
        public bool IsSkillOnCooldown(int skillId)
        {
            if (!_cooldowns.ContainsKey(skillId)) return false;
            return _cooldowns[skillId] > 0;
        }
        
        public float GetSkillCooldown(int skillId)
        {
            return _cooldowns.ContainsKey(skillId) ? _cooldowns[skillId] : 0;
        }
        
        public void UseSkill(int skillId, CharacterBody2D user)
        {
            var skill = SkillDatabase.Instance.GetSkill(skillId);
            if (skill == null) return;
            
            // Check cooldown
            if (IsSkillOnCooldown(skillId))
            {
                GD.Print("Skill on cooldown!");
                return;
            }
            
            // Check mana (would need player reference)
            // Apply skill effects...
            
            // Start cooldown
            _cooldowns[skillId] = skill.Cooldown;
            
            GD.Print("Used skill: " + skill.Name);
        }
        
        public void Update(float delta)
        {
            // Update cooldowns
            foreach (var key in _cooldowns.Keys)
            {
                _cooldowns[key] = Math.Max(0, _cooldowns[key] - delta);
            }
        }
    }
}
