using Godot;
using System;
using System.Collections.Generic;

namespace ClawRPG.Scripts.Systems {
    /// <summary>
    /// World Boss System - Global boss events that require player cooperation
    /// Main system class with core logic
    /// </summary>
    [GlobalClass]
    public partial class WorldBossSystem : BaseSystem {
        
        // Singleton instance
        public static WorldBossSystem Instance { get; private set; }
        
        // Data and Spawner subsystems
        public Data Data { get; private set; }
        public Spawner Spawner { get; private set; }
        
        // Timers
        private float spawnTimer = 0f;
        private float eventCheckTimer = 0f;
        
        // Configuration
        private float spawnInterval = 600f; // 10 minutes
        private int maxActiveBosses = 3;
        private float globalDamageMultiplier = 1.0f;
        
        public override void _Ready() {
            Instance = this;
            Data = new Data(this);
            Spawner = new Spawner(this);
            Spawner.InitializeWorldBosses();
            GD.Print("[WorldBossSystem] Initialized with ", Data.WorldBosses.Count, " world bosses");
        }
        
        public override void _Process(float delta) {
            spawnTimer += delta;
            eventCheckTimer += delta;
            
            // Spawn new world boss periodically
            if (spawnTimer >= spawnInterval && Data.ActiveBosses.Count < maxActiveBosses) {
                Spawner.TrySpawnWorldBoss();
                spawnTimer = 0f;
            }
            
            // Check boss events
            if (eventCheckTimer >= 5f) {
                Spawner.CheckWorldBossEvents();
                eventCheckTimer = 0f;
            }
            
            // Update active bosses
            Spawner.UpdateActiveBosses(delta);
        }
        
        // Public methods
        public void DealDamageToBoss(string bossId, string playerId, int damage) {
            if (!Data.ActiveBosses.TryGetValue(bossId, out var boss)) return;
            
            damage = (int)(damage * globalDamageMultiplier);
            boss.CurrentHealth -= damage;
            
            if (!boss.DamageContributors.ContainsKey(playerId)) {
                boss.DamageContributors[playerId] = 0;
            }
            boss.DamageContributors[playerId] += damage;
            
            Data.TotalDamageDealt += damage;
            Data.TotalPlayers参与++;
        }
        
        public Dictionary<string, WorldBossInstance> GetActiveBosses() => new Dictionary<string, WorldBossInstance>(Data.ActiveBosses);
        
        public Dictionary<string, WorldBossData> GetAllBosses() => new Dictionary<string, WorldBossData>(Data.WorldBosses);
        
        public WorldBossStatistics GetStatistics() {
            return new WorldBossStatistics {
                TotalEvents = Data.TotalWorldBossEvents,
                SuccessfulDefeats = Data.SuccessfulDefeats,
                TotalDamageDealt = Data.TotalDamageDealt,
                TotalPlayers参与 = Data.TotalPlayers参与,
                SuccessRate = Data.TotalWorldBossEvents > 0 ? (float)Data.SuccessfulDefeats / Data.TotalWorldBossEvents : 0,
                BossHistory = new List<string>(Data.BossHistory)
            };
        }
        
        public void SetSpawnInterval(float interval) => spawnInterval = interval;
        
        public void SetGlobalDamageMultiplier(float multiplier) => globalDamageMultiplier = multiplier;
        
        // Helper methods (mock implementations)
        private int GetOnlinePlayerCount() => 1;
        private int GetWorldLevel() => 50;
        private bool IsWeatherActive(string weather) => false;
        private bool IsWorldEventActive(string eventName) => false;
        private bool IsNightTime() => true;
        private bool IsDayTime() => false;
        private bool IsWeatherClear() => true;
        private int GetHighestDungeonFloor() => 50;
        
        private void NotifyPlayers(string message) {
            GD.Print("[WorldBossSystem] " + message);
        }
        
        // Provide access to helper methods for Spawner
        internal bool CheckOnlinePlayerCount(float required) => GetOnlinePlayerCount() >= required;
        internal bool CheckWorldLevel(float required) => GetWorldLevel() >= required;
        internal bool CheckWeatherActive(string weather) => IsWeatherActive(weather);
        internal bool CheckWorldEventActive(string eventName) => IsWorldEventActive(eventName);
        internal bool CheckNightTime() => IsNightTime();
        internal bool CheckDayTime() => IsDayTime();
        internal bool CheckWeatherClear() => IsWeatherClear();
        internal bool CheckDungeonFloor(float required) => GetHighestDungeonFloor() >= required;
        internal void NotifyPlayersInternal(string message) => NotifyPlayers(message);
        
        // ===== 持久化方法 =====

        public override Dictionary ExportSaveData()
        {
            var data = new Dictionary();
            
            // 统计数据
            data["totalWorldBossEvents"] = Data.TotalWorldBossEvents;
            data["successfulDefeats"] = Data.SuccessfulDefeats;
            data["totalDamageDealt"] = Data.TotalDamageDealt;
            data["totalPlayers参与"] = Data.TotalPlayers参与;
            data["bossHistory"] = Data.BossHistory;
            
            // 配置
            data["spawnInterval"] = spawnInterval;
            data["maxActiveBosses"] = maxActiveBosses;
            data["globalDamageMultiplier"] = globalDamageMultiplier;
            
            // 活跃的世界BOSS实例
            var activeBossesData = new List<Dictionary>();
            foreach (var kvp in Data.ActiveBosses)
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
                Data.TotalWorldBossEvents = (int)data["totalWorldBossEvents"];
            if (data.Contains("successfulDefeats"))
                Data.SuccessfulDefeats = (int)data["successfulDefeats"];
            if (data.Contains("totalDamageDealt"))
                Data.TotalDamageDealt = (int)data["totalDamageDealt"];
            if (data.Contains("totalPlayers参与"))
                Data.TotalPlayers参与 = (int)data["totalPlayers参与"];
            
            // 加载boss历史
            if (data.Contains("bossHistory"))
            {
                Data.BossHistory.Clear();
                var historyArray = (Array)data["bossHistory"];
                foreach (var item in historyArray)
                {
                    Data.BossHistory.Add(item.ToString());
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
                Data.ActiveBosses.Clear();
                var bossesData = (Array)data["activeBosses"];
                foreach (Dictionary bossDict in bossesData)
                {
                    var bossId = bossDict["bossId"].ToString();
                    if (!Data.WorldBosses.TryGetValue(bossId, out var bossData)) continue;
                    
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
                    
                    Data.ActiveBosses[bossId] = instance;
                }
            }
        }
    }
    
    // Data classes
    public enum WorldBossType { Elite, Cosmic, Divine, Corrupted, Construct, Assassin }
    public enum WorldBossStatus { Active, Defeated, Expired }
    public enum ElementType { Fire, Ice, Lightning, Dark, Holy, Earth, Poison, Physical }
    
    public class WorldBossData {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public WorldBossType Type { get; set; }
        public int Difficulty { get; set; }
        public int MaxHealth { get; set; }
        public int Attack { get; set; }
        public int Defense { get; set; }
        public int Speed { get; set; }
        public List<string> Abilities { get; set; }
        public ElementType Element { get; set; }
        public ElementType Weakness { get; set; }
        public Dictionary<string, float> SpawnConditions { get; set; }
        public WorldBossRewards Rewards { get; set; }
        public string SpawnMessage { get; set; }
        public string DefeatMessage { get; set; }
    }
    
    public class WorldBossRewards {
        public int GoldMin { get; set; }
        public int GoldMax { get; set; }
        public int ExperienceMin { get; set; }
        public int ExperienceMax { get; set; }
        public float DropRateBonus { get; set; }
        public List<string> UniqueDrops { get; set; }
    }
    
    public class WorldBossInstance {
        public WorldBossData Data { get; set; }
        public long CurrentHealth { get; set; }
        public long MaxHealth { get; set; }
        public int Phase { get; set; }
        public int CurrentAttack { get; set; }
        public DateTime SpawnTime { get; set; }
        public Dictionary<string, int> DamageContributors { get; set; }
        public WorldBossStatus Status { get; set; }
    }
    
    public class WorldBossStatistics {
        public int TotalEvents { get; set; }
        public int SuccessfulDefeats { get; set; }
        public int TotalDamageDealt { get; set; }
        public int TotalPlayers参与 { get; set; }
        public float SuccessRate { get; set; }
        public List<string> BossHistory { get; set; }
    }
}
