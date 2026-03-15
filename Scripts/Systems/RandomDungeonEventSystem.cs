using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// 随机地牢事件系统。管理地牢中的随机事件触发，包括战斗、宝藏、祝福、诅咒等多种类型的事件。
/// 支持事件稀有度权重、玩家状态影响和事件结果计算。
/// </summary>
public partial class RandomDungeonEventData : BaseSystem
{
    // Event tracking
    public Dictionary<string, int> EventHistory { get; set; } = new Dictionary<string, int>();
    public Dictionary<string, int> EventTriggersToday { get; set; } = new Dictionary<string, int>();
    
    // Active event states
    public Dictionary<string, bool> ActiveEventEffects { get; set; } = new Dictionary<string, bool>();
    public List<string> AppliedBuffs { get; set; } = new List<string>();
    public List<string> AppliedDebuffs { get; set; } = new List<string>();
    
    // Player state for events
    public int PlayerFloor { get; set; } = 1;
    public int PlayerHealth { get; set; } = 100;
    public int PlayerGold { get; set; } = 0;
    public int EnemiesDefeatedInRoom { get; set; } = 0;
    public int RoomsExplored { get; set; } = 0;
    public bool HasKey { get; set; } = false;
    public bool HasTreasure { get; set; } = false;
    public bool IsInjured { get; set; } = false;
    public bool IsFullHealth { get; set; } = true;
    public bool HasPet { get; set; } = false;
    
    // Event system stats
    public int TotalEventsTriggered { get; set; } = 0;
    public int PositiveEvents { get; set; } = 0;
    public int NegativeEvents { get; set; } = 0;
    public int NeutralEvents { get; set; } = 0;
    public int GoldGainedFromEvents { get; set; } = 0;
    public int GoldLostFromEvents { get; set; } = 0;
    public int ExpGainedFromEvents { get; set; } = 0;
    public int ItemsGained { get; set; } = 0;
    public int ItemsLost { get; set; } = 0;
    
    // Daily reset tracking
    public string LastEventDate { get; set; } = "";

    /// <summary>
    /// Export save data for persistence
    /// </summary>
    public override Dictionary ExportSaveData()
    {
        return new Dictionary();
    }

    /// <summary>
    /// Import save data from persistence
    /// </summary>
    public override void ImportSaveData(Dictionary data)
    {
        // No persistent data needed
    }
}

public enum DungeonEventCategory
{
    Combat,
    Treasure,
    Mystery,
    Hazard,
    Blessing,
    Curse,
    NPC,
    Exploration,
    Trap,
    Reward

    /// <summary>
    /// Export save data for persistence
    /// </summary>
    public override Dictionary ExportSaveData()
    {
        return new Dictionary();
    }

    /// <summary>
    /// Import save data from persistence
    /// </summary>
    public override void ImportSaveData(Dictionary data)
    {
        // No persistent data needed
    }
}

public enum DungeonEventRarity
{
    Common,
    Uncommon,
    Rare,
    Epic,
    Legendary

    /// <summary>
    /// Export save data for persistence
    /// </summary>
    public override Dictionary ExportSaveData()
    {
        return new Dictionary();
    }

    /// <summary>
    /// Import save data from persistence
    /// </summary>
    public override void ImportSaveData(Dictionary data)
    {
        // No persistent data needed
    }
}

public enum DungeonEventOutcome
{
    Success,
    Failure,
    Mixed,
    Nothing

    /// <summary>
    /// Export save data for persistence
    /// </summary>
    public override Dictionary ExportSaveData()
    {
        return new Dictionary();
    }

    /// <summary>
    /// Import save data from persistence
    /// </summary>
    public override void ImportSaveData(Dictionary data)
    {
        // No persistent data needed
    }
}

[GlobalClass]
/// <summary>
/// 随机地牢事件系统核心类。负责事件数据库初始化、事件触发、事件处理和结果计算。
/// </summary>
public partial class RandomDungeonEventSystem : BaseSystem
{
    private RandomDungeonEventData _data;
    private DungeonEventDatabase _database;
    private DungeonEventGenerator _generator;
    private DungeonEventEffects _effects;
    private Random _rand = new Random();
    
    public override void _Ready()
    {
        InitializeEventSystem();
    }
    
    private void InitializeEventSystem()
    {
        _data = GetData();
        _database = new DungeonEventDatabase();
        _generator = new DungeonEventGenerator(_database, _data);
        _effects = new DungeonEventEffects(_data);
    }
    
    public void SetData(RandomDungeonEventData data)
    {
        _data = data;
        _generator?.SetData(data);
        _effects?.SetData(data);
    }
    
    public RandomDungeonEventData GetData()
    {
        if (_data == null)
        {
            _data = new RandomDungeonEventData();
        }
        return _data;
    }
    
    // Main event triggering function
    public Dictionary<string, object> TriggerRandomEvent()
    {
        var result = _generator.GenerateRandomEvent();
        
        if (!(bool)(result.Get("success") ?? false) && !result.ContainsKey("event_data"))
            return result;
        
        // Process event effects
        var eventData = result["event_data"] as Dictionary;
        if (eventData != null)
        {
            var processedResult = ProcessEventEffects(eventData);
            foreach (var kvp in processedResult)
            {
                result[kvp.Key] = kvp.Value;
            }
            result.Remove("event_data");
        }
        
        return result;
    }
    
    private Dictionary<string, object> ProcessEventEffects(Dictionary eventData)
    {
        var result = new Dictionary<string, object>();
        string eventType = eventData["type"].ToString();
        
        // Process based on event type
        switch (eventType)
        {
            case "combat":
                result = _effects.ProcessCombatEvent(eventData);
                break;
            case "treasure":
                result = _effects.ProcessTreasureEvent(eventData);
                break;
            case "heal":
                result = _effects.ProcessHealingEvent(eventData);
                break;
            case "buff":
                result = _effects.ProcessBuffEvent(eventData);
                break;
            case "debuff":
                result = _effects.ProcessDebuffEvent(eventData);
                break;
            case "poison":
                result = _effects.ProcessPoisonEvent(eventData);
                break;
            case "damage":
                result = _effects.ProcessDamageEvent(eventData);
                break;
            case "choice":
                result = _effects.ProcessChoiceEvent(eventData);
                break;
            case "bonus":
            case "achievement":
            case "milestone":
                result = _effects.ProcessRewardEvent(eventData);
                break;
            default:
                result["message"] = eventData["description"].ToString();
                result["success"] = true;
                break;
        }
        
        return result;
    }
    
    // Player state updates
    public void UpdatePlayerFloor(int floor)
    {
        _data.PlayerFloor = floor;
    }
    
    public void UpdatePlayerHealth(int health)
    {
        _data.PlayerHealth = health;
        _data.IsInjured = health < 50;
        _data.IsFullHealth = health >= 100;
    }
    
    public void UpdatePlayerGold(int gold)
    {
        _data.PlayerGold = gold;
    }
    
    public void OnEnemyDefeated()
    {
        _data.EnemiesDefeatedInRoom++;
    }
    
    public void OnRoomExplored()
    {
        _data.RoomsExplored++;
    }
    
    // Statistics
    public Dictionary<string, object> GetStatistics()
    {
        return new Dictionary<string, object>
        {
            { "total_events", _data.TotalEventsTriggered },
            { "positive_events", _data.PositiveEvents },
            { "negative_events", _data.NegativeEvents },
            { "neutral_events", _data.NeutralEvents },
            { "gold_gained", _data.GoldGainedFromEvents },
            { "gold_lost", _data.GoldLostFromEvents },
            { "exp_gained", _data.ExpGainedFromEvents },
            { "items_gained", _data.ItemsGained },
            { "items_lost", _data.ItemsLost },
            { "event_history", _data.EventHistory }
        };
    }
    
    // Save/Load support
    public Dictionary<string, object> SaveData()
    {
        return new Dictionary<string, object>
        {
            { "event_history", _data.EventHistory },
            { "total_events", _data.TotalEventsTriggered },
            { "positive_events", _data.PositiveEvents },
            { "negative_events", _data.NegativeEvents },
            { "neutral_events", _data.NeutralEvents },
            { "gold_gained", _data.GoldGainedFromEvents },
            { "gold_lost", _data.GoldLostFromEvents },
            { "exp_gained", _data.ExpGainedFromEvents },
            { "items_gained", _data.ItemsGained },
            { "items_lost", _data.ItemsLost }
        };
    }
    
    public void LoadData(Dictionary<string, object> saveData)
    {
        if (saveData == null) return;
        
        if (saveData.ContainsKey("event_history"))
            _data.EventHistory = new Dictionary<string, int>((Dictionary<string, int>)saveData["event_history"]);
        if (saveData.ContainsKey("total_events"))
            _data.TotalEventsTriggered = Convert.ToInt32(saveData["total_events"]);
        if (saveData.ContainsKey("positive_events"))
            _data.PositiveEvents = Convert.ToInt32(saveData["positive_events"]);
        if (saveData.ContainsKey("negative_events"))
            _data.NegativeEvents = Convert.ToInt32(saveData["negative_events"]);
        if (saveData.ContainsKey("neutral_events"))
            _data.NeutralEvents = Convert.ToInt32(saveData["neutral_events"]);
        if (saveData.ContainsKey("gold_gained"))
            _data.GoldGainedFromEvents = Convert.ToInt32(saveData["gold_gained"]);
        if (saveData.ContainsKey("gold_lost"))
            _data.GoldLostFromEvents = Convert.ToInt32(saveData["gold_lost"]);
        if (saveData.ContainsKey("exp_gained"))
            _data.ExpGainedFromEvents = Convert.ToInt32(saveData["exp_gained"]);
        if (saveData.ContainsKey("items_gained"))
            _data.ItemsGained = Convert.ToInt32(saveData["items_gained"]);
        if (saveData.ContainsKey("items_lost"))
            _data.ItemsLost = Convert.ToInt32(saveData["items_lost"]);
    }

    /// <summary>
    /// Export save data for persistence
    /// </summary>
    public override Dictionary ExportSaveData()
    {
        return new Dictionary();
    }

    /// <summary>
    /// Import save data from persistence
    /// </summary>
    public override void ImportSaveData(Dictionary data)
    {
        // No persistent data needed
    }
}
