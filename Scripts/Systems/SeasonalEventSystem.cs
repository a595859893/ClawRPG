using Godot;
using System;
using System.Collections.Generic;

/// <summary>
/// 季节性事件数据类。存储玩家参与季节性活动的数据和进度。
/// </summary>
public class SeasonalEventData
{
    public enum EventType
    {
        DoubleGold,
        DoubleEXP,
        RareDropBoost,
        BossRush,
        PvPTournament,
        TreasureHunt,
        ElementalFestival,
        MonsterInvasion,
        LuckyWeek,
        Anniversary
    }

    public enum EventRarity
    {
        Common,
        Uncommon,
        Rare,
        Epic,
        Legendary
    }

    [System.Serializable]
    public class SeasonalEvent
    {
        public string EventId;
        public string EventName;
        public string Description;
        public EventType EventType;
        public EventRarity Rarity;
        public DateTime StartDate;
        public DateTime EndDate;
        public int RequiredLevel;
        public float GoldMultiplier;
        public float EXPMultiplier;
        public float DropRateMultiplier;
        public List<string> BonusItemIds;
        public List<string> RewardItemIds;
        public int EntryFee;
        public int MaxEntries;
        public bool IsActive;
    }

    [System.Serializable]
    public class PlayerEventData
    {
        public string EventId;
        public int Entries;
        public int BestScore;
        public bool ClaimedReward;
    }
}

public class SeasonalEventDatabase
{
    private static SeasonalEventData.SeasonalEvent[] _events = new SeasonalEventData.SeasonalEvent[]
    {
        // Double Gold Weekend
        new SeasonalEventData.SeasonalEvent
        {
            EventId = "double_gold_weekend",
            EventName = "双倍金币周末",
            Description = "周末期间所有金币获取翻倍！",
            EventType = SeasonalEventData.EventType.DoubleGold,
            EventRarity = SeasonalEventData.EventRarity.Common,
            StartDate = DateTime.MinValue,
            EndDate = DateTime.MaxValue,
            RequiredLevel = 1,
            GoldMultiplier = 2.0f,
            EXPMultiplier = 1.0f,
            DropRateMultiplier = 1.0f,
            BonusItemIds = new List<string>(),
            RewardItemIds = new List<string>(),
            EntryFee = 0,
            MaxEntries = -1,
            IsActive = true
        },
        // Double EXP Event
        new SeasonalEventData.SeasonalEvent
        {
            EventId = "double_exp_event",
            EventName = "双倍经验活动",
            Description = "活动期间所有经验获取翻倍！",
            EventType = SeasonalEventData.EventType.DoubleEXP,
            EventRarity = SeasonalEventData.EventRarity.Common,
            StartDate = DateTime.MinValue,
            EndDate = DateTime.MaxValue,
            RequiredLevel = 1,
            GoldMultiplier = 1.0f,
            EXPMultiplier = 2.0f,
            DropRateMultiplier = 1.0f,
            BonusItemIds = new List<string>(),
            RewardItemIds = new List<string>(),
            EntryFee = 0,
            MaxEntries = -1,
            IsActive = true
        },
        // Rare Drop Boost
        new SeasonalEventData.SeasonalEvent
        {
            EventId = "rare_drop_boost",
            EventName = "稀有掉落提升",
            Description = "稀有物品掉落率提升50%！",
            EventType = SeasonalEventData.EventType.RareDropBoost,
            EventRarity = SeasonalEventData.EventRarity.Uncommon,
            StartDate = DateTime.MinValue,
            EndDate = DateTime.MaxValue,
            RequiredLevel = 10,
            GoldMultiplier = 1.0f,
            EXPMultiplier = 1.0f,
            DropRateMultiplier = 1.5f,
            BonusItemIds = new List<string> { "rare_boost_potion" },
            RewardItemIds = new List<string>(),
            EntryFee = 0,
            MaxEntries = -1,
            IsActive = true
        },
        // Boss Rush Event
        new SeasonalEventData.SeasonalEvent
        {
            EventId = "boss_rush_event",
            EventName = "Boss rush挑战",
            Description = "连续击败Boss，挑战最高分！",
            EventType = SeasonalEventData.EventType.BossRush,
            EventRarity = SeasonalEventData.EventRarity.Rare,
            StartDate = DateTime.MinValue,
            EndDate = DateTime.MaxValue,
            RequiredLevel = 20,
            GoldMultiplier = 1.5f,
            EXPMultiplier = 1.5f,
            DropRateMultiplier = 2.0f,
            BonusItemIds = new List<string>(),
            RewardItemIds = new List<string> { "boss_trophy", "legendary_chest" },
            EntryFee = 1000,
            MaxEntries = 10,
            IsActive = true
        },
        // Treasure Hunt
        new SeasonalEventData.SeasonalEvent
        {
            EventId = "treasure_hunt",
            EventName = "宝藏猎人",
            Description = "寻找隐藏宝藏，获得稀有奖励！",
            EventType = SeasonalEventData.EventType.TreasureHunt,
            EventRarity = SeasonalEventData.EventRarity.Epic,
            StartDate = DateTime.MinValue,
            EndDate = DateTime.MaxValue,
            RequiredLevel = 15,
            GoldMultiplier = 2.0f,
            EXPMultiplier = 1.2f,
            DropRateMultiplier = 1.8f,
            BonusItemIds = new List<string> { "treasure_map" },
            RewardItemIds = new List<string> { "ancient_artifact", "mythril_ingot" },
            EntryFee = 500,
            MaxEntries = 5,
            IsActive = true
        },
        // Elemental Festival
        new SeasonalEventData.SeasonalEvent
        {
            EventId = "elemental_festival",
            EventName = "元素庆典",
            Description = "所有元素伤害提升30%！",
            EventType = SeasonalEventData.EventType.ElementalFestival,
            EventRarity = SeasonalEventData.EventRarity.Rare,
            StartDate = DateTime.MinValue,
            EndDate = DateTime.MaxValue,
            RequiredLevel = 25,
            GoldMultiplier = 1.0f,
            EXPMultiplier = 1.3f,
            DropRateMultiplier = 1.0f,
            BonusItemIds = new List<string> { "elemental_essence" },
            RewardItemIds = new List<string> { "elemental_orb" },
            EntryFee = 0,
            MaxEntries = -1,
            IsActive = true
        },
        // Monster Invasion
        new SeasonalEventData.SeasonalEvent
        {
            EventId = "monster_invasion",
            EventName = "怪物入侵",
            Description = "抵御入侵怪物，保护村庄！",
            EventType = SeasonalEventData.EventType.MonsterInvasion,
            EventRarity = SeasonalEventData.EventRarity.Epic,
            StartDate = DateTime.MinValue,
            EndDate = DateTime.MaxValue,
            RequiredLevel = 30,
            GoldMultiplier = 2.5f,
            EXPMultiplier = 2.0f,
            DropRateMultiplier = 2.0f,
            BonusItemIds = new List<string>(),
            RewardItemIds = new List<string> { "hero_medal", "defender_shield" },
            EntryFee = 0,
            MaxEntries = 3,
            IsActive = true
        },
        // Lucky Week
        new SeasonalEventData.SeasonalEvent
        {
            EventId = "lucky_week",
            EventName = "幸运周",
            Description = "暴击率提升20%，幸运掉落翻倍！",
            EventType = SeasonalEventData.EventType.LuckyWeek,
            EventRarity = SeasonalEventData.EventRarity.Rare,
            StartDate = DateTime.MinValue,
            EndDate = DateTime.MaxValue,
            RequiredLevel = 5,
            GoldMultiplier = 1.5f,
            EXPMultiplier = 1.5f,
            DropRateMultiplier = 2.0f,
            BonusItemIds = new List<string> { "lucky_charm" },
            RewardItemIds = new List<string>(),
            EntryFee = 0,
            MaxEntries = -1,
            IsActive = true
        },
        // Anniversary Event
        new SeasonalEventData.SeasonalEvent
        {
            EventId = "anniversary_event",
            EventName = "周年庆典",
            Description = "庆祝游戏周年，全服狂欢！",
            EventType = SeasonalEventData.EventType.Anniversary,
            EventRarity = SeasonalEventData.EventRarity.Legendary,
            StartDate = DateTime.MinValue,
            EndDate = DateTime.MaxValue,
            RequiredLevel = 1,
            GoldMultiplier = 3.0f,
            EXPMultiplier = 3.0f,
            DropRateMultiplier = 3.0f,
            BonusItemIds = new List<string> { "anniversary_badge" },
            RewardItemIds = new List<string> { "anniversary_mount", "legendary_weapon", "ancient_relic" },
            EntryFee = 0,
            MaxEntries = 1,
            IsActive = true
        }
    };

    public static SeasonalEventData.SeasonalEvent[] GetAllEvents() => _events;

    public static SeasonalEventData.SeasonalEvent GetEvent(string eventId)
    {
        foreach (var evt in _events)
        {
            if (evt.EventId == eventId)
                return evt;
        }
        return null;
    }

    public static SeasonalEventData.SeasonalEvent[] GetActiveEvents()
    {
        List<SeasonalEventData.SeasonalEvent> activeEvents = new List<SeasonalEventData.SeasonalEvent>();
        DateTime now = DateTime.Now;

        foreach (var evt in _events)
        {
            if (evt.IsActive && now >= evt.StartDate && now <= evt.EndDate)
            {
                activeEvents.Add(evt);
            }
        }

        return activeEvents.ToArray();
    }

    public static SeasonalEventData.SeasonalEvent[] GetEventsByRarity(SeasonalEventData.EventRarity rarity)
    {
        List<SeasonalEventData.SeasonalEvent> result = new List<SeasonalEventData.SeasonalEvent>();
        foreach (var evt in _events)
        {
            if (evt.EventRarity == rarity)
                result.Add(evt);
        }
        return result.ToArray();
    }
}

public partial class SeasonalEventSystem : BaseSystem
{
    /// <summary>
    /// 获取系统单例实例。
    /// </summary>
    private static SeasonalEventSystem _instance;

    /// <summary>
    /// 获取单例实例。
    /// </summary>
    public static SeasonalEventSystem Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = GetNode<SeasonalEventSystem>("/root/SeasonalEventSystem");
                if (_instance == null)
                {
                    var node = new SeasonalEventSystem();
                    node.Name = "SeasonalEventSystem";
                    Engine.GetMainLoop().Root.AddChild(node);
                }
            }
            return _instance;
        }
        private set => _instance = value;
    }

    private Dictionary<string, SeasonalEventData.PlayerEventData> _playerEventData = new Dictionary<string, SeasonalEventData.PlayerEventData>();
    private Dictionary<string, int> _eventEntries = new Dictionary<string, int>();

    // Signals
    public Action<string> EventStarted;
    public Action<string> EventEnded;
    public Action<string, int> EventRewardClaimed;
    public Action<string, int> EventEntryRecorded;

    protected override void Initialize()
    {
        base.Initialize();
        
        Instance = this;
        
        // 注册到保存系统
        SaveSystem.Instance?.Register(this);
        
        GD.Print("[SeasonalEventSystem] Initialized");
    }

    public SeasonalEventData.SeasonalEvent[] GetActiveEvents()
    {
        return SeasonalEventDatabase.GetActiveEvents();
    }

    public SeasonalEventData.SeasonalEvent GetEvent(string eventId)
    {
        return SeasonalEventDatabase.GetEvent(eventId);
    }

    public bool CanParticipate(string eventId)
    {
        var evt = GetEvent(eventId);
        if (evt == null)
            return false;

        // Check if player meets level requirement
        var player = GetPlayer();
        if (player != null)
        {
            int playerLevel = (int)player.Get("level", 1);
            if (playerLevel < evt.RequiredLevel)
                return false;
        }

        // Check if event is active
        if (!evt.IsActive)
            return false;

        DateTime now = DateTime.Now;
        if (now < evt.StartDate || now > evt.EndDate)
            return false;

        // Check entry limit
        if (evt.MaxEntries > 0)
        {
            int entries = GetEventEntries(eventId);
            if (entries >= evt.MaxEntries)
                return false;
        }

        return true;
    }

    public bool Participate(string eventId)
    {
        var evt = GetEvent(eventId);
        if (evt == null)
        {
            GD.PrintErr($"[SeasonalEventSystem] Event not found: {eventId}");
            return false;
        }

        if (!CanParticipate(eventId))
        {
            GD.Print($"[SeasonalEventSystem] Cannot participate in event: {eventId}");
            return false;
        }

        // Check entry fee
        if (evt.EntryFee > 0)
        {
            var player = GetPlayer();
            if (player == null) return false;
            
            int playerGold = (int)player.Get("gold", 0);
            if (playerGold < evt.EntryFee)
            {
                GD.Print($"[SeasonalEventSystem] Not enough gold for event: {eventId}");
                return false;
            }
            player.Set("gold", playerGold - evt.EntryFee);
        }

        // Record entry
        if (!_eventEntries.ContainsKey(eventId))
            _eventEntries[eventId] = 0;
        _eventEntries[eventId]++;

        EventEntryRecorded?.Invoke(eventId, _eventEntries[eventId]);
        GD.Print($"[SeasonalEventSystem] Participated in event: {eventId}, entries: {_eventEntries[eventId]}");
        return true;
    }

    public int GetEventEntries(string eventId)
    {
        return _eventEntries.ContainsKey(eventId) ? _eventEntries[eventId] : 0;
    }

    public float GetGoldMultiplier()
    {
        float multiplier = 1.0f;
        var activeEvents = GetActiveEvents();
        foreach (var evt in activeEvents)
        {
            if (evt.GoldMultiplier > multiplier)
                multiplier = evt.GoldMultiplier;
        }
        return multiplier;
    }

    public float GetEXPMultiplier()
    {
        float multiplier = 1.0f;
        var activeEvents = GetActiveEvents();
        foreach (var evt in activeEvents)
        {
            if (evt.EXPMultiplier > multiplier)
                multiplier = evt.EXPMultiplier;
        }
        return multiplier;
    }

    public float GetDropRateMultiplier()
    {
        float multiplier = 1.0f;
        var activeEvents = GetActiveEvents();
        foreach (var evt in activeEvents)
        {
            if (evt.DropRateMultiplier > multiplier)
                multiplier = evt.DropRateMultiplier;
        }
        return multiplier;
    }

    public bool HasBonusItem(string itemId)
    {
        var activeEvents = GetActiveEvents();
        foreach (var evt in activeEvents)
        {
            if (evt.BonusItemIds.Contains(itemId))
                return true;
        }
        return false;
    }
    
    /// <summary>
    /// 获取玩家节点
    /// </summary>
    private Node GetPlayer()
    {
        var tree = Engine.GetMainLoop();
        if (tree is SceneTree sceneTree) {
            var nodes = sceneTree.GetNodesInGroup("player");
            if (nodes.Count > 0) return nodes[0];
        }
        return null;
    }
    
    /// <summary>
    /// 导出保存数据
    /// </summary>
    public override Dictionary<string, object> ExportSaveData()
    {
        var data = new Dictionary<string, object>();
        
        var entriesData = new Dictionary<string, int>();
        foreach (var kvp in _eventEntries)
        {
            entriesData[kvp.Key] = kvp.Value;
        }
        data["event_entries"] = entriesData;
        
        return data;
    }
    
    /// <summary>
    /// 导入保存数据
    /// </summary>
    public override void ImportSaveData(Dictionary<string, object> data)
    {
        if (data == null) return;

        if (data.Contains("event_entries"))
        {
            var entries = data["event_entries"] as Dictionary;
            if (entries != null)
            {
                _eventEntries.Clear();
                foreach (var kvp in entries)
                {
                    _eventEntries[kvp.Key.ToString()] = Convert.ToInt32(kvp.Value);
                }
            }
        }
        
        GD.Print("[SeasonalEventSystem] Data loaded");
    }
    
    /// <summary>
    /// 获取系统ID
    /// </summary>
    public override string GetId()
    {
        return "SeasonalEventSystem";
    }
}
