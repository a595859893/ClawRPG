using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// 随机地牢事件系统。管理地牢中的随机事件触发，包括战斗、宝藏、祝福、诅咒等多种类型的事件。
/// 支持事件稀有度权重、玩家状态影响和事件结果计算。
/// </summary>
public partial class RandomDungeonEventData : Node
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
}

public enum DungeonEventRarity
{
    Common,
    Uncommon,
    Rare,
    Epic,
    Legendary
}

public enum DungeonEventOutcome
{
    Success,
    Failure,
    Mixed,
    Nothing
}

[GlobalClass]
/// <summary>
/// 随机地牢事件系统核心类。负责事件数据库初始化、事件触发、事件处理和结果计算。
/// </summary>
public partial class RandomDungeonEventSystem : Node
{
    private RandomDungeonEventData _data;
    private Random _rand = new Random();
    
    // Event database
    private Dictionary<string, Dictionary> _eventDatabase = new Dictionary<string, Dictionary>();
    
    // Event categories
    private List<string> _combatEvents = new List<string>();
    private List<string> _treasureEvents = new List<string>();
    private List<string> _mysteryEvents = new List<string>();
    private List<string> _hazardEvents = new List<string>();
    private List<string> _blessingEvents = new List<string>();
    private List<string> _curseEvents = new List<string>();
    private List<string> _npcEvents = new List<string>();
    private List<string> _explorationEvents = new List<string>();
    private List<string> _trapEvents = new List<string>();
    private List<string> _rewardEvents = new List<string>();
    
    public override void _Ready()
    {
        InitializeEventDatabase();
    }
    
    public void InitializeEventDatabase()
    {
        _eventDatabase = new Dictionary<string, Dictionary>();
        
        // Combat Events
        AddEventToDatabase("ambush", "Combat", "Uncommon", 
            "You walk into an ambush! A group of enemies has been waiting for you.",
            new Dictionary<string, object> {
                { "type", "combat" },
                { "enemy_count", 3 },
                { "enemy_type", "bandit" },
                { "difficulty", 1.2f },
                { "reward_gold", 50 },
                { "reward_exp", 30 },
                { "positive", false }
            });
            
        AddEventToDatabase("surprise_attack", "Combat", "Rare",
            "You catch enemies by surprise! You get the first strike.",
            new Dictionary<string, object> {
                { "type", "combat" },
                { "enemy_count", 2 },
                { "difficulty", 0.8f },
                { "reward_gold", 80 },
                { "reward_exp", 50 },
                { "positive", true }
            });
            
        AddEventToDatabase("reinforcements", "Combat", "Epic",
            "More enemies arrive as reinforcements!",
            new Dictionary<string, object> {
                { "type", "combat" },
                { "enemy_count", 5 },
                { "difficulty", 1.5f },
                { "reward_gold", 150 },
                { "reward_exp", 100 },
                { "positive", false }
            });
        
        // Treasure Events
        AddEventToDatabase("hidden_chest", "Treasure", "Common",
            "You find a hidden chest in the corner of the room!",
            new Dictionary<string, object> {
                { "type", "treasure" },
                { "gold_min", 20 },
                { "gold_max", 50 },
                { "item_chance", 0.3f },
                { "positive", true }
            });
            
        AddEventToDatabase("treasure_room", "Treasure", "Epic",
            "You discover a treasure room filled with riches!",
            new Dictionary<string, object> {
                { "type", "treasure" },
                { "gold_min", 200 },
                { "gold_max", 500 },
                { "item_chance", 0.8f },
                { "item_count_min", 2 },
                { "item_count_max", 4 },
                { "positive", true }
            });
            
        AddEventToDatabase("empty_chest", "Treasure", "Common",
            "You find a chest, but it's empty...",
            new Dictionary<string, object> {
                { "type", "treasure" },
                { "gold_min", 0 },
                { "gold_max", 10 },
                { "positive", false }
            });
        
        // Mystery Events
        AddEventToDatabase("strange_orb", "Mystery", "Rare",
            "You find a strange orb pulsing with mysterious energy.",
            new Dictionary<string, object> {
                { "type", "choice" },
                { "choice_a", "Touch the orb" },
                { "choice_b", "Leave it alone" },
                { "choice_a_effect", "random" },
                { "choice_b_effect", "safe" }
            });
            
        AddEventToDatabase("ancient_scroll", "Mystery", "Uncommon",
            "You discover an ancient scroll with unreadable text.",
            new Dictionary<string, object> {
                { "type", "scroll" },
                { "exp_bonus", 25 },
                { "positive", true }
            });
            
        AddEventToDatabase("cursed_door", "Mystery", "Rare",
            "A door with ominous symbols blocks your path.",
            new Dictionary<string, object> {
                { "type", "choice" },
                { "choice_a", "Open the door" },
                { "choice_b", "Find another way" },
                { "choice_a_effect", "curse" },
                { "choice_b_effect", "safe" }
            });
        
        // Hazard Events
        AddEventToDatabase("falling_rocks", "Hazard", "Uncommon",
            "Rocks start falling from the ceiling!",
            new Dictionary<string, object> {
                { "type", "dodge" },
                { "damage_min", 5 },
                { "damage_max", 15 },
                { "difficulty", 0.6f },
                { "positive", false }
            });
            
        AddEventToDatabase("flooding", "Hazard", "Rare",
            "Water starts flooding the room!",
            new Dictionary<string, object> {
                { "type", "escape" },
                { "time_limit", 10f },
                { "damage_per_second", 3 },
                { "positive", false }
            });
            
        AddEventToDatabase("collapsing_floor", "Hazard", "Epic",
            "The floor begins to collapse!",
            new Dictionary<string, object> {
                { "type", "escape" },
                { "time_limit", 8f },
                { "damage_max", 30 },
                { "reward_gold", 100 },
                { "positive", false }
            });
        
        // Blessing Events
        AddEventToDatabase("healing_fountain", "Blessing", "Common",
            "You find a fountain with glowing water that restores health.",
            new Dictionary<string, object> {
                { "type", "heal" },
                { "heal_amount", 30 },
                { "positive", true }
            });
            
        AddEventToDatabase("blessing_of_light", "Blessing", "Rare",
            "A beam of light descends upon you, granting divine blessing.",
            new Dictionary<string, object> {
                { "type", "buff" },
                { "buff_duration", 180 },
                { "attack_bonus", 1.2f },
                { "defense_bonus", 1.2f },
                { "positive", true }
            });
            
        AddEventToDatabase("treasure_blessing", "Blessing", "Epic",
            "The ancient god of wealth smiles upon you!",
            new Dictionary<string, object> {
                { "type", "fortune" },
                { "gold_multiplier", 2.0f },
                { "drop_bonus", 1.5f },
                { "duration", 300 },
                { "positive", true }
            });
        
        // Curse Events
        AddEventToDatabase("cursed_trap", "Curse", "Uncommon",
            "You trigger a curse trap! Dark energy surrounds you.",
            new Dictionary<string, object> {
                { "type", "debuff" },
                { "debuff_duration", 120 },
                { "attack_penalty", 0.8f },
                { "defense_penalty", 0.8f },
                { "positive", false }
            });
            
        AddEventToDatabase("shadow_curse", "Curse", "Rare",
            "A shadow curse binds your soul!",
            new Dictionary<string, object> {
                { "type", "curse" },
                { "curse_duration", 300 },
                { "gold_loss_rate", 0.5f },
                { "exp_loss_rate", 0.5f },
                { "positive", false }
            });
            
        AddEventToDatabase("monster_curse", "Curse", "Epic",
            "You are cursed to attract more enemies!",
            new Dictionary<string, object> {
                { "type", "attraction" },
                { "duration", 600 },
                { "enemy_spawn_rate", 2.0f },
                { "positive", false }
            });
        
        // NPC Events
        AddEventToDatabase("wandering_merchant", "NPC", "Uncommon",
            "A wandering merchant offers goods for sale.",
            new Dictionary<string, object> {
                { "type", "shop" },
                { "items", 3 },
                { "discount", 0.8f },
                { "positive", true }
            });
            
        AddEventToDatabase("mysterious_stranger", "NPC", "Rare",
            "A mysterious stranger offers to help you.",
            new Dictionary<string, object> {
                { "type", "help" },
                { "help_type", "random" },
                { "positive", true }
            });
            
        AddEventToDatabase("injured_adventurer", "NPC", "Common",
            "You find an injured adventurer who needs help.",
            new Dictionary<string, object> {
                { "type", "rescue" },
                { "gold_cost", 20 },
                { "reward_exp", 40 },
                { "reward_item", true },
                { "positive", true }
            });
        
        // Exploration Events
        AddEventToDatabase("secret_passage", "Exploration", "Uncommon",
            "You discover a secret passage leading to a hidden area!",
            new Dictionary<string, object> {
                { "type", "secret" },
                { "extra_rooms", 2 },
                { "positive", true }
            });
            
        AddEventToDatabase("dead_end", "Exploration", "Common",
            "You've reached a dead end with nothing but rubble.",
            new Dictionary<string, object> {
                { "type", "dead_end" },
                { "positive", false }
            });
            
        AddEventToDatabase("shortcut", "Exploration", "Rare",
            "You find a shortcut that saves time and effort!",
            new Dictionary<string, object> {
                { "type", "shortcut" },
                { "floor_skip", 1 },
                { "positive", true }
            });
        
        // Trap Events
        AddEventToDatabase("poison_trap", "Trap", "Common",
            "You step on a pressure plate! Poison gas fills the room.",
            new Dictionary<string, object> {
                { "type", "poison" },
                { "damage", 10 },
                { "dot_duration", 30 },
                { "positive", false }
            });
            
        AddEventToDatabase("spike_trap", "Trap", "Common",
            "Spikes shoot up from the floor!",
            new Dictionary<string, object> {
                { "type", "damage" },
                { "damage_min", 15 },
                { "damage_max", 25 },
                { "positive", false }
            });
            
        AddEventToDatabase("teleport_trap", "Trap", "Rare",
            "A teleportation trap activates!",
            new Dictionary<string, object> {
                { "type", "teleport" },
                { "destination", "random" },
                { "positive", "neutral" }
            });
        
        // Reward Events
        AddEventToDatabase("daily_bonus", "Reward", "Uncommon",
            "Your daily login bonus! The dungeon recognizes your dedication.",
            new Dictionary<string, object> {
                { "type", "bonus" },
                { "gold", 100 },
                { "exp", 50 },
                { "positive", true }
            });
            
        AddEventToDatabase("achievement_reward", "Reward", "Rare",
            "You've unlocked a dungeon achievement!",
            new Dictionary<string, object> {
                { "type", "achievement" },
                { "stat_bonus", 1.1f },
                { "positive", true }
            });
            
        AddEventToDatabase("milestone_reward", "Reward", "Legendary",
            "A major milestone! The dungeon itself acknowledges your prowess!",
            new Dictionary<string, object> {
                { "type", "milestone" },
                { "gold", 500 },
                { "exp", 300 },
                { "item_rarity", "Epic" },
                { "positive", true }
            });
        
        // Categorize events
        _combatEvents = new List<string> { "ambush", "surprise_attack", "reinforcements" };
        _treasureEvents = new List<string> { "hidden_chest", "treasure_room", "empty_chest" };
        _mysteryEvents = new List<string> { "strange_orb", "ancient_scroll", "cursed_door" };
        _hazardEvents = new List<string> { "falling_rocks", "flooding", "collapsing_floor" };
        _blessingEvents = new List<string> { "healing_fountain", "blessing_of_light", "treasure_blessing" };
        _curseEvents = new List<string> { "cursed_trap", "shadow_curse", "monster_curse" };
        _npcEvents = new List<string> { "wandering_merchant", "mysterious_stranger", "injured_adventurer" };
        _explorationEvents = new List<string> { "secret_passage", "dead_end", "shortcut" };
        _trapEvents = new List<string> { "poison_trap", "spike_trap", "teleport_trap" };
        _rewardEvents = new List<string> { "daily_bonus", "achievement_reward", "milestone_reward" };
    }
    
    private void AddEventToDatabase(string id, string category, string rarity, string description, Dictionary<string, object> properties)
    {
        var eventData = new Dictionary<string, object>
        {
            { "id", id },
            { "category", category },
            { "rarity", rarity },
            { "description", description }
        };
        
        foreach (var prop in properties)
        {
            eventData[prop.Key] = prop.Value;
        }
        
        _eventDatabase[id] = eventData;
    }
    
    public void SetData(RandomDungeonEventData data)
    {
        _data = data;
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
        var result = new Dictionary<string, object>();
        
        // Determine event category based on weights
        string category = GetRandomCategory();
        
        // Get event list for category
        List<string> eventList = GetEventListForCategory(category);
        if (eventList.Count == 0)
        {
            result["success"] = false;
            result["message"] = "No events available";
            return result;
        }
        
        // Select event based on rarity weights
        string eventId = SelectEventByRarity(eventList);
        
        // Process the event
        return ProcessEvent(eventId);
    }
    
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
    
    private List<string> GetEventListForCategory(string category)
    {
        return category switch
        {
            "Combat" => _combatEvents,
            "Treasure" => _treasureEvents,
            "Mystery" => _mysteryEvents,
            "Hazard" => _hazardEvents,
            "Blessing" => _blessingEvents,
            "Curse" => _curseEvents,
            "NPC" => _npcEvents,
            "Exploration" => _explorationEvents,
            "Trap" => _trapEvents,
            "Reward" => _rewardEvents,
            _ => new List<string>()
        };
    }
    
    private string SelectEventByRarity(List<string> eventList)
    {
        // Filter events by current floor difficulty
        var validEvents = new List<string>();
        
        foreach (var eventId in eventList)
        {
            if (_eventDatabase.TryGetValue(eventId, out var eventData))
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
            if (!_eventDatabase.TryGetValue(eventId, out var eventData))
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
    
    private Dictionary<string, object> ProcessEvent(string eventId)
    {
        var result = new Dictionary<string, object>();
        
        if (!_eventDatabase.TryGetValue(eventId, out var eventData))
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
        
        string eventType = eventData["type"].ToString();
        
        // Process based on event type
        switch (eventType)
        {
            case "combat":
                result = ProcessCombatEvent(eventData);
                break;
            case "treasure":
                result = ProcessTreasureEvent(eventData);
                break;
            case "heal":
                result = ProcessHealingEvent(eventData);
                break;
            case "buff":
                result = ProcessBuffEvent(eventData);
                break;
            case "debuff":
                result = ProcessDebuffEvent(eventData);
                break;
            case "poison":
                result = ProcessPoisonEvent(eventData);
                break;
            case "damage":
                result = ProcessDamageEvent(eventData);
                break;
            case "choice":
                result = ProcessChoiceEvent(eventData);
                break;
            case "bonus":
            case "achievement":
            case "milestone":
                result = ProcessRewardEvent(eventData);
                break;
            default:
                result["message"] = eventData["description"].ToString();
                result["success"] = true;
                break;
        }
        
        result["event_id"] = eventId;
        result["category"] = category;
        result["description"] = eventData["description"];
        
        return result;
    }
    
    private Dictionary<string, object> ProcessCombatEvent(Dictionary eventData)
    {
        var result = new Dictionary<string, object>();
        
        int enemyCount = eventData.Contains("enemy_count") ? Convert.ToInt32(eventData["enemy_count"]) : 1;
        float difficulty = eventData.Contains("difficulty") ? Convert.ToSingle(eventData["difficulty"]) : 1.0f;
        int goldReward = eventData.Contains("reward_gold") ? Convert.ToInt32(eventData["reward_gold"]) : 0;
        int expReward = eventData.Contains("reward_exp") ? Convert.ToInt32(eventData["reward_exp"]) : 0;
        
        // Calculate actual rewards based on difficulty
        goldReward = (int)(goldReward * difficulty);
        expReward = (int)(expReward * difficulty);
        
        _data.GoldGainedFromEvents += goldReward;
        _data.ExpGainedFromEvents += expReward;
        
        result["combat"] = true;
        result["enemy_count"] = enemyCount;
        result["difficulty"] = difficulty;
        result["gold_reward"] = goldReward;
        result["exp_reward"] = expReward;
        result["message"] = $"Combat encounter! {enemyCount} enemies (difficulty {difficulty:F1}x).";
        
        _data.EnemiesDefeatedInRoom = 0; // Reset for combat
        
        return result;
    }
    
    private Dictionary<string, object> ProcessTreasureEvent(Dictionary eventData)
    {
        var result = new Dictionary<string, object>();
        
        int goldMin = eventData.Contains("gold_min") ? Convert.ToInt32(eventData["gold_min"]) : 0;
        int goldMax = eventData.Contains("gold_max") ? Convert.ToInt32(eventData["gold_max"]) : 10;
        int gold = _rand.Next(goldMin, goldMax + 1);
        bool hasItem = eventData.Contains("item_chance") && (float)_rand.NextDouble() < Convert.ToSingle(eventData["item_chance"]);
        
        _data.GoldGainedFromEvents += gold;
        _data.HasTreasure = true;
        
        result["gold_found"] = gold;
        result["has_item"] = hasItem;
        
        if (hasItem)
        {
            _data.ItemsGained++;
            result["item_rarity"] = GetRandomItemRarity();
        }
        
        string message = gold > 0 ? $"Found {gold} gold!" : "Found nothing...";
        if (hasItem) message += " Also found an item!";
        
        result["message"] = message;
        result["success"] = true;
        
        return result;
    }
    
    private Dictionary<string, object> ProcessHealingEvent(Dictionary eventData)
    {
        var result = new Dictionary<string, object>();
        
        int healAmount = eventData.Contains("heal_amount") ? Convert.ToInt32(eventData["heal_amount"]) : 20;
        
        _data.PlayerHealth = Math.Min(_data.PlayerHealth + healAmount, 100);
        _data.IsInjured = _data.PlayerHealth < 50;
        _data.IsFullHealth = _data.PlayerHealth >= 100;
        
        result["healed"] = healAmount;
        result["current_health"] = _data.PlayerHealth;
        result["message"] = $"Restored {healAmount} health!";
        result["success"] = true;
        
        return result;
    }
    
    private Dictionary<string, object> ProcessBuffEvent(Dictionary eventData)
    {
        var result = new Dictionary<string, object>();
        
        string buffId = "buff_" + Guid.NewGuid().ToString().Substring(0, 8);
        int duration = eventData.Contains("buff_duration") ? Convert.ToInt32(eventData["buff_duration"]) : 60;
        
        _data.AppliedBuffs.Add(buffId);
        
        result["buff_id"] = buffId;
        result["duration"] = duration;
        
        if (eventData.Contains("attack_bonus"))
            result["attack_bonus"] = eventData["attack_bonus"];
        if (eventData.Contains("defense_bonus"))
            result["defense_bonus"] = eventData["defense_bonus"];
        if (eventData.Contains("gold_multiplier"))
            result["gold_multiplier"] = eventData["gold_multiplier"];
            
        result["message"] = "You received a blessing!";
        result["success"] = true;
        
        return result;
    }
    
    private Dictionary<string, object> ProcessDebuffEvent(Dictionary eventData)
    {
        var result = new Dictionary<string, object>();
        
        string debuffId = "debuff_" + Guid.NewGuid().ToString().Substring(0, 8);
        int duration = eventData.Contains("debuff_duration") ? Convert.ToInt32(eventData["debuff_duration"]) : 60;
        
        _data.AppliedDebuffs.Add(debuffId);
        
        result["debuff_id"] = debuffId;
        result["duration"] = duration;
        
        if (eventData.Contains("attack_penalty"))
            result["attack_penalty"] = eventData["attack_penalty"];
        if (eventData.Contains("defense_penalty"))
            result["defense_penalty"] = eventData["defense_penalty"];
            
        result["message"] = "You are cursed!";
        result["success"] = true;
        
        return result;
    }
    
    private Dictionary<string, object> ProcessPoisonEvent(Dictionary eventData)
    {
        var result = new Dictionary<string, object>();
        
        int damage = eventData.Contains("damage") ? Convert.ToInt32(eventData["damage"]) : 10;
        int dotDuration = eventData.Contains("dot_duration") ? Convert.ToInt32(eventData["dot_duration"]) : 10;
        
        _data.PlayerHealth = Math.Max(_data.PlayerHealth - damage, 0);
        _data.IsInjured = _data.PlayerHealth < 50;
        
        result["immediate_damage"] = damage;
        result["dot_duration"] = dotDuration;
        result["current_health"] = _data.PlayerHealth;
        result["message"] = $"Poison deals {damage} damage!";
        result["success"] = true;
        
        return result;
    }
    
    private Dictionary<string, object> ProcessDamageEvent(Dictionary eventData)
    {
        var result = new Dictionary<string, object>();
        
        int damageMin = eventData.Contains("damage_min") ? Convert.ToInt32(eventData["damage_min"]) : 5;
        int damageMax = eventData.Contains("damage_max") ? Convert.ToInt32(eventData["damage_max"]) : 15;
        int damage = _rand.Next(damageMin, damageMax + 1);
        
        _data.PlayerHealth = Math.Max(_data.PlayerHealth - damage, 0);
        _data.IsInjured = _data.PlayerHealth < 50;
        
        result["damage"] = damage;
        result["current_health"] = _data.PlayerHealth;
        result["message"] = $"Trap deals {damage} damage!";
        result["success"] = true;
        
        return result;
    }
    
    private Dictionary<string, object> ProcessChoiceEvent(Dictionary eventData)
    {
        var result = new Dictionary<string, object>();
        
        string choiceA = eventData.Contains("choice_a") ? eventData["choice_a"].ToString() : "Option A";
        string choiceB = eventData.Contains("choice_b") ? eventData["choice_b"].ToString() : "Option B";
        
        result["choice_a"] = choiceA;
        result["choice_b"] = choiceB;
        result["requires_choice"] = true;
        result["message"] = $"A choice appears: {choiceA} or {choiceB}?";
        
        return result;
    }
    
    private Dictionary<string, object> ProcessRewardEvent(Dictionary eventData)
    {
        var result = new Dictionary<string, object>();
        
        int gold = eventData.Contains("gold") ? Convert.ToInt32(eventData["gold"]) : 0;
        int exp = eventData.Contains("exp") ? Convert.ToInt32(eventData["exp"]) : 0;
        
        _data.GoldGainedFromEvents += gold;
        _data.ExpGainedFromEvents += exp;
        
        result["gold_reward"] = gold;
        result["exp_reward"] = exp;
        
        if (eventData.Contains("item_rarity"))
        {
            result["item_rarity"] = eventData["item_rarity"].ToString();
            _data.ItemsGained++;
        }
        
        string message = "";
        if (gold > 0) message += $" +{gold} gold";
        if (exp > 0) message += $" +{exp} exp";
        
        result["message"] = message.Length > 0 ? message : "You received a reward!";
        result["success"] = true;
        
        return result;
    }
    
    private string GetRandomItemRarity()
    {
        float roll = (float)_rand.NextDouble() * 100f;
        
        if (roll < 50) return "Common";
        if (roll < 80) return "Uncommon";
        if (roll < 95) return "Rare";
        if (roll < 99) return "Epic";
        return "Legendary";
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
}
