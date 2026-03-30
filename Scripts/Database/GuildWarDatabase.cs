using System;
using System.Collections.Generic;
using ClawRPG.Scripts.Database;

namespace ClawRPG.Core.Systems.GuildWar
{
    /// <summary>
    /// Guild War Configuration Database
    /// </summary>
    public class GuildWarDatabase : DatabaseBase
    {
        private static GuildWarDatabase _instance;
        public static GuildWarDatabase Singleton => _instance ??= new GuildWarDatabase();

        // War type configurations
        private Dictionary<GuildWarType, GuildWarTypeConfig> _warTypeConfigs;

        // Reward configurations
        private List<GuildWarRewardConfig> _rewardConfigs;

        // Territory configurations
        private List<TerritoryConfig> _territoryConfigs;

        // Guild level requirements (runtime-modifiable, persisted)
        private Dictionary<int, GuildLevelRequirement> _guildLevelRequirements;

        // Weekly war schedule
        private List<WarScheduleConfig> _weeklySchedule;

        public override object Instance => _instance ??= new GuildWarDatabase();

        public override void Initialize()
        {
            _warTypeConfigs = new Dictionary<GuildWarType, GuildWarTypeConfig>
            {
                {
                    GuildWarType.Territory, new GuildWarTypeConfig
                    {
                        Type = GuildWarType.Territory,
                        Name = "Territory War",
                        Description = "Fight for control of valuable territories",
                        DefaultDuration = 120,
                        MinGuilds = 4,
                        MaxGuilds = 16,
                        ScorePerKill = 100,
                        ScorePerAssist = 25,
                        ScorePerCapture = 500,
                        ScorePerDefense = 200
                    }
                },
                {
                    GuildWarType.Resource, new GuildWarTypeConfig
                    {
                        Type = GuildWarType.Resource,
                        Name = "Resource War",
                        Description = "Compete for scarce resources",
                        DefaultDuration = 90,
                        MinGuilds = 2,
                        MaxGuilds = 8,
                        ScorePerKill = 80,
                        ScorePerAssist = 20,
                        ScorePerResourceNode = 300,
                        ScorePerDefense = 150
                    }
                },
                {
                    GuildWarType.Elimination, new GuildWarTypeConfig
                    {
                        Type = GuildWarType.Elimination,
                        Name = "Elimination War",
                        Description = "Last guild standing wins",
                        DefaultDuration = 180,
                        MinGuilds = 4,
                        MaxGuilds = 32,
                        ScorePerKill = 150,
                        ScorePerAssist = 30,
                        ScorePerZoneControl = 400,
                        ScorePerSurvival = 100
                    }
                },
                {
                    GuildWarType.Conquest, new GuildWarTypeConfig
                    {
                        Type = GuildWarType.Conquest,
                        Name = "Conquest War",
                        Description = "Capture enemy base and hold it",
                        DefaultDuration = 150,
                        MinGuilds = 2,
                        MaxGuilds = 4,
                        ScorePerKill = 120,
                        ScorePerAssist = 30,
                        ScorePerBaseCapture = 1000,
                        ScorePerBaseDefense = 500
                    }
                },
                {
                    GuildWarType.Defense, new GuildWarTypeConfig
                    {
                        Type = GuildWarType.Defense,
                        Name = "Defense War",
                        Description = "Defend your guild's honor",
                        DefaultDuration = 60,
                        MinGuilds = 2,
                        MaxGuilds = 8,
                        ScorePerKill = 100,
                        ScorePerAssist = 25,
                        ScorePerWave = 200,
                        ScorePerBossKill = 500
                    }
                }
            };

            _rewardConfigs = new List<GuildWarRewardConfig>
            {
                new GuildWarRewardConfig { Rank = 1, Gold = 100000, Experience = 50000, Reputation = 1000, Title = "War Champion" },
                new GuildWarRewardConfig { Rank = 2, Gold = 75000, Experience = 40000, Reputation = 750, Title = "War Legend" },
                new GuildWarRewardConfig { Rank = 3, Gold = 50000, Experience = 30000, Reputation = 500, Title = "War Hero" },
                new GuildWarRewardConfig { Rank = 4, Gold = 40000, Experience = 25000, Reputation = 400, Title = "Battle Master" },
                new GuildWarRewardConfig { Rank = 5, Gold = 30000, Experience = 20000, Reputation = 300, Title = "Warrior" },
                new GuildWarRewardConfig { Rank = 6, Gold = 25000, Experience = 17500, Reputation = 250 },
                new GuildWarRewardConfig { Rank = 7, Gold = 20000, Experience = 15000, Reputation = 200 },
                new GuildWarRewardConfig { Rank = 8, Gold = 15000, Experience = 12500, Reputation = 150 },
                new GuildWarRewardConfig { Rank = 9, Gold = 10000, Experience = 10000, Reputation = 100 },
                new GuildWarRewardConfig { Rank = 10, Gold = 5000, Experience = 7500, Reputation = 50 }
            };

            _territoryConfigs = new List<TerritoryConfig>
            {
                new TerritoryConfig { TerritoryId = "t1", Name = "Iron Fortress", ResourceType = "Iron", DefenseLevel = 10, ResourceGeneration = 100 },
                new TerritoryConfig { TerritoryId = "t2", Name = "Golden Plains", ResourceType = "Gold", DefenseLevel = 8, ResourceGeneration = 150 },
                new TerritoryConfig { TerritoryId = "t3", Name = "Crystal Cave", ResourceType = "Crystal", DefenseLevel = 12, ResourceGeneration = 80 },
                new TerritoryConfig { TerritoryId = "t4", Name = "Mystic Forest", ResourceType = "Wood", DefenseLevel = 6, ResourceGeneration = 120 },
                new TerritoryConfig { TerritoryId = "t5", Name = "Dragon Peak", ResourceType = "Rare", DefenseLevel = 15, ResourceGeneration = 50 }
            };

            _guildLevelRequirements = new Dictionary<int, GuildLevelRequirement>
            {
                { 1, new GuildLevelRequirement { MinLevel = 1, MaxGuilds = 4, EntryFee = 1000 } },
                { 2, new GuildLevelRequirement { MinLevel = 5, MaxGuilds = 6, EntryFee = 5000 } },
                { 3, new GuildLevelRequirement { MinLevel = 10, MaxGuilds = 8, EntryFee = 25000 } },
                { 4, new GuildLevelRequirement { MinLevel = 20, MaxGuilds = 12, EntryFee = 100000 } },
                { 5, new GuildLevelRequirement { MinLevel = 30, MaxGuilds = 16, EntryFee = 500000 } }
            };

            _weeklySchedule = new List<WarScheduleConfig>
            {
                new WarScheduleConfig { DayOfWeek = 1, WarType = GuildWarType.Resource, StartHour = 19, Duration = 90 },
                new WarScheduleConfig { DayOfWeek = 2, WarType = GuildWarType.Defense, StartHour = 20, Duration = 60 },
                new WarScheduleConfig { DayOfWeek = 3, WarType = GuildWarType.Territory, StartHour = 19, Duration = 120 },
                new WarScheduleConfig { DayOfWeek = 4, WarType = GuildWarType.Elimination, StartHour = 20, Duration = 180 },
                new WarScheduleConfig { DayOfWeek = 5, WarType = GuildWarType.Conquest, StartHour = 19, Duration = 150 },
                new WarScheduleConfig { DayOfWeek = 6, WarType = GuildWarType.Territory, StartHour = 18, Duration = 120 },
                new WarScheduleConfig { DayOfWeek = 0, WarType = GuildWarType.Elimination, StartHour = 14, Duration = 180 }
            };
        }

        public bool ValidateData()
        {
            return _warTypeConfigs != null && _warTypeConfigs.Count > 0
                && _rewardConfigs != null && _rewardConfigs.Count > 0
                && _territoryConfigs != null && _territoryConfigs.Count > 0;
        }

        protected override void OnExportSaveData(Godot.Collections.Dictionary saveData)
        {
            base.OnExportSaveData(saveData);

            // 导出公会等级要求数据（runtime-modifiable）
            if (_guildLevelRequirements != null && _guildLevelRequirements.Count > 0)
            {
                var levelReqList = new Godot.Collections.Array();
                foreach (var kvp in _guildLevelRequirements)
                {
                    var entry = new Godot.Collections.Dictionary
                    {
                        ["level"] = kvp.Key,
                        ["minLevel"] = kvp.Value.MinLevel,
                        ["maxGuilds"] = kvp.Value.MaxGuilds,
                        ["entryFee"] = kvp.Value.EntryFee
                    };
                    levelReqList.Add(entry);
                }
                saveData["guildLevelRequirements"] = levelReqList;
            }
        }

        protected override void OnImportSaveData(Godot.Collections.Dictionary saveData)
        {
            base.OnImportSaveData(saveData);

            // 导入公会等级要求数据
            if (saveData.TryGetValue("guildLevelRequirements", out var levelReqData) && levelReqData is Godot.Collections.Array levelReqList)
            {
                foreach (Godot.Collections.Dictionary entry in levelReqList)
                {
                    if (entry.TryGetValue("level", out var levelVal) && entry.TryGetValue("minLevel", out var minLevelVal)
                        && entry.TryGetValue("maxGuilds", out var maxGuildsVal) && entry.TryGetValue("entryFee", out var entryFeeVal))
                    {
                        int level = Convert.ToInt32(levelVal);
                        _guildLevelRequirements[level] = new GuildLevelRequirement
                        {
                            MinLevel = Convert.ToInt32(minLevelVal),
                            MaxGuilds = Convert.ToInt32(maxGuildsVal),
                            EntryFee = Convert.ToInt32(entryFeeVal)
                        };
                    }
                }
            }
        }

        // Public accessors for backward compatibility
        public Dictionary<GuildWarType, GuildWarTypeConfig> WarTypeConfigs => _warTypeConfigs;
        public List<GuildWarRewardConfig> RewardConfigs => _rewardConfigs;
        public List<TerritoryConfig> TerritoryConfigs => _territoryConfigs;
        public Dictionary<int, GuildLevelRequirement> GuildLevelRequirements => _guildLevelRequirements;
        public List<WarScheduleConfig> WeeklySchedule => _weeklySchedule;

        // Get config by type
        public GuildWarTypeConfig GetConfig(GuildWarType type)
        {
            return _warTypeConfigs.ContainsKey(type) ? _warTypeConfigs[type] : null;
        }

        // Get reward by rank
        public GuildWarRewardConfig GetReward(int rank)
        {
            return _rewardConfigs.Find(r => r.Rank == rank);
        }

        // Get territory config
        public TerritoryConfig GetTerritory(string territoryId)
        {
            return _territoryConfigs.Find(t => t.TerritoryId == territoryId);
        }
    }

    // Configuration classes
    public class GuildWarTypeConfig
    {
        public GuildWarType Type { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int DefaultDuration { get; set; }
        public int MinGuilds { get; set; }
        public int MaxGuilds { get; set; }
        public int ScorePerKill { get; set; }
        public int ScorePerAssist { get; set; }
        public int ScorePerCapture { get; set; }
        public int ScorePerDefense { get; set; }
        public int ScorePerResourceNode { get; set; }
        public int ScorePerZoneControl { get; set; }
        public int ScorePerSurvival { get; set; }
        public int ScorePerBaseCapture { get; set; }
        public int ScorePerBaseDefense { get; set; }
        public int ScorePerWave { get; set; }
        public int ScorePerBossKill { get; set; }
    }

    public class GuildWarRewardConfig
    {
        public int Rank { get; set; }
        public int Gold { get; set; }
        public int Experience { get; set; }
        public int Reputation { get; set; }
        public string Title { get; set; }
    }

    public class TerritoryConfig
    {
        public string TerritoryId { get; set; }
        public string Name { get; set; }
        public string ResourceType { get; set; }
        public int DefenseLevel { get; set; }
        public int ResourceGeneration { get; set; }
    }

    public class GuildLevelRequirement
    {
        public int MinLevel { get; set; }
        public int MaxGuilds { get; set; }
        public int EntryFee { get; set; }
    }

    public class WarScheduleConfig
    {
        public int DayOfWeek { get; set; }
        public GuildWarType WarType { get; set; }
        public int StartHour { get; set; }
        public int Duration { get; set; }
    }
}
