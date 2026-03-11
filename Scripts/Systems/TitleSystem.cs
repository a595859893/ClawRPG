using Godot;
using System;
using System.Collections.Generic;

public class TitleSystem : Node
{
    private static TitleSystem _instance;
    public static TitleSystem Instance
    {
        get
        {
            return _instance;
        }
    }
    
    public PlayerTitleCollection PlayerTitles = new PlayerTitleCollection();
    public SignalManager.Signals Signals => SignalManager.Instance.Signals;
    
    // 统计跟踪
    private int _totalKills = 0;
    private int _bossKills = 0;
    private int _maxCombo = 0;
    private int _totalCrits = 0;
    private int _totalDamageTaken = 0;
    private int _fishingCount = 0;
    private int _alchemyCount = 0;
    private int _areasVisited = 0;
    private int _itemsCollected = 0;
    private int _dungeonsCompleted = 0;
    private int _playersHelped = 0;
    private int _itemsSold = 0;
    private int _friendsAdded = 0;
    private int _treasurePointsFound = 0;
    private HashSet<string> _visitedAreas = new HashSet<string>();
    private HashSet<string> _collectedItems = new HashSet<string>();
    
    public override void _Ready()
    {
        _instance = this;
        AddToGroup("TitleSystem");
        LoadTitleData();
    }
    
    public void Initialize()
    {
        // 初始化数据库
        var db = TitleDatabase.Instance;
        
        // 确保所有称号数据存在
        foreach (var titleDef in db.GetAllTitles())
        {
            if (!PlayerTitles.Titles.ContainsKey(titleDef.Id))
            {
                PlayerTitles.Titles[titleDef.Id] = new PlayerTitleData
                {
                    TitleId = titleDef.Id,
                    IsUnlocked = false,
                    IsActive = false
                };
            }
        }
        
        GD.Print($"[TitleSystem] Initialized with {PlayerTitles.Titles.Count} titles");
    }
    
    // 解锁称号
    public bool UnlockTitle(string titleId)
    {
        if (PlayerTitles.Titles.ContainsKey(titleId))
        {
            var titleData = PlayerTitles.Titles[titleId];
            if (!titleData.IsUnlocked)
            {
                titleData.IsUnlocked = true;
                titleData.UnlockTime = DateTime.Now;
                SaveTitleData();
                Signals.EmitSignal(SignalManager.SignalNames.TitleUnlocked, titleId);
                GD.Print($"[TitleSystem] Title unlocked: {titleId}");
                return true;
            }
        }
        return false;
    }
    
    // 激活称号
    public bool ActivateTitle(string titleId)
    {
        if (PlayerTitles.Titles.ContainsKey(titleId))
        {
            var titleData = PlayerTitles.Titles[titleId];
            if (titleData.IsUnlocked)
            {
                // 取消之前的激活状态
                if (PlayerTitles.ActiveTitleId != "" && PlayerTitles.Titles.ContainsKey(PlayerTitles.ActiveTitleId))
                {
                    PlayerTitles.Titles[PlayerTitles.ActiveTitleId].IsActive = false;
                }
                
                titleData.IsActive = true;
                PlayerTitles.ActiveTitleId = titleId;
                SaveTitleData();
                
                var titleDef = TitleDatabase.Instance.GetTitle(titleId);
                if (titleDef != null)
                {
                    Signals.EmitSignal(SignalManager.SignalNames.TitleActivated, titleId, titleDef.Name);
                }
                return true;
            }
        }
        return false;
    }
    
    // 取消激活称号
    public void DeactivateTitle(string titleId)
    {
        if (PlayerTitles.Titles.ContainsKey(titleId))
        {
            PlayerTitles.Titles[titleId].IsActive = false;
            if (PlayerTitles.ActiveTitleId == titleId)
            {
                PlayerTitles.ActiveTitleId = "";
            }
            SaveTitleData();
        }
    }
    
    // 获取激活称号的属性加成
    public Dictionary<string, float> GetActiveTitleBonuses()
    {
        Dictionary<string, float> bonuses = new Dictionary<string, float>();
        
        if (PlayerTitles.ActiveTitleId != "" && PlayerTitles.Titles.ContainsKey(PlayerTitles.ActiveTitleId))
        {
            var titleDef = TitleDatabase.Instance.GetTitle(PlayerTitles.ActiveTitleId);
            if (titleDef != null && titleDef.AttributeBonuses != null)
            {
                foreach (var kvp in titleDef.AttributeBonuses)
                {
                    bonuses[kvp.Key] = kvp.Value;
                }
            }
        }
        
        return bonuses;
    }
    
    // 检查并更新称号解锁状态
    public void CheckAndUnlockTitles()
    {
        var db = TitleDatabase.Instance;
        
        // 战斗称号检查
        CheckTitle("combat_novice", _totalKills >= 1);
        CheckTitle("combat_veteran", _totalKills >= 100);
        CheckTitle("boss_slayer", _bossKills >= 10);
        CheckTitle("unstoppable", _maxCombo >= 10);
        CheckTitle("critical_master", _totalCrits >= 100);
        CheckTitle("tank_master", _totalDamageTaken >= 1000);
        
        // 采集称号检查
        CheckTitle("fisherman", _fishingCount >= 10);
        CheckTitle("miner", true); // 需要根据挖掘系统集成
        
        // 探索称号检查
        CheckTitle("explorer", _areasVisited >= 5);
        CheckTitle("collector", _itemsCollected >= 100);
        CheckTitle("treasure_hunter", _treasurePointsFound >= 50);
        
        // 社交称号检查
        CheckTitle("team_leader", _dungeonsCompleted >= 10);
        CheckTitle("mentor", _playersHelped >= 10);
        CheckTitle("merchant", _itemsSold >= 100);
        CheckTitle("social_butterfly", _friendsAdded >= 50);
        
        // 检查完美主义者
        int unlockedCount = 0;
        foreach (var titleData in PlayerTitles.Titles.Values)
        {
            if (titleData.IsUnlocked)
                unlockedCount++;
        }
        
        var allTitles = db.GetAllTitles();
        if (unlockedCount >= allTitles.Count - 1) // 减去完美主义者本身
        {
            CheckTitle("perfectionist", true);
        }
    }
    
    private void CheckTitle(string titleId, bool condition)
    {
        if (condition && PlayerTitles.Titles.ContainsKey(titleId))
        {
            UnlockTitle(titleId);
        }
    }
    
    // 统计更新方法
    public void OnEnemyKilled(bool isBoss)
    {
        _totalKills++;
        if (isBoss)
        {
            _bossKills++;
            CheckAndUnlockTitles();
        }
    }
    
    public void OnComboMilestone(int combo)
    {
        if (combo > _maxCombo)
        {
            _maxCombo = combo;
            CheckAndUnlockTitles();
        }
    }
    
    public void OnCrit()
    {
        _totalCrits++;
        CheckAndUnlockTitles();
    }
    
    public void OnDamageTaken(int damage)
    {
        _totalDamageTaken += damage;
        CheckAndUnlockTitles();
    }
    
    public void OnFishCaught()
    {
        _fishingCount++;
        CheckAndUnlockTitles();
    }
    
    public void OnAlchemyCrafted()
    {
        _alchemyCount++;
        CheckAndUnlockTitles();
    }
    
    public void OnAreaVisited(string areaId)
    {
        if (_visitedAreas.Add(areaId))
        {
            _areasVisited++;
            CheckAndUnlockTitles();
        }
    }
    
    public void OnItemCollected(string itemId)
    {
        if (_collectedItems.Add(itemId))
        {
            _itemsCollected++;
            CheckAndUnlockTitles();
        }
    }
    
    public void OnDungeonCompleted()
    {
        _dungeonsCompleted++;
        CheckAndUnlockTitles();
    }
    
    public void OnPlayerHelped()
    {
        _playersHelped++;
        CheckAndUnlockTitles();
    }
    
    public void OnItemSold()
    {
        _itemsSold++;
        CheckAndUnlockTitles();
    }
    
    public void OnFriendAdded()
    {
        _friendsAdded++;
        CheckAndUnlockTitles();
    }
    
    public void OnTreasurePointFound()
    {
        _treasurePointsFound++;
        CheckAndUnlockTitles();
    }
    
    public void OnGoldChanged(int newGold)
    {
        if (newGold >= 1000000)
        {
            CheckTitle("millionaire", true);
        }
    }
    
    public void OnLevelUp(int newLevel)
    {
        // 假设满级是100
        if (newLevel >= 100)
        {
            CheckTitle("legend", true);
        }
    }
    
    // 获取玩家称号数据
    public PlayerTitleData GetTitleData(string titleId)
    {
        if (PlayerTitles.Titles.ContainsKey(titleId))
            return PlayerTitles.Titles[titleId];
        return null;
    }
    
    // 获取所有已解锁称号
    public List<TitleDefinition> GetUnlockedTitles()
    {
        List<TitleDefinition> result = new List<TitleDefinition>();
        foreach (var kvp in PlayerTitles.Titles)
        {
            if (kvp.Value.IsUnlocked)
            {
                var titleDef = TitleDatabase.Instance.GetTitle(kvp.Key);
                if (titleDef != null)
                    result.Add(titleDef);
            }
        }
        return result;
    }
    
    // 获取所有未解锁称号
    public List<TitleDefinition> GetLockedTitles()
    {
        List<TitleDefinition> result = new List<TitleDefinition>();
        foreach (var kvp in PlayerTitles.Titles)
        {
            if (!kvp.Value.IsUnlocked)
            {
                var titleDef = TitleDatabase.Instance.GetTitle(kvp.Key);
                if (titleDef != null)
                    result.Add(titleDef);
            }
        }
        return result;
    }
    
    // 获取当前激活称号
    public TitleDefinition GetActiveTitle()
    {
        if (PlayerTitles.ActiveTitleId != "")
            return TitleDatabase.Instance.GetTitle(PlayerTitles.ActiveTitleId);
        return null;
    }
    
    // 存档/读档
    public void SaveTitleData()
    {
        if (!IsInTree()) return;
        
        var saveSystem = GetTree().Root.GetNode<SaveSystem>("SaveSystem");
        if (saveSystem != null)
        {
            // 保存统计和称号数据
            var saveData = new Dictionary<string, object>();
            saveData["player_titles"] = PlayerTitles;
            saveData["title_stats"] = new Dictionary<string, int>
            {
                {"total_kills", _totalKills},
                {"boss_kills", _bossKills},
                {"max_combo", _maxCombo},
                {"total_crits", _totalCrits},
                {"total_damage_taken", _totalDamageTaken},
                {"fishing_count", _fishingCount},
                {"alchemy_count", _alchemyCount},
                {"areas_visited", _areasVisited},
                {"items_collected", _itemsCollected},
                {"dungeons_completed", _dungeonsCompleted},
                {"players_helped", _playersHelped},
                {"items_sold", _itemsSold},
                {"friends_added", _friendsAdded},
                {"treasure_points_found", _treasurePointsFound}
            };
            
            saveSystem.SaveCustomData("title_system", saveData);
        }
    }
    
    public void LoadTitleData()
    {
        var saveSystem = GetTree().Root.GetNode<SaveSystem>("SaveSystem");
        if (saveSystem != null)
        {
            var saveData = saveSystem.LoadCustomData("title_system");
            if (saveData != null)
            {
                if (saveData.ContainsKey("player_titles"))
                {
                    PlayerTitles = JsonUtils.Deserialize<PlayerTitleCollection>(JsonUtils.Serialize(saveData["player_titles"]));
                }
                
                if (saveData.ContainsKey("title_stats"))
                {
                    var stats = saveData["title_stats"] as Dictionary<string, object>;
                    if (stats != null)
                    {
                        _totalKills = stats.ContainsKey("total_kills") ? Convert.ToInt32(stats["total_kills"]) : 0;
                        _bossKills = stats.ContainsKey("boss_kills") ? Convert.ToInt32(stats["boss_kills"]) : 0;
                        _maxCombo = stats.ContainsKey("max_combo") ? Convert.ToInt32(stats["max_combo"]) : 0;
                        _totalCrits = stats.ContainsKey("total_crits") ? Convert.ToInt32(stats["total_crits"]) : 0;
                        _totalDamageTaken = stats.ContainsKey("total_damage_taken") ? Convert.ToInt32(stats["total_damage_taken"]) : 0;
                        _fishingCount = stats.ContainsKey("fishing_count") ? Convert.ToInt32(stats["fishing_count"]) : 0;
                        _alchemyCount = stats.ContainsKey("alchemy_count") ? Convert.ToInt32(stats["alchemy_count"]) : 0;
                        _areasVisited = stats.ContainsKey("areas_visited") ? Convert.ToInt32(stats["areas_visited"]) : 0;
                        _itemsCollected = stats.ContainsKey("items_collected") ? Convert.ToInt32(stats["items_collected"]) : 0;
                        _dungeonsCompleted = stats.ContainsKey("dungeons_completed") ? Convert.ToInt32(stats["dungeons_completed"]) : 0;
                        _playersHelped = stats.ContainsKey("players_helped") ? Convert.ToInt32(stats["players_helped"]) : 0;
                        _itemsSold = stats.ContainsKey("items_sold") ? Convert.ToInt32(stats["items_sold"]) : 0;
                        _friendsAdded = stats.ContainsKey("friends_added") ? Convert.ToInt32(stats["friends_added"]) : 0;
                        _treasurePointsFound = stats.ContainsKey("treasure_points_found") ? Convert.ToInt32(stats["treasure_points_found"]) : 0;
                    }
                }
            }
        }
        
        Initialize();
    }
}
