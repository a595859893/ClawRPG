using Godot;
using System;
using System.Collections.Generic;

public partial class GuildLevelSystem : BaseSystem
{
    private static GuildLevelSystem _instance;
    public static GuildLevelSystem Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = new GuildLevelSystem();
            }
            return _instance;
        }
    }
    
    // Guild level data storage
    private Dictionary<int, GuildLevelData> _guildLevels = new Dictionary<int, GuildLevelData>();
    
    // Signals
    public static string SignalGuildLevelUp = "guild_level_up";
    public static string SignalGuildExperienceGained = "guild_experience_gained";
    public static string SignalPerkUnlocked = "perk_unlocked";
    
    public GuildLevelSystem()
    {
        _instance = this;
    }
    
    public void Initialize()
    {
        LoadGuildLevels();
    }
    
    #region Data Management
    
    private void LoadGuildLevels()
    {
        var saveSystem = SaveSystem.Instance;
        if (saveSystem != null)
        {
            var data = saveSystem.GetData<Dictionary<int, GuildLevelData>>("guild_level_data");
            if (data != null)
            {
                _guildLevels = data;
            }
        }
    }
    
    public void SaveGuildLevels()
    {
        var saveSystem = SaveSystem.Instance;
        if (saveSystem != null)
        {
            saveSystem.SaveData("guild_level_data", _guildLevels);
        }
    }
    
    #endregion
    
    #region Level Management
    
    public GuildLevelData GetOrCreateGuildLevel(int guildId)
    {
        if (!_guildLevels.ContainsKey(guildId))
        {
            _guildLevels[guildId] = new GuildLevelData
            {
                GuildId = guildId,
                Level = 1,
                Experience = 0,
                TotalExperience = 0,
                MaxMembers = 10,
                LastDailyReset = OS.GetSystemTimeMsecs(),
                LastWeeklyReset = OS.GetSystemTimeMsecs()
            };
            SaveGuildLevels();
        }
        return _guildLevels[guildId];
    }
    
    public int GetGuildLevel(int guildId)
    {
        var data = GetOrCreateGuildLevel(guildId);
        return data.Level;
    }
    
    public int GetGuildExperience(int guildId)
    {
        var data = GetOrCreateGuildLevel(guildId);
        return data.Experience;
    }
    
    public int GetExperienceForNextLevel(int guildId)
    {
        var data = GetOrCreateGuildLevel(guildId);
        int currentLevelExp = GuildLevelDatabase.GetExperienceForLevel(data.Level);
        int nextLevelExp = GuildLevelDatabase.GetExperienceForLevel(data.Level + 1);
        return nextLevelExp - currentLevelExp;
    }
    
    public float GetLevelProgress(int guildId)
    {
        var data = GetOrCreateGuildLevel(guildId);
        int currentLevelExp = GuildLevelDatabase.GetExperienceForLevel(data.Level);
        int nextLevelExp = GuildLevelDatabase.GetExperienceForLevel(data.Level + 1);
        
        if (nextLevelExp == 0) return 1.0f;
        
        float progress = (float)(data.Experience - currentLevelExp) / (nextLevelExp - currentLevelExp);
        return Mathf.Clamp(progress, 0f, 1f);
    }
    
    public int GetMaxMembers(int guildId)
    {
        var data = GetOrCreateGuildLevel(guildId);
        return data.MaxMembers;
    }
    
    #endregion
    
    #region Experience & Level Up
    
    public void AddExperience(int guildId, int amount)
    {
        var data = GetOrCreateGuildLevel(guildId);
        
        // Apply any experience bonuses from perks
        float expBonus = GetExpBonus(guildId);
        int finalAmount = (int)(amount * (1 + expBonus));
        
        data.Experience += finalAmount;
        data.TotalExperience += finalAmount;
        
        // Check for level up
        CheckLevelUp(guildId);
        
        // Emit signal
        EmitSignal(SignalGuildExperienceGained, guildId, finalAmount);
        
        SaveGuildLevels();
    }
    
    private void CheckLevelUp(int guildId)
    {
        var data = GetOrCreateGuildLevel(guildId);
        int maxLevel = GuildLevelDatabase.GetMaxLevel();
        
        while (data.Level < maxLevel)
        {
            int currentLevelExp = GuildLevelDatabase.GetExperienceForLevel(data.Level);
            int nextLevelExp = GuildLevelDatabase.GetExperienceForLevel(data.Level + 1);
            
            if (data.Experience >= nextLevelExp)
            {
                data.Level++;
                
                // Update max members based on level
                data.MaxMembers = 10 + (data.Level - 1) * 2;
                
                // Check for new perks
                CheckPerkUnlocks(guildId);
                
                // Emit level up signal
                EmitSignal(SignalGuildLevelUp, guildId, data.Level);
                
                GD.Print($"[GuildLevel] Guild {guildId} leveled up to {data.Level}!");
            }
            else
            {
                break;
            }
        }
    }
    
    #endregion
    
    #region Perks
    
    private void CheckPerkUnlocks(int guildId)
    {
        var data = GetOrCreateGuildLevel(guildId);
        var availablePerks = GuildLevelDatabase.GetPerksForLevel(data.Level);
        
        foreach (string perkId in availablePerks)
        {
            if (!data.UnlockedPerks.Contains(perkId))
            {
                data.UnlockedPerks.Add(perkId);
                EmitSignal(SignalPerkUnlocked, guildId, perkId);
                GD.Print($"[GuildLevel] Guild {guildId} unlocked perk: {perkId}");
            }
        }
    }
    
    public List<string> GetUnlockedPerks(int guildId)
    {
        var data = GetOrCreateGuildLevel(guildId);
        return new List<string>(data.UnlockedPerks);
    }
    
    public Dictionary<string, object> GetPerkInfo(string perkId)
    {
        var definitions = GuildLevelDatabase.GetPerkDefinitions();
        if (definitions.ContainsKey(perkId))
        {
            return definitions[perkId];
        }
        return null;
    }
    
    #endregion
    
    #region Bonuses
    
    public float GetGoldBonus(int guildId)
    {
        var data = GetOrCreateGuildLevel(guildId);
        float bonus = 0f;
        
        foreach (string perkId in data.UnlockedPerks)
        {
            var perkInfo = GetPerkInfo(perkId);
            if (perkInfo != null && perkInfo["type"].ToString() == "gold_bonus")
            {
                bonus += Convert.ToSingle(perkInfo["value"]);
            }
        }
        
        return bonus;
    }
    
    public float GetExpBonus(int guildId)
    {
        var data = GetOrCreateGuildLevel(guildId);
        float bonus = 0f;
        
        foreach (string perkId in data.UnlockedPerks)
        {
            var perkInfo = GetPerkInfo(perkId);
            if (perkInfo != null && perkInfo["type"].ToString() == "exp_bonus")
            {
                bonus += Convert.ToSingle(perkInfo["value"]);
            }
        }
        
        return bonus;
    }
    
    public float GetWarBonus(int guildId)
    {
        var data = GetOrCreateGuildLevel(guildId);
        float bonus = 0f;
        
        foreach (string perkId in data.UnlockedPerks)
        {
            var perkInfo = GetPerkInfo(perkId);
            if (perkInfo != null && perkInfo["type"].ToString() == "war_bonus")
            {
                bonus += Convert.ToSingle(perkInfo["value"]);
            }
        }
        
        return bonus;
    }
    
    public float GetQuestDiscount(int guildId)
    {
        var data = GetOrCreateGuildLevel(guildId);
        float discount = 0f;
        
        foreach (string perkId in data.UnlockedPerks)
        {
            var perkInfo = GetPerkInfo(perkId);
            if (perkInfo != null && perkInfo["type"].ToString() == "quest_discount")
            {
                discount += Convert.ToSingle(perkInfo["value"]);
            }
        }
        
        return discount;
    }
    
    public float GetTechDiscount(int guildId)
    {
        var data = GetOrCreateGuildLevel(guildId);
        float discount = 0f;
        
        foreach (string perkId in data.UnlockedPerks)
        {
            var perkInfo = GetPerkInfo(perkId);
            if (perkInfo != null && perkInfo["type"].ToString() == "tech_discount")
            {
                discount += Convert.ToSingle(perkInfo["value"]);
            }
        }
        
        return discount;
    }
    
    public float GetBankDiscount(int guildId)
    {
        var data = GetOrCreateGuildLevel(guildId);
        float discount = 0f;
        
        foreach (string perkId in data.UnlockedPerks)
        {
            var perkInfo = GetPerkInfo(perkId);
            if (perkInfo != null && perkInfo["type"].ToString() == "bank_discount")
            {
                discount += Convert.ToSingle(perkInfo["value"]);
            }
        }
        
        return discount;
    }
    
    public float GetLootBonus(int guildId)
    {
        var data = GetOrCreateGuildLevel(guildId);
        float bonus = 0f;
        
        foreach (string perkId in data.UnlockedPerks)
        {
            var perkInfo = GetPerkInfo(perkId);
            if (perkInfo != null && perkInfo["type"].ToString() == "loot_bonus")
            {
                bonus += Convert.ToSingle(perkInfo["value"]);
            }
        }
        
        return bonus;
    }
    
    #endregion
    
    #region Statistics
    
    public void RecordQuestCompletion(int guildId)
    {
        var data = GetOrCreateGuildLevel(guildId);
        data.TotalQuestsCompleted++;
        data.DailyQuestsCompleted++;
        data.WeeklyQuestsCompleted++;
        SaveGuildLevels();
    }
    
    public void RecordWarResult(int guildId, bool won)
    {
        var data = GetOrCreateGuildLevel(guildId);
        if (won)
        {
            data.TotalWarsWon++;
        }
        else
        {
            data.TotalWarsLost++;
        }
        SaveGuildLevels();
    }
    
    public void RecordTechnologyResearch(int guildId)
    {
        var data = GetOrCreateGuildLevel(guildId);
        data.TotalTechnologyResearched++;
        SaveGuildLevels();
    }
    
    public void AddContribution(int guildId, int amount)
    {
        var data = GetOrCreateGuildLevel(guildId);
        data.DailyContributions += amount;
        data.WeeklyContributions += amount;
        SaveGuildLevels();
    }
    
    public Dictionary<string, int> GetGuildStats(int guildId)
    {
        var data = GetOrCreateGuildLevel(guildId);
        return new Dictionary<string, int>
        {
            ["level"] = data.Level,
            ["experience"] = data.Experience,
            ["total_experience"] = data.TotalExperience,
            ["max_members"] = data.MaxMembers,
            ["total_quests"] = data.TotalQuestsCompleted,
            ["wars_won"] = data.TotalWarsWon,
            ["wars_lost"] = data.TotalWarsLost,
            ["tech_researched"] = data.TotalTechnologyResearched,
            ["daily_contributions"] = data.DailyContributions,
            ["weekly_contributions"] = data.WeeklyContributions,
            ["daily_quests"] = data.DailyQuestsCompleted,
            ["weekly_quests"] = data.WeeklyQuestsCompleted
        };
    }
    
    #endregion
    
    #region Reset
    
    public void ResetDailyStats(int guildId)
    {
        var data = GetOrCreateGuildLevel(guildId);
        data.DailyContributions = 0;
        data.DailyQuestsCompleted = 0;
        data.LastDailyReset = OS.GetSystemTimeMsecs();
        SaveGuildLevels();
    }
    
    public void ResetWeeklyStats(int guildId)
    {
        var data = GetOrCreateGuildLevel(guildId);
        data.WeeklyContributions = 0;
        data.WeeklyQuestsCompleted = 0;
        data.LastWeeklyReset = OS.GetSystemTimeMsecs();
        SaveGuildLevels();
    }
    
    #endregion
    
    /// <summary>
    /// 导出保存数据
    /// </summary>
    public override Dictionary<string, object> ExportSaveData() {
        var data = new Dictionary<string, object>();
        
        var guildLevelsList = new List<Dictionary>();
        foreach (var kvp in _guildLevels) {
            var guildData = new Dictionary<string, object>();
            guildData["guildId"] = kvp.Key;
            guildData["level"] = kvp.Value.Level;
            guildData["experience"] = kvp.Value.Experience;
            guildData["totalExperience"] = kvp.Value.TotalExperience;
            guildData["maxMembers"] = kvp.Value.MaxMembers;
            guildData["unlockedPerks"] = new Godot.Array(kvp.Value.UnlockedPerks);
            guildData["totalQuestsCompleted"] = kvp.Value.TotalQuestsCompleted;
            guildData["totalWarsWon"] = kvp.Value.TotalWarsWon;
            guildData["totalWarsLost"] = kvp.Value.TotalWarsLost;
            guildData["totalTechnologyResearched"] = kvp.Value.TotalTechnologyResearched;
            guildData["dailyContributions"] = kvp.Value.DailyContributions;
            guildData["weeklyContributions"] = kvp.Value.WeeklyContributions;
            guildData["dailyQuestsCompleted"] = kvp.Value.DailyQuestsCompleted;
            guildData["weeklyQuestsCompleted"] = kvp.Value.WeeklyQuestsCompleted;
            guildData["lastDailyReset"] = kvp.Value.LastDailyReset;
            guildData["lastWeeklyReset"] = kvp.Value.LastWeeklyReset;
            guildLevelsList.Add(guildData);
        }
        data["guildLevels"] = guildLevelsList;
        
        return data;
    }

    /// <summary>
    /// 导入保存数据
    /// </summary>
    public override void ImportSaveData(Dictionary<string, object> data) {
        if (data == null) return;
        
        _guildLevels.Clear();
        
        if (data.Contains("guildLevels")) {
            var guildLevelsList = (Godot.Array)data["guildLevels"];
            foreach (Dictionary guildData in guildLevelsList) {
                var levelData = new GuildLevelData();
                levelData.GuildId = (int)guildData["guildId"];
                levelData.Level = (int)guildData["level"];
                levelData.Experience = (int)guildData["experience"];
                levelData.TotalExperience = (int)guildData["totalExperience"];
                levelData.MaxMembers = (int)guildData["maxMembers"];
                levelData.UnlockedPerks = ((Godot.Array)guildData["unlockedPerks"]).Select(v => (string)v).ToList();
                levelData.TotalQuestsCompleted = (int)guildData["totalQuestsCompleted"];
                levelData.TotalWarsWon = (int)guildData["totalWarsWon"];
                levelData.TotalWarsLost = (int)guildData["totalWarsLost"];
                levelData.TotalTechnologyResearched = (int)guildData["totalTechnologyResearched"];
                levelData.DailyContributions = (int)guildData["dailyContributions"];
                levelData.WeeklyContributions = (int)guildData["weeklyContributions"];
                levelData.DailyQuestsCompleted = (int)guildData["dailyQuestsCompleted"];
                levelData.WeeklyQuestsCompleted = (int)guildData["weeklyQuestsCompleted"];
                levelData.LastDailyReset = (long)guildData["lastDailyReset"];
                levelData.LastWeeklyReset = (long)guildData["lastWeeklyReset"];
                
                _guildLevels[levelData.GuildId] = levelData;
            }
        }
    }
}
