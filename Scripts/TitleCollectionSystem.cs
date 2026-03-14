using Godot;
using System;
using System.Collections.Generic;

public partial class TitleCollectionSystem : Node
{
    public static TitleCollectionSystem Instance { get; private set; }
    
    // Player title data
    private TitleCollectionData.PlayerTitleCollection _playerCollection = new();
    
    // Statistics
    private TitleCollectionData.TitleStatistics _statistics = new();
    
    // Signals
    [Signal] public delegate void TitleUnlockedEventHandler(string titleId, TitleCollectionData.Title title);
    [Signal] public delegate void TitleEquippedEventHandler(string titleId);
    [Signal] public delegate void TitleUnequippedEventHandler();
    
    // Callbacks for condition checking
    private Dictionary<string, Func<bool>> _customConditionChecks = new();
    
    public override void _Ready()
    {
        Instance = this;
        InitializeCustomConditions();
    }
    
    private void InitializeCustomConditions()
    {
        // Register custom condition checks
        _customConditionChecks["first_blood"] = CheckFirstBlood;
        _customConditionChecks["speed_demon"] = CheckSpeedDemon;
        _customConditionChecks["lucky_drop"] = CheckLuckyDrop;
        _customConditionChecks["secret_area"] = CheckSecretArea;
    }
    
    #region Public Methods
    
    public bool IsTitleUnlocked(string titleId)
    {
        return _playerCollection.UnlockedTitles.ContainsKey(titleId);
    }
    
    public void UnlockTitle(string titleId)
    {
        if (IsTitleUnlocked(titleId))
            return;
            
        var title = TitleCollectionDatabase.Instance.GetTitle(titleId);
        if (title == null)
            return;
            
        var playerTitleData = new TitleCollectionData.PlayerTitleData
        {
            TitleId = titleId,
            UnlockedAt = DateTime.Now,
            IsActive = false
        };
        
        _playerCollection.UnlockedTitles[titleId] = playerTitleData;
        _playerCollection.TotalUnlocked++;
        
        // Update category count
        var category = title.Category;
        if (_playerCollection.CategoryUnlocked.ContainsKey(category))
            _playerCollection.CategoryUnlocked[category]++;
        
        // Update statistics
        _statistics.TotalUnlocked++;
        _statistics.MostRecentTitle = titleId;
        if (_statistics.FirstUnlockTime == null)
            _statistics.FirstUnlockTime = DateTime.Now;
        
        // Emit signal
        EmitSignal(SignalName.TitleUnlocked, titleId, title);
        
        GD.Print($"[TitleCollection] Unlocked: {title.Name}");
    }
    
    public void EquipTitle(string titleId)
    {
        if (!IsTitleUnlocked(titleId))
            return;
            
        var playerTitleData = _playerCollection.UnlockedTitles[titleId];
        playerTitleData.IsActive = true;
        playerTitleData.EquippedAt = DateTime.Now;
        
        _playerCollection.ActiveTitleId = titleId;
        
        EmitSignal(SignalName.TitleEquipped, titleId);
        
        GD.Print($"[TitleCollection] Equipped: {titleId}");
    }
    
    public void UnequipTitle()
    {
        if (string.IsNullOrEmpty(_playerCollection.ActiveTitleId))
            return;
            
        var activeId = _playerCollection.ActiveTitleId;
        if (_playerCollection.UnlockedTitles.ContainsKey(activeId))
        {
            _playerCollection.UnlockedTitles[activeId].IsActive = false;
        }
        
        _playerCollection.ActiveTitleId = null;
        
        EmitSignal(SignalName.TitleUnequipped);
        
        GD.Print("[TitleCollection] Title unequipped");
    }
    
    public string GetActiveTitleId()
    {
        return _playerCollection.ActiveTitleId;
    }
    
    public TitleCollectionData.Title GetActiveTitle()
    {
        if (string.IsNullOrEmpty(_playerCollection.ActiveTitleId))
            return null;
            
        return TitleCollectionDatabase.Instance.GetTitle(_playerCollection.ActiveTitleId);
    }
    
    public Dictionary<string, TitleCollectionData.PlayerTitleData> GetUnlockedTitles()
    {
        return new Dictionary<string, TitleCollectionData.PlayerTitleData>(_playerCollection.UnlockedTitles);
    }
    
    public TitleCollectionData.TitleStatistics GetStatistics()
    {
        return _statistics;
    }
    
    public int GetTotalUnlocked() => _playerCollection.TotalUnlocked;
    
    public int GetTotalAvailable() => TitleCollectionDatabase.Instance.GetTotalTitleCount();
    
    public float GetCompletionPercentage()
    {
        int total = GetTotalAvailable();
        if (total == 0) return 0f;
        return (float)_playerCollection.TotalUnlocked / total * 100f;
    }
    
    #endregion
    
    #region Condition Checking
    
    public void CheckAndUnlockTitles()
    {
        var allTitles = TitleCollectionDatabase.Instance.GetAllTitles();
        
        foreach (var kvp in allTitles)
        {
            var title = kvp.Value;
            
            // Skip already unlocked
            if (IsTitleUnlocked(title.Id))
                continue;
            
            // Check hidden titles condition (skip for now)
            if (title.IsHidden)
                continue;
            
            // Check condition
            if (CheckCondition(title))
            {
                UnlockTitle(title.Id);
            }
        }
    }
    
    private bool CheckCondition(TitleCollectionData.Title title)
    {
        // Handle custom conditions
        if (title.Condition == TitleCollectionData.UnlockCondition.Custom)
        {
            if (!string.IsNullOrEmpty(title.CustomConditionScript) && 
                _customConditionChecks.ContainsKey(title.CustomConditionScript))
            {
                return _customConditionChecks[title.CustomConditionScript]();
            }
            return false;
        }
        
        // Get player stats (these would come from other systems)
        int playerKillCount = GetPlayerKillCount();
        int playerBossKills = GetPlayerBossKills();
        int playerDungeonCompletes = GetPlayerDungeonCompletes();
        int playerCraftCount = GetPlayerCraftCount();
        int playerAchievementCount = GetPlayerAchievementCount();
        int playerLevel = GetPlayerLevel();
        int playerGoldEarned = GetPlayerGoldEarned();
        int playerTradeCount = GetPlayerTradeCount();
        int playerPvPWins = GetPlayerPvPWins();
        int playerTimePlayed = GetPlayerTimePlayed();
        
        switch (title.Condition)
        {
            case TitleCollectionData.UnlockCondition.KillCount:
                return playerKillCount >= title.ConditionValue;
                
            case TitleCollectionData.UnlockCondition.BossKill:
                return playerBossKills >= title.ConditionValue;
                
            case TitleCollectionData.UnlockCondition.DungeonComplete:
                return playerDungeonCompletes >= title.ConditionValue;
                
            case TitleCollectionData.UnlockCondition.CraftCount:
                return playerCraftCount >= title.ConditionValue;
                
            case TitleCollectionData.UnlockCondition.AchievementCount:
                return playerAchievementCount >= title.ConditionValue;
                
            case TitleCollectionData.UnlockCondition.LevelReach:
                return playerLevel >= title.ConditionValue;
                
            case TitleCollectionData.UnlockCondition.GoldEarned:
                return playerGoldEarned >= title.ConditionValue;
                
            case TitleCollectionData.UnlockCondition.TradeCount:
                return playerTradeCount >= title.ConditionValue;
                
            case TitleCollectionData.UnlockCondition.PvPWins:
                return playerPvPWins >= title.ConditionValue;
                
            case TitleCollectionData.UnlockCondition.TimePlayed:
                return playerTimePlayed >= title.ConditionValue;
                
            default:
                return false;
        }
    }
    
    // Custom condition checks - these would integrate with actual game systems
    private bool CheckFirstBlood()
    {
        // Would check if player dealt first hit in a raid boss
        return false;
    }
    
    private bool CheckSpeedDemon()
    {
        // Would check if player completed dungeon under time limit
        return false;
    }
    
    private bool CheckLuckyDrop()
    {
        // Would check if player got a legendary drop
        return false;
    }
    
    private bool CheckSecretArea()
    {
        // Would check if player discovered secret area
        return false;
    }
    
    // Placeholder methods - would connect to actual player stats
    private int GetPlayerKillCount() => 0;
    private int GetPlayerBossKills() => 0;
    private int GetPlayerDungeonCompletes() => 0;
    private int GetPlayerCraftCount() => 0;
    private int GetPlayerAchievementCount() => 0;
    private int GetPlayerLevel() => 1;
    private int GetPlayerGoldEarned() => 0;
    private int GetPlayerTradeCount() => 0;
    private int GetPlayerPvPWins() => 0;
    private int GetPlayerTimePlayed() => 0;
    
    #endregion
    
    #region Save/Load
    
    public Dictionary<string, object> ExportSaveData()
    {
        var data = new Dictionary<string, object>();
        
        // Export unlocked titles
        var unlockedTitles = new List<Dictionary<string, object>>();
        foreach (var kvp in _playerCollection.UnlockedTitles)
        {
            unlockedTitles.Add(new Dictionary<string, object>
            {
                { "title_id", kvp.Key },
                { "unlocked_at", kvp.Value.UnlockedAt.ToString("o") },
                { "is_active", kvp.Value.IsActive },
                { "equipped_at", kvp.Value.EquippedAt?.ToString("o") ?? "" }
            });
        }
        data["unlocked_titles"] = unlockedTitles;
        data["active_title_id"] = _playerCollection.ActiveTitleId ?? "";
        data["total_unlocked"] = _playerCollection.TotalUnlocked;
        
        return data;
    }
    
    public void ImportSaveData(Dictionary<string, object> data)
    {
        if (data == null) return;
        
        _playerCollection = new TitleCollectionData.PlayerTitleCollection();
        
        // Import unlocked titles
        if (data.ContainsKey("unlocked_titles"))
        {
            var unlockedTitles = data["unlocked_titles"] as List<object>;
            if (unlockedTitles != null)
            {
                foreach (var titleData in unlockedTitles)
                {
                    var dict = titleData as Dictionary<string, object>;
                    if (dict == null) continue;
                    
                    var titleId = dict["title_id"] as string;
                    var unlockedAt = DateTime.Parse(dict["unlocked_at"] as string);
                    var isActive = (bool)dict["is_active"];
                    DateTime? equippedAt = null;
                    if (!string.IsNullOrEmpty(dict["equipped_at"] as string))
                    {
                        equippedAt = DateTime.Parse(dict["equipped_at"] as string);
                    }
                    
                    var playerTitleData = new TitleCollectionData.PlayerTitleData
                    {
                        TitleId = titleId,
                        UnlockedAt = unlockedAt,
                        IsActive = isActive,
                        EquippedAt = equippedAt
                    };
                    
                    _playerCollection.UnlockedTitles[titleId] = playerTitleData;
                    _playerCollection.TotalUnlocked++;
                }
            }
        }
        
        // Import active title
        if (data.ContainsKey("active_title_id"))
        {
            _playerCollection.ActiveTitleId = data["active_title_id"] as string;
        }
        
        // Update statistics
        _statistics.TotalUnlocked = _playerCollection.TotalUnlocked;
        if (_playerCollection.TotalUnlocked > 0)
        {
            _statistics.FirstUnlockTime = DateTime.Now;
        }
        
        GD.Print($"[TitleCollection] Loaded {_playerCollection.TotalUnlocked} titles");
    }
    
    #endregion
}
