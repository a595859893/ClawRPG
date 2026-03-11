using Godot;
using System;
using System.Collections.Generic;

namespace ClawRPG.Scripts.Systems.Pets
{
    /// <summary>
    /// 宠物技能数据库
    /// </summary>
    public static class PetSkillDatabase
    {
        private static Dictionary<string, PetSkill> _skills = new Dictionary<string, PetSkill>();
        private static bool _initialized = false; 

        public static void Initialize()
        {
            if (_initialized) return;
            
            // 攻击技能
            AddSkill(new PetSkill
            {
                SkillId = "pet_scratch",
                SkillName = "抓挠",
                Description = "基础攻击技能，对单个敌人造成伤害",
                Type = PetSkillType.Attack,
                Target = PetSkillTarget.Enemy,
                Damage = 30,
                Cooldown = 2f,
                Range = 80f,
                RequiredLevel = 1,
                SkillPointCost = 1,
                Rarity = PetSkillRarity.Common
            });

            AddSkill(new PetSkill
            {
                SkillId = "pet_bite",
                SkillName = "撕咬",
                Description = "凶猛撕咬敌人，造成较高伤害",
                Type = PetSkillType.Attack,
                Target = PetSkillTarget.Enemy,
                Damage = 50,
                Cooldown = 3f,
                Range = 80f,
                RequiredLevel = 5,
                SkillPointCost = 1,
                Rarity = PetSkillRarity.Uncommon
            });

            AddSkill(new PetSkill
            {
                SkillId = "pet_fang_strike",
                SkillName = "利牙突击",
                Description = "快速突击目标，有几率造成眩晕",
                Type = PetSkillType.Attack,
                Target = PetSkillTarget.Enemy,
                Damage = 65,
                Cooldown = 4f,
                Range = 120f,
                StunDuration = 1f,
                RequiredLevel = 10,
                SkillPointCost = 1,
                Rarity = PetSkillRarity.Rare
            });

            AddSkill(new PetSkill
            {
                SkillId = "pet_swipe",
                SkillName = "横扫",
                Description = "AOE攻击技能，对周围所有敌人造成伤害",
                Type = PetSkillType.Attack,
                Target = PetSkillTarget.EnemyAoe,
                Damage = 40,
                Cooldown = 5f,
                Range = 60f,
                AoeRadius = 100f,
                RequiredLevel = 8,
                SkillPointCost = 2,
                Rarity = PetSkillRarity.Rare
            });

            AddSkill(new PetSkill
            {
                SkillId = "pet_fury",
                SkillName = "狂怒打击",
                Description = "全力一击，造成巨大伤害",
                Type = PetSkillType.Attack,
                Target = PetSkillTarget.Enemy,
                Damage = 100,
                DamageMultiplier = 0.5f, // 500% of pet attack
                Cooldown = 8f,
                Range = 80f,
                RequiredLevel = 20,
                SkillPointCost = 2,
                Rarity = PetSkillRarity.Epic
            });

            AddSkill(new PetSkill
            {
                SkillId = "pet_feral_rage",
                SkillName = "野性狂怒",
                Description = "传说技能，对范围内所有敌人造成巨额伤害",
                Type = PetSkillType.Attack,
                Target = PetSkillTarget.EnemyAoe,
                Damage = 150,
                DamageMultiplier = 1f,
                Cooldown = 15f,
                Range = 80f,
                AoeRadius = 150f,
                RequiredLevel = 30,
                SkillPointCost = 3,
                Rarity = PetSkillRarity.Legendary
            });

            // 防御技能
            AddSkill(new PetSkill
            {
                SkillId = "pet_shield",
                SkillName = "护盾",
                Description = "为自身或玩家提供护盾",
                Type = PetSkillType.Defense,
                Target = PetSkillTarget.Self,
                ShieldAmount = 50,
                Cooldown = 10f,
                Range = 0f,
                RequiredLevel = 3,
                SkillPointCost = 1,
                Rarity = PetSkillRarity.Common
            });

            AddSkill(new PetSkill
            {
                SkillId = "pet_guardian",
                SkillName = "守护之光",
                Description = "为玩家提供大量护盾",
                Type = PetSkillType.Defense,
                Target = PetSkillTarget.Player,
                ShieldAmount = 100,
                Cooldown = 15f,
                Range = 200f,
                RequiredLevel = 15,
                SkillPointCost = 2,
                Rarity = PetSkillRarity.Rare
            });

            AddSkill(new PetSkill
            {
                SkillId = "pet_mirror_shield",
                SkillName = "镜面护盾",
                Description = "反射部分伤害给攻击者",
                Type = PetSkillType.Defense,
                Target = PetSkillTarget.Self,
                ShieldAmount = 80,
                Cooldown = 20f,
                Range = 0f,
                RequiredLevel = 25,
                SkillPointCost = 2,
                Rarity = PetSkillRarity.Epic
            });

            // 治疗技能
            AddSkill(new PetSkill
            {
                SkillId = "pet_heal",
                SkillName = "治疗",
                Description = "恢复目标生命值",
                Type = PetSkillType.Heal,
                Target = PetSkillTarget.Player,
                HealAmount = 30,
                Cooldown = 8f,
                Range = 150f,
                RequiredLevel = 5,
                SkillPointCost = 1,
                Rarity = PetSkillRarity.Uncommon
            });

            AddSkill(new PetSkill
            {
                SkillId = "pet_group_heal",
                SkillName = "群体治疗",
                Description = "恢复范围内所有友方生命",
                Type = PetSkillType.Heal,
                Target = PetSkillTarget.PlayerAoe,
                HealAmount = 40,
                Cooldown = 12f,
                Range = 100f,
                AoeRadius = 120f,
                RequiredLevel = 15,
                SkillPointCost = 2,
                Rarity = PetSkillRarity.Rare
            });

            AddSkill(new PetSkill
            {
                SkillId = "pet_regeneration",
                SkillName = "再生",
                Description = "为目标添加持续恢复效果",
                Type = PetSkillType.Heal,
                Target = PetSkillTarget.Player,
                HealPercent = 0.05f, // 5% max health per tick
                Cooldown = 20f,
                Range = 150f,
                RequiredLevel = 20,
                SkillPointCost = 2,
                Rarity = PetSkillRarity.Epic
            });

            AddSkill(new PetSkill
            {
                SkillId = "pet_lifebloom",
                SkillName = "生命绽放",
                Description = "传说治疗技能，大量恢复生命",
                Type = PetSkillType.Heal,
                Target = PetSkillTarget.Player,
                HealAmount = 150,
                HealPercent = 0.2f,
                Cooldown = 30f,
                Range = 200f,
                RequiredLevel = 30,
                SkillPointCost = 3,
                Rarity = PetSkillRarity.Legendary
            });

            // 辅助技能
            AddSkill(new PetSkill
            {
                SkillId = "pet_speed",
                SkillName = "加速",
                Description = "提升目标移动速度",
                Type = PetSkillType.Support,
                Target = PetSkillTarget.Player,
                SlowAmount = 0.3f, // 30% speed increase (stored as negative slow)
                Cooldown = 15f,
                Range = 150f,
                RequiredLevel = 8,
                SkillPointCost = 1,
                Rarity = PetSkillRarity.Uncommon
            });

            AddSkill(new PetSkill
            {
                SkillId = "pet_blessing",
                SkillName = "祝福",
                Description = "提升目标攻击力",
                Type = PetSkillType.Support,
                Target = PetSkillTarget.Player,
                Cooldown = 20f,
                Range = 150f,
                RequiredLevel = 12,
                SkillPointCost = 2,
                Rarity = PetSkillRarity.Rare
            });

            AddSkill(new PetSkill
            {
                SkillId = "pet_aura",
                SkillName = "光环",
                Description = "为范围内所有友方提供属性加成",
                Type = PetSkillType.Support,
                Target = PetSkillTarget.PlayerAoe,
                Cooldown = 30f,
                Range = 50f,
                AoeRadius = 100f,
                RequiredLevel = 25,
                SkillPointCost = 3,
                Rarity = PetSkillRarity.Epic
            });

            // 减益技能
            AddSkill(new PetSkill
            {
                SkillId = "pet_bark",
                SkillName = "咆哮",
                Description = "使敌人恐惧并减速",
                Type = PetSkillType.Debuff,
                Target = PetSkillTarget.Enemy,
                SlowAmount = 0.5f,
                Cooldown = 8f,
                Range = 100f,
                RequiredLevel = 10,
                SkillPointCost = 1,
                Rarity = PetSkillRarity.Rare
            });

            AddSkill(new PetSkill
            {
                SkillId = "pet_freeze",
                SkillName = "冰霜吐息",
                Description = "冰冻敌人",
                Type = PetSkillType.Debuff,
                Target = PetSkillTarget.EnemyAoe,
                FreezeDuration = 2f,
                Cooldown = 12f,
                Range = 80f,
                AoeRadius = 80f,
                RequiredLevel = 18,
                SkillPointCost = 2,
                Rarity = PetSkillRarity.Epic
            });

            AddSkill(new PetSkill
            {
                SkillId = "pet_inferno",
                SkillName = "地狱火",
                Description = "燃烧范围内所有敌人",
                Type = PetSkillType.Debuff,
                Target = PetSkillTarget.EnemyAoe,
                BurnDamage = 10f,
                Cooldown = 15f,
                Range = 80f,
                AoeRadius = 100f,
                RequiredLevel = 22,
                SkillPointCost = 2,
                Rarity = PetSkillRarity.Epic
            });

            _initialized = true;
            GD.Print($"宠物技能数据库已初始化: {_skills.Count} 个技能");
        }

        private static void AddSkill(PetSkill skill)
        {
            _skills[skill.SkillId] = skill;
        }

        public static PetSkill GetSkill(string skillId)
        {
            if (_skills.TryGetValue(skillId, out var skill))
                return skill;
            return null;
        }

        public static List<PetSkill> GetAllSkills()
        {
            return new List<PetSkill>(_skills.Values);
        }

        public static List<PetSkill> GetSkillsByType(PetSkillType type)
        {
            var result = new List<PetSkill>();
            foreach (var skill in _skills.Values)
            {
                if (skill.Type == type)
                    result.Add(skill);
            }
            return result;
        }

        public static List<PetSkill> GetSkillsByRarity(PetSkillRarity rarity)
        {
            var result = new List<PetSkill>();
            foreach (var skill in _skills.Values)
            {
                if (skill.Rarity == rarity)
                    result.Add(skill);
            }
            return result;
        }

        public static List<PetSkill> GetAvailableSkills(int petLevel)
        {
            var result = new List<PetSkill>();
            foreach (var skill in _skills.Values)
            {
                if (skill.RequiredLevel <= petLevel)
                    result.Add(skill);
            }
            return result;
        }
    }
}
