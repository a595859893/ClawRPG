using Godot;
using System;
using System.Collections.Generic;

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
public class PlayerProfileSystem : Node
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
    
    public override void _Process(float delta)
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
    
    #endregion
}
