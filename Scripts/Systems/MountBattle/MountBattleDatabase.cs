using Godot;
using System;
using System.Collections.Generic;

namespace ClawRPG.Scripts.Systems.MountBattle {
    /// <summary>
    /// 坐骑战斗数据库 - Mount Battle Database
    /// 配置所有坐骑战斗相关数据
    /// </summary>
    public class MountBattleDatabase {
        // 坐骑战斗技能数据库
        public static Dictionary<string, MountBattleSkill> MountSkills = new Dictionary<string, MountBattleSkill> {
            // 攻击技能
            {"charge", new MountBattleSkill {
                Id = "charge",
                Name = "冲锋",
                Description = "快速冲向敌人，造成150%伤害",
                Category = "Attack",
                BaseDamage = 150f,
                Cooldown = 8f,
                ManaCost = 20,
                UnlockLevel = 1,
                MaxLevel = 5,
                ScalingPerLevel = 0.1f
            }},
            {"trample", new MountBattleSkill {
                Id = "trample",
                Name = "践踏",
                Description = "踩踏地面，对周围敌人造成100%伤害并减速",
                Category = "Attack",
                BaseDamage = 100f,
                Cooldown = 12f,
                ManaCost = 30,
                UnlockLevel = 3,
                MaxLevel = 5,
                ScalingPerLevel = 0.1f,
                AreaEffect = true,
                EffectRadius = 5f
            }},
            {"piercing_strike", new MountBattleSkill {
                Id = "piercing_strike",
                Name = "穿刺",
                Description = "强力穿刺攻击，忽视敌人50%防御",
                Category = "Attack",
                BaseDamage = 200f,
                Cooldown = 15f,
                ManaCost = 40,
                IgnoreDefense = 0.5f,
                UnlockLevel = 5,
                MaxLevel = 5,
                ScalingPerLevel = 0.15f
            }},
            {"whirlwind", new MountBattleSkill {
                Id = "whirlwind",
                Name = "旋风",
                Description = "旋转攻击所有周围敌人",
                Category = "Attack",
                BaseDamage = 80f,
                Cooldown = 10f,
                ManaCost = 25,
                UnlockLevel = 4,
                MaxLevel = 5,
                ScalingPerLevel = 0.1f,
                AreaEffect = true,
                EffectRadius = 4f,
                HitsMultipleTargets = true
            }},
            
            // 防御技能
            {"mount_shield", new MountBattleSkill {
                Id = "mount_shield",
                Name = "坐骑护盾",
                Description = "为坐骑添加护盾，吸收伤害",
                Category = "Defense",
                BaseShield = 100f,
                Cooldown = 20f,
                ManaCost = 30,
                UnlockLevel = 2,
                MaxLevel = 5,
                ScalingPerLevel = 0.15f
            }},
            {"block", new MountBattleSkill {
                Id = "block",
                Name = "格挡",
                Description = "举起武器格挡，减少60%受到的伤害",
                Category = "Defense",
                Cooldown = 5f,
                ManaCost = 10,
                DamageReduction = 0.6f,
                UnlockLevel = 1,
                MaxLevel = 5,
                ScalingPerLevel = 0.05f
            }},
            {"evasive_maneuver", new MountBattleSkill {
                Id = "evasive_maneuver",
                Name = "闪避机动",
                Description = "快速闪避，有50%几率完全闪避攻击",
                Category = "Defense",
                Cooldown = 15f,
                ManaCost = 25,
                DodgeChance = 0.5f,
                UnlockLevel = 3,
                MaxLevel = 5,
                ScalingPerLevel = 0.1f
            }},
            
            // 辅助技能
            {"healing_mount", new MountBattleSkill {
                Id = "healing_mount",
                Name = "坐骑治疗",
                Description = "恢复坐骑生命值",
                Category = "Support",
                BaseHeal = 80f,
                Cooldown = 25f,
                ManaCost = 35,
                UnlockLevel = 2,
                MaxLevel = 5,
                ScalingPerLevel = 0.15f
            }},
            {"speed_boost", new MountBattleSkill {
                Id = "speed_boost",
                Name = "速度爆发",
                Description = "提升移动速度和攻击速度50%",
                Category = "Support",
                Cooldown = 30f,
                ManaCost = 30,
                SpeedBoost = 0.5f,
                AttackSpeedBoost = 0.5f,
                Duration = 8f,
                UnlockLevel = 4,
                MaxLevel = 5,
                ScalingPerLevel = 0.1f
            }},
            {"mount_inspire", new MountBattleSkill {
                Id = "mount_inspire",
                Name = "坐骑激励",
                Description = "激励队友，提升攻击力30%",
                Category = "Support",
                Cooldown = 40f,
                ManaCost = 40,
                AllyAttackBoost = 0.3f,
                Duration = 10f,
                UnlockLevel = 5,
                MaxLevel = 5,
                ScalingPerLevel = 0.1f
            }},
            
            // 终极技能
            {"mount_rampage", new MountBattleSkill {
                Id = "mount_rampage",
                Name = "坐骑狂怒",
                Description = "疯狂攻击所有周围敌人，持续5秒",
                Category = "Ultimate",
                BaseDamage = 300f,
                Cooldown = 90f,
                ManaCost = 80,
                AreaEffect = true,
                EffectRadius = 8f,
                Duration = 5f,
                UnlockLevel = 8,
                MaxLevel = 3,
                ScalingPerLevel = 0.2f
            }},
            {"mount_summon", new MountBattleSkill {
                Id = "mount_summon",
                Name = "召唤支援",
                Description = "召唤坐骑幽灵协同作战",
                Category = "Ultimate",
                Cooldown = 120f,
                ManaCost = 100,
                SummonDuration = 15f,
                SummonDamageBoost = 0.5f,
                UnlockLevel = 10,
                MaxLevel = 3,
                ScalingPerLevel = 0.15f
            }}
        };
        
        // 坐骑战斗等级经验
        public static Dictionary<int, int> LevelExpRequirements = new Dictionary<int, int> {
            {1, 0}, {2, 100}, {3, 250}, {4, 450}, {5, 700},
            {6, 1000}, {7, 1350}, {8, 1750}, {9, 2200}, {10, 2700},
            {11, 3250}, {12, 3850}, {13, 4500}, {14, 5200}, {15, 5950},
            {16, 6750}, {17, 7600}, {18, 8500}, {19, 9450}, {20, 10500}
        };
        
        // 段位系统
        public static Dictionary<string, RankInfo> Ranks = new Dictionary<string, RankInfo> {
            {"Bronze", new RankInfo { Name = "青铜", MinPoints = 0, MaxPoints = 500, Icon = "bronze" }},
            {"Silver", new RankInfo { Name = "白银", MinPoints = 501, MaxPoints = 1200, Icon = "silver" }},
            {"Gold", new RankInfo { Name = "黄金", MinPoints = 1201, MaxPoints = 2000, Icon = "gold" }},
            {"Platinum", new RankInfo { Name = "铂金", MinPoints = 2001, MaxPoints = 3000, Icon = "platinum" }},
            {"Diamond", new RankInfo { Name = "钻石", MinPoints = 3001, MaxPoints = 4500, Icon = "diamond" }},
            {"Master", new RankInfo { Name = "大师", MinPoints = 4501, MaxPoints = 6500, Icon = "master" }},
            {"Grandmaster", new RankInfo { Name = "宗师", MinPoints = 6501, MaxPoints = 10000, Icon = "grandmaster" }},
            {"Legend", new RankInfo { Name = "传奇", MinPoints = 10001, MaxPoints = 999999, Icon = "legend" }}
        };
        
        // 坐骑类型战斗加成
        public static Dictionary<string, MountCombatStats> MountTypeBonuses = new Dictionary<string, MountCombatStats> {
            {"horse", new MountCombatStats { AttackBonus = 0.1f, SpeedBonus = 0.15f }},
            {"griffin", new MountCombatStats { AttackBonus = 0.2f, CritChance = 0.1f }},
            {"wyvern", new MountCombatStats { AttackBonus = 0.25f, DefenseBonus = 0.1f }},
            {"unicorn", new MountCombatStats { HealthBonus = 0.2f, HealEffectiveness = 0.3f }},
            {"dragon", new MountCombatStats { AttackBonus = 0.3f, CritDamage = 0.2f }},
            {"phoenix", new MountCombatStats { DodgeChance = 0.15f, LifeSteal = 0.15f }},
            {"direwolf", new MountCombatStats { AttackBonus = 0.15f, SpeedBonus = 0.1f }},
            {"bear", new MountCombatStats { HealthBonus = 0.3f, BlockChance = 0.15f }},
            {"tiger", new MountCombatStats { CritChance = 0.15f, DodgeChance = 0.1f }},
            {"turtle", new MountCombatStats { DefenseBonus = 0.3f, BlockChance = 0.2f }}
        };
        
        // 每日任务
        public static List<DailyMountChallenge> DailyChallenges = new List<DailyMountChallenge> {
            new DailyMountChallenge { Id = "daily_wins", Name = "每日胜利", Description = "赢得3场坐骑战斗", TargetCount = 3, RewardPoints = 50, RewardGold = 100 },
            new DailyMountChallenge { Id = "daily_kills", Name = "每日击杀", Description = "在坐骑战斗中击杀5个敌人", TargetCount = 5, RewardPoints = 60, RewardGold = 120 },
            new DailyMountChallenge { Id = "daily_damage", Name = "每日输出", Description = "造成1000点伤害", TargetCount = 1000, RewardPoints = 70, RewardGold = 150 },
            new DailyMountChallenge { Id = "daily_streak", Name = "连胜", Description = "获得2连胜", TargetCount = 2, RewardPoints = 80, RewardGold = 200 }
        };
    }
    
    /// <summary>
    /// 坐骑战斗技能
    /// </summary>
    public class MountBattleSkill {
        public string Id { get; set; } = "";
        public string Name { get; set; } = "";
        public string Description { get; set; } = "";
        public string Category { get; set; } = "Attack"; // Attack/Defense/Support/Ultimate
        
        // 伤害/治疗/护盾属性
        public float BaseDamage { get; set; } = 0f;
        public float BaseHeal { get; set; } = 0f;
        public float BaseShield { get; set; } = 0f;
        
        // 冷却和消耗
        public float Cooldown { get; set; } = 10f;
        public int ManaCost { get; set; } = 0;
        
        // 特殊效果
        public float DamageReduction { get; set; } = 0f;
        public float IgnoreDefense { get; set; } = 0f;
        public float DodgeChance { get; set; } = 0f;
        public float BlockChance { get; set; } = 0f;
        
        // 区域效果
        public bool AreaEffect { get; set; } = false;
        public float EffectRadius { get; set; } = 0f;
        public bool HitsMultipleTargets { get; set; } = false;
        
        // 持续效果
        public float Duration { get; set; } = 0f;
        public float SpeedBoost { get; set; } = 0f;
        public float AttackSpeedBoost { get; set; } = 0f;
        public float AllyAttackBoost { get; set; } = 0f;
        
        // 召唤效果
        public float SummonDuration { get; set; } = 0f;
        public float SummonDamageBoost { get; set; } = 0f;
        
        // 等级相关
        public int UnlockLevel { get; set; } = 1;
        public int MaxLevel { get; set; } = 5;
        public float ScalingPerLevel { get; set; } = 0.1f;
    }
    
    /// <summary>
    /// 段位信息
    /// </summary>
    public class RankInfo {
        public string Name { get; set; } = "";
        public int MinPoints { get; set; } = 0;
        public int MaxPoints { get; set; } = 0;
        public string Icon { get; set; } = "";
    }
    
    /// <summary>
    /// 每日坐骑挑战
    /// </summary>
    public class DailyMountChallenge {
        public string Id { get; set; } = "";
        public string Name { get; set; } = "";
        public string Description { get; set; } = "";
        public int TargetCount { get; set; } = 0;
        public int RewardPoints { get; set; } = 0;
        public int RewardGold { get; set; } = 0;
    }
}
