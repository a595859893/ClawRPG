using Godot;
using System;
using System.Collections.Generic;
using SaveSystem = ClawRPG.Scripts.Framework.SaveSystem;

/// <summary>
/// 玩家档案数据
/// </summary>
public class PlayerProfileData
{
    // 玩家基本信息
    public string PlayerName { get; set; } = "Player";
    public int TotalPlayTime { get; set; } = 0; // 秒
    public DateTime FirstPlayDate { get; set; } = DateTime.Now;
    public DateTime LastPlayDate { get; set; } = DateTime.Now;
    public int CurrentLevel { get; set; } = 1;
    public long TotalExperience { get; set; } = 0;
    
    // 战斗统计
    public int TotalKills { get; set; } = 0;
    public int BossKills { get; set; } = 0;
    public int TotalDamageDealt { get; set; } = 0;
    public int TotalDamageTaken { get; set; } = 0;
    public int TotalHealingDone { get; set; } = 0;
    public int CriticalHits { get; set; } = 0;
    public int MaxCombo { get; set; } = 0;
    
    // 生存统计
    public int Deaths { get; set; } = 0;
    public int TotalGoldEarned { get; set; } = 0;
    public int TotalGoldSpent { get; set; } = 0;
    public int ItemsCollected { get; set; } = 0;
    public int ItemsCrafted { get; set; } = 0;
    
    // 探索统计
    public int RegionsDiscovered { get; set; } = 0;
    public int DungeonsCompleted { get; set; } = 0;
    public int QuestsCompleted { get; set; } = 0;
    public int SecretsFound { get; set; } = 0;
    
    // 社交统计
    public int TradesCompleted { get; set; } = 0;
    public int PvPWins { get; set; } = 0;
    public int PvPLosses { get; set; } = 0;
    public int PartiesJoined { get; set; } = 0;
    
    // 成就相关
    public int AchievementsUnlocked { get; set; } = 0;
    public int TotalAchievementPoints { get; set; } = 0;
}

/// <summary>
/// 玩家档案系统 - 显示玩家游戏统计和进度
/// </summary>
public partial class PlayerProfileSystem : BaseSystem
{
    public static PlayerProfileSystem Instance { get; private set; }
    
    // 玩家档案数据
    public PlayerProfileData Profile { get; private set; } = new PlayerProfileData();
    
    // 信号
    public delegate void ProfileUpdatedEvent();
    public event ProfileUpdatedEvent OnProfileUpdated;
    
    // 会话统计（重置）
    private int _sessionKills = 0;
    private int _sessionDamageDealt = 0;
    private int _sessionDamageTaken = 0;
    private int _sessionHealingDone = 0;
    private int _sessionGoldEarned = 0;
    private int _sessionPlayTime = 0;
    private DateTime _sessionStartTime;
    
    public override void _Ready()
    {
        Instance = this;
        _sessionStartTime = DateTime.Now;
        LoadProfile();
    }
    
    public override void _Process(double delta)
    {
        // 更新会话游戏时间
        _sessionPlayTime = (int)(DateTime.Now - _sessionStartTime).TotalSeconds;
    }
    
    #region 数据更新方法
    
    public void RecordKill(bool isBoss = false)
    {
        Profile.TotalKills++;
        _sessionKills++;
        if (isBoss) Profile.BossKills++;
        EmitSignal(nameof(OnProfileUpdated));
    }
    
    public void RecordDamageDealt(int damage, bool isCritical = false)
    {
        Profile.TotalDamageDealt += damage;
        _sessionDamageDealt += damage;
        if (isCritical)
        {
            Profile.CriticalHits++;
        }
        EmitSignal(nameof(OnProfileUpdated));
    }
    
    public void RecordDamageTaken(int damage)
    {
        Profile.TotalDamageTaken += damage;
        _sessionDamageTaken += damage;
        EmitSignal(nameof(OnProfileUpdated));
    }
    
    public void RecordHealing(int amount)
    {
        Profile.TotalHealingDone += amount;
        _sessionHealingDone += amount;
        EmitSignal(nameof(OnProfileUpdated));
    }
    
    public void RecordGoldEarned(int amount)
    {
        Profile.TotalGoldEarned += amount;
        _sessionGoldEarned += amount;
        Profile.TotalGoldSpent += amount; // 收入
        EmitSignal(nameof(OnProfileUpdated));
    }
    
    public void RecordGoldSpent(int amount)
    {
        Profile.TotalGoldSpent += amount;
        Profile.TotalGoldEarned -= amount; // 支出
        EmitSignal(nameof(OnProfileUpdated));
    }
    
    public void RecordDeath()
    {
        Profile.Deaths++;
        EmitSignal(nameof(OnProfileUpdated));
    }
    
    public void RecordItemCollected()
    {
        Profile.ItemsCollected++;
        EmitSignal(nameof(OnProfileUpdated));
    }
    
    public void RecordItemCrafted()
    {
        Profile.ItemsCrafted++;
        EmitSignal(nameof(OnProfileUpdated));
    }
    
    public void RecordRegionDiscovered()
    {
        Profile.RegionsDiscovered++;
        EmitSignal(nameof(OnProfileUpdated));
    }
    
    public void RecordDungeonCompleted()
    {
        Profile.DungeonsCompleted++;
        EmitSignal(nameof(OnProfileUpdated));
    }
    
    public void RecordQuestCompleted()
    {
        Profile.QuestsCompleted++;
        EmitSignal(nameof(OnProfileUpdated));
    }
    
    public void RecordSecretFound()
    {
        Profile.SecretsFound++;
        EmitSignal(nameof(OnProfileUpdated));
    }
    
    public void RecordTrade()
    {
        Profile.TradesCompleted++;
        EmitSignal(nameof(OnProfileUpdated));
    }
    
    public void RecordPvPWin()
    {
        Profile.PvPWins++;
        EmitSignal(nameof(OnProfileUpdated));
    }
    
    public void RecordPvPLoss()
    {
        Profile.PvPLosses++;
        EmitSignal(nameof(OnProfileUpdated));
    }
    
    public void RecordPartyJoined()
    {
        Profile.PartiesJoined++;
        EmitSignal(nameof(OnProfileUpdated));
    }
    
    public void UpdateMaxCombo(int combo)
    {
        if (combo > Profile.MaxCombo)
        {
            Profile.MaxCombo = combo;
            EmitSignal(nameof(OnProfileUpdated));
        }
    }
    
    public void UpdateLevel(int level)
    {
        Profile.CurrentLevel = level;
        EmitSignal(nameof(OnProfileUpdated));
    }
    
    public void UpdateExperience(long exp)
    {
        Profile.TotalExperience = exp;
        EmitSignal(nameof(OnProfileUpdated));
    }
    
    public void UnlockAchievement(int points)
    {
        Profile.AchievementsUnlocked++;
        Profile.TotalAchievementPoints += points;
        EmitSignal(nameof(OnProfileUpdated));
    }
    
    #endregion
    
    #region 统计获取方法
    
    public int GetSessionPlayTime() => _sessionPlayTime;
    public int GetSessionKills() => _sessionKills;
    public int GetSessionDamageDealt() => _sessionDamageDealt;
    public int GetSessionGoldEarned() => _sessionGoldEarned;
    
    public float GetKDA()
    {
        if (Profile.Deaths == 0) return Profile.TotalKills;
        return (float)Profile.TotalKills / Profile.Deaths;
    }
    
    public float GetWinRate()
    {
        int total = Profile.PvPWins + Profile.PvPLosses;
        if (total == 0) return 0;
        return (float)Profile.PvPWins / total * 100;
    }
    
    public string GetPlayTimeFormatted()
    {
        int totalSeconds = Profile.TotalPlayTime + _sessionPlayTime;
        int hours = totalSeconds / 3600;
        int minutes = (totalSeconds % 3600) / 60;
        return $"{hours}h {minutes}m";
    }
    
    public string GetDPS()
    {
        int playTime = Profile.TotalPlayTime + _sessionPlayTime;
        if (playTime == 0) return "0";
        int totalDamage = Profile.TotalDamageDealt + _sessionDamageDealt;
        return (totalDamage / playTime).ToString();
    }
    
    #endregion
    
    #region 存档
    
    public void SaveProfile()
    {
        // 更新总游戏时间
        Profile.TotalPlayTime += _sessionPlayTime;
        Profile.LastPlayDate = DateTime.Now;
        
        var saveData = new Dictionary<string, object>
        {
            {"player_name", Profile.PlayerName},
            {"total_play_time", Profile.TotalPlayTime},
            {"first_play_date", Profile.FirstPlayDate.ToString("o")},
            {"last_play_date", Profile.LastPlayDate.ToString("o")},
            {"current_level", Profile.CurrentLevel},
            {"total_experience", Profile.TotalExperience},
            {"total_kills", Profile.TotalKills},
            {"boss_kills", Profile.BossKills},
            {"total_damage_dealt", Profile.TotalDamageDealt},
            {"total_damage_taken", Profile.TotalDamageTaken},
            {"total_healing_done", Profile.TotalHealingDone},
            {"critical_hits", Profile.CriticalHits},
            {"max_combo", Profile.MaxCombo},
            {"deaths", Profile.Deaths},
            {"total_gold_earned", Profile.TotalGoldEarned},
            {"total_gold_spent", Profile.TotalGoldSpent},
            {"items_collected", Profile.ItemsCollected},
            {"items_crafted", Profile.ItemsCrafted},
            {"regions_discovered", Profile.RegionsDiscovered},
            {"dungeons_completed", Profile.DungeonsCompleted},
            {"quests_completed", Profile.QuestsCompleted},
            {"secrets_found", Profile.SecretsFound},
            {"trades_completed", Profile.TradesCompleted},
            {"pvp_wins", Profile.PvPWins},
            {"pvp_losses", Profile.PvPLosses},
            {"parties_joined", Profile.PartiesJoined},
            {"achievements_unlocked", Profile.AchievementsUnlocked},
            {"total_achievement_points", Profile.TotalAchievementPoints}
        };
        
        SaveSystem.Save("player_profile", saveData);
    }
    
    public void LoadProfile()
    {
        var saveData = SaveSystem.Load("player_profile");
        if (saveData == null) return;
        
        Profile.PlayerName = saveData.GetValueOrDefault("player_name", "Player").ToString();
        Profile.TotalPlayTime = saveData.GetValueOrDefault("total_play_time", 0);
        Profile.FirstPlayDate = DateTime.TryParse(saveData.GetValueOrDefault("first_play_date", "").ToString(), out var fpd) ? fpd : DateTime.Now;
        Profile.LastPlayDate = DateTime.TryParse(saveData.GetValueOrDefault("last_play_date", "").ToString(), out var lpd) ? lpd : DateTime.Now;
        Profile.CurrentLevel = saveData.GetValueOrDefault("current_level", 1);
        Profile.TotalExperience = saveData.GetValueOrDefault("total_experience", 0L);
        Profile.TotalKills = saveData.GetValueOrDefault("total_kills", 0);
        Profile.BossKills = saveData.GetValueOrDefault("boss_kills", 0);
        Profile.TotalDamageDealt = saveData.GetValueOrDefault("total_damage_dealt", 0);
        Profile.TotalHealingDone = saveData.GetValueOrDefault("total_healing_done", 0);
        Profile.CriticalHits = saveData.GetValueOrDefault("critical_hits", 0);
        Profile.MaxCombo = saveData.GetValueOrDefault("max_combo", 0);
        Profile.Deaths = saveData.GetValueOrDefault("deaths", 0);
        Profile.TotalGoldEarned = saveData.GetValueOrDefault("total_gold_earned", 0);
        Profile.TotalGoldSpent = saveData.GetValueOrDefault("total_gold_spent", 0);
        Profile.ItemsCollected = saveData.GetValueOrDefault("items_collected", 0);
        Profile.ItemsCrafted = saveData.GetValueOrDefault("items_crafted", 0);
        Profile.RegionsDiscovered = saveData.GetValueOrDefault("regions_discovered", 0);
        Profile.DungeonsCompleted = saveData.GetValueOrDefault("dungeons_completed", 0);
        Profile.QuestsCompleted = saveData.GetValueOrDefault("quests_completed", 0);
        Profile.SecretsFound = saveData.GetValueOrDefault("secrets_found", 0);
        Profile.TradesCompleted = saveData.GetValueOrDefault("trades_completed", 0);
        Profile.PvPWins = saveData.GetValueOrDefault("pvp_wins", 0);
        Profile.PvPLosses = saveData.GetValueOrDefault("pvp_losses", 0);
        Profile.PartiesJoined = saveData.GetValueOrDefault("parties_joined", 0);
        Profile.AchievementsUnlocked = saveData.GetValueOrDefault("achievements_unlocked", 0);
        Profile.TotalAchievementPoints = saveData.GetValueOrDefault("total_achievement_points", 0);
    }

    // ===== 持久化方法 =====

    public override Dictionary<string, object> ExportSaveData()
    {
        var data = new Dictionary<string, object>();
        
        // 玩家基本信息
        data["player_name"] = Profile.PlayerName;
        data["total_play_time"] = Profile.TotalPlayTime;
        data["first_play_date"] = Profile.FirstPlayDate.ToString("o");
        data["last_play_date"] = Profile.LastPlayDate.ToString("o");
        data["current_level"] = Profile.CurrentLevel;
        data["total_experience"] = Profile.TotalExperience;
        
        // 战斗统计
        data["total_kills"] = Profile.TotalKills;
        data["boss_kills"] = Profile.BossKills;
        data["total_damage_dealt"] = Profile.TotalDamageDealt;
        data["total_damage_taken"] = Profile.TotalDamageTaken;
        data["total_healing_done"] = Profile.TotalHealingDone;
        data["critical_hits"] = Profile.CriticalHits;
        data["max_combo"] = Profile.MaxCombo;
        
        // 生存统计
        data["deaths"] = Profile.Deaths;
        data["total_gold_earned"] = Profile.TotalGoldEarned;
        data["total_gold_spent"] = Profile.TotalGoldSpent;
        data["items_collected"] = Profile.ItemsCollected;
        data["items_crafted"] = Profile.ItemsCrafted;
        
        // 探索统计
        data["regions_discovered"] = Profile.RegionsDiscovered;
        data["dungeons_completed"] = Profile.DungeonsCompleted;
        data["quests_completed"] = Profile.QuestsCompleted;
        data["secrets_found"] = Profile.SecretsFound;
        
        // 社交统计
        data["trades_completed"] = Profile.TradesCompleted;
        data["pvp_wins"] = Profile.PvPWins;
        data["pvp_losses"] = Profile.PvPLosses;
        data["parties_joined"] = Profile.PartiesJoined;
        
        // 成就统计
        data["achievements_unlocked"] = Profile.AchievementsUnlocked;
        data["total_achievement_points"] = Profile.TotalAchievementPoints;
        
        return data;
    }

    public override void ImportSaveData(Dictionary<string, object> data)
    {
        if (data == null) return;
        
        // 玩家基本信息
        if (data.ContainsKey("player_name"))
            Profile.PlayerName = data["player_name"].ToString();
        if (data.ContainsKey("total_play_time"))
            Profile.TotalPlayTime = (int)data["total_play_time"];
        if (data.ContainsKey("first_play_date"))
            DateTime.TryParse(data["first_play_date"].ToString(), out Profile.FirstPlayDate);
        if (data.ContainsKey("last_play_date"))
            DateTime.TryParse(data["last_play_date"].ToString(), out Profile.LastPlayDate);
        if (data.ContainsKey("current_level"))
            Profile.CurrentLevel = (int)data["current_level"];
        if (data.ContainsKey("total_experience"))
            Profile.TotalExperience = Convert.ToInt64(data["total_experience"]);
        
        // 战斗统计
        if (data.ContainsKey("total_kills"))
            Profile.TotalKills = (int)data["total_kills"];
        if (data.ContainsKey("boss_kills"))
            Profile.BossKills = (int)data["boss_kills"];
        if (data.ContainsKey("total_damage_dealt"))
            Profile.TotalDamageDealt = (int)data["total_damage_dealt"];
        if (data.ContainsKey("total_damage_taken"))
            Profile.TotalDamageTaken = (int)data["total_damage_taken"];
        if (data.ContainsKey("total_healing_done"))
            Profile.TotalHealingDone = (int)data["total_healing_done"];
        if (data.ContainsKey("critical_hits"))
            Profile.CriticalHits = (int)data["critical_hits"];
        if (data.ContainsKey("max_combo"))
            Profile.MaxCombo = (int)data["max_combo"];
        
        // 生存统计
        if (data.ContainsKey("deaths"))
            Profile.Deaths = (int)data["deaths"];
        if (data.ContainsKey("total_gold_earned"))
            Profile.TotalGoldEarned = (int)data["total_gold_earned"];
        if (data.ContainsKey("total_gold_spent"))
            Profile.TotalGoldSpent = (int)data["total_gold_spent"];
        if (data.ContainsKey("items_collected"))
            Profile.ItemsCollected = (int)data["items_collected"];
        if (data.ContainsKey("items_crafted"))
            Profile.ItemsCrafted = (int)data["items_crafted"];
        
        // 探索统计
        if (data.ContainsKey("regions_discovered"))
            Profile.RegionsDiscovered = (int)data["regions_discovered"];
        if (data.ContainsKey("dungeons_completed"))
            Profile.DungeonsCompleted = (int)data["dungeons_completed"];
        if (data.ContainsKey("quests_completed"))
            Profile.QuestsCompleted = (int)data["quests_completed"];
        if (data.ContainsKey("secrets_found"))
            Profile.SecretsFound = (int)data["secrets_found"];
        
        // 社交统计
        if (data.ContainsKey("trades_completed"))
            Profile.TradesCompleted = (int)data["trades_completed"];
        if (data.ContainsKey("pvp_wins"))
            Profile.PvPWins = (int)data["pvp_wins"];
        if (data.ContainsKey("pvp_losses"))
            Profile.PvPLosses = (int)data["pvp_losses"];
        if (data.ContainsKey("parties_joined"))
            Profile.PartiesJoined = (int)data["parties_joined"];
        
        // 成就统计
        if (data.ContainsKey("achievements_unlocked"))
            Profile.AchievementsUnlocked = (int)data["achievements_unlocked"];
        if (data.ContainsKey("total_achievement_points"))
            Profile.TotalAchievementPoints = (int)data["total_achievement_points"];
    }
    
    #endregion
}
