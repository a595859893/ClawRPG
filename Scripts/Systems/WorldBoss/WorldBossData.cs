using System;
using System.Collections.Generic;

namespace ClawRPG.Scripts.Systems.WorldBoss
{
    /// <summary>
    /// World boss configuration data
    /// </summary>
    public class WorldBossData
    {
        /// <summary>
        /// Boss rarity tier
        /// </summary>
        public enum BossRarity
        {
            Elite,      // 精英首领
            Rare,       // 稀有首领
            Epic,       // 史诗首领
            Legendary,  // 传说首领
            Mythic      // 神级首领
        }
        
        /// <summary>
        /// Spawn condition type
        /// </summary>
        public enum SpawnCondition
        {
            Timer,          // 定时生成
            PlayerCount,    // 玩家数量触发
            EventTrigger,   // 事件触发
            Random          // 随机生成
        }
        
        /// <summary>
        /// World boss instance data
        /// </summary>
        public class WorldBoss
        {
            public string Id { get; set; } = "";
            public string Name { get; set; } = "";
            public string Description { get; set; } = "";
            public BossRarity Rarity { get; set; } = BossRarity.Elite;
            public int Level { get; set; } = 1;
            public int Health { get; set; } = 1000;
            public int Attack { get; set; } = 50;
            public int Defense { get; set; } = 10;
            public float MoveSpeed { get; set; } = 2.0f;
            public List<string> Skills { get; set; } = new List<string>();
            public SpawnCondition SpawnType { get; set; } = SpawnCondition.Random;
            public int SpawnIntervalMinutes { get; set; } = 60;
            public int MinPlayers { get; set; } = 1;
            public int GoldReward { get; set; } = 1000;
            public int ExpReward { get; set; } = 500;
            public List<string> ItemRewards { get; set; } = new List<string>();
            public float SpawnRadius { get; set; } = 500f;
            public int AttackRange { get; set; } = 150;
            public float AttackCooldown { get; set; } = 2.0f;
        }
        
        /// <summary>
        /// Active world boss instance
        /// </summary>
        public class ActiveWorldBoss
        {
            public string InstanceId { get; set; } = Guid.NewGuid().ToString();
            public string BossId { get; set; } = "";
            public string BossName { get; set; } = "";
            public BossRarity Rarity { get; set; } = BossRarity.Elite;
            public int CurrentHealth { get; set; } = 1000;
            public int MaxHealth { get; set; } = 1000;
            public float X { get; set; } = 0;
            public float Y { get; set; } = 0;
            public DateTime SpawnTime { get; set; } = DateTime.Now;
            public int LifeTimeMinutes { get; set; } = 30;
            public bool IsDefeated { get; set; } = false; 
            public int TotalDamageDealt { get; set; } = 0;
            public int PlayerCount { get; set; } = 0;
        }
        
        /// <summary>
        /// Player damage record
        /// </summary>
        public class PlayerDamageRecord
        {
            public string PlayerId { get; set; } = "";
            public string PlayerName { get; set; } = "";
            public int DamageDealt { get; set; } = 0;
            public float DamagePercent { get; set; } = 0f;
            public DateTime LastHitTime { get; set; } = DateTime.Now;
            public bool HasClaimed { get; set; } = false; 
        }
        
        /// <summary>
        /// Boss kill record
        /// </summary>
        public class BossKillRecord
        {
            public string BossId { get; set; } = "";
            public string BossName { get; set; } = "";
            public BossRarity Rarity { get; set; } = BossRarity.Elite;
            public DateTime KillTime { get; set; } = DateTime.Now;
            public int TotalDamage { get; set; } = 0;
            public int KillerCount { get; set; } = 0;
            public int TotalGoldReward { get; set; } = 0;
            public int TotalExpReward { get; set; } = 0;
        }
        
        /// <summary>
        /// Player world boss statistics
        /// </summary>
        public class PlayerWorldBossStats
        {
            public string PlayerId { get; set; } = "";
            public int TotalBossesKilled { get; set; } = 0;
            public int TotalDamageDealt { get; set; } = 0;
            public int TotalGoldEarned { get; set; } = 0;
            public int TotalExpEarned { get; set; } = 0;
            public Dictionary<string, int> BossKillCount { get; set; } = new Dictionary<string, int>();
            public Dictionary<string, int> RarityKillCount { get; set; } = new Dictionary<string, int>();
        }
    }
}
