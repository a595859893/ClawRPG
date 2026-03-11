using System;
using System.Collections.Generic;
using Godot;

/// <summary>
/// 坐骑战斗竞技场数据
/// </summary>
public class MountBattleArenaData
{
    public enum ArenaType
    {
        TrainingGround,
        BattleColosseum,
        DragonArena,
        PhoenixNest,
        ShadowRealm,
        SacredGround
    }
    
    public enum ArenaDifficulty
    {
        Easy,
        Normal,
        Hard,
        Epic,
        Legendary
    }
    
    public enum BattleState
    {
        NotStarted,
        Waiting,
        InProgress,
        Victory,
        Defeated
    }
    
    public class MountArena
    {
        public string Id { get; set; } = "";
        public string Name { get; set; } = "";
        public string Description { get; set; } = "";
        public ArenaType Type { get; set; }
        public ArenaDifficulty Difficulty { get; set; }
        public int RecommendedLevel { get; set; }
        public int TotalWaves { get; set; }
        public int EnemiesPerWave { get; set; }
        public float EnemyHealthMultiplier { get; set; }
        public float EnemyDamageMultiplier { get; set; }
        public int EntryFee { get; set; }
        public int BaseGoldReward { get; set; }
        public int BaseExpReward { get; set; }
        public List<string> RewardItems { get; set; } = new List<string>();
    }
    
    public class MountBattleInstance
    {
        public string MountId { get; set; } = "";
        public string ArenaId { get; set; } = "";
        public int CurrentWave { get; set; }
        public int EnemiesDefeated { get; set; }
        public int TotalDamageDealt { get; set; }
        public int TotalDamageTaken { get; set; }
        public int SkillsUsed { get; set; }
        public BattleState State { get; set; }
        public DateTime StartTime { get; set; }
    }
    
    public class PlayerMountArenaData
    {
        public int TotalBattles { get; set; }
        public int Victories { get; set; }
        public int Defeats { get; set; }
        public int TotalWavesCleared { get; set; }
        public int TotalGoldEarned { get; set; }
        public int TotalExpEarned { get; set; }
        public Dictionary<string, int> BestWaves { get; set; } = new Dictionary<string, int>();
        public Dictionary<string, int> BattleCount { get; set; } = new Dictionary<string, int>();
    }
}
