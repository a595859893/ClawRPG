using Godot;
using System;
using System.Collections.Generic;

namespace ClawRPG.Scripts.Systems.MountBattle {
    /// <summary>
    /// 坐骑战斗数据结构 - Mount Battle Data
    /// 坐骑战斗系统允许玩家在坐骑上进行特殊战斗
    /// </summary>
    public class MountBattleData {
        // 坐骑战斗属性
        public bool IsMountBattleEnabled { get; set; } = false;
        public int CurrentMountCombatLevel { get; set; } = 1;
        public int TotalMountKills { get; set; } = 0;
        public int TotalMountDamageDealt { get; set; } = 0;
        public int TotalMountDamageTaken { get; set; } = 0;
        
        // 坐骑战斗统计
        public int Wins { get; set; } = 0;
        public int Losses { get; set; } = 0;
        public int CurrentStreak { get; set; } = 0;
        public int BestStreak { get; set; } = 0;
        
        // 激活的坐骑战斗技能
        public List<string> UnlockedMountSkills { get; set; } = new List<string>();
        public Dictionary<string, int> SkillLevels { get; set; } = new Dictionary<string, int>();
        
        // 坐骑装备
        public string EquippedMountWeapon { get; set; } = "";
        public string EquippedMountArmor { get; set; } = "";
        public string EquippedMountAccessory { get; set; } = "";
        
        // 战斗历史
        public List<MountBattleRecord> BattleHistory { get; set; } = new List<MountBattleRecord>();
        
        // 每日挑战
        public int DailyBattlesCompleted { get; set; } = 0;
        public int DailyWins { get; set; } = 0;
        public string LastDailyReset { get; set; } = "";
        
        // 赛季数据
        public int SeasonNumber { get; set; } = 1;
        public int SeasonWins { get; set; } = 0;
        public int SeasonPoints { get; set; } = 0;
        public string SeasonRank { get; set; } = "Bronze";
    }
    
    /// <summary>
    /// 坐骑战斗记录
    /// </summary>
    public class MountBattleRecord {
        public string BattleId { get; set; } = Guid.NewGuid().ToString();
        public string OpponentName { get; set; } = "";
        public string OpponentMountType { get; set; } = "";
        public bool Victory { get; set; } = false;
        public int DamageDealt { get; set; } = 0;
        public int DamageTaken { get; set; } = 0;
        public int Kills { get; set; } = 0;
        public int EarnedPoints { get; set; } = 0;
        public int EarnedExp { get; set; } = 0;
        public DateTime BattleTime { get; set; } = DateTime.Now;
        public string BattleType { get; set; } = "Normal"; // Normal/Ranked/Tournament
    }
    
    /// <summary>
    /// 坐骑战斗类型
    /// </summary>
    public enum MountBattleType {
        Normal,      // 普通练习战
        Ranked,       // 排位赛
        Tournament,   // 锦标赛
        Duel,        // 1v1对决
        FreeForAll,  // 混战
        TeamBattle   // 团队战
    }
    
    /// <summary>
    /// 坐骑战斗状态
    /// </summary>
    public enum MountBattleState {
        Idle,
        Searching,
        Preparing,
        InBattle,
        Victory,
        Defeated
    }
    
    /// <summary>
    /// 坐骑战斗属性加成
    /// </summary>
    public class MountCombatStats {
        public float AttackBonus { get; set; } = 0f;
        public float DefenseBonus { get; set; } = 0f;
        public float SpeedBonus { get; set; } = 0f;
        public float HealthBonus { get; set; } = 0f;
        public float CritChance { get; set; } = 0f;
        public float CritDamage { get; set; } = 0f;
        public float DodgeChance { get; set; } = 0f;
        public float BlockChance { get; set; } = 0f;
        public float LifeSteal { get; set; } = 0f;
        public float CooldownReduction { get; set; } = 0f;
    }
}
