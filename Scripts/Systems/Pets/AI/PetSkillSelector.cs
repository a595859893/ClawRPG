using Godot;
using System;
using System.Collections.Generic;

namespace ClawRPG.Systems.Pets.AI
{
    /// <summary>
    /// 宠物技能选择器 - 智能选择使用哪个技能
    /// </summary>
    public partial class PetSkillSelector : BaseSystem
    {
        /// <summary>
        /// 技能类型
        /// </summary>
        public enum SkillType
        {
            Attack,      // 攻击技能
            Heal,        // 治疗技能
            Buff,        // 增益技能
            Debuff,      // 减益技能
            Special      // 特殊技能
        }
        
        /// <summary>
        /// 宠物技能
        /// </summary>
        public class PetSkill
        {
            public string Id { get; set; }
            public string Name { get; set; }
            public SkillType Type { get; set; }
            public float Cooldown { get; set; }
            public float LastUsedTime { get; set; }
            public float ManaCost { get; set; }
            public float Damage { get; set; }
            public float Range { get; set; }
            public float EffectDuration { get; set; }
        }
        
        /// <summary>
        /// 技能评估结果
        /// </summary>
        public class SkillEvaluation
        {
            public PetSkill Skill { get; set; }
            public float Score { get; set; }
            public string Reason { get; set; }
        }
        
        private List<PetSkill> _availableSkills = new List<PetSkill>();
        
        public override void _Ready()
        {
            base._Ready();
            InitializeSkills();
        }
        
        /// <summary>
        /// 初始化技能列表
        /// </summary>
        private void InitializeSkills()
        {
            // 攻击技能
            _availableSkills.Add(new PetSkill
            {
                Id = "attack_bite",
                Name = "撕咬",
                Type = SkillType.Attack,
                Cooldown = 2.0f,
                Damage = 30f,
                Range = 80f,
                ManaCost = 10f
            });
            
            _availableSkills.Add(new PetSkill
            {
                Id = "attack_claw",
                Name = "爪击",
                Type = SkillType.Attack,
                Cooldown = 1.5f,
                Damage = 25f,
                Range = 70f,
                ManaCost = 8f
            });
            
            _availableSkills.Add(new PetSkill
            {
                Id = "skill_fire_breath",
                Name = "火焰吐息",
                Type = SkillType.Attack,
                Cooldown = 8.0f,
                Damage = 80f,
                Range = 150f,
                ManaCost = 30f
            });
            
            // 治疗技能
            _availableSkills.Add(new PetSkill
            {
                Id = "heal_regenerate",
                Name = "再生",
                Type = SkillType.Heal,
                Cooldown = 10.0f,
                Damage = -40f,  // 负数表示治疗
                Range = 0f,
                ManaCost = 25f
            });
            
            // 增益技能
            _availableSkills.Add(new PetSkill
            {
                Id = "buff_might",
                Name = "力量祝福",
                Type = SkillType.Buff,
                Cooldown = 15.0f,
                EffectDuration = 30f,
                ManaCost = 20f
            });
            
            _availableSkills.Add(new PetSkill
            {
                Id = "buff_protection",
                Name = "神圣防护",
                Type = SkillType.Buff,
                Cooldown = 20.0f,
                EffectDuration = 45f,
                ManaCost = 30f
            });
            
            // 特殊技能
            _availableSkills.Add(new PetSkill
            {
                Id = "special_teleport",
                Name = "传送",
                Type = SkillType.Special,
                Cooldown = 30.0f,
                ManaCost = 40f
            });
        }
        
        /// <summary>
        /// 选择最佳技能
        /// </summary>
        public PetSkill SelectBestSkill(PetAIContext context)
        {
            var evaluations = EvaluateAllSkills(context);
            
            if (evaluations.Count == 0)
                return null;
            
            // 返回评分最高的技能
            evaluations.Sort((a, b) => b.Score.CompareTo(a.Score));
            return evaluations[0].Skill;
        }
        
        /// <summary>
        /// 评估所有技能
        /// </summary>
        public List<SkillEvaluation> EvaluateAllSkills(PetAIContext context)
        {
            var evaluations = new List<SkillEvaluation>();
            float currentTime = OS.GetUnixTimeFromSystem();
            
            foreach (var skill in _availableSkills)
            {
                // 检查冷却
                if (currentTime - skill.LastUsedTime < skill.Cooldown)
                    continue;
                
                var evaluation = EvaluateSkill(skill, context);
                if (evaluation != null)
                {
                    evaluations.Add(evaluation);
                }
            }
            
            return evaluations;
        }
        
        /// <summary>
        /// 评估单个技能
        /// </summary>
        private SkillEvaluation EvaluateSkill(PetSkill skill, PetAIContext context)
        {
            var evaluation = new SkillEvaluation
            {
                Skill = skill,
                Score = 0f,
                Reason = ""
            };
            
            // 根据技能类型评估
            switch (skill.Type)
            {
                case SkillType.Attack:
                    evaluation = EvaluateAttackSkill(skill, context, evaluation);
                    break;
                case SkillType.Heal:
                    evaluation = EvaluateHealSkill(skill, context, evaluation);
                    break;
                case SkillType.Buff:
                    evaluation = EvaluateBuffSkill(skill, context, evaluation);
                    break;
            }
            
            return evaluation;
        }
        
        /// <summary>
        /// 评估攻击技能
        /// </summary>
        private SkillEvaluation EvaluateAttackSkill(PetSkill skill, PetAIContext context, SkillEvaluation evaluation)
        {
            // 有敌人时才使用攻击技能
            if (context.NearbyEnemies.Count > 0)
            {
                evaluation.Score = 50f;
                
                // 距离加成
                var target = context.NearbyEnemies[0];
                float dist = context.PetPosition.DistanceTo(target.GlobalPosition);
                if (dist <= skill.Range)
                {
                    evaluation.Score += 30f;
                    evaluation.Reason = "目标在攻击范围内";
                }
                else
                {
                    evaluation.Score -= 20f;
                    evaluation.Reason = "目标距离过远";
                }
                
                // 冷却加成
                evaluation.Score += (60f - skill.Cooldown) * 0.5f;
            }
            else
            {
                return null;  // 没有敌人，不需要攻击
            }
            
            return evaluation;
        }
        
        /// <summary>
        /// 评估治疗技能
        /// </summary>
        private SkillEvaluation EvaluateHealSkill(PetSkill skill, PetAIContext context, SkillEvaluation evaluation)
        {
            // 血量低时使用治疗
            if (context.PetHealthPercent < 0.5f)
            {
                evaluation.Score = 70f;
                evaluation.Reason = "血量低，使用治疗";
            }
            else
            {
                evaluation.Score = 10f;
                evaluation.Reason = "血量充足";
            }
            
            return evaluation;
        }
        
        /// <summary>
        /// 评估增益技能
        /// </summary>
        private SkillEvaluation EvaluateBuffSkill(PetSkill skill, PetAIContext context, SkillEvaluation evaluation)
        {
            // 玩家在战斗时使用增益
            if (context.PlayerInCombat)
            {
                evaluation.Score = 60f;
                evaluation.Reason = "玩家在战斗中使用增益";
            }
            else
            {
                evaluation.Score = 20f;
                evaluation.Reason = "玩家不在战斗";
            }
            
            return evaluation;
        }
        
        /// <summary>
        /// 使用技能
        /// </summary>
        public void UseSkill(string skillId)
        {
            foreach (var skill in _availableSkills)
            {
                if (skill.Id == skillId)
                {
                    skill.LastUsedTime = OS.GetUnixTimeFromSystem();
                    GD.Print($"[PetSkillSelector] Used skill: {skill.Name}");
                    break;
                }
            }
        }
        
        /// <summary>
        /// 检查技能是否可用
        /// </summary>
        public bool IsSkillAvailable(string skillId)
        {
            float currentTime = OS.GetUnixTimeFromSystem();
            
            foreach (var skill in _availableSkills)
            {
                if (skill.Id == skillId)
                {
                    return currentTime - skill.LastUsedTime >= skill.Cooldown;
                }
            }
            
            return false;
        }
        
        /// <summary>
        /// 获取所有可用技能
        /// </summary>
        public List<PetSkill> GetAvailableSkills()
        {
            var available = new List<PetSkill>();
            float currentTime = OS.GetUnixTimeFromSystem();
            
            foreach (var skill in _availableSkills)
            {
                if (currentTime - skill.LastUsedTime >= skill.Cooldown)
                {
                    available.Add(skill);
                }
            }
            
            return available;
        }
        
        public override Dictionary ExportSaveData()
        {
            var data = new Dictionary();
            return data;
        }
        
        public override void ImportSaveData(Dictionary data)
        {
            // 加载数据
        }
    }
}
