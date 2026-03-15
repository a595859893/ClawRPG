using Godot;
using System;
using System.Collections.Generic;

namespace ClawRPG.Scripts.Systems {
    /// <summary>
    /// Player statistics tracking system
    /// Tracks various gameplay metrics for the player
    /// </summary>
    public class PlayerStatistics
    {
        // Combat statistics
        public int TotalKills { get; set; }
        public int TotalDeaths { get; set; }
        public int TotalDamageDealt { get; set; }
        public int TotalDamageTaken { get; set; }
        public int TotalHealing { get; set; }
        public int CriticalHits { get; set; }
        public int PerfectBlocks { get; set; }
        public int Dodges { get; set; }
        
        // Resource statistics
        public int GoldEarned { get; set; }
        public int GoldSpent { get; set; }
        public int ExperienceGained { get; set; }
        public int ItemsCollected { get; set; }
        public int ItemsCrafted { get; set; }
        
        // Quest statistics
        public int QuestsCompleted { get; set; }
        public int QuestsAbandoned { get; set; }
        
        // Skill statistics
        public int SkillsLearned { get; set; }
        public int SkillsUsed { get; set; }
        
        // Exploration statistics
        public int RegionsDiscovered { get; set; }
        public int EnemiesEncountered { get; set; }
        public int BossesDefeated { get; set; }
        
        // Time statistics
        public float TotalPlayTime { get; set; }
        public int HighestLevel { get; set; }
        public int HighestCombo { get; set; }
        
        // Achievement statistics
        public int AchievementsUnlocked { get; set; }
        
        public PlayerStatistics()
        {
            Reset();
        }
        
        public void Reset()
        {
            TotalKills = 0;
            TotalDeaths = 0;
            TotalDamageDealt = 0;
            TotalDamageTaken = 0;
            TotalHealing = 0;
            CriticalHits = 0;
            PerfectBlocks = 0;
            Dodges = 0;
            GoldEarned = 0;
            GoldSpent = 0;
            ExperienceGained = 0;
            ItemsCollected = 0;
            ItemsCrafted = 0;
            QuestsCompleted = 0;
            QuestsAbandoned = 0;
            SkillsLearned = 0;
            SkillsUsed = 0;
            RegionsDiscovered = 0;
            EnemiesEncountered = 0;
            BossesDefeated = 0;
            TotalPlayTime = 0;
            HighestLevel = 1;
            HighestCombo = 0;
            AchievementsUnlocked = 0;
        }
        
        public Dictionary<string, object> ToDictionary()
        {
            return new Dictionary<string, object>
            {
                ["TotalKills"] = TotalKills,
                ["TotalDeaths"] = TotalDeaths,
                ["TotalDamageDealt"] = TotalDamageDealt,
                ["TotalDamageTaken"] = TotalDamageTaken,
                ["TotalHealing"] = TotalHealing,
                ["CriticalHits"] = CriticalHits,
                ["PerfectBlocks"] = PerfectBlocks,
                ["Dodges"] = Dodges,
                ["GoldEarned"] = GoldEarned,
                ["GoldSpent"] = GoldSpent,
                ["ExperienceGained"] = ExperienceGained,
                ["ItemsCollected"] = ItemsCollected,
                ["ItemsCrafted"] = ItemsCrafted,
                ["QuestsCompleted"] = QuestsCompleted,
                ["QuestsAbandoned"] = QuestsAbandoned,
                ["SkillsLearned"] = SkillsLearned,
                ["SkillsUsed"] = SkillsUsed,
                ["RegionsDiscovered"] = RegionsDiscovered,
                ["EnemiesEncountered"] = EnemiesEncountered,
                ["BossesDefeated"] = BossesDefeated,
                ["TotalPlayTime"] = TotalPlayTime,
                ["HighestLevel"] = HighestLevel,
                ["HighestCombo"] = HighestCombo,
                ["AchievementsUnlocked"] = AchievementsUnlocked
            };
        }
        
        public void FromDictionary(Dictionary<string, object> data)
        {
            if (data == null) return;
            
            TotalKills = GetIntValue(data, "TotalKills", 0);
            TotalDeaths = GetIntValue(data, "TotalDeaths", 0);
            TotalDamageDealt = GetIntValue(data, "TotalDamageDealt", 0);
            TotalDamageTaken = GetIntValue(data, "TotalDamageTaken", 0);
            TotalHealing = GetIntValue(data, "TotalHealing", 0);
            CriticalHits = GetIntValue(data, "CriticalHits", 0);
            PerfectBlocks = GetIntValue(data, "PerfectBlocks", 0);
            Dodges = GetIntValue(data, "Dodges", 0);
            GoldEarned = GetIntValue(data, "GoldEarned", 0);
            GoldSpent = GetIntValue(data, "GoldSpent", 0);
            ExperienceGained = GetIntValue(data, "ExperienceGained", 0);
            ItemsCollected = GetIntValue(data, "ItemsCollected", 0);
            ItemsCrafted = GetIntValue(data, "ItemsCrafted", 0);
            QuestsCompleted = GetIntValue(data, "QuestsCompleted", 0);
            QuestsAbandoned = GetIntValue(data, "QuestsAbandoned", 0);
            SkillsLearned = GetIntValue(data, "SkillsLearned", 0);
            SkillsUsed = GetIntValue(data, "SkillsUsed", 0);
            RegionsDiscovered = GetIntValue(data, "RegionsDiscovered", 0);
            EnemiesEncountered = GetIntValue(data, "EnemiesEncountered", 0);
            BossesDefeated = GetIntValue(data, "BossesDefeated", 0);
            TotalPlayTime = GetFloatValue(data, "TotalPlayTime", 0);
            HighestLevel = GetIntValue(data, "HighestLevel", 1);
            HighestCombo = GetIntValue(data, "HighestCombo", 0);
            AchievementsUnlocked = GetIntValue(data, "AchievementsUnlocked", 0);
        }
        
        private int GetIntValue(Dictionary<string, object> data, string key, int defaultValue)
        {
            if (data.ContainsKey(key) && data[key] is int value)
                return value;
            if (data.ContainsKey(key) && data[key] is long longValue)
                return (int)longValue;
            return defaultValue;
        }
        
        private float GetFloatValue(Dictionary<string, object> data, string key, float defaultValue)
        {
            if (data.ContainsKey(key) && data[key] is float value)
                return value;
            if (data.ContainsKey(key) && data[key] is int intValue)
                return intValue;
            if (data.ContainsKey(key) && data[key] is double doubleValue)
                return (float)doubleValue;
            return defaultValue;
        }
        
        // Combat tracking methods
        public void RecordKill() => TotalKills++;
        public void RecordDeath() => TotalDeaths++;
        public void RecordDamageDealt(int damage) => TotalDamageDealt += damage;
        public void RecordDamageTaken(int damage) => TotalDamageTaken += damage;
        public void RecordHealing(int healing) => TotalHealing += healing;
        public void RecordCriticalHit() => CriticalHits++;
        public void RecordPerfectBlock() => PerfectBlocks++;
        public void RecordDodge() => Dodges++;
        
        // Resource tracking methods
        public void RecordGoldEarned(int gold) => GoldEarned += gold;
        public void RecordGoldSpent(int gold) => GoldSpent += gold;
        public void RecordExperience(int exp) => ExperienceGained += exp;
        public void RecordItemCollected() => ItemsCollected++;
        public void RecordItemCrafted() => ItemsCrafted++;
        
        // Quest tracking methods
        public void RecordQuestCompleted() => QuestsCompleted++;
        public void RecordQuestAbandoned() => QuestsAbandoned++;
        
        // Skill tracking methods
        public void RecordSkillLearned() => SkillsLearned++;
        public void RecordSkillUsed() => SkillsUsed++;
        
        // Exploration tracking methods
        public void RecordRegionDiscovered() => RegionsDiscovered++;
        public void RecordEnemyEncountered() => EnemiesEncountered++;
        public void RecordBossDefeated() => BossesDefeated++;
        
        // Level tracking
        public void UpdateHighestLevel(int level)
        {
            if (level > HighestLevel)
                HighestLevel = level;
        }
        
        // Combo tracking
        public void UpdateHighestCombo(int combo)
        {
            if (combo > HighestCombo)
                HighestCombo = combo;
        }
        
        // Achievement tracking
        public void RecordAchievementUnlocked() => AchievementsUnlocked++;
        
        // Play time tracking
        public void AddPlayTime(float seconds) => TotalPlayTime += seconds;
    }
    
    /// <summary>
    /// Statistics Manager - singleton to track player statistics
    /// </summary>
    public class StatisticsSystem : BaseSystem
    {
        private static StatisticsSystem _instance;
        public static StatisticsSystem Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = GetNode<StatisticsSystem>("/root/StatisticsSystem");
                    if (_instance == null)
                    {
                        var node = new StatisticsSystem();
                        node.Name = "StatisticsSystem";
                        Engine.GetMainLoop().Root.AddChild(node);
                    }
                }
                return _instance;
            }
        }
        
        public PlayerStatistics Stats { get; private set; } = new PlayerStatistics();
        
        // Statistics update signal
        public Action OnStatisticsUpdated;
        
        protected override void Initialize()
        {
            base.Initialize();
            
            // 注册到保存系统
            SaveSystem.Instance?.Register(this);
            
            GD.Print("[StatisticsSystem] Initialized");
        }
        
        public void ResetStatistics()
        {
            Stats.Reset();
            OnStatisticsUpdated?.Invoke();
        }
        
        /// <summary>
        /// 导出保存数据
        /// </summary>
        public override Dictionary ExportSaveData()
        {
            return Stats.ToDictionary();
        }
        
        /// <summary>
        /// 导入保存数据
        /// </summary>
        public override void ImportSaveData(Dictionary data)
        {
            if (data == null) return;
            
            var dict = new Dictionary<string, object>();
            foreach (var key in data.Keys)
            {
                dict[key.ToString()] = data[key];
            }
            Stats.FromDictionary(dict);
            OnStatisticsUpdated?.Invoke();
            
            GD.Print("[StatisticsSystem] Data loaded");
        }
        
        /// <summary>
        /// 获取系统ID
        /// </summary>
        public override string GetId()
        {
            return "StatisticsSystem";
        }
        
        // Convenience methods for tracking
        public void RecordKill() { Stats.RecordKill(); OnStatisticsUpdated?.Invoke(); }
        public void RecordDeath() { Stats.RecordDeath(); OnStatisticsUpdated?.Invoke(); }
        public void RecordDamageDealt(int damage) { Stats.RecordDamageDealt(damage); OnStatisticsUpdated?.Invoke(); }
        public void RecordDamageTaken(int damage) { Stats.RecordDamageTaken(damage); OnStatisticsUpdated?.Invoke(); }
        public void RecordHealing(int healing) { Stats.RecordHealing(healing); OnStatisticsUpdated?.Invoke(); }
        public void RecordCriticalHit() { Stats.RecordCriticalHit(); OnStatisticsUpdated?.Invoke(); }
        public void RecordPerfectBlock() { Stats.RecordPerfectBlock(); OnStatisticsUpdated?.Invoke(); }
        public void RecordDodge() { Stats.RecordDodge(); OnStatisticsUpdated?.Invoke(); }
        public void RecordGoldEarned(int gold) { Stats.RecordGoldEarned(gold); OnStatisticsUpdated?.Invoke(); }
        public void RecordGoldSpent(int gold) { Stats.RecordGoldSpent(gold); OnStatisticsUpdated?.Invoke(); }
        public void RecordExperience(int exp) { Stats.RecordExperience(exp); OnStatisticsUpdated?.Invoke(); }
        public void RecordItemCollected() { Stats.RecordItemCollected(); OnStatisticsUpdated?.Invoke(); }
        public void RecordItemCrafted() { Stats.RecordItemCrafted(); OnStatisticsUpdated?.Invoke(); }
        public void RecordQuestCompleted() { Stats.RecordQuestCompleted(); OnStatisticsUpdated?.Invoke(); }
        public void RecordQuestAbandoned() { Stats.RecordQuestAbandoned(); OnStatisticsUpdated?.Invoke(); }
        public void RecordSkillLearned() { Stats.RecordSkillLearned(); OnStatisticsUpdated?.Invoke(); }
        public void RecordSkillUsed() { Stats.RecordSkillUsed(); OnStatisticsUpdated?.Invoke(); }
        public void RecordRegionDiscovered() { Stats.RecordRegionDiscovered(); OnStatisticsUpdated?.Invoke(); }
        public void RecordEnemyEncountered() { Stats.RecordEnemyEncountered(); OnStatisticsUpdated?.Invoke(); }
        public void RecordBossDefeated() { Stats.RecordBossDefeated(); OnStatisticsUpdated?.Invoke(); }
        public void UpdateHighestLevel(int level) { Stats.UpdateHighestLevel(level); OnStatisticsUpdated?.Invoke(); }
        public void UpdateHighestCombo(int combo) { Stats.UpdateHighestCombo(combo); OnStatisticsUpdated?.Invoke(); }
        public void RecordAchievementUnlocked() { Stats.RecordAchievementUnlocked(); OnStatisticsUpdated?.Invoke(); }
        public void AddPlayTime(float seconds) { Stats.AddPlayTime(seconds); OnStatisticsUpdated?.Invoke(); }
        
        // Keep StatisticsManager as alias for compatibility
        public static StatisticsSystem Manager => Instance;
    }
    
    /// <summary>
    /// Statistics Manager - alias for compatibility
    /// </summary>
    public class StatisticsManager
    {
        public static StatisticsSystem Instance => StatisticsSystem.Instance;
    }
}
