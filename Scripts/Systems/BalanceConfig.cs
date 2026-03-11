using Godot;
using System;
using System.Collections.Generic;

namespace ClawRPG.Scripts.Systems {
    /// <summary>
    /// 游戏平衡配置数据
    /// </summary>
    [Serializable]
    public class BalanceConfig {
        // 玩家平衡配置
        public PlayerBalance Player = new PlayerBalance();
        
        // 敌人平衡配置
        public EnemyBalance Enemy = new EnemyBalance();
        
        // 战斗平衡配置
        public CombatBalance Combat = new CombatBalance();
        
        // 物品平衡配置
        public ItemBalance Item = new ItemBalance();
        
        // 技能平衡配置
        public SkillBalance Skill = new SkillBalance();
        
        // Boss平衡配置
        public BossBalance Boss = new BossBalance();
        
        // 经验/等级平衡配置
        public XPBalance XP = new XPBalance();
        
        // 经济平衡配置
        public EconomyBalance Economy = new EconomyBalance();
    }

    [Serializable]
    public class PlayerBalance {
        public float HealthMultiplier = 1.0f;
        public float ManaMultiplier = 1.0f;
        public float StaminaMultiplier = 1.0f;
        public float AttackMultiplier = 1.0f;
        public float DefenseMultiplier = 1.0f;
        public float SpeedMultiplier = 1.0f;
        public float CritChanceMultiplier = 1.0f;
        public float CritDamageMultiplier = 1.0f;
        public float DodgeMultiplier = 1.0f;
        public float ParryMultiplier = 1.0f;
        public float BlockMultiplier = 1.0f;
        public float HealMultiplier = 1.0f;
        public float CCResistanceMultiplier = 1.0f;
    }

    [Serializable]
    public class EnemyBalance {
        public float HealthMultiplier = 1.0f;
        public float DamageMultiplier = 1.0f;
        public float DefenseMultiplier = 1.0f;
        public float SpeedMultiplier = 1.0f;
        public float XPMultiplier = 1.0f;
        public float DropRateMultiplier = 1.0f;
        public float AggroRangeMultiplier = 1.0f;
        public float ChaseRangeMultiplier = 1.0f;
    }

    [Serializable]
    public class CombatBalance {
        public float BaseDamageMultiplier = 1.0f;
        public float SkillDamageMultiplier = 1.0f;
        public float CritBaseChance = 0.05f;
        public float CritBonusDamage = 0.5f;
        public float DodgeBaseChance = 0.05f;
        public float ParryBaseChance = 0.05f;
        public float BlockBaseReduction = 0.5f;
        public float PerfectBlockReduction = 1.0f;
        public float CounterAttackDamage = 1.5f;
        public float ComboDamageBonus = 0.1f;
        public float MaxCombo = 10;
        public float AttackSpeedMultiplier = 1.0f;
        public float CastSpeedMultiplier = 1.0f;
    }

    [Serializable]
    public class ItemBalance {
        public float DropRateMultiplier = 1.0f;
        public float RareDropBonus = 0.0f;
        public float LegendaryDropBonus = 0.0f;
        public float EnchantCostMultiplier = 1.0f;
        public float EnhancementCostMultiplier = 1.0f;
        public float EnhancementSuccessRate = 1.0f;
        public float CraftCostMultiplier = 1.0f;
        public float RuneSlotChance = 0.2f;
        public float SetEffectMultiplier = 1.0f;
    }

    [Serializable]
    public class SkillBalance {
        public float CooldownMultiplier = 1.0f;
        public float ManaCostMultiplier = 1.0f;
        public float DamageMultiplier = 1.0f;
        public float DurationMultiplier = 1.0f;
        public float HealMultiplier = 1.0f;
        public float AoERadiusMultiplier = 1.0f;
        public float RangeMultiplier = 1.0f;
    }

    [Serializable]
    public class BossBalance {
        public float HealthMultiplier = 1.0f;
        public float DamageMultiplier = 1.0f;
        public float EnrageTimeMultiplier = 1.0f;
        public float EnrageDamageMultiplier = 1.5f;
        public float Phase2HealthThreshold = 0.5f;
        public float Phase3HealthThreshold = 0.25f;
        public float MinionSpawnMultiplier = 1.0f;
        public float AbilityCooldownMultiplier = 1.0f;
    }

    [Serializable]
    public class XPBalance {
        public float KillXPMultiplier = 1.0f;
        public float QuestXPMultiplier = 1.0f;
        public float BossXPMultiplier = 1.0f;
        public float ExplorationXPMultiplier = 1.0f;
        public float LevelCurveExponent = 1.5f;
        public float DailyBonusXP = 0.1f;
    }

    [Serializable]
    public class EconomyBalance {
        public float GoldDropMultiplier = 1.0f;
        public float ItemPriceMultiplier = 1.0f;
        public float SellPriceMultiplier = 0.5f;
        public float QuestRewardMultiplier = 1.0f;
        public float BountyRewardMultiplier = 1.0f;
        public float RepairCostMultiplier = 1.0f;
        public float TravelCostMultiplier = 1.0f;
    }
}
