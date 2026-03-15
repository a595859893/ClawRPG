using Godot;
using System;
using System.Collections.Generic;

/// <summary>
/// 地牢事件生成器。负责随机选择事件、计算权重和应用稀有度规则。
/// </summary>
public partial class DungeonEventGenerator
{
    private Random _rand = new Random();
    private DungeonEventDatabase _database;
    private RandomDungeonEventData _data;
    
    public DungeonEventGenerator(DungeonEventDatabase database, RandomDungeonEventData data)
    {
        _database = database;
        _data = data;
    }
    
    public void SetDatabase(DungeonEventDatabase database)
    {
        _database = database;
    }
    
    public void SetData(RandomDungeonEventData data)
    {
        _data = data;
    }
    
    /// <summary>
    /// 生成一个随机事件
    /// </summary>
    public Dictionary<string, object> GenerateRandomEvent()
    {
        var result = new Dictionary<string, object>();
        
        // Determine event category based on weights
        string category = GetRandomCategory();
        
        // Get event list for category
        List<string> eventList = _database.GetEventListForCategory(category);
        if (eventList.Count == 0)
        {
            result["success"] = false;
            result["message"] = "No events available";
            return result;
        }
        
        // Select event based on rarity weights
        string eventId = SelectEventByRarity(eventList);
        
        // Process the event
        return GetEventWithData(eventId);
    }
    
    /// <summary>
    /// 根据权重随机选择事件分类
    /// </summary>
    private string GetRandomCategory()
    {
        float roll = (float)_rand.NextDouble() * 100f;
        
        // Weighted category selection
        if (roll < 20) return "Treasure";
        if (roll < 35) return "Combat";
        if (roll < 48) return "Exploration";
        if (roll < 58) return "Trap";
        if (roll < 68) return "Hazard";
        if (roll < 78) return "Mystery";
        if (roll < 85) return "Blessing";
        if (roll < 90) return "Curse";
        if (roll < 95) return "NPC";
        return "Reward";
    }
    
    /// <summary>
    /// 根据稀有度权重选择事件
    /// </summary>
    private string SelectEventByRarity(List<string> eventList)
    {
        // Filter events by current floor difficulty
        var validEvents = new List<string>();
        
        foreach (var eventId in eventList)
        {
            var eventData = _database.GetEvent(eventId);
            if (eventData != null)
            {
                string rarity = eventData.Contains("rarity") ? eventData["rarity"].ToString() : "Common";
                
                // Scale availability by floor
                if (_data.PlayerFloor < 5 && (rarity == "Epic" || rarity == "Legendary"))
                    continue;
                if (_data.PlayerFloor < 10 && rarity == "Legendary")
                    continue;
                    
                validEvents.Add(eventId);
            }
        }
        
        if (validEvents.Count == 0)
            return eventList[0];
            
        // Weighted random selection
        float roll = (float)_rand.NextDouble() * 100f;
        
        // Rarity weights
        float commonWeight = 50f;
        float uncommonWeight = 30f;
        float rareWeight = 15f;
        float epicWeight = 4f;
        float legendaryWeight = 1f;
        
        // Adjust based on floor
        int floorBonus = Math.Min(_data.PlayerFloor / 5, 3);
        rareWeight += floorBonus * 2;
        epicWeight += floorBonus;
        
        float totalWeight = commonWeight + uncommonWeight + rareWeight + epicWeight + legendaryWeight;
        float currentWeight = 0;
        
        foreach (var eventId in validEvents)
        {
            var eventData = _database.GetEvent(eventId);
            if (eventData == null)
                continue;
                
            string rarity = eventData.Contains("rarity") ? eventData["rarity"].ToString() : "Common";
            float eventWeight = rarity switch
            {
                "Common" => commonWeight,
                "Uncommon" => uncommonWeight,
                "Rare" => rareWeight,
                "Epic" => epicWeight,
                "Legendary" => legendaryWeight,
                _ => commonWeight
            };
            
            currentWeight += eventWeight;
            if (roll <= (currentWeight / totalWeight) * 100f)
                return eventId;
        }
        
        return validEvents[0];
    }
    
    /// <summary>
    /// 获取事件数据并记录统计
    /// </summary>
    private Dictionary<string, object> GetEventWithData(string eventId)
    {
        var result = new Dictionary<string, object>();
        var eventData = _database.GetEvent(eventId);
        
        if (eventData == null)
        {
            result["success"] = false;
            result["message"] = "Event not found";
            return result;
        }
        
        // Record event
        if (_data.EventHistory.ContainsKey(eventId))
            _data.EventHistory[eventId]++;
        else
            _data.EventHistory[eventId] = 1;
            
        _data.TotalEventsTriggered++;
        
        string category = eventData["category"].ToString();
        bool isPositive = eventData.Contains("positive") && eventData["positive"].ToString() == "true";
        bool isNegative = eventData.Contains("positive") && eventData["positive"].ToString() == "false";
        
        if (isPositive) _data.PositiveEvents++;
        else if (isNegative) _data.NegativeEvents++;
        else _data.NeutralEvents++;
        
        result["event_id"] = eventId;
        result["category"] = category;
        result["description"] = eventData["description"];
        result["event_data"] = eventData;
        
        return result;
    }
    
    /// <summary>
    /// 获取特定分类的随机事件
    /// </summary>
    public Dictionary<string, object> GenerateEventForCategory(string category)
    {
        var result = new Dictionary<string, object>();
        
        List<string> eventList = _database.GetEventListForCategory(category);
        if (eventList.Count == 0)
        {
            result["success"] = false;
            result["message"] = "No events available for category";
            return result;
        }
        
        string eventId = eventList[_rand.Next(eventList.Count)];
        return GetEventWithData(eventId);
    }
    
    /// <summary>
    /// 获取特定稀有度的事件
    /// </summary>
    public Dictionary<string, object> GenerateEventWithRarity(string rarity)
    {
        var result = new Dictionary<string, object>();
        
        // Search through all events to find matching rarity
        var database = _database.GetEventDatabase();
        var matchingEvents = new List<string>();
        
        foreach (var kvp in database)
        {
            string eventRarity = kvp.Value.Contains("rarity") ? kvp.Value["rarity"].ToString() : "Common";
            if (eventRarity == rarity)
            {
                matchingEvents.Add(kvp.Key);
            }
        }
        
        if (matchingEvents.Count == 0)
        {
            result["success"] = false;
            result["message"] = $"No events found with rarity {rarity}";
            return result;
        }
        
        string eventId = matchingEvents[_rand.Next(matchingEvents.Count)];
        return GetEventWithData(eventId);
    }
}
