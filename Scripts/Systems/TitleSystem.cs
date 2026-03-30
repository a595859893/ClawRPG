using Godot;
using System;
using System.Collections.Generic;

namespace ClawRPG.Scripts.Systems
{
    /// <summary>
    /// 称号系统 - 管理玩家称号的解锁、装备和展示
    /// </summary>
    public class TitleData : Resource
    {
    public string TitleId { get; set; }
    public string TitleName { get; set; }
    public string Description { get; set; }
    public TitleCategory Category { get; set; }
    public TitleRarity Rarity { get; set; }
    public int RequiredValue { get; set; }
    public string IconPath { get; set; }
    public bool IsUnlocked { get; set; }
    public DateTime UnlockTime { get; set; }

    public TitleData()
    {
        TitleId = "";
        TitleName = "";
        Description = "";
        Category = TitleCategory.Combat;
        Rarity = TitleRarity.Common;
        RequiredValue = 0;
        IconPath = "";
        IsUnlocked = false;
        UnlockTime = DateTime.MinValue;
    }
}

public enum TitleCategory
{
    Combat,      // 战斗相关
    Exploration, // 探索相关
    Collection,  // 收藏相关
    Social,      // 社交相关
    Economy,     // 经济相关
    Special,     // 特殊成就
    Seasonal     // 季节活动
}

public enum TitleRarity
{
    Common,      // 普通
    Uncommon,    // 优秀
    Rare,        // 稀有
    Epic,        // 史诗
    Legendary    // 传说
}

/// <summary>
/// Title system - manages player titles, tracks progress and handles unlocking
/// </summary>
/// <summary>
/// TitleSystem - 玩家称号系统，管理称号的解锁、条件检查和装备
/// </summary>
public class TitleSystem : BaseSystem
{
    // Singleton
    private static TitleSystem _instance;
    public static TitleSystem Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = new TitleSystem();
            }
            return _instance;
        }
    }

    // Title Database
    private Dictionary<string, TitleData> _titleDatabase = new Dictionary<string, TitleData>();
    
    // Player's unlocked titles
    private HashSet<string> _unlockedTitles = new HashSet<string>();
    
    // Currently equipped title
    private string _equippedTitle = "";
    
    // Title statistics
    private int _totalTitlesUnlocked = 0;
    private int _titlesByRarity => GetTitlesByRarityCount();
    
    // Signals (Godot 4 compatible)
    [Signal]
    public delegate void TitleUnlockedDelegate(string playerId, TitleData data);
    [Signal]
    public delegate void TitleEquippedDelegate(string playerId);
    [Signal]
    public delegate void TitleProgressUpdatedDelegate(string playerId, int current, int total);

    public override void _Ready()
    {
        _instance = this;
        InitializeTitleDatabase();
    }

    private void InitializeTitleDatabase()
    {
        // Combat Titles - based on kill counts
        AddTitle("killer_novice", "Novice Killer", "Defeat 100 enemies", TitleCategory.Combat, TitleRarity.Common, 100);
        AddTitle("killer_expert", "Expert Killer", "Defeat 500 enemies", TitleCategory.Combat, TitleRarity.Uncommon, 500);
        AddTitle("killer_master", "Master Killer", "Defeat 1000 enemies", TitleCategory.Combat, TitleRarity.Rare, 1000);
        AddTitle("killer_legend", "Legendary Killer", "Defeat 5000 enemies", TitleCategory.Combat, TitleRarity.Epic, 5000);
        AddTitle("killer_god", "God of Death", "Defeat 10000 enemies", TitleCategory.Combat, TitleRarity.Legendary, 10000);
        
        // Boss Titles
        AddTitle("boss_slayer_novice", "Novice Boss Slayer", "Defeat 10 bosses", TitleCategory.Combat, TitleRarity.Common, 10);
        AddTitle("boss_slayer_expert", "Expert Boss Slayer", "Defeat 50 bosses", TitleCategory.Combat, TitleRarity.Rare, 50);
        AddTitle("boss_slayer_legend", "Legendary Boss Slayer", "Defeat 100 bosses", TitleCategory.Combat, TitleRarity.Legendary, 100);
        
        // Gold Titles
        AddTitle("rich_novice", "Novice Rich", "Accumulate 10000 gold", TitleCategory.Economy, TitleRarity.Common, 10000);
        AddTitle("rich_merchant", "Merchant Prince", "Accumulate 100000 gold", TitleCategory.Economy, TitleRarity.Uncommon, 100000);
        AddTitle("rich_king", "Gold King", "Accumulate 1000000 gold", TitleCategory.Economy, TitleRarity.Epic, 1000000);
        AddTitle("rich_god", "Wealth God", "Accumulate 10000000 gold", TitleCategory.Economy, TitleRarity.Legendary, 10000000);
        
        // Level Titles
        AddTitle("level_10", "Adventurer", "Reach level 10", TitleCategory.Combat, TitleRarity.Common, 10);
        AddTitle("level_25", "Veteran Adventurer", "Reach level 25", TitleCategory.Combat, TitleRarity.Uncommon, 25);
        AddTitle("level_50", "Elite Adventurer", "Reach level 50", TitleCategory.Combat, TitleRarity.Rare, 50);
        AddTitle("level_75", "Master Adventurer", "Reach level 75", TitleCategory.Combat, TitleRarity.Epic, 75);
        AddTitle("level_100", "Legendary Hero", "Reach level 100", TitleCategory.Combat, TitleRarity.Legendary, 100);
        
        // Exploration Titles
        AddTitle("explorer_novice", "Novice Explorer", "Discover 10 locations", TitleCategory.Exploration, TitleRarity.Common, 10);
        AddTitle("explorer_expert", "Expert Explorer", "Discover 50 locations", TitleCategory.Exploration, TitleRarity.Uncommon, 50);
        AddTitle("explorer_master", "Master Explorer", "Discover 100 locations", TitleCategory.Exploration, TitleRarity.Rare, 100);
        AddTitle("explorer_legend", "Legendary Explorer", "Discover all locations", TitleCategory.Exploration, TitleRarity.Legendary, 200);
        
        // Pet Titles
        AddTitle("pet_collector_novice", "Pet Collector", "Own 5 different pets", TitleCategory.Collection, TitleRarity.Common, 5);
        AddTitle("pet_collector_expert", "Pet Master", "Own 15 different pets", TitleCategory.Collection, TitleRarity.Uncommon, 15);
        AddTitle("pet_collector_legend", "Legendary Pet Master", "Own 30 different pets", TitleCategory.Collection, TitleRarity.Epic, 30);
        
        // Mount Titles
        AddTitle("mount_rider_novice", "Mount Rider", "Own 3 mounts", TitleCategory.Collection, TitleRarity.Common, 3);
        AddTitle("mount_rider_expert", "Mount Master", "Own 10 mounts", TitleCategory.Collection, TitleRarity.Uncommon, 10);
        
        // Guild Titles
        AddTitle("guild_founder", "Guild Founder", "Create a guild", TitleCategory.Social, TitleRarity.Rare, 1);
        AddTitle("guild_leader", "Guild Leader", "Lead a guild with 10 members", TitleCategory.Social, TitleRarity.Epic, 10);
        
        // PvP Titles
        AddTitle("pvp_novice", "Novice Warrior", "Win 10 PvP battles", TitleCategory.Combat, TitleRarity.Common, 10);
        AddTitle("pvp_expert", "Expert Warrior", "Win 50 PvP battles", TitleCategory.Combat, TitleRarity.Rare, 50);
        AddTitle("pvp_legend", "Legendary Warrior", "Win 100 PvP battles", TitleCategory.Combat, TitleRarity.Epic, 100);
        
        // Crafting Titles
        AddTitle("crafter_novice", "Novice Crafter", "Craft 50 items", TitleCategory.Economy, TitleRarity.Common, 50);
        AddTitle("crafter_expert", "Expert Crafter", "Craft 200 items", TitleCategory.Economy, TitleRarity.Uncommon, 200);
        AddTitle("crafter_master", "Master Crafter", "Craft 500 items", TitleCategory.Economy, TitleRarity.Rare, 500);
        AddTitle("crafter_legend", "Legendary Crafter", "Craft 1000 items", TitleCategory.Economy, TitleRarity.Epic, 1000);
        
        // Special Titles
        AddTitle("first_blood", "First Blood", "Win your first battle", TitleCategory.Special, TitleRarity.Common, 1);
        AddTitle("survivor", "Survivor", "Survive 100 battles", TitleCategory.Special, TitleRarity.Rare, 100);
        AddTitle("dedicated", "Dedicated Player", "Play for 100 hours", TitleCategory.Special, TitleRarity.Epic, 100);
        AddTitle("veteran", "Veteran", "Play for 500 hours", TitleCategory.Special, TitleRarity.Legendary, 500);
        
        // Achievement Titles
        AddTitle("achiever_novice", "Novice Achiever", "Unlock 10 achievements", TitleCategory.Special, TitleRarity.Common, 10);
        AddTitle("achiever_expert", "Expert Achiever", "Unlock 25 achievements", TitleCategory.Special, TitleRarity.Uncommon, 25);
        AddTitle("achiever_master", "Master Achiever", "Unlock 50 achievements", TitleCategory.Special, TitleRarity.Rare, 50);
        AddTitle("achiever_legend", "Legendary Achiever", "Unlock all achievements", TitleCategory.Special, TitleRarity.Legendary, 100);
        
        // Seasonal Titles (placeholder for events)
        AddTitle("season_champion", "Season Champion", "Win a seasonal event", TitleCategory.Seasonal, TitleRarity.Epic, 1);
        AddTitle("season_legend", "Season Legend", "Win 3 seasonal events", TitleCategory.Seasonal, TitleRarity.Legendary, 3);
        
        GD.Print($"[TitleSystem] Initialized {_titleDatabase.Count} titles");
    }

    private void AddTitle(string id, string name, string desc, TitleCategory cat, TitleRarity rarity, int required)
    {
        var title = new TitleData
        {
            TitleId = id,
            TitleName = name,
            Description = desc,
            Category = cat,
            Rarity = rarity,
            RequiredValue = required,
            IconPath = ""
        };
        _titleDatabase[id] = title;
    }

    // Check if player meets requirements for a title

    /// <summary>
    /// 检查玩家是否满足称号要求
    /// </summary>
    /// <param name="titleId">称号ID</param>
    /// <param name="killCount">击杀数</param>
    /// <param name="bossKills">Boss击杀数</param>
    /// <param name="gold">金币数</param>
    /// <param name="level">等级</param>
    /// <param name="locations">地点数</param>
    /// <param name="pets">宠物数</param>
    /// <param name="mounts">坐骑数</param>
    /// <param name="guildMembers">公会成员数</param>
    /// <param name="pvpWins">PVP胜场数</param>
    /// <param name="crafted">制造数</param>
    /// <param name="battles">战斗数</param>
    /// <param name="hoursPlayed">游玩小时数</param>
    /// <param name="achievements">成就数</param>
    /// <param name="seasonalWins">季节赛胜利数</param>
    /// <returns>是否满足要求</returns>
    public bool CheckTitleRequirements(string titleId, int killCount = 0, int bossKills = 0, 
        int gold = 0, int level = 0, int locations = 0, int pets = 0, int mounts = 0,
        int guildMembers = 0, int pvpWins = 0, int crafted = 0, int battles = 0, 
        int hoursPlayed = 0, int achievements = 0, int seasonalWins = 0)
    {
        if (!_titleDatabase.ContainsKey(titleId))
            return false;

        var title = _titleDatabase[titleId];
        int currentValue = 0;

        switch (titleId)
        {
            case "killer_novice":
            case "killer_expert":
            case "killer_master":
            case "killer_legend":
            case "killer_god":
                currentValue = killCount;
                break;
            case "boss_slayer_novice":
            case "boss_slayer_expert":
            case "boss_slayer_legend":
                currentValue = bossKills;
                break;
            case "rich_novice":
            case "rich_merchant":
            case "rich_king":
            case "rich_god":
                currentValue = gold;
                break;
            case "level_10":
            case "level_25":
            case "level_50":
            case "level_75":
            case "level_100":
                currentValue = level;
                break;
            case "explorer_novice":
            case "explorer_expert":
            case "explorer_master":
            case "explorer_legend":
                currentValue = locations;
                break;
            case "pet_collector_novice":
            case "pet_collector_expert":
            case "pet_collector_legend":
                currentValue = pets;
                break;
            case "mount_rider_novice":
            case "mount_rider_expert":
                currentValue = mounts;
                break;
            case "guild_founder":
            case "guild_leader":
                currentValue = guildMembers > 0 ? 1 : 0;
                if (titleId == "guild_leader") currentValue = guildMembers;
                break;
            case "pvp_novice":
            case "pvp_expert":
            case "pvp_legend":
                currentValue = pvpWins;
                break;
            case "crafter_novice":
            case "crafter_expert":
            case "crafter_master":
            case "crafter_legend":
                currentValue = crafted;
                break;
            case "first_blood":
            case "survivor":
                currentValue = battles;
                break;
            case "dedicated":
            case "veteran":
                currentValue = hoursPlayed;
                break;
            case "achiever_novice":
            case "achiever_expert":
            case "achiever_master":
            case "achiever_legend":
                currentValue = achievements;
                break;
            case "season_champion":
            case "season_legend":
                currentValue = seasonalWins;
                break;
        }

        // Update progress
        TitleProgressUpdated?.Call(titleId, currentValue, title.RequiredValue);

        // Check if requirement is met
        return currentValue >= title.RequiredValue;
    }

    // Unlock a title

    /// <summary>
    /// 解锁称号
    /// </summary>
    /// <param name="titleId">称号ID</param>
    public void UnlockTitle(string titleId)
    {
        if (!_titleDatabase.ContainsKey(titleId))
            return;

        if (_unlockedTitles.Contains(titleId))
            return;

        var title = _titleDatabase[titleId];
        title.IsUnlocked = true;
        title.UnlockTime = DateTime.Now;
        _unlockedTitles.Add(titleId);
        _totalTitlesUnlocked++;

        GD.Print($"[TitleSystem] Title unlocked: {title.TitleName}");
        TitleUnlocked?.Call(titleId, title);
    }

    // Equip a title

    /// <summary>
    /// 装备称号
    /// </summary>
    /// <param name="titleId">称号ID</param>
    public void EquipTitle(string titleId)
    {
        if (!_unlockedTitles.Contains(titleId))
        {
            GD.Print($"[TitleSystem] Cannot equip locked title: {titleId}");
            return;
        }

        _equippedTitle = titleId;
        GD.Print($"[TitleSystem] Title equipped: {_titleDatabase[titleId].TitleName}");
        TitleEquipped?.Call(titleId);
    }

    // Unequip current title

    /// <summary>
    /// 卸下称号
    /// </summary>
    public void UnequipTitle()
    {
        _equippedTitle = "";
        GD.Print("[TitleSystem] Title unequipped");
    }

    // Get all titles in a category

    /// <summary>
    /// 获取指定分类的称号
    /// </summary>
    /// <param name="category">称号分类</param>
    /// <returns>称号列表</returns>
    public List<TitleData> GetTitlesByCategory(TitleCategory category)
    {
        var result = new List<TitleData>();
        foreach (var title in _titleDatabase.Values)
        {
            if (title.Category == category)
                result.Add(title);
        }
        return result;
    }

    // Get all unlocked titles

    /// <summary>
    /// 获取已解锁称号列表
    /// </summary>
    /// <returns>已解锁称号列表</returns>
    public List<TitleData> GetUnlockedTitles()
    {
        var result = new List<TitleData>();
        foreach (var titleId in _unlockedTitles)
        {
            if (_titleDatabase.ContainsKey(titleId))
                result.Add(_titleDatabase[titleId]);
        }
        return result;
    }

    // Get equipped title

    /// <summary>
    /// 获取已装备称号ID
    /// </summary>
    /// <returns>装备的称号ID</returns>
    public string GetEquippedTitle()
    {
        return _equippedTitle;
    }

    // Get equipped title name

    /// <summary>
    /// 获取已装备称号名称
    /// </summary>
    /// <returns>装备的称号名称</returns>
    public string GetEquippedTitleName()
    {
        if (string.IsNullOrEmpty(_equippedTitle) || !_titleDatabase.ContainsKey(_equippedTitle))
            return "";
        return _titleDatabase[_equippedTitle].TitleName;
    }

    // Check if title is unlocked

    /// <summary>
    /// 检查称号是否已解锁
    /// </summary>
    /// <param name="titleId">称号ID</param>
    /// <returns>是否已解锁</returns>
    public bool IsTitleUnlocked(string titleId)
    {
        return _unlockedTitles.Contains(titleId);
    }

    // Get total unlocked count

    /// <summary>
    /// 获取已解锁称号总数
    /// </summary>
    /// <returns>已解锁数量</returns>
    public int GetTotalUnlockedCount()
    {
        return _unlockedTitles.Count;
    }

    // Get titles by rarity count
    private int GetTitlesByRarityCount()
    {
        int count = 0;
        foreach (var titleId in _unlockedTitles)
        {
            if (_titleDatabase.ContainsKey(titleId))
            {
                var title = _titleDatabase[titleId];
                if (title.Rarity == TitleRarity.Legendary)
                    count++;
            }
        }
        return count;
    }

    // Get title data

    /// <summary>
    /// 获取称号数据
    /// </summary>
    /// <param name="titleId">称号ID</param>
    /// <returns>称号数据</returns>
    public TitleData GetTitleData(string titleId)
    {
        if (_titleDatabase.ContainsKey(titleId))
            return _titleDatabase[titleId];
        return null;
    }

    // Get all titles

    /// <summary>
    /// 获取所有称号
    /// </summary>
    /// <returns>所有称号列表</returns>
    public List<TitleData> GetAllTitles()
    {
        return new List<TitleData>(_titleDatabase.Values);
    }

    // Save title data
    public Dictionary<string, bool> SaveTitleData()
    {
        var saveData = new Dictionary<string, bool>();
        foreach (var titleId in _unlockedTitles)
        {
            saveData[titleId] = true;
        }
        return saveData;
    }

    // Load title data
    public void LoadTitleData(Dictionary<string, bool> data)
    {
        if (data == null) return;
        
        _unlockedTitles.Clear();
        foreach (var kvp in data)
        {
            if (kvp.Value && _titleDatabase.ContainsKey(kvp.Key))
            {
                _unlockedTitles.Add(kvp.Key);
                _titleDatabase[kvp.Key].IsUnlocked = true;
            }
        }
        _totalTitlesUnlocked = _unlockedTitles.Count;
    }

    // Save equipped title
    public string SaveEquippedTitle()
    {
        return _equippedTitle;
    }

    // Load equipped title
    public void LoadEquippedTitle(string titleId)
    {
        if (!string.IsNullOrEmpty(titleId) && _unlockedTitles.Contains(titleId))
        {
            _equippedTitle = titleId;
        }
    }
    
    // ===== 持久化 =====
    public override Dictionary<string, object> ExportSaveData()
    {
        var data = new Dictionary<string, object>();
        
        // 保存已解锁称号
        data["unlocked_titles"] = new Array(_unlockedTitles);
        
        // 保存已装备称号
        data["equipped_title"] = _equippedTitle;
        
        // 保存统计数据
        data["total_titles_unlocked"] = _totalTitlesUnlocked;
        
        return data;
    }
    
    public override void ImportSaveData(Dictionary<string, object> data)
    {
        if (data == null) return;
        
        // 恢复已解锁称号
        if (data.ContainsKey("unlocked_titles"))
        {
            _unlockedTitles.Clear();
            var unlockedList = (Array)data["unlocked_titles"];
            foreach (string titleId in unlockedList)
            {
                _unlockedTitles.Add(titleId);
                if (_titleDatabase.ContainsKey(titleId))
                {
                    _titleDatabase[titleId].IsUnlocked = true;
                }
            }
        }
        
        // 恢复已装备称号
        if (data.ContainsKey("equipped_title"))
        {
            string titleId = data["equipped_title"].ToString();
            if (!string.IsNullOrEmpty(titleId) && _unlockedTitles.Contains(titleId))
            {
                _equippedTitle = titleId;
            }
        }
        
        // 恢复统计数据
        if (data.ContainsKey("total_titles_unlocked"))
            _totalTitlesUnlocked = Convert.ToInt32(data["total_titles_unlocked"]);
    }
}
}
