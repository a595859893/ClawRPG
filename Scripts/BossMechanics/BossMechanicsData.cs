// BossMechanicsData.cs - Boss 机制系统数据结构
using Godot;
using System;
using System.Collections.Generic;

namespace ClawRPG.Scripts.BossMechanics {
    
    // Boss 类型
    public enum BossType {
        Normal,       // 普通 Boss
        Elite,        // 精英 Boss
        MiniBoss,     // 小 Boss
        WorldBoss,    // 世界 Boss
        RaidBoss,     // 副本 Boss
        TutorialBoss  // 教程 Boss
    }
    
    // Boss 元素类型
    public enum BossElement {
        Physical,
        Fire,
        Ice,
        Lightning,
        Dark,
        Light,
        Poison,
        Holy
    }
    
    // Boss AI 行为模式
    public enum BossAIBehavior {
        Aggressive,    // 激进型
        Defensive,     // 防守型
        Balanced,      // 平衡型
        Tactical,      // 战术型
        Berserker,     // 狂战士型
        Cunning        // 狡猾型
    }
    
    // 战斗阶段类型
    public enum BattlePhaseType {
        Normal,        // 正常阶段
        Enraged,       // 狂暴阶段
        Exhausted,     // 虚弱阶段
        Transformation,// 变身阶段
        Summon,        // 召唤阶段
        Special        // 特殊阶段
    }
    
    // Boss 技能类型
    public enum BossSkillType {
        Melee,         // 近战攻击
        Ranged,        // 远程攻击
        AOE,           // 范围攻击
        DoT,           // 持续伤害
        Debuff,        // 减益效果
        Heal,          // 治疗
        Buff,          // 增益效果
        Summon,        // 召唤
        Transform,     // 变身
        Ultimate       // 终极技能
    }
    
    // Boss 掉落类型
    public enum LootType {
        Equipment,     // 装备
        Currency,      // 货币
        Material,      // 材料
        Consumable,    // 消耗品
        QuestItem,     // 任务物品
        SkillBook,     // 技能书
        Pet,           // 宠物
        Mount          // 坐骑
    }
    
    // Boss 状态记录
    public class BossState {
        public string BossId { get; set; }
        public string BossName { get; set; }
        public BossType Type { get; set; }
        public BossElement Element { get; set; }
        public int Level { get; set; }
        public float MaxHealth { get; set; }
        public float CurrentHealth { get; set; }
        public float AttackDamage { get; set; }
        public float Defense { get; set; }
        public float MoveSpeed { get; set; }
        public int CurrentPhase { get; set; }
        public bool IsEnraged { get; set; }
        public float EnrageTimer { get; set; }
        public List<string> ActiveBuffs { get; set; }
        public List<string> ActiveDebuffs { get; set; }
        public DateTime LastAttackTime { get; set; }
        public int TotalDamageDealt { get; set; }
        public int TotalDamageTaken { get; set; }
        
        public BossState() {
            ActiveBuffs = new List<string>();
            ActiveDebuffs = new List<string>();
        }
        
        public float HealthPercentage => MaxHealth > 0 ? CurrentHealth / MaxHealth : 0;
        public bool IsAlive => CurrentHealth > 0;
    }
    
    // Boss 技能数据
    public class BossSkill {
        public string SkillId { get; set; }
        public string SkillName { get; set; }
        public BossSkillType Type { get; set; }
        public float Damage { get; set; }
        public float Cooldown { get; set; }
        public float CastTime { get; set; }
        public float Range { get; set; }
        public float AreaRadius { get; set; }
        public int Cost { get; set; }
        public string Description { get; set; }
        public string IconPath { get; set; }
        public List<string> Effects { get; set; }
        public int PhaseUnlock { get; set; }
        public float Weight { get; set; }
        
        public BossSkill() {
            Effects = new List<string>();
        }
    }
    
    // 战斗阶段数据
    public class BattlePhase {
        public int PhaseNumber { get; set; }
        public BattlePhaseType Type { get; set; }
        public string PhaseName { get; set; }
        public float HealthThreshold { get; set; }
        public float DamageMultiplier { get; set; }
        public float DefenseMultiplier { get; set; }
        public float SpeedMultiplier { get; set; }
        public List<string> UnlockedSkills { get; set; }
        public List<string> PhaseBuffs { get; set; }
        public string PhaseDescription { get; set; }
        public float Duration { get; set; }
        
        public BattlePhase() {
            UnlockedSkills = new List<string>();
            PhaseBuffs = new List<string>();
        }
    }
    
    // Boss 掉落记录
    public class BossLoot {
        public string LootId { get; set; }
        public LootType Type { get; set; }
        public string ItemId { get; set; }
        public string ItemName { get; set; }
        public int MinQuantity { get; set; }
        public int MaxQuantity { get; set; }
        public float DropRate { get; set; }
        public bool IsGuaranteed { get; set; }
        public string Condition { get; set; }
    }
    
    // Boss 战斗记录
    public class BossBattleRecord {
        public string RecordId { get; set; }
        public string BossId { get; set; }
        public string BossName { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public bool IsVictory { get; set; }
        public int DamageDealt { get; set; }
        public int DamageTaken { get; set; }
        public int PhaseReached { get; set; }
        public List<string> LootReceived { get; set; }
        public List<string> SkillsUsed { get; set; }
        public int StarsEarned { get; set; }
        
        public BossBattleRecord() {
            LootReceived = new List<string>();
            SkillsUsed = new List<string>();
        }
        
        public TimeSpan Duration => EndTime - StartTime;
    }
    
    // Boss 统计信息
    public class BossStatistics {
        public int TotalBattles { get; set; }
        public int Victories { get; set; }
        public int Defeats { get; set; }
        public int TotalDamageDealt { get; set; }
        public int TotalDamageTaken { get; set; }
        public Dictionary<string, int> BossKills { get; set; }
        public Dictionary<string, int> BossDeaths { get; set; }
        public List<string> UnlockedBosses { get; set; }
        public int HighestPhaseReached { get; set; }
        public int TotalLootCollected { get; set; }
        
        public BossStatistics() {
            BossKills = new Dictionary<string, int>();
            BossDeaths = new Dictionary<string, int>();
            UnlockedBosses = new List<string>();
        }
        
        public float WinRate => TotalBattles > 0 ? (float)Victories / TotalBattles * 100 : 0;
    }
    
    // Boss AI 配置
    public class BossAIConfig {
        public BossAIBehavior Behavior { get; set; }
        public float AggressionLevel { get; set; }
        public float DefensiveThreshold { get; set; }
        public float SkillUsageRate { get; set; }
        public float PriorityHealThreshold { get; set; }
        public float UltimateAbilityThreshold { get; set; }
        public bool UseEnvironment { get; set; }
        public bool CallReinforcements { get; set; }
        public float RetreatThreshold { get; set; }
    }
    
    // Boss 进度数据
    public class BossProgress {
        public Dictionary<string, bool> UnlockedBosses { get; set; }
        public Dictionary<string, int> BestPhases { get; set; }
        public Dictionary<string, int> BestStars { get; set; }
        public Dictionary<string, bool> AchievementsCompleted { get; set; }
        public List<string> FavoriteBosses { get; set; }
        
        public BossProgress() {
            UnlockedBosses = new Dictionary<string, bool>();
            BestPhases = new Dictionary<string, int>();
            BestStars = new Dictionary<string, int>();
            AchievementsCompleted = new Dictionary<string, bool>();
            FavoriteBosses = new List<string>();
        }
    }
}
