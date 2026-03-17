using Godot;
using System;
using System.Collections.Generic;

namespace ClawRPG.Scripts.Systems {
    /// <summary>
    /// World Boss System - Global boss events that require player cooperation
    /// </summary>
    [GlobalClass]
    public partial class WorldBossSystem : BaseSystem {
        
        // Singleton instance
        public static WorldBossSystem Instance { get; private set; }
        
        // World boss data
        private Dictionary<string, WorldBossData> worldBosses = new();
        private Dictionary<string, WorldBossInstance> activeBosses = new();
        
        // Event tracking
        private int totalWorldBossEvents = 0;
        private int successfulDefeats = 0;
        private int totalDamageDealt = 0;
        private int totalPlayers参与 = 0;
        private List<string> bossHistory = new();
        
        // Timers
        private float spawnTimer = 0f;
        private float eventCheckTimer = 0f;
        
        // Configuration
        private float spawnInterval = 600f; // 10 minutes
        private int maxActiveBosses = 3;
        private float globalDamageMultiplier = 1.0f;
        
        public override void _Ready() {
            Instance = this;
            InitializeWorldBosses();
            GD.Print("[WorldBossSystem] Initialized with ", worldBosses.Count, " world bosses");
        }
        
        public override void _Process(float delta) {
            spawnTimer += delta;
            eventCheckTimer += delta;
            
            // Spawn new world boss periodically
            if (spawnTimer >= spawnInterval && activeBosses.Count < maxActiveBosses) {
                TrySpawnWorldBoss();
                spawnTimer = 0f;
            }
            
            // Check boss events
            if (eventCheckTimer >= 5f) {
                CheckWorldBossEvents();
                eventCheckTimer = 0f;
            }
            
            // Update active bosses
            UpdateActiveBosses(delta);
        }
        
        // ===== Public Methods =====
        
        /// <summary>
        /// Deal damage to a world boss
        /// </summary>
        public void DealDamageToBoss(string bossId, string playerId, int damage) {
            if (!activeBosses.TryGetValue(bossId, out var boss)) return;
            
            damage = (int)(damage * globalDamageMultiplier);
            boss.CurrentHealth -= damage;
            
            if (!boss.DamageContributors.ContainsKey(playerId)) {
                boss.DamageContributors[playerId] = 0;
            }
            boss.DamageContributors[playerId] += damage;
            
            totalDamageDealt += damage;
            totalPlayers参与++;
        }
        
        /// <summary>
        /// Get all currently active world bosses
        /// </summary>
        public Dictionary<string, WorldBossInstance> GetActiveBosses() => new Dictionary<string, WorldBossInstance>(activeBosses);
        
        /// <summary>
        /// Get all world boss configurations
        /// </summary>
        public Dictionary<string, WorldBossData> GetAllBosses() => new Dictionary<string, WorldBossData>(worldBosses);
        
        /// <summary>
        /// Get world boss statistics
        /// </summary>
        public WorldBossStatistics GetStatistics() {
            return new WorldBossStatistics {
                TotalEvents = totalWorldBossEvents,
                SuccessfulDefeats = successfulDefeats,
                TotalDamageDealt = totalDamageDealt,
                TotalPlayers参与 = totalPlayers参与,
                SuccessRate = totalWorldBossEvents > 0 ? (float)successfulDefeats / totalWorldBossEvents : 0,
                BossHistory = new List<string>(bossHistory)
            };
        }
        
        /// <summary>
        /// Set the spawn interval for world bosses
        /// </summary>
        public void SetSpawnInterval(float interval) => spawnInterval = interval;
        
        /// <summary>
        /// Set global damage multiplier
        /// </summary>
        public void SetGlobalDamageMultiplier(float multiplier) => globalDamageMultiplier = multiplier;
        
        // ===== Helper Methods =====
        
        /// <summary>
        /// Notify players of world boss events
        /// </summary>
        private void NotifyPlayers(string message) {
            GD.Print("[WorldBossSystem] " + message);
        }
        
        // Mock implementations - replace with actual game systems
        private int GetOnlinePlayerCount() => 1;
        private int GetWorldLevel() => 50;
        private bool IsWeatherActive(string weather) => false;
        private bool IsWorldEventActive(string eventName) => false;
        private bool IsNightTime() => true;
        private bool IsDayTime() => false;
        private bool IsWeatherClear() => true;
        private int GetHighestDungeonFloor() => 50;
        
        // ===== Persistence Methods =====
        
        public override Dictionary ExportSaveData()
        {
            var data = new Dictionary();
            
            // 统计数据
            data["totalWorldBossEvents"] = totalWorldBossEvents;
            data["successfulDefeats"] = successfulDefeats;
            data["totalDamageDealt"] = totalDamageDealt;
            data["totalPlayers参与"] = totalPlayers参与;
            data["bossHistory"] = bossHistory;
            
            // 配置
            data["spawnInterval"] = spawnInterval;
            data["maxActiveBosses"] = maxActiveBosses;
            data["globalDamageMultiplier"] = globalDamageMultiplier;
            
            // 活跃的世界BOSS实例
            var activeBossesData = new List<Dictionary>();
            foreach (var kvp in activeBosses)
            {
                var bossDict = new Dictionary();
                bossDict["bossId"] = kvp.Key;
                bossDict["currentHealth"] = kvp.Value.CurrentHealth;
                bossDict["maxHealth"] = kvp.Value.MaxHealth;
                bossDict["phase"] = kvp.Value.Phase;
                bossDict["currentAttack"] = kvp.Value.CurrentAttack;
                bossDict["spawnTime"] = kvp.Value.SpawnTime.ToString("o");
                bossDict["status"] = (int)kvp.Value.Status;
                
                // 伤害贡献者
                var contributors = new Dictionary<string, int>();
                foreach (var contributor in kvp.Value.DamageContributors)
                {
                    contributors[contributor.Key] = contributor.Value;
                }
                bossDict["damageContributors"] = contributors;
                
                activeBossesData.Add(bossDict);
            }
            data["activeBosses"] = activeBossesData;
            
            return data;
        }

        public override void ImportSaveData(Dictionary data)
        {
            if (data == null) return;
            
            // 加载统计数据
            if (data.Contains("totalWorldBossEvents"))
                totalWorldBossEvents = (int)data["totalWorldBossEvents"];
            if (data.Contains("successfulDefeats"))
                successfulDefeats = (int)data["successfulDefeats"];
            if (data.Contains("totalDamageDealt"))
                totalDamageDealt = (int)data["totalDamageDealt"];
            if (data.Contains("totalPlayers参与"))
                totalPlayers参与 = (int)data["totalPlayers参与"];
            
            // 加载boss历史
            if (data.Contains("bossHistory"))
            {
                bossHistory.Clear();
                var historyArray = (Array)data["bossHistory"];
                foreach (var item in historyArray)
                {
                    bossHistory.Add(item.ToString());
                }
            }
            
            // 加载配置
            if (data.Contains("spawnInterval"))
                spawnInterval = (float)data["spawnInterval"];
            if (data.Contains("maxActiveBosses"))
                maxActiveBosses = (int)data["maxActiveBosses"];
            if (data.Contains("globalDamageMultiplier"))
                globalDamageMultiplier = (float)data["globalDamageMultiplier"];
            
            // 加载活跃BOSS
            if (data.Contains("activeBosses"))
            {
                activeBosses.Clear();
                var bossesData = (Array)data["activeBosses"];
                foreach (Dictionary bossDict in bossesData)
                {
                    var bossId = bossDict["bossId"].ToString();
                    if (!worldBosses.TryGetValue(bossId, out var bossData)) continue;
                    
                    var instance = new WorldBossInstance
                    {
                        Data = bossData,
                        CurrentHealth = (long)bossDict["currentHealth"],
                        MaxHealth = (long)bossDict["maxHealth"],
                        Phase = (int)bossDict["phase"],
                        CurrentAttack = (int)bossDict["currentAttack"],
                        Status = (WorldBossStatus)(int)bossDict["status"]
                    };
                    
                    DateTime.TryParse(bossDict["spawnTime"].ToString(), out instance.SpawnTime);
                    
                    instance.DamageContributors = new Dictionary<string, int>();
                    if (bossDict.Contains("damageContributors"))
                    {
                        var contributors = (Dictionary<string, Variant>)bossDict["damageContributors"];
                        foreach (var kvp in contributors)
                        {
                            instance.DamageContributors[kvp.Key] = (int)kvp.Value;
                        }
                    }
                    
                    activeBosses[bossId] = instance;
                }
            }
        }
    }
}
